//---------------------------------------------------------------------
// <copyright file="RowTypePropertyElement.cs" company="Microsoft">
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
    using System.Globalization;
    using System.Text;
    using System.Xml;
    using Som = System.Data.EntityModel.SchemaObjectModel;

    class RowTypePropertyElement : ModelFunctionTypeElement
    {
        private ModelFunctionTypeElement _typeSubElement = null;
        private bool _isRefType = false;
        private CollectionKind _collectionKind = CollectionKind.None;

        internal RowTypePropertyElement(SchemaElement parentElement)
            : base(parentElement)
        {
            _typeUsageBuilder = new TypeUsageBuilder(this);
        }

        internal override void ResolveTopLevelNames()
        {
            if (_unresolvedType != null)
            {
                base.ResolveTopLevelNames();
            }

            if (_typeSubElement != null)
            {
                _typeSubElement.ResolveTopLevelNames();
            }
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.TypeElement))
            {
                HandleTypeAttribute(reader);
                return true;
            }

            return false;
        }

        protected void HandleTypeAttribute(XmlReader reader)
        {
            Debug.Assert(reader != null);

            string type;
            if (!Utils.GetString(Schema, reader, out type))
                return;

            TypeModifier typeModifier;
            Function.RemoveTypeModifier(ref type, out typeModifier, out _isRefType);

            switch (typeModifier)
            {
                case TypeModifier.Array:
                    _collectionKind = CollectionKind.Bag;
                    break;
                default:
                    Debug.Assert(typeModifier == TypeModifier.None, string.Format(CultureInfo.CurrentCulture, "Type is not valid for property {0}: {1}. The modifier for the type cannot be used in this context.", FQName, reader.Value));
                    break;
            }

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

        internal override void WriteIdentity(StringBuilder builder)
        {
            builder.Append("Property(");

            if (UnresolvedType != null && !UnresolvedType.Trim().Equals(String.Empty))
            {
                if (_collectionKind != CollectionKind.None)
                {
                    builder.Append("Collection(" + UnresolvedType + ")");
                }
                else if (_isRefType)
                {
                    builder.Append("Ref(" + UnresolvedType + ")");
                }
                else
                {
                    builder.Append(UnresolvedType);
                }
            }
            else
            {
                _typeSubElement.WriteIdentity(builder);
            }

            builder.Append(")");
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
                _typeUsage = _typeSubElement.GetTypeUsage();
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
                        _typeUsage = _typeUsageBuilder.TypeUsage;
                    }
                    else  //Try to resolve edm type. If not now, it will resolve in the second pass
                    {
                        EdmType edmType = (EdmType)Converter.LoadSchemaElement(_type, _type.Schema.ProviderManifest, convertedItemCache, newGlobalItems);
                        if (edmType != null)
                        {
                            if (_isRefType)
                            {
                                EntityType entityType = edmType as EntityType;
                                Debug.Assert(entityType != null);
                                _typeUsage = TypeUsage.Create(new RefType(entityType));
                            }
                            else
                            {
                                _typeUsageBuilder.ValidateAndSetTypeUsage(edmType, false); //use typeusagebuilder so dont lose facet information
                                _typeUsage = _typeUsageBuilder.TypeUsage;
                            }
                        }
                    }
                    if (_collectionKind != CollectionKind.None)
                    {
                        _typeUsage = TypeUsage.Create(new CollectionType(_typeUsage));
                    }

                    return _typeUsage != null;
                }
            }
            return true;
        }

        /// <summary>
        /// True is property is scalar, otherwise false.
        /// During validation (after all types have been resolved).
        /// </summary>
        internal bool ValidateIsScalar()
        {
            if (_type != null)
            {
                if (_type is ScalarType == false || _isRefType || _collectionKind != CollectionKind.None)
                {
                    return false;
                }
            }
            else if (_typeSubElement != null)
            {
                if (_typeSubElement.Type is ScalarType == false)
                {
                    return false;
                }
            }
            return true;
        }

        internal override void Validate()
        {
            base.Validate();

            ValidationHelper.ValidateFacets(this, _type, _typeUsageBuilder);
            ValidationHelper.ValidateTypeDeclaration(this, _type, _typeSubElement);

            if (_isRefType)
            {
                ValidationHelper.ValidateRefType(this, _type);
            }

            if (_typeSubElement != null)
            {
                _typeSubElement.Validate();
            }
        }
    }
}
