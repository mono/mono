//
// Mono.Data.Tds.Protocol.TdsConnectionParameters.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;
namespace Mono.Data.Tds.Protocol {
	public class TdsConnectionParameters
	{
		public string ApplicationName = "Mono";
		public string Database = String.Empty;
		public string Charset = String.Empty;
		public string Hostname = "localhost";
		public string Language = String.Empty;
		public string LibraryName = "Mono";
		public string Password = String.Empty;
		public string ProgName = "Mono";
		public string User = String.Empty;
	}
}
