//
// System.Runtime.Remoting.RemotingServices.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Lluis Sanchez Gual (lluis@ideary.com)
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
			object svr = GetServerForUri (uri);

			if (svr == null)
				return null;
			
			return svr.GetType ();
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

			if (uri == null) uri = app_id + Environment.TickCount + "_" + next_id++;

			// It creates the identity if not found
			Identity identity = GetServerIdentity (obj, uri);

			if (obj != identity.RealObject)
				throw new RemotingException ("uri already in use, " + uri);
			// already registered

			return identity.CreateObjRef(requested_type);
		}

		public static RealProxy GetRealProxy (object proxy)
		{
			if (!IsTransparentProxy(proxy)) throw new RemotingException("Cannot get the real proxy from an object that is not a transparent proxy");
			return (RealProxy)((TransparentProxy)proxy)._rp;
		}

		public static MethodBase GetMethodBaseFromMethodMessage(IMethodMessage msg)
		{
			Type type = Type.GetType(msg.TypeName);

			if (msg.MethodSignature == null)
				return type.GetMethod (msg.MethodName);
			else
				return type.GetMethod (msg.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, (Type[]) msg.MethodSignature, null);
		}

		public static void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			if (obj == null) throw new ArgumentNullException ("obj");

			MarshalByRefObject mbr = (MarshalByRefObject)obj;

			ObjRef oref;
			Identity ident = GetObjectIdentity(mbr);

			if (ident != null)
				oref = ident.CreateObjRef(null);
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
			Identity ident = GetObjectIdentity((MarshalByRefObject)tp);
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
		
		internal static MarshalByRefObject GetServerForUri (string uri)
		{
			lock (uri_hash)
			{
				return (MarshalByRefObject)((Identity)uri_hash [uri]).RealObject;
			}
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

		private static Identity GetClientIdentity(Type requiredType, string url, object channelData, string remotedObjectUri)
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
				Identity identity = (Identity)uri_hash [objectUri];
				if (identity != null) 
					return identity;	// Object already registered

				// Creates an identity and a proxy for the remote object

				identity = new Identity(objectUri, null, requiredType);
				identity.ClientSink = sink;

				RemotingProxy proxy = new RemotingProxy (requiredType, identity);

				identity.RealObject = proxy.GetTransparentProxy();

				// Registers the identity
				uri_hash [objectUri] = identity;
				return identity;
			}
		}

		private static Identity GetServerIdentity(MarshalByRefObject realObject, string objectUri)
		{
			// This method looks for an identity for the given object. 
			// If an identity is not found, it creates the identity and 
			// assigns it to the given object

			lock (uri_hash)
			{
				Identity identity = (Identity)uri_hash [objectUri];
				if (identity != null) 
					return identity;	// Object already registered

				identity = new Identity (objectUri, Context.DefaultContext, realObject.GetType());
				identity.RealObject = realObject;

				// Registers the identity
				uri_hash[objectUri] = identity;
				realObject.ObjectIdentity = identity;

				return identity;
			}
		}

		internal static object GetRemoteObject(Type requiredType, string url, object channelData, string remotedObjectUri)
		{
			Identity id = GetClientIdentity(requiredType, url, channelData, remotedObjectUri);
			return id.RealObject;
		}

		#endregion
		
	}


}
