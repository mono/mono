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
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

    public class RichTextBox : TextBoxBase {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public RichTextBox()
		{
			throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
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
		//public IAsyncResult BeginInvoke(Delegate del)
		//{
		//	throw new NotImplementedException ();
		//}
		//public IAsyncResult BeginInvoke(Delegate del, object[] objs)
		//{
		//	throw new NotImplementedException ();
		//}
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

		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}
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
		//public void Invalidate()
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(Rectangle rect)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(Region reg)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(Rectangle rect, bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(Region reg, bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		//public object Invoke(Delegate del)
		//{
		//	throw new NotImplementedException ();
		//}
		//public object Invoke(Delegate del, object[] objs)
		//{
		//	throw new NotImplementedException ();
		//}
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
		
		//public void Paste()
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public void Paste(DataFormats.Format format)
		{
			throw new NotImplementedException ();
		}
		//public void PerformLayout()
		//{
		//	throw new NotImplementedException ();
		//}
		//public void PerformLayout(Control ctl, string str)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public void Redo()
		{
			throw new NotImplementedException ();
		}
		//public void ResumeLayout()
		//{
		//	throw new NotImplementedException ();
		//}
		//public void ResumeLayout(bool val)
		//{
		//	throw new NotImplementedException ();
		//}
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
		//public void Scale(float val)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Scale(float val1, float val2)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Select()
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Select(int val1, int val2)
		//{
		//	throw new NotImplementedException ();
		//}
		//public virtual void Select(bool val1, bool val2)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void SetBounds(int val1, int val2, int val3, int val4)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void SetBounds(int val1, int val2, int val3, int val4, BoundsSpecified bounds)
		//{
		//	throw new NotImplementedException ();
		//}

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
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		protected override Size DefaultSize {
			get {
				throw new NotImplementedException ();
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
		//protected ContentAlignment RtlTranslateAlignment(ContentAlignment calign)
		//{
		//	throw new NotImplementedException ();
		//}
		//protected HorizontalAlignment RtlTranslateAlignment(HorizontalAlignment halign)
		//{
		//	throw new NotImplementedException ();
		//}
		//protected LeftRightAlignment RtlTranslateAlignment(LeftRightAlignment lralign)
		//{
		//	throw new NotImplementedException ();
		//}
		//protected void Select()
		//{
		//	throw new NotImplementedException ();
		//}
		//protected virtual void Select(bool val1, bool val2)
		//{
		//	throw new NotImplementedException ();
		//}
		//protected void UpdateBounds()
		//{
		//	throw new NotImplementedException ();
		//}
		//protected void UpdateBounds(int val1, int val2, int val3, int val4)
		//{
		//	throw new NotImplementedException ();
		//}
		//protected void UpdateBounds(int val1, int val2, int val3, int val4, int val5, int val6)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			throw new NotImplementedException ();
		}
	 }
}
