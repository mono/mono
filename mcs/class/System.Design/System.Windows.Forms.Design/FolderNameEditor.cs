//
// System.Windows.Forms.Design.FolderNameEditor.cs
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
// (C) 2004 Ximian, Inc.  http://www.ximian.com
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
using System.ComponentModel;
using System.Drawing.Design;

namespace System.Windows.Forms.Design
{
	[MonoTODO]
	public class FolderNameEditor : UITypeEditor
	{
		#region Public Instance Constructors

		public FolderNameEditor ()
		{
		}

		#endregion Public Instance Constructors

		#region Override implementation of UITypeEditor

		public override object EditValue (ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (folderBrowser == null)
			{
				folderBrowser = new FolderBrowser ();
				InitializeDialog (folderBrowser);
			}
			if (this.folderBrowser.ShowDialog () != DialogResult.OK)
			{
				return value;
			}
			return folderBrowser.DirectoryPath;
		}

		public override UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		#endregion Override implementation of UITypeEditor

		#region Protected Instance Methods

		protected virtual void InitializeDialog (FolderBrowser folderBrowser)
		{
		}

		#endregion Protected Instance Methods

		#region Private Instance Fields

		private FolderBrowser folderBrowser;

		#endregion Private Instance Fields

		protected enum FolderBrowserFolder
		{
			Desktop = 0,
			Favorites = 6,
			MyComputer = 17,
			MyDocuments = 5,
			MyPictures = 39,
			NetAndDialUpConnections = 49,
			NetworkNeighborhood = 18,
			Printers = 4,
			Recent = 8,
			SendTo = 9,
			StartMenu = 11,
			Templates = 21
		}

		[Flags]
		protected enum FolderBrowserStyles
		{
			BrowseForComputer = 4096,
			BrowseForEverything = 16384,
			BrowseForPrinter = 8192,
			RestrictToDomain = 2,
			RestrictToFilesystem = 1,
			RestrictToSubfolders = 8,
			ShowTextBox = 16
		}

		protected sealed class FolderBrowser : Component
		{
			[MonoTODO]
			public FolderBrowser ()
			{
				startLocation = FolderBrowserFolder.Desktop;
				publicOptions = FolderBrowserStyles.RestrictToFilesystem;
				descriptionText = string.Empty;
				directoryPath = string.Empty;
			}

			#region Public Instance Properties

			public string Description
			{
				get
				{
					return descriptionText;
				}
				set
				{
					descriptionText = (value == null) ? string.Empty : value;
				}
			}
			public string DirectoryPath
			{
				get
				{
					return directoryPath;
				}
			}

			public FolderBrowserFolder StartLocation
			{
				get
				{
					return startLocation;
				}
				set
				{
					startLocation = value;
				}
			}

			public FolderBrowserStyles Style
			{
				get
				{
					return publicOptions;
				}

				set
				{
					publicOptions = value;
				}
			}

			#endregion Public Instance Properties

			#region Public Instance Methods

			[MonoTODO]
			public DialogResult ShowDialog ()
			{
				return ShowDialog (null);
			}

			[MonoTODO]
			public DialogResult ShowDialog (IWin32Window owner)
			{
				throw new NotImplementedException ();
			}

			#endregion Public Instance Methods

			#region Private Instance Fields

			private string descriptionText;
			private string directoryPath;
			private FolderBrowserStyles publicOptions;
			private FolderBrowserFolder startLocation;

			#endregion Private Instance Fields
		}
	}
}
