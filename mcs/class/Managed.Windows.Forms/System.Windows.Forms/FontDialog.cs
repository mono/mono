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
// Copyright (c) 2004 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Alexander Olk	xenomorph2@onlinehome.de
//
//

// NOT COMPLETE - work in progress

// TODO:
// - apply button
// - help button
// - color combobox
// - correct drawing of the selected font in examplePanel
// - select values for font/style/size via the TextBoxes
// - etc

using System.Drawing;

namespace System.Windows.Forms
{
	public class FontDialog : CommonDialog
	{
		private FontDialogPanel fontDialogPanel;
		
		private Font font;
		private Color color = Color.Black;
		
		#region Public Constructors
		public FontDialog( )
		{
			form.ClientSize = new Size( 430, 318 );
			
			form.Size = new Size( 430, 318 );
			
			form.Text = "Font";
			
			fontDialogPanel = new FontDialogPanel( this );
			
			
		}
		#endregion	// Public Constructors
		
		#region Public Instance Properties
		public Font Font
		{
			get {
				return font;
			}
			
			set {
				font = value;
			}
		}
		
		public bool FontMustExist
		{
			get {
				throw new NotImplementedException( );
			}
			
			set {
				throw new NotImplementedException( );
			}
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
		
		#endregion	// Public Instance Properties
		
		#region Protected Instance Properties
		#endregion	// Protected Instance Properties
		
		#region Public Instance Methods
		[MonoTODO]
		public override void Reset( )
		{
			throw new NotImplementedException( );
		}
		#endregion	// Public Instance Methods
		
		#region Protected Instance Methods
		[MonoTODO]
		protected override bool RunDialog( IntPtr hwndOwner )
		{
			form.Controls.Add( fontDialogPanel );
			
			return true;
		}
		#endregion	// Protected Instance Methods
	}
	
	internal class FontDialogPanel : Panel
	{
		private Panel examplePanel;
		
		private Button okButton;
		private Button cancelButton;
		private Button applyButton;
		private Button helpButton;
		
		private TextBox fontTextBox;
		private TextBox fontstyleTextBox;
		private TextBox sizeTextBox;
		
		private ListBox fontListBox;
		private ListBox fontstyleListBox;
		private ListBox sizeListBox;
		
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
		
		private FontFamily[] fontFamilies;
		
		private string currentFontName;
		
		private Font currentFont;
		
		private int currentSize;
		
		private FontFamily currentFamily;
		
		private Color currentColor;
		
		private FontStyle currentFontStyle;
		
		private FontDialog fontDialog;
		
		private System.Collections.ArrayList fontStyleArray = new System.Collections.ArrayList();
		
		private System.Collections.Hashtable fontHash = new System.Collections.Hashtable();
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fontDialog">A  FontDialog</param>
		public FontDialogPanel( FontDialog fontDialog )
		{
			this.fontDialog = fontDialog;
			
			okButton = new Button( );
			cancelButton = new Button( );
			applyButton = new Button( );
			helpButton = new Button( );
			
			fontTextBox = new TextBox( );
			fontstyleTextBox = new TextBox( );
			sizeTextBox = new TextBox( );
			
			fontListBox = new ListBox( );
			sizeListBox = new ListBox( );
			
			fontLabel = new Label( );
			fontstyleLabel = new Label( );
			sizeLabel = new Label( );
			scriptLabel = new Label( );
			
			exampleGroupBox = new GroupBox( );
			fontstyleListBox = new ListBox( );
			
			effectsGroupBox = new GroupBox( );
			underlinedCheckBox = new CheckBox( );
			strikethroughCheckBox = new CheckBox( );
			scriptComboBox = new ComboBox( );
			
			examplePanel = new Panel( );
			
			colorComboBox = new ColorComboBox( this );
			
			exampleGroupBox.SuspendLayout( );
			effectsGroupBox.SuspendLayout( );
			SuspendLayout( );
			
			// typesizeListBox
			sizeListBox.Location = new Point( 284, 47 );
			sizeListBox.Size = new Size( 52, 95 );
			sizeListBox.TabIndex = 10;
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
			sizeTextBox.Location = new Point( 284, 26 );
			sizeTextBox.Size = new Size( 52, 21 );
			sizeTextBox.TabIndex = 7;
			sizeTextBox.Text = "";
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
			scriptComboBox.Text = "-/-";
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
			examplePanel.BorderStyle = BorderStyle.Fixed3D;
			examplePanel.Size = new Size( 156, 40 );
			
			ClientSize = new Size( 430, 318 );
			
			Controls.Add( scriptComboBox );
			Controls.Add( scriptLabel );
			Controls.Add( exampleGroupBox );
			Controls.Add( effectsGroupBox );
			Controls.Add( sizeListBox );
			Controls.Add( fontstyleListBox );
			Controls.Add( fontListBox );
			Controls.Add( sizeTextBox );
			Controls.Add( fontstyleTextBox );
			Controls.Add( fontTextBox );
			Controls.Add( cancelButton );
			Controls.Add( okButton );
			Controls.Add( sizeLabel );
			Controls.Add( fontstyleLabel );
			Controls.Add( fontLabel );
			Controls.Add( applyButton );
			Controls.Add( helpButton );
			
			exampleGroupBox.ResumeLayout( false );
			effectsGroupBox.ResumeLayout( false );
			
			ResumeLayout( false );
			
			fontFamilies = FontFamily.Families;
			
			fontListBox.BeginUpdate( );
			foreach ( FontFamily ff in fontFamilies )
			{
				fontListBox.Items.Add( ff.Name );
				fontHash.Add( ff.Name, ff );
			}
			fontListBox.EndUpdate( );
			
			// TODO: If Font is provided via FontDialog.Font property set correct font in FontListBox
			currentFontName = fontListBox.Items[ 0 ].ToString( );
			fontTextBox.Text = currentFontName;
			
			// default 12 ?!?
			currentSize = 12;
			
			currentFamily = FindByName( currentFontName );
			
			currentFontStyle = FontStyle.Regular;
			
			currentFont = new Font( currentFamily, currentSize, currentFontStyle );
			
			currentColor = fontDialog.Color;
			
			UpdateFontStyleListBox();
			
			fontstyleTextBox.Text = "Regular";
			
			sizeTextBox.Text = currentSize.ToString( );
			
			sizeListBox.Items.AddRange( new object[] {
										   "8",
										   "9",
										   "10",
										   "11",
										   "12",
										   "14",
										   "16",
										   "18",
										   "20",
										   "22",
										   "24",
										   "26",
										   "28",
										   "36",
										   "48",
										   "72" } );
			
			sizeListBox.SelectedIndex = 0;
			
			applyButton.Hide( );
			helpButton.Hide( );
			
			cancelButton.Click += new EventHandler( OnClickCancelButton );
			okButton.Click += new EventHandler( OnClickOkButton );
			examplePanel.Paint += new PaintEventHandler( OnPaintExamplePanel );
			fontListBox.SelectedIndexChanged += new EventHandler( OnSelectedIndexChangedFontListBox );
			sizeListBox.SelectedIndexChanged += new EventHandler( OnSelectedIndexChangedSizeListBox );
			fontstyleListBox.SelectedIndexChanged += new EventHandler( OnSelectedIndexChangedFontStyleListBox );
			underlinedCheckBox.CheckedChanged += new EventHandler( OnCheckedChangedUnderlinedCheckBox );
			strikethroughCheckBox.CheckedChanged += new EventHandler( OnCheckedChangedStrikethroughCheckBox );
		}
		
		public Color CurrentColor
		{
			set
			{
				currentColor = value;
				examplePanel.Invalidate();
			}
			
			get
			{
				return currentColor;
			}
		}

		private void UpdateFontStyleListBox( )
		{
			// don't know if that works, IsStyleAvailable returns true for all styles under X
			
			fontStyleArray.Clear();
			
			fontstyleListBox.BeginUpdate( );
			
			fontstyleListBox.Items.Clear();
			
			if ( currentFamily.IsStyleAvailable( FontStyle.Regular ) )
			{
				fontstyleListBox.Items.Add( "Regular" );
				fontStyleArray.Add(0);
			}
			
			if ( currentFamily.IsStyleAvailable( FontStyle.Bold ) )
			{
				fontstyleListBox.Items.Add( "Bold" );
				fontStyleArray.Add(1);
			}
			
			if ( currentFamily.IsStyleAvailable( FontStyle.Italic ) )
			{
				fontstyleListBox.Items.Add( "Italic" );
				fontStyleArray.Add(2);
			}
			
			if ( currentFamily.IsStyleAvailable( FontStyle.Bold ) && currentFamily.IsStyleAvailable( FontStyle.Italic ) )
			{
				fontstyleListBox.Items.Add( "Bold Italic" );
				fontStyleArray.Add(3);
			}
			
			fontstyleListBox.EndUpdate( );
		}
		
		private FontFamily FindByName( string name )
		{
			return fontHash[ name ] as FontFamily;
		}
		
		void OnClickCancelButton( object sender, EventArgs e )
		{
			fontDialog.form.Controls.Remove( this );
			fontDialog.form.DialogResult = DialogResult.Cancel;
		}
		
		void OnClickOkButton( object sender, EventArgs e )
		{
			fontDialog.form.Controls.Remove( this );
			fontDialog.Font = currentFont;
			fontDialog.Color = currentColor;
			fontDialog.form.DialogResult = DialogResult.OK;
		}
		
		void OnPaintExamplePanel( object sender, PaintEventArgs e )
		{
			SolidBrush brush = new SolidBrush( currentColor );
			
			// FIXME: need to find a better algorithm to display the font
			
			int x = ( examplePanel.Width / 2 ) - ( currentSize * 4 );
			int y = ( examplePanel.Height / 2 ) - ( currentFont.Height / 2 );
			
			if ( x < 0 ) x = 0;
			if ( y < 0 ) y = 0;
			
			e.Graphics.DrawString( "AaBbYyZz", currentFont, brush, new Point( x, y ) );
			
//			StringFormat strformat = new StringFormat();
//			strformat.Alignment = StringAlignment.Center;
//			strformat.Alignment = StringAlignment.Center;
//
//			e.Graphics.DrawString ("AaBbYyZz", currentFont, brush, new Rectangle(0, 0, examplePanel.Width, examplePanel.Height), strformat);
		}
		
		void OnSelectedIndexChangedFontListBox( object sender, EventArgs e )
		{
			if ( fontListBox.SelectedIndex != -1 )
			{
				currentFamily = FindByName( fontListBox.Items[ fontListBox.SelectedIndex ].ToString( ) );
				
				fontTextBox.Text = currentFamily.Name;
				
				UpdateFontStyleListBox();
				
				UpdateExamplePanel( );
			}
		}
		
		void OnSelectedIndexChangedSizeListBox( object sender, EventArgs e )
		{
			if ( sizeListBox.SelectedIndex != -1 )
			{
				currentSize = System.Convert.ToInt32( sizeListBox.Items[ sizeListBox.SelectedIndex ] );
				
				sizeTextBox.Text = currentSize.ToString( );
				
				UpdateExamplePanel( );
			}
		}
		
		void OnSelectedIndexChangedFontStyleListBox( object sender, EventArgs e )
		{
			if ( fontstyleListBox.SelectedIndex != -1 )
			{
				switch ( (int)fontStyleArray[ fontstyleListBox.SelectedIndex] )
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
				
				fontstyleTextBox.Text = fontstyleListBox.Items[ fontstyleListBox.SelectedIndex ].ToString( );
				
				UpdateExamplePanel( );
			}
		}
		
		void OnCheckedChangedUnderlinedCheckBox( object sender, EventArgs e )
		{
			if ( underlinedCheckBox.Checked )
				currentFontStyle = currentFontStyle | FontStyle.Underline;
			else
				currentFontStyle = currentFontStyle ^ FontStyle.Underline;
			
			UpdateExamplePanel( );
		}
		
		void OnCheckedChangedStrikethroughCheckBox( object sender, EventArgs e )
		{
			if ( strikethroughCheckBox.Checked )
				currentFontStyle = currentFontStyle | FontStyle.Strikeout;
			else
				currentFontStyle = currentFontStyle ^ FontStyle.Strikeout;
			
			UpdateExamplePanel( );
		}
				
		private void UpdateExamplePanel( )
		{
			currentFont = new Font( currentFamily, currentSize, currentFontStyle );
			
			examplePanel.Invalidate( );
			examplePanel.Update( );
		}
		
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
			}
			
			private Color selectedColor;
			
			private FontDialogPanel fontDialogPanel;
			
			// FIXME: TextBox backcolor shouldn't be the same as the selected item in the ListBox/ListCtrl
			
			public ColorComboBox( FontDialogPanel fontDialogPanel )
			{
				this.fontDialogPanel = fontDialogPanel;
				
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
					e.Graphics.FillRectangle( new SolidBrush( Color.Blue ), e.Bounds ); // bot blue
					e.Graphics.FillRectangle( new SolidBrush( ccbi.Color ), e.Bounds.X + 3, e.Bounds.Y + 3, e.Bounds.X + 16, e.Bounds.Y + 8 );
					e.Graphics.DrawRectangle( new Pen( Color.Black ), e.Bounds.X + 2, e. Bounds.Y + 2, e.Bounds.X + 17, e.Bounds.Y + 9 );
					e.Graphics.DrawString( ccbi.Name, this.Font, new SolidBrush( Color.White ), r );
				}
				else
				{
					e.Graphics.FillRectangle( new SolidBrush( Color.White ), e.Bounds );
					e.Graphics.FillRectangle( new SolidBrush( ccbi.Color ), e.Bounds.X + 3, e.Bounds.Y + 3, e.Bounds.X + 16, e.Bounds.Y + 8 );
					e.Graphics.DrawRectangle( new Pen( Color.Black ), e.Bounds.X + 2, e. Bounds.Y + 2, e.Bounds.X + 17, e.Bounds.Y + 9 );
					e.Graphics.DrawString( ccbi.Name, this.Font, new SolidBrush( Color.Black ), r );
				}
			}
			
			protected override void OnSelectedIndexChanged( EventArgs e )
			{
				ColorComboBoxItem ccbi = Items[ SelectedIndex ] as ColorComboBoxItem;
				selectedColor = ccbi.Color;
				
				fontDialogPanel.CurrentColor = selectedColor;
			}
		}
	}
}


