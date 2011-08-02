//
// System.ComponentModel.DescriptionAttribute.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Andreas Nahr
//
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

namespace System.ComponentModel {

	[AttributeUsage (AttributeTargets.All)]
	public class DescriptionAttribute : Attribute 
	{
		private string desc;

		public static readonly DescriptionAttribute Default = new DescriptionAttribute ();
			
		public DescriptionAttribute ()
		{
			desc = string.Empty;
		}

		public DescriptionAttribute (string description)
		{
			desc = description;
		}

		public virtual string Description {
			get {
				return DescriptionValue;
			}
		}

		//
		// Notice that the default Description implementation uses this by default
		//
		protected string DescriptionValue {
			get {
				return desc;
			}

			set {
				desc = value;
			}
		}

		public override bool Equals (object obj)
		{
			if (!(obj is DescriptionAttribute))
				return false;
			if (obj == this)
				return true;
			return ((DescriptionAttribute) obj).Description == desc;
		}
			
		public override int GetHashCode ()
		{
			return desc.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return this == Default;
		}
	}
}

