//---------------------------------------------------------------------
// <copyright file="MetadataItemSerializer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Text;
using System.Data.Metadata.Edm;
using System.Reflection;
using System.Diagnostics;
using System.Xml;
using System.Globalization;
using System.IO;
using System.Data.Entity.Design.SsdlGenerator;
using System.Linq;

namespace System.Data.Entity.Design.Common
{
    /// <summary>
    /// This class is reponsible for serailizing Edm Metadata out to the appropriate file .csdl or .ssdl
    /// </summary>
    internal class MetadataItemSerializer
    {
        public static readonly EdmType NoSpecificTypeSentinal = MetadataItem.GetBuiltInType(BuiltInTypeKind.EdmType);

        private bool _isModel;
        private ErrorsLookup _errorsLookup;
        private XmlWriter _writer;
        private Version _schemaVersion;

        private MetadataItemSerializer(XmlWriter writer, bool isModel, ErrorsLookup errorsLookup, Version schemaVersion)
        {
            _writer = writer;
            _isModel = isModel;
            _errorsLookup = errorsLookup;
            _schemaVersion = schemaVersion;
        }

        public class ErrorsLookup : Dictionary<MetadataItem, List<EdmSchemaError>> { }

        internal readonly string EdmNamespace = "Edm";

        public static void WriteXml(XmlWriter writer, ItemCollection collection, string namespaceToWrite, Version schemaVersion, params KeyValuePair<string, string> [] xmlPrefixToNamespaces)
        {
            WriteXml(writer, collection, namespaceToWrite, new ErrorsLookup(), new List<EdmType>(), null, null, schemaVersion, xmlPrefixToNamespaces);
        }

        internal static void WriteXml(XmlWriter writer, ItemCollection collection, string namespaceToWrite, ErrorsLookup errorsLookup, List<EdmType> commentedOutItems, string provider, string providerManifestToken, Version schemaVersion, params KeyValuePair<string, string>[] xmlPrefixToNamespaces)
        {
            Debug.Assert(writer != null, "writer parameter is null");
            Debug.Assert(collection != null, "collection parameter is null");
            Debug.Assert(errorsLookup != null, "errorsLookup parameter is null");
            Debug.Assert(!string.IsNullOrEmpty(namespaceToWrite), "namespaceToWrite parameter is null or empty");
            
            MetadataItemSerializer serializer = new MetadataItemSerializer(writer, collection.DataSpace == DataSpace.CSpace, errorsLookup, schemaVersion);

            serializer.ValidateNamespace(namespaceToWrite);
            serializer.WriteSchemaElement(namespaceToWrite, provider, providerManifestToken, xmlPrefixToNamespaces);
            serializer.WriteErrorsComment(NoSpecificTypeSentinal);
            foreach (EntityContainer item in collection.GetItems<EntityContainer>())
            {
                serializer.WriteEntityContainerElement(item);
            }

            foreach (EdmType type in collection.GetItems<EdmType>())
            {
                // is it in the right space (c or s)
                // does it have the right namespace?
                if (type.NamespaceName == namespaceToWrite)
                {
                    serializer.WriteTypeElement(type);
                }
            }

            if(commentedOutItems.Count > 0)
            {
                StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.OmitXmlDeclaration = true;
                // we can have more than one commented out type
                // which will look like multiple root elements, so this is a fragment
                settings.ConformanceLevel = ConformanceLevel.Fragment;
                XmlWriter commentWriter = XmlWriter.Create(stringWriter, settings);
                MetadataItemSerializer commmentSerializer = new MetadataItemSerializer(commentWriter, collection.DataSpace == DataSpace.CSpace, errorsLookup, schemaVersion);
                foreach (EdmType type in commentedOutItems)
                {
                    commmentSerializer.WriteTypeElement(type);
                }
                commentWriter.Flush();
                //This is not the cleanest thing to do but XmlTextWriter 
                //does not allow writing xml comment characters while writing a comment.
                //and since we know exactly the string we write, this is pretty safe.
                string comment = RemoveXmlCommentCharacters(stringWriter);
                writer.WriteComment(comment);
            }
            writer.WriteEndElement();
        }

        private static string RemoveXmlCommentCharacters(StringWriter stringWriter)
        {
            string comment = stringWriter.GetStringBuilder().ToString();
            while (comment.Contains(XmlConstants.XmlCommentStartString))
            {
                comment = comment.Replace(XmlConstants.XmlCommentStartString, String.Empty);
            }
            while (comment.Contains(XmlConstants.XmlCommentEndString))
            {
                comment = comment.Replace(XmlConstants.XmlCommentEndString, String.Empty);
            }
            return comment;
        }

        private void ValidateNamespace(string namespaceToWrite)
        {
            if (EdmItemCollection.IsSystemNamespace(MetadataItem.EdmProviderManifest, namespaceToWrite))
            {
                throw EDesignUtil.EdmReservedNamespace(namespaceToWrite);
            }
        }

        private void WriteTypeElement(EdmType type)
        {
            WriteErrorsComment(type);
            switch (type.BuiltInTypeKind)
            {
                case BuiltInTypeKind.EntityType:
                    WriteEntityTypeElement((EntityType)type);
                    break;
                case BuiltInTypeKind.AssociationType:
                    WriteAssociationTypeElement((AssociationType)type);
                    break;
                case BuiltInTypeKind.EdmFunction:
                    WriteFunctionElement((EdmFunction)type);
                    break;
                case BuiltInTypeKind.ComplexType:
                    WriteComplexTypeElement((ComplexType)type);
                    break;
                case BuiltInTypeKind.RowType:
                    WriteRowTypeElement((RowType)type);
                    break;
                default:
                    throw EDesignUtil.NonSerializableType(type.BuiltInTypeKind);
            }
        }

        private void WriteFunctionElement(EdmFunction function)
        {
            _writer.WriteStartElement(function.IsFunctionImport ? XmlConstants.FunctionImport : XmlConstants.Function);
            _writer.WriteAttributeString(XmlConstants.Name, function.Name);

            // Write function ReturnType as attribute if possible.
            bool returnParameterHandled = false;
            if (function.ReturnParameter != null)
            {
                var returnTypeUsage = function.ReturnParameter.TypeUsage;
                bool collection = returnTypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType;
                if (collection)
                {
                    Debug.Assert(_schemaVersion >= EntityFrameworkVersions.Version3, "_schemaVersion >= EntityFrameworkVersions.Version3");
                    returnTypeUsage = ((CollectionType)returnTypeUsage.EdmType).TypeUsage;
                }
                if (TypeSemantics.IsPrimitiveType(returnTypeUsage) || TypeSemantics.IsNominalType(returnTypeUsage))
                {
                    string typeName = GetFullName(returnTypeUsage.EdmType);
                    if (collection)
                    {
                        typeName = "Collection(" + typeName + ")";
                    }
                    _writer.WriteAttributeString(XmlConstants.ReturnType, typeName);
                    returnParameterHandled = true;
                }
            }

            if (!_isModel)
            {
                _writer.WriteAttributeString(XmlConstants.AggregateAttribute, GetAttributeValueString(function.AggregateAttribute));
                _writer.WriteAttributeString(XmlConstants.BuiltInAttribute, GetAttributeValueString(function.BuiltInAttribute));
                _writer.WriteAttributeString(XmlConstants.NiladicFunction, GetAttributeValueString(function.NiladicFunctionAttribute));
                _writer.WriteAttributeString(XmlConstants.IsComposable, GetAttributeValueString(function.IsComposableAttribute));
                _writer.WriteAttributeString(XmlConstants.ParameterTypeSemantics, GetAttributeValueString(function.ParameterTypeSemanticsAttribute));
            }
            else if (function.IsFunctionImport && function.IsComposableAttribute)
            {
                Debug.Assert(_schemaVersion >= EntityFrameworkVersions.Version3, "_schemaVersion >= EntityFrameworkVersions.Version3");
                _writer.WriteAttributeString(XmlConstants.IsComposable, GetAttributeValueString(true));
            }
            
            if (function.StoreFunctionNameAttribute != null)
            {
                _writer.WriteAttributeString(XmlConstants.StoreFunctionName, function.StoreFunctionNameAttribute);
            }
            
            if(function.CommandTextAttribute != null)
            {
                Debug.Assert(!_isModel, "Serialization of CommandTextAttribute is not supported for CSDL.");
                _writer.WriteAttributeString(XmlConstants.CommandText, function.CommandTextAttribute);
            }

            if (function.Schema != null)
            {
                _writer.WriteAttributeString(XmlConstants.Schema, function.Schema);
            }

            foreach (FunctionParameter parameter in function.Parameters)
            {
                WriteFunctionParameterElement(parameter);
            }

            // Write function ReturnType subelement if needed.
            if (function.ReturnParameter != null && !returnParameterHandled)
            {
                // Handle a TVF in s-space: Collection(RowType)
                if (function.ReturnParameter.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType)
                {
                    Debug.Assert(_schemaVersion >= EntityFrameworkVersions.Version3 && !_isModel, "_schemaVersion >= EntityFrameworkVersions.Version3 && !_isModel");
                    var elementType = ((CollectionType)function.ReturnParameter.TypeUsage.EdmType).TypeUsage.EdmType;
                    Debug.Assert(elementType.BuiltInTypeKind == BuiltInTypeKind.RowType, "TVF return type is expected to be Collection(RowType)");
                    var rowType = (RowType)elementType;
                    _writer.WriteStartElement(XmlConstants.ReturnType);
                    _writer.WriteStartElement(XmlConstants.CollectionType);
                    WriteTypeElement(rowType);
                    _writer.WriteEndElement();
                    _writer.WriteEndElement();
                    returnParameterHandled = true;
                }
            }

            Debug.Assert(function.ReturnParameter == null || returnParameterHandled, "ReturnParameter was not handled.");
            _writer.WriteEndElement();
        }

        private void WriteFunctionParameterElement(FunctionParameter parameter)
        {
            _writer.WriteStartElement(XmlConstants.Parameter);
            _writer.WriteAttributeString(XmlConstants.Name, parameter.Name);
            _writer.WriteAttributeString(XmlConstants.TypeAttribute, GetFullName(parameter.TypeUsage.EdmType));
            if (!_isModel)
            {
                _writer.WriteAttributeString(XmlConstants.Mode, GetAttributeValueString(parameter.Mode));
            }
            _writer.WriteEndElement();
        }


        private void WriteComplexTypeElement(ComplexType complexType)
        {
            _writer.WriteStartElement(XmlConstants.ComplexType);
            _writer.WriteAttributeString(XmlConstants.Name, complexType.Name);
            if (complexType.BaseType != null)
            {
                _writer.WriteAttributeString(XmlConstants.BaseType, GetFullName(complexType.BaseType));
            }

            foreach (EdmMember member in complexType.GetDeclaredOnlyMembers<EdmMember>())
            {
                WritePropertyElement(member);
            }
            _writer.WriteEndElement();
        }

        private void WriteAssociationTypeElement(AssociationType associationType)
        {
            _writer.WriteStartElement(XmlConstants.Association);
            _writer.WriteAttributeString(XmlConstants.Name, associationType.Name);
            foreach (RelationshipEndMember end in associationType.RelationshipEndMembers)
            {
                WriteRelationshipEndElement(end);
            }

            foreach (ReferentialConstraint constraint in associationType.ReferentialConstraints)
            {
                WriteReferentialConstraintElement(constraint);
            }

            _writer.WriteEndElement();
        }

        private void WriteRowTypeElement(RowType rowType)
        {
            _writer.WriteStartElement(XmlConstants.RowType);
            foreach (var property in rowType.Properties)
            {
                WritePropertyElement(property);
            }
            _writer.WriteEndElement();
        }

        private void WriteReferentialConstraintElement(ReferentialConstraint constraint)
        {
            _writer.WriteStartElement(XmlConstants.ReferentialConstraint);
            WriteReferentialConstraintRoleElement(XmlConstants.PrincipalRole, constraint.FromRole, constraint.FromProperties);
            WriteReferentialConstraintRoleElement(XmlConstants.DependentRole, constraint.ToRole, constraint.ToProperties);
            _writer.WriteEndElement();
        }

        private void WriteReferentialConstraintRoleElement(string nodeName, RelationshipEndMember end, IList<EdmProperty> properties)
        {
            // Generate the principal and dependent role nodes
            _writer.WriteStartElement(nodeName);
            _writer.WriteAttributeString(XmlConstants.Role, end.Name);
            for (int i = 0; i < properties.Count; i++)
            {
                _writer.WriteStartElement(XmlConstants.PropertyRef);
                _writer.WriteAttributeString(XmlConstants.Name, properties[i].Name);
                _writer.WriteEndElement();
            }
            _writer.WriteEndElement();
        }

        private void WriteRelationshipEndElement(RelationshipEndMember end)
        {
            _writer.WriteStartElement(XmlConstants.End);
            _writer.WriteAttributeString(XmlConstants.Role, end.Name);

            string typeName = GetFullName(((RefType)end.TypeUsage.EdmType).ElementType);
            _writer.WriteAttributeString(XmlConstants.TypeAttribute, typeName);
            _writer.WriteAttributeString(XmlConstants.Multiplicity, GetXmlMultiplicity(end.RelationshipMultiplicity));
            if (end.DeleteBehavior != OperationAction.None)
            {
                WriteOperationActionElement(XmlConstants.OnDelete, end.DeleteBehavior);
            }
            _writer.WriteEndElement();
        }

        private void WriteOperationActionElement(string elementName, OperationAction operationAction)
        {
            _writer.WriteStartElement(elementName);
            _writer.WriteAttributeString(XmlConstants.Action, operationAction.ToString());
            _writer.WriteEndElement();
        }

        private string GetXmlMultiplicity(RelationshipMultiplicity relationshipMultiplicity)
        {
            switch(relationshipMultiplicity)
            {
                case RelationshipMultiplicity.Many:
                    return "*";
                case RelationshipMultiplicity.One:
                    return "1";
                case RelationshipMultiplicity.ZeroOrOne:
                    return "0..1";
                default:
                    Debug.Fail("Did you add a new RelationshipMultiplicity?");
                    return string.Empty;
            }
        }

        private void WriteEntityTypeElement(EntityType entityType)
        {
            _writer.WriteStartElement(XmlConstants.EntityType);
            _writer.WriteAttributeString(XmlConstants.Name, entityType.Name);
            if (entityType.BaseType != null)
            {
                _writer.WriteAttributeString(XmlConstants.BaseType, GetFullName(entityType.BaseType));
            }

            if (entityType.Abstract)
            {
                _writer.WriteAttributeString(XmlConstants.Abstract, XmlConstants.True);
            }

            if (entityType.KeyMembers.Count != 0 && 
                entityType.KeyMembers[0].DeclaringType == entityType) // they are declared on this entity
            {
                _writer.WriteStartElement(XmlConstants.Key);
                for (int i = 0; i < entityType.KeyMembers.Count; i++)
                {
                    _writer.WriteStartElement(XmlConstants.PropertyRef);
                    _writer.WriteAttributeString(XmlConstants.Name, entityType.KeyMembers[i].Name);
                    _writer.WriteEndElement();
                }
                _writer.WriteEndElement();
            }

            foreach (EdmProperty member in entityType.GetDeclaredOnlyMembers<EdmProperty>())
            {
                WritePropertyElement(member);
            }
            
            foreach (NavigationProperty navigationProperty in entityType.NavigationProperties )
            {
                if (navigationProperty.DeclaringType == entityType)
                {
                    WriteNavigationPropertyElement(navigationProperty);
                }
            }
            _writer.WriteEndElement();
        }

        private void WriteErrorsComment(EdmType type)
        {
            List<EdmSchemaError> errors;
            if (_errorsLookup.TryGetValue(type, out errors))
            {
                Debug.Assert(errors.Count > 0, "how did we get an empty errors collection?");

                StringBuilder builder = new StringBuilder();
                builder.AppendLine(Strings.MetadataItemErrorsFoundDuringGeneration);
                foreach (EdmSchemaError error in errors)
                {
                    builder.AppendLine(error.ToString());
                }
                _writer.WriteComment(builder.ToString());
            }
        }

        private void WriteNavigationPropertyElement(NavigationProperty member)
        {
            _writer.WriteStartElement(XmlConstants.NavigationProperty);
            _writer.WriteAttributeString(XmlConstants.Name, member.Name);
            _writer.WriteAttributeString(XmlConstants.Relationship, member.RelationshipType.FullName);
            _writer.WriteAttributeString(XmlConstants.FromRole, member.FromEndMember.Name);
            _writer.WriteAttributeString(XmlConstants.ToRole, member.ToEndMember.Name);
            _writer.WriteEndElement();
        }

        private void WritePropertyElement(EdmMember member)
        {
            _writer.WriteStartElement(XmlConstants.Property);
            _writer.WriteAttributeString(XmlConstants.Name, member.Name);
            _writer.WriteAttributeString(XmlConstants.TypeAttribute, GetTypeName(member.TypeUsage));
            WritePropertyTypeFacets(member.TypeUsage);

            //
            // Generate "annotation:StoreGeneratedPattern="Identity"" for model schema
            //
            if (_isModel && member.MetadataProperties.Contains(DesignXmlConstants.EdmAnnotationNamespace + ":" + DesignXmlConstants.StoreGeneratedPattern))
            {
                _writer.WriteAttributeString(
                    TranslateFacetNameToAttributeName(
                        DesignXmlConstants.StoreGeneratedPattern),
                    DesignXmlConstants.EdmAnnotationNamespace, 
                    GetAttributeValueString(
                        member.MetadataProperties[DesignXmlConstants.EdmAnnotationNamespace + ":" + DesignXmlConstants.StoreGeneratedPattern].Value));
            }

            _writer.WriteEndElement();
        }

        private void WritePropertyTypeFacets(TypeUsage typeUsage)
        {
            // we need to use the facets for this particular provider, not the ones that they type
            // may have been converted to (think CSDL types converted to provider types)
            EdmType type = GetEdmType(typeUsage);
            IEnumerable<FacetDescription> providerDescriptions = GetAssociatedFacetDescriptions(type);

            foreach (Facet facet in typeUsage.Facets)
            {
                FacetDescription providerFacetDescription = null;
                if (IsSpecialFacet(facet))
                {
                    providerFacetDescription = facet.Description;
                }
                else
                {
                    foreach (FacetDescription description in providerDescriptions)
                    {
                        if (description.FacetName == facet.Name)
                        {
                            providerFacetDescription = description;
                            break;
                        }
                    }
                }

                //
                // Don't emit this facet if we shouldn't
                //
                if (SkipFacet(facet, providerFacetDescription))
                {
                    continue;
                }

                //
                // Special case for MaxLength facet value of "Max"
                //
                if (_isModel && 
                    type.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
                {
                    PrimitiveType primitiveType = (PrimitiveType)type;

                    if ((primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.String ||
                         primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Binary) &&
                        facet.Name == DbProviderManifest.MaxLengthFacetName &&
                        Helper.IsUnboundedFacetValue(facet))
                    {
                        _writer.WriteAttributeString(TranslateFacetNameToAttributeName(facet.Name), XmlConstants.Max);
                        continue;
                    }
                }

                _writer.WriteAttributeString(TranslateFacetNameToAttributeName(facet.Name), GetAttributeValueString(facet.Value));
            }
        }

        
        private string TranslateFacetNameToAttributeName(string facetName)
        {
            if(DbProviderManifest.DefaultValueFacetName == facetName)
            {
                return XmlConstants.DefaultValueAttribute;
            }

            return facetName;
        }

        /// <summary>
        /// Should this facet be skipped ?
        /// A facet should be skipped if it satsifies one of the following
        ///   - the providerFacetDescription is null - (ie) the provider knows of no such facet
        ///   - the facetDescription indicates that the facet must have a constant value
        ///   - the facet value is null
        ///   - the facet value is the default value for the facet, and the facet is not required
        ///   - we're emitting a model schema, and the facet in question is one of the following
        ///       - MaxLength, FixedLength, Unicode, Collation, Precision, Scale, DateTimeKind
        /// </summary>
        /// <param name="facet">the facet in question</param>
        /// <param name="providerFacetDescription">facet description in the provider</param>
        /// <returns>true, if the facet should be skipped</returns>
        private bool SkipFacet(Facet facet, FacetDescription providerFacetDescription)
        {
            //
            // if the provider doesn't recognize it, it will complain
            // when it sees it; so don't put it in
            //
            if (providerFacetDescription == null) 
            {
                return true;
            }
            // skip it if it is constant for the current provider
            if (providerFacetDescription.IsConstant)
            {
                return true;
            }

            //
            // Null facets can and should be omitted
            //
            if (facet.Value == null)
            {
                return true;
            }

            //
            // skip if it is not required, and has the default value
            //
            if (!providerFacetDescription.IsRequired &&
                facet.Value.Equals(providerFacetDescription.DefaultValue))
            {
                return true;
            }

            return false;
        }

        private bool IsSpecialFacet(Facet facet)
        {
            if(_isModel)
            {
                return (facet.Name == "ClientAutoGenerated" ||
                        facet.Name == EdmProviderManifest.ConcurrencyModeFacetName ||
                        facet.Name == XmlConstants.StoreGeneratedPattern ||
                        facet.Name == DbProviderManifest.CollationFacetName);
            }
            else
            {
                return (facet.Name == EdmProviderManifest.StoreGeneratedPatternFacetName || 
                        facet.Name == DbProviderManifest.CollationFacetName);
            }
        }

        private IEnumerable<FacetDescription> GetAssociatedFacetDescriptions(EdmType type)
        {
            MethodInfo mi = typeof(EdmType).GetMethod("GetAssociatedFacetDescriptions", BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(mi != null, "Method GetAssociatedFacetDescriptions is missing");
            return (IEnumerable<FacetDescription>)mi.Invoke(type, new object[0]);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private string GetAttributeValueString(object o)
        {
            if (o.GetType() == typeof(bool))
            {
                return o.ToString().ToLower(CultureInfo.InvariantCulture);
            }
            else
            {
                return o.ToString();
            }
        }

        private EdmType GetEdmType(TypeUsage typeUsage)
        {
            if (_isModel)
            {
                return GetModelType(typeUsage.EdmType);
            }
            else
            {
                return typeUsage.EdmType;
            }
        }
        private string GetTypeName(TypeUsage typeUsage)
        {
            EdmType type = GetEdmType(typeUsage);
            if (type.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
            {
                return type.Name;
            }
            else
            {
                return GetFullName(type);
            }
        }

        private EdmType GetModelType(EdmType edmType)
        {
            if (edmType.BuiltInTypeKind != BuiltInTypeKind.PrimitiveType)
            {
                return edmType;
            }

            while (edmType != null && edmType.NamespaceName != EdmNamespace)
            {
                edmType = edmType.BaseType;
            }

            return edmType;
        }

        private void WriteSchemaElement(string schemaNamespace, string provider, string providerManifestToken, params KeyValuePair<string, string>[] xmlPrefixToNamespaces)
        {
            string xmlNamespace = EntityFrameworkVersions.GetSchemaNamespace(_schemaVersion, _isModel ? DataSpace.CSpace : DataSpace.SSpace);
            _writer.WriteStartElement(XmlConstants.Schema, xmlNamespace);
            _writer.WriteAttributeString(XmlConstants.Namespace, schemaNamespace);
            _writer.WriteAttributeString(XmlConstants.Alias, "Self");
            if (_isModel && _schemaVersion >= EntityFrameworkVersions.Version3)
            {
                _writer.WriteAttributeString(XmlConstants.UseStrongSpatialTypes, XmlConstants.AnnotationNamespace, XmlConstants.False);
            }
            if (!_isModel)
            {
                if (!string.IsNullOrEmpty(provider))
                {
                    _writer.WriteAttributeString(XmlConstants.Provider, provider);
                }

                if (!string.IsNullOrEmpty(providerManifestToken))
                {
                    _writer.WriteAttributeString(XmlConstants.ProviderManifestToken, providerManifestToken);
                }
            }

            // write out the extra xml namespaces and their pretty prefix
            foreach (KeyValuePair<string, string> xmlPrefixToNamespace in xmlPrefixToNamespaces)
            {
                // see http://www.w3.org/TR/2006/REC-xml-names-20060816/
                _writer.WriteAttributeString("xmlns", xmlPrefixToNamespace.Key, null, xmlPrefixToNamespace.Value);
            }
        }

        private void WriteEntityContainerElement(EntityContainer container)
        {
            _writer.WriteStartElement(XmlConstants.EntityContainer);
            _writer.WriteAttributeString(XmlConstants.Name, container.Name);

            //
            // Generate "annotation:LazyLoadingEnabled="true"" for model schema
            //
            if (_isModel && container.MetadataProperties.Contains(DesignXmlConstants.EdmAnnotationNamespace + ":" + DesignXmlConstants.LazyLoadingEnabled))
            {
                _writer.WriteAttributeString(
                    TranslateFacetNameToAttributeName(
                        DesignXmlConstants.LazyLoadingEnabled),
                    DesignXmlConstants.EdmAnnotationNamespace,
                    GetAttributeValueString(
                        container.MetadataProperties[DesignXmlConstants.EdmAnnotationNamespace + ":" + DesignXmlConstants.LazyLoadingEnabled].Value));
            }

            foreach (EntitySetBase set in container.BaseEntitySets)
            {
                switch (set.BuiltInTypeKind)
                {
                    case BuiltInTypeKind.EntitySet:
                        WriteEntitySetElement((EntitySet)set);
                        break;
                    case BuiltInTypeKind.AssociationSet:
                        WriteAssociationSetElement((AssociationSet)set);
                        break;
                    default:
                        throw EDesignUtil.NonSerializableType(set.BuiltInTypeKind);
                }
            }

            foreach (EdmFunction functionImport in container.FunctionImports.Where(fi => fi.IsComposableAttribute))
            {
                WriteFunctionElement(functionImport);
            }
            
            _writer.WriteEndElement();
        }

        private void WriteAssociationSetElement(AssociationSet associationSet)
        {
            _writer.WriteStartElement(XmlConstants.AssociationSet);
            _writer.WriteAttributeString(XmlConstants.Name, associationSet.Name);
            _writer.WriteAttributeString(XmlConstants.Association, GetFullName(associationSet.ElementType));
            
            foreach (AssociationSetEnd end in associationSet.AssociationSetEnds)
            {
                WriteAssociationSetEndElement(end);
            }
            _writer.WriteEndElement();
        }

        private void WriteAssociationSetEndElement(AssociationSetEnd end)
        {
            _writer.WriteStartElement(XmlConstants.End);
            _writer.WriteAttributeString(XmlConstants.Role, end.Name);
            _writer.WriteAttributeString(XmlConstants.EntitySet, end.EntitySet.Name);
            _writer.WriteEndElement();
        }

        private void WriteEntitySetElement(EntitySet entitySet)
        {
            _writer.WriteStartElement(XmlConstants.EntitySet);
            _writer.WriteAttributeString(XmlConstants.Name, entitySet.Name);
            _writer.WriteAttributeString(XmlConstants.EntityType, GetFullName(entitySet.ElementType));
            WriteExtendedPropertyAttributes(entitySet);

            MetadataProperty property;
            if (entitySet.MetadataProperties.TryGetValue(XmlConstants.DefiningQuery, false, out property) &&
                property.Value != null)
            {
                _writer.WriteStartElement(XmlConstants.DefiningQuery);
                _writer.WriteString(entitySet.DefiningQuery);
                _writer.WriteEndElement();
            }
            else
            {
                if (entitySet.MetadataProperties.TryGetValue(XmlConstants.Schema, false, out property) &&
                    property.Value != null)
                {
                    _writer.WriteAttributeString(property.Name, property.Value.ToString());
                }

                if (entitySet.MetadataProperties.TryGetValue(XmlConstants.Table, false, out property) &&
                    property.Value != null)
                {
                    _writer.WriteAttributeString(property.Name, property.Value.ToString());
                }
            }


            _writer.WriteEndElement();
        }

        private void WriteExtendedPropertyAttributes(MetadataItem item)
        {
            foreach (MetadataProperty property in item.MetadataProperties.Where(p => p.PropertyKind == PropertyKind.Extended))
            {
                string xmlNamespace, attributeName;
                if (MetadataUtil.TrySplitExtendedMetadataPropertyName(property.Name, out xmlNamespace, out attributeName))
                {
                    _writer.WriteAttributeString(attributeName, xmlNamespace, property.Value.ToString());
                }
            }
        }

        private string GetFullName(EdmType type)
        {
            string namespaceName = null;
            string name;
            string modifierFormat = null;

            if (type.BuiltInTypeKind == BuiltInTypeKind.CollectionType)
            {
                type = ((CollectionType)type).TypeUsage.EdmType;
                modifierFormat = "Collection({0})";
            }

            if (type.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
            {
                // primitive types are not required to be qualified   
                name = type.Name;
            }
            else
            {
                namespaceName = type.NamespaceName;
                name = type.Name;
            }

            string qualifiedTypeName;
            if (namespaceName == null)
            {
                qualifiedTypeName = name;
            }
            else
            {
                qualifiedTypeName = namespaceName + "." + name;
            }

            if (modifierFormat != null)
            {
                qualifiedTypeName = string.Format(CultureInfo.InvariantCulture, modifierFormat, qualifiedTypeName);
            }

            return qualifiedTypeName;
         }

    }
}
