//------------------------------------------------------------------------------
// <copyright file="CodeGenerator.cs" company="Microsoft">
// 
// <OWNER>[....]</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom.Compiler {
    using System.Runtime.Remoting;
    using System.Runtime.InteropServices;

    using System.Diagnostics;
    using System;
    using Microsoft.Win32;
    using System.IO;
    using System.Collections;
    using System.Reflection;
    using System.Globalization;
    using System.CodeDom;
    using System.Security.Permissions;
    using System.Text;
    
    /// <devdoc>
    ///    <para>Provides a base class for code generators.</para>
    /// </devdoc>
    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class CodeGenerator : ICodeGenerator {
        private const int ParameterMultilineThreshold = 15;        
        private IndentedTextWriter output;
        private CodeGeneratorOptions options;

        private CodeTypeDeclaration currentClass;
        private CodeTypeMember currentMember;

        private bool inNestedBinary = false;

        /// <devdoc>
        ///    <para>
        ///       Gets the current class.
        ///    </para>
        /// </devdoc>
        protected CodeTypeDeclaration CurrentClass {
            get {
                return currentClass;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the current class name.
        ///    </para>
        /// </devdoc>
        protected string CurrentTypeName {
            get {
                if (currentClass != null) {
                    return currentClass.Name;
                }
                return "<% unknown %>";
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the current member of the class.
        ///    </para>
        /// </devdoc>
        protected CodeTypeMember CurrentMember {
            get {
                return currentMember;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the current member name.
        ///    </para>
        /// </devdoc>
        protected string CurrentMemberName {
            get {
                if (currentMember != null) {
                    return currentMember.Name;
                }
                return "<% unknown %>";
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether the current object being
        ///       generated is an interface.
        ///    </para>
        /// </devdoc>
        protected bool IsCurrentInterface {
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
        protected bool IsCurrentClass {
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
        protected bool IsCurrentStruct {
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
        protected bool IsCurrentEnum {
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
        protected bool IsCurrentDelegate {
            get {
                if (currentClass != null && currentClass is CodeTypeDelegate) {
                    return true;
                }
                return false;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the amount of spaces to indent.
        ///    </para>
        /// </devdoc>
        protected int Indent {
            get {
                return output.Indent;
            }
            set {
                output.Indent = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the token that represents <see langword='null'/>.
        ///    </para>
        /// </devdoc>
        protected abstract string NullToken { get; }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the System.IO.TextWriter
        ///       to use for output.
        ///    </para>
        /// </devdoc>
        protected TextWriter Output {
            get {
                return output;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected CodeGeneratorOptions Options {
            get {
                return options;
            }
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
        
        protected virtual void GenerateDirectives(CodeDirectiveCollection directives) {            
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

        /// <devdoc>
        ///    <para> Generates code for the namepsaces in the specifield CodeDom compile unit.
        ///     </para>
        /// </devdoc>
        protected void GenerateNamespaces(CodeCompileUnit e) {
            foreach (CodeNamespace n in e.Namespaces) {
                ((ICodeGenerator)this).GenerateCodeFromNamespace(n, output.InnerWriter, options);
            }
        }

        /// <devdoc>
        ///    <para> Generates code for the specified CodeDom namespace representation and the classes it
        ///       contains.</para>
        /// </devdoc>
        protected void GenerateTypes(CodeNamespace e) {
            foreach (CodeTypeDeclaration c in e.Types) {
                if (options.BlankLinesBetweenMembers) {
                            Output.WriteLine();
                }
                ((ICodeGenerator)this).GenerateCodeFromType(c, output.InnerWriter, options);
            }
        }

        /// <internalonly/>
        bool ICodeGenerator.Supports(GeneratorSupport support) {
            return this.Supports(support);
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
        
        public virtual void GenerateCodeFromMember(CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options) {
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
        

        /// <internalonly/>
        bool ICodeGenerator.IsValidIdentifier(string value) {
            return this.IsValidIdentifier(value);
        }
        /// <internalonly/>
        void ICodeGenerator.ValidateIdentifier(string value) {
            this.ValidateIdentifier(value);
        }

        /// <internalonly/>
        string ICodeGenerator.CreateEscapedIdentifier(string value) {
            return this.CreateEscapedIdentifier(value);
        }

        /// <internalonly/>
        string ICodeGenerator.CreateValidIdentifier(string value) {
            return this.CreateValidIdentifier(value);
        }

        /// <internalonly/>
        string ICodeGenerator.GetTypeOutput(CodeTypeReference type) {
            return this.GetTypeOutput(type);
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

        /// <devdoc>
        ///    <para>Generates code for the specified CodeDom code expression representation.</para>
        /// </devdoc>
        protected void GenerateExpression(CodeExpression e) {
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

        /// <devdoc>
        ///    <para> Generates code for the specified snippet code block
        ///       </para>
        /// </devdoc>
        protected virtual void GenerateSnippetCompileUnit(CodeSnippetCompileUnit e) {
            
            GenerateDirectives(e.StartDirectives);

            if (e.LinePragma != null) GenerateLinePragmaStart(e.LinePragma);
            Output.WriteLine(e.Value);
            if (e.LinePragma != null) GenerateLinePragmaEnd(e.LinePragma);

            if (e.EndDirectives.Count > 0) {
                GenerateDirectives(e.EndDirectives);
            }            
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
        ///    <para> Generates code for the specified CodeDom
        ///       compile unit representation.</para>
        /// </devdoc>
        protected virtual void GenerateCompileUnit(CodeCompileUnit e) {
            GenerateCompileUnitStart(e);
            GenerateNamespaces(e);
            GenerateCompileUnitEnd(e);
        }

        /// <devdoc>
        ///    <para> Generates code for the specified CodeDom
        ///       namespace representation.</para>
        /// </devdoc>
        protected virtual void GenerateNamespace(CodeNamespace e) {
            GenerateCommentStatements(e.Comments);
            GenerateNamespaceStart(e);

            GenerateNamespaceImports(e);
            Output.WriteLine("");

            GenerateTypes(e);
            GenerateNamespaceEnd(e);
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based namespace import
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected void GenerateNamespaceImports(CodeNamespace e) {
            IEnumerator en = e.Imports.GetEnumerator();
            while (en.MoveNext()) {
                CodeNamespaceImport imp = (CodeNamespaceImport)en.Current;
                if (imp.LinePragma != null) GenerateLinePragmaStart(imp.LinePragma);
                GenerateNamespaceImport(imp);
                if (imp.LinePragma != null) GenerateLinePragmaEnd(imp.LinePragma);
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
        ///       Generates code for
        ///       the specified CodeDom based statement representation.
        ///    </para>
        /// </devdoc>
        protected void GenerateStatement(CodeStatement e) {        
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
        protected void GenerateStatements(CodeStatementCollection stms) {
            IEnumerator en = stms.GetEnumerator();
            while (en.MoveNext()) {
                ((ICodeGenerator)this).GenerateCodeFromStatement((CodeStatement)en.Current, output.InnerWriter, options);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified System.CodeDom.CodeAttributeBlock.
        ///    </para>
        /// </devdoc>
        protected virtual void OutputAttributeDeclarations(CodeAttributeDeclarationCollection attributes) {
            if (attributes.Count == 0) return;
            GenerateAttributeDeclarationsStart(attributes);
            bool first = true;
            IEnumerator en = attributes.GetEnumerator();
            while (en.MoveNext()) {
                if (first) {
                    first = false;
                }
                else {
                    ContinueOnNewLine(", ");
                }

                CodeAttributeDeclaration current = (CodeAttributeDeclaration)en.Current;
                Output.Write(current.Name);
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

            }
            GenerateAttributeDeclarationsEnd(attributes);
        }


        /// <devdoc>
        ///    <para>
        ///       Outputs an argument in a attribute block.
        ///    </para>
        /// </devdoc>
        protected virtual void OutputAttributeArgument(CodeAttributeArgument arg) {
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
        protected virtual void OutputDirection(FieldDirection dir) {
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
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void OutputFieldScopeModifier(MemberAttributes attributes) {
            switch (attributes & MemberAttributes.VTableMask) {
                case MemberAttributes.New:
                    Output.Write("new ");
                    break;
            }

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
        ///       Generates code for the specified member access modifier.
        ///    </para>
        /// </devdoc>
        protected virtual void OutputMemberAccessModifier(MemberAttributes attributes) {
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

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified member scope modifier.
        ///    </para>
        /// </devdoc>
        protected virtual void OutputMemberScopeModifier(MemberAttributes attributes) {
            switch (attributes & MemberAttributes.VTableMask) {
                case MemberAttributes.New:
                    Output.Write("new ");
                    break;
            }

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
        ///       Generates code for the specified type.
        ///    </para>
        /// </devdoc>
        protected abstract void OutputType(CodeTypeReference typeRef);

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified type attributes.
        ///    </para>
        /// </devdoc>
        protected virtual void OutputTypeAttributes(TypeAttributes attributes, bool isStruct, bool isEnum) {
            switch(attributes & TypeAttributes.VisibilityMask) {
                case TypeAttributes.Public:                  
                case TypeAttributes.NestedPublic:                    
                    Output.Write("public ");
                    break;
                case TypeAttributes.NestedPrivate:
                    Output.Write("private ");
                    break;
            }
            
            if (isStruct) {
                Output.Write("struct ");
            }
            else if (isEnum) {
                Output.Write("enum ");
            }     
            else {            
                switch (attributes & TypeAttributes.ClassSemanticsMask) {
                    case TypeAttributes.Class:
                        if ((attributes & TypeAttributes.Sealed) == TypeAttributes.Sealed) {
                            Output.Write("sealed ");
                        }
                        if ((attributes & TypeAttributes.Abstract) == TypeAttributes.Abstract) {
                            Output.Write("abstract ");
                        }
                        Output.Write("class ");
                        break;                
                    case TypeAttributes.Interface:
                        Output.Write("interface ");
                        break;
                }     
            }   
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified object type and name pair.
        ///    </para>
        /// </devdoc>
        protected virtual void OutputTypeNamePair(CodeTypeReference typeRef, string name) {
            OutputType(typeRef);
            Output.Write(" ");
            OutputIdentifier(name);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void OutputIdentifier(string ident) {
            Output.Write(ident);
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified expression list.
        ///    </para>
        /// </devdoc>
        protected virtual void OutputExpressionList(CodeExpressionCollection expressions) {
            OutputExpressionList(expressions, false /*newlineBetweenItems*/);
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified expression list.
        ///    </para>
        /// </devdoc>
        protected virtual void OutputExpressionList(CodeExpressionCollection expressions, bool newlineBetweenItems) {
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
        ///       Generates code for the specified operator.
        ///    </para>
        /// </devdoc>
        protected virtual void OutputOperator(CodeBinaryOperatorType op) {
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

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified parameters.
        ///    </para>
        /// </devdoc>
        protected virtual void OutputParameters(CodeParameterDeclarationExpressionCollection parameters) {
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
        ///       Generates code for the specified CodeDom based array creation expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateArrayCreateExpression(CodeArrayCreateExpression e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based base reference expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateBaseReferenceExpression(CodeBaseReferenceExpression e);

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based binary operator
        ///       expression representation.
        ///    </para>
        /// </devdoc>
        protected virtual void GenerateBinaryOperatorExpression(CodeBinaryOperatorExpression e) {
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
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void ContinueOnNewLine(string st) {
            Output.WriteLine(st);
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based cast expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateCastExpression(CodeCastExpression e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based delegate creation expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateDelegateCreateExpression(CodeDelegateCreateExpression e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based field reference
        ///       expression representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateFieldReferenceExpression(CodeFieldReferenceExpression e);

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract void GenerateArgumentReferenceExpression(CodeArgumentReferenceExpression e);

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract void GenerateVariableReferenceExpression(CodeVariableReferenceExpression e);
        
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based indexer expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateIndexerExpression(CodeIndexerExpression e);

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract void GenerateArrayIndexerExpression(CodeArrayIndexerExpression e);

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based snippet
        ///       expression representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateSnippetExpression(CodeSnippetExpression e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based method invoke expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateMethodInvokeExpression(CodeMethodInvokeExpression e);

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract void GenerateMethodReferenceExpression(CodeMethodReferenceExpression e);

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract void GenerateEventReferenceExpression(CodeEventReferenceExpression e);

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based delegate invoke expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateDelegateInvokeExpression(CodeDelegateInvokeExpression e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom
        ///       based object creation expression representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateObjectCreateExpression(CodeObjectCreateExpression e);

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom
        ///       based parameter declaration expression representation.
        ///    </para>
        /// </devdoc>
        protected virtual void GenerateParameterDeclarationExpression(CodeParameterDeclarationExpression e) {
            if (e.CustomAttributes.Count > 0) {
                OutputAttributeDeclarations(e.CustomAttributes);
                Output.Write(" ");
            }

            OutputDirection(e.Direction);
            OutputTypeNamePair(e.Type, e.Name);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void GenerateDirectionExpression(CodeDirectionExpression e) {
            OutputDirection(e.Direction);
            GenerateExpression(e.Expression);
        }


        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based primitive expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected virtual void GeneratePrimitiveExpression(CodePrimitiveExpression e) {
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

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void GenerateSingleFloatValue(Single s) {
            Output.Write(s.ToString("R", CultureInfo.InvariantCulture));
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void GenerateDoubleValue(Double d) {
            Output.Write(d.ToString("R", CultureInfo.InvariantCulture));
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void GenerateDecimalValue(Decimal d) {
            Output.Write(d.ToString(CultureInfo.InvariantCulture));
        }

        // 
        protected virtual void GenerateDefaultValueExpression(CodeDefaultValueExpression e) {
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based property reference
        ///       expression representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GeneratePropertyReferenceExpression(CodePropertyReferenceExpression e);

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract void GeneratePropertySetValueReferenceExpression(CodePropertySetValueReferenceExpression e);

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based this reference expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateThisReferenceExpression(CodeThisReferenceExpression e);

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based type reference expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected virtual void GenerateTypeReferenceExpression(CodeTypeReferenceExpression e) {
            OutputType(e.Type);
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based type of expression
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected virtual void GenerateTypeOfExpression(CodeTypeOfExpression e) {
            Output.Write("typeof(");
            OutputType(e.Type);
            Output.Write(")");
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based method
        ///       invoke statement representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateExpressionStatement(CodeExpressionStatement e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based for loop statement
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateIterationStatement(CodeIterationStatement e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based throw exception statement
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateThrowExceptionStatement(CodeThrowExceptionStatement e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based comment statement
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected virtual void GenerateCommentStatement(CodeCommentStatement e) {
            if(e.Comment == null)
                throw new ArgumentException(SR.GetString(SR.Argument_NullComment, "e"), "e");
            GenerateComment(e.Comment);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void GenerateCommentStatements(CodeCommentStatementCollection e) {
            foreach (CodeCommentStatement comment in e) {
                GenerateCommentStatement(comment);
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract void GenerateComment(CodeComment e);

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based method return statement
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateMethodReturnStatement(CodeMethodReturnStatement e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based if statement representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateConditionStatement(CodeConditionStatement e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based try catch finally
        ///       statement representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateTryCatchFinallyStatement(CodeTryCatchFinallyStatement e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based assignment statement
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateAssignStatement(CodeAssignStatement e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based attach event statement
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateAttachEventStatement(CodeAttachEventStatement e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based detach event statement
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateRemoveEventStatement(CodeRemoveEventStatement e);

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract void GenerateGotoStatement(CodeGotoStatement e);

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract void GenerateLabeledStatement(CodeLabeledStatement e);

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based snippet statement
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected virtual void GenerateSnippetStatement(CodeSnippetStatement e) {
            Output.WriteLine(e.Value);
        }

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based variable declaration statement
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateVariableDeclarationStatement(CodeVariableDeclarationStatement e);

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based line pragma start
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateLinePragmaStart(CodeLinePragma e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based line pragma end
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateLinePragmaEnd(CodeLinePragma e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based event
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateEvent(CodeMemberEvent e, CodeTypeDeclaration c);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based member field
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateField(CodeMemberField e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based snippet class member
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateSnippetMember(CodeSnippetTypeMember e);

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract void GenerateEntryPointMethod(CodeEntryPointMethod e, CodeTypeDeclaration c);

        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based method
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateMethod(CodeMemberMethod e, CodeTypeDeclaration c);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based property
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateProperty(CodeMemberProperty e, CodeTypeDeclaration c);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based constructor
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateConstructor(CodeConstructor e, CodeTypeDeclaration c);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based class constructor
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateTypeConstructor(CodeTypeConstructor e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based start class representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateTypeStart(CodeTypeDeclaration e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based end class representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateTypeEnd(CodeTypeDeclaration e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based compile unit start
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected virtual void GenerateCompileUnitStart(CodeCompileUnit e) {
            if (e.StartDirectives.Count > 0) {
                GenerateDirectives(e.StartDirectives);
            }
        }
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based compile unit end
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected virtual void GenerateCompileUnitEnd(CodeCompileUnit e) {
            if (e.EndDirectives.Count > 0) {
                GenerateDirectives(e.EndDirectives);
            }
        }
         /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based namespace start
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateNamespaceStart(CodeNamespace e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based namespace end
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateNamespaceEnd(CodeNamespace e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based namespace import
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateNamespaceImport(CodeNamespaceImport e);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based attribute block start
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateAttributeDeclarationsStart(CodeAttributeDeclarationCollection attributes);
        /// <devdoc>
        ///    <para>
        ///       Generates code for the specified CodeDom based attribute block end
        ///       representation.
        ///    </para>
        /// </devdoc>
        protected abstract void GenerateAttributeDeclarationsEnd(CodeAttributeDeclarationCollection attributes);

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract bool Supports(GeneratorSupport support);

        /// <devdoc>
        ///    <para>
        ///       Gets or sets whether the specified value is a value identifier.
        ///    </para>
        /// </devdoc>
        protected abstract bool IsValidIdentifier(string value);
        /// <devdoc>
        ///    <para>
        ///       Gets whether the specified identifier is valid.
        ///    </para>
        /// </devdoc>
        protected virtual void ValidateIdentifier(string value) {
            if (!IsValidIdentifier(value)) {
                throw new ArgumentException(SR.GetString(SR.InvalidIdentifier, value));
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract string CreateEscapedIdentifier(string value);

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract string CreateValidIdentifier(string value);

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract string GetTypeOutput(CodeTypeReference value);

        /// <devdoc>
        ///    <para>
        ///       Provides conversion to formatting with escape codes.
        ///    </para>
        /// </devdoc>
        protected abstract string QuoteSnippetString(string value);

        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether the specified value is a valid language
        ///       independent identifier.
        ///    </para>
        /// </devdoc>
        public static bool IsValidLanguageIndependentIdentifier(string value)
        {
            return IsValidTypeNameOrIdentifier(value, false);
        }

        internal static bool IsValidLanguageIndependentTypeName(string value)
        {
            return IsValidTypeNameOrIdentifier(value, true);
        }

        private static bool IsValidTypeNameOrIdentifier(string value, bool isTypeName) {
            bool nextMustBeStartChar = true;
            
            if (value.Length == 0) 
                return false;

            // each char must be Lu, Ll, Lt, Lm, Lo, Nd, Mn, Mc, Pc
            // 
            for(int i = 0; i < value.Length; i++) {
                char ch = value[i];
                UnicodeCategory uc = Char.GetUnicodeCategory(ch);
                switch (uc) {
                    case UnicodeCategory.UppercaseLetter:        // Lu
                    case UnicodeCategory.LowercaseLetter:        // Ll
                    case UnicodeCategory.TitlecaseLetter:        // Lt
                    case UnicodeCategory.ModifierLetter:         // Lm
                    case UnicodeCategory.LetterNumber:           // Lm
                    case UnicodeCategory.OtherLetter:            // Lo
                        nextMustBeStartChar = false;
                        break;

                    case UnicodeCategory.NonSpacingMark:         // Mn
                    case UnicodeCategory.SpacingCombiningMark:   // Mc
                    case UnicodeCategory.ConnectorPunctuation:   // Pc
                    case UnicodeCategory.DecimalDigitNumber:     // Nd
                        // Underscore is a valid starting character, even though it is a ConnectorPunctuation.
                        if (nextMustBeStartChar && ch != '_')
                            return false;
                        
                        nextMustBeStartChar = false;
                        break;
                    default:
                        // We only check the special Type chars for type names. 
                        if (isTypeName && IsSpecialTypeChar(ch, ref nextMustBeStartChar)) {
                            break;
                        }

                        return false;
                }
            }

            return true;
        }

        // This can be a special character like a separator that shows up in a type name
        // This is an odd set of characters.  Some come from characters that are allowed by C++, like < and >.
        // Others are characters that are specified in the type and assembly name grammer. 
        private static bool IsSpecialTypeChar(char ch, ref bool nextMustBeStartChar) {
            switch(ch) {
                case ':':
                case '.':
                case '$':
                case '+':
                case '<':
                case '>':
                case '-':
                case '[':
                case ']':
                case ',':
                case '&':
                case '*':
                    nextMustBeStartChar = true;
                    return true;

                case '`':
                    return true;
            }
            return false;
        }

        /// <devdoc>
        ///    <para>
        ///       Validates a tree to check if all the types and idenfier names follow the rules of an identifier
        ///       in a langauge independent manner.
        ///    </para>
        /// </devdoc>
        public static void ValidateIdentifiers(CodeObject e) {
            CodeValidator codeValidator = new CodeValidator(); // This has internal state and hence is not static
            codeValidator.ValidateIdentifiers(e);
        }

    }
}
