//---------------------------------------------------------------------
// <copyright file="keyproperty.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Metadata.Edm;


namespace System.Data.EntityModel.SchemaObjectModel
{
    /// <summary>
    /// Represents PropertyRef Element for Entity keys and referential constraints
    /// </summary>
    internal sealed class PropertyRefElement : SchemaElement
    {
        #region Instance Fields
        private StructuredProperty _property = null;
        #endregion

        #region Public Methods
        /// <summary>
        /// construct a KeyProperty object
        /// </summary>
        /// <param name="parentElement"></param>
        public PropertyRefElement(SchemaElement parentElement)
            : base(parentElement)
        {
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// property chain from KeyedType to Leaf property
        /// </summary>
        public StructuredProperty Property
        {
            get
            {
                return _property;
            }
        }
        #endregion

        #region Private Methods

        internal override void ResolveTopLevelNames()
        {
            Debug.Assert(false, "This method should never be used. Use other overload instead");
        }

        /// <summary>
        /// Since this method can be used in different context, this method does not add any errors
        /// Please make sure that the caller of this methods handles the error case and add errors 
        /// appropriately
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        internal bool ResolveNames(SchemaEntityType entityType)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                // Don't flag this error. This must already must have flaged as error, while handling name attribute
                return true;
            }

            // Make sure there is a property by this name
            _property = entityType.FindProperty(this.Name);

            return (_property != null);
        }

        #endregion
    }
}
