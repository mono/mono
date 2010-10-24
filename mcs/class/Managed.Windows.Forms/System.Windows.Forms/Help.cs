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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// NOT COMPLETE

using System;
using System.Drawing;

namespace System.Windows.Forms
{
	public class Help
	{
		#region Constructor
		private Help ()
		{
		}
		#endregion

		#region Public Static Methods
		public static void ShowHelp (Control parent, string url)
		{
			ShowHelp(parent, url, HelpNavigator.TableOfContents, null);
		}

		public static void ShowHelp (Control parent, string url, HelpNavigator navigator)
		{
			ShowHelp(parent, url, navigator, null);
		}

		[MonoTODO ("Stub, does nothing")]
		public static void ShowHelp (Control parent, string url, HelpNavigator command, object parameter)
		{
		}

		public static void ShowHelp (Control parent, string url, string keyword)
		{
			if (keyword == null || keyword == String.Empty)
				ShowHelp (parent, url, HelpNavigator.TableOfContents, null);
			ShowHelp (parent, url, HelpNavigator.Topic, keyword);
		}

		public static void ShowHelpIndex (Control parent, string url)
		{
			ShowHelp (parent, url, HelpNavigator.Index, null);
		}

		[MonoTODO ("Stub, does nothing")]
		public static void ShowPopup (Control parent, string caption, Point location)
		{
		}
		#endregion	// Public Static Methods
	}
}
