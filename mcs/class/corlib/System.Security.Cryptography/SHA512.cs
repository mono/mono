//
// System.Security.Cryptography SHA512 Class implementation
//
// Authors:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//
// Copyright 2001 by Matthew S. Ford.
//


using System.Security.Cryptography;

namespace System.Security.Cryptography {
	
	/// <summary>
	/// Common base class for all derived SHA512 iplementations.
	/// </summary>
	public abstract class SHA512 : HashAlgorithm {
		
		/// <summary>
		/// Called from constructor of derived class.
		/// </summary>
		public SHA512 () {
		
		}

	
		/// <summary>
		/// Creates the default derived class.
		/// </summary>
		public static new SHA512 Create () {
			return new SHA512Managed();
		}
	
		/// <summary>
		/// Creates a new derived class.
		/// </summary>
		/// <param name="st">FIXME: No clue.  Specifies which derived class to create?</param>
		[MonoTODO]
		public static new SHA512 Create (string st) {
			return Create();
		}
	}
}

