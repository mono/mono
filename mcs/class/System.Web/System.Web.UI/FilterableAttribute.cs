//
// System.Web.UI.FilterableAttribute
//
// Authors:
//	Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2004 Novell, Inc. (http://www.novell.com)
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
#if NET_2_0
using System;
using System.ComponentModel;

namespace System.Web.UI {
	public sealed class FilterableAttribute : Attribute, IDisposable 
	{
		private bool filterable;
		private bool dispose;

		public FilterableAttribute (bool filterable) 
		{
			this.filterable = filterable;
		}

		public static readonly FilterableAttribute Default = new FilterableAttribute (true);

		public static readonly FilterableAttribute No = new FilterableAttribute (false);

		public static readonly FilterableAttribute Yes = new FilterableAttribute (true);
		
		public bool Filterable { 
			get { return Filterable; } 
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
		
		private void Dispose (bool disposing)
		{
			if (!this.dispose)
			{
				//Do nothing
				this.dispose = true;
			}
		}
	}
}
#endif
