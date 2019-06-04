// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using System.Text;
using System.IO;

namespace System.Diagnostics
{
	// this file can be used in order to ignore all RemoteInvoke-tests
	// should be used instead of /corefx/.../RemoteExecutorTestBase.Process.cs
	public abstract partial class RemoteExecutorTestBase : FileCleanupTestBase
	{
		static RemoteInvokeHandle RemoteInvoke (MethodInfo method, string[] args, 
			RemoteInvokeOptions options, bool pasteArguments = true)
		{
			options = options ?? new RemoteInvokeOptions ();
			//do nothing. Or we can invoke the method in the same process instead:
			//method.Invoke(Activator.CreateInstance(method.DeclaringType), args.OfType<object>().ToArray());
			return new RemoteInvokeHandle (null, null);
		}
	}
}