/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.IO;

namespace Mono.PEToolkit.Metadata {

	/// <remarks>
	/// See Partition II
	/// 24.4 Common Intermediate Language Physical Layout
	/// </remarks>
	public class MethodIL {

		public enum Format {
			// CorILMethod_TinyFormat, 24.4.2
			Tiny = 2,
			// CorILMethod_FatFormat, 24.4.3
			Fat = 3,

			// encoded in 3 bits
			Shift = 3,
			Mask = 0x7
		}

		public enum Flags {
			TinyFormat = MethodIL.Format.Tiny,
			FatFormat = MethodIL.Format.Fat,
			MoreSections = 0x8,
			InitLocals = 0x10
		}

		internal int fatFlags;
		internal int maxStack;

		internal byte[] bytecode;

		public MethodIL()
		{
			fatFlags = 0;
			maxStack = 0;
		}

		public byte [] ByteCode {
			get {
				return bytecode;
			}
		}

		public int CodeSize {
			get {
				return (bytecode != null)
				        ? bytecode.Length : 0;
			}
		}

		public int MaxStack {
			get {
				return maxStack;
			}
			set {
				maxStack = value;
			}
		}

		public bool InitLocals {
			get {
				return (fatFlags & (int)Flags.InitLocals) != 0;
			}
		}

		public bool HasMoreSections {
			get {
				return (fatFlags & (int)Flags.MoreSections) != 0;
			}
		}

		internal static bool IsMethodTiny(int flags)
		{
			return ((Format)(flags & ((int)Format.Mask >> 1)) == Format.Tiny);
		}

		public void Read(BinaryReader reader)
		{
			fatFlags = 0;
			int codeSize;
			bytecode = null;
			int data = reader.ReadByte();
			if (IsMethodTiny(data)) {
				codeSize = data >> ((int)Format.Shift - 1);
				maxStack = 0; // no locals
				bytecode = reader.ReadBytes(codeSize);
			} else {
				long headPos = reader.BaseStream.Position - 1;
				fatFlags = data | (reader.ReadByte() << 8);
				// first 12 bits are flags
				// next 4 bits is the
				// "size of this header expressed as the count
				// of 4-byte integers occupied"
				int headSize = ((fatFlags >> 12) & 0xF) << 2;
				fatFlags &= 0xFFF;
				maxStack = reader.ReadInt16();
				codeSize = reader.ReadInt32();
				int localTok = reader.ReadInt32();
				reader.BaseStream.Position = headPos + headSize;
				bytecode = reader.ReadBytes(codeSize);
			}
		}

		public virtual void Dump(TextWriter writer)
		{
			string dump = String.Format(
				"Code size    : {0:x4}" + Environment.NewLine + 
				"MaxStack     : {1:x4}" + Environment.NewLine + 
				"InitLocals   : {2}" + Environment.NewLine + 
				"MoreSections : {3}" + Environment.NewLine,
				CodeSize, MaxStack, InitLocals, HasMoreSections
			);
			writer.Write(dump);
		}

		public void DumpHexBytecode(TextWriter w)
		{
			int n = CodeSize >> 3;
			int i = 0;
			for (int x = n; --x >= 0; i += 8) {
				w.WriteLine(
					String.Format("{0:x2} {1:x2} {2:x2} {3:x2} {4:x2} {5:x2} {6:x2} {7:x2}",
						ByteCode[i], ByteCode[i + 1], ByteCode[i + 2], ByteCode[i + 3],
						ByteCode[i + 4], ByteCode[i + 5], ByteCode[i + 6], ByteCode[i + 7]
					)
				);
			}
			for (;i < CodeSize; i++) {
				w.Write("{0:x2} ", ByteCode[i]);
			}
			w.WriteLine();
		}

		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}
	}
}

