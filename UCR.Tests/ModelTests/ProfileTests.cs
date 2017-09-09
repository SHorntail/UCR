﻿using System;
using NUnit.Framework;
using UCR.Models;
using UCR.Models.Plugins.Remapper;

namespace UCR.Tests
{
    [TestFixture]
    class ProfileTests
    {
        private UCRContext _ctx;
        private Profile _profile;
        private string _profileName;

        [SetUp]
        public void Setup()
        {
            _ctx = new UCRContext(false);
            _ctx.AddProfile("Base profile");
            _profile = _ctx.Profiles[0];
            _profileName = "Test";
        }

        [Test]
        public void AddChildProfile()
        {
            Assert.That(_profile.ChildProfiles.Count, Is.EqualTo(0));
            _profile.AddNewChildProfile(_profileName);
            Assert.That(_profile.ChildProfiles.Count, Is.EqualTo(1));
            Assert.That(_profile.ChildProfiles[0].Title, Is.EqualTo(_profileName));
            Assert.That(_profile.ChildProfiles[0].Parent, Is.EqualTo(_profile));
            Assert.That(_profile.ChildProfiles[0].Guid, Is.Not.EqualTo(Guid.Empty));
            Assert.That(_profile.IsActive, Is.Not.True);
            Assert.That(_ctx.IsNotSaved, Is.True);
        }
        
        [Test]
        public void RemoveChildProfile()
        {
            Assert.That(_profile.ChildProfiles.Count, Is.EqualTo(0));
            _profile.AddNewChildProfile(_profileName);
            Assert.That(_profile.ChildProfiles.Count, Is.EqualTo(1));
            Assert.That(_profile.ChildProfiles[0].Title, Is.EqualTo(_profileName));
            _profile.ChildProfiles[0].Remove();
            Assert.That(_profile.ChildProfiles.Count, Is.EqualTo(0));
            Assert.That(_ctx.IsNotSaved, Is.True);
        }

        [Test]
        public void RenameProfile()
        {
            var newName = "Renamed profile";
            Assert.That(_profile.Rename(newName), Is.True);
            Assert.That(_profile.Title, Is.EqualTo(newName));
            Assert.That(_ctx.IsNotSaved, Is.True);
        }

        [Test]
        public void AddPlugin()
        {
            var pluginName = "Test plugin";
            _profile.AddPlugin(new ButtonToButton(), pluginName);
            var plugin = _profile.Plugins[0];
            Assert.That(plugin, Is.Not.Null);
            Assert.That(plugin.Title, Is.EqualTo(pluginName));
            Assert.That(plugin.Inputs, Is.Not.Null);
            Assert.That(plugin.Outputs, Is.Not.Null);
            Assert.That(plugin.ParentProfile, Is.EqualTo(_profile));
            Assert.That(_ctx.IsNotSaved, Is.True);
        }

        [Test]
        public void GetDevice()
        {
            
        }

        [Test]
        public void GetDeviceList()
        {
            
        }

        [Test]
        public void ActivateProfile()
        {
            // TODO
            //_profile.
        }
    }
}
