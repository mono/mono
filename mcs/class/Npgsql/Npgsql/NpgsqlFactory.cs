// Npgsql.NpgsqlFactory.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002-2006 The Npgsql Development Team
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

using System;
using System.Data.Common;

namespace Npgsql
{
	/// <summary>
	/// A factory to create instances of various Npgsql objects.
	/// </summary>
	[Serializable]
	public sealed class NpgsqlFactory : DbProviderFactory, IServiceProvider
	{
		public static NpgsqlFactory Instance = new NpgsqlFactory();


		private NpgsqlFactory()
		{
		}


		/// <summary>
		/// Creates an NpgsqlCommand object.
		/// </summary>
		public override DbCommand CreateCommand()
		{
			return new NpgsqlCommand();
		}


		public override DbCommandBuilder CreateCommandBuilder()
		{
			return new NpgsqlCommandBuilder();
		}

		public override DbConnection CreateConnection()
		{
			return new NpgsqlConnection();
		}

		public override DbDataAdapter CreateDataAdapter()
		{
			return new NpgsqlDataAdapter();
		}

		public override DbParameter CreateParameter()
		{
			return new NpgsqlParameter();
		}

		public override DbConnectionStringBuilder CreateConnectionStringBuilder()
		{
			return new NpgsqlConnectionStringBuilder();
		}

		#region IServiceProvider Members

		public object GetService(Type serviceType)
		{
#if ENTITIES
            if (serviceType == typeof(DbProviderServices))
                return NpgsqlServices.Instance;
            else
#endif
			return null;
		}

		#endregion
	}
}