//
// Mono.Data.Tds.Protocol.TdsPacketSubType.cs
//
// Authors:
//   Tim Coleman (tim@timcoleman.com)
//   Daniel Morgan (danielmorgan@verizon.net)
//
// Copyright (C) Tim Coleman, 2002
// Portions (C) 2003 Daniel Morgan
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

namespace Mono.Data.Tds.Protocol {
	public enum TdsPacketSubType {
		Capability = 0xe2,
		Dynamic = 0xe7,
		Dynamic2 = 0xa3,
		EnvironmentChange = 0xe3,
		Error = 0xaa,
		Info = 0xab,
		EED = 0xe5,
		Param = 0xac,
		Authentication = 0xed,
		LoginAck = 0xad,
		ReturnStatus = 0x79,
		ProcId = 0x7c,
		Done = 0xfd,
		DoneProc = 0xfe,
		DoneInProc = 0xff,
		ColumnName = 0xa0,
		ColumnInfo = 0xa1,
		ColumnDetail = 0xa5,
		AltName = 0xa7,
		AltFormat = 0xa8,
		TableName = 0xa4,
		ColumnOrder = 0xa9,
		Control = 0xae,
		Row = 0xd1,
		ColumnMetadata = 0x81,
		RowFormat = 0xee,
		ParamFormat = 0xec,
		Parameters = 0xd7
	}
}
