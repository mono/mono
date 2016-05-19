//------------------------------------------------------------------------------
// <copyright file="ResourceDisplayNameAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Reflection;

namespace System.Web.UI.WebControls
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    internal sealed class ResourceDisplayNameAttribute : DisplayNameAttribute
    {
        private bool _resourceLoaded;
        private readonly string _displayNameResourceName;

        public ResourceDisplayNameAttribute(string displayNameResourceName)
        {
            _displayNameResourceName = displayNameResourceName;
        }

        public override string DisplayName
        {
            get
            {
                if (!_resourceLoaded)
                {
                    _resourceLoaded = true;
                    DisplayNameValue = System.Web.UI.WebControlsRes.GetString(_displayNameResourceName);
                }
                return base.DisplayName;
            }
        }
    }
}
