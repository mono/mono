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

		protected IDictionary ExternalProperties;
		protected IDictionary InternalProperties;

		public MethodCall (Header [] headers)
		{
			Init();

			if (headers == null || headers.Length == 0) return;

			foreach (Header header in headers)
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
			_typeName = string.Copy (msg.TypeName);
			_uri = string.Copy (msg.Uri);
			_methodName = string.Copy (msg.MethodName);
			
			// Get unmarshalled arguments
			ArrayList args = msg.GetArguments ();

			_args = msg.GetArgs (args);
			_methodSignature = (Type []) msg.GetMethodSignature (args);
			_callContext = msg.GetLogicalCallContext (args);
			if (_callContext == null) _callContext = new LogicalCallContext ();
	
			ResolveMethod ();
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
				IDictionary dic = msg.Properties;
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
			get { return _callContext; }
		}
		
		public MethodBase MethodBase {
			get {
				if (_methodBase == null)
					ResolveMethod ();
					
				return _methodBase;
			}
		}

		public string MethodName {
			get { return _methodName; }
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
			get { return _typeName; }
		}

		public string Uri {
			get { return _uri; }
			set { _uri = value; }
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
			if (_uri != null)
			{
				Type type = RemotingServices.GetServerTypeForUri (_uri);
				if (type == null) throw new RemotingException ("Requested service not found. No receiver for uri " + _uri);

				if (CanCastTo (_typeName, type)) {
					_methodBase = RemotingServices.GetMethodBaseFromName (type, _methodName, _methodSignature);
					return;
				}
				else
					throw new RemotingException ("Cannot cast from client type '" + _typeName + "' to server type '" + type.FullName + "'");
			}
			_methodBase = RemotingServices.GetMethodBaseFromMethodMessage (this);
		}

		bool CanCastTo (string clientType, Type serverType)
		{
			int i = clientType.IndexOf(',');
			if (i != -1) clientType = clientType.Substring (0,i).Trim();

			if (clientType == serverType.FullName) return true;

 			// base class hierarchy

 			Type baseType = serverType.BaseType;
 			while (baseType != null) {
    			if (clientType == baseType.FullName) return true;
       			baseType = baseType.BaseType;
    		}

 			// Implemented interfaces

 			Type[] interfaces = serverType.GetInterfaces();
 			foreach (Type itype in interfaces)
 				if (clientType == itype.FullName) return true;
     
     		return false;
		}
		
		[MonoTODO]
		public void RootSetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		Identity IInternalMessage.TargetIdentity
		{
			get { return _targetIdentity; }
			set { _targetIdentity = value; }
		}

		public override string ToString ()
		{
			string s = _typeName.Split(',')[0] + "." + _methodName + " (";
			Type[] ts = (Type[]) MethodSignature;
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
	}
}
