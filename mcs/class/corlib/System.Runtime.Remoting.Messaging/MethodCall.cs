//
// System.Runtime.Remoting.Messaging.MethodCall.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Reflection;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging {

	[Serializable] [CLSCompliant (false)]
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
	public class MethodCall : IMethodCallMessage, IMethodMessage, IMessage, ISerializable, IInternalMessage, ISerializationRootObject
	{
		string _uri;
		string _typeName;
		string _methodName;
		object[] _args;
		Type[] _methodSignature;
		MethodBase _methodBase;
		LogicalCallContext _callContext;
		ArgInfo _inArgInfo;
		Identity _targetIdentity;
#if NET_2_0
		Type[] _genericArguments;
#endif

		protected IDictionary ExternalProperties;
		protected IDictionary InternalProperties;

		public MethodCall (Header [] h1)
		{
			Init();

			if (h1 == null || h1.Length == 0) return;

			foreach (Header header in h1)
				InitMethodProperty (header.Name, header.Value);

			ResolveMethod ();
		}

		internal MethodCall (SerializationInfo info, StreamingContext context)
		{
			Init();

			foreach (SerializationEntry entry in info)
				InitMethodProperty ((string)entry.Name, entry.Value);
		}

		internal MethodCall (CADMethodCallMessage msg) 
		{
			_uri = string.Copy (msg.Uri);
			
			// Get unmarshalled arguments
			ArrayList args = msg.GetArguments ();

			_args = msg.GetArgs (args);
			_callContext = msg.GetLogicalCallContext (args);
			if (_callContext == null)
				_callContext = new LogicalCallContext ();
	
			_methodBase = msg.GetMethod ();
			
			Init();

			if (msg.PropertiesCount > 0)
				CADMessageBase.UnmarshalProperties (Properties, msg.PropertiesCount, args);
		}

		public MethodCall (IMessage msg)
		{
			if (msg is IMethodMessage)
				CopyFrom ((IMethodMessage) msg);
			else
			{
				foreach (DictionaryEntry entry in msg.Properties)
					InitMethodProperty ((String) entry.Key, entry.Value);
				Init();
    		}
		}

		internal MethodCall (string uri, string typeName, string methodName, object[] args)
		{
			_uri = uri;
			_typeName = typeName;
			_methodName = methodName;
			_args = args;

			Init();
			ResolveMethod();
		}

		internal MethodCall ()
		{
		}
		
		internal void CopyFrom (IMethodMessage call)
		{
			_uri = call.Uri;
			_typeName = call.TypeName;
			_methodName = call.MethodName;
			_args = call.Args;
			_methodSignature = (Type[]) call.MethodSignature;
			_methodBase = call.MethodBase;
			_callContext = call.LogicalCallContext;
			Init();
		}
		
		internal virtual void InitMethodProperty(string key, object value)
		{
			switch (key)
			{
				case "__TypeName" : _typeName = (string) value; return;
				case "__MethodName" : _methodName = (string) value; return;
				case "__MethodSignature" : _methodSignature = (Type[]) value; return;
				case "__Args" : _args = (object[]) value; return;
				case "__CallContext" : _callContext = (LogicalCallContext) value; return;
				case "__Uri" : _uri = (string) value; return;
#if NET_2_0
				case "__GenericArguments" : _genericArguments = (Type[]) value; return;
#endif
				default: Properties[key] = value; return;
			}
		}

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("__TypeName", _typeName);
			info.AddValue ("__MethodName", _methodName);
			info.AddValue ("__MethodSignature", _methodSignature);
			info.AddValue ("__Args", _args);
			info.AddValue ("__CallContext", _callContext);
			info.AddValue ("__Uri", _uri);
#if NET_2_0
			info.AddValue ("__GenericArguments", _genericArguments);
#endif

			if (InternalProperties != null) {
				foreach (DictionaryEntry entry in InternalProperties)
					info.AddValue ((string) entry.Key, entry.Value);
			}
		} 

		public int ArgCount {
			get { return _args.Length; }
		}

		public object[] Args {
			get { return _args; }
		}
		
		public bool HasVarArgs {
			get { return (MethodBase.CallingConvention | CallingConventions.VarArgs) != 0; }
		}

		public int InArgCount 
		{
			get 
			{ 
				if (_inArgInfo == null) _inArgInfo = new ArgInfo (_methodBase, ArgInfoType.In);
				return _inArgInfo.GetInOutArgCount();
			}
		}

		public object[] InArgs 
		{
			get 
			{ 
				if (_inArgInfo == null) _inArgInfo = new ArgInfo (_methodBase, ArgInfoType.In);
				return _inArgInfo.GetInOutArgs (_args);
			}
		}
		
		public LogicalCallContext LogicalCallContext {
			get {
				if (_callContext == null)
					_callContext = new LogicalCallContext ();
				return _callContext;
			}
		}
		
		public MethodBase MethodBase {
			get {
				if (_methodBase == null)
					ResolveMethod ();
					
				return _methodBase;
			}
		}

		public string MethodName {
			get {
				// lazily fill in _methodName from _methodBase
				if (_methodName == null)
					_methodName = _methodBase.Name;
				return _methodName;
			}
		}

		public object MethodSignature {
			get { 
				if (_methodSignature == null && _methodBase != null)
				{
					ParameterInfo[] parameters = _methodBase.GetParameters();
					_methodSignature = new Type[parameters.Length];
					for (int n=0; n<parameters.Length; n++)
						_methodSignature[n] = parameters[n].ParameterType;
				}
				return _methodSignature;
			}
		}

		public virtual IDictionary Properties {
			get 
			{ 
				if (ExternalProperties == null) InitDictionary ();
				return ExternalProperties; 
			}
		}

		internal virtual void InitDictionary()
		{
			MethodCallDictionary props = new MethodCallDictionary (this);
			ExternalProperties = props;
			InternalProperties = props.GetInternalProperties();
		}

		public string TypeName 
		{
			get {
				// lazily fill in _typeName from _methodBase
				if (_typeName == null)
					_typeName = _methodBase.DeclaringType.AssemblyQualifiedName;
				return _typeName;
			}
		}

		public string Uri {
			get { return _uri; }
			set { _uri = value; }
		}

		string IInternalMessage.Uri {
			get { return Uri; }
			set { Uri = value; }
		}

		public object GetArg (int argNum)
		{
			return _args[argNum];
		}

		public string GetArgName (int index)
		{
			return _methodBase.GetParameters()[index].Name;
		}

		public object GetInArg (int argNum)
		{
			if (_inArgInfo == null) _inArgInfo = new ArgInfo (_methodBase, ArgInfoType.In);
			return _args[_inArgInfo.GetInOutArgIndex (argNum)];
		}

		public string GetInArgName (int index)
		{
			if (_inArgInfo == null) _inArgInfo = new ArgInfo (_methodBase, ArgInfoType.In);
			return _inArgInfo.GetInOutArgName(index);
		}

		[MonoTODO]
		public virtual object HeaderHandler (Header[] h)
		{
			throw new NotImplementedException ();
		}

		public virtual void Init ()
		{
		}

		public void ResolveMethod ()
		{
			#if !DISABLE_REMOTING
			if (_uri != null)
			{
				Type type = RemotingServices.GetServerTypeForUri (_uri);
				if (type == null) {
					string sname = _typeName != null ? " (" + _typeName + ")" : "";
					throw new RemotingException ("Requested service not found" + sname + ". No receiver for uri " + _uri);
				}

				Type requestType = CastTo (_typeName, type);
				if (requestType == null)
					throw new RemotingException ("Cannot cast from client type '" + _typeName + "' to server type '" + type.FullName + "'");

				// Look for the method in the requested type. The method signature is provided
				// only if the method is overloaded in the requested type.
				_methodBase = RemotingServices.GetMethodBaseFromName (requestType, _methodName, _methodSignature);

				if (_methodBase == null)
					throw new RemotingException ("Method " + _methodName + " not found in " + requestType);

				// If the method is implemented in an interface, look for the method implementation.
				// It can't be done in the previous GetMethodBaseFromName call because at that point we
				// may not yet have the method signature.
				if (requestType != type && requestType.IsInterface && !type.IsInterface) {
					_methodBase = RemotingServices.GetVirtualMethod (type, _methodBase);
					if (_methodBase == null)
						throw new RemotingException ("Method " + _methodName + " not found in " + type);
				}

			} else {
				_methodBase = RemotingServices.GetMethodBaseFromMethodMessage (this);
				if (_methodBase == null) throw new RemotingException ("Method " + _methodName + " not found in " + TypeName);
			}
			#endif

#if NET_2_0
			if (_methodBase.IsGenericMethod && _methodBase.ContainsGenericParameters) {
				if (GenericArguments == null)
					throw new RemotingException ("The remoting infrastructure does not support open generic methods.");
				_methodBase = ((MethodInfo) _methodBase).MakeGenericMethod (GenericArguments);
			}
#endif
		}

		Type CastTo (string clientType, Type serverType)
		{
			clientType = GetTypeNameFromAssemblyQualifiedName (clientType);
			if (clientType == serverType.FullName) return serverType;

 			// base class hierarchy

 			Type baseType = serverType.BaseType;
 			while (baseType != null) {
				if (clientType == baseType.FullName) return baseType;
				baseType = baseType.BaseType;
			}

 			// Implemented interfaces

 			Type[] interfaces = serverType.GetInterfaces();
 			foreach (Type itype in interfaces)
 				if (clientType == itype.FullName) return itype;
     
			return null;
		}

		static string GetTypeNameFromAssemblyQualifiedName (string aqname)
		{
#if NET_2_0
			int p = aqname.IndexOf ("]]");
			int i = aqname.IndexOf(',', p == -1 ? 0 : p + 2);
#else
			int i = aqname.IndexOf(',');
#endif
			if (i != -1) aqname = aqname.Substring (0, i).Trim ();
			return aqname;
		}
		
		[MonoTODO]
		public void RootSetObjectData (SerializationInfo info, StreamingContext ctx)
		{
			throw new NotImplementedException ();
		}

		Identity IInternalMessage.TargetIdentity
		{
			get { return _targetIdentity; }
			set { _targetIdentity = value; }
		}

#if !NET_2_0
		public override string ToString ()
		{
			string s = _typeName.Split(',')[0] + "." + _methodName + " (";
			if (_args != null)
			{
				for (int n=0; n<_args.Length; n++)
				{
					if (n>0) s+= ", ";
					if (_args[n] != null) s += _args[n].GetType().Name + " ";
					s += GetArgName (n);
					if (_args[n] != null) s += " = {" + _args[n] + "}";
					else s+=" = {null}";
				}
			}
			s += ")";
			return s;
		}
#endif

#if NET_2_0
		Type[] GenericArguments {
			get {
				if (_genericArguments != null)
					return _genericArguments;

				return _genericArguments = MethodBase.GetGenericArguments ();
			}
		}
#endif
	}
}
