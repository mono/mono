// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace System.Data.SqlClient
{
	partial class SqlCommand : IDbCommand, ICloneable, IDisposable
	{
		[MonoTODO]
		public bool NotificationAutoEnlist
		{
			get => Notification != null;
			set => throw new NotImplementedException();
		}

		[System.Security.Permissions.HostProtectionAttribute(ExternalThreading=true)]
		public IAsyncResult BeginExecuteReader() =>
			BeginExecuteReader(CommandBehavior.Default, null, null);

		[System.Security.Permissions.HostProtectionAttribute(ExternalThreading=true)]
		public IAsyncResult BeginExecuteReader(AsyncCallback callback, object stateObject) =>
			BeginExecuteReader(CommandBehavior.Default, callback, stateObject);

		[System.Security.Permissions.HostProtectionAttribute(ExternalThreading=true)]
		public IAsyncResult BeginExecuteReader(AsyncCallback callback, object stateObject, CommandBehavior behavior) =>
			BeginExecuteReader(behavior, callback, stateObject);

		[System.Security.Permissions.HostProtectionAttribute(ExternalThreading=true)]
		public IAsyncResult BeginExecuteReader(CommandBehavior behavior) =>
			BeginExecuteReader(behavior, null, null);
	}
}