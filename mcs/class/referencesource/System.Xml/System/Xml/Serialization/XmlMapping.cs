//------------------------------------------------------------------------------
// <copyright file="XmlMapping.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {
    using System;
    using System.ComponentModel;
    using System.Globalization;

    [Flags]
    public enum XmlMappingAccess {
        None = 0x00,
        Read = 0x01,
        Write = 0x02,        
    }

    /// <include file='doc\XmlMapping.uex' path='docs/doc[@for="XmlMapping"]/*' />
    ///<internalonly/>
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public abstract class XmlMapping {
        TypeScope scope;
        bool generateSerializer = false;
        bool isSoap;
        ElementAccessor accessor;
        string key;
        bool shallow = false;
        XmlMappingAccess access;

        internal XmlMapping(TypeScope scope, ElementAccessor accessor) : this(scope, accessor, XmlMappingAccess.Read | XmlMappingAccess.Write){
        }

        internal XmlMapping(TypeScope scope, ElementAccessor accessor, XmlMappingAccess access) {
            this.scope = scope;
            this.accessor = accessor;
            this.access = access;
            this.shallow = scope == null;
        }

        internal ElementAccessor Accessor {
            get { return accessor; }
        }

        internal TypeScope Scope {
            get { return scope; }
        }

        /// <include file='doc\XmlMapping.uex' path='docs/doc[@for="XmlMapping.ElementName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string ElementName { 
            get { return System.Xml.Serialization.Accessor.UnescapeName(Accessor.Name); }
        }

        /// <include file='doc\XmlMapping.uex' path='docs/doc[@for="XmlMapping.XsdElementName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string XsdElementName { 
            get { return Accessor.Name; }
        }

        /// <include file='doc\XmlMapping.uex' path='docs/doc[@for="XmlMapping.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Namespace {
            get { return accessor.Namespace; }
        }

        internal bool GenerateSerializer {
            get { return generateSerializer; }
            set { generateSerializer = value; }
        }

        internal bool IsReadable {
            get { return ((access & XmlMappingAccess.Read) != 0); }
        }

        internal bool IsWriteable {
            get { return ((access & XmlMappingAccess.Write) != 0); }
        }

        internal bool IsSoap {
            get { return isSoap; }
            set { isSoap = value; }
        }

        /// <include file='doc\XmlMapping.uex' path='docs/doc[@for="XmlMapping.SetKey"]/*' />
        ///<internalonly/>
        public void SetKey(string key){
            SetKeyInternal(key);
        }

        /// <include file='doc\XmlMapping.uex' path='docs/doc[@for="XmlMapping.SetKeyInternal"]/*' />
        ///<internalonly/>
        internal void SetKeyInternal(string key){
            this.key = key;
        }

        internal static string GenerateKey(Type type, XmlRootAttribute root, string ns) {
            if (root == null) {
                root = (XmlRootAttribute)XmlAttributes.GetAttr(type, typeof(XmlRootAttribute));
            }
            return type.FullName + ":" + (root == null ? String.Empty : root.Key) + ":" + (ns == null ? String.Empty : ns);
        }

        internal string Key { get { return key; } }
        internal void CheckShallow() {
            if (shallow) {
                throw new InvalidOperationException(Res.GetString(Res.XmlMelformMapping)); 
            }
        }
        internal static bool IsShallow(XmlMapping[] mappings) {
            for (int i = 0; i < mappings.Length; i++) {
                if (mappings[i] == null || mappings[i].shallow)
                    return true;
            }
            return false;
        }
    }
}
