//
// System.MultiCastDelegate.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO: Mucho left to implement.
//

using System.Globalization;
namespace System {

	public abstract class MulticastDelegate : Delegate {

		Delegate [] invocation_list;
		
		protected MulticastDelegate (object target, string method)
			: base (target, method)
		{
			invocation_list = null;
		}

		protected MulticastDelegate (Type target_type, string method)
			: base (target_type, method)
		{
			invocation_list = null;
		}

		private MulticastDelegate (Type target_type, string method, Delegate [] list)
			: base (target_type, method)
		{
			invocation_list = (Delegate[])list.Clone ();
		}
		
#if NOTYET
		public MethodInfo Method {
			get {
				return null;
			}
		}
#endif

		// <remarks>
		//   Equals: two multicast delegates are equal if their base is equal
		//   and their invocations list is equal.
		// </remarks>
		public override bool Equals (object o)
		{
			if (o == null)
				return false;

			if (!(o is System.MulticastDelegate))
				return false;

			if (!base.Equals (o))
				return false;

			MulticastDelegate d = (MulticastDelegate) o;

			if (d.invocation_list == null){
				if (invocation_list == null)
					return true;
				return false;
			} else if (invocation_list == null)
				return false;

			int i = 0;
			foreach (Delegate del in invocation_list){
				if (del != d.invocation_list [i++])
					return false;
			}
			
			return true;
		}

		//
		// FIXME: This could use some improvements.
		//
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		// <summary>
		//   Combines this MulticastDelegate with the Delegate `follow'.
		//   This can combine MulticastDelegates and Delegates
		// </summary>
		[MonoTODO]
		protected override Delegate CombineImpl (Delegate follow)
		{
			
			throw new NotImplementedException ();

			// FIXME: Implement me.
			// This is not as simple to implement, as we can
			// not create an instance of MulticastDelegate.
			//
			// Got to think more about this.
		}

		public static bool operator == (MulticastDelegate a, MulticastDelegate b) {
			if ((object)a == null) {
				if ((object)b == null)
					return false;
				return false;
			}
			return a.Equals (b);
		}

		public static bool operator != (MulticastDelegate a, MulticastDelegate b) {
			return !(a == b);
		}
	}
}
