// MethodToken.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection.Emit {


	/// <summary>
	///  Represents the Token returned by the metadata to represent a Method.
	/// </summary>
	public struct MethodToken {

		internal int tokValue;

		public static readonly MethodToken Empty;


		static MethodToken ()
		{
			Empty = new MethodToken ();
		}


		internal MethodToken (int val)
		{
			tokValue = val;
		}



		/// <summary>
		/// </summary>
		public override bool Equals (object obj)
		{
			bool res = obj is MethodToken;

			if (res) {
				MethodToken that = (MethodToken) obj;
				res = (this.tokValue == that.tokValue);
			}

			return res;
		}


		/// <summary>
		///  Tests whether the given object is an instance of
		///  MethodToken and has the same token value.
		/// </summary>
		public override int GetHashCode ()
		{
			return tokValue;
		}


		/// <summary>
		///  Returns the metadata token for this Method.
		/// </summary>
		public int Token {
			get {
				return tokValue;
			}
		}

	}

}

