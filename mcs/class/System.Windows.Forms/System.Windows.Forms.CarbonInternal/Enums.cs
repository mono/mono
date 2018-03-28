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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Geoff Norton  <gnorton@customerdna.com>
//

using System;

namespace System.Windows.Forms.CarbonInternal {
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
		kWindowLiveResizeAttribute = (1u << 28),
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
	
	internal enum MouseTrackingResult : ushort {
		kMouseTrackingMouseDown = 1,
		kMouseTrackingMouseUp = 2,
		kMouseTrackingMouseExited = 3,
		kMouseTrackingMouseEntered = 4,
		kMouseTrackingMouseDragged = 5,
		kMouseTrackingKeyModifiersChanged = 6,
		kMouseTrackingUserCancelled = 7,
		kMouseTrackingTimedOut = 8,
		kMouseTrackingMouseMoved = 9
	}

	internal enum CFStringEncoding : uint {
		kCFStringEncodingMacRoman = 0,
		kCFStringEncodingWindowsLatin1 = 0x0500,
		kCFStringEncodingISOLatin1 = 0x0201,
		kCFStringEncodingNextStepLatin = 0x0B01,
		kCFStringEncodingASCII = 0x0600,
		kCFStringEncodingUnicode = 0x0100,
		kCFStringEncodingUTF8 = 0x08000100,
		kCFStringEncodingNonLossyASCII = 0x0BFF,
		kCFStringEncodingUTF16 = 0x0100,
		kCFStringEncodingUTF16BE = 0x10000100,
		kCFStringEncodingUTF16LE = 0x14000100,
		kCFStringEncodingUTF32 = 0x0c000100,
		kCFStringEncodingUTF32BE = 0x18000100,
		kCFStringEncodingUTF32LE = 0x1c000100
	}
}
