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
		private MulticastDelegate prev;
		private MulticastDelegate kpm_next;

		protected MulticastDelegate (object target, string method)
			: base (target, method)
		{
			prev = null;
		}

		protected MulticastDelegate (Type target, string method)
			: base (target, method)
		{
			prev = null;
		}
		
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData  (info, context);
		}


		protected sealed override object DynamicInvokeImpl (object[] args)
		{
			if (prev != null)
				prev.DynamicInvokeImpl (args);

			return base.DynamicInvokeImpl (args);
		}

		internal bool HasSingleTarget {
			get { return prev == null; }
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

			MulticastDelegate this_prev = this.prev;
			MulticastDelegate obj_prev = d.prev;

			do {
				if (this_prev == null)
					return obj_prev == null;

				if (!this_prev.Compare (obj_prev))
					return false;
				
				this_prev = this_prev.prev;
				obj_prev = obj_prev.prev;
			} while (true);
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
			MulticastDelegate d;
			d = (MulticastDelegate) this.Clone ();
			for (d.kpm_next = null; d.prev != null; d = d.prev)
				d.prev.kpm_next = d;

			if (d.kpm_next == null) {
				MulticastDelegate other = (MulticastDelegate) d.Clone ();
				other.prev = null;
				other.kpm_next = null;				
				return new Delegate [1] { other };
			}

			var list = new List<Delegate> ();
			for (; d != null; d = d.kpm_next) {
				MulticastDelegate other = (MulticastDelegate) d.Clone ();
				other.prev = null;
				other.kpm_next = null;
				list.Add (other);
			}

			return list.ToArray ();
		}

		// <summary>
		//   Combines this MulticastDelegate with the (Multicast)Delegate `follow'.
		//   This does _not_ combine with Delegates. ECMA states the whole delegate
		//   thing should have better been a simple System.Delegate class.
		//   Compiler generated delegates are always MulticastDelegates.
		// </summary>
		protected sealed override Delegate CombineImpl (Delegate follow)
		{
			MulticastDelegate combined, orig, clone;

			if (this.GetType() != follow.GetType ())
				throw new ArgumentException (Locale.GetText ("Incompatible Delegate Types. First is {0} second is {1}.", this.GetType ().FullName, follow.GetType ().FullName));

			combined = (MulticastDelegate)follow.Clone ();
			combined.SetMulticastInvoke ();

			for (clone = combined, orig = ((MulticastDelegate)follow).prev; orig != null; orig = orig.prev) {
				
				clone.prev = (MulticastDelegate)orig.Clone ();
				clone = clone.prev;
			}

			clone.SetMulticastInvoke ();
			clone.prev = (MulticastDelegate)this.Clone ();

			for (clone = clone.prev, orig = this.prev; orig != null; orig = orig.prev) {

				clone.prev = (MulticastDelegate)orig.Clone ();
				clone = clone.prev;
			}

			return combined;
		}

		private bool BaseEquals (MulticastDelegate value)
		{
			return base.Equals (value);
		}

		/* 
		 * Perform a slightly crippled version of
		 * Knuth-Pratt-Morris over MulticastDelegate chains.
		 * Border values are set as pointers in kpm_next;
		 * Generally, KPM border arrays are length n+1 for
		 * strings of n. This one works with length n at the
		 * expense of a few additional comparisions.
		 */
		private static MulticastDelegate KPM (MulticastDelegate needle, MulticastDelegate haystack,
		                                      out MulticastDelegate tail)
		{
			MulticastDelegate nx, hx;

			// preprocess
			hx = needle;
			nx = needle.kpm_next = null;
			do {
				while ((nx != null) && (!nx.BaseEquals (hx)))
					nx = nx.kpm_next;

				hx = hx.prev;
				if (hx == null)
					break;
					
				nx = nx == null ? needle : nx.prev;
				if (hx.BaseEquals (nx))
					hx.kpm_next = nx.kpm_next;
				else
					hx.kpm_next = nx;

			} while (true);

			// match
			MulticastDelegate match = haystack;
			nx = needle;
			hx = haystack;
			do {
				while (nx != null && !nx.BaseEquals (hx)) {
					nx = nx.kpm_next;
					match = match.prev;
				}

				nx = nx == null ? needle : nx.prev;
				if (nx == null) {
					// bingo
					tail = hx.prev;
					return match;
				}

				hx = hx.prev;
			} while (hx != null);

			tail = null;
			return null;
		}

		protected sealed override Delegate RemoveImpl (Delegate value)
		{
			if (value == null)
				return this;

			// match this with value
			MulticastDelegate head, tail;
			head = KPM ((MulticastDelegate)value, this, out tail);
			if (head == null)
				return this;

			// duplicate chain without head..tail
			MulticastDelegate prev = null, retval = null, orig;
			for (orig = this; (object)orig != (object)head; orig = orig.prev) {
				MulticastDelegate clone = (MulticastDelegate)orig.Clone ();
				if (prev != null)
					prev.prev = clone;
				else
					retval = clone;
				prev = clone;
			}
			for (orig = tail; (object)orig != null; orig = orig.prev) {
				MulticastDelegate clone = (MulticastDelegate)orig.Clone ();
				if (prev != null)
					prev.prev = clone;
				else
					retval = clone;
				prev = clone;
			}
			if (prev != null)
				prev.prev = null;

			return retval;
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
