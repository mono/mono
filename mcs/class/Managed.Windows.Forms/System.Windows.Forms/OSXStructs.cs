// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software",, to deal in the Software without restriction, including
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
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//	Geoff Norton  <gnorton@customerdna.com>
//


// NOT COMPLETE

using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

/// Mac OSX Version
namespace System.Windows.Forms {

	internal class OSXConstants {

		#region EventClass constants
		internal const uint kEventClassMouse = 1836021107;
		internal const uint kEventClassKeyboard = 1801812322;
		internal const uint kEventClassTextInput = 1952807028;
		internal const uint kEventClassApplication = 1634758764;
		internal const uint kEventClassAppleEvent = 1701867619;
		internal const uint kEventClassMenu = 1835363957;
		internal const uint kEventClassWindow = 2003398244;
		internal const uint kEventClassControl = 1668183148;
		internal const uint kEventClassCommand = 1668113523;
		internal const uint kEventClassTablet = 1952607348;
		internal const uint kEventClassVolume = 1987013664;
		internal const uint kEventClassAppearance = 1634758765;
		internal const uint kEventClassService = 1936028278;
		internal const uint kEventClassToolbar = 1952604530;
		internal const uint kEventClassToolbarItem = 1952606580;
		internal const uint kEventClassAccessibility = 1633903461;
		#endregion

		#region kEventClassMouse constants
		internal const uint kEventMouseDown = 1;
		internal const uint kEventMouseUp = 2;
		internal const uint kEventMouseMoved = 5;
		internal const uint kEventMouseDragged = 6;
		internal const uint kEventMouseEntered = 8;
		internal const uint kEventMouseExited = 9;
		internal const uint kEventMouseWheelMoved = 10;
		#endregion

		#region kEventClassKeyboard constants
		internal const uint kEventRawKeyDown = 1;
		internal const uint kEventRawKeyRepeat = 2;
		internal const uint kEventRawKeyUp = 3;
		internal const uint kEventRawKeyModifiersChanged = 4;
		internal const uint kEventHotKeyPressed = 5;
		internal const uint kEventHotKeyReleased = 6;
		#endregion

		#region kEventClassTextInput constants
		// TODO: We dont use these yet; fill if needed
		#endregion
		
		#region kEventClassApplication constants
		// TODO: We dont use these yet; fill if needed
		#endregion

		#region kEventClassAppleEvent constants
		// TODO: We dont use these yet; fill if needed
		#endregion

		#region kEventClassMenu constants
		// TODO: We dont use these yet; fill if needed
		#endregion

		#region kEventClassWindow constants
		internal const uint kEventWindowUpdate = 1;
		internal const uint kEventWindowDrawContent = 2;
		internal const uint kEventWindowActivated = 5;
		internal const uint kEventWindowDeactivated = 6;
		internal const uint kEventWindowGetClickActivation = 7;
		internal const uint kEventWindowShowing = 22;
		internal const uint kEventWindowHiding = 23;
		internal const uint kEventWindowShown = 24;
		internal const uint kEventWindowHidden = 25;
		internal const uint kEventWindowCollapsing = 86;
		internal const uint kEventWindowExpanding = 87;
		internal const uint kEventWindowZoomed = 76;
		internal const uint kEventWindowBoundsChanging = 26;
		internal const uint kEventWindowBoundsChanged = 27;
		internal const uint kEventWindowResizeStarted = 28;
		internal const uint kEventWindowResizeCompleted = 29;
		internal const uint kEventWindowDragStarted = 30;
		internal const uint kEventWindowDragCompleted = 31;
		internal const uint kEventWindowTransitionStarted = 88;
		internal const uint kEventWindowTransitionCompleted = 89;
		internal const uint kEventWindowClickDragRgn = 32;
		internal const uint kEventWindowClickResizeRgn = 33;
		internal const uint kEventWindowClickCollapseRgn = 34;
		internal const uint kEventWindowClickCloseRgn = 35;
		internal const uint kEventWindowClickZoomRgn = 36;
		internal const uint kEventWindowClickContentRgn = 37;
		internal const uint kEventWindowClickProxyIconRgn = 38;
		internal const uint kEventWindowClickToolbarButtonRgn = 41;
		internal const uint kEventWindowClickStructureRgn = 42;
		internal const uint kEventWindowCursorChange = 40;
		internal const uint kEventWindowCollapse = 66;
		internal const uint kEventWindowCollapsed = 67;
		internal const uint kEventWindowCollapseAll = 68;
		internal const uint kEventWindowExpand = 69;
		internal const uint kEventWindowExpanded = 70;
		internal const uint kEventWindowExpandAll = 71;
		internal const uint kEventWindowClose = 72;
		internal const uint kEventWindowClosed = 73;
		internal const uint kEventWindowCloseAll = 74;
		internal const uint kEventWindowZoom = 75;
		internal const uint kEventWindowZoomAll = 77;
		internal const uint kEventWindowContextualMenuSelect = 78;
		internal const uint kEventWindowPathSelect = 79;
		internal const uint kEventWindowGetIdealSize = 80;
		internal const uint kEventWindowGetMinimumSize = 81;
		internal const uint kEventWindowGetMaximumSize = 82;
		internal const uint kEventWindowConstrain = 83;
		internal const uint kEventWindowHandleContentClick = 85;
		internal const uint kEventWindowGetDockTileMenu = 90;
		internal const uint kEventWindowHandleActivate = 91;
		internal const uint kEventWindowHandleDeactivate = 92;
		internal const uint kEventWindowProxyBeginDrag = 128;
		internal const uint kEventWindowProxyEndDrag = 129;
		internal const uint kEventWindowToolbarSwitchMode = 150;
		internal const uint kEventWindowFocusAcquired = 200;
		internal const uint kEventWindowFocusRelinquish = 201;
		internal const uint kEventWindowFocusContent = 202;
		internal const uint kEventWindowFocusToolbar = 203;
		internal const uint kEventWindowDrawerOpening = 220;
		internal const uint kEventWindowDrawerOpened = 221;
		internal const uint kEventWindowDrawerClosing = 222;
		internal const uint kEventWindowDrawerClosed = 223;
		internal const uint kEventWindowDrawFrame = 1000;
		internal const uint kEventWindowDrawPart = 1001;
		internal const uint kEventWindowGetRegion = 1002;
		internal const uint kEventWindowHitTest = 1003;
		internal const uint kEventWindowInit = 1004;
		internal const uint kEventWindowDispose = 1005;
		internal const uint kEventWindowDragHilite = 1006;
		internal const uint kEventWindowModified = 1007;
		internal const uint kEventWindowSetupProxyDragImage = 1008;
		internal const uint kEventWindowStateChanged = 1009;
		internal const uint kEventWindowMeasureTitle = 1010;
		internal const uint kEventWindowDrawGrowBox = 1011;
		internal const uint kEventWindowGetGrowImageRegion = 1012;
		internal const uint kEventWindowPaint = 1013;
		#endregion

		#region kEventClassControl constants
		internal const uint kEventControlInitialize = 1000;
		internal const uint kEventControlDispose = 1001;
		internal const uint kEventControlGetOptimalBounds = 1003;
		internal const uint kEventControlDefInitialize = kEventControlInitialize;
		internal const uint kEventControlDefDispose = kEventControlDispose;
		internal const uint kEventControlHit = 1;
		internal const uint kEventControlSimulateHit = 2;
		internal const uint kEventControlHitTest = 3;
		internal const uint kEventControlDraw = 4;
		internal const uint kEventControlApplyBackground = 5;
		internal const uint kEventControlApplyTextColor = 6;
		internal const uint kEventControlSetFocusPart = 7;
		internal const uint kEventControlGetFocusPart = 8;
		internal const uint kEventControlActivate = 9;
		internal const uint kEventControlDeactivate = 10;
		internal const uint kEventControlSetCursor = 11;
		internal const uint kEventControlContextualMenuClick = 12;
		internal const uint kEventControlClick = 13;
		internal const uint kEventControlGetNextFocusCandidate = 14;
		internal const uint kEventControlGetAutoToggleValue = 15;
		internal const uint kEventControlInterceptSubviewClick = 16;
		internal const uint kEventControlGetClickActivation = 17;
		internal const uint kEventControlDragEnter = 18;
		internal const uint kEventControlDragWithin = 19;
		internal const uint kEventControlDragLeave = 20;
		internal const uint kEventControlDragReceive = 21;
		internal const uint kEventControlInvalidateForSizeChange = 22;
		internal const uint kEventControlTrackingAreaEntered = 23;
		internal const uint kEventControlTrackingAreaExited = 24;
		internal const uint kEventControlTrack = 51;
		internal const uint kEventControlGetScrollToHereStartPoint = 52;
		internal const uint kEventControlGetIndicatorDragConstraint = 53;
		internal const uint kEventControlIndicatorMoved = 54;
		internal const uint kEventControlGhostingFinished = 55;
		internal const uint kEventControlGetActionProcPart = 56;
		internal const uint kEventControlGetPartRegion = 101;
		internal const uint kEventControlGetPartBounds = 102;
		internal const uint kEventControlSetData = 103;
		internal const uint kEventControlGetData = 104;
		internal const uint kEventControlGetSizeConstraints= 105;
		internal const uint kEventControlGetFrameMetrics = 106;
		internal const uint kEventControlValueFieldChanged = 151;
		internal const uint kEventControlAddedSubControl = 152;
		internal const uint kEventControlRemovingSubControl = 153;
		internal const uint kEventControlBoundsChanged = 154;
		internal const uint kEventControlVisibilityChanged = 157;
		internal const uint kEventControlTitleChanged = 158;
		internal const uint kEventControlOwningWindowChanged = 159;
		internal const uint kEventControlHiliteChanged = 160;
		internal const uint kEventControlEnabledStateChanged = 161;
		internal const uint kEventControlLayoutInfoChanged = 162;
		internal const uint kEventControlArbitraryMessage = 201;
		#endregion
		
		#region kEventClassCommand constants
		// TODO: We dont use these yet; fill if needed
		#endregion

		#region kEventClassTablet constants
		// TODO: We dont use these yet; fill if needed
		#endregion

		#region kEventClassVolume constants
		// TODO: We dont use these yet; fill if needed
		#endregion

		#region kEventClassAppearance constants
		// TODO: We dont use these yet; fill if needed
		#endregion

		#region kEventClassService constants
		// TODO: We dont use these yet; fill if needed
		#endregion

		#region kEventClassToolbar constants
		// TODO: We dont use these yet; fill if needed
		#endregion

		#region kEventClassToolbarItem constants
		// TODO: We dont use these yet; fill if needed
		#endregion

		#region kEventClassAccessibility constants
		// TODO: We dont use these yet; fill if needed
		#endregion
	}

	internal enum WindowClass : uint {
		kAlertWindowClass = 1,
		kMovableAlertWindowClass = 2,
		kModalWindowClass = 3,
		kMovableModalWindowClass = 4,
		kFloatingWindowClass = 5,
		kDocumentWindowClass = 6,
		kUtilityWindowClass = 8,
		kHelpWindowClass = 10,
		kSheetWindowClass = 11,
		kToolbarWindowClass = 12,
		kPlainWindowClass = 13,
		kOverlayWindowClass = 14,
		kSheetAlertWindowClass = 15,
		kAltPlainWindowClass = 16,
		kDrawerWindowClass = 20,
		kAllWindowClasses = 0xFFFFFFFF
	}

	internal enum WindowAttributes : uint {
		kWindowNoAttributes = 0,
		kWindowCloseBoxAttribute = (1u << 0),
		kWindowHorizontalZoomAttribute = (1u << 1),
		kWindowVerticalZoomAttribute = (1u << 2),
		kWindowFullZoomAttribute = (kWindowVerticalZoomAttribute | kWindowHorizontalZoomAttribute),
		kWindowCollapseBoxAttribute = (1u << 3),
		kWindowResizableAttribute = (1u << 4),
		kWindowSideTitlebarAttribute = (1u << 5),
		kWindowToolbarButtonAttribute = (1u << 6),
		kWindowMetalAttribute = (1u << 8),
		kWindowNoUpdatesAttribute = (1u << 16),
		kWindowNoActivatesAttribute = (1u << 17),
		kWindowOpaqueForEventsAttribute = (1u << 18),
		kWindowCompositingAttribute = (1u << 19),
		kWindowNoShadowAttribute = (1u << 21),
		kWindowHideOnSuspendAttribute = (1u << 24),
		kWindowStandardHandlerAttribute = (1u << 25),
		kWindowHideOnFullScreenAttribute = (1u << 26),
		kWindowInWindowMenuAttribute = (1u << 27),
		kWindowuiveResizeAttribute = (1u << 28),
		kWindowIgnoreClicksAttribute = (1u << 29),
		kWindowNoConstrainAttribute = (1u << 31),
		kWindowStandardDocumentAttributes = (kWindowCloseBoxAttribute | kWindowFullZoomAttribute | kWindowCollapseBoxAttribute | kWindowResizableAttribute),
		kWindowStandardFloatingAttributes = (kWindowCloseBoxAttribute | kWindowCollapseBoxAttribute)
	}
	internal enum ThemeCursor : uint {
		kThemeArrowCursor = 0,
		kThemeCopyArrowCursor = 1,
		kThemeAliasArrowCursor = 2,
		kThemeContextualMenuArrowCursor = 3,
		kThemeIBeamCursor = 4,
		kThemeCrossCursor = 5,
		kThemePlusCursor = 6,
		kThemeWatchCursor = 7,
		kThemeClosedHandCursor = 8,
		kThemeOpenHandCursor = 9,
		kThemePointingHandCursor = 10,
		kThemeCountingUpHandCursor = 11,
		kThemeCountingDownHandCursor = 12,
		kThemeCountingUpAndDownHandCursor = 13,
		kThemeSpinningCursor = 14,
		kThemeResizeLeftCursor = 15,
		kThemeResizeRightCursor = 16,
		kThemeResizeLeftRightCursor = 17,
		kThemeNotAllowedCursor = 18
	}
	
	internal struct CGSize {
		public float width;
		public float height;

		public CGSize (int w, int h) {
			this.width = (float)w;
			this.height = (float)h;
		}
	}

	internal struct QDPoint {
		public short y;
		public short x;

		public QDPoint (short x, short y) {
			this.x = x;
			this.y = y;
		}
	}
	internal struct CGPoint {
		public float x;
		public float y;

		public CGPoint (int x, int y) {
			this.x = (float)x;
			this.y = (float)y;
		}
	}

	internal struct HIRect {
		public CGPoint origin;
		public CGSize size;

		public HIRect (int x, int y, int w, int h) {
			this.origin = new CGPoint (x, y);
			this.size = new CGSize (w, h);
		}
	}

	internal struct HIViewID {
		public uint type;
		public uint id;

		public HIViewID (uint type, uint id) {
			this.type = type;
			this.id = id;
		}
	}
	
	internal struct EventTypeSpec
        {
		public UInt32 eventClass;
		public UInt32 eventKind;

		public EventTypeSpec (UInt32 eventClass, UInt32 eventKind)
		{
			this.eventClass = eventClass;
			this.eventKind = eventKind;
		}
	}
	
	internal struct CarbonEvent
        {
		public IntPtr hWnd;
		public IntPtr evt;

		public CarbonEvent (IntPtr hWnd, IntPtr evt)
		{
			this.hWnd = hWnd;
			this.evt = evt;
		}
	}
	
	internal struct RGBColor
	{
		public short red;
		public short green;
		public short blue;
	}

	internal struct Rect
	{
		public short top;
		public short left;
		public short bottom;
		public short right;
	}

	internal struct OSXCaret
	{
		internal Timer timer;
		internal IntPtr hwnd;
		internal int x;
		internal int y;
		internal int width;
		internal int height;
		internal int visible;
		internal bool on;
		internal bool paused;
	}

	internal struct OSXHover {
		internal Timer timer;
		internal IntPtr hwnd;
		internal int x;
		internal int y;
		internal int interval;
	}

	internal struct CGAffineTransform
	{
		internal float a;
		internal float b;
		internal float c;
		internal float d;
		internal float tx;
		internal float ty;
	}
}	
