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
// Copyright (c) 2006, Alexander Olk
//
// Authors:
//	Alexander Olk	alex.olk@googlemail.com
//
//

// NOT COMPLETE - work in progress

using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System;
using System.Collections;

namespace System.Windows.Forms
{
	[DefaultProperty( "Font" )]
	[DefaultEvent("Apply")]
	public class FontDialog : CommonDialog
	{
		protected static readonly object EventApply = new object ();

		private Font font;
		private Color color = Color.Black;
		private bool allowSimulations = true;
		private bool allowVectorFonts = true;
		private bool allowVerticalFonts = true;
		private bool allowScriptChange = true;
		private bool fixedPitchOnly = false;
		private int maxSize = 0;
		private int minSize = 0;
		private bool scriptsOnly = false;
		private bool showApply = false;
		private bool showColor = false;
		private bool showEffects = true;
		private bool showHelp = false;
		
		private bool fontMustExist = false;
		
		private Panel examplePanel;
		
		private Button okButton;
		private Button cancelButton;
		private Button applyButton;
		private Button helpButton;
		
		private TextBox fontTextBox;
		private TextBox fontstyleTextBox;
		private TextBox fontsizeTextBox;
		
		private MouseWheelListBox fontListBox;
		private MouseWheelListBox fontstyleListBox;
		private MouseWheelListBox fontsizeListBox;
		
		private GroupBox effectsGroupBox;
		private CheckBox strikethroughCheckBox;
		private CheckBox underlinedCheckBox;
		private ComboBox scriptComboBox;
		
		private Label fontLabel;
		private Label fontstyleLabel;
		private Label sizeLabel;
		private Label scriptLabel;
		
		private GroupBox exampleGroupBox;
		
		private ColorComboBox colorComboBox;
		
		private string currentFontName;
		
		private float currentSize;
		
		private FontFamily currentFamily;
		
		private FontStyle currentFontStyle;
		
		private bool underlined = false;
		private bool strikethrough = false;
		
		private Hashtable fontHash = new Hashtable();
		
		private int[] a_sizes = {
			6, 7, 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72
		};
		
		// char set stuff is only here to make me happy :-)
		private string [] char_sets_names = {
			"Western",
			"Symbol",
			"Shift Jis",
			"Hangul",
			"GB2312",
			"BIG5",
			"Greek",
			"Turkish",
			"Hebrew",
			"Arabic",
			"Baltic",
			"Vietname",
			"Cyrillic",
			"East European",
			"Thai",
			"Johab",
			"Mac",
			"OEM",
			"VISCII",
			"TCVN",
			"KOI-8",
			"ISO-8859-3",
			"ISO-8859-4",
			"ISO-8859-10",
			"Celtic"
		};
		
		private string [] char_sets = {
			"AaBbYyZz",
			"Symbol",
			"Aa" + (char)0x3042 + (char)0x3041 + (char)0x30a2  + (char)0x30a1 + (char)0x4e9c + (char)0x5b87,
			(char)0xac00 + (char)0xb098 + (char)0xb2e4 + "AaBYyZz",
			new String(new Char [] {(char)0x5fae, (char)0x8f6f, (char)0x4e2d, (char)0x6587, (char)0x8f6f, (char)0x4ef6}),
			new String(new Char [] {(char)0x4e2d, (char)0x6587, (char)0x5b57, (char)0x578b, (char)0x7bc4, (char)0x4f8b}),
			"AaBb" + (char)0x0391 + (char)0x03b1 + (char)0x0392 + (char)0x03b2,
			"AaBb" + (char)0x011e + (char)0x011f + (char)0x015e + (char)0x015f,
			"AaBb" + (char)0x05e0 + (char)0x05e1 + (char)0x05e9 + (char)0x05ea,
			"AaBb" + (char)0x0627 + (char)0x0628 + (char)0x062c + (char)0x062f + (char)0x0647 + (char)0x0648 + (char)0x0632,
			"AaBbYyZz",
			"AaBb" + (char)0x01a0 + (char)0x01a1 + (char)0x01af + (char)0x01b0,
			"AaBb" + (char)0x0411 + (char)0x0431 + (char)0x0424 + (char)0x0444,
			"AaBb" + (char)0xc1 + (char)0xe1 + (char)0xd4 + (char)0xf4,
			"AaBb" + (char)0x0e2d + (char)0x0e31 + (char)0x0e01 + (char)0x0e29 + (char)0x0e23 + (char)0x0e44 + (char)0x0e17 +(char)0x0e22,
			(char)0xac00 + (char)0xb098 + (char)0xb2e4 + "AaBYyZz",
			"AaBbYyZz",
			"AaBb" + (char)0xf8 + (char)0xf1 + (char)0xfd,
			"",
			"",
			"",
			"",
			"",
			"",
			""
		};
		
		private string example_panel_text;
		
		private bool internal_change = false;
		
		#region Public Constructors
		public FontDialog( )
		{
			form = new DialogForm (this);
			example_panel_text = char_sets [0];
			
			okButton = new Button( );
			cancelButton = new Button( );
			applyButton = new Button( );
			helpButton = new Button( );
			
			fontTextBox = new TextBox( );
			fontstyleTextBox = new TextBox( );
			fontsizeTextBox = new TextBox( );
			
			fontListBox = new MouseWheelListBox ();
			fontsizeListBox = new MouseWheelListBox ();
			fontstyleListBox = new MouseWheelListBox ();
			
			fontLabel = new Label( );
			fontstyleLabel = new Label( );
			sizeLabel = new Label( );
			scriptLabel = new Label( );
			
			exampleGroupBox = new GroupBox( );
			
			effectsGroupBox = new GroupBox( );
			underlinedCheckBox = new CheckBox( );
			strikethroughCheckBox = new CheckBox( );
			scriptComboBox = new ComboBox( );
			
			examplePanel = new Panel( );
			
			colorComboBox = new ColorComboBox( this );
			
			exampleGroupBox.SuspendLayout( );
			effectsGroupBox.SuspendLayout( );
			form.SuspendLayout( );
			
			form.FormBorderStyle = FormBorderStyle.FixedDialog;
			form.MaximizeBox = false;
			
			// fontsizeListBox
			fontsizeListBox.Location = new Point( 284, 47 );
			fontsizeListBox.Size = new Size( 52, 95 );
			fontsizeListBox.TabIndex = 10;
			fontListBox.Sorted = true;
			// fontTextBox
			fontTextBox.Location = new Point( 16, 26 );
			fontTextBox.Size = new Size( 140, 21 );
			fontTextBox.TabIndex = 5;
			fontTextBox.Text = "";
			// fontstyleLabel
			fontstyleLabel.Location = new Point( 164, 10 );
			fontstyleLabel.Size = new Size( 100, 16 );
			fontstyleLabel.TabIndex = 1;
			fontstyleLabel.Text = "Font Style:";
			// typesizeTextBox
			fontsizeTextBox.Location = new Point( 284, 26 );
			fontsizeTextBox.Size = new Size( 52, 21 );
			fontsizeTextBox.TabIndex = 7;
			fontsizeTextBox.Text = "";
			fontsizeTextBox.MaxLength = 2;
			// schriftartListBox
			fontListBox.Location = new Point( 16, 47 );
			fontListBox.Size = new Size( 140, 95 );
			fontListBox.TabIndex = 8;
			fontListBox.Sorted = true;
			// exampleGroupBox
			exampleGroupBox.Controls.Add( examplePanel );
			exampleGroupBox.FlatStyle = FlatStyle.System;
			exampleGroupBox.Location = new Point( 164, 158 );
			exampleGroupBox.Size = new Size( 172, 70 );
			exampleGroupBox.TabIndex = 12;
			exampleGroupBox.TabStop = false;
			exampleGroupBox.Text = "Example";
			// fontstyleListBox
			fontstyleListBox.Location = new Point( 164, 47 );
			fontstyleListBox.Size = new Size( 112, 95 );
			fontstyleListBox.TabIndex = 9;
			// schriftartLabel
			fontLabel.Location = new Point( 16, 10 );
			fontLabel.Size = new Size( 88, 16 );
			fontLabel.TabIndex = 0;
			fontLabel.Text = "Font:";
			// effectsGroupBox
			effectsGroupBox.Controls.Add( underlinedCheckBox );
			effectsGroupBox.Controls.Add( strikethroughCheckBox );
			effectsGroupBox.Controls.Add( colorComboBox );
			effectsGroupBox.FlatStyle = FlatStyle.System;
			effectsGroupBox.Location = new Point( 16, 158 );
			effectsGroupBox.Size = new Size( 140, 116 );
			effectsGroupBox.TabIndex = 11;
			effectsGroupBox.TabStop = false;
			effectsGroupBox.Text = "Effects";
			// strikethroughCheckBox
			strikethroughCheckBox.FlatStyle = FlatStyle.System;
			strikethroughCheckBox.Location = new Point( 8, 16 );
			strikethroughCheckBox.TabIndex = 0;
			strikethroughCheckBox.Text = "Strikethrough";
			// colorComboBox
			colorComboBox.Location = new Point( 8, 70 );
			colorComboBox.Size = new Size( 130, 21 );
			// sizeLabel
			sizeLabel.Location = new Point( 284, 10 );
			sizeLabel.Size = new Size( 100, 16 );
			sizeLabel.TabIndex = 2;
			sizeLabel.Text = "Size:";
			// scriptComboBox
			scriptComboBox.Location = new Point( 164, 253 );
			scriptComboBox.Size = new Size( 172, 21 );
			scriptComboBox.TabIndex = 14;
			scriptComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			// okButton
			okButton.FlatStyle = FlatStyle.System;
			okButton.Location = new Point( 352, 26 );
			okButton.Size = new Size( 70, 23 );
			okButton.TabIndex = 3;
			okButton.Text = "OK";
			// cancelButton
			cancelButton.FlatStyle = FlatStyle.System;
			cancelButton.Location = new Point( 352, 52 );
			cancelButton.Size = new Size( 70, 23 );
			cancelButton.TabIndex = 4;
			cancelButton.Text = "Cancel";
			// applyButton
			applyButton.FlatStyle = FlatStyle.System;
			applyButton.Location = new Point( 352, 78 );
			applyButton.Size = new Size( 70, 23 );
			applyButton.TabIndex = 5;
			applyButton.Text = "Apply";
			// helpButton
			helpButton.FlatStyle = FlatStyle.System;
			helpButton.Location = new Point( 352, 104 );
			helpButton.Size = new Size( 70, 23 );
			helpButton.TabIndex = 6;
			helpButton.Text = "Help";
			// underlinedCheckBox
			underlinedCheckBox.FlatStyle = FlatStyle.System;
			underlinedCheckBox.Location = new Point( 8, 36 );
			underlinedCheckBox.TabIndex = 1;
			underlinedCheckBox.Text = "Underlined";
			// fontstyleTextBox
			fontstyleTextBox.Location = new Point( 164, 26 );
			fontstyleTextBox.Size = new Size( 112, 21 );
			fontstyleTextBox.TabIndex = 6;
			fontstyleTextBox.Text = "";
			// scriptLabel
			scriptLabel.Location = new Point( 164, 236 );
			scriptLabel.Size = new Size( 100, 16 );
			scriptLabel.TabIndex = 13;
			scriptLabel.Text = "Script:";
			// examplePanel
			examplePanel.Location = new Point( 8, 20 );
			examplePanel.TabIndex = 0;
			examplePanel.Size = new Size( 156, 40 );
			examplePanel.BorderStyle = BorderStyle.Fixed3D;
			
			form.AcceptButton = okButton;
			form.CancelButton = cancelButton;
			
			form.Controls.Add( scriptComboBox );
			form.Controls.Add( scriptLabel );
			form.Controls.Add( exampleGroupBox );
			form.Controls.Add( effectsGroupBox );
			form.Controls.Add( fontsizeListBox );
			form.Controls.Add( fontstyleListBox );
			form.Controls.Add( fontListBox );
			form.Controls.Add( fontsizeTextBox );
			form.Controls.Add( fontstyleTextBox );
			form.Controls.Add( fontTextBox );
			form.Controls.Add( cancelButton );
			form.Controls.Add( okButton );
			form.Controls.Add( sizeLabel );
			form.Controls.Add( fontstyleLabel );
			form.Controls.Add( fontLabel );
			form.Controls.Add( applyButton );
			form.Controls.Add( helpButton );
			
			exampleGroupBox.ResumeLayout( false );
			effectsGroupBox.ResumeLayout( false );
			
			form.Size = new Size( 430, 318 );
			
			form.FormBorderStyle = FormBorderStyle.FixedDialog;
			form.MaximizeBox = false;
			
			form.Text = "Font";
			
			form.ResumeLayout( false );
			
			scriptComboBox.BeginUpdate ();
			scriptComboBox.Items.AddRange (char_sets_names);
			scriptComboBox.SelectedIndex = 0;
			scriptComboBox.EndUpdate ();
			
			applyButton.Hide( );
			helpButton.Hide( );
			colorComboBox.Hide( );
			
			cancelButton.Click += new EventHandler( OnClickCancelButton );
			okButton.Click += new EventHandler( OnClickOkButton );
			applyButton.Click += new EventHandler (OnApplyButton);
			examplePanel.Paint += new PaintEventHandler( OnPaintExamplePanel );
			fontListBox.SelectedIndexChanged += new EventHandler( OnSelectedIndexChangedFontListBox );
			fontsizeListBox.SelectedIndexChanged += new EventHandler( OnSelectedIndexChangedSizeListBox );
			fontstyleListBox.SelectedIndexChanged += new EventHandler( OnSelectedIndexChangedFontStyleListBox );
			underlinedCheckBox.CheckedChanged += new EventHandler( OnCheckedChangedUnderlinedCheckBox );
			strikethroughCheckBox.CheckedChanged += new EventHandler( OnCheckedChangedStrikethroughCheckBox );
			scriptComboBox.SelectedIndexChanged += new EventHandler (OnSelectedIndexChangedScriptComboBox);
			
			fontTextBox.KeyPress += new KeyPressEventHandler (OnFontTextBoxKeyPress);
			fontstyleTextBox.KeyPress += new KeyPressEventHandler (OnFontStyleTextBoxKeyPress);
			fontsizeTextBox.KeyPress += new KeyPressEventHandler (OnFontSizeTextBoxKeyPress);
			
			fontTextBox.TextChanged += new EventHandler (OnFontTextBoxTextChanged);
			fontstyleTextBox.TextChanged += new EventHandler (OnFontStyleTextTextChanged);
			fontsizeTextBox.TextChanged += new EventHandler (OnFontSizeTextBoxTextChanged);
			
			fontTextBox.KeyDown += new KeyEventHandler (OnFontTextBoxKeyDown);
			fontstyleTextBox.KeyDown += new KeyEventHandler (OnFontStyleTextBoxKeyDown);
			fontsizeTextBox.KeyDown += new KeyEventHandler (OnFontSizeTextBoxKeyDown);
			
			fontTextBox.MouseWheel += new MouseEventHandler (OnFontTextBoxMouseWheel);
			fontstyleTextBox.MouseWheel += new MouseEventHandler (OnFontStyleTextBoxMouseWheel);
			fontsizeTextBox.MouseWheel += new MouseEventHandler (OnFontSizeTextBoxMouseWheel);
			
			PopulateFontList ();
		}
		#endregion	// Public Constructors
		
		#region Public Instance Properties
		public Font Font
		{
			get {
				return font;
			}
			
			set {
				if (value != null) {
					font = new Font(value, value.Style);
					
					currentFontStyle = font.Style;
					currentSize = font.SizeInPoints;
					currentFontName = font.Name;
					
					strikethroughCheckBox.Checked = font.Strikeout;
					underlinedCheckBox.Checked = font.Underline;
					
					int index = fontListBox.FindString (currentFontName);
					
					if (index != -1) {
						fontListBox.SelectedIndex = index;
					} else {
						fontListBox.SelectedIndex = 0;
					}
					
					UpdateFontSizeListBox ();
					UpdateFontStyleListBox ();
					
					fontListBox.TopIndex = fontListBox.SelectedIndex;
				}
			}
		}
		
		[DefaultValue(false)]
		public bool FontMustExist
		{
			get {
				return fontMustExist;
			}
			
			set {
				fontMustExist = value;
			}
		}

#if NET_2_0
		[DefaultValue ("Color [Black]")]
#endif
		public Color Color
		{
			set {
				color = value;
				examplePanel.Invalidate( );
			}
			
			get {
				return color;
			}
		}
		
		[DefaultValue(true)]
		public bool AllowSimulations
		{
			set {
				allowSimulations = value;
			}
			
			get {
				return allowSimulations;
			}
		}
		
		[DefaultValue(true)]
		public bool AllowVectorFonts
		{
			set {
				allowVectorFonts = value;
			}
			
			get {
				return allowVectorFonts;
			}
		}
		
		[DefaultValue(true)]
		public bool AllowVerticalFonts
		{
			set {
				allowVerticalFonts = value;
			}
			
			get {
				return allowVerticalFonts;
			}
		}
		
		[DefaultValue(true)]
		public bool AllowScriptChange
		{
			set {
				allowScriptChange = value;
			}
			
			get {
				return allowScriptChange;
			}
		}
		
		[DefaultValue(false)]
		public bool FixedPitchOnly
		{
			set {
				if (fixedPitchOnly != value) {
					fixedPitchOnly = value;
					PopulateFontList ();
				}
			}
			
			get {
				return fixedPitchOnly;
			}
		}
		
		[DefaultValue(0)]
		public int MaxSize
		{
			set {
				maxSize = value;
				
				if (maxSize < 0)
					maxSize = 0;
				
				if (maxSize < minSize)
					minSize = maxSize;
				
				CreateFontSizeListBoxItems ();
			}
			
			get {
				return maxSize;
			}
		}
		
		[DefaultValue(0)]
		public int MinSize
		{
			set {
				minSize = value;
				
				if (minSize < 0)
					minSize = 0;
				
				if (minSize > maxSize)
					maxSize = minSize;
				
				CreateFontSizeListBoxItems ();
				
				if (minSize > currentSize)
					if (font != null) {
						font.Dispose();
						
						currentSize = minSize;
						
						font = new Font( currentFamily, currentSize, currentFontStyle );
						
						UpdateExamplePanel ();
						
						fontsizeTextBox.Text = currentSize.ToString ();
					}
			}
			
			get {
				return minSize;
			}
		}
		
		[DefaultValue(false)]
		public bool ScriptsOnly
		{
			set {
				scriptsOnly = value;
			}
			
			get {
				return scriptsOnly;
			}
		}
		
		[DefaultValue(false)]
		public bool ShowApply
		{
			set {
				if (value != showApply)
				{
					showApply = value;
					if (showApply)
						applyButton.Show ();
					else
						applyButton.Hide ();
					
					form.Refresh();
				}
				
			}
			
			get {
				return showApply;
			}
		}
		
		[DefaultValue(false)]
		public bool ShowColor
		{
			set {
				if (value != showColor)
				{
					showColor = value;
					if (showColor)
						colorComboBox.Show ();
					else
						colorComboBox.Hide ();
					
					form.Refresh();
				}
			}
			
			get {
				return showColor;
			}
		}
		
		[DefaultValue(true)]
		public bool ShowEffects
		{
			set {
				if (value != showEffects)
				{
					showEffects = value;
					if (showEffects)
						effectsGroupBox.Show ();
					else
						effectsGroupBox.Hide ();
					
					form.Refresh();
				}
			}
			
			get {
				return showEffects;
			}
		}
		
		[DefaultValue(false)]
		public bool ShowHelp
		{
			set {
				if (value != showHelp)
				{
					showHelp = value;
					if (showHelp)
						helpButton.Show ();
					else
						helpButton.Hide ();
					
					form.Refresh();
				}
			}
			
			get {
				return showHelp;
			}
		}
		
		#endregion	// Public Instance Properties
		
		#region Protected Instance Properties
		protected int Options {
			get { return 0; }
		}
		#endregion	// Protected Instance Properties
		
		#region Public Instance Methods
		public override void Reset( )
		{
			color = Color.Black;
			allowSimulations = true;
			allowVectorFonts = true;
			allowVerticalFonts = true;
			allowScriptChange = true;
			fixedPitchOnly = false;
			
			maxSize = 0;
			minSize = 0;
			CreateFontSizeListBoxItems ();
			
			scriptsOnly = false;
			
			showApply = false;
			applyButton.Hide ();
			
			showColor = false;
			colorComboBox.Hide ();
			
			showEffects = true;
			effectsGroupBox.Show ();
			
			showHelp = false;
			helpButton.Hide ();
			
			form.Refresh ();
		}

		public override string ToString ()
		{
			if (font == null)
				return base.ToString ();
			return String.Concat (base.ToString (), ", Font: ", font.ToString ());
		}
		#endregion	// Public Instance Methods
		
		#region Protected Instance Methods
		protected override IntPtr HookProc (IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
		{
			return base.HookProc (hWnd, msg, wparam, lparam);
		}

		protected override bool RunDialog( IntPtr hWndOwner )
		{
			form.Refresh();
			
			return true;
		}

		internal void OnApplyButton (object sender, EventArgs e)
		{
			OnApply (e);
		}

		protected virtual void OnApply (EventArgs e)
		{
			EventHandler apply = (EventHandler) Events [EventApply];
			if (apply != null)
				apply (this, e);
		}
		#endregion	// Protected Instance Methods
		
		void OnClickCancelButton( object sender, EventArgs e )
		{
			form.DialogResult = DialogResult.Cancel;
		}
		
		void OnClickOkButton( object sender, EventArgs e )
		{
			form.DialogResult = DialogResult.OK;
		}
		
		void OnPaintExamplePanel( object sender, PaintEventArgs e )
		{
			SolidBrush brush = ThemeEngine.Current.ResPool.GetSolidBrush( color );
			
			e.Graphics.FillRectangle( ThemeEngine.Current.ResPool.GetSolidBrush( SystemColors.Control ), 0, 0, 156, 40 );
			
			SizeF fontSizeF = e.Graphics.MeasureString( example_panel_text, font );
			
			int text_width = (int)fontSizeF.Width;
			int text_height = (int)fontSizeF.Height;
			
			int x = ( examplePanel.Width / 2 ) - ( text_width / 2 );
			if ( x < 0 ) x = 0;
			
			int y = ( examplePanel.Height / 2 ) - ( text_height / 2 );
			
			e.Graphics.DrawString( example_panel_text, font, brush, new Point( x, y ) );
		}
		
		void OnSelectedIndexChangedFontListBox( object sender, EventArgs e )
		{
			if ( fontListBox.SelectedIndex != -1 )
			{
				currentFamily = FindByName( fontListBox.Items[ fontListBox.SelectedIndex ].ToString( ) );
				
				fontTextBox.Text = currentFamily.Name;
				
				internal_change = true;
				
				UpdateFontStyleListBox( );
				
				UpdateFontSizeListBox ();
				
				UpdateExamplePanel ();
				
				form.Select(fontTextBox);
				
				internal_change = false;
			}
		}
		
		void OnSelectedIndexChangedSizeListBox( object sender, EventArgs e )
		{
			if ( fontsizeListBox.SelectedIndex != -1 )
			{
				currentSize = (float)System.Convert.ToDouble( fontsizeListBox.Items[ fontsizeListBox.SelectedIndex ] );
				
				fontsizeTextBox.Text = currentSize.ToString( );
				
				UpdateExamplePanel( );
				
				if (!internal_change)
					form.Select(fontsizeTextBox);
			}
		}
		
		void OnSelectedIndexChangedFontStyleListBox( object sender, EventArgs e )
		{
			if ( fontstyleListBox.SelectedIndex != -1 )
			{
				switch ( fontstyleListBox.SelectedIndex )
				{
				case 0:
					currentFontStyle = FontStyle.Regular;
					break;
				case 1:
					currentFontStyle = FontStyle.Bold;
					break;
				case 2:
					currentFontStyle = FontStyle.Italic;
					break;
				case 3:
					currentFontStyle = FontStyle.Bold | FontStyle.Italic;
					break;
				default:
					currentFontStyle = FontStyle.Regular;
					break;
				}
				
				if (underlined) 
					currentFontStyle = currentFontStyle | FontStyle.Underline;
				
				if (strikethrough)
					currentFontStyle = currentFontStyle | FontStyle.Strikeout;
				
				fontstyleTextBox.Text = fontstyleListBox.Items[ fontstyleListBox.SelectedIndex ].ToString( );
				
				if (!internal_change) {
					UpdateExamplePanel( );
					
					form.Select(fontstyleTextBox);
				}
			}
		}
		
		void OnCheckedChangedUnderlinedCheckBox( object sender, EventArgs e )
		{
			if ( underlinedCheckBox.Checked ) {
				currentFontStyle = currentFontStyle | FontStyle.Underline;
				underlined = true;
			}
			else {
				currentFontStyle = currentFontStyle ^ FontStyle.Underline;
				underlined = false;
			}
			
			UpdateExamplePanel( );
		}
		
		void OnCheckedChangedStrikethroughCheckBox( object sender, EventArgs e )
		{
			if ( strikethroughCheckBox.Checked ) {
				currentFontStyle = currentFontStyle | FontStyle.Strikeout;
				strikethrough = true;
			}
			else {
				currentFontStyle = currentFontStyle ^ FontStyle.Strikeout;
				strikethrough = false;
			}
			
			UpdateExamplePanel( );
		}
		
		bool internal_textbox_change = false;
		
		void OnFontTextBoxMouseWheel (object sender, MouseEventArgs e)
		{
			fontListBox.SendMouseWheelEvent (e);
		}
		
		void OnFontStyleTextBoxMouseWheel (object sender, MouseEventArgs e)
		{
			fontstyleListBox.SendMouseWheelEvent (e);
		}
		
		void OnFontSizeTextBoxMouseWheel (object sender, MouseEventArgs e)
		{
			fontsizeListBox.SendMouseWheelEvent (e);
		}
		
		void OnFontTextBoxKeyDown (object sender, KeyEventArgs e)
		{
			// Forward these keys on to the font style listbox
			switch (e.KeyCode) {
				case Keys.Up:
				case Keys.Down:
				case Keys.PageDown:
				case Keys.PageUp:
					fontListBox.HandleKeyDown (e.KeyCode);
					break;
			}
		}
		
		void OnFontStyleTextBoxKeyDown (object sender, KeyEventArgs e)
		{
			// Forward these keys on to the font style listbox
			switch (e.KeyCode) {
				case Keys.Up:
				case Keys.Down:
				case Keys.PageDown:
				case Keys.PageUp:
					fontstyleListBox.HandleKeyDown (e.KeyCode);
					break;
			}
		}
		
		void OnFontSizeTextBoxKeyDown (object sender, KeyEventArgs e)
		{
			// Forward these keys on to the font size listbox
			switch (e.KeyCode) {
				case Keys.Up:
				case Keys.Down:
				case Keys.PageDown:
				case Keys.PageUp:
					fontsizeListBox.HandleKeyDown (e.KeyCode);
					break;
			}
		}
		
		void OnFontTextBoxKeyPress (object sender, KeyPressEventArgs e)
		{
			internal_textbox_change = true;
			
			if (fontListBox.SelectedIndex > -1)
				fontListBox.SelectedIndex = -1;
		}
		
		void OnFontStyleTextBoxKeyPress (object sender, KeyPressEventArgs e)
		{
			internal_textbox_change = true;
			
			if (fontstyleListBox.SelectedIndex > -1)
				fontstyleListBox.SelectedIndex = -1;
		}
		
		void OnFontSizeTextBoxKeyPress (object sender, KeyPressEventArgs e)
		{
			if (Char.IsLetter (e.KeyChar) || Char.IsWhiteSpace (e.KeyChar) || Char.IsPunctuation (e.KeyChar) || e.KeyChar == ',') {
				e.Handled = true;
				return; 
			}
			
			internal_textbox_change = true;
		}
		
		void OnFontTextBoxTextChanged (object sender, EventArgs e)
		{
			if (!internal_textbox_change)
				return;
			
			internal_textbox_change = false;
			
			string search = fontTextBox.Text;
			
			// Look for an exact match
			int found = fontListBox.FindStringExact (search);
			
			if (found != ListBox.NoMatches) {
				fontListBox.SelectedIndex = found;
				return;
			}
			
			// Look for a partial match
			found = fontListBox.FindString (search);
			
			if (found != ListBox.NoMatches) {
				fontListBox.TopIndex = found;
				return;
			}

			// No match, scroll to the top
			if (fontListBox.Items.Count > 0)
				fontListBox.TopIndex = 0;
		}
		
		void OnFontStyleTextTextChanged (object sender, EventArgs e)
		{
			if (!internal_textbox_change)
				return;
			
			internal_textbox_change = false;

			// Look for an exact match
			int found = fontstyleListBox.FindStringExact (fontstyleTextBox.Text);

			if (found != ListBox.NoMatches)
				fontstyleListBox.SelectedIndex = found;
		}
		
		void OnFontSizeTextBoxTextChanged (object sender, EventArgs e)
		{
			if (!internal_textbox_change)
				return;
			
			internal_textbox_change = false;
			
			if (fontsizeTextBox.Text.Length == 0)
				return;
			
			for (int i = 0; i < fontsizeListBox.Items.Count; i++) {
				string name = fontsizeListBox.Items [i] as string;
				
				if (name.StartsWith(fontsizeTextBox.Text)) {
					if (name == fontsizeTextBox.Text)
						fontsizeListBox.SelectedIndex = i;
					else
						fontsizeListBox.TopIndex = i;
					
					break;
				}
			}
		}
		
		void OnSelectedIndexChangedScriptComboBox (object sender, EventArgs e)
		{
			string tmp_str = char_sets [scriptComboBox.SelectedIndex];
			
			if (tmp_str.Length > 0) {
				example_panel_text = tmp_str;
				
				UpdateExamplePanel ();
			}
		}
		
		void UpdateExamplePanel( )
		{
			if (font != null)
				font.Dispose();
			
			font = new Font( currentFamily, currentSize, currentFontStyle );
			
			examplePanel.Invalidate( );
		}
		
		void UpdateFontSizeListBox ()
		{
			int index = fontsizeListBox.FindString(((int)Math.Round ((currentSize))).ToString());
			
			if (index != -1)
				fontsizeListBox.SelectedIndex = index;
			else 
				fontsizeListBox.SelectedIndex = 0;
		}
		
		void UpdateFontStyleListBox( )
		{
			// don't know if that works, IsStyleAvailable returns true for all styles under X
			
			fontstyleListBox.BeginUpdate( );
			
			fontstyleListBox.Items.Clear( );
			
			int index = -1;
			int to_select = 0;
			
			if ( currentFamily.IsStyleAvailable( FontStyle.Regular ) )
			{
				index = fontstyleListBox.Items.Add( "Regular" );
				
				if ((currentFontStyle & FontStyle.Regular) == FontStyle.Regular)
					to_select = index;
			}
			
			if ( currentFamily.IsStyleAvailable( FontStyle.Bold ) )
			{
				index = fontstyleListBox.Items.Add( "Bold" );
				
				if ((currentFontStyle & FontStyle.Bold) == FontStyle.Bold)
					to_select = index;
			}
			
			if ( currentFamily.IsStyleAvailable( FontStyle.Italic ) )
			{
				index = fontstyleListBox.Items.Add( "Italic" );
				
				if ((currentFontStyle & FontStyle.Italic) == FontStyle.Italic)
					to_select = index;
			}
			
			if ( currentFamily.IsStyleAvailable( FontStyle.Bold ) && currentFamily.IsStyleAvailable( FontStyle.Italic ) )
			{
				index = fontstyleListBox.Items.Add( "Bold Italic" );
				
				if ((currentFontStyle & (FontStyle.Bold | FontStyle.Italic)) == (FontStyle.Bold | FontStyle.Italic))
					to_select = index;
			}
			
			if (fontstyleListBox.Items.Count > 0) {
				fontstyleListBox.SelectedIndex = to_select;

				switch ((string)fontstyleListBox.SelectedItem) {
					case "Regular":
						currentFontStyle = FontStyle.Regular;
						break;
					case "Bold":
						currentFontStyle = FontStyle.Bold;
						break;
					case "Italic":
						currentFontStyle = FontStyle.Italic;
						break;
					case "Bold Italic":
						currentFontStyle = FontStyle.Bold | FontStyle.Italic;
						break;
				}
				
				if (strikethroughCheckBox.Checked)
					currentFontStyle |= FontStyle.Strikeout;
				if (underlinedCheckBox.Checked)
					currentFontStyle |= FontStyle.Underline;
			}
			
			fontstyleListBox.EndUpdate( );
		}
		
		FontFamily FindByName( string name )
		{
			return fontHash[ name ] as FontFamily;
		}
		
		void CreateFontSizeListBoxItems ()
		{
			fontsizeListBox.BeginUpdate ();
			
			fontsizeListBox.Items.Clear();
			
			if (minSize == 0 && maxSize == 0)
			{
				foreach (int i in a_sizes)
					fontsizeListBox.Items.Add (i.ToString());
			} else {
				foreach (int i in a_sizes) {
					if (i >= minSize && i <= maxSize)
						fontsizeListBox.Items.Add (i.ToString());
				}
			}
			
			fontsizeListBox.EndUpdate ();
		}

		#region Private Methods
		private void PopulateFontList ()
		{
			fontListBox.Items.Clear ();
			fontHash.Clear ();

			fontListBox.BeginUpdate ();

			foreach (FontFamily ff in FontFamily.Families) {
				if (!fontHash.ContainsKey (ff.Name)) {
					if (!fixedPitchOnly || (IsFontFamilyFixedPitch (ff))) {
						fontListBox.Items.Add (ff.Name);
						fontHash.Add (ff.Name, ff);
					}
				}
			}
			
			fontListBox.EndUpdate ();
			CreateFontSizeListBoxItems ();

			if (fixedPitchOnly)
				this.Font = new Font (FontFamily.GenericMonospace, 8.25f);
			else
				this.Font = form.Font;	
		}
		
		private bool IsFontFamilyFixedPitch (FontFamily family)
		{
			FontStyle fs;
			
			if (family.IsStyleAvailable (FontStyle.Regular))
				fs = FontStyle.Regular;
			else if (family.IsStyleAvailable (FontStyle.Bold))
				fs = FontStyle.Bold;
			else if (family.IsStyleAvailable (FontStyle.Italic))
				fs = FontStyle.Italic;
			else if (family.IsStyleAvailable (FontStyle.Strikeout))
				fs = FontStyle.Strikeout;
			else if (family.IsStyleAvailable (FontStyle.Underline))
				fs = FontStyle.Underline;
			else
				return false;

			Font f = new Font (family.Name, 10, fs);

			if (TextRenderer.MeasureString ("i", f).Width == TextRenderer.MeasureString ("w", f).Width)
				return true;
				
			return false;
		}
		#endregion
		
		internal class ColorComboBox : ComboBox
		{
			internal class ColorComboBoxItem
			{
				private Color color;
				private string name;
				
				public ColorComboBoxItem( Color color, string name )
				{
					this.color = color;
					this.name = name;
				}
				
				public Color Color
				{
					set {
						color = value;
					}
					
					get {
						return color;
					}
				}
				
				public string Name
				{
					set {
						name = value;
					}
					
					get {
						return name;
					}
				}

				public override string ToString()
				{
					return this.Name;
				}
			}
			
			private Color selectedColor;
			
			private FontDialog fontDialog;
			
			public ColorComboBox( FontDialog fontDialog )
			{
				this.fontDialog = fontDialog;
				
				DropDownStyle = ComboBoxStyle.DropDownList;
				DrawMode = DrawMode.OwnerDrawFixed;
				
				Items.AddRange( new object[] {
						       new ColorComboBoxItem( Color.Black, "Black" ),
						       new ColorComboBoxItem( Color.DarkRed, "Dark-Red" ),
						       new ColorComboBoxItem( Color.Green, "Green" ),
						       new ColorComboBoxItem( Color.Olive, "Olive-Green" ), // color not correct
						       new ColorComboBoxItem( Color.Aquamarine, "Aquamarine" ), // color not correct
						       new ColorComboBoxItem( Color.Crimson, "Crimson" ),
						       new ColorComboBoxItem( Color.Cyan, "Cyan" ),
						       new ColorComboBoxItem( Color.Gray, "Gray" ),
						       new ColorComboBoxItem( Color.Silver, "Silver" ),
						       new ColorComboBoxItem( Color.Red, "Red" ),
						       new ColorComboBoxItem( Color.YellowGreen, "Yellow-Green" ),
						       new ColorComboBoxItem( Color.Yellow, "Yellow" ),
						       new ColorComboBoxItem( Color.Blue, "Blue" ),
						       new ColorComboBoxItem( Color.Purple, "Purple" ),
						       new ColorComboBoxItem( Color.Aquamarine, "Aquamarine" ),
						       new ColorComboBoxItem( Color.White, "White" ) }
					       );
				
				SelectedIndex = 0;
				MaxDropDownItems = 16;
			}
			
			protected override void OnDrawItem( DrawItemEventArgs e )
			{
				if ( e.Index == -1 )
					return;
				
				ColorComboBoxItem ccbi = Items[ e.Index ] as ColorComboBoxItem;
				
				Rectangle r = e.Bounds;
				r.X = r.X + 24;
				
				if ( ( e.State & DrawItemState.Selected ) == DrawItemState.Selected )
				{
					e.Graphics.FillRectangle( ThemeEngine.Current.ResPool.GetSolidBrush( Color.Blue ), e.Bounds ); // bot blue
					e.Graphics.FillRectangle( ThemeEngine.Current.ResPool.GetSolidBrush( ccbi.Color ), e.Bounds.X + 3, e.Bounds.Y + 3, e.Bounds.X + 16, e.Bounds.Bottom - 3 );
					e.Graphics.DrawRectangle( ThemeEngine.Current.ResPool.GetPen( Color.Black ), e.Bounds.X + 2, e. Bounds.Y + 2, e.Bounds.X + 17, e.Bounds.Bottom - 3 );
					e.Graphics.DrawString( ccbi.Name, this.Font, ThemeEngine.Current.ResPool.GetSolidBrush( Color.White ), r );
				}
				else
				{
					e.Graphics.FillRectangle( ThemeEngine.Current.ResPool.GetSolidBrush( Color.White ), e.Bounds );
					e.Graphics.FillRectangle( ThemeEngine.Current.ResPool.GetSolidBrush( ccbi.Color ), e.Bounds.X + 3, e.Bounds.Y + 3, e.Bounds.X + 16, e.Bounds.Bottom - 3 );
					e.Graphics.DrawRectangle( ThemeEngine.Current.ResPool.GetPen( Color.Black ), e.Bounds.X + 2, e. Bounds.Y + 2, e.Bounds.X + 17, e.Bounds.Bottom - 3 );
					e.Graphics.DrawString( ccbi.Name, this.Font, ThemeEngine.Current.ResPool.GetSolidBrush( Color.Black ), r );
				}
			}
			
			protected override void OnSelectedIndexChanged( EventArgs e )
			{
				ColorComboBoxItem ccbi = Items[ SelectedIndex ] as ColorComboBoxItem;
				selectedColor = ccbi.Color;
				
				fontDialog.Color = selectedColor;
			}
		}

		public event EventHandler Apply {
			add { Events.AddHandler (EventApply, value); }
			remove { Events.RemoveHandler (EventApply, value); }
		}
	}
		
	internal class MouseWheelListBox : ListBox
	{
		public void SendMouseWheelEvent(MouseEventArgs e)
		{
			OnMouseWheel (e);
		}
	}
}

