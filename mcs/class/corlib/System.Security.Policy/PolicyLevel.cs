//
// System.Security.Policy.PolicyLevel.cs
//
// Authors:
//      Nick Drochak (ndrochak@gol.com)
//      Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Nick Drochak
// (C) 2003 Duncan Mak, Ximian Inc.
//

using System.Collections; // for IList
using System.Globalization;
using System.Security.Policy;

namespace System.Security.Policy
{
	[MonoTODO][Serializable]
	public sealed class PolicyLevel
	{
                string label;
                StrongNameMembershipCondition [] full_trust_assemblies;
                CodeGroup root_code_group;
                NamedPermissionSet [] named_permission_sets;

		internal PolicyLevel (string label)
                {
                        this.label = label;

                        // What's a good default size?                        
                        full_trust_assemblies = new StrongNameMembershipCondition [10];
                        named_permission_sets = new NamedPermissionSet [10];
                }

                [MonoTODO]
		public IList FullTrustAssemblies
		{
			get {
                                if (full_trust_assemblies != null)
                                        return (IList) full_trust_assemblies;
                                
                                return (IList) null;
			}
		}

		public string Label {

			get { return label; }
		}

		public IList NamedPermissionSets {

			get {
				return (IList) named_permission_sets;
			}
		}

		public CodeGroup RootCodeGroup {
                        
                        get { return root_code_group; }
			
			set { root_code_group = value; }
		}

                [MonoTODO]
		public string StoreLocation {
			get {
				throw new NotImplementedException ();
			}
		}

                public void AddFullTrustAssembly (StrongName sn)
                {
                        StrongNameMembershipCondition snMC = new StrongNameMembershipCondition(
                                sn.PublicKey, sn.Name, sn.Version);

                        AddFullTrustAssembly (snMC);
                }

                public void AddFullTrustAssembly (StrongNameMembershipCondition snMC)
                {
                        if (snMC == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("The argument is null."));
                        
                        if (((IList) full_trust_assemblies).Contains (snMC))
                                throw new ArgumentException (
                                        Locale.GetText ("sn already has full trust."));
                                        
                        ((IList) full_trust_assemblies).Add (snMC);
                }

                public void AddNamedPermissionSet (NamedPermissionSet permSet)
                {
                        if (permSet == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("The argument is null."));

                        foreach (NamedPermissionSet n in named_permission_sets)
                                if (permSet.Name == n.Name)
                                        throw new ArgumentException (
                                                Locale.GetText ("This NamedPermissionSet is the same an existing NamedPermissionSet."));

                        ((IList) named_permission_sets).Add (permSet);
                }

                [MonoTODO ("Set NamedPermissionSet to the one from default policy and grant a FullTrust RootCodeGroup")]
                public static PolicyLevel CreateAppDomainLevel ()
                {
                        PolicyLevel p = new PolicyLevel ("AppDomain");

                        return p;
                }

                [MonoTODO ("Check for the element's validity")]
                public void FromXml (SecurityElement e)
                {
                        if (e == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("The Argument is null."));
                }

                public NamedPermissionSet GetNamedPermissionSet (string name)
                {
                        if (name == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("The Argument is null."));

                        foreach (NamedPermissionSet n in named_permission_sets)
                                if (n.Name == name)
                                        return n;

                        return null;
                }

                [MonoTODO]
                public void Recover ()
                {
                        throw new NotImplementedException ();
                }

                public void RemoveFullTrustAssembly (StrongName sn)
                {
                        StrongNameMembershipCondition s = new StrongNameMembershipCondition (sn.PublicKey, sn.Name, sn.Version);

                        RemoveFullTrustAssembly (s);
                }

                public void RemoveFullTrustAssembly (StrongNameMembershipCondition snMC)
                {
                        if (snMC == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("The Argument is null."));

                        if (((IList) full_trust_assemblies).Contains (snMC))
                                ((IList) full_trust_assemblies).Remove (snMC);

                        else
                                throw new ArgumentException (
                                        Locale.GetText ("sn does not have full trust."));
                }

                public NamedPermissionSet RemoveNamedPermissionSet (NamedPermissionSet permSet)
                {
                        if (permSet == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("The Argument is null."));

                        if (! ((IList )named_permission_sets).Contains (permSet))
                                throw new ArgumentException (
                                        Locale.GetText ("permSet cannot be found."));

                        ((IList) named_permission_sets).Remove (permSet);

                        return permSet;
                }

                [MonoTODO ("Check for reserver names")]
                public NamedPermissionSet RemoveNamedPermissionSet (string name)
                {
                        if (name == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("The Argument is null."));

                        int idx = -1;
                        for (int i = 0; i < named_permission_sets.Length; i++) {
                                NamedPermissionSet current = named_permission_sets [i];

                                if (current.Name == name)
                                        idx = i;
                                i ++;
                        }                       

                        if (idx == -1)
                                throw new ArgumentException (
                                        Locale.GetText ("Name cannot be found."));

                        NamedPermissionSet retval = named_permission_sets [idx];
                        ((IList) named_permission_sets).RemoveAt (idx);

                        return retval;
                }

                [MonoTODO ("Find out what the default state is")]
                public void Reset ()
                {
                        throw new NotImplementedException ();
                }

                [MonoTODO]
                public PolicyStatement Resolve (Evidence evidence)
                {
                        if (evidence == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("The Argument is null."));

                        throw new NotImplementedException ();
                }

                [MonoTODO]
                public CodeGroup ResolveMatchingCodeGroups (Evidence evidence)
                {
                        if (evidence == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("The Argument is null."));

                        throw new NotImplementedException ();
                }

                [MonoTODO ("Populate security_classes")]
                public SecurityElement ToXml ()
                {
                        SecurityElement element = new SecurityElement (
                                typeof (System.Security.Policy.PolicyLevel).Name);
                        
                        element.AddAttribute ("version", "1");

                        SecurityElement security_classes = new SecurityElement ("SecurityClasses");
                        element.AddChild (security_classes);
                        
                        SecurityElement namedPSs = new SecurityElement ("NamedPermissionSets");
                        element.AddChild (namedPSs);

                        foreach (NamedPermissionSet nps in named_permission_sets)
                                namedPSs.AddChild (nps.ToXml ());

                        element.AddChild (root_code_group.ToXml ());

                        SecurityElement fta = new SecurityElement ("FullTrustAssemblies");
                        element.AddChild (fta);
                        
                        foreach (StrongNameMembershipCondition s in full_trust_assemblies)
                                element.AddChild (s.ToXml (this));
                        
                        return element;
                }
        }
}
