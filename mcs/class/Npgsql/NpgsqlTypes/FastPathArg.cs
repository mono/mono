/*-------------------------------------------------------------------------
 
  FastpathArg.cs
      This class is a port of the class FastpathArg.java implemented by 
      PostgreSQL Global Development Group
  
 Copyright (c) 2004, Emiliano Necciari
 Original Code: Copyright (c) 2003, PostgreSQL Global Development Group
 
 Note: (Francisco Figueiredo Jr.)
  	Changed case of method names to conform to .Net names standard.
  	Also changed type names to their true names. i.e. int -> Int32
 
 This library is free software; you can redistribute it and/or
 modify it under the terms of the GNU Lesser General Public
 License as published by the Free Software Foundation; either
 version 2.1 of the License, or (at your option) any later version.
 
 This library is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 Lesser General Public License for more details.
 
 You should have received a copy of the GNU Lesser General Public
 License along with this library; if not, write to the Free Software
 Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
-------------------------------------------------------------------------
*/

using System;
using System.IO;
using Npgsql;

namespace NpgsqlTypes
{
    public class FastpathArg
    {
        /*
         * Type of argument, true=integer, false=byte[]
         */
        public Boolean type;

        /*
         * Integer value if type=true
         */
        public Int32 value;

        /*
         * Byte value if type=false;
         */
        public Byte[] bytes;

        /*
         * Constructs an argument that consists of an integer value
         * @param value int value to set
         */
        public FastpathArg(Int32 value)
        {
            type = true;
            this.value = value;
        }

        /*
         * Constructs an argument that consists of an array of bytes
         * @param bytes array to store
         */
        public FastpathArg(Byte[] bytes)
        {
            type = false;
            this.bytes = bytes;
        }

        /*
         * Constructs an argument that consists of part of a byte array
         * @param buf source array
         * @param off offset within array
         * @param len length of data to include
         */
        public FastpathArg(Byte[] buf, Int32 off, Int32 len)
        {
            type = false;
            bytes = new Byte[len];
            //TODO:
            bytes = buf;
        }

        /*
         * Constructs an argument that consists of a String.
         * @param s String to store
         */
        public FastpathArg(String s)
        {
            //this(s.ToCharArray());
        }

        /*
         * This sends this argument down the network stream.
         *
         * <p>The stream sent consists of the length.int4 then the contents.
         *
         * <p><b>Note:</b> This is called from Fastpath, and cannot be called from
         * client code.
         *
         * @param s output stream
         * @exception IOException if something failed on the network stream
         */
        public void Send(Stream s)
        {
            if (type)
            {
                // argument is an integer
                PGUtil.WriteInt32(s, 4);
                PGUtil.WriteInt32(s, value);	// integer value of argument
            }
            else
            {
                // argument is a byte array
                PGUtil.WriteInt32(s, bytes.Length);
                s.Write(bytes,0,bytes.Length);
            }
        }

        public Int32 SendSize()
        {
            if (type)
            {
                return 8;
            }
            else
            {
                return 4+bytes.Length;
            }
        }
    }
}
