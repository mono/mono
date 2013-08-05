/******************************************************************************
* The MIT License
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
using System;
using System.ComponentModel;

namespace System.DirectoryServices
{
	public class DirectoryVirtualListView
	{
		[DefaultValue(0), DSDescription("DSBeforeCount")]
		public int BeforeCount {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue(0), DSDescription("DSAfterCount")]
		public int AfterCount {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue(0), DSDescription("DSOffset")]
		public int Offset {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue(0), DSDescription("DSTargetPercentage")]
		public int TargetPercentage {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue("")]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		[DSDescription("DSTarget")]
		public string Target {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue(0), DSDescription("DSApproximateTotal")]
		public int ApproximateTotal {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue(null), DSDescription("DSDirectoryVirtualListViewContext")]
		public DirectoryVirtualListViewContext DirectoryVirtualListViewContext {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public DirectoryVirtualListView ()
		{
		}

		public DirectoryVirtualListView (int afterCount)
		{

		}

		public DirectoryVirtualListView (int beforeCount, int afterCount, int offset)
		{
		}

		public DirectoryVirtualListView (int beforeCount, int afterCount, string target)
		{
		}

		public DirectoryVirtualListView (int beforeCount, int afterCount, int offset, DirectoryVirtualListViewContext context)
		{
		}

		public DirectoryVirtualListView (int beforeCount, int afterCount, string target, DirectoryVirtualListViewContext context)
		{
		}
	}
}
