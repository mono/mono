//
// System.Resources.ResourceReader.cs
//
// Authors: 
// 	Duncan Mak <duncan@ximian.com>
//	Nick Drochak <ndrochak@gol.com>
//	Dick Porter <dick@ximian.com>
//	Marek Safar <marek.safar@seznam.cz>
//	Atsushi Enomoto  <atsushi@ximian.com>
//      Larry Ewing <lewing@novell.com>
//
// (C) 2001, 2002 Ximian Inc, http://www.ximian.com
// Copyright (C) 2004-2005,2007-2008 Novell, Inc (http://www.novell.com)
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
using System.Resources;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Collections.Generic;

namespace System.Resources
{
	internal enum PredefinedResourceType
	{
		Null		= 0,
		String		= 1,
		Bool		= 2,
		Char		= 3,
		Byte		= 4,
		SByte		= 5,
		Int16		= 6,
		UInt16		= 7,
		Int32		= 8,
		UInt32		= 9,
		Int64		= 10,
		UInt64		= 11,
		Single		= 12,
		Double		= 13,
		Decimal		= 14,
		DateTime	= 15,
		TimeSpan	= 16,
		ByteArray	= 32,
		Stream		= 33,
		FistCustom	= 64
	}

	[System.Runtime.InteropServices.ComVisible (true)]
	public sealed class ResourceReader : IResourceReader, IEnumerable, IDisposable
	{
		struct ResourceInfo
		{
			public readonly long ValuePosition;
			public readonly string ResourceName;
			public readonly int TypeIndex;
			
			public ResourceInfo (string resourceName, long valuePosition, int type_index)
			{
				ValuePosition = valuePosition;
				ResourceName = resourceName;
				TypeIndex = type_index;
			}
		}

		struct ResourceCacheItem
		{
			public readonly string ResourceName;
			public readonly object ResourceValue;

			public ResourceCacheItem (string name, object value)
			{
				ResourceName = name;
				ResourceValue = value;
			}
		}
		
		BinaryReader reader;
		object readerLock = new object ();
		IFormatter formatter;
		internal int resourceCount = 0;
		int typeCount = 0;
		string[] typeNames;
		int[] hashes;
		ResourceInfo[] infos;
		int dataSectionOffset;
		long nameSectionOffset;
		int resource_ver;
		ResourceCacheItem[] cache;
		object cache_lock = new object ();
		
		// Constructors
		[SecurityPermission (SecurityAction.LinkDemand, SerializationFormatter = true)]
		public ResourceReader (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			
			if (!stream.CanRead)
				throw new ArgumentException ("Stream was not readable.");

			reader = new BinaryReader(stream, Encoding.UTF8);
			formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.File|StreamingContextStates.Persistence));
			
			ReadHeaders();
		}
		
		public ResourceReader (string fileName)
		{
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

				if(manager_magic != ResourceManager.MagicNumber) 
					throw new ArgumentException(String.Format ("Stream is not a valid .resources file, magic=0x{0:x}", manager_magic));

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
				resource_ver = reader.ReadInt32();

				if(resource_ver != 1 && resource_ver != 2) {
					throw new NotSupportedException("This .resources file requires unsupported set class version: " + resource_ver.ToString());
				}

				resourceCount = reader.ReadInt32();
				typeCount = reader.ReadInt32();
				
				typeNames=new string[typeCount];

				for(int i=0; i<typeCount; i++) {
					typeNames[i]=reader.ReadString();
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
				long[] positions = new long [resourceCount];
				for(int i = 0; i < resourceCount; i++)
					positions [i] = reader.ReadInt32();
				
				dataSectionOffset = reader.ReadInt32();
				nameSectionOffset = reader.BaseStream.Position;

				long origPosition = reader.BaseStream.Position;
				infos = new ResourceInfo [resourceCount];
				for (int i = 0; i < resourceCount; i++)
					CreateResourceInfo (positions [i], ref infos [i]);
				reader.BaseStream.Seek (origPosition, SeekOrigin.Begin);
				
				positions = null;
			} catch(EndOfStreamException e) {
				throw new ArgumentException("Stream is not a valid .resources file!  It was possibly truncated.", e);
			}
		}

		void CreateResourceInfo (long position, ref ResourceInfo info)
		{
			long pos = position + nameSectionOffset;

			reader.BaseStream.Seek (pos, SeekOrigin.Begin);

			// Resource name
			int len = Read7BitEncodedInt ();
			byte[] str = new byte [len];
			
			reader.Read (str, 0, len);
			string resourceName = Encoding.Unicode.GetString (str);

			long data_offset = reader.ReadInt32 () + dataSectionOffset;
			reader.BaseStream.Seek (data_offset, SeekOrigin.Begin);
			int type_index = Read7BitEncodedInt ();
			
			info = new ResourceInfo (resourceName, reader.BaseStream.Position, type_index);
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

		object ReadValueVer2 (int type_index)
		{
			switch ((PredefinedResourceType)type_index)
			{
				case PredefinedResourceType.Null:
					return null;

				case PredefinedResourceType.String:
					return reader.ReadString();

				case PredefinedResourceType.Bool:
					return reader.ReadBoolean ();

				case PredefinedResourceType.Char:
					return (char)reader.ReadUInt16();

				case PredefinedResourceType.Byte:
					return reader.ReadByte();

				case PredefinedResourceType.SByte:
					return reader.ReadSByte();

				case PredefinedResourceType.Int16:
					return reader.ReadInt16();

				case PredefinedResourceType.UInt16:
					return reader.ReadUInt16();

				case PredefinedResourceType.Int32:
					return reader.ReadInt32();

				case PredefinedResourceType.UInt32:
					return reader.ReadUInt32();

				case PredefinedResourceType.Int64:
					return reader.ReadInt64();

				case PredefinedResourceType.UInt64:
					return reader.ReadUInt64();

				case PredefinedResourceType.Single:
					return reader.ReadSingle();

				case PredefinedResourceType.Double:
					return reader.ReadDouble();

				case PredefinedResourceType.Decimal:
					return reader.ReadDecimal();

				case PredefinedResourceType.DateTime:
					return new DateTime(reader.ReadInt64());

				case PredefinedResourceType.TimeSpan:
					return new TimeSpan(reader.ReadInt64());

				case PredefinedResourceType.ByteArray:
					return reader.ReadBytes (reader.ReadInt32 ());

				case PredefinedResourceType.Stream:
					// FIXME: create pinned UnmanagedMemoryStream for efficiency.
					byte [] bytes = new byte [reader.ReadUInt32 ()];
					reader.Read (bytes, 0, bytes.Length);
					return new MemoryStream (bytes);
			}

			type_index -= (int)PredefinedResourceType.FistCustom;
			return ReadNonPredefinedValue (Type.GetType (typeNames[type_index], true));
		}

		object ReadValueVer1 (Type type)
		{
			// The most common first
			if (type==typeof(String))
				return reader.ReadString();
			if (type==typeof(Int32))
				return reader.ReadInt32();
			if (type==typeof(Byte))
				return(reader.ReadByte());
			if (type==typeof(Double))
				return(reader.ReadDouble());
			if (type==typeof(Int16))
				return(reader.ReadInt16());
			if (type==typeof(Int64))
				return(reader.ReadInt64());
			if (type==typeof(SByte))
				return(reader.ReadSByte());
			if (type==typeof(Single))
				return(reader.ReadSingle());
			if (type==typeof(TimeSpan))
				return(new TimeSpan(reader.ReadInt64()));
			if (type==typeof(UInt16))
				return(reader.ReadUInt16());
			if (type==typeof(UInt32))
				return(reader.ReadUInt32());
			if (type==typeof(UInt64))
				return(reader.ReadUInt64());
			if (type==typeof(Decimal))
				return(reader.ReadDecimal());
			if (type==typeof(DateTime))
				return(new DateTime(reader.ReadInt64()));

			return ReadNonPredefinedValue(type);
		}

		// TODO: Add security checks
		object ReadNonPredefinedValue (Type exp_type)
		{
			object obj=formatter.Deserialize(reader.BaseStream);
			if(obj.GetType() != exp_type) {
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
			return obj;
		}		

		void LoadResourceValues (ResourceCacheItem[] store)
		{
			ResourceInfo ri;
			object value;
			
			lock (readerLock) {
				for (int i = 0; i < resourceCount; i++) {
					ri = infos [i];
					if (ri.TypeIndex == -1) {
						store [i] = new ResourceCacheItem (ri.ResourceName, null);
						continue;
					}

					reader.BaseStream.Seek (ri.ValuePosition, SeekOrigin.Begin);
					if (resource_ver == 2)
						value = ReadValueVer2 (ri.TypeIndex);
					else
						value = ReadValueVer1 (Type.GetType (typeNames [ri.TypeIndex], true));

					store [i] = new ResourceCacheItem (ri.ResourceName, value);
				}
			}
		}
		
		internal UnmanagedMemoryStream ResourceValueAsStream (string name, int index)
		{
			ResourceInfo ri = infos [index];
			if ((PredefinedResourceType)ri.TypeIndex != PredefinedResourceType.Stream)
				throw new InvalidOperationException (String.Format ("Resource '{0}' was not a Stream. Use GetObject() instead.", name));
			
			lock (readerLock) {
				reader.BaseStream.Seek (ri.ValuePosition, SeekOrigin.Begin);
				
				// here we return a Stream from exactly
				// current position so that the returned
				// Stream represents a single object stream.
				long slen = reader.ReadInt32();
				UnmanagedMemoryStream basePtrStream = reader.BaseStream as UnmanagedMemoryStream;
				unsafe {
					if (basePtrStream != null) {
						return new UnmanagedMemoryStream (basePtrStream.PositionPointer, slen);
					} else {
						IntPtr ptr = Marshal.AllocHGlobal ((int) slen);
						byte* addr = (byte*) ptr.ToPointer ();
						UnmanagedMemoryStream ms = new UnmanagedMemoryStream (addr, slen, slen, FileAccess.ReadWrite);
						// The memory resource must be freed
						// when the stream is disposed.
						ms.Closed += delegate (object o, EventArgs e) {
							Marshal.FreeHGlobal (ptr);
						};

						byte [] bytes = new byte [slen < 1024 ? slen : 1024];
						while (slen > 0 ) {
							int x = reader.Read (bytes, 0, (int)Math.Min (bytes.Length, slen));

							if (x == 0)
								throw new FormatException ("The resource data is corrupt. Resource stream ended");

							ms.Write (bytes, 0, x);
							slen -= x;
						}
						ms.Seek (0, SeekOrigin.Begin);
						return ms;
					}
				}
			}
		}

		public void Close ()
		{
			Dispose(true);
		}

#if NET_4_0
		public void Dispose ()
		{
			Dispose(true);
		}
#endif
		
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

		public void GetResourceData (string resourceName, out string resourceType, out byte [] resourceData)
		{
			if (resourceName == null)
				throw new ArgumentNullException ("resourceName");
			ResourceEnumerator en = new ResourceEnumerator (this);
			while (en.MoveNext ())
				if ((string) en.Key == resourceName) {
					GetResourceDataAt (en.Index, out resourceType, out resourceData);
					return;
				}
			throw new ArgumentException (String.Format ("Specified resource not found: {0}", resourceName));
		}

		private void GetResourceDataAt (int index, out string resourceType, out byte [] data)
		{
			ResourceInfo ri = infos [index];
			int type_index = ri.TypeIndex;
			if (type_index == -1)
				throw new FormatException ("The resource data is corrupt");
			
			lock (readerLock) {
				reader.BaseStream.Seek (ri.ValuePosition, SeekOrigin.Begin);
				long pos2 = reader.BaseStream.Position;

				// Simply read data, and seek back to the original position
				if (resource_ver == 2) {
					if (type_index >= (int) PredefinedResourceType.FistCustom) {
						int typenameidx = type_index - (int)PredefinedResourceType.FistCustom;
						if (typenameidx >= typeNames.Length)
							throw new FormatException ("The resource data is corrupt. Invalid index to types");
						resourceType = typeNames[typenameidx];
					}
					else
						resourceType = "ResourceTypeCode." + (PredefinedResourceType) type_index;
					ReadValueVer2 (type_index);
				} else {
					// resource ver 1 == untyped
					resourceType = "ResourceTypeCode.Null";
					ReadValueVer1 (Type.GetType (typeNames [type_index], true));
				}

				// FIXME: the data size is wrong.
				int datalen = (int) (reader.BaseStream.Position - pos2);
				reader.BaseStream.Seek (-datalen, SeekOrigin.Current);
				data = new byte [datalen];
				reader.BaseStream.Read (data, 0, datalen);
			}
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
			infos=null;
			typeNames=null;
			cache = null;
		}
		
		internal sealed class ResourceEnumerator : IDictionaryEnumerator
		{
			private ResourceReader reader;
			private int index = -1;
			private bool finished;
			
			internal ResourceEnumerator(ResourceReader readerToEnumerate)
			{
				reader = readerToEnumerate;
				FillCache ();
			}

			public int Index
			{
				get { return index; }
			}

			public DictionaryEntry Entry {
				get {
					if (reader.reader == null)
						throw new InvalidOperationException("ResourceReader is closed.");
					if (index < 0)
						throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");

					return new DictionaryEntry(Key, Value);
				}
			}
			
			public object Key
			{
				get { 
					if (reader.reader == null)
						throw new InvalidOperationException("ResourceReader is closed.");
					if (index < 0)
						throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");

					return reader.cache [index].ResourceName;
				}
			}
			
			public object Value
			{
				get { 
					if (reader.reader == null)
						throw new InvalidOperationException("ResourceReader is closed.");
					if (index < 0)
						throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
					return reader.cache [index].ResourceValue;
				}
			}
			
			public UnmanagedMemoryStream ValueAsStream
			{
				get {
					if (reader.reader == null)
						throw new InvalidOperationException("ResourceReader is closed.");
					if (index < 0)
						throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
					return(reader.ResourceValueAsStream((string) Key, index));
				}
			}
			
			public object Current
			{
				get {
					/* Entry does the checking, no
					 * need to repeat it here
					 */
					return Entry; 
				}
			}
			
			public bool MoveNext ()
			{
				if (reader.reader == null)
					throw new InvalidOperationException("ResourceReader is closed.");
				if (finished) {
					return false;
				}
				
				if (++index < reader.resourceCount){
					return true;
				}

				finished=true;
				return false;
			}
			
			public void Reset () {
				if (reader.reader == null)
					throw new InvalidOperationException("ResourceReader is closed.");
				index = -1;
				finished = false;
			}

			void FillCache ()
			{
				if (reader.cache != null)
					return;
				
				lock (reader.cache_lock) {
					if (reader.cache != null)
						return;
					
					ResourceCacheItem[] resources = new ResourceCacheItem [reader.resourceCount];				
					reader.LoadResourceValues (resources);
					reader.cache = resources;
				}
			}
		} // internal class ResourceEnumerator
	}  // public sealed class ResourceReader
} // namespace System.Resources
