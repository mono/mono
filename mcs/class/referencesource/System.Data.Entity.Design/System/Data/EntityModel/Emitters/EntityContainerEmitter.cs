//---------------------------------------------------------------------
// <copyright file="EntityContainerEmitter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.CodeDom;
using System.Diagnostics;
using SOM = System.Data.EntityModel.SchemaObjectModel;
using System.Collections.Generic;
using System.Data.Entity.Design;
using System.Data.Objects;
using System.Data.Entity.Design.Common;
using System.Data.Metadata.Edm;
using System.Data.Entity.Design.SsdlGenerator;
using System.Linq;
using System.Data.Common.Utils;


namespace System.Data.EntityModel.Emitters
{
    /// <summary>
    /// This class is responsible for emiting the code for the EntityContainer schema element
    /// </summary>
    internal sealed class EntityContainerEmitter : SchemaTypeEmitter
    {
        #region Fields
        string _onContextCreatedString = "OnContextCreated";
        #endregion


        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="entityContainer"></param>
        public EntityContainerEmitter(ClientApiGenerator generator, EntityContainer entityContainer)
            : base(generator, entityContainer)
        {
        }

        #endregion

        #region Properties, Methods, Events & Delegates
        /// <summary>
        /// Creates the CodeTypeDeclarations necessary to generate the code for the EntityContainer schema element
        /// </summary>
        /// <returns></returns>
        public override CodeTypeDeclarationCollection EmitApiClass()
        {
            Validate(); // emitter-specific validation

            // declare the new class
            // public partial class LOBScenario : ObjectContext
            CodeTypeDeclaration typeDecl = new CodeTypeDeclaration(Item.Name);
            typeDecl.IsPartial = true;

            // raise the TypeGenerated event
            CodeTypeReference objectContextTypeRef = TypeReference.ObjectContext;
            TypeGeneratedEventArgs eventArgs = new TypeGeneratedEventArgs(Item, objectContextTypeRef);
            Generator.RaiseTypeGeneratedEvent(eventArgs);

            if (eventArgs.BaseType != null && !eventArgs.BaseType.Equals(objectContextTypeRef))
            {
                typeDecl.BaseTypes.Add(eventArgs.BaseType);
            }
            else
            {
                typeDecl.BaseTypes.Add(TypeReference.ObjectContext);
            }
            AddInterfaces(Item.Name, typeDecl, eventArgs.AdditionalInterfaces);

            CommentEmitter.EmitSummaryComments(Item, typeDecl.Comments);
            EmitTypeAttributes(Item.Name, typeDecl, eventArgs.AdditionalAttributes);

            CreateConstructors(typeDecl);
            // adding partial OnContextCreated method 
            CreateContextPartialMethods(typeDecl);           

            foreach (EntitySetBase entitySetBase in Item.BaseEntitySets)
            {
                if (MetadataUtil.IsEntitySet(entitySetBase))
                {
                    EntitySet set = (EntitySet)entitySetBase;
                    CodeMemberProperty codeProperty = CreateEntitySetProperty(set);
                    typeDecl.Members.Add(codeProperty);

                    CodeMemberField codeField = CreateEntitySetField(set);
                    typeDecl.Members.Add(codeField);
                }
            }

            foreach (EntitySetBase entitySetBase in Item.BaseEntitySets)
            {
                if (MetadataUtil.IsEntitySet(entitySetBase))
                {
                    EntitySet set = (EntitySet)entitySetBase;
                    CodeMemberMethod codeProperty = CreateEntitySetAddObjectProperty(set);
                    typeDecl.Members.Add(codeProperty);
                }
            }

            foreach (EdmFunction functionImport in Item.FunctionImports)
            {
                if (ShouldEmitFunctionImport(functionImport))
                {
                    CodeMemberMethod functionMethod = CreateFunctionImportStructuralTypeReaderMethod(functionImport);
                    typeDecl.Members.Add(functionMethod);
                }
            }

            // additional members, if provided by the event subscriber
            AddMembers(Item.Name, typeDecl, eventArgs.AdditionalMembers);

            CodeTypeDeclarationCollection typeDecls = new CodeTypeDeclarationCollection();
            typeDecls.Add(typeDecl);
            return typeDecls;
        }

        private bool ShouldEmitFunctionImport(EdmFunction functionImport)
        {
            EdmType returnType = GetReturnTypeFromFunctionImport(functionImport);

            StructuralType structuralReturnType = returnType as StructuralType;

            // we only code gen the functionimport that has a collection of EntityType as return type and ignore the rest,
            // to be more specific, the rest include no return type and collection of scalar type.
            if (null != functionImport.EntitySet)
            {
                return true;
            }

            return false;
        }

        private EdmType GetReturnTypeFromFunctionImport(EdmFunction functionImport)
        {
            EdmType returnType = null;

            if (null != functionImport.ReturnParameter)
            {
                // determine element return type
                returnType = functionImport.ReturnParameter.TypeUsage.EdmType;

                if (Helper.IsCollectionType(returnType))
                {
                    // get the type in the collection
                    returnType = ((CollectionType)returnType).TypeUsage.EdmType;
                }
            }
            return returnType;
        }

        /// <summary>
        /// Emitter-specific validation: check if there exist entity containers and
        /// entity sets that have the same name but differ in case
        /// </summary>
        protected override void Validate()
        {
            base.Validate();
            Generator.VerifyLanguageCaseSensitiveCompatibilityForEntitySet(Item);
            VerifyEntityTypeAndSetAccessibilityCompatability();
        }

        /// <summary>
        /// Verify that Entity Set and Type have compatible accessibilty.
        /// They are compatible if the generated code will compile.
        /// </summary>
        private void VerifyEntityTypeAndSetAccessibilityCompatability()
        {
            foreach (EntitySetBase entitySetBase in Item.BaseEntitySets)
            {
                if (MetadataUtil.IsEntitySet(entitySetBase))
                {
                    EntitySet set = (EntitySet)entitySetBase;
                    if(!AreTypeAndSetAccessCompatible(GetEntityTypeAccessibility(set.ElementType), GetEntitySetPropertyAccessibility(set)))
                    {
                        Generator.AddError(
                            System.Data.Entity.Design.Strings.EntityTypeAndSetAccessibilityConflict(
                                set.ElementType.Name, GetAccessibilityCsdlStringFromMemberAttribute(GetEntityTypeAccessibility(set.ElementType)), set.Name, GetAccessibilityCsdlStringFromMemberAttribute(GetEntitySetPropertyAccessibility(set))),
                            ModelBuilderErrorCode.EntityTypeAndSetAccessibilityConflict,
                            EdmSchemaErrorSeverity.Error);
                    }
                }
            }
        }


        /// <summary>
        /// Tells whether Entity Type's specified accessibility and Entity Set Property's specified Accessibility will work together (compile) when codegen'd.
        /// False if (Type is internal and Set's Property is Public OR, type is internal and Set's property is protected).
        /// True otherwise
        /// </summary>
        private bool AreTypeAndSetAccessCompatible(MemberAttributes typeAccess, MemberAttributes setAccess)
        {
            return !(typeAccess == MemberAttributes.Assembly && (setAccess == MemberAttributes.Public || setAccess == MemberAttributes.Family));
        }


        /// <summary>
        /// Creates the necessary constructors for the entity container.
        /// </summary>
        private void CreateConstructors(CodeTypeDeclaration typeDecl)
        {
            // Empty constructor.
            //
            // public ctor()
            //    : base("name=" + ContainerName, "ContainerName")
            // {
            //      this.OnContextCreated();
            // }
            CodeConstructor emptyCtor = new CodeConstructor();
            emptyCtor.Attributes = MemberAttributes.Public;
            emptyCtor.BaseConstructorArgs.Add(new CodePrimitiveExpression("name=" + Item.Name));
            emptyCtor.BaseConstructorArgs.Add(new CodePrimitiveExpression(Item.Name));
            CommentEmitter.EmitSummaryComments(Strings.EmptyCtorSummaryComment(Item.Name, Item.Name), emptyCtor.Comments);

            emptyCtor.Statements.Add(OnContextCreatedCodeMethodInvokeExpression());
            
            typeDecl.Members.Add(emptyCtor);

            // Constructor that takes a connection string.
            //
            // public ctor(string connectionString)
            //    : base(connectionString, "ContainerName")
            // {
            //      this.OnContextCreated();
            // }
            CodeConstructor connectionStringCtor = new CodeConstructor();
            connectionStringCtor.Attributes = MemberAttributes.Public;
            CodeParameterDeclarationExpression connectionStringParam = new CodeParameterDeclarationExpression(TypeReference.String, "connectionString");
            connectionStringCtor.Parameters.Add(connectionStringParam);
            connectionStringCtor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression(connectionStringParam.Name));
            connectionStringCtor.BaseConstructorArgs.Add(new CodePrimitiveExpression(Item.Name));
            CommentEmitter.EmitSummaryComments(Strings.CtorSummaryComment(Item.Name), connectionStringCtor.Comments);

            connectionStringCtor.Statements.Add(OnContextCreatedCodeMethodInvokeExpression());

            typeDecl.Members.Add(connectionStringCtor);

            // Constructor that takes a connection
            //
            // public ctor(System.Data.EntityClient.EntityConnection connection)
            //    : base(connection, "ContainerName")
            // {
            //      this.OnContextCreated();
            // }
            CodeConstructor connectionWorkspaceCtor = new CodeConstructor();
            connectionWorkspaceCtor.Attributes = MemberAttributes.Public;
            CodeParameterDeclarationExpression connectionParam = new CodeParameterDeclarationExpression(TypeReference.AdoEntityClientType("EntityConnection"), "connection");
            connectionWorkspaceCtor.Parameters.Add(connectionParam);
            connectionWorkspaceCtor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression(connectionParam.Name));
            connectionWorkspaceCtor.BaseConstructorArgs.Add(new CodePrimitiveExpression(Item.Name));
            CommentEmitter.EmitSummaryComments(Strings.CtorSummaryComment(Item.Name), connectionWorkspaceCtor.Comments);

            connectionWorkspaceCtor.Statements.Add(OnContextCreatedCodeMethodInvokeExpression());
            typeDecl.Members.Add(connectionWorkspaceCtor);
        }

        /// <summary>
        /// Adds the OnContextCreated partial method for the entity container.
        /// </summary>
        private void CreateContextPartialMethods(CodeTypeDeclaration typeDecl)
        {
            CodeMemberMethod onContextCreatedPartialMethod = new CodeMemberMethod();
            onContextCreatedPartialMethod.Name = _onContextCreatedString;
            onContextCreatedPartialMethod.ReturnType = new CodeTypeReference(typeof(void));
            onContextCreatedPartialMethod.Attributes = MemberAttributes.Abstract | MemberAttributes.Public;
            typeDecl.Members.Add(onContextCreatedPartialMethod);

            Generator.FixUps.Add(new FixUp(Item.Name + "." + _onContextCreatedString, FixUpType.MarkAbstractMethodAsPartial));
        } 

        private CodeMemberField CreateEntitySetField(EntitySet set)
        {
            Debug.Assert(set != null, "Field is Null");

            // trying to get
            //
            // For Version < 2:
            //
            // private ObjectQuery<SpanTestsModel.Customer> _Customers = null;
            //
            // For Version >= 2:
            //
            // private ObjectSet<SpanTestsModel.Customer> _Customers = null;

            CodeMemberField codeField = new CodeMemberField();
            Generator.AttributeEmitter.EmitGeneratedCodeAttribute(codeField);
            codeField.Attributes = MemberAttributes.Final | MemberAttributes.Private;
            codeField.Name = Utils.FieldNameFromPropName(set.Name);


            CodeTypeReference genericParameter = Generator.GetLeastPossibleQualifiedTypeReference(set.ElementType);
            codeField.Type = TypeReference.AdoFrameworkGenericClass("ObjectQuery", genericParameter);

            return codeField;
        }

        private CodeMemberProperty CreateEntitySetProperty(EntitySet set)
        {
            Debug.Assert(set != null, "Property is Null");

            // trying to get
            //
            // [System.ComponentModel.Browsable(false)]
            // public ObjectQuery<Customer> Customers
            // {
            //      get
            //      {
            //          if ((this._Customers == null))
            //          {
            //              this._Customers = base.CreateQuery<Customer>("[Customers]");
            //          }
            //          return this._Customers;
            //      }
            // }
            //
            CodeMemberProperty codeProperty = new CodeMemberProperty();
            Generator.AttributeEmitter.EmitGeneratedCodeAttribute(codeProperty);
            codeProperty.Attributes = MemberAttributes.Final | GetEntitySetPropertyAccessibility(set);
            codeProperty.Name = set.Name;
            codeProperty.HasGet = true;
            codeProperty.HasSet = false;

            // Determine type to use for field/property and name of factory method on ObjectContext
            string typeName = "ObjectQuery";
            string createMethodName = "CreateQuery";
            // When the EntitySet name is used as CommandText, it should be quoted
            string createMethodArgument = "[" + set.Name + "]";

            CodeTypeReference genericParameter = Generator.GetLeastPossibleQualifiedTypeReference(set.ElementType);
            codeProperty.Type = TypeReference.AdoFrameworkGenericClass(typeName, genericParameter);
            string fieldName = Utils.FieldNameFromPropName(set.Name);

            // raise the PropertyGenerated event before proceeding further
            PropertyGeneratedEventArgs eventArgs = new PropertyGeneratedEventArgs(set, fieldName, codeProperty.Type);
            Generator.RaisePropertyGeneratedEvent(eventArgs);

            if (eventArgs.ReturnType == null || !eventArgs.ReturnType.Equals(codeProperty.Type))
            {
                throw EDesignUtil.InvalidOperation(Strings.CannotChangePropertyReturnType(set.Name, Item.Name));
            }

            List<CodeAttributeDeclaration> additionalAttributes = eventArgs.AdditionalAttributes;
            if (additionalAttributes != null && additionalAttributes.Count > 0)
            {
                try
                {
                    codeProperty.CustomAttributes.AddRange(additionalAttributes.ToArray());
                }
                catch (ArgumentNullException e)
                {
                    Generator.AddError(Strings.InvalidAttributeSuppliedForProperty(Item.Name),
                                               ModelBuilderErrorCode.InvalidAttributeSuppliedForProperty,
                                               EdmSchemaErrorSeverity.Error,
                                               e);
                }
            }

            // we need to insert user-specified code before other/existing code, including
            // the return statement
            List<CodeStatement> additionalGetStatements = eventArgs.AdditionalGetStatements;

            if (additionalGetStatements != null && additionalGetStatements.Count > 0)
            {
                try
                {
                    codeProperty.GetStatements.AddRange(additionalGetStatements.ToArray());
                }
                catch (ArgumentNullException e)
                {
                    Generator.AddError(Strings.InvalidGetStatementSuppliedForProperty(Item.Name),
                                       ModelBuilderErrorCode.InvalidGetStatementSuppliedForProperty,
                                       EdmSchemaErrorSeverity.Error,
                                       e);
                }
            }

            codeProperty.GetStatements.Add(
                new CodeConditionStatement(
                    EmitExpressionEqualsNull(new CodeFieldReferenceExpression(ThisRef, fieldName)),
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(ThisRef, fieldName),
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodeBaseReferenceExpression(),
                                createMethodName,
                                new CodeTypeReference[] { genericParameter }
                            ),
                            new CodePrimitiveExpression(createMethodArgument)
                        )
                    )
                )
            );

            codeProperty.GetStatements.Add(
                new CodeMethodReturnStatement(
                    new CodeFieldReferenceExpression(
                        ThisRef,
                        fieldName
                    )
                )
            );

            // property summary
            CommentEmitter.EmitSummaryComments(set, codeProperty.Comments);

            return codeProperty;
        }
        /// <summary>
        /// Create an AddTo-EntitysetName methiod for each entityset in the context.
        /// </summary>
        /// <param name="set">EntityContainerEntitySet that we will go over to get the existing entitysets.</param>
        /// <returns> Method definition </returns>

        private CodeMemberMethod CreateEntitySetAddObjectProperty(EntitySet set)
        {
            Debug.Assert(set != null, "Property is Null");

            // trying to get
            //
            // public void AddToCustomer(Customer customer)
            // {
            //      base.AddObject("Customer", customer);
            // }
            CodeMemberMethod codeMethod = new CodeMemberMethod();
            Generator.AttributeEmitter.EmitGeneratedCodeAttribute(codeMethod);
            codeMethod.Attributes = MemberAttributes.Final | GetEntityTypeAccessibility(set.ElementType);
            codeMethod.Name = ("AddTo" + set.Name);

            CodeParameterDeclarationExpression parameter = new CodeParameterDeclarationExpression();

            parameter.Type = Generator.GetLeastPossibleQualifiedTypeReference(set.ElementType);
            parameter.Name = Utils.FixParameterName(set.ElementType.Name);
            codeMethod.Parameters.Add(parameter);

            codeMethod.ReturnType = new CodeTypeReference(typeof(void));

            codeMethod.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodeBaseReferenceExpression(),
                    "AddObject",
                    new CodePrimitiveExpression(set.Name),
                    new CodeFieldReferenceExpression(null, parameter.Name)
                )
            );

            // method summary
            CommentEmitter.EmitSummaryComments(set, codeMethod.Comments);
            return codeMethod;
        }

        /// <summary>
        /// Create a method entry point for a function import yielding an entity reader.
        /// </summary>
        /// <param name="functionImport">SOM for function import; must not be null and must yield
        /// an entity reader.</param>
        /// <returns>Method definition.</returns>
        private CodeMemberMethod CreateFunctionImportStructuralTypeReaderMethod(EdmFunction functionImport)
        {
            // Trying to get:
            //
            ///// <summary>
            ///// Documentation
            ///// </summary>
            //public ObjectQueryResult<MyType> MyFunctionImport(Nullable<int> id, string foo)
            //{
            //    ObjectParameter idParameter;
            //    if (id.HasValue)
            //    {
            //        idParameter = new ObjectParameter("id", id);
            //    }
            //    else
            //    {
            //        idParameter = new ObjectParameter("id", typeof(int));
            //    }
            //    ObjectParameter fooParameter;
            //    if (null != foo)
            //    {
            //        fooParameter = new ObjectParameter("foo", foo);
            //    }
            //    else
            //    {
            //        fooParameter = new ObjectParameter("foo", typeof(string));
            //    }
            //    return base.ExecuteFunction<MyType>("MyFunctionImport", idParameter, fooParameter);
            //}
            Debug.Assert(null != functionImport);

            CodeMemberMethod method = new CodeMemberMethod();
            Generator.AttributeEmitter.EmitGeneratedCodeAttribute(method);
            method.Name = functionImport.Name;
            method.Attributes = GetFunctionImportAccessibility(functionImport) | MemberAttributes.Final;

            UniqueIdentifierService uniqueIdentifierService = new UniqueIdentifierService(
                this.Generator.IsLanguageCaseSensitive, 
                s => Utils.FixParameterName(s));

            // determine element return type
            EdmType returnType = GetReturnTypeFromFunctionImport(functionImport);
            if (Helper.IsCollectionType(returnType))
            {
                // get the type in the collection
                returnType = ((CollectionType)returnType).TypeUsage.EdmType;
            }
            CodeTypeReference elementType = Generator.GetLeastPossibleQualifiedTypeReference(returnType);
            method.ReturnType = TypeReference.ObjectResult(elementType);

            // generate <summary> comments based on CSDL Documentation element
            CommentEmitter.EmitSummaryComments(functionImport, method.Comments);

            // build up list of arguments to ExecuteFunction
            List<CodeExpression> executeArguments = new List<CodeExpression>();
            executeArguments.Add(new CodePrimitiveExpression(functionImport.Name)); // first argument is the name of the function
            foreach (FunctionParameter parameter in functionImport.Parameters)
            {
                CreateFunctionArgument(method, uniqueIdentifierService, parameter);
            }

            // add fields representing object parameters
            foreach (FunctionParameter parameter in functionImport.Parameters)
            {
                if (parameter.Mode == ParameterMode.In)
                {
                    CodeExpression variableReference = CreateFunctionParameter(method, uniqueIdentifierService, parameter);
                    executeArguments.Add(variableReference);
                }
                else
                {
                    // the parameter is already being passed in as an argument; just remember it and 
                    // pass it in as an argument
                    string adjustedParameterName;
                    if (!uniqueIdentifierService.TryGetAdjustedName(parameter, out adjustedParameterName))
                    {
                        Debug.Fail("parameter must be registered in identifier service");
                    }
                    executeArguments.Add(new CodeVariableReferenceExpression(adjustedParameterName));
                }
            }

            // Add call to ExecuteFunction
            //      return ExecuteFunction<elementType>("FunctionImportName", { object parameters });
            CodeMethodReferenceExpression executeFunctionMethod = new CodeMethodReferenceExpression(
                new CodeBaseReferenceExpression(),
                "ExecuteFunction",
                new CodeTypeReference[] { elementType });
            method.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeMethodInvokeExpression(executeFunctionMethod, executeArguments.ToArray())
                )
            );

            // invoke the ExecuteFunction method passing in parameters
            return method;
        }

        private CodeExpression CreateFunctionParameter(CodeMemberMethod method, UniqueIdentifierService uniqueIdentifierService, FunctionParameter parameter)
        {
            // get (adjusted) name of parameter
            string adjustedParameterName;
            if (!uniqueIdentifierService.TryGetAdjustedName(parameter, out adjustedParameterName))
            {
                Debug.Fail("parameter must be registered in identifier service");
            }
            Type parameterType = DetermineParameterType(parameter);

            // make sure the variable name does not collide with any parameters to the method, or any
            // existing variables (all registered in the service)
            string variableName = uniqueIdentifierService.AdjustIdentifier(parameter.Name + "Parameter");

            // ObjectParameter variableName;
            // if (null != parameterName)
            // {
            //     variableName = new ObjectParameter("parameterName", adjustedParameterName);
            // }
            // else
            // {
            //     variableName = new ObjectParameter("parameterName", typeof(parameterType));
            // }
            method.Statements.Add(
                new CodeVariableDeclarationStatement(TypeReference.ForType(typeof(ObjectParameter)), variableName));
            CodeExpression variableReference = new CodeVariableReferenceExpression(variableName);
            CodeExpression parameterReference = new CodeVariableReferenceExpression(adjustedParameterName);
            CodeStatement nullConstructor = new CodeAssignStatement(variableReference,
                new CodeObjectCreateExpression(TypeReference.ForType(typeof(ObjectParameter)),
                new CodePrimitiveExpression(parameter.Name),
                new CodeTypeOfExpression(TypeReference.ForType(parameterType))));
            CodeStatement valueConstructor = new CodeAssignStatement(variableReference,
                new CodeObjectCreateExpression(TypeReference.ForType(typeof(ObjectParameter)),
                new CodePrimitiveExpression(parameter.Name),
                parameterReference));
            CodeExpression notNullCondition;
            if (parameterType.IsValueType)
            {
                // Value type parameters generate Nullable<ValueType> arguments (see CreateFunctionArgument).
                // We call Nullable<ValueType>.HasValue to determine whether the argument passed in is null
                // (since null != nullableTypeInstance does not work in VB)
                //
                // parameterReference.HasValue
                notNullCondition = new CodePropertyReferenceExpression(
                    parameterReference,
                    "HasValue");
            }
            else
            {
                // use parameterReference != null
                notNullCondition = new CodeBinaryOperatorExpression(
                    parameterReference,
                    CodeBinaryOperatorType.IdentityInequality,
                    NullExpression);
            }
            method.Statements.Add(
                new CodeConditionStatement(
                    notNullCondition,
                    new CodeStatement[] { valueConstructor, },
                    new CodeStatement[] { nullConstructor, }
                )
            );
            return variableReference;
        }

        private void CreateFunctionArgument(CodeMemberMethod method, UniqueIdentifierService uniqueIdentifierService, FunctionParameter parameter)
        {
            // get type of parameter
            Type clrType = DetermineParameterType(parameter);

            // parameters to stored procedures must be nullable
            CodeTypeReference argumentType = clrType.IsValueType ? TypeReference.NullableForType(clrType) : TypeReference.ForType(clrType);
            string parameterName = uniqueIdentifierService.AdjustIdentifier(parameter.Name, parameter);
            CodeParameterDeclarationExpression codeParameter = new CodeParameterDeclarationExpression(argumentType, parameterName);
            method.Parameters.Add(codeParameter);
        }

        // requires: parameter type is constrained to be a scalar type
        // Determines CLR type for function parameter
        private static Type DetermineParameterType(FunctionParameter parameter)
        {
            Debug.Assert(null != parameter && MetadataUtil.IsPrimitiveType(parameter.TypeUsage.EdmType),
                "validation must ensure only scalar type parameter are given");

            if (parameter.Mode != ParameterMode.In)
            {
                // non input parameter must be treated as ObjectParameter instances so that the 
                // value can be set asynchronously (after the method has yielded and the reader
                // has been consumed)
                return typeof(ObjectParameter);
            }

            PrimitiveType parameterType = (PrimitiveType)parameter.TypeUsage.EdmType;
            Type clrType = parameterType.ClrType;
            return clrType;
        }

        /// <summary>
        /// return a code expression for invoking OnContextCreated partial method
        /// </summary>
        private CodeMethodInvokeExpression OnContextCreatedCodeMethodInvokeExpression()
        {
            return (new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), _onContextCreatedString, new CodeExpression[] { }));
        }

        /// <summary>
        /// Returns the type specific SchemaElement
        /// </summary>
        private new EntityContainer Item
        {
            get
            {
                return base.Item as EntityContainer;
            }
        }

        #endregion
    }
}
