//---------------------------------------------------------------------
// <copyright file="SchemaManager.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Common.Utils;
using System.Data.Entity;
using System.Data.Mapping;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace System.Data.EntityModel.SchemaObjectModel
{
    internal delegate void AttributeValueNotification(string token, Action<string, ErrorCode, EdmSchemaErrorSeverity> addError);
    internal delegate DbProviderManifest ProviderManifestNeeded(Action<string, ErrorCode, EdmSchemaErrorSeverity> addError);

    /// <summary>
    /// Class responsible for parsing,validating a collection of schema
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("DataModel={DataModel}")]
    internal class SchemaManager
    {
        #region Instance Fields
        // This keeps track of all the possible namespaces encountered till now. This helps in displaying the error to the
        // user - if the particular type is not found, we can report whether the namespace was invalid or the type with the
        // given name was not found in the given namespace. This also helps in validating the namespace in the using elements
        private readonly HashSet<string> _namespaceLookUpTable = new HashSet<string>(StringComparer.Ordinal);

        // List of all the schema types across all the schemas. This is to ensure that there is no duplicate type encountered
        // across schemas
        private readonly SchemaElementLookUpTable<SchemaType> _schemaTypes = new SchemaElementLookUpTable<SchemaType>();

        // We want to stop parsing/resolving/validation after the first 100 errors
        private const int MaxErrorCount = 100;

        // delay loaded
        private DbProviderManifest _providerManifest;
        private PrimitiveSchema _primitiveSchema;
        private double effectiveSchemaVersion = XmlConstants.UndefinedVersion;

        private readonly SchemaDataModelOption _dataModel;
        private readonly ProviderManifestNeeded _providerManifestNeeded;
        private readonly AttributeValueNotification _providerNotification;
        private readonly AttributeValueNotification _providerManifestTokenNotification;
        #endregion

        #region Constructor
        private SchemaManager(SchemaDataModelOption dataModel, AttributeValueNotification providerNotification, AttributeValueNotification providerManifestTokenNotification, ProviderManifestNeeded providerManifestNeeded)
        {
            _dataModel = dataModel;
            _providerNotification = providerNotification;
            _providerManifestTokenNotification = providerManifestTokenNotification;
            _providerManifestNeeded = providerManifestNeeded;
        }
        #endregion

        #region Public Methods


        
        public static IList<EdmSchemaError> LoadProviderManifest(XmlReader xmlReader, string location,
            bool checkForSystemNamespace, out Schema schema)
        {
            IList<Schema> schemaCollection = new List<Schema>(1);

            DbProviderManifest providerManifest = checkForSystemNamespace ? EdmProviderManifest.Instance : null;
            IList<EdmSchemaError> errors = ParseAndValidate(new XmlReader[] { xmlReader },
                new string[] { location }, SchemaDataModelOption.ProviderManifestModel,
                providerManifest, out schemaCollection);

            // In case of errors, there are no schema in the schema collection
            if (schemaCollection.Count != 0)
            {
                schema = schemaCollection[0];
            }
            else
            {
                Debug.Assert(errors.Count != 0, "There must be some error encountered");
                schema = null;
            }

            return errors;
        }

        public static void NoOpAttributeValueNotification(string attributeValue, Action<string, ErrorCode, EdmSchemaErrorSeverity> addError) { }
        
        public static IList<EdmSchemaError> ParseAndValidate(IEnumerable<XmlReader> xmlReaders,
            IEnumerable<string> sourceFilePaths, SchemaDataModelOption dataModel,
            DbProviderManifest providerManifest,
            out IList<Schema> schemaCollection)
        {
            return ParseAndValidate(xmlReaders, 
                sourceFilePaths, 
                dataModel,
                NoOpAttributeValueNotification,
                NoOpAttributeValueNotification,
                delegate(Action<string, ErrorCode, EdmSchemaErrorSeverity> addError) { return providerManifest == null ? MetadataItem.EdmProviderManifest : providerManifest; },
                out schemaCollection);
        }

        public static IList<EdmSchemaError> ParseAndValidate(IEnumerable<XmlReader> xmlReaders,
            IEnumerable<string> sourceFilePaths, SchemaDataModelOption dataModel,
            AttributeValueNotification providerNotification,
            AttributeValueNotification providerManifestTokenNotification,
            ProviderManifestNeeded providerManifestNeeded,
            out IList<Schema> schemaCollection)
        {
            SchemaManager schemaManager = new SchemaManager(dataModel, providerNotification, providerManifestTokenNotification, providerManifestNeeded);
            var errorCollection = new List<EdmSchemaError>();
            schemaCollection = new List<Schema>();
            bool errorEncountered = false;

            List<string> filePathList;
            if (sourceFilePaths != null)
            {
                filePathList = new List<string>(sourceFilePaths);
            }
            else
            {
                filePathList = new List<string>();
            }

            int index = 0;
            foreach (XmlReader xmlReader in xmlReaders)
            {
                string location = null;
                if (filePathList.Count <= index)
                {
                    TryGetBaseUri(xmlReader, out location);
                }
                else
                {
                    location = filePathList[index];
                }

                Schema schema;
                schema = new Schema(schemaManager);

                var errorsForCurrentSchema = schema.Parse(xmlReader, location);

                CheckIsSameVersion(schema, schemaCollection, errorCollection);
                
                // If the number of errors exceeded the max error count, then return
                if (UpdateErrorCollectionAndCheckForMaxErrors(errorCollection, errorsForCurrentSchema, ref errorEncountered))
                {
                    return errorCollection;
                }

                // Add the schema to the collection if there are no errors. There are errors in which schema do not have any namespace.
                // Also if there is an error encountered in one of the schema, we do not need to add the remaining schemas since
                // we will never go to the resolve phase.
                if (!errorEncountered)
                {
                    schemaCollection.Add(schema);
                    schemaManager.AddSchema(schema);
                    var currentSchemaVersion = schema.SchemaVersion;
                    Debug.Assert(schemaCollection.All(s => s.SchemaVersion == currentSchemaVersion || s.SchemaVersion != XmlConstants.UndefinedVersion));
                }
                index++;
            }

            // If there are no errors encountered in the parsing stage, we can proceed to the 
            // parsing and validating phase
            if (!errorEncountered)
            {
                foreach (Schema schema in schemaCollection)
                {
                    if (UpdateErrorCollectionAndCheckForMaxErrors(errorCollection, schema.Resolve(), ref errorEncountered))
                    {
                        return errorCollection;
                    }
                }

                // If there are no errors encountered in the parsing stage, we can proceed to the 
                // parsing and validating phase
                if (!errorEncountered)
                {
                    foreach (Schema schema in schemaCollection)
                    {
                        if (UpdateErrorCollectionAndCheckForMaxErrors(errorCollection, schema.ValidateSchema(), ref errorEncountered))
                        {
                            return errorCollection;
                        }
                    }
                }
            }
            
            return errorCollection;
        }

        // this method will move skip down to the first element, or to the end if it doesn't find one
        internal static bool TryGetSchemaVersion(XmlReader reader, out double version, out DataSpace dataSpace)
        {
            // to make life simpler, we skip down to the first/root element, unless we're
            // already there
            if (!reader.EOF && reader.NodeType != XmlNodeType.Element)
            {
                while (reader.Read() && reader.NodeType != XmlNodeType.Element)
                {
                }
            }

            if (!reader.EOF &&
                (reader.LocalName == XmlConstants.Schema || reader.LocalName == StorageMslConstructs.MappingElement))
            {
                return TryGetSchemaVersion(reader.NamespaceURI, out version, out dataSpace);
            }

            version = default(double);
            dataSpace = default(DataSpace);
            return false;
        }

        internal static bool TryGetSchemaVersion(string xmlNamespaceName, out double version, out DataSpace dataSpace)
        {
            switch (xmlNamespaceName)
            {
                case XmlConstants.ModelNamespace_1:
                    version = XmlConstants.EdmVersionForV1;
                    dataSpace = DataSpace.CSpace;
                    return true;
                case XmlConstants.ModelNamespace_1_1:
                    version = XmlConstants.EdmVersionForV1_1;
                    dataSpace = DataSpace.CSpace;
                    return true;
                case XmlConstants.ModelNamespace_2:
                    version = XmlConstants.EdmVersionForV2;
                    dataSpace = DataSpace.CSpace;
                    return true;
                case XmlConstants.ModelNamespace_3:
                    version = XmlConstants.EdmVersionForV3;
                    dataSpace = DataSpace.CSpace;
                    return true;
                case XmlConstants.TargetNamespace_1:
                    version = XmlConstants.StoreVersionForV1;
                    dataSpace = DataSpace.SSpace;
                    return true;
                case XmlConstants.TargetNamespace_2:
                    version = XmlConstants.StoreVersionForV2;
                    dataSpace = DataSpace.SSpace;
                    return true;
                case XmlConstants.TargetNamespace_3:
                    version = XmlConstants.StoreVersionForV3;
                    dataSpace = DataSpace.SSpace;
                    return true;
                case StorageMslConstructs.NamespaceUriV1:
                    version = StorageMslConstructs.MappingVersionV1;
                    dataSpace = DataSpace.CSSpace;
                    return true;
                case StorageMslConstructs.NamespaceUriV2:
                    version = StorageMslConstructs.MappingVersionV2;
                    dataSpace = DataSpace.CSSpace;
                    return true;
                case StorageMslConstructs.NamespaceUriV3:
                    version = StorageMslConstructs.MappingVersionV3;
                    dataSpace = DataSpace.CSSpace;
                    return true;
                default:
                    version = default(Double);
                    dataSpace = default(DataSpace);
                    return false;
            }
        }

        private static bool CheckIsSameVersion(Schema schemaToBeAdded, IEnumerable<Schema> schemaCollection, List<EdmSchemaError> errorCollection)
        {
            if (schemaToBeAdded.SchemaVersion != XmlConstants.UndefinedVersion && schemaCollection.Count() > 0)
            {
                if (schemaCollection.Any(s => s.SchemaVersion != XmlConstants.UndefinedVersion && s.SchemaVersion != schemaToBeAdded.SchemaVersion))
                {
                    errorCollection.Add(
                    new EdmSchemaError(
                        Strings.CannotLoadDifferentVersionOfSchemaInTheSameItemCollection,
                        (int)ErrorCode.CannotLoadDifferentVersionOfSchemaInTheSameItemCollection,
                        EdmSchemaErrorSeverity.Error));
                }
            }
            return true;
        }

        public double SchemaVersion { get { return this.effectiveSchemaVersion; } }

        /// <summary>
        /// Add the namespace of the given schema to the namespace lookup table
        /// </summary>
        public void AddSchema(Schema schema)
        {
            Debug.Assert(schema.DataModel == _dataModel, "DataModel must match");

            if (_namespaceLookUpTable.Count == 0 && schema.DataModel != SchemaDataModelOption.ProviderManifestModel)
            {
                // Add the primitive type namespace to the namespace look up table
                if (this.PrimitiveSchema.Namespace != null)
                {
                    _namespaceLookUpTable.Add(this.PrimitiveSchema.Namespace);
                }
            }
                                    
            // Add the namespace to the namespaceLookUpTable. 
            // Its okay to have multiple schemas with the same namespace
            _namespaceLookUpTable.Add(schema.Namespace);
        }

        /// <summary>
        /// Resolve the type - if the type is not found, return appropriate error
        /// </summary>
        /// <returns></returns>
        public bool TryResolveType(string namespaceName, string typeName, out SchemaType schemaType)
        {
            // For resolving entity container names, namespace can be null
            string fullyQualifiedName = String.IsNullOrEmpty(namespaceName) ? typeName : namespaceName + "." + typeName;

            schemaType = SchemaTypes.LookUpEquivalentKey(fullyQualifiedName);
            if (schemaType != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if this is a valid namespace name or else returns false
        /// </summary>
        public bool IsValidNamespaceName(string namespaceName)
        {
            return _namespaceLookUpTable.Contains(namespaceName);
        }
        #endregion // Public Methods

        #region Private Methods

        /// <summary>
        /// Checks if the xml reader has base uri. If it doesn't have, it adds error, other
        /// returns the location from the base uri
        /// </summary>
        /// <returns></returns>
        internal static bool TryGetBaseUri(XmlReader xmlReader, out string location)
        {
            string baseUri = xmlReader.BaseURI;
            Uri uri = null;

            if (!string.IsNullOrEmpty(baseUri) &&
                 Uri.TryCreate(baseUri, UriKind.Absolute, out uri) &&
                 uri.Scheme == "file")
            {
                location = Helper.GetFileNameFromUri(uri);
                return true;
            }
            else
            {
                location = null;                
                return false;
            }
        }

        /// <summary>
        /// Add the given list of newErrors to the error collection. If there is a error in the new errors,
        /// it sets the errorEncountered to true. Returns true if the number of errors encountered is more 
        /// than max errors
        /// </summary>
        /// <returns></returns>
        private static bool UpdateErrorCollectionAndCheckForMaxErrors(List<EdmSchemaError> errorCollection, 
            IList<EdmSchemaError> newErrors, ref bool errorEncountered)
        {
            // If we have encountered error already in one of the schemas, then we don't need to check for errors in the remaining schemas.
            // Just keep aggregating the errors and throw them at the end.
            if (!errorEncountered)
            {
                if (!MetadataHelper.CheckIfAllErrorsAreWarnings(newErrors))
                {
                    errorEncountered = true;
                }
            }

            // Add the new errors to the error collection
            errorCollection.AddRange(newErrors);

            if (errorEncountered && 
                errorCollection.Where(e => e.Severity == EdmSchemaErrorSeverity.Error).Count() > MaxErrorCount)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Internal Properties

        internal SchemaElementLookUpTable<SchemaType> SchemaTypes
        {
            get
            {
                return _schemaTypes;
            }
        }

        internal DbProviderManifest GetProviderManifest(Action<string, ErrorCode, EdmSchemaErrorSeverity> addError)
        { 
            if (_providerManifest == null)
            {
                _providerManifest = _providerManifestNeeded(addError);
            }
            return _providerManifest;
        }

        internal SchemaDataModelOption DataModel { get { return _dataModel; } }

        internal void EnsurePrimitiveSchemaIsLoaded(double forSchemaVersion)
        {
            if (_primitiveSchema == null)
            {
                this.effectiveSchemaVersion = forSchemaVersion;
                _primitiveSchema = new PrimitiveSchema(this);
            }
        }

        internal PrimitiveSchema PrimitiveSchema
        {
            get
            {
                return _primitiveSchema;
            }
        }

        internal AttributeValueNotification ProviderNotification
        {
            get
            {
                return _providerNotification;
            }
        }

        internal AttributeValueNotification ProviderManifestTokenNotification
        {
            get
            {
                return _providerManifestTokenNotification;
            }
        }

        #endregion
    }
}
