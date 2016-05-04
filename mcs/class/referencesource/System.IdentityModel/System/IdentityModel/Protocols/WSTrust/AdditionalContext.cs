//-----------------------------------------------------------------------
// <copyright file="AdditionalContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the auth:AdditionalContext element.
    /// </summary>
    public class AdditionalContext
    {
        List<ContextItem> _contextItems = new List<ContextItem>();

        /// <summary>
        /// Initializes an instance of <see cref="AdditionalContext"/>
        /// </summary>
        public AdditionalContext()
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="AdditionalContext"/>
        /// </summary>
        /// <param name="items">Collection of ContextItems</param>
        /// <exception cref="ArgumentNullException">Input argument 'items' is null.</exception>
        public AdditionalContext( IEnumerable<ContextItem> items )
        {
            if ( items == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "items" );
            }

            foreach ( ContextItem item in items )
            {
                _contextItems.Add( item );
            }
        }

        /// <summary>
        /// Gets the Collection of items.
        /// </summary>
        public IList<ContextItem> Items
        {
            get { return _contextItems; }
        }
    }
}
