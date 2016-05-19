//---------------------------------------------------------------------
// <copyright file="AttributeEmitter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.CodeDom;

using System.Diagnostics;
using System.Data.SqlTypes;
using System.Data.Metadata.Edm;
using System.Collections.Generic;
using System.Data.Entity.Design;
using System.Data.Entity.Design.Common;
using System.Data.EntityModel.SchemaObjectModel;
using System.Data.Entity.Design.SsdlGenerator;
using System.Globalization;


namespace System.Data.EntityModel.Emitters
{
    /// <summary>
    /// Summary description for AttributeEmitter.
    /// </summary>
    internal sealed class AttributeEmitter
    {
        TypeReference _typeReference;

        internal TypeReference TypeReference
        {
            get { return _typeReference; }
        }

        static readonly string AdoAttributeDataClassesNamespace = "System.Data.Objects.DataClasses";
        internal AttributeEmitter(TypeReference typeReference)
        {
            _typeReference = typeReference;
        }

        /// <summary>
        /// The method to be called to create the type level attributes for the ItemTypeEmitter
        /// </summary>
        /// <param name="emitter">The strongly typed emitter</param>
        /// <param name="typeDecl">The type declaration to add the attribues to.</param>
        public void EmitTypeAttributes(EntityTypeEmitter emitter, CodeTypeDeclaration typeDecl)
        {
            Debug.Assert(emitter != null, "emitter should not be null");
            Debug.Assert(typeDecl != null, "typeDecl should not be null");

            EmitSchemaTypeAttribute(FQAdoFrameworkDataClassesName("EdmEntityTypeAttribute"), emitter, typeDecl);
            
            CodeAttributeDeclaration attribute2 = EmitSimpleAttribute("System.Runtime.Serialization.DataContractAttribute");
            AttributeEmitter.AddNamedAttributeArguments(attribute2,
                    "IsReference", true);
            typeDecl.CustomAttributes.Add(attribute2);

            CodeAttributeDeclaration attribute3 = EmitSimpleAttribute("System.Serializable");
            typeDecl.CustomAttributes.Add(attribute3);

            EmitKnownTypeAttributes(emitter.Item, emitter.Generator, typeDecl);
        }

        private void EmitKnownTypeAttributes(EdmType baseType, ClientApiGenerator generator, CodeTypeDeclaration typeDecl)
        {
            foreach (EdmType edmType in generator.GetDirectSubTypes(baseType))
            {
                Debug.Assert(edmType.BaseType == baseType, "The types must be directly derived from basetype");
                
                CodeTypeReference subTypeRef;
                if (generator.Language == LanguageOption.GenerateCSharpCode)
                {
                    bool useGlobalPrefix = true;
                    subTypeRef = generator.GetFullyQualifiedTypeReference(edmType, useGlobalPrefix);
                }
                else
                {
                    Debug.Assert(generator.Language == LanguageOption.GenerateVBCode, "Did you add a new language?");
                    subTypeRef = generator.GetLeastPossibleQualifiedTypeReference(edmType);
                }
                CodeAttributeDeclaration attribute = EmitSimpleAttribute("System.Runtime.Serialization.KnownTypeAttribute", new CodeTypeOfExpression(subTypeRef));
                typeDecl.CustomAttributes.Add(attribute);
            }
        }

        /// <summary>
        /// The method to be called to create the type level attributes for the StructuredTypeEmitter
        /// </summary>
        /// <param name="emitter">The strongly typed emitter</param>
        /// <param name="typeDecl">The type declaration to add the attribues to.</param>
        public void EmitTypeAttributes(StructuredTypeEmitter emitter, CodeTypeDeclaration typeDecl)
        {
            Debug.Assert(emitter != null, "emitter should not be null");
            Debug.Assert(typeDecl != null, "typeDecl should not be null");

            // nothing to do here yet
        }

        /// <summary>
        /// The method to be called to create the type level attributes for the SchemaTypeEmitter
        /// </summary>
        /// <param name="emitter">The strongly typed emitter</param>
        /// <param name="typeDecl">The type declaration to add the attribues to.</param>
        public void EmitTypeAttributes(SchemaTypeEmitter emitter, CodeTypeDeclaration typeDecl)
        {
            Debug.Assert(emitter != null, "emitter should not be null");
            Debug.Assert(typeDecl != null, "typeDecl should not be null");
        }

        /// <summary>
        /// Common way to fill out EdmTypeAttribute derived attributes
        /// </summary>
        /// <param name="attributeName">Unqualified name of the attribute</param>
        /// <param name="emitter">The strongly typed emitter</param>
        /// <param name="typeDecl">The type declaration to add the attribues to.</param>
        public void EmitSchemaTypeAttribute(string attributeName, SchemaTypeEmitter emitter, CodeTypeDeclaration typeDecl)
        {
            // call the shared static version
            EdmType type = emitter.Item as EdmType;
            Debug.Assert(type != null, "type is not an EdmType");
            EmitSchemaTypeAttribute(attributeName, type, typeDecl as CodeTypeMember);
        }

        /// <summary>
        /// Shared code for adding a EdmTypeAttribute derived attribute including parameters to a type or property
        /// </summary>
        /// <param name="attributeName">Unqualified name of the attribute</param>
        /// <param name="type">The type or property type of the code that is having the attribute attached.</param>
        /// <param name="member">The type declaration to add the attribues to.</param>
        public void EmitSchemaTypeAttribute(string attributeName, EdmType type, CodeTypeMember member)
        {
            Debug.Assert(attributeName != null, "attributeName should not be null");
            Debug.Assert(type != null, "type should not be null");
            Debug.Assert(member != null, "typeDecl should not be null");

            // [mappingattribute(SchemaName="namespace",TypeName="classname")
            CodeAttributeDeclaration attribute = EmitSimpleAttribute(attributeName);
            AttributeEmitter.AddNamedAttributeArguments(attribute,
                    "NamespaceName", type.NamespaceName,
                    "Name", type.Name);

            member.CustomAttributes.Add(attribute);
        }

        /// <summary>
        /// Emit the attributes for the new navigation property
        /// </summary>
        /// <param name="generator">The ClientApiGenerator instance</param>
        /// <param name="targetRelationshipEnd">The relationship end that is being targeted</param>
        /// <param name="propertyDecl">The property declaration to attach the attribute to.</param>
        /// <param name="additionalAttributes">Additional attributes</param>
        public void EmitNavigationPropertyAttributes(ClientApiGenerator generator,
                                                     RelationshipEndMember targetRelationshipEnd, 
                                                     CodeMemberProperty propertyDecl,
                                                     List<CodeAttributeDeclaration> additionalAttributes)
        {
            CodeAttributeDeclaration attribute = EmitSimpleAttribute(FQAdoFrameworkDataClassesName("EdmRelationshipNavigationPropertyAttribute"),
                targetRelationshipEnd.DeclaringType.NamespaceName,
                targetRelationshipEnd.DeclaringType.Name,
                targetRelationshipEnd.Name);

            propertyDecl.CustomAttributes.Add(attribute);
            EmitGeneratedCodeAttribute(propertyDecl);
            if (additionalAttributes != null && additionalAttributes.Count > 0)
            {
                try
                {
                    propertyDecl.CustomAttributes.AddRange(additionalAttributes.ToArray());
                }
                catch (ArgumentNullException e)
                {
                    generator.AddError(Strings.InvalidAttributeSuppliedForProperty(propertyDecl.Name),
                                       ModelBuilderErrorCode.InvalidAttributeSuppliedForProperty,
                                       EdmSchemaErrorSeverity.Error,
                                       e);
                }
            }
        }
        
        //
        //
        // Emit
        //     [global::System.CodeDom.Compiler.GeneratedCode("System.Data.Entity.Design.EntityClassGenerator", "4.0.0.0")]
        //
        // this allows FxCop to skip analysis of these methods and types, it should not be applied to partial types, only the 
        // generated members of partial types
        //
        CodeAttributeDeclaration _GeneratedCodeAttribute;
        internal void EmitGeneratedCodeAttribute(CodeTypeMember member)
        {
            if(_GeneratedCodeAttribute == null)
            {
                _GeneratedCodeAttribute = EmitSimpleAttribute("System.CodeDom.Compiler.GeneratedCode",
                    "System.Data.Entity.Design.EntityClassGenerator",
                    typeof(EntityClassGenerator).Assembly.GetName().Version.ToString());
                
            }

            member.CustomAttributes.Add(_GeneratedCodeAttribute);
        }

        /// <summary>
        /// The method to be called to create the property level attributes for the PropertyEmitter
        /// </summary>
        /// <param name="emitter">The strongly typed emitter</param>
        /// <param name="propertyDecl">The type declaration to add the attribues to.</param>
        /// <param name="additionalAttributes">Additional attributes to emit</param>
        public void EmitPropertyAttributes(PropertyEmitter emitter,
                                           CodeMemberProperty propertyDecl,
                                           List<CodeAttributeDeclaration> additionalAttributes)
        {
            if (MetadataUtil.IsPrimitiveType(emitter.Item.TypeUsage.EdmType) || MetadataUtil.IsEnumerationType(emitter.Item.TypeUsage.EdmType))
            {
                CodeAttributeDeclaration scalarPropertyAttribute = EmitSimpleAttribute(FQAdoFrameworkDataClassesName("EdmScalarPropertyAttribute"));

                if (emitter.IsKeyProperty)
                {
                    Debug.Assert(emitter.Item.Nullable == false, "An EntityKeyProperty cannot be nullable.");

                    AttributeEmitter.AddNamedAttributeArguments(scalarPropertyAttribute, "EntityKeyProperty", true);
                }

                if (!emitter.Item.Nullable)
                {
                    AttributeEmitter.AddNamedAttributeArguments(scalarPropertyAttribute, "IsNullable", false);
                }

                propertyDecl.CustomAttributes.Add(scalarPropertyAttribute);
            }
            else //Complex property
            {
                Debug.Assert(MetadataUtil.IsComplexType(emitter.Item.TypeUsage.EdmType) ||
                             (MetadataUtil.IsCollectionType(emitter.Item.TypeUsage.EdmType)),
                             "not a complex type or a collection type");
                CodeAttributeDeclaration attribute = EmitSimpleAttribute(FQAdoFrameworkDataClassesName("EdmComplexPropertyAttribute"));
                propertyDecl.CustomAttributes.Add(attribute);

                // Have CodeDOM serialization set the properties on the ComplexObject, not the ComplexObject instance.
                attribute = EmitSimpleAttribute("System.ComponentModel.DesignerSerializationVisibility");
                AttributeEmitter.AddAttributeArguments(attribute,
                    new object[] { new CodePropertyReferenceExpression(
                        new CodeTypeReferenceExpression(TypeReference.ForType(
                            typeof(System.ComponentModel.DesignerSerializationVisibility))),"Content") });
                propertyDecl.CustomAttributes.Add(attribute);

                if (!MetadataUtil.IsCollectionType(emitter.Item.TypeUsage.EdmType))
                {
                    // Non-collection complex properties also need additional serialization attributes to force them to be explicitly serialized if they are null
                    // If this is omitted, null complex properties do not get explicitly set to null during deserialization, which causes
                    // them to be lazily constructed once the property is accessed after the entity is deserialized. If the property is
                    // actually null during serialiation, that means the user has explicitly set it, so we need to maintain that during serialization.
                    // This doesn't apply to collection types because they aren't lazily constructed and don't need this extra information.
                    attribute = EmitSimpleAttribute("System.Xml.Serialization.XmlElement");
                    AttributeEmitter.AddNamedAttributeArguments(attribute, "IsNullable", true);
                    propertyDecl.CustomAttributes.Add(attribute);

                    attribute = EmitSimpleAttribute("System.Xml.Serialization.SoapElement");
                    AttributeEmitter.AddNamedAttributeArguments(attribute, "IsNullable", true);
                    propertyDecl.CustomAttributes.Add(attribute);
                }

            }

            // serialization attribute
            AddDataMemberAttribute(propertyDecl);            

            if (additionalAttributes != null && additionalAttributes.Count > 0)
            {
                try
                {
                    propertyDecl.CustomAttributes.AddRange(additionalAttributes.ToArray());
                }
                catch (ArgumentNullException e)
                {
                    emitter.Generator.AddError(Strings.InvalidAttributeSuppliedForProperty(emitter.Item.Name),
                                               ModelBuilderErrorCode.InvalidAttributeSuppliedForProperty,
                                               EdmSchemaErrorSeverity.Error,
                                               e);
                }
            }

            EmitGeneratedCodeAttribute(propertyDecl);
        }

        /// <summary>
        /// The method to be called to create the type level attributes for the NestedTypeEmitter
        /// </summary>
        /// <param name="emitter">The strongly typed emitter</param>
        /// <param name="typeDecl">The type declaration to add the attribues to.</param>
        public void EmitTypeAttributes(ComplexTypeEmitter emitter, CodeTypeDeclaration typeDecl)
        {
            Debug.Assert(emitter != null, "emitter should not be null");
            Debug.Assert(typeDecl != null, "typeDecl should not be null");

            EmitSchemaTypeAttribute(FQAdoFrameworkDataClassesName("EdmComplexTypeAttribute"),
                emitter, typeDecl);

            CodeAttributeDeclaration attribute = EmitSimpleAttribute("System.Runtime.Serialization.DataContractAttribute");
            AttributeEmitter.AddNamedAttributeArguments(attribute,
                    "IsReference", true);
            typeDecl.CustomAttributes.Add(attribute);

            CodeAttributeDeclaration attribute2 = EmitSimpleAttribute("System.Serializable");
            typeDecl.CustomAttributes.Add(attribute2);

            EmitKnownTypeAttributes(emitter.Item, emitter.Generator, typeDecl);
        }

        #region Static Methods
        /// <summary>
        /// Returns the name qualified with the Ado.Net EDM DataClasses Attribute namespace
        /// </summary>
        /// <param name="unqualifiedName"></param>
        /// <returns></returns>
        public static string FQAdoFrameworkDataClassesName(string unqualifiedName)
        {
            return AdoAttributeDataClassesNamespace + "." + unqualifiedName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attributeType"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public CodeAttributeDeclaration EmitSimpleAttribute(string attributeType, params object[] arguments)
        {
            CodeAttributeDeclaration attribute = new CodeAttributeDeclaration(TypeReference.FromString(attributeType, true));

            AddAttributeArguments(attribute, arguments);

            return attribute;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="arguments"></param>
        public static void AddAttributeArguments(CodeAttributeDeclaration attribute, object[] arguments)
        {
            foreach (object argument in arguments)
            {
                CodeExpression expression = argument as CodeExpression;
                if (expression == null)
                    expression = new CodePrimitiveExpression(argument);
                attribute.Arguments.Add(new CodeAttributeArgument(expression));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="arguments"></param>
        public static void AddNamedAttributeArguments(CodeAttributeDeclaration attribute, params object[] arguments)
        {
            for (int i = 1; i < arguments.Length; i += 2)
            {
                CodeExpression expression = arguments[i] as CodeExpression;
                if (expression == null)
                    expression = new CodePrimitiveExpression(arguments[i]);
                attribute.Arguments.Add(new CodeAttributeArgument(arguments[i - 1].ToString(), expression));
            }
        }

        /// <summary>
        /// Adds an XmlIgnore attribute to the given property declaration.  This is 
        /// used to explicitly skip certain properties during XML serialization.
        /// </summary>
        /// <param name="propertyDecl">the property to mark with XmlIgnore</param>
        public void AddIgnoreAttributes(CodeMemberProperty propertyDecl)
        {
            CodeAttributeDeclaration xmlIgnoreAttribute = EmitSimpleAttribute(typeof(System.Xml.Serialization.XmlIgnoreAttribute).FullName);
            CodeAttributeDeclaration soapIgnoreAttribute = EmitSimpleAttribute(typeof(System.Xml.Serialization.SoapIgnoreAttribute).FullName);
            propertyDecl.CustomAttributes.Add(xmlIgnoreAttribute);
            propertyDecl.CustomAttributes.Add(soapIgnoreAttribute);

        }

        /// <summary>
        /// Adds an Browsable(false) attribute to the given property declaration.
        /// This is used to explicitly avoid display property in the PropertyGrid.
        /// </summary>
        /// <param name="propertyDecl">the property to mark with XmlIgnore</param>
        public void AddBrowsableAttribute(CodeMemberProperty propertyDecl)
        {
            CodeAttributeDeclaration browsableAttribute = EmitSimpleAttribute(typeof(System.ComponentModel.BrowsableAttribute).FullName, false);
            propertyDecl.CustomAttributes.Add(browsableAttribute);
        }

        public void AddDataMemberAttribute(CodeMemberProperty propertyDecl)
        {
            CodeAttributeDeclaration browsableAttribute = EmitSimpleAttribute("System.Runtime.Serialization.DataMemberAttribute");
            propertyDecl.CustomAttributes.Add(browsableAttribute);
        }

        #endregion
    
    }
}
