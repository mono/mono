//---------------------------------------------------------------------
// <copyright file="EntityContainerEntitySetDefiningQuery.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System;
    using System.Data.Metadata.Edm;
    using System.Xml;

    /// <summary>
    /// Represents an DefiningQuery element.
    /// </summary>
    internal sealed class EntityContainerEntitySetDefiningQuery : SchemaElement
    {
        private string _query;

        /// <summary>
        /// Constructs an EntityContainerEntitySet
        /// </summary>
        /// <param name="parentElement">Reference to the schema element.</param>
        public EntityContainerEntitySetDefiningQuery(EntityContainerEntitySet parentElement)
            : base( parentElement )
        {
        }

        public string Query
        {
            get { return _query; }
        }

        protected override bool HandleText(XmlReader reader)
        {
            _query = reader.Value;
            return true;
        }

        internal override void Validate()
        {
            base.Validate();

            if(String.IsNullOrEmpty(_query))
            {
                AddError(ErrorCode.EmptyDefiningQuery, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.EmptyDefiningQuery);
            }
        }
   }
}
