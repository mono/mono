// System.Security.Policy.CodeGroup
//
// Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak

using System.Security.Policy;
using System.Security.Permissions;

namespace System.Security.Policy
{
	[MonoTODO]
	[Serializable]
	public abstract class CodeGroup
	{
		PolicyStatement m_policy;
		IMembershipCondition m_membershipCondition;

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

		public PolicyStatement PolicyStatement
		{
			get
			{
				return m_policy;
			}
			set
			{
				m_policy = value;
			}
		}

	}  // public abstract class CodeGroup

	// FIXME: remove after done testing in VS.NET
	public class MonoTODO : Attribute {}
}  // namespace System.Security.Policy