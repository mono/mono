// LzwException.cs
//
// Copyright (C) 2009 Gabriel Burca
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
//
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

using System;

#if !NETCF_1_0 && !NETCF_2_0
using System.Runtime.Serialization;
#endif

namespace ICSharpCode.SharpZipLib.LZW
{

    /// <summary>
    /// LzwException represents a LZW specific exception
    /// </summary>
#if !NETCF_1_0 && !NETCF_2_0
    [Serializable]
#endif
    public class LzwException : SharpZipBaseException
    {

#if !NETCF_1_0 && !NETCF_2_0
        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> for this constructor</param>
        /// <param name="context"><see cref="StreamingContext"/> for this constructor</param>
        protected LzwException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
#endif

        /// <summary>
        /// Initialise a new instance of LzwException
        /// </summary>
        public LzwException() {
        }

        /// <summary>
        /// Initialise a new instance of LzwException with its message string.
        /// </summary>
        /// <param name="message">A <see cref="string"/> that describes the error.</param>
        public LzwException(string message)
            : base(message) {
        }

        /// <summary>
        /// Initialise a new instance of <see cref="LzwException"></see>.
        /// </summary>
        /// <param name="message">A <see cref="string"/> that describes the error.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused this exception.</param>
        public LzwException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }
}
