﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;
using UCR.Core.Models.Profile;

namespace UCR.Core.Managers
{
    public class ProfilesManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Context _context;
        private readonly List<Profile> _profiles;

        public ProfilesManager(Context context, List<Profile> profiles)
        {
            _context = context;
            _profiles = profiles;
        }

        public bool AddProfile(string title)
        {
            _profiles.Add(Profile.CreateProfile(_context, title));
            _context.ContextChanged();
            return true;
        }

        public bool ActivateProfile(Profile profile)
        {
            Logger.Debug($"Activating profile: {{{profile.ProfileBreadCrumbs()}}}");
            var success = true;
            if (_context.ActiveProfile?.Guid == profile.Guid) return true;
            var lastActiveProfile = _context.ActiveProfile;
            _context.ActiveProfile = profile;
            success &= profile.Activate(profile);
            if (success)
            {
                Logger.Debug($"Successfully activated profile");
                var subscribeSuccess = profile.SubscribeDeviceLists();
                _context.IOController.SetProfileState(profile.Guid, true);
                if (lastActiveProfile != null) DeactivateProfile(lastActiveProfile);
                foreach (var action in _context.ActiveProfileCallbacks)
                {
                    action();
                }
            }
            else
            {
                Logger.Debug($"Failed to activate profile");
                _context.ActiveProfile = lastActiveProfile;
            }
            return success;
        }

        public bool CopyProfile(Profile profile, string title = "Untitled")
        {
            var newProfile = Context.DeepXmlClone<Profile>(profile);
            newProfile.Title = title;
            newProfile.Guid = Guid.NewGuid();
            newProfile.PostLoad(_context, profile.ParentProfile);

            if (profile.ParentProfile != null)
            {
                profile.ParentProfile.AddChildProfile(newProfile, title);
            }
            else
            {
                _profiles.Add(newProfile);
            }
            _context.ContextChanged();

            return true;
        }

        public bool DeactivateProfile(Profile profile)
        {
            Logger.Debug($"Deactivating profile: {{{profile.ProfileBreadCrumbs()}}}");
            if (_context.ActiveProfile == null || profile == null) return true;
            if (_context.ActiveProfile.Guid == profile.Guid) _context.ActiveProfile = null;

            var success = profile.UnsubscribeDeviceLists();
            _context.IOController.SetProfileState(profile.Guid, false);

            foreach (var action in _context.ActiveProfileCallbacks)
            {
                action();
            }
            if (!success) Logger.Error($"Failed to deactivate profile: {{{profile.ProfileBreadCrumbs()}}}");
            return success;
        }

        /// <summary>
        /// Breadth-first search for nested profiles
        /// Find first search result and looks for the next result in the children
        /// </summary>
        /// <param name="search">List of profiles to search for nested under each other</param>
        /// <returns>The most specific profile found in the chain, otherwise null</returns>
        public Profile FindProfile(List<string> search)
        {
            Logger.Debug($"Searching for profile: {{{string.Join(",", search)}}}");
            Profile foundProfile = null;
            if (search?.Count == 0) return null;
            var queue = new List<Profile>();
            queue.AddRange(_profiles);
            while (queue.Count > 0)
            {
                var profile = queue[0];
                queue.RemoveAt(0);
                if (profile.Title.ToLower().Equals(search.First().ToLower()))
                {
                    if (search.Count == 1)
                    {
                        Logger.Debug($"Found profile: {{{profile.ProfileBreadCrumbs()}}}");
                        return profile;
                    }
                    foundProfile = profile;
                    search.RemoveAt(0);
                    Logger.Trace($"Found intermediate profile: {{{profile.ProfileBreadCrumbs()}}}. Remaining search: {{{string.Join(",", search)}}}");
                    queue.Clear();
                }
                if (profile.ChildProfiles != null) queue.AddRange(profile.ChildProfiles);

            }
            if (foundProfile == null) Logger.Debug($"No profile found for {{{string.Join(",", search)}}}");
            return foundProfile;
        }
    }
}
