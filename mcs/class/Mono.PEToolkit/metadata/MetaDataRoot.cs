/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.IO;
using System.Text;
using System.Collections;

namespace Mono.PEToolkit.Metadata {

	public class MetaDataRoot {

		/// <summary>
		/// "BSJB" signature.
		/// </summary>
		public static readonly uint Sig = 0x424A5342;

		// Metadata Root header, see 23.1.1
		internal uint sig;
		internal short majVer; // currently 1
		internal short minVer; // currently 1
		internal uint reserved;
		internal int len;
		internal string verStr;
		internal short flags;
		internal short nStreams;


		// Instance data

		internal Hashtable streams;

		// file position of the first byte of the header
		internal long filePos;

		private int strIdx;
		private int guidIdx;
		private int blobIdx;

		internal Image img;


		public MetaDataRoot(Image img)
		{
			this.img = img;
		}

		public Hashtable Streams {
			get {
				// never return null
				if (streams == null) streams = new Hashtable();
				return streams;
			}
		}


		public uint Signature {
			get {
				return sig;
			}
		}

		public string Version {
			get {
				return String.Format("{0}.{1}", majVer, minVer);
			}
		}

		public string VersionString {
			get {
				return verStr;
			}
			set {
				verStr = value;
				len = value.Length;
			}
		}



		public int StringsIndexSize {
			get {
				return strIdx;
			}
		}

		public int GUIDIndexSize {
			get {
				return guidIdx;
			}
		}

		public int BlobIndexSize {
			get {
				return blobIdx;
			}
		}


		unsafe public void Read(BinaryReader reader)
		{
			filePos = reader.BaseStream.Position;
			
			sig = reader.ReadUInt32();
			if (sig != Sig) {
				throw new BadImageException("Invalid MetaData Signature.");
			}

			majVer = reader.ReadInt16();
			minVer = reader.ReadInt16();
			reserved = reader.ReadUInt32();

			// Length of version string.
			len = reader.ReadInt32();
			
			// Read version string.
			if (len != 0) {
				sbyte* pVer = stackalloc sbyte [len];
				sbyte* p = pVer;

				long pos = reader.BaseStream.Position;
				int i;
				for (i = len; --i >= 0;) {
					sbyte c = reader.ReadSByte();
					if (c == 0) break;
					*p++ = c;
				}

				verStr = PEUtils.GetString (pVer, 0, len-i-1, Encoding.UTF8);

				// Round up to dword boundary, relative to header start.
				pos += len;
				pos -= filePos;
				pos += 3;
				pos &= ~3;
				pos += filePos;

				// Advance file pointer.
				reader.BaseStream.Position = pos;
			} else {
				VersionString = String.Empty;
			}
			
			flags = reader.ReadInt16();
			nStreams = reader.ReadInt16();
			streams = new Hashtable(nStreams);

			// load all streams into memory
			for (int i = nStreams; --i >=0;) {
				MDStream s = new MDStream(this);
				s.Read(reader);
				// TODO: check for duplicated streams,
				// use Add instead of indexer.
				streams[s.Name] = s;
			}

			MDStream tabs = Streams["#~"] as MDStream;
			// Try uncompressed stream.
			if (tabs == null) tabs = Streams["#-"] as MDStream;
			if (tabs == null) throw new BadMetaDataException("Missing #~ stream.");

			TablesHeap tabsHeap = tabs.Heap as TablesHeap;
			// cache index sizes
			strIdx = tabsHeap.StringsIndexSize;
			guidIdx = tabsHeap.GUIDIndexSize;
			blobIdx = tabsHeap.BlobIndexSize;
		}

		public void Write (BinaryWriter writer)
		{
			filePos = writer.BaseStream.Position;
			
			writer.Write (Sig);
			
			writer.Write (majVer);
			writer.Write (minVer);
			writer.Write (reserved);

			// Length of version string
			writer.Write (verStr.Length);

			if (verStr.Length > 0) {
				long pos = writer.BaseStream.Position;
				for (int i=0; i<verStr.Length; i++)
					writer.Write ((sbyte) verStr[i]);

				// Round up to dword boundary, relative to header start.
				pos += verStr.Length;
				pos -= filePos;
				pos += 3;
				pos &= ~3;
				pos += filePos;
				
				// Advance file pointer
				writer.BaseStream.Position = pos;
			}
		
			writer.Write (flags);
			writer.Write (nStreams);

			// load all streams into memory
			foreach (MDStream stream in streams.Values)
				stream.Write (writer);
			
		}

		public TablesHeap TablesHeap {
			get {
				MDStream tabs = Streams["#~"] as MDStream;
				// Try uncompressed stream.
				if (tabs == null) tabs = Streams["#-"] as MDStream;
				return (tabs.Heap as TablesHeap);
			}
		}

		public MethodIL GetMethodBody(int num)
		{
			MethodIL il = null;
			if (img == null) return il;
			MDStream tabs = Streams["#~"] as MDStream;
			TablesHeap tabsHeap = tabs.Heap as TablesHeap;
			if (tabsHeap.HasMethod) {
				MDTable methods = tabsHeap[TableId.Method];
				if (methods == null) return il;
				MethodRow row = methods[num] as MethodRow;
				if (row == null) return il;
				BinaryReader reader = img.reader;
				reader.BaseStream.Position = img.RVAToVA(row.RVA);
				il = new MethodIL();
				il.Read(reader);
			}
			return il;
		}

	}
}
