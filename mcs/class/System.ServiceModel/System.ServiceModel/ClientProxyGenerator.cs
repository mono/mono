//
// ClientProxyGenerator.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Mono.CodeGeneration;

namespace System.ServiceModel
{
	internal class ClientProxyGenerator
	{
		public static Type CreateProxyType (ContractDescription cd)
		{
			string modname = "dummy";

			// public class __clientproxy_MyContract : ClientRuntimeChannel, [ContractType]
			CodeClass c = new CodeModule (modname).CreateClass (
				"__clientproxy_" + cd.Name,
				typeof (ClientRuntimeChannel),
				new Type [] {cd.ContractType});

			//
			// public __clientproxy_MyContract (
			//	ClientRuntime arg1, ChannelFactory arg2)
			//	: base (arg1, arg2)
			// {
			// }
			//
			Type [] ctorargs = new Type [] {typeof (ClientRuntime),
				typeof (ChannelFactory)};
			CodeMethod ctor = c.CreateConstructor (
				MethodAttributes.Public, ctorargs);
			CodeBuilder b = ctor.CodeBuilder;
			MethodBase baseCtor = typeof (ClientRuntimeChannel).GetConstructors (
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) [0];
			if (baseCtor == null) throw new Exception ("INTERNAL ERROR: ClientRuntimeChannel#.ctor(ClientRuntime,ChannelFactory) does not exist.");
			b.Call (
				ctor.GetThis (),
				baseCtor,
				new CodeArgumentReference (typeof (ClientRuntime), 1, "arg0"),
				new CodeArgumentReference (typeof (ChannelFactory), 2, "arg1"));

			// member implementation
			foreach (OperationDescription od in cd.Operations) {
				// FIXME: handle properties and events.
				if (od.SyncMethod != null)
					GenerateMethodImpl (c, typeof (ClientRuntimeChannel).GetMethod ("Process"), od.Name, od.SyncMethod);
				if (od.BeginMethod != null)
					GenerateMethodImpl (c, typeof (ClientRuntimeChannel).GetMethod ("BeginProcess"), od.Name, od.BeginMethod);
				if (od.EndMethod != null)
					GenerateMethodImpl (c, typeof (ClientRuntimeChannel).GetMethod ("EndProcess"), od.Name, od.EndMethod);
			}

			//Type zzz = c.CreateType ();
			//((System.Reflection.Emit.AssemblyBuilder) zzz.Assembly).Save (modname + ".dll");
			//return zzz;
			return c.CreateType ();
		}

		static void GenerateMethodImpl (CodeClass c, MethodInfo processMethod, string name, MethodInfo mi)
		{
			CodeMethod m = c.ImplementMethod (mi);
			CodeBuilder b = m.CodeBuilder;
			// object [] parameters = new object [x];
			// parameters [0] = arg1;
			// parameters [1] = arg2;
			// ...
			// (return) Process (MethodBase.GetCurrentMethod(), operName, parameters);
			ParameterInfo [] pinfos = mi.GetParameters ();
			CodeVariableDeclaration paramsDecl = new CodeVariableDeclaration (typeof (object []), "parameters");
			b.CurrentBlock.Add (paramsDecl);
			CodeVariableReference paramsRef = paramsDecl.Variable;
			b.Assign (paramsRef,
				  new CodeNewArray (typeof (object), new CodeLiteral (pinfos.Length)));
			for (int i = 0; i < pinfos.Length; i++) {
				ParameterInfo par = pinfos [i];
				if (!par.IsOut)
					b.Assign (
						new CodeArrayItem (paramsRef, new CodeLiteral (i)),
						new CodeCast (typeof (object),
							new CodeArgumentReference (par.ParameterType, par.Position + 1, "arg" + i)));
			}
			CodeMethodCall argMethodInfo = new CodeMethodCall (typeof (MethodBase), "GetCurrentMethod");
			CodeLiteral argOperName = new CodeLiteral (name);
			CodeVariableReference retValue = null;
			if (mi.ReturnType == typeof (void))
				b.Call (m.GetThis (), processMethod, argMethodInfo, argOperName, paramsRef);
			else {
				CodeVariableDeclaration retValueDecl = new CodeVariableDeclaration (mi.ReturnType, "retValue");
				b.CurrentBlock.Add (retValueDecl);
				retValue = retValueDecl.Variable;
				b.Assign (retValue,
					new CodeCast (mi.ReturnType,
						b.CallFunc (m.GetThis (), processMethod, argMethodInfo, argOperName, paramsRef)));
			}
			for (int i = 0; i < pinfos.Length; i++) {
				ParameterInfo par = pinfos [i];
				if (par.IsOut || par.ParameterType.IsByRef)
					b.Assign (
						new CodeArgumentReference (par.ParameterType, par.Position + 1, "arg" + i),
						new CodeCast (par.ParameterType.GetElementType (),
							new CodeArrayItem (paramsRef, new CodeLiteral (i))));
			}
			if (retValue != null)
				b.Return (retValue);
		}
	}
}
