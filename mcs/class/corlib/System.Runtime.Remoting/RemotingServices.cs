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
			
			MonoMethod method = (MonoMethod)reqMsg.MethodBase;

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
			return GetRemoteObject(classToProxy, url, null, null);
		}

		public static object Connect (Type classToProxy, string url, object data)
		{
			return GetRemoteObject (classToProxy, url, data, null);
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
			if (ident != null) return ident.ObjectUri;
			else return null;
		}

		public static object Unmarshal (ObjRef objref)
		{
			return Unmarshal(objref, false);
		}

		public static object Unmarshal (ObjRef objref, bool fRefine)
		{
			// FIXME: use type name when fRefine==true
			Type requiredType = Type.GetType(objref.TypeInfo.TypeName);
			return GetRemoteObject(requiredType, null, objref.ChannelInfo.ChannelData, objref.URI);
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
				if (proxy != null && proxy.ObjectIdentity != null)
				{
					if (uri != null)
						throw new RemotingException ("It is not possible marshal a proxy of a remote object");

					return proxy.ObjectIdentity.CreateObjRef(requested_type);
				}
			}

			if (requested_type == null) requested_type = obj.GetType();

			if (uri == null) 
			{
				uri = app_id + Environment.TickCount + "_" + next_id++;
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

			MarshalByRefObject mbr = (MarshalByRefObject)obj;

			ObjRef oref;
			Identity ident = GetObjectIdentity(mbr);

			if (ident != null)
				oref = mbr.CreateObjRef(null);
			else
				oref = Marshal (mbr);

			oref.GetObjectData (info, context);
		}

		public static ObjRef GetObjRefForProxy(MarshalByRefObject obj)
		{
			Identity ident = GetObjectIdentity(obj);
			if (ident == null) return null;
			else return ident.CreateObjRef(null);
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
			object[] atts = method.GetCustomAttributes (typeof (OneWayAttribute), false);
			return atts.Length > 0;
		}

		public static void SetObjectUriForMarshal(MarshalByRefObject obj, string uri)
		{
			if (IsTransparentProxy (obj)) throw new RemotingException ("SetObjectUriForMarshal method should only be called for MarshalByRefObjects that exist in the current AppDomain.");
			Marshal (obj, uri);
		}

		#region Internal Methods
		
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

		private static ClientIdentity GetClientIdentity(Type requiredType, string url, object channelData, string remotedObjectUri)
		{
			// This method looks for an identity for the given url. 
			// If an identity is not found, it creates the identity and 
			// assigns it a proxy to the remote object.

			// Creates the client sink chain for the given url or channelData.
			// It will also get the object uri from the url.

			string objectUri;
			IMessageSink sink = ChannelServices.CreateClientChannelSinkChain (url, channelData, out objectUri);
			if (sink == null) 
			{
				if (url != null) {
					string msg = String.Format ("Cannot create channel sink to connect to URL {0}. An appropriate channel has probably not been registered.", url); 
					throw new RemotingException (msg);
				}
				else {
					string msg = String.Format ("Cannot create channel sink to connect to the remote object. An appropriate channel has probably not been registered.", url); 
					throw new RemotingException (msg);
				}
			}

			if (objectUri == null) objectUri = remotedObjectUri;

			lock (uri_hash)
			{
				ClientIdentity identity = uri_hash [objectUri] as ClientIdentity;
				if (identity != null) 
					return identity;	// Object already registered

				// Creates an identity and a proxy for the remote object

				identity = new ClientIdentity (objectUri, requiredType);
				identity.ClientSink = sink;

				RemotingProxy proxy = new RemotingProxy (requiredType, identity);

				identity.ClientProxy = (MarshalByRefObject) proxy.GetTransparentProxy();

				// Registers the identity
				uri_hash [objectUri] = identity;
				return identity;
			}
		}

		internal static ClientActivatedIdentity CreateClientActivatedServerIdentity(MarshalByRefObject realObject, Type objectType, string objectUri)
		{
			ClientActivatedIdentity identity = new ClientActivatedIdentity (objectUri, Context.DefaultContext, objectType);
			identity.AttachServerObject (realObject);
			RegisterServerIdentity (identity);
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

		private static void RegisterServerIdentity(Identity identity)
		{
			lock (uri_hash)
			{
				if (uri_hash.ContainsKey (identity.ObjectUri)) 
					throw new RemotingException ("Uri already in use: " + identity.ObjectUri);

				uri_hash[identity.ObjectUri] = identity;
			}
		}

		internal static object GetRemoteObject(Type requiredType, string url, object channelData, string remotedObjectUri)
		{
			ClientIdentity id = GetClientIdentity(requiredType, url, channelData, remotedObjectUri);
			return id.ClientProxy;
		}

		internal static object GetDomainProxy(AppDomain domain) 
		{
			ObjRef appRef = null;

			// Make sure that the channels is active in this domain
			RegisterInternalChannels();

			// this should use contexts in the future
			AppDomain currentDomain = AppDomain.InternalSetDomain (domain);
			try 
			{
				// Make sure that our new domain also has the internal channels
				RegisterInternalChannels();

				appRef = RemotingServices.Marshal(domain, null, null);
			}
			finally 
			{
				AppDomain.InternalSetDomain (currentDomain);
			}

			return (AppDomain) RemotingServices.Unmarshal(appRef);
		}

		private static void RegisterInternalChannels() 
		{
			CrossAppDomainChannel.RegisterCrossAppDomainChannel();
		}
	        
		internal static void DisposeIdentity (Identity ident)
		{
			uri_hash.Remove (ident.ObjectUri);
		}

		internal static ServerIdentity GetMessageTargetIdentity (IMessage msg)
		{
			// Returns the identity where the message is sent
			// TODO: check for identity embedded in MethodCall

			lock (uri_hash)
			{
				return uri_hash [((IMethodMessage)msg).Uri] as ServerIdentity;
			}
		}

		#endregion
	}
}
