//
// Mono.Data.TdsClient.Internal.TdsConnectionParameters.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

namespace Mono.Data.TdsClient.Internal {
	internal class TdsConnectionParameters
	{
		public string ApplicationName = "Mono";
		public string Database;
		public string DataSource;
		public string Encoding = "iso-8859-1";
		public string Hostname = "localhost";
		public string Language = "us_english";
		public string LibraryName = "Mono";
		public int PacketSize;
		public string Password;
		public int Port = 1433;
		public string ProgName = "Mono";
		public TdsVersion TdsVersion = TdsVersion.tds42;
		public string User;
	}
}
