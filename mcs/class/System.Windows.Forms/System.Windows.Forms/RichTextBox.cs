//
// System.Windows.Forms.RichTextBox.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;
using System.IO;

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

    public class RichTextBox : TextBoxBase {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public RichTextBox()
		{
			
		}

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public override bool AllowDrop {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override bool AutoSize {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool AutoWordSelection {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Image BackgroundImage {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int BulletIndent {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool CanRedo {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool DetectUrls {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Font Font {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Color ForeColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override int MaxLength {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override bool Multiline {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string RedoActionName {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int RightMargin {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string Rtf {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public RichTextBoxScrollBars ScrollBars {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string SelectedRtf {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override string SelectedText {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public HorizontalAlignment SelectionAlignment {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool SelectionBullet {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int SelectionCharOffset {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Color SelectionColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Font SelectionFont {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int SelectionHangingIndent {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int SelectionIndent {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override int SelectionLength {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool SelectionProtected {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int SelectionRightIndent {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int[] SelectionTabs {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public RichTextBoxSelectionTypes SelectionType {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool ShowSelectionMargin {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override string Text {
			get {
				return base.Text;
			}
			set {
				base.Text = value;
			}
		}
		[MonoTODO]
		public override int TextLength {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string UndoActionName {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public float ZoomFactor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public bool CanPaste(DataFormats.Format clipFormat)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}		[MonoTODO]
		public int Find(char[] chars)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int Find(string srt)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int Find(char[] chars, int val)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int Find(string srt, RichTextBoxFinds finds)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int Find(char[] chars, int val1, int val2)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int Find(string srt, int val, RichTextBoxFinds finds)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int Find(string srt, int val1, int val2, RichTextBoxFinds finds)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public char GetCharFromPosition(Point pt)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int GetLineFromCharIndex(int index)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Point GetPositionFromCharIndex(int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void LoadFile(string str)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void LoadFile(Stream str, RichTextBoxStreamType type)
		{
			throw new NotImplementedException ();
		}
		

		[MonoTODO]
		public void Paste(DataFormats.Format format)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Redo()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SaveFile(string str)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SaveFile(Stream str, RichTextBoxStreamType type)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SaveFile(string str, RichTextBoxStreamType type)
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Events
		//
		public event ContentsResizedEventHandler ContentsResized;
		public event EventHandler HScroll;
		public event EventHandler ImeChange;
		public event LinkClickedEventHandler LinkClicked;
		public event EventHandler Protected;
		public event EventHandler SelectionChanged;
		public event EventHandler VScroll;

		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				window = new ControlNativeWindow (this);

				createParams.Caption = Text;
				createParams.ClassName = "RICHTEXTBOX";
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
			get {
				return new System.Drawing.Size(300,300);
			}
		}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected virtual object CreateRichTextEditOleCallback()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnBackColorChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnContentsResized(ContentsResizedEventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void  OnContextMenuChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void  OnHandleCreated(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void  OnHandleDestroyed(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnHScroll(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnImeChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnClicked(LinkClickedEventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnProtected(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnRightToLeftChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnSelectionChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnSystemColorsChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnTextChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnVScroll(EventArgs e)
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
