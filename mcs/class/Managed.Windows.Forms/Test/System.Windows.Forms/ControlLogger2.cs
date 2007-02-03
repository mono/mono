// Automatically generated for assembly: System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// To regenerate:
// gmcs -r:System.Windows.Forms.dll LogGenerator.cs && mono LogGenerator.exe System.Windows.Forms.Control overrides ControlLogger2.cs
#if NET_2_0

#region ControlOverrideLogger
using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.Remoting;
using System.Windows.Forms.Layout;
using System.Text;

namespace MonoTests.System.Windows.Forms 
{
	public class ControlOverrideLogger : Control
	{
		public StringBuilder Log = new StringBuilder ();
		void ShowLocation(string message)
		{
			Log.Append (message + Environment.NewLine);
			//Console.WriteLine(DateTime.Now.ToLongTimeString() + " Control." + message);
		}
		
		protected override void Dispose(Boolean disposing )
		{
			ShowLocation (string.Format("Dispose (disposing=<{0}>) ", disposing));
			base.Dispose(disposing);;
		}


		public override Size GetPreferredSize(Size proposedSize )
		{
			ShowLocation (string.Format("GetPreferredSize (proposedSize=<{0}>) ", proposedSize));
			return base.GetPreferredSize(proposedSize);;
		}


		public override Boolean PreProcessMessage(ref Message msg )
		{
			ShowLocation (string.Format("PreProcessMessage (msg=<{0}>) ", msg));
			return base.PreProcessMessage(ref msg);;
		}


		public override void Refresh()
		{
			ShowLocation (string.Format("Refresh () "));
			base.Refresh();;
		}


		public override void ResetBackColor()
		{
			ShowLocation (string.Format("ResetBackColor () "));
			base.ResetBackColor();;
		}


		public override void ResetCursor()
		{
			ShowLocation (string.Format("ResetCursor () "));
			base.ResetCursor();;
		}


		public override void ResetFont()
		{
			ShowLocation (string.Format("ResetFont () "));
			base.ResetFont();;
		}


		public override void ResetForeColor()
		{
			ShowLocation (string.Format("ResetForeColor () "));
			base.ResetForeColor();;
		}


		public override void ResetRightToLeft()
		{
			ShowLocation (string.Format("ResetRightToLeft () "));
			base.ResetRightToLeft();;
		}


		public override void ResetText()
		{
			ShowLocation (string.Format("ResetText () "));
			base.ResetText();;
		}


		protected override AccessibleObject CreateAccessibilityInstance()
		{
			ShowLocation (string.Format("CreateAccessibilityInstance () "));
			return base.CreateAccessibilityInstance();;
		}


		protected override ControlCollection CreateControlsInstance()
		{
			ShowLocation (string.Format("CreateControlsInstance () "));
			return base.CreateControlsInstance();;
		}


		protected override void CreateHandle()
		{
			ShowLocation (string.Format("CreateHandle () "));
			base.CreateHandle();;
		}


		protected override void DefWndProc(ref Message m )
		{
			ShowLocation (string.Format("DefWndProc (m=<{0}>) ", m));
			base.DefWndProc(ref m);;
		}


		protected override void DestroyHandle()
		{
			ShowLocation (string.Format("DestroyHandle () "));
			base.DestroyHandle();;
		}


		protected override AccessibleObject GetAccessibilityObjectById(Int32 objectId )
		{
			ShowLocation (string.Format("GetAccessibilityObjectById (objectId=<{0}>) ", objectId));
			return base.GetAccessibilityObjectById(objectId);;
		}


		protected override void InitLayout()
		{
			ShowLocation (string.Format("InitLayout () "));
			base.InitLayout();;
		}


		protected override Boolean IsInputChar(Char charCode )
		{
			ShowLocation (string.Format("IsInputChar (charCode=<{0}>) ", charCode));
			return base.IsInputChar(charCode);;
		}


		protected override Boolean IsInputKey(Keys keyData )
		{
			ShowLocation (string.Format("IsInputKey (keyData=<{0}>) ", keyData));
			return base.IsInputKey(keyData);;
		}


		protected override void NotifyInvalidate(Rectangle invalidatedArea )
		{
			ShowLocation (string.Format("NotifyInvalidate (invalidatedArea=<{0}>) ", invalidatedArea));
			base.NotifyInvalidate(invalidatedArea);;
		}


		protected override Boolean ProcessCmdKey(ref Message msg , Keys keyData )
		{
			ShowLocation (string.Format("ProcessCmdKey (msg=<{0}>, keyData=<{1}>) ", msg, keyData));
			return base.ProcessCmdKey(ref msg, keyData);;
		}


		protected override Boolean ProcessDialogChar(Char charCode )
		{
			ShowLocation (string.Format("ProcessDialogChar (charCode=<{0}>) ", charCode));
			return base.ProcessDialogChar(charCode);;
		}


		protected override Boolean ProcessDialogKey(Keys keyData )
		{
			ShowLocation (string.Format("ProcessDialogKey (keyData=<{0}>) ", keyData));
			return base.ProcessDialogKey(keyData);;
		}


		protected override Boolean ProcessKeyEventArgs(ref Message msg )
		{
			ShowLocation (string.Format("ProcessKeyEventArgs (msg=<{0}>) ", msg));
			return base.ProcessKeyEventArgs(ref msg);;
		}


		protected override Boolean ProcessKeyMessage(ref Message msg )
		{
			ShowLocation (string.Format("ProcessKeyMessage (msg=<{0}>) ", msg));
			return base.ProcessKeyMessage(ref msg);;
		}


		protected override Boolean ProcessKeyPreview(ref Message msg )
		{
			ShowLocation (string.Format("ProcessKeyPreview (msg=<{0}>) ", msg));
			return base.ProcessKeyPreview(ref msg);;
		}


		protected override Boolean ProcessMnemonic(Char charCode )
		{
			ShowLocation (string.Format("ProcessMnemonic (charCode=<{0}>) ", charCode));
			return base.ProcessMnemonic(charCode);;
		}


		protected override void ScaleCore(Single dx , Single dy )
		{
			ShowLocation (string.Format("ScaleCore (dx=<{0}>, dy=<{1}>) ", dx, dy));
			base.ScaleCore(dx, dy);;
		}


		protected override void Select(Boolean directed , Boolean forward )
		{
			ShowLocation (string.Format("Select (directed=<{0}>, forward=<{1}>) ", directed, forward));
			base.Select(directed, forward);;
		}


		protected override void SetBoundsCore(Int32 x , Int32 y , Int32 width , Int32 height , BoundsSpecified specified )
		{
			ShowLocation (string.Format("SetBoundsCore (x=<{0}>, y=<{1}>, width=<{2}>, height=<{3}>, specified=<{4}>) ", x, y, width, height, specified));
			base.SetBoundsCore(x, y, width, height, specified);;
		}


		protected override void SetClientSizeCore(Int32 x , Int32 y )
		{
			ShowLocation (string.Format("SetClientSizeCore (x=<{0}>, y=<{1}>) ", x, y));
			base.SetClientSizeCore(x, y);;
		}


		protected override void SetVisibleCore(Boolean value )
		{
			ShowLocation (string.Format("SetVisibleCore (value=<{0}>) ", value));
			base.SetVisibleCore(value);;
		}


		protected override Size SizeFromClientSize(Size clientSize )
		{
			ShowLocation (string.Format("SizeFromClientSize (clientSize=<{0}>) ", clientSize));
			return base.SizeFromClientSize(clientSize);;
		}


		protected override void WndProc(ref Message m )
		{
			ShowLocation (string.Format("WndProc (m=<{0}>) ", m));
			base.WndProc(ref m);;
		}


		protected override void OnAutoSizeChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnAutoSizeChanged (e=<{0}>) ", e));
			base.OnAutoSizeChanged(e);;
		}


		protected override void OnBackColorChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnBackColorChanged (e=<{0}>) ", e));
			base.OnBackColorChanged(e);;
		}


		protected override void OnBackgroundImageChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnBackgroundImageChanged (e=<{0}>) ", e));
			base.OnBackgroundImageChanged(e);;
		}


		protected override void OnBackgroundImageLayoutChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnBackgroundImageLayoutChanged (e=<{0}>) ", e));
			base.OnBackgroundImageLayoutChanged(e);;
		}


		protected override void OnBindingContextChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnBindingContextChanged (e=<{0}>) ", e));
			base.OnBindingContextChanged(e);;
		}


		protected override void OnCausesValidationChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnCausesValidationChanged (e=<{0}>) ", e));
			base.OnCausesValidationChanged(e);;
		}


		protected override void OnChangeUICues(UICuesEventArgs e )
		{
			ShowLocation (string.Format("OnChangeUICues (e=<{0}>) ", e));
			base.OnChangeUICues(e);;
		}


		protected override void OnClick(EventArgs e )
		{
			ShowLocation (string.Format("OnClick (e=<{0}>) ", e));
			base.OnClick(e);;
		}


		protected override void OnClientSizeChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnClientSizeChanged (e=<{0}>) ", e));
			base.OnClientSizeChanged(e);;
		}


		protected override void OnContextMenuChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnContextMenuChanged (e=<{0}>) ", e));
			base.OnContextMenuChanged(e);;
		}


		protected override void OnContextMenuStripChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnContextMenuStripChanged (e=<{0}>) ", e));
			base.OnContextMenuStripChanged(e);;
		}


		protected override void OnControlAdded(ControlEventArgs e )
		{
			ShowLocation (string.Format("OnControlAdded (e=<{0}>) ", e));
			base.OnControlAdded(e);;
		}


		protected override void OnControlRemoved(ControlEventArgs e )
		{
			ShowLocation (string.Format("OnControlRemoved (e=<{0}>) ", e));
			base.OnControlRemoved(e);;
		}


		protected override void OnCreateControl()
		{
			ShowLocation (string.Format("OnCreateControl () "));
			base.OnCreateControl();;
		}


		protected override void OnCursorChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnCursorChanged (e=<{0}>) ", e));
			base.OnCursorChanged(e);;
		}


		protected override void OnDockChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnDockChanged (e=<{0}>) ", e));
			base.OnDockChanged(e);;
		}


		protected override void OnDoubleClick(EventArgs e )
		{
			ShowLocation (string.Format("OnDoubleClick (e=<{0}>) ", e));
			base.OnDoubleClick(e);;
		}


		protected override void OnDragDrop(DragEventArgs drgevent )
		{
			ShowLocation (string.Format("OnDragDrop (drgevent=<{0}>) ", drgevent));
			base.OnDragDrop(drgevent);;
		}


		protected override void OnDragEnter(DragEventArgs drgevent )
		{
			ShowLocation (string.Format("OnDragEnter (drgevent=<{0}>) ", drgevent));
			base.OnDragEnter(drgevent);;
		}


		protected override void OnDragLeave(EventArgs e )
		{
			ShowLocation (string.Format("OnDragLeave (e=<{0}>) ", e));
			base.OnDragLeave(e);;
		}


		protected override void OnDragOver(DragEventArgs drgevent )
		{
			ShowLocation (string.Format("OnDragOver (drgevent=<{0}>) ", drgevent));
			base.OnDragOver(drgevent);;
		}


		protected override void OnEnabledChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnEnabledChanged (e=<{0}>) ", e));
			base.OnEnabledChanged(e);;
		}


		protected override void OnEnter(EventArgs e )
		{
			ShowLocation (string.Format("OnEnter (e=<{0}>) ", e));
			base.OnEnter(e);;
		}


		protected override void OnFontChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnFontChanged (e=<{0}>) ", e));
			base.OnFontChanged(e);;
		}


		protected override void OnForeColorChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnForeColorChanged (e=<{0}>) ", e));
			base.OnForeColorChanged(e);;
		}


		protected override void OnGiveFeedback(GiveFeedbackEventArgs gfbevent )
		{
			ShowLocation (string.Format("OnGiveFeedback (gfbevent=<{0}>) ", gfbevent));
			base.OnGiveFeedback(gfbevent);;
		}


		protected override void OnGotFocus(EventArgs e )
		{
			ShowLocation (string.Format("OnGotFocus (e=<{0}>) ", e));
			base.OnGotFocus(e);;
		}


		protected override void OnHandleCreated(EventArgs e )
		{
			ShowLocation (string.Format("OnHandleCreated (e=<{0}>) ", e));
			base.OnHandleCreated(e);;
		}


		protected override void OnHandleDestroyed(EventArgs e )
		{
			ShowLocation (string.Format("OnHandleDestroyed (e=<{0}>) ", e));
			base.OnHandleDestroyed(e);;
		}


		protected override void OnHelpRequested(HelpEventArgs hevent )
		{
			ShowLocation (string.Format("OnHelpRequested (hevent=<{0}>) ", hevent));
			base.OnHelpRequested(hevent);;
		}


		protected override void OnImeModeChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnImeModeChanged (e=<{0}>) ", e));
			base.OnImeModeChanged(e);;
		}


		protected override void OnInvalidated(InvalidateEventArgs e )
		{
			ShowLocation (string.Format("OnInvalidated (e=<{0}>) ", e));
			base.OnInvalidated(e);;
		}


		protected override void OnKeyDown(KeyEventArgs e )
		{
			ShowLocation (string.Format("OnKeyDown (e=<{0}>) ", e));
			base.OnKeyDown(e);;
		}


		protected override void OnKeyPress(KeyPressEventArgs e )
		{
			ShowLocation (string.Format("OnKeyPress (e=<{0}>) ", e));
			base.OnKeyPress(e);;
		}


		protected override void OnKeyUp(KeyEventArgs e )
		{
			ShowLocation (string.Format("OnKeyUp (e=<{0}>) ", e));
			base.OnKeyUp(e);;
		}


		protected override void OnLayout(LayoutEventArgs levent )
		{
			ShowLocation (string.Format("OnLayout (levent=<{0}>) ", levent));
			base.OnLayout(levent);;
		}


		protected override void OnLeave(EventArgs e )
		{
			ShowLocation (string.Format("OnLeave (e=<{0}>) ", e));
			base.OnLeave(e);;
		}


		protected override void OnLocationChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnLocationChanged (e=<{0}>) ", e));
			base.OnLocationChanged(e);;
		}


		protected override void OnLostFocus(EventArgs e )
		{
			ShowLocation (string.Format("OnLostFocus (e=<{0}>) ", e));
			base.OnLostFocus(e);;
		}


		protected override void OnMarginChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnMarginChanged (e=<{0}>) ", e));
			base.OnMarginChanged(e);;
		}


		protected override void OnMouseCaptureChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnMouseCaptureChanged (e=<{0}>) ", e));
			base.OnMouseCaptureChanged(e);;
		}


		protected override void OnMouseClick(MouseEventArgs e )
		{
			ShowLocation (string.Format("OnMouseClick (e=<{0}>) ", e));
			base.OnMouseClick(e);;
		}


		protected override void OnMouseDoubleClick(MouseEventArgs e )
		{
			ShowLocation (string.Format("OnMouseDoubleClick (e=<{0}>) ", e));
			base.OnMouseDoubleClick(e);;
		}


		protected override void OnMouseDown(MouseEventArgs e )
		{
			ShowLocation (string.Format("OnMouseDown (e=<{0}>) ", e));
			base.OnMouseDown(e);;
		}


		protected override void OnMouseEnter(EventArgs e )
		{
			ShowLocation (string.Format("OnMouseEnter (e=<{0}>) ", e));
			base.OnMouseEnter(e);;
		}


		protected override void OnMouseHover(EventArgs e )
		{
			ShowLocation (string.Format("OnMouseHover (e=<{0}>) ", e));
			base.OnMouseHover(e);;
		}


		protected override void OnMouseLeave(EventArgs e )
		{
			ShowLocation (string.Format("OnMouseLeave (e=<{0}>) ", e));
			base.OnMouseLeave(e);;
		}


		protected override void OnMouseMove(MouseEventArgs e )
		{
			ShowLocation (string.Format("OnMouseMove (e=<{0}>) ", e));
			base.OnMouseMove(e);;
		}


		protected override void OnMouseUp(MouseEventArgs e )
		{
			ShowLocation (string.Format("OnMouseUp (e=<{0}>) ", e));
			base.OnMouseUp(e);;
		}


		protected override void OnMouseWheel(MouseEventArgs e )
		{
			ShowLocation (string.Format("OnMouseWheel (e=<{0}>) ", e));
			base.OnMouseWheel(e);;
		}


		protected override void OnMove(EventArgs e )
		{
			ShowLocation (string.Format("OnMove (e=<{0}>) ", e));
			base.OnMove(e);;
		}


		protected override void OnNotifyMessage(Message m )
		{
			ShowLocation (string.Format("OnNotifyMessage (m=<{0}>) ", m));
			base.OnNotifyMessage(m);;
		}


		protected override void OnPaddingChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnPaddingChanged (e=<{0}>) ", e));
			base.OnPaddingChanged(e);;
		}


		protected override void OnPaint(PaintEventArgs e )
		{
			ShowLocation (string.Format("OnPaint (e=<{0}>) ", e));
			base.OnPaint(e);;
		}


		protected override void OnPaintBackground(PaintEventArgs pevent )
		{
			ShowLocation (string.Format("OnPaintBackground (pevent=<{0}>) ", pevent));
			base.OnPaintBackground(pevent);;
		}


		protected override void OnParentBackColorChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnParentBackColorChanged (e=<{0}>) ", e));
			base.OnParentBackColorChanged(e);;
		}


		protected override void OnParentBackgroundImageChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnParentBackgroundImageChanged (e=<{0}>) ", e));
			base.OnParentBackgroundImageChanged(e);;
		}


		protected override void OnParentBindingContextChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnParentBindingContextChanged (e=<{0}>) ", e));
			base.OnParentBindingContextChanged(e);;
		}


		protected override void OnParentChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnParentChanged (e=<{0}>) ", e));
			base.OnParentChanged(e);;
		}


		protected override void OnParentEnabledChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnParentEnabledChanged (e=<{0}>) ", e));
			base.OnParentEnabledChanged(e);;
		}


		protected override void OnParentFontChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnParentFontChanged (e=<{0}>) ", e));
			base.OnParentFontChanged(e);;
		}


		protected override void OnParentForeColorChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnParentForeColorChanged (e=<{0}>) ", e));
			base.OnParentForeColorChanged(e);;
		}


		protected override void OnParentRightToLeftChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnParentRightToLeftChanged (e=<{0}>) ", e));
			base.OnParentRightToLeftChanged(e);;
		}


		protected override void OnParentVisibleChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnParentVisibleChanged (e=<{0}>) ", e));
			base.OnParentVisibleChanged(e);;
		}


		protected override void OnQueryContinueDrag(QueryContinueDragEventArgs e )
		{
			ShowLocation (string.Format("OnQueryContinueDrag (e=<{0}>) ", e));
			base.OnQueryContinueDrag(e);;
		}


		protected override void OnRegionChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnRegionChanged (e=<{0}>) ", e));
			base.OnRegionChanged(e);;
		}


		protected override void OnResize(EventArgs e )
		{
			ShowLocation (string.Format("OnResize (e=<{0}>) ", e));
			base.OnResize(e);;
		}


		protected override void OnRightToLeftChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnRightToLeftChanged (e=<{0}>) ", e));
			base.OnRightToLeftChanged(e);;
		}


		protected override void OnSizeChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnSizeChanged (e=<{0}>) ", e));
			base.OnSizeChanged(e);;
		}


		protected override void OnStyleChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnStyleChanged (e=<{0}>) ", e));
			base.OnStyleChanged(e);;
		}


		protected override void OnSystemColorsChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnSystemColorsChanged (e=<{0}>) ", e));
			base.OnSystemColorsChanged(e);;
		}


		protected override void OnTabIndexChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnTabIndexChanged (e=<{0}>) ", e));
			base.OnTabIndexChanged(e);;
		}


		protected override void OnTabStopChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnTabStopChanged (e=<{0}>) ", e));
			base.OnTabStopChanged(e);;
		}


		protected override void OnTextChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnTextChanged (e=<{0}>) ", e));
			base.OnTextChanged(e);;
		}


		protected override void OnValidated(EventArgs e )
		{
			ShowLocation (string.Format("OnValidated (e=<{0}>) ", e));
			base.OnValidated(e);;
		}


		protected override void OnValidating(CancelEventArgs e )
		{
			ShowLocation (string.Format("OnValidating (e=<{0}>) ", e));
			base.OnValidating(e);;
		}


		protected override void OnVisibleChanged(EventArgs e )
		{
			ShowLocation (string.Format("OnVisibleChanged (e=<{0}>) ", e));
			base.OnVisibleChanged(e);;
		}


		protected override Object GetService(Type service )
		{
			ShowLocation (string.Format("GetService (service=<{0}>) ", service));
			return base.GetService(service);;
		}


		public override String ToString()
		{
			ShowLocation (string.Format("ToString () "));
			return base.ToString();;
		}


		public override ObjRef CreateObjRef(Type type )
		{
			ShowLocation (string.Format("CreateObjRef (type=<{0}>) ", type));
			return base.CreateObjRef(type);;
		}


		public override Object InitializeLifetimeService()
		{
			ShowLocation (string.Format("InitializeLifetimeService () "));
			return base.InitializeLifetimeService();;
		}


		public override Boolean Equals(Object o )
		{
			ShowLocation (string.Format("Equals (o=<{0}>) ", o));
			return base.Equals(o);;
		}


		public override Int32 GetHashCode()
		{
			ShowLocation (string.Format("GetHashCode () "));
			return base.GetHashCode();;
		}


		public override Boolean AllowDrop
		{
			get {
				ShowLocation (string.Format("get_AllowDrop () "));
				return base.AllowDrop;
			}

			set {
				ShowLocation (string.Format("get_AllowDrop () "));
				base.AllowDrop = value;
			}
}



		public override AnchorStyles Anchor
		{
			get {
				ShowLocation (string.Format("get_Anchor () "));
				return base.Anchor;
			}

			set {
				ShowLocation (string.Format("get_Anchor () "));
				base.Anchor = value;
			}
}



		public override Boolean AutoSize
		{
			get {
				ShowLocation (string.Format("get_AutoSize () "));
				return base.AutoSize;
			}

			set {
				ShowLocation (string.Format("get_AutoSize () "));
				base.AutoSize = value;
			}
}



		public override Size MaximumSize
		{
			get {
				ShowLocation (string.Format("get_MaximumSize () "));
				return base.MaximumSize;
			}

			set {
				ShowLocation (string.Format("get_MaximumSize () "));
				base.MaximumSize = value;
			}
}



		public override Size MinimumSize
		{
			get {
				ShowLocation (string.Format("get_MinimumSize () "));
				return base.MinimumSize;
			}

			set {
				ShowLocation (string.Format("get_MinimumSize () "));
				base.MinimumSize = value;
			}
}



		public override Color BackColor
		{
			get {
				ShowLocation (string.Format("get_BackColor () "));
				return base.BackColor;
			}

			set {
				ShowLocation (string.Format("get_BackColor () "));
				base.BackColor = value;
			}
}



		public override Image BackgroundImage
		{
			get {
				ShowLocation (string.Format("get_BackgroundImage () "));
				return base.BackgroundImage;
			}

			set {
				ShowLocation (string.Format("get_BackgroundImage () "));
				base.BackgroundImage = value;
			}
}



		public override ImageLayout BackgroundImageLayout
		{
			get {
				ShowLocation (string.Format("get_BackgroundImageLayout () "));
				return base.BackgroundImageLayout;
			}

			set {
				ShowLocation (string.Format("get_BackgroundImageLayout () "));
				base.BackgroundImageLayout = value;
			}
}



		public override BindingContext BindingContext
		{
			get {
				ShowLocation (string.Format("get_BindingContext () "));
				return base.BindingContext;
			}

			set {
				ShowLocation (string.Format("get_BindingContext () "));
				base.BindingContext = value;
			}
}



		public override ContextMenu ContextMenu
		{
			get {
				ShowLocation (string.Format("get_ContextMenu () "));
				return base.ContextMenu;
			}

			set {
				ShowLocation (string.Format("get_ContextMenu () "));
				base.ContextMenu = value;
			}
}



		public override ContextMenuStrip ContextMenuStrip
		{
			get {
				ShowLocation (string.Format("get_ContextMenuStrip () "));
				return base.ContextMenuStrip;
			}

			set {
				ShowLocation (string.Format("get_ContextMenuStrip () "));
				base.ContextMenuStrip = value;
			}
}



		public override Cursor Cursor
		{
			get {
				ShowLocation (string.Format("get_Cursor () "));
				return base.Cursor;
			}

			set {
				ShowLocation (string.Format("get_Cursor () "));
				base.Cursor = value;
			}
}



		public override Rectangle DisplayRectangle
		{
			get {
				ShowLocation (string.Format("get_DisplayRectangle () "));
				return base.DisplayRectangle;
			}
}



		public override DockStyle Dock
		{
			get {
				ShowLocation (string.Format("get_Dock () "));
				return base.Dock;
			}

			set {
				ShowLocation (string.Format("get_Dock () "));
				base.Dock = value;
			}
}



		public override Boolean Focused
		{
			get {
				ShowLocation (string.Format("get_Focused () "));
				return base.Focused;
			}
}



		public override Font Font
		{
			get {
				ShowLocation (string.Format("get_Font () "));
				return base.Font;
			}

			set {
				ShowLocation (string.Format("get_Font () "));
				base.Font = value;
			}
}



		public override Color ForeColor
		{
			get {
				ShowLocation (string.Format("get_ForeColor () "));
				return base.ForeColor;
			}

			set {
				ShowLocation (string.Format("get_ForeColor () "));
				base.ForeColor = value;
			}
}



		public override LayoutEngine LayoutEngine
		{
			get {
				ShowLocation (string.Format("get_LayoutEngine () "));
				return base.LayoutEngine;
			}
}



		public override RightToLeft RightToLeft
		{
			get {
				ShowLocation (string.Format("get_RightToLeft () "));
				return base.RightToLeft;
			}

			set {
				ShowLocation (string.Format("get_RightToLeft () "));
				base.RightToLeft = value;
			}
}



		public override ISite Site
		{
			get {
				ShowLocation (string.Format("get_Site () "));
				return base.Site;
			}

			set {
				ShowLocation (string.Format("get_Site () "));
				base.Site = value;
			}
}



		public override String Text
		{
			get {
				ShowLocation (string.Format("get_Text () "));
				return base.Text;
			}

			set {
				ShowLocation (string.Format("get_Text () "));
				base.Text = value;
			}
}



	}
#endregion
}
#endif
