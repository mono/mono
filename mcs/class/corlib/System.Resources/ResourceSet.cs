//
// System.Resources.ResourceSet.cs
//
// Author:
//	Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.		http://www.ximian.com
//

namespace System.Resources {

	   public class ResourceSet : IDisposible {

			 protected IResourceReader Reader;
			 protected HashTable Table;
			 
			 // Constructors
			 protected ResourceSet () {}
			 protected ResourceSet (IResourceReader reader) {
				    if (reader == null)
						  throw new ArgumentNullException("The reader is null.");
				    Reader = reader;
			 }

			 protected ResourceSet (Stream stream) {
				    Reader = new ResourceReader (stream);
			 }
			 
			 protected ResourceSet (String fileName) {
				    Reader = new ResourceReader (filename);
			 }

			 public virtual void Close () {
				    Dispose (true);
			 }

			 public void Dispose() {
				    Dispose (true);
			 }
			 
			 public void Dispose (bool disposing) {
				    if (disposing) {
						  Reader = null;
						  Table = null;
				    } 
			 }
			 
			 public virtual Type GetDefaultReader () {
				    return (typeof (ResourceReader));
			 } 
			 public virtual Type GetDefaultWriter () {
				    return (typeof (ResourceWriter));
			 }

			 public virtual object GetObject (string name) {
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
			    return null;
			 }

			 public virtual object GetObject (string name, bool ignoreCase) {
				    if (name == null)
						  throw new ArgumentNullException ("The name parameter is null.");
				    if (Reader == null)
						  throw new InvalidOperationException ("ResourceSet has been closed.");
				    if (Table != null && ignoreCase == false)
						  return Table[name];
				    
				    if (ignoreCase && Table == null)
						  ReadResources (); // find out how to get element from Hashtable

				    if (ignoreCase && Table != null) // while ignoring case.
						  throw new NoImplementedException ();

				    return null;
			 }

			 public virtual string GetString (string name) {
				    Object o = GetObject (name);
				    if (o is string)
						  return o;
				    return null;
			 }
			 public virtual string GetString (string name, bool ignoreCase) {}

			 public virtual void ReadResources () {
				    IDictonaryEnumerator i = Reader.getEnumerator();

				    if (Table == null)
						  Table = new HashTable ();
				    i.Reset ();

				    while (i.MoveNext ()) 
						  Table.add (i.Key, i.Value);
				    
			 }
	   }
}
