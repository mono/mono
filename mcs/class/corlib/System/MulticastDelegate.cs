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

using System.Globalization;

namespace System {

	public abstract class MulticastDelegate : Delegate
	{
		private MulticastDelegate prev;

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

			MulticastDelegate d = this;
			MulticastDelegate c = (MulticastDelegate) o;
			do {
				if ( d != c )
					return false;
				
				c = c.prev;
				d = d.prev;
			} while ( (object)d != null );
		
			if ( (object)c == null )
				return true;

			return false;
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
			throw new NotImplementedException();
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
			      (object)orig != null; orig = orig.prev ) {

				clone.prev = (MulticastDelegate)orig.Clone();
				clone = clone.prev;
			}

			clone.prev = (MulticastDelegate)this.Clone();

			for ( clone = clone.prev, orig = this.prev;
			      (object)orig != null; orig = orig.prev ) {

				clone.prev = (MulticastDelegate)orig.Clone();
				clone = clone.prev;
			}

			return combined;
		}

		protected override Delegate RemoveImpl( Delegate value )
		{
			throw new NotImplementedException();
		}

		public static bool operator == (MulticastDelegate a, MulticastDelegate b) {
			if ((object)a == null) {
				if ((object)b == null)
					return true;
				return false;
			}
			return a.Equals (b);
		}
		
		public static bool operator != (MulticastDelegate a, MulticastDelegate b) {
			return !(a == b);
		}
	}
}
