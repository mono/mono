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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Mono.Mozilla {

	public delegate void CallbackDelegate();
	public delegate void GetControlSizeCallbackDelegate		(ref SizeInfo sz);
	public delegate void ResizeToCallbackDelegate			(Int32 width, Int32 height);
	public delegate void ShowTooltipCallbackDelegate		(Int32 x, Int32 y, string tiptext);
	public delegate void StateSpecialCallbackDelegate		(UInt32 flags, Int32 status);
	public delegate void StateChangeCallbackDelegate		(string URI, UInt32 flags, Int32 status);
	public delegate void ProgressCallbackDelegate			(Int32 currentTotalProgess, Int32 maxTotalProgress);
	public delegate void ProgressAllCallbackDelegate		(string URI, Int32 currentTotalProgess, Int32 maxTotalProgress);
	public delegate void StatusChangeCallbackDelegate		(Int32 status, string message);
	public delegate void SecurityChangeCallbackDelegate		(UInt32 state);
	public delegate void VisibilityCallbackDelegate			(bool val);
	
	//Don't have to worry about marshelling bool, PRBool seems very constant and uses 4 bit int underneath
	public delegate bool DomKeyCallbackDelegate				(KeyInfo keyInfo, ModifierKeys modifiers);
	public delegate bool MouseCallbackDelegate				(MouseInfo mouseInfo, ModifierKeys modifiers);
	public delegate bool FocusCallbackDelegate				(Int32 detail);
	public delegate bool BeforeUriOpenCallbackDelegate		(string URI);
	public delegate bool CreateNewWindowCallbackDelegate ();

	public delegate void GenericCallbackDelegate (IntPtr type);
	
	
	[StructLayout (LayoutKind.Sequential)]
	public struct CallbackBinder {

		public CallbackDelegate					EventOnWidgetLoaded;
		public CallbackDelegate 				EventJSStatus;
		public CallbackDelegate 				EventLinkStatus;
		public CallbackDelegate 				EventDestoryBrowser;
		public ResizeToCallbackDelegate 		EventSizeTo;
		public CallbackDelegate 				EventFocusNext;
		public CallbackDelegate 				EventFocusPrev;
		public CallbackDelegate 				EventTitleChanged;
		public ShowTooltipCallbackDelegate		EventShowTooltipWindow;
		public CallbackDelegate 				EventHideTooltipWindow;
		public CallbackDelegate 				EventStateNetStart;
		public CallbackDelegate 				EventStateNetStop;
		public StateSpecialCallbackDelegate		EventStateSpecial;
		public StateChangeCallbackDelegate		EventStateChange;
		public ProgressCallbackDelegate			EventProgress;
		public ProgressAllCallbackDelegate		EventProgressAll;
		public CallbackDelegate					EventLocationChanged;
		public StatusChangeCallbackDelegate		EventStatusChange;
		public SecurityChangeCallbackDelegate	EventSecurityChange;
		public VisibilityCallbackDelegate		EventVisibility;
		public GetControlSizeCallbackDelegate 	GetControlSize;
		public DomKeyCallbackDelegate			EventDomKeyDown;
		public DomKeyCallbackDelegate			EventDomKeyUp;
		public DomKeyCallbackDelegate			EventDomKeyPress;
		public MouseCallbackDelegate			EventMouseDown;
		public MouseCallbackDelegate			EventMouseUp;
		public MouseCallbackDelegate			EventMouseClick;
		public MouseCallbackDelegate			EventMouseDoubleClick;
		public MouseCallbackDelegate			EventMouseOver;
		public MouseCallbackDelegate			EventMouseOut;
		public FocusCallbackDelegate			EventActivate;
		public FocusCallbackDelegate			EventFocusIn;
		public FocusCallbackDelegate			EventFocusOut;
		public BeforeUriOpenCallbackDelegate	EventBeforeURIOpen;
		public CallbackDelegate					EventFocus;
		public CreateNewWindowCallbackDelegate EventCreateNewWindow;
	
		public GenericCallbackDelegate			EventGeneric;
		
		internal CallbackBinder (ICallback callback) {
			this.EventOnWidgetLoaded = new CallbackDelegate (callback.OnWidgetLoaded);
			this.GetControlSize = new GetControlSizeCallbackDelegate(callback.GetControlSize);
			this.EventJSStatus = new CallbackDelegate(callback.OnJSStatus);
			this.EventLinkStatus = new CallbackDelegate(callback.OnLinkStatus);
			this.EventDestoryBrowser = new CallbackDelegate(callback.OnDestroyBrowser);
			this.EventSizeTo = new ResizeToCallbackDelegate(callback.OnClientSizeTo);
			this.EventFocusNext = new CallbackDelegate(callback.OnFocusNext);
			this.EventFocusPrev = new CallbackDelegate(callback.OnFocusPrev);
			this.EventTitleChanged = new CallbackDelegate(callback.OnTitleChanged);
			this.EventShowTooltipWindow = new ShowTooltipCallbackDelegate(callback.OnShowTooltipWindow);
			this.EventHideTooltipWindow = new CallbackDelegate(callback.OnHideTooltipWindow);
			this.EventStateNetStart = new CallbackDelegate(callback.OnStateNetStart);
			this.EventStateNetStop = new CallbackDelegate(callback.OnStateNetStop);
			this.EventStateSpecial = new StateSpecialCallbackDelegate(callback.OnStateSpecial);
			this.EventStateChange =  new StateChangeCallbackDelegate(callback.OnStateChange);
			this.EventProgress = new ProgressCallbackDelegate(callback.OnProgress);
			this.EventProgressAll = new ProgressAllCallbackDelegate(callback.OnProgressAll);
			this.EventLocationChanged = new CallbackDelegate(callback.OnLocationChanged);
			this.EventStatusChange = new StatusChangeCallbackDelegate(callback.OnStatusChange);
			this.EventSecurityChange = new SecurityChangeCallbackDelegate(callback.OnSecurityChange);
			this.EventVisibility = new VisibilityCallbackDelegate(callback.OnVisibility);
			this.EventDomKeyDown = new DomKeyCallbackDelegate(callback.OnClientDomKeyDown);
			this.EventDomKeyUp = new DomKeyCallbackDelegate(callback.OnClientDomKeyUp);
			this.EventDomKeyPress = new DomKeyCallbackDelegate(callback.OnClientDomKeyPress);
			this.EventMouseDown = new MouseCallbackDelegate(callback.OnClientMouseDown);
			this.EventMouseUp = new MouseCallbackDelegate(callback.OnClientMouseUp);
			this.EventMouseClick = new MouseCallbackDelegate(callback.OnClientMouseClick);
			this.EventMouseDoubleClick = new MouseCallbackDelegate(callback.OnClientMouseDoubleClick);
			this.EventMouseOver = new MouseCallbackDelegate(callback.OnClientMouseOver);
			this.EventMouseOut = new MouseCallbackDelegate(callback.OnClientMouseOut);
			this.EventActivate = new FocusCallbackDelegate(callback.OnClientActivate);
			this.EventFocusIn = new FocusCallbackDelegate(callback.OnClientFocusIn);
			this.EventFocusOut = new FocusCallbackDelegate(callback.OnClientFocusOut);
			this.EventBeforeURIOpen = new BeforeUriOpenCallbackDelegate(callback.OnBeforeURIOpen);
			this.EventFocus= new CallbackDelegate (callback.OnFocus);
			this.EventCreateNewWindow = new CreateNewWindowCallbackDelegate (callback.OnCreateNewWindow);
			this.EventGeneric = new GenericCallbackDelegate (callback.OnGeneric);
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct SizeInfo {
		public UInt32 width;
		public UInt32 height;
	}
	
	[StructLayout (LayoutKind.Sequential)]
	public struct ModifierKeys {
		public Int32 altKey;
		public Int32 ctrlKey;
		public Int32 metaKey;
		public Int32 shiftKey;
	}
	
	[StructLayout (LayoutKind.Sequential)]
	public struct MouseInfo {
		public UInt16 button;
		public Int32 clientX;
		public Int32 clientY;
		public Int32 screenX;
		public Int32 screenY;
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct KeyInfo {
		public UInt32 charCode;
		public UInt32 keyCode;
	}
}
