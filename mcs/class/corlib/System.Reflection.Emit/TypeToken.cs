// TypeToken.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection.Emit {


	/// <summary>
	///  Represents the Token returned by the metadata to represent a Type.
	/// </summary>
	public struct TypeToken {

		internal int tokValue;

		public static readonly TypeToken Empty;


		static TypeToken ()
		{
			Empty = new TypeToken ();
		}


		internal TypeToken (int val)
		{
			tokValue = val;
		}



		/// <summary>
		/// </summary>
		public override bool Equals (object obj)
		{
			bool res = obj is TypeToken;

			if (res) {
				TypeToken that = (TypeToken) obj;
				res = (this.tokValue == that.tokValue);
			}

			return res;
		}


		/// <summary>
		///  Tests whether the given object is an instance of
		///  TypeToken and has the same token value.
		/// </summary>
		public override int GetHashCode ()
		{
			return tokValue;
		}


		/// <summary>
		///  Returns the metadata token for this Type.
		/// </summary>
		public int Token {
			get {
				return tokValue;
			}
		}

	}

}

