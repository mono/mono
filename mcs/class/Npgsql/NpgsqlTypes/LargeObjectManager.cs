/*-------------------------------------------------------------------------
 
  LargeObjectManager.cs
      This class is a port of the class LargeObjectManager.java implemented by 
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
using System.Data;
using Npgsql;

namespace NpgsqlTypes
{
    /// <summary>
    /// Summary description for LargeObjectManager.
    /// </summary>
    public class LargeObjectManager
    {
        // the fastpath api for this connection
        private Fastpath fp;

        /*
         * This mode indicates we want to write to an object
         */
        public const Int32 WRITE = 0x00020000;

        /*
         * This mode indicates we want to read an object
         */
        public static  Int32 READ = 0x00040000;

        /*
         * This mode is the default. It indicates we want read and write access to
         * a large object
         */
        public static Int32 READWRITE = READ | WRITE;

        /*
         * This prevents us being created by mere mortals
         */
        private LargeObjectManager()
        {}

        /*
         * Constructs the LargeObject API.
         *
         * <p><b>Important Notice</b>
         * <br>This method should only be called by org.postgresql.Connection
         *
         * <p>There should only be one LargeObjectManager per Connection. The
         * org.postgresql.Connection class keeps track of the various extension API's
         * and it's advised you use those to gain access, and not going direct.
         */
        public LargeObjectManager(NpgsqlConnection conn)
        {
            // We need Fastpath to do anything
            // Now get the function oid's for the api
            //
            // This is an example of Fastpath.addFunctions();
            //
            String sql;
            if (conn.ServerVersion > new ServerVersion(7,3,0) )
            {

                sql = "SELECT p.proname,p.oid "+
                      " FROM pg_catalog.pg_proc p, pg_catalog.pg_namespace n "+
                      " WHERE p.pronamespace=n.oid AND n.nspname='pg_catalog' AND (";
            }
            else
            {
                sql = "SELECT proname,oid FROM pg_proc WHERE ";
            }
            sql += " proname = 'lo_open'" +
                   " or proname = 'lo_close'" +
                   " or proname = 'lo_creat'" +
                   " or proname = 'lo_unlink'" +
                   " or proname = 'lo_lseek'" +
                   " or proname = 'lo_tell'" +
                   " or proname = 'loread'" +
                   " or proname = 'lowrite'";

            if (conn.ServerVersion > new ServerVersion(7,3,0) )
            {
                sql += ")";
            }

            IDbCommand cmd = new NpgsqlCommand(sql);
            cmd.Connection = conn;

            this.fp = new Fastpath(conn,conn.Connector.Stream);

            IDataReader res = cmd.ExecuteReader(CommandBehavior.CloseConnection);


            if (res == null)
                throw new Exception("postgresql.lo.init");


            fp.AddFunctions(res);
        }

        /*
         * This opens an existing large object, based on its OID. This method
         * assumes that READ and WRITE access is required (the default).
         *
         * @param oid of large object
         * @return LargeObject instance providing access to the object
         * @exception SQLException on error
         */
        public LargeObject Open(Int32 oid)
        {
            return new LargeObject(fp, oid, READWRITE);
        }

        /*
         * This opens an existing large object, based on its OID
         *
         * @param oid of large object
         * @param mode mode of open
         * @return LargeObject instance providing access to the object
         * @exception SQLException on error
         */
        public LargeObject Open(Int32 oid, Int32 mode)
        {
            return new LargeObject(fp, oid, mode);
        }

        /*
         * This creates a large object, returning its OID.
         *
         * <p>It defaults to READWRITE for the new object's attributes.
         *
         * @return oid of new object
         * @exception SQLException on error
         */
        public Int32 Create()
        {
            FastpathArg[] args = new FastpathArg[1];
            args[0] = new FastpathArg(READWRITE);
            return fp.GetInteger("lo_creat", args);
        }

        /*
         * This creates a large object, returning its OID
         *
         * @param mode a bitmask describing different attributes of the new object
         * @return oid of new object
         * @exception SQLException on error
         */
        public Int32 Create(Int32 mode)
        {
            FastpathArg[] args = new FastpathArg[1];
            args[0] = new FastpathArg(mode);
            return fp.GetInteger("lo_creat", args);
        }

        /*
         * This deletes a large object.
         *
         * @param oid describing object to delete
         * @exception SQLException on error
         */
        public void Delete(Int32 oid)
        {
            FastpathArg[] args = new FastpathArg[1];
            args[0] = new FastpathArg(oid);
            fp.FastpathCall("lo_unlink", false, args);
        }

        /*
         * This deletes a large object.
         *
         * <p>It is identical to the delete method, and is supplied as the C API uses
         * unlink.
         *
         * @param oid describing object to delete
         * @exception SQLException on error
         */
        public void Unlink(Int32 oid)
        {
            Delete(oid);
        }

    }

}
