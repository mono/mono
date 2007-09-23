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

namespace Mono.Mozilla
{
	internal interface ICallback
	{
		void OnWidgetLoaded				();

		void GetControlSize				(ref SizeInfo sz);
		void OnJSStatus					();
		void OnLinkStatus				();
		void OnDestroyBrowser			();
		void OnClientSizeTo				(Int32 width, Int32 height);
		void OnFocusNext				();
		void OnFocusPrev				();
		void OnTitleChanged				();
		void OnShowTooltipWindow		(string tiptext, Int32 x, Int32 y);
		void OnHideTooltipWindow		();
		void OnStateNetStart			();
		void OnStateNetStop				();
		void OnStateSpecial				(UInt32 stateFlags, Int32 status);
		void OnStateChange				(string URI, Int32 status, UInt32 stateFlags);
		void OnProgress					(Int32 currentTotalProgress, Int32 maxTotalProgress);
		void OnProgressAll				(string URI, Int32 currentTotalProgress, Int32 maxTotalProgress);
		void OnLocationChanged			();
		void OnStatusChange				(string message, Int32 status);
		void OnSecurityChange			(UInt32 state);
		void OnVisibility				(bool val);

		//Don't have to worry about marshelling bool, bool seems very constant and uses 4 bit int underneath
		bool OnClientDomKeyDown			(KeyInfo keyInfo, ModifierKeys modKey);
		bool OnClientDomKeyUp			(KeyInfo keyInfo, ModifierKeys modKey);
		bool OnClientDomKeyPress		(KeyInfo keyInfo, ModifierKeys modKey);

		bool OnClientMouseDown			(MouseInfo mouseInfo, ModifierKeys modifiers);
		bool OnClientMouseUp			(MouseInfo mouseInfo, ModifierKeys modifiers);
		bool OnClientMouseClick			(MouseInfo mouseInfo, ModifierKeys modifiers);
		bool OnClientMouseDoubleClick	(MouseInfo mouseInfo, ModifierKeys modifiers);
		bool OnClientMouseOver			(MouseInfo mouseInfo, ModifierKeys modifiers);
		bool OnClientMouseOut			(MouseInfo mouseInfo, ModifierKeys modifiers);

		bool OnClientActivate			();
		bool OnClientFocusIn			();
		bool OnClientFocusOut			();

		bool OnBeforeURIOpen			(string URL);
		void OnFocus					();
		bool OnCreateNewWindow			();
		
		/*
		 * Popup dialogs
		 */

		void OnAlert					(IntPtr title, IntPtr text);
		bool OnAlertCheck				(IntPtr title, IntPtr text, IntPtr chkMsg, ref bool chkState);
		bool OnConfirm					(IntPtr title, IntPtr text);
		bool OnConfirmCheck				(IntPtr title, IntPtr text, IntPtr chkMsg, ref bool chkState);

		bool OnConfirmEx				(IntPtr title, IntPtr text, Mono.WebBrowser.DialogButtonFlags flags, 
										 IntPtr title0, IntPtr title1, IntPtr title2,
										 IntPtr chkMsg, ref bool chkState, out Int32 retVal);

		bool OnPrompt					(IntPtr title, IntPtr text,
										 IntPtr chkMsg, ref bool chkState, StringBuilder retVal);

		bool OnPromptUsernameAndPassword (IntPtr title, IntPtr text,
										 IntPtr chkMsg, ref bool chkState, 
										 out IntPtr username, out IntPtr password);

		bool OnPromptPassword			(IntPtr title, IntPtr text,
										 IntPtr chkMsg, ref bool chkState, 
										 out IntPtr password);

		bool OnSelect					(IntPtr title, IntPtr text, 
										 UInt32 count, IntPtr list, 
										 out Int32 retVal);


		/*
		 * Generic logging event
		 */
		void OnGeneric					(IntPtr type);

	}

	[StructLayout (LayoutKind.Sequential)]
	public struct SizeInfo
	{
		public UInt32 width;
		public UInt32 height;
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct ModifierKeys
	{
		public Int32 altKey;
		public Int32 ctrlKey;
		public Int32 metaKey;
		public Int32 shiftKey;
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct MouseInfo
	{
		public UInt16 button;
		public Int32 clientX;
		public Int32 clientY;
		public Int32 screenX;
		public Int32 screenY;
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct KeyInfo
	{
		public UInt32 charCode;
		public UInt32 keyCode;
	}

}
