//
// Mono.Data.TdsClient.Internal.Tds80.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;

namespace Mono.Data.TdsClient.Internal {
        internal class Tds80 : Tds
	{
		#region Fields

		public static readonly TdsVersion Version = TdsVersion.tds80;

		#endregion // Fields

		#region Constructors

		public Tds80 (string server, int port)
			: this (server, port, 512, 15)
		{
		}

		public Tds80 (string server, int port, int packetSize, int timeout)
			: base (server, port, packetSize, timeout, Version)
		{
		}

		#endregion // Constructors

		#region Methods

		public override bool Connect (TdsConnectionParameters connectionParameters)
		{
			throw new NotImplementedException ();
		}

		protected override TdsPacketColumnInfoResult ProcessColumnInfo ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
