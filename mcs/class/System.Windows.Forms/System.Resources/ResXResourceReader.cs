//
// System.Resources.ResXResourceReader.cs
//
// Authors: 
// 	Duncan Mak <duncan@ximian.com>
//	Nick Drochak <ndrochak@gol.com>
//
// 2001 (C) Ximian Inc, http://www.ximian.com
//
// TODO: Finish this

using System.Collections;
using System.Resources;
using System.IO;

namespace System.Resources
{
	public sealed class ResXResourceReader : IResourceReader, IDisposable
	{
		Stream stream;

		// Constructors
		public ResXResourceReader (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("Value cannot be null.");
			
			if (!stream.CanRead)
				throw new ArgumentException ("Stream was not readable.");

			this.stream = stream;
			
			if (!IsStreamValid()){
				throw new ArgumentException("Stream is not a valid .resources file!  It was possibly truncated.");
			}
		}
		
		public ResXResourceReader (string fileName)
		{
			if (fileName == null)
				throw new ArgumentException ("Path cannot be null.");
			
			if (String.Empty == fileName)
				throw new ArgumentException("Empty path name is not legal.");

			if (!System.IO.File.Exists (fileName)) 
				throw new FileNotFoundException ("Could not find file " + Path.GetFullPath(fileName));

			stream = new FileStream (fileName, FileMode.Open);

			if (!IsStreamValid()){
				throw new ArgumentException("Stream is not a valid .resources file!  It was possibly truncated.");
			}
		}
		
		[MonoTODO]
		private bool IsStreamValid() {
			return true;
		}

		public void Close ()
		{
			stream.Close ();
			stream = null;
		}
		
		public IDictionaryEnumerator GetEnumerator () {
			if (null == stream){
				throw new InvalidOperationException("ResourceReader is closed.");
			}
			else {
				return null;
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((IResourceReader) this).GetEnumerator();
		}
		
		[MonoTODO]
		void IDisposable.Dispose ()
		{
			// FIXME: is this all we need to do?
			Close();
		}
		
	}  // public sealed class ResXResourceReader
} // namespace System.Resources
