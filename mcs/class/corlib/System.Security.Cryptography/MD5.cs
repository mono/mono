//
// System.Security.Cryptography MD5 Class implementation
//
// Authors:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//
// Copyright 2001 by Matthew S. Ford.
//


using System.Security.Cryptography;

namespace System.Security.Cryptography {
	
	/// <summary>
	/// Common base class for all derived MD5 iplementations.
	/// </summary>
	public abstract class MD5 : HashAlgorithm {
		
		/// <summary>
		/// Called from constructor of derived class.
		/// </summary>
		protected MD5 () {
		
		}
	
		/// <summary>
		/// Creates the default derived class.
		/// </summary>
		public static new MD5 Create () {
			return new MD5CryptoServiceProvider();
		}
	
		/// <summary>
		/// Creates a new derived implementation.
		/// </summary>
		/// <param name="st">FIXME: No clue.  Specifies which derived class to create?</param>
		[MonoTODO]
		public static new MD5 Create (string st) {
			return Create();
		}
	}
}

