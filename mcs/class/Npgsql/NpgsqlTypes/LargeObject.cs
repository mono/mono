/*-------------------------------------------------------------------------
 
  LargeObject.cs
      This class is a port of the class LargeObject.java implemented by 
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

    public class LargeObject
    {
        /*
         * Indicates a seek from the begining of a file
         */
        public  const Int32 SEEK_SET = 0;

        /*
         * Indicates a seek from the current position
         */
        public  const Int32 SEEK_CUR = 1;

        /*
         * Indicates a seek from the end of a file
         */
        public  const Int32 SEEK_END = 2;

        private Fastpath	fp; // Fastpath API to use
        private Int32 oid;	// OID of this object
        private Int32 fd; // the descriptor of the open large object

        private Boolean closed = false; // true when we are closed

        /*
         * This opens a large object.
         *
         * <p>If the object does not exist, then an NpgsqlException is thrown.
         *
         * @param fp FastPath API for the connection to use
         * @param oid of the Large Object to open
         * @param mode Mode of opening the large object
         * (defined in LargeObjectManager)
         * @exception NpgsqlException if a database-access error occurs.
         * @see org.postgresql.largeobject.LargeObjectManager
         */
        public LargeObject(Fastpath fp, Int32 oid, Int32 mode)
        {
            this.fp = fp;
            this.oid = oid;

            FastpathArg[] args = new FastpathArg[2];
            args[0] = new FastpathArg(oid);
            args[1] = new FastpathArg(mode);
            this.fd = fp.GetInteger("lo_open", args);
        }


        /*
         * @return the OID of this LargeObject
         */
        public Int32 GetOID()
        {
            return oid;
        }

        /*
         * This method closes the object. You must not call methods in this
         * object after this is called.
         * @exception NpgsqlException if a database-access error occurs.
         */
        public void Close()
        {
            if (!closed)
            {

                // finally close
                FastpathArg[] args = new FastpathArg[1];
                args[0] = new FastpathArg(fd);
                fp.FastpathCall("lo_close", false, args); // true here as we dont care!!
                closed = true;
            }
        }

        /*
         * Reads some data from the object, and return as a byte[] array
         *
         * @param len number of bytes to read
         * @return byte[] array containing data read
         * @exception NpgsqlException if a database-access error occurs.
         */
        public Byte[] Read(Int32 len)
        {
            // This is the original method, where the entire block (len bytes)
            // is retrieved in one go.
            FastpathArg[] args = new FastpathArg[2];
            args[0] = new FastpathArg(fd);
            args[1] = new FastpathArg(len);
            return fp.GetData("loread", args);

            // This version allows us to break this down Int32o 4k blocks
            //if (len<=4048) {
            //// handle as before, return the whole block in one go
            //FastpathArg args[] = new FastpathArg[2];
            //args[0] = new FastpathArg(fd);
            //args[1] = new FastpathArg(len);
            //return fp.getData("loread",args);
            //} else {
            //// return in 4k blocks
            //byte[] buf=new byte[len];
            //int off=0;
            //while (len>0) {
            //int bs=4048;
            //len-=bs;
            //if (len<0) {
            //bs+=len;
            //len=0;
            //}
            //read(buf,off,bs);
            //off+=bs;
            //}
            //return buf;
            //}
        }

        /*
         * Reads some data from the object into an existing array
         *
         * @param buf destination array
         * @param off offset within array
         * @param len number of bytes to read
         * @return the number of bytes actually read
         * @exception NpgsqlException if a database-access error occurs.
         */
        public Int32 Read(Byte[] buf, Int32 off, Int32 len)
        {
            Byte[] b = Read(len);
            if (b.Length < len)
                len = b.Length;
            Array.Copy(b,0,buf,off,len);
            return len;
        }

        /*
         * Writes an array to the object
         *
         * @param buf array to write
         * @exception NpgsqlException if a database-access error occurs.
         */
        public void Write(Byte[] buf)
        {
            FastpathArg[] args = new FastpathArg[2];
            args[0] = new FastpathArg(fd);
            args[1] = new FastpathArg(buf);
            fp.FastpathCall("lowrite", false, args);
        }

        /*
         * Writes some data from an array to the object
         *
         * @param buf destination array
         * @param off offset within array
         * @param len number of bytes to write
         * @exception NpgsqlException if a database-access error occurs.
         */
        public void Write(Byte[] buf, Int32 off, Int32 len)
        {
            Byte[] data = new Byte[len];

            System.Array.Copy(buf, off, data, 0, len);
            Write(data);
        }

        /*
         * Sets the current position within the object.
         *
         * <p>This is similar to the fseek() call in the standard C library. It
         * allows you to have random access to the large object.
         *
         * @param pos position within object
         * @param ref Either SEEK_SET, SEEK_CUR or SEEK_END
         * @exception NpgsqlException if a database-access error occurs.
         */
        public void Seek(Int32 pos, Int32 refi)
        {
            FastpathArg[] args = new FastpathArg[3];
            args[0] = new FastpathArg(fd);
            args[1] = new FastpathArg(pos);
            args[2] = new FastpathArg(refi);
            fp.FastpathCall("lo_lseek", false, args);
        }

        /*
         * Sets the current position within the object.
         *
         * <p>This is similar to the fseek() call in the standard C library. It
         * allows you to have random access to the large object.
         *
         * @param pos position within object from begining
         * @exception NpgsqlException if a database-access error occurs.
         */
        public void Seek(Int32 pos)
        {
            Seek(pos, SEEK_SET);
        }

        /*
         * @return the current position within the object
         * @exception NpgsqlException if a database-access error occurs.
         */
        public Int32 Tell()
        {
            FastpathArg[] args = new FastpathArg[1];
            args[0] = new FastpathArg(fd);
            return fp.GetInteger("lo_tell", args);
        }

        /*
         * This method is inefficient, as the only way to find out the size of
         * the object is to seek to the end, record the current position, then
         * return to the original position.
         *
         * <p>A better method will be found in the future.
         *
         * @return the size of the large object
         * @exception NpgsqlException if a database-access error occurs.
         */
        public Int32 Size()
        {
            Int32 cp = Tell();
            Seek(0, SEEK_END);
            Int32 sz = Tell();
            Seek(cp, SEEK_SET);
            return sz;
        }

    }
}
