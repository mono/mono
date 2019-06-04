// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;

namespace System.Data.SqlClient
{
	public enum SqlConnectionColumnEncryptionSetting 
	{
		Disabled = 0,
		Enabled,
	}

	public enum SqlAuthenticationMethod 
	{
		NotSpecified = 0,
		SqlPassword,
		ActiveDirectoryPassword,
		ActiveDirectoryIntegrated,
	}

	public enum SqlCommandColumnEncryptionSetting 
	{
		UseConnectionSetting = 0,
		Enabled,
		ResultSetOnly,
		Disabled,
	}

	public enum PoolBlockingPeriod
	{
		Auto,
		AlwaysBlock,
		NeverBlock
	}
}