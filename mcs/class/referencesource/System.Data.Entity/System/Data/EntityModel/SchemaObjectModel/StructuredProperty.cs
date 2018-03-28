//---------------------------------------------------------------------
// <copyright file="StructuredProperty.cs" company="Microsoft">
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
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Xml;

    /// <summary>
    /// Summary description for StructuredProperty.
    /// </summary>
    internal class StructuredProperty : Property
    {
        #region Instance Fields
        private SchemaType _type = null;
        private string _unresolvedType = null;
        
        // Facets
        private TypeUsageBuilder _typeUsageBuilder;

        //Type of the Collection. By Default Single, and in case of Collections, will be either Bag or List
        private CollectionKind _collectionKind = CollectionKind.None;

        #endregion
        #region Static Fields
        
        //private static System.Text.RegularExpressions.Regex _binaryValueValidator = new System.Text.RegularExpressions.Regex("0[xX][0-9a-fA-F]+");
        #endregion
        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentElement"></param>
        internal StructuredProperty(StructuredType parentElement)
            : base(parentElement)
        {
            _typeUsageBuilder = new TypeUsageBuilder(this);
        }

        #endregion
        #region Public Properties

        /// <summary>
        /// 
        /// </summary>
        public override SchemaType Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Returns a TypeUsage that represent this property. 
        /// </summary>
        public TypeUsage TypeUsage
        {
            get
            {
                return _typeUsageBuilder.TypeUsage;
            }
        }
        
        /// <summary>
        /// The nullablity of this property.
        /// </summary>
        public bool Nullable
        {
            get
            {
                return _typeUsageBuilder.Nullable;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Default
        {
            get
            {
                return _typeUsageBuilder.Default;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public object DefaultAsObject
        {
            get
            {
                return _typeUsageBuilder.DefaultAsObject;
            }
        }


        /// <summary>
        /// Specifies the type of the Collection. 
        /// By Default this is Single( i.e. not a Collection.
        /// And in case of Collections, will be either Bag or List
        /// </summary>
        public CollectionKind CollectionKind
        {
            get
            {
                return _collectionKind;
            }
        }

        #endregion
        #region Internal Methods

        /// <summary>
        /// 
        /// </summary>
        internal override void ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();

            if (_type != null)
            {
                return;
            }

            _type = ResolveType(UnresolvedType);

            _typeUsageBuilder.ValidateDefaultValue(_type);

            ScalarType scalar = _type as ScalarType;
            if (scalar != null)
            {
                _typeUsageBuilder.ValidateAndSetTypeUsage(scalar, true);
            }
        }

        // 



        internal void EnsureEnumTypeFacets(Converter.ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
        {
            Debug.Assert(Type is SchemaEnumType);
            EdmType propertyType = (EdmType)Converter.LoadSchemaElement(Type, Type.Schema.ProviderManifest, convertedItemCache, newGlobalItems);
            _typeUsageBuilder.ValidateAndSetTypeUsage(propertyType, false);//use typeusagebuilder so dont lose facet information
        }

        /// <summary>
        /// Resolve the type string to a SchemaType object
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        protected virtual SchemaType ResolveType(string typeName)
        {
            SchemaType element;
            if (!Schema.ResolveTypeName(this, typeName, out element))
            {
                return null;
            }

            if (!(element is SchemaComplexType) && !(element is ScalarType) && !(element is SchemaEnumType))
            {
                AddError(ErrorCode.InvalidPropertyType, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.InvalidPropertyType(UnresolvedType));
                return null;
            }

            SchemaType structuredType = element as SchemaType;

            return element;
        }
        #endregion

        #region Internal Properties
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        internal string UnresolvedType
        {
            get
            {
                return _unresolvedType;
            }
            set
            {
                _unresolvedType = value;
            }
        }

        #endregion
        #region Protected Methods

        internal override void Validate()
        {
            base.Validate();
            //Non Complex Collections are not supported
            if ((_collectionKind == CollectionKind.Bag) ||
                (_collectionKind == CollectionKind.List))
            {
                Debug.Assert(Schema.SchemaVersion != XmlConstants.EdmVersionForV1,
                    "CollctionKind Attribute is not supported in EDM V1");
            }

            var schemaEnumType = this._type as SchemaEnumType;
            if (schemaEnumType != null)
            {
                this._typeUsageBuilder.ValidateEnumFacets(schemaEnumType);
            } 
            else if (Nullable && (this.Schema.SchemaVersion != XmlConstants.EdmVersionForV1_1)
                && (this._type is SchemaComplexType))
            {
                //Nullable Complex Types are not supported in V1.0, V2 and V3
                AddError(ErrorCode.NullableComplexType, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.ComplexObject_NullableComplexTypesNotSupported(this.FQName));
            }
        }

        #endregion
        #region Protected Properties
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
            else if (CanHandleAttribute(reader, XmlConstants.CollectionKind))
            {
                HandleCollectionKindAttribute(reader);
                return true;
            }
            else if (_typeUsageBuilder.HandleAttribute(reader))
            {
                return true;
            }
            return false;
        }
        #endregion
        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        private void HandleTypeAttribute(XmlReader reader)
        {
            if (UnresolvedType != null)
            {
                AddError(ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error, reader,
                    System.Data.Entity.Strings.PropertyTypeAlreadyDefined(reader.Name));
                return;
            }

            string type;
            if (!Utils.GetDottedName(Schema, reader, out type))
                return;

            UnresolvedType = type;
        }

        /// <summary>
        /// Handles the Multiplicity attribute on the property.
        /// </summary>
        /// <param name="reader"></param>
        private void HandleCollectionKindAttribute(XmlReader reader)
        {
            string value = reader.Value;
            if (value == XmlConstants.CollectionKind_None)
            {
                _collectionKind = CollectionKind.None;
            }
            else
            {
                if (value == XmlConstants.CollectionKind_List)
                {
                    _collectionKind = CollectionKind.List;
                }
                else if (value == XmlConstants.CollectionKind_Bag)
                {
                    _collectionKind = CollectionKind.Bag;
                }
                else
                {
                    Debug.Fail("Xsd should have changed", "XSD validation should have ensured that" +
                        " Multiplicity attribute has only 'None' or 'Bag' or 'List' as the values");
                    return;
                }
            }
        }

        #endregion
    }
}
