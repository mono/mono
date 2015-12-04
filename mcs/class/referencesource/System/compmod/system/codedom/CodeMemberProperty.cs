//------------------------------------------------------------------------------
// <copyright file="CodeMemberProperty.cs" company="Microsoft">
// 
// <OWNER>[....]</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom {

    using System.Diagnostics;
    using System;
    using Microsoft.Win32;
    using System.Collections;
    using System.Runtime.InteropServices;

    /// <devdoc>
    ///    <para>
    ///       Represents a class property.
    ///    </para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeMemberProperty : CodeTypeMember {
        private CodeTypeReference type;
        private CodeParameterDeclarationExpressionCollection parameters = new CodeParameterDeclarationExpressionCollection();
        private bool hasGet;
        private bool hasSet;
        private CodeStatementCollection getStatements = new CodeStatementCollection();
        private CodeStatementCollection setStatements = new CodeStatementCollection();
        private CodeTypeReference privateImplements = null;
        private CodeTypeReferenceCollection implementationTypes = null;
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CodeTypeReference PrivateImplementationType {
            get {
                return privateImplements;
            }
            set {
                privateImplements = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CodeTypeReferenceCollection ImplementationTypes {
            get {
                if (implementationTypes == null) {
                    implementationTypes = new CodeTypeReferenceCollection();
                }
                return implementationTypes;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the data type of the property.</para>
        /// </devdoc>
        public CodeTypeReference Type {
            get {
                if (type == null) {
                    type = new CodeTypeReference("");
                }
                return type;
            }
            set {
                type = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a value
        ///       indicating whether the property has a get method accessor.
        ///    </para>
        /// </devdoc>
        public bool HasGet {
            get {
                return hasGet || getStatements.Count > 0;
            }
            set {
                hasGet = value;
                if (!value) {
                    getStatements.Clear();
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a value
        ///       indicating whether the property has a set method accessor.
        ///    </para>
        /// </devdoc>
        public bool HasSet {
            get {
                return hasSet || setStatements.Count > 0;
            }
            set {
                hasSet = value;
                if (!value) {
                    setStatements.Clear();
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the collection of get statements for the
        ///       property.
        ///    </para>
        /// </devdoc>
        public CodeStatementCollection GetStatements {
            get {
                return getStatements;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the collection of get statements for the property.
        ///    </para>
        /// </devdoc>
        public CodeStatementCollection SetStatements {
            get {
                return setStatements;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the collection of declaration expressions
        ///       for
        ///       the property.
        ///    </para>
        /// </devdoc>
        public CodeParameterDeclarationExpressionCollection Parameters {
            get {
                return parameters;
            }
        }
    }
}
