// EventToken.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Reflection.Emit {


	/// <summary>
	///  Represents the Token returned by the metadata to represent a Event.
	/// </summary>
	public struct EventToken {

		internal int tokValue;

		public static readonly EventToken Empty;


		static EventToken ()
		{
			Empty = new EventToken ();
		}


		internal EventToken (int val)
		{
			tokValue = val;
		}



		/// <summary>
		/// </summary>
		public override bool Equals (object obj)
		{
			bool res = obj is EventToken;

			if (res) {
				EventToken that = (EventToken) obj;
				res = (this.tokValue == that.tokValue);
			}

			return res;
		}


		/// <summary>
		///  Tests whether the given object is an instance of
		///  EventToken and has the same token value.
		/// </summary>
		public override int GetHashCode ()
		{
			return tokValue;
		}


		/// <summary>
		///  Returns the metadata token for this Event.
		/// </summary>
		public int Token {
			get {
				return tokValue;
			}
		}

	}

}

