// System.Security.Policy.PolicyLevel
//
// Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak

using System.Collections; // for IList

namespace System.Security.Policy
{
	[MonoTODO]
	[Serializable]
	public sealed class PolicyLevel
	{
		internal PolicyLevel () {}

		public IList FullTrustAssemblies
		{
			get
			{
				return (IList)null;
			}
		}

		public string Label
		{
			get 
			{
				return "";
			}
		}

		public IList NamedPermissionSets
		{
			get
			{
				return (IList)null;
			}
		}

		public CodeGroup RootCodeGroup
		{
			get
			{
				return (CodeGroup)null;
			}
			
			set
			{
				
			}
		}

		public string StoreLocation
		{
			get
			{
				return "";
			}
		}
	}
}
