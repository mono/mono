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
		public string Charset;
		public string Hostname = "localhost";
		public string Language;
		public string LibraryName = "Mono";
		public string Password;
		public string ProgName = "Mono";
		public string User;
	}
}
