//
// System.ComponentModel.Design.StandardCommands.cs
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
	public class StandardCommands
	{
		public static readonly CommandID AlignBottom;
		public static readonly CommandID AlignHorizontalCenters;
		public static readonly CommandID AlignLeft;
		public static readonly CommandID AlignRight;
		public static readonly CommandID AlignToGrid;
		public static readonly CommandID AlignTop;
		public static readonly CommandID AlignVerticalCenters;
		public static readonly CommandID ArrangeBottom;
		public static readonly CommandID ArrangeIcons;
		public static readonly CommandID ArrangeRight;
		public static readonly CommandID BringForward;
		public static readonly CommandID BringToFront;
		public static readonly CommandID CenterHorizontally;
		public static readonly CommandID CenterVertically;
		public static readonly CommandID Copy;
		public static readonly CommandID Cut;
		public static readonly CommandID Delete;
		public static readonly CommandID F1Help;
		public static readonly CommandID Group;
		public static readonly CommandID HorizSpaceConcatenate;
		public static readonly CommandID HorizSpaceDecrease;
		public static readonly CommandID HorizSpaceIncrease;
		public static readonly CommandID HorizSpaceMakeEqual;
		public static readonly CommandID LineupIcons;
		public static readonly CommandID LockControls;
		public static readonly CommandID MultiLevelRedo;
		public static readonly CommandID MultiLevelUndo;
		public static readonly CommandID Paste;
		public static readonly CommandID Properties;
		public static readonly CommandID PropertiesWindow;
		public static readonly CommandID Redo;
		public static readonly CommandID Replace;
		public static readonly CommandID SelectAll;
		public static readonly CommandID SendBackward;
		public static readonly CommandID SendToBack;
		public static readonly CommandID ShowGrid;
		public static readonly CommandID ShowLargeIcons;
		public static readonly CommandID SizeToControl;
		public static readonly CommandID SizeToControlHeight;
		public static readonly CommandID SizeToControlWidth;
		public static readonly CommandID SizeToFit;
		public static readonly CommandID SizeToGrid;
		public static readonly CommandID SnapToGrid;
		public static readonly CommandID TabOrder;
		public static readonly CommandID Undo;
		public static readonly CommandID Ungroup;
		public static readonly CommandID VerbFirst;
		public static readonly CommandID VerbLast;
		public static readonly CommandID VertSpaceConcatenate;
		public static readonly CommandID VertSpaceDecrease;
		public static readonly CommandID VertSpaceIncrease;
		public static readonly CommandID VertSpaceMakeEqual;
		public static readonly CommandID ViewGrid;
		public static readonly CommandID DocumentOutline;
		public static readonly CommandID ViewCode;

		static StandardCommands()
		{
			// It seems that all static commands use this Guid values in MS impl
			Guid guidA = new Guid("5efc7975-14bc-11cf-9b2b-00aa00573819");
			Guid guidB = new Guid("74d21313-2aee-11d1-8bfb-00a0c90f26f7");

			// got command IDs by looking at StandardCommands.AlignBottom.ToString in MS impl
			AlignBottom = new CommandID (guidA, 1);
			AlignHorizontalCenters = new CommandID (guidA, 2);
			AlignLeft = new CommandID (guidA, 3);
			AlignRight = new CommandID (guidA, 4);
			AlignToGrid = new CommandID (guidA, 5);
			AlignTop = new CommandID (guidA, 6);
			AlignVerticalCenters = new CommandID (guidA, 7);
			ArrangeBottom = new CommandID (guidA, 8);

			ArrangeIcons = new CommandID (guidB, 12298);

			ArrangeRight = new CommandID (guidA, 9);
			BringForward = new CommandID (guidA, 10);
			BringToFront = new CommandID (guidA, 11);
			CenterHorizontally = new CommandID (guidA, 12);
			CenterVertically = new CommandID (guidA, 13);

			Copy = new CommandID (guidA, 15);
			Cut = new CommandID (guidA, 16);
			Delete = new CommandID (guidA, 17);

			F1Help = new CommandID (guidA, 377);

			Group = new CommandID (guidA, 20);
			HorizSpaceConcatenate = new CommandID (guidA, 21);
			HorizSpaceDecrease = new CommandID (guidA, 22);
			HorizSpaceIncrease = new CommandID (guidA, 23);
			HorizSpaceMakeEqual = new CommandID (guidA, 24);

			LineupIcons = new CommandID (guidB, 12299);

			LockControls = new CommandID (guidA, 369);

			MultiLevelRedo = new CommandID (guidA, 30);

			MultiLevelUndo = new CommandID (guidA, 44);

			Paste = new CommandID (guidA, 26);
			Properties = new CommandID (guidA, 28);

			PropertiesWindow = new CommandID (guidA, 235);

			Redo = new CommandID (guidA, 29);

			Replace = new CommandID (guidA, 230);

			SelectAll = new CommandID (guidA, 31);
			SendBackward = new CommandID (guidA, 32);
			SendToBack = new CommandID (guidA, 33);

			ShowGrid = new CommandID (guidA, 103);

			ShowLargeIcons = new CommandID (guidB, 12300);

			SizeToControl = new CommandID (guidA, 35);
			SizeToControlHeight = new CommandID (guidA, 36);
			SizeToControlWidth = new CommandID (guidA, 37);
			SizeToFit = new CommandID (guidA, 38);
			SizeToGrid = new CommandID (guidA, 39);
			SnapToGrid = new CommandID (guidA, 40);
			TabOrder = new CommandID (guidA, 41);

			Undo = new CommandID (guidA, 43);

			Ungroup = new CommandID (guidA, 45);

			VerbFirst = new CommandID (guidB, 8192);
			VerbLast = new CommandID (guidB, 8448);

			VertSpaceConcatenate = new CommandID (guidA, 46);
			VertSpaceDecrease = new CommandID (guidA, 47);
			VertSpaceIncrease = new CommandID (guidA, 48);
			VertSpaceMakeEqual = new CommandID (guidA, 49);

			ViewGrid = new CommandID (guidA, 125);

			DocumentOutline = new CommandID (guidA, 239);
			ViewCode = new CommandID (guidA, 333);
		}

		public StandardCommands()
		{
			// LAMESPEC having a public constructor but only static methods
		}
	}
}
