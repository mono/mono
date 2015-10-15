//------------------------------------------------------------------------------
// <copyright file="XmlQualifiedName.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml {
    using System.Collections;
    using System.Diagnostics;
#if !SILVERLIGHT
    using Microsoft.Win32;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
#endif

    /// <include file='doc\XmlQualifiedName.uex' path='docs/doc[@for="XmlQualifiedName"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class XmlQualifiedName {
#if !SILVERLIGHT
        delegate int HashCodeOfStringDelegate(string s, int sLen, long additionalEntropy);
        static HashCodeOfStringDelegate hashCodeDelegate = null;
#endif
        string name;
        string ns;

#if !SILVERLIGHT
        [NonSerialized]
#endif
        Int32  hash;

        /// <include file='doc\XmlQualifiedName.uex' path='docs/doc[@for="XmlQualifiedName.Empty"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static readonly XmlQualifiedName Empty = new XmlQualifiedName(string.Empty);

        /// <include file='doc\XmlQualifiedName.uex' path='docs/doc[@for="XmlQualifiedName.XmlQualifiedName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlQualifiedName() : this(string.Empty, string.Empty) {}

        /// <include file='doc\XmlQualifiedName.uex' path='docs/doc[@for="XmlQualifiedName.XmlQualifiedName1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlQualifiedName(string name) : this(name, string.Empty) {}

        /// <include file='doc\XmlQualifiedName.uex' path='docs/doc[@for="XmlQualifiedName.XmlQualifiedName2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlQualifiedName(string name, string ns) {
            this.ns   = ns   == null ? string.Empty : ns;
            this.name = name == null ? string.Empty : name;
        }

        /// <include file='doc\XmlQualifiedName.uex' path='docs/doc[@for="XmlQualifiedName.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Namespace {
            get { return ns; }
        }

        /// <include file='doc\XmlQualifiedName.uex' path='docs/doc[@for="XmlQualifiedName.Name"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Name {
            get { return name; }
        }

        /// <include file='doc\XmlQualifiedName.uex' path='docs/doc[@for="XmlQualifiedName.GetHashCode"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override int GetHashCode() {
            if(hash == 0) {
#if !SILVERLIGHT
                if (hashCodeDelegate == null) {
                    hashCodeDelegate = GetHashCodeDelegate();
                }

                hash = hashCodeDelegate(Name, Name.Length, 0);
#else

                hash = Name.GetHashCode() /*+ Namespace.GetHashCode()*/; // for perf reasons we are not taking ns's hashcode.
#endif
            }
            return hash;
        }

        /// <include file='doc\XmlQualifiedName.uex' path='docs/doc[@for="XmlQualifiedName.IsEmpty"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsEmpty {
            get { return Name.Length == 0 && Namespace.Length == 0; }
        }

        /// <include file='doc\XmlQualifiedName.uex' path='docs/doc[@for="XmlQualifiedName.ToString"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string ToString() {
            return Namespace.Length == 0 ? Name : string.Concat(Namespace, ":" , Name);
        }

        /// <include file='doc\XmlQualifiedName.uex' path='docs/doc[@for="XmlQualifiedName.Equals"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override bool Equals(object other) {
            XmlQualifiedName qname;

            if ((object) this == other) {
                return true;
            }

            qname = other as XmlQualifiedName;
            if (qname != null) {
                return (Name == qname.Name && Namespace == qname.Namespace);
            }
            return false;
        }

        /// <include file='doc\XmlQualifiedName.uex' path='docs/doc[@for="XmlQualifiedName.operator=="]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static bool operator ==(XmlQualifiedName a, XmlQualifiedName b) {
            if ((object) a == (object) b)
                return true;

            if ((object) a == null || (object) b == null)
                return false;

            return (a.Name == b.Name && a.Namespace == b.Namespace);
        }

        /// <include file='doc\XmlQualifiedName.uex' path='docs/doc[@for="XmlQualifiedName.operator!="]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static bool operator !=(XmlQualifiedName a, XmlQualifiedName b) {
            return !(a == b);
        }

        /// <include file='doc\XmlQualifiedName.uex' path='docs/doc[@for="XmlQualifiedName.ToString1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static string ToString(string name, string ns) {
            return ns == null || ns.Length == 0 ? name : ns + ":" + name;
        }

#if !SILVERLIGHT // These methods are not used in Silverlight
        [SecuritySafeCritical]
        [ReflectionPermission(SecurityAction.Assert, Unrestricted = true)]
        private static HashCodeOfStringDelegate GetHashCodeDelegate() {
             // If we are using randomized hashing and we find the Marving hash method, we use that
             // Otherwise, we use the old string hashing function.
 
             if (!IsRandomizedHashingDisabled())
             {
                MethodInfo getHashCodeMethodInfo = typeof(String).GetMethod("InternalMarvin32HashString", BindingFlags.NonPublic | BindingFlags.Static);
                 if (getHashCodeMethodInfo != null)
                 {
                    return (HashCodeOfStringDelegate)Delegate.CreateDelegate(typeof(HashCodeOfStringDelegate), getHashCodeMethodInfo);
                 }
                // This will fall through and return a delegate to the old hash function
                Debug.Assert(false, "Randomized hashing is not supported.");
            }
            return new HashCodeOfStringDelegate(GetHashCodeOfString);
        }

        [SecuritySafeCritical]
        [RegistryPermission(SecurityAction.Assert, Unrestricted = true)]
        private static bool IsRandomizedHashingDisabled() {
            const string regValueName = "DisableRandomizedHashingOnXmlQualifiedName";
            bool disableHashing = false; // default value
            if (!ReadBoolFromXmlRegistrySettings(Registry.CurrentUser, regValueName, ref disableHashing)) {
                ReadBoolFromXmlRegistrySettings(Registry.LocalMachine, regValueName, ref disableHashing);
            }
            return disableHashing;
        }

        [SecurityCritical]
        private static bool ReadBoolFromXmlRegistrySettings(RegistryKey hive, string regValueName, ref bool value) {
            const string regValuePath = @"SOFTWARE\Microsoft\.NETFramework\XML";
            try {
                using (RegistryKey xmlRegKey = hive.OpenSubKey(regValuePath, false)) {
                    if (xmlRegKey != null) {
                        if (xmlRegKey.GetValueKind(regValueName) == RegistryValueKind.DWord) {
                            value = ((int)xmlRegKey.GetValue(regValueName)) == 1;
                            return true;
                        }
                    }
                }
            }
            catch { /* use the default if we couldn't read the key */ }
            return false;
        }

        private static int GetHashCodeOfString(string s, int length, long additionalEntropy)
        {
            // This is the fallback method for calling the regular hashcode method
            return s.GetHashCode();
        }
		
        // --------- Some useful internal stuff -----------------
        internal void Init(string name, string ns) {
            Debug.Assert(name != null && ns != null);
            this.name = name;
            this.ns = ns;
            this.hash = 0;
        }
        
        internal void SetNamespace(string ns) {
            Debug.Assert(ns != null);
            this.ns = ns; //Not changing hash since ns is not used to compute hashcode
        }

        internal void Verify() {
            XmlConvert.VerifyNCName(name);
            if (ns.Length != 0) {
                XmlConvert.ToUri(ns);
            }
        }

        internal void Atomize(XmlNameTable nameTable) {
            Debug.Assert(name != null);
            name = nameTable.Add(name);
            ns = nameTable.Add(ns);
        }

        internal static XmlQualifiedName Parse(string s, IXmlNamespaceResolver nsmgr, out string prefix) {
            string localName;
            ValidateNames.ParseQNameThrow(s, out prefix, out localName);

            string uri = nsmgr.LookupNamespace(prefix);
            if (uri == null) {
                if (prefix.Length != 0) {
                    throw new XmlException(Res.Xml_UnknownNs, prefix);
                }
                else { //Re-map namespace of empty prefix to string.Empty when there is no default namespace declared
                    uri = string.Empty;
                }
            }
            return new XmlQualifiedName(localName, uri);
        }

        internal XmlQualifiedName Clone() {
            return (XmlQualifiedName)MemberwiseClone();
        }

        internal static int Compare(XmlQualifiedName a, XmlQualifiedName b) {
            if (null == a) {
                return (null == b) ? 0 : -1;
            }   
            if (null == b) {
                return 1;
            }
            int i = String.CompareOrdinal(a.Namespace, b.Namespace);
            if (i == 0) {
                i = String.CompareOrdinal(a.Name, b.Name);
            }
            return i;
        }
#endif
    }
}

