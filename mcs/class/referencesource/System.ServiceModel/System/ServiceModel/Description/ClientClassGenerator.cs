//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.CodeDom;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;

    class ClientClassGenerator : IServiceContractGenerationExtension
    {
        bool tryAddHelperMethod = false;
        bool generateEventAsyncMethods = false;

        internal ClientClassGenerator(bool tryAddHelperMethod)
            : this(tryAddHelperMethod, false)
        {
        }

        internal ClientClassGenerator(bool tryAddHelperMethod, bool generateEventAsyncMethods)
        {
            this.tryAddHelperMethod = tryAddHelperMethod;
            this.generateEventAsyncMethods = generateEventAsyncMethods;
        }

        static Type clientBaseType = typeof(ClientBase<>);
        static Type duplexClientBaseType = typeof(DuplexClientBase<>);
        static Type instanceContextType = typeof(InstanceContext);
        static Type objectType = typeof(object);
        static Type objectArrayType = typeof(object[]);
        static Type exceptionType = typeof(Exception);
        static Type boolType = typeof(bool);
        static Type stringType = typeof(string);
        static Type endpointAddressType = typeof(EndpointAddress);
        static Type uriType = typeof(Uri);
        static Type bindingType = typeof(Binding);
        static Type sendOrPostCallbackType = typeof(SendOrPostCallback);
        static Type asyncCompletedEventArgsType = typeof(AsyncCompletedEventArgs);
        static Type eventHandlerType = typeof(EventHandler<>);
        static Type voidType = typeof(void);
        static Type asyncResultType = typeof(IAsyncResult);
        static Type asyncCallbackType = typeof(AsyncCallback);
        
        static CodeTypeReference voidTypeRef = new CodeTypeReference(typeof(void));
        static CodeTypeReference asyncResultTypeRef = new CodeTypeReference(typeof(IAsyncResult));

        static string inputInstanceName = "callbackInstance";
        static string invokeAsyncCompletedEventArgsTypeName = "InvokeAsyncCompletedEventArgs";
        static string invokeAsyncMethodName = "InvokeAsync";
        static string raiseExceptionIfNecessaryMethodName = "RaiseExceptionIfNecessary";
        static string beginOperationDelegateTypeName = "BeginOperationDelegate";
        static string endOperationDelegateTypeName = "EndOperationDelegate";
        static string getDefaultValueForInitializationMethodName = "GetDefaultValueForInitialization";

        // IMPORTANT: this table tracks the set of .ctors in ClientBase and DuplexClientBase. 
        // This table must be kept in sync
        // for DuplexClientBase, the initial InstanceContext param is assumed; ctor overloads must match between ClientBase and DuplexClientBase
        static Type[][] ClientCtorParamTypes = new Type[][]
            {
                new Type[] { },
                new Type[] { stringType, },
                new Type[] { stringType, stringType, },
                new Type[] { stringType, endpointAddressType, },
                new Type[] { bindingType, endpointAddressType, },
            };

        static string[][] ClientCtorParamNames = new string[][]
            {
                new string[] { },
                new string[] { "endpointConfigurationName", },
                new string[] { "endpointConfigurationName", "remoteAddress", },
                new string[] { "endpointConfigurationName", "remoteAddress", },
                new string[] { "binding", "remoteAddress", },
            };

        static Type[] EventArgsCtorParamTypes = new Type[] 
        { 
            objectArrayType, 
            exceptionType, 
            boolType, 
            objectType 
        };

        static string[] EventArgsCtorParamNames = new string[] 
        {
            "results",
            "exception",
            "cancelled",
            "userState"
        };

        static string[] EventArgsPropertyNames = new string[]
        {
            "Results",
            "Error",
            "Cancelled",
            "UserState"
        };

#if DEBUG
        static BindingFlags ctorBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        static string DebugCheckTable_errorString = "Client code generation table out of sync with ClientBase and DuplexClientBase ctors. Please investigate.";

        // check the table against what we would get from reflection
        static void DebugCheckTable()
        {
            Fx.Assert(ClientCtorParamNames.Length == ClientCtorParamTypes.Length, DebugCheckTable_errorString);

            for (int i = 0; i < ClientCtorParamTypes.Length; i++)
            {
                DebugCheckTable_ValidateCtor(clientBaseType.GetConstructor(ctorBindingFlags, null, ClientCtorParamTypes[i], null), ClientCtorParamNames[i]);

                Type[] duplexCtorTypes1 = DebugCheckTable_InsertAtStart(ClientCtorParamTypes[i], objectType);
                Type[] duplexCtorTypes2 = DebugCheckTable_InsertAtStart(ClientCtorParamTypes[i], instanceContextType);
                string[] duplexCtorNames = DebugCheckTable_InsertAtStart(ClientCtorParamNames[i], inputInstanceName);

                DebugCheckTable_ValidateCtor(duplexClientBaseType.GetConstructor(ctorBindingFlags, null, duplexCtorTypes1, null), duplexCtorNames);
                DebugCheckTable_ValidateCtor(duplexClientBaseType.GetConstructor(ctorBindingFlags, null, duplexCtorTypes2, null), duplexCtorNames);
            }

            // ClientBase<> has extra InstanceContext overloads that we do not call directly from the generated code, but which we 
            // need to account for in this assert
            Fx.Assert(clientBaseType.GetConstructors(ctorBindingFlags).Length == ClientCtorParamTypes.Length * 2, DebugCheckTable_errorString);

            // DuplexClientBase<> also has extra object/InstanceContext overloads (but we call these)
            Fx.Assert(duplexClientBaseType.GetConstructors(ctorBindingFlags).Length == ClientCtorParamTypes.Length * 2, DebugCheckTable_errorString);
        }

        static T[] DebugCheckTable_InsertAtStart<T>(T[] arr, T item)
        {
            T[] newArr = new T[arr.Length + 1];
            newArr[0] = item;
            Array.Copy(arr, 0, newArr, 1, arr.Length);
            return newArr;
        }

        static void DebugCheckTable_ValidateCtor(ConstructorInfo ctor, string[] paramNames)
        {
            Fx.Assert(ctor != null, DebugCheckTable_errorString);

            ParameterInfo[] parameters = ctor.GetParameters();
            Fx.Assert(parameters.Length == paramNames.Length, DebugCheckTable_errorString);
            for (int i = 0; i < paramNames.Length; i++)
            {
                Fx.Assert(parameters[i].Name == paramNames[i], DebugCheckTable_errorString);
            }
        }
#endif

        void IServiceContractGenerationExtension.GenerateContract(ServiceContractGenerationContext context)
        {
#if DEBUG
            // DebugCheckTable();
#endif
            CodeTypeDeclaration clientType = context.TypeFactory.CreateClassType();
            // Have to make sure that client name does not match any methods: member names can not be the same as their enclosing type (CSharp only)
            clientType.Name = NamingHelper.GetUniqueName(GetClientClassName(context.ContractType.Name), DoesMethodNameExist, context.Operations);
            CodeTypeReference contractTypeRef = context.ContractTypeReference;
            if (context.DuplexCallbackType == null)
                clientType.BaseTypes.Add(new CodeTypeReference(context.ServiceContractGenerator.GetCodeTypeReference(typeof(ClientBase<>)).BaseType, context.ContractTypeReference));
            else
                clientType.BaseTypes.Add(new CodeTypeReference(context.ServiceContractGenerator.GetCodeTypeReference(typeof(DuplexClientBase<>)).BaseType, context.ContractTypeReference));

            clientType.BaseTypes.Add(context.ContractTypeReference);

            if (!(ClientCtorParamNames.Length == ClientCtorParamTypes.Length))
            {
                Fx.Assert("Invalid client generation constructor table initialization");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Invalid client generation constructor table initialization")));
            }

            for (int i = 0; i < ClientCtorParamNames.Length; i++)
            {
                if (!(ClientCtorParamNames[i].Length == ClientCtorParamTypes[i].Length))
                {
                    Fx.Assert("Invalid client generation constructor table initialization");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Invalid client generation constructor table initialization")));
                }

                CodeConstructor ctor = new CodeConstructor();
                ctor.Attributes = MemberAttributes.Public;
                if (context.DuplexCallbackType != null)
                {
                    ctor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(InstanceContext), inputInstanceName));
                    ctor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression(inputInstanceName));
                }
                for (int j = 0; j < ClientCtorParamNames[i].Length; j++)
                {
                    ctor.Parameters.Add(new CodeParameterDeclarationExpression(ClientCtorParamTypes[i][j], ClientCtorParamNames[i][j]));
                    ctor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression(ClientCtorParamNames[i][j]));
                }
                clientType.Members.Add(ctor);
            }

            foreach (OperationContractGenerationContext operationContext in context.Operations)
            {
                // Note that we generate all the client-side methods, even inherited ones.
                if (operationContext.Operation.IsServerInitiated()) continue;
                CodeTypeReference declaringContractTypeRef = operationContext.DeclaringTypeReference;
                GenerateClientClassMethod(clientType, contractTypeRef, operationContext.SyncMethod, this.tryAddHelperMethod, declaringContractTypeRef);

                if (operationContext.IsAsync)
                {
                    CodeMemberMethod beginMethod = GenerateClientClassMethod(clientType, contractTypeRef, operationContext.BeginMethod, this.tryAddHelperMethod, declaringContractTypeRef);
                    CodeMemberMethod endMethod = GenerateClientClassMethod(clientType, contractTypeRef, operationContext.EndMethod, this.tryAddHelperMethod, declaringContractTypeRef);

                    if (this.generateEventAsyncMethods)
                    {
                        GenerateEventAsyncMethods(context, clientType, operationContext.SyncMethod.Name, beginMethod, endMethod);
                    }
                }

                if (operationContext.IsTask)
                {
                    GenerateClientClassMethod(clientType, contractTypeRef, operationContext.TaskMethod, !operationContext.Operation.HasOutputParameters && this.tryAddHelperMethod, declaringContractTypeRef);
                }
            }

            context.Namespace.Types.Add(clientType);
            context.ClientType = clientType;
            context.ClientTypeReference = ServiceContractGenerator.NamespaceHelper.GetCodeTypeReference(context.Namespace, clientType);
        }

        static CodeMemberMethod GenerateClientClassMethod(CodeTypeDeclaration clientType, CodeTypeReference contractTypeRef, CodeMemberMethod method, bool addHelperMethod, CodeTypeReference declaringContractTypeRef)
        {
            CodeMemberMethod methodImpl = GetImplementationOfMethod(contractTypeRef, method);
            AddMethodImpl(methodImpl);
            int methodPosition = clientType.Members.Add(methodImpl);
            CodeMemberMethod helperMethod = null;

            if (addHelperMethod)
            {
                helperMethod = GenerateHelperMethod(declaringContractTypeRef, methodImpl);
                if (helperMethod != null)
                {
                    clientType.Members[methodPosition].CustomAttributes.Add(CreateEditorBrowsableAttribute(EditorBrowsableState.Advanced));
                    clientType.Members.Add(helperMethod);
                }
            }

            return (helperMethod != null) ? helperMethod : methodImpl;
        }

        internal static CodeAttributeDeclaration CreateEditorBrowsableAttribute(EditorBrowsableState editorBrowsableState)
        {
            CodeAttributeDeclaration browsableAttribute = new CodeAttributeDeclaration(new CodeTypeReference(typeof(EditorBrowsableAttribute)));
            CodeTypeReferenceExpression browsableAttributeState = new CodeTypeReferenceExpression(typeof(EditorBrowsableState));
            CodeAttributeArgument browsableAttributeValue = new CodeAttributeArgument(new CodeFieldReferenceExpression(browsableAttributeState, editorBrowsableState.ToString()));
            browsableAttribute.Arguments.Add(browsableAttributeValue);

            return browsableAttribute;
        }

        private static CodeMemberMethod GenerateHelperMethod(CodeTypeReference ifaceType, CodeMemberMethod method)
        {
            CodeMemberMethod helperMethod = new CodeMemberMethod();
            helperMethod.Name = method.Name;
            helperMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            CodeMethodInvokeExpression invokeMethod = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeCastExpression(ifaceType, new CodeThisReferenceExpression()), method.Name));
            bool hasTypedMessage = false;
            foreach (CodeParameterDeclarationExpression param in method.Parameters)
            {
                CodeTypeDeclaration paramTypeDecl = ServiceContractGenerator.NamespaceHelper.GetCodeType(param.Type);
                if (paramTypeDecl != null)
                {
                    hasTypedMessage = true;
                    CodeVariableReferenceExpression inValue = new CodeVariableReferenceExpression("inValue");
                    helperMethod.Statements.Add(new CodeVariableDeclarationStatement(param.Type, inValue.VariableName, new CodeObjectCreateExpression(param.Type)));
                    invokeMethod.Parameters.Add(inValue);
                    GenerateParameters(helperMethod, paramTypeDecl, inValue, FieldDirection.In);
                }
                else
                {
                    helperMethod.Parameters.Add(new CodeParameterDeclarationExpression(param.Type, param.Name));
                    invokeMethod.Parameters.Add(new CodeArgumentReferenceExpression(param.Name));
                }
            }
            if (method.ReturnType.BaseType == voidTypeRef.BaseType)
                helperMethod.Statements.Add(invokeMethod);
            else
            {
                CodeTypeDeclaration returnTypeDecl = ServiceContractGenerator.NamespaceHelper.GetCodeType(method.ReturnType);
                if (returnTypeDecl != null)
                {
                    hasTypedMessage = true;
                    CodeVariableReferenceExpression outVar = new CodeVariableReferenceExpression("retVal");

                    helperMethod.Statements.Add(new CodeVariableDeclarationStatement(method.ReturnType, outVar.VariableName, invokeMethod));
                    CodeMethodReturnStatement returnStatement = GenerateParameters(helperMethod, returnTypeDecl, outVar, FieldDirection.Out);
                    if (returnStatement != null)
                        helperMethod.Statements.Add(returnStatement);
                }
                else
                {
                    helperMethod.Statements.Add(new CodeMethodReturnStatement(invokeMethod));
                    helperMethod.ReturnType = method.ReturnType;
                }
            }
            if (hasTypedMessage)
                method.PrivateImplementationType = ifaceType;
            return hasTypedMessage ? helperMethod : null;
        }

        private static CodeMethodReturnStatement GenerateParameters(CodeMemberMethod helperMethod, CodeTypeDeclaration codeTypeDeclaration, CodeExpression target, FieldDirection dir)
        {
            CodeMethodReturnStatement returnStatement = null;
            foreach (CodeTypeMember member in codeTypeDeclaration.Members)
            {
                CodeMemberField field = member as CodeMemberField;
                if (field == null)
                    continue;
                CodeFieldReferenceExpression fieldRef = new CodeFieldReferenceExpression(target, field.Name);
                CodeTypeDeclaration bodyTypeDecl = ServiceContractGenerator.NamespaceHelper.GetCodeType(field.Type);
                if (bodyTypeDecl != null)
                {
                    if (dir == FieldDirection.In)
                        helperMethod.Statements.Add(new CodeAssignStatement(fieldRef, new CodeObjectCreateExpression(field.Type)));
                    returnStatement = GenerateParameters(helperMethod, bodyTypeDecl, fieldRef, dir);
                    continue;
                }
                CodeParameterDeclarationExpression param = GetRefParameter(helperMethod.Parameters, dir, field);
                if (param == null && dir == FieldDirection.Out && helperMethod.ReturnType.BaseType == voidTypeRef.BaseType)
                {
                    helperMethod.ReturnType = field.Type;
                    returnStatement = new CodeMethodReturnStatement(fieldRef);
                }
                else
                {
                    if (param == null)
                    {
                        param = new CodeParameterDeclarationExpression(field.Type, NamingHelper.GetUniqueName(field.Name, DoesParameterNameExist, helperMethod));
                        param.Direction = dir;
                        helperMethod.Parameters.Add(param);
                    }
                    if (dir == FieldDirection.Out)
                        helperMethod.Statements.Add(new CodeAssignStatement(new CodeArgumentReferenceExpression(param.Name), fieldRef));
                    else
                        helperMethod.Statements.Add(new CodeAssignStatement(fieldRef, new CodeArgumentReferenceExpression(param.Name)));
                }
            }
            return returnStatement;
        }

        private static CodeParameterDeclarationExpression GetRefParameter(CodeParameterDeclarationExpressionCollection parameters, FieldDirection dir, CodeMemberField field)
        {
            foreach (CodeParameterDeclarationExpression p in parameters)
            {
                if (p.Name == field.Name)
                {
                    if (p.Direction != dir && p.Type.BaseType == field.Type.BaseType)
                    {
                        p.Direction = FieldDirection.Ref;
                        return p;
                    }
                    return null;
                }
            }
            return null;
        }

        internal static bool DoesMemberNameExist(string name, object typeDeclarationObject)
        {
            CodeTypeDeclaration typeDeclaration = (CodeTypeDeclaration)typeDeclarationObject;

            if (string.Compare(typeDeclaration.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }

            foreach (CodeTypeMember member in typeDeclaration.Members)
            {
                if (string.Compare(member.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool DoesTypeNameExists(string name, object codeTypeDeclarationCollectionObject)
        {
            CodeTypeDeclarationCollection codeTypeDeclarations = (CodeTypeDeclarationCollection)codeTypeDeclarationCollectionObject;
            foreach (CodeTypeDeclaration codeTypeDeclaration in codeTypeDeclarations)
            {
                if (string.Compare(codeTypeDeclaration.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool DoesTypeAndMemberNameExist(string name, object nameCollection)
        {
            object[] nameCollections = (object[])nameCollection;

            if (DoesTypeNameExists(name, nameCollections[0]))
            {
                return true;
            }
            if (DoesMemberNameExist(name, nameCollections[1]))
            {
                return true;
            }

            return false;
        }

        internal static bool DoesMethodNameExist(string name, object operationsObject)
        {
            Collection<OperationContractGenerationContext> operations = (Collection<OperationContractGenerationContext>)operationsObject;
            foreach (OperationContractGenerationContext operationContext in operations)
            {
                if (String.Compare(operationContext.SyncMethod.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
                if (operationContext.IsAsync)
                {
                    if (String.Compare(operationContext.BeginMethod.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                        return true;
                    if (String.Compare(operationContext.EndMethod.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                        return true;
                }
                if (operationContext.IsTask)
                {
                    if (String.Compare(operationContext.TaskMethod.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                        return true;
                }
            }
            return false;
        }

        internal static bool DoesParameterNameExist(string name, object methodObject)
        {
            CodeMemberMethod method = (CodeMemberMethod)methodObject;
            if (String.Compare(method.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                return true;
            CodeParameterDeclarationExpressionCollection parameters = method.Parameters;
            foreach (CodeParameterDeclarationExpression p in parameters)
            {
                if (String.Compare(p.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            }
            return false;
        }

        static void AddMethodImpl(CodeMemberMethod method)
        {
            CodeMethodInvokeExpression methodInvoke = new CodeMethodInvokeExpression(GetChannelReference(), method.Name);
            foreach (CodeParameterDeclarationExpression parameter in method.Parameters)
            {
                methodInvoke.Parameters.Add(new CodeDirectionExpression(parameter.Direction, new CodeVariableReferenceExpression(parameter.Name)));
            }
            if (IsVoid(method))
                method.Statements.Add(methodInvoke);
            else
                method.Statements.Add(new CodeMethodReturnStatement(methodInvoke));
        }

        static CodeMemberMethod GetImplementationOfMethod(CodeTypeReference ifaceType, CodeMemberMethod method)
        {
            CodeMemberMethod m = new CodeMemberMethod();
            m.Name = method.Name;
            m.ImplementationTypes.Add(ifaceType);
            m.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            foreach (CodeParameterDeclarationExpression parameter in method.Parameters)
            {
                CodeParameterDeclarationExpression newParam = new CodeParameterDeclarationExpression(parameter.Type, parameter.Name);
                newParam.Direction = parameter.Direction;
                m.Parameters.Add(newParam);
            }
            m.ReturnType = method.ReturnType;
            return m;
        }

        static void GenerateEventAsyncMethods(ServiceContractGenerationContext context, CodeTypeDeclaration clientType,
            string syncMethodName, CodeMemberMethod beginMethod, CodeMemberMethod endMethod)
        {
            CodeTypeDeclaration operationCompletedEventArgsType = CreateOperationCompletedEventArgsType(context, syncMethodName, endMethod);
            CodeMemberEvent operationCompletedEvent = CreateOperationCompletedEvent(context, clientType, syncMethodName, operationCompletedEventArgsType);

            CodeMemberField beginOperationDelegate = CreateBeginOperationDelegate(context, clientType, syncMethodName);
            CodeMemberMethod beginOperationMethod = CreateBeginOperationMethod(context, clientType, syncMethodName, beginMethod);

            CodeMemberField endOperationDelegate = CreateEndOperationDelegate(context, clientType, syncMethodName);
            CodeMemberMethod endOperationMethod = CreateEndOperationMethod(context, clientType, syncMethodName, endMethod);

            CodeMemberField operationCompletedDelegate = CreateOperationCompletedDelegate(context, clientType, syncMethodName);
            CodeMemberMethod operationCompletedMethod = CreateOperationCompletedMethod(context, clientType, syncMethodName, operationCompletedEventArgsType, operationCompletedEvent);

            CodeMemberMethod eventAsyncMethod = CreateEventAsyncMethod(context, clientType, syncMethodName, beginMethod, 
                beginOperationDelegate, beginOperationMethod, endOperationDelegate, endOperationMethod, operationCompletedDelegate, operationCompletedMethod);

            CreateEventAsyncMethodOverload(clientType, eventAsyncMethod);

            // hide the normal async methods from intellisense
            beginMethod.CustomAttributes.Add(CreateEditorBrowsableAttribute(EditorBrowsableState.Advanced));
            endMethod.CustomAttributes.Add(CreateEditorBrowsableAttribute(EditorBrowsableState.Advanced));            
        }

        static CodeTypeDeclaration CreateOperationCompletedEventArgsType(ServiceContractGenerationContext context, 
            string syncMethodName, CodeMemberMethod endMethod)
        {
            if ((endMethod.Parameters.Count == 1) && (endMethod.ReturnType.BaseType == voidTypeRef.BaseType))
            {
                // no need to create new event args type, use AsyncCompletedEventArgs
                return null;
            }

            CodeTypeDeclaration argsType = context.TypeFactory.CreateClassType();
            argsType.BaseTypes.Add(new CodeTypeReference(asyncCompletedEventArgsType));

            // define object[] results field.
            CodeMemberField resultsField = new CodeMemberField();
            resultsField.Type = new CodeTypeReference(objectArrayType);

            CodeFieldReferenceExpression resultsFieldReference = new CodeFieldReferenceExpression();
            resultsFieldReference.TargetObject = new CodeThisReferenceExpression();

            // create constructor, that assigns the results field.
            CodeConstructor ctor = new CodeConstructor();
            ctor.Attributes = MemberAttributes.Public;
            for (int i = 0; i < EventArgsCtorParamTypes.Length; i++)
            {
                ctor.Parameters.Add(new CodeParameterDeclarationExpression(EventArgsCtorParamTypes[i], EventArgsCtorParamNames[i]));
                if (i > 0)
                {
                    ctor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression(EventArgsCtorParamNames[i]));
                }
            }
            argsType.Members.Add(ctor);
            ctor.Statements.Add(new CodeAssignStatement(resultsFieldReference, new CodeVariableReferenceExpression(EventArgsCtorParamNames[0])));
             
            // create properties for the out parameters
            int asyncResultParamIndex = GetAsyncResultParamIndex(endMethod);
            int count = 0;
            for (int i = 0; i < endMethod.Parameters.Count; i++)
            {
                if (i != asyncResultParamIndex)
                {
                    CreateEventAsyncCompletedArgsTypeProperty(argsType,
                        endMethod.Parameters[i].Type,
                        endMethod.Parameters[i].Name,
                        new CodeArrayIndexerExpression(resultsFieldReference, new CodePrimitiveExpression(count++)));
                }
            }

            // create the property for the return type
            if (endMethod.ReturnType.BaseType != voidTypeRef.BaseType)
            {
                CreateEventAsyncCompletedArgsTypeProperty(
                    argsType,
                    endMethod.ReturnType,
                    NamingHelper.GetUniqueName("Result", DoesMemberNameExist, argsType),
                    new CodeArrayIndexerExpression(resultsFieldReference, 
                        new CodePrimitiveExpression(count)));
               
            }

            // Name the "results" field after generating the properties to make sure it does 
            // not conflict with the property names.
            resultsField.Name = NamingHelper.GetUniqueName("results", DoesMemberNameExist, argsType);
            resultsFieldReference.FieldName = resultsField.Name;
            argsType.Members.Add(resultsField);

            // Name the type making sure that it does not conflict with its members and types already present in
            // the namespace. 
            argsType.Name = NamingHelper.GetUniqueName(GetOperationCompletedEventArgsTypeName(syncMethodName),
                DoesTypeAndMemberNameExist, new object[] { context.Namespace.Types, argsType });
            context.Namespace.Types.Add(argsType);

            return argsType;
        }

        static int GetAsyncResultParamIndex(CodeMemberMethod endMethod)
        {
            int index = endMethod.Parameters.Count - 1;
            if (endMethod.Parameters[index].Type.BaseType != asyncResultTypeRef.BaseType)
            {
                // workaround for CSD Dev Framework:10826, the unwrapped end method has IAsyncResult as first param. 
                index = 0;
            }

            return index;
        }
        
        static CodeMemberProperty CreateEventAsyncCompletedArgsTypeProperty(CodeTypeDeclaration ownerTypeDecl, 
            CodeTypeReference propertyType, string propertyName, CodeExpression propertyValueExpr)
        {
            CodeMemberProperty property = new CodeMemberProperty();
            property.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            property.Type = propertyType;
            property.Name = propertyName;
            property.HasSet = false;
            property.HasGet = true;

            CodeCastExpression castExpr = new CodeCastExpression(propertyType, propertyValueExpr);
            CodeMethodReturnStatement returnStmt = new CodeMethodReturnStatement(castExpr);

            property.GetStatements.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), raiseExceptionIfNecessaryMethodName));
            property.GetStatements.Add(returnStmt);
            ownerTypeDecl.Members.Add(property);

            return property;
        }

        static CodeMemberEvent CreateOperationCompletedEvent(ServiceContractGenerationContext context, 
            CodeTypeDeclaration clientType, string syncMethodName, CodeTypeDeclaration operationCompletedEventArgsType)
        {
            CodeMemberEvent operationCompletedEvent = new CodeMemberEvent();
            operationCompletedEvent.Attributes = MemberAttributes.Public;
            operationCompletedEvent.Type = new CodeTypeReference(eventHandlerType);
            
            if (operationCompletedEventArgsType == null)
            {
                operationCompletedEvent.Type.TypeArguments.Add(asyncCompletedEventArgsType);
            }
            else
            {
                operationCompletedEvent.Type.TypeArguments.Add(operationCompletedEventArgsType.Name);
            }

            operationCompletedEvent.Name = NamingHelper.GetUniqueName(GetOperationCompletedEventName(syncMethodName),
                DoesMethodNameExist, context.Operations);
            
            clientType.Members.Add(operationCompletedEvent);
            return operationCompletedEvent;
        }

        static CodeMemberField CreateBeginOperationDelegate(ServiceContractGenerationContext context,
            CodeTypeDeclaration clientType, string syncMethodName)
        {
            CodeMemberField beginOperationDelegate = new CodeMemberField();
            beginOperationDelegate.Attributes = MemberAttributes.Private;
            beginOperationDelegate.Type = new CodeTypeReference(beginOperationDelegateTypeName);
            beginOperationDelegate.Name = NamingHelper.GetUniqueName(GetBeginOperationDelegateName(syncMethodName),
                DoesMethodNameExist, context.Operations);

            clientType.Members.Add(beginOperationDelegate);
            return beginOperationDelegate;
        }

        static CodeMemberMethod CreateBeginOperationMethod(ServiceContractGenerationContext context, CodeTypeDeclaration clientType, 
            string syncMethodName, CodeMemberMethod beginMethod)
        {
            CodeMemberMethod onBeginOperationMethod = new CodeMemberMethod();
            onBeginOperationMethod.Attributes = MemberAttributes.Private;
            onBeginOperationMethod.ReturnType = new CodeTypeReference(asyncResultType);
            onBeginOperationMethod.Name = NamingHelper.GetUniqueName(GetBeginOperationMethodName(syncMethodName), 
                DoesMethodNameExist, context.Operations);

            CodeParameterDeclarationExpression inValuesParam = new CodeParameterDeclarationExpression();
            inValuesParam.Type = new CodeTypeReference(objectArrayType);
            inValuesParam.Name = NamingHelper.GetUniqueName("inValues", DoesParameterNameExist, beginMethod);
            onBeginOperationMethod.Parameters.Add(inValuesParam);

            CodeMethodInvokeExpression invokeBegin = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), beginMethod.Name);
            CodeExpression inValuesRef = new CodeVariableReferenceExpression(inValuesParam.Name);

            for (int i = 0; i < beginMethod.Parameters.Count - 2; i++)
            {
                CodeVariableDeclarationStatement variableDecl = new CodeVariableDeclarationStatement();
                variableDecl.Type = beginMethod.Parameters[i].Type;
                variableDecl.Name = beginMethod.Parameters[i].Name;
                variableDecl.InitExpression = new CodeCastExpression(variableDecl.Type,
                    new CodeArrayIndexerExpression(inValuesRef, new CodePrimitiveExpression(i)));

                onBeginOperationMethod.Statements.Add(variableDecl);
                invokeBegin.Parameters.Add(new CodeDirectionExpression(beginMethod.Parameters[i].Direction,
                    new CodeVariableReferenceExpression(variableDecl.Name)));
            }

            for (int i = beginMethod.Parameters.Count - 2; i < beginMethod.Parameters.Count; i++)
            {
                onBeginOperationMethod.Parameters.Add(new CodeParameterDeclarationExpression(
                    beginMethod.Parameters[i].Type, beginMethod.Parameters[i].Name));
                invokeBegin.Parameters.Add(new CodeVariableReferenceExpression(beginMethod.Parameters[i].Name));
            }

            onBeginOperationMethod.Statements.Add(new CodeMethodReturnStatement(invokeBegin));
            clientType.Members.Add(onBeginOperationMethod);
            return onBeginOperationMethod;
        }
       
        static CodeMemberField CreateEndOperationDelegate(ServiceContractGenerationContext context,
            CodeTypeDeclaration clientType, string syncMethodName)
        {
            CodeMemberField endOperationDelegate = new CodeMemberField();
            endOperationDelegate.Attributes = MemberAttributes.Private;
            endOperationDelegate.Type = new CodeTypeReference(endOperationDelegateTypeName);
            endOperationDelegate.Name = NamingHelper.GetUniqueName(GetEndOperationDelegateName(syncMethodName),
                DoesMethodNameExist, context.Operations);

            clientType.Members.Add(endOperationDelegate);
            return endOperationDelegate;
        }

        static CodeMemberMethod CreateEndOperationMethod(ServiceContractGenerationContext context, CodeTypeDeclaration clientType, string syncMethodName, CodeMemberMethod endMethod)
        {
            CodeMemberMethod onEndOperationMethod = new CodeMemberMethod();
            onEndOperationMethod.Attributes = MemberAttributes.Private;
            onEndOperationMethod.ReturnType = new CodeTypeReference(objectArrayType);
            onEndOperationMethod.Name = NamingHelper.GetUniqueName(GetEndOperationMethodName(syncMethodName), DoesMethodNameExist, context.Operations);

            int asyncResultParamIndex = GetAsyncResultParamIndex(endMethod);
            CodeMethodInvokeExpression invokeEnd = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), endMethod.Name);
            CodeArrayCreateExpression retArray = new CodeArrayCreateExpression();
            retArray.CreateType = new CodeTypeReference(objectArrayType);
            for (int i = 0; i < endMethod.Parameters.Count; i++)
            {
                if (i == asyncResultParamIndex)
                {
                    onEndOperationMethod.Parameters.Add(new CodeParameterDeclarationExpression(
                        endMethod.Parameters[i].Type, endMethod.Parameters[i].Name));
                    invokeEnd.Parameters.Add(new CodeVariableReferenceExpression(endMethod.Parameters[i].Name));
                }
                else
                {
                    CodeVariableDeclarationStatement variableDecl = new CodeVariableDeclarationStatement(
                        endMethod.Parameters[i].Type, endMethod.Parameters[i].Name);
                    CodeMethodReferenceExpression getDefaultValueMethodRef = new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), getDefaultValueForInitializationMethodName, endMethod.Parameters[i].Type);
                    variableDecl.InitExpression = new CodeMethodInvokeExpression(getDefaultValueMethodRef); 
                    onEndOperationMethod.Statements.Add(variableDecl);

                    invokeEnd.Parameters.Add(new CodeDirectionExpression(endMethod.Parameters[i].Direction,
                            new CodeVariableReferenceExpression(variableDecl.Name)));

                    retArray.Initializers.Add(new CodeVariableReferenceExpression(variableDecl.Name));
                }
            }

            if (endMethod.ReturnType.BaseType != voidTypeRef.BaseType)
            {
                CodeVariableDeclarationStatement retValDecl = new CodeVariableDeclarationStatement();
                retValDecl.Type = endMethod.ReturnType;
                retValDecl.Name = NamingHelper.GetUniqueName("retVal", DoesParameterNameExist, endMethod);
                retValDecl.InitExpression = invokeEnd;
                retArray.Initializers.Add(new CodeVariableReferenceExpression(retValDecl.Name));

                onEndOperationMethod.Statements.Add(retValDecl);
            }
            else
            {
                onEndOperationMethod.Statements.Add(invokeEnd);
            }

            if (retArray.Initializers.Count > 0)
            {
                onEndOperationMethod.Statements.Add(new CodeMethodReturnStatement(retArray));
            }
            else
            {
                onEndOperationMethod.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
            }

            clientType.Members.Add(onEndOperationMethod);
            return onEndOperationMethod;
        }

        static CodeMemberField CreateOperationCompletedDelegate(ServiceContractGenerationContext context, 
            CodeTypeDeclaration clientType, string syncMethodName)
        {
            CodeMemberField operationCompletedDelegate = new CodeMemberField();
            operationCompletedDelegate.Attributes = MemberAttributes.Private;
            operationCompletedDelegate.Type = new CodeTypeReference(sendOrPostCallbackType);
            operationCompletedDelegate.Name = NamingHelper.GetUniqueName(GetOperationCompletedDelegateName(syncMethodName),
                DoesMethodNameExist, context.Operations);

            clientType.Members.Add(operationCompletedDelegate);
            return operationCompletedDelegate;
        }

        static CodeMemberMethod CreateOperationCompletedMethod(ServiceContractGenerationContext context, CodeTypeDeclaration clientType, 
            string syncMethodName, CodeTypeDeclaration operationCompletedEventArgsType, CodeMemberEvent operationCompletedEvent)
        {
            CodeMemberMethod operationCompletedMethod = new CodeMemberMethod();
            operationCompletedMethod.Attributes = MemberAttributes.Private;
            operationCompletedMethod.Name = NamingHelper.GetUniqueName(GetOperationCompletedMethodName(syncMethodName), 
                DoesMethodNameExist, context.Operations);

            operationCompletedMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(objectType), "state"));
            operationCompletedMethod.ReturnType = new CodeTypeReference(voidType);

            CodeVariableDeclarationStatement eventArgsDecl =
                new CodeVariableDeclarationStatement(invokeAsyncCompletedEventArgsTypeName, "e");

            eventArgsDecl.InitExpression = new CodeCastExpression(invokeAsyncCompletedEventArgsTypeName,
                new CodeArgumentReferenceExpression(operationCompletedMethod.Parameters[0].Name));

            CodeObjectCreateExpression newEventArgsExpr;
            CodeVariableReferenceExpression eventArgsRef = new CodeVariableReferenceExpression(eventArgsDecl.Name);
            if (operationCompletedEventArgsType != null)
            {
                newEventArgsExpr = new CodeObjectCreateExpression(operationCompletedEventArgsType.Name,
                    new CodePropertyReferenceExpression(eventArgsRef, EventArgsPropertyNames[0]),
                    new CodePropertyReferenceExpression(eventArgsRef, EventArgsPropertyNames[1]),
                    new CodePropertyReferenceExpression(eventArgsRef, EventArgsPropertyNames[2]),
                    new CodePropertyReferenceExpression(eventArgsRef, EventArgsPropertyNames[3]));
            }
            else
            {
                newEventArgsExpr = new CodeObjectCreateExpression(asyncCompletedEventArgsType,
                    new CodePropertyReferenceExpression(eventArgsRef, EventArgsPropertyNames[1]),
                    new CodePropertyReferenceExpression(eventArgsRef, EventArgsPropertyNames[2]),
                    new CodePropertyReferenceExpression(eventArgsRef, EventArgsPropertyNames[3]));
            }

            CodeEventReferenceExpression completedEvent = new CodeEventReferenceExpression(new CodeThisReferenceExpression(), operationCompletedEvent.Name);

            CodeDelegateInvokeExpression raiseEventExpr = new CodeDelegateInvokeExpression(
                completedEvent,
                new CodeThisReferenceExpression(),
                newEventArgsExpr);

            CodeConditionStatement ifEventHandlerNotNullBlock = new CodeConditionStatement(
                new CodeBinaryOperatorExpression(
                    completedEvent,
                    CodeBinaryOperatorType.IdentityInequality,
                    new CodePrimitiveExpression(null)),
                eventArgsDecl,
                new CodeExpressionStatement(raiseEventExpr));

            operationCompletedMethod.Statements.Add(ifEventHandlerNotNullBlock);

            clientType.Members.Add(operationCompletedMethod);
            return operationCompletedMethod;
        }
        
        static CodeMemberMethod CreateEventAsyncMethod(ServiceContractGenerationContext context, CodeTypeDeclaration clientType, 
            string syncMethodName, CodeMemberMethod beginMethod,
            CodeMemberField beginOperationDelegate, CodeMemberMethod beginOperationMethod, 
            CodeMemberField endOperationDelegate, CodeMemberMethod endOperationMethod, 
            CodeMemberField operationCompletedDelegate, CodeMemberMethod operationCompletedMethod)
        {
            CodeMemberMethod eventAsyncMethod = new CodeMemberMethod();
            eventAsyncMethod.Name = NamingHelper.GetUniqueName(GetEventAsyncMethodName(syncMethodName), 
                DoesMethodNameExist, context.Operations);
            eventAsyncMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            eventAsyncMethod.ReturnType = new CodeTypeReference(voidType);

            CodeArrayCreateExpression invokeAsyncInValues = new CodeArrayCreateExpression(new CodeTypeReference(objectArrayType));
            for (int i = 0; i < beginMethod.Parameters.Count - 2; i++)
            {
                CodeParameterDeclarationExpression beginMethodParameter = beginMethod.Parameters[i];
                CodeParameterDeclarationExpression eventAsyncMethodParameter = new CodeParameterDeclarationExpression(
                    beginMethodParameter.Type, beginMethodParameter.Name);

                eventAsyncMethodParameter.Direction = FieldDirection.In;
                eventAsyncMethod.Parameters.Add(eventAsyncMethodParameter);
                invokeAsyncInValues.Initializers.Add(new CodeVariableReferenceExpression(eventAsyncMethodParameter.Name));
            }

            string userStateParamName = NamingHelper.GetUniqueName("userState", DoesParameterNameExist, eventAsyncMethod);
            eventAsyncMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(objectType), userStateParamName));
            
            eventAsyncMethod.Statements.Add(CreateDelegateIfNotNull(beginOperationDelegate, beginOperationMethod));
            eventAsyncMethod.Statements.Add(CreateDelegateIfNotNull(endOperationDelegate, endOperationMethod));
            eventAsyncMethod.Statements.Add(CreateDelegateIfNotNull(operationCompletedDelegate, operationCompletedMethod));

            CodeMethodInvokeExpression invokeAsync = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), invokeAsyncMethodName);
            invokeAsync.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), beginOperationDelegate.Name));
            if (invokeAsyncInValues.Initializers.Count > 0)
            {
                invokeAsync.Parameters.Add(invokeAsyncInValues);
            }
            else
            {
                invokeAsync.Parameters.Add(new CodePrimitiveExpression(null));
            }
            invokeAsync.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), endOperationDelegate.Name));
            invokeAsync.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), operationCompletedDelegate.Name));
            invokeAsync.Parameters.Add(new CodeVariableReferenceExpression(userStateParamName));
        
            eventAsyncMethod.Statements.Add(new CodeExpressionStatement(invokeAsync));

            clientType.Members.Add(eventAsyncMethod);
            return eventAsyncMethod;
        }

        static CodeMemberMethod CreateEventAsyncMethodOverload(CodeTypeDeclaration clientType, CodeMemberMethod eventAsyncMethod)
        {
            CodeMemberMethod eventAsyncMethodOverload = new CodeMemberMethod();
            eventAsyncMethodOverload.Attributes = eventAsyncMethod.Attributes;
            eventAsyncMethodOverload.Name = eventAsyncMethod.Name;
            eventAsyncMethodOverload.ReturnType = eventAsyncMethod.ReturnType;

            CodeMethodInvokeExpression invokeEventAsyncMethod = new CodeMethodInvokeExpression(
                new CodeThisReferenceExpression(), eventAsyncMethod.Name);

            for (int i = 0; i < eventAsyncMethod.Parameters.Count - 1; i++)
            {
                eventAsyncMethodOverload.Parameters.Add(new CodeParameterDeclarationExpression(
                    eventAsyncMethod.Parameters[i].Type,
                    eventAsyncMethod.Parameters[i].Name));

                invokeEventAsyncMethod.Parameters.Add(new CodeVariableReferenceExpression(
                    eventAsyncMethod.Parameters[i].Name));
            }
            invokeEventAsyncMethod.Parameters.Add(new CodePrimitiveExpression(null));

            eventAsyncMethodOverload.Statements.Add(invokeEventAsyncMethod);

            int eventAsyncMethodPosition = clientType.Members.IndexOf(eventAsyncMethod);
            Fx.Assert(eventAsyncMethodPosition != -1,
                "The eventAsyncMethod must be added to the clientType before calling CreateEventAsyncMethodOverload");

            clientType.Members.Insert(eventAsyncMethodPosition, eventAsyncMethodOverload);
            return eventAsyncMethodOverload;
        }

        static CodeStatement CreateDelegateIfNotNull(CodeMemberField delegateField, CodeMemberMethod delegateMethod)
        {
            return new CodeConditionStatement(
                new CodeBinaryOperatorExpression(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), delegateField.Name),
                    CodeBinaryOperatorType.IdentityEquality,
                    new CodePrimitiveExpression(null)),
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), delegateField.Name),
                    new CodeDelegateCreateExpression(delegateField.Type, 
                        new CodeThisReferenceExpression(), delegateMethod.Name)));
        }

        static string GetClassName(string interfaceName)
        {
            // maybe strip a leading 'I'
            if (interfaceName.Length >= 2 &&
                String.Compare(interfaceName, 0, Strings.InterfaceTypePrefix, 0, Strings.InterfaceTypePrefix.Length, StringComparison.Ordinal) == 0 &&
                Char.IsUpper(interfaceName, 1))
                return interfaceName.Substring(1);
            else
                return interfaceName;
        }

        static string GetEventAsyncMethodName(string syncMethodName)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}Async", syncMethodName);
        }

        static string GetBeginOperationDelegateName(string syncMethodName)
        {
            return string.Format(CultureInfo.InvariantCulture, "onBegin{0}Delegate", syncMethodName);
        }

        static string GetBeginOperationMethodName(string syncMethodName)
        {
            return string.Format(CultureInfo.InvariantCulture, "OnBegin{0}", syncMethodName);
        }

        static string GetEndOperationDelegateName(string syncMethodName)
        {
            return string.Format(CultureInfo.InvariantCulture, "onEnd{0}Delegate", syncMethodName);
        }

        static string GetEndOperationMethodName(string syncMethodName)
        {
            return string.Format(CultureInfo.InvariantCulture, "OnEnd{0}", syncMethodName);
        }

        static string GetOperationCompletedDelegateName(string syncMethodName)
        {
            return string.Format(CultureInfo.InvariantCulture, "on{0}CompletedDelegate", syncMethodName);
        }

        static string GetOperationCompletedMethodName(string syncMethodName)
        {
            return string.Format(CultureInfo.InvariantCulture, "On{0}Completed", syncMethodName);
        }

        static string GetOperationCompletedEventName(string syncMethodName)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}Completed", syncMethodName);
        }

        static string GetOperationCompletedEventArgsTypeName(string syncMethodName)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}CompletedEventArgs", syncMethodName);
        }

        static internal string GetClientClassName(string interfaceName)
        {
            return GetClassName(interfaceName) + Strings.ClientTypeSuffix;
        }

        static bool IsVoid(CodeMemberMethod method)
        {
            return method.ReturnType == null || String.Compare(method.ReturnType.BaseType, typeof(void).FullName, StringComparison.Ordinal) == 0;
        }

        static CodeExpression GetChannelReference()
        {
            return new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), Strings.ClientBaseChannelProperty);
        }

        static class Strings
        {
            public const string ClientBaseChannelProperty = "Channel";
            public const string ClientTypeSuffix = "Client";
            public const string InterfaceTypePrefix = "I";
        }
    }
}
