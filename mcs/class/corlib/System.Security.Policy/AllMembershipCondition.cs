//
// System.Security.Policy.AllMembershipCondition.cs
//
// Author:
//   Ajay kumar Dwivedi (adwiv@yahoo.com)
//

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

using System;
using System.Security;

namespace System.Security.Policy {

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

		public override bool Equals (object o)
		{
			return (o is System.Security.Policy.AllMembershipCondition);
		}
 
		public void FromXml(SecurityElement e)
		{
			FromXml(e, null);
		}

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
