// System.Security.Policy.ApplicationDirectoryMembershipCondition
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//   Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2002 Nick Drochak, All rights reserved.

using System.Security;

namespace System.Security.Policy
{

	[Serializable]
	public sealed class ApplicationDirectoryMembershipCondition :
		IMembershipCondition, 
		ISecurityEncodable, 
		ISecurityPolicyEncodable
	{
		// Tag for Xml Data
		private static readonly string XmlTag = "IMembershipCondition";

		// Methods
		[MonoTODO]
		public bool Check(Evidence evidence) { 
			throw new NotImplementedException (); 
		}

		public IMembershipCondition Copy() { 
			return new ApplicationDirectoryMembershipCondition ();
		}
		
		public override bool Equals(object o) { 
			return o is ApplicationDirectoryMembershipCondition; 
		}
		
		public void FromXml(SecurityElement e) { 
			FromXml (e, null);
		}
		
		public void FromXml(SecurityElement e, PolicyLevel level) { 
			
			if (null == e)
				throw new ArgumentNullException ();
			if (XmlTag != e.Tag)
				throw new ArgumentException("e","The Tag of SecurityElement must be "
					+ ApplicationDirectoryMembershipCondition.XmlTag);
		}
		
		/// <summary>
		///   All instances of ApplicationDirectoryMembershipCondition are equal so they should
		///   have the same hashcode
		/// </summary>
		public override int GetHashCode() 
		{ 
			return typeof (ApplicationDirectoryMembershipCondition).GetHashCode ();
		}
		
		public override string ToString() 
		{ 
			return "ApplicationDirectory";
		}
		
		public SecurityElement ToXml() 
		{ 
			return ToXml (null);
		}
		
		public SecurityElement ToXml(PolicyLevel level) 
		{
			SecurityElement element = new SecurityElement (XmlTag);
			Type type = GetType ();
			string classString = type.FullName + ", " + type.Assembly;
			element.AddAttribute ("class", classString);
			element.AddAttribute ("version", "1");

			return element;
		}
	}
}