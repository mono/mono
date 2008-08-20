//
// ServiceAssert.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.ObjectModel;
using System.Net.Security;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	public static class ServiceAssert
	{
		public static void AssertOperationDescription (
			string name, MethodInfo begin,
			MethodInfo end, MethodInfo sync,
			bool isInitiating, bool isOneWay, bool isTerminating,
			OperationDescription od, string label)
		{
			Assert.AreEqual (name, od.Name, label + " Name");
			Assert.AreEqual (begin, od.BeginMethod, label + " BeginMethod");
			Assert.AreEqual (end, od.EndMethod, label + " EndMethod");
			Assert.AreEqual (sync, od.SyncMethod, label + " SyncMethod");
			Assert.AreEqual (isInitiating, od.IsInitiating, label + " IsInitiating");
			Assert.AreEqual (isOneWay, od.IsOneWay, label + " IsOneWay");
			Assert.AreEqual (isTerminating, od.IsTerminating, label + " IsTerminating");
		}

		public static void AssertContractDescription (
			string name, string ns,
			SessionMode session, Type contractType,
			Type callbackContractType,
			ContractDescription cd, string label)
		{
			Assert.AreEqual (name, cd.Name, label + " Name");
			Assert.AreEqual (ns, cd.Namespace, label + " Namespace");
			Assert.AreEqual (session, cd.SessionMode, label + " Session");
			Assert.AreEqual (contractType, cd.ContractType,
				label + " ContractType");
			Assert.AreEqual (callbackContractType, cd.CallbackContractType,
				label + " CallbackContractType");
		}

		public static void AssertMessageAndBodyDescription (
			string action, MessageDirection dir,
			Type messageType, string bodyWrapperName,
			string bodyWrapperNS, bool bodyHasReturn,
			MessageDescription md, string label)
		{
			Assert.AreEqual (action, md.Action, label + " Action");
			Assert.AreEqual (dir, md.Direction, label + " Direction");
			Assert.AreEqual (messageType, md.MessageType, label + " MessageType");
			Assert.AreEqual (bodyWrapperName, md.Body.WrapperName,
				label + " Body.WrapperName");
			Assert.AreEqual (bodyWrapperNS, md.Body.WrapperNamespace,
				label + " Body.WrapperNamespace");
			Assert.AreEqual (bodyHasReturn, md.Body.ReturnValue != null,
				label + "Body hasReturn");
		}

		public static void AssertMessagePartDescription (
			string name, string ns, int index, bool multiple,
			ProtectionLevel? protectionLevel, Type type,
			MessagePartDescription mp, string label)
		{
			Assert.AreEqual (name, mp.Name, label + " Name");
			Assert.AreEqual (ns, mp.Namespace, label + " Namespace");
			Assert.AreEqual (index, mp.Index, label + " Index");
			Assert.AreEqual (multiple, mp.Multiple,
				label + " Multiple");
			Assert.AreEqual (protectionLevel, mp.ProtectionLevel,
				label + " ProtectionLevel");
			Assert.AreEqual (type, mp.Type, label + " Type");
		}

		public static void AssertMessageEncoder (
			string contentType, string mediaType, MessageVersion version,
			MessageEncoder encoder, string label)
		{
			// Those curly double quotations are smelly, so just remove them out.
			Assert.AreEqual (contentType.Replace ("\"", ""),
				encoder.ContentType.Replace ("\"", ""),
				label + " ContentType");
			Assert.AreEqual (mediaType, encoder.MediaType, label + " MediaType");
			Assert.AreEqual (version, encoder.MessageVersion, label + " MessageVersion");
		}
	}
}
