//
// System.Runtime.Remoting.RemotingServices.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Lluis Sanchez Gual (lluis@ideary.com)
//   Patrik Torstensson
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004, 2006 Novell, Inc (http://www.novell.com)
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

using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Remoting.Services;
using System.Security.Permissions;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization.Formatters;

namespace System.Runtime.Remoting
{
	[System.Runtime.InteropServices.ComVisible (true)]
#if NET_4_0
	static
#else
	sealed
#endif
	public class RemotingServices 
	{
		// Holds the identities of the objects, using uri as index
		static Hashtable uri_hash = new Hashtable ();		

		static BinaryFormatter _serializationFormatter;
		static BinaryFormatter _deserializationFormatter;
		
		static string app_id;
		static readonly object app_id_lock = new object ();
		
		static int next_id = 1;
		const BindingFlags methodBindings = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		static readonly MethodInfo FieldSetterMethod;
		static readonly MethodInfo FieldGetterMethod;
		
		// Holds information in xdomain calls. Names are short to minimize serialized size.
		[Serializable]
		class CACD {
			public object d;	/* call data */
			public object c;	/* call context */
		}
		
		static RemotingServices ()
		{
			RemotingSurrogateSelector surrogateSelector = new RemotingSurrogateSelector ();
			StreamingContext context = new StreamingContext (StreamingContextStates.Remoting, null);
			_serializationFormatter = new BinaryFormatter (surrogateSelector, context);
			_deserializationFormatter = new BinaryFormatter (null, context);
			_serializationFormatter.AssemblyFormat = FormatterAssemblyStyle.Full;
			_deserializationFormatter.AssemblyFormat = FormatterAssemblyStyle.Full;
			
			RegisterInternalChannels ();
			CreateWellKnownServerIdentity (typeof(RemoteActivator), "RemoteActivationService.rem", WellKnownObjectMode.Singleton);
			
			FieldSetterMethod = typeof(object).GetMethod ("FieldSetter", BindingFlags.NonPublic|BindingFlags.Instance);
			FieldGetterMethod = typeof(object).GetMethod ("FieldGetter", BindingFlags.NonPublic|BindingFlags.Instance);
		}
#if !NET_4_0
		private RemotingServices () {}
#endif

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static object InternalExecute (MethodBase method, Object obj,
							       Object[] parameters, out object [] out_args);

		// Returns the actual implementation of @method in @type.
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static MethodBase GetVirtualMethod (Type type, MethodBase method);

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static bool IsTransparentProxy (object proxy);
		
		internal static IMethodReturnMessage InternalExecuteMessage (
		        MarshalByRefObject target, IMethodCallMessage reqMsg)
		{
			ReturnMessage result;
			
			Type tt = target.GetType ();
			MethodBase method;
			if (reqMsg.MethodBase.DeclaringType == tt ||
			    reqMsg.MethodBase == FieldSetterMethod || 
			    reqMsg.MethodBase == FieldGetterMethod) {
				method = reqMsg.MethodBase;
			} else {
				method = GetVirtualMethod (tt, reqMsg.MethodBase);

				if (method == null)
					throw new RemotingException (
						String.Format ("Cannot resolve method {0}:{1}", tt, reqMsg.MethodName));
			}

			if (reqMsg.MethodBase.IsGenericMethod) {
				Type[] genericArguments = reqMsg.MethodBase.GetGenericArguments ();
				MethodInfo gmd = ((MethodInfo)method).GetGenericMethodDefinition ();
				method = gmd.MakeGenericMethod (genericArguments);
			}

			object oldContext = CallContext.SetCurrentCallContext (reqMsg.LogicalCallContext);
			
			try 
			{
				object [] out_args;
				object rval = InternalExecute (method, target, reqMsg.Args, out out_args);
			
				// Collect parameters with Out flag from the request message
				// FIXME: This can be done in the unmanaged side and will be
				// more efficient
				
				ParameterInfo[] parameters = method.GetParameters();
				object[] returnArgs = new object [parameters.Length];
				
				int n = 0;
				int noa = 0;
				foreach (ParameterInfo par in parameters)
				{
					if (par.IsOut && !par.ParameterType.IsByRef) 
						returnArgs [n++] = reqMsg.GetArg (par.Position);
					else if (par.ParameterType.IsByRef)
						returnArgs [n++] = out_args [noa++]; 
					else
						returnArgs [n++] = null; 
				}
				
				result = new ReturnMessage (rval, returnArgs, n, CallContext.CreateLogicalCallContext (true), reqMsg);
			} 
			catch (Exception e) 
			{
				result = new ReturnMessage (e, reqMsg);
			}
			
			CallContext.RestoreCallContext (oldContext);
			return result;
		}

		public static IMethodReturnMessage ExecuteMessage (
			MarshalByRefObject target, IMethodCallMessage reqMsg)
		{
			if (IsTransparentProxy(target))
			{
				// Message must go through all chain of sinks
				RealProxy rp = GetRealProxy (target);
				return (IMethodReturnMessage) rp.Invoke (reqMsg);
			}
			else	// Direct call
				return InternalExecuteMessage (target, reqMsg);
		}

		[System.Runtime.InteropServices.ComVisible (true)]
		public static object Connect (Type classToProxy, string url)
		{
			ObjRef objRef = new ObjRef (classToProxy, url, null);
			return GetRemoteObject (objRef, classToProxy);
		}

		[System.Runtime.InteropServices.ComVisible (true)]
		public static object Connect (Type classToProxy, string url, object data)
		{
			ObjRef objRef = new ObjRef (classToProxy, url, data);
			return GetRemoteObject (objRef, classToProxy);
		}

		public static bool Disconnect (MarshalByRefObject obj)
		{
			if (obj == null) throw new ArgumentNullException ("obj");

			ServerIdentity identity;

			if (IsTransparentProxy (obj))
			{
				// CBOs are always accessed through a proxy, even in the server, so
				// for server CBOs it is ok to disconnect a proxy

				RealProxy proxy = GetRealProxy(obj);
				if (proxy.GetProxiedType().IsContextful && (proxy.ObjectIdentity is ServerIdentity))
					identity = proxy.ObjectIdentity as ServerIdentity;
				else
					throw new ArgumentException ("The obj parameter is a proxy.");
			}
			else {
				identity = obj.ObjectIdentity;
				obj.ObjectIdentity = null;
			}

			if (identity == null || !identity.IsConnected)
				return false;
			else
			{
				LifetimeServices.StopTrackingLifetime (identity);
				DisposeIdentity (identity);
				TrackingServices.NotifyDisconnectedObject (obj);
				return true;
			}
		}

		public static Type GetServerTypeForUri (string URI)
		{
			ServerIdentity ident = GetIdentityForUri (URI) as ServerIdentity;
			if (ident == null) return null;
			return ident.ObjectType;
		}

		public static string GetObjectUri (MarshalByRefObject obj)
		{
			Identity ident = GetObjectIdentity(obj);
			if (ident is ClientIdentity) return ((ClientIdentity)ident).TargetUri;
			else if (ident != null) return ident.ObjectUri;
			else return null;
		}

		public static object Unmarshal (ObjRef objectRef)
		{
			return Unmarshal (objectRef, true);
		}

		public static object Unmarshal (ObjRef objectRef, bool fRefine)
		{
			Type classToProxy = fRefine ? objectRef.ServerType : typeof (MarshalByRefObject);
			if (classToProxy == null) classToProxy = typeof (MarshalByRefObject);

			if (objectRef.IsReferenceToWellKnow) {
				object obj = GetRemoteObject (objectRef, classToProxy);
				TrackingServices.NotifyUnmarshaledObject (obj, objectRef);
				return obj;
			}
			else
			{
				object obj;
				
				if (classToProxy.IsContextful) {
					// Look for a ProxyAttribute
					ProxyAttribute att = (ProxyAttribute) Attribute.GetCustomAttribute (classToProxy, typeof(ProxyAttribute), true);
					if (att != null) {
						obj = att.CreateProxy (objectRef, classToProxy, null, null).GetTransparentProxy();
						TrackingServices.NotifyUnmarshaledObject (obj, objectRef);
						return obj;
					}
				}
				obj = GetProxyForRemoteObject (objectRef, classToProxy);
				TrackingServices.NotifyUnmarshaledObject (obj, objectRef);
				return obj;
			}
		}

		public static ObjRef Marshal (MarshalByRefObject Obj)
		{
			return Marshal (Obj, null, null);
		}
		
		public static ObjRef Marshal (MarshalByRefObject Obj, string URI)
		{
			return Marshal (Obj, URI, null);
		}
		
		public static ObjRef Marshal (MarshalByRefObject Obj, string ObjURI, Type RequestedType)
		{
			if (IsTransparentProxy (Obj))
			{
				RealProxy proxy = RemotingServices.GetRealProxy (Obj);
				Identity identity = proxy.ObjectIdentity;

				if (identity != null)
				{
					if (proxy.GetProxiedType().IsContextful && !identity.IsConnected)
					{
						// Unregistered local contextbound object. Register now.
						ClientActivatedIdentity cboundIdentity = (ClientActivatedIdentity)identity;
						if (ObjURI == null) ObjURI = NewUri();
						cboundIdentity.ObjectUri = ObjURI;
						RegisterServerIdentity (cboundIdentity);
						cboundIdentity.StartTrackingLifetime ((ILease)Obj.InitializeLifetimeService());
						return cboundIdentity.CreateObjRef (RequestedType);
					}
					else if (ObjURI != null)
						throw new RemotingException ("It is not possible marshal a proxy of a remote object.");

					ObjRef or = proxy.ObjectIdentity.CreateObjRef (RequestedType);
					TrackingServices.NotifyMarshaledObject (Obj, or);
					return or;
				}
			}

			if (RequestedType == null) RequestedType = Obj.GetType ();

			if (ObjURI == null) 
			{
				if (Obj.ObjectIdentity == null)
				{
					ObjURI = NewUri();
					CreateClientActivatedServerIdentity (Obj, RequestedType, ObjURI);
				}
			}
			else
			{
				ClientActivatedIdentity identity = GetIdentityForUri ("/" + ObjURI) as ClientActivatedIdentity;
				if (identity == null || Obj != identity.GetServerObject()) 
					CreateClientActivatedServerIdentity (Obj, RequestedType, ObjURI);
			}

			ObjRef oref;
			
			if (IsTransparentProxy (Obj))
				oref = RemotingServices.GetRealProxy (Obj).ObjectIdentity.CreateObjRef (RequestedType);
			else
				oref = Obj.CreateObjRef (RequestedType);
			
			TrackingServices.NotifyMarshaledObject (Obj, oref);
			return oref;
		}

		static string NewUri ()
		{
			if (app_id == null) {
				lock (app_id_lock) {
					if (app_id == null)
						app_id = Guid.NewGuid().ToString().Replace('-', '_') + "/";
				}
			}

			int n = Interlocked.Increment (ref next_id);
			return app_id + Environment.TickCount.ToString("x") + "_" + n + ".rem";
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static RealProxy GetRealProxy (object proxy)
		{
			if (!IsTransparentProxy(proxy)) throw new RemotingException("Cannot get the real proxy from an object that is not a transparent proxy.");
			return (RealProxy)((TransparentProxy)proxy)._rp;
		}

		public static MethodBase GetMethodBaseFromMethodMessage(IMethodMessage msg)
		{
			Type type = Type.GetType (msg.TypeName);
			if (type == null)
				throw new RemotingException ("Type '" + msg.TypeName + "' not found.");

			return GetMethodBaseFromName (type, msg.MethodName, (Type[]) msg.MethodSignature);
		}

		internal static MethodBase GetMethodBaseFromName (Type type, string methodName, Type[] signature)
		{
			if (type.IsInterface) {
				return FindInterfaceMethod (type, methodName, signature);
			}
			else {
				MethodBase method = null;
				if (signature == null)
					method = type.GetMethod (methodName, methodBindings);
				else
					method = type.GetMethod (methodName, methodBindings, null, (Type[]) signature, null);
				
				if (method != null)
					return method;
					
				if (methodName == "FieldSetter")
					return FieldSetterMethod;

				if (methodName == "FieldGetter")
					return FieldGetterMethod;
				
				if (signature == null)
					return type.GetConstructor (methodBindings, null, Type.EmptyTypes, null);
				else
					return type.GetConstructor (methodBindings, null, signature, null);
			}
		}
		
		static MethodBase FindInterfaceMethod (Type type, string methodName, Type[] signature)
		{
			MethodBase method = null;
			
			if (signature == null)
				method = type.GetMethod (methodName, methodBindings);
			else
				method = type.GetMethod (methodName, methodBindings, null, signature, null);
				
			if (method != null) return method;
			
			foreach (Type t in type.GetInterfaces ()) {
				method = FindInterfaceMethod (t, methodName, signature);
				if (method != null) return method;
			}
			
			return null;
		}

		public static void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			if (obj == null) throw new ArgumentNullException ("obj");

			ObjRef oref = Marshal ((MarshalByRefObject)obj);
			oref.GetObjectData (info, context);
		}

		public static ObjRef GetObjRefForProxy(MarshalByRefObject obj)
		{
			Identity ident = GetObjectIdentity(obj);
			if (ident == null) return null;
			else return ident.CreateObjRef(null);
		}

		public static object GetLifetimeService (MarshalByRefObject obj)
		{
			if (obj == null) return null;
			return obj.GetLifetimeService ();
		}

		public static IMessageSink GetEnvoyChainForProxy (MarshalByRefObject obj)
		{
			if (IsTransparentProxy(obj))
				return ((ClientIdentity)GetRealProxy (obj).ObjectIdentity).EnvoySink;
			else
				throw new ArgumentException ("obj must be a proxy.","obj");			
		}

		[MonoTODO]
		[Conditional ("REMOTING_PERF")]
		[Obsolete ("It existed for only internal use in .NET and unimplemented in mono")]
		public static void LogRemotingStage (int stage)
		{
			throw new NotImplementedException ();
		}

		public static string GetSessionIdForMethodMessage(IMethodMessage msg)
		{
			// It seems that this it what MS returns.
			return msg.Uri;
		}

		public static bool IsMethodOverloaded(IMethodMessage msg)
		{
			const BindingFlags bfinst = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
			MonoType type = (MonoType) msg.MethodBase.DeclaringType;
			return type.GetMethodsByName (msg.MethodName, bfinst, false, type).Length > 1;
		}

		public static bool IsObjectOutOfAppDomain(object tp)
		{
			MarshalByRefObject mbr = tp as MarshalByRefObject;

			if (mbr == null)
				return false;

			// TODO: use internal call for better performance
			Identity ident = GetObjectIdentity (mbr);
			return ident is ClientIdentity;
		}

		public static bool IsObjectOutOfContext(object tp)
		{
			MarshalByRefObject mbr = tp as MarshalByRefObject;

			if (mbr == null)
				return false;

			// TODO: use internal call for better performance
			Identity ident = GetObjectIdentity (mbr);
			if (ident == null) return false;
			
			ServerIdentity sident = ident as ServerIdentity;
			if (sident != null) return sident.Context != System.Threading.Thread.CurrentContext;
			else return true;
		}

		public static bool IsOneWay(MethodBase method)
		{
			return method.IsDefined (typeof (OneWayAttribute), false);
		}

		internal static bool IsAsyncMessage(IMessage msg)
		{
			if (! (msg is MonoMethodMessage)) return false;
			else if (((MonoMethodMessage)msg).IsAsync) return true;
			else if (IsOneWay (((MonoMethodMessage)msg).MethodBase)) return true;
			else return false;
		}

		public static void SetObjectUriForMarshal(MarshalByRefObject obj, string uri)
		{
			if (IsTransparentProxy (obj)) {
				RealProxy proxy = RemotingServices.GetRealProxy(obj);
				Identity identity = proxy.ObjectIdentity;

				if (identity != null && !(identity is ServerIdentity) && !proxy.GetProxiedType().IsContextful)
					throw new RemotingException ("SetObjectUriForMarshal method should only be called for MarshalByRefObjects that exist in the current AppDomain.");
			}
			
			Marshal (obj, uri);
		}

		#region Internal Methods
		
		internal static object CreateClientProxy (ActivatedClientTypeEntry entry, object[] activationAttributes)
		{
			if (entry.ContextAttributes != null || activationAttributes != null)
			{
				ArrayList props = new ArrayList ();
				if (entry.ContextAttributes != null) props.AddRange (entry.ContextAttributes);
				if (activationAttributes != null) props.AddRange (activationAttributes);
				return CreateClientProxy (entry.ObjectType, entry.ApplicationUrl, props.ToArray ());
			}
			else
				return CreateClientProxy (entry.ObjectType, entry.ApplicationUrl, null);
		}
	
		internal static object CreateClientProxy (Type objectType, string url, object[] activationAttributes)
		{
#if MOONLIGHT
			throw new NotSupportedException ();
#else
			string activationUrl = url;
			if (!activationUrl.EndsWith ("/"))
				activationUrl += "/";
			activationUrl += "RemoteActivationService.rem";

			string objectUri;
			GetClientChannelSinkChain (activationUrl, null, out objectUri);

			RemotingProxy proxy = new RemotingProxy (objectType, activationUrl, activationAttributes);
			return proxy.GetTransparentProxy();
#endif
		}
	
		internal static object CreateClientProxy (WellKnownClientTypeEntry entry)
		{
			return Connect (entry.ObjectType, entry.ObjectUrl, null);
		}
	
		internal static object CreateClientProxyForContextBound (Type type, object[] activationAttributes)
		{
			if (type.IsContextful)
			{
				// Look for a ProxyAttribute
				ProxyAttribute att = (ProxyAttribute) Attribute.GetCustomAttribute (type, typeof(ProxyAttribute), true);
				if (att != null)
					return att.CreateInstance (type);
			}
#if MOONLIGHT
			throw new NotSupportedException ();
#else
			RemotingProxy proxy = new RemotingProxy (type, ChannelServices.CrossContextUrl, activationAttributes);
			return proxy.GetTransparentProxy();
#endif
		}
#if !NET_2_1
		internal static object CreateClientProxyForComInterop (Type type)
		{
			Mono.Interop.ComInteropProxy proxy = Mono.Interop.ComInteropProxy.CreateProxy (type);
			return proxy.GetTransparentProxy ();
		}
#endif
		internal static Identity GetIdentityForUri (string uri)
		{
			string normUri = GetNormalizedUri (uri);
			lock (uri_hash)
			{
				Identity i = (Identity) uri_hash [normUri];

				if (i == null) {
					normUri = RemoveAppNameFromUri (uri);
					if (normUri != null)
						i = (Identity) uri_hash [normUri];
				}

				return i;
			}
		}

		//
		// If the specified uri starts with the application name,
		// RemoveAppNameFromUri returns the uri w/out the leading
		// application name, otherwise it returns null.
		//
		// Assumes that the uri is not normalized.
		//
		static string RemoveAppNameFromUri (string uri)
		{
			string name = RemotingConfiguration.ApplicationName;
			if (name == null) return null;
			name = "/" + name + "/";
			if (uri.StartsWith (name))
				return uri.Substring (name.Length);
			else
				return null;
		}

		internal static Identity GetObjectIdentity (MarshalByRefObject obj)
		{
			if (IsTransparentProxy(obj))
				return GetRealProxy (obj).ObjectIdentity;
			else
				return obj.ObjectIdentity;
		}

		internal static ClientIdentity GetOrCreateClientIdentity(ObjRef objRef, Type proxyType, out object clientProxy)
		{
			// This method looks for an identity for the given url. 
			// If an identity is not found, it creates the identity and 
			// assigns it a proxy to the remote object.

			// Creates the client sink chain for the given url or channelData.
			// It will also get the object uri from the url.

			object channelData = objRef.ChannelInfo != null ? objRef.ChannelInfo.ChannelData : null;

			string objectUri;
			IMessageSink sink = GetClientChannelSinkChain (objRef.URI, channelData, out objectUri);

			if (objectUri == null) objectUri = objRef.URI;

			lock (uri_hash)
			{
				clientProxy = null;
				string uri = GetNormalizedUri (objRef.URI);
				
				ClientIdentity identity = uri_hash [uri] as ClientIdentity;
				if (identity != null)
				{
					// Object already registered
					clientProxy = identity.ClientProxy;
					if (clientProxy != null) return identity;
					
					// The proxy has just been GCed, so its identity cannot
					// be reused. Just dispose it.
					DisposeIdentity (identity);
				}

				// Creates an identity and a proxy for the remote object

				identity = new ClientIdentity (objectUri, objRef);
				identity.ChannelSink = sink;

				// Registers the identity
				uri_hash [uri] = identity;
#if !MOONLIGHT
				if (proxyType != null)
				{
					RemotingProxy proxy = new RemotingProxy (proxyType, identity);
					CrossAppDomainSink cds = sink as CrossAppDomainSink;
					if (cds != null)
						proxy.SetTargetDomain (cds.TargetDomainId);

					clientProxy = proxy.GetTransparentProxy();
					identity.ClientProxy = (MarshalByRefObject) clientProxy;
				}
#endif
				return identity;
			}
		}

		static IMessageSink GetClientChannelSinkChain(string url, object channelData, out string objectUri)
		{
			IMessageSink sink = ChannelServices.CreateClientChannelSinkChain (url, channelData, out objectUri);
			if (sink == null) 
			{
				if (url != null) 
				{
					string msg = String.Format ("Cannot create channel sink to connect to URL {0}. An appropriate channel has probably not been registered.", url); 
					throw new RemotingException (msg);
				}
				else 
				{
					string msg = String.Format ("Cannot create channel sink to connect to the remote object. An appropriate channel has probably not been registered.", url); 
					throw new RemotingException (msg);
				}
			}
			return sink;
		}

		internal static ClientActivatedIdentity CreateContextBoundObjectIdentity(Type objectType)
		{
			ClientActivatedIdentity identity = new ClientActivatedIdentity (null, objectType);
			identity.ChannelSink = ChannelServices.CrossContextChannel;
			return identity;
		}

		internal static ClientActivatedIdentity CreateClientActivatedServerIdentity(MarshalByRefObject realObject, Type objectType, string objectUri)
		{
			ClientActivatedIdentity identity = new ClientActivatedIdentity (objectUri, objectType);
			identity.AttachServerObject (realObject, Context.DefaultContext);
			RegisterServerIdentity (identity);
			identity.StartTrackingLifetime ((ILease)realObject.InitializeLifetimeService ());
			return identity;
		}

		internal static ServerIdentity CreateWellKnownServerIdentity(Type objectType, string objectUri, WellKnownObjectMode mode)
		{
			ServerIdentity identity;

			if (mode == WellKnownObjectMode.SingleCall)
				identity = new  SingleCallIdentity(objectUri, Context.DefaultContext, objectType);
			else
				identity = new  SingletonIdentity(objectUri, Context.DefaultContext, objectType);

			RegisterServerIdentity (identity);
			return identity;
		}

		private static void RegisterServerIdentity(ServerIdentity identity)
		{
			lock (uri_hash)
			{
				if (uri_hash.ContainsKey (identity.ObjectUri)) 
					throw new RemotingException ("Uri already in use: " + identity.ObjectUri + ".");

				uri_hash[identity.ObjectUri] = identity;
			}
		}

		internal static object GetProxyForRemoteObject (ObjRef objref, Type classToProxy)
		{
			ClientActivatedIdentity identity = GetIdentityForUri (objref.URI) as ClientActivatedIdentity;
			if (identity != null) return identity.GetServerObject ();
			else return GetRemoteObject (objref, classToProxy);
		}

		internal static object GetRemoteObject(ObjRef objRef, Type proxyType)
		{
			object proxy;
			GetOrCreateClientIdentity (objRef, proxyType, out proxy);
			return proxy;
		}
		
		// This method is called by the runtime
		internal static object GetServerObject (string uri)
		{
			ClientActivatedIdentity identity = GetIdentityForUri (uri) as ClientActivatedIdentity;
			if (identity == null) throw new RemotingException ("Server for uri '" + uri + "' not found");
			return identity.GetServerObject ();
		}

		// This method is called by the runtime
		[SecurityPermission (SecurityAction.Assert, SerializationFormatter = true)] // FIXME: to be reviewed
		internal static byte[] SerializeCallData (object obj)
		{
			LogicalCallContext ctx = CallContext.CreateLogicalCallContext (false);
			if (ctx != null) {
				CACD cad = new CACD ();
				cad.d = obj;
				cad.c = ctx;
				obj = cad;
			}
			
			if (obj == null) return null;
			MemoryStream ms = new MemoryStream ();
			_serializationFormatter.Serialize (ms, obj);
			return ms.ToArray ();
		}

		// This method is called by the runtime
		[SecurityPermission (SecurityAction.Assert, SerializationFormatter = true)] // FIXME: to be reviewed
		internal static object DeserializeCallData (byte[] array)
		{
			if (array == null) return null;
			
			MemoryStream ms = new MemoryStream (array);
			object obj = _deserializationFormatter.Deserialize (ms);
			
			if (obj is CACD) {
				CACD cad = (CACD) obj;
				obj = cad.d;
				CallContext.UpdateCurrentCallContext ((LogicalCallContext) cad.c);
			}
			return obj;
		}
		
		// This method is called by the runtime
		[SecurityPermission (SecurityAction.Assert, SerializationFormatter = true)] // FIXME: to be reviewed
		internal static byte[] SerializeExceptionData (Exception ex)
		{
			try {
				int retry = 4;
				
				do {
					try {
						MemoryStream ms = new MemoryStream ();
						_serializationFormatter.Serialize (ms, ex);
						return ms.ToArray ();
					}
					catch (Exception e) {
						if (e is ThreadAbortException) {
							Thread.ResetAbort ();
							retry = 5;
							ex = e;
						}
						else if (retry == 2) {
							ex = new Exception ();
							ex.SetMessage (e.Message);
							ex.SetStackTrace (e.StackTrace);
						}
						else
							ex = e;
					}
					retry--;
				}
				while (retry > 0);
				
				return null;
			}
			catch (Exception tex)
			{
				byte[] data = SerializeExceptionData (tex);
				Thread.ResetAbort ();
				return data;
			}
		}
		
		internal static object GetDomainProxy(AppDomain domain) 
		{
			byte[] data = null;

			Context currentContext = Thread.CurrentContext;

			try
			{
				data = (byte[])AppDomain.InvokeInDomain (domain, typeof (AppDomain).GetMethod ("GetMarshalledDomainObjRef", BindingFlags.Instance|BindingFlags.NonPublic), domain, null);
			}
			finally
			{
				AppDomain.InternalSetContext (currentContext);
			}				

			byte[] data_copy = new byte [data.Length];
			data.CopyTo (data_copy, 0);
			MemoryStream stream = new MemoryStream (data_copy);
			ObjRef appref = (ObjRef) CADSerializer.DeserializeObject (stream);
			return (AppDomain) RemotingServices.Unmarshal(appref);
		}

		private static void RegisterInternalChannels() 
		{
			CrossAppDomainChannel.RegisterCrossAppDomainChannel();
		}
	        
		internal static void DisposeIdentity (Identity ident)
		{
			lock (uri_hash)
			{
				if (!ident.Disposed) {
					ClientIdentity clientId = ident as ClientIdentity;
					if (clientId != null)
						uri_hash.Remove (GetNormalizedUri (clientId.TargetUri));
					else
						uri_hash.Remove (ident.ObjectUri);
						
					ident.Disposed = true;
				}
			}
		}

		internal static Identity GetMessageTargetIdentity (IMessage msg)
		{
			// Returns the identity where the message is sent

			if (msg is IInternalMessage) 
				return ((IInternalMessage)msg).TargetIdentity;

			lock (uri_hash)
			{
				string uri = GetNormalizedUri (((IMethodMessage)msg).Uri);
				return uri_hash [uri] as ServerIdentity;
			}
		}

		internal static void SetMessageTargetIdentity (IMessage msg, Identity ident)
		{
			if (msg is IInternalMessage) 
				((IInternalMessage)msg).TargetIdentity = ident;
		}
		
		internal static bool UpdateOutArgObject (ParameterInfo pi, object local, object remote)
		{
			if (pi.ParameterType.IsArray && ((Array)local).Rank == 1)
			{
				Array alocal = (Array) local;
				if (alocal.Rank == 1)
				{
					Array.Copy ((Array) remote, alocal, alocal.Length);
					return true;
				}
				else
				{
					// TODO
				}
			}
			return false;
		}
		
		static string GetNormalizedUri (string uri)
		{
			if (uri.StartsWith ("/")) return uri.Substring (1);
			else return uri;
		}

		#endregion
	}
}
