//
// ContractDescriptionGenerator.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2007 Novell, Inc.  http://www.novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Description
{
	internal static class ContractDescriptionGenerator
	{
		public static OperationContractAttribute
			GetOperationContractAttribute (MethodBase method)
		{
			object [] matts = method.GetCustomAttributes (
				typeof (OperationContractAttribute), false);
			if (matts.Length == 0)
				return null;
			return (OperationContractAttribute) matts [0];
		}

		static void GetServiceContractAttribute (Type type, Dictionary<Type,ServiceContractAttribute> table)
		{
			for (; type != null; type = type.BaseType) {
				foreach (ServiceContractAttribute i in
					type.GetCustomAttributes (
					typeof (ServiceContractAttribute), true))
					table [type] = i;
				foreach (Type t in type.GetInterfaces ())
					GetServiceContractAttribute (t, table);
			}
		}
		public static Dictionary<Type, ServiceContractAttribute> GetServiceContractAttributes (Type type) 
		{
			Dictionary<Type, ServiceContractAttribute> table = new Dictionary<Type, ServiceContractAttribute> ();
			GetServiceContractAttribute (type, table);
			return table;
		}

		public static ContractDescription GetContract (
			Type contractType) {
			return GetContract (contractType, (Type) null);
		}

		public static ContractDescription GetContract (
			Type contractType, object serviceImplementation) {
			if (serviceImplementation == null)
				throw new ArgumentNullException ("serviceImplementation");
			return GetContract (contractType,
				serviceImplementation.GetType ());
		}

		public static MessageContractAttribute GetMessageContractAttribute (Type type)
		{
			for (Type t = type; t != null; t = t.BaseType) {
				object [] matts = t.GetCustomAttributes (
					typeof (MessageContractAttribute), true);
				if (matts.Length > 0)
					return (MessageContractAttribute) matts [0];
			}
			return null;
		}

		public static ContractDescription GetCallbackContract (Type serviceType, Type callbackType)
		{
			return GetContract (callbackType, null, serviceType);
		}

		public static ContractDescription GetContract (
			Type givenContractType, Type givenServiceType)
		{
			return GetContract (givenContractType, givenServiceType, null);
		}

		static ContractDescription GetContract (Type givenContractType, Type givenServiceType, Type serviceTypeForCallback)
		{
			// FIXME: serviceType should be used for specifying attributes like OperationBehavior.

			Type exactContractType = null;
			ServiceContractAttribute sca = null;
			Dictionary<Type, ServiceContractAttribute> contracts = 
				GetServiceContractAttributes (serviceTypeForCallback ?? givenServiceType ?? givenContractType);
			if (contracts.ContainsKey(givenContractType)) {
				exactContractType = givenContractType;
				sca = contracts [givenContractType];
			} else {
				foreach (Type t in contracts.Keys)
					if (t.IsAssignableFrom(givenContractType)) {
						if (t.IsAssignableFrom (exactContractType)) // exact = IDerived, t = IBase
							continue;
						if (sca != null && (exactContractType == null || !exactContractType.IsAssignableFrom (t))) // t = IDerived, exact = IBase
							throw new InvalidOperationException ("The contract type of " + givenContractType + " is ambiguous: can be either " + exactContractType + " or " + t);
						exactContractType = t;
						sca = contracts [t];
					}
			}
			if (exactContractType == null)
				exactContractType = givenContractType;
			if (sca == null) {
				if (serviceTypeForCallback != null)
					sca = contracts.Values.First ();
				else
					throw new InvalidOperationException (String.Format ("Attempted to get contract type from '{0}' which neither is a service contract nor does it inherit service contract.", serviceTypeForCallback ?? givenContractType));
			}
			string name = sca.Name ?? exactContractType.Name;
			string ns = sca.Namespace ?? "http://tempuri.org/";

			ContractDescription cd =
				new ContractDescription (name, ns);
			cd.ContractType = exactContractType;
			cd.CallbackContractType = sca.CallbackContract;
			cd.SessionMode = sca.SessionMode;
			if (sca.ConfigurationName != null)
				cd.ConfigurationName = sca.ConfigurationName;
			else
				cd.ConfigurationName = exactContractType.FullName;
			if (sca.HasProtectionLevel)
				cd.ProtectionLevel = sca.ProtectionLevel;

			FillOperationsForInterface (cd, cd.ContractType, givenServiceType, false);
			if (cd.CallbackContractType != null && cd.CallbackContractType != cd.ContractType)
				FillOperationsForInterface (cd, cd.CallbackContractType, null, true);

			// FIXME: enable this when I found where this check is needed.
			/*
			if (cd.Operations.Count == 0)
				throw new InvalidOperationException (String.Format ("The service contract type {0} has no operation. At least one operation must exist.", contractType));
			*/
			return cd;
		}
		
		static void FillOperationsForInterface (ContractDescription cd, Type exactContractType, Type givenServiceType, bool isCallback)
		{
			// FIXME: load Behaviors
			MethodInfo [] contractMethods = exactContractType.IsInterface ? GetAllMethods (exactContractType) : exactContractType.GetMethods ();
			MethodInfo [] serviceMethods = contractMethods;
			if (givenServiceType != null && exactContractType.IsInterface) {
				var l = new List<MethodInfo> ();
				foreach (Type t in GetAllInterfaceTypes (exactContractType))
					l.AddRange (givenServiceType.GetInterfaceMap (t).TargetMethods);
				serviceMethods = l.ToArray ();
			}
			
			for (int i = 0; i < contractMethods.Length; ++i)
			{

				MethodInfo mi = contractMethods [i];
				OperationContractAttribute oca = GetOperationContractAttribute (mi);
				if (oca == null)
					continue;
				MethodInfo end = null;
				if (oca.AsyncPattern) {
					if (String.Compare ("Begin", 0, mi.Name,0, 5) != 0)
						throw new InvalidOperationException ("For async operation contract patterns, the initiator method name must start with 'Begin'.");
					string endName = "End" + mi.Name.Substring (5);
					end = mi.DeclaringType.GetMethod (endName);
					if (end == null)
						throw new InvalidOperationException (String.Format ("'{0}' method is missing. For async operation contract patterns, corresponding End method is required for each Begin method.", endName));
					if (GetOperationContractAttribute (end) != null)
						throw new InvalidOperationException ("Async 'End' method must not have OperationContractAttribute. It is automatically treated as the EndMethod of the corresponding 'Begin' method.");
				}
				OperationDescription od = GetOrCreateOperation (cd, mi, serviceMethods [i], oca, end != null ? end.ReturnType : null, isCallback);
				if (end != null)
					od.EndMethod = end;
			}
		}

		static MethodInfo [] GetAllMethods (Type type)
		{
			var l = new List<MethodInfo> ();
			foreach (var t in GetAllInterfaceTypes (type)) {
#if MONOTOUCH
				// The MethodBase[] from t.GetMethods () is cast to a IEnumerable <MethodInfo>
				// when passed to List<MethodInfo>.AddRange, which in turn casts it to 
				// ICollection <MethodInfo>.  The full-aot compiler has no idea of this, so
				// we're going to make it aware.
				int c = ((ICollection <MethodInfo>) t.GetMethods ()).Count;
#endif
				l.AddRange (t.GetMethods ());
			}
			return l.ToArray ();
		}

		static IEnumerable<Type> GetAllInterfaceTypes (Type type)
		{
			yield return type;
			foreach (var t in type.GetInterfaces ())
				foreach (var tt in GetAllInterfaceTypes (t))
					yield return tt;
		}

		static OperationDescription GetOrCreateOperation (
			ContractDescription cd, MethodInfo mi, MethodInfo serviceMethod,
			OperationContractAttribute oca,
			Type asyncReturnType,
			bool isCallback)
		{
			string name = oca.Name ?? (oca.AsyncPattern ? mi.Name.Substring (5) : mi.Name);

			OperationDescription od = cd.Operations.FirstOrDefault (o => o.Name == name);
			if (od == null) {
				od = new OperationDescription (name, cd);
				od.IsOneWay = oca.IsOneWay;
				if (oca.HasProtectionLevel)
					od.ProtectionLevel = oca.ProtectionLevel;
				od.Messages.Add (GetMessage (od, mi, oca, true, isCallback, null));
				if (!od.IsOneWay)
					od.Messages.Add (GetMessage (od, mi, oca, false, isCallback, asyncReturnType));
				foreach (ServiceKnownTypeAttribute a in cd.ContractType.GetCustomAttributes (typeof (ServiceKnownTypeAttribute), false))
					foreach (Type t in a.GetTypes ())
						od.KnownTypes.Add (t);
				foreach (ServiceKnownTypeAttribute a in serviceMethod.GetCustomAttributes (typeof (ServiceKnownTypeAttribute), false))
					foreach (Type t in a.GetTypes ())
						od.KnownTypes.Add (t);
				foreach (FaultContractAttribute a in mi.GetCustomAttributes (typeof (FaultContractAttribute), false)) {
					var fname = a.Name ?? a.DetailType.Name + "Fault";
					var fns = a.Namespace ?? cd.Namespace;
					var fd = new FaultDescription (a.Action ?? cd.Namespace + cd.Name + "/" + od.Name + fname) { DetailType = a.DetailType, Name = fname, Namespace = fns };
#if !NET_2_1
					if (a.HasProtectionLevel)
						fd.ProtectionLevel = a.ProtectionLevel;
#endif
					od.Faults.Add (fd);
				}
				cd.Operations.Add (od);
			}
			else if (oca.AsyncPattern && od.BeginMethod != null && od.BeginMethod != mi ||
				 !oca.AsyncPattern && od.SyncMethod != null && od.SyncMethod != mi)
				throw new InvalidOperationException (String.Format ("contract '{1}' cannot have two operations for '{0}' that have the identical names and different set of parameters.", name, cd.Name));

			if (oca.AsyncPattern)
				od.BeginMethod = mi;
			else
				od.SyncMethod = mi;
			od.IsInitiating = oca.IsInitiating;
			od.IsTerminating = oca.IsTerminating;

			if (mi != serviceMethod)
				foreach (object obj in mi.GetCustomAttributes (typeof (IOperationBehavior), true))
					od.Behaviors.Add ((IOperationBehavior) obj);

			if (serviceMethod != null) {
				foreach (object obj in serviceMethod.GetCustomAttributes (typeof(IOperationBehavior),true))
					od.Behaviors.Add ((IOperationBehavior) obj);
			}
#if !NET_2_1
			if (od.Behaviors.Find<OperationBehaviorAttribute>() == null)
				od.Behaviors.Add (new OperationBehaviorAttribute ());
#endif
			// FIXME: fill KnownTypes, Behaviors and Faults.

			if (isCallback)
				od.InCallbackContract = true;
			else
				od.InOrdinalContract = true;

			return od;
		}

		static MessageDescription GetMessage (
			OperationDescription od, MethodInfo mi,
			OperationContractAttribute oca, bool isRequest,
			bool isCallback, Type asyncReturnType)
		{
			ContractDescription cd = od.DeclaringContract;
			ParameterInfo [] plist = mi.GetParameters ();
			Type messageType = null;
			string action = isRequest ? oca.Action : oca.ReplyAction;
			MessageContractAttribute mca;

			Type retType = asyncReturnType;
			if (!isRequest && retType == null)
				retType =  mi.ReturnType;

			// If the argument is only one and has [MessageContract]
			// then infer it as a typed messsage
			if (isRequest) {
				int len = mi.Name.StartsWith ("Begin", StringComparison.Ordinal) ? 3 : 1;
				mca = plist.Length != len ? null :
					GetMessageContractAttribute (plist [0].ParameterType);
				if (mca != null)
					messageType = plist [0].ParameterType;
			}
			else {
				mca = GetMessageContractAttribute (retType);
				if (mca != null)
					messageType = retType;
			}

			if (action == null)
				action = String.Concat (cd.Namespace, 
					cd.Namespace.Length == 0 ? "urn:" : cd.Namespace.EndsWith ("/") ? "" : "/", cd.Name, "/",
					od.Name, isRequest ? String.Empty : "Response");

			if (mca != null)
				return CreateMessageDescription (messageType, cd.Namespace, action, isRequest, isCallback, mca);
			return CreateMessageDescription (oca, plist, od.Name, cd.Namespace, action, isRequest, isCallback, retType, mi.ReturnTypeCustomAttributes);
		}

		public static MessageDescription CreateMessageDescription (
			Type messageType, string defaultNamespace, string action, bool isRequest, bool isCallback, MessageContractAttribute mca)
		{
			MessageDescription md = new MessageDescription (action, isRequest ^ isCallback ? MessageDirection.Input : MessageDirection.Output) { IsRequest = isRequest };
			md.MessageType = MessageFilterOutByRef (messageType);
			if (mca.HasProtectionLevel)
				md.ProtectionLevel = mca.ProtectionLevel;

			MessageBodyDescription mb = md.Body;
			if (mca.IsWrapped) {
				mb.WrapperName = mca.WrapperName ?? messageType.Name;
				mb.WrapperNamespace = mca.WrapperNamespace ?? defaultNamespace;
			}

			int index = 0;
			foreach (MemberInfo bmi in messageType.GetMembers (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
				Type mtype = null;
				string mname = null;
				if (bmi is FieldInfo) {
					FieldInfo fi = (FieldInfo) bmi;
					mtype = fi.FieldType;
					mname = fi.Name;
				}
				else if (bmi is PropertyInfo) {
					PropertyInfo pi = (PropertyInfo) bmi;
					mtype = pi.PropertyType;
					mname = pi.Name;
				}
				else
					continue;

				MessageBodyMemberAttribute mba = GetMessageBodyMemberAttribute (bmi);
				if (mba == null)
					continue;

				MessagePartDescription pd = CreatePartCore (mba, mname, defaultNamespace);
				pd.Index = index++;
				pd.Type = MessageFilterOutByRef (mtype);
				pd.MemberInfo = bmi;
				mb.Parts.Add (pd);
			}

			// FIXME: fill headers and properties.
			return md;
		}

		public static MessageDescription CreateMessageDescription (
			OperationContractAttribute oca, ParameterInfo[] plist, string name, string defaultNamespace, string action, bool isRequest, bool isCallback, Type retType, ICustomAttributeProvider retTypeAttributes)
		{
			var dir = isRequest ^ isCallback ? MessageDirection.Input : MessageDirection.Output;
			MessageDescription md = new MessageDescription (action, dir) { IsRequest = isRequest };

			MessageBodyDescription mb = md.Body;
			mb.WrapperName = name + (isRequest ? String.Empty : "Response");
			mb.WrapperNamespace = defaultNamespace;

			if (oca.HasProtectionLevel)
				md.ProtectionLevel = oca.ProtectionLevel;

			// Parts
			int index = 0;
			foreach (ParameterInfo pi in plist) {
				// AsyncCallback and state are extraneous.
				if (oca.AsyncPattern && pi.Position == plist.Length - 2)
					break;

				// They are ignored:
				// - out parameter in request
				// - neither out nor ref parameter in reply
				if (isRequest && pi.IsOut)
					continue;
				if (!isRequest && !pi.IsOut && !pi.ParameterType.IsByRef)
					continue;

				MessagePartDescription pd = CreatePartCore (GetMessageParameterAttribute (pi), pi.Name, defaultNamespace);
				pd.Index = index++;
				pd.Type = MessageFilterOutByRef (pi.ParameterType);
				mb.Parts.Add (pd);			
			}

			// ReturnValue
			if (!isRequest) {
				MessagePartDescription mp = CreatePartCore (GetMessageParameterAttribute (retTypeAttributes), name + "Result", mb.WrapperNamespace);
				mp.Index = 0;
				mp.Type = retType;
				mb.ReturnValue = mp;
			}

			// FIXME: fill properties.

			return md;
		}

		public static void FillMessageBodyDescriptionByContract (
			Type messageType, MessageBodyDescription mb)
		{
		}

		static MessagePartDescription CreatePartCore (
			MessageParameterAttribute mpa, string defaultName,
			string defaultNamespace)
		{
			string pname = null;
			if (mpa != null && mpa.Name != null)
				pname = mpa.Name;
			if (pname == null)
				pname = defaultName;
			return new MessagePartDescription (pname, defaultNamespace);
		}

		static MessagePartDescription CreatePartCore (
			MessageBodyMemberAttribute mba, string defaultName,
			string defaultNamespace)
		{
			string pname = null, pns = null;
			if (mba != null) {
				if (mba.Name != null)
					pname = mba.Name;
				if (mba.Namespace != null)
					pns = mba.Namespace;
			}
			if (pname == null)
				pname = defaultName;
			if (pns == null)
				pns = defaultNamespace;

			return new MessagePartDescription (pname, pns);
		}

		static Type MessageFilterOutByRef (Type type)
		{
			return type == null ? null :
				type.IsByRef ? type.GetElementType () : type;
		}

		static MessageParameterAttribute GetMessageParameterAttribute (ICustomAttributeProvider provider)
		{
			object [] attrs = provider.GetCustomAttributes (
				typeof (MessageParameterAttribute), true);
			return attrs.Length > 0 ? (MessageParameterAttribute) attrs [0] : null;
		}

		static MessageBodyMemberAttribute GetMessageBodyMemberAttribute (MemberInfo mi)
		{
			object [] matts = mi.GetCustomAttributes (
				typeof (MessageBodyMemberAttribute), true);
			return matts.Length > 0 ? (MessageBodyMemberAttribute) matts [0] : null;
		}
	}
}
