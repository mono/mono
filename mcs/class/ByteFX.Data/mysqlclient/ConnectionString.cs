using System;
using ByteFX.Data.Common;

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Summary description for MySqlConnectionString.
	/// </summary>
	internal sealed class MySqlConnectionString : DBConnectionString
	{
		private bool useCompression;

		public MySqlConnectionString() 
		{
			connectLifetime = 0;
			pooling = true;
			minPoolSize = 0;
			maxPoolSize = 100;
			connectTimeout = 15;
			port = 3306;
		}

		public MySqlConnectionString(string connectString) : base(connectString)
		{
			connectLifetime = 0;
			pooling = true;
			minPoolSize = 0;
			maxPoolSize = 100;
			connectTimeout = 15;
			port = 3306;
			Parse();
		}

		#region Properties
		public bool UseCompression 
		{
			get { return useCompression; }
		}
		#endregion

		protected override void ConnectionParameterParsed(string key, string value)
		{
			switch (key.ToLower()) 
			{
				case "use compression":
				case "compress":
					if (value.ToLower() == "no" || value.ToLower() == "false")
						useCompression = false;
					else
						useCompression = true;
					break;
			}

			base.ConnectionParameterParsed(key, value);
		}

	}
}
