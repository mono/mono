//
// System.Security.Cryptography SHA1 Class implementation
//
// Authors:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//
// Copyright 2001 by Matthew S. Ford.
//


using System.Security.Cryptography;

namespace System.Security.Cryptography {
	
	/// <summary>
	/// Common base class for all derived SHA1 iplementations.
	/// </summary>
	public abstract class SHA1 : HashAlgorithm {
		/// <summary>
		/// Called from constructor of derived class.
		/// </summary>
		protected SHA1 () {
		
		}

	
		/// <summary>
		/// Creates the default derived class.
		/// </summary>
		public static new SHA1 Create () {
			return new SHA1CryptoServiceProvider();
		}
	
		/// <summary>
		/// Creates a new derived class.
		/// </summary>
		/// <param name="st">FIXME: No clue.  Specifies which derived class to create?</param>
		[MonoTODO]
		public static new SHA1 Create (string st) {
			return Create();
		}
	}
}

