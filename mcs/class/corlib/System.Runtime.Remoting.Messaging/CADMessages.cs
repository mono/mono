//
// System.Runtime.Remoting.Messaging.CADMessages.cs
//
// Author:
//   Patrik Torstensson
//
// (C) Patrik Torstensson
//

using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace System.Runtime.Remoting.Messaging {

	internal class CADArgHolder {
		public int index;

		public CADArgHolder (int i) {
			index = i;
		}
	}
	
	internal class CADObjRef {
		public ObjRef objref;

		public CADObjRef (ObjRef o) {
			this.objref = o;
		}
	}

	internal class CADMessageBase {

		// Helper to marshal properties
		internal static int MarshalProperties (IDictionary dict, ref ArrayList args) {
			IDictionary serDict = dict;
			int count = 0;

			MethodDictionary msgDict = dict as MethodDictionary;
			if (null != msgDict) {
				if (msgDict.HasInternalProperties) {
					serDict = msgDict.InternalProperties;
					if (null != serDict) {
						foreach (DictionaryEntry e in serDict) {
							if (null == args)
								args = new ArrayList();
							args.Add(e);
							count++;
						}
					}
				}
			} else {
				if (null != dict) {
					foreach (DictionaryEntry e in serDict) {
						if (null == args)
							args = new ArrayList();
						args.Add(e);
						count++;
					}
				}
			}

			return count;
		}

		internal static void UnmarshalProperties (IDictionary dict, int count, ArrayList args) {
			for (int i = 0; i < count; i++) {
				DictionaryEntry e = (DictionaryEntry) args [i];
				dict [e.Key] = e.Value;
			}
		}

		// We can ignore marshalling for string and primitive types
		private static bool IsPossibleToIgnoreMarshal (object obj) {

			// until this is more tested, we disable it....
			return false;
			// until this is more tested, we disable it....
			return false;

			if (obj is string) 
				return true;

			Type objType = obj.GetType();
			if (objType.IsPrimitive || objType == typeof(void))
				return true;
			
			return false;
		}

		// Checks an argument if it's possible to pass without marshalling and
		// if not it will be added to arguments to be serialized
		protected static object MarshalArgument (object arg, ref ArrayList args) {
			if (null == arg)
				return null;

			if (IsPossibleToIgnoreMarshal (arg))
				return arg;

			MarshalByRefObject mbr = arg as MarshalByRefObject;
			if (null != mbr) {
				if (!RemotingServices.IsTransparentProxy(mbr) || RemotingServices.GetRealProxy(mbr) is RemotingProxy) {
					ObjRef objRef = RemotingServices.Marshal(mbr);
					
					// we should check if we can move this..
					if (objRef.IsPossibleToCAD ()) {
						return new CADObjRef(new ObjRef(objRef, true));
					}
				}
			}

			if (null == args)
				args = new ArrayList();
			
			args.Add (arg);
			
			// return position that the arg exists in the serialized list
			return new CADArgHolder(args.Count - 1);
		}

		protected static object UnmarshalArgument (object arg, ArrayList args) {
			// Check if argument is an holder (then we know that it's a serialized argument)
			CADArgHolder holder = arg as CADArgHolder;
			if (null != holder) {
				return args [holder.index];
			}

			CADObjRef objref = arg as CADObjRef;
			if (null != objref) {
				return objref.objref.GetRealObject (new StreamingContext (StreamingContextStates.Other));
			}

			// otherwise the argument is ok just to return
			return arg;
		}

		internal static object [] MarshalArguments (object [] arguments, ref ArrayList args) {
			object [] marshalledArgs = new object [arguments.Length];

			int total = arguments.Length;
			for (int i = 0; i < total; i++)
				marshalledArgs [i] = MarshalArgument (arguments [i], ref args);

			return marshalledArgs;
		}

		internal static object [] UnmarshalArguments (object [] arguments, ArrayList args) {
			object [] unmarshalledArgs = new object [arguments.Length];

			int total = arguments.Length;
			for (int i = 0; i < total; i++)
				unmarshalledArgs [i] = UnmarshalArgument (arguments [i], args);

			return unmarshalledArgs;
		}
	}

	// Used when passing a IMethodCallMessage between appdomains
	internal class CADMethodCallMessage : CADMessageBase {
		string _uri;
		string _methodName;
		string _typeName;
		object [] _args;

		byte [] _serializedArgs = null;

		CADArgHolder _methodSignature;
		CADArgHolder _callContext;

		int _propertyCount = 0;

		internal string TypeName {
			get {
				return _typeName;
			}
		}

		internal string Uri {
			get {
				return _uri;
			}
		}

		internal string MethodName {
			get {
				return _methodName;
			}
		}

		static internal CADMethodCallMessage Create (IMessage callMsg) {
			IMethodCallMessage msg = callMsg as IMethodCallMessage;
			if (null == msg)
				return null;

			return new CADMethodCallMessage (msg);
		}

		// todo
		internal CADMethodCallMessage (IMethodCallMessage callMsg) {
			_methodName = callMsg.MethodName;
			_typeName = callMsg.TypeName;
			_uri = callMsg.Uri;

			ArrayList serializeList = null; 
			
			_propertyCount = MarshalProperties (callMsg.Properties, ref serializeList);

			_args = MarshalArguments ( callMsg.Args, ref serializeList);

			// check if we need to save method signature
			if (RemotingServices.IsMethodOverloaded (callMsg)) {
				if (null == serializeList)
					serializeList = new ArrayList();

				_methodSignature = new CADArgHolder (serializeList.Count);
				serializeList.Add(callMsg.MethodSignature);
			}

			// todo: save callcontext

			if (null != serializeList) {
				MemoryStream stm = CADSerializer.SerializeObject (serializeList);
				_serializedArgs = stm.GetBuffer();
			}
		}

		internal ArrayList GetArguments () {
			ArrayList ret = null;

			if (null != _serializedArgs) {
				ret = (ArrayList) CADSerializer.DeserializeObject (new MemoryStream (_serializedArgs));
				_serializedArgs = null;
			}

			return ret;
		}

		internal object [] GetArgs (ArrayList args) {
			return UnmarshalArguments (_args, args);
		}
			
		internal object [] GetMethodSignature (ArrayList args) {
			if (null == _methodSignature)
				return null;

			return (object []) args [_methodSignature.index];
		}

		internal int PropertiesCount {
			get {
				return _propertyCount;
			}
		}
	}
	
	// Used when passing a IMethodReturnMessage between appdomains
	internal class CADMethodReturnMessage : CADMessageBase {
		object [] _args;
		object _returnValue;

		byte [] _serializedArgs = null;

		CADArgHolder _exception = null;
		CADArgHolder _callContext;

		int _propertyCount = 0;

		static internal CADMethodReturnMessage Create (IMessage callMsg) {
			IMethodReturnMessage msg = callMsg as IMethodReturnMessage;
			if (null == msg)
				return null;

			return new CADMethodReturnMessage (msg);
		}

		internal CADMethodReturnMessage(IMethodReturnMessage retMsg) {
			ArrayList serializeList = null; 
			
			_propertyCount = MarshalProperties (retMsg.Properties, ref serializeList);

			_returnValue = MarshalArgument ( retMsg.ReturnValue, ref serializeList);
			_args = MarshalArguments ( retMsg.Args, ref serializeList);

			if (null != retMsg.Exception) {
				if (null == serializeList)
					serializeList = new ArrayList();
				
				_exception = new CADArgHolder (serializeList.Count);
				serializeList.Add(retMsg.Exception);
			}

			// todo: save callcontext

			if (null != serializeList) {
				MemoryStream stm = CADSerializer.SerializeObject (serializeList);
				_serializedArgs = stm.GetBuffer();
			}
		}

		internal ArrayList GetArguments () {
			ArrayList ret = null;

			if (null != _serializedArgs) {
				ret = (ArrayList) CADSerializer.DeserializeObject (new MemoryStream (_serializedArgs));
				_serializedArgs = null;
			}

			return ret;
		}

		internal object [] GetArgs (ArrayList args) {
			return UnmarshalArguments (_args, args);
		}
			
		internal object GetReturnValue (ArrayList args) {
			return UnmarshalArgument (_returnValue, args);
		}

		internal Exception GetException(ArrayList args) {
			if (null == _exception)
				return null;

			return (Exception) args [_exception.index]; 
		}

		internal int PropertiesCount {
			get {
				return _propertyCount;
			}
		}
	}
}
