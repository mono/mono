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
		internal ArrayList resourceNames = null;
		internal ArrayList resourceValues = null;
		BinaryReader binaryReader;
		internal int resourceCount = 0;
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

				// LAMESPEC: what is the next Int32 here?
				binaryReader.ReadInt32();
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
		
		internal struct NameOffsetPair {
			public string name;
			public int offset;
		}

		[MonoTODO]
		private void FillResources(){
			NameOffsetPair pair;
			resourceNames = new ArrayList();
			resourceValues = new ArrayList();
			BinaryReader unicodeReader = 
				new BinaryReader(binaryReader.BaseStream, System.Text.Encoding.Unicode);
			// TODO: need to put these in an array and work out when to get the values.
			// also need to figure out the hash and how/if to use it.
			for (int index=0; index < resourceCount; index++){
				pair = new NameOffsetPair();
				pair.name = unicodeReader.ReadString();
				pair.offset = binaryReader.ReadInt32();
				resourceNames.Add(pair);
			}
			for (int index=0; index < resourceCount; index++){
				// LAMESPEC: what the heck is this byte here?  always 0? just a separator?
				binaryReader.ReadByte();
				resourceValues.Add(binaryReader.ReadString());
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
		
		internal class ResourceEnumerator : IDictionaryEnumerator
		{
			protected ResourceReader reader;
			protected int index = -1;

			
			internal ResourceEnumerator(ResourceReader readerToEnumerate){
				reader = readerToEnumerate;
			}

			public DictionaryEntry Entry
			{
				get {
					if (null == reader.stream)
						throw new InvalidOperationException("ResourceReader is closed.");
					if (index < 0)
						throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");

					DictionaryEntry entry = new DictionaryEntry();
					entry.Key = Key;
					entry.Value = Value;
					return entry; 
				}
			}
			
			public object Key
			{
				get { 
					if (null == reader.stream)
						throw new InvalidOperationException("ResourceReader is closed.");
					if (index < 0)
						throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
					return ((NameOffsetPair)(reader.resourceNames[index])).name; 
				}
			}
			
			public object Value
			{
				get { 
					if (null == reader.stream)
						throw new InvalidOperationException("ResourceReader is closed.");
					if (index < 0)
						throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
					return reader.resourceValues[index];
				}
			}
			
			public object Current
			{
				get {
					if (null == reader.stream)
						throw new InvalidOperationException("ResourceReader is closed.");
					if (index < 0)
						throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
					return Entry; 
				}
			}
			
			public bool MoveNext ()
			{
				if (null == reader.stream)
					throw new InvalidOperationException("ResourceReader is closed.");
				if (++index < reader.resourceCount){
					return true;
				}
				else {
					--index;
					return false;
				}
			}
			
			public void Reset () {
				if (null == reader.stream)
					throw new InvalidOperationException("ResourceReader is closed.");
				index = -1;
			}
		} // internal class ResourceEnumerator
	}  // public sealed class ResourceReader
} // namespace System.Resources
