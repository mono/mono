//
// Mono.Data.TdsClient.Internal.TdsConnectionParametersInternal.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

namespace Mono.Data.TdsClient.Internal {
	internal class TdsConnectionParametersInternal 
	{
		public string ApplicationName = "Mono.Data.TdsClient Data Provider";
		public string Database;
		public string Encoding;
		public string Host;
		public string Language = "us_english";
		public string LibraryName = "Mono.Data.TdsClient";
		public int PacketSize;
		public string Password;
		public int Port;
		public string ProgName = "Mono.Data.TdsClient Data Provider";
		public TdsVersionInternal TdsVersion;
		public string User;
	}
}
