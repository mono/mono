// PropertyToken.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection.Emit {


	/// <summary>
	///  Represents the Token returned by the metadata to represent a Property.
	/// </summary>
	public struct PropertyToken {

		internal int tokValue;

		public static readonly PropertyToken Empty;


		static PropertyToken ()
		{
			Empty = new PropertyToken ();
		}


		internal PropertyToken (int val)
		{
			tokValue = val;
		}



		/// <summary>
		/// </summary>
		public override bool Equals (object obj)
		{
			bool res = obj is PropertyToken;

			if (res) {
				PropertyToken that = (PropertyToken) obj;
				res = (this.tokValue == that.tokValue);
			}

			return res;
		}


		/// <summary>
		///  Tests whether the given object is an instance of
		///  PropertyToken and has the same token value.
		/// </summary>
		public override int GetHashCode ()
		{
			return tokValue;
		}


		/// <summary>
		///  Returns the metadata token for this Property.
		/// </summary>
		public int Token {
			get {
				return tokValue;
			}
		}

	}

}

