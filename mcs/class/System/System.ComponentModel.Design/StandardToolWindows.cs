//
// System.ComponentModel.Design.StandardToolWindows.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
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

namespace System.ComponentModel.Design
{
	public class StandardToolWindows
	{
		public static readonly Guid ObjectBrowser = new Guid ("970d9861-ee83-11d0-a778-00a0c91110c3");
		public static readonly Guid OutputWindow = new Guid ("34e76e81-ee4a-11d0-ae2e-00a0c90fffc3");
		public static readonly Guid ProjectExplorer = new Guid ("3ae79031-e1bc-11d0-8f78-00a0c9110057");
		public static readonly Guid PropertyBrowser = new Guid ("eefa5220-e298-11d0-8f78-00a0c9110057");
		public static readonly Guid RelatedLinks = new Guid ("66dba47c-61df-11d2-aa79-00c04f990343");
		public static readonly Guid ServerExplorer = new Guid ("74946827-37a0-11d2-a273-00c04f8ef4ff");
		public static readonly Guid TaskList = new Guid ("4a9b7e51-aa16-11d0-a8c5-00a0c921a4d2");
		public static readonly Guid Toolbox = new Guid ("b1e99781-ab81-11d0-b683-00aa00a3ee26");

		public StandardToolWindows()
		{
		}
	}
}
