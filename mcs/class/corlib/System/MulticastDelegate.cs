//
// System.MultiCastDelegate.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO: Remove Missing
//

using System.Collections;
using System.Globalization;

namespace System {

	public abstract class MulticastDelegate : Delegate
	{
		private MulticastDelegate prev;
		private MulticastDelegate kpm_next;

		protected MulticastDelegate (object target, string method)
			: base (target, method)
		{
			prev = null;
		}

		protected MulticastDelegate (Type target_type, string method)
			: base (target_type, method)
		{
			prev = null;
		}

#if NOTYET
		private MulticastDelegate (Type target_type, string method, Delegate [] list)
			: base (target_type, method)
		{
			invocation_list = (Delegate[])list.Clone ();
		}
#endif
		
#if NOTYET
		public MethodInfo Method {
			get {
				return null;
			}
		}
#endif

		public override object DynamicInvokeImpl( object[] args )
		{
			if ( prev != null )
				prev.DynamicInvokeImpl( args );

			return base.DynamicInvokeImpl( args );
		}

		// <remarks>
		//   Equals: two multicast delegates are equal if their base is equal
		//   and their invocations list is equal.
		// </remarks>
		public override bool Equals (object o)
		{
			if ( ! base.Equals( o ) )
				return false;

			MulticastDelegate d = (MulticastDelegate) o;

			if ( this.prev == null ) {
				if ( d.prev == null )
					return true;
				else
					return false;
			}

			return this.prev.Equals( d.prev );
		}
		
		//
		// FIXME: This could use some improvements.
		//
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
		
		// <summary>
		//   Return, in order of invocation, the invocation list
		//   of a MulticastDelegate
		// </summary>
		public override Delegate[] GetInvocationList()
		{
			MulticastDelegate d;
			for (d = (MulticastDelegate) this.Clone (); d.prev != null; d = d.prev)
				d.prev.kpm_next = d;

			if (d.kpm_next == null) {
				MulticastDelegate other = (MulticastDelegate) d.Clone ();
				other.prev = null;
				other.kpm_next = null;				
				return new Delegate [1] { other };
			}
			ArrayList list = new ArrayList ();
			for (; d != null; d = d.kpm_next) {
				MulticastDelegate other = (MulticastDelegate) d.Clone ();
				other.prev = null;
				other.kpm_next = null;
				list.Add (other);
			}

			return (Delegate []) list.ToArray (typeof (Delegate));
		}

		// <summary>
		//   Combines this MulticastDelegate with the (Multicast)Delegate `follow'.
		//   This does _not_ combine with Delegates. ECMA states the whole delegate
		//   thing should have better been a simple System.Delegate class.
		//   Compiler generated delegates are always MulticastDelegates.
		// </summary>
		protected override Delegate CombineImpl( Delegate follow )
		{
			MulticastDelegate combined, orig, clone;
			
			if ( this.GetType() != follow.GetType() )
				throw new ArgumentException( Locale.GetText("Incompatible Delegate Types") );

			combined = (MulticastDelegate)follow.Clone();

			for ( clone = combined, orig = ((MulticastDelegate)follow).prev;
			      orig != null; orig = orig.prev ) {
				
				clone.prev = (MulticastDelegate)orig.Clone();
				clone = clone.prev;
			}

			clone.prev = (MulticastDelegate)this.Clone();

			for ( clone = clone.prev, orig = this.prev;
			      orig != null; orig = orig.prev ) {

				clone.prev = (MulticastDelegate)orig.Clone();
				clone = clone.prev;
			}

			return combined;
		}
		
		private bool BaseEquals( MulticastDelegate value )
		{
			return base.Equals( value );
		}

		/* 
		 * Perform a slightly crippled version of
		 * Knuth-Pratt-Morris over MulticastDelegate chains.
		 * Border values are set as pointers in kpm_next;
		 * Generally, KPM border arrays are length n+1 for
		 * strings of n. This one works with length n at the
		 * expense of a few additional comparisions.
		 */
		private static MulticastDelegate KPM( MulticastDelegate needle,
						      MulticastDelegate haystack,
						      out MulticastDelegate tail )
		{ 
			MulticastDelegate nx, hx;
			
			// preprocess
			hx = needle;
			nx = needle.kpm_next = null;
			do {
				while ( nx != null && !nx.BaseEquals(hx) )
					nx = nx.kpm_next;

				hx = hx.prev;
				if (hx == null)
					break;
					
				nx = nx == null ? needle : nx.prev;
				if ( hx.BaseEquals(nx) )
					hx.kpm_next = nx.kpm_next;
				else
					hx.kpm_next = nx;

			} while (true);

			// match
			MulticastDelegate match = haystack;
			nx = needle;
			hx = haystack;
			do {
				while ( nx != null && !nx.BaseEquals(hx) ) {
					nx = nx.kpm_next;
					match = match.prev;
				}

				nx = nx == null ? needle : nx.prev;
				if ( nx == null ) {
					// bingo
					tail = hx.prev;
					return match;
				}
				
				hx = hx.prev;
			} while ( hx != null );

			tail = null;
			return null;
		}

		protected override Delegate RemoveImpl( Delegate value )
		{
			if ( value == null )
				return this;

			// match this with value
			MulticastDelegate head, tail;
			head = KPM((MulticastDelegate)value, this, out tail);
			if ( head == null )
				return this;
			
			// duplicate chain without head..tail
			MulticastDelegate prev = null, retval = null, orig;
			for ( orig = this; (object)orig != (object)head; orig = orig.prev ) {
				MulticastDelegate clone = (MulticastDelegate)orig.Clone();
				if ( prev != null )
					prev.prev = clone;
				else
					retval = clone;
				prev = clone;
			}
			for ( orig = tail; (object)orig != null; orig = orig.prev ) {
				MulticastDelegate clone = (MulticastDelegate)orig.Clone();
				if ( prev != null )
					prev.prev = clone;
				else
					retval = clone;
				prev = clone;
			}
			if ( prev != null )
				prev.prev = null;

			return retval;
		}

		public static bool operator == (MulticastDelegate a, MulticastDelegate b) 
		{
			if ((object)a == null) {
				if ((object)b == null)
					return true;
				return false;
			}
			return a.Equals (b);
		}
		
		public static bool operator != (MulticastDelegate a, MulticastDelegate b) 
		{
			return !(a == b);
		}
	}
}
