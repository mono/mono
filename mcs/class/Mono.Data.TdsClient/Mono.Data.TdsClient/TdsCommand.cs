//
// Mono.Data.TdsClient.TdsCommand.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.ComponentModel;

namespace Mono.Data.TdsClient {
        public sealed class TdsCommand : Component, ICloneable
	{
		#region Fields

		string commandText;
		TdsConnection connection;
		TdsTransaction transaction;

		#endregion // Fields

		#region Constructors

		public TdsCommand ()
		{
			commandText = String.Empty;
			connection = null;
			transaction = null;
		}

		#endregion // Constructors

		#region Properties

		public string CommandText {
			get { return commandText; }
			set { commandText = value; }
		}

		public TdsConnection Connection {
			get { return connection; }
			set { connection = value; }
		}

		public TdsTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
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
