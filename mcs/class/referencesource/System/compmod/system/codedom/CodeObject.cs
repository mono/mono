//------------------------------------------------------------------------------
// <copyright file="CodeObject.cs" company="Microsoft">
// 
// <OWNER>Microsoft</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom {

    using System.Collections;
    using System.Collections.Specialized;
    using System.Runtime.Serialization;
    using System.Runtime.InteropServices;

    /// <devdoc>
    ///    <para>
    ///       The base class for CodeDom objects
    ///    </para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeObject {
        private IDictionary userData = null;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CodeObject() {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public IDictionary UserData {
            get {
                if (userData == null) {
                    userData = new ListDictionary();
                }
                return userData;
            }
        }
    }
}
