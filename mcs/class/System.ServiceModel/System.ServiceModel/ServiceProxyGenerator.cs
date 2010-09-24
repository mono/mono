//
// ServiceProxyGenerator.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Mono.CodeGeneration;
using System.ServiceModel.MonoInternal;

namespace System.ServiceModel
{
	internal class ServiceProxyGenerator : ProxyGeneratorBase
	{
		public static Type CreateCallbackProxyType (Type serviceType, Type callbackType)
		{
			var cd = ContractDescriptionGenerator.GetCallbackContract (serviceType, callbackType);
			string modname = "dummy";
			Type crtype = typeof (DuplexServiceRuntimeChannel);

			// public class __clientproxy_MyContract : ClientRuntimeChannel, [ContractType]
			CodeClass c = new CodeModule (modname).CreateClass (
				"__callbackproxy_" + cd.Name,
				crtype,
				new Type [] {callbackType});

			//
			// public __callbackproxy_MyContract (
			//	IChannel channel, DispatchRuntime runtime)
			//	: base (channel, runtime)
			// {
			// }
			//
			Type [] ctorargs = new Type [] {typeof (IChannel), typeof (DispatchRuntime)};
			CodeMethod ctor = c.CreateConstructor (
				MethodAttributes.Public, ctorargs);
			CodeBuilder b = ctor.CodeBuilder;
			MethodBase baseCtor = crtype.GetConstructors (
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) [0];
			if (baseCtor == null) throw new Exception ("INTERNAL ERROR: DuplexServiceRuntimeChannel.ctor() was not found.");
			b.Call (
				ctor.GetThis (),
				baseCtor,
				new CodeArgumentReference (typeof (IChannel), 1, "arg0"),
				new CodeArgumentReference (typeof (DispatchRuntime), 2, "arg1"));

			return CreateProxyTypeOperations (crtype, c, cd);
		}
	}
}
