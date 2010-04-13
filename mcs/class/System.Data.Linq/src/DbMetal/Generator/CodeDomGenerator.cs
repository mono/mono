#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq.Mapping;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.CSharp;
using Microsoft.VisualBasic;

using DbLinq.Schema.Dbml;
using DbLinq.Schema.Dbml.Adapter;
using DbLinq.Util;

namespace DbMetal.Generator
{
#if !MONO_STRICT
    public
#endif
    class CodeDomGenerator : ICodeGenerator
    {
        CodeDomProvider Provider { get; set; }

        // Provided only for Processor.EnumerateCodeGenerators().  DO NOT USE.
        public CodeDomGenerator()
        {
        }

        public CodeDomGenerator(CodeDomProvider provider)
        {
            this.Provider = provider;
        }

        public string LanguageCode {
            get { return "*"; }
        }

        public string Extension {
            get { return "*"; }
        }

        public static CodeDomGenerator CreateFromFileExtension(string extension)
        {
            return CreateFromLanguage(CodeDomProvider.GetLanguageFromExtension(extension));
        }

        public static CodeDomGenerator CreateFromLanguage(string language)
        {
            return new CodeDomGenerator(CodeDomProvider.CreateProvider(language));
        }

        public void Write(TextWriter textWriter, Database dbSchema, GenerationContext context)
        {
            Context = context;
            Provider.CreateGenerator(textWriter).GenerateCodeFromNamespace(
                GenerateCodeDomModel(dbSchema), textWriter, 
                new CodeGeneratorOptions() {
                    BracingStyle = "C",
                    IndentString = "\t",
                });
        }

        static void Warning(string format, params object[] args)
        {
            Console.Error.Write(Path.GetFileName(Environment.GetCommandLineArgs()[0]));
            Console.Error.Write(": warning: ");
            Console.Error.WriteLine(format, args);
        }

        private CodeTypeMember CreatePartialMethod(string methodName, params CodeParameterDeclarationExpression[] parameters)
        {
            string prototype = null;
            if (Provider is CSharpCodeProvider)
            {
                prototype =
                    "\t\tpartial void {0}({1});" + Environment.NewLine +
                    "\t\t";
            }
            else if (Provider is VBCodeProvider)
            {
                prototype =
                    "\t\tPartial Private Sub {0}({1})" + Environment.NewLine +
                    "\t\tEnd Sub" + Environment.NewLine +
                    "\t\t";
            }

            if (prototype == null)
            {
                var method = new CodeMemberMethod() {
                    Name = methodName,
                };
                method.Parameters.AddRange(parameters);
                return method;
            }

            var methodDecl = new StringWriter();
            var gen = Provider.CreateGenerator(methodDecl);

            bool comma = false;
            foreach (var p in parameters)
            {
                if (comma)
                    methodDecl.Write(", ");
                comma = true;
                gen.GenerateCodeFromExpression(p, methodDecl, null);
            }
            return new CodeSnippetTypeMember(string.Format(prototype, methodName, methodDecl.ToString()));
        }

        CodeThisReferenceExpression thisReference = new CodeThisReferenceExpression();

        protected GenerationContext Context { get; set; }

        protected virtual CodeNamespace GenerateCodeDomModel(Database database)
        {
            CodeNamespace _namespace = new CodeNamespace(Context.Parameters.Namespace ?? database.ContextNamespace);

            _namespace.Imports.Add(new CodeNamespaceImport("System"));
            _namespace.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));
#if MONO_STRICT
            _namespace.Imports.Add(new CodeNamespaceImport("System.Data"));
            _namespace.Imports.Add(new CodeNamespaceImport("System.Data.Linq"));
            _namespace.Imports.Add(new CodeNamespaceImport("System.Data.Linq.Mapping"));
#else
            AddConditionalImports(_namespace.Imports,
                "System.Data",
                "MONO_STRICT",
                new[] { "System.Data.Linq" },
                new[] { "DbLinq.Data.Linq", "DbLinq.Vendor" },
                "System.Data.Linq.Mapping");
#endif
            _namespace.Imports.Add(new CodeNamespaceImport("System.Diagnostics"));

            var time = Context.Parameters.GenerateTimestamps ? DateTime.Now.ToString("u") : "[TIMESTAMP]";
            var header = new CodeCommentStatement(GenerateCommentBanner(database, time));
            _namespace.Comments.Add(header);

            _namespace.Types.Add(GenerateContextClass(database));
#if !MONO_STRICT
            _namespace.Types.Add(GenerateMonoStrictContextConstructors(database));
            _namespace.Types.Add(GenerateNotMonoStrictContextConstructors(database));
#endif

            foreach (Table table in database.Tables)
                _namespace.Types.Add(GenerateTableClass(table, database));
            return _namespace;
        }

        void AddConditionalImports(CodeNamespaceImportCollection imports,
                string firstImport,
                string conditional,
                string[] importsIfTrue,
                string[] importsIfFalse,
                string lastImport)
        {
            if (Provider is CSharpCodeProvider)
            {
                // HACK HACK HACK
                // Would be better if CodeDom actually supported conditional compilation constructs...
                // This is predecated upon CSharpCodeGenerator.GenerateNamespaceImport() being implemented as:
                //      output.Write ("using ");
                //      output.Write (GetSafeName (import.Namespace));
                //      output.WriteLine (';');
                // Thus, with "crafty" execution of the namespace, we can stuff arbitrary text in there...

                var block = new StringBuilder();
                // No 'using', as GenerateNamespaceImport() writes it.
                block.Append(firstImport).Append(";").Append(Environment.NewLine);
                block.Append("#if ").Append(conditional).Append(Environment.NewLine);
                foreach (var ns in importsIfTrue)
                    block.Append("\tusing ").Append(ns).Append(";").Append(Environment.NewLine);
                block.Append("#else   // ").Append(conditional).Append(Environment.NewLine);
                foreach (var ns in importsIfFalse)
                    block.Append("\tusing ").Append(ns).Append(";").Append(Environment.NewLine);
                block.Append("#endif  // ").Append(conditional).Append(Environment.NewLine);
                block.Append("\tusing ").Append(lastImport);
                // No ';', as GenerateNamespaceImport() writes it.

                imports.Add(new CodeNamespaceImport(block.ToString()));
            }
            else if (Provider is VBCodeProvider)
            {
                // HACK HACK HACK
                // Would be better if CodeDom actually supported conditional compilation constructs...
                // This is predecated upon VBCodeGenerator.GenerateNamespaceImport() being implemented as:
                //      output.Write ("Imports ");
                //      output.Write (import.Namespace);
                //      output.WriteLine ();
                // Thus, with "crafty" execution of the namespace, we can stuff arbitrary text in there...

                var block = new StringBuilder();
                // No 'Imports', as GenerateNamespaceImport() writes it.
                block.Append(firstImport).Append(Environment.NewLine);
                block.Append("#If ").Append(conditional).Append(" Then").Append(Environment.NewLine);
                foreach (var ns in importsIfTrue)
                    block.Append("Imports ").Append(ns).Append(Environment.NewLine);
                block.Append("#Else     ' ").Append(conditional).Append(Environment.NewLine);
                foreach (var ns in importsIfFalse)
                    block.Append("Imports ").Append(ns).Append(Environment.NewLine);
                block.Append("#End If   ' ").Append(conditional).Append(Environment.NewLine);
                block.Append("Imports ").Append(lastImport);
                // No newline, as GenerateNamespaceImport() writes it.

                imports.Add(new CodeNamespaceImport(block.ToString()));
            }
            else
            {
                // Default to using the DbLinq imports
                imports.Add(new CodeNamespaceImport(firstImport));
                foreach (var ns in importsIfTrue)
                    imports.Add(new CodeNamespaceImport(ns));
                imports.Add(new CodeNamespaceImport(lastImport));
            }
        }

        private string GenerateCommentBanner(Database database, string time)
        {
            var result = new StringBuilder();

            // http://www.network-science.de/ascii/
            // http://www.network-science.de/ascii/ascii.php?TEXT=MetalSequel&x=14&y=14&FONT=_all+fonts+with+your+text_&RICH=no&FORM=left&STRE=no&WIDT=80 
            result.Append(
                @"
  ____  _     __  __      _        _ 
 |  _ \| |__ |  \/  | ___| |_ __ _| |
 | | | | '_ \| |\/| |/ _ \ __/ _` | |
 | |_| | |_) | |  | |  __/ || (_| | |
 |____/|_.__/|_|  |_|\___|\__\__,_|_|

");
            result.AppendLine(String.Format(" Auto-generated from {0} on {1}.", database.Name, time));
            result.AppendLine(" Please visit http://code.google.com/p/dblinq2007/ for more information.");

            return result.ToString();
        }

        protected virtual CodeTypeDeclaration GenerateContextClass(Database database)
        {
            var _class = new CodeTypeDeclaration() {
                IsClass         = true, 
                IsPartial       = true, 
                Name            = database.Class, 
                TypeAttributes  = TypeAttributes.Public 
            };

            _class.BaseTypes.Add(GetContextBaseType(database.BaseType));

            var onCreated = CreatePartialMethod("OnCreated");
            onCreated.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Extensibility Method Declarations"));
            onCreated.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, null));
            _class.Members.Add(onCreated);

            // Implement Constructor
            GenerateContextConstructors(_class, database);

            foreach (Table table in database.Tables)
            {
                var tableType = new CodeTypeReference(table.Type.Name);
                var property = new CodeMemberProperty() {
                    Attributes  = MemberAttributes.Public | MemberAttributes.Final,
                    Name        = table.Member, 
                    Type        = new CodeTypeReference("Table", tableType), 
                };
                property.GetStatements.Add(
                    new CodeMethodReturnStatement(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(thisReference, "GetTable", tableType))));
                _class.Members.Add(property);
            }

            foreach (var function in database.Functions)
            {
                GenerateContextFunction(_class, function);
            }

            return _class;
        }

        static string GetContextBaseType(string type)
        {
            string baseType = "DataContext";

            if (!string.IsNullOrEmpty(type))
            {
                var t = TypeLoader.Load(type);
                if (t != null)
                    baseType = t.Name;
            }

            return baseType;
        }

        void GenerateContextConstructors(CodeTypeDeclaration contextType, Database database)
        {
            // .ctor(string connectionString);
            var constructor = new CodeConstructor() {
                Attributes = MemberAttributes.Public,
                Parameters = { new CodeParameterDeclarationExpression(typeof(string), "connectionString") },
            };
            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("connectionString"));
            constructor.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, "OnCreated")));
            contextType.Members.Add(constructor);

#if MONO_STRICT
            // .ctor(IDbConnection connection);
            constructor = new CodeConstructor() {
                Attributes = MemberAttributes.Public,
                Parameters = { new CodeParameterDeclarationExpression("IDbConnection", "connection") },
            };
            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("connection"));
            constructor.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, "OnCreated")));
            contextType.Members.Add(constructor);
#endif

            // .ctor(string connection, MappingSource mappingSource);
            constructor = new CodeConstructor() {
                Attributes = MemberAttributes.Public,
                Parameters = { 
                    new CodeParameterDeclarationExpression(typeof(string), "connection"),
                    new CodeParameterDeclarationExpression("MappingSource", "mappingSource"),
                },
            };
            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("connection"));
            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("mappingSource"));
            constructor.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, "OnCreated")));
            contextType.Members.Add(constructor);

            // .ctor(IDbConnection connection, MappingSource mappingSource);
            constructor = new CodeConstructor() {
                Attributes = MemberAttributes.Public,
                Parameters = { 
                    new CodeParameterDeclarationExpression("IDbConnection", "connection"),
                    new CodeParameterDeclarationExpression("MappingSource", "mappingSource"),
                },
            };
            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("connection"));
            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("mappingSource"));
            constructor.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, "OnCreated")));
            contextType.Members.Add(constructor);
        }

        CodeTypeDeclaration GenerateMonoStrictContextConstructors(Database database)
        {
            var contextType = new CodeTypeDeclaration()
            {
                IsClass         = true,
                IsPartial       = true,
                Name            = database.Class,
                TypeAttributes  = TypeAttributes.Public
            };
            AddConditionalIfElseBlocks(contextType, "MONO_STRICT");

            // .ctor(IDbConnection connection);
            var constructor = new CodeConstructor() {
                Attributes = MemberAttributes.Public,
                Parameters = { new CodeParameterDeclarationExpression("IDbConnection", "connection") },
            };
            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("connection"));
            constructor.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, "OnCreated")));
            contextType.Members.Add(constructor);

            return contextType;
        }

        void AddConditionalIfElseBlocks(CodeTypeMember member, string condition)
        {
            string startIf = null, elseIf = null;
            if (Provider is CSharpCodeProvider)
            {
                startIf = string.Format("Start {0}{1}#if {0}{1}", condition, Environment.NewLine);
                elseIf  = string.Format("End {0}{1}\t#endregion{1}#else     // {0}", condition, Environment.NewLine);
            }
            if (Provider is VBCodeProvider)
            {
                startIf = string.Format("Start {0}\"{1}#If {0} Then{1}    '", condition, Environment.NewLine);
                elseIf  = string.Format("End {0}\"{1}\t#End Region{1}#Else     ' {0}", condition, Environment.NewLine);
            }
            if (startIf != null && elseIf != null)
            {
                member.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, startIf));
                member.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, elseIf));
            }
        }

        void AddConditionalEndifBlocks(CodeTypeMember member, string condition)
        {
            string endIf = null;
            if (Provider is CSharpCodeProvider)
            {
                endIf   = string.Format("End Not {0}{1}\t#endregion{1}#endif     // {0}", condition, Environment.NewLine);
            }
            if (Provider is VBCodeProvider)
            {
                endIf   = string.Format("End Not {0}\"{1}\t#End Region{1}#End If     ' {0}", condition, Environment.NewLine);
            }
            if (endIf != null)
            {
                member.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, endIf));
                member.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, null));
            }
        }

        CodeTypeDeclaration GenerateNotMonoStrictContextConstructors(Database database)
        {
            var contextType = new CodeTypeDeclaration() {
                IsClass         = true,
                IsPartial       = true,
                Name            = database.Class,
                TypeAttributes  = TypeAttributes.Public
            };
            AddConditionalEndifBlocks(contextType, "MONO_STRICT");

            // .ctor(IDbConnection connection);
            var constructor = new CodeConstructor() {
                Attributes = MemberAttributes.Public,
                Parameters = { new CodeParameterDeclarationExpression("IDbConnection", "connection") },
            };
            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("connection"));
            constructor.BaseConstructorArgs.Add(new CodeObjectCreateExpression(Context.SchemaLoader.Vendor.GetType()));
            constructor.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, "OnCreated")));
            contextType.Members.Add(constructor);

            // .ctor(IDbConnection connection, IVendor mappingSource);
            constructor = new CodeConstructor() {
                Attributes = MemberAttributes.Public,
                Parameters = {
                    new CodeParameterDeclarationExpression("IDbConnection", "connection"),
                    new CodeParameterDeclarationExpression("IVendor", "sqlDialect"),
                },
            };
            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("connection"));
            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("sqlDialect"));
            constructor.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, "OnCreated")));
            contextType.Members.Add(constructor);

            // .ctor(IDbConnection connection, MappingSource mappingSource, IVendor mappingSource);
            constructor = new CodeConstructor() {
                Attributes = MemberAttributes.Public,
                Parameters = {
                    new CodeParameterDeclarationExpression("IDbConnection", "connection"),
                    new CodeParameterDeclarationExpression("MappingSource", "mappingSource"),
                    new CodeParameterDeclarationExpression("IVendor", "sqlDialect"),
                },
            };
            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("connection"));
            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("mappingSource"));
            constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("sqlDialect"));
            constructor.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, "OnCreated")));
            contextType.Members.Add(constructor);

            return contextType;
        }
        
        void GenerateContextFunction(CodeTypeDeclaration contextType, Function function)
        {
            if (function == null || string.IsNullOrEmpty(function.Name))
            {
                Warning("L33 Invalid storedProcdure object: missing name.");
                return;
            }

            var methodRetType = GetFunctionReturnType(function);
            var method = new CodeMemberMethod() {
                Attributes  = ToMemberAttributes(function),
                Name        = function.Method ?? function.Name,
                ReturnType  = methodRetType,
                CustomAttributes = {
                    new CodeAttributeDeclaration("Function",
                        new CodeAttributeArgument("Name", new CodePrimitiveExpression(function.Name)),
                        new CodeAttributeArgument("IsComposable", new CodePrimitiveExpression(function.IsComposable))),
                },
            };
            if (method.Parameters != null)
                method.Parameters.AddRange(function.Parameters.Select(x => GetFunctionParameterType(x)).ToArray());
            if (function.Return != null && !string.IsNullOrEmpty(function.Return.DbType))
                method.ReturnTypeCustomAttributes.Add(
                        new CodeAttributeDeclaration("Parameter",
                            new CodeAttributeArgument("DbType", new CodePrimitiveExpression(function.Return.DbType))));

            contextType.Members.Add(method);

            for (int i = 0; i < function.Parameters.Count; ++i)
            {
                var p = function.Parameters[i];
                if (!p.DirectionOut)
                    continue;
                method.Statements.Add(
                        new CodeAssignStatement(
                            new CodeVariableReferenceExpression(p.Name),
                            new CodeDefaultValueExpression(new CodeTypeReference(p.Type))));
            }

            var executeMethodCallArgs = new List<CodeExpression>() {
                thisReference,
                new CodeCastExpression(
                    new CodeTypeReference("System.Reflection.MethodInfo"),
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodeTypeReferenceExpression("System.Reflection.MethodBase"), "GetCurrentMethod"))),
            };
            if (method.Parameters != null)
                executeMethodCallArgs.AddRange(
                        function.Parameters.Select(p => (CodeExpression) new CodeVariableReferenceExpression(p.Name)));
            method.Statements.Add(
                    new CodeVariableDeclarationStatement(
                        new CodeTypeReference("IExecuteResult"),
                        "result",
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(thisReference, "ExecuteMethodCall"),
                            executeMethodCallArgs.ToArray())));
            for (int i = 0; i < function.Parameters.Count; ++i)
            {
                var p = function.Parameters[i];
                if (!p.DirectionOut)
                    continue;
                method.Statements.Add(
                        new CodeAssignStatement(
                            new CodeVariableReferenceExpression(p.Name),
                            new CodeCastExpression(
                                new CodeTypeReference(p.Type),
                                new CodeMethodInvokeExpression(
                                    new CodeMethodReferenceExpression(
                                        new CodeVariableReferenceExpression("result"),
                                        "GetParameterValue"),
                                    new CodePrimitiveExpression(i)))));
            }

            if (methodRetType != null)
            {
                method.Statements.Add(
                        new CodeMethodReturnStatement(
                            new CodeCastExpression(
                                method.ReturnType,
                                new CodePropertyReferenceExpression(
                                    new CodeVariableReferenceExpression("result"),
                                    "ReturnValue"))));
            }
        }

        CodeTypeReference GetFunctionReturnType(Function function)
        {
            CodeTypeReference type = null;
            if (function.Return != null)
            {
                type = GetFunctionType(function.Return.Type);
            }

            bool isDataShapeUnknown = function.ElementType == null
                                      && function.BodyContainsSelectStatement
                                      && !function.IsComposable;
            if (isDataShapeUnknown)
            {
                //if we don't know the shape of results, and the proc body contains some selects,
                //we have no choice but to return an untyped DataSet.
                //
                //TODO: either parse proc body like microsoft, 
                //or create a little GUI tool which would call the proc with test values, to determine result shape.
                type = new CodeTypeReference(typeof(DataSet));
            }
            return type;
        }

        static CodeTypeReference GetFunctionType(string type)
        {
            var t = System.Type.GetType(type);
            if (t == null)
                return new CodeTypeReference(type);
            if (t.IsValueType)
                return new CodeTypeReference(typeof(Nullable<>)) {
                    TypeArguments = {
                        new CodeTypeReference(t),
                    },
                };
            return new CodeTypeReference(t);
        }

        CodeParameterDeclarationExpression GetFunctionParameterType(Parameter parameter)
        {
            var p = new CodeParameterDeclarationExpression(GetFunctionType(parameter.Type), parameter.Name) {
                CustomAttributes = {
                    new CodeAttributeDeclaration("Parameter",
                        new CodeAttributeArgument("Name", new CodePrimitiveExpression(parameter.Name)),
                        new CodeAttributeArgument("DbType", new CodePrimitiveExpression(parameter.DbType))),
                },
            };
            switch (parameter.Direction)
            {
                case DbLinq.Schema.Dbml.ParameterDirection.In:
                    p.Direction = FieldDirection.In;
                    break;
                case DbLinq.Schema.Dbml.ParameterDirection.Out:
                    p.Direction = FieldDirection.Out;
                    break;
                case DbLinq.Schema.Dbml.ParameterDirection.InOut:
                    p.Direction = FieldDirection.In | FieldDirection.Out;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return p;
        }

        protected CodeTypeDeclaration GenerateTableClass(Table table, Database database)
        {
            var _class = new CodeTypeDeclaration() {
                IsClass         = true, 
                IsPartial       = true, 
                Name            = table.Type.Name, 
                TypeAttributes  = TypeAttributes.Public,
                CustomAttributes = {
                    new CodeAttributeDeclaration("Table", 
                        new CodeAttributeArgument("Name", new CodePrimitiveExpression(table.Name))),
                },
            };

            WriteCustomTypes(_class, table);

            var havePrimaryKeys = table.Type.Columns.Any(c => c.IsPrimaryKey);
            if (havePrimaryKeys)
            {
                GenerateINotifyPropertyChanging(_class);
                GenerateINotifyPropertyChanged(_class);
            }

            // Implement Constructor
            var constructor = new CodeConstructor() { Attributes = MemberAttributes.Public };
            // children are EntitySet
            foreach (var child in GetClassChildren(table))
            {
                // if the association has a storage, we use it. Otherwise, we use the property name
                var entitySetMember = GetStorageFieldName(child);
                constructor.Statements.Add(
                    new CodeAssignStatement(
                        new CodeVariableReferenceExpression(entitySetMember),
                        new CodeObjectCreateExpression(
                            new CodeTypeReference("EntitySet", new CodeTypeReference(child.Type)),
                            new CodeDelegateCreateExpression(
                                new CodeTypeReference("Action", new CodeTypeReference(child.Type)),
                                thisReference, child.Member + "_Attach"),
                            new CodeDelegateCreateExpression(
                                new CodeTypeReference("Action", new CodeTypeReference(child.Type)),
                                thisReference, child.Member + "_Detach"))));
            }
            constructor.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, "OnCreated")));
            _class.Members.Add(constructor);

            if (Context.Parameters.GenerateEqualsHash)
            {
                GenerateEntityGetHashCodeAndEquals(_class, table);
            }

            GenerateExtensibilityDeclarations(_class, table);

            // todo: add these when the actually get called
            //partial void OnLoaded();
            //partial void OnValidate(System.Data.Linq.ChangeAction action);

            // columns
            foreach (Column column in table.Type.Columns)
            {
                var relatedAssociations = from a in table.Type.Associations
                                          where a.IsForeignKey && a.TheseKeys.Contains(column.Name)
                                          select a;

                var type = ToCodeTypeReference(column);
                var columnMember = column.Member ?? column.Name;

                var field = new CodeMemberField(type, GetStorageFieldName(column));
                _class.Members.Add(field);
                var fieldReference = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name);

                var onChanging  = GetChangingMethodName(columnMember);
                var onChanged   = GetChangedMethodName(columnMember);

                var property = new CodeMemberProperty();
                property.Type = type;
                property.Name = columnMember;
                property.Attributes = MemberAttributes.Public | MemberAttributes.Final;

                var defAttrValues = new ColumnAttribute();
                var args = new List<CodeAttributeArgument>() {
                    new CodeAttributeArgument("Storage", new CodePrimitiveExpression(GetStorageFieldName(column))),
                    new CodeAttributeArgument("Name", new CodePrimitiveExpression(column.Name)),
                    new CodeAttributeArgument("DbType", new CodePrimitiveExpression(column.DbType)),
                };
                if (defAttrValues.IsPrimaryKey != column.IsPrimaryKey)
                    args.Add(new CodeAttributeArgument("IsPrimaryKey", new CodePrimitiveExpression(column.IsPrimaryKey)));
                if (defAttrValues.IsDbGenerated != column.IsDbGenerated)
                    args.Add(new CodeAttributeArgument("IsDbGenerated", new CodePrimitiveExpression(column.IsDbGenerated)));
                if (column.AutoSync != DbLinq.Schema.Dbml.AutoSync.Default)
                    args.Add(new CodeAttributeArgument("AutoSync", 
                        new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("AutoSync"), column.AutoSync.ToString())));
                if (defAttrValues.CanBeNull != column.CanBeNull)
                    args.Add(new CodeAttributeArgument("CanBeNull", new CodePrimitiveExpression(column.CanBeNull)));
                if (column.Expression != null)
                    args.Add(new CodeAttributeArgument("Expression", new CodePrimitiveExpression(column.Expression)));
                property.CustomAttributes.Add(
                    new CodeAttributeDeclaration("Column", args.ToArray()));
                property.CustomAttributes.Add(new CodeAttributeDeclaration("DebuggerNonUserCode"));

                property.GetStatements.Add(new CodeMethodReturnStatement(fieldReference));

                var whenUpdating = new List<CodeStatement>(
                    from assoc in relatedAssociations
                    select (CodeStatement) new CodeConditionStatement(
                        new CodePropertyReferenceExpression(
                            new CodeVariableReferenceExpression(GetStorageFieldName(assoc)),
                            "HasLoadedOrAssignedValue"),
                        new CodeThrowExceptionStatement(
                            new CodeObjectCreateExpression(typeof(System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException)))));
                whenUpdating.Add(
                        new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, onChanging, new CodePropertySetValueReferenceExpression())));
                if (havePrimaryKeys)
                    whenUpdating.Add(
                            new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, "SendPropertyChanging")));
                whenUpdating.Add(
                        new CodeAssignStatement(fieldReference, new CodePropertySetValueReferenceExpression()));
                if (havePrimaryKeys)
                    whenUpdating.Add(
                            new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, "SendPropertyChanged", new CodePrimitiveExpression(property.Name))));
                whenUpdating.Add(
                        new CodeExpressionStatement(new CodeMethodInvokeExpression(thisReference, onChanged)));

                var fieldType = TypeLoader.Load(column.Type);
                // This is needed for VB.NET generation; 
                // int/string/etc. can use '<>' for comparison, but NOT arrays and other reference types.
                // arrays/etc. require the 'Is' operator, which is CodeBinaryOperatorType.IdentityEquality.
                // The VB IsNot operator is not exposed from CodeDom.
                // Thus, we need to special-case: if fieldType is a ref or nullable type,
                //  generate '(field Is value) = false'; otherwise, 
                //  generate '(field <> value)'
                CodeBinaryOperatorExpression condition = fieldType.IsClass || fieldType.IsNullable()
                    ? ValuesAreNotEqual_Ref(new CodeVariableReferenceExpression(field.Name), new CodePropertySetValueReferenceExpression())
                    : ValuesAreNotEqual(new CodeVariableReferenceExpression(field.Name), new CodePropertySetValueReferenceExpression());
                property.SetStatements.Add(new CodeConditionStatement(condition, whenUpdating.ToArray()));
                _class.Members.Add(property);
            }

            GenerateEntityChildren(_class, table, database);
            GenerateEntityChildrenAttachment(_class, table, database);
            GenerateEntityParents(_class, table, database);

            return _class;
        }

        void WriteCustomTypes(CodeTypeDeclaration entity, Table table)
        {
            // detect required custom types
            foreach (var column in table.Type.Columns)
            {
                var extendedType = column.ExtendedType;
                var enumType = extendedType as EnumType;
                if (enumType != null)
                {
                    Context.ExtendedTypes[column] = new GenerationContext.ExtendedTypeAndName {
                        Type = column.ExtendedType,
                        Table = table
                    };
                }
            }

            var customTypesNames = new List<string>();

            // create names and avoid conflits
            foreach (var extendedTypePair in Context.ExtendedTypes)
            {
                if (extendedTypePair.Value.Table != table)
                    continue;

                if (string.IsNullOrEmpty(extendedTypePair.Value.Type.Name))
                {
                    string name = extendedTypePair.Key.Member + "Type";
                    for (; ; )
                    {
                        if ((from t in Context.ExtendedTypes.Values where t.Type.Name == name select t).FirstOrDefault() == null)
                        {
                            extendedTypePair.Value.Type.Name = name;
                            break;
                        }
                        // at 3rd loop, it will look ugly, however we will never go there
                        name = extendedTypePair.Value.Table.Type.Name + name;
                    }
                }
                customTypesNames.Add(extendedTypePair.Value.Type.Name);
            }

            // write custom types
            if (customTypesNames.Count > 0)
            {
                var customTypes = new List<CodeTypeDeclaration>(customTypesNames.Count);

                foreach (var extendedTypePair in Context.ExtendedTypes)
                {
                    if (extendedTypePair.Value.Table != table)
                        continue;

                    var extendedType = extendedTypePair.Value.Type;
                    var enumValue = extendedType as EnumType;

                    if (enumValue != null)
                    {
                        var enumType = new CodeTypeDeclaration(enumValue.Name) {
                            TypeAttributes = TypeAttributes.Public,
                            IsEnum = true,
                        };
                        customTypes.Add(enumType);
                        var orderedValues = from nv in enumValue orderby nv.Value select nv;
                        int currentValue = 1;
                        foreach (var nameValue in orderedValues)
                        {
                            var field = new CodeMemberField() {
                                Name = nameValue.Key,
                            };
                            enumType.Members.Add(field);
                            if (nameValue.Value != currentValue)
                            {
                                currentValue = nameValue.Value;
                                field.InitExpression = new CodePrimitiveExpression(nameValue.Value);
                            }
                            currentValue++;
                        }
                    }
                }

                if (customTypes.Count == 0)
                    return;
                customTypes.First().StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start,
                        string.Format("Custom type definitions for {0}", string.Join(", ", customTypesNames.ToArray()))));
                customTypes.Last().EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, null));
                entity.Members.AddRange(customTypes.ToArray());
            }
        }

        void GenerateExtensibilityDeclarations(CodeTypeDeclaration entity, Table table)
        {
            var partialMethods = new[] { CreatePartialMethod("OnCreated") }
                .Concat(table.Type.Columns.Select(c => new[] { CreateChangedMethodDecl(c), CreateChangingMethodDecl(c) })
                    .SelectMany(md => md)).ToArray();
            partialMethods.First().StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Extensibility Method Declarations"));
            partialMethods.Last().EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, null));
            entity.Members.AddRange(partialMethods);
        }

        static string GetChangedMethodName(string columnName)
        {
            return string.Format("On{0}Changed", columnName);
        }

        CodeTypeMember CreateChangedMethodDecl(Column column)
        {
            return CreatePartialMethod(GetChangedMethodName(column.Member));
        }

        static string GetChangingMethodName(string columnName)
        {
            return string.Format("On{0}Changing", columnName);
        }

        CodeTypeMember CreateChangingMethodDecl(Column column)
        {
            return CreatePartialMethod(GetChangingMethodName(column.Member),
                    new CodeParameterDeclarationExpression(ToCodeTypeReference(column), "value"));
        }

        static CodeTypeReference ToCodeTypeReference(Column column)
        {
            var t = System.Type.GetType(column.Type);
            if (t == null)
                return new CodeTypeReference(column.Type);
            return t.IsValueType && column.CanBeNull
                ? new CodeTypeReference("System.Nullable", new CodeTypeReference(column.Type))
                : new CodeTypeReference(column.Type);
        }

        CodeBinaryOperatorExpression ValuesAreNotEqual(CodeExpression a, CodeExpression b)
        {
            return new CodeBinaryOperatorExpression(a, CodeBinaryOperatorType.IdentityInequality, b);
        }

        CodeBinaryOperatorExpression ValuesAreNotEqual_Ref(CodeExpression a, CodeExpression b)
        {
            return new CodeBinaryOperatorExpression(
                        new CodeBinaryOperatorExpression(
                            a,
                            CodeBinaryOperatorType.IdentityEquality,
                            b),
                        CodeBinaryOperatorType.ValueEquality,
                        new CodePrimitiveExpression(false));
        }

        CodeBinaryOperatorExpression ValueIsNull(CodeExpression value)
        {
            return new CodeBinaryOperatorExpression(
                value,
                CodeBinaryOperatorType.IdentityEquality,
                new CodePrimitiveExpression(null));
        }

        CodeBinaryOperatorExpression ValueIsNotNull(CodeExpression value)
        {
            return new CodeBinaryOperatorExpression(
                value,
                CodeBinaryOperatorType.IdentityInequality, 
                new CodePrimitiveExpression(null));
        }

        static string GetStorageFieldName(Column column)
        {
            return GetStorageFieldName(column.Storage ?? column.Member);
        }

        static string GetStorageFieldName(string storage)
        {
            if (storage.StartsWith("_"))
                return storage;
            return "_" + storage;
        }

        private void GenerateINotifyPropertyChanging(CodeTypeDeclaration entity)
        {
            entity.BaseTypes.Add(typeof(INotifyPropertyChanging));
            var propertyChangingEvent = new CodeMemberEvent() {
                Attributes  = MemberAttributes.Public,
                Name        = "PropertyChanging",
                Type        = new CodeTypeReference(typeof(PropertyChangingEventHandler)),
                ImplementationTypes = {
                    new CodeTypeReference(typeof(INotifyPropertyChanging))
                },
            };
            var eventArgs = new CodeMemberField(new CodeTypeReference(typeof(PropertyChangingEventArgs)), "emptyChangingEventArgs") {
                Attributes      = MemberAttributes.Static | MemberAttributes.Private,
                InitExpression  = new CodeObjectCreateExpression(new CodeTypeReference(typeof(PropertyChangingEventArgs)),
                    new CodePrimitiveExpression("")),
            };
            var method = new CodeMemberMethod() {
                Attributes  = MemberAttributes.Family,
                Name        = "SendPropertyChanging",
            };
            method.Statements.Add(new CodeVariableDeclarationStatement(typeof(PropertyChangingEventHandler), "h") {
                InitExpression  = new CodeEventReferenceExpression(thisReference, "PropertyChanging"),
            });
            method.Statements.Add(new CodeConditionStatement(
                    ValueIsNotNull(new CodeVariableReferenceExpression("h")),
                    new CodeExpressionStatement(
                        new CodeDelegateInvokeExpression(new CodeVariableReferenceExpression("h"), thisReference, new CodeFieldReferenceExpression(null, "emptyChangingEventArgs")))));

            entity.Members.Add(propertyChangingEvent);
            entity.Members.Add(eventArgs);
            entity.Members.Add(method);
        }

        private void GenerateINotifyPropertyChanged(CodeTypeDeclaration entity)
        {
            entity.BaseTypes.Add(typeof(INotifyPropertyChanged));

            var propertyChangedEvent = new CodeMemberEvent() {
                Attributes = MemberAttributes.Public,
                Name = "PropertyChanged",
                Type = new CodeTypeReference(typeof(PropertyChangedEventHandler)),
                ImplementationTypes = {
                    new CodeTypeReference(typeof(INotifyPropertyChanged))
                },
            };

            var method = new CodeMemberMethod() { 
                Attributes = MemberAttributes.Family, 
                Name = "SendPropertyChanged", 
                Parameters = { new CodeParameterDeclarationExpression(typeof(System.String), "propertyName") } 
            };
            method.Statements.Add(new CodeVariableDeclarationStatement(typeof(PropertyChangedEventHandler), "h") {
                InitExpression = new CodeEventReferenceExpression(thisReference, "PropertyChanged"),
            });
            method.Statements.Add(new CodeConditionStatement(
                    ValueIsNotNull(new CodeVariableReferenceExpression("h")),
                    new CodeExpressionStatement(
                        new CodeDelegateInvokeExpression(new CodeVariableReferenceExpression("h"), thisReference, new CodeObjectCreateExpression(typeof(PropertyChangedEventArgs), new CodeVariableReferenceExpression("propertyName"))))));

            entity.Members.Add(propertyChangedEvent);
            entity.Members.Add(method);
        }

        void GenerateEntityGetHashCodeAndEquals(CodeTypeDeclaration entity, Table table)
        {
            var primaryKeys = table.Type.Columns.Where(c => c.IsPrimaryKey);
            var pkCount = primaryKeys.Count();
            if (pkCount == 0)
            {
                Warning("Table {0} has no primary key(s).  Skipping /generate-equals-hash for this table.",
                        table.Name);
                return;
            }
            entity.BaseTypes.Add(new CodeTypeReference(typeof(IEquatable<>)) {
                TypeArguments = { new CodeTypeReference(entity.Name) },
            });

            var method = new CodeMemberMethod() {
                Attributes  = MemberAttributes.Public | MemberAttributes.Override,
                Name        = "GetHashCode",
                ReturnType  = new CodeTypeReference(typeof(int)),
            };
            entity.Members.Add(method);
            method.Statements.Add(new CodeVariableDeclarationStatement(typeof(int), "hc", new CodePrimitiveExpression(0)));
            var numShifts = 32 / pkCount;
            int pki = 0;
            foreach (var pk in primaryKeys)
            {
                var shift = 1 << (pki++ * numShifts);
                // lack of exclusive-or means we instead split the 32-bit hash code value
                // into pkCount "chunks", each chunk being numShifts in size.
                // Thus, if there are two primary keys, the first primary key gets the
                // lower 16 bits, while the second primray key gets the upper 16 bits.
                CodeStatement update = new CodeAssignStatement(
                        new CodeVariableReferenceExpression("hc"),
                        new CodeBinaryOperatorExpression(
                            new CodeVariableReferenceExpression("hc"),
                            CodeBinaryOperatorType.BitwiseOr,
                            new CodeBinaryOperatorExpression(
                                new CodeMethodInvokeExpression(
                                    new CodeMethodReferenceExpression(
                                        new CodeVariableReferenceExpression(GetStorageFieldName(pk)),
                                        "GetHashCode")),
                                CodeBinaryOperatorType.Multiply,
                                new CodePrimitiveExpression(shift))));
                var pkType = System.Type.GetType(pk.Type);
                if (pk.CanBeNull || (pkType != null && (pkType.IsClass || pkType.IsNullable())))
                {
                    update = new CodeConditionStatement(
                            ValueIsNotNull(new CodeVariableReferenceExpression(GetStorageFieldName(pk))),
                            update);
                }
                method.Statements.Add(update);
            }
            method.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("hc")));

            method = new CodeMemberMethod() {
                Attributes  = MemberAttributes.Public | MemberAttributes.Override,
                Name        = "Equals",
                ReturnType  = new CodeTypeReference(typeof(bool)),
                Parameters = {
                    new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(object)), "value"),
                },
            };
            entity.Members.Add(method);
            method.Statements.Add(
                    new CodeConditionStatement(
                        ValueIsNull(new CodeVariableReferenceExpression("value")), 
                        new CodeMethodReturnStatement(new CodePrimitiveExpression(false))));
            method.Statements.Add(
                    new CodeConditionStatement(
                        ValuesAreNotEqual_Ref(
                            new CodeMethodInvokeExpression(
                                new CodeMethodReferenceExpression(
                                    new CodeVariableReferenceExpression("value"),
                                    "GetType")),
                            new CodeMethodInvokeExpression(
                                new CodeMethodReferenceExpression(thisReference, "GetType"))),
                        new CodeMethodReturnStatement(new CodePrimitiveExpression(false))));
            method.Statements.Add(
                    new CodeVariableDeclarationStatement(
                        new CodeTypeReference(entity.Name),
                        "other",
                        new CodeCastExpression(new CodeTypeReference(entity.Name), new CodeVariableReferenceExpression("value"))));
            method.Statements.Add(
                    new CodeMethodReturnStatement(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(thisReference, "Equals"),
                            new CodeVariableReferenceExpression("other"))));

            method = new CodeMemberMethod() {
                Attributes  = MemberAttributes.Public,
                Name        = "Equals",
                ReturnType  = new CodeTypeReference(typeof(bool)),
                Parameters  = {
                    new CodeParameterDeclarationExpression(new CodeTypeReference(entity.Name), "value"),
                },
                ImplementationTypes = {
                    new CodeTypeReference("IEquatable", new CodeTypeReference(entity.Name)),
                },
            };
            entity.Members.Add(method);
            method.Statements.Add(
                    new CodeConditionStatement(
                        ValueIsNull(new CodeVariableReferenceExpression("value")),
                        new CodeMethodReturnStatement(new CodePrimitiveExpression(false))));

            CodeExpression equals = null;
            foreach (var pk in primaryKeys)
            {
                var compare = new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodePropertyReferenceExpression(
                                new CodeTypeReferenceExpression(
                                    new CodeTypeReference("System.Collections.Generic.EqualityComparer",
                                        new CodeTypeReference(pk.Type))),
                                "Default"),
                            "Equals"),
                        new CodeFieldReferenceExpression(thisReference, GetStorageFieldName(pk)),
                        new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("value"), GetStorageFieldName(pk)));
                equals = equals == null
                    ? (CodeExpression) compare
                    : (CodeExpression) new CodeBinaryOperatorExpression(
                        equals,
                        CodeBinaryOperatorType.BooleanAnd,
                        compare);
            }
            method.Statements.Add(
                    new CodeMethodReturnStatement(equals));
        }

        void GenerateEntityChildren(CodeTypeDeclaration entity, Table table, Database schema)
        {
            var children = GetClassChildren(table);
            if (children.Any())
            {
                var childMembers = new List<CodeTypeMember>();

                foreach (var child in children)
                {
                    bool hasDuplicates = (from c in children where c.Member == child.Member select c).Count() > 1;

                    // the following is apparently useless
                    var targetTable = schema.Tables.FirstOrDefault(t => t.Type.Name == child.Type);
                    if (targetTable == null)
                    {
                        //Logger.Write(Level.Error, "ERROR L143 target table class not found:" + child.Type);
                        continue;
                    }

                    var childType = new CodeTypeReference("EntitySet", new CodeTypeReference(child.Type));
                    var storage = GetStorageFieldName(child);
                    entity.Members.Add(new CodeMemberField(childType, storage));

                    var childName = hasDuplicates
                        ? child.Member + "_" + string.Join("", child.OtherKeys.ToArray())
                        : child.Member;
                    var property = new CodeMemberProperty() {
                        Name        = childName,
                        Type        = childType,
                        Attributes  = ToMemberAttributes(child),
                        CustomAttributes = {
                            new CodeAttributeDeclaration("Association",
                                new CodeAttributeArgument("Storage", new CodePrimitiveExpression(GetStorageFieldName(child))),
                                new CodeAttributeArgument("OtherKey", new CodePrimitiveExpression(child.OtherKey)),
                                new CodeAttributeArgument("ThisKey", new CodePrimitiveExpression(child.ThisKey)),
                                new CodeAttributeArgument("Name", new CodePrimitiveExpression(child.Name))),
                            new CodeAttributeDeclaration("DebuggerNonUserCode"),
                        },
                    };
                    childMembers.Add(property);
                    property.GetStatements.Add(new CodeMethodReturnStatement(
                            new CodeFieldReferenceExpression(thisReference, storage)));
                    property.SetStatements.Add(new CodeAssignStatement(
                            new CodeFieldReferenceExpression(thisReference, storage),
                            new CodePropertySetValueReferenceExpression()));
                }

                if (childMembers.Count == 0)
                    return;
                childMembers.First().StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Children"));
                childMembers.Last().EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, null));
                entity.Members.AddRange(childMembers.ToArray());
            }
        }

        IEnumerable<Association> GetClassChildren(Table table)
        {
            return table.Type.Associations.Where(a => !a.IsForeignKey);
        }

        static MemberAttributes ToMemberAttributes(Association association)
        {
            MemberAttributes attrs = 0;
            if (!association.AccessModifierSpecified)
                attrs |= MemberAttributes.Public;
            else
                switch (association.AccessModifier)
                {
                    case AccessModifier.Internal:           attrs = MemberAttributes.Assembly; break;
                    case AccessModifier.Private:            attrs = MemberAttributes.Private; break;
                    case AccessModifier.Protected:          attrs = MemberAttributes.Family; break;
                    case AccessModifier.ProtectedInternal:  attrs = MemberAttributes.FamilyOrAssembly; break;
                    case AccessModifier.Public:             attrs = MemberAttributes.Public; break;
                    default:
                        throw new ArgumentOutOfRangeException("association", "Modifier value '" + association.AccessModifierSpecified + "' is an unsupported value.");
                }
            if (!association.ModifierSpecified)
                attrs |= MemberAttributes.Final;
            else
                switch (association.Modifier)
                {
                    case MemberModifier.New:        attrs |= MemberAttributes.New | MemberAttributes.Final; break;
                    case MemberModifier.NewVirtual: attrs |= MemberAttributes.New; break;
                    case MemberModifier.Override:   attrs |= MemberAttributes.Override; break;
                    case MemberModifier.Virtual:    break;
                }
            return attrs;
        }

        static MemberAttributes ToMemberAttributes(Function function)
        {
            MemberAttributes attrs = 0;
            if (!function.AccessModifierSpecified)
                attrs |= MemberAttributes.Public;
            else
                switch (function.AccessModifier)
                {
                    case AccessModifier.Internal:           attrs = MemberAttributes.Assembly; break;
                    case AccessModifier.Private:            attrs = MemberAttributes.Private; break;
                    case AccessModifier.Protected:          attrs = MemberAttributes.Family; break;
                    case AccessModifier.ProtectedInternal:  attrs = MemberAttributes.FamilyOrAssembly; break;
                    case AccessModifier.Public:             attrs = MemberAttributes.Public; break;
                    default:
                        throw new ArgumentOutOfRangeException("function", "Modifier value '" + function.AccessModifierSpecified + "' is an unsupported value.");
                }
            if (!function.ModifierSpecified)
                attrs |= MemberAttributes.Final;
            else
                switch (function.Modifier)
                {
                    case MemberModifier.New:        attrs |= MemberAttributes.New | MemberAttributes.Final; break;
                    case MemberModifier.NewVirtual: attrs |= MemberAttributes.New; break;
                    case MemberModifier.Override:   attrs |= MemberAttributes.Override; break;
                    case MemberModifier.Virtual:    break;
                }
            return attrs;
        }

        static string GetStorageFieldName(Association association)
        {
            return association.Storage != null 
                ? GetStorageFieldName(association.Storage) 
                : "_" + CreateIdentifier(association.Member ?? association.Name);
        }

        static string CreateIdentifier(string value)
        {
            return Regex.Replace(value, @"\W", "_");
        }

        void GenerateEntityChildrenAttachment(CodeTypeDeclaration entity, Table table, Database schema)
        {
            var children = GetClassChildren(table).ToList();
            if (!children.Any())
                return;

            var havePrimaryKeys = table.Type.Columns.Any(c => c.IsPrimaryKey);

            var handlers = new List<CodeTypeMember>();

            foreach (var child in children)
            {
                // the reverse child is the association seen from the child
                // we're going to use it...
                var reverseChild = schema.GetReverseAssociation(child);
                // ... to get the parent name
                var memberName = reverseChild.Member;

                var sendPropertyChanging = new CodeExpressionStatement(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(thisReference, "SendPropertyChanging")));

                var attach = new CodeMemberMethod() {
                    Name = child.Member + "_Attach",
                    Parameters = {
                        new CodeParameterDeclarationExpression(child.Type, "entity"),
                    },
                };
                handlers.Add(attach);
                if (havePrimaryKeys)
                    attach.Statements.Add(sendPropertyChanging);
                attach.Statements.Add(
                        new CodeAssignStatement(
                            new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("entity"), memberName),
                            thisReference));

                var detach = new CodeMemberMethod() {
                    Name = child.Member + "_Detach",
                    Parameters = {
                        new CodeParameterDeclarationExpression(child.Type, "entity"),
                    },
                };
                handlers.Add(detach);
                if (havePrimaryKeys)
                    detach.Statements.Add(sendPropertyChanging);
                detach.Statements.Add(
                        new CodeAssignStatement(
                            new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("entity"), memberName),
                            new CodePrimitiveExpression(null)));
            }

            if (handlers.Count == 0)
                return;

            handlers.First().StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Attachment handlers"));
            handlers.Last().EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, null));
            entity.Members.AddRange(handlers.ToArray());
        }

        void GenerateEntityParents(CodeTypeDeclaration entity, Table table, Database schema)
        {
            var parents = table.Type.Associations.Where(a => a.IsForeignKey);
            if (!parents.Any())
                return;

            var parentMembers = new List<CodeTypeMember>();

            foreach (var parent in parents)
            {
                bool hasDuplicates = (from p in parents where p.Member == parent.Member select p).Count() > 1;
                // WriteClassParent(writer, parent, hasDuplicates, schema, context);
                // the following is apparently useless
                DbLinq.Schema.Dbml.Table targetTable = schema.Tables.FirstOrDefault(t => t.Type.Name == parent.Type);
                if (targetTable == null)
                {
                    //Logger.Write(Level.Error, "ERROR L191 target table type not found: " + parent.Type + "  (processing " + parent.Name + ")");
                    continue;
                }

                string member = parent.Member;
                string storageField = GetStorageFieldName(parent);
                // TODO: remove this
                if (member == parent.ThisKey)
                {
                    member = parent.ThisKey + targetTable.Type.Name; //repeat name to prevent collision (same as Linq)
                    storageField = "_x_" + parent.Member;
                }

                var parentType = new CodeTypeReference(targetTable.Type.Name);
                entity.Members.Add(new CodeMemberField(new CodeTypeReference("EntityRef", parentType), storageField) {
                    InitExpression = new CodeObjectCreateExpression(new CodeTypeReference("EntityRef", parentType)),
                });

                var parentName = hasDuplicates
                    ? member + "_" + string.Join("", parent.TheseKeys.ToArray())
                    : member;
                var property = new CodeMemberProperty() {
                    Name        = parentName,
                    Type        = parentType,
                    Attributes  = ToMemberAttributes(parent),
                    CustomAttributes = {
                        new CodeAttributeDeclaration("Association",
                            new CodeAttributeArgument("Storage", new CodePrimitiveExpression(storageField)),
                            new CodeAttributeArgument("OtherKey", new CodePrimitiveExpression(parent.OtherKey)),
                            new CodeAttributeArgument("ThisKey", new CodePrimitiveExpression(parent.ThisKey)),
                            new CodeAttributeArgument("Name", new CodePrimitiveExpression(parent.Name)),
                            new CodeAttributeArgument("IsForeignKey", new CodePrimitiveExpression(parent.IsForeignKey))),
                        new CodeAttributeDeclaration("DebuggerNonUserCode"),
                    },
                };
                parentMembers.Add(property);
                property.GetStatements.Add(new CodeMethodReturnStatement(
                        new CodePropertyReferenceExpression(
                            new CodeFieldReferenceExpression(thisReference, storageField),
                            "Entity")));

                // algorithm is:
                // 1.1. must be different than previous value
                // 1.2. or HasLoadedOrAssignedValue is false (but why?)
                // 2. implementations before change
                // 3. if previous value not null
                // 3.1. place parent in temp variable
                // 3.2. set [Storage].Entity to null
                // 3.3. remove it from parent list
                // 4. assign value to [Storage].Entity
                // 5. if value is not null
                // 5.1. add it to parent list
                // 5.2. set FK members with entity keys
                // 6. else
                // 6.1. set FK members to defaults (null or 0)
                // 7. implementationas after change
                var otherAssociation = schema.GetReverseAssociation(parent);
                var parentEntity = new CodePropertyReferenceExpression(
                        new CodeFieldReferenceExpression(thisReference, storageField),
                        "Entity");
                var parentTable = schema.Tables.Single(t => t.Type.Associations.Contains(parent));
                var childKeys = parent.TheseKeys.ToArray();
                var childColumns = (from ck in childKeys select table.Type.Columns.Single(c => c.Member == ck))
                                    .ToArray();
                var parentKeys = parent.OtherKeys.ToArray();
                property.SetStatements.Add(new CodeConditionStatement(
                        // 1.1
                        ValuesAreNotEqual_Ref(parentEntity, new CodePropertySetValueReferenceExpression()),
                        // 2. TODO: code before the change
                        // 3. 
                        new CodeConditionStatement(
                            ValueIsNotNull(parentEntity),
                            // 3.1
                            new CodeVariableDeclarationStatement(parentType, "previous" + parent.Type, parentEntity),
                            // 3.2
                            new CodeAssignStatement(parentEntity, new CodePrimitiveExpression(null)),
                            // 3.3
                            new CodeExpressionStatement(
                                 new CodeMethodInvokeExpression(
                                    new CodeMethodReferenceExpression(
                                        new CodePropertyReferenceExpression(
                                            new CodeVariableReferenceExpression("previous" + parent.Type),
                                            otherAssociation.Member),
                                        "Remove"),
                                    thisReference))),
                        // 4.
                        new CodeAssignStatement(parentEntity, new CodePropertySetValueReferenceExpression()),
                        // 5. if value is null or not...
                        new CodeConditionStatement(
                            ValueIsNotNull(new CodePropertySetValueReferenceExpression()),
                            // 5.1
                            new CodeStatement[]{
                                new CodeExpressionStatement(
                                    new CodeMethodInvokeExpression(
                                        new CodeMethodReferenceExpression(
                                            new CodePropertyReferenceExpression(
                                                new CodePropertySetValueReferenceExpression(),
                                                otherAssociation.Member),
                                            "Add"),
                                        thisReference))
                            // 5.2
                            }.Concat(Enumerable.Range(0, parentKeys.Length).Select(i =>
                                (CodeStatement) new CodeAssignStatement(
                                    new CodeVariableReferenceExpression(GetStorageFieldName(childColumns[i])),
                                    new CodePropertyReferenceExpression(
                                        new CodePropertySetValueReferenceExpression(),
                                        parentKeys[i]))
                            )).ToArray(),
                            // 6.
                            Enumerable.Range(0, parentKeys.Length).Select(i => {
                                var column = parentTable.Type.Columns.Single(c => c.Member == childKeys[i]);
                                return (CodeStatement) new CodeAssignStatement(
                                    new CodeVariableReferenceExpression(GetStorageFieldName(childColumns[i])),
                                    column.CanBeNull
                                        ? (CodeExpression) new CodePrimitiveExpression(null)
                                        : (CodeExpression) new CodeDefaultValueExpression(new CodeTypeReference(column.Type)));
                            }).ToArray())
                        // 7: TODO
                ));
            }

            if (parentMembers.Count == 0)
                return;
            parentMembers.First().StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Parents"));
            parentMembers.Last().EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, null));
            entity.Members.AddRange(parentMembers.ToArray());
        }
    }
}
