//
// System.Runtime.Remoting.RemotingConfiguration.cs
//
// Author: Jaime Anguiano Olarra (jaime@gnome.org)
//         Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2002, Jaime Anguiano Olarra
//
// FIXME: This is just the skeleton for practical purposes

using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Security.Cryptography;

namespace System.Runtime.Remoting
{	
	public class RemotingConfiguration
	{
		static string applicationID = null;
		static string applicationName = null;
		static string processId = null;
		static string configFile = "";
		static MiniParser parser = null; 

		static Hashtable wellKnownClientEntries = new Hashtable();
		static Hashtable activatedClientEntries = new Hashtable();
		static Hashtable wellKnownServiceEntries = new Hashtable();
		static Hashtable activatedServiceEntries = new Hashtable();
		
		// public properties
		// At this time the ID will be the application name 
		public static string ApplicationId 
		{
			get 
			{ 
				applicationID = AppDomain.CurrentDomain.SetupInformation.ApplicationName; 
				return applicationID;
			}
		}
		
		public static string ApplicationName 
		{
			get { 
				try {
					applicationName = AppDomain.CurrentDomain.SetupInformation.ApplicationName; 
				}
				catch (Exception e) {
					throw e;
				}
				// We return null if the application name has not been set.
				return null;
			}
			set { applicationName = value; }
		}
		
		public static string ProcessId 
		{
			get { 
				processId = AppDomain.CurrentDomain.SetupInformation.ApplicationName;
				return processId; 
			}
		}

		// public methods
		
		public static void Configure (string filename) 
		{
			throw new NotImplementedException ();
		}

		public static ActivatedClientTypeEntry[] GetRegisteredActivatedClientTypes () 
		{
			ActivatedClientTypeEntry[] entries = new ActivatedClientTypeEntry[activatedClientEntries.Count];
			activatedClientEntries.Values.CopyTo (entries,0);
			return entries;
		}

		public static ActivatedServiceTypeEntry[] GetRegisteredActivatedServiceTypes () 
		{
			ActivatedServiceTypeEntry[] entries = new ActivatedServiceTypeEntry[activatedServiceEntries.Count];
			activatedServiceEntries.Values.CopyTo (entries,0);
			return entries;
		}

		public static WellKnownClientTypeEntry[] GetRegisteredWellKnownClientTypes () 
		{
			WellKnownClientTypeEntry[] entries = new WellKnownClientTypeEntry[wellKnownClientEntries.Count];
			wellKnownClientEntries.Values.CopyTo (entries,0);
			return entries;
		}

		public static WellKnownServiceTypeEntry[] GetRegisteredWellKnownServiceTypes () 
		{
			WellKnownServiceTypeEntry[] entries = new WellKnownServiceTypeEntry[wellKnownServiceEntries.Count];
			wellKnownServiceEntries.Values.CopyTo (entries,0);
			return entries;
		}

		public static bool IsActivationAllowed (Type serverType) 
		{
			return activatedServiceEntries.ContainsKey (serverType);
		}

		public static ActivatedClientTypeEntry IsRemotelyActivatedClientType (Type serviceType) 
		{
			return activatedClientEntries [serviceType] as ActivatedClientTypeEntry;
		}

		public static ActivatedClientTypeEntry IsRemotelyActivatedClientType (string typeName, string assemblyName) 
		{
			return IsRemotelyActivatedClientType (Assembly.Load(assemblyName).GetType (typeName));
		}

		public static WellKnownClientTypeEntry IsWellKnownClientType (Type serviceType) 
		{
			return wellKnownClientEntries [serviceType] as WellKnownClientTypeEntry;
		}

		public static WellKnownClientTypeEntry IsWellKnownClientType (string typeName, string assemblyName) 
		{
			return IsWellKnownClientType (Assembly.Load(assemblyName).GetType (typeName));
		}

		public static void RegisterActivatedClientType (ActivatedClientTypeEntry entry) 
		{
			if (wellKnownClientEntries.ContainsKey (entry.ObjectType) || activatedClientEntries.ContainsKey (entry.ObjectType))
				throw new RemotingException ("Attempt to redirect activation of type '" + entry.ObjectType.FullName + "' which is already redirected.");

			activatedClientEntries[entry.ObjectType] = entry;
		}

		public static void RegisterActivatedClientType (Type type, string appUrl) 
		{
			if (type == null) throw new ArgumentNullException ("type");
			if (appUrl == null) throw new ArgumentNullException ("appUrl");

			RegisterActivatedClientType (new ActivatedClientTypeEntry (type, appUrl));
		}

		public static void RegisterActivatedServiceType (ActivatedServiceTypeEntry entry) 
		{
			activatedServiceEntries.Add (entry.ObjectType, entry);
		}

		public static void RegisterActivatedServiceType (Type type) 
		{
			RegisterActivatedServiceType (new ActivatedServiceTypeEntry (type));
		}

		public static void RegisterWellKnownClientType (Type type, string objectUrl) 
		{
			if (type == null) throw new ArgumentNullException ("type");
			if (objectUrl == null) throw new ArgumentNullException ("objectUrl");

			RegisterWellKnownClientType (new WellKnownClientTypeEntry (type, objectUrl));
		}

		public static void RegisterWellKnownClientType (WellKnownClientTypeEntry entry) 
		{
			if (wellKnownClientEntries.ContainsKey (entry.ObjectType) || activatedClientEntries.ContainsKey (entry.ObjectType))
				throw new RemotingException ("Attempt to redirect activation of type '" + entry.ObjectType.FullName + "' which is already redirected.");

			wellKnownClientEntries[entry.ObjectType] = entry;
		}

		public static void RegisterWellKnownServiceType (Type type, string objectUrl, WellKnownObjectMode mode) 
		{
			RegisterWellKnownServiceType (new WellKnownServiceTypeEntry (type, objectUrl, mode));
		}

		public static void RegisterWellKnownServiceType (WellKnownServiceTypeEntry entry) 
		{
			wellKnownServiceEntries [entry.ObjectUri] = entry;
			ServiceType st = (entry.Mode == WellKnownObjectMode.SingleCall) ? ServiceType.SingleCall : ServiceType.Singleton;
			RemotingServices.CreateServerIdentity (null, entry.ObjectType, entry.ObjectUri, st);
		}
	}	
}


