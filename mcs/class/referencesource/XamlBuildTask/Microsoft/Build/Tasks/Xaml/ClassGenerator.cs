//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using System.Reflection;
    using System.Xaml;
    using System.Xaml.Schema;
    using System.Windows.Markup;
    using System.Runtime;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;
    using System.CodeDom.Compiler;
    using System.Runtime.InteropServices;
    using System.Xml;
    using Microsoft.Build.Utilities;
    using XamlBuildTask;

    class ClassGenerator
    {
        static CodeThisReferenceExpression CodeThis = new CodeThisReferenceExpression();
        static CodeAttributeDeclaration generatedCodeAttribute;

        TaskLoggingHelper buildLogger;
        CodeDomProvider codeDomProvider;
        string language;


        public ClassGenerator(TaskLoggingHelper buildLogger, CodeDomProvider codeDomProvider, string language)
        {
            this.buildLogger = buildLogger;
            this.codeDomProvider = codeDomProvider;
            this.language = language;
        }

        public CodeCompileUnit Generate(ClassData classData)
        {
            if (classData == null)
            {
                throw FxTrace.Exception.ArgumentNull("classData");
            }
            CodeCompileUnit result = new CodeCompileUnit();

            // Add global namespace
            CodeNamespace globalNamespace = new CodeNamespace();
            result.Namespaces.Add(globalNamespace);

            CodeTypeDeclaration classDeclaration = GenerateClass(classData);
            if (!String.IsNullOrEmpty(classData.Namespace))
            {
                // Add namespace the class is defined in
                CodeNamespace classNamespace = new CodeNamespace(classData.Namespace);
                result.Namespaces.Add(classNamespace);
                classNamespace.Types.Add(classDeclaration);
            }
            else
            {
                // Add class to global namespace
                globalNamespace.Types.Add(classDeclaration);
            }

            return result;
        }

        static CodeAttributeDeclaration GeneratedCodeAttribute
        {
            get
            {
                if (generatedCodeAttribute == null)
                {
                    AssemblyName assemblyName = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
                    generatedCodeAttribute = new CodeAttributeDeclaration(new CodeTypeReference(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute).FullName),
                        new CodeAttributeArgument(new CodePrimitiveExpression(assemblyName.Name)),
                        new CodeAttributeArgument(new CodePrimitiveExpression(assemblyName.Version.ToString())));
                }

                return generatedCodeAttribute;
            }
        }

        CodeTypeDeclaration GenerateClass(ClassData classData)
        {
            CodeTypeDeclaration result = GenerateClassDeclaration(classData);

            result.Members.Add(new CodeMemberField() { Name = "_contentLoaded", Type = new CodeTypeReference(typeof(bool)) });

            // Generate fields that match x:Name objects
            var fields = classData.NamedObjects;
            var memberFields = new List<CodeMemberField>();
            foreach (NamedObject field in fields)
            {
                CodeMemberField fieldMember = GenerateField(field, classData);
                memberFields.Add(fieldMember);
                result.Members.Add(fieldMember);
            }

            // Generate properties
            if (classData.Properties != null)
            {
                foreach (PropertyData property in classData.Properties)
                {
                    CodeTypeMember[] propertyMembers = GenerateProperty(property, classData);
                    result.Members.AddRange(propertyMembers);
                }
            }

            // Generate all x:Code
            foreach (string code in classData.CodeSnippets)
            {
                CodeSnippetTypeMember codeSnippet = new CodeSnippetTypeMember(code);
                result.Members.Add(codeSnippet);
            }

            // Generate InitializeComponent method 
            CodeMemberMethod initializeMethod = GenerateInitializeMethod(classData, memberFields);
            result.Members.Add(initializeMethod);

            // Generate Before/AfterInitializeComponent partial methods
            if (ArePartialMethodsSupported())
            {
                result.Members.AddRange(GeneratePartialMethods());
            }

            // Generate helper method to look up assembly resources
            result.Members.Add(GenerateFindResourceMethod(classData));

            // Generate ISupportInitialize implementation
            result.BaseTypes.Add(new CodeTypeReference(typeof(ISupportInitialize)));
            result.Members.AddRange(GenerateISupportInitializeImpl(initializeMethod));

            // Generate public parameterless constructor if user-provided source file does not exist
            if (!classData.SourceFileExists)
            {
                result.Members.Add(GenerateConstructorImpl(initializeMethod));
            }

            return result;
        }


        CodeTypeDeclaration GenerateClassDeclaration(ClassData classData)
        {
            if (!this.codeDomProvider.IsValidIdentifier(classData.Name))
            {
                throw FxTrace.Exception.AsError(
                    new InvalidOperationException(SR.InvalidIdentifiers(classData.Name)),
                    classData.FileName);
            }

            // <%= visibility%> partial class <%= className %> : <%= type %>
            // {
            // }
            //
            CodeTypeDeclaration rootType = new CodeTypeDeclaration()
                {
                    Name = classData.Name,
                    IsPartial = true,
                    TypeAttributes = classData.IsPublic ? TypeAttributes.Public : TypeAttributes.NotPublic
                };

            if (classData.Attributes != null && classData.Attributes.Count > 0)
            {
                CodeAttributeDeclarationCollection attributeCollection = GetAttributeDeclarations(classData.Attributes, classData);
                if (attributeCollection != null && attributeCollection.Count > 0)
                {
                    rootType.CustomAttributes.AddRange(attributeCollection);
                }
            }


            string baseClrTypeName;
            bool isLocal = false;
            if (!XamlBuildTaskServices.TryGetClrTypeName(classData.BaseType, classData.RootNamespace, out baseClrTypeName, out isLocal))
            {
                throw FxTrace.Exception.AsError(
                    new InvalidOperationException(SR.TaskCannotResolveType(XamlBuildTaskServices.GetFullTypeName(classData.BaseType))),
                    classData.FileName);
            }
            classData.RequiresCompilationPass2 |= isLocal;
            rootType.BaseTypes.Add(baseClrTypeName);

            Type baseClrType = classData.BaseType.UnderlyingType;
            if (baseClrType != null)
            {
                if (!IsComVisible(baseClrType))
                {
                    CodeAttributeDeclaration comVisibleFalseDeclaration = new CodeAttributeDeclaration("System.Runtime.InteropServices.ComVisible",
                                                                                                       new CodeAttributeArgument(new CodePrimitiveExpression(false)));
                    rootType.CustomAttributes.Add(comVisibleFalseDeclaration);
                }
            }

            return rootType;
        }

        CodeAttributeDeclarationCollection GetAttributeDeclarations(IList<AttributeData> attributes, ClassData classData)
        {
            CodeAttributeDeclarationCollection attributeCollection = new CodeAttributeDeclarationCollection();
            foreach (AttributeData attrib in attributes)
            {
                string clrTypeName;
                bool isLocal = false;
                if (!XamlBuildTaskServices.TryGetClrTypeName(attrib.Type, classData.RootNamespace, out clrTypeName, out isLocal))
                {
                    throw FxTrace.Exception.AsError(
                        new InvalidOperationException(SR.TaskCannotResolveType(XamlBuildTaskServices.GetFullTypeName(attrib.Type))),
                        classData.FileName);
                }
                classData.RequiresCompilationPass2 |= isLocal;

                CodeAttributeArgument[] arguments = new CodeAttributeArgument[attrib.Parameters.Count + attrib.Properties.Count];
                int i;
                for (i = 0; i < attrib.Parameters.Count; i++)
                {
                    arguments[i] = new CodeAttributeArgument(GetCodeExpressionForAttributeArgument(attrib, attrib.Parameters[i], classData));
                }
                foreach (KeyValuePair<string, AttributeParameterData> propertyEntry in attrib.Properties)
                {
                    arguments[i] = new CodeAttributeArgument(propertyEntry.Key, GetCodeExpressionForAttributeArgument(attrib, propertyEntry.Value, classData));
                    i++;
                }
                attributeCollection.Add(new CodeAttributeDeclaration(new CodeTypeReference(clrTypeName), arguments));
            }
            return attributeCollection;
        }

        CodeExpression GetCodeExpressionForAttributeArgument(AttributeData attrib, AttributeParameterData paramInfo, ClassData classData)
        {
            CodeExpression codeExp;
            if (paramInfo.IsArray)
            {
                CodeExpression[] codeInitializationArray;
                if (paramInfo.ArrayContents != null && paramInfo.ArrayContents.Count > 0)
                {
                    codeInitializationArray = new CodeExpression[paramInfo.ArrayContents.Count];
                    for (int i = 0; i < paramInfo.ArrayContents.Count; i++)
                    {
                        codeInitializationArray[i] = GetCodeExpressionForAttributeArgument(/* attrib = */ null, paramInfo.ArrayContents[i], classData);
                    }

                    codeExp = new CodeArrayCreateExpression(paramInfo.Type.UnderlyingType.GetElementType(), codeInitializationArray);
                }
                else
                {
                    codeExp = new CodeArrayCreateExpression(paramInfo.Type.UnderlyingType.GetElementType());
                }
            }
            else
            {
                if (attrib != null && language.Equals("VB") && string.Equals(attrib.Type.UnderlyingType.FullName, typeof(DefaultValueAttribute).FullName) && paramInfo.Type == null)
                {
                    // 
                    // This is a special case for VB DefaultValueAttribute because by default the VB compiler does not compile the following code:
                    // 
                    // < System.ComponentModel.DefaultValueAttribute(Nothing) >
                    // 
                    // VB compiler complained that code has multiple interpretation because DefaultValueAttribute has multiple constructors that accept null.
                    // 
                    // The solution here is to just pick the one that take in an object as a parameter. Internally, all these constructor will simply set
                    // an internal field named value to null and therefore picking which one does not matter anyway.
                    // 
                    codeExp = new CodeCastExpression { TargetType = new CodeTypeReference(typeof(object)), Expression = new CodePrimitiveExpression(null) };
                }
                else if (paramInfo.TextValue == null)
                {
                    codeExp = new CodePrimitiveExpression(null);
                }
                else if (typeof(System.Type).IsAssignableFrom(paramInfo.Type.UnderlyingType))
                {
                    codeExp = new CodeTypeOfExpression(paramInfo.TextValue);
                }
                else if (paramInfo.Type.UnderlyingType == typeof(String))
                {
                    codeExp = new CodePrimitiveExpression(paramInfo.TextValue);
                }
                else if (paramInfo.Type.UnderlyingType == typeof(bool))
                {
                    if (paramInfo.TextValue == "true")
                    {
                        codeExp = new CodePrimitiveExpression(true);
                    }
                    else
                    {
                        codeExp = new CodePrimitiveExpression(false);
                    }
                }
                else
                {
                    codeExp = new CodeSnippetExpression(paramInfo.TextValue);
                }
            }
            return codeExp;
        }

        bool IsComVisible(Type t)
        {
            IList<CustomAttributeData> cads = CustomAttributeData.GetCustomAttributes(t);

            bool found = false;
            bool visible = false;

            foreach (var cad in cads)
            {
                if (cad.Constructor.DeclaringType == typeof(ComVisibleAttribute))
                {
                    found = true;
                    visible = (bool)cad.ConstructorArguments[0].Value;

                    if (!visible)
                    {
                        return false;
                    }
                }
            }

            if (found)
            {
                return true;
            }

            cads = CustomAttributeData.GetCustomAttributes(t.Assembly);

            foreach (var cad in cads)
            {
                if (cad.Constructor.DeclaringType == typeof(ComVisibleAttribute))
                {
                    found = true;
                    visible = (bool)cad.ConstructorArguments[0].Value;

                    if (!visible)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private CodeMemberField GenerateField(NamedObject fieldData, ClassData classData)
        {
            CodeTypeReference fieldCodeType = null;

            if (!GetCodeTypeReferenceFromXamlType(fieldData.Type, classData, out fieldCodeType))
            {
                throw FxTrace.Exception.AsError(
                    new InvalidOperationException(SR.TaskCannotResolveFieldType(XamlBuildTaskServices.GetFullTypeName(fieldData.Type), fieldData.Name)),
                    classData.FileName);
            }

            if (!this.codeDomProvider.IsValidIdentifier(fieldData.Name))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.InvalidIdentifiers(fieldData.Name)),
                    classData.FileName);
            }

            //     <%= fieldData.Visibility %> WithEvents <%= fieldData.Type %> <%= fieldData.Name %>;
            //
            CodeMemberField field = new CodeMemberField()
                {
                    Name = fieldData.Name,
                    Type = fieldCodeType,
                    Attributes = GetMemberAttributes(fieldData.Visibility)
                };
            field.UserData["WithEvents"] = true;
            return field;
        }

        CodeMemberMethod GenerateInitializeMethod(ClassData classData, List<CodeMemberField> memberFields)
        {
            // /// <summary> InitializeComponent </summary>
            // [DebuggerNonUserCodeAttribute]
            // [System.CodeDom.Compiler.GeneratedCodeAttribute("<%= AssemblyName %>", "<%= AssemblyVersion %>")]
            // public void InitializeComponent() {
            //
            CodeMemberMethod initializeMethod = new CodeMemberMethod()
                {
                    Name = "InitializeComponent",
                    Attributes = MemberAttributes.Public | MemberAttributes.Final,
                    CustomAttributes =
                    {
                        new CodeAttributeDeclaration(new CodeTypeReference(typeof(DebuggerNonUserCodeAttribute))),
                        GeneratedCodeAttribute
                    }
                };

            initializeMethod.Comments.AddRange(GenerateXmlComments(initializeMethod.Name));

            // if (__contentLoaded) { return; }
            initializeMethod.Statements.Add(
                new CodeConditionStatement()
                {
                    Condition = new CodeBinaryOperatorExpression()
                    {
                        Left = new CodeFieldReferenceExpression() { FieldName = "_contentLoaded", TargetObject = new CodeThisReferenceExpression() },
                        Operator = CodeBinaryOperatorType.ValueEquality,
                        Right = new CodePrimitiveExpression(true)
                    },
                    TrueStatements = {
                        new CodeMethodReturnStatement()
                    }
                }
                );

            // __contentLoaded = true;
            initializeMethod.Statements.Add(
                new CodeAssignStatement()
                {
                    Left = new CodeFieldReferenceExpression() { FieldName = "_contentLoaded", TargetObject = new CodeThisReferenceExpression() },
                    Right = new CodePrimitiveExpression(true)
                }
                );

            if (ArePartialMethodsSupported())
            {
                // bool isInitialized = false;
                // BeforeInitializeComponent(ref isInitialized);
                // if (isInitialized) {
                //    AfterInitializeComponent();
                //    return;
                // }
                initializeMethod.Statements.Add(new CodeVariableDeclarationStatement(
                    typeof(bool), "isInitialized", new CodePrimitiveExpression(false)));
                initializeMethod.Statements.Add(new CodeMethodInvokeExpression(
                    new CodeThisReferenceExpression(), "BeforeInitializeComponent",
                    new CodeDirectionExpression(FieldDirection.Ref, new CodeVariableReferenceExpression("isInitialized"))
                ));
                initializeMethod.Statements.Add(
                    new CodeConditionStatement
                    {
                        Condition = new CodeBinaryOperatorExpression(
                            new CodeVariableReferenceExpression("isInitialized"),
                            CodeBinaryOperatorType.ValueEquality,
                            new CodePrimitiveExpression(true)
                        ),
                        TrueStatements =
                        {
                            new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "AfterInitializeComponent"),
                            new CodeMethodReturnStatement()
                        }
                    }
                );
            }

            //     string resourceName = FindResource();
            CodeVariableReferenceExpression resourceNameVar =
                initializeMethod.Statements.DeclareVar(
                typeof(string),
                "resourceName",
                new CodeMethodInvokeExpression()
                {
                    Method =
                    new CodeMethodReferenceExpression()
                    {
                        MethodName = "FindResource",
                        TargetObject = new CodeThisReferenceExpression(),
                    },
                }
                );

            //     Stream initializeXaml = typeof(<%= className %>).Assembly.GetManifestResourceStream(resourceName);
            //
            CodeVariableReferenceExpression initializeXamlVar =
                initializeMethod.Statements.DeclareVar(
                typeof(Stream),
                "initializeXaml",
                new CodeMethodInvokeExpression()
                {
                    Method =
                    new CodeMethodReferenceExpression()
                    {
                        MethodName = "GetManifestResourceStream",
                        TargetObject =
                        new CodePropertyReferenceExpression()
                        {
                            PropertyName = "Assembly",
                            TargetObject =
                            new CodeTypeOfExpression()
                            {
                                Type = new CodeTypeReference(classData.Name)
                            }
                        }
                    },
                    Parameters =
                    {
                        new CodeVariableReferenceExpression(resourceNameVar.VariableName),
                    }
                }
                );

            //     var reader = new System.Xaml.XamlXmlReader(new System.IO.StreamReader(initializeXaml));
            //
            CodeVariableReferenceExpression xmlReaderVar = initializeMethod.Statements.DeclareVar(
                typeof(XmlReader), "xmlReader", new CodePrimitiveExpression(null));

            CodeVariableReferenceExpression xamlReaderVar = initializeMethod.Statements.DeclareVar(
                typeof(XamlReader), "reader", new CodePrimitiveExpression(null));

            CodeVariableReferenceExpression objWriterVar = initializeMethod.Statements.DeclareVar(
                typeof(XamlObjectWriter), "objectWriter", new CodePrimitiveExpression(null));

            // Enclose in try finally block
            // This is to call Dispose on the xmlReader in the finally block, which is the CodeDom way of the C# "using" block
            CodeTryCatchFinallyStatement tryCatchFinally = new CodeTryCatchFinallyStatement();
            tryCatchFinally.TryStatements.AddRange(GetInitializeMethodTryStatements(xmlReaderVar, xamlReaderVar, objWriterVar, initializeXamlVar, classData, memberFields));
            tryCatchFinally.FinallyStatements.AddRange(GetInitializeMethodFinallyStatements(xmlReaderVar, xamlReaderVar, objWriterVar));
            initializeMethod.Statements.Add(tryCatchFinally);

            if (ArePartialMethodsSupported())
            {
                // AfterInitializeComponent();
                initializeMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "AfterInitializeComponent"));
            }

            return initializeMethod;
        }

        CodeStatementCollection GetInitializeMethodTryStatements(CodeExpression xmlReaderVar, CodeExpression xamlReaderVar, CodeExpression objWriterVar,
            CodeExpression initializeXamlVar, ClassData classData, List<CodeMemberField> memberFields)
        {
            CodeStatementCollection tryStatements = new CodeStatementCollection();

            // System.Xaml.XamlSchemaContext schemaContext = _XamlStaticHelperNamespace._XamlStaticHelper.SchemaContext;
            CodeVariableReferenceExpression SchemaContextReference = new CodeVariableReferenceExpression(classData.HelperClassFullName + ".SchemaContext");
            CodeVariableReferenceExpression SchemaContext = tryStatements.DeclareVar(typeof(XamlSchemaContext), "schemaContext", SchemaContextReference);

            //    xmlReader = System.Xml.XmlReader.Create(initializeXaml);
            CodeExpression xmlReader = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression()
                {
                    MethodName = "System.Xml.XmlReader.Create"
                },
                initializeXamlVar);
            tryStatements.Add(new CodeAssignStatement(xmlReaderVar, xmlReader));

            //   System.Xaml.XamlXmlReaderSettings readerSettings = new System.Xaml.XamlXmlReaderSettings();
            CodeVariableReferenceExpression readerSettingsVar = tryStatements.DeclareVar(
                    typeof(XamlXmlReaderSettings), "readerSettings", typeof(XamlXmlReaderSettings).New());

            //  readerSettings.LocalAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            tryStatements.Add(
                new CodeAssignStatement(
                    new CodePropertyReferenceExpression(readerSettingsVar, "LocalAssembly"),
                    new CodeMethodInvokeExpression(
                       new CodeMethodReferenceExpression()
                       {
                           MethodName = "System.Reflection.Assembly.GetExecutingAssembly"
                       })));

            //  readerSettings.AllowProtectedMembersOnRoot = true;
            tryStatements.Add(
                new CodeAssignStatement(
                    new CodePropertyReferenceExpression(readerSettingsVar, "AllowProtectedMembersOnRoot"),
                    new CodePrimitiveExpression(true)));

            //  reader = new System.Xaml.XamlXmlReader(xmlReader, schemaContext, readerSettings);
            CodeExpression newReader = typeof(XamlXmlReader).New(xmlReaderVar, SchemaContext, readerSettingsVar);
            tryStatements.Add(new CodeAssignStatement(xamlReaderVar, newReader));

            //     XamlObjectWriterSettings writerSettings = new XamlObjectWriterSettings();
            CodeVariableReferenceExpression writerSettingsVar = tryStatements.DeclareVar(
                typeof(XamlObjectWriterSettings), "writerSettings", typeof(XamlObjectWriterSettings).New());

            //  writerSettings.RootObjectInstance = this;
            tryStatements.Add(new CodeAssignStatement()
            {
                Left = writerSettingsVar.Property("RootObjectInstance"),
                Right = CodeThis
            });

            //  writerSettings.AccessLevel = System.Xaml.Permissions.XamlAccessLevel.PrivateAccessTo(typeof(<TypeBeingGenerated>));
            tryStatements.Add(new CodeAssignStatement()
            {
                Left = writerSettingsVar.Property("AccessLevel"),
                Right = new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression()
                            {
                                MethodName = "System.Xaml.Permissions.XamlAccessLevel.PrivateAccessTo"
                            },
                            new CodeTypeOfExpression(classData.Name)
                        )
            });

            //     var writer = new XamlObjectWriter(schemaContext, settings);
            //
            CodeExpression newObjectWriter = typeof(XamlObjectWriter).New(SchemaContext, writerSettingsVar);
            tryStatements.Add(new CodeAssignStatement(objWriterVar, newObjectWriter));

            //      XamlServices.Transform(reader, writer);
            //
            tryStatements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression()
                        {
                            MethodName = "System.Xaml.XamlServices.Transform"
                        },
                        xamlReaderVar,
                        objWriterVar
                    ));

            //  For all fields generated, generate the wireup to get the value from xaml. For eg, for a field of type "Bar" and name "Baz":
            //  Baz = ((Tests.Build.Tasks.Xaml.Bar)(objectWriter.RootNameScope.FindName("Baz")));
            if (memberFields != null && memberFields.Count > 0)
            {
                foreach (var field in memberFields)
                {
                    tryStatements.Add(
                        new CodeAssignStatement(
                            new CodeVariableReferenceExpression(field.Name),
                            new CodeCastExpression(
                                field.Type,
                                new CodeMethodInvokeExpression(
                                    new CodePropertyReferenceExpression(objWriterVar, "RootNameScope"),
                                    "FindName",
                                    new CodePrimitiveExpression(field.Name)))));

                }
            }

            return tryStatements;
        }

        CodeStatementCollection GetInitializeMethodFinallyStatements(CodeExpression xmlReaderVar, CodeExpression xamlReaderVar, CodeExpression objWriterVar)
        {
            CodeStatementCollection finallyStatements = new CodeStatementCollection();

            CodeConditionStatement xmlReaderNotNull = new CodeConditionStatement();
            xmlReaderNotNull.Condition = new CodeBinaryOperatorExpression(xmlReaderVar, CodeBinaryOperatorType.IdentityInequality,
                new CodePrimitiveExpression(null));
            CodeCastExpression iDisposibleCastXmlReader = new CodeCastExpression(typeof(IDisposable), xmlReaderVar);
            xmlReaderNotNull.TrueStatements.Add(new CodeMethodInvokeExpression(iDisposibleCastXmlReader, "Dispose"));
            finallyStatements.Add(xmlReaderNotNull);

            CodeConditionStatement xamlReaderNotNull = new CodeConditionStatement();
            xamlReaderNotNull.Condition = new CodeBinaryOperatorExpression(xamlReaderVar, CodeBinaryOperatorType.IdentityInequality,
                new CodePrimitiveExpression(null));
            CodeCastExpression iDisposibleCastXamlReader = new CodeCastExpression(typeof(IDisposable), xamlReaderVar);
            xamlReaderNotNull.TrueStatements.Add(new CodeMethodInvokeExpression(iDisposibleCastXamlReader, "Dispose"));
            finallyStatements.Add(xamlReaderNotNull);

            CodeConditionStatement objWriterNotNull = new CodeConditionStatement();
            objWriterNotNull.Condition = new CodeBinaryOperatorExpression(objWriterVar, CodeBinaryOperatorType.IdentityInequality,
                new CodePrimitiveExpression(null));
            CodeCastExpression iDisposibleCastObjWriter = new CodeCastExpression(typeof(IDisposable), objWriterVar);
            objWriterNotNull.TrueStatements.Add(new CodeMethodInvokeExpression(iDisposibleCastObjWriter, "Dispose"));
            finallyStatements.Add(objWriterNotNull);

            return finallyStatements;
        }

        // CodeDOM has no language-independent support for partial methods
        bool ArePartialMethodsSupported()
        {
            return string.Equals(this.language, "C#", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(this.language, "VB", StringComparison.OrdinalIgnoreCase);
        }

        [SuppressMessage(FxCop.Category.Globalization, FxCop.Rule.DoNotPassLiteralsAsLocalizedParameters,
            Justification = "The string literals are code snippets, not localizable values.")]
        CodeTypeMember[] GeneratePartialMethods()
        {
            if (string.Equals(this.language, "C#", StringComparison.OrdinalIgnoreCase))
            {
                return new CodeTypeMember[]
                {
                    new CodeSnippetTypeMember("partial void BeforeInitializeComponent(ref bool isInitialized);\r\n"),
                    new CodeSnippetTypeMember("partial void AfterInitializeComponent();\r\n"),
                };
            }

            if (string.Equals(this.language, "VB", StringComparison.OrdinalIgnoreCase))
            {
                return new CodeTypeMember[]
                {
                    new CodeSnippetTypeMember("Partial Private Sub BeforeInitializeComponent(ByRef isInitialized as Boolean)\r\nEnd Sub\r\n"),
                    new CodeSnippetTypeMember("Partial Private Sub AfterInitializeComponent\r\nEnd Sub\r\n"),
                };
            }

            throw Fx.AssertAndThrow("GeneratePartialMethods should not be called if ArePartialMethodsSupported returns false.");
        }

        CodeMemberMethod GenerateFindResourceMethod(ClassData classData)
        {
            //     [System.CodeDom.Compiler.GeneratedCodeAttribute("<%= AssemblyName %>", "<%= AssemblyVersion %>")]
            //     private string FindResource() {
            //
            CodeMemberMethod findResourceMethod = new CodeMemberMethod()
                {
                    Name = "FindResource",
                    Attributes = MemberAttributes.Private | MemberAttributes.Final,
                    ReturnType = new CodeTypeReference(typeof(string)),
                    CustomAttributes = { GeneratedCodeAttribute }
                };

            //     string[] resources = typeof(<%= className %>).Assembly.GetManifestResourceNames();
            //
            CodeVariableReferenceExpression resourcesVar =
                findResourceMethod.Statements.DeclareVar(
                typeof(string[]),
                "resources",
                new CodeMethodInvokeExpression()
                {
                    Method =
                    new CodeMethodReferenceExpression()
                    {
                        MethodName = "GetManifestResourceNames",
                        TargetObject =
                        new CodePropertyReferenceExpression()
                        {
                            PropertyName = "Assembly",
                            TargetObject =
                            new CodeTypeOfExpression()
                            {
                                Type = new CodeTypeReference(classData.Name)
                            }
                        }
                    },
                }
                );

            // for (int i = 0; i < resources.Length; i++) {
            //     string resource = resources[i];
            //     if (resource.Contains(searchString)) {
            //         return resource;
            //     }
            // }
            findResourceMethod.Statements.Add(
                new CodeIterationStatement()
                {
                    InitStatement = new CodeVariableDeclarationStatement()
                    {
                        Type = new CodeTypeReference(typeof(int)),
                        Name = "i",
                        InitExpression = new CodePrimitiveExpression(0),
                    },
                    TestExpression = new CodeBinaryOperatorExpression()
                    {
                        Left = new CodeVariableReferenceExpression("i"),
                        Operator = CodeBinaryOperatorType.LessThan,
                        Right = new CodePropertyReferenceExpression()
                        {
                            TargetObject = new CodeVariableReferenceExpression(resourcesVar.VariableName),
                            PropertyName = "Length",
                        },
                    },
                    IncrementStatement = new CodeAssignStatement()
                    {
                        Left = new CodeVariableReferenceExpression("i"),
                        Right = new CodeBinaryOperatorExpression()
                        {
                            Left = new CodeVariableReferenceExpression("i"),
                            Operator = CodeBinaryOperatorType.Add,
                            Right = new CodePrimitiveExpression(1),
                        }
                    },
                    Statements = {
                        new CodeVariableDeclarationStatement()
                        {
                            Type = new CodeTypeReference(typeof(string)),
                            Name = "resource",
                            InitExpression = new CodeArrayIndexerExpression(
                            new CodeVariableReferenceExpression(resourcesVar.VariableName),
                            new CodeVariableReferenceExpression("i")),
                        },
                        new CodeConditionStatement()
                        {
                            Condition = new CodeBinaryOperatorExpression()
                            {
                                Left = new CodeMethodInvokeExpression()
                                {
                                    Method = new CodeMethodReferenceExpression()
                                    {
                                        TargetObject = new CodeVariableReferenceExpression("resource"),
                                        MethodName = "Contains",
                                    },
                                    Parameters = { new CodePrimitiveExpression("." + classData.EmbeddedResourceFileName), },
                                },
                                Operator = CodeBinaryOperatorType.BooleanOr,
                                Right = new CodeMethodInvokeExpression()
                                {
                                    Method = new CodeMethodReferenceExpression()
                                    {
                                        TargetObject = new CodeVariableReferenceExpression("resource"),
                                        MethodName = "Equals",
                                    },
                                    Parameters = {
                                        new CodePrimitiveExpression(classData.EmbeddedResourceFileName),
                                    },
                                }                                   
                            },
                            TrueStatements = {
                                new CodeMethodReturnStatement()
                                {
                                    Expression = new CodeVariableReferenceExpression("resource"),
                                }
                            }
                        }
                    },
                }
                );

            // throw new InvalidOperationException("Resource not found.");
            //
            findResourceMethod.Statements.Add(
                new CodeThrowExceptionStatement()
                {
                    ToThrow =
                    new CodeObjectCreateExpression()
                    {
                        CreateType = new CodeTypeReference(typeof(InvalidOperationException)),
                        Parameters =
                        {
                            new CodePrimitiveExpression("Resource not found."),
                        }
                    }
                }
                );

            return findResourceMethod;
        }

        CodeTypeMember[] GenerateISupportInitializeImpl(CodeMemberMethod initializeMethod)
        {
            CodeTypeReference ifaceType = new CodeTypeReference(typeof(ISupportInitialize));

            // Suppress Code Analysis violation errors arising from defining interface methods explicitly.
            //
            CodeAttributeArgument[] suppressMessageArguments = { 
                                                                   new CodeAttributeArgument(new CodePrimitiveExpression("Microsoft.Design")), 
                                                                   new CodeAttributeArgument(new CodePrimitiveExpression("CA1033")) 
                                                               };
            CodeAttributeDeclaration suppressMessageDeclaration = new CodeAttributeDeclaration("System.Diagnostics.CodeAnalysis.SuppressMessage", suppressMessageArguments);

            //    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033")]
            //    [System.CodeDom.Compiler.GeneratedCodeAttribute("<%= AssemblyName %>", "<%= AssemblyVersion %>")]
            //    void ISupportInitialize.BeginInit() {}
            //
            CodeMemberMethod beginMethod = new CodeMemberMethod()
                {
                    PrivateImplementationType = ifaceType,
                    Name = "BeginInit",
                };
            beginMethod.CustomAttributes.Add(suppressMessageDeclaration);
            beginMethod.CustomAttributes.Add(GeneratedCodeAttribute);

            //    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033")]
            //    [System.CodeDom.Compiler.GeneratedCodeAttribute("<%= AssemblyName %>", "<%= AssemblyVersion %>")]
            //    void ISupportInitialize.EndInit()
            //    {
            //        this.InitializeComponent();
            //    }
            //
            CodeMemberMethod endMethod = new CodeMemberMethod()
                {
                    PrivateImplementationType = ifaceType,
                    Name = "EndInit",
                    Statements = { CodeThis.Invoke(initializeMethod.Name) },
                };
            endMethod.CustomAttributes.Add(suppressMessageDeclaration);
            endMethod.CustomAttributes.Add(GeneratedCodeAttribute);

            return new CodeMemberMethod[] { beginMethod, endMethod };
        }

        CodeConstructor GenerateConstructorImpl(CodeMemberMethod initializeMethod)
        {
            // [System.CodeDom.Compiler.GeneratedCodeAttribute("<%= AssemblyName %>", "<%= AssemblyVersion %>")]
            // public <%= className %>()
            //    {
            //        this.InitializeComponent();
            //    }
            //
            CodeConstructor constructor = new CodeConstructor()
                {
                    Attributes = MemberAttributes.Public,
                    Statements = { CodeThis.Invoke(initializeMethod.Name) },
                    CustomAttributes = { GeneratedCodeAttribute }
                };

            return constructor;
        }

        CodeTypeMember[] GenerateProperty(PropertyData property, ClassData classData)
        {
            // 

            CodeTypeReference propertyCodeType = null;

            if (!GetCodeTypeReferenceFromXamlType(property.Type, classData, out propertyCodeType))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.TaskCannotResolvePropertyType(
                    XamlBuildTaskServices.GetFullTypeName(property.Type), property.Name)), classData.FileName);
            }

            if (!this.codeDomProvider.IsValidIdentifier(property.Name))
            {
                throw FxTrace.Exception.AsError(
                    new InvalidOperationException(SR.InvalidIdentifiers(property.Name)),
                    classData.FileName);
            }

            //     private <%= property.Type %> _<%= property.Name %>;
            //
            CodeMemberField fieldMember = new CodeMemberField()
                {
                    Attributes = MemberAttributes.Private,
                    Name = "_" + property.Name,
                    Type = propertyCodeType
                };

            //    public <%= property.Type %> <%= property.Name %> {
            //       get { return this._<%= property.Name %>; }
            //       set { this._<%= property.Name %> = value; }
            //    }
            //
            CodeMemberProperty propertyMember = new CodeMemberProperty()
                {
                    Attributes = MemberAttributes.Final,
                    Name = property.Name,
                    Type = propertyCodeType,
                    GetStatements =
                    {
                        new CodeMethodReturnStatement(CodeThis.Field(fieldMember.Name))
                    },
                    SetStatements =
                    {
                        new CodeAssignStatement()
                        {
                            Left = CodeThis.Field(fieldMember.Name),
                            Right = new CodeVariableReferenceExpression("value")
                        }
                    }
                };
            propertyMember.Attributes |= GetMemberAttributes(property.Visibility);

            if (property.Attributes != null && property.Attributes.Count > 0)
            {
                CodeAttributeDeclarationCollection attributeCollection = GetAttributeDeclarations(property.Attributes, classData);
                if (attributeCollection != null && attributeCollection.Count > 0)
                {
                    propertyMember.CustomAttributes.AddRange(attributeCollection);
                }
            }

            return new CodeTypeMember[] { fieldMember, propertyMember };
        }

        bool GetCodeTypeReferenceFromXamlType(XamlType xamlType, ClassData classData, out CodeTypeReference codeTypeReference)
        {
            codeTypeReference = null;
            string propClrTypeName;
            bool isLocal = false;
            if (!XamlBuildTaskServices.TryGetClrTypeName(xamlType, classData.RootNamespace, out propClrTypeName, out isLocal))
            {
                return false;
            }
            classData.RequiresCompilationPass2 |= isLocal;

            codeTypeReference = new CodeTypeReference(propClrTypeName);
            if (!GetTypeArgumentFromXamlType(codeTypeReference, xamlType, classData))
            {
                return false;
            }
            return true;
        }

        bool GetTypeArgumentFromXamlType(CodeTypeReference codeTypeReference, XamlType xamlType, ClassData classData)
        {
            //
            // Depending on the name passed into the CodeTypeReference 
            // constructor the type args may already be populated
            if (codeTypeReference.TypeArguments != null && codeTypeReference.TypeArguments.Count == 0 &&
                xamlType.TypeArguments != null && xamlType.TypeArguments.Count > 0)
            {
                foreach (XamlType argumentTypeReference in xamlType.TypeArguments)
                {
                    CodeTypeReference argumentCodeTypeReference = null;
                    if (!GetCodeTypeReferenceFromXamlType(argumentTypeReference, classData, out argumentCodeTypeReference))
                    {
                        return false;
                    }
                    codeTypeReference.TypeArguments.Add(argumentCodeTypeReference);
                }
            }

            return true;
        }

        [SuppressMessage(FxCop.Category.Globalization, "CA1303",
            Justification = "Literals are used as comments in generated code.")]
        private CodeCommentStatement[] GenerateXmlComments(string comment)
        {
            //     /// <summary>
            //     /// <%= comment %>
            //     /// </summary>
            return new CodeCommentStatement[] {
                    new CodeCommentStatement("<summary>", true),
                    new CodeCommentStatement(comment, true),
                    new CodeCommentStatement("</summary>", true)
                };
        }

        MemberAttributes GetMemberAttributes(MemberVisibility visibility)
        {
            switch (visibility)
            {
                case MemberVisibility.Private:
                    return MemberAttributes.Private;
                case MemberVisibility.Public:
                    return MemberAttributes.Public;
                case MemberVisibility.Family:
                    return MemberAttributes.Family;
                case MemberVisibility.Assembly:
                    return MemberAttributes.Assembly;
                case MemberVisibility.FamilyOrAssembly:
                    return MemberAttributes.FamilyOrAssembly;
                case MemberVisibility.FamilyAndAssembly:
                    return MemberAttributes.FamilyAndAssembly;
                default:
                    throw Fx.AssertAndThrow("Invalid MemberVisibility value");
            }
        }

        public CodeCompileUnit GenerateHelperClass(string namespaceName, string className, IList<Assembly> loadedAssemblyList)
        {

            CodeCompileUnit result = new CodeCompileUnit();

            // Add global namespace
            CodeNamespace classNamespace = new CodeNamespace(namespaceName);
            result.Namespaces.Add(classNamespace);

            CodeTypeDeclaration classDeclaration = GenerateHelperClassBody(className, loadedAssemblyList);
            classDeclaration.CustomAttributes.Add(GeneratedCodeAttribute);
            classNamespace.Types.Add(classDeclaration);

            return result;
        }

        CodeTypeDeclaration GenerateHelperClassBody(string className, IList<Assembly> loadedAssemblyList)
        {
            // <%= visibility%> partial class <%= className %> : <%= type %>
            // {
            // }
            //
            CodeTypeDeclaration result = new CodeTypeDeclaration()
            {
                Name = className,
                TypeAttributes = TypeAttributes.NotPublic,
            };

            CodeTypeMember[] schemaContextMembers = GenerateSchemaContext();
            result.Members.AddRange(schemaContextMembers);

            CodeTypeMember[] assemblyListMembers = GenerateAssemblyListProperty();
            result.Members.AddRange(assemblyListMembers);

            // Generate helper Loadassembly method to go through all the assemblies and load them
            result.Members.Add(GenerateLoadAssembliesMethod(loadedAssemblyList));

            // Generate helper load to load assemblies correctly
            result.Members.Add(GenerateLoadMethod());

            return result;
        }

        CodeTypeMember[] GenerateAssemblyListProperty()
        {
            ////Generate the following code:

            //private static System.Collections.Generic.IList<System.Reflection.Assembly> assemblyListField;
            //internal static System.Collections.Generic.IList<System.Reflection.Assembly> AssemblyList {
            //    get {
            //        if ((assemblyListField == null)) {
            //            assemblyListField = LoadAssemblies();
            //        }
            //        return assemblyListField;
            //    }
            //}

            CodeMemberField assemblyListField = new CodeMemberField()
            {
                Name = "assemblyListField",
                Type = new CodeTypeReference(typeof(IList<Assembly>)),
                Attributes = MemberAttributes.Private | MemberAttributes.Static
            };
            CodeVariableReferenceExpression assemblyList = new CodeVariableReferenceExpression("assemblyListField");

            CodeMemberProperty AssemblyList = new CodeMemberProperty()
            {
                Name = "AssemblyList",
                Type = new CodeTypeReference(typeof(IList<Assembly>)),
                Attributes = MemberAttributes.Assembly | MemberAttributes.Static,
            };

            CodeConditionStatement assemblyListNull = new CodeConditionStatement(
                new CodeBinaryOperatorExpression(assemblyList, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null)),
                new CodeAssignStatement(assemblyList,
                    new CodeMethodInvokeExpression { Method = new CodeMethodReferenceExpression { MethodName = "LoadAssemblies" } }));
            AssemblyList.GetStatements.Add(assemblyListNull);
            AssemblyList.GetStatements.Add(new CodeMethodReturnStatement(assemblyList));

            return new CodeTypeMember[] { assemblyListField, AssemblyList };
        }

        CodeTypeMember[] GenerateSchemaContext()
        {
            ////Generate the following code:

            //private static System.WeakReference schemaContextField;
            //internal static System.Xaml.XamlSchemaContext SchemaContext {
            //get {
            //    System.Xaml.XamlSchemaContext xsc = null;
            //    if ((schemaContextField != null)) {
            //        xsc = ((System.Xaml.XamlSchemaContext)(schemaContextField.Target));
            //        if ((xsc != null)) {
            //            return xsc;
            //        }
            //    }
            //    if ((AssemblyList.Count > 0)) {
            //        xsc = new System.Xaml.XamlSchemaContext(AssemblyList);
            //    }
            //    else {
            //        xsc = new System.Xaml.XamlSchemaContext();
            //    }
            //    schemaContextField = new System.WeakReference(xsc);
            //    return xsc;
            //}


            CodeMemberField schemaContextField = new CodeMemberField()
            {
                Name = "schemaContextField",
                Type = new CodeTypeReference(typeof(WeakReference)),
                Attributes = MemberAttributes.Private | MemberAttributes.Static,
            };
            CodeVariableReferenceExpression schemaContext = new CodeVariableReferenceExpression("schemaContextField");

            CodeMemberProperty SchemaContext = new CodeMemberProperty()
            {
                Name = "SchemaContext",
                Type = new CodeTypeReference(typeof(XamlSchemaContext)),
                Attributes = MemberAttributes.Assembly | MemberAttributes.Static,
            };

            CodeVariableReferenceExpression xsc = SchemaContext.GetStatements.DeclareVar(typeof(XamlSchemaContext), "xsc", new CodePrimitiveExpression(null));

            CodeConditionStatement getSchemaContextIfNotNull = new CodeConditionStatement();
            CodeBinaryOperatorExpression schemaContextNotNull = new CodeBinaryOperatorExpression(schemaContext,
                CodeBinaryOperatorType.IdentityInequality,
                new CodePrimitiveExpression(null));
            getSchemaContextIfNotNull.Condition = schemaContextNotNull;
            CodeAssignStatement assignSchemaContext = new CodeAssignStatement(xsc,
                new CodeCastExpression(typeof(XamlSchemaContext), new CodePropertyReferenceExpression(schemaContext, "Target")));
            CodeConditionStatement xscReturnIfNotNull = new CodeConditionStatement(
                new CodeBinaryOperatorExpression(xsc, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null)),
                new CodeMethodReturnStatement(xsc));
            getSchemaContextIfNotNull.TrueStatements.Add(assignSchemaContext);
            getSchemaContextIfNotNull.TrueStatements.Add(xscReturnIfNotNull);
            SchemaContext.GetStatements.Add(getSchemaContextIfNotNull);

            CodeVariableReferenceExpression AssemblyList = new CodeVariableReferenceExpression("AssemblyList");
            CodeConditionStatement initSchemaContext = new CodeConditionStatement();
            initSchemaContext.Condition = new CodeBinaryOperatorExpression(
                new CodePropertyReferenceExpression(AssemblyList, "Count"),
                CodeBinaryOperatorType.GreaterThan,
                new CodePrimitiveExpression(0));
            initSchemaContext.TrueStatements.Add(new CodeAssignStatement(xsc, typeof(XamlSchemaContext).New(AssemblyList)));
            initSchemaContext.FalseStatements.Add(new CodeAssignStatement(xsc, typeof(XamlSchemaContext).New()));
            SchemaContext.GetStatements.Add(initSchemaContext);

            CodeAssignStatement assignSchemaContextField = new CodeAssignStatement(schemaContext, typeof(WeakReference).New(xsc));
            SchemaContext.GetStatements.Add(assignSchemaContextField);

            SchemaContext.GetStatements.Add(new CodeMethodReturnStatement(xsc));

            return new CodeTypeMember[] { schemaContextField, SchemaContext };
        }

        CodeMemberMethod GenerateLoadMethod()
        {
            ////Generate the following code:

            //private static System.Reflection.Assembly Load(string assemblyNameVal) {
            //    System.Reflection.AssemblyName assemblyName = new System.Reflection.AssemblyName(assemblyNameVal);
            //    byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
            //    System.Reflection.Assembly asm = null;
            //    try {
            //        asm = System.Reflection.Assembly.Load(assemblyName.FullName);
            //    }
            //    catch (System.Exception ) {
            //        System.Reflection.AssemblyName shortName = new System.Reflection.AssemblyName(assemblyName.Name);
            //        if ((publicKeyToken != null)) {
            //            shortName.SetPublicKeyToken(publicKeyToken);
            //        }
            //        asm = System.Reflection.Assembly.Load(shortName);
            //    }
            //    return asm;
            //}

            CodeMemberMethod loadMethod = new CodeMemberMethod()
            {
                Name = "Load",
                Attributes = MemberAttributes.Private | MemberAttributes.Static,
                ReturnType = new CodeTypeReference(typeof(Assembly))
            };
            loadMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "assemblyNameVal"));
            CodeVariableReferenceExpression assemblyNameVal = new CodeVariableReferenceExpression("assemblyNameVal");

            CodeExpression initAssemblyName = typeof(AssemblyName).New(assemblyNameVal);
            CodeVariableReferenceExpression assemblyName = loadMethod.Statements.DeclareVar(typeof(AssemblyName), "assemblyName", initAssemblyName);

            CodeVariableReferenceExpression publicKeyToken = loadMethod.Statements.DeclareVar(typeof(byte[]),
                "publicKeyToken",
                new CodeMethodInvokeExpression()
                {
                    Method =
                    new CodeMethodReferenceExpression()
                    {
                        MethodName = "GetPublicKeyToken",
                        TargetObject = assemblyName,
                    },
                }
                );
            CodeVariableReferenceExpression asm = loadMethod.Statements.DeclareVar(typeof(Assembly), "asm", new CodePrimitiveExpression(null));

            CodeExpression publicKeyTokenNotNullExp = new CodeBinaryOperatorExpression(publicKeyToken,
                CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));

            CodeTryCatchFinallyStatement tryCatchExp = new CodeTryCatchFinallyStatement();

            tryCatchExp.TryStatements.Add(new CodeAssignStatement(asm,
                new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression()
                    {
                        MethodName = "System.Reflection.Assembly.Load"
                    },
                    new CodePropertyReferenceExpression(assemblyName, "FullName")
                )
            ));

            CodeCatchClause catchClause = new CodeCatchClause();
            CodeVariableReferenceExpression shortName = catchClause.Statements.DeclareVar(typeof(AssemblyName), "shortName",
                typeof(AssemblyName).New(new CodePropertyReferenceExpression(assemblyName, "Name")));
            CodeConditionStatement setPublicKeyTokenExp = new CodeConditionStatement();
            setPublicKeyTokenExp.Condition = publicKeyTokenNotNullExp;
            setPublicKeyTokenExp.TrueStatements.Add(
                new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(shortName, "SetPublicKeyToken"), publicKeyToken)
            );
            catchClause.Statements.Add(setPublicKeyTokenExp);
            catchClause.Statements.Add(new CodeAssignStatement(asm,
                new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression()
                    {
                        MethodName = "System.Reflection.Assembly.Load"
                    },
                    shortName
                )
            ));
            tryCatchExp.CatchClauses.Add(catchClause);

            loadMethod.Statements.Add(tryCatchExp);
            loadMethod.Statements.Add(new CodeMethodReturnStatement(asm));
            return loadMethod;
        }

        CodeMemberMethod GenerateLoadAssembliesMethod(IEnumerable<Assembly> references)
        {
            //// Generate the following code:

            //private static System.Collections.Generic.IList<System.Reflection.Assembly> LoadAssemblies() {
            //    System.Collections.Generic.IList<System.Reflection.Assembly> assemblyList = new System.Collections.Generic.List<System.Reflection.Assembly>();
            //
            //    assemblyList.Add(Load(<%= AssemblyFullName %>);
            //      ...
            //    assemblyList.Add(Assembly.GetExecutingAssembly());
            //    return assemblyList;
            //}

            CodeMemberMethod loadAssembliesMethod = new CodeMemberMethod()
            {
                Name = "LoadAssemblies",
                Attributes = MemberAttributes.Private | MemberAttributes.Static,
                ReturnType = new CodeTypeReference(typeof(IList<Assembly>)),
            };

            CodeVariableReferenceExpression assemblyList = loadAssembliesMethod.Statements.DeclareVar(typeof(IList<Assembly>),
                "assemblyList", typeof(List<Assembly>).New());

            foreach (var reference in references)
            {
                loadAssembliesMethod.Statements.Add(
                    new CodeMethodInvokeExpression(assemblyList, "Add",
                       new CodeMethodInvokeExpression(
                           new CodeMethodReferenceExpression()
                           {
                               MethodName = "Load"
                           },
                           new CodePrimitiveExpression(reference.FullName)
                        )
                    )
                );
            }

            loadAssembliesMethod.Statements.Add(
                    new CodeMethodInvokeExpression(assemblyList, "Add",
                       new CodeMethodInvokeExpression(
                           new CodeMethodReferenceExpression()
                           {
                               MethodName = "System.Reflection.Assembly.GetExecutingAssembly"
                           }
                        )
                    )
                );

            loadAssembliesMethod.Statements.Add(new CodeMethodReturnStatement(assemblyList));


            return loadAssembliesMethod;
        }
    }
}
