// System.Security.Policy.CodeGroup
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak, All rights reserved.

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Security.Policy;
using System.Security.Permissions;
//using System;  // for MonoTODO attribute

namespace System.Security.Policy {

	[Serializable]
	public abstract class CodeGroup {
		PolicyStatement m_policy = null;
		IMembershipCondition m_membershipCondition = null;
		string m_description = null;
		string m_name = null;
		ArrayList m_children = new ArrayList();
		PolicyLevel m_level;

		public CodeGroup (IMembershipCondition membershipCondition, PolicyStatement policy)
		{
			if (null == membershipCondition)
				throw new ArgumentNullException("Value cannot be null.");

			m_policy = policy;
			m_membershipCondition = membershipCondition;
		}

		// for PolicyLevel (to avoid validation duplication)
		internal CodeGroup (SecurityElement e) 
		{
			FromXml (e);
		}

		// abstract

		public abstract CodeGroup Copy();
		public abstract string MergeLogic {get;}
		public abstract PolicyStatement Resolve (Evidence evidence);
		public abstract CodeGroup ResolveMatchingCodeGroups(Evidence evidence);

		public PolicyStatement PolicyStatement {

			get { return m_policy; }

			set { m_policy = value; }
		}

		public string Description {

			get { return m_description; }

			set { m_description = value; }
		}

		public IMembershipCondition MembershipCondition	 {

			get {
				return m_membershipCondition;
			}

			set {
				if (null == value)
					throw new ArgumentException("Value cannot be null");
				m_membershipCondition = value;
			}
		}

		public string Name {
			get { return m_name; }
			set { m_name = value; }
		}

		public IList Children {
			get { return m_children; }
			set {
				if (null == value)
					throw new ArgumentNullException ("value");
				m_children = new ArrayList(value);
			}
		}

		public virtual string AttributeString {
			get {
				if (null != m_policy)
					return m_policy.AttributeString;
				return null;
			}
		}

		public virtual string PermissionSetName {
			get {
				if (m_policy.PermissionSet is Security.NamedPermissionSet)
					return ((NamedPermissionSet)(m_policy.PermissionSet)).Name;
				return null;
			}
		}

		public void AddChild(CodeGroup group)
		{
			if (null == group)
				throw new ArgumentNullException("The group parameter cannot be null");
			m_children.Add(group);
		}

		public override bool Equals(object o)
		{
			if (!(o is CodeGroup))
				return false;

			return Equals((CodeGroup)o, false);
		}

		public bool Equals(CodeGroup cg, bool compareChildren)
		{
			if (cg.Name != this.Name)
				return false;

			if (cg.Description != this.Description)
				return false;

// FIXME: this compiles with CSC. Didn't succeed at creating a smaller/different test case :(
//			if (!cg.MembershipCondition.Equals (m_membershipCondition))
			if (((object) cg.MembershipCondition).ToString () !=
			    ((object) m_membershipCondition).ToString ())
				return false;

			if (compareChildren) {
				int childCount = cg.Children.Count;
				if (this.Children.Count != childCount)
					return false;

				for (int index = 0; index < childCount; index++) {
					// LAMESPEC: are we supposed to check child equality recursively?
					//		The docs imply 'no' but it seems natural to do a 'deep' compare.
					//		Will check the children's children, and so-on unless we find out that
					//		we shouldn't
					if (!((CodeGroup)(this.Children[index])).Equals((CodeGroup)(cg.Children[index]), true))
						return false;
				}
			}
			return true;
		}

		public void RemoveChild (CodeGroup group)
		{
			if (group != null)
				m_children.Remove (group);
		}

		public override int GetHashCode ()
		{
			int hashCode = m_membershipCondition.GetHashCode ();
			if (m_policy != null)
				hashCode += m_policy.GetHashCode ();
			return hashCode;
		}

		public void FromXml (SecurityElement e)
		{
			FromXml (e, null);
		}

		public void FromXml (SecurityElement e, PolicyLevel level)
		{
			if (null == e)
				throw new ArgumentNullException("e");

			PermissionSet ps = null;
			SecurityElement pset = e.SearchForChildByTag ("PermissionSet");
			if (pset != null) {
				Type classType = Type.GetType (pset.Attribute ("class"));
				ps = (PermissionSet) Activator.CreateInstance (classType, true);
				ps.FromXml (pset);
			}
			else
				ps = new NamedPermissionSet ("Nothing", new PermissionSet (PermissionState.None));
			m_policy = new PolicyStatement (ps);

			m_children.Clear ();
			if ((e.Children != null) && (e.Children.Count > 0)) {
				foreach (SecurityElement se in e.Children) {
					if (se.Tag == "CodeGroup") {
						this.AddChild (CodeGroup.CreateFromXml (se));
					}
				}
			}
			
			m_membershipCondition = null;
			SecurityElement mc = e.SearchForChildByTag ("IMembershipCondition");
			if (mc != null) {
				string className = mc.Attribute ("class");
				Type classType = Type.GetType (className);
				if (classType == null)
					classType = Type.GetType ("System.Security.Policy." + className);
				m_membershipCondition = (IMembershipCondition) Activator.CreateInstance (classType);
				m_membershipCondition.FromXml (mc, level);
			}

			m_name = e.Attribute("Name");
			m_description = e.Attribute("Description");

			// seems like we might need this to Resolve() in subclasses
			m_level = level;

			ParseXml (e, level);
		}

		protected virtual void ParseXml(SecurityElement e, PolicyLevel level)
		{
		}
		
		public SecurityElement ToXml()
		{
			return ToXml (null);
		}

		public SecurityElement ToXml (PolicyLevel level)
		{
			SecurityElement e = new SecurityElement("CodeGroup");
			e.AddAttribute("class", this.GetType().AssemblyQualifiedName);
			e.AddAttribute("version", "1");

			if (null != Name)
				e.AddAttribute("Name", Name);

			if (null != Description)
				e.AddAttribute("Description", Description);

			if (null != MembershipCondition)
				e.AddChild(MembershipCondition.ToXml());

			if ((PolicyStatement != null) && (PolicyStatement.PermissionSet != null))
				e.AddChild (PolicyStatement.PermissionSet.ToXml ());

			foreach (CodeGroup child in Children)
				e.AddChild(child.ToXml());

			CreateXml(e, level);
			return e;
		}
		
		protected virtual void CreateXml(SecurityElement element, PolicyLevel level)
		{
		}

		// internal stuff

		internal static CodeGroup CreateFromXml (SecurityElement se) 
		{
			string fullClassName = se.Attribute ("class");
			string className = fullClassName;
			// many possible formats
			// a. "FirstMatchCodeGroup"
			// b. "System.Security.Policy.FirstMatchCodeGroup"
			// c. "System.Security.Policy.FirstMatchCodeGroup, mscorlib, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"\r\n           version=\"1\">\r\n   <IMembershipCondition class=\"System.Security.Policy.AllMembershipCondition, mscorlib, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
			int n = className.IndexOf (",");
			if (n > 0) {
				className = className.Substring (0, n);
			}
			n = className.LastIndexOf (".");
			if (n > 0)
				className = className.Substring (n + 1);
			// much faster than calling Activator.CreateInstance
			switch (className) {
				case "FileCodeGroup":
					return new FileCodeGroup (se);
				case "FirstMatchCodeGroup":
					return new FirstMatchCodeGroup (se);
				case "NetCodeGroup":
					return new NetCodeGroup (se);
				case "UnionCodeGroup":
					return new UnionCodeGroup (se);
				default: // unknown
					Type classType = Type.GetType (fullClassName);
					return (CodeGroup) Activator.CreateInstance (classType, true);
			}
		}
	}  // public abstract class CodeGroup
}  // namespace System.Security.Policy
