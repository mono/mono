//
// System.Web.UI.PersistChildrenAttribute.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
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

namespace System.Web.UI {

	[AttributeUsage (AttributeTargets.Class)]
	public sealed class PersistChildrenAttribute : Attribute
	{
		bool persist;
		
		public PersistChildrenAttribute (bool persist)
		{
			this.persist = persist;
		}

		public static readonly PersistChildrenAttribute Default = new PersistChildrenAttribute (true);
		public static readonly PersistChildrenAttribute Yes = new PersistChildrenAttribute (true);
		public static readonly PersistChildrenAttribute No = new PersistChildrenAttribute (false);

		public bool Persist {
			get { return persist; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is PersistChildrenAttribute))
				return false;

			return (((PersistChildrenAttribute) obj).persist == persist);
		}

		public override int GetHashCode ()
		{
			return persist ? 1 : 0;
		}

		public override bool IsDefaultAttribute ()
		{
			return (persist == true);
		}
	}
}

