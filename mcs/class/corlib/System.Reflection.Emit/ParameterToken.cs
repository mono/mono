// ParameterToken.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection.Emit {


	/// <summary>
	///  Represents the Token returned by the metadata to represent a Parameter.
	/// </summary>
	[Serializable]
	public struct ParameterToken {

		internal int tokValue;

		public static readonly ParameterToken Empty;


		static ParameterToken ()
		{
			Empty = new ParameterToken ();
		}


		internal ParameterToken (int val)
		{
			tokValue = val;
		}



		/// <summary>
		/// </summary>
		public override bool Equals (object obj)
		{
			bool res = obj is ParameterToken;

			if (res) {
				ParameterToken that = (ParameterToken) obj;
				res = (this.tokValue == that.tokValue);
			}

			return res;
		}


		/// <summary>
		///  Tests whether the given object is an instance of
		///  ParameterToken and has the same token value.
		/// </summary>
		public override int GetHashCode ()
		{
			return tokValue;
		}


		/// <summary>
		///  Returns the metadata token for this Parameter.
		/// </summary>
		public int Token {
			get {
				return tokValue;
			}
		}

	}

}

