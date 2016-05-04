//---------------------------------------------------------------------
// <copyright file="ReferenceTypeElement.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
namespace System.Data.EntityModel.SchemaObjectModel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Text;
    using System.Xml;
    using Som = System.Data.EntityModel.SchemaObjectModel;

    class ReferenceTypeElement : ModelFunctionTypeElement
    {
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentElement"></param>
        internal ReferenceTypeElement(SchemaElement parentElement)
            : base(parentElement)
        {

        }
        #endregion

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.TypeElement))
            {
                HandleTypeElementAttribute(reader);
                return true;
            }

            return false;
        }

        protected void HandleTypeElementAttribute(XmlReader reader)
        {
            Debug.Assert(reader != null);

            string type;
            if (!Utils.GetString(Schema, reader, out type))
                return;

            if (!Utils.ValidateDottedName(Schema, reader, type))
                return;

            _unresolvedType = type;
        }

        internal override void WriteIdentity(StringBuilder builder)
        {
            Debug.Assert(UnresolvedType != null && !UnresolvedType.Trim().Equals(String.Empty));
            builder.Append("Ref(" + UnresolvedType + ")");
        }

        internal override TypeUsage GetTypeUsage()
        {
            return _typeUsage;
        }

        internal override bool ResolveNameAndSetTypeUsage(Converter.ConversionCache convertedItemCache, Dictionary<Som.SchemaElement, GlobalItem> newGlobalItems)
        {
            if (_typeUsage == null)
            {
                Debug.Assert(!(_type is ScalarType));

                EdmType edmType = (EdmType)Converter.LoadSchemaElement(_type, _type.Schema.ProviderManifest, convertedItemCache, newGlobalItems);
                EntityType entityType = edmType as EntityType;

                Debug.Assert(entityType != null);

                RefType refType = new RefType(entityType);
                refType.AddMetadataProperties(this.OtherContent);
                _typeUsage = TypeUsage.Create(refType);
            }
            return true;
        }

        internal override void Validate()
        {
            base.Validate();

            ValidationHelper.ValidateRefType(this, _type);
        }
    }
}
