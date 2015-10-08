//
// System.MultiCastDelegate.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) Ximian, Inc.  http://www.ximian.com
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

using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace System
{
	[System.Runtime.InteropServices.ComVisible (true)]
	[Serializable]
	[StructLayout (LayoutKind.Sequential)]
	public abstract class MulticastDelegate : Delegate
	{
		MulticastData multicast;

		protected MulticastDelegate (object target, string method)
			: base (target, method)
		{
		}

		protected MulticastDelegate (Type target, string method)
			: base (target, method)
		{
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData  (info, context);
		}

		protected sealed override object DynamicInvokeImpl (object[] args)
		{
			if (multicast == null) {
				return base.DynamicInvokeImpl (args);
			} else {
				object r;
				int i = multicast.offset, last = i + multicast.count;
				do {
					r = multicast.delegates [i].DynamicInvoke (args);
				} while (++i < last);
				return r;
			}
		}

		// Some high-performance applications use this internal property
		// to avoid using a slow path to determine if there is more than one handler
		// This brings an API that we removed in f410e545e2db0e0dc338673a6b10a5cfd2d3340f
		// which some users depeneded on
		//
		// This is an example of code that used this:
		// https://gist.github.com/migueldeicaza/cd99938c2a4372e7e5d5
		//
		// Do not remove this API
		internal bool HasSingleTarget {
			get { return multicast == null; }
		}

		// <remarks>
		//   Equals: two multicast delegates are equal if their base is equal
		//   and their invocations list is equal.
		// </remarks>
		public sealed override bool Equals (object obj)
		{
			if (!base.Equals (obj))
				return false;

			MulticastDelegate d = obj as MulticastDelegate;
			if (d == null)
				return false;

			if (multicast == null && d.multicast == null) {
				return true;
			} else if (multicast == null ^ d.multicast == null) {
				return false;
			} else {
				if (multicast.count != d.multicast.count)
					return false;

				for (int i = 0; i < multicast.count; ++i) {
					if (!multicast.delegates [multicast.offset + i].Equals (d.multicast.delegates [d.multicast.offset + i]))
						return false;
				}

				return true;
			}
		}

		//
		// FIXME: This could use some improvements.
		//
		public sealed override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		// <summary>
		//   Return, in order of invocation, the invocation list
		//   of a MulticastDelegate
		// </summary>
		public sealed override Delegate[] GetInvocationList ()
		{
			if (this.multicast == null)
				return new Delegate[1] { this };

			Delegate [] ret = new Delegate [this.multicast.count];
			for (int i = 0; i < this.multicast.count; ++i)
				ret [i] = this.multicast.delegates [this.multicast.offset + i];

			return ret;
		}

		// <summary>
		//   Combines this MulticastDelegate with the (Multicast)Delegate `follow'.
		//   This does _not_ combine with Delegates. ECMA states the whole delegate
		//   thing should have better been a simple System.Delegate class.
		//   Compiler generated delegates are always MulticastDelegates.
		// </summary>
		protected sealed override Delegate CombineImpl (Delegate follow)
		{
			int i, len;

			if (follow == null)
				return this;

			MulticastDelegate other = (MulticastDelegate) follow;

			MulticastDelegate ret = AllocDelegateLike_internal (this);

			if (multicast == null && other.multicast == null) {
				len = GetDelegatesArraySize (2);

				ret.multicast = new MulticastData {
					offset = 0,
					count = 2,
					delegates = new Delegate [len],
				};

				ret.multicast.delegates [0] = this;
				ret.multicast.delegates [1] = other;
			} else if (this.multicast == null) {
				if (other.multicast.offset > 0) {
					ret.multicast = new MulticastData {
						delegates = other.multicast.delegates,
						offset = other.multicast.offset - 1,
						count = other.multicast.count + 1,
					};

					ret.multicast.delegates [ret.multicast.offset] = this;
				} else {
					len = GetDelegatesArraySize (1 + other.multicast.count);

					ret.multicast = new MulticastData {
						offset = 0,
						count = 1 + other.multicast.count,
						delegates = new Delegate [len],
					};

					ret.multicast.delegates [0] = this;
					for (i = 0; i < other.multicast.count; ++i)
						ret.multicast.delegates [1 + i] = other.multicast.delegates [other.multicast.offset + i];
				}
			} else if (other.multicast == null) {
				if (this.multicast.offset + this.multicast.count < this.multicast.delegates.Length) {
					ret.multicast = new MulticastData {
						offset = this.multicast.offset,
						count = this.multicast.count + 1,
						delegates = this.multicast.delegates,
					};

					ret.multicast.delegates [ret.multicast.offset + ret.multicast.count - 1] = other;
				} else {
					len = GetDelegatesArraySize (this.multicast.count + 1);

					ret.multicast = new MulticastData {
						offset = 0,
						count = this.multicast.count + 1,
						delegates = new Delegate [len],
					};

					for (i = 0; i < multicast.count; ++i)
						ret.multicast.delegates [i] = this.multicast.delegates [this.multicast.offset + i];
					ret.multicast.delegates [ret.multicast.count - 1] = other;
				}
			} else {
				if (this.multicast.offset + this.multicast.count + other.multicast.count < this.multicast.delegates.Length - 1) {
					/* use this.delegates */

					ret.multicast = new MulticastData {
						offset = this.multicast.offset,
						count = this.multicast.count + other.multicast.count,
						delegates = this.multicast.delegates,
					};

					for (i = 0; i < other.multicast.count; ++i)
						ret.multicast.delegates [this.multicast.offset + this.multicast.count + i] = other.multicast.delegates [other.multicast.offset + i];
				} else if (this.multicast.count < other.multicast.offset - 1) {
					/* use other.delegates */

					ret.multicast = new MulticastData {
						offset = other.multicast.offset - this.multicast.count,
						count = other.multicast.count + this.multicast.count,
						delegates = other.multicast.delegates,
					};

					for (i = 0; i < this.multicast.count; ++i)
						ret.multicast.delegates [other.multicast.offset - this.multicast.count + i] = this.multicast.delegates [this.multicast.offset + i];
				} else {
					len = GetDelegatesArraySize (this.multicast.count + other.multicast.count);

					ret.multicast = new MulticastData {
						offset = 0,
						count = this.multicast.count + other.multicast.count,
						delegates = new Delegate [len],
					};

					for (i = 0; i < this.multicast.count; ++i)
						ret.multicast.delegates [i] = this.multicast.delegates [this.multicast.offset + i];
					for (i = 0; i < other.multicast.count; ++i)
						ret.multicast.delegates [this.multicast.count + i] = other.multicast.delegates [other.multicast.offset + i];
				}
			}

			return ret;
		}

		protected sealed override Delegate RemoveImpl (Delegate value)
		{
			int i, j, len;

			if (value == null)
				return this;

			MulticastDelegate other = (MulticastDelegate) value;

			if (this.multicast == null && other.multicast == null) {
				/* if they are not equal and the current one is not
				 * a multicastdelegate then we cannot delete it */
				return this.Equals (other) ? null : this;
			} else if (this.multicast == null) {
				for (i = 0; i < other.multicast.count; ++i) {
					if (this.Equals (other.multicast.delegates [other.multicast.offset + i]))
						return null;
				}
				return this;
			} else if (other.multicast == null) {
				int idx = Array.LastIndexOf (this.multicast.delegates, other, this.multicast.offset + this.multicast.count - 1, this.multicast.count);
				if (idx == -1)
					return this;

				if (this.multicast.delegates.Length <= 1) {
					/* delegates.Length should never be equal or lower than 1, it should be 2 or greater */
					throw new InvalidOperationException ();
				}

				if (this.multicast.count <= 1) {
					/* count should never be equal or lower than 1, it should be 2 or greater */
					throw new InvalidOperationException ();
				}

				if (this.multicast.count == 2)
					return this.multicast.delegates [idx == this.multicast.offset ? this.multicast.offset + 1 : this.multicast.offset];

				MulticastDelegate ret = AllocDelegateLike_internal (this);

				if (idx == 0) {
					ret.multicast = new MulticastData {
						offset = this.multicast.offset + 1,
						count = this.multicast.count - 1,
						delegates = this.multicast.delegates,
					};
				} else if (idx == this.multicast.offset + this.multicast.count - 1) {
					ret.multicast = new MulticastData {
						offset = this.multicast.offset,
						count = this.multicast.count - 1,
						delegates = this.multicast.delegates,
					};
				} else {
					len = GetDelegatesArraySize (this.multicast.count - 1);

					ret.multicast = new MulticastData {
						offset = 0,
						count = this.multicast.count - 1,
						delegates = new Delegate [len],
					};

					for (i = 0, j = 0; i < this.multicast.count; ++i) {
						if (i != idx)
							ret.multicast.delegates [j++] = this.multicast.delegates [this.multicast.offset + i];
					}
				}

				return ret;
			} else {
				len = GetDelegatesArraySize (this.multicast.count);

				/* wild case : remove MulticastDelegate from MulticastDelegate
				 * complexity is O(m * n), with n the number of elements in
				 * this.delegates and m the number of elements in other.delegates */
				MulticastDelegate ret = AllocDelegateLike_internal (this);

				ret.multicast = new MulticastData {
					offset = 0,
					count = this.multicast.count,
					delegates = new Delegate [len],
				};

				/* we should use a set with O(1) lookup complexity
				 * but HashSet is implemented in System.Core.dll */
				List<Delegate> other_delegates = new List<Delegate> ();
				for (i = 0; i < other.multicast.count; ++i)
					other_delegates.Add (other.multicast.delegates [other.multicast.offset + i]);

				int idx = this.multicast.count;

				/* we need to remove elements from the end to the beginning, as
				 * the addition and removal of delegates behaves like a stack */
				for (i = this.multicast.count - 1; i >= 0; --i) {
					/* if delegates[i] is not in other_delegates,
					 * then we can safely add it to ret.delegates
					 * otherwise we remove it from other_delegates */
					if (!other_delegates.Remove (this.multicast.delegates [this.multicast.offset + i]))
						ret.multicast.delegates [--idx] = this.multicast.delegates [this.multicast.offset + i];
				}

				/* the elements are at the end of the array, we
				 * need to move them back to the beginning of it */
				ret.multicast.count -= idx;
				Array.Copy (ret.multicast.delegates, idx, ret.multicast.delegates, 0, ret.multicast.count);

				if (ret.multicast.count == 0)
					return null;

				if (ret.multicast.count == 1)
					return ret.multicast.delegates [0];

				len = GetDelegatesArraySize (ret.multicast.count);

				if (len != ret.multicast.delegates.Length)
					Array.Resize (ref ret.multicast.delegates, len);

				return ret;
			}
		}

		public static bool operator == (MulticastDelegate d1, MulticastDelegate d2)
		{
			if (d1 == null)
				return d2 == null;

			return d1.Equals (d2);
		}

		public static bool operator != (MulticastDelegate d1, MulticastDelegate d2)
		{
			if (d1 == null)
				return d2 != null;

			return !d1.Equals (d2);
		}

		static int GetDelegatesArraySize (int n)
		{
			if (n <= 0)
				throw new ArgumentException ("n");

			return (int) Math.Pow (2, ((int) Math.Floor (Math.Log (n, 2))) + 1);
		}

		[StructLayout (LayoutKind.Sequential)]
		class MulticastData
		{
			internal Delegate [] delegates;
			internal int offset;
			internal int count;
		}
	}
}
