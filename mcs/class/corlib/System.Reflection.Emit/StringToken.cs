// StringToken.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection.Emit {


	/// <summary>
	///  Represents the Token returned by the metadata to represent a String.
	/// </summary>
	[Serializable]
	public struct StringToken {

		internal int tokValue;

		static StringToken ()
		{
		}


		internal StringToken (int val)
		{
			tokValue = val;
		}



		/// <summary>
		/// </summary>
		public override bool Equals (object obj)
		{
			bool res = obj is StringToken;

			if (res) {
				StringToken that = (StringToken) obj;
				res = (this.tokValue == that.tokValue);
			}

			return res;
		}


		/// <summary>
		///  Tests whether the given object is an instance of
		///  StringToken and has the same token value.
		/// </summary>
		public override int GetHashCode ()
		{
			return tokValue;
		}


		/// <summary>
		///  Returns the metadata token for this String.
		/// </summary>
		public int Token {
			get {
				return tokValue;
			}
		}

	}

}

