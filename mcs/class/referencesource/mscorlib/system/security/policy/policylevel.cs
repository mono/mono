// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// PolicyLevel.cs
//
// Abstraction for a level of policy (e.g. Enterprise, Machine, User)
//

namespace System.Security.Policy {
    using Microsoft.Win32;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Security.Util;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization;
    using System.Runtime.Versioning;
    using System.Text;
    using System.Threading;
    using System.Diagnostics.Contracts;
#if !FEATURE_PAL    
    using System.Runtime.Hosting;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.Deployment.Internal.Isolation;
#endif
    // Duplicated in vm\SecurityConfig.h
    [Serializable]
    internal enum ConfigId {
        None                    = 0,
        MachinePolicyLevel      = 1,
        UserPolicyLevel         = 2,
        EnterprisePolicyLevel   = 3,
    }

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    sealed public class PolicyLevel {
        private ArrayList m_fullTrustAssemblies;
        private ArrayList m_namedPermissionSets;
        private CodeGroup m_rootCodeGroup;
        private string m_label;
        [OptionalField(VersionAdded = 2)]
        private PolicyLevelType m_type;

        // Following fields are present purely for serialization compatability with Everett: not used in Whidbey
#pragma warning disable 169
        private ConfigId m_configId;
        private bool m_useDefaultCodeGroupsOnReset;
        private bool m_generateQuickCacheOnLoad;
        private bool m_caching;
        private bool m_throwOnLoadError;
        private Encoding m_encoding;
#pragma warning restore 169

        private bool m_loaded;
        private SecurityElement m_permSetElement;
        private string m_path;

        static PolicyLevel()
        {
        }

        private static Object s_InternalSyncObject;
        private static Object InternalSyncObject {
            get {
                if (s_InternalSyncObject == null) {
                    Object o = new Object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, o, null);
                }
                return s_InternalSyncObject;
            }
        }

        private static readonly string[] s_reservedNamedPermissionSets = {
            "FullTrust",
            "Nothing",
            "Execution",
            "SkipVerification",
            "Internet",
            "LocalIntranet",
            "Everything"
        };

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx) {
            // If label != null, then we know that we can derive the type from that. In Whidbey, we might be doing unnecessary work here
            if (m_label != null)
                DeriveTypeFromLabel();
        }

        private void DeriveTypeFromLabel() {
            if(m_label.Equals(Environment.GetResourceString("Policy_PL_User")))
                m_type = System.Security.PolicyLevelType.User;
            else if(m_label.Equals(Environment.GetResourceString("Policy_PL_Machine")))
                m_type = System.Security.PolicyLevelType.Machine;
            else if(m_label.Equals(Environment.GetResourceString("Policy_PL_Enterprise")))
                m_type = System.Security.PolicyLevelType.Enterprise;
            else if(m_label.Equals(Environment.GetResourceString("Policy_PL_AppDomain")))
                m_type = System.Security.PolicyLevelType.AppDomain;
            else
                throw new ArgumentException(Environment.GetResourceString("Policy_Default"));
        }

        private string DeriveLabelFromType() {
            switch(m_type) {
            case System.Security.PolicyLevelType.User:
                return Environment.GetResourceString("Policy_PL_User");
            case System.Security.PolicyLevelType.Machine:
                return Environment.GetResourceString("Policy_PL_Machine");
            case System.Security.PolicyLevelType.Enterprise:
                return Environment.GetResourceString("Policy_PL_Enterprise");
            case System.Security.PolicyLevelType.AppDomain:
                return Environment.GetResourceString("Policy_PL_AppDomain");
            default:
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", (int)m_type));
            }
        }

        //
        // Constructors.
        //   No public constructors are exposed. CreateAppDomainLevel is the only public API to create
        //   an AppDomain policy level, and it ensures it is an AppDomain policy level.
        //

        private PolicyLevel() {}

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal PolicyLevel (PolicyLevelType type) : this (type, GetLocationFromType(type)) {}
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal PolicyLevel (PolicyLevelType type, string path) : this (type, path, ConfigId.None) {}
        [ResourceExposure(ResourceScope.Machine)]
        internal PolicyLevel (PolicyLevelType type, string path, ConfigId configId) {
            m_type = type;
            m_path = path;
            m_loaded = (path == null);
            if (m_path == null) {
                m_rootCodeGroup = CreateDefaultAllGroup();
                SetFactoryPermissionSets();
                SetDefaultFullTrustAssemblies();
            }
            m_configId = configId;
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static string GetLocationFromType (PolicyLevelType type) {
            switch (type) {
            case PolicyLevelType.User:
                return Config.UserDirectory + "security.config";
            case PolicyLevelType.Machine:
                return Config.MachineDirectory + "security.config";
            case PolicyLevelType.Enterprise:
                return Config.MachineDirectory + "enterprisesec.config";
            default:
                return null;
            }
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [Obsolete("AppDomain policy levels are obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public static PolicyLevel CreateAppDomainLevel() {
            return new PolicyLevel(System.Security.PolicyLevelType.AppDomain);
        }

        public string Label {
            get {
                if (m_label == null)
                    m_label = DeriveLabelFromType();
                return m_label;
            }
        }

        //
        // Public properties and methods.
        //

        [ComVisible(false)]
        public PolicyLevelType Type {
            get {
                return m_type;
            }
        }

        internal ConfigId ConfigId {
            get {
                return m_configId;
            }
        }

        internal string Path {
            [ResourceExposure(ResourceScope.Machine)]
            get {
                return m_path;
            }
        }

        public string StoreLocation {
            [System.Security.SecuritySafeCritical]  // auto-generated
            [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPolicy)]
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            get {
                return GetLocationFromType(m_type);
            }
        }

        public CodeGroup RootCodeGroup {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get {
                CheckLoaded();
                return m_rootCodeGroup;
            }
            [System.Security.SecuritySafeCritical]  // auto-generated
            set {
                if (value == null)
                    throw new ArgumentNullException("RootCodeGroup");
                Contract.EndContractBlock();

                CheckLoaded();
                m_rootCodeGroup = value.Copy();
            }
        }

        public IList NamedPermissionSets {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get {
                CheckLoaded();
                LoadAllPermissionSets();

                ArrayList newList = new ArrayList(m_namedPermissionSets.Count);

                IEnumerator enumerator = m_namedPermissionSets.GetEnumerator();
                while (enumerator.MoveNext()) {
                    newList.Add(((NamedPermissionSet)enumerator.Current).Copy());
                }

                return newList;
            }
        }

        public CodeGroup ResolveMatchingCodeGroups(Evidence evidence) {
            if (evidence == null)
                throw new ArgumentNullException("evidence");
            Contract.EndContractBlock();

            return this.RootCodeGroup.ResolveMatchingCodeGroups(evidence);
        }

        [Obsolete("Because all GAC assemblies always get full trust, the full trust list is no longer meaningful. You should install any assemblies that are used in security policy in the GAC to ensure they are trusted.")]
        public void AddFullTrustAssembly(StrongName sn) {
            if (sn == null)
                throw new ArgumentNullException("sn");
            Contract.EndContractBlock();

            AddFullTrustAssembly(new StrongNameMembershipCondition(sn.PublicKey, sn.Name, sn.Version));
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [Obsolete("Because all GAC assemblies always get full trust, the full trust list is no longer meaningful. You should install any assemblies that are used in security policy in the GAC to ensure they are trusted.")]
        public void AddFullTrustAssembly(StrongNameMembershipCondition snMC) {
            if (snMC == null)
                throw new ArgumentNullException("snMC");
            Contract.EndContractBlock();

            CheckLoaded();

            IEnumerator enumerator = m_fullTrustAssemblies.GetEnumerator();
            while (enumerator.MoveNext()) {
                if (((StrongNameMembershipCondition)enumerator.Current).Equals(snMC))
                    throw new ArgumentException(Environment.GetResourceString("Argument_AssemblyAlreadyFullTrust"));
            }

            lock (m_fullTrustAssemblies) {
                m_fullTrustAssemblies.Add(snMC);
            }
        }

        [Obsolete("Because all GAC assemblies always get full trust, the full trust list is no longer meaningful. You should install any assemblies that are used in security policy in the GAC to ensure they are trusted.")]
        public void RemoveFullTrustAssembly(StrongName sn) {
            if (sn == null)
                throw new ArgumentNullException("assembly");
            Contract.EndContractBlock();

            RemoveFullTrustAssembly(new StrongNameMembershipCondition(sn.PublicKey, sn.Name, sn.Version));
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        [Obsolete("Because all GAC assemblies always get full trust, the full trust list is no longer meaningful. You should install any assemblies that are used in security policy in the GAC to ensure they are trusted.")]
        public void RemoveFullTrustAssembly(StrongNameMembershipCondition snMC) {
            if (snMC == null)
                throw new ArgumentNullException("snMC");
            Contract.EndContractBlock();

            CheckLoaded();

            Object toRemove = null;
            IEnumerator enumerator = m_fullTrustAssemblies.GetEnumerator();

            while (enumerator.MoveNext()) {
                if (((StrongNameMembershipCondition)enumerator.Current).Equals(snMC)) {
                    toRemove = enumerator.Current;
                    break;
                }
            }

            if (toRemove == null)
                throw new ArgumentException(Environment.GetResourceString("Argument_AssemblyNotFullTrust"));

            lock (m_fullTrustAssemblies) {
                m_fullTrustAssemblies.Remove(toRemove);
            }
        }

        [Obsolete("Because all GAC assemblies always get full trust, the full trust list is no longer meaningful. You should install any assemblies that are used in security policy in the GAC to ensure they are trusted.")]
        public IList FullTrustAssemblies {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get {
                CheckLoaded();
                return new ArrayList(m_fullTrustAssemblies);
            }
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public void AddNamedPermissionSet(NamedPermissionSet permSet) {
            if (permSet == null)
                throw new ArgumentNullException("permSet");
            Contract.EndContractBlock();

            CheckLoaded();
            LoadAllPermissionSets();

            lock (this) {
                IEnumerator enumerator = m_namedPermissionSets.GetEnumerator();
                while (enumerator.MoveNext()) {
                    if (((NamedPermissionSet)enumerator.Current).Name.Equals(permSet.Name))
                        throw new ArgumentException(Environment.GetResourceString("Argument_DuplicateName"));
                }

                NamedPermissionSet npsCopy = (NamedPermissionSet)permSet.Copy();
                npsCopy.IgnoreTypeLoadFailures = true;
                m_namedPermissionSets.Add(npsCopy);
            }
        }

        public NamedPermissionSet RemoveNamedPermissionSet(NamedPermissionSet permSet) {
            if (permSet == null)
                throw new ArgumentNullException("permSet");
            Contract.EndContractBlock();

            return RemoveNamedPermissionSet(permSet.Name);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public NamedPermissionSet RemoveNamedPermissionSet(string name) {
            if (name == null)
                throw new ArgumentNullException("name");
            Contract.EndContractBlock();

            CheckLoaded();
            LoadAllPermissionSets();

            int permSetIndex = -1;

            // First, make sure it's not a reserved permission set.
            for (int index = 0; index < s_reservedNamedPermissionSets.Length; ++index) {
                if (s_reservedNamedPermissionSets[index].Equals(name))
                    throw new ArgumentException(Environment.GetResourceString("Argument_ReservedNPMS", name));
            }

            // Then, find out if a named permission set of that name exists
            // and remember its index;

            ArrayList namedPermissionSets = m_namedPermissionSets;

            for (int index = 0; index < namedPermissionSets.Count; ++index) {
                if (((NamedPermissionSet)namedPermissionSets[index]).Name.Equals(name)) {
                    permSetIndex = index;
                    break;
                }
            }

            if (permSetIndex == -1)
                throw new ArgumentException(Environment.GetResourceString("Argument_NoNPMS"));

            // Now, as best as we can in the face of custom CodeGroups figure
            // out if the permission set is in use. If it is we don't allow
            // it to be removed.

            ArrayList groups = new ArrayList();
            groups.Add(this.m_rootCodeGroup);

            for (int index = 0; index < groups.Count; ++index) {
                CodeGroup group = (CodeGroup)groups[index];

                if (group.PermissionSetName != null && group.PermissionSetName.Equals(name)) {
                    throw new ArgumentException(Environment.GetResourceString("Argument_NPMSInUse", name));
                }

                IEnumerator childEnumerator = group.Children.GetEnumerator();

                if (childEnumerator != null) {
                    while (childEnumerator.MoveNext()) {
                        groups.Add(childEnumerator.Current);
                    }
                }
            }

            NamedPermissionSet permSet = (NamedPermissionSet)namedPermissionSets[permSetIndex];
            namedPermissionSets.RemoveAt(permSetIndex);
            return permSet;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public NamedPermissionSet ChangeNamedPermissionSet(string name, PermissionSet pSet) {
            if (name == null)
                throw new ArgumentNullException("name");
            if (pSet == null)
                throw new ArgumentNullException("pSet");
            Contract.EndContractBlock();

            // First, make sure it's not a reserved permission set.
            for (int index = 0; index < s_reservedNamedPermissionSets.Length; ++index) {
                if (s_reservedNamedPermissionSets[index].Equals(name))
                    throw new ArgumentException(Environment.GetResourceString("Argument_ReservedNPMS", name));
            }

            // Get the current permission set (don't copy it).
            NamedPermissionSet currentPSet = GetNamedPermissionSetInternal(name);

            // If the permission set doesn't exist, throw an argument exception
            if (currentPSet == null)
                throw new ArgumentException(Environment.GetResourceString("Argument_NoNPMS"));

            // Copy the current permission set so that we can return it.
            NamedPermissionSet retval = (NamedPermissionSet)currentPSet.Copy();

            // Reset the permission set
            currentPSet.Reset();
            currentPSet.SetUnrestricted(pSet.IsUnrestricted());

            IEnumerator enumerator = pSet.GetEnumerator();
            while (enumerator.MoveNext()) {
                currentPSet.SetPermission(((IPermission)enumerator.Current).Copy());
            }

            if (pSet is NamedPermissionSet) {
                currentPSet.Description = ((NamedPermissionSet)pSet).Description;
            }

            return retval;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public NamedPermissionSet GetNamedPermissionSet(string name) {
            if (name == null)
                throw new ArgumentNullException("name");
            Contract.EndContractBlock();

            NamedPermissionSet permSet = GetNamedPermissionSetInternal(name);

            // Copy it so that no corruption can occur.
            if (permSet != null)
                return new NamedPermissionSet(permSet);
            else
                return null;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public void Recover() {
            if (m_configId == ConfigId.None)
                throw new PolicyException(Environment.GetResourceString("Policy_RecoverNotFileBased"));

            lock (this) {
                // This call will safely swap the files.
                if (!Config.RecoverData(m_configId))
                    throw new PolicyException(Environment.GetResourceString("Policy_RecoverNoConfigFile"));

                // Now we need to blank out the level
                m_loaded = false;
                m_rootCodeGroup = null;
                m_namedPermissionSets = null;
                m_fullTrustAssemblies = new ArrayList();
            }
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public void Reset() {
            SetDefault();
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public PolicyStatement Resolve(Evidence evidence) {
            return Resolve(evidence, 0, null);
        }

        [System.Security.SecuritySafeCritical]  // auto-generated
        public SecurityElement ToXml() {
            // Make sure we have loaded everything and that all the
            // permission sets are loaded.

            CheckLoaded();
            LoadAllPermissionSets();

            IEnumerator enumerator;
            SecurityElement e = new SecurityElement("PolicyLevel");
            e.AddAttribute("version", "1");

            Hashtable classes = new Hashtable();
            lock (this) {
                SecurityElement elPermSets = new SecurityElement("NamedPermissionSets");
                enumerator = m_namedPermissionSets.GetEnumerator();
                while (enumerator.MoveNext()) {
                    elPermSets.AddChild(NormalizeClassDeep(((NamedPermissionSet)enumerator.Current).ToXml(), classes));
                }

                SecurityElement elCodeGroup = NormalizeClassDeep(m_rootCodeGroup.ToXml(this), classes);

                SecurityElement elFullTrust = new SecurityElement("FullTrustAssemblies");
                enumerator = m_fullTrustAssemblies.GetEnumerator();
                while (enumerator.MoveNext()) {
                    elFullTrust.AddChild(NormalizeClassDeep(((StrongNameMembershipCondition)enumerator.Current).ToXml(), classes));
                }

                SecurityElement elClasses = new SecurityElement("SecurityClasses");
                IDictionaryEnumerator dicEnumerator = classes.GetEnumerator();
                while (dicEnumerator.MoveNext()) {
                    SecurityElement elClass = new SecurityElement("SecurityClass");
                    elClass.AddAttribute("Name", (string)dicEnumerator.Value);
                    elClass.AddAttribute("Description", (string)dicEnumerator.Key);
                    elClasses.AddChild(elClass);
                }

                e.AddChild(elClasses);
                e.AddChild(elPermSets);
                e.AddChild(elCodeGroup);
                e.AddChild(elFullTrust);
            }

            return e;
        }

        public void FromXml(SecurityElement e) {
            if (e == null)
                throw new ArgumentNullException("e");
            Contract.EndContractBlock();

            Hashtable classes;
            lock (this) {
                ArrayList fullTrustAssemblies = new ArrayList();

                SecurityElement eClasses = e.SearchForChildByTag("SecurityClasses");
                if (eClasses != null) {
                    classes = new Hashtable();
                    IEnumerator enumerator = eClasses.Children.GetEnumerator();
                    while (enumerator.MoveNext()) {
                        SecurityElement current = (SecurityElement)enumerator.Current;
                        if (current.Tag.Equals("SecurityClass")) {
                            string name = current.Attribute("Name");
                            string description = current.Attribute("Description");

                            if (name != null && description != null)
                                classes.Add(name, description);
                        }
                    }
                }
                else {
                    classes = null;
                }

                SecurityElement elFullTrust = e.SearchForChildByTag("FullTrustAssemblies");
                if (elFullTrust != null && elFullTrust.InternalChildren != null) {
                    string className = typeof(System.Security.Policy.StrongNameMembershipCondition).AssemblyQualifiedName;

                    IEnumerator enumerator = elFullTrust.Children.GetEnumerator();
                    while (enumerator.MoveNext()) {
                        StrongNameMembershipCondition sn = new StrongNameMembershipCondition();
                        sn.FromXml((SecurityElement)enumerator.Current);
                        fullTrustAssemblies.Add(sn);
                    }
                }

                m_fullTrustAssemblies = fullTrustAssemblies;

                ArrayList namedPermissionSets = new ArrayList();

                SecurityElement elPermSets = e.SearchForChildByTag("NamedPermissionSets");
                SecurityElement permSetElement = null;

                // Here we just find the parent element for the named permission sets and
                // store it so that we can lazily load them later.

                if (elPermSets != null && elPermSets.InternalChildren != null) {
                    permSetElement = UnnormalizeClassDeep(elPermSets, classes);

                    // Call FindElement for each of the reserved sets (this removes their xml from
                    // permSetElement).
                    foreach (string builtInPermissionSet in s_reservedNamedPermissionSets) {
                        FindElement(permSetElement, builtInPermissionSet);
                    }
                }

                if (permSetElement == null)
                    permSetElement = new SecurityElement("NamedPermissionSets");

                // Then we add in the immutable permission sets (this prevents any alterations
                // to them in the XML file from impacting the runtime versions).

                namedPermissionSets.Add(BuiltInPermissionSets.FullTrust);
                namedPermissionSets.Add(BuiltInPermissionSets.Everything);
                namedPermissionSets.Add(BuiltInPermissionSets.SkipVerification);
                namedPermissionSets.Add(BuiltInPermissionSets.Execution);
                namedPermissionSets.Add(BuiltInPermissionSets.Nothing);
                namedPermissionSets.Add(BuiltInPermissionSets.Internet);
                namedPermissionSets.Add(BuiltInPermissionSets.LocalIntranet);

                foreach(PermissionSet ps in namedPermissionSets)
                    ps.IgnoreTypeLoadFailures = true;

                m_namedPermissionSets = namedPermissionSets;
                m_permSetElement = permSetElement;

                // Parse the root code group.
                SecurityElement elCodeGroup = e.SearchForChildByTag("CodeGroup");
                if (elCodeGroup == null)
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidXMLElement",  "CodeGroup", this.GetType().FullName));

                CodeGroup rootCodeGroup = System.Security.Util.XMLUtil.CreateCodeGroup(UnnormalizeClassDeep(elCodeGroup, classes));
                if (rootCodeGroup == null)
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidXMLElement",  "CodeGroup", this.GetType().FullName));

                rootCodeGroup.FromXml(elCodeGroup, this);
                m_rootCodeGroup = rootCodeGroup;
            }
        }

        //
        // Internal methods.
        //

        [System.Security.SecurityCritical]  // auto-generated
        internal static PermissionSet GetBuiltInSet(string name) {
            // Used by PermissionSetAttribute to create one of the built-in,
            // immutable permission sets.

            if (String.IsNullOrEmpty(name)) {
                return null;
            }
            else if (name.Equals("FullTrust")) {
                return BuiltInPermissionSets.FullTrust;
            }
            else if (name.Equals("Nothing")) {
                return BuiltInPermissionSets.Nothing;
            }
            else if (name.Equals("Execution")) {
                return BuiltInPermissionSets.Execution;
            }
            else if (name.Equals("SkipVerification")) {
                return BuiltInPermissionSets.SkipVerification;
            }
            else if (name.Equals("Internet")) {
                return BuiltInPermissionSets.Internet;
            }
            else if (name.Equals("LocalIntranet")) {
                return BuiltInPermissionSets.LocalIntranet;
            }
            else {
                return null;
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal NamedPermissionSet GetNamedPermissionSetInternal(string name) {
            CheckLoaded();

            lock (InternalSyncObject)
            {
                // First, try to find it in the list.
                foreach (NamedPermissionSet permissionSet in m_namedPermissionSets) {
                    if (permissionSet.Name.Equals(name)) { 
                        return permissionSet;
                    }
                }


                // We didn't find it in the list, so if we have a stored element
                // see if it is there.

                if (m_permSetElement != null)
                {
                    SecurityElement elem = FindElement(m_permSetElement, name);
                    if (elem != null)
                    {
                        NamedPermissionSet permSet = new NamedPermissionSet();
                        permSet.Name = name;
                        m_namedPermissionSets.Add(permSet);
                        try
                        {
                            // We play it conservative here and just say that we are loading policy
                            // anytime we have to decode a permission set.
                            permSet.FromXml(elem, false, true);
                        }
                        catch
                        {
                            m_namedPermissionSets.Remove(permSet);
                            return null;
                        }

                        if (permSet.Name != null)
                        {
                            return permSet;
                        }
                        else
                        {
                            m_namedPermissionSets.Remove(permSet);
                        }
                    }
                }
            }

            return null;
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal PolicyStatement Resolve (Evidence evidence, int count, byte[] serializedEvidence) {
            if (evidence == null)
                throw new ArgumentNullException("evidence");
            Contract.EndContractBlock();

            PolicyStatement policy = null;
            if (serializedEvidence != null)
                policy = CheckCache(count, serializedEvidence);

            if (policy == null) {
                CheckLoaded();

                bool allConst;
                bool isFullTrust = m_fullTrustAssemblies != null && IsFullTrustAssembly(m_fullTrustAssemblies, evidence);
                if (isFullTrust) {
                    policy = new PolicyStatement(new PermissionSet(true), PolicyStatementAttribute.Nothing);
                    allConst = true;
                }
                else {
                    ArrayList list = GenericResolve(evidence, out allConst);
                    policy = new PolicyStatement();
                    // This will set the permission set to the empty set.
                    policy.PermissionSet = null;

                    IEnumerator enumerator = list.GetEnumerator();
                    while (enumerator.MoveNext()) {
                        PolicyStatement ps = ((CodeGroupStackFrame)enumerator.Current).policy;
                        if (ps != null) {
                            policy.GetPermissionSetNoCopy().InplaceUnion(ps.GetPermissionSetNoCopy());
                            policy.Attributes |= ps.Attributes;

                            // If we find a policy statement that's dependent upon unverified evidence, we
                            // need to mark that as used so that the VM can potentially force verification on
                            // the evidence.
                            if (ps.HasDependentEvidence) {
                                foreach (IDelayEvaluatedEvidence delayEvidence in ps.DependentEvidence) {
                                    delayEvidence.MarkUsed();
                                }
                            }
                        }
                    }
                }
                if (allConst) {
                    // We want to store in the cache the evidence that was touched during policy evaluation
                    // rather than the input serialized evidence, since that evidence is optimized for the
                    // standard policy and is not all-inclusive.  We need to make sure that any evidence
                    // used to determine the grant set is added to the cache key.
                    Cache(count, evidence.RawSerialize(), policy);
                }
            }

            return policy;
        }

        //
        // Private methods.
        //

        [System.Security.SecurityCritical]  // auto-generated
        private void CheckLoaded () {
            if (!m_loaded) {
                lock (InternalSyncObject) {
                    if (!m_loaded)
                        LoadPolicyLevel ();
                }
            }
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private static byte[] ReadFile (string fileName) {
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
                int size = (int) stream.Length;
                byte[] data = new byte[size];
                size = stream.Read(data, 0, size);
                stream.Close();
                return data;
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private void LoadPolicyLevel () {
            SecurityElement elRoot;
            Exception exception = null;

            CodeAccessPermission.Assert(true);
            if (!File.InternalExists(m_path))
                goto SETDEFAULT;

            Encoding encoding = Encoding.UTF8;

            try {
                string data = encoding.GetString(ReadFile(m_path));
                elRoot = SecurityElement.FromString(data);
            }
            catch (Exception ex) {
                string message;
                if (!String.IsNullOrEmpty(ex.Message)) {
                    message = ex.Message;
                }
                else {
                    message = ex.GetType().AssemblyQualifiedName;
                }
                exception = LoadError(Environment.GetResourceString("Error_SecurityPolicyFileParseEx", Label, message));
                goto SETDEFAULT;
            }

            if (elRoot == null) {
                exception = LoadError(Environment.GetResourceString("Error_SecurityPolicyFileParse", Label));
                goto SETDEFAULT;
            }

            SecurityElement elMscorlib = elRoot.SearchForChildByTag("mscorlib");
            if (elMscorlib == null) {
                exception = LoadError(Environment.GetResourceString("Error_SecurityPolicyFileParse", Label));
                goto SETDEFAULT;
            }

            SecurityElement elSecurity = elMscorlib.SearchForChildByTag("security");
            if (elSecurity == null) {
                exception = LoadError(Environment.GetResourceString("Error_SecurityPolicyFileParse", Label));
                goto SETDEFAULT;
            }

            SecurityElement elPolicy = elSecurity.SearchForChildByTag("policy");
            if (elPolicy == null) {
                exception = LoadError(Environment.GetResourceString("Error_SecurityPolicyFileParse", Label));
                goto SETDEFAULT;
            }

            SecurityElement elPolicyLevel = elPolicy.SearchForChildByTag("PolicyLevel");
            if (elPolicyLevel != null) {
                try {
                    this.FromXml(elPolicyLevel);
                }
                catch (Exception) {
                    exception = LoadError(Environment.GetResourceString("Error_SecurityPolicyFileParse", Label));
                    goto SETDEFAULT;
                }
            }
            else {
                exception = LoadError(Environment.GetResourceString("Error_SecurityPolicyFileParse", Label));
                goto SETDEFAULT;
            }

            m_loaded = true;
            return;

        SETDEFAULT:
            SetDefault();
            m_loaded = true;

            if (exception != null)
                throw exception;
        }

        [System.Security.SecurityCritical]  // auto-generated
        private Exception LoadError (string message) {
            //
            // We ignore TypeLoadExceptions in the case of user, machine
            // and Enterprise policy levels as some clients depend on that
            // behavior. We'll throw an exception for any other policy levels.
            //

            if (m_type != PolicyLevelType.User && 
                m_type != PolicyLevelType.Machine &&
                m_type != PolicyLevelType.Enterprise) {
                return new ArgumentException(message);
            }
            else {
                Config.WriteToEventLog(message);
                return null;
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        private void Cache (int count, byte[] serializedEvidence, PolicyStatement policy) {
            if (m_configId == ConfigId.None)
                return;
            if (serializedEvidence == null)
                return;

            byte[] policyArray = new SecurityDocument(policy.ToXml(null, true)).m_data;
            Config.AddCacheEntry(m_configId, count, serializedEvidence, policyArray);
        }

        [System.Security.SecurityCritical]  // auto-generated
        private PolicyStatement CheckCache (int count, byte[] serializedEvidence) {
            if (m_configId == ConfigId.None)
                return null;
            if (serializedEvidence == null)
                return null;

            byte[] cachedValue;
            if (!Config.GetCacheEntry(m_configId, count, serializedEvidence, out cachedValue))
                return null;

            PolicyStatement cachedSet = new PolicyStatement();
            SecurityDocument doc = new SecurityDocument(cachedValue);
            cachedSet.FromXml(doc, 0, null, true);
            return cachedSet;
        }

        [System.Security.SecurityCritical]  // auto-generated
        private static bool IsFullTrustAssembly(ArrayList fullTrustAssemblies, Evidence evidence) {
            if (fullTrustAssemblies.Count == 0)
                return false;

            if (evidence != null) {
                lock (fullTrustAssemblies) {
                    IEnumerator enumerator = fullTrustAssemblies.GetEnumerator();

                    while (enumerator.MoveNext()) {
                        StrongNameMembershipCondition snMC = (StrongNameMembershipCondition) enumerator.Current;
                        if (snMC.Check(evidence)) {
                            if (Environment.GetCompatibilityFlag(CompatibilityFlag.FullTrustListAssembliesInGac)) {
                                if (new ZoneMembershipCondition().Check(evidence))
                                    return true;
                            }
                            else {
                                if (new GacMembershipCondition().Check(evidence))
                                    return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

#pragma warning disable 618 // Policy is obsolete
        private CodeGroup CreateDefaultAllGroup() {
            UnionCodeGroup group = new UnionCodeGroup();
            group.FromXml(CreateCodeGroupElement("UnionCodeGroup", "FullTrust", new AllMembershipCondition().ToXml()), this);
            group.Name = Environment.GetResourceString("Policy_AllCode_Name");
            group.Description = Environment.GetResourceString("Policy_AllCode_DescriptionFullTrust");
            return group;
        }

        [System.Security.SecurityCritical]  // auto-generated
        private CodeGroup CreateDefaultMachinePolicy() {
            UnionCodeGroup root = new UnionCodeGroup();
            root.FromXml(CreateCodeGroupElement("UnionCodeGroup", "Nothing", new AllMembershipCondition().ToXml()), this);
            root.Name = Environment.GetResourceString("Policy_AllCode_Name");
            root.Description = Environment.GetResourceString("Policy_AllCode_DescriptionNothing");

            UnionCodeGroup myComputerCodeGroup = new UnionCodeGroup();
            myComputerCodeGroup.FromXml(CreateCodeGroupElement("UnionCodeGroup", "FullTrust", new ZoneMembershipCondition(SecurityZone.MyComputer).ToXml()), this);
            myComputerCodeGroup.Name = Environment.GetResourceString("Policy_MyComputer_Name");
            myComputerCodeGroup.Description = Environment.GetResourceString("Policy_MyComputer_Description");

            // This code give trust to anything StrongName signed by Microsoft.
            StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob(AssemblyRef.MicrosoftPublicKeyFull);
            UnionCodeGroup microsoft = new UnionCodeGroup();
            microsoft.FromXml(CreateCodeGroupElement("UnionCodeGroup", "FullTrust", new StrongNameMembershipCondition(blob, null, null).ToXml()), this);
            microsoft.Name = Environment.GetResourceString("Policy_Microsoft_Name");
            microsoft.Description = Environment.GetResourceString("Policy_Microsoft_Description");
            myComputerCodeGroup.AddChildInternal(microsoft);

            // This code give trust to anything StrongName signed using the ECMA
            // public key (core system assemblies).
            blob = new StrongNamePublicKeyBlob(AssemblyRef.EcmaPublicKeyFull);
            UnionCodeGroup ecma = new UnionCodeGroup();
            ecma.FromXml(CreateCodeGroupElement("UnionCodeGroup", "FullTrust", new StrongNameMembershipCondition(blob, null, null).ToXml()), this);
            ecma.Name = Environment.GetResourceString("Policy_Ecma_Name");
            ecma.Description = Environment.GetResourceString("Policy_Ecma_Description");
            myComputerCodeGroup.AddChildInternal(ecma);

            root.AddChildInternal(myComputerCodeGroup);

            // do the rest of the zones
            CodeGroup intranet = new UnionCodeGroup();
            intranet.FromXml(CreateCodeGroupElement("UnionCodeGroup", "LocalIntranet", new ZoneMembershipCondition(SecurityZone.Intranet).ToXml()), this);
            intranet.Name = Environment.GetResourceString("Policy_Intranet_Name");
            intranet.Description = Environment.GetResourceString("Policy_Intranet_Description");

            CodeGroup intranetNetCode = new NetCodeGroup(new AllMembershipCondition());
            intranetNetCode.Name = Environment.GetResourceString("Policy_IntranetNet_Name");
            intranetNetCode.Description = Environment.GetResourceString("Policy_IntranetNet_Description");
            intranet.AddChildInternal(intranetNetCode);

            CodeGroup intranetFileCode = new FileCodeGroup(new AllMembershipCondition(), FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery);
            intranetFileCode.Name = Environment.GetResourceString("Policy_IntranetFile_Name");
            intranetFileCode.Description = Environment.GetResourceString("Policy_IntranetFile_Description");
            intranet.AddChildInternal(intranetFileCode);

            root.AddChildInternal(intranet);

            CodeGroup internet = new UnionCodeGroup();
            internet.FromXml(CreateCodeGroupElement("UnionCodeGroup", "Internet", new ZoneMembershipCondition(SecurityZone.Internet).ToXml()), this);
            internet.Name = Environment.GetResourceString("Policy_Internet_Name");
            internet.Description = Environment.GetResourceString("Policy_Internet_Description");

            CodeGroup internetNet = new NetCodeGroup(new AllMembershipCondition());
            internetNet.Name = Environment.GetResourceString("Policy_InternetNet_Name");
            internetNet.Description = Environment.GetResourceString("Policy_InternetNet_Description");
            internet.AddChildInternal(internetNet);

            root.AddChildInternal(internet);

            CodeGroup untrusted = new UnionCodeGroup();
            untrusted.FromXml(CreateCodeGroupElement("UnionCodeGroup", "Nothing", new ZoneMembershipCondition(SecurityZone.Untrusted).ToXml()), this);
            untrusted.Name = Environment.GetResourceString("Policy_Untrusted_Name");
            untrusted.Description = Environment.GetResourceString("Policy_Untrusted_Description");
            root.AddChildInternal(untrusted);

            CodeGroup trusted = new UnionCodeGroup();
            trusted.FromXml(CreateCodeGroupElement("UnionCodeGroup", "Internet", new ZoneMembershipCondition(SecurityZone.Trusted).ToXml()), this);
            trusted.Name = Environment.GetResourceString("Policy_Trusted_Name");
            trusted.Description = Environment.GetResourceString("Policy_Trusted_Description");
            CodeGroup trustedNet = new NetCodeGroup(new AllMembershipCondition());
            trustedNet.Name = Environment.GetResourceString("Policy_TrustedNet_Name");
            trustedNet.Description = Environment.GetResourceString("Policy_TrustedNet_Description");
            trusted.AddChildInternal(trustedNet);

            root.AddChildInternal(trusted);

            return root;
        }

        private static SecurityElement CreateCodeGroupElement(string codeGroupType, string permissionSetName, SecurityElement mshipElement) {
            SecurityElement root = new SecurityElement("CodeGroup");
            root.AddAttribute("class", "System.Security." + codeGroupType + ", mscorlib, Version={VERSION}, Culture=neutral, PublicKeyToken=" + AssemblyRef.EcmaPublicKeyToken + "");
            root.AddAttribute("version", "1");
            root.AddAttribute("PermissionSetName", permissionSetName);

            root.AddChild(mshipElement);

            return root;
        }
#pragma warning restore 618

        private static string[] EcmaFullTrustAssemblies = new string[] {
                                                                "mscorlib.resources",
                                                                "System",
                                                                "System.resources",
                                                                "System.Xml",
                                                                "System.Xml.resources",
                                                                "System.Windows.Forms",
                                                                "System.Windows.Forms.resources",
                                                                #if !FEATURE_PAL
                                                                "System.Data",
                                                                "System.Data.resources",
                                                                #endif // !FEATURE_PAL
                                                            };
        private static string[] MicrosoftFullTrustAssemblies = new string[] {
                                                                #if !FEATURE_PAL
                                                                "System.Security",
                                                                "System.Security.resources",
                                                                "System.Drawing",
                                                                "System.Drawing.resources",
                                                                "System.Messaging",
                                                                "System.Messaging.resources",
                                                                "System.ServiceProcess",
                                                                "System.ServiceProcess.resources",
                                                                "System.DirectoryServices",
                                                                "System.DirectoryServices.resources",
                                                                "System.Deployment",
                                                                "System.Deployment.resources"
                                                                #endif // !FEATURE_PAL
                                                            };

        private void SetDefaultFullTrustAssemblies() {
            m_fullTrustAssemblies = new ArrayList();

            StrongNamePublicKeyBlob ecmaBlob = new StrongNamePublicKeyBlob(AssemblyRef.EcmaPublicKeyFull);
            for (int index=0; index < EcmaFullTrustAssemblies.Length; index++) {
                StrongNameMembershipCondition sn = new StrongNameMembershipCondition(ecmaBlob,
                                                                                     EcmaFullTrustAssemblies[index],
                                                                                     new Version(ThisAssembly.Version));
                m_fullTrustAssemblies.Add(sn);
            }

            StrongNamePublicKeyBlob microsoftBlob = new StrongNamePublicKeyBlob(AssemblyRef.MicrosoftPublicKeyFull);
            for (int index=0; index < MicrosoftFullTrustAssemblies.Length; index++) {
                StrongNameMembershipCondition sn = new StrongNameMembershipCondition(microsoftBlob,
                                                                                     MicrosoftFullTrustAssemblies[index],
                                                                                     new Version(ThisAssembly.Version));
                m_fullTrustAssemblies.Add(sn);
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private void SetDefault() {
            lock (this) {
                string path = GetLocationFromType(m_type) + ".default";
                if (File.InternalExists(path)) {
                    PolicyLevel level = new PolicyLevel(m_type, path);
                    m_rootCodeGroup = level.RootCodeGroup;
                    m_namedPermissionSets = (ArrayList)level.NamedPermissionSets;
                    #pragma warning disable 618 // for obsolete FullTrustAssemblies property.
                    m_fullTrustAssemblies = (ArrayList)level.FullTrustAssemblies;
                    #pragma warning restore 618
                    m_loaded = true;
                }
                else {
                    m_namedPermissionSets = null;
                    m_rootCodeGroup = null;
                    m_permSetElement = null;
                    m_rootCodeGroup = (m_type == PolicyLevelType.Machine ? CreateDefaultMachinePolicy() : CreateDefaultAllGroup());
                    SetFactoryPermissionSets();
                    SetDefaultFullTrustAssemblies();
                    m_loaded = true;
                }
            }
        }

        private void SetFactoryPermissionSets() {
            lock (InternalSyncObject) {
                m_namedPermissionSets = new ArrayList();
                m_namedPermissionSets.Add(BuiltInPermissionSets.FullTrust);
                m_namedPermissionSets.Add(BuiltInPermissionSets.Everything);
                m_namedPermissionSets.Add(BuiltInPermissionSets.Nothing);
                m_namedPermissionSets.Add(BuiltInPermissionSets.SkipVerification);
                m_namedPermissionSets.Add(BuiltInPermissionSets.Execution);
                m_namedPermissionSets.Add(BuiltInPermissionSets.Internet);
                m_namedPermissionSets.Add(BuiltInPermissionSets.LocalIntranet);
            }
        }

        private SecurityElement FindElement(SecurityElement element, string name) {
            // This method searches through the children of the saved element
            // for a named permission set that matches the input name.
            // If it finds a matching set, the appropriate xml element is
            // removed from as a child of the parent and then returned.

            IEnumerator elemEnumerator = element.Children.GetEnumerator();

            while (elemEnumerator.MoveNext()) {
                SecurityElement elPermSet = (SecurityElement)elemEnumerator.Current;
                if (elPermSet.Tag.Equals("PermissionSet")) {
                    string elName = elPermSet.Attribute("Name");

                    if (elName != null && elName.Equals(name)) {
                        element.InternalChildren.Remove(elPermSet);
                        return elPermSet;
                    }
                }
            }

            return null;
        }

        [System.Security.SecurityCritical]  // auto-generated
        private void LoadAllPermissionSets()
        {
            // This function loads all the permission sets held in the m_permSetElement member.
            // This is useful when you know that an arbitrary permission set loaded from
            // the config file could be accessed so you just want to forego the lazy load
            // and play it safe.

            if (m_permSetElement != null && m_permSetElement.InternalChildren != null) {
                lock (InternalSyncObject) {
                    while (m_permSetElement != null && m_permSetElement.InternalChildren.Count != 0) {
                        SecurityElement elPermSet = (SecurityElement)m_permSetElement.Children[m_permSetElement.InternalChildren.Count-1];
                        m_permSetElement.InternalChildren.RemoveAt(m_permSetElement.InternalChildren.Count-1);

                        if (elPermSet.Tag.Equals("PermissionSet") && elPermSet.Attribute("class").Equals("System.Security.NamedPermissionSet")) {
                            NamedPermissionSet permSet = new NamedPermissionSet();
                            permSet.FromXmlNameOnly(elPermSet);

                            if (permSet.Name != null) {
                                m_namedPermissionSets.Add(permSet);
                                try {
                                    permSet.FromXml(elPermSet, false, true);
                                }
                                catch {
                                    m_namedPermissionSets.Remove(permSet);
                                }
                            }
                        }
                    }

                    m_permSetElement = null;
                }
            }
        }

#pragma warning disable 618 // Legacy policy is obsolete
        [System.Security.SecurityCritical]  // auto-generated
        private ArrayList GenericResolve(Evidence evidence, out bool allConst) {
            CodeGroupStack stack = new CodeGroupStack();

            // Note: if m_rootCodeGroup is null it means that we've
            // hit a recursive load case and ended up needing to
            // do a resolve on an assembly used in policy but is
            // not covered by the full trust assemblies list.  We'll
            // throw a policy exception to cover this case.

            CodeGroupStackFrame frame;
            CodeGroup rootCodeGroupRef = m_rootCodeGroup;

            if (rootCodeGroupRef == null)
                throw new PolicyException(Environment.GetResourceString("Policy_NonFullTrustAssembly"));

            frame = new CodeGroupStackFrame();
            frame.current = rootCodeGroupRef;
            frame.parent = null;

            stack.Push(frame);

            ArrayList accumulator = new ArrayList();

            bool foundExclusive = false;

            allConst = true;

            Exception storedException = null;

            while (!stack.IsEmpty()) {
                frame = stack.Pop();

                FirstMatchCodeGroup firstMatchGroup = frame.current as FirstMatchCodeGroup;
                UnionCodeGroup unionGroup = frame.current as UnionCodeGroup;

                if (!(frame.current.MembershipCondition is IConstantMembershipCondition) ||
                    (unionGroup == null && firstMatchGroup == null)) {
                    allConst = false;
                }

                try {
                    frame.policy = PolicyManager.ResolveCodeGroup(frame.current, evidence);
                }
                catch (Exception e) {
                    // If any exception occurs while attempting a resolve, we catch it here and
                    // set the equivalent of the resolve not matching to the evidence.
                    //frame.policy = null;

                    if (storedException == null)
                        storedException = e;
                }

                if (frame.policy != null) {                    
                    if ((frame.policy.Attributes & PolicyStatementAttribute.Exclusive) != 0) {
                        if (foundExclusive)
                            throw new PolicyException(Environment.GetResourceString("Policy_MultipleExclusive"));

                        accumulator.RemoveRange(0, accumulator.Count);
                        accumulator.Add(frame);
                        foundExclusive = true;
                    }

                    if (!foundExclusive) {
                        accumulator.Add(frame);
                    }
                }
            }

            if (storedException != null)
                throw storedException;

            return accumulator;
        }
#pragma warning restore 618

        private static string GenerateFriendlyName(string className, Hashtable classes) {
            if (classes.ContainsKey(className))
                return (string)classes[className];

            Type type = System.Type.GetType(className, false, false);
            if (type != null && !type.IsVisible) 
                type = null;

            if (type == null)
                return className;

            if (!classes.ContainsValue(type.Name)) {
                classes.Add(className, type.Name);
                return type.Name;
            }
            else if (!classes.ContainsValue(type.FullName)) {
                classes.Add(className, type.FullName);
                return type.FullName;
            }
            else {
                classes.Add(className, type.AssemblyQualifiedName);
                return type.AssemblyQualifiedName;
            }
        }

        private SecurityElement NormalizeClassDeep(SecurityElement elem, Hashtable classes) {
            NormalizeClass(elem, classes);

            if (elem.InternalChildren != null && elem.InternalChildren.Count > 0) {
                IEnumerator enumerator = elem.Children.GetEnumerator();
                while (enumerator.MoveNext()) {
                    NormalizeClassDeep((SecurityElement)enumerator.Current, classes);
                }
            }

            return elem;
        }

        private SecurityElement NormalizeClass(SecurityElement elem, Hashtable classes) {
            if (elem.m_lAttributes == null || elem.m_lAttributes.Count == 0)
                return elem;

            int iMax = elem.m_lAttributes.Count;
            Contract.Assert(iMax % 2 == 0, "Odd number of strings means the attr/value pairs were not added correctly");

            for (int i = 0; i < iMax; i += 2) {
                string strAttrName = (string)elem.m_lAttributes[i];

                if (strAttrName.Equals("class")) {
                    string strAttrValue = (string)elem.m_lAttributes[i+1];

                    elem.m_lAttributes[i+1] = GenerateFriendlyName(strAttrValue, classes);

                    // only one class attribute so we can stop once we found it
                    break;
                }
            }

            return elem;
        }

        private SecurityElement UnnormalizeClassDeep(SecurityElement elem, Hashtable classes) {
            UnnormalizeClass(elem, classes);

            if (elem.InternalChildren != null && elem.InternalChildren.Count > 0) {
                IEnumerator enumerator = elem.Children.GetEnumerator();

                while (enumerator.MoveNext()) {
                    UnnormalizeClassDeep((SecurityElement)enumerator.Current, classes);
                }
            }

            return elem;
        }

        private SecurityElement UnnormalizeClass(SecurityElement elem, Hashtable classes) {
            if (classes == null || elem.m_lAttributes == null || elem.m_lAttributes.Count == 0)
                return elem;

            int iMax = elem.m_lAttributes.Count;
            Contract.Assert(iMax % 2 == 0, "Odd number of strings means the attr/value pairs were not added correctly");

            for (int i = 0; i < iMax; i += 2) {
                string strAttrName = (string)elem.m_lAttributes[i];

                if (strAttrName.Equals("class")) {
                    string strAttrValue = (string)elem.m_lAttributes[i+1];
                    string className = (string)classes[strAttrValue];

                    if (className != null)
                        elem.m_lAttributes[i+1] = className;

                    // only one class attribute so we can stop after we found it
                    // no other matches are possible
                    break;
                }
            }

            return elem;
        }
    }

    internal sealed class CodeGroupStackFrame {
        internal CodeGroup current;
        internal PolicyStatement policy;
        internal CodeGroupStackFrame parent;
    }

    internal sealed class CodeGroupStack {
        private ArrayList m_array;

        internal CodeGroupStack() {
            m_array = new ArrayList();
        }

        internal void Push(CodeGroupStackFrame element) {
            m_array.Add(element);
        }

        internal CodeGroupStackFrame Pop() {
            if (IsEmpty())
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EmptyStack"));
            Contract.EndContractBlock();

            int count = m_array.Count;
            CodeGroupStackFrame temp = (CodeGroupStackFrame) m_array[count-1];
            m_array.RemoveAt(count-1);
            return temp;
        }

        [Pure]
        internal bool IsEmpty() {
            return m_array.Count == 0;
        }
    }
}
