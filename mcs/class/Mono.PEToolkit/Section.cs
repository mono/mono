/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace Mono.PEToolkit {

	public class Section {

		// IMAGE_SECTION_HEADER
		protected class Header {
			internal uint  phAddr_virtSize;
			internal RVA   virtAddr;
			internal uint  rawSize;
			internal RVA   rawDataPtr;
			internal RVA   relocPtr;
			internal RVA   lineNumPtr;
			internal short relocNum;
			internal short linenumNum;
			internal SectionCharacteristics flags;
			
			public Header (BinaryReader reader)
			{
				Read (reader);
			}

			public void Read (BinaryReader reader)
			{
				phAddr_virtSize = reader.ReadUInt32 ();
				virtAddr = new RVA (reader.ReadUInt32 ());
				rawSize = reader.ReadUInt32 ();
				rawDataPtr = new RVA (reader.ReadUInt32 ());
				relocPtr = new RVA (reader.ReadUInt32 ());
				lineNumPtr = new RVA (reader.ReadUInt32 ());
				relocNum = reader.ReadInt16 ();
				linenumNum = reader.ReadInt16 ();
				flags = (SectionCharacteristics) reader.ReadUInt32 ();
			}
		}

		private string name;
		private Header hdr;

		public readonly static Section Invalid;

		static Section()
		{
			Invalid = new Section();
		}

		public Section()
		{
		}


		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}


		public uint PhysicalAddress {
			get {
				return hdr.phAddr_virtSize;
			}
			set {
				hdr.phAddr_virtSize = value;
			}
		}

		public uint VirtualSize {
			get {
				return hdr.phAddr_virtSize;
			}
			set {
				hdr.phAddr_virtSize = value;
			}
		}

		public RVA VirtualAddress {
			get {
				return hdr.virtAddr;
			}
			set {
				hdr.virtAddr = value;
			}
		}

		public uint SizeOfRawData {
			get {
				return hdr.rawSize;
			}
			set {
				hdr.rawSize = value;
			}
		}

		public RVA PointerToRawData {
			get {
				return hdr.rawDataPtr;
			}
			set {
				hdr.rawDataPtr = value;
			}
		}
		
		public RVA PointerToRelocations {
			get {
				return hdr.relocPtr;
			}
			set {
				hdr.relocPtr = value;
			}
		}
		
		public RVA PointerToLinenumbers {
			get {
				return hdr.lineNumPtr;
			}
			set {
				hdr.lineNumPtr = value;
			}
		}
		
		public short NumberOfRelocations {
			get {
				return hdr.relocNum;
			}
			set {
				hdr.relocNum = value;
			}
		}
		
		public short NumberOfLinenumbers {
			get {
				return hdr.linenumNum;
			}
			set {
				hdr.linenumNum = value;
			}
		}
		
		public SectionCharacteristics Characteristics {
			get {
				return hdr.flags;
			}
			set {
				hdr.flags = value;
			}
		}



		/// <summary>
		/// </summary>
		unsafe public void Read(BinaryReader reader)
		{
			sbyte* pName = stackalloc sbyte [8];
			sbyte* p = pName;

			for (int i = 8; --i >=0;) {
				sbyte c = reader.ReadSByte();
				if (c == 0) break;
				*p++ = c;
			}

			int len = (int) (p - pName);
			if (len != 0) {
				name = PEUtils.GetString (pName, 0, len, Encoding.ASCII);
			} else {
				name = String.Empty;
			}

			reader.BaseStream.Position += 8 - len - 1;

			hdr = new Header (reader);
		}

	}

}

