//
// System.Resources.ResourceSet.cs
//
// Author:
//	Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.		http://www.ximian.com
//

using System.Collections;
using System.IO;

namespace System.Resources
{
	   
	   [Serializable]
	   public class ResourceSet : IDisposable
	   {

			 protected IResourceReader Reader;
			 protected Hashtable Table;
			 
			 // Constructors
			 protected ResourceSet () {}
			 protected ResourceSet (IResourceReader reader)
			 {
				    if (reader == null)
						  throw new ArgumentNullException ("The reader is null.");
				    Reader = reader;
			 }

			 protected ResourceSet (Stream stream)
			 {
				    Reader = new ResourceReader (stream);
			 }
			 
			 protected ResourceSet (String fileName)
			 {
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
			 
			 public void Dispose (bool disposing)
			 {
				    if (disposing) {
						  Reader = null;
						  Table = null;
				    } 
			 }
			 
			 public virtual Type GetDefaultReader ()
			 {
				    return (typeof (ResourceReader));
			 } 
			 public virtual Type GetDefaultWriter ()
			 {
				    return (typeof (ResourceWriter));
			 }

			 public virtual object GetObject (string name)
			 {
				    if (name == null)
						  throw new ArgumentNullException ("The name parameter is null.");
				    if (Reader == null)
						  throw new InvalidOperationException ("The ResourceSet has been closed.");
				    if (Table == null) {
						  ReadResources ();
						  return Table[name];
				    }
				    if (Table != null)
						  return Table[name];
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

								if (String.Compare (key, name, true))
									   return de.Value;
						  }
				    } else
						  return Table[name]
			 }

			 public virtual string GetString (string name)
			 {
				    Object o = GetObject (name);
				    if (o is string)
						  return (string) o;
				    return null;
			 }

			 public virtual string GetString (string name, bool ignoreCase)
			 {
				    Object o = GetObject (name, ignoreCase);
				    if (o is string)
						  return (string) o;
				    return null;
			 }

			 public virtual void ReadResources ()
			 {
				    IDictionaryEnumerator i = Reader.GetEnumerator();

				    if (Table == null)
						  Table = new ResourceHashtable ();
				    i.Reset ();

				    while (i.MoveNext ()) 
						  Table.Add (i.Key, i.Value);
			 }
	   }
}

