//
// System.Diagnostics.LookupBindingPropertiesAttribute.cs
//
// Authors:
//   Rolf Bjarne Kvinge  (RKvinge@novell.com)
//
// (C) 2002
//

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

namespace System.ComponentModel
{	
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class LookupBindingPropertiesAttribute : Attribute
	{
		private string data_source;
		private string display_member;
		private string value_member;
		private string lookup_member;
		
		public static readonly LookupBindingPropertiesAttribute Default;

		static LookupBindingPropertiesAttribute()
		{
			Default = new LookupBindingPropertiesAttribute ();
		}
		
		public LookupBindingPropertiesAttribute(string dataSource, string displayMember, string valueMember, string lookupMember)
		{
			data_source = dataSource;
			display_member = displayMember;
			value_member = valueMember;
			lookup_member = lookupMember;
		}

		public LookupBindingPropertiesAttribute()
		{
		}
		
		public override int GetHashCode()
		{
			return  (data_source != null ? data_source.GetHashCode () : 1) << 24 +
				(display_member != null ? display_member.GetHashCode () : 1) << 16 +
				(lookup_member != null ? lookup_member.GetHashCode () : 1) << 8 +
				(value_member != null ? value_member.GetHashCode () : 1);
		}

		public override bool Equals(object obj)
		{
			LookupBindingPropertiesAttribute other = obj as LookupBindingPropertiesAttribute;
			
			if (other == null)
				return false;
			
			if (data_source != other.data_source || display_member != other.display_member)
				return false;
			
			if (value_member != other.value_member || lookup_member != other.lookup_member)
				return false;
			
			return true;
		}
		
		public string DataSource {
			get {
				return data_source;
			}
		}

		public string DisplayMember {
			get	{
				return display_member;
			}
		}

		public string LookupMember {
			get	{
				return lookup_member;
			}
		}

		public string ValueMember {
			get	{
				return value_member;
			}
		}

	}
}