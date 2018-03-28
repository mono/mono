//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Configuration;


namespace System.IdentityModel.Configuration
{
#pragma warning disable 1591
    /// <summary>
    /// A collection of SecurityTokenHandlerElementCollection objects.
    /// </summary>
    [ConfigurationCollection( typeof( SecurityTokenHandlerElementCollection ), AddItemName = ConfigurationStrings.SecurityTokenHandlers, CollectionType = ConfigurationElementCollectionType.BasicMap )]
    public sealed partial class SecurityTokenHandlerSetElementCollection : ConfigurationElementCollection
    {
        public SecurityTokenHandlerSetElementCollection()
        {
        }

        protected override bool ThrowOnDuplicate
        {
          get
           {
             return true;
           }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SecurityTokenHandlerElementCollection();
        }

        protected override object GetElementKey( ConfigurationElement element )
        {
            return ( (SecurityTokenHandlerElementCollection)element ).Name;
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            string name = GetElementKey(element) as string;
            SecurityTokenHandlerElementCollection result = base.BaseGet(name) as SecurityTokenHandlerElementCollection;

            if (result != null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID7029, "<securityTokenHandlers>", name));
            }

            base.BaseAdd(element);
         }

        /// <summary>
        /// Returns a value indicating whether this element has been configured with non-default values.
        /// </summary>
        public bool IsConfigured
        {
            get
            {
                return ( Count > 0 );
            }
        }
    }
#pragma warning restore 1591
}
