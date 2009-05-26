//
// Mono.Data.Tds.Protocol.TdsRpcProcId.cs
//
// Author:
//	 Veerapuram Varadhan  <vvaradhan@novell.com>
//
// Copyright (C) 2009 Novell Inc.,
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

namespace Mono.Data.Tds.Protocol
{
	public enum TdsRpcProcId
	{
		Cursor = 1,
		CursorOpen = 2,
		CursorPrepare = 3,
		CursorExecute = 4,
		CursorPrepExec = 5,
		CursorUnprepare = 6,
		CursorFetch = 7,
		CursorOption = 8,
		CursorClose = 9,
		ExecuteSql = 10,
		Prepare = 11,
		Execute = 12, 
		PrepExec = 13,
		PrepExecRpc = 14,
		Unprepare = 15
	}
}
