//
// System.Web.UI.WebControls.DataControlCommands.cs
//
// Authors:
//  Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005-2010 Novell, Inc. (http://www.novell.com)
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
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class DataControlCommands
	{
		public const string CancelCommandName = "Cancel";
		public const string DeleteCommandName = "Delete";
		public const string EditCommandName = "Edit";
		public const string FirstPageCommandArgument = "First";
		public const string InsertCommandName = "Insert";
		public const string LastPageCommandArgument = "Last";
		public const string NextPageCommandArgument = "Next";
		public const string NewCommandName = "New";
		public const string PageCommandName = "Page";
		public const string PreviousPageCommandArgument = "Prev";
		public const string SelectCommandName = "Select";
		public const string SortCommandName = "Sort";
		public const string UpdateCommandName = "Update";

		DataControlCommands ()
		{
		}
	}
}

