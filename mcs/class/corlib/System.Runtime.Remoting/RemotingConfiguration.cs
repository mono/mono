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
		static string applicationName = null;
		static string processId = null;
		static string configFile = "";
		static MiniParser parser = null; 
		
		// public properties
		public static string ApplicationId {
			get { return null; }
		}
		
		public static string ApplicationName {
			get { return applicationName; }
			set { applicationName = value; }
		}

		public static string ProcessId {
			get { return processId; }
		}

		// public methods
		
		///////////////////////////////////////////////////////////////////////////
		// This code will be un-reimplemented till I get known of how can I read //
		// XML with the MiniParser. 						 //
		///////////////////////////////////////////////////////////////////////////
		
		public static void Configure (string filename) 
		{
		/*	if (filename != null)
				configFile = filename;
			
			try {
				try {
					parser = new MiniParser ();
				} catch (Exception e) {
					Console.WriteLine (e); // Look for possible exceptions thrown.
				}

				InitRead (parser);
				ReadConfigFile (parser);
			} finally {
				if (parser != null)
					parser.Close ();
			}		
	*/		
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
	
		// Private methods
		// A lot of code has been taken from System.Configuration.ConfigurationSettings.
		// But with some modifications ;-)
		private void InitRead () 
		{
		/*	reader.MoveToContent ();
			if (reader.NodeType != XmlNodeType.Element || reader.Name != "configuration")
				ThrowException ("Configuration file does not have a valid root element", reader);
	
			if (reader.HasAttributes)
				ThrowException ("Unrecognized attribute in root element", reader);
			
			MoveToNextElement (reader);
			
			// We add this to the original to test wheter there next element is the expected
			// <system.runtime.remoting> or if it's not we throw an exception.
			
			if (reader.NodeType != XmlNodeType.Element || reader.Name != "system.runtime.remoting")
				ThrownException ("Expected <system.runtime.remoting> after root element <configuration>", reader);
			
			MoveToNextElement (reader);

			// This one is to check the <application> element

			if (reader.NodeType != XmlNodeType.Element || reader.Name != "application")
				ThrownException ("Expected <application []> after <system.runtime.remoting> element", reader);
		*/
		}
		
		// TODO: check for all the possible sections of remoting configuration files.
		// At this point we are not considering that the order in the file sections at the same depth level
		// f.ex. service, channels, should be in a particular order.
		private void ReadConfigFile () 
		{
		/*	// If all went right, we should be at the <application []> element.
			// So we will take the name and other info from the attributes if 
			// they are there.
			int depth = reader.Depth;
			while (reader.Depth == depth) {
				string name = reader.Name;
				if (name == "service") {
					ReadSection (reader, "service");
					continue;
				}

				if (name == "channels") {
					ReadSection (reader, "channels");
					continue;
				}
				
				MoveToNextElement (reader);
			}
		*/
		}

		private void ReadSection (string sectionName) 
		{
		/*
			string attName;
			string nameValue = null;

			if (sectionName == "service") {
				while (reader.MoveToNextAttribute ()) {
					attName = reader.Name;
					if (attName == null)
						continue;
	
					if (attName == "name") {
						nameValue = reader.Value;
						continue;
					}

					ThrownException ("Unrecognized attribute", reader);
				}
			}

			// ** NEED TO PROVIDE THE SAME FOR CHANNELS, ETC
			
			if (sectionName == "service") {
			}
			
			reader.MoveToElement ();
			// Take action here
		*/
		}
			
		private void MoveToNextElement () 
		{
		/*
			while (reader.Read ()) {
				XmlNodeType ntype = reader.NodeType;
				if (ntype == XmlNodeType.Element)
					return;

				if (ntype != XmlNodeType.Whitespace &&
				    ntype != XmlNodeType.Comment &&
				    ntype != XmlNodeType.SignificantWhitespace &&
				    ntype != XmlNodeType.EndElement)
					ThrownException ("Unrecognized element", reader);
			}
		*/
		}
		
		private void ThrownException (string message) 
		{
		/*
			throw new ConfigurationException (text, fileName, reader.LineNumber);
		*/
		}		
	}	
}


