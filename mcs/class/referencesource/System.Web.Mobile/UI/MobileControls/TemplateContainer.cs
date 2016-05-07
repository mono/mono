//------------------------------------------------------------------------------
// <copyright file="TemplateContainer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Collections;                    
using System.Web.UI;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * TemplateContainer class. A specialized version of Panel that is
     * also a naming container. This class must be used by all mobile controls
     * as the container for instantiating templates.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\TemplateContainer.uex' path='docs/doc[@for="TemplateContainer"]/*' />
    [
        ToolboxItem(false)
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class TemplateContainer : Panel, INamingContainer
    {
        /// <include file='doc\TemplateContainer.uex' path='docs/doc[@for="TemplateContainer.TemplateContainer"]/*' />
        public TemplateContainer()
        {
            _breakAfter = false;
        } 

        // Override this property to change the default value attribute.
        /// <include file='doc\TemplateContainer.uex' path='docs/doc[@for="TemplateContainer.BreakAfter"]/*' />
        [
            DefaultValue(false)    
        ]
        public override bool BreakAfter
        {
            get
            {
                return base.BreakAfter;
            }

            set
            {
                base.BreakAfter = value;
            }
        }
    }
}
