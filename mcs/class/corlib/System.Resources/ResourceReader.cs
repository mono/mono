//
// System.Resources.ResourceReader.cs
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
	class MonoTODO : Attribute {}
	public sealed class ResourceReader : IResourceReader, IDisposable
	{
		Stream stream;
		ArrayList resourceNames = null;
		ArrayList resourceValues = null;
		BinaryReader binaryReader;
		int resourceCount = 0;
		int typeCount = 0;
		ArrayList typeArray = new ArrayList();
		ArrayList hashes = new ArrayList();
		ArrayList positions = new ArrayList();
		int dataSectionOffset;

		// Constructors
		public ResourceReader (Stream stream)
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
		
		public ResourceReader (string fileName)
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
			// not sure how much to check to determine if it's valid, 
			// but look at magic number, version numbers and class names
			string readerClass;
			string resourceSetClass;
			try {
				binaryReader = new BinaryReader(stream);
				int magicNumber = binaryReader.ReadInt32();
				if (-1091581234 != magicNumber) {
					return false;
				}
				int versionNumber = binaryReader.ReadInt32();
				if (1 != versionNumber){
					return false;
				}
				// Ignore next 32bits. they contain the length of the class name strings
				binaryReader.ReadInt32();

				readerClass = binaryReader.ReadString();
				if (!readerClass.StartsWith("System.Resources.ResourceReader")){
					return false;
				}
				resourceSetClass = binaryReader.ReadString();
				if (!resourceSetClass.StartsWith("System.Resources.RuntimeResourceSet")){
					return false;
				}
				int versionNumber2 = binaryReader.ReadInt32();
				if (1 != versionNumber2){
					return false;
				}

				resourceCount = binaryReader.ReadInt32();
				typeCount = binaryReader.ReadInt32();

				for (int i = 0; i < typeCount; i++) {
					typeArray.Add(binaryReader.ReadString());
				}
				for (int i = 0; i < resourceCount; i++) {
					hashes.Add(binaryReader.ReadInt32());
				}
				for (int i = 0; i < resourceCount; i++) {
					positions.Add(binaryReader.ReadInt32());
				}

				dataSectionOffset = binaryReader.ReadInt32();
			}
			catch{
				return false;
			}
			return true;
		}

		private string ReadString(BinaryReader br) {
			return br.ReadString();
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
				// STRATEGY: if this is the first enumerator requested, fill the hash.
				// delaying in this way seems ok since there's not much you can do with just
				// a reader except close it.  And if you close it, you cannot get the enumerator.
				// So, create the hash for the first enumerator, and re-use it for all others.
				if (null == resourceNames) {
					FillResources();
				}
				return new ResourceEnumerator (this);
			}
		}
		
		[MonoTODO]
		private void FillResources(){
			resourceNames = new ArrayList();
			BinaryReader unicodeReader = 
				new BinaryReader(binaryReader.BaseStream, System.Text.Encoding.Unicode);
			// TODO: need to put these in an array and work out when to get the values.
			// also need to figure out the hash and how/if to use it.
			string test = unicodeReader.ReadString();
			int offset = binaryReader.ReadInt32();
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
		
	}
	
	internal class ResourceEnumerator : IDictionaryEnumerator
	{
		protected DictionaryEntry entry;
		protected object key;
		protected object value;
		protected ResourceReader reader;
		
		public ResourceEnumerator(ResourceReader readerToEnumerate){
			reader = readerToEnumerate;
		}

		public DictionaryEntry Entry
		{
			get { return entry; }
		}
		
		public object Key
	     {
			get { return key; }
		}
		
		public object Value
		{
			get { return value; }
		}
		
		[MonoTODO]
		public object Current
		{
			get { return null; }
		}
		
		[MonoTODO]
		public bool MoveNext ()
		{
			return false;
		}
		
		[MonoTODO]
		public void Reset () { }
	}
}
