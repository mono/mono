//
// System.ComponentModel.WeakObjectWrapperComparer.cs
//
// Authors:
//   Marek Habersack <mhabersack@novell.com>
// 
//
// (C) 2009 Novell, Inc (http://novell.com)
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
using System.Collections;
using System.Collections.Generic;

namespace System.ComponentModel
{
	sealed class WeakObjectWrapperComparer : EqualityComparer <WeakObjectWrapper>
	{
		public override bool Equals (WeakObjectWrapper x, WeakObjectWrapper y)
		{
			if (x == null && y == null)
				return false;

			if (x == null || y == null)
				return false;

			WeakReference xWeak = x.Weak;
			WeakReference yWeak = y.Weak;
			
			if (!xWeak.IsAlive && !yWeak.IsAlive)
				return false;

			return xWeak.Target == yWeak.Target;
		}

		public override int GetHashCode (WeakObjectWrapper obj)
		{
			if (obj == null)
				return 0;

			return obj.TargetHashCode;
		}
	}
}
