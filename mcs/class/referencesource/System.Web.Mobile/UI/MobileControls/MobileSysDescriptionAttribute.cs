//------------------------------------------------------------------------------
// <copyright file="MobileSysDescriptionAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace System.Web.UI.MobileControls
{
    /// <devdoc>
    ///     DescriptionAttribute marks a property, event, or extender with a
    ///     description. Visual designers can display this description when referencing
    ///     the member.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.All)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class MobileSysDescriptionAttribute : DescriptionAttribute
    {
        private bool replaced;

        /// <devdoc>
        ///    <para>Constructs a new sys description.</para>
        /// </devdoc>
        internal MobileSysDescriptionAttribute(String description) : base(description)
        {
        }

        /// <devdoc>
        ///    <para>Retrieves the description text.</para>
        /// </devdoc>
        public override String Description
        {
            get
            {
                if (!replaced)
                {
                    replaced = true;
                    DescriptionValue = SR.GetString(base.Description);                
                }
                return base.Description;
            }
        }
    }
}
