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
				//FIXME:
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
				//FIXME:
			}
		}
		[MonoTODO]
		public override Color BackColor {
			get {
				//FIXME:
				return base.BackColor;
			}
			set
			{
				//FIXME:
				base.BackColor = value;
			}
		}
		[MonoTODO]
		public override Image BackgroundImage {
			get
			{
				//FIXME:
				return base.BackgroundImage;
			}
			set
			{
				//FIXME:
				base.BackgroundImage = value;
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
				//FIXME:
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
				//FIXME:
				return base.ForeColor;
			}
			set
			{
				//FIXME:
				base.ForeColor = value;
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
				//FIXME:
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
				//FIXME:
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
				//FIXME:
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
				//FIXME:
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
				//FIXME:
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
				//FIXME:
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
				//FIXME:
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
				//FIXME:
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
				//FIXME:
			}
		}
		[MonoTODO]
		public override string Text {
			get
			{
				//FIXME:
				return base.Text;
			}
			set
			{
				//FIXME:
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
				//FIXME:
			}
		}
		
		// --- Public Methods
		
		[MonoTODO]
		public void AppendText(string text) 
		{
			//FIXME:
		}
		[MonoTODO]
		public void Clear()
		{
			//FIXME:
		}
		[MonoTODO]
		public void ClearUndo()
		{
			//FIXME:
		}
		[MonoTODO]
		public void Copy()
		{
			//FIXME:
		}
		[MonoTODO]
		public void Cut()
		{
			//FIXME:
		}
		[MonoTODO]
		public void Paste()
		{
			//FIXME:
		}
		[MonoTODO]
		public void ScrollToCaret()
		{
			//FIXME:
		}
		[MonoTODO]
		public void Select(int start, int length) 
		{
			//FIXME:
		}
		[MonoTODO]
		public void SelectAll()
		{
			//FIXME:
		}
		[MonoTODO]
		public override string ToString()
		{
			//FIXME:
			return base.ToString();
		}
		[MonoTODO]
		public void Undo()
		{
			//FIXME:
		}
		
		// --- Public Events
		
		public event EventHandler AcceptsTabChanged;
		public event EventHandler AutoSizeChanged;
		public event EventHandler BorderStyleChanged;
		//[MonoTODO]
		//public event EventHandler Click;
		public event EventHandler HideSelectionChanged;
		public event EventHandler ModifiedChanged;
		public event EventHandler MultilineChanged;
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
				return new Size(100,20); //Correct size
			}
		}
		
		// --- Protected Methods
		
		[MonoTODO]
		protected override void CreateHandle()
		{
			//FIXME:
			base.CreateHandle();
		}
		[MonoTODO]
		protected override bool IsInputKey(Keys keyData)
		{
			//FIXME:
			return base.IsInputKey(keyData);
		}
		[MonoTODO]
		protected virtual void OnAcceptsTabChanged(EventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnAutoSizeChanged(EventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnBorderStyleChanged(EventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected override void OnFontChanged(EventArgs e)
		{
			//FIXME:
			base.OnFontChanged(e);
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e)
		{
			//FIXME:
			base.OnHandleCreated(e);
		}
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e)
		{
			//FIXME:
			base.OnHandleDestroyed(e);
		}
		[MonoTODO]
		protected virtual void OnHideSelectionChanged(EventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnModifiedChanged(EventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnMultilineChanged(EventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnReadOnlyChanged(EventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected override bool ProcessDialogKey(Keys keyData)
		{
			//FIXME:
			return base.ProcessDialogKey(keyData);
		}
		[MonoTODO]
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			//FIXME:
			base.SetBoundsCore(x, y, width, height, specified);
		}
		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			//FIXME:
			base.WndProc(ref m);
		}
	}
}

