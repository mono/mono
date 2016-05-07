//------------------------------------------------------------------------------
// <copyright file="WebCodeGenerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {

    
    using System;
    using System.Globalization;
    using System.Collections;
    using System.IO;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Diagnostics;
    using System.Reflection;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using System.Threading;
    using System.Web.Services.Protocols;

    internal enum CodeFlags {
        IsPublic = 0x1,
        IsAbstract = 0x2,
        IsStruct = 0x4,
        IsNew = 0x8,
        IsByRef = 0x10,
        IsOut = 0x20,
        IsInterface = 0x40
    }

    internal class WebCodeGenerator {
        private static CodeAttributeDeclaration generatedCodeAttribute;
        private WebCodeGenerator() { }

        internal static CodeAttributeDeclaration GeneratedCodeAttribute {
            get {
                if (generatedCodeAttribute == null) {
                    CodeAttributeDeclaration decl = new CodeAttributeDeclaration(typeof(GeneratedCodeAttribute).FullName);
                    
                    Assembly a = Assembly.GetEntryAssembly();
                    if (a == null) {
                        a = Assembly.GetExecutingAssembly();
                        if (a == null) {
                            a = typeof(WebCodeGenerator).Assembly;
                        }
                    }
                    AssemblyName assemblyName = a.GetName();
                    decl.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(assemblyName.Name)));
                    string version = GetProductVersion(a);
                    decl.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(version == null ? assemblyName.Version.ToString() : version)));
                    generatedCodeAttribute = decl;
                }
                return generatedCodeAttribute;
            }
        }

        private static string GetProductVersion(Assembly assembly) {
            object[] attributes = assembly.GetCustomAttributes(true);
            for ( int i = 0; i < attributes.Length; i++ ) {
                if (attributes[i] is AssemblyInformationalVersionAttribute) {
                    AssemblyInformationalVersionAttribute version = (AssemblyInformationalVersionAttribute)attributes[i];
                    return version.InformationalVersion;
                }
            }
            return null;
        }

        internal static string[] GetNamespacesForTypes(Type[] types) {
            Hashtable names = new Hashtable();
            for (int i = 0; i < types.Length; i++) {
                string name = types[i].FullName;
                int dot = name.LastIndexOf('.');
                if (dot > 0)
                    names[name.Substring(0, dot)] = types[i];
            }
            string[] ns = new string[names.Keys.Count];
            names.Keys.CopyTo(ns, 0);
            return ns;
        }

        internal static void AddImports(CodeNamespace codeNamespace, string[] namespaces) {
            Debug.Assert(codeNamespace != null, "Inalid (null) CodeNamespace");
            foreach (string ns in namespaces)
                codeNamespace.Imports.Add(new CodeNamespaceImport(ns));
        }

        static CodeMemberProperty CreatePropertyDeclaration(CodeMemberField field, string name, string typeName) {
            CodeMemberProperty prop = new CodeMemberProperty();
            prop.Type = new CodeTypeReference(typeName);
            prop.Name = name;
            //add get
            CodeMethodReturnStatement ret = new CodeMethodReturnStatement();
            ret.Expression = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name);
            prop.GetStatements.Add(ret);
   
            CodeExpression left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name);
            CodeExpression right = new CodeArgumentReferenceExpression("value");
            prop.SetStatements.Add(new CodeAssignStatement(left, right));
            return prop;
        }

        internal static CodeTypeMember AddMember(CodeTypeDeclaration codeClass, string typeName, string memberName, CodeExpression initializer, CodeAttributeDeclarationCollection metadata, CodeFlags flags, CodeGenerationOptions options) {
            CodeTypeMember member;
            bool generateProperty = (options & CodeGenerationOptions.GenerateProperties) != 0;
            string fieldName = generateProperty ? MakeFieldName(memberName) : memberName;

            CodeMemberField field = new CodeMemberField(typeName, fieldName);
            field.InitExpression = initializer;

            if (generateProperty) {
                codeClass.Members.Add(field);
                member = CreatePropertyDeclaration(field, memberName, typeName);
            }
            else {
                member = field;
            }

            member.CustomAttributes = metadata;
            if ((flags & CodeFlags.IsPublic) != 0)
                member.Attributes = (field.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;

            codeClass.Members.Add(member);
            return member;
        }

        internal static string FullTypeName(XmlMemberMapping mapping, CodeDomProvider codeProvider) {
            return mapping.GenerateTypeName(codeProvider);
        }

        static string MakeFieldName(string name) {
            return CodeIdentifier.MakeCamel(name) + "Field";
        }

        internal static CodeConstructor AddConstructor(CodeTypeDeclaration codeClass, string[] parameterTypeNames, string[] parameterNames, CodeAttributeDeclarationCollection metadata, CodeFlags flags) {
            CodeConstructor ctor = new CodeConstructor();

            if ((flags & CodeFlags.IsPublic) != 0)
                ctor.Attributes = (ctor.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            if ((flags & CodeFlags.IsAbstract) != 0)
                ctor.Attributes |= MemberAttributes.Abstract;

            ctor.CustomAttributes = metadata;

            Debug.Assert(parameterTypeNames.Length == parameterNames.Length, "invalid set of parameters");
            for (int i = 0; i < parameterTypeNames.Length; i++) {
                CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression(parameterTypeNames[i], parameterNames[i]);
                ctor.Parameters.Add(param);
            }
            codeClass.Members.Add(ctor);
            return ctor;
        }

        internal static CodeMemberMethod AddMethod(CodeTypeDeclaration codeClass, string methodName, 
            CodeFlags[] parameterFlags, string[] parameterTypeNames, string[] parameterNames, 
            string returnTypeName, CodeAttributeDeclarationCollection metadata, CodeFlags flags) {

            return AddMethod(codeClass, methodName, parameterFlags, 
                parameterTypeNames, parameterNames, new CodeAttributeDeclarationCollection[0],
                returnTypeName, metadata, flags);
        }

        internal static CodeMemberMethod AddMethod(CodeTypeDeclaration codeClass, string methodName, 
            CodeFlags[] parameterFlags, string[] parameterTypeNames, string[] parameterNames, 
            CodeAttributeDeclarationCollection[] parameterAttributes, string returnTypeName, CodeAttributeDeclarationCollection metadata, CodeFlags flags) {

            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = methodName;
            method.ReturnType = new CodeTypeReference(returnTypeName);
            method.CustomAttributes = metadata;

            if ((flags & CodeFlags.IsPublic) != 0)
                method.Attributes = (method.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            if ((flags & CodeFlags.IsAbstract) != 0)
                method.Attributes = (method.Attributes & ~MemberAttributes.ScopeMask) | MemberAttributes.Abstract;

            if ((flags & CodeFlags.IsNew) != 0)
                method.Attributes = (method.Attributes & ~MemberAttributes.VTableMask) | MemberAttributes.New;

            Debug.Assert(parameterFlags.Length == parameterTypeNames.Length && parameterTypeNames.Length == parameterNames.Length, "invalid set of parameters");
            for (int i = 0; i < parameterNames.Length; i++) {
                CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression(parameterTypeNames[i], parameterNames[i]);
                
                if ((parameterFlags[i] & CodeFlags.IsByRef) != 0)
                    param.Direction = FieldDirection.Ref;
                else if ((parameterFlags[i] & CodeFlags.IsOut) != 0)
                    param.Direction = FieldDirection.Out;

                if (i < parameterAttributes.Length) {
                    param.CustomAttributes = parameterAttributes[i];
                }
                method.Parameters.Add(param);
            }
            codeClass.Members.Add(method);
            return method;
        }

        internal static CodeTypeDeclaration AddClass(CodeNamespace codeNamespace, string className, string baseClassName, string[] implementedInterfaceNames, CodeAttributeDeclarationCollection metadata, CodeFlags flags, bool isPartial) {
            CodeTypeDeclaration codeClass = CreateClass(className, baseClassName, implementedInterfaceNames, metadata, flags, isPartial);
            codeNamespace.Types.Add(codeClass);
            return codeClass;
        }

        internal static CodeTypeDeclaration CreateClass(string className, string baseClassName, string[] implementedInterfaceNames, CodeAttributeDeclarationCollection metadata, CodeFlags flags, bool isPartial) {
            CodeTypeDeclaration codeClass = new CodeTypeDeclaration(className);
            
            if (baseClassName != null && baseClassName.Length > 0)
                codeClass.BaseTypes.Add(baseClassName);
            foreach (string interfaceName in implementedInterfaceNames)
                codeClass.BaseTypes.Add(interfaceName);
            codeClass.IsStruct = (flags & CodeFlags.IsStruct) != 0;
            if ((flags & CodeFlags.IsPublic) != 0)
                codeClass.TypeAttributes |= TypeAttributes.Public;
            else
                codeClass.TypeAttributes &= ~TypeAttributes.Public;
            if ((flags & CodeFlags.IsAbstract) != 0)
                codeClass.TypeAttributes |= TypeAttributes.Abstract;
            else
                codeClass.TypeAttributes &= ~TypeAttributes.Abstract;

            if ((flags & CodeFlags.IsInterface) != 0)
                codeClass.IsInterface = true;
            else
                codeClass.IsPartial = isPartial;

            codeClass.CustomAttributes = metadata;
            codeClass.CustomAttributes.Add(GeneratedCodeAttribute);
            return codeClass;
        }

        internal static CodeAttributeDeclarationCollection AddCustomAttribute(CodeAttributeDeclarationCollection metadata, Type type, CodeAttributeArgument[] arguments) {
            if (metadata == null) metadata = new CodeAttributeDeclarationCollection();
            CodeAttributeDeclaration attribute = new CodeAttributeDeclaration(type.FullName, arguments);
            metadata.Add(attribute);
            return metadata;    
        }

        internal static CodeAttributeDeclarationCollection AddCustomAttribute(CodeAttributeDeclarationCollection metadata, Type type, CodeExpression[] arguments) {
            return AddCustomAttribute(metadata, type, arguments, new string[0], new CodeExpression[0]);    
        }

        internal static CodeAttributeDeclarationCollection AddCustomAttribute(CodeAttributeDeclarationCollection metadata, Type type, CodeExpression[] parameters, string[] propNames, CodeExpression[] propValues) {
            Debug.Assert(propNames.Length == propValues.Length, "propNames.Length !=  propValues.Length");
            int count = (parameters == null ? 0 : parameters.Length) + (propNames == null ? 0 : propNames.Length);
            CodeAttributeArgument[] arguments = new CodeAttributeArgument[count];

            for (int i = 0; i < parameters.Length; i++)
                arguments[i] = new CodeAttributeArgument(null, parameters[i]);

            for (int i = 0; i < propNames.Length; i++)
                arguments[parameters.Length + i] = new CodeAttributeArgument(propNames[i], propValues[i]);

            return AddCustomAttribute(metadata, type, arguments);
        }

        // public event xxxCompletedEventHandler xxxCompleted;
        internal static void AddEvent(CodeTypeMemberCollection members, string handlerType, string handlerName) {
            CodeMemberEvent eventCompleted = new CodeMemberEvent();
            eventCompleted.Type = new CodeTypeReference(handlerType);
            eventCompleted.Name = handlerName;
            eventCompleted.Attributes = (eventCompleted.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            eventCompleted.Comments.Add(new CodeCommentStatement(Res.GetString(Res.CodeRemarks), true));
            members.Add(eventCompleted);
        }

        // public delegate void xxxCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs args);
        internal static void AddDelegate(CodeTypeDeclarationCollection codeClasses, string handlerType, string handlerArgs) {
            CodeTypeDelegate handler = new CodeTypeDelegate(handlerType);
            handler.CustomAttributes.Add(GeneratedCodeAttribute);
            handler.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "sender"));
            handler.Parameters.Add(new CodeParameterDeclarationExpression(handlerArgs, "e"));
            handler.Comments.Add(new CodeCommentStatement(Res.GetString(Res.CodeRemarks), true));
            codeClasses.Add(handler);
        }

        // private SendOrPostCallback xxxOperationCompleted;
        internal static void AddCallbackDeclaration(CodeTypeMemberCollection members, string callbackMember) {
            CodeMemberField callback = new CodeMemberField();
            callback.Type = new CodeTypeReference(typeof(SendOrPostCallback));
            callback.Name = callbackMember;
            members.Add(callback);
        }

        // private void On_xxx_OperationCompleted(object arg) {..}
        internal static void AddCallbackImplementation(CodeTypeDeclaration codeClass, string callbackName, string handlerName, string handlerArgs, bool methodHasOutParameters) {
            CodeMemberMethod asyncCompleted = WebCodeGenerator.AddMethod(codeClass, callbackName, 
                new CodeFlags[1] { 0 }, new string[] { typeof(object).FullName }, new string[] { "arg" },
                typeof(void).FullName, null, 0);
               
            CodeEventReferenceExpression member = new CodeEventReferenceExpression(new CodeThisReferenceExpression(), handlerName);
            CodeBinaryOperatorExpression checkIfNull = new CodeBinaryOperatorExpression(member, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));

            CodeStatement[] trueStatements = new CodeStatement[2];
            trueStatements[0] = new CodeVariableDeclarationStatement(typeof(InvokeCompletedEventArgs), "invokeArgs", new CodeCastExpression(typeof(InvokeCompletedEventArgs), new CodeArgumentReferenceExpression("arg")));
            CodeVariableReferenceExpression invokeArgs = new CodeVariableReferenceExpression("invokeArgs");
            CodeObjectCreateExpression create = new CodeObjectCreateExpression();
                    
            if (methodHasOutParameters) {
                create.CreateType = new CodeTypeReference(handlerArgs);
                create.Parameters.Add(new CodePropertyReferenceExpression(invokeArgs, "Results"));
            }
            else {
                create.CreateType = new CodeTypeReference(typeof(AsyncCompletedEventArgs));
            }
            create.Parameters.Add(new CodePropertyReferenceExpression(invokeArgs, "Error"));
            create.Parameters.Add(new CodePropertyReferenceExpression(invokeArgs, "Cancelled"));
            create.Parameters.Add(new CodePropertyReferenceExpression(invokeArgs, "UserState"));
            trueStatements[1] = new CodeExpressionStatement(new CodeDelegateInvokeExpression(new CodeEventReferenceExpression(new CodeThisReferenceExpression(), handlerName), new CodeExpression[] { new CodeThisReferenceExpression(), create }));

            asyncCompleted.Statements.Add(new CodeConditionStatement(checkIfNull, trueStatements, new CodeStatement[0]));
        }

        internal static CodeMemberMethod AddAsyncMethod(CodeTypeDeclaration codeClass, string methodName, 
            string[] parameterTypeNames, string[] parameterNames, string callbackMember, string callbackName, string userState) {

            CodeMemberMethod asyncCodeMethod = WebCodeGenerator.AddMethod(codeClass, methodName, 
                new CodeFlags[parameterNames.Length], parameterTypeNames, parameterNames, typeof(void).FullName, null, CodeFlags.IsPublic);

            asyncCodeMethod.Comments.Add(new CodeCommentStatement(Res.GetString(Res.CodeRemarks), true));
            CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), methodName);

            for (int i = 0; i < parameterNames.Length; i++) {
                invoke.Parameters.Add(new CodeArgumentReferenceExpression(parameterNames[i]));
            }
            invoke.Parameters.Add(new CodePrimitiveExpression(null));
            asyncCodeMethod.Statements.Add(invoke);

            asyncCodeMethod = WebCodeGenerator.AddMethod(codeClass, methodName, 
                new CodeFlags[parameterNames.Length], parameterTypeNames, parameterNames, typeof(void).FullName, null, CodeFlags.IsPublic);

            asyncCodeMethod.Comments.Add(new CodeCommentStatement(Res.GetString(Res.CodeRemarks), true));
            
            asyncCodeMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), userState));

            CodeFieldReferenceExpression member = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), callbackMember);
            CodeBinaryOperatorExpression checkIfNull = new CodeBinaryOperatorExpression(member, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
            CodeDelegateCreateExpression createDelegate = new CodeDelegateCreateExpression();
            createDelegate.DelegateType = new CodeTypeReference(typeof(SendOrPostCallback));
            createDelegate.TargetObject = new CodeThisReferenceExpression();
            createDelegate.MethodName = callbackName;

            CodeStatement[] trueStatements = new CodeStatement[] { new CodeAssignStatement(member, createDelegate) };
            asyncCodeMethod.Statements.Add(new CodeConditionStatement(checkIfNull, trueStatements, new CodeStatement[0]));

            return asyncCodeMethod;
        }

        internal static CodeTypeDeclaration CreateArgsClass(string name, string[] paramTypes, string[] paramNames, bool isPartial) {
            CodeTypeDeclaration codeClass = new CodeTypeDeclaration(name);
            codeClass.CustomAttributes.Add(GeneratedCodeAttribute);

            // Add [DebuggerStepThrough]
            codeClass.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(DebuggerStepThroughAttribute).FullName));
            // Add [DesignerCategory("code")]
            codeClass.CustomAttributes.Add(new CodeAttributeDeclaration(typeof(DesignerCategoryAttribute).FullName, new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression("code")) }));

            codeClass.IsPartial = isPartial; 
            codeClass.BaseTypes.Add(new CodeTypeReference(typeof(AsyncCompletedEventArgs)));

            CodeIdentifiers identifiers = new CodeIdentifiers();
            identifiers.AddUnique("Error", "Error");
            identifiers.AddUnique("Cancelled", "Cancelled");
            identifiers.AddUnique("UserState", "UserState");

            for (int i = 0; i < paramNames.Length; i++) {
                if (paramNames[i] != null) {
                    identifiers.AddUnique(paramNames[i], paramNames[i]);
                }
            }
            string results = identifiers.AddUnique("results", "results");
            CodeMemberField data = new CodeMemberField(typeof(object[]), results);
            codeClass.Members.Add(data);
            
            CodeConstructor ctor = new CodeConstructor();
            ctor.Attributes = (ctor.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Assembly;

            CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression(typeof(object[]), results);
            ctor.Parameters.Add(param);
            ctor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Exception), "exception"));
            ctor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(bool), "cancelled"));
            ctor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "userState"));

            ctor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("exception"));
            ctor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("cancelled"));
            ctor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("userState"));

            ctor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), data.Name), new CodeArgumentReferenceExpression(results)));

            codeClass.Members.Add(ctor);

            int index = 0;
            for (int i = 0; i < paramNames.Length; i++) {
                if (paramNames[i] != null) {
                    codeClass.Members.Add(CreatePropertyDeclaration(data, paramNames[i], paramTypes[i], index++));
                }
            }
            codeClass.Comments.Add(new CodeCommentStatement(Res.GetString(Res.CodeRemarks), true));
            return codeClass;
        }

        static CodeMemberProperty CreatePropertyDeclaration(CodeMemberField field, string name, string typeName, int index) {
            CodeMemberProperty prop = new CodeMemberProperty();
            prop.Type = new CodeTypeReference(typeName);
            prop.Name = name;
            prop.Attributes = (prop.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
            //add get
            prop.GetStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "RaiseExceptionIfNecessary", new CodeExpression[0]));
            CodeArrayIndexerExpression valueRef = new CodeArrayIndexerExpression();
            valueRef.TargetObject = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name);
            valueRef.Indices.Add(new CodePrimitiveExpression(index));

            CodeMethodReturnStatement ret = new CodeMethodReturnStatement();
            ret.Expression = new CodeCastExpression(typeName, valueRef);
            prop.GetStatements.Add(ret);
   
            prop.Comments.Add(new CodeCommentStatement(Res.GetString(Res.CodeRemarks), true));
            return prop;
        }
    }
}
