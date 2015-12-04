//---------------------------------------------------------------------
// <copyright file="DbProviderManifest.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Common
{
    using System.Data.Metadata.Edm;
    using System.Xml;

    /// <summary>
    /// Metadata Interface for all CLR types types
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public abstract class DbProviderManifest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        protected DbProviderManifest()
        {
        }

        /// <summary>Value to pass to GetInformation to get the StoreSchemaDefinition</summary>
        public static readonly string StoreSchemaDefinition = "StoreSchemaDefinition";
        /// <summary>Value to pass to GetInformation to get the StoreSchemaMapping</summary>
        public static readonly string StoreSchemaMapping = "StoreSchemaMapping";
        /// <summary>Value to pass to GetInformation to get the ConceptualSchemaDefinition</summary>
        public static readonly string ConceptualSchemaDefinition = "ConceptualSchemaDefinition";

        /// <summary>Value to pass to GetInformation to get the StoreSchemaDefinitionVersion3</summary>
        public static readonly string StoreSchemaDefinitionVersion3 = "StoreSchemaDefinitionVersion3";
        /// <summary>Value to pass to GetInformation to get the StoreSchemaMappingVersion3</summary>
        public static readonly string StoreSchemaMappingVersion3 = "StoreSchemaMappingVersion3";
        /// <summary>Value to pass to GetInformation to get the ConceptualSchemaDefinitionVersion3</summary>
        public static readonly string ConceptualSchemaDefinitionVersion3 = "ConceptualSchemaDefinitionVersion3";

        // System Facet Info
        /// <summary>
        /// Name of the MaxLength Facet
        /// </summary>
        internal const string MaxLengthFacetName = "MaxLength";

        /// <summary>
        /// Name of the Unicode Facet
        /// </summary>
        internal const string UnicodeFacetName = "Unicode";

        /// <summary>
        /// Name of the FixedLength Facet
        /// </summary>
        internal const string FixedLengthFacetName = "FixedLength";

        /// <summary>
        /// Name of the Precision Facet
        /// </summary>
        internal const string PrecisionFacetName = "Precision";

        /// <summary>
        /// Name of the Scale Facet
        /// </summary>
        internal const string ScaleFacetName = "Scale";

        /// <summary>
        /// Name of the Nullable Facet
        /// </summary>
        internal const string NullableFacetName = "Nullable";

        /// <summary>
        /// Name of the DefaultValue Facet
        /// </summary>
        internal const string DefaultValueFacetName = "DefaultValue";

        /// <summary>
        /// Name of the Collation Facet
        /// </summary>
        internal const string CollationFacetName = "Collation";

        /// <summary>
        /// Name of the SRID Facet
        /// </summary>
        internal const string SridFacetName = "SRID";

        /// <summary>
        /// Name of the IsStrict Facet
        /// </summary>
        internal const string IsStrictFacetName = "IsStrict";
       
        /// <summary>
        /// Returns the namespace used by this provider manifest
        /// </summary>
        public abstract string NamespaceName {get;}
                
        /// <summary>
        /// Return the set of types supported by the store
        /// </summary>
        /// <returns>A collection of primitive types</returns>
        public abstract System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType> GetStoreTypes();

        /// <summary>
        /// Returns all the edm functions supported by the provider manifest.
        /// </summary>
        /// <returns>A collection of edm functions.</returns>
        public abstract System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> GetStoreFunctions();

        /// <summary>
        /// Returns all the FacetDescriptions for a particular type
        /// </summary>
        /// <param name="edmType">the type to return FacetDescriptions for</param>
        /// <returns>The FacetDescriptions for the type given</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public abstract System.Collections.ObjectModel.ReadOnlyCollection<FacetDescription> GetFacetDescriptions(EdmType edmType);

        /// <summary>
        /// This method allows a provider writer to take a type and a set of facets
        /// and reason about what the best mapped equivalent type in EDM would be.
        /// </summary>
        /// <param name="storeType">A TypeUsage encapsulating a store type and a set of facets</param>
        /// <returns>A TypeUsage encapsulating an EDM type and a set of facets</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        public abstract TypeUsage GetEdmType(TypeUsage storeType);

        /// <summary>
        /// This method allows a provider writer to take a type and a set of facets
        /// and reason about what the best mapped equivalent type in the store would be.
        /// </summary>
        /// <param name="storeType">A TypeUsage encapsulating an EDM type and a set of facets</param>
        /// <returns>A TypeUsage encapsulating a store type and a set of facets</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public abstract TypeUsage GetStoreType(TypeUsage edmType);

        /// <summary>
        /// Providers should override this to return information specific to their provider.  
        /// 
        /// This method should never return null.
        /// </summary>
        /// <param name="informationType">The name of the information to be retrieved.</param>
        /// <returns>An XmlReader at the begining of the information requested.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        protected abstract XmlReader GetDbInformation(string informationType);

        /// <summary>
        /// Gets framework and provider specific information
        /// 
        /// This method should never return null.
        /// </summary>
        /// <param name="informationType">The name of the information to be retrieved.</param>
        /// <returns>An XmlReader at the begining of the information requested.</returns>
        public XmlReader GetInformation(string informationType)
        {
            XmlReader reader = null;
            try
            {
                reader = GetDbInformation(informationType);
            }
            catch (Exception e)
            {
                // we should not be wrapping all exceptions
                if (EntityUtil.IsCatchableExceptionType(e))
                {
                    // we don't want folks to have to know all the various types of exceptions that can 
                    // occur, so we just rethrow a ProviderIncompatibleException and make whatever we caught  
                    // the inner exception of it.
                    throw EntityUtil.ProviderIncompatible(
                            System.Data.Entity.Strings.EntityClient_FailedToGetInformation(informationType), e);
                }
                throw;
            }
            if (reader == null)
            {
                // if the provider returned null for the conceptual schema definition, return the default one
                if (informationType == ConceptualSchemaDefinitionVersion3 ||
                    informationType == ConceptualSchemaDefinition)
                {
                    return DbProviderServices.GetConceptualSchemaDefinition(informationType);
                }

                throw EntityUtil.ProviderIncompatible(System.Data.Entity.Strings.ProviderReturnedNullForGetDbInformation(informationType));
            }
            return reader;

        }

        /// <summary>
        /// Does the provider support escaping strings to be used as patterns in a Like expression.
        /// If the provider overrides this method to return true, <cref = "EscapeLikeArgument"/> should 
        /// also be overridden. 
        /// </summary>
        /// <param name="escapeCharacter">If the provider supports escaping, the character that would be used
        /// as the escape character</param>
        /// <returns>True, if this provider supports escaping strings to be used as patterns in a Like expression,
        /// false otherwise. The default implementation returns false.</returns>
        public virtual bool SupportsEscapingLikeArgument(out char escapeCharacter)
        {
            escapeCharacter = default(char);
            return false;
        }

        /// <summary>
        /// Provider writers should override this method to returns the argument with the wildcards and the escape 
        /// character escaped.  This method is only used if <cref = "SupportsEscapingLikeArgument"/> returns true.
        /// </summary>
        /// <param name="argument">The argument to be escaped</param>
        /// <returns>The argument with the wildcards and the escape character escaped</returns>
        public virtual string EscapeLikeArgument(string argument)
        {
            throw EntityUtil.ProviderIncompatible(System.Data.Entity.Strings.ProviderShouldOverrideEscapeLikeArgument);
        }
    }
}
