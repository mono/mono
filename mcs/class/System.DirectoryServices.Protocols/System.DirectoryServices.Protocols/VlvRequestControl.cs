//
// VlvRequestControl.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
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
using System.Text;

namespace System.DirectoryServices.Protocols
{
	public class VlvRequestControl : DirectoryControl
	{
		[MonoTODO]
		public VlvRequestControl ()
			: base (null, null, false, false)
		{
			throw new NotImplementedException ("ctor-chain");
		}

		[MonoTODO]
		public VlvRequestControl (int beforeCount, int afterCount, byte [] target)
			: this ()
		{
			BeforeCount = beforeCount;
			AfterCount = afterCount;
			Target = target;
		}

		public VlvRequestControl (int beforeCount, int afterCount, int offset)
			: this ()
		{
			BeforeCount = beforeCount;
			AfterCount = afterCount;
			Offset = offset;
		}

		public VlvRequestControl (int beforeCount, int afterCount, string target)
			: this ()
		{
			BeforeCount = beforeCount;
			AfterCount = afterCount;
			Target = Encoding.ASCII.GetBytes (target);
		}

		public int AfterCount { get; set; }
		public int BeforeCount { get; set; }
		public byte [] ContextId { get; set; }
		public int EstimateCount { get; set; }
		public int Offset { get; set; }
		public byte [] Target { get; set; }

		[MonoTODO]
		public override byte [] GetValue ()
		{
			throw new NotImplementedException ();
		}
	}
}
