//
// System.ComponentModel.MergablePropertyAttribute
//
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
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

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.All)]
	public sealed class MergablePropertyAttribute : Attribute
	{

		private bool mergable;

		public static readonly MergablePropertyAttribute Default = new MergablePropertyAttribute (true);
		public static readonly MergablePropertyAttribute No = new MergablePropertyAttribute (false);
		public static readonly MergablePropertyAttribute Yes = new MergablePropertyAttribute (true);

		public MergablePropertyAttribute (bool allowMerge)
		{
			this.mergable = allowMerge;
		}

		public bool AllowMerge {
			get { return mergable; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is MergablePropertyAttribute))
				return false;
			if (obj == this)
				return true;
			return ((MergablePropertyAttribute) obj).AllowMerge == mergable;
		}

		public override int GetHashCode()
		{
			return mergable.GetHashCode ();
		}

		public override bool IsDefaultAttribute()
		{
			return mergable == MergablePropertyAttribute.Default.AllowMerge;
		}
	}
}
