//
// System.ComponentModel.DisplayNameAttribute
//
// Authors:
//  Marek Habersack <grendello@gmail.com>
//
// (C) 2006 Marek Habersack
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

#if NET_2_0
namespace System.ComponentModel
{
	[AttributeUsageAttribute(AttributeTargets.Class|AttributeTargets.Method|AttributeTargets.Property|AttributeTargets.Event)]
	public class DisplayNameAttribute : Attribute
	{
		public static readonly DisplayNameAttribute Default = new DisplayNameAttribute ();

		private string attributeDisplayName;

		public DisplayNameAttribute ()
		{
			this.attributeDisplayName = String.Empty;
		}

		public DisplayNameAttribute (string displayName)
		{
			this.attributeDisplayName = displayName != null ? displayName : String.Empty;
		}

		public override bool IsDefaultAttribute ()
		{
			return attributeDisplayName.Length == 0;
		}

		public override int GetHashCode ()
		{
			return attributeDisplayName.GetHashCode ();
		}
		
		public override bool Equals (object obj)
		{
			if (obj == this)
				return true;
			
			DisplayNameAttribute dna = obj as DisplayNameAttribute;
			
			if (dna == null)
				return false;
			return dna.DisplayName == attributeDisplayName;
		}
		
		public virtual string DisplayName {
			get { return attributeDisplayName; }
		}
		
		protected string DisplayNameValue {
			get { return attributeDisplayName; }
			set { attributeDisplayName = value != null ? value : String.Empty; }
		}
	}
}
#endif
