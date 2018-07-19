//------------------------------------------------------------------------------
// <copyright file="CodeAttributeDeclaration.cs" company="Microsoft">
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
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;    

    /// <devdoc>
    ///    <para>
    ///       Represents a single custom attribute.
    ///    </para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeAttributeDeclaration {
        private string name;
        private CodeAttributeArgumentCollection arguments = new CodeAttributeArgumentCollection();
        [OptionalField] 
        private CodeTypeReference attributeType;
        
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeAttributeDeclaration'/>.
        ///    </para>
        /// </devdoc>
        public CodeAttributeDeclaration() {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeAttributeDeclaration'/> using the specified name.
        ///    </para>
        /// </devdoc>
        public CodeAttributeDeclaration(string name) {
            Name = name;
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeAttributeDeclaration'/> using the specified
        ///       arguments.
        ///    </para>
        /// </devdoc>
        public CodeAttributeDeclaration(string name, params CodeAttributeArgument[] arguments) {
            Name = name;
            Arguments.AddRange(arguments);
        }

        public CodeAttributeDeclaration(CodeTypeReference attributeType) : this ( attributeType, null) {
        }

        public CodeAttributeDeclaration(CodeTypeReference attributeType, params CodeAttributeArgument[] arguments) {
            this.attributeType = attributeType;                
            if( attributeType != null) {
                this.name = attributeType.BaseType;
            }

            if(arguments != null) {
                Arguments.AddRange(arguments);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       The name of the attribute being declared.
        ///    </para>
        /// </devdoc>
        public string Name {
            get {
                return (name == null) ? string.Empty : name;
            }
            set {
                name = value;
                attributeType = new CodeTypeReference(name);                
            }
        }

        /// <devdoc>
        ///    <para>
        ///       The arguments for the attribute.
        ///    </para>
        /// </devdoc>
        public CodeAttributeArgumentCollection Arguments {
            get {
                return arguments;
            }
        }

        public CodeTypeReference AttributeType {
            get {
                return attributeType;
            }
        }
    }
}
