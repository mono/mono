//
// System.Windows.Forms.PrintPreviewDialog
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.Drawing;
using System.Drawing.Printing;
using System.ComponentModel;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a dialog box form that contains a PrintPreviewControl.
	///
	/// </summary>

	[MonoTODO]
	public class PrintPreviewDialog : Form {

		#region Fields
		IButtonControl acceptButton;
		string accessibleDescription;
		string accessibleName;
		AccessibleRole accessibleRole;
		bool autoScale;
		Size autoScrollMargin;
		Size autoScrollMinSize;
		bool causesValidation;
		bool controlBox;
		ControlBindingsCollection dataBindings;
		PrintDocument document;
		bool enabled;
		FormBorderStyle formBorderStyle;
		bool helpButton;
		Icon icon;
		ImeMode imeMode;
		bool isMdiContainer;
		bool keyPreview;
		Point location;
		bool maximizeBox;
		Size maximumSize;
		MainMenu menu;
		bool minimizeBox;
		Size minimumSize;
		PrintPreviewControl printPreviewControl;
		bool showInTaskbar;
		Size size;
		FormStartPosition startPosition;
		bool tabStop;
		object tag;
		bool topMost;
		Color transparencyKey;
		bool useAntiAlias;
		bool visible;
		FormWindowState windowState;
		#endregion

		#region Constructor
		[MonoTODO]
		public PrintPreviewDialog() 
		{
			document=null;
			
			accessibleDescription=null;
			accessibleName=null;
			accessibleRole=AccessibleRole.Default;
			autoScale=true;
			controlBox=true;
			enabled=true;
			helpButton=false;
			imeMode=ImeMode.Inherit;
			isMdiContainer=false;
			keyPreview=false;
			maximizeBox=true;
			minimizeBox=true;
			showInTaskbar=true;
			tabStop=true;
			tag=null;
			topMost=false;
			visible=true;
			windowState=FormWindowState.Normal;

		}
		#endregion

		#region Properties
		public new IButtonControl AcceptButton {
			get { return acceptButton; }
			set { acceptButton=value; }
		}
		
		public new string AccessibleDescription {
			get { return accessibleDescription; }
			set { accessibleDescription=value; }
		}
		
		public new string AccessibleName {
			get { return accessibleName; }
			set { accessibleName=value; }
		}
		
		public new AccessibleRole AccessibleRole {
			get { return accessibleRole; }
			set { accessibleRole=value; }
		}
		
		[MonoTODO]
		public override bool AllowDrop {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override AnchorStyles Anchor {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public new bool AutoScale {
			get { return autoScale; }
			set { autoScale=value; }
		}
		
		[MonoTODO]
		public override Size AutoScaleBaseSize {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override bool AutoScroll {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public new Size AutoScrollMargin {
			get { return autoScrollMargin; }
			set { autoScrollMargin=value; }
		}
		
		public new Size AutoScrollMinSize {
			get { return autoScrollMinSize; }
			set { autoScrollMinSize=value; }
		}
		
		[MonoTODO]
		public override Color BackColor {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override Image BackgroundImage {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		/// public new IButtonControl CancelButton {get; set;}
		
		public new bool CausesValidation {
			get { return causesValidation; }
			set { causesValidation=value; }
		}
		
		[MonoTODO]
		public override ContextMenu ContextMenu {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public new bool ControlBox {
			get { return controlBox; }
			set { controlBox=value; }
		}
		
		[MonoTODO]
		public override Cursor Cursor {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public new ControlBindingsCollection DataBindings {
			get { return dataBindings; }
			set { dataBindings=value; }
		}
		
		[MonoTODO]
		public override DockStyle Dock {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		/// public new ScrollableControl.DockPaddingEdges DockPadding {get;}
		
		public PrintDocument Document {
			get { return document; }
			set { document=value; }
		}
		
		public new bool Enabled {
			get { return enabled; }
			set { enabled=value; }
		}
		
		[MonoTODO]
		public override Font Font {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override Color ForeColor  {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public new FormBorderStyle FormBorderStyle {
			get { return formBorderStyle; }
			set { formBorderStyle=value; }
		}
		
		public new bool HelpButton {
			get { return helpButton; }
			set { helpButton=value; }
		}
		
		public new Icon Icon {
			get { return icon; }
			set { icon=value; }
		}
		
		public new ImeMode ImeMode {
			get { return imeMode; }
			set { imeMode=value; }
		}
		
		public bool IsMdiContainer {
			get { return isMdiContainer; }
			set { isMdiContainer=value; }
		}
		
		public new bool KeyPreview {
			get { return keyPreview; }
			set { keyPreview=value; }
		}
		
		public new Point Location {
			get { return location; }
			set { location=value; }
		}
		
		public new bool MaximizeBox {
			get { return maximizeBox; }
			set { maximizeBox=value; }
		}
		
		public new Size MaximumSize {
			get { return maximumSize; }
			set { maximumSize=value; }
		}
		
		public new MainMenu Menu {
			get { return menu; }
			set { menu=value; }
		}
		
		public new bool MinimizeBox {
			get { return minimizeBox; }
			set { minimizeBox=value; }
		}
		
		public new Size MinimumSize {
			get { return minimumSize; }
			set { minimumSize=value; }
		}
		
		/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		/// public new double Opacity {get; set;}
		
		public PrintPreviewControl PrintPreviewControl {
			get { return printPreviewControl; }
			set { printPreviewControl=value; }
		}
		
		[MonoTODO]
		public override RightToLeft RightToLeft {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public new bool ShowInTaskbar {
			get { return showInTaskbar; }
			set { showInTaskbar=value; }
		}
		
		public new Size Size {
			get { return size; }
			set { size=value; }
		}
		
		/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		/// public new SizeGripStyle SizeGripStyle {get; set;}
		
		public new FormStartPosition StartPosition {
			get { return startPosition; }
			set { startPosition=value; }
		}
		
		public new bool TabStop {
			get { return tabStop; }
			set { tabStop=value; }
		}
		
		public new object Tag {
			get { return tag; }
			set { tag=value; }
		}
		
		[MonoTODO]
		public override string Text {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public new bool TopMost {
			get { return topMost; }
			set { topMost=value; }
		}
		
		public new Color TransparencyKey {
			get { return transparencyKey; }
			set { transparencyKey=value; }
		}
		
		public bool UseAntiAlias {
			get { return useAntiAlias; }
			set { useAntiAlias=value; }
		}
		
		public new bool Visible {
			get { return visible; }
			set { visible=value; }
		}
		
		public new FormWindowState WindowState {
			get { return windowState; }
			set { windowState=value; }
		}
		#endregion

		#region Methods
		[MonoTODO]
		protected override void CreateHandle() 
		{
			base.CreateHandle();
		}
		
		[MonoTODO]
		protected override void OnClosing(CancelEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}
