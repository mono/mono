// System.Security.Policy.ApplicationDirectoryMembershipCondition
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//   Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2002 Nick Drochak, All rights reserved.

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Security;

namespace System.Security.Policy {

	[Serializable]
	public sealed class ApplicationDirectoryMembershipCondition :
                IConstantMembershipCondition,
		IMembershipCondition, 
		ISecurityEncodable, 
		ISecurityPolicyEncodable
	{
		// Tag for Xml Data
		private static readonly string XmlTag = "IMembershipCondition";

		// Methods
		[MonoTODO]
		public bool Check (Evidence evidence)
		{
			if (evidence == null)
				return false;
			
			foreach (object o in evidence) {
				if (o is ApplicationDirectory) {
					ApplicationDirectory ad = (o as ApplicationDirectory);
					// TODO
					throw new NotImplementedException (); 
				}
			}
			return false;
		}

		public IMembershipCondition Copy () 
		{ 
			return new ApplicationDirectoryMembershipCondition ();
		}
		
		public override bool Equals (object o) 
		{ 
			return o is ApplicationDirectoryMembershipCondition; 
		}
		
		public void FromXml (SecurityElement e)
		{
			FromXml (e, null);
		}
		
		public void FromXml (SecurityElement e, PolicyLevel level)
		{
			if (null == e)
				throw new ArgumentNullException ("e");

			if (XmlTag != e.Tag)
				throw new ArgumentException("e","The Tag of SecurityElement must be "
					+ ApplicationDirectoryMembershipCondition.XmlTag);
		}
		
		/// <summary>
		///   All instances of ApplicationDirectoryMembershipCondition are equal so they should
		///   have the same hashcode
		/// </summary>
		public override int GetHashCode () 
		{ 
			return typeof (ApplicationDirectoryMembershipCondition).GetHashCode ();
		}
		
		public override string ToString () 
		{ 
			return "ApplicationDirectory";
		}
		
		public SecurityElement ToXml () 
		{ 
			return ToXml (null);
		}
		
		public SecurityElement ToXml (PolicyLevel level) 
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
