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
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Channels;
using Mono.Xml;

namespace System.Runtime.Remoting
{	
	public class RemotingConfiguration
	{
		static string applicationID = null;
		static string applicationName = null;
		static string configFile = "";
		static MiniParser parser = null; 
		static string processGuid = null;

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
				if (processGuid == null)
					processGuid = AppDomain.GetProcessGuid ();

				return processGuid;
			}
		}


		// public methods
		
		public static void Configure (string filename) 
		{
			RConfigurator cftor;
			cftor = new RConfigurator (filename,
						   wellKnownClientEntries,
						   activatedClientEntries,
						   wellKnownServiceEntries,
						   activatedServiceEntries);
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
			ActivationServices.EnableProxyActivation (entry.ObjectType, true);
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
			ActivationServices.EnableProxyActivation (entry.ObjectType, true);
		}

		public static void RegisterWellKnownServiceType (Type type, string objectUrl, WellKnownObjectMode mode) 
		{
			RegisterWellKnownServiceType (new WellKnownServiceTypeEntry (type, objectUrl, mode));
		}

		public static void RegisterWellKnownServiceType (WellKnownServiceTypeEntry entry) 
		{
			wellKnownServiceEntries [entry.ObjectUri] = entry;
			RemotingServices.CreateWellKnownServerIdentity (entry.ObjectType, entry.ObjectUri, entry.Mode);
		}
	}

	/***************************************************************
	 * Internal classes used by RemotingConfiguration.Configure () *
	 ***************************************************************/
	internal class RConfigurator {
		// As the Configure method is just an alternative to the hardcoded configuration,
		// we will be using the same hashtables.
		internal RConfigurator (string filename,
				 Hashtable wellKnownClientEntries,
				 Hashtable activatedClientEntries,
				 Hashtable wellKnownServiceEntries,
				 Hashtable activatedServiceEntries)
		{
			MiniParser parser1 = new MiniParser ();
			MiniParser parser2 = new MiniParser ();

			RReader rreader1 = new RReader (GetMachineConfigPath());
			RReader rreader2 = new RReader (filename);
			MHandler mhandler = new MHandler (wellKnownClientEntries,
							  activatedClientEntries,
							  wellKnownServiceEntries,
							  activatedServiceEntries);
			RHandler rhandler = new RHandler (wellKnownClientEntries,
							  activatedClientEntries,
							  wellKnownServiceEntries,
							  activatedServiceEntries);
			// We first read the machine.config file
			parser1.Parse (rreader1, mhandler);
			// Then we read the file specified by the user
			// ** WARNING **
			// if any other file, as web.config or app.config must be read,
			// before that specified by the user, we have to do it here.
			// Using the same RReader class is OK for reading, just the
			// IHandler class needs to be changed for different file schemas.
			parser2.Parse (rreader2, rhandler);
		}
		// FIXME: this is hardcoded now
		private static string GetMachineConfigPath ()
		{
			return "/etc/mono/machine.config";
		}
	}
	
	internal class RReader : MiniParser.IReader {
		private string xml; // custom remoting config file
		private int pos;

		public RReader (string filename)
		{
			try {
				StreamReader sr = new StreamReader (filename);
				xml = sr.ReadToEnd ();
				sr.Close ();
			}
			catch {
				xml = null;
			}
		}

		public int Read () {
			try {
				return (int) xml[pos++];
			}
			catch {
				return -1;
			}
		}
	}

	internal class MHandler : MiniParser.IHandler {
		Hashtable wellKnownClientEntries;
		Hashtable activatedClientEntries;
		Hashtable wellKnownServiceEntries;
		Hashtable activatedServiceEntries;
		Hashtable channels = null;
		Hashtable formatters = null;
		bool declaresChannels = false;
		bool declaresFormatters = false;
		long pos = 0;
		string appName;
		
		public MHandler (Hashtable wellKnownClientEntries,
				 Hashtable activatedClientEntries,
				 Hashtable wellKnownServiceEntries,
				 Hashtable activatedServiceEntries)
		{
			this.wellKnownClientEntries = wellKnownClientEntries;
			this.activatedClientEntries = activatedClientEntries;
			this.wellKnownServiceEntries = wellKnownServiceEntries;
			this.activatedServiceEntries = activatedServiceEntries;
		}
		public void OnStartParsing (MiniParser parser) {}
		public void OnStartElement (string name, MiniParser.IAttrList attrs)
		{
			string _ref, dn, id, type, version, culture, pkt;
			switch (name) {
			case "configuration":
				if (pos++ != 0)
					throw new Exception ();
				break;
			case "system.runtime.remoting":
				break;
			case "application":
				if (attrs.Names.Length > 0)
					appName = attrs.Values[0];
				break;
			case "lifetime":
				if (pos++ < 3)
					throw new Exception ();
				for (int i=0; i < attrs.Names.Length; ++i) {
					switch (attrs.Names[i]) {
					case "leaseTime":
						break;
					case "sponsorShipTimeOut":
						break;
					case "renewOnCallTime":
						break;
					case "leaseManagerPollTime":
						break;
					default:
						throw new Exception ();
					}
				}
				break;
			case "channels":
				break;
			case "channel":
				declaresChannels = true;
				channel_data channel;
				_ref = dn = id = type = version = culture = pkt = null;
				bool dl = false; // <-- FIX that
				for (int i=0; i < attrs.Names.Length; ++i) {
					switch (attrs.Names[i]) {
					case "ref":
						_ref = attrs.Values[i];
						break;
					case "displayName":
						dn = attrs.Values[i];
						break;
					case "delayLoadAsClientChannel":
						dl = Convert.ToBoolean(attrs.Values[i]);
						break;
					case "id":
						id = attrs.Values[i];
						break;
					case "type":
						type = attrs.Values[i];
						break;
					case "Version":
						version = attrs.Values[i];
						break;
					case "Culture":
						culture = attrs.Values[i];
						break;
					case "PublicKeyToken":
						pkt = attrs.Values[i];
						break;
					default:
						throw new Exception ();
					}
				}
				if (!declaresChannels) {
					declaresChannels = true;
					channels = new Hashtable ();
				}
				if (_ref != null) {
					channel = new channel_data (_ref, dn, dl);
					// This kind of channels are added by Ref reference
					channels.Add (_ref, channel);
				} else {
					channel = new channel_data (id, type, version, culture, pkt);
					// This kind of channels are added by PublicKeyToken reference
					channels.Add (pkt, channel);
				}
				break;
			case "serverProviders":
				break;
			case "provider":
				break;
			case "formatter":
				formatter_data formatter;
				id = type = version = culture = pkt = null;
				for (int i=0; i < attrs.Names.Length; ++i) {
					switch (attrs.Names[i]) {
					case "id":
						id = attrs.Values[i];
						break;
					case "type":
						type = attrs.Values[i];
						break;
					case "Version":
						version = attrs.Values[i];
						break;
					case "Culture":
						culture = attrs.Values[i];
						break;
					case "PublicKeyToken":
						pkt = attrs.Values[i];
						break;
					default:
						throw new Exception ();
					}
				}
				if (!declaresFormatters) {
					declaresFormatters = true;
					formatters = new Hashtable ();
				}
				formatter = new formatter_data (id, type, version, culture, pkt);
				// Formatters are add by PublicKeyToken reference
				formatters.Add (pkt, formatter);
				break;
			case "client":
				break;
			case "service":
				break;
			case "wellknown":
				break;
			case "activated":
				break;
			default:
				break;
			}
		}
		public void OnEndElement (string name) {}
		public void OnChars (string ch) {}
		public void OnEndParsing (MiniParser parser)
		{
			// Here we register all the appropiate channels and formatters
			// except those channels that have the attribute
			// "delayLoadAsClientChannel" set to 'true'
			if (declaresChannels) {
				foreach (channel_data cd in channels)
				{
					IChannel channel = null;
					switch (cd.Type) {
					case "System.Runtime.Remoting.Channels.Tcp.TcpChannel":
						break;
					case "System.Runtime.Remoting.Channels.TcpClientChannel":
						break;
					case "System.Runtime.Remoting.Channels.TcpServerChannel":
						break;
					case "System.Runtime.Remoting.Channels.HttpChannel":
						break;
					case "System.Runtime.Remoting.Channels.HttpClientChannel":
						break;
					case "System.Runtime.Remoting.Channels.HttpServerChannel":
						break;
					}
					// ChannelServices.RegisterChannel ();
				}
			}
			if (declaresFormatters) {
				foreach (formatter_data fd in formatters)
				{
					switch (fd.Type) {
					case "System.Runtime.Remoting.Channels.SoapClientFormatterSinkProvider":
						break;
					case "System.Runtime.Remoting.Channels.BinaryClientFormatterSinkProvider":
						break;
					case "System.Runtime.Remoting.Channels.SoapServerFormatterSinkProvider":
						break;
					case "System.Runtime.Remoting.Channels.BinaryServerFormatterSinkProvider":
						break;
					case "System.Runtime.Remoting.MetadataServices.SdlChannelSinkProvider":
						break;
					}
				}
			}
		}
	}

	internal class RHandler : MiniParser.IHandler {
		Hashtable wellKnownClientEntries;
		Hashtable activatedClientEntries;
		Hashtable wellKnownServiceEntries;
		Hashtable activatedServiceEntries;
		long pos = 0;
		string appName;
		
		public RHandler (Hashtable wellKnownClientEntries,
				 Hashtable activatedClientEntries,
				 Hashtable wellKnownServiceEntries,
				 Hashtable activatedServiceEntries)
		{
			this.wellKnownClientEntries = wellKnownClientEntries;
			this.activatedClientEntries = activatedClientEntries;
			this.wellKnownServiceEntries = wellKnownServiceEntries;
			this.activatedServiceEntries = activatedServiceEntries;
		}
		public void OnStartParsing (MiniParser parser) {}
		public void OnStartElement (string name, MiniParser.IAttrList attrs)
		{
			switch (name) {
			case "configuration":
				if (pos++ != 0)
					throw new Exception ();
				break;
			case "system.runtime.remoting":
				if (pos++ != 1)
					throw new Exception ();
				break;
			case "application":
				if (pos++ != 2)
					throw new Exception ();
				if (attrs.Names.Length > 0)
					appName = attrs.Values[0];
				break;
			case "lifetime":
				if (pos++ < 3)
					throw new Exception ();
				for (int i=0; i < attrs.Names.Length; ++i) {
					switch (attrs.Names[i]) {
					case "leaseTime":
						break;
					case "sponsorShipTimeOut":
						break;
					case "renewOnCallTime":
						break;
					case "leaseManagerPollTime":
						break;
					default:
						throw new Exception ();
					}
				}
				break;
			case "channels":
				break;
			case "channel":
				break;
			case "serverProviders":
				break;
			case "provider":
				break;
			case "formatter":
				break;
			case "client":
				break;
			case "service":
				break;
			case "wellknown":
				break;
			case "activated":
				break;
			default:
				Console.WriteLine ("Not supported element " + name);
				break;
			}
		}
		public void OnEndElement (string name) {}
		public void OnChars (string ch) {}
		public void OnEndParsing (MiniParser parser) {}
	}

		/*******************************************************************
         * Internal data structures used by MHandler, RHandler... to store *
         * machine.config's remoting related data.                         *
         * If having them implemented this way, makes configuration too    *
         * slow, we can use string arrays.                                 *
         *******************************************************************/
	internal struct channel_data {
		private string _ref;
		private string id;
		private string displayName;
		private bool delayLoadAsClientChannel;
		private string type;
		private string _namespace;
		private string version;
		private string culture;
		private string publicKeyToken;

		public channel_data (string _ref,
				     string displayName,
				     bool delayLoadAsClientChannel)
		{
			this._ref = _ref;
			this.id = null;
			this.displayName = displayName;
			this.delayLoadAsClientChannel = delayLoadAsClientChannel;
			this.type = null;
			this._namespace = null;
			this.version = null;
			this.culture = null;
			this.publicKeyToken = null;
		}

		public channel_data (string id,
				     string type,
				     string version,
				     string culture,
				     string publicKeyToken)
		{
			string[] t_ns = type.Split (',');
			this._ref = null;
			this.id = id;
			this.displayName = null;
			this.delayLoadAsClientChannel = false; // value does not matter
			this.type = t_ns[0];
			// FIXME: 
			// Check wether or not the user enters an space between the
			// ',' and the namespace. Check behavior in MS.NET.
			this._namespace = t_ns[1];
			this.version = version;
			this.culture = culture;
			this.publicKeyToken = publicKeyToken;
		}

		internal string Ref { get { return _ref; } }
		internal string Id { get { return id; } }
		internal string DisplayName { get { return displayName; } }
		internal bool DelayLoadAsClientChannel { get { return delayLoadAsClientChannel; } }
		internal string Type { get { return type; } }
		internal string Namespace { get { return _namespace; } }
		internal string Version { get { return version; } }
		internal string Culture { get { return culture; } }
		internal string PublicKeyToken { get { return publicKeyToken; } }
	}

	internal struct formatter_data {
		private string id;
		private string type;
		private string _namespace;
		private string version;
		private string culture;
		private string publicKeyToken;

		public formatter_data (string id,
				       string type,
				       string version,
				       string culture,
				       string publicKeyToken)
		{
			string[] t_ns = type.Split (',');
			this.id = id;
			
			this.type = t_ns[0];
			// FIXME: 
			// Check wether or not the user enters an space between the
			// ',' and the namespace. Check behavior in MS.NET.
			this._namespace = t_ns[1];
			this.version = version;
			this.culture = culture;
			this.publicKeyToken = publicKeyToken;
		}

		internal string Id { get { return id; } }
		internal string Type { get { return type; } }
		internal string Namespace { get { return _namespace; } }
		internal string Version { get { return version; } }
		internal string Culture { get { return culture; } }
		internal string PublicKeyToken { get { return publicKeyToken; } }
	}
}
