//
// System.Security.Policy.AllMembershipCondition.cs
//
// Author:
//   Ajay kumar Dwivedi (adwiv@yahoo.com)
//

using System;
using System.Security;


namespace System.Security.Policy
{
	/// <summary>
	/// Summary description for AllMembershipCondition.
	/// </summary>
	[Serializable]
	public sealed class AllMembershipCondition
                : IMembershipCondition, IConstantMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable
	{
		// Tag for Xml Data
		private static readonly string XmlTag = "IMembershipCondition";

		public AllMembershipCondition()
		{}

		//Always returns true
		public bool Check(Evidence evidence)
		{
			return true;
		}

		public IMembershipCondition Copy()
		{
			return new AllMembershipCondition();
		}

		public override bool Equals(object o)
		{
			if(o is System.Security.Policy.AllMembershipCondition)
				return true;
			return false;
		}
 
		public void FromXml(SecurityElement e)
		{
			FromXml(e, null);
		}

		//Fixme: is there a need for all this????
		public void FromXml(SecurityElement e, PolicyLevel level)
		{
			if(e == null)
				throw new ArgumentNullException("e");
			if(e.Tag != XmlTag)
				throw new ArgumentException("e","The Tag of SecurityElement must be "
					+ AllMembershipCondition.XmlTag);
		}

		
		public override int GetHashCode()
		{
			return typeof (AllMembershipCondition).GetHashCode ();
		}

		public override string ToString()
		{
			return "All Code";
		}

		public SecurityElement ToXml()
		{
			return ToXml(null);
		}

		public SecurityElement ToXml(PolicyLevel level)
		{
			SecurityElement se = new SecurityElement(XmlTag);
			Type type = this.GetType();
			string classString = type.FullName + ", " + type.Assembly;
			se.AddAttribute("class",classString);
			se.AddAttribute("version","1");
			return se;
		}
	}
}
