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
		Delegate[] delegates;

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
			if (delegates == null) {
				return base.DynamicInvokeImpl (args);
			} else {
				object r;
				int i = 0, len = delegates.Length;
				do {
					r = delegates [i].DynamicInvoke (args);
				} while (++i < len);
				return r;
			}
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

			if (delegates == null && d.delegates == null) {
				return true;
			} else if (delegates == null ^ d.delegates == null) {
				return false;
			} else {
				if (delegates.Length != d.delegates.Length)
					return false;

				for (int i = 0; i < delegates.Length; ++i) {
					if (!delegates [i].Equals (d.delegates [i]))
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
			if (delegates != null)
				return (Delegate[]) delegates.Clone ();
			else
				return new Delegate[1] { this };
		}

		// <summary>
		//   Combines this MulticastDelegate with the (Multicast)Delegate `follow'.
		//   This does _not_ combine with Delegates. ECMA states the whole delegate
		//   thing should have better been a simple System.Delegate class.
		//   Compiler generated delegates are always MulticastDelegates.
		// </summary>
		protected sealed override Delegate CombineImpl (Delegate follow)
		{
			if (follow == null)
				return this;

			MulticastDelegate other = (MulticastDelegate) follow;

			MulticastDelegate ret = AllocDelegateLike_internal (this);

			if (delegates == null && other.delegates == null) {
				ret.delegates = new Delegate [2] { this, other };
			} else if (delegates == null) {
				ret.delegates = new Delegate [1 + other.delegates.Length];

				ret.delegates [0] = this;
				Array.Copy (other.delegates, 0, ret.delegates, 1, other.delegates.Length);
			} else if (other.delegates == null) {
				ret.delegates = new Delegate [delegates.Length + 1];

				Array.Copy (delegates, 0, ret.delegates, 0, delegates.Length);
				ret.delegates [ret.delegates.Length - 1] = other;
			} else {
				ret.delegates = new Delegate [delegates.Length + other.delegates.Length];

				Array.Copy (delegates, 0, ret.delegates, 0, delegates.Length);
				Array.Copy (other.delegates, 0, ret.delegates, delegates.Length, other.delegates.Length);
			}

			return ret;
		}

		protected sealed override Delegate RemoveImpl (Delegate value)
		{
			if (value == null)
				return this;

			MulticastDelegate other = (MulticastDelegate) value;

			if (delegates == null && other.delegates == null) {
				/* if they are not equal and the current one is not
				 * a multicastdelegate then we cannot delete it */
				return this.Equals (other) ? null : this;
			} else if (delegates == null) {
				foreach (var d in other.delegates) {
					if (this.Equals (d))
						return null;
				}
				return this;
			} else if (other.delegates == null) {
				int idx = Array.LastIndexOf (delegates, other);
				if (idx == -1)
					return this;

				if (delegates.Length <= 1) {
					/* delegates.Length should never be equal or
					 * lower than 1, it should be 2 or greater */
					throw new InvalidOperationException ();
				}

				if (delegates.Length == 2)
					return delegates [idx == 0 ? 1 : 0];

				MulticastDelegate ret = AllocDelegateLike_internal (this);
				ret.delegates = new Delegate [delegates.Length - 1];

				Array.Copy (delegates, ret.delegates, idx);
				Array.Copy (delegates, idx + 1, ret.delegates, idx, delegates.Length - idx - 1);

				return ret;
			} else {
				/* wild case : remove MulticastDelegate from MulticastDelegate
				 * complexity is O(m * n), with n the number of elements in
				 * this.delegates and m the number of elements in other.delegates */
				MulticastDelegate ret = AllocDelegateLike_internal (this);
				ret.delegates = new Delegate [delegates.Length];

				/* we should use a set with O(1) lookup complexity
				 * but HashSet is implemented in System.Core.dll */
				List<Delegate> other_delegates = new List<Delegate> ();
				for (int i = 0; i < other.delegates.Length; ++i)
					other_delegates.Add (other.delegates [i]);

				int idx = delegates.Length;

				/* we need to remove elements from the end to the beginning, as
				 * the addition and removal of delegates behaves like a stack */
				for (int i = delegates.Length - 1; i >= 0; --i) {
					/* if delegates[i] is not in other_delegates,
					 * then we can safely add it to ret.delegates
					 * otherwise we remove it from other_delegates */
					if (!other_delegates.Remove (delegates [i]))
						ret.delegates [--idx] = delegates [i];
				}

				/* the elements are at the end of the array, we
				 * need to move them back to the beginning of it */
				int count = delegates.Length - idx;
				Array.Copy (ret.delegates, idx, ret.delegates, 0, count);

				if (count == 0)
					return null;

				if (count == 1)
					return ret.delegates [0];

				if (count != delegates.Length)
					Array.Resize (ref ret.delegates, count);

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
	}
}
