//
// System.Resources.ResourceReader.cs
//
// Authors: 
// 	Duncan Mak <duncan@ximian.com>
//	Nick Drochak <ndrochak@gol.com>
//	Dick Porter <dick@ximian.com>
//
// (C) 2001, 2002 Ximian Inc, http://www.ximian.com
//

using System.Collections;
using System.Resources;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Resources
{
	public sealed class ResourceReader : IResourceReader, IEnumerable, IDisposable
	{
		BinaryReader reader;
		IFormatter formatter;
		internal int resourceCount = 0;
		int typeCount = 0;
		Type[] types;
		int[] hashes;
		long[] positions;
		int dataSectionOffset;
		long nameSectionOffset;
		
		// Constructors
		public ResourceReader (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("Value cannot be null.");
			
			if (!stream.CanRead)
				throw new ArgumentException ("Stream was not readable.");

			reader = new BinaryReader(stream, Encoding.UTF8);
			formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File|StreamingContextStates.Persistence));
			
			ReadHeaders();
		}
		
		public ResourceReader (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ("Path cannot be null.");

			if (!System.IO.File.Exists (fileName)) 
				throw new FileNotFoundException ("Could not find file " + Path.GetFullPath(fileName));

			reader = new BinaryReader (new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));
			formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File|StreamingContextStates.Persistence));

			ReadHeaders();
		}
		
		/* Read the ResourceManager header and the
		 * ResourceReader header.
		 */
		private void ReadHeaders()
		{
			try {
				int manager_magic = reader.ReadInt32();

				if(manager_magic != ResourceManager.MagicNumber) {
					throw new ArgumentException("Stream is not a valid .resources file!");
				}

				int manager_ver = reader.ReadInt32();
				int manager_len = reader.ReadInt32();
				
				/* We know how long the header is, even if
				 * the version number is too new
				 */
				if(manager_ver > ResourceManager.HeaderVersionNumber) {
					reader.BaseStream.Seek(manager_len, SeekOrigin.Current);
				} else {
					string reader_class=reader.ReadString();
					if(!reader_class.StartsWith("System.Resources.ResourceReader")) {
						throw new NotSupportedException("This .resources file requires reader class " + reader_class);
					}
					
					string set_class=reader.ReadString();
					if(!set_class.StartsWith(typeof(ResourceSet).FullName) && !set_class.StartsWith("System.Resources.RuntimeResourceSet")) {
						throw new NotSupportedException("This .resources file requires set class " + set_class);
					}
				}

				/* Now read the ResourceReader header */
				int reader_ver = reader.ReadInt32();

				if(reader_ver != 1) {
					throw new NotSupportedException("This .resources file requires unsupported set class version: " + reader_ver.ToString());
				}

				resourceCount = reader.ReadInt32();
				typeCount = reader.ReadInt32();
				
				types=new Type[typeCount];
				for(int i=0; i<typeCount; i++) {
					string type_name=reader.ReadString();

					/* FIXME: Should we ask for
					 * type loading exceptions
					 * here?
					 */
					types[i]=Type.GetType(type_name);
					if(types[i]==null) {
						throw new ArgumentException("Could not load type {0}", type_name);
					}
				}

				/* There are between 0 and 7 bytes of
				 * padding here, consisting of the
				 * letters PAD.  The next item (Hash
				 * values for each resource name) need
				 * to be aligned on an 8-byte
				 * boundary.
				 */

				int pad_align=(int)(reader.BaseStream.Position & 7);
				int pad_chars=0;

				if(pad_align!=0) {
					pad_chars=8-pad_align;
				}

				for(int i=0; i<pad_chars; i++) {
					byte pad_byte=reader.ReadByte();
					if(pad_byte!="PAD"[i%3]) {
						throw new ArgumentException("Malformed .resources file (padding values incorrect)");
					}
				}

				/* Read in the hash values for each
				 * resource name.  These can be used
				 * by ResourceSet (calling internal
				 * methods) to do a fast compare on
				 * resource names without doing
				 * expensive string compares (but we
				 * dont do that yet, so far we only
				 * implement the Enumerator interface)
				 */
				hashes=new int[resourceCount];
				for(int i=0; i<resourceCount; i++) {
					hashes[i]=reader.ReadInt32();
				}
				
				/* Read in the virtual offsets for
				 * each resource name
				 */
				positions=new long[resourceCount];
				for(int i=0; i<resourceCount; i++) {
					positions[i]=reader.ReadInt32();
				}
				
				dataSectionOffset = reader.ReadInt32();
				nameSectionOffset = reader.BaseStream.Position;
			} catch(EndOfStreamException e) {
				throw new ArgumentException("Stream is not a valied .resources file!  It was possibly truncated.", e);
			}
		}

		/* Cut and pasted from BinaryReader, because it's
		 * 'protected' there
		 */
		private int Read7BitEncodedInt() {
			int ret = 0;
			int shift = 0;
			byte b;

			do {
				b = reader.ReadByte();
				
				ret = ret | ((b & 0x7f) << shift);
				shift += 7;
			} while ((b & 0x80) == 0x80);

			return ret;
		}

		private string ResourceName(int index)
		{
			lock(this) 
			{
				long pos=positions[index]+nameSectionOffset;
				reader.BaseStream.Seek(pos, SeekOrigin.Begin);

				/* Read a 7-bit encoded byte length field */
				int len=Read7BitEncodedInt();
				byte[] str=new byte[len];

				reader.Read(str, 0, len);
				return Encoding.Unicode.GetString(str);
			}
		}

		private object ResourceValue(int index)
		{
			lock(this)
			{
				long pos=positions[index]+nameSectionOffset;
				reader.BaseStream.Seek(pos, SeekOrigin.Begin);

				/* Read a 7-bit encoded byte length field */
				long len=Read7BitEncodedInt();
				/* ... and skip that data to the info
				 * we want, the offset into the data
				 * section
				 */
				reader.BaseStream.Seek(len, SeekOrigin.Current);

				long data_offset=reader.ReadInt32();
				reader.BaseStream.Seek(data_offset+dataSectionOffset, SeekOrigin.Begin);
				int type_index=Read7BitEncodedInt();
				Type type=types[type_index];
				
				if (type==typeof(Byte)) {
					return(reader.ReadByte());
				/* for some reason Char is serialized */
				/*} else if (type==typeof(Char)) {
					return(reader.ReadChar());*/
				} else if (type==typeof(Decimal)) {
					return(reader.ReadDecimal());
				} else if (type==typeof(DateTime)) {
					return(new DateTime(reader.ReadInt64()));
				} else if (type==typeof(Double)) {
					return(reader.ReadDouble());
				} else if (type==typeof(Int16)) {
					return(reader.ReadInt16());
				} else if (type==typeof(Int32)) {
					return(reader.ReadInt32());
				} else if (type==typeof(Int64)) {
					return(reader.ReadInt64());
				} else if (type==typeof(SByte)) {
					return(reader.ReadSByte());
				} else if (type==typeof(Single)) {
					return(reader.ReadSingle());
				} else if (type==typeof(String)) {
					return(reader.ReadString());
				} else if (type==typeof(TimeSpan)) {
					return(new TimeSpan(reader.ReadInt64()));
				} else if (type==typeof(UInt16)) {
					return(reader.ReadUInt16());
				} else if (type==typeof(UInt32)) {
					return(reader.ReadUInt32());
				} else if (type==typeof(UInt64)) {
					return(reader.ReadUInt64());
				} else {
					/* non-intrinsic types are
					 * serialized
					 */
					object obj=formatter.Deserialize(reader.BaseStream);
					if(obj.GetType() != type) {
						/* We got a bogus
						 * object.  This
						 * exception is the
						 * best match I could
						 * find.  (.net seems
						 * to throw
						 * BadImageFormatException,
						 * which the docs
						 * state is used when
						 * file or dll images
						 * cant be loaded by
						 * the runtime.)
						 */
						throw new InvalidOperationException("Deserialized object is wrong type");
					}
					
					return(obj);
				}
			}
		}

		public void Close ()
		{
			Dispose(true);
		}
		
		public IDictionaryEnumerator GetEnumerator () {
			if (reader == null){
				throw new InvalidOperationException("ResourceReader is closed.");
			}
			else {
				return new ResourceEnumerator (this);
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((IResourceReader) this).GetEnumerator();
		}
		
		void IDisposable.Dispose ()
		{
			Dispose(true);
		}

		private void Dispose (bool disposing)
		{
			if(disposing) {
				if(reader!=null) {
					reader.Close();
				}
			}

			reader=null;
			hashes=null;
			positions=null;
			types=null;
		}
		
		internal class ResourceEnumerator : IDictionaryEnumerator
		{
			private ResourceReader reader;
			private int index = -1;
			private bool finished = false;
			
			internal ResourceEnumerator(ResourceReader readerToEnumerate){
				reader = readerToEnumerate;
			}

			public virtual DictionaryEntry Entry
			{
				get {
					if (reader.reader == null)
						throw new InvalidOperationException("ResourceReader is closed.");
					if (index < 0)
						throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");

					DictionaryEntry entry = new DictionaryEntry();
					entry.Key = Key;
					entry.Value = Value;
					return entry; 
				}
			}
			
			public virtual object Key
			{
				get { 
					if (reader.reader == null)
						throw new InvalidOperationException("ResourceReader is closed.");
					if (index < 0)
						throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
					return (reader.ResourceName(index)); 
				}
			}
			
			public virtual object Value
			{
				get { 
					if (reader.reader == null)
						throw new InvalidOperationException("ResourceReader is closed.");
					if (index < 0)
						throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
					return(reader.ResourceValue(index));
				}
			}
			
			public virtual object Current
			{
				get {
					/* Entry does the checking, no
					 * need to repeat it here
					 */
					return Entry; 
				}
			}
			
			public virtual bool MoveNext ()
			{
				if (reader.reader == null)
					throw new InvalidOperationException("ResourceReader is closed.");
				if (finished) {
					return false;
				}
				
				if (++index < reader.resourceCount){
					return true;
				}
				else {
					finished=true;
					return false;
				}
			}
			
			public void Reset () {
				if (reader.reader == null)
					throw new InvalidOperationException("ResourceReader is closed.");
				index = -1;
				finished = false;
			}
		} // internal class ResourceEnumerator
	}  // public sealed class ResourceReader
} // namespace System.Resources
