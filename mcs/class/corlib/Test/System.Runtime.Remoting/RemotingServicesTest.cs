//
// MonoTests.System.Runtime.Remoting.RemotingServicesTest.cs
//
// Author: Alexis Christoforides (alchri@microsoft.com)
//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Remoting
{
	[TestFixture]
	public class RemotingServicesTest
	{
		public class AppDomainObject : MarshalByRefObject
		{
			public void Init(CrossDomainSerializedObject applicationDependencies) // racy exception here
			{

			}
		}

		public class CrossDomainSerializedObject : MarshalByRefObject
		{
		}
		private static CrossDomainSerializedObject crossDomainSerializedObject;

		private static void AppDomainWithRemotingSerialization(Assembly assembly, string name)
		{
				var appDomain = AppDomain.CreateDomain(name);
				var appDomainObject = (AppDomainObject)appDomain.CreateInstanceAndUnwrap(assembly.GetName().Name, typeof(AppDomainObject).FullName);
				appDomainObject.Init(crossDomainSerializedObject);
		}

		[Test]
		public void Bug46473 () // concurrent serialization/deserialization
		{
			bool success = true;
			crossDomainSerializedObject = new CrossDomainSerializedObject();
			Task[] tasks = new Task [20];
			for (int i = 0; i < tasks.Length; i++)
			{
				var assembly = Assembly.GetAssembly(typeof(AppDomainObject));
				var name = "AppDomainWithCall" + i;
				tasks [i] = Task.Factory.StartNew(() => AppDomainWithRemotingSerialization(assembly, name));
			}
			try {
				Task.WaitAll (tasks);
			} catch (AggregateException e) {
				success = false;
				Console.WriteLine ($"{e}, {e.InnerException}");
			}
			Assert.IsTrue (success, "Bug46473 (exception during remoting call)");
		}
	}
}
