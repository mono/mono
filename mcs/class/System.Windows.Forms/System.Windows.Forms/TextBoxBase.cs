//
// System.Windows.Forms.TextBoxBase
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//   Dennis Hayes (dennish@Raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) 2002/3 Ximian, Inc
//
using System.Drawing;
using System.ComponentModel;
using System.Collections.Specialized;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

        public class TextBoxBase : Control {

		BorderStyle borderStyle;
		int maxLength;
		int  selectionStart;
		int  selectionLength;

		BitVector32 flags;
		private static readonly int acceptsTab    = BitVector32.CreateMask();
		private static readonly int autoSize      = BitVector32.CreateMask( acceptsTab );
		private static readonly int hideSelection = BitVector32.CreateMask( autoSize );
		private static readonly int modified      = BitVector32.CreateMask( hideSelection );
		private static readonly int multiline     = BitVector32.CreateMask( modified );
		private static readonly int readOnly      = BitVector32.CreateMask( multiline );
		private static readonly int wordWrap      = BitVector32.CreateMask( readOnly );

		internal TextBoxBase ( ) {
			flags = new BitVector32 ( );

			flags[ autoSize ] = true;
			flags[ hideSelection ] = true;
			flags[ wordWrap ] = true;

			borderStyle = BorderStyle.Fixed3D;
			selectionStart = -1;

			base.foreColor = SystemColors.WindowText;
			base.backColor = SystemColors.Window;
		}

		[MonoTODO]
		public bool AcceptsTab {
			get { return flags[ acceptsTab ]; }
			set {
				if ( flags[ acceptsTab ] != value ) {
					flags[ acceptsTab ] = value;
					OnAcceptsTabChanged ( EventArgs.Empty );
				}
			}
		}

		[MonoTODO]
		public virtual bool AutoSize {
			get { return flags[ autoSize ];	}
			set {
				if ( flags[ autoSize ] != value ) {
					flags[ autoSize ] = value;
					OnAutoSizeChanged ( EventArgs.Empty );
				}
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

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value;}
		}


		public BorderStyle BorderStyle {
			get {   return borderStyle; }
			set {
				if ( !Enum.IsDefined ( typeof(BorderStyle), value ) )
					throw new InvalidEnumArgumentException( "BorderStyle",
						(int)value,
						typeof(BorderStyle));
				
				if ( borderStyle != value ) {
					borderStyle = value;
					OnBorderStyleChanged ( EventArgs.Empty );
					RecreateHandle ( );
				}
			}
		}

		public bool CanUndo {
			get {
				if ( IsHandleCreated ) 
					return Win32.SendMessage ( Handle, (int) EditControlMessages.EM_CANUNDO, 0, 0 ) != 0; 
				return false;
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

		public bool HideSelection {
			get {	return flags[ hideSelection ];	}
			set {
				if ( flags[ hideSelection ] != value ) {
					flags[ hideSelection ] = value;
					RecreateHandle ( );
					OnHideSelectionChanged ( EventArgs.Empty );
				}
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

		public virtual int MaxLength {
			get { 
				if ( IsHandleCreated ) 
					maxLength = Win32.SendMessage ( Handle, (int) EditControlMessages.EM_GETLIMITTEXT, 0, 0 );
				return maxLength;
			}
			set {
				if ( value < 0 )
					throw new ArgumentException (
						string.Format ( "'{0}' is not a valid value for 'MaxLength'.  'MaxLength' must be greater than or equal to 0.", value ),
						"MaxLength" );

				maxLength = value;
				if ( IsHandleCreated )
					Win32.SendMessage ( Handle, (int) EditControlMessages.EM_LIMITTEXT, maxLength, 0 );
			}
		}

		public bool Modified {
			get {
				if ( IsHandleCreated )
					flags[ modified ] = Win32.SendMessage ( Handle, (int) EditControlMessages.EM_GETMODIFY, 0, 0 ) != 0;
				return flags[ modified ];
			}
			set {
				if ( flags[ modified ] != value ) {
					flags[ modified ] = value;
					if ( IsHandleCreated )
						Win32.SendMessage ( Handle, (int) EditControlMessages.EM_SETMODIFY, flags[ modified ] ? 1 : 0, 0 );
					OnModifiedChanged ( EventArgs.Empty );
				}
			}
		}

		public virtual bool Multiline {
			get { return flags[ multiline ]; }
			set {
				if ( flags[ multiline ] != value ) {
					flags[ multiline ] = value;
					RecreateHandle ( );
					OnMultilineChanged ( EventArgs.Empty );
				}
			}
		}

		[MonoTODO]
		public int PreferredHeight {
			get
			{
				throw new NotImplementedException ();
			}
		}

		public bool ReadOnly {
			get {	return flags[ readOnly ]; }
			set {
				if ( flags[ readOnly ] != value ) {
					flags[ readOnly ] = value;
					if ( IsHandleCreated )
						Win32.SendMessage ( Handle, (int) EditControlMessages.EM_SETREADONLY,
									flags[ readOnly ] ? 1 : 0, 0 );
					OnReadOnlyChanged ( EventArgs.Empty );
				}
			}
		}

		public virtual string SelectedText {
			get {
				if ( SelectionStart < 0 || SelectionStart >= TextLength || SelectionLength == 0 )
					return String.Empty;

				return Text.Substring ( SelectionStart,
							SelectionStart + SelectionLength <= Text.Length ?
							SelectionLength : Text.Length - SelectionStart );
				
			}
			set {
				if ( IsHandleCreated )
					Win32.SendMessage ( Handle, (int) EditControlMessages.EM_REPLACESEL, -1, value );
				else {
					if ( SelectionStart >= 0 && SelectionStart < TextLength && SelectionLength != 0 ) {
						Text = 	Text.Remove (   SelectionStart,
									SelectionStart + SelectionLength <= Text.Length ?
									SelectionLength : Text.Length - SelectionStart )
									.Insert (   SelectionStart, value );
					}
				}
			}
		}

		public virtual int SelectionLength {
			get {
				if ( IsHandleCreated ) {
					int selectionEnd = 0;
					Win32.SendMessage2ref ( Handle, (int) EditControlMessages.EM_GETSEL, ref selectionStart, ref selectionEnd );
					selectionLength = selectionEnd - selectionStart;
				}
				return selectionLength;
			}
			set {
			 	if ( value < 0 )
					throw new ArgumentException (
						String.Format ( "'{0}' is not a valid value for 'value'.", value ), "SelectionLength" );
				
				selectionLength = value;
				selectImpl ( SelectionStart, selectionLength );
			}
		}

		public int SelectionStart {
			get {
				if ( IsHandleCreated ) {
					int selectionEnd = 0;
					Win32.SendMessage2ref ( Handle, (int) EditControlMessages.EM_GETSEL, ref selectionStart, ref selectionEnd );
				}
				return selectionStart;
			}
			set {
				if ( value < 0 )
					throw new ArgumentException (
							String.Format ( "'{0}' is not a valid value for 'value'.", value ), "SelectionStart" );

				selectionStart = value;
				selectImpl ( selectionStart, SelectionLength );
			}
		}

		[MonoTODO]
		public override string Text {
			get {
				return base.Text;
			}
			set
			{
				//FIXME:
				base.Text = value;
			}
		}

		public virtual int TextLength  {
			get {	return Text.Length; }
		}

		public bool WordWrap {
			get {	return flags[ wordWrap ]; }
			set {
				if ( flags[ wordWrap ] != value ) {
					flags[ wordWrap ] = value;
					RecreateHandle ( );
				}
			}
		}
		
		// --- Public Methods
		
		[MonoTODO]
		public void AppendText(string text) 
		{
			if ( !IsHandleCreated )
				Text += text;
			else {
				selectImpl ( TextLength, 1 );
				if ( IsHandleCreated )
					Win32.SendMessage ( Handle, (int) EditControlMessages.EM_REPLACESEL, -1, text );
			}
		}
		
		public void Clear()
		{
			Text = String.Empty;
		}

		public void ClearUndo()
		{
			if ( IsHandleCreated )
				Win32.SendMessage ( Handle, (int) EditControlMessages.EM_EMPTYUNDOBUFFER, 0, 0 );
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

		public void Select(int start, int length) 
		{
			if ( start < 0 )
				throw new ArgumentException ( 
					String.Format ( " '{0}' is not a valid value for 'start'.", start ) );

			if ( length < 0 )
				throw new ArgumentException ( 
					String.Format ( " '{0}' is not a valid value for 'length'.", length ) );

			selectImpl ( start, length );
		}
		
		public void SelectAll()
		{
			Select ( 0, TextLength );
		}

		public override string ToString()
		{
			return GetType( ).FullName.ToString ( ) + ", Text: " + Text;
		}

		public void Undo()
		{
			if ( IsHandleCreated )
				Win32.SendMessage ( Handle, (int) EditControlMessages.EM_UNDO, 0, 0 );
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
				CreateParams createParams = base.CreateParams;
				
				switch ( BorderStyle ) {
				case BorderStyle.Fixed3D:
					createParams.ExStyle |= (int)WindowExStyles.WS_EX_CLIENTEDGE;
				break;
				case BorderStyle.FixedSingle:
					createParams.Style |= (int) WindowStyles.WS_BORDER;
				break;
				};

				if ( !HideSelection )
					createParams.Style |= (int) EditControlStyles.ES_NOHIDESEL;

				if ( Multiline ) {
					createParams.Style |= (int) EditControlStyles.ES_MULTILINE;
					createParams.Style |= (int) EditControlStyles.ES_AUTOVSCROLL;
				}

				if ( !WordWrap )
					createParams.Style |= (int) EditControlStyles.ES_AUTOHSCROLL;

				return createParams; 
			}
		}

		protected override Size DefaultSize {
			get { return new Size(100,20); }
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

		protected virtual void OnAcceptsTabChanged(EventArgs e)
		{
			if ( AcceptsTabChanged != null )
				AcceptsTabChanged ( this, e );
		}

		protected virtual void OnAutoSizeChanged(EventArgs e)
		{
			if ( AutoSizeChanged != null )
				AutoSizeChanged ( this, e );
		}

		protected virtual void OnBorderStyleChanged(EventArgs e)
		{
			if ( BorderStyleChanged != null )
				BorderStyleChanged ( this, e );			
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

			if ( maxLength != 0 )
				Win32.SendMessage ( Handle, (int) EditControlMessages.EM_LIMITTEXT, maxLength, 0 );
			if ( flags[ modified ] )
				Win32.SendMessage ( Handle, (int) EditControlMessages.EM_SETMODIFY, flags[ modified ] ? 1 : 0, 0 );
			if ( ReadOnly )
				Win32.SendMessage ( Handle, (int) EditControlMessages.EM_SETREADONLY, ReadOnly ? 1 : 0, 0 );
			
			selectImpl ( selectionStart, selectionLength );
		}

		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e)
		{
			//FIXME:
			base.OnHandleDestroyed(e);
		}

		protected virtual void OnHideSelectionChanged(EventArgs e)
		{
			if ( HideSelectionChanged != null )
				HideSelectionChanged ( this, e );
		}

		protected virtual void OnModifiedChanged(EventArgs e)
		{
			if ( ModifiedChanged != null )
				ModifiedChanged ( this, e );
		}

		protected virtual void OnMultilineChanged(EventArgs e)
		{
			if ( MultilineChanged != null )
				MultilineChanged ( this, e );
		}

		protected virtual void OnReadOnlyChanged(EventArgs e)
		{
			if ( ReadOnlyChanged != null )
				ReadOnlyChanged ( this, e );
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
			switch ( m.Msg ) {
			case Msg.WM_CTLCOLOR:
				CallControlWndProc ( ref m );
			break;
			case Msg.WM_COMMAND:
				if ( m.HiWordWParam == (int) EditControlNotifications.EN_CHANGE )
					OnTextChanged ( EventArgs.Empty );
				CallControlWndProc ( ref m );
			break;
			default:
				base.WndProc(ref m);
			break;
			}
		}

		private void selectImpl ( int start, int length ) {
			if ( IsHandleCreated )
				Win32.SendMessage ( Handle, (int) EditControlMessages.EM_SETSEL, start, start + length );
			else {
				selectionStart = start;
				selectionLength = length;
			}
		}

	}
}

