//
// Mono.Data.Tds.Protocol.TdsConnectionParameters.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

namespace Mono.Data.Tds.Protocol {
	public class TdsConnectionParameters
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
