//
// System.Resources.ResourceWriter.cs
//
// Authors:
//	Duncan Mak <duncan@ximian.com>
//	Dick Porter <dick@ximian.com>
//
// (C) 2001, 2002 Ximian, Inc. 	http://www.ximian.com
//

using System.IO;
using System.Collections;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Resources
{
	public sealed class ResourceWriter : IResourceWriter, IDisposable
	{
		Hashtable resources;
		Stream stream;
		
		public ResourceWriter (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream is null");
			if (stream.CanWrite == false)
				throw new ArgumentException ("stream is not writable.");

			this.stream=stream;
			resources=new Hashtable(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default);
		}
		
		public ResourceWriter (String fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName is null.");

			stream=new FileStream(fileName, FileMode.Create, FileAccess.Write);
			resources=new Hashtable(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default);
		}
		
		public void AddResource (string name, byte[] value)
		{
			if (name == null) {
				throw new ArgumentNullException ("name is null");
			}
			if (value == null) {
				throw new ArgumentNullException ("value is null");
			}
			if(resources==null) {
				throw new InvalidOperationException ("ResourceWriter has been closed");
			}
			if(resources[name]!=null) {
				throw new ArgumentException ("Resource already present: " + name);
			}

			resources.Add(name, value);
		}
		
		public void AddResource (string name, object value)
		{			 
			if (name == null) {
				throw new ArgumentNullException ("name is null");
			}
			if (value == null) {
				throw new ArgumentNullException ("value is null");
			}
			if(resources==null) {
				throw new InvalidOperationException ("ResourceWriter has been closed");
			}
			if(resources[name]!=null) {
				throw new ArgumentException ("Resource already present: " + name);
			}

			resources.Add(name, value);
		}
		
		public void AddResource (string name, string value)
		{
			if (name == null) {
				throw new ArgumentNullException ("name is null");
			}
			if (value == null) {
				throw new ArgumentNullException ("value is null");
			}
			if(resources==null) {
				throw new InvalidOperationException ("ResourceWriter has been closed");
			}
			if(resources[name]!=null) {
				throw new ArgumentException ("Resource already present: " + name);
			}

			resources.Add(name, value);
		}

		public void Close () {
			Dispose(true);
		}
		
		public void Dispose ()
		{
			Dispose(true);
		}

		private void Dispose (bool disposing)
		{
			if(disposing) {
				if(resources.Count>0 && generated==false) {
					Generate();
				}
				if(stream!=null) {
					stream.Close();
				}
			}
			resources=null;
			stream=null;
		}
		
		private bool generated=false;
		
		public void Generate () {
			BinaryWriter writer;
			IFormatter formatter;

			if(resources==null) {
				throw new InvalidOperationException ("ResourceWriter has been closed");
			}

			if(generated) {
				throw new InvalidOperationException ("ResourceWriter can only Generate() once");
			}
			generated=true;
			
			writer=new BinaryWriter(stream, Encoding.UTF8);
			formatter=new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File|StreamingContextStates.Persistence));

			/* The ResourceManager header */
			
			writer.Write(ResourceManager.MagicNumber);
			writer.Write(ResourceManager.HeaderVersionNumber);
			
			/* Build the rest of the ResourceManager
			 * header in memory, because we need to know
			 * how long it is in advance
			 */
			MemoryStream resman_stream=new MemoryStream();
			BinaryWriter resman=new BinaryWriter(resman_stream,
							     Encoding.UTF8);

			resman.Write(typeof(ResourceReader).AssemblyQualifiedName);
			resman.Write(typeof(ResourceSet).AssemblyQualifiedName);

			/* Only space for 32 bits of header len in the
			 * resource file format
			 */
			int resman_len=(int)resman_stream.Length;
			writer.Write(resman_len);
			writer.Write(resman_stream.GetBuffer(), 0, resman_len);

			/* We need to build the ResourceReader name
			 * and data sections simultaneously
			 */
			MemoryStream res_name_stream=new MemoryStream();
			BinaryWriter res_name=new BinaryWriter(res_name_stream, Encoding.Unicode);

			MemoryStream res_data_stream=new MemoryStream();
			BinaryWriter res_data=new BinaryWriter(res_data_stream,
							       Encoding.UTF8);

			/* Not sure if this is the best collection to
			 * use, I just want an unordered list of
			 * objects with fast lookup, but without
			 * needing a key.  (I suppose a hashtable with
			 * key==value would work, but I need to find
			 * the index of each item later)
			 */
			ArrayList types=new ArrayList();
			int[] hashes=new int[resources.Count];
			int[] name_offsets=new int[resources.Count];
			int count=0;
			
			IDictionaryEnumerator res_enum=resources.GetEnumerator();
			while(res_enum.MoveNext()) {
				Type type=res_enum.Value.GetType();

				/* Keep a list of unique types */
				if(!types.Contains(type)) {
					types.Add(type);
				}

				/* Hash the name */
				hashes[count]=GetHash((string)res_enum.Key);

				/* Record the offsets */
				name_offsets[count]=(int)res_name.BaseStream.Position;

				/* Write the name section */
				res_name.Write((string)res_enum.Key);
				res_name.Write((int)res_data.BaseStream.Position);

				/* Write the data section */
				Write7BitEncodedInt(res_data, types.IndexOf(type));
				/* Strangely, Char is serialized
				 * rather than just written out
				 */
				if(type==typeof(Byte)) {
					res_data.Write((Byte)res_enum.Value);
				} else if (type==typeof(Decimal)) {
					res_data.Write((Decimal)res_enum.Value);
				} else if (type==typeof(DateTime)) {
					res_data.Write(((DateTime)res_enum.Value).Ticks);
				} else if (type==typeof(Double)) {
					res_data.Write((Double)res_enum.Value);
				} else if (type==typeof(Int16)) {
					res_data.Write((Int16)res_enum.Value);
				} else if (type==typeof(Int32)) {
					res_data.Write((Int32)res_enum.Value);
				} else if (type==typeof(Int64)) {
					res_data.Write((Int64)res_enum.Value);
				} else if (type==typeof(SByte)) {
					res_data.Write((SByte)res_enum.Value);
				} else if (type==typeof(Single)) {
					res_data.Write((Single)res_enum.Value);
				} else if (type==typeof(String)) {
					res_data.Write((String)res_enum.Value);
				} else if (type==typeof(TimeSpan)) {
					res_data.Write(((TimeSpan)res_enum.Value).Ticks);
				} else if (type==typeof(UInt16)) {
					res_data.Write((UInt16)res_enum.Value);
				} else if (type==typeof(UInt32)) {
					res_data.Write((UInt32)res_enum.Value);
				} else if (type==typeof(UInt64)) {
					res_data.Write((UInt64)res_enum.Value);
				} else {
					/* non-intrinsic types are
					 * serialized
					 */
					formatter.Serialize(res_data.BaseStream, res_enum.Value);
				}

				count++;
			}

			/* Sort the hashes, keep the name offsets
			 * matching up
			 */
			Array.Sort(hashes, name_offsets);
			
			/* now do the ResourceReader header */

			writer.Write(1);
			writer.Write(resources.Count);
			writer.Write(types.Count);

			/* Write all of the unique types */
			foreach(Type type in types) {
				writer.Write(type.AssemblyQualifiedName);
			}

			/* Pad the next fields (hash values) on an 8
			 * byte boundary, using the letters "PAD"
			 */
			int pad_align=(int)(writer.BaseStream.Position & 7);
			int pad_chars=0;

			if(pad_align!=0) {
				pad_chars=8-pad_align;
			}

			for(int i=0; i<pad_chars; i++) {
				writer.Write((byte)"PAD"[i%3]);
			}

			/* Write the hashes */
			for(int i=0; i<resources.Count; i++) {
				writer.Write(hashes[i]);
			}

			/* and the name offsets */
			for(int i=0; i<resources.Count; i++) {
				writer.Write(name_offsets[i]);
			}

			/* Write the data section offset */
			int data_offset=(int)writer.BaseStream.Position +
				(int)res_name_stream.Length + 4;
			writer.Write(data_offset);

			/* The name section goes next */
			writer.Write(res_name_stream.GetBuffer(), 0,
				     (int)res_name_stream.Length);
			/* The data section is last */
			writer.Write(res_data_stream.GetBuffer(), 0,
				     (int)res_data_stream.Length);

			res_name.Close();
			res_data.Close();

			/* Don't close writer, according to the spec */
			writer.Flush();
		}

		private int GetHash(string name)
		{
			uint hash=5381;

			for(int i=0; i<name.Length; i++) {
				hash=((hash<<5)+hash)^name[i];
			}
			
			return((int)hash);
		}

		/* Cut and pasted from BinaryWriter, because it's
		 * 'protected' there.
		 */
		private void Write7BitEncodedInt(BinaryWriter writer,
						 int value)
		{
			do {
				int high = (value >> 7) & 0x01ffffff;
				byte b = (byte)(value & 0x7f);

				if (high != 0) {
					b = (byte)(b | 0x80);
				}

				writer.Write(b);
				value = high;
			} while(value != 0);
		}

		internal Stream Stream {
			get {
				return stream;
			}
		}
	}
}
