// created on 20/02/2003

// Npgsql.MD5.cs
//

//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
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
        protected MD5 ()
        {
            HashSizeValue = 128;
        }

        /// <summary>
        /// Creates the default derived class.
        /// </summary>
        public static MD5 Create ()
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
