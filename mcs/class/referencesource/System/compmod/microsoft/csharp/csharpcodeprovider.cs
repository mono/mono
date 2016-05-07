//------------------------------------------------------------------------------
// <copyright file="CSharpCodeProvider.cs" company="Microsoft">
// 
// <OWNER>gpaperin</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Diagnostics;
using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Microsoft.CSharp {    

    #region class CSharpCodeProvider

    /// <devdoc>
    /// <para>[To be supplied.]</para>
    /// </devdoc>
    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class CSharpCodeProvider: CodeDomProvider {
        private CSharpCodeGenerator generator;

        public CSharpCodeProvider() {
            generator = new CSharpCodeGenerator();
        }

        public CSharpCodeProvider(IDictionary<string, string> providerOptions) {
            if (providerOptions == null) {
                throw new ArgumentNullException("providerOptions");
            }

            generator = new CSharpCodeGenerator(providerOptions);
        }

        /// <devdoc>
        /// <para>Retrieves the default extension to use when saving files using this code dom provider.</para>
        /// </devdoc>
        public override string FileExtension {
            get {
                return "cs";
            }
        }

        [Obsolete("Callers should not use the ICodeGenerator interface and should instead use the methods directly on the CodeDomProvider class.")]
        public override ICodeGenerator CreateGenerator() {
            return (ICodeGenerator)generator;
        }

        [Obsolete("Callers should not use the ICodeCompiler interface and should instead use the methods directly on the CodeDomProvider class.")]
        public override ICodeCompiler CreateCompiler() {
            return (ICodeCompiler)generator;
        }

        /// <devdoc>
        /// This method allows a code dom provider implementation to provide a different type converter
        /// for a given data type.  At design time, a designer may pass data types through this
        /// method to see if the code dom provider wants to provide an additional converter.  
        /// A typical way this would be used is if the language this code dom provider implements
        /// does not support all of the values of the MemberAttributes enumeration, or if the language
        /// uses different names (Protected instead of Family, for example).  The default 
        /// implementation just calls TypeDescriptor.GetConverter for the given type.
        /// </devdoc>
        public override TypeConverter GetConverter(Type type) {
            if (type == typeof(MemberAttributes)) {
                return CSharpMemberAttributeConverter.Default;
            }
            else if (type == typeof(TypeAttributes)) {
                return CSharpTypeAttributeConverter.Default;
            }
            
            return base.GetConverter(type);
        }
        
        public override void GenerateCodeFromMember(CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options) {
            generator.GenerateCodeFromMember(member, writer, options);
        }        
    }  // CSharpCodeProvider

    #endregion class CSharpCodeProvider


    #region class CSharpCodeGenerator

    /// <devdoc>
    ///    <para>
    ///       C# (C Sharp) Code Generator.
    ///    </para>
    /// </devdoc>
    internal class CSharpCodeGenerator : ICodeCompiler, ICodeGenerator{
        private IndentedTextWriter output;
        private CodeGeneratorOptions options;
        private CodeTypeDeclaration currentClass;
        private CodeTypeMember currentMember;
        private bool inNestedBinary = false;
        private IDictionary<string, string> provOptions;

        private const int ParameterMultilineThreshold = 15;        
        private const int MaxLineLength = 80;
        private const GeneratorSupport LanguageSupport = GeneratorSupport.ArraysOfArrays |
                                                         GeneratorSupport.EntryPointMethod |
                                                         GeneratorSupport.GotoStatements |
                                                         GeneratorSupport.MultidimensionalArrays |
                                                         GeneratorSupport.StaticConstructors |
                                                         GeneratorSupport.TryCatchStatements |
                                                         GeneratorSupport.ReturnTypeAttributes |
                                                         GeneratorSupport.AssemblyAttributes |
                                                         GeneratorSupport.DeclareValueTypes |
                                                         GeneratorSupport.DeclareEnums | 
                                                         GeneratorSupport.DeclareEvents | 
                                                         GeneratorSupport.DeclareDelegates |
                                                         GeneratorSupport.DeclareInterfaces |
                                                         GeneratorSupport.ParameterAttributes |
                                                         GeneratorSupport.ReferenceParameters |
                                                         GeneratorSupport.ChainedConstructorArguments |
                                                         GeneratorSupport.NestedTypes |
                                                         GeneratorSupport.MultipleInterfaceMembers |
                                                         GeneratorSupport.PublicStaticMembers |
                                                         GeneratorSupport.ComplexExpressions |
#if !FEATURE_PAL
                                                         GeneratorSupport.Win32Resources |
#endif // !FEATURE_PAL
                                                         GeneratorSupport.Resources|
                                                         GeneratorSupport.PartialTypes |
                                                         GeneratorSupport.GenericTypeReference |
                                                         GeneratorSupport.GenericTypeDeclaration |
                                                         GeneratorSupport.DeclareIndexerProperties;
        private static volatile Regex outputRegWithFileAndLine;
        private static volatile Regex outputRegSimple;

        private static readonly string[][] keywords = new string[][] {
            null,           // 1 character
            new string[] {  // 2 characters
                "as",
                "do",
                "if",
                "in",
                "is",
            },
            new string[] {  // 3 characters
                "for",
                "int",
                "new",
                "out",
                "ref",
                "try",
            },
            new string[] {  // 4 characters
                "base",
                "bool",
                "byte",
                "case",
                "char",
                "else",
                "enum",
                "goto",
                "lock",
                "long",
                "null",
                "this",
                "true",
                "uint",
                "void",
            },
            new string[] {  // 5 characters
                "break",
                "catch",
                "class",
                "const",
                "event",
                "false",
                "fixed",
                "float",
                "sbyte",
                "short",
                "throw",
                "ulong",
                "using",
                "while",
            },
            new string[] {  // 6 characters
                "double",
                "extern",
                "object",
                "params",
                "public",
                "return",
                "sealed",
                "sizeof",
                "static",
                "string",
                "struct",
                "switch",
                "typeof",
                "unsafe",
                "ushort",
            },
            new string[] {  // 7 characters
                "checked",
                "decimal",
                "default",
                "finally",
                "foreach",
                "private",
                "virtual",
            },
            new string[] {  // 8 characters
                "abstract",
                "continue",
                "delegate",
                "explicit",
                "implicit",
                "internal",
                "operator",
                "override",
                "readonly",
                "volatile",
            },
            new string[] {  // 9 characters
                "__arglist",
                "__makeref",
                "__reftype",
                "interface",
                "namespace",
                "protected",
                "unchecked",
            },
            new string[] {  // 10 characters
                "__refvalue",
                "stackalloc",
            },
        };

        internal CSharpCodeGenerator() {
        }

        internal CSharpCodeGenerator(IDictionary<string, string> providerOptions) {
            provOptions = providerOptions;
        }

#if DEBUG
        static CSharpCodeGenerator() {
            FixedStringLookup.VerifyLookupTable(keywords, false);

            // Sanity check: try some values;
            Debug.Assert(IsKeyword("for"));
            Debug.Assert(!IsKeyword("foR"));
            Debug.Assert(IsKeyword("operator"));
            Debug.Assert(!IsKeyword("blah"));
        }
#endif

        private bool generatingForLoop = false;

        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       or sets the file extension to use for source files.
        ///    </para>
        /// </devdoc>
        private string FileExtension { get { return ".cs"; } }

        /// <devdoc>
        ///    <para>
        ///       Gets or
        ///       sets the name of the compiler executable.
        ///    </para>
        /// </devdoc>
#if !PLATFORM_UNIX
        private string CompilerName { get { return "csc.exe"; } }
#else // !PLATFORM_UNIX
        private string CompilerName { get { return "csc"; } }
#endif // !PLATFORM_UNIX
        
        /// <devdoc>
        ///    <para>
        ///       Gets or sets the current class name.
        ///    </para>
        /// </devdoc>
        private string CurrentTypeName {
            get {
                if (currentClass != null) {
                    return currentClass.Name;
                }
                return "<% unknown %>";
            }
        }

        private int Indent {
            get {
                return output.Indent;
            }
            set {
                output.Indent = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether the current object being
        ///       generated is an interface.
        ///    </para>
        /// </devdoc>
        private bool IsCurrentInterface {
            get {
                if (currentClass != null && !(currentClass is CodeTypeDelegate)) {
                    return currentClass.IsInterface;
                }
                return false;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether the current object being generated
        ///       is a class.
        ///    </para>
        /// </devdoc>
        private bool IsCurrentClass {
            get {
                if (currentClass != null && !(currentClass is CodeTypeDelegate)) {
                    return currentClass.IsClass;
                }
                return false;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether the current object being generated
        ///       is a struct.
        ///    </para>
        /// </devdoc>
        private bool IsCurrentStruct {
            get {
                if (currentClass != null && !(currentClass is CodeTypeDelegate)) {
                    return currentClass.IsStruct;
                }
                return false;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether the current object being generated
        ///       is an enumeration.
        ///    </para>
        /// </devdoc>
        private bool IsCurrentEnum {
            get {
                if (currentClass != null && !(currentClass is CodeTypeDelegate)) {
                    return currentClass.IsEnum;
                }
                return false;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether the current object being generated
        ///       is a delegate.
        ///    </para>
        /// </devdoc>
        private bool IsCurrentDelegate {
            get {
                if (currentClass != null && currentClass is CodeTypeDelegate) {
                    return true;
                }
                return false;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the token used to represent <see langword='null'/>.
        ///    </para>
        /// </devdoc>
        private string NullToken {
            get {
                return "null";
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        private CodeGeneratorOptions Options {
            get {
                return options;
            }
        }

        private TextWriter Output {
            get {
                return output;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Provides conversion to C-style formatting with escape codes.
        ///    </para>
        /// </devdoc>
        private string QuoteSnippetStringCStyle(string value) {
            StringBuilder b = new StringBuilder(value.Length+5);
            Indentation indentObj = new Indentation((IndentedTextWriter)Output, Indent + 1);

            b.Append("\"");

            int i = 0;
            while(i < value.Length) {
                switch (value[i]) {
                    case '\r':
                        b.Append("\\r");
                        break;
                    case '\t':
                        b.Append("\\t");
                        break;
                    case '\"':
                        b.Append("\\\"");
                        break;
                    case '\'':
                        b.Append("\\\'");
                        break;
                    case '\\':
                        b.Append("\\\\");
                        break;
                    case '\0':
                        b.Append("\\0");
                        break;
                    case '\n':
                        b.Append("\\n");
                        break;
                    case '\u2028':
                    case '\u2029':
                        AppendEscapedChar(b,value[i]);
                        break;

                    default:
                        b.Append(value[i]);
                        break;
                }
                
                if (i > 0 && i % MaxLineLength == 0) {
                    //
                    // If current character is a high surrogate and the following 
                    // character is a low surrogate, don't break them. 
                    // Otherwise when we write the string to a file, we might lose 
                    // the characters.
                    // 
                    if( Char.IsHighSurrogate(value[i])
                        && (i < value.Length -1) 
                        && Char.IsLowSurrogate(value[i+1])){ 
                        b.Append(value[++i]);    
                    }
                    
                    b.Append("\" +");
                    b.Append(Environment.NewLine);
                    b.Append(indentObj.IndentationString);
                    b.Append('\"');
                }
                ++i;
            }

            b.Append("\"");

            return b.ToString();
        }

        private string QuoteSnippetStringVerbatimStyle(string value) {
            StringBuilder b = new StringBuilder(value.Length+5);

            b.Append("@\"");

            for (int i=0; i<value.Length; i++) {
                if (value[i] == '\"')
                    b.Append("\"\"");
                else
                    b.Append(value[i]);
            }

            b.Append("\"");

            return b.ToString();
        }

        /// <devdoc>
        ///    <para>
        ///       Provides conversion to formatting with escape codes.
        ///    </para>
        /// </devdoc>
        private  string QuoteSnippetString(string value) {
            // If the string is short, use C style quoting (e.g "\r\n")
            // Also do it if it is too long to fit in one line
            // If the string contains '\0', verbatim style won't work.
            if (value.Length < 256 || value.Length > 1500 || (value.IndexOf('\0') != -1))
                return QuoteSnippetStringCStyle(value);

            // Otherwise, use 'verbatim' style quoting (e.g. @"foo")
            return QuoteSnippetStringVerbatimStyle(value);
        }

        /// <devdoc>
        ///    <para>
        ///       Processes the <see cref='System.CodeDom.Compiler.CompilerResults'/> returned from compilation.
        ///    </para>
        /// </devdoc>
        private void ProcessCompilerOutputLine(CompilerResults results, string line) {
            if (outputRegSimple == null) {
                outputRegWithFileAndLine = 
                    new Regex(@"(^(.*)(\(([0-9]+),([0-9]+)\)): )(error|warning) ([A-Z]+[0-9]+) ?: (.*)");
                outputRegSimple =
                    new Regex(@"(error|warning) ([A-Z]+[0-9]+) ?: (.*)");
            }

            //First look for full file info
            Match m = outputRegWithFileAndLine.Match(line);
            bool full;
            if (m.Success) {
                full = true;
            }
            else {
                m = outputRegSimple.Match(line);
                full = false;
            }

            if (m.Success) {
                CompilerError ce = new CompilerError();
                if (full) {
                    ce.FileName = m.Groups[2].Value;
                    ce.Line = int.Parse(m.Groups[4].Value, CultureInfo.InvariantCulture);
                    ce.Column = int.Parse(m.Groups[5].Value, CultureInfo.InvariantCulture);
                }
                if (string.Compare(m.Groups[full ? 6 : 1].Value, "warning", StringComparison.OrdinalIgnoreCase) == 0) {
                    ce.IsWarning = true;
                }
                ce.ErrorNumber = m.Groups[full ? 7 : 2].Value;
                ce.ErrorText = m.Groups[full ? 8 : 3].Value;

                results.Errors.Add(ce);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the command arguments from the specified <see cref='System.CodeDom.Compiler.CompilerParameters'/>.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private string CmdArgsFromParameters(CompilerParameters options) {
            StringBuilder sb = new StringBuilder(128);
            if (options.GenerateExecutable) {
                sb.Append("/t:exe ");
                if (options.MainClass != null && options.MainClass.Length > 0) {
                    sb.Append("/main:");
                    sb.Append(options.MainClass);
                    sb.Append(" ");
                }
            }
            else {
                sb.Append("/t:library ");
            }

            // Get UTF8 output from the compiler
            sb.Append("/utf8output ");

            string coreAssemblyFileName = options.CoreAssemblyFileName;

            if (String.IsNullOrWhiteSpace(options.CoreAssemblyFileName)) {
                string probableCoreAssemblyFilePath;
                if(CodeDomProvider.TryGetProbableCoreAssemblyFilePath(options, out probableCoreAssemblyFilePath)) {
                    coreAssemblyFileName = probableCoreAssemblyFilePath;
                }
            }

            if (!String.IsNullOrWhiteSpace(coreAssemblyFileName)) {

                sb.Append("/nostdlib+ ");
                sb.Append("/R:\"").Append(coreAssemblyFileName.Trim()).Append("\" ");
            }

            foreach (string s in options.ReferencedAssemblies) {
                sb.Append("/R:");
                sb.Append("\"");
                sb.Append(s);
                sb.Append("\"");
                sb.Append(" ");
            }

            sb.Append("/out:");
            sb.Append("\"");
            sb.Append(options.OutputAssembly);
            sb.Append("\"");
            sb.Append(" ");

            if (options.IncludeDebugInformation) {
                sb.Append("/D:DEBUG ");
                sb.Append("/debug+ ");
                sb.Append("/optimize- ");
            }
            else {
                sb.Append("/debug- ");
                sb.Append("/optimize+ ");
            }

#if !FEATURE_PAL
            if (options.Win32Resource != null) {
                sb.Append("/win32res:\"" + options.Win32Resource + "\" ");
            }
#endif // !FEATURE_PAL

            foreach (string s in options.EmbeddedResources) {
                sb.Append("/res:\"");
                sb.Append(s);
                sb.Append("\" ");
            }

            foreach (string s in options.LinkedResources) {
                sb.Append("/linkres:\"");
                sb.Append(s);
                sb.Append("\" ");
            }

            if (options.TreatWarningsAsErrors) {
                sb.Append("/warnaserror ");
            }

            if (options.WarningLevel >= 0) {
                sb.Append("/w:" + options.WarningLevel + " ");
            }

            if (options.CompilerOptions != null) {
                sb.Append(options.CompilerOptions + " ");
            }

            return sb.ToString();
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        private void ContinueOnNewLine(string st) {
            Output.WriteLine(st);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private string GetResponseFileCmdArgs(CompilerParameters options, string cmdArgs) {

            string responseFileName = options.TempFiles.AddExtension("cmdline");

            Stream temp = new FileStream(responseFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
            try {
                using (StreamWriter sw = new StreamWriter(temp, Encoding.UTF8)) {
                    sw.Write(cmdArgs);
                    sw.Flush();
                }
            }
            finally {
                temp.Close();
            }

            // Always specify the /noconfig flag (outside of the response file)
            return "/noconfig /fullpaths @\"" + responseFileName + "\"";
        }

        private  void OutputIdentifier(string ident) {
            Output.Write(CreateEscapedIdentifier(ident));
        }

        /// <devdoc>
        ///    <para>
        ///       Sets the output type.
        ///    </para>
        /// </devdoc>
        private  void OutputType(CodeTypeReference typeRef) {
            Output.Write(GetTypeOutput(typeRef));
        }



        /// <devdoc>
        ///    <para>
        ///       Generates code for
        ///       the specified CodeDom based array creation expression representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateArrayCreateExpression(CodeArrayCreateExpression e) {
            Output.Write("new ");

            CodeExpressionCollection init = e.Initializers;
            if (init.Count > 0) {
                OutputType(e.CreateType);
                if (e.CreateType.ArrayRank == 0) {
                    // Unfortunately, many clients are already calling this without array
                    // types. This will allow new clients to correctly use the array type and
                    // not break existing clients. For VNext, stop doing this.
                    Output.Write("[]");
                }
                Output.WriteLine(" {");
                Indent++;
                OutputExpressionList(init, true /*newlineBetweenItems*/);
                Indent--;
                Output.Write("}");
            }
            else {
                Output.Write(GetBaseTypeOutput(e.CreateType));

                Output.Write("[");
                if (e.SizeExpression != null) {
                    GenerateExpression(e.SizeExpression);
                }
                else {
                    Output.Write(e.Size);
                }
                Output.Write("]");

                int nestedArrayDepth = e.CreateType.NestedArrayDepth;
                for (int i = 0; i < nestedArrayDepth - 1; i++) {
                    Output.Write("[]");
                }
            }
        }
        /// <devdoc>
        ///    <para>
        ///       Generates
        ///       code for the specified CodeDom based base reference expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateBaseReferenceExpression(CodeBaseReferenceExpression e) {
            Output.Write("base");
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based binary operator
        ///       expression representation.
        ///    </para>
        /// </devdoc>
        private void GenerateBinaryOperatorExpression(CodeBinaryOperatorExpression e) {
            bool indentedExpression = false;
            Output.Write("(");

            GenerateExpression(e.Left);
            Output.Write(" ");

            if (e.Left is CodeBinaryOperatorExpression || e.Right is CodeBinaryOperatorExpression) {
                // In case the line gets too long with nested binary operators, we need to output them on
                // different lines. However we want to indent them to maintain readability, but this needs
                // to be done only once;
                if (!inNestedBinary) {
                    indentedExpression = true;
                    inNestedBinary = true;
                    Indent += 3;
                }
                ContinueOnNewLine("");
            }
 
            OutputOperator(e.Operator);

            Output.Write(" ");
            GenerateExpression(e.Right);

            Output.Write(")");
            if (indentedExpression) {
                Indent -= 3;
                inNestedBinary = false;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based cast expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateCastExpression(CodeCastExpression e) {
            Output.Write("((");
            OutputType(e.TargetType);
            Output.Write(")(");
            GenerateExpression(e.Expression);
            Output.Write("))");
        }

        public void GenerateCodeFromMember(CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options) {
            if (this.output != null) {
                throw new InvalidOperationException(SR.GetString(SR.CodeGenReentrance));
            }
            this.options = (options == null) ? new CodeGeneratorOptions() : options;
            this.output = new IndentedTextWriter(writer, this.options.IndentString);

            try {
                CodeTypeDeclaration dummyClass = new CodeTypeDeclaration();
                this.currentClass = dummyClass;
                GenerateTypeMember(member, dummyClass);
            }
            finally {
                this.currentClass = null;
                this.output = null;
                this.options = null;
            }
        }
        
        private  void GenerateDefaultValueExpression(CodeDefaultValueExpression e) {
            Output.Write("default(");
            OutputType(e.Type);
            Output.Write(")");            
        }
        
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based delegate creation
        ///       expression representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateDelegateCreateExpression(CodeDelegateCreateExpression e) {
            Output.Write("new ");
            OutputType(e.DelegateType);
            Output.Write("(");
            GenerateExpression(e.TargetObject);
            Output.Write(".");
            OutputIdentifier(e.MethodName);
            Output.Write(")");
        }

        private void GenerateEvents(CodeTypeDeclaration e) {
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext()) {
                if (en.Current is CodeMemberEvent) {
                    currentMember = (CodeTypeMember)en.Current;

                    if (options.BlankLinesBetweenMembers) {
                        Output.WriteLine();
                    }
                    if (currentMember.StartDirectives.Count > 0) {
                        GenerateDirectives(currentMember.StartDirectives);
                    }
                    GenerateCommentStatements(currentMember.Comments);
                    CodeMemberEvent imp = (CodeMemberEvent)en.Current;
                    if (imp.LinePragma != null) GenerateLinePragmaStart(imp.LinePragma);
                    GenerateEvent(imp, e);
                    if (imp.LinePragma != null) GenerateLinePragmaEnd(imp.LinePragma);
                    if (currentMember.EndDirectives.Count > 0) {
                        GenerateDirectives(currentMember.EndDirectives);
                    }
                }
            }
        }


        private void GenerateFields(CodeTypeDeclaration e) {
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext()) {
                if (en.Current is CodeMemberField) {
                    currentMember = (CodeTypeMember)en.Current;

                    if (options.BlankLinesBetweenMembers) {
                        Output.WriteLine();
                    }
                    if (currentMember.StartDirectives.Count > 0) {
                        GenerateDirectives(currentMember.StartDirectives);
                    }
                    GenerateCommentStatements(currentMember.Comments);
                    CodeMemberField imp = (CodeMemberField)en.Current;
                    if (imp.LinePragma != null) GenerateLinePragmaStart(imp.LinePragma);
                    GenerateField(imp);
                    if (imp.LinePragma != null) GenerateLinePragmaEnd(imp.LinePragma);
                    if (currentMember.EndDirectives.Count > 0) {
                        GenerateDirectives(currentMember.EndDirectives);
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based field reference expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateFieldReferenceExpression(CodeFieldReferenceExpression e) {
            if (e.TargetObject != null) {
                GenerateExpression(e.TargetObject);
                Output.Write(".");
            }
            OutputIdentifier(e.FieldName);
        }

        private  void GenerateArgumentReferenceExpression(CodeArgumentReferenceExpression e) {
            OutputIdentifier(e.ParameterName);
        }

        private  void GenerateVariableReferenceExpression(CodeVariableReferenceExpression e) {
            OutputIdentifier(e.VariableName);
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based indexer expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateIndexerExpression(CodeIndexerExpression e) {
            GenerateExpression(e.TargetObject);
            Output.Write("[");
            bool first = true;
            foreach(CodeExpression exp in e.Indices) {            
                if (first) {
                    first = false;
                }
                else {
                    Output.Write(", ");
                }
                GenerateExpression(exp);
            }
            Output.Write("]");

        }

        private  void GenerateArrayIndexerExpression(CodeArrayIndexerExpression e) {
            GenerateExpression(e.TargetObject);
            Output.Write("[");
            bool first = true;
            foreach(CodeExpression exp in e.Indices) {            
                if (first) {
                    first = false;
                }
                else {
                    Output.Write(", ");
                }
                GenerateExpression(exp);
            }
            Output.Write("]");

        }

        /// <devdoc>
        ///    <para> Generates code for the specified snippet code block
        ///       </para>
        /// </devdoc>
        private void GenerateSnippetCompileUnit(CodeSnippetCompileUnit e) {
            
            GenerateDirectives(e.StartDirectives);

            if (e.LinePragma != null) GenerateLinePragmaStart(e.LinePragma);
            Output.WriteLine(e.Value);
            if (e.LinePragma != null) GenerateLinePragmaEnd(e.LinePragma);

            if (e.EndDirectives.Count > 0) {
                GenerateDirectives(e.EndDirectives);
            }            
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based snippet expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateSnippetExpression(CodeSnippetExpression e) {
            Output.Write(e.Value);
        }
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based method invoke expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateMethodInvokeExpression(CodeMethodInvokeExpression e) {
            GenerateMethodReferenceExpression(e.Method);
            Output.Write("(");
            OutputExpressionList(e.Parameters);
            Output.Write(")");
        }

        private  void GenerateMethodReferenceExpression(CodeMethodReferenceExpression e) {
            if (e.TargetObject != null) {
                if (e.TargetObject is CodeBinaryOperatorExpression) {
                    Output.Write("(");
                    GenerateExpression(e.TargetObject);
                    Output.Write(")");
                }
                else {
                    GenerateExpression(e.TargetObject);
                }
                Output.Write(".");
            }
            OutputIdentifier(e.MethodName);
            
            if( e.TypeArguments.Count > 0) {
                Output.Write(GetTypeArgumentsOutput(e.TypeArguments));
            }

        }

        private bool GetUserData(CodeObject e, string property, bool defaultValue) {
            object o = e.UserData[property];
            if (o != null && o is bool) {
                return (bool)o;
            }
            return defaultValue;
        }

        private  void GenerateNamespace(CodeNamespace e) {
            GenerateCommentStatements(e.Comments);
            GenerateNamespaceStart(e);

            if (GetUserData(e, "GenerateImports", true)) {
                GenerateNamespaceImports(e);
            }

            Output.WriteLine("");

            GenerateTypes(e);
            GenerateNamespaceEnd(e);
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for
        ///       the specified CodeDom based statement representation.
        ///    </para>
        /// </devdoc>
        private void GenerateStatement(CodeStatement e) {        
            if (e.StartDirectives.Count > 0) {
                GenerateDirectives(e.StartDirectives);
            }
        
            if (e.LinePragma != null) {
                GenerateLinePragmaStart(e.LinePragma);
            }

            if (e is CodeCommentStatement) {
                GenerateCommentStatement((CodeCommentStatement)e);
            }
            else if (e is CodeMethodReturnStatement) {
                GenerateMethodReturnStatement((CodeMethodReturnStatement)e);
            }
            else if (e is CodeConditionStatement) {
                GenerateConditionStatement((CodeConditionStatement)e);
            }
            else if (e is CodeTryCatchFinallyStatement) {
                GenerateTryCatchFinallyStatement((CodeTryCatchFinallyStatement)e);
            }
            else if (e is CodeAssignStatement) {
                GenerateAssignStatement((CodeAssignStatement)e);
            }
            else if (e is CodeExpressionStatement) {
                GenerateExpressionStatement((CodeExpressionStatement)e);
            }
            else if (e is CodeIterationStatement) {
                GenerateIterationStatement((CodeIterationStatement)e);
            }
            else if (e is CodeThrowExceptionStatement) {
                GenerateThrowExceptionStatement((CodeThrowExceptionStatement)e);
            }
            else if (e is CodeSnippetStatement) {
                // Don't indent snippet statements, in order to preserve the column
                // information from the original code.  This improves the debugging
                // experience.
                int savedIndent = Indent;
                Indent=0;

                GenerateSnippetStatement((CodeSnippetStatement)e);

                // Restore the indent
                Indent=savedIndent;
            }
            else if (e is CodeVariableDeclarationStatement) {
                GenerateVariableDeclarationStatement((CodeVariableDeclarationStatement)e);
            }
            else if (e is CodeAttachEventStatement) {
                GenerateAttachEventStatement((CodeAttachEventStatement)e);
            }
            else if (e is CodeRemoveEventStatement) {
                GenerateRemoveEventStatement((CodeRemoveEventStatement)e);
            }
            else if (e is CodeGotoStatement) {
                GenerateGotoStatement((CodeGotoStatement)e);
            }
            else if (e is CodeLabeledStatement) {
                GenerateLabeledStatement((CodeLabeledStatement)e);
            }
            else {
                throw new ArgumentException(SR.GetString(SR.InvalidElementType, e.GetType().FullName), "e");
            }

            if (e.LinePragma != null) {
                GenerateLinePragmaEnd(e.LinePragma);
            }
            if (e.EndDirectives.Count > 0) {
                GenerateDirectives(e.EndDirectives);
            }            
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based statement representations.
        ///    </para>
        /// </devdoc>
        private void GenerateStatements(CodeStatementCollection stms) {
            IEnumerator en = stms.GetEnumerator();
            while (en.MoveNext()) {
                ((ICodeGenerator)this).GenerateCodeFromStatement((CodeStatement)en.Current, output.InnerWriter, options);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based namespace import
        ///       representation.
        ///    </para>
        /// </devdoc>
        private void GenerateNamespaceImports(CodeNamespace e) {
            IEnumerator en = e.Imports.GetEnumerator();
            while (en.MoveNext()) {
                CodeNamespaceImport imp = (CodeNamespaceImport)en.Current;
                if (imp.LinePragma != null) GenerateLinePragmaStart(imp.LinePragma);
                GenerateNamespaceImport(imp);
                if (imp.LinePragma != null) GenerateLinePragmaEnd(imp.LinePragma);
            }
        }

        private  void GenerateEventReferenceExpression(CodeEventReferenceExpression e) {
            if (e.TargetObject != null) {
                GenerateExpression(e.TargetObject);
                Output.Write(".");
            }
            OutputIdentifier(e.EventName);
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based delegate invoke
        ///       expression representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateDelegateInvokeExpression(CodeDelegateInvokeExpression e) {
            if (e.TargetObject != null) {
                GenerateExpression(e.TargetObject);
            }
            Output.Write("(");
            OutputExpressionList(e.Parameters);
            Output.Write(")");
        }
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based object creation expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateObjectCreateExpression(CodeObjectCreateExpression e) {
            Output.Write("new ");
            OutputType(e.CreateType);
            Output.Write("(");
            OutputExpressionList(e.Parameters);
            Output.Write(")");
        }
        
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based primitive expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GeneratePrimitiveExpression(CodePrimitiveExpression e) {
            if (e.Value is char) {
                GeneratePrimitiveChar((char)e.Value);
            }
            else if (e.Value is SByte) {
                // C# has no literal marker for types smaller than Int32                
                Output.Write(((SByte)e.Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is UInt16) {
                // C# has no literal marker for types smaller than Int32, and you will
                // get a conversion error if you use "u" here.
                Output.Write(((UInt16)e.Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is UInt32) {
                Output.Write(((UInt32)e.Value).ToString(CultureInfo.InvariantCulture));
                Output.Write("u");
            }
            else if (e.Value is UInt64) {
                Output.Write(((UInt64)e.Value).ToString(CultureInfo.InvariantCulture));
                Output.Write("ul");
            }            
            else {
                GeneratePrimitiveExpressionBase(e);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based primitive expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        private void GeneratePrimitiveExpressionBase(CodePrimitiveExpression e) {
            if (e.Value == null) {
                Output.Write(NullToken);
            }
            else if (e.Value is string) {
                Output.Write(QuoteSnippetString((string)e.Value));
            }
            else if (e.Value is char) {
                Output.Write("'" + e.Value.ToString() + "'");
            }
            else if (e.Value is byte) {
                Output.Write(((byte)e.Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is Int16) {
                Output.Write(((Int16)e.Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is Int32) {
                Output.Write(((Int32)e.Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is Int64) {
                Output.Write(((Int64)e.Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is Single) {
                GenerateSingleFloatValue((Single)e.Value);
            }
            else if (e.Value is Double) {
                GenerateDoubleValue((Double)e.Value);
            }
            else if (e.Value is Decimal) {
                GenerateDecimalValue((Decimal)e.Value);
            }
            else if (e.Value is bool) {
                if ((bool)e.Value) {
                    Output.Write("true");
                }
                else {
                    Output.Write("false");
                }
            }
            else {
                throw new ArgumentException(SR.GetString(SR.InvalidPrimitiveType, e.Value.GetType().ToString()));
            }
        }

        private void GeneratePrimitiveChar(char c) {
            Output.Write('\'');
            switch (c) {
                case '\r':
                    Output.Write("\\r");
                    break;
                case '\t':
                    Output.Write("\\t");
                    break;
                case '\"':
                    Output.Write("\\\"");
                    break;
                case '\'':
                    Output.Write("\\\'");
                    break;
                case '\\':
                    Output.Write("\\\\");
                    break;
                case '\0':
                    Output.Write("\\0");
                    break;
                case '\n':
                    Output.Write("\\n");
                    break;
                case '\u2028':
                case '\u2029':
                case '\u0084':
                case '\u0085':               
                    AppendEscapedChar(null,c);
                    break;
                
                default:
                    if(Char.IsSurrogate(c)) {
                        AppendEscapedChar(null,c);
                    }
                    else {
                        Output.Write(c);
                    }
                    break;
            }
            Output.Write('\'');
         }

        private void AppendEscapedChar(StringBuilder b,char value) {
            if (b == null) {
                Output.Write("\\u");
                Output.Write(((int)value).ToString("X4", CultureInfo.InvariantCulture));
            } else {
                b.Append("\\u");
                b.Append(((int)value).ToString("X4", CultureInfo.InvariantCulture));
            }
        }
       
        private  void GeneratePropertySetValueReferenceExpression(CodePropertySetValueReferenceExpression e) {
            Output.Write("value");
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based this reference expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateThisReferenceExpression(CodeThisReferenceExpression e) {
            Output.Write("this");
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based method invoke statement
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateExpressionStatement(CodeExpressionStatement e) {
            GenerateExpression(e.Expression);
            if (!generatingForLoop) {
                Output.WriteLine(";");
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based for loop statement
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateIterationStatement(CodeIterationStatement e) {
            generatingForLoop = true;
            Output.Write("for (");
            GenerateStatement(e.InitStatement);
            Output.Write("; ");
            GenerateExpression(e.TestExpression);
            Output.Write("; ");
            GenerateStatement(e.IncrementStatement);
            Output.Write(")");
            OutputStartingBrace();
            generatingForLoop = false;
            Indent++;
            GenerateStatements(e.Statements);
            Indent--;
            Output.WriteLine("}");
        }
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based throw exception statement
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateThrowExceptionStatement(CodeThrowExceptionStatement e) {
            Output.Write("throw");
            if (e.ToThrow != null) {
                Output.Write(" ");
                GenerateExpression(e.ToThrow);
            }
            Output.WriteLine(";");
        }

        private  void GenerateComment(CodeComment e) {
            String commentLineStart = e.DocComment? "///": "//";
            Output.Write(commentLineStart);
            Output.Write(" ");

            string value = e.Text;
            for (int i=0; i<value.Length; i++) {
                if( value[i] == '\u0000') {
                    continue;
                }
                Output.Write(value[i]);

                if( value[i] == '\r') {
                    if (i < value.Length - 1 && value[i+1] == '\n') { // if next char is '\n', skip it
                        Output.Write('\n');
                        i++;
                    }
                    ((IndentedTextWriter)Output).InternalOutputTabs();
                    Output.Write(commentLineStart);
                }
                else if( value[i] == '\n') {
                    ((IndentedTextWriter)Output).InternalOutputTabs();
                    Output.Write(commentLineStart);
                }
                else if( value[i] == '\u2028' || value[i] == '\u2029' || value[i] == '\u0085') {
                    Output.Write(commentLineStart);
                }
            }
            Output.WriteLine();
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based comment statement
        ///       representation.
        ///    </para>
        /// </devdoc>
        private void GenerateCommentStatement(CodeCommentStatement e) {
            if(e.Comment == null)
                throw new ArgumentException(SR.GetString(SR.Argument_NullComment, "e"), "e");
            GenerateComment(e.Comment);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        private void GenerateCommentStatements(CodeCommentStatementCollection e) {
            foreach (CodeCommentStatement comment in e) {
                GenerateCommentStatement(comment);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based method return statement
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateMethodReturnStatement(CodeMethodReturnStatement e) {
            Output.Write("return");
            if (e.Expression != null) {
                Output.Write(" ");
                GenerateExpression(e.Expression);
            }
            Output.WriteLine(";");
        }
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based if statement
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateConditionStatement(CodeConditionStatement e) {
            Output.Write("if (");
            GenerateExpression(e.Condition);
            Output.Write(")");
            OutputStartingBrace();            
            Indent++;
            GenerateStatements(e.TrueStatements);
            Indent--;

            CodeStatementCollection falseStatemetns = e.FalseStatements;
            if (falseStatemetns.Count > 0) {
                Output.Write("}");
                if (Options.ElseOnClosing) {
                    Output.Write(" ");
                } 
                else {
                    Output.WriteLine("");
                }
                Output.Write("else");
                OutputStartingBrace();
                Indent++;
                GenerateStatements(e.FalseStatements);
                Indent--;
            }
            Output.WriteLine("}");
        }
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based try catch finally
        ///       statement representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateTryCatchFinallyStatement(CodeTryCatchFinallyStatement e) {
            Output.Write("try");
            OutputStartingBrace();
            Indent++;
            GenerateStatements(e.TryStatements);
            Indent--;
            CodeCatchClauseCollection catches = e.CatchClauses;
            if (catches.Count > 0) {
                IEnumerator en = catches.GetEnumerator();
                while (en.MoveNext()) {
                    Output.Write("}");
                    if (Options.ElseOnClosing) {
                        Output.Write(" ");
                    } 
                    else {
                        Output.WriteLine("");
                    }
                    CodeCatchClause current = (CodeCatchClause)en.Current;
                    Output.Write("catch (");
                    OutputType(current.CatchExceptionType);
                    Output.Write(" ");
                    OutputIdentifier(current.LocalName);
                    Output.Write(")");
                    OutputStartingBrace();
                    Indent++;
                    GenerateStatements(current.Statements);
                    Indent--;
                }
            }

            CodeStatementCollection finallyStatements = e.FinallyStatements;
            if (finallyStatements.Count > 0) {
                Output.Write("}");
                if (Options.ElseOnClosing) {
                    Output.Write(" ");
                } 
                else {
                    Output.WriteLine("");
                }
                Output.Write("finally");
                OutputStartingBrace();
                Indent++;
                GenerateStatements(finallyStatements);
                Indent--;
            }
            Output.WriteLine("}");
        }
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based assignment statement
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateAssignStatement(CodeAssignStatement e) {
            GenerateExpression(e.Left);
            Output.Write(" = ");
            GenerateExpression(e.Right);
            if (!generatingForLoop) {
                Output.WriteLine(";");
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based attach event statement
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateAttachEventStatement(CodeAttachEventStatement e) {
            GenerateEventReferenceExpression(e.Event);
            Output.Write(" += ");
            GenerateExpression(e.Listener);
            Output.WriteLine(";");
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based detach event statement
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateRemoveEventStatement(CodeRemoveEventStatement e) {
            GenerateEventReferenceExpression(e.Event);
            Output.Write(" -= ");
            GenerateExpression(e.Listener);
            Output.WriteLine(";");
        }

        private  void GenerateSnippetStatement(CodeSnippetStatement e) {
            Output.WriteLine(e.Value);
        }

        private  void GenerateGotoStatement(CodeGotoStatement e) {
            Output.Write("goto ");
            Output.Write(e.Label);
            Output.WriteLine(";");
        }

        private  void GenerateLabeledStatement(CodeLabeledStatement e) {
            Indent--;
            Output.Write(e.Label);
            Output.WriteLine(":");
            Indent++;
            if (e.Statement != null) {
                GenerateStatement(e.Statement);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based variable declaration
        ///       statement representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateVariableDeclarationStatement(CodeVariableDeclarationStatement e) {
            OutputTypeNamePair(e.Type, e.Name);
            if (e.InitExpression != null) {
                Output.Write(" = ");
                GenerateExpression(e.InitExpression);
            }
            if (!generatingForLoop) {
                Output.WriteLine(";");
            }
        }
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based line pragma start
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateLinePragmaStart(CodeLinePragma e) {
            Output.WriteLine("");
            Output.Write("#line ");
            Output.Write(e.LineNumber);
            Output.Write(" \"");
            Output.Write(e.FileName);
            Output.Write("\"");
            Output.WriteLine("");
        }
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based line pragma end
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateLinePragmaEnd(CodeLinePragma e) {
            Output.WriteLine();
            Output.WriteLine("#line default");
            Output.WriteLine("#line hidden");
        }

        private  void GenerateEvent(CodeMemberEvent e, CodeTypeDeclaration c) {
            if (IsCurrentDelegate || IsCurrentEnum) return;

            if (e.CustomAttributes.Count > 0) {
                GenerateAttributes(e.CustomAttributes);
            }

            if (e.PrivateImplementationType == null) {
                OutputMemberAccessModifier(e.Attributes);
            }
            Output.Write("event ");
            string name = e.Name;
            if (e.PrivateImplementationType != null) {
                name = GetBaseTypeOutput(e.PrivateImplementationType)+ "." + name;
            }
            OutputTypeNamePair(e.Type, name);
            Output.WriteLine(";");
        }

        /// <devdoc>
        ///    <para>Generates code for the specified CodeDom code expression representation.</para>
        /// </devdoc>
        private void GenerateExpression(CodeExpression e) {
            if (e is CodeArrayCreateExpression) {
                GenerateArrayCreateExpression((CodeArrayCreateExpression)e);
            }
            else if (e is CodeBaseReferenceExpression) {
                GenerateBaseReferenceExpression((CodeBaseReferenceExpression)e);
            }
            else if (e is CodeBinaryOperatorExpression) {
                GenerateBinaryOperatorExpression((CodeBinaryOperatorExpression)e);
            }
            else if (e is CodeCastExpression) {
                GenerateCastExpression((CodeCastExpression)e);
            }
            else if (e is CodeDelegateCreateExpression) {
                GenerateDelegateCreateExpression((CodeDelegateCreateExpression)e);
            }
            else if (e is CodeFieldReferenceExpression) {
                GenerateFieldReferenceExpression((CodeFieldReferenceExpression)e);
            }
            else if (e is CodeArgumentReferenceExpression) {
                GenerateArgumentReferenceExpression((CodeArgumentReferenceExpression)e);
            }
            else if (e is CodeVariableReferenceExpression) {
                GenerateVariableReferenceExpression((CodeVariableReferenceExpression)e);
            }
            else if (e is CodeIndexerExpression) {
                GenerateIndexerExpression((CodeIndexerExpression)e);
            }
            else if (e is CodeArrayIndexerExpression) {
                GenerateArrayIndexerExpression((CodeArrayIndexerExpression)e);
            }
            else if (e is CodeSnippetExpression) {
                GenerateSnippetExpression((CodeSnippetExpression)e);
            }
            else if (e is CodeMethodInvokeExpression) {
                GenerateMethodInvokeExpression((CodeMethodInvokeExpression)e);
            }
            else if (e is CodeMethodReferenceExpression) {
                GenerateMethodReferenceExpression((CodeMethodReferenceExpression)e);
            }
            else if (e is CodeEventReferenceExpression) {
                GenerateEventReferenceExpression((CodeEventReferenceExpression)e);
            }
            else if (e is CodeDelegateInvokeExpression) {
                GenerateDelegateInvokeExpression((CodeDelegateInvokeExpression)e);
            }
            else if (e is CodeObjectCreateExpression) {
                GenerateObjectCreateExpression((CodeObjectCreateExpression)e);
            }
            else if (e is CodeParameterDeclarationExpression) {
                GenerateParameterDeclarationExpression((CodeParameterDeclarationExpression)e);
            }
            else if (e is CodeDirectionExpression) {
                GenerateDirectionExpression((CodeDirectionExpression)e);
            }
            else if (e is CodePrimitiveExpression) {
                GeneratePrimitiveExpression((CodePrimitiveExpression)e);
            }
            else if (e is CodePropertyReferenceExpression) {
                GeneratePropertyReferenceExpression((CodePropertyReferenceExpression)e);
            }
            else if (e is CodePropertySetValueReferenceExpression) {
                GeneratePropertySetValueReferenceExpression((CodePropertySetValueReferenceExpression)e);
            }
            else if (e is CodeThisReferenceExpression) {
                GenerateThisReferenceExpression((CodeThisReferenceExpression)e);
            }
            else if (e is CodeTypeReferenceExpression) {
                GenerateTypeReferenceExpression((CodeTypeReferenceExpression)e);
            }
            else if (e is CodeTypeOfExpression) {
                GenerateTypeOfExpression((CodeTypeOfExpression)e);
            }
            else if (e is CodeDefaultValueExpression) {
                GenerateDefaultValueExpression((CodeDefaultValueExpression)e);
            }
            else {
                if (e == null) {
                    throw new ArgumentNullException("e");
                }
                else {
                    throw new ArgumentException(SR.GetString(SR.InvalidElementType, e.GetType().FullName), "e");
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom
        ///       based field representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateField(CodeMemberField e) {
            if (IsCurrentDelegate || IsCurrentInterface) return;

            if (IsCurrentEnum) {
                if (e.CustomAttributes.Count > 0) {
                    GenerateAttributes(e.CustomAttributes);
                }
                OutputIdentifier(e.Name);
                if (e.InitExpression != null) {
                    Output.Write(" = ");
                    GenerateExpression(e.InitExpression);
                }
                Output.WriteLine(",");
            }
            else {
                if (e.CustomAttributes.Count > 0) {
                    GenerateAttributes(e.CustomAttributes);
                }

                OutputMemberAccessModifier(e.Attributes);
                OutputVTableModifier(e.Attributes);
                OutputFieldScopeModifier(e.Attributes);
                OutputTypeNamePair(e.Type, e.Name);
                if (e.InitExpression != null) {
                    Output.Write(" = ");
                    GenerateExpression(e.InitExpression);
                }
                Output.WriteLine(";");
            }
        }
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based snippet class member
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateSnippetMember(CodeSnippetTypeMember e) {
            Output.Write(e.Text);
        }

        private  void GenerateParameterDeclarationExpression(CodeParameterDeclarationExpression e) {
            if (e.CustomAttributes.Count > 0) {
                // Parameter attributes should be in-line for readability
                GenerateAttributes(e.CustomAttributes, null, true);
            }

            OutputDirection(e.Direction);
            OutputTypeNamePair(e.Type, e.Name);
        }

        private  void GenerateEntryPointMethod(CodeEntryPointMethod e, CodeTypeDeclaration c) {

            if (e.CustomAttributes.Count > 0) {
                GenerateAttributes(e.CustomAttributes);
            }
            Output.Write("public static ");
            OutputType(e.ReturnType);
            Output.Write(" Main()");
            OutputStartingBrace();
            Indent++;

            GenerateStatements(e.Statements);

            Indent--;
            Output.WriteLine("}");
        }

        private void GenerateMethods(CodeTypeDeclaration e) {
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext()) {
                if (en.Current is CodeMemberMethod
                    && !(en.Current is CodeTypeConstructor)
                    && !(en.Current is CodeConstructor)) {
                    currentMember = (CodeTypeMember)en.Current;

                    if (options.BlankLinesBetweenMembers) {
                        Output.WriteLine();
                    }
                    if (currentMember.StartDirectives.Count > 0) {
                        GenerateDirectives(currentMember.StartDirectives);
                    }
                    GenerateCommentStatements(currentMember.Comments);
                    CodeMemberMethod imp = (CodeMemberMethod)en.Current;
                    if (imp.LinePragma != null) GenerateLinePragmaStart(imp.LinePragma);
                    if (en.Current is CodeEntryPointMethod) {
                        GenerateEntryPointMethod((CodeEntryPointMethod)en.Current, e);
                    } 
                    else {
                        GenerateMethod(imp, e);
                    }
                    if (imp.LinePragma != null) GenerateLinePragmaEnd(imp.LinePragma);
                    if (currentMember.EndDirectives.Count > 0) {
                        GenerateDirectives(currentMember.EndDirectives);
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based member method
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateMethod(CodeMemberMethod e, CodeTypeDeclaration c) {
            if (!(IsCurrentClass || IsCurrentStruct || IsCurrentInterface)) return;

            if (e.CustomAttributes.Count > 0) {
                GenerateAttributes(e.CustomAttributes);
            }
            if (e.ReturnTypeCustomAttributes.Count > 0) {
                GenerateAttributes(e.ReturnTypeCustomAttributes, "return: ");
            }

            if (!IsCurrentInterface) {
                if (e.PrivateImplementationType == null) {
                    OutputMemberAccessModifier(e.Attributes);
                    OutputVTableModifier(e.Attributes);
                    OutputMemberScopeModifier(e.Attributes);
                }
            }
            else {
                // interfaces still need "new"
                OutputVTableModifier(e.Attributes);
            }
            OutputType(e.ReturnType);
            Output.Write(" ");
            if (e.PrivateImplementationType != null) {
                Output.Write(GetBaseTypeOutput(e.PrivateImplementationType));
                Output.Write(".");
            }
            OutputIdentifier(e.Name);

            OutputTypeParameters(e.TypeParameters);

            Output.Write("(");
            OutputParameters(e.Parameters);
            Output.Write(")");
            
            OutputTypeParameterConstraints(e.TypeParameters);

            if (!IsCurrentInterface 
                && (e.Attributes & MemberAttributes.ScopeMask) != MemberAttributes.Abstract) {

                OutputStartingBrace();
                Indent++;

                GenerateStatements(e.Statements);

                Indent--;
                Output.WriteLine("}");
            }
            else {
                Output.WriteLine(";");
            }
        }
        
        private void GenerateProperties(CodeTypeDeclaration e) {
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext()) {
                if (en.Current is CodeMemberProperty) {
                    currentMember = (CodeTypeMember)en.Current;

                    if (options.BlankLinesBetweenMembers) {
                        Output.WriteLine();
                    }
                    if (currentMember.StartDirectives.Count > 0) {
                        GenerateDirectives(currentMember.StartDirectives);
                    }
                    GenerateCommentStatements(currentMember.Comments);
                    CodeMemberProperty imp = (CodeMemberProperty)en.Current;
                    if (imp.LinePragma != null) GenerateLinePragmaStart(imp.LinePragma);
                    GenerateProperty(imp, e);
                    if (imp.LinePragma != null) GenerateLinePragmaEnd(imp.LinePragma);
                    if (currentMember.EndDirectives.Count > 0) {
                        GenerateDirectives(currentMember.EndDirectives);
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based property representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateProperty(CodeMemberProperty e, CodeTypeDeclaration c) {
            if (!(IsCurrentClass || IsCurrentStruct || IsCurrentInterface)) return;

            if (e.CustomAttributes.Count > 0) {
                GenerateAttributes(e.CustomAttributes);
            }

            if (!IsCurrentInterface) {
                if (e.PrivateImplementationType == null) {
                    OutputMemberAccessModifier(e.Attributes);
                    OutputVTableModifier(e.Attributes);
                    OutputMemberScopeModifier(e.Attributes);
                }
            }
            else {
                OutputVTableModifier(e.Attributes);
            }
            OutputType(e.Type);
            Output.Write(" ");

            if (e.PrivateImplementationType != null && !IsCurrentInterface) {
                Output.Write(GetBaseTypeOutput(e.PrivateImplementationType));
                Output.Write(".");
            }

            if (e.Parameters.Count > 0 && String.Compare(e.Name, "Item", StringComparison.OrdinalIgnoreCase) == 0) {
                Output.Write("this[");
                OutputParameters(e.Parameters);
                Output.Write("]");
            }
            else {
                OutputIdentifier(e.Name);
            }

            OutputStartingBrace();
            Indent++;

            if (e.HasGet) {
                if (IsCurrentInterface || (e.Attributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract) {
                    Output.WriteLine("get;");
                }
                else {
                    Output.Write("get");
                    OutputStartingBrace();
                    Indent++;
                    GenerateStatements(e.GetStatements);
                    Indent--;
                    Output.WriteLine("}");
                }
            }
            if (e.HasSet) {
                if (IsCurrentInterface || (e.Attributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract) {
                    Output.WriteLine("set;");
                }
                else {
                    Output.Write("set");
                    OutputStartingBrace();
                    Indent++;
                    GenerateStatements(e.SetStatements);
                    Indent--;
                    Output.WriteLine("}");
                }
            }

            Indent--;
            Output.WriteLine("}");
        }
        
        private  void GenerateSingleFloatValue(Single s) {
            if( float.IsNaN(s)) {
                Output.Write("float.NaN");
            }
            else if( float.IsNegativeInfinity(s)) {
                Output.Write("float.NegativeInfinity");                
            }
            else if( float.IsPositiveInfinity(s)) {
                Output.Write("float.PositiveInfinity");                
            }
            else {
                Output.Write(s.ToString(CultureInfo.InvariantCulture));
                Output.Write('F');
            }
        }

        private  void GenerateDoubleValue(double d) {
            if( double.IsNaN(d)) {
                Output.Write("double.NaN");
            }
            else if( double.IsNegativeInfinity(d)) {
                Output.Write("double.NegativeInfinity");                
            }
            else if( double.IsPositiveInfinity(d)) {
                Output.Write("double.PositiveInfinity");                
            }
            else {
                Output.Write(d.ToString("R", CultureInfo.InvariantCulture));
                // always mark a double as being a double in case we have no decimal portion (e.g write 1D instead of 1 which is an int)
                Output.Write("D");
            }
        }

        private  void GenerateDecimalValue(Decimal d) {
            Output.Write(d.ToString(CultureInfo.InvariantCulture));
            Output.Write('m');
        }

        private void OutputVTableModifier(MemberAttributes attributes) {
            switch (attributes & MemberAttributes.VTableMask) {
                case MemberAttributes.New:
                    Output.Write("new ");
                    break;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified member access modifier.
        ///    </para>
        /// </devdoc>
        private void OutputMemberAccessModifier(MemberAttributes attributes) {
            switch (attributes & MemberAttributes.AccessMask) {
                case MemberAttributes.Assembly:
                    Output.Write("internal ");
                    break;
                case MemberAttributes.FamilyAndAssembly:
                    Output.Write("internal ");  /*FamANDAssem*/ 
                    break;
                case MemberAttributes.Family:
                    Output.Write("protected ");
                    break;
                case MemberAttributes.FamilyOrAssembly:
                    Output.Write("protected internal ");
                    break;
                case MemberAttributes.Private:
                    Output.Write("private ");
                    break;
                case MemberAttributes.Public:
                    Output.Write("public ");
                    break;
            }
        }

        private  void OutputMemberScopeModifier(MemberAttributes attributes) {
            switch (attributes & MemberAttributes.ScopeMask) {
                case MemberAttributes.Abstract:
                    Output.Write("abstract ");
                    break;
                case MemberAttributes.Final:
                    Output.Write("");
                    break;
                case MemberAttributes.Static:
                    Output.Write("static ");
                    break;
                case MemberAttributes.Override:
                    Output.Write("override ");
                    break;
                default:
                    switch (attributes & MemberAttributes.AccessMask) {
                        case MemberAttributes.Family:
                        case MemberAttributes.Public:
                        case MemberAttributes.Assembly:
                            Output.Write("virtual ");
                            break;
                        default:
                            // nothing;
                            break;
                    }
                    break;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified operator.
        ///    </para>
        /// </devdoc>
        private void OutputOperator(CodeBinaryOperatorType op) {
            switch (op) {
                case CodeBinaryOperatorType.Add:
                    Output.Write("+");
                    break;
                case CodeBinaryOperatorType.Subtract:
                    Output.Write("-");
                    break;
                case CodeBinaryOperatorType.Multiply:
                    Output.Write("*");
                    break;
                case CodeBinaryOperatorType.Divide:
                    Output.Write("/");
                    break;
                case CodeBinaryOperatorType.Modulus:
                    Output.Write("%");
                    break;
                case CodeBinaryOperatorType.Assign:
                    Output.Write("=");
                    break;
                case CodeBinaryOperatorType.IdentityInequality:
                    Output.Write("!=");
                    break;
                case CodeBinaryOperatorType.IdentityEquality:
                    Output.Write("==");
                    break;
                case CodeBinaryOperatorType.ValueEquality:
                    Output.Write("==");
                    break;
                case CodeBinaryOperatorType.BitwiseOr:
                    Output.Write("|");
                    break;
                case CodeBinaryOperatorType.BitwiseAnd:
                    Output.Write("&");
                    break;
                case CodeBinaryOperatorType.BooleanOr:
                    Output.Write("||");
                    break;
                case CodeBinaryOperatorType.BooleanAnd:
                    Output.Write("&&");
                    break;
                case CodeBinaryOperatorType.LessThan:
                    Output.Write("<");
                    break;
                case CodeBinaryOperatorType.LessThanOrEqual:
                    Output.Write("<=");
                    break;
                case CodeBinaryOperatorType.GreaterThan:
                    Output.Write(">");
                    break;
                case CodeBinaryOperatorType.GreaterThanOrEqual:
                    Output.Write(">=");
                    break;
            }
        }

        private  void OutputFieldScopeModifier(MemberAttributes attributes) {
            switch (attributes & MemberAttributes.ScopeMask) {
                case MemberAttributes.Final:
                    break;
                case MemberAttributes.Static:
                    Output.Write("static ");
                    break;
                case MemberAttributes.Const:
                    Output.Write("const ");
                    break;
                default:
                    break;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based property reference
        ///       expression representation.
        ///    </para>
        /// </devdoc>
        private  void GeneratePropertyReferenceExpression(CodePropertyReferenceExpression e) {

            if (e.TargetObject != null) {
                GenerateExpression(e.TargetObject);
                Output.Write(".");
            }
            OutputIdentifier(e.PropertyName);
        }

        private void GenerateConstructors(CodeTypeDeclaration e) {
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext()) {
                if (en.Current is CodeConstructor) {
                    currentMember = (CodeTypeMember)en.Current;

                    if (options.BlankLinesBetweenMembers) {
                        Output.WriteLine();
                    }
                    if (currentMember.StartDirectives.Count > 0) {
                        GenerateDirectives(currentMember.StartDirectives);
                    }
                    GenerateCommentStatements(currentMember.Comments);
                    CodeConstructor imp = (CodeConstructor)en.Current;
                    if (imp.LinePragma != null) GenerateLinePragmaStart(imp.LinePragma);
                    GenerateConstructor(imp, e);
                    if (imp.LinePragma != null) GenerateLinePragmaEnd(imp.LinePragma);
                    if (currentMember.EndDirectives.Count > 0) {
                        GenerateDirectives(currentMember.EndDirectives);
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based constructor
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateConstructor(CodeConstructor e, CodeTypeDeclaration c) {
            if (!(IsCurrentClass || IsCurrentStruct)) return;

            if (e.CustomAttributes.Count > 0) {
                GenerateAttributes(e.CustomAttributes);
            }

            OutputMemberAccessModifier(e.Attributes);
            OutputIdentifier(CurrentTypeName);
            Output.Write("(");
            OutputParameters(e.Parameters);
            Output.Write(")");

            CodeExpressionCollection baseArgs = e.BaseConstructorArgs;
            CodeExpressionCollection thisArgs = e.ChainedConstructorArgs;

            if (baseArgs.Count > 0) {
                Output.WriteLine(" : ");
                Indent++;
                Indent++;
                Output.Write("base(");
                OutputExpressionList(baseArgs);
                Output.Write(")");
                Indent--;
                Indent--;
            }

            if (thisArgs.Count > 0) {
                Output.WriteLine(" : ");
                Indent++;
                Indent++;
                Output.Write("this(");
                OutputExpressionList(thisArgs);
                Output.Write(")");
                Indent--;
                Indent--;
            }

            OutputStartingBrace();
            Indent++;
            GenerateStatements(e.Statements);
            Indent--;
            Output.WriteLine("}");
        }
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based class constructor
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateTypeConstructor(CodeTypeConstructor e) {
            if (!(IsCurrentClass || IsCurrentStruct)) return;

            if (e.CustomAttributes.Count > 0) {
                GenerateAttributes(e.CustomAttributes);
            }
            Output.Write("static ");
            Output.Write(CurrentTypeName);
            Output.Write("()");
            OutputStartingBrace();
            Indent++;
            GenerateStatements(e.Statements);
            Indent--;
            Output.WriteLine("}");
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based type reference expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        private void GenerateTypeReferenceExpression(CodeTypeReferenceExpression e) {
            OutputType(e.Type);
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based type of expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        private void GenerateTypeOfExpression(CodeTypeOfExpression e) {
            Output.Write("typeof(");
            OutputType(e.Type);
            Output.Write(")");
        }

        private void GenerateType(CodeTypeDeclaration e) {
            currentClass = e;

            if (e.StartDirectives.Count > 0) {
                GenerateDirectives(e.StartDirectives);
            }

            GenerateCommentStatements(e.Comments);
            
            if (e.LinePragma != null) GenerateLinePragmaStart(e.LinePragma);

            GenerateTypeStart(e);
            
            if (Options.VerbatimOrder) {
                foreach (CodeTypeMember member in e.Members) {
                    GenerateTypeMember(member, e);
                }                
            }
            else {

                GenerateFields(e);

                GenerateSnippetMembers(e);

                GenerateTypeConstructors(e);

                GenerateConstructors(e);

                GenerateProperties(e);

                GenerateEvents(e);

                GenerateMethods(e);

                GenerateNestedTypes(e);
            }
            // Nested types clobber the current class, so reset it.
            currentClass = e;

            GenerateTypeEnd(e);
            if (e.LinePragma != null) GenerateLinePragmaEnd(e.LinePragma);
            
            if (e.EndDirectives.Count > 0) {
                GenerateDirectives(e.EndDirectives);
            }
            
        }
        
        /// <devdoc>
        ///    <para> Generates code for the specified CodeDom namespace representation and the classes it
        ///       contains.</para>
        /// </devdoc>
        private void GenerateTypes(CodeNamespace e) {
            foreach (CodeTypeDeclaration c in e.Types) {
                if (options.BlankLinesBetweenMembers) {
                            Output.WriteLine();
                }
                ((ICodeGenerator)this).GenerateCodeFromType(c, output.InnerWriter, options);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based class start
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateTypeStart(CodeTypeDeclaration e) {
            if (e.CustomAttributes.Count > 0) {
                GenerateAttributes(e.CustomAttributes);
            }

            if (IsCurrentDelegate) {
                switch (e.TypeAttributes & TypeAttributes.VisibilityMask) {
                    case TypeAttributes.Public:
                        Output.Write("public ");
                        break;
                    case TypeAttributes.NotPublic:
                    default:
                        break;
                }

                CodeTypeDelegate del = (CodeTypeDelegate)e;
                Output.Write("delegate ");
                OutputType(del.ReturnType);
                Output.Write(" ");
                OutputIdentifier(e.Name);
                Output.Write("(");
                OutputParameters(del.Parameters);
                Output.WriteLine(");");
            } else {
                OutputTypeAttributes(e);                                
                OutputIdentifier(e.Name);           

                OutputTypeParameters(e.TypeParameters);

                bool first = true;
                foreach (CodeTypeReference typeRef in e.BaseTypes) {
                    if (first) {
                        Output.Write(" : ");
                        first = false;
                    }
                    else {
                        Output.Write(", ");
                    }                 
                    OutputType(typeRef);
                }

                OutputTypeParameterConstraints(e.TypeParameters);

                OutputStartingBrace();
                Indent++;                
            }
        }

        private void GenerateTypeMember(CodeTypeMember member, CodeTypeDeclaration declaredType) {

            if (options.BlankLinesBetweenMembers) {
                Output.WriteLine();
            }
            
            if (member is CodeTypeDeclaration) {
                ((ICodeGenerator)this).GenerateCodeFromType((CodeTypeDeclaration)member, output.InnerWriter, options);
                
                // Nested types clobber the current class, so reset it.
                currentClass = declaredType;
                
                // For nested types, comments and line pragmas are handled separately, so return here
                return;
            }
            
            if (member.StartDirectives.Count > 0) {
                GenerateDirectives(member.StartDirectives);
            }
                       
            GenerateCommentStatements(member.Comments);
            
            if (member.LinePragma != null) {
                GenerateLinePragmaStart(member.LinePragma);
            }
            
            if (member is CodeMemberField) {
                GenerateField((CodeMemberField)member);
            }
            else if (member is CodeMemberProperty) {
                GenerateProperty((CodeMemberProperty)member, declaredType);
            }
            else if (member is CodeMemberMethod) {
                if (member is CodeConstructor) {
                    GenerateConstructor((CodeConstructor)member, declaredType);
                }
                else if (member is CodeTypeConstructor) {
                    GenerateTypeConstructor((CodeTypeConstructor) member);
                }
                else if (member is CodeEntryPointMethod) {
                    GenerateEntryPointMethod((CodeEntryPointMethod)member, declaredType);
                } 
                else {
                    GenerateMethod((CodeMemberMethod)member, declaredType);
                }
            }            
            else if (member is CodeMemberEvent) {
                GenerateEvent((CodeMemberEvent)member, declaredType);
            }
            else if (member is CodeSnippetTypeMember) {

                // Don't indent snippets, in order to preserve the column
                // information from the original code.  This improves the debugging
                // experience.
                int savedIndent = Indent;
                Indent=0;

                GenerateSnippetMember((CodeSnippetTypeMember)member);

                // Restore the indent
                Indent=savedIndent;
                
                // Generate an extra new line at the end of the snippet.
                // If the snippet is comment and this type only contains comments.
                // The generated code will not compile. 
                Output.WriteLine();
            }

            if (member.LinePragma != null) {
                GenerateLinePragmaEnd(member.LinePragma);
            }

            if (member.EndDirectives.Count > 0) {
                GenerateDirectives(member.EndDirectives);
            }
        }

        private void GenerateTypeConstructors(CodeTypeDeclaration e) {
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext()) {
                if (en.Current is CodeTypeConstructor) {
                    currentMember = (CodeTypeMember)en.Current;

                    if (options.BlankLinesBetweenMembers) {
                        Output.WriteLine();
                    }
                    if (currentMember.StartDirectives.Count > 0) {
                        GenerateDirectives(currentMember.StartDirectives);
                    }
                    GenerateCommentStatements(currentMember.Comments);
                    CodeTypeConstructor imp = (CodeTypeConstructor)en.Current;
                    if (imp.LinePragma != null) GenerateLinePragmaStart(imp.LinePragma);
                    GenerateTypeConstructor(imp);
                    if (imp.LinePragma != null) GenerateLinePragmaEnd(imp.LinePragma);
                    if (currentMember.EndDirectives.Count > 0) {
                        GenerateDirectives(currentMember.EndDirectives);
                    }
                }
            }
        }

        private void GenerateSnippetMembers(CodeTypeDeclaration e) {
            IEnumerator en = e.Members.GetEnumerator();
            bool hasSnippet = false;
            while (en.MoveNext()) {
                if (en.Current is CodeSnippetTypeMember) {
                    hasSnippet = true;
                    currentMember = (CodeTypeMember)en.Current;

                    if (options.BlankLinesBetweenMembers) {
                        Output.WriteLine();
                    }
                    if (currentMember.StartDirectives.Count > 0) {
                        GenerateDirectives(currentMember.StartDirectives);
                    }
                    GenerateCommentStatements(currentMember.Comments);
                    CodeSnippetTypeMember imp = (CodeSnippetTypeMember)en.Current;
                    if (imp.LinePragma != null) GenerateLinePragmaStart(imp.LinePragma);

                    // Don't indent snippets, in order to preserve the column
                    // information from the original code.  This improves the debugging
                    // experience.
                    int savedIndent = Indent;
                    Indent=0;

                    GenerateSnippetMember(imp);

                    // Restore the indent
                    Indent=savedIndent;

                    if (imp.LinePragma != null) GenerateLinePragmaEnd(imp.LinePragma);
                    if (currentMember.EndDirectives.Count > 0) {
                        GenerateDirectives(currentMember.EndDirectives);
                    }

                }
            }
            // Generate an extra new line at the end of the snippet.
            // If the snippet is comment and this type only contains comments.
            // The generated code will not compile. 
            if(hasSnippet) {
                Output.WriteLine();
            }
        }

        private void GenerateNestedTypes(CodeTypeDeclaration e) {
            IEnumerator en = e.Members.GetEnumerator();
            while (en.MoveNext()) {
                if (en.Current is CodeTypeDeclaration) {
                    if (options.BlankLinesBetweenMembers) {
                        Output.WriteLine();
                    }
                    CodeTypeDeclaration currentClass = (CodeTypeDeclaration)en.Current;
                    ((ICodeGenerator)this).GenerateCodeFromType(currentClass, output.InnerWriter, options);
                }
            }
        }

        /// <devdoc>
        ///    <para> Generates code for the namepsaces in the specifield CodeDom compile unit.
        ///     </para>
        /// </devdoc>
        private void GenerateNamespaces(CodeCompileUnit e) {
            foreach (CodeNamespace n in e.Namespaces) {
                ((ICodeGenerator)this).GenerateCodeFromNamespace(n, output.InnerWriter, options);
            }
        }



        /// <devdoc>
        ///    <para>
        ///       Outputs an argument in a attribute block.
        ///    </para>
        /// </devdoc>
        private void OutputAttributeArgument(CodeAttributeArgument arg) {
            if (arg.Name != null && arg.Name.Length > 0) {
                OutputIdentifier(arg.Name);
                Output.Write("=");
            }
            ((ICodeGenerator)this).GenerateCodeFromExpression(arg.Value, output.InnerWriter, options);
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified System.CodeDom.FieldDirection.
        ///    </para>
        /// </devdoc>
        private void OutputDirection(FieldDirection dir) {
            switch (dir) {
                case FieldDirection.In:
                    break;
                case FieldDirection.Out:
                    Output.Write("out ");
                    break;
                case FieldDirection.Ref:
                    Output.Write("ref ");
                    break;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified expression list.
        ///    </para>
        /// </devdoc>
        private void OutputExpressionList(CodeExpressionCollection expressions) {
            OutputExpressionList(expressions, false /*newlineBetweenItems*/);
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified expression list.
        ///    </para>
        /// </devdoc>
        private void OutputExpressionList(CodeExpressionCollection expressions, bool newlineBetweenItems) {
            bool first = true;
            IEnumerator en = expressions.GetEnumerator();
            Indent++;
            while (en.MoveNext()) {
                if (first) {
                    first = false;
                }
                else {
                    if (newlineBetweenItems)
                        ContinueOnNewLine(",");
                    else
                        Output.Write(", ");
                }
                ((ICodeGenerator)this).GenerateCodeFromExpression((CodeExpression)en.Current, output.InnerWriter, options);
            }
            Indent--;
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified parameters.
        ///    </para>
        /// </devdoc>
        private void OutputParameters(CodeParameterDeclarationExpressionCollection parameters) {
            bool first = true;
            bool multiline = parameters.Count > ParameterMultilineThreshold;
            if (multiline) {
                Indent += 3;
            }
            IEnumerator en = parameters.GetEnumerator();
            while (en.MoveNext()) {
                CodeParameterDeclarationExpression current = (CodeParameterDeclarationExpression)en.Current;
                if (first) {
                    first = false;
                }
                else {
                    Output.Write(", ");
                }
                if (multiline) {
                    ContinueOnNewLine("");
                }
                GenerateExpression(current);
            }
            if (multiline) {
                Indent -= 3;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified object type and name pair.
        ///    </para>
        /// </devdoc>
        private void OutputTypeNamePair(CodeTypeReference typeRef, string name) {
            OutputType(typeRef);
            Output.Write(" ");
            OutputIdentifier(name);
        }

        private void OutputTypeParameters(CodeTypeParameterCollection typeParameters) {            
            if( typeParameters.Count == 0) {
                return;
            }

            Output.Write('<');
            bool first = true;
            for(int i = 0; i < typeParameters.Count; i++) {
                if( first) {
                    first = false;
                }
                else {
                    Output.Write(", ");
                }

                if (typeParameters[i].CustomAttributes.Count > 0) {
                    GenerateAttributes(typeParameters[i].CustomAttributes, null, true);
                    Output.Write(' ');
                }

                Output.Write(typeParameters[i].Name);
            }

            Output.Write('>');
        }

        private void OutputTypeParameterConstraints(CodeTypeParameterCollection typeParameters) {            
            if( typeParameters.Count == 0) {
                return;
            }

            for(int i = 0; i < typeParameters.Count; i++) {
                // generating something like: "where KeyType: IComparable, IEnumerable"

                Output.WriteLine();
                Indent++;

                bool first = true;
                if( typeParameters[i].Constraints.Count > 0) {
                    foreach (CodeTypeReference typeRef in typeParameters[i].Constraints) {
                        if (first) {
                            Output.Write("where ");
                            Output.Write(typeParameters[i].Name);                                
                            Output.Write(" : ");
                            first = false;
                        }
                        else {
                            Output.Write(", ");
                        }                 
                        OutputType(typeRef);
                    }
                }
                
                if( typeParameters[i].HasConstructorConstraint) {
                    if( first) {
                        Output.Write("where ");
                        Output.Write(typeParameters[i].Name);                                
                        Output.Write(" : new()");
                    }
                    else {
                        Output.Write(", new ()");                    
                    }
                }

                Indent--;
            }
        }


        private void OutputTypeAttributes(CodeTypeDeclaration e) {            
            if((e.Attributes & MemberAttributes.New) != 0) {
                Output.Write("new ");
            }
            
            TypeAttributes attributes = e.TypeAttributes;
            switch(attributes & TypeAttributes.VisibilityMask) {
                case TypeAttributes.Public:                  
                case TypeAttributes.NestedPublic:
                    Output.Write("public ");
                    break;
                case TypeAttributes.NestedPrivate:
                    Output.Write("private ");
                    break;
                case TypeAttributes.NestedFamily:
                    Output.Write("protected ");
                    break;
                case TypeAttributes.NotPublic:
                case TypeAttributes.NestedAssembly:
                case TypeAttributes.NestedFamANDAssem:
                    Output.Write("internal ");
                    break;
                case TypeAttributes.NestedFamORAssem:
                    Output.Write("protected internal ");
                    break;
            }
            
            if (e.IsStruct) {
                if (e.IsPartial) {
                    Output.Write("partial ");
                }                
                Output.Write("struct ");
            }
            else if (e.IsEnum) {
                Output.Write("enum ");
            }     
            else {            
                switch (attributes & TypeAttributes.ClassSemanticsMask) {
                    case TypeAttributes.Class:
                        if ((attributes & TypeAttributes.Sealed) == TypeAttributes.Sealed) {
                            Output.Write("sealed ");
                        }
                        if ((attributes & TypeAttributes.Abstract) == TypeAttributes.Abstract)  {
                            Output.Write("abstract ");
                        }
                        if (e.IsPartial) {
                            Output.Write("partial ");
                        }
                                        
                        Output.Write("class ");

                        break;                
                    case TypeAttributes.Interface:
                        if (e.IsPartial) {
                            Output.Write("partial ");
                        }
                        Output.Write("interface ");
                        break;
                }     
            }   
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based class end representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateTypeEnd(CodeTypeDeclaration e) {
            if (!IsCurrentDelegate) {
                Indent--;
                Output.WriteLine("}");
            }
        }
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based namespace start
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateNamespaceStart(CodeNamespace e) {

            if (e.Name != null && e.Name.Length > 0) {
                Output.Write("namespace ");
                string[] names = e.Name.Split('.');
                Debug.Assert( names.Length > 0);
                OutputIdentifier(names[0]);
                for( int i = 1; i< names.Length; i++) {
                    Output.Write(".");
                    OutputIdentifier(names[i]);                    
                }
                OutputStartingBrace();
                Indent++;
            }
        }
        
        /// <devdoc>
        ///    <para> Generates code for the specified CodeDom
        ///       compile unit representation.</para>
        /// </devdoc>
        private void GenerateCompileUnit(CodeCompileUnit e) {
            GenerateCompileUnitStart(e);
            GenerateNamespaces(e);
            GenerateCompileUnitEnd(e);
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based compile unit start
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateCompileUnitStart(CodeCompileUnit e) {
        
            if (e.StartDirectives.Count > 0) {
                GenerateDirectives(e.StartDirectives);
            }
        
            Output.WriteLine("//------------------------------------------------------------------------------");
            Output.Write("// <");
            Output.WriteLine(SR.GetString(SR.AutoGen_Comment_Line1));
            Output.Write("//     ");
            Output.WriteLine(SR.GetString(SR.AutoGen_Comment_Line2));
            Output.Write("//     ");
            Output.Write(SR.GetString(SR.AutoGen_Comment_Line3));
            Output.WriteLine(System.Environment.Version.ToString());
            Output.WriteLine("//");
            Output.Write("//     ");
            Output.WriteLine(SR.GetString(SR.AutoGen_Comment_Line4));
            Output.Write("//     ");
            Output.WriteLine(SR.GetString(SR.AutoGen_Comment_Line5));
            Output.Write("// </");
            Output.WriteLine(SR.GetString(SR.AutoGen_Comment_Line1));            
            Output.WriteLine("//------------------------------------------------------------------------------");
            Output.WriteLine("");

            SortedList importList;            
            // CSharp needs to put assembly attributes after using statements.
            // Since we need to create a empty namespace even if we don't need it,
            // using will generated after assembly attributes.
            importList = new SortedList(StringComparer.Ordinal);
            foreach (CodeNamespace nspace in e.Namespaces) {
                if( String.IsNullOrEmpty(nspace.Name)) {
                    // mark the namespace to stop it generating its own import list
                    nspace.UserData["GenerateImports"] = false;

                    // Collect the unique list of imports
                    foreach (CodeNamespaceImport import in nspace.Imports) {
                        if (!importList.Contains(import.Namespace)) {
                            importList.Add(import.Namespace, import.Namespace);
                        }
                    }
                }
            }

            // now output the imports
            foreach(string import in importList.Keys) {
                Output.Write("using ");
                OutputIdentifier(import);
                Output.WriteLine(";");
            }
            if( importList.Keys.Count > 0) {
                Output.WriteLine("");
            }

            // in C# the best place to put these is at the top level.
            if (e.AssemblyCustomAttributes.Count > 0) {
                GenerateAttributes(e.AssemblyCustomAttributes, "assembly: ");
                Output.WriteLine("");
            }
            
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based compile unit end
        ///       representation.
        ///    </para>
        /// </devdoc>
        private void GenerateCompileUnitEnd(CodeCompileUnit e) {
            if (e.EndDirectives.Count > 0) {
                GenerateDirectives(e.EndDirectives);
            }
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        private void GenerateDirectionExpression(CodeDirectionExpression e) {
            OutputDirection(e.Direction);
            GenerateExpression(e.Expression);
        }
        
        private  void GenerateDirectives(CodeDirectiveCollection directives) {
            for (int i = 0; i < directives.Count; i++) {
                CodeDirective directive = directives[i];
                if (directive is CodeChecksumPragma) {
                    GenerateChecksumPragma((CodeChecksumPragma)directive);
                }
                else if (directive is CodeRegionDirective) {
                    GenerateCodeRegionDirective((CodeRegionDirective)directive);
                }
            }
        }
        
        private void GenerateChecksumPragma(CodeChecksumPragma checksumPragma) {
            Output.Write("#pragma checksum \"");
            Output.Write(checksumPragma.FileName);
            Output.Write("\" \"");
            Output.Write(checksumPragma.ChecksumAlgorithmId.ToString("B", CultureInfo.InvariantCulture));
            Output.Write("\" \"");
            if (checksumPragma.ChecksumData != null) {
                foreach(Byte b in checksumPragma.ChecksumData) {
                    Output.Write(b.ToString("X2", CultureInfo.InvariantCulture));
                }
            }
            Output.WriteLine("\"");            
        }
        
        private void GenerateCodeRegionDirective(CodeRegionDirective regionDirective) {
            if (regionDirective.RegionMode == CodeRegionMode.Start) {
                Output.Write("#region ");
                Output.WriteLine(regionDirective.RegionText);
            }
            else if (regionDirective.RegionMode == CodeRegionMode.End) {
                Output.WriteLine("#endregion");
            }
        }
                
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based namespace end
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateNamespaceEnd(CodeNamespace e) {
            if (e.Name != null && e.Name.Length > 0) {
                Indent--;
                Output.WriteLine("}");
            }
        }
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based namespace import
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateNamespaceImport(CodeNamespaceImport e) {
            Output.Write("using ");
            OutputIdentifier(e.Namespace);
            Output.WriteLine(";");
        }
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based attribute block start
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateAttributeDeclarationsStart(CodeAttributeDeclarationCollection attributes) {
            Output.Write("[");
        }
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based attribute block end
        ///       representation.
        ///    </para>
        /// </devdoc>
        private  void GenerateAttributeDeclarationsEnd(CodeAttributeDeclarationCollection attributes) {
            Output.Write("]");
        }

        private void GenerateAttributes(CodeAttributeDeclarationCollection attributes) {
            GenerateAttributes(attributes, null, false);
        }

        private void GenerateAttributes(CodeAttributeDeclarationCollection attributes, string prefix) {
            GenerateAttributes(attributes, prefix, false);
        }
    
        private void GenerateAttributes(CodeAttributeDeclarationCollection attributes, string prefix, bool inLine) {
            if (attributes.Count == 0) return;
            IEnumerator en = attributes.GetEnumerator();
            bool paramArray =false;

            while (en.MoveNext()) { 
                // we need to convert paramArrayAttribute to params keyword to 
                // make csharp compiler happy. In addition, params keyword needs to be after 
                // other attributes.

                CodeAttributeDeclaration current = (CodeAttributeDeclaration)en.Current;

                if( current.Name.Equals("system.paramarrayattribute", StringComparison.OrdinalIgnoreCase)) {
                    paramArray = true;
                    continue;
                }

                GenerateAttributeDeclarationsStart(attributes);
                if (prefix != null) {
                    Output.Write(prefix);
                }

                if( current.AttributeType != null) {
                    Output.Write(GetTypeOutput(current.AttributeType));
                }
                Output.Write("(");

                bool firstArg = true;
                foreach (CodeAttributeArgument arg in current.Arguments) {
                    if (firstArg) {
                        firstArg = false;
                    }
                    else {
                        Output.Write(", ");
                    }

                    OutputAttributeArgument(arg);
                }

                Output.Write(")");
                GenerateAttributeDeclarationsEnd(attributes);
                if (inLine) {
                    Output.Write(" ");
                } 
                else {
                    Output.WriteLine();
                }
            }

            if( paramArray) {
                if (prefix != null) {
                    Output.Write(prefix);
                }
                Output.Write("params");
                
                if (inLine) {
                    Output.Write(" ");
                } 
                else {
                    Output.WriteLine();
                }
            }


        }

        static bool IsKeyword(string value) {            
            return FixedStringLookup.Contains(keywords, value, false);
        }

        static bool IsPrefixTwoUnderscore(string value) {
            if( value.Length < 3) {
                return false;
            }
            else {
                return ((value[0] == '_') && (value[1] == '_') && (value[2] != '_'));
            }
        }

        public bool Supports(GeneratorSupport support) {
            return ((support & LanguageSupport) == support);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets whether the specified value is a valid identifier.
        ///    </para>
        /// </devdoc>
        public bool IsValidIdentifier(string value) {

            // identifiers must be 1 char or longer
            //
            if (value == null || value.Length == 0) {
                return false;
            }

            if (value.Length > 512)
                return false;
            
            // identifiers cannot be a keyword, unless they are escaped with an '@'
            //
            if (value[0] != '@') {
                if (IsKeyword(value))
                    return false;
            }
            else  {
                value = value.Substring(1);
            }

            return CodeGenerator.IsValidLanguageIndependentIdentifier(value);
        }

        public void ValidateIdentifier(string value) {
            if (!IsValidIdentifier(value)) {
                throw new ArgumentException(SR.GetString(SR.InvalidIdentifier, value));
            }
        }

        public string CreateValidIdentifier(string name) {
            if(IsPrefixTwoUnderscore(name)) {
                name = "_" + name;
            }
            
            while (IsKeyword(name)) {                
                name = "_" + name;
            }

            return name;
        }

        public string CreateEscapedIdentifier(string name) {
            // Any identifier started with two consecutive underscores are 
            // reserved by CSharp.
            if (IsKeyword(name) || IsPrefixTwoUnderscore(name)) {
                return "@" + name;
            }
            return name;
        }

        // returns the type name without any array declaration.
        private string GetBaseTypeOutput(CodeTypeReference typeRef) {
            string s = typeRef.BaseType;
            if (s.Length == 0) {
                s = "void";
                return s;
            }
 
            string lowerCaseString  = s.ToLower( CultureInfo.InvariantCulture).Trim();

            switch (lowerCaseString) {
                case "system.int16":            
                    s = "short";
                    break;   
                case "system.int32":            
                    s = "int";
                    break;   
                case "system.int64":            
                    s = "long";
                    break;   
                case "system.string":            
                    s = "string";
                    break;   
                case "system.object":            
                    s = "object";
                    break;   
                case "system.boolean":            
                    s = "bool";
                    break;   
                case "system.void":            
                    s = "void";
                    break;   
                case "system.char":            
                    s = "char";
                    break;   
                case "system.byte":            
                    s = "byte";
                    break;   
                case "system.uint16":            
                    s = "ushort";
                    break;   
                case "system.uint32":            
                    s = "uint";
                    break;   
                case "system.uint64":            
                    s = "ulong";
                    break;   
                case "system.sbyte":            
                    s = "sbyte";
                    break;                       
                case "system.single":            
                    s = "float";
                    break;   
                case "system.double":            
                    s = "double";
                    break;   
                case "system.decimal":            
                    s = "decimal";
                    break;                       
                default:
                    // replace + with . for nested classes.
                    //
                    StringBuilder sb = new StringBuilder(s.Length + 10);                      
                    if ((typeRef.Options & CodeTypeReferenceOptions.GlobalReference) != 0) {
                        sb.Append("global::");
                    }

                    string baseType = typeRef.BaseType;
                    
                    int lastIndex = 0;
                    int currentTypeArgStart = 0;
                    for (int i=0; i<baseType.Length; i++) {
                        switch (baseType[i]) {
                        case '+':
                        case '.':
                            sb.Append(CreateEscapedIdentifier(baseType.Substring(lastIndex, i-lastIndex)));
                            sb.Append('.');
                            i++;
                            lastIndex = i;
                            break;
                            
                        case '`':
                            sb.Append(CreateEscapedIdentifier(baseType.Substring(lastIndex, i-lastIndex)));
                            i++;    // skip the '
                            int numTypeArgs = 0;
                            while (i < baseType.Length && baseType[i] >= '0' && baseType[i] <='9') {
                                numTypeArgs = numTypeArgs*10 + (baseType[i] - '0');
                                i++;
                            }
                    
                            GetTypeArgumentsOutput(typeRef.TypeArguments, currentTypeArgStart, numTypeArgs, sb);
                            currentTypeArgStart += numTypeArgs;
                    
                            // Arity can be in the middle of a nested type name, so we might have a . or + after it. 
                            // Skip it if so. 
                            if (i < baseType.Length &&  (baseType[i] =='+' || baseType[i] == '.')) {
                                sb.Append('.');
                                i++;
                            }
                                
                            lastIndex = i;
                            break;
                        }
                    }
                    
                    if (lastIndex < baseType.Length)
                        sb.Append(CreateEscapedIdentifier(baseType.Substring(lastIndex)));

                    return sb.ToString();
            }
            return s;
        }


       private String GetTypeArgumentsOutput(CodeTypeReferenceCollection typeArguments) {
            StringBuilder sb = new StringBuilder(128);
            GetTypeArgumentsOutput(typeArguments, 0, typeArguments.Count, sb);
            return sb.ToString();
       }

       private void GetTypeArgumentsOutput(CodeTypeReferenceCollection typeArguments, int start, int length, StringBuilder sb) {
            sb.Append('<');
            bool first = true;
            for( int i = start; i < start+length; i++) {
                if( first) {
                    first = false;
                }
                else {
                    sb.Append(", ");
                }

                // it's possible that we call GetTypeArgumentsOutput with an empty typeArguments collection.  This is the case
                // for open types, so we want to just output the brackets and commas. 
                if (i < typeArguments.Count)
                    sb.Append(GetTypeOutput(typeArguments[i])); 
            }
            sb.Append('>');
        }

        public string GetTypeOutput(CodeTypeReference typeRef) {
            string s = String.Empty;
            
            CodeTypeReference baseTypeRef = typeRef;
            while(baseTypeRef.ArrayElementType != null) {
                baseTypeRef = baseTypeRef.ArrayElementType;
            }
            s += GetBaseTypeOutput(baseTypeRef);

            while(typeRef !=null && typeRef.ArrayRank > 0) {
                char [] results = new char [typeRef.ArrayRank + 1];
                results[0] = '[';
                results[typeRef.ArrayRank] = ']';
                for (int i = 1; i < typeRef.ArrayRank; i++) {
                    results[i] = ',';
                }
                s += new string(results);
                typeRef = typeRef.ArrayElementType;
            }

            return s;
        }

        private void OutputStartingBrace() {
            if (Options.BracingStyle == "C") {
                Output.WriteLine("");
                Output.WriteLine("{");
            }
            else {
                Output.WriteLine(" {");
            }
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private CompilerResults FromFileBatch(CompilerParameters options, string[] fileNames) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }
            if (fileNames == null)
                throw new ArgumentNullException("fileNames");

            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();

            string outputFile = null;
            int retValue = 0;

            CompilerResults results = new CompilerResults(options.TempFiles);
            SecurityPermission perm1 = new SecurityPermission(SecurityPermissionFlag.ControlEvidence);
            perm1.Assert();
            try {
#pragma warning disable 618
               results.Evidence = options.Evidence;
#pragma warning restore 618
            }
            finally {
                 SecurityPermission.RevertAssert();
            }
            bool createdEmptyAssembly = false;

            if (options.OutputAssembly == null || options.OutputAssembly.Length == 0) {
                string extension = (options.GenerateExecutable) ? "exe" : "dll";
                options.OutputAssembly = results.TempFiles.AddExtension(extension, !options.GenerateInMemory);

                // Create an empty assembly.  This is so that the file will have permissions that
                // we can later access with our current credential. If we don't do this, the compiler
                // could end up creating an assembly that we cannot open.
                new FileStream(options.OutputAssembly, FileMode.Create, FileAccess.ReadWrite).Close();
                createdEmptyAssembly = true;
            }

#if FEATURE_PAL
            string pdbname = "ildb";
#else
            string pdbname = "pdb";
#endif
            
            // Don't delete pdbs when debug=false but they have specified pdbonly. 
            if (options.CompilerOptions!= null
                    && -1 != CultureInfo.InvariantCulture.CompareInfo.IndexOf(options.CompilerOptions,"/debug:pdbonly", CompareOptions.IgnoreCase))
                results.TempFiles.AddExtension(pdbname, true);
            else
                results.TempFiles.AddExtension(pdbname);

            string args = CmdArgsFromParameters(options) + " " + JoinStringArray(fileNames, " ");

            // Use a response file if the compiler supports it
            string responseFileArgs = GetResponseFileCmdArgs(options, args);
            string trueArgs = null;
            if (responseFileArgs != null) {
                trueArgs = args;
                args = responseFileArgs;
            }

            Compile(options,
                RedistVersionInfo.GetCompilerPath(provOptions, CompilerName),
                CompilerName,
                args,
                ref outputFile,
                ref retValue,
                trueArgs);

            results.NativeCompilerReturnValue = retValue;

            // only look for errors/warnings if the compile failed or the caller set the warning level
            if (retValue != 0 || options.WarningLevel > 0) {

                // The output of the compiler is in UTF8
                string [] lines = ReadAllLines(outputFile, Encoding.UTF8, FileShare.ReadWrite);
                foreach (string line in lines) {
                    results.Output.Add(line);

                    ProcessCompilerOutputLine(results, line);
                }

                // Delete the empty assembly if we created one
                if (retValue != 0 && createdEmptyAssembly)
                    File.Delete(options.OutputAssembly);
            }

            if (results.Errors.HasErrors || !options.GenerateInMemory) {

                results.PathToAssembly = options.OutputAssembly;
                return results;
            }

            // Read assembly into memory:
            byte[] assemblyBuff = File.ReadAllBytes(options.OutputAssembly);

            // Read symbol file into mempory and ignore any errors that may be encountered:
            // (This functionality was added in NetFx 4.5, errors must be ignored to ensure compatibility)
            byte[] symbolsBuff = null;
            try {

                String symbFileName = options.TempFiles.BasePath + "." + pdbname;
            
                if (File.Exists(symbFileName))
                    symbolsBuff = File.ReadAllBytes(symbFileName);

            } catch {
                symbolsBuff = null;
            }
           
            // Now get permissions and load assembly from buffer into the CLR:
            SecurityPermission perm = new SecurityPermission(SecurityPermissionFlag.ControlEvidence);
            perm.Assert();

            try {

                #pragma warning disable 618 // Load with evidence is obsolete - this warning is passed on via the options.Evidence property
                results.CompiledAssembly = Assembly.Load(assemblyBuff, symbolsBuff, options.Evidence);
                #pragma warning restore 618

            } finally {
                SecurityPermission.RevertAssert();
            }

            return results;
        }
        
        private static string[] ReadAllLines(String file, Encoding encoding, FileShare share)
        {
            using(FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, share))
            {
                String line;
                List<String> lines = new List<String>();
                
                using (StreamReader sr = new StreamReader(stream, encoding))
                    while ((line = sr.ReadLine()) != null)
                        lines.Add(line);

                return lines.ToArray();
            }
        }

        /// <internalonly/>
        CompilerResults ICodeCompiler.CompileAssemblyFromDom(CompilerParameters options, CodeCompileUnit e) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }

            try {                
                return FromDom(options, e);
            }
            finally {
                options.TempFiles.SafeDelete();
            }
        }

        /// <internalonly/>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        CompilerResults ICodeCompiler.CompileAssemblyFromFile(CompilerParameters options, string fileName) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }

            try {
                return FromFile(options, fileName);
            }
            finally {
                options.TempFiles.SafeDelete();
            }
        }

        /// <internalonly/>
        CompilerResults ICodeCompiler.CompileAssemblyFromSource(CompilerParameters options, string source) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }

            try {
                return FromSource(options, source);
            }
            finally {
                options.TempFiles.SafeDelete();
            }
        }

        /// <internalonly/>
        CompilerResults ICodeCompiler.CompileAssemblyFromSourceBatch(CompilerParameters options, string[] sources) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }

            try {
                return FromSourceBatch(options, sources);
            }
            finally {
                options.TempFiles.SafeDelete();
            }
        }
        
        /// <internalonly/>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)] 
        CompilerResults ICodeCompiler.CompileAssemblyFromFileBatch(CompilerParameters options, string[] fileNames) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }
            if (fileNames == null)
                throw new ArgumentNullException("fileNames");

            try {
                // Try opening the files to make sure they exists.  This will throw an exception
                // if it doesn't
                foreach (string fileName in fileNames) {
                    using (Stream str = File.OpenRead(fileName)) { }
                }

                return FromFileBatch(options, fileNames);
            }
            finally {
                options.TempFiles.SafeDelete();
            }
        }

        /// <internalonly/>
        CompilerResults ICodeCompiler.CompileAssemblyFromDomBatch(CompilerParameters options, CodeCompileUnit[] ea) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }

            try {
                return FromDomBatch(options, ea);
            }
            finally {
                options.TempFiles.SafeDelete();
            }
        }
        
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal void Compile(CompilerParameters options, string compilerDirectory, string compilerExe, string arguments,
                              ref string outputFile, ref int nativeReturnValue, string trueArgs) {
            string errorFile = null;
            outputFile = options.TempFiles.AddExtension("out");
            
            // We try to execute the compiler with a full path name.
            string fullname = Path.Combine(compilerDirectory, compilerExe);
            if (File.Exists(fullname)) {
                string trueCmdLine = null;
                if (trueArgs != null)
                    trueCmdLine = "\"" + fullname + "\" " + trueArgs;
                nativeReturnValue = Executor.ExecWaitWithCapture(options.SafeUserToken, "\"" + fullname + "\" " + arguments,
                                                                 Environment.CurrentDirectory, options.TempFiles, ref outputFile, ref errorFile,
                                                                 trueCmdLine);
            }
            else {
                throw new InvalidOperationException(SR.GetString(SR.CompilerNotFound, fullname));
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Compiles the specified compile unit and options, and returns the results
        ///       from the compilation.
        ///    </para>
        /// </devdoc>
        private CompilerResults FromDom(CompilerParameters options, CodeCompileUnit e) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                        
            CodeCompileUnit[] units = new CodeCompileUnit[1];
            units[0] = e;
            return FromDomBatch(options, units);
        }

        /// <devdoc>
        ///    <para>
        ///       Compiles the specified file using the specified options, and returns the
        ///       results from the compilation.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private CompilerResults FromFile(CompilerParameters options, string fileName) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            
            // Try opening the file to make sure it exists.  This will throw an exception
            // if it doesn't
            using (Stream str = File.OpenRead(fileName)) { }

            string[] filenames = new string[1];
            filenames[0] = fileName;
            return FromFileBatch(options, filenames);
        }
        
        /// <devdoc>
        ///    <para>
        ///       Compiles the specified source code using the specified options, and
        ///       returns the results from the compilation.
        ///    </para>
        /// </devdoc>
         private CompilerResults FromSource(CompilerParameters options, string source) {
             if( options == null) {
                 throw new ArgumentNullException("options");
             }

            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();

            string[] sources = new string[1];
            sources[0] = source;

            return FromSourceBatch(options, sources);
        }
        
        /// <devdoc>
        ///    <para>
        ///       Compiles the specified compile units and
        ///       options, and returns the results from the compilation.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private CompilerResults FromDomBatch(CompilerParameters options, CodeCompileUnit[] ea) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }
            if (ea == null)
                throw new ArgumentNullException("ea");

            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();

            string[] filenames = new string[ea.Length];

            CompilerResults results = null;

#if !FEATURE_PAL
            // the extra try-catch is here to mitigate exception filter injection attacks. 
            try {
                WindowsImpersonationContext impersonation = Executor.RevertImpersonation();
                try {
#endif // !FEATURE_PAL
                    for (int i = 0; i < ea.Length; i++) {
                        if (ea[i] == null)
                            continue;       // the other two batch methods just work if one element is null, so we'll match that. 
                        
                        ResolveReferencedAssemblies(options, ea[i]);
                        filenames[i] = options.TempFiles.AddExtension(i + FileExtension);
                        Stream temp = new FileStream(filenames[i], FileMode.Create, FileAccess.Write, FileShare.Read);
                        try {
                            using (StreamWriter sw = new StreamWriter(temp, Encoding.UTF8)){
                                ((ICodeGenerator)this).GenerateCodeFromCompileUnit(ea[i], sw, Options);
                                sw.Flush();
                            }
                        }
                        finally {
                            temp.Close();
                        }
                    }

                    results = FromFileBatch(options, filenames);
#if !FEATURE_PAL
                }
                finally {
                    Executor.ReImpersonate(impersonation);
                }
            }
            catch {
                throw;
            }
#endif // !FEATURE_PAL
            return results;
        }

        /// <devdoc>
        ///    <para>
        ///       Because CodeCompileUnit and CompilerParameters both have a referenced assemblies 
        ///       property, they must be reconciled. However, because you can compile multiple
        ///       compile units with one set of options, it will simply merge them.
        ///    </para>
        /// </devdoc>
        private void ResolveReferencedAssemblies(CompilerParameters options, CodeCompileUnit e) {
            if (e.ReferencedAssemblies.Count > 0) {
                foreach(string assemblyName in e.ReferencedAssemblies) {
                    if (!options.ReferencedAssemblies.Contains(assemblyName)) {
                        options.ReferencedAssemblies.Add(assemblyName);
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Compiles the specified source code strings using the specified options, and
        ///       returns the results from the compilation.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private CompilerResults FromSourceBatch(CompilerParameters options, string[] sources) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }
            if (sources == null)
                throw new ArgumentNullException("sources");

            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();

            string[] filenames = new string[sources.Length];

            CompilerResults results = null;
#if !FEATURE_PAL
            // the extra try-catch is here to mitigate exception filter injection attacks. 
            try {
                WindowsImpersonationContext impersonation = Executor.RevertImpersonation();
                try {      
#endif // !FEATURE_PAL
                    for (int i = 0; i < sources.Length; i++) {
                        string name = options.TempFiles.AddExtension(i + FileExtension);
                        Stream temp = new FileStream(name, FileMode.Create, FileAccess.Write, FileShare.Read);
                        try {
                            using (StreamWriter sw = new StreamWriter(temp, Encoding.UTF8)) {
                                sw.Write(sources[i]);
                                sw.Flush();
                            }
                        }
                        finally {
                            temp.Close();
                        }
                        filenames[i] = name;
                   }
                   results = FromFileBatch(options, filenames);
#if !FEATURE_PAL
                }
                finally {
                    Executor.ReImpersonate(impersonation);
                }
            }   
            catch {
                throw;
            }
#endif // !FEATURE_PAL

            return results;
        }

        /// <devdoc>
        ///    <para>Joins the specified string arrays.</para>
        /// </devdoc>
        private static string JoinStringArray(string[] sa, string separator) {
            if (sa == null || sa.Length == 0)
                return String.Empty;

            if (sa.Length == 1) {
                return "\"" + sa[0] + "\"";
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < sa.Length - 1; i++) {
                sb.Append("\"");
                sb.Append(sa[i]);
                sb.Append("\"");
                sb.Append(separator);
            }
            sb.Append("\"");
            sb.Append(sa[sa.Length - 1]);
            sb.Append("\"");

            return sb.ToString();
        }

        /// <internalonly/>
        void ICodeGenerator.GenerateCodeFromType(CodeTypeDeclaration e, TextWriter w, CodeGeneratorOptions o) {
            bool setLocal = false;
            if (output != null && w != output.InnerWriter) {
                throw new InvalidOperationException(SR.GetString(SR.CodeGenOutputWriter));
            }
            if (output == null) {
                setLocal = true;
                options = (o == null) ? new CodeGeneratorOptions() : o;
                output = new IndentedTextWriter(w, options.IndentString);
            }

            try {
                GenerateType(e);
            }
            finally {
                if (setLocal) {
                    output = null;
                    options = null;
                }
            }
        }

        /// <internalonly/>
        void ICodeGenerator.GenerateCodeFromExpression(CodeExpression e, TextWriter w, CodeGeneratorOptions o) {
            bool setLocal = false;
            if (output != null && w != output.InnerWriter) {
                throw new InvalidOperationException(SR.GetString(SR.CodeGenOutputWriter));
            }
            if (output == null) {
                setLocal = true;
                options = (o == null) ? new CodeGeneratorOptions() : o;
                output = new IndentedTextWriter(w, options.IndentString);
            }

            try {
                GenerateExpression(e);
            }
            finally {
                if (setLocal) {
                    output = null;
                    options = null;
                }
            }
        }

        /// <internalonly/>
        void ICodeGenerator.GenerateCodeFromCompileUnit(CodeCompileUnit e, TextWriter w, CodeGeneratorOptions o) {
            bool setLocal = false;
            if (output != null && w != output.InnerWriter) {
                throw new InvalidOperationException(SR.GetString(SR.CodeGenOutputWriter));
            }
            if (output == null) {
                setLocal = true;
                options = (o == null) ? new CodeGeneratorOptions() : o;
                output = new IndentedTextWriter(w, options.IndentString);
            }

            try {
                if (e is CodeSnippetCompileUnit) {
                    GenerateSnippetCompileUnit((CodeSnippetCompileUnit) e);
                }
                else {
                    GenerateCompileUnit(e);
                }
            }
            finally {
                if (setLocal) {
                    output = null;
                    options = null;
                }
            }
        }

        /// <internalonly/>
        void ICodeGenerator.GenerateCodeFromNamespace(CodeNamespace e, TextWriter w, CodeGeneratorOptions o) {
            bool setLocal = false;
            if (output != null && w != output.InnerWriter) {
                throw new InvalidOperationException(SR.GetString(SR.CodeGenOutputWriter));
            }
            if (output == null) {
                setLocal = true;
                options = (o == null) ? new CodeGeneratorOptions() : o;
                output = new IndentedTextWriter(w, options.IndentString);
            }

            try {
                GenerateNamespace(e);
            }
            finally {
                if (setLocal) {
                    output = null;
                    options = null;
                }
            }
        }

        /// <internalonly/>
        void ICodeGenerator.GenerateCodeFromStatement(CodeStatement e, TextWriter w, CodeGeneratorOptions o) {
            bool setLocal = false;
            if (output != null && w != output.InnerWriter) {
                throw new InvalidOperationException(SR.GetString(SR.CodeGenOutputWriter));
            }
            if (output == null) {
                setLocal = true;
                options = (o == null) ? new CodeGeneratorOptions() : o;
                output = new IndentedTextWriter(w, options.IndentString);
            }

            try {
                GenerateStatement(e);
            }
            finally {
                if (setLocal) {
                    output = null;
                    options = null;
                }
            }
        }
    }  // CSharpCodeGenerator

    #endregion class CSharpCodeGenerator


    #region class CSharpTypeAttributeConverter

    internal class CSharpTypeAttributeConverter : CSharpModifierAttributeConverter {
        private static volatile string[] names;
        private static volatile object[] values;
        private static volatile CSharpTypeAttributeConverter defaultConverter;
       
        private CSharpTypeAttributeConverter() {
            // no  need to create an instance; use Default
        }

        public static CSharpTypeAttributeConverter Default {
            get {
                if (defaultConverter == null) {
                    defaultConverter = new CSharpTypeAttributeConverter();
                }
                return defaultConverter;
            }
        }
    
        /// <devdoc>
        ///      Retrieves an array of names for attributes.
        /// </devdoc>
        protected override string[] Names {
            get {
                if (names == null) {
                    names = new string[] {
                        "Public",
                        "Internal"
                    };
                }
                
                return names;
            }
        }
        
        /// <devdoc>
        ///      Retrieves an array of values for attributes.
        /// </devdoc>
        protected override object[] Values {
            get {
                if (values == null) {
                    values = new object[] {
                        (object)TypeAttributes.Public,
                        (object)TypeAttributes.NotPublic                       
                    };
                }
                
                return values;
            }
        }

        protected override object DefaultValue {
            get {
                return TypeAttributes.NotPublic;
            }
        }
    }  // CSharpTypeAttributeConverter

    #endregion class CSharpTypeAttributeConverter


    #region class CSharpMemberAttributeConverter

    internal class CSharpMemberAttributeConverter : CSharpModifierAttributeConverter {
        private static volatile string[] names;
        private static volatile object[] values;
        private static volatile CSharpMemberAttributeConverter defaultConverter;
        
        private CSharpMemberAttributeConverter() {
            // no  need to create an instance; use Default
        }

        public static CSharpMemberAttributeConverter Default {
            get {
                if (defaultConverter == null) {
                    defaultConverter = new CSharpMemberAttributeConverter();
                }
                return defaultConverter;
            }
        }
    
        /// <devdoc>
        ///      Retrieves an array of names for attributes.
        /// </devdoc>
        protected override string[] Names {
            get {
                if (names == null) {
                    names = new string[] {
                        "Public",
                        "Protected",
                        "Protected Internal",
                        "Internal",
                        "Private"
                    };
                }
                
                return names;
            }
        }
        
        /// <devdoc>
        ///      Retrieves an array of values for attributes.
        /// </devdoc>
        protected override object[] Values {
            get {
                if (values == null) {
                    values = new object[] {
                        (object)MemberAttributes.Public,
                        (object)MemberAttributes.Family,
                        (object)MemberAttributes.FamilyOrAssembly,
                        (object)MemberAttributes.Assembly,
                        (object)MemberAttributes.Private
                    };
                }
                
                return values;
            }
        }

        protected override object DefaultValue {
            get {
                return MemberAttributes.Private;
            }
        }
    }  // CSharpMemberAttributeConverter

    #endregion class CSharpMemberAttributeConverter


    #region class CSharpModifierAttributeConverter

    /// <devdoc>
    ///      This type converter provides common values for MemberAttributes
    /// </devdoc>
    internal abstract class CSharpModifierAttributeConverter : TypeConverter {          

        protected abstract object[] Values { get; }
        protected abstract string[] Names  { get; }
        protected abstract object DefaultValue { get; }
        
       

        /// <devdoc>
        ///      We override this because we can convert from string types.
        /// </devdoc>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string)) {
                return true;
            }
            
            return base.CanConvertFrom(context, sourceType);
        }

        /// <devdoc>
        ///      Converts the given object to the converter's native type.
        /// </devdoc>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is string) {
                string name = (string)value;
                string[] names = Names;
                for (int i = 0; i < names.Length; i++) {
                    if (names[i].Equals(name)) {
                        return Values[i];
                    }
                }
            }
            
            return DefaultValue;
        }

        /// <devdoc>
        ///      Converts the given object to another type.  The most common types to convert
        ///      are to and from a string object.  The default implementation will make a call
        ///      to ToString on the object if the object is valid and if the destination
        ///      type is string.  If this cannot convert to the desitnation type, this will
        ///      throw a NotSupportedException.
        /// </devdoc>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == null) {
                throw new ArgumentNullException("destinationType");
            }
            
            if (destinationType == typeof(string)) {
                object[] modifiers = Values;
                for (int i = 0; i < modifiers.Length; i++) {
                    if (modifiers[i].Equals(value)) {
                        return Names[i];
                    }
                }
                
                return SR.GetString(SR.toStringUnknown);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <devdoc>
        ///      Determines if the list of standard values returned from
        ///      GetStandardValues is an exclusive list.  If the list
        ///      is exclusive, then no other values are valid, such as
        ///      in an enum data type.  If the list is not exclusive,
        ///      then there are other valid values besides the list of
        ///      standard values GetStandardValues provides.
        /// </devdoc>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
            return true;
        }
        
        /// <devdoc>
        ///      Determines if this object supports a standard set of values
        ///      that can be picked from a list.
        /// </devdoc>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return true;
        }
        
        /// <devdoc>
        ///      Retrieves a collection containing a set of standard values
        ///      for the data type this validator is designed for.  This
        ///      will return null if the data type does not support a
        ///      standard set of values.
        /// </devdoc>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) { 
            return new StandardValuesCollection(Values);
        }
    }  // CSharpModifierAttributeConverter

    #endregion class CSharpModifierAttributeConverter
}

