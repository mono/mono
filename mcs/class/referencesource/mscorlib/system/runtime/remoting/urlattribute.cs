// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    UrlAttribute.cs
**
** <EMAIL>Author:  Tarun Anand ([....])</EMAIL>
**
** Purpose: Defines an attribute which can be used at the callsite to
**          specify the URL at which the activation will happen.
**
** Date:    [....] 30, 2000
**
===========================================================*/
namespace System.Runtime.Remoting.Activation {
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Permissions;
    using System;
    [System.Security.SecurityCritical]  // auto-generated
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class UrlAttribute : ContextAttribute
    {
        private String url;
        private static String propertyName = "UrlAttribute";

        [System.Security.SecurityCritical]  // auto-generated_required
        public UrlAttribute(String callsiteURL) :base(propertyName)
        {
            if(null == callsiteURL)
            {
                // Invalid arg
                throw new ArgumentNullException("callsiteURL");
            }
            url = callsiteURL;
        }        


        // Object::Equals
        // Override the default implementation which just compares the names
        [System.Security.SecuritySafeCritical] // overrides public transparent member
        public override bool Equals(Object o)
        {
            return (o is IContextProperty) && (o is UrlAttribute) && 
                   (((UrlAttribute)o).UrlValue.Equals(url));
        }

        [System.Security.SecuritySafeCritical] // overrides public transparent member
        public override int GetHashCode()
        {
            return this.url.GetHashCode();
        }
        
        // Override ContextAttribute's implementation of IContextAttribute::IsContextOK
        [System.Security.SecurityCritical]  // auto-generated_required
        [System.Runtime.InteropServices.ComVisible(true)]
        public override bool IsContextOK(Context ctx, IConstructionCallMessage msg)
        {
            return false;
        }
    
        // Override ContextAttribute's impl. of IContextAttribute::GetPropForNewCtx
        [System.Security.SecurityCritical]  // auto-generated_required
        [System.Runtime.InteropServices.ComVisible(true)]
        public override void GetPropertiesForNewContext(IConstructionCallMessage ctorMsg)
        {
            // We are not interested in contributing any properties to the
            // new context since the only purpose of this property is to force
            // the creation of the context and the server object inside it at
            // the specified URL.
            return;
        }
        
        public String UrlValue
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get { return url; }            
        }
    }
} // namespace

