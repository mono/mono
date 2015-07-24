// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
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
//	Geoff Norton  <gnorton@novell.com>
//
//

using System;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.CarbonInternal {
	internal delegate int EventDelegate (IntPtr callref, IntPtr eventref, IntPtr user_data);

	internal class EventHandler {
		internal static EventDelegate EventHandlerDelegate = new EventDelegate (EventCallback);
		internal static XplatUICarbon Driver;

		internal const int EVENT_NOT_HANDLED = 0;
		internal const int EVENT_HANDLED = -9874;

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
		internal const uint kEventClassHIObject = 1751740258;
		
		internal static EventTypeSpec [] HIObjectEvents = new EventTypeSpec [] {
									new EventTypeSpec (kEventClassHIObject, HIObjectHandler.kEventHIObjectConstruct),
									new EventTypeSpec (kEventClassHIObject, HIObjectHandler.kEventHIObjectInitialize),
									new EventTypeSpec (kEventClassHIObject, HIObjectHandler.kEventHIObjectDestruct)
									};
		internal static EventTypeSpec [] ControlEvents = new EventTypeSpec [] {
									new EventTypeSpec (kEventClassControl, ControlHandler.kEventControlBoundsChanged), 
									new EventTypeSpec (kEventClassControl, ControlHandler.kEventControlDraw),
									new EventTypeSpec (kEventClassControl, ControlHandler.kEventControlDragEnter),
									new EventTypeSpec (kEventClassControl, ControlHandler.kEventControlDragWithin),
									new EventTypeSpec (kEventClassControl, ControlHandler.kEventControlDragLeave),
									new EventTypeSpec (kEventClassControl, ControlHandler.kEventControlDragReceive),
									new EventTypeSpec (kEventClassControl, ControlHandler.kEventControlGetFocusPart), 
									new EventTypeSpec (kEventClassControl, ControlHandler.kEventControlInitialize), 
									new EventTypeSpec (kEventClassControl, ControlHandler.kEventControlVisibilityChanged) 
									};

		internal static EventTypeSpec [] ApplicationEvents = new EventTypeSpec[] {
									new EventTypeSpec (kEventClassApplication, ApplicationHandler.kEventAppActivated),
									new EventTypeSpec (kEventClassApplication, ApplicationHandler.kEventAppDeactivated)
									};
		
		private static EventTypeSpec [] WindowEvents = new EventTypeSpec[] {
									new EventTypeSpec (kEventClassMouse, MouseHandler.kEventMouseMoved),
									new EventTypeSpec (kEventClassMouse, MouseHandler.kEventMouseDragged),
									new EventTypeSpec (kEventClassMouse, MouseHandler.kEventMouseDown),
									new EventTypeSpec (kEventClassMouse, MouseHandler.kEventMouseUp),
									new EventTypeSpec (kEventClassMouse, MouseHandler.kEventMouseWheelMoved),
									new EventTypeSpec (kEventClassMouse, MouseHandler.kEventMouseScroll),

									new EventTypeSpec (kEventClassWindow, WindowHandler.kEventWindowDeactivated),
									new EventTypeSpec (kEventClassWindow, WindowHandler.kEventWindowActivated),
									new EventTypeSpec (kEventClassWindow, WindowHandler.kEventWindowDeactivated),
									new EventTypeSpec (kEventClassWindow, WindowHandler.kEventWindowCollapsed),
									new EventTypeSpec (kEventClassWindow, WindowHandler.kEventWindowCollapsing),
									new EventTypeSpec (kEventClassWindow, WindowHandler.kEventWindowExpanded),
									new EventTypeSpec (kEventClassWindow, WindowHandler.kEventWindowExpanding),
									new EventTypeSpec (kEventClassWindow, WindowHandler.kEventWindowBoundsChanged),
									new EventTypeSpec (kEventClassWindow, WindowHandler.kEventWindowResizeStarted),
									new EventTypeSpec (kEventClassWindow, WindowHandler.kEventWindowResizeCompleted),
									new EventTypeSpec (kEventClassWindow, WindowHandler.kEventWindowClose),
									new EventTypeSpec (kEventClassWindow, WindowHandler.kEventWindowShown),

									new EventTypeSpec (kEventClassKeyboard, KeyboardHandler.kEventRawKeyModifiersChanged),
									new EventTypeSpec (kEventClassKeyboard, KeyboardHandler.kEventRawKeyDown),
									new EventTypeSpec (kEventClassKeyboard, KeyboardHandler.kEventRawKeyRepeat),
									new EventTypeSpec (kEventClassKeyboard, KeyboardHandler.kEventRawKeyUp),
									new EventTypeSpec (kEventClassTextInput, KeyboardHandler.kEventTextInputUnicodeForKeyEvent)
									};

		internal static int EventCallback (IntPtr callref, IntPtr eventref, IntPtr handle) {
			uint klass = GetEventClass (eventref);
			uint kind = GetEventKind (eventref);
			MSG msg = new MSG ();
			IEventHandler handler = null;

			switch (klass) {
				case kEventClassHIObject: {
					handler = (IEventHandler) Driver.HIObjectHandler;
					break;
				}
				case kEventClassKeyboard:
				case kEventClassTextInput:
					handler = (IEventHandler) Driver.KeyboardHandler;
					break;
				case kEventClassWindow:
					handler = (IEventHandler) Driver.WindowHandler;
					break;
				case kEventClassMouse:
					handler = (IEventHandler) Driver.MouseHandler;
					break;
				case kEventClassControl:
					handler = (IEventHandler) Driver.ControlHandler;
					break;
				case kEventClassApplication:
					handler = (IEventHandler) Driver.ApplicationHandler;
					break;
				default:
					return EVENT_NOT_HANDLED;
			}

			if (handler.ProcessEvent (callref, eventref, handle, kind, ref msg)) {
				Driver.EnqueueMessage (msg);
				return EVENT_HANDLED;
			}
			
			return EVENT_NOT_HANDLED;
		}

		internal static bool TranslateMessage (ref MSG msg) {
			bool result = false;
 
			if (!result)
				result = Driver.KeyboardHandler.TranslateMessage (ref msg);
			if (!result)
				result = Driver.MouseHandler.TranslateMessage (ref msg);

 			return result;
		}

		internal static void InstallApplicationHandler () {
			InstallEventHandler (GetApplicationEventTarget (), EventHandlerDelegate, (uint)ApplicationEvents.Length, ApplicationEvents, IntPtr.Zero, IntPtr.Zero);
		}

		internal static void InstallControlHandler (IntPtr control) {
			InstallEventHandler (GetControlEventTarget (control), EventHandlerDelegate, (uint)ControlEvents.Length, ControlEvents, control, IntPtr.Zero);
		}
		
		internal static void InstallWindowHandler (IntPtr window) {
			InstallEventHandler (GetWindowEventTarget (window), EventHandlerDelegate, (uint)WindowEvents.Length, WindowEvents, window, IntPtr.Zero);
		}

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern IntPtr GetApplicationEventTarget ();
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr GetControlEventTarget (IntPtr control);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr GetWindowEventTarget (IntPtr window);

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern uint GetEventClass (IntPtr eventref);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern uint GetEventKind (IntPtr eventref);
		
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int InstallEventHandler (IntPtr window, EventDelegate event_handler, uint count, EventTypeSpec [] types, IntPtr user_data, IntPtr handlerref);
	}
}
