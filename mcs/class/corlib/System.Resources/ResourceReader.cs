//
// System.Resources.ResourceReader.cs
//
// Author: 
// 	Duncan Mak <duncan@ximian.com>
//
// 2001 (C) Ximian Inc, http://www.ximian.com
//

using System.Collections;
using System.IO;

namespace System.Resources {
	public sealed class ResoureReader : IResourceReader, IDisposable {
		Stream stream;
		
		// Constructors
		public ResoureReader (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("Stream is null.");
			
			if (stream.CanWrite == false)
				throw new ArgumentException ("stream is not writable.");

			this.stream = stream;
		}
		
		public ResoureReader (string fileName)
		{
			if (fileName == null)
				throw new ArgumentException ("fileName is null.");
			
			if (System.IO.File.Exists (fileName) == false) 
				throw new FileNotFoundException ("The file cannot be found.");

			stream = new FileStream (fileName, FileMode.Open);
		}
		
		public void Close ()
		{
			stream.Close ();
		}
		
		public IDictionaryEnumerator GetEnumerator () {
			return new DictionaryEnumerator ();
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((IResourceReader) this).GetEnumerator();
		}
		
		[MonoTODO]
		void IDisposable.Dispose ()
		{
			
		}
		
	}
	
	internal class DictionaryEnumerator : IDictionaryEnumerator {
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
		
		[MonoTODO]
		public object Current {
			get { return null; }
		}
		
		[MonoTODO]
		public bool MoveNext () {
			return false;
		}
		
		[MonoTODO]
		public void Reset () { }
	}
}
