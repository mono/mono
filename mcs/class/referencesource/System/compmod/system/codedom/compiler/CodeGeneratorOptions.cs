//------------------------------------------------------------------------------
// <copyright file="CodeGeneratorOptions.cs" company="Microsoft">
// 
// <OWNER>[....]</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom.Compiler {
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para>
    ///       Represents options used in code generation
    ///    </para>
    /// </devdoc>
    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class CodeGeneratorOptions {
        private IDictionary options = new ListDictionary();

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CodeGeneratorOptions() {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public object this[string index] {
            get {
                return options[index];
            }
            set {
                options[index] = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string IndentString {
            get {
                object o = options["IndentString"];
                return ((o == null) ? "    " : (string)o);
            }
            set {
                options["IndentString"] = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string BracingStyle {
            get {
                object o = options["BracingStyle"];
                return ((o == null) ? "Block" : (string)o);
            }
            set {
                options["BracingStyle"] = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool ElseOnClosing {
            get {
                object o = options["ElseOnClosing"];
                return ((o == null) ? false : (bool)o);
            }
            set {
                options["ElseOnClosing"] = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool BlankLinesBetweenMembers {
            get {
                object o = options["BlankLinesBetweenMembers"];
                return ((o == null) ? true : (bool)o);
            }
            set {
                options["BlankLinesBetweenMembers"] = value;
            }
        }

        [System.Runtime.InteropServices.ComVisible(false)]
        public bool VerbatimOrder {
            get {
                object o = options["VerbatimOrder"];
                return ((o == null) ? false : (bool)o);
            }
            set {
                options["VerbatimOrder"] = value;
            }
        }
    }
}
