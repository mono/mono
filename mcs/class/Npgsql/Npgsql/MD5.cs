// created on 20/02/2003

// Npgsql.MD5.cs
//

//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
//


//
// System.Security.Cryptography MD5 Class implementation
//
// Authors:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//   Sebastien Pouliot (spouliot@motus.com)
//
// Copyright 2001 by Matthew S. Ford.
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//
// Comment: Adapted to the Project from Mono CVS as Sebastien Pouliot suggested to enable
// support of Npgsql MD5 authentication in platforms which don't have support for MD5 algorithm.
//
//
//


namespace Npgsql
{
	/// <summary>
	/// Common base class for all derived MD5 implementations.
	/// </summary>
	internal abstract class MD5 : HashAlgorithm
	{
		/// <summary>
		/// Called from constructor of derived class.
		/// </summary>
		// Why is it protected when others abstract hash classes are public ?
		protected MD5()
		{
			HashSizeValue = 128;
		}

		/// <summary>
		/// Creates the default derived class.
		/// </summary>
		public static MD5 Create()
		{
			//return Create ("System.Security.Cryptography.MD5");
			return new MD5CryptoServiceProvider();
		}

		/*
        // Commented out because it uses the CryptoConfig which can't be available in all
        // platforms.
        /// <summary>
        /// Creates a new derived implementation.
        /// </summary>
        /// <param name="hashName">Specifies which derived class to create</param>
        public static new MD5 Create (string hashName)
        {
        	return (MD5) CryptoConfig.CreateFromName (hashName);
        }*/
	}
}