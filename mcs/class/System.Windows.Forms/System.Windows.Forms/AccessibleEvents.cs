//
// System.Windows.Forms.AccessibleEvents.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {

	/// <summary>
	/// </summary>
	public enum AccessibleEvents{
		AcceleratorChange,
		Create,
		DefaultActionChange,
		DescriptionChange,
		Destroy,
		Focus,
		HelpChange,
		Hide,
		LocationChange,
		NameChange,
		ParentChange,
		Reorder,
		Selection,
		SelectionAdd,
		SelectionRemove,
		SelectionWithin,
		Show,
		StateChange,
		SystemAlert,
		SystemCaptureEnd,
		SystemCaptureStart,
		SystemContextHelpEnd,
		SystemContextHelpStart,
		SystemDialogEnd,
		SystemDialogStart,
		SystemDragDropEnd,
		SystemDragDropStart,
		SystemForeground,
		SystemMenuEnd,
		SystemMenuPopupEnd,
		SystemMenuPopupStart,
		SystemMenuStart,
		SystemMinimizeEnd,
		SystemMinimizeStart,
		SystemMoveSizeEnd,
		SystemMoveSizeStart,
		SystemScrollingEnd,
		SystemScrollingStart,
		SystemSound,
		SystemSwitchEnd,
		SystemSwitchStart,
		ValueChange
	}
}
