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

		#endregion // Fields

		#region Constructors

		public TdsConnection (TdsServerType serverType)
		{
		}

		#endregion // Constructors

		#region Methods

                object ICloneable.Clone()
                {
                        throw new NotImplementedException ();
                }

		#endregion // Methods
	}
}
