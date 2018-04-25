//------------------------------------------------------------------------------
// <copyright file="ResourceDescriptionAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Reflection;

namespace System.Web.UI.Design.WebControls.Util
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Event, Inherited = true, AllowMultiple = false)]
    internal sealed class ResourceDescriptionAttribute : DescriptionAttribute
    {
        private bool _resourceLoaded;
        private readonly string _descriptionResourceName;

        public ResourceDescriptionAttribute(string descriptionResourceName)
        {
            _descriptionResourceName = descriptionResourceName;
        }

        public override string Description
        {
            get
            {
                if (!_resourceLoaded)
                {
                    _resourceLoaded = true;
                    DescriptionValue = System.Web.UI.Design.WebControlsRes.GetString(_descriptionResourceName);
                }
                return base.Description;
            }
        }
    }
}
