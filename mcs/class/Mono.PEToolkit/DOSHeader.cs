/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Mono.PEToolkit {

	[StructLayout(LayoutKind.Explicit)]
	public struct DOSHeader {
		// Magic number (ExeSignature.DOS).
		[FieldOffset(0*2)]  public ExeSignature magic;

		// Bytes on last page of file.
		[FieldOffset(1*2)]  public short cblp;

		// Pages in file.
		[FieldOffset(2*2)]  public short cp;

		// Relocations.
		[FieldOffset(3*2)]  public short crlc;

		// Size of header in paragraphs.
		[FieldOffset(4*2)]  public short cparhdr;

		// Minimum extra paragraphs needed.
		[FieldOffset(5*2)]  public short minalloc;

		// Maximum extra paragraphs needed.
		[FieldOffset(6*2)]  public short maxalloc;

		// Initial (relative) SS value.
		[FieldOffset(7*2)]  public short ss;

		// Initial SP value.
		[FieldOffset(8*2)]  public short sp;

		// Checksum.
		[FieldOffset(9*2)]  public short csum;

		// Initial IP value.
		[FieldOffset(10*2)] public short ip;

		// Initial (relative) CS value.
		[FieldOffset(11*2)] public short cs;

		// File address of relocation table.
		[FieldOffset(12*2)] public short lfarlc;

		// Overlay number.
		[FieldOffset(13*2)] public short ovno;

		// Reserved words.
		// short[4] res;

		// OEM identifier (for e_oeminfo).
		[FieldOffset(18*2)] public short oemid;

		// OEM information; e_oemid specific.
		[FieldOffset(19*2)] public short oeminfo;

		// Reserved words
		// short[10] res2;

		// File address of new exe header.
		[FieldOffset(30*2)] public uint lfanew;



		/// <summary>
		/// </summary>
		unsafe public void Read(BinaryReader reader)
		{
			fixed (void* pThis = &this) {
				PEUtils.ReadStruct(reader, pThis, sizeof (DOSHeader), typeof (DOSHeader));
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="writer"></param>
		public void Dump(TextWriter writer)
		{
			writer.WriteLine(
				"Number of pages     : {0}" + Environment.NewLine +
				"Bytes on last pages : {1}" + Environment.NewLine +
				"New header offset   : {2}" + Environment.NewLine +
				"Initial CS:IP       : {3}:{4}" + Environment.NewLine +
				"Initial SS:SP       : {5}:{6}" + Environment.NewLine +
				"Overlay number      : {7}" + Environment.NewLine,
				cp, cblp,
				lfanew + " (0x" + lfanew.ToString("X") + ")",
				cs.ToString("X"), ip.ToString("X"),
				ss.ToString("X"), sp.ToString("X"),
				ovno
			);
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}

}

