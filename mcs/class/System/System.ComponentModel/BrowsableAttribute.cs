//
// System.ComponentModel.BrowsableAttribute.cs
//
// Author:
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
	public sealed class BrowsableAttribute : Attribute
	{
		private bool browsable;

		public static readonly BrowsableAttribute Default = new BrowsableAttribute (true);
		public static readonly BrowsableAttribute No = new BrowsableAttribute (false);
		public static readonly BrowsableAttribute Yes = new BrowsableAttribute (true);

		public BrowsableAttribute (bool browsable)
		{
			this.browsable = browsable;
		}

		public bool Browsable {
			get { return browsable; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is BrowsableAttribute))
				return false;
			if (obj == this)
				return true;
			return ((BrowsableAttribute) obj).Browsable == browsable;
		}

		public override int GetHashCode ()
		{
			return browsable.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return browsable == BrowsableAttribute.Default.Browsable;
		}
	}
}

