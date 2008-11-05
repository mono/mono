/*-------------------------------------------------------------------------
 
  LargeObjectManager.cs
      This class is a port of the class LargeObjectManager.java implemented by 
      PostgreSQL Global Development Group
 
 
 Copyright (c) 2004, Emiliano Necciari
 Original Code: Copyright (c) 2003, PostgreSQL Global Development Group
 
 Note: (Francisco Figueiredo Jr.)
  	Changed case of method names to conform to .Net names standard.
  	Also changed type names to their true names. i.e. int -> Int32
    
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

-------------------------------------------------------------------------
*/

using System;
using System.Data;
using System.Text;
using Npgsql;

namespace NpgsqlTypes
{
	/// <summary>
	/// Summary description for LargeObjectManager.
	/// </summary>
	public class LargeObjectManager
	{
		// the fastpath api for this connection
		private readonly Fastpath fp;

		/*
         * This mode indicates we want to write to an object
         */
		public const Int32 WRITE = 0x00020000;

		/*
         * This mode indicates we want to read an object
         */
		public static Int32 READ = 0x00040000;

		/*
         * This mode is the default. It indicates we want read and write access to
         * a large object
         */
		public static Int32 READWRITE = READ | WRITE;


		private LargeObjectManager()
		{
			/*
			* This prevents us being created by mere mortals
			*/
		}

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
			//String sql;
			StringBuilder sql = null;
			try
			{
				sql = new StringBuilder();
				if (conn.PostgreSqlVersion > new Version(7, 3, 0))
				{
					sql.Append("SELECT p.proname,p.oid ");
					sql.Append(" FROM pg_catalog.pg_proc p, pg_catalog.pg_namespace n ");
					sql.Append(" WHERE p.pronamespace=n.oid AND n.nspname='pg_catalog' AND (");
				}
				else
				{
					sql.Append("SELECT proname,oid FROM pg_proc WHERE ");
				}
				sql.Append(" proname = 'lo_open'");
				sql.Append(" or proname = 'lo_close'");
				sql.Append(" or proname = 'lo_creat'");
				sql.Append(" or proname = 'lo_unlink'");
				sql.Append(" or proname = 'lo_lseek'");
				sql.Append(" or proname = 'lo_tell'");
				sql.Append(" or proname = 'loread'");
				sql.Append(" or proname = 'lowrite'");

				if (conn.PostgreSqlVersion > new Version(7, 3, 0))
				{
					sql.Append(")");
				}

				using (IDbCommand cmd = new NpgsqlCommand(sql.ToString()))
				{
					cmd.Connection = conn;

					this.fp = new Fastpath(conn, conn.Connector.Stream);

					using (IDataReader res = cmd.ExecuteReader())
					{
						if (res == null)
						{
							throw new NpgsqlException("postgresql.lo.init");
						}


						fp.AddFunctions(res);
					}
				}
			}
			finally
			{
				sql = null;
			}
		}

		/*
         * This opens an existing large object, based on its OID. This method
         * assumes that READ and WRITE access is required (the default).
         *
         * @param oid of large object
         * @return LargeObject instance providing access to the object
         * @exception NpgsqlException on error
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
         * @exception NpgsqlException on error
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
         * @exception NpgsqlException on error
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
         * @exception NpgsqlException on error
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
         * @exception NpgsqlException on error
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
         * @exception NpgsqlException on error
         */

		public void Unlink(Int32 oid)
		{
			Delete(oid);
		}
	}
}