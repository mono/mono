//------------------------------------------------------------------------------
// <copyright file="CodeTypeMember.cs" company="Microsoft">
// 
// <OWNER>Microsoft</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom {

    using System.Diagnostics;
    using System;
    using Microsoft.Win32;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;    

    /// <devdoc>
    ///    <para>
    ///       Represents a class member.
    ///    </para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeTypeMember : CodeObject {
        private MemberAttributes attributes = MemberAttributes.Private | MemberAttributes.Final;
        private string name;
        private CodeCommentStatementCollection comments = new CodeCommentStatementCollection();
        private CodeAttributeDeclarationCollection customAttributes = null;
        private CodeLinePragma linePragma;
        
        // Optionally Serializable
        [OptionalField]
        private CodeDirectiveCollection startDirectives = null;
        [OptionalField]        
        private CodeDirectiveCollection endDirectives = null;
        

        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       the name of the member.
        ///    </para>
        /// </devdoc>
        public string Name {
            get {
                return (name == null) ? string.Empty : name;
            }
            set {
                name = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a <see cref='System.CodeDom.MemberAttributes'/> indicating
        ///       the attributes of the member.
        ///    </para>
        /// </devdoc>
        public MemberAttributes Attributes {
            get {
                return attributes;
            }
            set {
                attributes = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a <see cref='System.CodeDom.CodeAttributeDeclarationCollection'/> indicating
        ///       the custom attributes of the
        ///       member.
        ///    </para>
        /// </devdoc>
        public CodeAttributeDeclarationCollection CustomAttributes {
            get {
                if (customAttributes == null) {
                    customAttributes = new CodeAttributeDeclarationCollection();
                }
                return customAttributes;
            }
            set {
                customAttributes = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       The line the statement occurs on.
        ///    </para>
        /// </devdoc>
        public CodeLinePragma LinePragma {
            get {
                return linePragma;
            }
            set {
                linePragma = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the member comment collection members.
        ///    </para>
        /// </devdoc>
        public CodeCommentStatementCollection Comments {
            get {
                return comments;
            }
        }
        
        public CodeDirectiveCollection StartDirectives {
            get {
                if (startDirectives == null) {
                    startDirectives = new CodeDirectiveCollection();
                }
                return startDirectives;                
            }
        }

        public CodeDirectiveCollection EndDirectives {
            get {
                if (endDirectives == null) {
                    endDirectives = new CodeDirectiveCollection();
                }
                return endDirectives ;                
            }
        }        
    }
}

