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

	public delegate void CallbackDelegate ();

	public delegate void CallbackDelegateStringString	(string arg1, string arg2);
	public delegate void CallbackDelegateStringInt		(string arg1, Int32 arg2);
	public delegate void CallbackDelegateStringIntInt	(string arg1, Int32 arg2, Int32 arg3);
	public delegate void CallbackDelegateStringIntUint	(string arg1, Int32 arg2, UInt32 arg3);


	public delegate void CallbackDelegateIntInt			(Int32 arg1, Int32 arg2);

	public delegate void CallbackDelegateUint			(UInt32 arg1);
	public delegate void CallbackDelegateUintInt		(UInt32 arg1, Int32 arg2);

	public delegate void CallbackDelegatePtrPtr			(IntPtr arg1, IntPtr arg2);

	//Don't have to worry about marshelling bool, PRBool seems very constant and uses 4 bit int underneath
	public delegate void CallbackDelegateBool			(bool val);
	
	public delegate bool DomKeyCallbackDelegate			(KeyInfo keyInfo, ModifierKeys modifiers);
	public delegate bool MouseCallbackDelegate			(MouseInfo mouseInfo, ModifierKeys modifiers);

	public delegate void GetControlSizeCallbackDelegate (ref SizeInfo sz);

	public delegate void GenericCallbackDelegate		(IntPtr type);

	public delegate bool CallbackDelegate2				();
	public delegate bool CallbackDelegate2String		(string arg1);


	public delegate bool CallbackDelegate2OnAlertCheck	(IntPtr title, IntPtr text, IntPtr chkMsg, ref bool chkState);
	public delegate bool CallbackDelegate2OnConfirm		(IntPtr title, IntPtr text);
	public delegate bool CallbackDelegate2OnConfirmCheck(IntPtr title, IntPtr text, IntPtr chkMsg, ref bool chkState);
	public delegate bool CallbackDelegate2OnConfirmEx	(IntPtr title, IntPtr text, Mono.WebBrowser.DialogButtonFlags flags, 
														 IntPtr title0, IntPtr title1, IntPtr title2,
														 IntPtr chkMsg, ref bool chkState, out Int32 retVal);
	public delegate bool CallbackDelegate2OnPrompt		(IntPtr title, IntPtr text,
														 IntPtr chkMsg, ref bool chkState, StringBuilder retVal);
	public delegate bool CallbackDelegate2OnPromptUsernameAndPassword 
														(IntPtr title, IntPtr text,
														 IntPtr chkMsg, ref bool chkState, 
														 out IntPtr username, out IntPtr password);
	public delegate bool CallbackDelegate2OnPromptPassword
														(IntPtr title, IntPtr text,
														 IntPtr chkMsg, ref bool chkState, 
														 out IntPtr password);
	public delegate bool CallbackDelegate2OnSelect		(IntPtr title, IntPtr text, 
														 UInt32 count, IntPtr list, 
														 out Int32 retVal);

	
	[StructLayout (LayoutKind.Sequential)]
	public struct CallbackBinder {

		public CallbackDelegate					OnWidgetLoaded;
		public CallbackDelegate 				OnJSStatus;
		public CallbackDelegate 				OnLinkStatus;
		public CallbackDelegate 				OnDestroyBrowser;
		public CallbackDelegateIntInt			OnSizeTo;
		public CallbackDelegate 				OnFocusNext;
		public CallbackDelegate 				OnFocusPrev;
		public CallbackDelegate 				OnTitleChanged;
		public CallbackDelegateStringIntInt		OnShowTooltipWindow;
		public CallbackDelegate 				OnHideTooltipWindow;
		public CallbackDelegate 				OnStateNetStart;
		public CallbackDelegate 				OnStateNetStop;
		public CallbackDelegateUintInt			OnStateSpecial;
		public CallbackDelegateStringIntUint	OnStateChange;
		public CallbackDelegateIntInt			OnProgress;
		public CallbackDelegateStringIntInt		OnProgressAll;
		public CallbackDelegate					OnLocationChanged;
		public CallbackDelegateStringInt		OnStatusChange;
		public CallbackDelegateUint				OnSecurityChange;
		public CallbackDelegateBool				OnVisibility;
		public GetControlSizeCallbackDelegate 	GetControlSize;
		public DomKeyCallbackDelegate			OnDomKeyDown;
		public DomKeyCallbackDelegate			OnDomKeyUp;
		public DomKeyCallbackDelegate			OnDomKeyPress;
		public MouseCallbackDelegate			OnMouseDown;
		public MouseCallbackDelegate			OnMouseUp;
		public MouseCallbackDelegate			OnMouseClick;
		public MouseCallbackDelegate			OnMouseDoubleClick;
		public MouseCallbackDelegate			OnMouseOver;
		public MouseCallbackDelegate			OnMouseOut;
		public CallbackDelegate2				OnActivate;
		public CallbackDelegate2				OnFocusIn;
		public CallbackDelegate2				OnFocusOut;
		public CallbackDelegate2String			OnBeforeURIOpen;
		public CallbackDelegate					OnFocus;
		public CallbackDelegate2				OnCreateNewWindow;
		public CallbackDelegatePtrPtr			OnAlert;

		public CallbackDelegate2OnAlertCheck OnAlertCheck;
		public CallbackDelegate2OnConfirm OnConfirm;
		public CallbackDelegate2OnConfirmCheck OnConfirmCheck;
		public CallbackDelegate2OnConfirmEx OnConfirmEx;
		public CallbackDelegate2OnPrompt OnPrompt;
		public CallbackDelegate2OnPromptUsernameAndPassword OnPromptUsernameAndPassword;
		public CallbackDelegate2OnPromptPassword OnPromptPassword;
		public CallbackDelegate2OnSelect OnSelect;

		public GenericCallbackDelegate			OnGeneric;
		
		internal CallbackBinder (ICallback callback) {
			this.OnWidgetLoaded			= new CallbackDelegate (callback.OnWidgetLoaded);
			this.GetControlSize			= new GetControlSizeCallbackDelegate (callback.GetControlSize);
			this.OnJSStatus				= new CallbackDelegate (callback.OnJSStatus);
			this.OnLinkStatus			= new CallbackDelegate (callback.OnLinkStatus);
			this.OnDestroyBrowser		= new CallbackDelegate (callback.OnDestroyBrowser);
			this.OnSizeTo				= new CallbackDelegateIntInt (callback.OnClientSizeTo);
			this.OnFocusNext			= new CallbackDelegate (callback.OnFocusNext);
			this.OnFocusPrev			= new CallbackDelegate (callback.OnFocusPrev);
			this.OnTitleChanged			= new CallbackDelegate (callback.OnTitleChanged);
			this.OnShowTooltipWindow	= new CallbackDelegateStringIntInt (callback.OnShowTooltipWindow);
			this.OnHideTooltipWindow	= new CallbackDelegate (callback.OnHideTooltipWindow);
			this.OnStateNetStart		= new CallbackDelegate (callback.OnStateNetStart);
			this.OnStateNetStop			= new CallbackDelegate (callback.OnStateNetStop);
			this.OnStateSpecial			= new CallbackDelegateUintInt (callback.OnStateSpecial);
			this.OnStateChange			= new CallbackDelegateStringIntUint (callback.OnStateChange);
			this.OnProgress				= new CallbackDelegateIntInt (callback.OnProgress);
			this.OnProgressAll			= new CallbackDelegateStringIntInt (callback.OnProgressAll);
			this.OnLocationChanged		= new CallbackDelegate (callback.OnLocationChanged);
			this.OnStatusChange			= new CallbackDelegateStringInt (callback.OnStatusChange);
			this.OnSecurityChange		= new CallbackDelegateUint (callback.OnSecurityChange);
			this.OnVisibility			= new CallbackDelegateBool (callback.OnVisibility);
			this.OnDomKeyDown			= new DomKeyCallbackDelegate (callback.OnClientDomKeyDown);
			this.OnDomKeyUp				= new DomKeyCallbackDelegate (callback.OnClientDomKeyUp);
			this.OnDomKeyPress			= new DomKeyCallbackDelegate (callback.OnClientDomKeyPress);
			this.OnMouseDown			= new MouseCallbackDelegate (callback.OnClientMouseDown);
			this.OnMouseUp				= new MouseCallbackDelegate (callback.OnClientMouseUp);
			this.OnMouseClick			= new MouseCallbackDelegate (callback.OnClientMouseClick);
			this.OnMouseDoubleClick		= new MouseCallbackDelegate (callback.OnClientMouseDoubleClick);
			this.OnMouseOver			= new MouseCallbackDelegate (callback.OnClientMouseOver);
			this.OnMouseOut				= new MouseCallbackDelegate (callback.OnClientMouseOut);
			this.OnActivate				= new CallbackDelegate2 (callback.OnClientActivate);
			this.OnFocusIn				= new CallbackDelegate2 (callback.OnClientFocusIn);
			this.OnFocusOut				= new CallbackDelegate2 (callback.OnClientFocusOut);
			this.OnBeforeURIOpen		= new CallbackDelegate2String (callback.OnBeforeURIOpen);
			this.OnFocus				= new CallbackDelegate (callback.OnFocus);
			this.OnCreateNewWindow		= new CallbackDelegate2 (callback.OnCreateNewWindow);
			this.OnAlert				= new CallbackDelegatePtrPtr (callback.OnAlert);

			this.OnAlertCheck= new CallbackDelegate2OnAlertCheck (callback.OnAlertCheck);
			this.OnConfirm = new CallbackDelegate2OnConfirm (callback.OnConfirm);
			this.OnConfirmCheck = new CallbackDelegate2OnConfirmCheck (callback.OnConfirmCheck);
			this.OnConfirmEx = new CallbackDelegate2OnConfirmEx (callback.OnConfirmEx);
			this.OnPrompt = new CallbackDelegate2OnPrompt (callback.OnPrompt);
			this.OnPromptUsernameAndPassword = new CallbackDelegate2OnPromptUsernameAndPassword (callback.OnPromptUsernameAndPassword);
			this.OnPromptPassword = new CallbackDelegate2OnPromptPassword (callback.OnPromptPassword);
			this.OnSelect = new CallbackDelegate2OnSelect (callback.OnSelect);


			this.OnGeneric				= new GenericCallbackDelegate (callback.OnGeneric);
		}
	}

}
