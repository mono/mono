/*
  Copyright (C) 2008, 2009 Jeroen Frijters

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
using System.Runtime.InteropServices;
using System.Diagnostics.SymbolStore;
using IKVM.Reflection.Emit;

namespace IKVM.Reflection.Impl
{
	[StructLayout(LayoutKind.Sequential)]
	struct IMAGE_DEBUG_DIRECTORY
	{
		public uint Characteristics;
		public uint TimeDateStamp;
		public ushort MajorVersion;
		public ushort MinorVersion;
		public uint Type;
		public uint SizeOfData;
		public uint AddressOfRawData;
		public uint PointerToRawData;
	}

	interface ISymbolWriterImpl : ISymbolWriter
	{
		byte[] GetDebugInfo(ref IMAGE_DEBUG_DIRECTORY idd);
		void RemapToken(int oldToken, int newToken);
		void DefineLocalVariable2(string name, FieldAttributes attributes, int signature, SymAddressKind addrKind, int addr1, int addr2, int addr3, int startOffset, int endOffset);
	}

	static class SymbolSupport
	{
		private static readonly bool runningOnMono = System.Type.GetType("Mono.Runtime") != null;

		internal static ISymbolWriterImpl CreateSymbolWriterFor(ModuleBuilder moduleBuilder)
		{
#if !NO_SYMBOL_WRITER
			throw new NotSupportedException ("IKVM.Reflection with no symbol writer support");
#else
			if (runningOnMono)
			{
#if MONO
				return new MdbWriter(moduleBuilder);
#else
				throw new NotSupportedException("IKVM.Reflection must be compiled with MONO defined to support writing Mono debugging symbols.");
#endif
			}
			else
			{
				return new PdbWriter(moduleBuilder);
			}
#endif
		}

		internal static byte[] GetDebugInfo(ISymbolWriterImpl writer, ref IMAGE_DEBUG_DIRECTORY idd)
		{
			return writer.GetDebugInfo(ref idd);
		}

		internal static void RemapToken(ISymbolWriterImpl writer, int oldToken, int newToken)
		{
			writer.RemapToken(oldToken, newToken);
		}
	}
}
