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
			Packet packet;
			try 
			{
				packet = driver.SendSql( "show status like 'uptime'" );
				// we have to read for two last packets since MySql sends
				// us a last packet after schema and again after rows
				// I will likely change this later to have the driver just
				// return schema in one very large packet.
				while (packet.Type != PacketType.Last)
					packet = driver.ReadPacket();
			}
			catch
			{
				return false;
			}
			
			return true;
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
			MySqlCommand cmd = new MySqlCommand("select @@max_allowed_packet", connection);
			driver.MaxPacketSize = Convert.ToInt64(cmd.ExecuteScalar());

			cmd.CommandText = "show variables like 'character_set'";
			MySqlDataReader reader = cmd.ExecuteReader();
			if (reader.Read())
				driver.Encoding = CharSetMap.GetEncoding( reader.GetString(1) );
			else
				driver.Encoding = System.Text.Encoding.Default;
			reader.Close();

			serverVariablesSet = true;
		}

		public void Open() 
		{
			driver = new Driver();
			driver.Open( settings.Host, settings.Port, settings.Username, settings.Password,
				settings.UseCompression, settings.ConnectTimeout );

			createTime = DateTime.Now;
		}

		public void Close() 
		{
			driver.Close();
		}

		#endregion

	}
}
