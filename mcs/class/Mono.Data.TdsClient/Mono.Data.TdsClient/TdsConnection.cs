//
// Mono.Data.TdsClient.TdsConnection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.ComponentModel;

namespace Mono.Data.TdsClient {
        public sealed class TdsConnection : Component, ICloneable
	{
		#region Fields

		TdsServerType serverType;
		TdsVersion tdsVersion = TdsVersion.tds42; 
		// default to TDS version 4.2 which is used by both servers

		#endregion // Fields

		#region Constructors

		public TdsConnection (TdsServerType serverType)
		{
		}

		#endregion // Constructors

		#region Properties

		public TdsVersion TdsVersion {
			get {
				return tdsVersion;
			}
			set {
				tdsVersion = value;
			}
		}

		#endregion // Properties

		#region Methods

                object ICloneable.Clone()
                {
                        throw new NotImplementedException ();
                }

		#endregion // Methods
	}
}
