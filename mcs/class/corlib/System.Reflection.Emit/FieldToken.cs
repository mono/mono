// FieldToken.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection.Emit {


	/// <summary>
	///  Represents the Token returned by the metadata to represent a Field.
	/// </summary>
	public struct FieldToken {

		internal int tokValue;

		public static readonly FieldToken Empty;


		static FieldToken ()
		{
			Empty = new FieldToken ();
		}


		internal FieldToken (int val)
		{
			tokValue = val;
		}



		/// <summary>
		/// </summary>
		public override bool Equals (object obj)
		{
			bool res = obj is FieldToken;

			if (res) {
				FieldToken that = (FieldToken) obj;
				res = (this.tokValue == that.tokValue);
			}

			return res;
		}


		/// <summary>
		///  Tests whether the given object is an instance of
		///  FieldToken and has the same token value.
		/// </summary>
		public override int GetHashCode ()
		{
			return tokValue;
		}


		/// <summary>
		///  Returns the metadata token for this Field.
		/// </summary>
		public int Token {
			get {
				return tokValue;
			}
		}

	}

}

