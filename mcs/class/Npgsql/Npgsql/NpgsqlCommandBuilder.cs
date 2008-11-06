// NpgsqlCommandBuilder.cs
//
// Author:
//   Pedro Martínez Juliá (yoros@wanadoo.es)
//
// Copyright (C) 2003 Pedro Martínez Juliá
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


using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Resources;
using NpgsqlTypes;

namespace Npgsql
{
	///<summary>
	/// This class is responsible to create database commands for automatic insert, update and delete operations.
	///</summary>
	public sealed class NpgsqlCommandBuilder : DbCommandBuilder
	{
		// Logging related values
		//private static readonly String CLASSNAME = "NpgsqlCommandBuilder";
		private readonly  static ResourceManager resman = new ResourceManager(typeof (NpgsqlCommandBuilder));
		private NpgsqlRowUpdatingEventHandler rowUpdatingHandler;


		public NpgsqlCommandBuilder()
			: this(null)
		{
		}

		public NpgsqlCommandBuilder(NpgsqlDataAdapter adapter)
			: base()
		{
			DataAdapter = adapter;
			this.QuotePrefix = "\"";
			this.QuoteSuffix = "\"";
		}

		public override string QuotePrefix
		{
			get { return base.QuotePrefix; }
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					base.QuotePrefix = value;
				}
				else
				{
					base.QuotePrefix = "\"";
				}
			}
		}

		public override string QuoteSuffix
		{
			get { return base.QuoteSuffix; }
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					base.QuoteSuffix = value;
				}
				else
				{
					base.QuoteSuffix = "\"";
				}
			}
		}

		///<summary>
		///
		/// This method is reponsible to derive the command parameter list with values obtained from function definition.
		/// It clears the Parameters collection of command. Also, if there is any parameter type which is not supported by Npgsql, an InvalidOperationException will be thrown.
		/// Parameters name will be parameter1, parameter2, ...
		/// For while, only parameter name and NpgsqlDbType are obtained.
		///</summary>
		/// <param name="command">NpgsqlCommand whose function parameters will be obtained.</param>
		public static void DeriveParameters(NpgsqlCommand command)
		{
			// Updated after 0.99.3 to support the optional existence of a name qualifying schema and case insensitivity when the schema ror procedure name do not contain a quote.
			// This fixed an incompatibility with NpgsqlCommand.CheckFunctionReturn(String ReturnType)
			String query = null;
			string procedureName = null;
			string schemaName = null;
			string[] fullName = command.CommandText.Split('.');
			if (fullName.Length > 1 && fullName[0].Length > 0)
			{
				query =
					"select proargtypes from pg_proc p left join pg_namespace n on p.pronamespace = n.oid where proname=:proname and n.nspname=:nspname";
				schemaName = (fullName[0].IndexOf("\"") != -1) ? fullName[0] : fullName[0].ToLower();
				procedureName = (fullName[1].IndexOf("\"") != -1) ? fullName[1] : fullName[1].ToLower();
			}
			else
			{
				query = "select proargtypes from pg_proc where proname = :proname";
				procedureName = (fullName[0].IndexOf("\"") != -1) ? fullName[0] : fullName[0].ToLower();
			}

			using (NpgsqlCommand c = new NpgsqlCommand(query, command.Connection))
			{
				c.Parameters.Add(new NpgsqlParameter("proname", NpgsqlDbType.Text));
				c.Parameters[0].Value = procedureName.Replace("\"", "").Trim();
				if (fullName.Length > 1 &&  !String.IsNullOrEmpty(schemaName))
				{
					NpgsqlParameter prm = c.Parameters.Add(new NpgsqlParameter("nspname", NpgsqlDbType.Text));
					prm.Value = schemaName.Replace("\"", "").Trim();
				}
				String types = (String) c.ExecuteScalar();
				if (types == null)
				{
					throw new InvalidOperationException(
						String.Format(resman.GetString("Exception_InvalidFunctionName"), command.CommandText));
				}

				command.Parameters.Clear();
				Int32 i = 1;
				foreach (String s in types.Split())
				{
					NpgsqlBackendTypeInfo typeInfo = null;
					if (!c.Connector.OidToNameMapping.TryGetValue(int.Parse(s), out typeInfo))
					{
						command.Parameters.Clear();
						throw new InvalidOperationException(String.Format("Invalid parameter type: {0}", s));
					}
					command.Parameters.Add(new NpgsqlParameter("parameter" + i++, typeInfo.NpgsqlDbType));
				}
			}
		}

		public new NpgsqlCommand GetInsertCommand()
		{
			return (NpgsqlCommand) base.GetInsertCommand();
		}

		public new NpgsqlCommand GetInsertCommand(bool useColumnsForParameterNames)
		{
			return (NpgsqlCommand) base.GetInsertCommand(useColumnsForParameterNames);
		}

		public new NpgsqlCommand GetUpdateCommand()
		{
			return (NpgsqlCommand) base.GetUpdateCommand();
		}

		public new NpgsqlCommand GetUpdateCommand(bool useColumnsForParameterNames)
		{
			return (NpgsqlCommand) base.GetUpdateCommand(useColumnsForParameterNames);
		}

		public new NpgsqlCommand GetDeleteCommand()
		{
			return (NpgsqlCommand) base.GetDeleteCommand();
		}

		public new NpgsqlCommand GetDeleteCommand(bool useColumnsForParameterNames)
		{
			return (NpgsqlCommand) base.GetDeleteCommand(useColumnsForParameterNames);
		}

		//never used
		//private string QualifiedTableName(string schema, string tableName)
		//{
		//    if (schema == null || schema.Length == 0)
		//    {
		//        return tableName;
		//    }
		//    else
		//    {
		//        return schema + "." + tableName;
		//    }
		//}

/*
		private static void SetParameterValuesFromRow(NpgsqlCommand command, DataRow row)
		{
			foreach (NpgsqlParameter parameter in command.Parameters)
			{
				parameter.Value = row[parameter.SourceColumn, parameter.SourceVersion];
			}
		}
*/

		protected override void ApplyParameterInfo(DbParameter p, DataRow row, StatementType statementType, bool whereClause)
		{
			NpgsqlParameter parameter = (NpgsqlParameter) p;
			parameter.DbType = NpgsqlTypesHelper.GetNativeTypeInfo((Type) row[SchemaTableColumn.DataType]).DbType;
		}

		protected override string GetParameterName(int parameterOrdinal)
		{
			return String.Format(CultureInfo.InvariantCulture, "@p{0}", parameterOrdinal);
		}

		protected override string GetParameterName(string parameterName)
		{
			return String.Format(CultureInfo.InvariantCulture, "@{0}", parameterName);
		}

		protected override string GetParameterPlaceholder(int parameterOrdinal)
		{
			return GetParameterName(parameterOrdinal);
		}

		protected override void SetRowUpdatingHandler(DbDataAdapter adapter)
		{
			if (!(adapter is NpgsqlDataAdapter))
			{
				throw new InvalidOperationException("adapter needs to be a NpgsqlDataAdapter");
			}


			this.rowUpdatingHandler = new NpgsqlRowUpdatingEventHandler(this.RowUpdatingHandler);

			((NpgsqlDataAdapter) adapter).RowUpdating += this.rowUpdatingHandler;
		}


		private void RowUpdatingHandler(object sender, NpgsqlRowUpdatingEventArgs e)

		{
			base.RowUpdatingHandler(e);
		}


		public override string QuoteIdentifier(string unquotedIdentifier)

		{
			if (unquotedIdentifier == null)

			{
				throw new ArgumentNullException("Unquoted identifier parameter cannot be null");
			}


			return String.Format("{0}{1}{2}", this.QuotePrefix, unquotedIdentifier, this.QuoteSuffix);
		}


		public override string UnquoteIdentifier(string quotedIdentifier)

		{
			if (quotedIdentifier == null)

			{
				throw new ArgumentNullException("Quoted identifier parameter cannot be null");
			}


			string unquotedIdentifier = quotedIdentifier.Trim();


			if (unquotedIdentifier.StartsWith(this.QuotePrefix))

			{
				unquotedIdentifier = unquotedIdentifier.Remove(0, 1);
			}

			if (unquotedIdentifier.EndsWith(this.QuoteSuffix))

			{
				unquotedIdentifier = unquotedIdentifier.Remove(unquotedIdentifier.Length - 1, 1);
			}


			return unquotedIdentifier;
		}
	}
}