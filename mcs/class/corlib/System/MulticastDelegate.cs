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

namespace System {

	public abstract class MulticastDelegate : Delegate {

		protected MulticastDelegate (object target, string method)
			: base (target, method)
		{
		}

		protected MulticastDelegate (Type target_type, string method)
			: base (target_type, method)
		{
		}

#if NOTYET
		public MethodInfo Method {
			get {
				return null;
			}
		}
#endif

		//
		// Methods
		//
		public override bool Equals (object o)
		{
			if (!(o is System.MulticastDelegate))
				return false;

			return base.Equals (o);
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		
	}
}
