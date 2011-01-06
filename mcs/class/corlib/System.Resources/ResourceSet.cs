//
// System.Resources.ResourceSet.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//	Dick Porter (dick@ximian.com)
//	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2001, 2002 Ximian, Inc.		http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Resources
{
	[Serializable]
	[ComVisible (true)]
	public class ResourceSet : IDisposable, IEnumerable
	{
		[NonSerialized]
		protected IResourceReader Reader;
		protected Hashtable Table;
		bool resources_read;
		[NonSerialized] Hashtable table_nocase;

		[NonSerialized]
		private bool disposed;

		// Constructors
		protected ResourceSet ()
		{
			Table = new Hashtable ();
			resources_read = true;
		}

		public ResourceSet (IResourceReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			Table = new Hashtable ();
			Reader = reader;
		}

		[SecurityPermission (SecurityAction.LinkDemand, SerializationFormatter = true)]
		public ResourceSet (Stream stream)
		{
			Table = new Hashtable ();
			Reader = new ResourceReader (stream);
		}

		internal ResourceSet (UnmanagedMemoryStream stream)
		{
			Table = new Hashtable ();
			Reader = new ResourceReader (stream);
		}
		
		public ResourceSet (string fileName)
		{
			Table = new Hashtable ();
			Reader = new ResourceReader (fileName);
		}

		public virtual void Close ()
		{
			Dispose ();
		}

		public void Dispose()
		{
			Dispose (true);

			// If we are explicitly disposed, we can avoid finalization.
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				if(Reader != null)
					Reader.Close();
			}

			Reader = null;
			Table = null;
			table_nocase = null;
			disposed = true;
		}

		public virtual Type GetDefaultReader ()
		{
			return (typeof (ResourceReader));
		}

		public virtual Type GetDefaultWriter ()
		{
			return (typeof (ResourceWriter));
		}

		[ComVisible (false)]
		public virtual IDictionaryEnumerator GetEnumerator ()
		{
			if (disposed)
				throw new ObjectDisposedException ("ResourceSet is closed.");
			ReadResources ();
			return Table.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return this.GetEnumerator ();
		}

		private object GetObjectInternal (string name, bool ignoreCase)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (disposed)
				throw new ObjectDisposedException ("ResourceSet is closed.");

			ReadResources ();

			if (!ignoreCase)
				return Table [name];

			if (table_nocase == null) {
				lock (Table) {
					if (table_nocase == null) {
						Hashtable ht = new Hashtable (StringComparer.InvariantCultureIgnoreCase);
						foreach (DictionaryEntry de in Table) {
							ht.Add (de.Key, de.Value);
						}
						table_nocase = ht;
					}
				}
			}
			return table_nocase [name];
		}

		public virtual object GetObject (string name)
		{
			return GetObjectInternal (name, false);
		}

		public virtual object GetObject (string name, bool ignoreCase)
		{
			return GetObjectInternal (name, ignoreCase);
		}

		private string GetStringInternal (string name, bool ignoreCase)
		{
			object value = GetObject (name, ignoreCase);
			if (value == null)
				return null;

			string s = (value as string);
			if (s == null)
				throw new InvalidOperationException (string.Format (
					"Resource '{0}' is not a String. Use " +
					"GetObject instead.", name));

			return s;
		}

		public virtual string GetString (string name)
		{
			return GetStringInternal (name, false);
		}

		public virtual string GetString (string name, bool ignoreCase)
		{
			return GetStringInternal (name, ignoreCase);
		}

		protected virtual void ReadResources ()
		{
			if (resources_read)
				return;

			if (Reader == null)
				throw new ObjectDisposedException ("ResourceSet is closed.");
			lock (Table) {
				if (resources_read)
					return;

				IDictionaryEnumerator i = Reader.GetEnumerator();
				i.Reset ();
				while (i.MoveNext ()) 
					Table.Add (i.Key, i.Value);
				resources_read = true;
			}
		}

		internal UnmanagedMemoryStream GetStream (string name, bool ignoreCase)
		{
			if (Reader == null)
				throw new ObjectDisposedException ("ResourceSet is closed.");

			IDictionaryEnumerator i = Reader.GetEnumerator();
			i.Reset ();
			while (i.MoveNext ()){
				if (String.Compare (name, (string) i.Key, ignoreCase) == 0)
					return ((ResourceReader.ResourceEnumerator) i).ValueAsStream;
			}
			return null;
		}
	}
}
