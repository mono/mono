//
// System.Resources.ResourceReader.cs
//
// Author: 
// 	Duncan Mak <duncan@ximian.com>
//
// 2001 (C) Ximian Inc, http://www.ximian.com
//

namespace System.Resources {
	   public sealed class ResoureReader : IResourceReader, IDisposible {

			 // Constructors
			 public ResoureReader (Stream stream) {
				    if (stream == null)
						  throw new ArgumentNullException ("Stream is null.");
				    if (stream.CanWrite == false)
						  throw new ArgumentException ("stream is not writable.");
			 }

			 public ResoureReader (string fileName) {
				    if (fileName == null) throw new ArgumentException ("fileName is null.");
				    if (System.IO.File.Exists (fileName) == false) 
						  throw new FileNotFoundException ("The file cannot be found.");

			 }

			 public void Close () {}

			 public IDictionaryEnumerator GetEnumerator () {
				    return new DictionaryEnumerator
			 }

	   }

	   private class DictionaryEnumerator : IDictionaryEnumerator {
			 protected DictionaryEntry entry;
			 protected object key;
			 protected object value;

			 public DictionaryEntry Entry {
				    get { return entry; }
			 }

			 public object Key {
				    get { return key; }
			 }

			 public object Value {
				    get { return value; }
			 }
	   }
}
