// Imported from https://github.com/kumpera/SimpleJIT/blob/77a7f3a7fcd971426bd3f6d3416eab6e42bc535b/src/SimpleJit.Metadata/MethodBody.cs
//
// MethodData.cs
//
// Author:
//   Rodrigo Kumpera  <kumpera@gmail.com>
//
//
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
using System;
using System.Collections.Generic;
using SimpleJit.CIL;
using Mono;

namespace SimpleJit.Metadata {
	
public struct IlIterator {
	byte [] body;
	int idx, op_idx, end;
	OpcodeTraits current;

	public IlIterator (byte [] body) {
		this.body = body;
		this.idx = 0;
		this.op_idx = -1;
		this.end = body.Length;
		OpcodeTraits.DecodeNext (body, idx, out current);
	}

	public IlIterator (byte [] body, int idx, int end) {
		this.body = body;
		this.idx = idx;
		this.op_idx = -1;
		this.end = end;
		if (idx < end)
			OpcodeTraits.DecodeNext (body, idx, out current);
		else
			current = default (OpcodeTraits);
	}

	public bool HasNext {
		get { return idx < end; }
	}

	public bool MoveNext () {
		if (idx >= end)
			return false;
		op_idx = idx;
		OpcodeTraits.DecodeNext (body, idx, out current);
		idx += current.Size;
		return true;
	}

	public string Mnemonic {
		get { return current.Mnemonic; }
	}

	public int Index {
		get { return op_idx; }
	}

	public int NextIndex { 
		get { return idx; }
}

	public OpcodeFlags Flags {
		get { return current.flags; }
	}

	public int DecodeParamI () {
		return current.DecodeParamI (body, op_idx);
	}

	public Opcode Opcode {
		get { return current.Opcode; }
	}
}

public class MethodBody {
	byte[] body;
	int maxStack;
	bool initLocals;
	int localsToken;
	IList<LocalVariableInfo> localInfo;

	public byte[] Body {
		get { return body; }
	}
#if FIXME_USE_SIMPLEJIT_METADATA
	// II.25.4.4
	const int FlagsFormatMask   = 0x03;
	const int FlagsFatFormat 	= 0x03;
	const int FlagsTinyFormat   = 0x02;
	const int FlagsMoreSections = 0x08;
	const int FlagsInitLocals   = 0x10;
#endif

	public IlIterator GetIterator () {
		return new IlIterator (body);
	}

#if FIXME_USE_SIMPLEJIT_METADATA
	public MethodBody (Image image, int index) {
		byte[] data = image.data;
		if ((data [index] & FlagsFormatMask) == FlagsTinyFormat) {//tiny format
			int size = data [index] >> 2;
			this.body = new byte [size];
			Array.Copy (data, index + 1, body, 0, size);
			maxStack = 8;
		} else if ((data [index] & FlagsFormatMask) == FlagsFatFormat) {
			ushort h = DataConverter.UInt16FromLE (data, index);
			index += 2;
			
			int flags = h & 0xFFF;
			int header_size = h >> 12;
			if (header_size != 3)
				throw new Exception ("Invalid fat header size " + header_size);

			this.initLocals = (h & FlagsInitLocals) == FlagsInitLocals;
			if ((flags & FlagsMoreSections) == FlagsMoreSections)
				throw new Exception ("Don't support extra sections");

			this.maxStack = DataConverter.UInt16FromLE (data, index);
			index += 2;

			int size = DataConverter.Int32FromLE (data, index);
			index += 4;

			this.localsToken = DataConverter.Int32FromLE (data, index);
			index += 4;

			this.body = new byte [size];
			Array.Copy (data, index, body, 0, size);

		} else {
			throw new Exception ("Invalid body method body format " + (data [index] & 0x3));
		}
	}
#endif // FIXME_USE_SIMPLEJIT_METADATA

	public MethodBody (byte[] body, int maxStack, bool initLocals, int localsToken, IList<LocalVariableInfo> localInfo)
	{
		this.body = body;
		this.maxStack = maxStack;
		this.initLocals = initLocals;
		this.localsToken = localsToken;
		this.localInfo = localInfo;
	}

	public override string ToString () {
		return $"method-body maxStack {maxStack} bodySize {body.Length} localsTok 0x{localsToken:X}";
	}


	public IList<LocalVariableInfo> LocalVariables { get => localInfo; }
}

}

