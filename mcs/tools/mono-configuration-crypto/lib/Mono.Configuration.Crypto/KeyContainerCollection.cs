using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Mono.Security.Cryptography;

namespace Mono.Configuration.Crypto
{
	public class KeyContainerCollection : ICollection <KeyContainer>
	{
		bool machineStore;
		SortedDictionary <string, KeyContainer> containers;

		SortedDictionary <string, KeyContainer> Containers {
			get {
				if (containers == null)
					containers = new SortedDictionary <string, KeyContainer> ();
				return containers;
			}
		}

		public KeyContainer this [string name] {
			get { return GetContainer (name); }
		}
		
		public KeyContainerCollection (bool machineStore)
		{
			string topPath;

			if (machineStore)
				topPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.CommonApplicationData), ".mono");
			else
				topPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), ".mono");
			topPath = Path.Combine (topPath, "keypairs");
			
			this.machineStore = machineStore;
			if (Directory.Exists (topPath))
				LoadKeys (topPath);
		}
		
		public bool Contains (string name)
		{
			if (String.IsNullOrEmpty (name) || Count == 0)
				return false;

			KeyContainer c = GetContainer (name);
			if (c == null || c.Count == 0)
				return false;

			return true;
		}
		
		public IEnumerator <KeyContainer> GetEnumerator ()
		{
			foreach (var de in Containers)
				yield return de.Value;
		}

		KeyContainer GetContainer (string name)
		{
			KeyContainer c;

			if (Containers.TryGetValue (name, out c))
				return c;

			var md5 = MD5.Create ();
			byte[] result = md5.ComputeHash (Encoding.UTF8.GetBytes (name));
			string hashed = new Guid (result).ToString ();

			if (Containers.TryGetValue (hashed, out c))
				return c;

			return null;
		}
		
		void LoadKeys (string path)
		{
			string[] files = Directory.GetFiles (path, "*.xml");
			if (files == null || files.Length == 0)
				return;

			SortedDictionary <string, KeyContainer> containers = Containers;
			KeyContainer keyContainer;
			Key key;
			string containerName;
			
			foreach (string file in files) {
				key = new Key (file, machineStore);
				if (!key.IsValid)
					continue;

				containerName = key.ContainerName;
				if (!containers.TryGetValue (containerName, out keyContainer)) {
					keyContainer = new KeyContainer (containerName, machineStore);
					containers.Add (containerName, keyContainer);
				}
				
				keyContainer.Add (key);
			}
		}
		
#region IEnumerable
		IEnumerator <KeyContainer> IEnumerable <KeyContainer>.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
#endregion
		
#region ICollection
		public int Count {
			get {
				if (containers == null)
					return 0;

				return containers.Count;
			}
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public void Add (KeyContainer item)
		{
			if (item == null)
				return;

			SortedDictionary <string, KeyContainer> containers = Containers;
			string name = item.Name;
			
			if (containers.ContainsKey (name))
				containers [name] = item;
			else
				containers.Add (name, item);
		}

		public void Clear ()
		{
			if (containers == null)
				return;

			containers.Clear ();
		}

		public bool Contains (KeyContainer item)
		{
			if (Count == 0 || item == null)
				return false;

			return containers.ContainsKey (item.Name);
		}

		public void CopyTo (KeyContainer[] array, int arrayIndex)
		{
			if (Count == 0)
				return;

			containers.Values.CopyTo (array, arrayIndex);
		}

		public bool Remove (KeyContainer item)
		{
			if (Count == 0 || item == null)
				return false;

			return containers.Remove (item.Name);
		}
#endregion
	}
}
