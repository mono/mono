// System.Security.Policy.CodeGroup
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak, All rights reserved.

using System.Security.Policy;
using System.Security.Permissions;
using System.Collections;
using System;  // for MonoTODO attribute

namespace System.Security.Policy
{
	[Serializable]
	public abstract class CodeGroup
	{
		PolicyStatement m_policy = null;
		IMembershipCondition m_membershipCondition = null;
		string m_description = null;
		string m_name = null;
		ArrayList m_children = new ArrayList();
		PolicyLevel m_level;

		public CodeGroup(IMembershipCondition membershipCondition,
				PolicyStatement policy)
		{
			if (null == membershipCondition)
				throw new ArgumentNullException("Value cannot be null.");

			m_policy = policy;
			m_membershipCondition = membershipCondition;
		}

		public abstract CodeGroup Copy();
		public abstract string MergeLogic {get;}
		public abstract PolicyStatement Resolve(	Evidence evidence);
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

		public string Name 
		{
			get
			{
				return m_name;
			}
			set
			{
				m_name = value;
			}
		}

		public IList Children
		{
			get
			{
				return m_children;
			}
			set
			{
				if (null == value)
					throw new ArgumentException("Value cannot be null");
				m_children = new ArrayList(value);
			}
		}

		public virtual string AttributeString
		{
			get
			{
				if (null != m_policy)
					return m_policy.AttributeString;

				return null;
			}
		}

		public virtual string PermissionSetName
		{
			get
			{
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

			if (cg.MembershipCondition != this.MembershipCondition)
				return false;

			if (compareChildren)
			{
				int childCount = cg.Children.Count;
				if (this.Children.Count != childCount)
					return false;

				for (int index = 0; index < childCount; index++)
				{
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

		public void RemoveChild(CodeGroup group)
		{
			if (!m_children.Contains(group))
				throw new ArgumentException();

			m_children.Remove(group);
		}

		[MonoTODO]
		public override int GetHashCode()
		{
			return 42;
		}

		public void FromXml (SecurityElement e)
		{
			FromXml(e, (PolicyLevel)null);
		}

		[MonoTODO]
		public void FromXml (SecurityElement e, PolicyLevel level)
		{
			if (null == e)
				throw new ArgumentNullException("e");

			// Not sure what might be serialized in this XML, so just do the strings for now
			// and null's for everything else
			m_children = null;
			m_policy = null;
			m_membershipCondition = null;

			m_name = e.Attribute("Name");
			m_description = e.Attribute("Description");

			// seems like we might need this to Resolve() in subclasses
			m_level = level;

			ParseXml(e, level);
		}

		protected virtual void ParseXml(SecurityElement e, PolicyLevel level)
		{
		}
		
		public SecurityElement ToXml()
		{
			return ToXml(null);
		}

		[MonoTODO("Not sure what to do with PolicyLevel parameter")]
		public SecurityElement ToXml(PolicyLevel level)
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

			if (null != PolicyStatement)
				e.AddChild(PolicyStatement.PermissionSet.ToXml());

			foreach (CodeGroup child in Children)
				e.AddChild(child.ToXml());

			CreateXml(e, level);
			return e;
		}
		
		protected virtual void CreateXml(SecurityElement element, PolicyLevel level)
		{
		}
	}  // public abstract class CodeGroup

}  // namespace System.Security.Policy
