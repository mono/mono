//
// System.Security.Cryptography SHA384 Class implementation
//
// Authors:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//
// Copyright 2001 by Matthew S. Ford.
//


using System.Security.Cryptography;

namespace System.Security.Cryptography {
	
	/// <summary>
	/// Common base class for all derived SHA384 iplementations.
	/// </summary>
	public abstract class SHA384 : HashAlgorithm {
		
		/// <summary>
		/// Called from constructor of derived class.
		/// </summary>
		public SHA384 () {
		
		}

	
		/// <summary>
		/// Creates the default derived class.
		/// </summary>
		public static new SHA384 Create () {
			return new SHA384Managed();
		}
	
		/// <summary>
		/// Creates a new derived class.
		/// </summary>
		/// <param name="st">FIXME: No clue.  Specifies which derived class to create?</param>
		[MonoTODO]
		public static new SHA384 Create (string st) {
			return Create();
		}
	}
}

