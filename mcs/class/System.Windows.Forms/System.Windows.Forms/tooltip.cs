//
// System.Windows.Forms.ToolTip
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//	Dennis Hayes (dennish@raytek.com)
//   implemented by Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) 2002/3 Ximian, Inc
//
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections;

namespace System.Windows.Forms {

	// <summary>
	// Tooltip control
	// </summary>

	public sealed class ToolTip : Component, IExtenderProvider {
		bool active = true;
		int  automaticDelay = 500;
		int  autoPopDelay   = 5000;
		int  initialDelay   = 500;
		int  reshowDelay    = 100;
		bool showAlways     = false;
		const int MAX_SHORT = 32767;

		NativeWindow tooltipWnd	= new NativeWindow ();
		Hashtable tooltipTexts  = new Hashtable();

		public ToolTip() {
			createToolTipWindow ( );
		}

		public ToolTip(IContainer cont) {
			createToolTipWindow ( );
			cont.Add ( this );
		}

		public bool Active {
			get {	return active;	}
			set {
				if ( active != value ) {
					active = value;
					activateToolTip ( active );
				}
			}	
		}

		public int AutomaticDelay {
			get {	return automaticDelay;	}
			set {
				if ( automaticDelay != value ) {
					automaticDelay = value;
					AutoPopDelay = 10*automaticDelay;
					InitialDelay = automaticDelay;
					ReshowDelay = automaticDelay / 5;
					setToolTipDelay ( ToolTipControlDelayFlags.TTDT_AUTOMATIC, automaticDelay );
				}
			}	
		}

		public int AutoPopDelay{
			get {	return autoPopDelay; }
			set {
				autoPopDelay = value;
				setToolTipDelay ( ToolTipControlDelayFlags.TTDT_AUTOPOP, autoPopDelay );
			}
		}

		public int InitialDelay {
			get {	return initialDelay; }
			set {
				initialDelay = value;
				setToolTipDelay ( ToolTipControlDelayFlags.TTDT_INITIAL, initialDelay );
			}
		}

		public int ReshowDelay {
			get {	return reshowDelay; }
			set {
				reshowDelay = value;
				setToolTipDelay ( ToolTipControlDelayFlags.TTDT_RESHOW, reshowDelay );
			}
		}

		public bool ShowAlways {
			get {	return showAlways ; }
			set {
				if ( showAlways != value ) {
					bool OldStyle = showAlways;
					showAlways = value;
					if ( tooltipWnd.Handle != IntPtr.Zero )
						Win32.UpdateWindowStyle ( tooltipWnd.Handle,
									  OldStyle ? (int)ToolTipControlStyles.TTS_ALWAYSTIP : 0,
									  value ? (int)ToolTipControlStyles.TTS_ALWAYSTIP : 0 );
				}
			}
		}
		
		public void RemoveAll() {
			foreach (object o in tooltipTexts.Keys) {
				Control target = o as Control;
				if ( target != null ) {
					removeToolTip ( target );
					target.HandleCreated	-= new System.EventHandler( control_HandleCreated );
					target.HandleDestroyed -= new System.EventHandler ( control_HandleDestroyed );
				}
			}
			tooltipTexts.Clear ( );
		}

		public void SetToolTip(Control control, string caption) {
			if ( caption == null || caption.Length == 0 ) {
				if ( tooltipTexts.Contains ( control )  ) {
					removeToolTip ( control );					
					control.HandleCreated	-= new System.EventHandler( control_HandleCreated );
					control.HandleDestroyed -= new System.EventHandler ( control_HandleDestroyed );
					tooltipTexts.Remove ( control );
					return;
				}
			}
			if ( !tooltipTexts.Contains ( control )  ) {
				control.HandleCreated	+= new System.EventHandler( control_HandleCreated );
				control.HandleDestroyed += new System.EventHandler ( control_HandleDestroyed );
				if ( control.IsHandleCreated )
					addTool ( control, caption );
			}
			else {
				if ( control.IsHandleCreated )
					updateTipText ( control, caption );
			}
			tooltipTexts[ control ] = caption;
		}

		public string GetToolTip( Control control ) {
			string text = (string) tooltipTexts[control];
			if ( text == null )
				text = string.Empty;
			return text;
		}

		public override string ToString() {
			return "[" + GetType().FullName.ToString() + "] InitialDelay: " + InitialDelay.ToString() +
				", ShowAlways: " + ShowAlways.ToString();
		}

		bool IExtenderProvider.CanExtend( object extendee ){
			return ( extendee is Control ) && !( extendee is ToolTip );
		}
		
		private void createToolTipWindow ( ) {
			if ( tooltipWnd.Handle == IntPtr.Zero ) {
				initCommonControlsLibrary ( );
				
				CreateParams pars = new CreateParams ( );

				pars.ClassName = Win32.TOOLTIPS_CLASS;
				pars.ExStyle = (int) WindowExStyles.WS_EX_TOPMOST;
				pars.Style = (int) ToolTipControlStyles.TTS_NOPREFIX;
				
				if ( ShowAlways )
					pars.Style |= (int)ToolTipControlStyles.TTS_ALWAYSTIP;

				tooltipWnd.CreateHandle ( pars );

				Win32.SetWindowPos ( tooltipWnd.Handle,
						SetWindowPosZOrder.HWND_TOPMOST,
						0, 0, 0, 0, 
						SetWindowPosFlags.SWP_NOMOVE |
						SetWindowPosFlags.SWP_NOSIZE |
						SetWindowPosFlags.SWP_NOACTIVATE );

				Win32.SendMessage ( tooltipWnd.Handle,
					(int)ToolTipControlMessages.TTM_SETMAXTIPWIDTH,
					0, MAX_SHORT );

				activateToolTip ( Active );
			}
		}

		private void initCommonControlsLibrary ( ) {
			INITCOMMONCONTROLSEX	initEx = new INITCOMMONCONTROLSEX();
			initEx.dwICC = CommonControlInitFlags.ICC_BAR_CLASSES;
			Win32.InitCommonControlsEx(initEx);
		}

		private void control_HandleCreated(object sender, System.EventArgs e) {
			Control ctrl = sender as Control;
			if ( ctrl != null && tooltipTexts.Contains ( ctrl ) )
				addTool ( ctrl, GetToolTip ( ctrl ) );
		}

		private void control_HandleDestroyed(object sender, System.EventArgs e) {
			Control ctrl = sender as Control;
			if ( ctrl != null && tooltipTexts.Contains ( ctrl ) )
				removeToolTip ( ctrl );
		}

		private void addTool ( Control target, string tiptext ) {
			TOOLINFO ti	= new TOOLINFO( );
			ti.cbSize	= (uint)Marshal.SizeOf( ti );
			ti.hwnd		= target.Handle;
			ti.uId		= (uint)target.Handle.ToInt32();
			ti.lpszText	= tiptext;
			ti.uFlags	= (int)(ToolTipFlags.TTF_SUBCLASS | ToolTipFlags.TTF_IDISHWND);
			sendMessageHelper ( ToolTipControlMessages.TTM_ADDTOOL, ref ti);
		}

		private void updateTipText ( Control target, string tiptext ) {
			TOOLINFO ti	= new TOOLINFO( );
			ti.cbSize	= (uint)Marshal.SizeOf( ti );
			ti.hwnd		= target.Handle;
			ti.uId		= (uint)target.Handle.ToInt32();
			ti.lpszText	= tiptext;
			sendMessageHelper ( ToolTipControlMessages.TTM_UPDATETIPTEXT, ref ti );
		}

		private void activateToolTip ( bool avtivate ) {
			if ( tooltipWnd.Handle != IntPtr.Zero )
				Win32.SendMessage ( tooltipWnd.Handle,
					(int)ToolTipControlMessages.TTM_ACTIVATE, avtivate ? 1 : 0, 0 );
		}

		private void removeToolTip ( Control target ) {
			if ( target.IsHandleCreated ) {
				TOOLINFO ti	= new TOOLINFO( );
				ti.cbSize	= (uint)Marshal.SizeOf( ti );
				ti.hwnd		= target.Handle;
				ti.uId		= (uint)target.Handle.ToInt32();
				sendMessageHelper ( ToolTipControlMessages.TTM_DELTOOL, ref ti );
			}
		}

		private void setToolTipDelay ( ToolTipControlDelayFlags flag, int DelayTime ) {
			if ( tooltipWnd.Handle != IntPtr.Zero )
				Win32.SendMessage ( tooltipWnd.Handle,
					(int)ToolTipControlMessages.TTM_SETDELAYTIME,
					(int)flag, Win32.MAKELONG( DelayTime, 0) );
		}

		private void sendMessageHelper ( ToolTipControlMessages mes, ref TOOLINFO ti ) {
			if ( tooltipWnd.Handle != IntPtr.Zero ) {
				IntPtr ptr	= Marshal.AllocHGlobal ( Marshal.SizeOf ( ti ) );
				Marshal.StructureToPtr( ti, ptr, false );
				Win32.SendMessage ( tooltipWnd.Handle ,	(int)mes, 0, ptr.ToInt32() );
				Marshal.FreeHGlobal ( ptr );
			}
		}
	}
}
