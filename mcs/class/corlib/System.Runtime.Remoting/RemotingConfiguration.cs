//
// System.Runtime.Remoting.RemotingConfiguration.cs
//
// Author: Jaime Anguiano Olarra (jaime@gnome.org)
//
// (C) 2002, Jaime Anguiano Olarra
//
// FIXME: This is just the skeleton for practical purposes

using System;
using System.IO;
using System.Security.Cryptography;

namespace System.Runtime.Remoting
{	
	[MonoTODO]
	public class RemotingConfiguration
	{
		static string applicationID = null;
		static string applicationName = null;
		static string processId = null;
		static string configFile = "";
		static MiniParser parser = null; 
		
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

		public static ActivatedClientTypeEntry GetRegisteredActivatedClientTypes () {
			throw new NotImplementedException ();
		}

		public static ActivatedServiceTypeEntry GetRegisteredActivatedServiceTypes () {
			throw new NotImplementedException ();
		}

		public static WellKnownClientTypeEntry GetRegisteredWellKnownClientTypes () {
			throw new NotImplementedException ();
		}

		public static WellKnownServiceTypeEntry GetRegisteredWellKnownServiceTypes () {
			throw new NotImplementedException ();
		}

		public static bool IsActivationAllowed (Type serverType) {
			throw new NotImplementedException ();
		}

		public static ActivatedClientTypeEntry IsRemotelyActivatedClientType (Type serviceTypes) {
			throw new NotImplementedException ();
		}

		public static ActivatedClientTypeEntry IsRemotelyActivatedClientType (string typeName, string assemblyName) {
			throw new NotImplementedException ();
		}

		public static WellKnownClientTypeEntry IsWellKnownClientType (Type serviceType) {
			throw new NotImplementedException ();
		}

		public static WellKnownClientTypeEntry IsWellKnownClientType (string typeName, string assemblyName) {
			throw new NotImplementedException ();
		}

		public static void RegisterActivatedClientType (ActivatedClientTypeEntry entry) {
			throw new NotImplementedException ();
		}

		public static void RegisterActivatedClientType (Type type, string appUrl) {
			throw new NotImplementedException ();
		}

		public static void RegisterActivatedServiceType (ActivatedServiceTypeEntry entry) {
			throw new NotImplementedException ();
		}

		public static void RegisterActivatedServiceType (Type type) {
			throw new NotImplementedException ();
		}

		public static void RegisterWellKnownClientType (WellKnownClientTypeEntry entry) {
			throw new NotImplementedException ();
		}

		public static void RegisterWellKnownServiceType (Type type, string objectUrl) {
			throw new NotImplementedException ();
		}
	}	
}


