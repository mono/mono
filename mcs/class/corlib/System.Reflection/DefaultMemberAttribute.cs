//
// System.Reflection.DefaultMemberAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Reflection {

	[Serializable]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct |
		AttributeTargets.Interface)]
	public sealed class DefaultMemberAttribute : Attribute {
		string member_name;
		
		public DefaultMemberAttribute (string member_name)
		{
			this.member_name = member_name;
		}

		public string MemberName {
			get {
				return member_name;
			}
		}
	}
}
