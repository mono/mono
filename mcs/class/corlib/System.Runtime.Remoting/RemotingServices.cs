//
// System.Runtime.Remoting.RemotingServices.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Lluis Sanchez Gual (lluis@ideary.com)
//   Patrik Torstensson
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
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
using System.IO;

namespace System.Runtime.Remoting
{
	public sealed class RemotingServices 
	{
		// Holds the identities of the objects, using uri as index
		static Hashtable uri_hash = new Hashtable ();		

		internal static string app_id;
		static int next_id = 1;
		
		static RemotingServices ()
		{
			RegisterInternalChannels ();
			app_id = "/" + Guid.NewGuid().ToString().Replace('-', '_') + "/";
			CreateWellKnownServerIdentity (typeof(RemoteActivator), "RemoteActivationService.rem", WellKnownObjectMode.Singleton);
		}
	
		private RemotingServices () {}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static object InternalExecute (MonoMethod method, Object obj,
							       Object[] parameters, out object [] out_args);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static bool IsTransparentProxy (object proxy);
		
		internal static IMethodReturnMessage InternalExecuteMessage (
		        MarshalByRefObject target, IMethodCallMessage reqMsg)
		{
			ReturnMessage result;
			
			MonoMethod method = (MonoMethod) target.GetType().GetMethod(reqMsg.MethodName, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance, null, (Type[]) reqMsg.MethodSignature, null);

			try {
				object [] out_args;
				object rval = InternalExecute (method, target, reqMsg.Args, out out_args);
				result = new ReturnMessage (rval, out_args, out_args.Length,
							    reqMsg.LogicalCallContext, reqMsg);
			
			} catch (Exception e) {
				result = new ReturnMessage (e, reqMsg);
			}

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

		public static object Connect (Type classToProxy, string url)
		{
			ObjRef objRef = new ObjRef (classToProxy, url, null);
			return GetRemoteObject (objRef, classToProxy);
		}

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
					throw new ArgumentException ("The obj parameter is a proxy");
			}
			else
				identity = obj.ObjectIdentity;

			if (identity == null || !identity.IsConnected)
				return false;
			else
			{
				LifetimeServices.StopTrackingLifetime (identity);
				DisposeIdentity (identity);
				return true;
			}
		}

		public static Type GetServerTypeForUri (string uri)
		{
			Identity ident = GetIdentityForUri (uri);
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

		public static object Unmarshal (ObjRef objref)
		{
			return Unmarshal(objref, true);
		}

		public static object Unmarshal (ObjRef objref, bool fRefine)
		{
			// FIXME: use type name when fRefine==true

			Type classToProxy = fRefine ? objref.ServerType : typeof (MarshalByRefObject);
			if (classToProxy == null) classToProxy = typeof (MarshalByRefObject);

			if (objref.IsReferenceToWellKnow)
				return GetRemoteObject(objref, classToProxy);
			else
			{
				if (classToProxy.IsContextful)
				{
					// Look for a ProxyAttribute
					ProxyAttribute att = (ProxyAttribute) Attribute.GetCustomAttribute (classToProxy, typeof(ProxyAttribute),true);
					if (att != null)
						return att.CreateProxy (objref, classToProxy, null, null).GetTransparentProxy();
				}
				return GetProxyForRemoteObject (objref, classToProxy);
			}
		}

		public static ObjRef Marshal (MarshalByRefObject obj)
		{
			return Marshal (obj, null, null);
		}
		
		public static ObjRef Marshal (MarshalByRefObject obj, string uri)
		{
			return Marshal (obj, uri, null);
		}
		
		public static ObjRef Marshal (MarshalByRefObject obj, string uri, Type requested_type)
		{
			if (IsTransparentProxy (obj))
			{
				RealProxy proxy = RemotingServices.GetRealProxy(obj);
				Identity identity = proxy.ObjectIdentity;

				if (identity != null)
				{
					if (identity.ObjectType.IsContextful && !identity.IsConnected)
					{
						// Unregistered local contextbound object. Register now.
						ClientActivatedIdentity cboundIdentity = (ClientActivatedIdentity)identity;
						if (uri == null) uri = NewUri();
						cboundIdentity.ObjectUri = uri;
						RegisterServerIdentity (cboundIdentity);
						cboundIdentity.StartTrackingLifetime ((ILease)obj.InitializeLifetimeService());
						return cboundIdentity.CreateObjRef(requested_type);
					}
					else if (uri != null)
						throw new RemotingException ("It is not possible marshal a proxy of a remote object");

					return proxy.ObjectIdentity.CreateObjRef(requested_type);
				}
			}

			if (requested_type == null) requested_type = obj.GetType();

			if (uri == null) 
			{
				uri = NewUri();
				CreateClientActivatedServerIdentity (obj, requested_type, uri);
			}
			else
			{
				ClientActivatedIdentity identity = uri_hash [uri] as ClientActivatedIdentity;
				if (identity == null || obj != identity.GetServerObject()) 
					CreateClientActivatedServerIdentity (obj, requested_type, uri);
			}

			return obj.CreateObjRef(requested_type);
		}

		static string NewUri ()
		{
			return app_id + Environment.TickCount + "_" + next_id++;
		}

		public static RealProxy GetRealProxy (object proxy)
		{
			if (!IsTransparentProxy(proxy)) throw new RemotingException("Cannot get the real proxy from an object that is not a transparent proxy");
			return (RealProxy)((TransparentProxy)proxy)._rp;
		}

		public static MethodBase GetMethodBaseFromMethodMessage(IMethodMessage msg)
		{
			Type type = Type.GetType (msg.TypeName);
			if (type == null)
				throw new RemotingException ("Type '" + msg.TypeName + "' not found!");

			BindingFlags bflags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			if (msg.MethodSignature == null)
				return type.GetMethod (msg.MethodName, bflags);
			else
				return type.GetMethod (msg.MethodName, bflags, null, (Type[]) msg.MethodSignature, null);
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
				throw new ArgumentException ("obj must be a proxy","obj");			
		}

		public static void LogRemotingStage (int stage)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string GetSessionIdForMethodMessage(IMethodMessage msg)
		{
			throw new NotImplementedException (); 
		}

		public static bool IsMethodOverloaded(IMethodMessage msg)
		{
			Type type = msg.MethodBase.DeclaringType;
			MemberInfo[] members = type.GetMember (msg.MethodName, MemberTypes.Method, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			return members.Length > 1;
		}

		public static bool IsObjectOutOfAppDomain(object tp)
		{
			Identity ident = GetObjectIdentity((MarshalByRefObject)tp);
			if (ident != null) return !ident.IsFromThisAppDomain;
			else return false;
		}

		public static bool IsObjectOutOfContext(object tp)
		{
			ServerIdentity ident = GetObjectIdentity((MarshalByRefObject)tp) as ServerIdentity;
			if (ident != null) return ident.Context != System.Threading.Thread.CurrentContext;
			else return false;
		}

		public static bool IsOneWay(MethodBase method)
		{
			// TODO: use internal call for better performance
			object[] atts = method.GetCustomAttributes (typeof (OneWayAttribute), false);
			return atts.Length > 0;
		}

		public static bool IsAsyncMessage(IMessage msg)
		{
			if (! (msg is MonoMethodMessage)) return false;
			else if (((MonoMethodMessage)msg).IsAsync) return true;
			else if (IsOneWay (((MonoMethodMessage)msg).MethodBase)) return true;
			else return false;
		}

		public static void SetObjectUriForMarshal(MarshalByRefObject obj, string uri)
		{
			if (IsTransparentProxy (obj)) throw new RemotingException ("SetObjectUriForMarshal method should only be called for MarshalByRefObjects that exist in the current AppDomain.");
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
			string activationUrl = url + "/RemoteActivationService.rem";

			string objectUri;
			IMessageSink sink = GetClientChannelSinkChain (activationUrl, null, out objectUri);

			RemotingProxy proxy = new RemotingProxy (objectType, activationUrl, activationAttributes);
			return proxy.GetTransparentProxy();
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
			RemotingProxy proxy = new RemotingProxy (type, ChannelServices.CrossContextUrl, activationAttributes);
			return proxy.GetTransparentProxy();
		}
	
		internal static Identity GetIdentityForUri (string uri)
		{
			lock (uri_hash)
			{
				return (Identity)uri_hash [uri];
			}
		}

		internal static Identity GetObjectIdentity (MarshalByRefObject obj)
		{
			if (IsTransparentProxy(obj))
				return GetRealProxy (obj).ObjectIdentity;
			else
				return obj.ObjectIdentity;
		}

		internal static ClientIdentity GetOrCreateClientIdentity(ObjRef objRef, Type proxyType)
		{
			// This method looks for an identity for the given url. 
			// If an identity is not found, it creates the identity and 
			// assigns it a proxy to the remote object.

			// Creates the client sink chain for the given url or channelData.
			// It will also get the object uri from the url.

			object channelData = objRef.ChannelInfo != null ? objRef.ChannelInfo.ChannelData : null;
			string url = (channelData == null) ? objRef.URI : null;

			string objectUri;
			IMessageSink sink = GetClientChannelSinkChain (url, channelData, out objectUri);

			if (objectUri == null) objectUri = objRef.URI;

			lock (uri_hash)
			{
				ClientIdentity identity = uri_hash [objRef.URI] as ClientIdentity;
				if (identity != null) 
					return identity;	// Object already registered

				// Creates an identity and a proxy for the remote object

				identity = new ClientIdentity (objectUri, objRef);
				identity.ChannelSink = sink;

				if (proxyType != null)
				{
					RemotingProxy proxy = new RemotingProxy (proxyType, identity);
					identity.ClientProxy = (MarshalByRefObject) proxy.GetTransparentProxy();
				}

				// Registers the identity
				uri_hash [objRef.URI] = identity;
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
					throw new RemotingException ("Uri already in use: " + identity.ObjectUri);

				uri_hash[identity.ObjectUri] = identity;
			}
		}

		internal static object GetProxyForRemoteObject (ObjRef objref, Type classToProxy)
		{
			ClientActivatedIdentity identity = uri_hash [objref.URI] as ClientActivatedIdentity;
			if (identity != null) return identity.GetServerObject ();
			else return GetRemoteObject (objref, classToProxy);
		}

		internal static object GetRemoteObject(ObjRef objRef, Type proxyType)
		{
			ClientIdentity id = GetOrCreateClientIdentity (objRef, proxyType);
			return id.ClientProxy;
		}

		internal static object GetDomainProxy(AppDomain domain) 
		{
			byte[] data = null;

			Context currentContext = Thread.CurrentContext;
			AppDomain currentDomain = AppDomain.InternalSetDomain (domain);
			try 
			{
				data = domain.GetMarshalledDomainObjRef ();
			}
			finally 
			{
				AppDomain.InternalSetDomain (currentDomain);
				AppDomain.InternalSetContext (currentContext);
			}

			MemoryStream stream = new MemoryStream (data);
			ObjRef appref = (ObjRef) CADSerializer.DeserializeObject (stream);
			return (AppDomain) RemotingServices.Unmarshal(appref);
		}

		private static void RegisterInternalChannels() 
		{
			CrossAppDomainChannel.RegisterCrossAppDomainChannel();
		}
	        
		internal static void DisposeIdentity (ServerIdentity ident)
		{
			uri_hash.Remove (ident.ObjectUri);
		}

		internal static Identity GetMessageTargetIdentity (IMessage msg)
		{
			// Returns the identity where the message is sent

			if (msg is IInternalMessage) 
				return ((IInternalMessage)msg).TargetIdentity;

			lock (uri_hash)
			{
				return uri_hash [((IMethodMessage)msg).Uri] as ServerIdentity;
			}
		}

		internal static void SetMessageTargetIdentity (IMessage msg, Identity ident)
		{
			if (msg is IInternalMessage) 
				((IInternalMessage)msg).TargetIdentity = ident;
		}

		#endregion
	}
}
