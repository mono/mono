using System;

namespace SimpleJit.Metadata
{
	public struct ClrType {
		private System.RuntimeTypeHandle rttype;

		internal ClrType (System.RuntimeTypeHandle rttype) {
			this.rttype = rttype;
		}

		/// Escape hatch, use sparingly
		public Type AsSystemType { get => Type.GetTypeFromHandle (rttype); }

		public override bool Equals (object obj)
		{
			if (obj == null || GetType () != obj.GetType ())
				return false;
			return rttype.Equals (((ClrType)obj).rttype);
		}

		public bool Equals (ClrType other)
		{
			return rttype.Equals (other.rttype);
		}

		public override int GetHashCode ()
		{
			return rttype.GetHashCode ();
		}

		public static bool operator == (ClrType left, ClrType right)
		{
			return left.Equals (right);
		}

		public static bool operator != (ClrType left, ClrType right)
		{
			return !left.Equals (right);
		}

	}
}
