//
// System.Runtime.Remoting.Messaging.MethodCall.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging {

	[Serializable] [CLSCompliant (false)]
	public class MethodCall : IMethodCallMessage, IMethodMessage, IMessage, ISerializable
	{
		string _uri;
		string _typeName;
		string _methodName;
		object[] _args;
		Type[] _methodSignature;
		MethodBase _methodBase;
		LogicalCallContext _callContext;
		ArgInfo _inArgInfo;
		InternalDictionary _properties;

		public MethodCall (Header [] headers)
		{
			if (headers == null || headers.Length == 0) return;

			foreach (Header header in headers)
			{
				switch (header.Name)
				{
					case "__TypeName" : _typeName = (string)header.Value; break;
					case "__MethodName" : _methodName = (string)header.Value; break;
					case "__MethodSignature" : _methodSignature = (Type[])header.Value; break;
					case "__Args" : _args = (object[])header.Value; break;
					case "__CallContext" : _callContext = (LogicalCallContext)header.Value; break;
				}
			}

			ResolveMethod ();
			Init();
		}

		internal MethodCall (CADMethodCallMessage msg) {
			_typeName = msg.TypeName;
			_uri = msg.Uri;
			_methodName = msg.MethodName;
			
			// Get unmarshalled arguments
			ArrayList args = msg.GetArguments ();

			_args = msg.GetArgs (args);
			_methodSignature = (Type []) msg.GetMethodSignature (args);
	
			ResolveMethod ();
			Init();

			if (msg.PropertiesCount > 0)
				CADMessageBase.UnmarshalProperties (Properties, msg.PropertiesCount, args);
		}

		[MonoTODO]
		public MethodCall (IMessage msg)
		{
			Init();
			throw new NotImplementedException ();
		}

		protected IDictionary ExternalProperties;
		protected IDictionary InternalProperties;

		public int ArgCount {
			get { return _args.Length; }
		}

		public object[] Args {
			get { return _args; }
		}
		
		[MonoTODO]
		public bool HasVarArgs {
			get { throw new NotImplementedException (); }
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
			get { return _methodBase; }
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
			get { return _properties; }
		}

		public string TypeName {
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
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		} 

		[MonoTODO]
		public virtual object HeaderHandler (Header[] h)
		{
			throw new NotImplementedException ();
		}

		public virtual void Init ()
		{
			_properties = new InternalDictionary (this);
			ExternalProperties = _properties;
			InternalProperties = _properties.GetInternalProperties();
		}

		public void ResolveMethod ()
		{
			_methodBase = RemotingServices.GetMethodBaseFromMethodMessage (this);
		}

		[MonoTODO]
		public void RootSetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		class InternalDictionary : MethodCallDictionary
		{
			public InternalDictionary(MethodCall message) : base (message) { }

			protected override void SetMethodProperty (string key, object value)
			{
				if (key == "__Uri") ((MethodCall)_message).Uri = (string)value;
				else base.SetMethodProperty (key, value);
			}
		}
	}
}
