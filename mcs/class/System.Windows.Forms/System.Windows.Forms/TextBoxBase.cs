//
// System.Windows.Forms.TextBoxBase
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

        public class TextBoxBase : Control {
			private string text;
		//
		//  --- Public Properties
		//
		[MonoTODO]
		public bool AcceptsTab {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public virtual bool AutoSize {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Color BackColor {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Image BackgroundImage {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public BorderStyle BorderStyle {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool CanUndo {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Color ForeColor {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool HideSelection {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string[] Lines {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public virtual int MaxLength {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool Modified {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public virtual bool Multiline {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int PreferredHeight {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool ReadOnly {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public virtual string SelectedText {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public virtual int SelectionLength {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int SelectionStart {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override string Text {
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
		}
		[MonoTODO]
		public virtual int TextLength  {

			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool WordWrap {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		
		// --- Public Methods
		
		[MonoTODO]
		public void AppendText(string text) 
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Clear()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void ClearUndo()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Copy()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Cut()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Paste()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void ScrollToCaret()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Select(int start, int length) 
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SelectAll()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Undo()
		{
			throw new NotImplementedException ();
		}
		
		// --- Public Events
		
		[MonoTODO]
		public event EventHandler AcceptsTabChanged;
		[MonoTODO]
		public event EventHandler AutoSizeChanged;
		[MonoTODO]
		public event EventHandler BorderStyleChanged;
		//[MonoTODO]
		//public event EventHandler Click;
		[MonoTODO]
		public event EventHandler HideSelectionChanged;
		[MonoTODO]
		public event EventHandler ModifiedChanged;
		[MonoTODO]
		public event EventHandler MultilineChanged;
		[MonoTODO]
		public event EventHandler ReadOnlyChanged;
        
        // --- Protected Properties
        
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				window = new ControlNativeWindow (this);

				createParams.Caption = Text;
				createParams.ClassName = "BUTTONBASE";
				createParams.X = Left;
				createParams.Y = Top;
				createParams.Width = Width;
				createParams.Height = Height;
				createParams.ClassStyle = 0;
				createParams.ExStyle = 0;
				createParams.Param = 0;
				//			createParams.Parent = Parent.Handle;
				createParams.Style = (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_VISIBLE);
				window.CreateHandle (createParams);
				return createParams;
			}
		}
		[MonoTODO]
		protected override Size DefaultSize {
			get
			{
				throw new NotImplementedException ();
			}
		}
		
		// --- Protected Methods
		
		[MonoTODO]
		protected override void CreateHandle()
		{
			base.CreateHandle();
		}
		[MonoTODO]
		protected override bool IsInputKey(Keys keyData)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnAcceptsTabChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnAutoSizeChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnBorderStyleChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnFontChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnHideSelectionChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnModifiedChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnMultilineChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnReadOnlyChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override bool ProcessDialogKey(Keys keyData)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			throw new NotImplementedException ();
		}
	}
}

