// LzwConstants.cs
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

namespace ICSharpCode.SharpZipLib.LZW {

    /// <summary>
    /// This class contains constants used for LZW
    /// </summary>
    sealed public class LzwConstants {
        /// <summary>
        /// Magic number found at start of LZW header: 0x1f 0x9d
        /// </summary>
        public const int MAGIC = 0x1f9d;

        /// <summary>
        /// Maximum number of bits per code
        /// </summary>
        public const int MAX_BITS = 16;

        /* 3rd header byte:
         * bit 0..4 Number of compression bits
         * bit 5    Extended header
         * bit 6    Free
         * bit 7    Block mode
         */

        /// <summary>
        /// Mask for 'number of compression bits'
        /// </summary>
        public const int BIT_MASK       = 0x1f;

        /// <summary>
        /// Indicates the presence of a fourth header byte
        /// </summary>
        public const int EXTENDED_MASK  = 0x20;
        //public const int FREE_MASK      = 0x40;

        /// <summary>
        /// Reserved bits
        /// </summary>
        public const int RESERVED_MASK  = 0x60;

        /// <summary>
        /// Block compression: if table is full and compression rate is dropping,
        /// clear the dictionary.
        /// </summary>
        public const int BLOCK_MODE_MASK = 0x80;

        /// <summary>
        /// LZW file header size (in bytes)
        /// </summary>
        public const int HDR_SIZE = 3;

        /// <summary>
        /// Initial number of bits per code
        /// </summary>
        public const int INIT_BITS = 9;

        LzwConstants() {
        }
    }
}
