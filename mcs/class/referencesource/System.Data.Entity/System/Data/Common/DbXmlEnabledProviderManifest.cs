//---------------------------------------------------------------------
// <copyright file="DbXmlEnabledProviderManifest.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
//---------------------------------------------------------------------

namespace System.Data.Common
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.EntityModel.SchemaObjectModel;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Xml;

    /// <summary>
    /// A specialization of the ProviderManifest that accepts an XmlReader
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public abstract class DbXmlEnabledProviderManifest : DbProviderManifest
    {
        private string _namespaceName;

        private System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType> _primitiveTypes;
        private Dictionary<PrimitiveType, System.Collections.ObjectModel.ReadOnlyCollection<FacetDescription>> _facetDescriptions = new Dictionary<PrimitiveType, System.Collections.ObjectModel.ReadOnlyCollection<FacetDescription>>();
        private System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> _functions;

        private Dictionary<string, PrimitiveType> _storeTypeNameToEdmPrimitiveType = new Dictionary<string, PrimitiveType>();
        private Dictionary<string, PrimitiveType> _storeTypeNameToStorePrimitiveType = new Dictionary<string, PrimitiveType>();

        protected DbXmlEnabledProviderManifest(XmlReader reader)
        {
            if (reader == null)
            {
                throw EntityUtil.ProviderIncompatible(Strings.IncorrectProviderManifest, new ArgumentNullException("reader"));
            }

            Load(reader);
        }

        #region Protected Properties For Fields

        public override string NamespaceName
        {
            get
            {
                return this._namespaceName;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        protected Dictionary<string, PrimitiveType> StoreTypeNameToEdmPrimitiveType
        {
            get
            {
                return this._storeTypeNameToEdmPrimitiveType;
            }
        }

        protected Dictionary<string, PrimitiveType> StoreTypeNameToStorePrimitiveType
        {
            get
            {
                return this._storeTypeNameToStorePrimitiveType;
            }
        }

        #endregion

        /// <summary>
        /// Returns all the FacetDescriptions for a particular type
        /// </summary>
        /// <param name="type">the type to return FacetDescriptions for.</param>
        /// <returns>The FacetDescriptions for the type given.</returns>
        public override System.Collections.ObjectModel.ReadOnlyCollection<FacetDescription> GetFacetDescriptions(EdmType type)
        {
            Debug.Assert(type is PrimitiveType, "DbXmlEnabledProviderManifest.GetFacetDescriptions(): Argument is not a PrimitiveType");
            return GetReadOnlyCollection(type as PrimitiveType, _facetDescriptions, Helper.EmptyFacetDescriptionEnumerable);
        }

        public override System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType> GetStoreTypes()
        {
            return _primitiveTypes;
        }

        /// <summary>
        /// Returns all the edm functions supported by the provider manifest.
        /// </summary>
        /// <returns>A collection of edm functions.</returns>
        public override System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> GetStoreFunctions()
        {
            return _functions;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private void Load(XmlReader reader)
        {
            Schema schema;
            IList<EdmSchemaError> errors = SchemaManager.LoadProviderManifest(reader, reader.BaseURI.Length > 0 ? reader.BaseURI : null, true /*checkForSystemNamespace*/, out schema);

            if (errors.Count != 0)
            {
                throw EntityUtil.ProviderIncompatible(Strings.IncorrectProviderManifest + Helper.CombineErrorMessage(errors));
            }

            _namespaceName = schema.Namespace;

            List<PrimitiveType> listOfPrimitiveTypes = new List<PrimitiveType>();
            foreach (System.Data.EntityModel.SchemaObjectModel.SchemaType schemaType in schema.SchemaTypes)
            {
                TypeElement typeElement = schemaType as TypeElement;
                if (typeElement != null)
                {
                    PrimitiveType type = typeElement.PrimitiveType;
                    type.ProviderManifest = this;
                    type.DataSpace = DataSpace.SSpace;
                    type.SetReadOnly();
                    listOfPrimitiveTypes.Add(type);

                    _storeTypeNameToStorePrimitiveType.Add(type.Name.ToLowerInvariant(), type);
                    _storeTypeNameToEdmPrimitiveType.Add(type.Name.ToLowerInvariant(), EdmProviderManifest.Instance.GetPrimitiveType(type.PrimitiveTypeKind));

                    System.Collections.ObjectModel.ReadOnlyCollection<FacetDescription> descriptions;
                    if (EnumerableToReadOnlyCollection(typeElement.FacetDescriptions, out descriptions))
                    {
                        _facetDescriptions.Add(type, descriptions);
                    }
                }
            }
            this._primitiveTypes = Array.AsReadOnly(listOfPrimitiveTypes.ToArray());

            // load the functions
            ItemCollection collection = new EmptyItemCollection();
            IEnumerable<GlobalItem> items = Converter.ConvertSchema(schema, this, collection);
            if (!EnumerableToReadOnlyCollection<EdmFunction, GlobalItem>(items, out this._functions))
            {
                this._functions = Helper.EmptyEdmFunctionReadOnlyCollection;
            }
            //SetReadOnly on all the Functions
            foreach (EdmFunction function in this._functions)
            {
                function.SetReadOnly();
            }
        }

        private static System.Collections.ObjectModel.ReadOnlyCollection<T> GetReadOnlyCollection<T>(PrimitiveType type, Dictionary<PrimitiveType, System.Collections.ObjectModel.ReadOnlyCollection<T>> typeDictionary, System.Collections.ObjectModel.ReadOnlyCollection<T> useIfEmpty)
        {
            System.Collections.ObjectModel.ReadOnlyCollection<T> collection;
            if (typeDictionary.TryGetValue(type, out collection))
            {
                return collection;
            }
            else
            {
                return useIfEmpty;
            }
        }

        private static bool EnumerableToReadOnlyCollection<Target, BaseType>(IEnumerable<BaseType> enumerable, out System.Collections.ObjectModel.ReadOnlyCollection<Target> collection) where Target : BaseType
        {
            List<Target> list = new List<Target>();
            foreach (BaseType item in enumerable)
            {
                if (typeof(Target) == typeof(BaseType) || item is Target)
                {
                    list.Add((Target)item);
                }
            }

            if (list.Count != 0)
            {
                collection = list.AsReadOnly();
                return true;
            }

            collection = null;
            return false;
        }

        private class EmptyItemCollection : ItemCollection
        {
            public EmptyItemCollection()
                : base(DataSpace.SSpace)
            {
            }
        }
    }
}
