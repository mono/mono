using System;
using ByteFX.Data.Common;

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// 
	/// </summary>
	internal sealed class MySqlInternalConnection
	{
		MySqlConnectionString	settings;
		Driver					driver;
		DateTime				createTime;
		bool					serverVariablesSet;

		public MySqlInternalConnection( MySqlConnectionString connectString )
		{
			settings = connectString;
			serverVariablesSet = false;
		}

		#region Properties
		public MySqlConnectionString Settings 
		{
			get { return settings; }
			set { settings = value; }
		}

		internal Driver Driver 
		{
			get { return driver; }
		}

		#endregion

		#region Methods

		public bool IsAlive() 
		{
			try 
			{
				CommandResult result = driver.Send( DBCmd.PING, (byte[])null );
				// we don't care about the result.  The fact that it responded is enough
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public bool IsTooOld() 
		{
			TimeSpan ts = DateTime.Now.Subtract( createTime );
			if (ts.Seconds > settings.ConnectionLifetime)
				return true;
			return false;
		}

		/// <summary>
		/// I don't like this setup but can't think of a better way of doing
		/// right now.
		/// </summary>
		/// <param name="connection"></param>
		public void SetServerVariables(MySqlConnection connection)
		{
			if (serverVariablesSet) return;

			// retrieve the encoding that should be used for character data
			MySqlCommand cmd = new MySqlCommand("show variables like 'max_allowed_packet'", connection);
			try 
			{
				MySqlDataReader reader = cmd.ExecuteReader();
				reader.Read();
				driver.MaxPacketSize = reader.GetInt64( 1 );
				reader.Close();
			}
			catch (Exception)
			{
				driver.MaxPacketSize = 1047552;
			}

			cmd.CommandText = "show variables like 'character_set'";
			driver.Encoding = System.Text.Encoding.Default;
		
			try 
			{
				MySqlDataReader reader = cmd.ExecuteReader();
				if (reader.Read())
					driver.Encoding = CharSetMap.GetEncoding( reader.GetString(1) );
				reader.Close();
			}
			catch 
			{ 
				throw new MySqlException("Failure to initialize connection");
			}

			serverVariablesSet = true;
		}

		public void Open() 
		{
			driver = new Driver();
			driver.Open( settings );

			createTime = DateTime.Now;
		}

		public void Close() 
		{
			driver.Close();
		}

		#endregion

	}
}
