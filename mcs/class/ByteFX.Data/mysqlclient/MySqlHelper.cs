using System;
using System.Data;
using ByteFX.Data.MySqlClient;

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Helper class that makes it easier to work with the provider.
	/// </summary>
	public sealed class MySqlHelper
	{
		// this class provides only static methods
		private MySqlHelper()
		{
		}

		#region ExecuteNonQuery

		/// <summary>
		/// Executes a single command against a MySQL database.  The <see cref="MySqlConnection"/> is assumed to be
		/// open when the method is called and remains open after the method completes.
		/// </summary>
		/// <param name="connection"><see cref="MySqlConnection"/> object to use</param>
		/// <param name="commandText">SQL command to be executed</param>
		/// <param name="commandParameters">Array of <see cref="MySqlParameter"/> objects to use with the command.</param>
		/// <returns></returns>
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

		/// <summary>
		/// Executes a single command against a MySQL database.  A new <see cref="MySqlConnection"/> is created
		/// using the <see cref="MySqlConnection.ConnectionString"/> given.
		/// </summary>
		/// <param name="connectionString"><see cref="MySqlConnection.ConnectionString"/> to use</param>
		/// <param name="commandText">SQL command to be executed</param>
		/// <param name="parms">Array of <see cref="MySqlParameter"/> objects to use with the command.</param>
		/// <returns></returns>
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

		/// <summary>
		/// Executes a single SQL command and returns the first row of the resultset.  A new MySqlConnection object
		/// is created, opened, and closed during this method.
		/// </summary>
		/// <param name="connectionString">Settings to be used for the connection</param>
		/// <param name="commandText">Command to execute</param>
		/// <param name="parms">Parameters to use for the command</param>
		/// <returns>DataRow containing the first row of the resultset</returns>
		public static DataRow ExecuteDatarow( string connectionString, string commandText, params MySqlParameter[] parms )
		{
			DataSet ds = ExecuteDataset( connectionString, commandText, parms );
			if (ds == null) return null;
			if (ds.Tables.Count == 0) return null;
			if (ds.Tables[0].Rows.Count == 0) return null;
			return ds.Tables[0].Rows[0];
		}

		/// <summary>
		/// Executes a single SQL command and returns the resultset in a <see cref="DataSet"/>.  
		/// A new MySqlConnection object is created, opened, and closed during this method.
		/// </summary>
		/// <param name="connectionString">Settings to be used for the connection</param>
		/// <param name="commandText">Command to execute</param>
		/// <returns><see cref="DataSet"/> containing the resultset</returns>
		public static DataSet ExecuteDataset(string connectionString, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteDataset(connectionString, commandText, (MySqlParameter[])null);
		}

		/// <summary>
		/// Executes a single SQL command and returns the resultset in a <see cref="DataSet"/>.  
		/// A new MySqlConnection object is created, opened, and closed during this method.
		/// </summary>
		/// <param name="connectionString">Settings to be used for the connection</param>
		/// <param name="commandText">Command to execute</param>
		/// <param name="commandParameters">Parameters to use for the command</param>
		/// <returns><see cref="DataSet"/> containing the resultset</returns>
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

		/// <summary>
		/// Executes a single SQL command and returns the resultset in a <see cref="DataSet"/>.  
		/// The state of the <see cref="MySqlConnection"/> object remains unchanged after execution
		/// of this method.
		/// </summary>
		/// <param name="connection"><see cref="MySqlConnection"/> object to use</param>
		/// <param name="commandText">Command to execute</param>
		/// <returns><see cref="DataSet"/> containing the resultset</returns>
		public static DataSet ExecuteDataset(MySqlConnection connection, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteDataset(connection, commandText, (MySqlParameter[])null);
		}

		/// <summary>
		/// Executes a single SQL command and returns the resultset in a <see cref="DataSet"/>.  
		/// The state of the <see cref="MySqlConnection"/> object remains unchanged after execution
		/// of this method.
		/// </summary>
		/// <param name="connection"><see cref="MySqlConnection"/> object to use</param>
		/// <param name="commandText">Command to execute</param>
		/// <param name="commandParameters">Parameters to use for the command</param>
		/// <returns><see cref="DataSet"/> containing the resultset</returns>
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

		/// <summary>
		/// Updates the given table with data from the given <see cref="DataSet"/>
		/// </summary>
		/// <param name="connectionString">Settings to use for the update</param>
		/// <param name="commandText">Command text to use for the update</param>
		/// <param name="ds"><see cref="DataSet"/> containing the new data to use in the update</param>
		/// <param name="tablename">Tablename in the dataset to update</param>
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

		/// <summary>
		/// Executes a single command against a MySQL database, possibly inside an existing transaction.
		/// </summary>
		/// <param name="connection"><see cref="MySqlConnection"/> object to use for the command</param>
		/// <param name="transaction"><see cref="MySqlTransaction"/> object to use for the command</param>
		/// <param name="commandText">Command text to use</param>
		/// <param name="commandParameters">Array of <see cref="MySqlParameter"/> objects to use with the command</param>
		/// <param name="ExternalConn">True if the connection should be preserved, false if not</param>
		/// <returns><see cref="MySqlDataReader"/> object ready to read the results of the command</returns>
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

		/// <summary>
		/// Executes a single command against a MySQL database.
		/// </summary>
		/// <param name="connectionString">Settings to use for this command</param>
		/// <param name="commandText">Command text to use</param>
		/// <returns><see cref="MySqlDataReader"/> object ready to read the results of the command</returns>
		public static MySqlDataReader ExecuteReader(string connectionString, string commandText)
		{
			//pass through the call providing null for the set of SqlParameters
			return ExecuteReader(connectionString, commandText, (MySqlParameter[])null);
		}

		/// <summary>
		/// Executes a single command against a MySQL database.
		/// </summary>
		/// <param name="connectionString">Settings to use for this command</param>
		/// <param name="commandText">Command text to use</param>
		/// <param name="commandParameters">Array of <see cref="MySqlParameter"/> objects to use with the command</param>
		/// <returns><see cref="MySqlDataReader"/> object ready to read the results of the command</returns>
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

		/// <summary>
		/// Execute a single command against a MySQL database.
		/// </summary>
		/// <param name="connectionString">Settings to use for the update</param>
		/// <param name="commandText">Command text to use for the update</param>
		/// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
		public static object ExecuteScalar(string connectionString, string commandText)
		{
			//pass through the call providing null for the set of MySqlParameters
			return ExecuteScalar(connectionString, commandText, (MySqlParameter[])null);
		}

		/// <summary>
		/// Execute a single command against a MySQL database.
		/// </summary>
		/// <param name="connectionString">Settings to use for the command</param>
		/// <param name="commandText">Command text to use for the command</param>
		/// <param name="commandParameters">Parameters to use for the command</param>
		/// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
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

		/// <summary>
		/// Execute a single command against a MySQL database.
		/// </summary>
		/// <param name="connection"><see cref="MySqlConnection"/> object to use</param>
		/// <param name="commandText">Command text to use for the command</param>
		/// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
		public static object ExecuteScalar(MySqlConnection connection, string commandText)
		{
			//pass through the call providing null for the set of MySqlParameters
			return ExecuteScalar(connection, commandText, (MySqlParameter[])null);
		}

		/// <summary>
		/// Execute a single command against a MySQL database.
		/// </summary>
		/// <param name="connection"><see cref="MySqlConnection"/> object to use</param>
		/// <param name="commandText">Command text to use for the command</param>
		/// <param name="commandParameters">Parameters to use for the command</param>
		/// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
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
