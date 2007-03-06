//
// Mainsoft.Web.Security.GenericDatabaseHelper
//
// Authors:
//      Marek Habersack <grendello@gmail.com>
//
// (C) 2007 Marek Habersack
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Web.Configuration;

using Mainsoft.Web.Configuration;

namespace Mainsoft.Web.Security 
{
	public class GenericDatabaseHelper
	{
                public delegate string QueryBuilder (DbParameter[] parms, object data);
		const string backendsSection = "aspnetConfigProviderBackends";
		
		DbProviderFactory factory;
		GenericSchemaBuilder schemaBuilder;
		ConnectionStringSettings connectionString;
		SortedDictionary <string, string> queries;
		
		public GenericDatabaseHelper (ConnectionStringSettings cs)
		{
			if (cs == null)
				throw new ArgumentNullException ("cs", "Database connection string must not be null");
			
			connectionString = cs;
			queries = new SortedDictionary <string, string> ();
		}

		public void Initialize ()
		{
			factory = DbProviderFactories.GetFactory (connectionString.ProviderName);
			if (factory == null)
				throw new ApplicationException (
					String.Format ("Unable to create database factory for provider {0}",
						       connectionString.ProviderName));

			BackendProviderInfo bpi = GetBackendProviderInfo ();
			Type schemaType = Type.GetType (bpi.SchemaBuilderType);
			
			schemaBuilder = Activator.CreateInstance (schemaType) as GenericSchemaBuilder;
			if (schemaBuilder == null)
				throw new ApplicationException (
					String.Format ("Type {0} not found or it does not descend from GenericSchemaBuilder",
						       bpi.SchemaBuilderType));
			schemaBuilder.Initialize (connectionString, this);
			schemaBuilder.CheckSchema ();
		}
		
		public DbCommand GetCommand (string queryName, DbConnection connection, QueryBuilder builder)
                {
                        return GetCommand (queryName, connection, null, null, builder, null);
                }
                
                public DbCommand GetCommand (string queryName, DbConnection connection, QueryBuilder builder, object data)
                {
                        return GetCommand (queryName, connection, null, null, builder, data);
                }
                
                public DbCommand GetCommand (string queryName, DbConnection connection, List <DbParameter> parms, QueryBuilder builder)
                {
                        return GetCommand (queryName, connection, null, parms, builder, null);
                }
                
                public DbCommand GetCommand (string queryName, DbConnection connection, List <DbParameter> parms, QueryBuilder builder, 
					     object data)
                {
                        return GetCommand (queryName, connection, null, parms, builder, data);
                }
                
                public DbCommand GetCommand (string queryName, DbConnection connection, DbTransaction transaction,
					     List <DbParameter> parms, QueryBuilder builder)
                {
            		return GetCommand (queryName, connection, transaction, parms, builder, null);
                }
                
                public DbCommand GetCommand (string queryName, DbConnection connection, DbTransaction transaction,
					     List <DbParameter> parms, QueryBuilder builder, object data)
                {
			string query;
			DbParameter[] parameters = parms != null ? parms.ToArray () : null;
			
                        if (queries.ContainsKey (queryName))
                                query = queries [queryName];
			else {
				query = builder (parameters, data);
				if (query == null)
					throw new ApplicationException (String.Format ("Failed to build query {0}", queryName));
				queries [queryName] = query;
			}
			
                        DbCommand ret = NewCommand (query, connection, transaction);
                        if (parameters != null)
                                ret.Parameters.AddRange (parameters);
                        return ret;
                }

                public string GetParamName (DbParameter[] parms, int i)
                {
                        if (parms.Length < i)
                                throw new ApplicationException ("Invalid array size");
                        DbParameter p = parms [i];
                        if (p == null)
                                throw new ApplicationException (String.Format ("Parameter {0} is null", i));
                        return p.ParameterName;
                }
		
		public BackendProviderInfo GetBackendProviderInfo ()
                {
                        if (connectionString == null)
                                throw new ApplicationException ("Missing connection string");

                        BackendProvidersSection bps = WebConfigurationManager.GetSection (backendsSection) as BackendProvidersSection;
                        if (bps == null)
                                throw new ApplicationException (
                                        String.Format ("Missing {0} section in your config file", backendsSection));
                        BackendProviderCollection bpc = bps.Backends;
                        if (bpc.Count == 0)
                                throw new ApplicationException ("No provider backends defined");
                        BackendProviderInfo bpi = bpc [connectionString.ProviderName];
                        if (bpi == null)
                                throw new ApplicationException (
                                        String.Format ("Cannot find backend definition for provider name {0}",
						       connectionString.ProviderName));

                        return bpi;
                }
		
		public DbParameter NewParameter (string paramName, object paramValue)
		{
			DbParameter ret = factory.CreateParameter ();
			ret.ParameterName = paramName;
			ret.Value = paramValue;

			return ret;
		}

		public DbCommand NewCommand (string dbQuery, DbConnection connection, DbTransaction transaction)
		{
			DbCommand ret = factory.CreateCommand ();
			ret.CommandText = dbQuery;
			ret.Connection = connection;
			ret.Transaction = transaction;

			return ret;
		}

		public DbCommand NewCommand (string dbQuery, DbConnection connection)
		{
			return NewCommand (dbQuery, connection, null);
		}

		public DbConnection NewConnection ()
		{

			DbConnection ret = factory.CreateConnection ();
			ret.ConnectionString = connectionString.ConnectionString;
			ret.Open ();
			return ret;
		}
		
		public string PrepareQueryParameter (object o, BackendProviderInfo bpi)
		{
			if (bpi.ParametersArePositional)
				return "?";
			if (o == null)
				throw new ArgumentNullException ("Parameter name must not be null");
			
			return String.Format ("{0}{1}", bpi.ParameterPlaceholderChar, o.ToString ());
		}

		public GenericSchemaBuilder GetSchemaBuilder ()
		{
			return schemaBuilder;
		}

		public void RegisterSchemaUnloadHandler ()
		{
			schemaBuilder.RegisterUnloadHandler ();
		}

		private DbParameter AddParameter (DbCommand command, string paramName, object paramValue)
                {
                        DbParameter prm = NewParameter (paramName, paramValue);
                        command.Parameters.Add (prm);
                        return prm;
                }
		
		public object CreateApplication (DbConnection connection, string applicationName)
                {
			BackendProviderInfo bpi = GetBackendProviderInfo ();
                        string selectQuery =
				String.Format ("SELECT ApplicationId FROM aspnet_Applications WHERE LoweredApplicationName = {0}",
					       PrepareQueryParameter ("LoweredApplicationName", bpi));

                        DbCommand selectCmd = NewCommand (selectQuery, connection);
                        AddParameter (selectCmd, "LoweredApplicationName", applicationName.ToLower ());

                        using (DbDataReader reader = selectCmd.ExecuteReader ()) {
                                if (reader.Read ())
                                        return reader.GetString (0);
                        }
			
                        string insertQuery =
				String.Format ("INSERT INTO aspnet_Applications (ApplicationId, ApplicationName, LoweredApplicationName) VALUES ({0}, {1}, {2})",
					       PrepareQueryParameter ("ApplicationId", bpi),
					       PrepareQueryParameter ("ApplicationName", bpi),
					       PrepareQueryParameter ("LoweredApplicationName", bpi));
			
                        string applicationId = Guid.NewGuid ().ToString ();
                        DbCommand insertCmd = NewCommand (insertQuery, connection);
                        AddParameter (insertCmd, "ApplicationId", applicationId);
                        AddParameter (insertCmd, "ApplicationName", applicationName);
                        AddParameter (insertCmd, "LoweredApplicationName", applicationName.ToLower ());
                        insertCmd.ExecuteNonQuery ();

                        return applicationId;
                }

		public string GetApplicationId (DbConnection connection, string applicationName)
                {
			BackendProviderInfo bpi = GetBackendProviderInfo ();
                        string selectQuery =
				String.Format ("SELECT ApplicationId FROM aspnet_Applications WHERE LoweredApplicationName = {0}",
					       PrepareQueryParameter ("LoweredApplicationName", bpi));

                        DbCommand selectCmd = NewCommand (selectQuery, connection);
                        AddParameter (selectCmd, "LoweredApplicationName", applicationName.ToLower ());
                        using (DbDataReader reader = selectCmd.ExecuteReader ()) {
                                if (reader.Read ())
                                        return reader.GetString (0);
                        }

                        return null;
                }
	}
}
#endif