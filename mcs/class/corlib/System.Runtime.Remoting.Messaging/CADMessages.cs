//
// System.Runtime.Remoting.Messaging.CADMessages.cs
//
// Author:
//   Patrik Torstensson
//
// (C) Patrik Torstensson
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
using System.IO;
using System.Reflection;
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
		ObjRef objref;
		public int SourceDomain;

		public CADObjRef (ObjRef o, int sourceDomain) {
			objref = o;
			SourceDomain = sourceDomain;
		}
		
		public string TypeName {
			get { return objref.TypeInfo.TypeName; }
		}
		
		public string URI {
			get { return objref.URI; }
		}
	}

	internal class CADMessageBase {

		protected object [] _args;
		protected byte [] _serializedArgs = null;
		protected int _propertyCount = 0;
		protected CADArgHolder _callContext;
		
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

			Type objType = obj.GetType();
			if (objType.IsPrimitive || objType == typeof(void))
				return true;
			
			if (objType.IsArray && objType.GetElementType().IsPrimitive && ((Array)obj).Rank == 1)
				return true;
				
			if (obj is string || obj is DateTime || obj is TimeSpan)
				return true;
				
			return false;
		}

		// Checks an argument if it's possible to pass without marshalling and
		// if not it will be added to arguments to be serialized
		protected object MarshalArgument (object arg, ref ArrayList args) {
			if (null == arg)
				return null;

			if (IsPossibleToIgnoreMarshal (arg))
				return arg;

			MarshalByRefObject mbr = arg as MarshalByRefObject;
			#if !DISABLE_REMOTING
			if (null != mbr)
			{
				if (RemotingServices.IsTransparentProxy(mbr)) {
					// We don't deal with this case yet
				}
				else {
					ObjRef objRef = RemotingServices.Marshal(mbr);
					return new CADObjRef(objRef, System.Threading.Thread.GetDomainID());
				}
			}
			#endif

			if (null == args)
				args = new ArrayList();
			
			args.Add (arg);
			
			// return position that the arg exists in the serialized list
			return new CADArgHolder(args.Count - 1);
		}

		protected object UnmarshalArgument (object arg, ArrayList args) {
			if (arg == null) return null;
			
			// Check if argument is an holder (then we know that it's a serialized argument)
			CADArgHolder holder = arg as CADArgHolder;
			if (null != holder) {
				return args [holder.index];
			}

			CADObjRef objref = arg as CADObjRef;
			#if !DISABLE_REMOTING
			if (null != objref) {
				string typeName = string.Copy (objref.TypeName);
				string uri = string.Copy (objref.URI);
				int domid = objref.SourceDomain;
				
				ChannelInfo cinfo = new ChannelInfo (new CrossAppDomainData (domid));
				ObjRef localRef = new ObjRef (typeName, uri, cinfo);
				return RemotingServices.Unmarshal (localRef);
			}
			#endif
			
			if (arg is Array)
			{
				Array argb = (Array)arg;
				Array argn;
				
				// We can't use Array.CreateInstance (arg.GetType().GetElementType()) because
				// GetElementType() returns a type from the source domain.
				
				switch (Type.GetTypeCode (arg.GetType().GetElementType()))
				{
					case TypeCode.Boolean: argn = new bool [argb.Length]; break;
					case TypeCode.Byte: argn = new Byte [argb.Length]; break;
					case TypeCode.Char: argn = new Char [argb.Length]; break;
					case TypeCode.Decimal: argn = new Decimal [argb.Length]; break;
					case TypeCode.Double: argn = new Double [argb.Length]; break;
					case TypeCode.Int16: argn = new Int16 [argb.Length]; break;
					case TypeCode.Int32: argn = new Int32 [argb.Length]; break;
					case TypeCode.Int64: argn = new Int64 [argb.Length]; break;
					case TypeCode.SByte: argn = new SByte [argb.Length]; break;
					case TypeCode.Single: argn = new Single [argb.Length]; break;
					case TypeCode.UInt16: argn = new UInt16 [argb.Length]; break;
					case TypeCode.UInt32: argn = new UInt32 [argb.Length]; break;
					case TypeCode.UInt64: argn = new UInt64 [argb.Length]; break;
					default: throw new NotSupportedException ();
				}
				
				argb.CopyTo (argn, 0);
				return argn;
			}

			switch (Type.GetTypeCode (arg.GetType()))
			{
				case TypeCode.Boolean: return (bool)arg;
				case TypeCode.Byte: return (byte)arg;
				case TypeCode.Char: return (char)arg;
				case TypeCode.Decimal: return (decimal)arg;
				case TypeCode.Double: return (double)arg;
				case TypeCode.Int16: return (Int16)arg;
				case TypeCode.Int32: return (Int32)arg;
				case TypeCode.Int64: return (Int64)arg;
				case TypeCode.SByte: return (SByte)arg;
				case TypeCode.Single: return (Single)arg;
				case TypeCode.UInt16: return (UInt16)arg;
				case TypeCode.UInt32: return (UInt32)arg;
				case TypeCode.UInt64: return (UInt64)arg;
				case TypeCode.String: return string.Copy ((string) arg);
				case TypeCode.DateTime: return new DateTime (((DateTime)arg).Ticks);
				default:
					if (arg is TimeSpan) return new TimeSpan (((TimeSpan)arg).Ticks);
					if (arg is IntPtr) return (IntPtr) arg;
					break;
			}	

			throw new NotSupportedException ("Parameter of type " + arg.GetType () + " cannot be unmarshalled");
		}

		internal object [] MarshalArguments (object [] arguments, ref ArrayList args) {
			object [] marshalledArgs = new object [arguments.Length];

			int total = arguments.Length;
			for (int i = 0; i < total; i++)
				marshalledArgs [i] = MarshalArgument (arguments [i], ref args);

			return marshalledArgs;
		}

		internal object [] UnmarshalArguments (object [] arguments, ArrayList args) {
			object [] unmarshalledArgs = new object [arguments.Length];

			int total = arguments.Length;
			for (int i = 0; i < total; i++)
				unmarshalledArgs [i] = UnmarshalArgument (arguments [i], args);

			return unmarshalledArgs;
		}
		
		protected void SaveLogicalCallContext (IMethodMessage msg, ref ArrayList serializeList)
		{
			if (msg.LogicalCallContext != null && msg.LogicalCallContext.HasInfo) 
			{
				if (serializeList == null)
					serializeList = new ArrayList();

				_callContext = new CADArgHolder (serializeList.Count);
				serializeList.Add (msg.LogicalCallContext);
			}
		}
		
		internal LogicalCallContext GetLogicalCallContext (ArrayList args) 
		{
			if (null == _callContext)
				return null;

			return (LogicalCallContext) args [_callContext.index];
		}
	}

	// Used when passing a IMethodCallMessage between appdomains
	internal class CADMethodCallMessage : CADMessageBase {
		string _uri;
		
		internal RuntimeMethodHandle MethodHandle;
		internal string FullTypeName;
		
		internal string Uri {
			get {
				return _uri;
			}
		}

		static internal CADMethodCallMessage Create (IMessage callMsg) {
			IMethodCallMessage msg = callMsg as IMethodCallMessage;
			if (null == msg)
				return null;

			return new CADMethodCallMessage (msg);
		}

		internal CADMethodCallMessage (IMethodCallMessage callMsg) {
			_uri = callMsg.Uri;
			MethodHandle = callMsg.MethodBase.MethodHandle;
			FullTypeName = callMsg.MethodBase.DeclaringType.AssemblyQualifiedName;

			ArrayList serializeList = null; 
			
			_propertyCount = MarshalProperties (callMsg.Properties, ref serializeList);

			_args = MarshalArguments ( callMsg.Args, ref serializeList);

			// Save callcontext
			SaveLogicalCallContext (callMsg, ref serializeList);
			
			// Serialize message data if needed

			if (null != serializeList) {
				MemoryStream stm = CADSerializer.SerializeObject (serializeList.ToArray());
				_serializedArgs = stm.GetBuffer();
			}
		}

		internal ArrayList GetArguments () {
			ArrayList ret = null;

			if (null != _serializedArgs) {
				object[] oret = (object[]) CADSerializer.DeserializeObject (new MemoryStream (_serializedArgs));
				ret = new ArrayList (oret);
				_serializedArgs = null;
			}

			return ret;
		}

		internal object [] GetArgs (ArrayList args) {
			return UnmarshalArguments (_args, args);
		}

		internal int PropertiesCount {
			get {
				return _propertyCount;
			}
		}
		
		static Type [] GetSignature (MethodBase methodBase, bool load)
		{
			ParameterInfo[] pars = methodBase.GetParameters ();
			Type[] signature = new Type [pars.Length];
			for (int n=0; n<pars.Length; n++) {
				// The parameter types may also be loaded from a different assembly, so we need
				// to load them again
				if (load)
					signature [n] = Type.GetType (pars [n].ParameterType.AssemblyQualifiedName, true);
				else
					signature [n] = pars [n].ParameterType;
			}
			return signature;
		}

		internal MethodBase GetMethod ()
		{
			MethodBase methodBase = null;
			Type tt = Type.GetType (FullTypeName);
#if NET_2_0 && !MICRO_LIB
			if (tt.IsGenericType || tt.IsGenericTypeDefinition) {
				methodBase = MethodBase.GetMethodFromHandleNoGenericCheck (MethodHandle);
			} else
#endif
			{
				methodBase = MethodBase.GetMethodFromHandle (MethodHandle);
			}
			
			if (tt != methodBase.DeclaringType) {
				// The target domain has loaded the type from a different assembly.
				// We need to locate the correct type and get the method from it
				Type [] signature = GetSignature (methodBase, true);
#if NET_2_0
				if (methodBase.IsGenericMethod) {
					MethodBase [] methods = tt.GetMethods (BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
					Type [] base_args = methodBase.GetGenericArguments ();
					foreach (MethodBase method in methods) {
						if (!method.IsGenericMethod || method.Name != methodBase.Name)
							continue;
						Type [] method_args = method.GetGenericArguments ();
						if (base_args.Length != method_args.Length)
							continue;

						MethodInfo method_instance = ((MethodInfo) method).MakeGenericMethod (base_args);
						Type [] base_sig = GetSignature (method_instance, false);
						if (base_sig.Length != signature.Length) {
							continue;
						}
						bool dont = false;
						for (int i = base_sig.Length - 1; i >= 0; i--) {
							if (base_sig [i] != signature [i]) {
								dont = true;
								break;
							}
						}
						if (dont)
							continue;
						return method_instance;
					}
					return methodBase;
				}
#endif
				MethodBase mb = tt.GetMethod (methodBase.Name, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance, null, signature, null);
				if (mb == null)
					throw new RemotingException ("Method '" + methodBase.Name + "' not found in type '" + tt + "'");
				return mb;
			}
			return methodBase;
		}
	}
	
	// Used when passing a IMethodReturnMessage between appdomains
	internal class CADMethodReturnMessage : CADMessageBase {
		object _returnValue;
		CADArgHolder _exception = null;

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

			// Save callcontext
			SaveLogicalCallContext (retMsg, ref serializeList);

			if (null != serializeList) {
				MemoryStream stm = CADSerializer.SerializeObject (serializeList.ToArray());
				_serializedArgs = stm.GetBuffer();
			}
		}

		internal ArrayList GetArguments () {
			ArrayList ret = null;

			if (null != _serializedArgs) {
				object[] oret = (object[]) CADSerializer.DeserializeObject (new MemoryStream (_serializedArgs));
				ret = new ArrayList (oret);
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
