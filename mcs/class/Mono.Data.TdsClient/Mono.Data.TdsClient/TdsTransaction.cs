//
// Mono.Data.TdsClient.TdsTransaction.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.ComponentModel;

namespace Mono.Data.TdsClient {
        public sealed class TdsTransaction : Component, ICloneable
	{
		#region Constructors

		public TdsTransaction (TdsConnection connection, IsolationLevel isolevel)
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
