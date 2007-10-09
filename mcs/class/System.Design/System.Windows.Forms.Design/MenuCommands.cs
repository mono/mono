//
// System.Windows.Forms.Design.MenuCommands.cs
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
using System.ComponentModel.Design;

namespace System.Windows.Forms.Design
{
	public sealed class MenuCommands : StandardCommands
	{
		#region Public Instance Constructors

		public MenuCommands()
		{
			// LAMESPEC having a public constructor but only static methods
		}

		#endregion Public Instance Constructors

		#region Static Constructor

		static MenuCommands()
		{
			MenuCommands.wfMenuGroup = new Guid ("{74D21312-2AEE-11d1-8BFB-00A0C90F26F7}");
			MenuCommands.wfCommandSet = new Guid ("{74D21313-2AEE-11d1-8BFB-00A0C90F26F7}");
			MenuCommands.guidVSStd2K = new Guid ("{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}");
			MenuCommands.guidVSStd97 = new Guid ("{5efc7975-14bc-11cf-9b2b-00aa00573819}");

			MenuCommands.SelectionMenu = new CommandID (MenuCommands.wfMenuGroup, 1280);
			MenuCommands.ContainerMenu = new CommandID (MenuCommands.wfMenuGroup, 1281);
			MenuCommands.TraySelectionMenu = new CommandID (MenuCommands.wfMenuGroup, 1283);
			MenuCommands.ComponentTrayMenu = new CommandID (MenuCommands.wfMenuGroup, 1286);
			MenuCommands.DesignerProperties = new CommandID (MenuCommands.wfCommandSet, 4097);
			MenuCommands.KeyCancel = new CommandID (MenuCommands.guidVSStd2K, 103);
			MenuCommands.KeyReverseCancel = new CommandID (MenuCommands.wfCommandSet, 16385);
			MenuCommands.KeyDefaultAction = new CommandID (MenuCommands.guidVSStd2K, 3);
			MenuCommands.KeyMoveUp = new CommandID (MenuCommands.guidVSStd2K, 11);
			MenuCommands.KeyMoveDown = new CommandID (MenuCommands.guidVSStd2K, 13);
			MenuCommands.KeyMoveLeft = new CommandID (MenuCommands.guidVSStd2K, 7);
			MenuCommands.KeyMoveRight = new CommandID (MenuCommands.guidVSStd2K, 9);
			MenuCommands.KeyNudgeUp = new CommandID (MenuCommands.guidVSStd2K, 1227);
			MenuCommands.KeyNudgeDown = new CommandID (MenuCommands.guidVSStd2K, 1225);
			MenuCommands.KeyNudgeLeft = new CommandID (MenuCommands.guidVSStd2K, 1224);
			MenuCommands.KeyNudgeRight = new CommandID (MenuCommands.guidVSStd2K, 1226);
			MenuCommands.KeySizeWidthIncrease = new CommandID (MenuCommands.guidVSStd2K, 10);
			MenuCommands.KeySizeHeightIncrease = new CommandID (MenuCommands.guidVSStd2K, 12);
			MenuCommands.KeySizeWidthDecrease = new CommandID (MenuCommands.guidVSStd2K, 8);
			MenuCommands.KeySizeHeightDecrease = new CommandID (MenuCommands.guidVSStd2K, 14);
			MenuCommands.KeyNudgeWidthIncrease = new CommandID (MenuCommands.guidVSStd2K, 1231);
			MenuCommands.KeyNudgeHeightIncrease = new CommandID (MenuCommands.guidVSStd2K, 1228);
			MenuCommands.KeyNudgeWidthDecrease = new CommandID (MenuCommands.guidVSStd2K, 1230);
			MenuCommands.KeyNudgeHeightDecrease = new CommandID (MenuCommands.guidVSStd2K, 1229);
			MenuCommands.KeySelectNext = new CommandID (MenuCommands.guidVSStd2K, 4);
			MenuCommands.KeySelectPrevious = new CommandID (MenuCommands.guidVSStd2K, 5);
			MenuCommands.KeyTabOrderSelect = new CommandID (MenuCommands.wfCommandSet, 16405);
#if NET_2_0
			MenuCommands.KeyHome = new CommandID (MenuCommands.guidVSStd2K, 15);
			MenuCommands.KeyShiftHome = new CommandID (MenuCommands.guidVSStd2K, 16);
			MenuCommands.KeyEnd = new CommandID (MenuCommands.guidVSStd2K, 17);
			MenuCommands.KeyShiftEnd = new CommandID (MenuCommands.guidVSStd2K, 18);
			MenuCommands.KeyInvokeSmartTag = new CommandID (MenuCommands.guidVSStd2K, 147);
			MenuCommands.EditLabel = new CommandID (MenuCommands.guidVSStd97, 338);
			MenuCommands.SetStatusText = new CommandID (MenuCommands.wfCommandSet, 16387);
			MenuCommands.SetStatusRectangle = new CommandID (MenuCommands.wfCommandSet, 16388);
#endif
		}

		#endregion Static Constructor

		#region Public Static Fields

		public static readonly CommandID ComponentTrayMenu;
		public static readonly CommandID ContainerMenu;
		public static readonly CommandID DesignerProperties;
		public static readonly CommandID KeyCancel;
		public static readonly CommandID KeyDefaultAction;
		public static readonly CommandID KeyMoveDown;
		public static readonly CommandID KeyMoveLeft;
		public static readonly CommandID KeyMoveRight;
		public static readonly CommandID KeyMoveUp;
		public static readonly CommandID KeyNudgeDown;
		public static readonly CommandID KeyNudgeHeightDecrease;
		public static readonly CommandID KeyNudgeHeightIncrease;
		public static readonly CommandID KeyNudgeLeft;
		public static readonly CommandID KeyNudgeRight;
		public static readonly CommandID KeyNudgeUp;
		public static readonly CommandID KeyNudgeWidthDecrease;
		public static readonly CommandID KeyNudgeWidthIncrease;
		public static readonly CommandID KeyReverseCancel;
		public static readonly CommandID KeySelectNext;
		public static readonly CommandID KeySelectPrevious;
		public static readonly CommandID KeySizeHeightDecrease;
		public static readonly CommandID KeySizeHeightIncrease;
		public static readonly CommandID KeySizeWidthDecrease;
		public static readonly CommandID KeySizeWidthIncrease;
		public static readonly CommandID KeyTabOrderSelect;
		public static readonly CommandID SelectionMenu;
		public static readonly CommandID TraySelectionMenu;
#if NET_2_0
		public static readonly CommandID EditLabel;
		public static readonly CommandID KeyEnd;
		public static readonly CommandID KeyHome;
		public static readonly CommandID KeyInvokeSmartTag;
		public static readonly CommandID KeyShiftEnd;
		public static readonly CommandID KeyShiftHome;
		public static readonly CommandID SetStatusRectangle;
		public static readonly CommandID SetStatusText;
#endif

		#endregion Public Static Fields

		#region Private Static Fields

		private static readonly Guid guidVSStd97;
		private static readonly Guid guidVSStd2K;
		private static readonly Guid wfCommandSet;
		private static readonly Guid wfMenuGroup;

		#endregion Private Static Fields
	}
}
