//---------------------------------------------------------------------
// <copyright file="PropertyEmitter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.CodeDom;
using System.Collections.Generic;
using System.Data.Entity.Design;
using System.Data.Entity.Design.Common;
using System.Data.Entity.Design.SsdlGenerator;
using System.Data.Metadata.Edm;
using System.Data.Objects.ELinq;
using System.Diagnostics;
using System.Globalization;

namespace System.Data.EntityModel.Emitters
{
    internal sealed class PropertyEmitter : PropertyEmitterBase
    {
        private CodeFieldReferenceExpression _fieldRef = null;
        private CodeFieldReferenceExpression _complexPropertyInitializedFieldRef = null;

        // statics
        private const string NestedStoreObjectCollection = "InlineObjectCollection";
        private const string DetachFromParentMethodName = "DetachFromParent";

        #region Public Methods

        public PropertyEmitter(ClientApiGenerator generator, EdmProperty property, bool declaringTypeUsesStandardBaseType)
            : base(generator, property, declaringTypeUsesStandardBaseType)
        {
        }

        /// <summary>
        /// Emit the declaration of the property for the class.
        /// </summary>
        /// <returns>The Property declaration pieces of the CodeDom.</returns>
        public CodeMemberProperty EmitPropertyDeclaration(CodeTypeReference propertyReturnType)
        {
            MemberAttributes scope = AccessibilityFromGettersAndSetters(Item);
            CodeMemberProperty memberProperty = EmitPropertyDeclaration(scope, propertyReturnType, IsVirtualProperty, HidesBaseClassProperty);

            memberProperty.HasSet = true;
            memberProperty.HasGet = true;

            return memberProperty;
        }

        /// <summary>
        /// Main method for Emitting property code.
        /// </summary>
        /// <param name="typeDecl">The CodeDom representation of the type that the property is being added to.</param>
        protected override void EmitProperty(CodeTypeDeclaration typeDecl)
        {
            CodeTypeReference typeRef = PropertyType;

            // raise the PropertyGenerated event
            //
            PropertyGeneratedEventArgs eventArgs = new PropertyGeneratedEventArgs(Item, FieldName, typeRef);
            this.Generator.RaisePropertyGeneratedEvent(eventArgs);

            // the event subscriber cannot change the return type of the property
            //
            DisallowReturnTypeChange(typeRef, eventArgs.ReturnType);

            CodeMemberProperty memberProperty = EmitPropertyDeclaration(eventArgs.ReturnType);
            if (memberProperty == null)
            {
                return;
            }

            EmitCustomAttributes(memberProperty, eventArgs.AdditionalAttributes);

            EmitPropertyGetter(memberProperty, eventArgs.AdditionalGetStatements);
            EmitPropertySetter(memberProperty, eventArgs.AdditionalSetStatements);
            typeDecl.Members.Add(memberProperty);

            EmitField(typeDecl, eventArgs.ReturnType);

            EmitPropertyOnChangePartialMethods(typeDecl, eventArgs.ReturnType);
        }

        /// <summary>
        /// Emit these methods as "abstract" and fix them up later to be "partial".
        /// CodeDOM does not support partial methods
        /// </summary>
        /// <param name="typeDecl"></param>
        private void EmitPropertyOnChangePartialMethods(CodeTypeDeclaration typeDecl, CodeTypeReference returnType)
        {
            CodeMemberMethod onChangingDomMethod = new CodeMemberMethod();
            Generator.AttributeEmitter.EmitGeneratedCodeAttribute(onChangingDomMethod);
            onChangingDomMethod.Name = OnChangingPartialMethodName(PropertyName);
            onChangingDomMethod.ReturnType = new CodeTypeReference(typeof(void));
            onChangingDomMethod.Attributes = MemberAttributes.Abstract | MemberAttributes.Public;
            onChangingDomMethod.Parameters.Add(new CodeParameterDeclarationExpression(returnType, "value"));
            typeDecl.Members.Add(onChangingDomMethod);

            CodeMemberMethod onChangedDomMethod = new CodeMemberMethod();
            Generator.AttributeEmitter.EmitGeneratedCodeAttribute(onChangedDomMethod);
            onChangedDomMethod.Name = OnChangedPartialMethodName(PropertyName);
            onChangedDomMethod.ReturnType = new CodeTypeReference(typeof(void));
            onChangedDomMethod.Attributes = MemberAttributes.Abstract | MemberAttributes.Public;
            typeDecl.Members.Add(onChangedDomMethod);

            Generator.FixUps.Add(new FixUp(PropertyClassName + "." + OnChangingPartialMethodName(PropertyName), FixUpType.MarkAbstractMethodAsPartial));
            Generator.FixUps.Add(new FixUp(PropertyClassName + "." + OnChangedPartialMethodName(PropertyName), FixUpType.MarkAbstractMethodAsPartial));
        }

        private void EmitField(CodeTypeDeclaration typeDecl, CodeTypeReference fieldType)
        {
            CodeMemberField memberField = new CodeMemberField(fieldType, FieldName);
            Generator.AttributeEmitter.EmitGeneratedCodeAttribute(memberField);

            memberField.Attributes = MemberAttributes.Private;
            if (HasDefault(Item))
            {
                memberField.InitExpression = GetDefaultValueExpression(Item);
            }

            typeDecl.Members.Add(memberField);

            if (MetadataUtil.IsComplexType(Item.TypeUsage.EdmType))
            {
                CodeMemberField complexInitField = new CodeMemberField(TypeReference.ForType(typeof(bool)), ComplexPropertyInitializedFieldName);
                Generator.AttributeEmitter.EmitGeneratedCodeAttribute(complexInitField);
                complexInitField.Attributes = MemberAttributes.Private;
                typeDecl.Members.Add(complexInitField);
            }
        }

        /// <summary>
        /// Get a reference to the base class DataObject
        /// </summary>
        public static CodeTypeReferenceExpression CreateEdmStructuralObjectRef(TypeReference typeReference)
        {
            return new CodeTypeReferenceExpression(typeReference.ForType(typeof(System.Data.Objects.DataClasses.StructuralObject)));
        }

        #endregion

        #region Public Properties

        public CodeTypeReference PropertyType
        {
            get
            {
                CodeTypeReference typeRef = GetType(Item, false);
                return typeRef;
            }
        }

        public new EdmProperty Item
        {
            get
            {
                return base.Item as EdmProperty;
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Name of the associated Entity property for Ref(T) properties
        /// </summary>
        public string EntityPropertyName
        {
            get
            {
                return Item.Name;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberProperty"></param>
        /// <param name="additionalAttributes">Additional attributes to emit</param>
        private void EmitCustomAttributes(CodeMemberProperty memberProperty,
                                          List<CodeAttributeDeclaration> additionalAttributes)
        {
            Generator.AttributeEmitter.EmitPropertyAttributes(this, memberProperty, additionalAttributes);
        }

        private void EmitPropertyGetter(CodeMemberProperty memberProperty, List<CodeStatement> additionalGetStatements)
        {
            CodeStatementCollection statements = memberProperty.GetStatements;

            // we need to insert user-specified code before other/existing code, including
            // the return statement
            if (additionalGetStatements != null && additionalGetStatements.Count > 0)
            {
                try
                {
                    CodeStatementCollection getStatements = new CodeStatementCollection();
                    getStatements.AddRange(additionalGetStatements.ToArray());
                    if (statements != null && statements.Count > 0)
                    {
                        getStatements.AddRange(statements);
                    }
                    statements.Clear();
                    statements.AddRange(getStatements);
                }
                catch (ArgumentNullException e)
                {
                    Generator.AddError(Strings.InvalidGetStatementSuppliedForProperty(Item.Name),
                                       ModelBuilderErrorCode.InvalidGetStatementSuppliedForProperty,
                                       EdmSchemaErrorSeverity.Error,
                                       e);
                }
            }

            MemberAttributes access = memberProperty.Attributes & MemberAttributes.AccessMask;

            AddGetterSetterFixUp(Generator.FixUps, PropertyFQName, GetGetterAccessibility(Item), access, true);

            EmitPropertyGetterBody(statements);
        }

        private void EmitPropertyGetterBody(CodeStatementCollection statements)
        {
            // If the SchemaElement.Type isn't a ComplexType it better be PrimitiveType.
            if (MetadataUtil.IsComplexType(Item.TypeUsage.EdmType))
            {
                //Since Complex Collections are not supported by
                //the stack, we don't need to do anything special 
                //like doing an Attach or Detatch like the way we do for complex types.
                if (GetCollectionKind(Item.TypeUsage) == CollectionKind.None)
                {
                    // _field = GetValidValue( _field, FieldPropertyInfo, _fieldInitialized);
                    statements.Add(
                        new CodeAssignStatement(FieldRef,
                            new CodeMethodInvokeExpression(
                                    ThisRef,
                                    Utils.GetValidValueMethodName,
                                    new CodeDirectionExpression(FieldDirection.In, FieldRef),
                                    new CodePrimitiveExpression(PropertyName),
                                    new CodePrimitiveExpression(Item.Nullable),
                                    ComplexPropertyInitializedFieldRef)));

                    // this._complexPropertyInitialized = true;
                    statements.Add(
                        new CodeAssignStatement(
                            ComplexPropertyInitializedFieldRef,
                            new CodePrimitiveExpression(true)));
                }
                // return _field;
                statements.Add(new CodeMethodReturnStatement(FieldRef));
            }
            else
            {
                PrimitiveType primitiveType = Item.TypeUsage.EdmType as PrimitiveType;
                if (primitiveType != null && primitiveType.ClrEquivalentType == typeof(byte[]))
                {
                    // return GetValidValue(_field);
                    statements.Add(
                        new CodeMethodReturnStatement(
                            new CodeMethodInvokeExpression(
                                CreateEdmStructuralObjectRef(TypeReference),
                                Utils.GetValidValueMethodName,
                                this.FieldRef)));
                }
                else
                {
                    // for everything else just return the field.
                    statements.Add(new CodeMethodReturnStatement(FieldRef));
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberProperty"></param>
        /// <param name="additionalSetStatements">Additional statements to emit</param>
        private void EmitPropertySetter(CodeMemberProperty memberProperty, List<CodeStatement> additionalSetStatements)
        {
            CodeStatementCollection statements = memberProperty.SetStatements;

            MemberAttributes access = memberProperty.Attributes & MemberAttributes.AccessMask;

            AddGetterSetterFixUp(Generator.FixUps, PropertyFQName, GetSetterAccessibility(Item), access, false);

            EmitPropertySetterBody(statements, additionalSetStatements);
        }

        /// <summary>
        /// This is a control function to delegate the creation of the 
        /// setter statments to the correct code path
        /// </summary>
        /// <param name="statements">The collection that the setter statements should be added to.</param>
        /// <param name="additionalSetStatements">Additional statements to emit</param>
        private void EmitPropertySetterBody(CodeStatementCollection statements, List<CodeStatement> additionalSetStatements)
        {
            // Invoke the partial method "On[PropertyName]Changing();
            statements.Add(
                new CodeMethodInvokeExpression(
                    ThisRef,
                    OnChangingPartialMethodName(PropertyName), new CodePropertySetValueReferenceExpression()));

            // ReportPropertyChanging( _piFieldName );
            statements.Add(
                new CodeMethodInvokeExpression(
                    ThisRef,
                    Utils.ReportPropertyChangingMethodName,
                    new CodePrimitiveExpression(PropertyName)));

            // insert additional statements following the PropertyChanging event
            if (additionalSetStatements != null && additionalSetStatements.Count > 0)
            {
                try
                {
                    statements.AddRange(additionalSetStatements.ToArray());
                }
                catch (ArgumentNullException e)
                {
                    Generator.AddError(Strings.InvalidSetStatementSuppliedForProperty(Item.Name),
                                       ModelBuilderErrorCode.InvalidSetStatementSuppliedForProperty,
                                       EdmSchemaErrorSeverity.Error,
                                       e);
                }
            }

            if (MetadataUtil.IsPrimitiveType(Item.TypeUsage.EdmType))
            {
                EmitScalarTypePropertySetStatements(statements, CollectionKind.None);
            }
            else if (MetadataUtil.IsComplexType(Item.TypeUsage.EdmType))
            {
                // ComplexTypes have a completely different set pattern:
                EmitComplexTypePropertySetStatements(statements, CollectionKind.None);
            }
            else if (MetadataUtil.IsCollectionType(Item.TypeUsage.EdmType))
            {
                if (MetadataUtil.IsComplexType(((CollectionType)Item.TypeUsage.EdmType).TypeUsage.EdmType))
                {
                    EmitComplexTypePropertySetStatements(statements, GetCollectionKind(Item.TypeUsage));
                }
                else
                {
                    Debug.Assert(MetadataUtil.IsPrimitiveType(((CollectionType)Item.TypeUsage.EdmType).TypeUsage.EdmType),
                        "Collections should be of primitive types or complex types");
                    EmitScalarTypePropertySetStatements(statements, GetCollectionKind(Item.TypeUsage));
                }

            }
            else if (MetadataUtil.IsEnumerationType(Item.TypeUsage.EdmType))
            {
                // this.fieldName = value;
                statements.Add(
                    new CodeAssignStatement(
                            FieldRef,
                            new CodePropertySetValueReferenceExpression()));

            }

            // ReportPropertyChanged( _piFieldName );
            statements.Add(
                new CodeMethodInvokeExpression(
                    ThisRef,
                    Utils.ReportPropertyChangedMethodName,
                    new CodePrimitiveExpression(PropertyName)));

            // Invoke the partial method "On[PropertyName]Changed();
            statements.Add(
                new CodeMethodInvokeExpression(
                    ThisRef,
                    OnChangedPartialMethodName(PropertyName)));
        }

        /// <summary>
        /// Do the fixups to allow get and set statements in properties
        /// to have different accessibility than the property itself.
        /// </summary>
        /// <param name="accessibility">The accessibility for the getter or setter</param>
        /// <param name="propertyAccessibility">The property's accessibility</param>
        /// <param name="isGetter">True if this is a getter, false if a setter</param>
        internal static void AddGetterSetterFixUp(FixUpCollection fixups, string propertyFqName, MemberAttributes accessibility, MemberAttributes propertyAccessibility, bool isGetter)
        {
            Debug.Assert(GetAccessibilityRank(accessibility) >= 0, "bad accessibility");

            // Private
            if (accessibility == MemberAttributes.Private && propertyAccessibility != MemberAttributes.Private)
            {
                if (isGetter)
                {
                    fixups.Add(new FixUp(propertyFqName, FixUpType.MarkPropertyGetAsPrivate));
                }
                else
                {
                    fixups.Add(new FixUp(propertyFqName, FixUpType.MarkPropertySetAsPrivate));
                }
            }

            // Internal
            if (accessibility == MemberAttributes.Assembly && propertyAccessibility != MemberAttributes.Assembly)
            {
                if (isGetter)
                {
                    fixups.Add(new FixUp(propertyFqName, FixUpType.MarkPropertyGetAsInternal));
                }
                else
                {
                    fixups.Add(new FixUp(propertyFqName, FixUpType.MarkPropertySetAsInternal));
                }
            }

            // Public
            if (accessibility == MemberAttributes.Public && propertyAccessibility != MemberAttributes.Public)
            {
                if (isGetter)
                {
                    fixups.Add(new FixUp(propertyFqName, FixUpType.MarkPropertyGetAsPublic));
                }
                else
                {
                    fixups.Add(new FixUp(propertyFqName, FixUpType.MarkPropertySetAsPublic));
                }
            }

            // Protected
            if (accessibility == MemberAttributes.Family && propertyAccessibility != MemberAttributes.Family)
            {
                if (isGetter)
                {
                    fixups.Add(new FixUp(propertyFqName, FixUpType.MarkPropertyGetAsProtected));
                }
                else
                {
                    fixups.Add(new FixUp(propertyFqName, FixUpType.MarkPropertySetAsProtected));
                }
            }

        }

        /// <summary>
        /// Emit the set statements for a property that is a scalar type
        /// </summary>
        /// <param name="statements">The statement collection to add the set statements to.</param>
        private void EmitScalarTypePropertySetStatements(CodeStatementCollection statements,
            CollectionKind collectionKind)
        {
            Debug.Assert(statements != null, "statments can't be null");
            Debug.Assert(((MetadataUtil.IsPrimitiveType(Item.TypeUsage.EdmType)) || (MetadataUtil.IsCollectionType(Item.TypeUsage.EdmType)))
                , "Must be a primitive type or collection type property");

            CodePropertySetValueReferenceExpression valueRef = new CodePropertySetValueReferenceExpression();
            //Since collections are not supported by
            //the stack, we don't need to do anything special 
            //like doing an Attach or Detatch like the way we do for complex types.
            if (collectionKind == CollectionKind.None)
            {

                PrimitiveType primitiveType = (PrimitiveType)Item.TypeUsage.EdmType;

                // basic pattern
                // this.fieldName = SetValidValue( value );
                //
                List<CodeExpression> parameters = new List<CodeExpression>();
                parameters.Add(valueRef);


                // pattern for non Nullable<T> types (string, byte[])
                //
                // this.fieldName = SetValidVaue( value, nullability );

                if (primitiveType.ClrEquivalentType.IsClass)
                {
                    // ref types have an extra boolean parameter to tell if the property is allowed to 
                    // be null or not
                    parameters.Add(new CodePrimitiveExpression(Item.Nullable));
                }

                // now create and add the built statement
                statements.Add(
                    new CodeAssignStatement(
                            FieldRef,
                            new CodeMethodInvokeExpression(
                                CreateEdmStructuralObjectRef(TypeReference),
                                Utils.SetValidValueMethodName,
                                parameters.ToArray())));
            }
            else
            {
                // this.fieldName = value;
                statements.Add(
                    new CodeAssignStatement(
                        FieldRef, valueRef));

            }
        }

        private CodeExpression GetEnumValue<T>(T value)
        {
            Type type = typeof(T);
            return new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(TypeReference.ForType(type)), Enum.GetName(type, value));
        }

        /// <summary>
        /// Emit the property set statments to properly set a ComplexType.
        /// </summary>
        /// <param name="statements">The collection of statements that the set statements should be added to.</param>
        private void EmitComplexTypePropertySetStatements(CodeStatementCollection statements, CollectionKind collectionKind)
        {
            CodePropertySetValueReferenceExpression valueRef = new CodePropertySetValueReferenceExpression();
            //Since collections are not supported by
            //the stack, we don't need to do anything special 
            //like doing an Attach or Detatch like the way we do for complex types.
            if (collectionKind == CollectionKind.None)
            {

                // this.fieldName = SetValidValue( this.fieldName, value, _pifieldName);
                statements.Add(
                    new CodeAssignStatement(
                        FieldRef,
                        new CodeMethodInvokeExpression(
                                ThisRef,
                                Utils.SetValidValueMethodName,
                                FieldRef,
                                valueRef,
                                new CodePrimitiveExpression(PropertyName))));

                // this._complexPropertyInitialized = true;
                statements.Add(
                    new CodeAssignStatement(
                        ComplexPropertyInitializedFieldRef,
                        new CodePrimitiveExpression(true)));
            }
            else
            {
                // this.fieldName = value;
                statements.Add(
                    new CodeAssignStatement(
                        FieldRef, valueRef));

            }

        }

        /// <summary>
        /// See if a property names will hide a base class member name
        /// </summary>
        private bool HidesBaseClassProperty
        {
            get
            {
                StructuralType parentBaseClass = Item.DeclaringType.BaseType as StructuralType;
                if (parentBaseClass != null && parentBaseClass.Members.Contains(PropertyName))
                    return true;

                return false;
            }
        }

        private CodeTypeReference GetType(EdmProperty property, bool getElementType)
        {
            PropertyTypeReferences types = default(PropertyTypeReferences);
            EdmType propertyType = property.TypeUsage.EdmType;

            // Initialize types
            if (MetadataUtil.IsPrimitiveType(propertyType))
            {
                types = new PropertyTypeReferences(TypeReference, (PrimitiveType)propertyType);
            }
            else if (MetadataUtil.IsComplexType(propertyType))
            {
                types = new PropertyTypeReferences(TypeReference, (ComplexType)propertyType, Generator);
            }
            else if (Helper.IsCollectionType(propertyType))
            {
                TypeUsage typeUsage = ((CollectionType)propertyType).TypeUsage;
                if (MetadataUtil.IsPrimitiveType(typeUsage.EdmType))
                {
                    types = new PropertyTypeReferences(TypeReference, (PrimitiveType)typeUsage.EdmType, GetCollectionKind(property.TypeUsage));
                }
                else
                {
                    Debug.Assert(MetadataUtil.IsComplexType(typeUsage.EdmType));
                    types = new PropertyTypeReferences(TypeReference, (ComplexType)typeUsage.EdmType, GetCollectionKind(property.TypeUsage), Generator);
                }
            }
            else
            {
                // shouldn't be able to get here....
                Debug.Fail("Unexpected Property.Type type: " + propertyType.GetType());
            }

            // Set types, or retrieve existing types if they have been set in the interim
            // Don't cache Collection types since CollectionKind is really a facet and
            //it is not part of the key we are using for the dictionary used to cache.
            if (!Helper.IsCollectionType(propertyType))
            {
                Debug.Assert(types.NonNullable != null && types.Nullable != null, "did you forget to set the types variable?");
            }

            if (property.Nullable)
            {
                return types.Nullable;
            }
            else
            {
                return types.NonNullable;
            }
        }


        private static CollectionKind GetCollectionKind(TypeUsage usage)
        {
            Facet collectionFacet;
            if (usage.Facets.TryGetValue(EdmConstants.CollectionKind, false, out collectionFacet))
            {
                return (CollectionKind)collectionFacet.Value;
            }

            return CollectionKind.None;
        }

        private string OnChangingPartialMethodName(string propertyName) { return "On" + propertyName + "Changing"; }
        private string OnChangedPartialMethodName(string propertyName) { return "On" + propertyName + "Changed"; }

        #endregion

        #region Private Properties

        private CodeFieldReferenceExpression FieldRef
        {
            get
            {
                if (_fieldRef == null)
                    _fieldRef = new CodeFieldReferenceExpression(ThisRef, FieldName);

                return _fieldRef;
            }
        }

        private CodeFieldReferenceExpression ComplexPropertyInitializedFieldRef
        {
            get
            {
                if (_complexPropertyInitializedFieldRef == null)
                    _complexPropertyInitializedFieldRef = new CodeFieldReferenceExpression(ThisRef, ComplexPropertyInitializedFieldName);

                return _complexPropertyInitializedFieldRef;
            }
        }

        private string FieldName
        {
            get
            {
                return Utils.FieldNameFromPropName(PropertyName);
            }
        }

        private string ComplexPropertyInitializedFieldName
        {
            get
            {
                return Utils.ComplexPropertyInitializedNameFromPropName(PropertyName);
            }
        }

        internal bool IsKeyProperty
        {
            get
            {
                EntityType entity = Item.DeclaringType as EntityType;
                if (entity != null)
                {
                    return entity.KeyMembers.Contains(Item.Name);
                }
                return false;
            }
        }

        internal static bool HasDefault(EdmProperty property)
        {
            return property.DefaultValue != null;
        }

        private CodeExpression GetDefaultValueExpression(EdmProperty property)
        {
            PrimitiveTypeKind type;
            object value = property.DefaultValue;
            if (value != null
                 && Utils.TryGetPrimitiveTypeKind(property.TypeUsage.EdmType, out type))
            {
                if (!property.Nullable && value.Equals(TypeSystem.GetDefaultValue(value.GetType())))
                {
                    return null;
                }

                switch (type)
                {
                    case PrimitiveTypeKind.Boolean:
                    case PrimitiveTypeKind.Byte:
                    case PrimitiveTypeKind.Int16:
                    case PrimitiveTypeKind.Int32:
                    case PrimitiveTypeKind.Int64:
                    case PrimitiveTypeKind.Decimal:
                    case PrimitiveTypeKind.Single:
                    case PrimitiveTypeKind.Double:
                    case PrimitiveTypeKind.String:
                        {
                            return new CodePrimitiveExpression(value);
                        }
                    case PrimitiveTypeKind.Guid:
                        {
                            return GetCodeExpressionFromGuid(value);
                        }
                    case PrimitiveTypeKind.DateTime:
                        {
                            return GetCodeExpressionFromDateTimeDefaultValue(value, property);
                        }
                    case PrimitiveTypeKind.DateTimeOffset:
                        {
                            return GetCodeExpressionFromDateTimeOffsetDefaultValue(value, property);
                        }
                    case PrimitiveTypeKind.Time:
                        {
                            return GetCodeExpressionFromTimeSpanDefaultValue(value, property);
                        }
                    case PrimitiveTypeKind.Binary:
                        {
                            return GetCodeExpressionFromBinary(value);
                        }
                    default:
                        Debug.Fail("Unsupported property type:" + type.ToString());
                        break;
                }
                return null;
            }
            return null;
        }

        private CodeExpression GetCodeExpressionFromBinary(object value)
        {
            byte[] data = (byte[])value;
            CodeExpression[] bytes = new CodeExpression[data.Length];

            for (int iByte = 0; iByte < data.Length; ++iByte)
            {
                bytes[iByte] = new CodePrimitiveExpression(data[iByte]);
            }

            return new CodeArrayCreateExpression(TypeReference.ByteArray, bytes);
        }

        private CodeExpression GetCodeExpressionFromGuid(object value)
        {
            Guid guid = (Guid)value;
            return new CodeObjectCreateExpression(TypeReference.Guid,
                new CodePrimitiveExpression(guid.ToString("D", CultureInfo.InvariantCulture)));
        }

        private CodeExpression GetCodeExpressionFromDateTimeDefaultValue(object value, EdmProperty property)
        {
            DateTime utc = (DateTime)value;
            DateTime dateTime = DateTime.SpecifyKind(utc, DateTimeKind.Unspecified);

            return new CodeObjectCreateExpression(TypeReference.DateTime, new CodePrimitiveExpression(dateTime.Ticks), GetEnumValue(DateTimeKind.Unspecified));
        }

        private CodeExpression GetCodeExpressionFromDateTimeOffsetDefaultValue(object value, EdmProperty property)
        {
            DateTimeOffset dateTimeOffset = (DateTimeOffset)value;

            return new CodeObjectCreateExpression(TypeReference.DateTimeOffset, new CodePrimitiveExpression(dateTimeOffset.Ticks),
                new CodeObjectCreateExpression(TypeReference.TimeSpan, new CodePrimitiveExpression(dateTimeOffset.Offset.Ticks)));
        }

        private CodeExpression GetCodeExpressionFromTimeSpanDefaultValue(object value, EdmProperty property)
        {
            TimeSpan timeSpan = (TimeSpan)value;
            return new CodeObjectCreateExpression(TypeReference.TimeSpan, new CodePrimitiveExpression(timeSpan.Ticks));
        }

        public bool IsVirtualProperty
        {
            get
            {
                return false;
            }
        }

        private struct PropertyTypeReferences
        {
            CodeTypeReference _nonNullable;
            CodeTypeReference _nullable;
            public PropertyTypeReferences(TypeReference typeReference, PrimitiveType primitiveType)
                : this(typeReference, primitiveType, CollectionKind.None)
            {
            }

            public PropertyTypeReferences(TypeReference typeReference, PrimitiveType primitiveType, CollectionKind collectionKind)
            {
                Type type = primitiveType.ClrEquivalentType;
                if (collectionKind == CollectionKind.None)
                {
                    _nonNullable = typeReference.ForType(type);
                    if (type.IsValueType)
                    {
                        _nullable = typeReference.NullableForType(type);
                    }
                    else
                    {
                        _nullable = typeReference.ForType(type);
                    }
                }
                else
                {
                    CodeTypeReference primitiveTypeRef = typeReference.ForType(type);
                    CodeTypeReference collectionType = GetCollectionTypeReference(typeReference, primitiveTypeRef, collectionKind);
                    _nonNullable = collectionType;
                    _nullable = collectionType;
                }
            }

            public PropertyTypeReferences(TypeReference typeReference, ComplexType complexType, CollectionKind collectionKind, ClientApiGenerator generator)
            {
                CodeTypeReference baseType = generator.GetLeastPossibleQualifiedTypeReference(complexType);
                baseType = GetCollectionTypeReference(typeReference, baseType, collectionKind);
                _nonNullable = baseType;
                _nullable = baseType;
            }

            private static CodeTypeReference GetCollectionTypeReference(TypeReference typeReference, CodeTypeReference baseType, CollectionKind collectionKind)
            {
                if (collectionKind == CollectionKind.Bag)
                {
                    baseType = GetCollectionTypeReferenceForBagSemantics(typeReference, baseType);
                }
                else if (collectionKind == CollectionKind.List)
                {
                    baseType = GetCollectionTypeReferenceForListSemantics(typeReference, baseType);
                }
                else
                {
                    Debug.Assert(collectionKind == CollectionKind.None, "Was another CollectionKind value added");
                    // nothing more to do for .None
                }
                return baseType;
            }

            public PropertyTypeReferences(TypeReference typeReference, ComplexType complexType, ClientApiGenerator generator)
                : this(typeReference, complexType, CollectionKind.None, generator)
            {
            }

            private static CodeTypeReference GetCollectionTypeReferenceForBagSemantics(TypeReference typeReference, CodeTypeReference baseType)
            {
                CodeTypeReference typeRef = typeReference.ForType(typeof(System.Collections.Generic.ICollection<>), baseType);
                return typeRef;
            }

            private static CodeTypeReference GetCollectionTypeReferenceForListSemantics(TypeReference typeReference, CodeTypeReference baseType)
            {
                CodeTypeReference typeRef = typeReference.ForType(typeof(System.Collections.Generic.IList<>), baseType);
                return typeRef;
            }

            public CodeTypeReference NonNullable
            {
                get { return _nonNullable; }
            }
            public CodeTypeReference Nullable
            {
                get { return _nullable; }
            }
        }
        #endregion

        // properties from ClassPropertyEmitter

        public string PropertyFQName
        {
            get
            {
                return Item.DeclaringType.FullName + "." + Item.Name;
            }
        }

        public string PropertyName
        {
            get
            {
                return EntityPropertyName;
            }
        }

        private string PropertyClassName
        {
            get
            {
                return Item.DeclaringType.Name;
            }
        }

        private CodeMemberProperty EmitPropertyDeclaration(MemberAttributes scope, CodeTypeReference propertyType, bool isVirtual,
            bool hidesBaseProperty)
        {
            Debug.Assert(GetAccessibilityRank(scope) >= 0, "scope should only be an accessibility attribute");

            CodeMemberProperty memberProperty = new CodeMemberProperty();
            memberProperty.Name = PropertyName;
            CommentEmitter.EmitSummaryComments(Item, memberProperty.Comments);

            memberProperty.Attributes = scope;

            if (!isVirtual)
            {
                memberProperty.Attributes |= MemberAttributes.Final;
            }

            if (hidesBaseProperty || AncestorClassDefinesName(memberProperty.Name))
            {
                memberProperty.Attributes |= MemberAttributes.New;
            }

            memberProperty.Type = propertyType;

            return memberProperty;
        }

        private void DisallowReturnTypeChange(CodeTypeReference baseType, CodeTypeReference newType)
        {
            if (Helper.IsCollectionType(Item.TypeUsage.EdmType) && GetCollectionKind(Item.TypeUsage) != CollectionKind.None)
            {
                if (newType == null)
                {
                    throw EDesignUtil.InvalidOperation(Strings.CannotChangePropertyReturnTypeToNull(Item.Name, Item.DeclaringType.Name));
                }

                // you can change the return type of collection properties
                // we don't even need to check
                return;
            }

            if (!(baseType == null && newType == null) &&
                (
                    (baseType != null && !baseType.Equals(newType)) ||
                    (newType != null && !newType.Equals(baseType))
                )
               )
            {
                throw EDesignUtil.InvalidOperation(Strings.CannotChangePropertyReturnType(Item.Name, Item.DeclaringType.Name));
            }
        }
    }
}
