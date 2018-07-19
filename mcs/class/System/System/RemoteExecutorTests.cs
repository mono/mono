// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;
using System;
using System.IO;
using System.Diagnostics;

public class RemoteExecutorTests : RemoteExecutorTestBase
{
	[Fact]
	public void RemoteInvokeWritesToFile ()
	{
		var file = Path.GetTempFileName ();

		RemoteInvoke (arg =>
			{
				File.WriteAllText (arg, "42");
				return SuccessExitCode;
			}, 
			file).Dispose ();

		Assert.Equal (File.ReadAllText (file), "42");
		File.Delete (file);
	}
}