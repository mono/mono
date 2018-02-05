// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace System.Data.SqlClient
{
	partial class SqlConnectionStringBuilder
	{
		[Obsolete("This property is ignored beginning in .NET Framework 4.5." +
			"For more information about SqlClient support for asynchronous programming, see" +
			"https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/asynchronous-programming")]
		public bool AsynchronousProcessing { get; set; }

		[Obsolete("ConnectionReset has been deprecated.  SqlConnection will ignore the 'connection reset'" +
			"keyword and always reset the connection")]
		public bool ConnectionReset { get; set; }

		[MonoTODO("Not implemented in corefx: https://github.com/dotnet/corefx/issues/22474")]
		public SqlAuthenticationMethod Authentication {
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		[MonoTODO("Not implemented in corefx: https://github.com/dotnet/corefx/issues/22474")]
		public bool ContextConnection {
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		[MonoTODO("Not implemented in corefx: https://github.com/dotnet/corefx/issues/22474")]
		public string NetworkLibrary {
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		[MonoTODO("Not implemented in corefx: https://github.com/dotnet/corefx/issues/22474")]
		public PoolBlockingPeriod PoolBlockingPeriod {
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		[MonoTODO("Not implemented in corefx: https://github.com/dotnet/corefx/issues/22474")]
		public bool TransparentNetworkIPResolution {
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		[MonoTODO("Not implemented in corefx: https://github.com/dotnet/corefx/issues/22474")]
		public SqlConnectionColumnEncryptionSetting ColumnEncryptionSetting {
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}
	}
}