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
		public string ApplicationName = "Mono.Data.TdsClient Data Provider";
		public string Database;
		public string Encoding = "iso-8859-1";
		public string Host;
		public string Language = "us_english";
		public string LibraryName = "Mono.Data.TdsClient";
		public int PacketSize;
		public string Password;
		public int Port = 1433;
		public string ProgName = "Mono.Data.TdsClient Data Provider";
		public TdsVersion TdsVersion = TdsVersion.tds42;
		public string User;
	}
}
