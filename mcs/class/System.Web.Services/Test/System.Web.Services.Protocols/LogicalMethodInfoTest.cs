//
// LogicalMethodInfoTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.
//
#if !MOBILE
using NUnit.Framework;

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Web.Services;
using System.Web.Services.Configuration;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MonoTests.System.Web.Services.Protocol
{
	[TestFixture]
	public class LogicalMethodInfoTest
	{
		[Test]
		public void BeginEndMethodInfo ()
		{
			LogicalMethodInfo [] ll = LogicalMethodInfo.Create (
			new MethodInfo [] {
			typeof (FooService).GetMethod ("BeginEcho"),
			typeof (FooService).GetMethod ("EndEcho")});
			Assert.AreEqual (1, ll.Length, "#1");
			LogicalMethodInfo l = ll [0];
			Assert.IsNull (l.MethodInfo, "#2");
			Assert.IsNotNull (l.BeginMethodInfo, "#3");
			Assert.IsNotNull (l.EndMethodInfo, "#4");
		}

		class FooService : WebService
		{
			public string Echo (string arg)
			{
				return arg;
			}

			public IAsyncResult BeginEcho (string arg, AsyncCallback cb, object state)
			{
				return null;
			}

			public string EndEcho (IAsyncResult result)
			{
				return null;
			}
		}
	}
}
#endif
