/*
  Copyright (C) 2010-2012 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection.Writer
{
	sealed class ResourceSection
	{
		private const int RT_ICON = 3;
		private const int RT_GROUP_ICON = 14;
		private const int RT_VERSION = 16;
		private const int RT_MANIFEST = 24;
		private ResourceDirectoryEntry root = new ResourceDirectoryEntry(new OrdinalOrName("root"));
		private ByteBuffer bb;
		private List<int> linkOffsets;

		internal void AddVersionInfo(ByteBuffer versionInfo)
		{
			root[new OrdinalOrName(RT_VERSION)][new OrdinalOrName(1)][new OrdinalOrName(0)].Data = versionInfo;
		}

		internal void AddIcon(byte[] iconFile)
		{
			BinaryReader br = new BinaryReader(new MemoryStream(iconFile));
			ushort idReserved = br.ReadUInt16();
			ushort idType = br.ReadUInt16();
			ushort idCount = br.ReadUInt16();
			if (idReserved != 0 || idType != 1)
			{
				throw new ArgumentException("The supplied byte array is not a valid .ico file.");
			}
			ByteBuffer group = new ByteBuffer(6 + 14 * idCount);
			group.Write(idReserved);
			group.Write(idType);
			group.Write(idCount);
			for (int i = 0; i < idCount; i++)
			{
				byte bWidth = br.ReadByte();
				byte bHeight = br.ReadByte();
				byte bColorCount = br.ReadByte();
				byte bReserved = br.ReadByte();
				ushort wPlanes = br.ReadUInt16();
				ushort wBitCount = br.ReadUInt16();
				uint dwBytesInRes = br.ReadUInt32();
				uint dwImageOffset = br.ReadUInt32();

				// we start the icon IDs at 2
				ushort id = (ushort)(2 + i);

				group.Write(bWidth);
				group.Write(bHeight);
				group.Write(bColorCount);
				group.Write(bReserved);
				group.Write(wPlanes);
				group.Write(wBitCount);
				group.Write(dwBytesInRes);
				group.Write(id);

				byte[] icon = new byte[dwBytesInRes];
				Buffer.BlockCopy(iconFile, (int)dwImageOffset, icon, 0, icon.Length);
				root[new OrdinalOrName(RT_ICON)][new OrdinalOrName(id)][new OrdinalOrName(0)].Data = ByteBuffer.Wrap(icon);
			}
			root[new OrdinalOrName(RT_GROUP_ICON)][new OrdinalOrName(32512)][new OrdinalOrName(0)].Data = group;
		}

		internal void AddManifest(byte[] manifest, ushort resourceID)
		{
			root[new OrdinalOrName(RT_MANIFEST)][new OrdinalOrName(resourceID)][new OrdinalOrName(0)].Data = ByteBuffer.Wrap(manifest);
		}

		internal void ExtractResources(byte[] buf)
		{
			ByteReader br = new ByteReader(buf, 0, buf.Length);
			while (br.Length >= 32)
			{
				br.Align(4);
				RESOURCEHEADER hdr = new RESOURCEHEADER(br);
				if (hdr.DataSize != 0)
				{
					root[hdr.TYPE][hdr.NAME][new OrdinalOrName(hdr.LanguageId)].Data = ByteBuffer.Wrap(br.ReadBytes(hdr.DataSize));
				}
			}
		}

		internal void Finish()
		{
			if (bb != null)
			{
				throw new InvalidOperationException();
			}
			bb = new ByteBuffer(1024);
			linkOffsets = new List<int>();
			root.Write(bb, linkOffsets);
			root = null;
		}

		internal int Length
		{
			get { return bb.Length; }
		}

		internal void Write(MetadataWriter mw, uint rva)
		{
			foreach (int offset in linkOffsets)
			{
				bb.Position = offset;
				bb.Write(bb.GetInt32AtCurrentPosition() + (int)rva);
			}
			mw.Write(bb);
		}
	}

	sealed class ResourceDirectoryEntry
	{
		internal readonly OrdinalOrName OrdinalOrName;
		internal ByteBuffer Data;
		private int namedEntries;
		private readonly List<ResourceDirectoryEntry> entries = new List<ResourceDirectoryEntry>();

		internal ResourceDirectoryEntry(OrdinalOrName id)
		{
			this.OrdinalOrName = id;
		}

		internal ResourceDirectoryEntry this[OrdinalOrName id]
		{
			get
			{
				foreach (ResourceDirectoryEntry entry in entries)
				{
					if (entry.OrdinalOrName.IsEqual(id))
					{
						return entry;
					}
				}
				// the entries must be sorted
				ResourceDirectoryEntry newEntry = new ResourceDirectoryEntry(id);
				if (id.Name == null)
				{
					for (int i = namedEntries; i < entries.Count; i++)
					{
						if (entries[i].OrdinalOrName.IsGreaterThan(id))
						{
							entries.Insert(i, newEntry);
							return newEntry;
						}
					}
					entries.Add(newEntry);
					return newEntry;
				}
				else
				{
					for (int i = 0; i < namedEntries; i++)
					{
						if (entries[i].OrdinalOrName.IsGreaterThan(id))
						{
							entries.Insert(i, newEntry);
							namedEntries++;
							return newEntry;
						}
					}
					entries.Insert(namedEntries++, newEntry);
					return newEntry;
				}
			}
		}

		private int DirectoryLength
		{
			get
			{
				if (Data != null)
				{
					return 16;
				}
				else
				{
					int length = 16 + entries.Count * 8;
					foreach (ResourceDirectoryEntry entry in entries)
					{
						length += entry.DirectoryLength;
					}
					return length;
				}
			}
		}

		internal void Write(ByteBuffer bb, List<int> linkOffsets)
		{
			if (entries.Count != 0)
			{
				int stringTableOffset = this.DirectoryLength;
				Dictionary<string, int> strings = new Dictionary<string, int>();
				ByteBuffer stringTable = new ByteBuffer(16);
				int offset = 16 + entries.Count * 8;
				for (int pass = 0; pass < 3; pass++)
				{
					Write(bb, pass, 0, ref offset, strings, ref stringTableOffset, stringTable);
				}
				// the pecoff spec says that the string table is between the directory entries and the data entries,
				// but the windows linker puts them after the data entries, so we do too.
				stringTable.Align(4);
				offset += stringTable.Length;
				WriteResourceDataEntries(bb, linkOffsets, ref offset);
				bb.Write(stringTable);
				WriteData(bb);
			}
		}

		private void WriteResourceDataEntries(ByteBuffer bb, List<int> linkOffsets, ref int offset)
		{
			foreach (ResourceDirectoryEntry entry in entries)
			{
				if (entry.Data != null)
				{
					linkOffsets.Add(bb.Position);
					bb.Write(offset);
					bb.Write(entry.Data.Length);
					bb.Write(0);	// code page
					bb.Write(0);	// reserved
					offset += (entry.Data.Length + 3) & ~3;
				}
				else
				{
					entry.WriteResourceDataEntries(bb, linkOffsets, ref offset);
				}
			}
		}

		private void WriteData(ByteBuffer bb)
		{
			foreach (ResourceDirectoryEntry entry in entries)
			{
				if (entry.Data != null)
				{
					bb.Write(entry.Data);
					bb.Align(4);
				}
				else
				{
					entry.WriteData(bb);
				}
			}
		}

		private void Write(ByteBuffer bb, int writeDepth, int currentDepth, ref int offset, Dictionary<string, int> strings, ref int stringTableOffset, ByteBuffer stringTable)
		{
			if (currentDepth == writeDepth)
			{
				// directory header
				bb.Write(0);	// Characteristics
				bb.Write(0);	// Time/Date Stamp
				bb.Write(0);	// Version (Major / Minor)
				bb.Write((ushort)namedEntries);
				bb.Write((ushort)(entries.Count - namedEntries));
			}
			foreach (ResourceDirectoryEntry entry in entries)
			{
				if (currentDepth == writeDepth)
				{
					entry.WriteEntry(bb, ref offset, strings, ref stringTableOffset, stringTable);
				}
				else
				{
					entry.Write(bb, writeDepth, currentDepth + 1, ref offset, strings, ref stringTableOffset, stringTable);
				}
			}
		}

		private void WriteEntry(ByteBuffer bb, ref int offset, Dictionary<string, int> strings, ref int stringTableOffset, ByteBuffer stringTable)
		{
			WriteNameOrOrdinal(bb, OrdinalOrName, strings, ref stringTableOffset, stringTable);
			if (Data == null)
			{
				bb.Write(0x80000000U | (uint)offset);
			}
			else
			{
				bb.Write(offset);
			}
			offset += 16 + entries.Count * 8;
		}

		private static void WriteNameOrOrdinal(ByteBuffer bb, OrdinalOrName id, Dictionary<string, int> strings, ref int stringTableOffset, ByteBuffer stringTable)
		{
			if (id.Name == null)
			{
				bb.Write((int)id.Ordinal);
			}
			else
			{
				int stringOffset;
				if (!strings.TryGetValue(id.Name, out stringOffset))
				{
					stringOffset = stringTableOffset;
					strings.Add(id.Name, stringOffset);
					stringTableOffset += id.Name.Length * 2 + 2;
					stringTable.Write((ushort)id.Name.Length);
					foreach (char c in id.Name)
					{
						stringTable.Write((short)c);
					}
				}
				bb.Write(0x80000000U | (uint)stringOffset);
			}
		}
	}

	struct OrdinalOrName
	{
		internal readonly ushort Ordinal;
		internal readonly string Name;

		internal OrdinalOrName(ushort value)
		{
			Ordinal = value;
			Name = null;
		}

		internal OrdinalOrName(string value)
		{
			Ordinal = 0xFFFF;
			Name = value;
		}

		internal bool IsGreaterThan(OrdinalOrName other)
		{
			return this.Name == null
				? this.Ordinal > other.Ordinal
				: String.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase) > 0;
		}

		internal bool IsEqual(OrdinalOrName other)
		{
			return this.Name == null
				? this.Ordinal == other.Ordinal
				: String.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase) == 0;
		}
	}

	struct RESOURCEHEADER
	{
		internal int DataSize;
		internal int HeaderSize;
		internal OrdinalOrName TYPE;
		internal OrdinalOrName NAME;
		internal int DataVersion;
		internal ushort MemoryFlags;
		internal ushort LanguageId;
		internal int Version;
		internal int Characteristics;

		internal RESOURCEHEADER(ByteReader br)
		{
			DataSize = br.ReadInt32();
			HeaderSize = br.ReadInt32();
			TYPE = ReadOrdinalOrName(br);
			NAME = ReadOrdinalOrName(br);
			br.Align(4);
			DataVersion = br.ReadInt32();
			MemoryFlags = br.ReadUInt16();
			LanguageId = br.ReadUInt16();
			Version = br.ReadInt32();
			Characteristics = br.ReadInt32();
		}

		private static OrdinalOrName ReadOrdinalOrName(ByteReader br)
		{
			char c = br.ReadChar();
			if (c == 0xFFFF)
			{
				return new OrdinalOrName(br.ReadUInt16());
			}
			else
			{
				StringBuilder sb = new StringBuilder();
				while (c != 0)
				{
					sb.Append(c);
					c = br.ReadChar();
				}
				return new OrdinalOrName(sb.ToString());
			}
		}
	}
}
