using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Cryptography;

using Mono.Security.Cryptography;
using Mono.Xml;

namespace Mono.Configuration.Crypto
{
	public class Key
	{
		static readonly char[] splitChars = {']'};
		
		string file;
		KeyPairPersistence keypair;
		bool machineStore;
		uint keySize;

		public bool IsValid {
			get;
			private set;
		}

		public string KeyValue {
			get;
			set;
		}

		public bool Local {
			get { return !machineStore; }
		}
		
		public string ContainerName {
			get;
			set;
		}

		public int ProviderType {
			get;
			set;
		}
		
		public Key (string file, bool machineStore)
		{
			if (String.IsNullOrEmpty (file))
				throw new ArgumentNullException ("file");
			
			this.file = file;
			this.machineStore = machineStore;
			this.keySize = 0;
			ReadFile ();
		}

		public Key (string containerName, string keyValue, bool machineStore)
		{
			if (String.IsNullOrEmpty (containerName))
				throw new ArgumentNullException ("containerName");

			if (String.IsNullOrEmpty (keyValue))
				throw new ArgumentNullException ("keyValue");

			this.file = null;
			this.machineStore = machineStore;
			this.keySize = 0;
			this.KeyValue = keyValue;
			this.IsValid = true;

			if (keyValue.StartsWith ("RSAKeyValue", StringComparison.Ordinal))
				this.ProviderType = 1;
			else if (keyValue.StartsWith ("DSAKeyValue", StringComparison.Ordinal))
				this.ProviderType = 13;
			else
				this.ProviderType = -1;
		}
		
		public Key (string containerName, uint keySize, bool machineStore)
		{
			if (String.IsNullOrEmpty (containerName))
				throw new ArgumentNullException ("containerName");

			this.ContainerName = containerName;
			this.machineStore = machineStore;
			this.keySize = keySize;
			this.ProviderType = 1;
			
			GenerateKeyPair ();
		}
		
		public void Save ()
		{
			var csp = new CspParameters (1);
			if (machineStore)
				csp.Flags |= CspProviderFlags.UseMachineKeyStore;
			csp.ProviderName = String.Empty;
			csp.KeyContainerName = ContainerName;
			
			var kpp = new KeyPairPersistence (csp, KeyValue);
			kpp.Save ();
		}
		
		void GenerateKeyPair ()
		{
			var rsa = new RSAManaged ((int)keySize);
			KeyValue = rsa.ToXmlString (true);
			IsValid = true;
		}
		
		void ReadFile ()
		{
			var sp = new SecurityParser ();
			sp.LoadXml (File.ReadAllText (file));

			SecurityElement root = sp.ToXml ();
			if (root.Tag == "KeyPair") {
				SecurityElement keyvalue = root.SearchForChildByTag ("KeyValue");
				if (keyvalue.Children.Count == 0)
					return;
				
				KeyValue = keyvalue.Children [0].ToString ();

				SecurityElement properties = root.SearchForChildByTag ("Properties");
				if (properties.Children.Count == 0)
					return;

				SecurityElement property;
				string containerName = null, attribute;
				int providerType = -1;
				
				foreach (object o in properties.Children) {
					property = o as SecurityElement;
					if (property == null)
						continue;

					switch (property.Tag) {
						case "Provider":
							attribute = property.Attribute ("Type");
							if (String.IsNullOrEmpty (attribute))
								return;

							if (!Int32.TryParse (attribute, out providerType))
								return;
							break;

						case "Container":
							attribute = property.Attribute ("Name");
							if (String.IsNullOrEmpty (attribute))
								return;

							containerName = attribute;
							break;
					}
				}

				if (String.IsNullOrEmpty (containerName) || providerType == -1)
					return;

				ContainerName = containerName;
				ProviderType = providerType;
				IsValid = true;
			}
		}
	}
}
