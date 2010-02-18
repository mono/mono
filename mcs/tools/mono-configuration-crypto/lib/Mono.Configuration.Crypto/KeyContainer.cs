using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;

using Mono.Security.Cryptography;

namespace Mono.Configuration.Crypto
{
	public class KeyContainer : IEnumerable <Key>
	{
		List <Key> keys;

		List <Key> Keys {
			get {
				if (keys == null)
					keys = new List <Key> ();

				return keys;
			}
		}

		public int Count {
			get {
				if (keys == null)
					return 0;

				return keys.Count;
			}
		}

		public bool Local {
			get;
			set;
		}
		
		public string Name {
			get;
			set;
		}

		public Key this [int index] {
			get {
				int count = Count;

				if (count == 0)
					return null;
				
				if (index < 0 || index >= count)
					throw new ArgumentOutOfRangeException ("index");

				return keys [index];
			}
		}
		
		public KeyContainer (string name, bool machineStore)
		{
			Name = name;
			Local = !machineStore;
		}
		
		public static void RemoveFromDisk (string containerName, bool machineStore)
		{
			var csp = new CspParameters ();
			if (machineStore)
				csp.Flags |= CspProviderFlags.UseMachineKeyStore;
			csp.ProviderName = String.Empty;
			csp.KeyContainerName = containerName;

			var kpp = new KeyPairPersistence (csp, String.Empty);
			if (!File.Exists (kpp.Filename))
				return;

			File.Delete (kpp.Filename);
		}
		
		public void Add (Key key)
		{
			if (key == null)
				return;
			
			Keys.Add (key);
		}

		public IEnumerator <Key> GetEnumerator ()
		{
			return Keys.GetEnumerator ();
		}
		
#region IEnumerable
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
#endregion
		
#region IEnumerable <T>
		IEnumerator <Key> IEnumerable <Key>.GetEnumerator ()
		{
			return GetEnumerator ();
		}
#endregion
	}
}
