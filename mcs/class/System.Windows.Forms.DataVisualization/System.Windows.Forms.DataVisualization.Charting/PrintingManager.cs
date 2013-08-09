// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//
// (C) Francis Fisher 2013
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

using System.Drawing;
using System.Drawing.Printing;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class PrintingManager : IDisposable
	{
		public PrintDocument PrintDocument { get; set; }

		[MonoTODO]
		public void Dispose ()
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected virtual void Dispose (bool disposing)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void PageSetup ()
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void Print (bool showPrintDialog)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void PrintPaint (Graphics graphics,Rectangle position)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void PrintPreview ()
		{
			throw new NotImplementedException ();
		}
	}
}
