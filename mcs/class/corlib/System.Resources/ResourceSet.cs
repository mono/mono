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
	public class ResourceSet : IDisposable

#if (NET_1_1)
						, IEnumerable
#endif

	{

		protected IResourceReader Reader;
		protected Hashtable Table;

		// Constructors
		protected ResourceSet () {}

		public ResourceSet (IResourceReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("The reader is null.");
			Reader = reader;
		}

		[SecurityPermission (SecurityAction.LinkDemand, SerializationFormatter = true)]
		public ResourceSet (Stream stream)
		{
			if(stream==null) {
				throw new ArgumentNullException("stream is null");
			}

			if(!stream.CanRead) {
				throw new ArgumentException("stream is not readable");
			}
			
			Reader = new ResourceReader (stream);
		}

		public ResourceSet (String fileName)
		{
			if(fileName==null) {
				throw new ArgumentNullException("filename is null");
			}
			
			Reader = new ResourceReader (fileName);
		}

		public virtual void Close ()
		{
			Dispose (true);
		}

		public void Dispose()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				if(Reader!=null) {
					Reader.Close();
				}
			}

			Reader = null;
			Table = null;
		}

		public virtual Type GetDefaultReader ()
		{
			return (typeof (ResourceReader));
		} 
		public virtual Type GetDefaultWriter ()
		{
			return (typeof (ResourceWriter));
		}

#if (NET_1_1)

		[ComVisible (false)]
		public virtual IDictionaryEnumerator GetEnumerator ()
		{
			if (Table == null)
				ReadResources ();
			return Table.GetEnumerator(); 
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return this.GetEnumerator (); 
		}

#endif

		public virtual object GetObject (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("The name parameter is null.");
			if (Reader == null)
				throw new InvalidOperationException ("The ResourceSet has been closed.");

			if (Table == null) { 
				ReadResources ();
			}
			
			return(Table[name]);
		}

		public virtual object GetObject (string name, bool ignoreCase)
		{
			if (name == null)
				throw new ArgumentNullException ("The name parameter is null.");
			if (Reader == null)
				throw new InvalidOperationException ("ResourceSet has been closed.");
			if (Table == null)
				ReadResources ();

			if (ignoreCase) {
				foreach (DictionaryEntry de in Table) {
					string key = (string) de.Key;
					if (String.Compare (key, name, true, CultureInfo.InvariantCulture) == 0)
						return de.Value;
				}
				return null;
			} else
				return Table[name];
		}

		public virtual string GetString (string name)
		{
			Object o = GetObject (name);
			if (o == null)
				return null;
			if (o is string)
				return (string) o;
			throw new InvalidOperationException("Not a string");
		}

		public virtual string GetString (string name, bool ignoreCase)
		{
			Object o = GetObject (name, ignoreCase);
			if (o == null)
				return null;
			if (o is string)
				return (string) o;
			throw new InvalidOperationException("Not a string");
		}

		protected virtual void ReadResources ()
		{
			if (Reader == null)
				throw new InvalidOperationException ("ResourceSet is closed.");
			
			IDictionaryEnumerator i = Reader.GetEnumerator();

			if (Table == null)
				Table = new Hashtable ();
			i.Reset ();

			while (i.MoveNext ()) 
				Table.Add (i.Key, i.Value);
		}
	}
}
