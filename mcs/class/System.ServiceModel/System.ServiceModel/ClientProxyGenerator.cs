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
using System.ServiceModel.MonoInternal;

namespace System.ServiceModel
{
	internal class ClientProxyKey {
		Type contractInterface;
		ContractDescription cd;
		bool duplex;

		public ClientProxyKey (Type contractInterface, ContractDescription cd, bool duplex) {
			this.contractInterface = contractInterface;
			this.cd = cd;
			this.duplex = duplex;
		}


		public override int GetHashCode () {
			return contractInterface.GetHashCode () ^ cd.GetHashCode ();
		}

		public override bool Equals (object o) {
			ClientProxyKey key = o as ClientProxyKey;
			if (key == null)
				return false;
			return contractInterface == key.contractInterface && cd == key.cd && duplex == key.duplex;
		}
	}

	internal class ClientProxyGenerator : ProxyGeneratorBase
	{
		static Dictionary<ClientProxyKey, Type> proxy_cache = new Dictionary<ClientProxyKey, Type> ();


		public static Type CreateProxyType (Type requestedType, ContractDescription cd, bool duplex)
		{
			ClientProxyKey key = new ClientProxyKey (requestedType, cd, duplex);
			Type res;
			lock (proxy_cache) {
				if (proxy_cache.TryGetValue (key, out res))
					return res;
			}

			string modname = "dummy";
			Type crtype =
#if !NET_2_1
				duplex ? typeof (DuplexClientRuntimeChannel) :
#endif
				typeof (ClientRuntimeChannel);

			// public class __clientproxy_MyContract : (Duplex)ClientRuntimeChannel, [ContractType]
			var types = new List<Type> ();
			types.Add (requestedType);
			if (!cd.ContractType.IsAssignableFrom (requestedType))
				types.Add (cd.ContractType);
			if (cd.CallbackContractType != null && !cd.CallbackContractType.IsAssignableFrom (requestedType))
				types.Add (cd.CallbackContractType);
			CodeClass c = new CodeModule (modname).CreateClass ("__clientproxy_" + cd.Name, crtype, types.ToArray ());

			//
			// public __clientproxy_MyContract (
			//	ServiceEndpoint arg1, ChannelFactory arg2, EndpointAddress arg3, Uri arg4)
			//	: base (arg1, arg2, arg3, arg4)
			// {
			// }
			//
			Type [] ctorargs = new Type [] {typeof (ServiceEndpoint), typeof (ChannelFactory), typeof (EndpointAddress), typeof (Uri)};
			CodeMethod ctor = c.CreateConstructor (
				MethodAttributes.Public, ctorargs);
			CodeBuilder b = ctor.CodeBuilder;
			MethodBase baseCtor = crtype.GetConstructors (
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) [0];
			if (baseCtor == null) throw new Exception ("INTERNAL ERROR: ClientRuntimeChannel.ctor() was not found.");
			b.Call (
				ctor.GetThis (),
				baseCtor,
				new CodeArgumentReference (typeof (ServiceEndpoint), 1, "arg0"),
				new CodeArgumentReference (typeof (ChannelFactory), 2, "arg1"),
				new CodeArgumentReference (typeof (EndpointAddress), 3, "arg2"),
				new CodeArgumentReference (typeof (Uri), 4, "arg3"));
			res = CreateProxyTypeOperations (crtype, c, cd);

			lock (proxy_cache) {
				proxy_cache [key] = res;
			}
			return res;
		}
	}

	internal class ProxyGeneratorBase
	{
		protected static Type CreateProxyTypeOperations (Type crtype, CodeClass c, ContractDescription cd)
		{
			// member implementation
			BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			foreach (OperationDescription od in cd.Operations) {
				// FIXME: handle properties and events.
#if !NET_2_1
				if (od.SyncMethod != null)
					GenerateMethodImpl (c, crtype.GetMethod ("Process", bf), od.Name, od.SyncMethod);
#endif
				if (od.BeginMethod != null)
					GenerateBeginMethodImpl (c, crtype.GetMethod ("BeginProcess", bf), od.Name, od.BeginMethod);
				if (od.EndMethod != null)
					GenerateEndMethodImpl (c, crtype.GetMethod ("EndProcess", bf), od.Name, od.EndMethod);
			}

			Type ret = c.CreateType ();
			return ret;
		}

		static void GenerateMethodImpl (CodeClass c, MethodInfo processMethod, string name, MethodInfo mi)
		{
			CodeMethod m = c.ImplementMethod (mi);
			CodeBuilder b = m.CodeBuilder;
			// object [] parameters = new object [x];
			// parameters [0] = arg1;
			// parameters [1] = arg2;
			// ...
			// (return) Process (Contract.Operations [operName].SyncMethod, operName, parameters);
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
#if USE_OD_REFERENCE_IN_PROXY
			CodePropertyReference argMethodInfo = GetOperationMethod (m, b, name, "SyncMethod");
#else
			CodeMethodCall argMethodInfo = new CodeMethodCall (typeof (MethodBase), "GetCurrentMethod");
#endif
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

		static CodePropertyReference GetOperationMethod (CodeMethod m, CodeBuilder b, string name, string methodPropertyName)
		{
			return new CodePropertyReference (
				b.CallFunc (
					// this.Contract.Operations
					new CodePropertyReference (
						new CodePropertyReference (
							m.GetThis (),
							typeof (ClientRuntimeChannel).GetProperty ("Contract")),
						typeof (ContractDescription).GetProperty ("Operations")),
					// .Find (name)
					typeof (OperationDescriptionCollection).GetMethod ("Find"),
					new CodeLiteral (name)),
				// .SyncMethod
				typeof (OperationDescription).GetProperty (methodPropertyName));
		}

		static void GenerateBeginMethodImpl (CodeClass c, MethodInfo beginProcessMethod, string name, MethodInfo mi)
		{
			CodeMethod m = c.ImplementMethod (mi);
			CodeBuilder b = m.CodeBuilder;
			// object [] parameters = new object [x];
			// parameters [0] = arg1;
			// parameters [1] = arg2;
			// ...
			// (return) BeginProcess (Contract.Operations [operName].BeginMethod, operName, parameters, asyncCallback, userState);
			ParameterInfo [] pinfos = mi.GetParameters ();
			CodeVariableDeclaration paramsDecl = new CodeVariableDeclaration (typeof (object []), "parameters");
			b.CurrentBlock.Add (paramsDecl);
			CodeVariableReference paramsRef = paramsDecl.Variable;
			b.Assign (paramsRef,
				  new CodeNewArray (typeof (object), new CodeLiteral (pinfos.Length - 2)));
			for (int i = 0; i < pinfos.Length - 2; i++) {
				ParameterInfo par = pinfos [i];
				if (!par.IsOut)
					b.Assign (
						new CodeArrayItem (paramsRef, new CodeLiteral (i)),
						new CodeCast (typeof (object), m.GetArg (i)));
			}
#if USE_OD_REFERENCE_IN_PROXY
			CodePropertyReference argMethodInfo = GetOperationMethod (m, b, name, "BeginMethod");
#else
			CodeMethodCall argMethodInfo = new CodeMethodCall (typeof (MethodBase), "GetCurrentMethod");
#endif
			CodeLiteral argOperName = new CodeLiteral (name);

			ParameterInfo p = pinfos [pinfos.Length - 2];
			CodeArgumentReference callbackRef = new CodeArgumentReference (typeof (AsyncCallback), p.Position + 1, p.Name);
			p = pinfos [pinfos.Length - 1];
			CodeArgumentReference stateRef = new CodeArgumentReference (typeof (object), p.Position + 1, p.Name);

			CodeVariableDeclaration retValueDecl = new CodeVariableDeclaration (mi.ReturnType, "retValue");
			b.CurrentBlock.Add (retValueDecl);
			CodeVariableReference retValue = retValueDecl.Variable;
			b.Assign (retValue,
				new CodeCast (mi.ReturnType,
					b.CallFunc (m.GetThis (), beginProcessMethod, argMethodInfo, argOperName, paramsRef, callbackRef, stateRef)));

			b.Return (retValue);
		}

		static void GenerateEndMethodImpl (CodeClass c, MethodInfo endProcessMethod, string name, MethodInfo mi)
		{
			CodeMethod m = c.ImplementMethod (mi);
			CodeBuilder b = m.CodeBuilder;
			ParameterInfo [] pinfos = mi.GetParameters ();

			ParameterInfo p = pinfos [0];
			CodeArgumentReference asyncResultRef = m.GetArg (0);
			
			CodeVariableDeclaration paramsDecl = new CodeVariableDeclaration (typeof (object []), "parameters");
			b.CurrentBlock.Add (paramsDecl);
			CodeVariableReference paramsRef = paramsDecl.Variable;
			b.Assign (paramsRef,
				  new CodeNewArray (typeof (object), new CodeLiteral (pinfos.Length - 1)));
			/*
			for (int i = 0; i < pinfos.Length - 2; i++) {
				ParameterInfo par = pinfos [i];
				if (!par.IsOut)
					b.Assign (
						new CodeArrayItem (paramsRef, new CodeLiteral (i)),
						new CodeCast (typeof (object),
							new CodeArgumentReference (par.ParameterType, par.Position + 1, "arg" + i)));
			}
			*/
#if USE_OD_REFERENCE_IN_PROXY
			CodePropertyReference argMethodInfo = GetOperationMethod (m, b, name, "EndMethod");
#else
			CodeMethodCall argMethodInfo = new CodeMethodCall (typeof (MethodBase), "GetCurrentMethod");
#endif
			CodeLiteral argOperName = new CodeLiteral (name);
			
			CodeVariableReference retValue = null;
			if (mi.ReturnType == typeof (void))
				b.Call (m.GetThis (), endProcessMethod, argMethodInfo, argOperName, paramsRef, asyncResultRef);
			else {
				CodeVariableDeclaration retValueDecl = new CodeVariableDeclaration (mi.ReturnType, "retValue");
				b.CurrentBlock.Add (retValueDecl);
				retValue = retValueDecl.Variable;
				b.Assign (retValue,
					new CodeCast (mi.ReturnType,
						b.CallFunc (m.GetThis (), endProcessMethod, argMethodInfo, argOperName, paramsRef, asyncResultRef)));
			}
			// FIXME: fill out parameters
			if (retValue != null)
				b.Return (retValue);
		}
	}
}
