using System;
using System.Data;
using ByteFX.Data.MySqlClient;

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Summary description for MySqlHelper.
	/// </summary>
	public sealed class MySqlHelper
	{
		// this class provides only static methods
		private MySqlHelper()
		{
		}

		#region ExecuteNonQuery
		public static int ExecuteNonQuery( MySqlConnection connection, string commandText, params MySqlParameter[] commandParameters )
		{
			//create a command and prepare it for execution
			MySqlCommand cmd = new MySqlCommand();
			cmd.Connection = connection;
			cmd.CommandText = commandText;
			cmd.CommandType = CommandType.Text;

			if (commandParameters != null)
				foreach (MySqlParameter p in commandParameters)
					cmd.Parameters.Add( p );

			int result = cmd.ExecuteNonQuery();
			cmd.Parameters.Clear();

			return result;
		}

		public static int ExecuteNonQuery( string connectionString, string commandText, params MySqlParameter[] parms )
		{
			//create & open a SqlConnection, and dispose of it after we are done.
			using (MySqlConnection cn = new MySqlConnection(connectionString))
			{
				cn.Open();

				//call the overload that takes a connection in place of the connection string
				return ExecuteNonQuery(cn, commandText, parms );
			}
		}
		#endregion

		#region ExecuteDataSet
		public static DataRow ExecuteDatarow( string connectionString, string commandText, params MySqlParameter[] parms )
		{
			DataSet ds = ExecuteDataset( connectionString, commandText, parms );
			if (ds == null) return null;
			if (ds.Tables.Count == 0) return null;
			if (ds.Tables[0].Rows.Count == 0) return null;
			return ds.Tables[0].Rows[0];
		}

		public static DataSet ExecuteDataset(string connectionString, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteDataset(connectionString, commandText, (MySqlParameter[])null);
		}

		public static DataSet ExecuteDataset(string connectionString, string commandText, params MySqlParameter[] commandParameters)
		{
			//create & open a SqlConnection, and dispose of it after we are done.
			using (MySqlConnection cn = new MySqlConnection(connectionString))
			{
				cn.Open();

				//call the overload that takes a connection in place of the connection string
				return ExecuteDataset(cn, commandText, commandParameters);
			}
		}

		public static DataSet ExecuteDataset(MySqlConnection connection, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteDataset(connection, commandText, (MySqlParameter[])null);
		}


		public static DataSet ExecuteDataset(MySqlConnection connection, string commandText, params MySqlParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			MySqlCommand cmd = new MySqlCommand();
			cmd.Connection = connection;
			cmd.CommandText = commandText;
			cmd.CommandType = CommandType.Text;

			if (commandParameters != null)
				foreach (MySqlParameter p in commandParameters)
					cmd.Parameters.Add( p );
			
			//create the DataAdapter & DataSet
			MySqlDataAdapter da = new MySqlDataAdapter(cmd);
			DataSet ds = new DataSet();

			//fill the DataSet using default values for DataTable names, etc.
			da.Fill(ds);
			
			// detach the MySqlParameters from the command object, so they can be used again.			
			cmd.Parameters.Clear();
			
			//return the dataset
			return ds;						
		}

		public static void UpdateDataSet( string connectionString, string commandText, DataSet ds, string tablename )
		{
			MySqlConnection cn = new MySqlConnection( connectionString );
			cn.Open();
			MySqlDataAdapter da = new MySqlDataAdapter( commandText, cn );
			MySqlCommandBuilder cb = new MySqlCommandBuilder( da );
			da.Update( ds, tablename );
			cn.Close();
		}

		#endregion

		#region ExecuteDataReader
		private static MySqlDataReader ExecuteReader(MySqlConnection connection, MySqlTransaction transaction, string commandText, MySqlParameter[] commandParameters, bool ExternalConn )
		{	
			//create a command and prepare it for execution
			MySqlCommand cmd = new MySqlCommand();
			cmd.Connection = connection;
			cmd.Transaction = transaction;
			cmd.CommandText = commandText;
			cmd.CommandType = CommandType.Text;
			
			if (commandParameters != null)
				foreach (MySqlParameter p in commandParameters)
					cmd.Parameters.Add( p );

			//create a reader
			MySqlDataReader dr;

			// call ExecuteReader with the appropriate CommandBehavior
			if (ExternalConn)
			{
				dr = cmd.ExecuteReader();
			}
			else
			{
				dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
			}
			
			// detach the SqlParameters from the command object, so they can be used again.
			cmd.Parameters.Clear();
			
			return dr;
		}

		public static MySqlDataReader ExecuteReader(string connectionString, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteReader(connectionString, commandText, (MySqlParameter[])null);
		}

		public static MySqlDataReader ExecuteReader(string connectionString, string commandText, params MySqlParameter[] commandParameters)
		{
			//create & open a SqlConnection
			MySqlConnection cn = new MySqlConnection(connectionString);
			cn.Open();

			try
			{
				//call the private overload that takes an internally owned connection in place of the connection string
				return ExecuteReader(cn, null, commandText, commandParameters, false );
			}
			catch
			{
				//if we fail to return the SqlDatReader, we need to close the connection ourselves
				cn.Close();
				throw;
			}
		}
		#endregion

		#region ExecuteScalar
		public static object ExecuteScalar(string connectionString, string commandText)
		{
			//pass through the call providing null for the set of MySqlParameters
			return ExecuteScalar(connectionString, commandText, (MySqlParameter[])null);
		}

		public static object ExecuteScalar(string connectionString, string commandText, params MySqlParameter[] commandParameters)
		{
			//create & open a SqlConnection, and dispose of it after we are done.
			using (MySqlConnection cn = new MySqlConnection(connectionString))
			{
				cn.Open();

				//call the overload that takes a connection in place of the connection string
				return ExecuteScalar(cn, commandText, commandParameters);
			}
		}

		public static object ExecuteScalar(MySqlConnection connection, string commandText)
		{
			//pass through the call providing null for the set of MySqlParameters
			return ExecuteScalar(connection, commandText, (MySqlParameter[])null);
		}

		public static object ExecuteScalar(MySqlConnection connection, string commandText, params MySqlParameter[] commandParameters)
		{
			//create a command and prepare it for execution
			MySqlCommand cmd = new MySqlCommand();
			cmd.Connection = connection;
			cmd.CommandText = commandText;
			cmd.CommandType = CommandType.Text;
			
			if (commandParameters != null)
				foreach (MySqlParameter p in commandParameters)
					cmd.Parameters.Add( p );
			
			//execute the command & return the results
			object retval = cmd.ExecuteScalar();
			
			// detach the SqlParameters from the command object, so they can be used again.
			cmd.Parameters.Clear();
			return retval;
			
		}

		#endregion
	}
}
