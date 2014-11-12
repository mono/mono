//------------------------------------------------------------------------------
// <copyright file="XmlMembersMapping.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {
    using System.Reflection;
    using System;
    using System.Text;

    /// <include file='doc\XmlMembersMapping.uex' path='docs/doc[@for="XmlMembersMapping"]/*' />
    ///<internalonly/>
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlMembersMapping : XmlMapping {
        XmlMemberMapping[] mappings;

        internal XmlMembersMapping(TypeScope scope, ElementAccessor accessor, XmlMappingAccess access) : base(scope, accessor, access) {
            MembersMapping mapping = (MembersMapping)accessor.Mapping;
            StringBuilder key = new StringBuilder();
            key.Append(":");
            mappings = new XmlMemberMapping[mapping.Members.Length];
            for (int i = 0; i < mappings.Length; i++) {
                if (mapping.Members[i].TypeDesc.Type != null) {
                    key.Append(GenerateKey(mapping.Members[i].TypeDesc.Type, null, null));
                    key.Append(":");
                }
                mappings[i] = new XmlMemberMapping(mapping.Members[i]);
            }
            SetKeyInternal(key.ToString());
        }

        /// <include file='doc\XmlMembersMapping.uex' path='docs/doc[@for="XmlMembersMapping.TypeName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string TypeName {
            get { return Accessor.Mapping.TypeName; }
        }

        /// <include file='doc\XmlMembersMapping.uex' path='docs/doc[@for="XmlMembersMapping.TypeNamespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string TypeNamespace {
            get { return Accessor.Mapping.Namespace; }
        }

        /// <include file='doc\XmlMembersMapping.uex' path='docs/doc[@for="XmlMembersMapping.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlMemberMapping this[int index] {
            get { return mappings[index]; }
        }

        /// <include file='doc\XmlMembersMapping.uex' path='docs/doc[@for="XmlMembersMapping.Count"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Count {
            get { return mappings.Length; }
        }
    }
}
