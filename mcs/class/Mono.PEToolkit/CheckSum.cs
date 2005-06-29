
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
/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

// see http://cvs.winehq.com/cvsweb/wine/dlls/imagehlp/modify.c
// starting from Revision 1.8

using System;
using System.IO;

namespace Mono.PEToolkit {

	public sealed class CheckSum {

		private CheckSum()
		{
			// Never instantiated.
		}


		public static uint Calc(string peFile)
		{
			uint res = 0;

			FileInfo pe = new FileInfo(peFile);
			if (!pe.Exists) {
				throw new Exception("CheckSum : Invalid file path.");
			}

			using (BinaryReader reader = new BinaryReader(pe.OpenRead())) {
				if (!reader.BaseStream.CanSeek) {
					throw new Exception("Can't seek.");
				}

				DOSHeader dosHdr = new DOSHeader();
				COFFHeader coffHdr = new COFFHeader();
				PEHeader peHdr = new PEHeader();

				dosHdr.Read (reader);
				reader.BaseStream.Position = dosHdr.Lfanew;
				ExeSignature peSig = (ExeSignature) reader.ReadUInt16();
				if (peSig != ExeSignature.NT) {
					throw new BadImageException("Checksum : Invalid image format, cannot find PE signature.");
				}
	
				peSig = (ExeSignature) reader.ReadUInt16();
				if (peSig != ExeSignature.NT2) {
					throw new BadImageException("Checksum : Invalid image format, cannot find PE signature.");
				}

				coffHdr.Read(reader);
				peHdr.Read(reader);

				uint oldSum = peHdr.CheckSum;
				reader.BaseStream.Position = 0;
				long len = pe.Length;
				long whole = len >> 1;
				uint sum = 0;
				uint hi, lo;
				for (long i = whole; --i >= 0;) {
					sum += reader.ReadUInt16();
					hi = sum >> 16;
					if (hi != 0) {
						sum = hi + (sum & 0xFFFF);
					}
				}
				if ((len & 1L) != 0) {
					sum += (uint) reader.ReadByte();
					hi = sum >> 16;
					if (hi != 0) {
						sum = hi + (sum & 0xFFFF);
					}
				}

				// fix low word of checksum
				lo = oldSum & 0xFFFF;
				if ((sum & 0xFFFF) >= lo) {
					sum -= lo;
				} else {
					sum = (((sum & 0xFFFF) - lo) & 0xFFFF) - 1;
				}

				// fix high word of checksum
				hi = oldSum >> 16;
				if ((sum & 0xFFFF) >= hi) {
					sum -= hi;
				} else {
					sum = (((sum & 0xFFFF) - hi) & 0xFFFF) - 1;
				}
			}

			return res;
		}
	}

}
