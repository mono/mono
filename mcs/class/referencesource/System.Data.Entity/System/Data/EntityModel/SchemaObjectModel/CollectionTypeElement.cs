//---------------------------------------------------------------------
// <copyright file="CollectionTypeElement.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
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

    /// <summary>
    /// class representing the Schema element in the schema
    /// </summary>
    internal class CollectionTypeElement : ModelFunctionTypeElement
    {
        private ModelFunctionTypeElement _typeSubElement = null;
        
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentElement"></param>
        internal CollectionTypeElement(SchemaElement parentElement)
            : base(parentElement)
        {

        }
        #endregion

        internal ModelFunctionTypeElement SubElement
        {
            get { return _typeSubElement; }
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.ElementType))
            {
                HandleElementTypeAttribute(reader);
                return true;
            }

            return false;
        }

        protected void HandleElementTypeAttribute(XmlReader reader)
        {
            Debug.Assert(reader != null);

            string type;
            if (!Utils.GetString(Schema, reader, out type))
                return;

            if (!Utils.ValidateDottedName(Schema, reader, type))
                return;

            _unresolvedType = type;
        }

        protected override bool HandleElement(XmlReader reader)
        {
            if (CanHandleElement(reader, XmlConstants.CollectionType))
            {
                HandleCollectionTypeElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.ReferenceType))
            {
                HandleReferenceTypeElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.TypeRef))
            {
                HandleTypeRefElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.RowType))
            {
                HandleRowTypeElement(reader);
                return true;
            }

            return false;
        }


        protected void HandleCollectionTypeElement(XmlReader reader)
        {
            Debug.Assert(reader != null);

            var subElement = new CollectionTypeElement(this);
            subElement.Parse(reader);
            _typeSubElement = subElement;
        }

        protected void HandleReferenceTypeElement(XmlReader reader)
        {
            Debug.Assert(reader != null);

            var subElement = new ReferenceTypeElement(this);
            subElement.Parse(reader);
            _typeSubElement = subElement;
        }

        protected void HandleTypeRefElement(XmlReader reader)
        {
            Debug.Assert(reader != null);

            var subElement = new TypeRefElement(this);
            subElement.Parse(reader);
            _typeSubElement = subElement;
        }

        protected void HandleRowTypeElement(XmlReader reader)
        {
            Debug.Assert(reader != null);

            var subElement = new RowTypeElement(this);
            subElement.Parse(reader);
            _typeSubElement = subElement;
        }

        internal override void ResolveTopLevelNames()
        {
            if (_typeSubElement != null)
            {
                _typeSubElement.ResolveTopLevelNames();
            }

            // Can't be "else if" because element could have attribute AND sub-element, 
            // in which case semantic validation won't work unless it has resolved both (so _type is not null)
            if( _unresolvedType != null) 
            {
                base.ResolveTopLevelNames();
            }

        }

        internal override void WriteIdentity(StringBuilder builder)
        {

            if (UnresolvedType != null && !UnresolvedType.Trim().Equals(String.Empty))
            {
                builder.Append("Collection(" + UnresolvedType + ")");
            }
            else
            {
                builder.Append("Collection(");
                _typeSubElement.WriteIdentity(builder);
                builder.Append(")");
            }
        }

        internal override TypeUsage GetTypeUsage()
        {
            if (_typeUsage != null)
            {
                return _typeUsage;
            }
            Debug.Assert(_typeSubElement != null, "For attributes typeusage should have been resolved");
            
            if (_typeSubElement != null)
            {
                CollectionType collectionType = new CollectionType(_typeSubElement.GetTypeUsage());

                collectionType.AddMetadataProperties(this.OtherContent);
                _typeUsage = TypeUsage.Create(collectionType);
            }
            return _typeUsage;
        }

        internal override bool ResolveNameAndSetTypeUsage(Converter.ConversionCache convertedItemCache, Dictionary<Som.SchemaElement, GlobalItem> newGlobalItems)
        {
            if (_typeUsage == null)
            {
                if (_typeSubElement != null) //Has sub-elements
                {
                    return _typeSubElement.ResolveNameAndSetTypeUsage(convertedItemCache, newGlobalItems);
                }
                else //Does not have sub-elements; try to resolve
                {
                    if (_type is ScalarType) //Create and store type usage for scalar type
                    {
                        _typeUsageBuilder.ValidateAndSetTypeUsage(_type as ScalarType, false);
                        _typeUsage = TypeUsage.Create(new CollectionType(_typeUsageBuilder.TypeUsage));
                        return true;
                    }
                    else  //Try to resolve edm type. If not now, it will resolve in the second pass
                    {
                        EdmType edmType = (EdmType)Converter.LoadSchemaElement(_type, _type.Schema.ProviderManifest, convertedItemCache, newGlobalItems);
                        if (edmType != null)
                        {
                            _typeUsageBuilder.ValidateAndSetTypeUsage(edmType, false); //use typeusagebuilder so dont lose facet information
                            _typeUsage = TypeUsage.Create(new CollectionType(_typeUsageBuilder.TypeUsage));
                        }

                        return _typeUsage != null;
                    }
                }
            }
            return true;
        }

        internal override void Validate()
        {
            base.Validate();

            ValidationHelper.ValidateFacets(this, _type, _typeUsageBuilder);
            ValidationHelper.ValidateTypeDeclaration(this, _type, _typeSubElement);

            if (_typeSubElement != null)
            {
                _typeSubElement.Validate();
            }
        }
    }
}
