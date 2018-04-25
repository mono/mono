//------------------------------------------------------------------------------
// <copyright file="ICodeGenerator.cs" company="Microsoft">
// 
// <OWNER>petes</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom.Compiler {

    using System.Diagnostics;
    using System.IO;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>
    ///       Provides an
    ///       interface for code generation.
    ///    </para>
    /// </devdoc>
    public interface ICodeGenerator {
        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether
        ///       the specified value is a valid identifier for this language.
        ///    </para>
        /// </devdoc>
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        bool IsValidIdentifier(string value);

        /// <devdoc>
        ///    <para>
        ///       Throws an exception if value is not a valid identifier.
        ///    </para>
        /// </devdoc>
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        void ValidateIdentifier(string value);

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        string CreateEscapedIdentifier(string value);

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        string CreateValidIdentifier(string value);

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        string GetTypeOutput(CodeTypeReference type);

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        bool Supports(GeneratorSupport supports);

        /// <devdoc>
        ///    <para>
        ///       Generates code from the specified expression and
        ///       outputs it to the specified textwriter.
        ///    </para>
        /// </devdoc>
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        void GenerateCodeFromExpression(CodeExpression e, TextWriter w, CodeGeneratorOptions o);

        /// <devdoc>
        ///    <para>
        ///       Outputs the language specific representaion of the CodeDom tree
        ///       refered to by e, into w.
        ///    </para>
        /// </devdoc>
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        void GenerateCodeFromStatement(CodeStatement e, TextWriter w, CodeGeneratorOptions o);

        /// <devdoc>
        ///    <para>
        ///       Outputs the language specific representaion of the CodeDom tree
        ///       refered to by e, into w.
        ///    </para>
        /// </devdoc>
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        void GenerateCodeFromNamespace(CodeNamespace e, TextWriter w, CodeGeneratorOptions o);

        /// <devdoc>
        ///    <para>
        ///       Outputs the language specific representaion of the CodeDom tree
        ///       refered to by e, into w.
        ///    </para>
        /// </devdoc>
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        void GenerateCodeFromCompileUnit(CodeCompileUnit e, TextWriter w, CodeGeneratorOptions o);

        /// <devdoc>
        ///    <para>
        ///       Outputs the language specific representaion of the CodeDom tree
        ///       refered to by e, into w.
        ///    </para>
        /// </devdoc>
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        void GenerateCodeFromType(CodeTypeDeclaration e, TextWriter w, CodeGeneratorOptions o);

    }
}
