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
//	Peter Bartok	pbartok@novell.com
//
//

// COMPLETE

namespace System.Windows.Forms {
	public enum AccessibleEvents {
		SystemSound			= 1,
		SystemAlert			= 2,
		SystemForeground		= 3,
		SystemMenuStart			= 4,
		SystemMenuEnd			= 5,
		SystemMenuPopupStart		= 6,
		SystemMenuPopupEnd		= 7,
		SystemCaptureStart		= 8,
		SystemCaptureEnd		= 9,
		SystemMoveSizeStart		= 10,
		SystemMoveSizeEnd		= 11,
		SystemContextHelpStart		= 12,
		SystemContextHelpEnd		= 13,
		SystemDragDropStart		= 14,
		SystemDragDropEnd		= 15,
		SystemDialogStart		= 16,
		SystemDialogEnd			= 17,
		SystemScrollingStart		= 18,
		SystemScrollingEnd		= 19,
		SystemSwitchStart		= 20,
		SystemSwitchEnd			= 21,
		SystemMinimizeStart		= 22,
		SystemMinimizeEnd		= 23,
		Create				= 32768,
		Destroy				= 32769,
		Show				= 32770,
		Hide				= 32771,
		Reorder				= 32772,
		Focus				= 32773,
		Selection			= 32774,
		SelectionAdd			= 32775,
		SelectionRemove			= 32776,
		SelectionWithin			= 32777,
		StateChange			= 32778,
		LocationChange			= 32779,
		NameChange			= 32780,
		DescriptionChange		= 32781,
		ValueChange			= 32782,
		ParentChange			= 32783,
		HelpChange			= 32784,
		DefaultActionChange		= 32785,
		AcceleratorChange		= 32786
	}
}
