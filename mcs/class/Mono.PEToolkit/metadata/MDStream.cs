/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace Mono.PEToolkit.Metadata {

	/// <summary>
	/// Metadata stream.
	/// </summary>
	public class MDStream {

		/// <summary>
		/// MetaData stream header as described
		/// in ECMA CLI specs, Partition II Metadata, 23.1.2
		/// </summary>
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct Header {
			internal uint offs;
			internal uint size;
			internal string name;


			unsafe public void Read(BinaryReader reader, MDStream stream)
			{
				sbyte* pName = stackalloc sbyte [32];
				sbyte* p = pName;

				fixed (void* pThis = &this.offs) {
					PEUtils.ReadStruct(reader, pThis, 2 * sizeof (uint));
				}

				while (true) {
					sbyte c = reader.ReadSByte();
					if (c == 0) break;
					*p++ = c;
				}

				int len = (int) (p - pName);
				if (len != 0) {
					name = PEUtils.GetString (pName, 0, len, Encoding.ASCII);
				} else {
					throw new BadImageException("Invalid stream name.");
				}

				// Round up to dword boundary.
				long pos = reader.BaseStream.Position;
				if (stream != null) pos -= stream.Root.filePos;
				pos += 3;
				pos &= ~3;
				if (stream != null) pos += stream.Root.filePos;

				// Advance file pointer.
				reader.BaseStream.Position = pos;
			}
		} // header



		private MetaDataRoot root;
		private MDHeap heap;
		internal Header hdr;
		private byte [] data;


		public MDStream(MetaDataRoot root)
		{
			this.root = root;
			hdr = new Header();
			data = null;
			heap = null;
		}


		public uint Offset {
			get {
				return hdr.offs;
			}
			set {
				hdr.offs = value;
			}
		}

		public uint Size {
			get {
				return hdr.size;
			}
			set {
				hdr.size = value;
			}
		}

		/// <summary>
		/// Name of the stream.
		/// </summary>
		/// <remarks>
		/// Stored on-disk as a null-terminated ASCII string,
		/// rounded up to 4-byte boundary.
		/// </remarks>
		public string Name {
			get {
				return hdr.name;
			}
			set {
				hdr.name = value;
			}
		}

		public byte [] RawData {
			get {
				return data;
			}
		}

		public MetaDataRoot Root {
			get {
				return root;
			}
		}

		public MDHeap Heap {
			get {
				lock (this) {
					if (heap == null) InitHeap();
					return heap;
				}
			}
		}


		/// <summary>
		/// Reads stream header and body from supplied BinaryReader.
		/// </summary>
		/// <remarks>
		/// Reader must be positioned at the first byte of metadata stream.
		/// </remarks>
		/// <param name="reader"></param>
		unsafe public void Read(BinaryReader reader)
		{
			hdr.Read(reader, this);
			long oldPos = reader.BaseStream.Position;

			// Offset field in the stream header is relataive to
			// the start of metadata.
			reader.BaseStream.Position = root.filePos + hdr.offs;
			data = reader.ReadBytes((int) hdr.size);

			// set reader's position to the first byte after
			// stream header.
			reader.BaseStream.Position = oldPos;
		}


		/// <summary>
		/// Initializes heap for this stream.
		/// </summary>
		protected void InitHeap()
		{
			heap = MDHeap.Create(this);
		}


		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer) {
			string dump = String.Format(
				"Name        : {0}" + Environment.NewLine +
				"Offset      : 0x{1:x8}" + Environment.NewLine +
				"Size        : 0x{2:x8}" + Environment.NewLine,
				hdr.name, hdr.offs, hdr.size
			);
			writer.WriteLine(dump);
		}


		/// <summary>
		/// </summary>
		public override string ToString()
		{
			StringWriter sw = new StringWriter ();
			Dump(sw);
			return sw.ToString();
		}

	}
}
