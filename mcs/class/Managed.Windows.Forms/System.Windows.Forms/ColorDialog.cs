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
// Copyright (c) 2004-2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Alexander Olk	xenomorph2@onlinehome.de
//

// NOT COMPLETE

using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Resources;

namespace System.Windows.Forms
{
	[DefaultProperty( "Color" )]
	public class ColorDialog : CommonDialog
	{
		#region Local Variables
		private ColorDialogPanel colorDialogPanel = null;
		private bool allowFullOpen = true;
		private bool anyColor = false;
		private Color color = Color.Black;
		private int[] customColors = null;
		private bool fullOpen = false;
		private bool showHelp = false;
		private bool solidColorOnly = false;
		#endregion	// Local Variables
		
		#region Public Constructors
		public ColorDialog( ) : base()
		{
			form.Text = "Color";
			
			form.Size = new Size( 221, 332 ); // 300
			
			colorDialogPanel = new ColorDialogPanel( this );
		}
		#endregion	// Public Constructors
		
		#region Public Instance Properties
		public Color Color
		{
			get {
				return color;
			}
			
			set {
				color = value;
			}
		}
		
		[DefaultValue(true)]
		public virtual bool AllowFullOpen
		{
			get {
				return allowFullOpen;
			}
			
			set {
				allowFullOpen = value;
			}
		}
		
		// Currently AnyColor internally is always true
		// Does really anybody still use 256 or less colors ???
		// Naw, cairo only supports 24bit anyway - pdb
		[DefaultValue(false)]
		public virtual bool AnyColor
		{
			get {
				return anyColor;
			}
			
			set {
				anyColor = value;
			}
		}
		
		[DefaultValue(false)]
		public virtual bool FullOpen
		{
			get {
				return fullOpen;
			}
			
			set {
				fullOpen = value;
			}
		}
		
		[Browsable(false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int[] CustomColors
		{
			get {
				return customColors;
			}
			
			set {
				customColors = value;
			}
		}
		
		[DefaultValue(false)]
		public virtual bool ShowHelp
		{
			get {
				return showHelp;
			}
			
			set {
				showHelp = value;
			}
		}
		
		[DefaultValue(false)]
		public virtual bool SolidColorOnly
		{
			get {
				return solidColorOnly;
			}
			
			set {
				solidColorOnly = value;
			}
		}
		#endregion	// Public Instance Properties
		
		#region Public Instance Methods
		public override void Reset( )
		{
			allowFullOpen = true;
			anyColor = false;
			color = Color.Black;
			customColors = null;
			fullOpen = false;
			showHelp = false;
			solidColorOnly = false;
		}
		
		public override string ToString( )
		{
			return base.ToString( ) + ", Color: " + Color.ToString( );
		}
		#endregion	// Public Instance Methods
		
		#region Protected Instance Properties
		protected virtual IntPtr Instance
		{
			get {
				// MS Internal
				return (IntPtr)GetHashCode( );
			}
		}
		
		protected virtual int Options
		{
			get {
				// MS Internal
				return 0;
			}
		}
		#endregion	// Protected Instance Properties
		
		#region Protected Instance Methods
		protected override bool RunDialog( IntPtr hwndOwner )
		{
			form.Controls.Add( colorDialogPanel );
			
			if ( customColors != null )
				colorDialogPanel.BaseColorControl.SetCustomColors( );
			
			return true;
		}
		#endregion	// Protected Instance Methods
		
		#region Private Classes
		internal class ColorDialogPanel : Panel
		{
			#region Local Variables
			private Panel selectedColorPanel;
			private BaseColorControl baseColorControl;
			private ColorMatrixControl colorMatrixControl;
			private BrightnessControl brightnessControl;
			private TriangleControl triangleControl;
			
			private Button okButton;
			private Button cancelButton;
			private Button helpButton;
			private Button addColoursButton;
			private Button defineColoursButton;
			
			private TextBox hueTextBox;
			private TextBox satTextBox;
			private TextBox briTextBox;
			private TextBox redTextBox;
			private TextBox greenTextBox;
			private TextBox blueTextBox;
			
			private Label briLabel;
			private Label satLabel;
			private Label hueLabel;
			private Label colorBaseLabel;
			private Label greenLabel;
			private Label blueLabel;
			private Label redLabel;
			
			private ColorDialog colorDialog;
			#endregion	// Local Variables
			
			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="colorDialog">A  ColorDialog</param>
			internal ColorDialogPanel( ColorDialog colorDialog )
			{
				this.colorDialog = colorDialog;
				
				satTextBox = new TextBox( );
				briTextBox = new TextBox( );
				blueTextBox = new TextBox( );
				greenTextBox = new TextBox( );
				redTextBox = new TextBox( );
				hueTextBox = new TextBox( );
				
				redLabel = new Label( );
				blueLabel = new Label( );
				greenLabel = new Label( );
				colorBaseLabel = new Label( );
				hueLabel = new Label( );
				satLabel = new Label( );
				briLabel = new Label( );
				
				okButton = new Button( );
				cancelButton = new Button( );
				helpButton = new Button( );
				defineColoursButton = new Button( );
				addColoursButton = new Button( );
				
				baseColorControl = new BaseColorControl( this );
				colorMatrixControl = new ColorMatrixControl( this );
				brightnessControl = new BrightnessControl( this );
				triangleControl = new TriangleControl( this );
				
				selectedColorPanel = new Panel( );
				
				SuspendLayout( );
				
				// hueTextBox
				hueTextBox.Location = new Point( 324, 203 );
				hueTextBox.Size = new Size( 27, 21 );
				hueTextBox.TabIndex = 11;
				hueTextBox.TextAlign = HorizontalAlignment.Right;
				hueTextBox.MaxLength = 3;
				// satTextBox
				satTextBox.Location = new Point( 324, 225 );
				satTextBox.Size = new Size( 27, 21 );
				satTextBox.TabIndex = 15;
				satTextBox.TextAlign = HorizontalAlignment.Right;
				satTextBox.MaxLength = 3;
				// greenTextBox
				greenTextBox.Location = new Point( 404, 225 );
				greenTextBox.Size = new Size( 27, 21 );
				greenTextBox.TabIndex = 18;
				greenTextBox.TextAlign = HorizontalAlignment.Right;
				greenTextBox.MaxLength = 3;
				// briTextBox
				briTextBox.Location = new Point( 324, 247 );
				briTextBox.Size = new Size( 27, 21 );
				briTextBox.TabIndex = 16;
				briTextBox.TextAlign = HorizontalAlignment.Right;
				briTextBox.MaxLength = 3;
				// blueTextBox
				blueTextBox.Location = new Point( 404, 247 );
				blueTextBox.Size = new Size( 27, 21 );
				blueTextBox.TabIndex = 19;
				blueTextBox.TextAlign = HorizontalAlignment.Right;
				blueTextBox.MaxLength = 3;
				// redTextBox
				redTextBox.Location = new Point( 404, 203 );
				redTextBox.Size = new Size( 27, 21 );
				redTextBox.TabIndex = 17;
				redTextBox.TextAlign = HorizontalAlignment.Right;
				redTextBox.MaxLength = 3;
				
				// redLabel
				redLabel.FlatStyle = FlatStyle.System;
				redLabel.Location = new Point( 361, 206 );
				redLabel.Size = new Size( 40, 16 );
				redLabel.TabIndex = 25;
				redLabel.Text = Locale.GetText( "Red" ) + ":";
				redLabel.TextAlign = ContentAlignment.MiddleRight;
				// blueLabel
				blueLabel.FlatStyle = FlatStyle.System;
				blueLabel.Location = new Point( 361, 250 );
				blueLabel.Size = new Size( 40, 16 );
				blueLabel.TabIndex = 26;
				blueLabel.Text = Locale.GetText( "Blue" ) + ":";
				blueLabel.TextAlign = ContentAlignment.MiddleRight;
				// greenLabel
				greenLabel.FlatStyle = FlatStyle.System;
				greenLabel.Location = new Point( 361, 228 );
				greenLabel.Size = new Size( 40, 16 );
				greenLabel.TabIndex = 27;
				greenLabel.Text = Locale.GetText( "Green" ) + ":";
				greenLabel.TextAlign = ContentAlignment.MiddleRight;
				// colorBaseLabel
				colorBaseLabel.Location = new Point( 228, 247 );
				colorBaseLabel.Size = new Size( 60, 25 );
				colorBaseLabel.TabIndex = 28;
				colorBaseLabel.Text = Locale.GetText( "Color" );
				colorBaseLabel.TextAlign = ContentAlignment.MiddleCenter;
				// hueLabel
				hueLabel.FlatStyle = FlatStyle.System;
				hueLabel.Location = new Point( 287, 206 );
				hueLabel.Size = new Size( 36, 16 );
				hueLabel.TabIndex = 23;
				hueLabel.Text = Locale.GetText( "Hue" ) + ":";
				hueLabel.TextAlign = ContentAlignment.MiddleRight;
				// satLabel
				satLabel.FlatStyle = FlatStyle.System;
				satLabel.Location = new Point( 287, 228 );
				satLabel.Size = new Size( 36, 16 );
				satLabel.TabIndex = 22;
				satLabel.Text = Locale.GetText( "Sat" ) + ":";
				satLabel.TextAlign = ContentAlignment.MiddleRight;
				// briLabel
				briLabel.FlatStyle = FlatStyle.System;
				briLabel.Location = new Point( 287, 250 );
				briLabel.Size = new Size( 36, 16 );
				briLabel.TabIndex = 24;
				briLabel.Text = Locale.GetText( "Bri" ) + ":";
				briLabel.TextAlign = ContentAlignment.MiddleRight;
				
				// defineColoursButton
				defineColoursButton.FlatStyle = FlatStyle.System;
				defineColoursButton.Location = new Point( 5, 244 );
				defineColoursButton.Size = new Size( 210, 22 );
				defineColoursButton.TabIndex = 6;
				defineColoursButton.Text = Locale.GetText( "Define Colours >>" );
				// okButton
				okButton.FlatStyle = FlatStyle.System;
				okButton.Location = new Point( 5, 271 );
				okButton.Size = new Size( 66, 22 );
				okButton.TabIndex = 0;
				okButton.Text = Locale.GetText( "OK" );
				// cancelButton
				cancelButton.FlatStyle = FlatStyle.System;
				cancelButton.Location = new Point( 78, 271 );
				cancelButton.Size = new Size( 66, 22 );
				cancelButton.TabIndex = 1;
				cancelButton.Text = Locale.GetText( "Cancel" );
				// helpButton
				helpButton.FlatStyle = FlatStyle.System;
				helpButton.Location = new Point( 149, 271 );
				helpButton.Size = new Size( 66, 22 );
				helpButton.TabIndex = 5;
				helpButton.Text = Locale.GetText( "Help" );
				// addColoursButton
				addColoursButton.FlatStyle = FlatStyle.System;
				addColoursButton.Location = new Point( 227, 271 );
				addColoursButton.Size = new Size( 213, 22 );
				addColoursButton.TabIndex = 7;
				addColoursButton.Text =  Locale.GetText( "Add Colours" );
				
				// baseColorControl
				baseColorControl.Location = new Point( 3, 7 );
				baseColorControl.Size = new Size( 212, 230 );
				baseColorControl.TabIndex = 13;
				// colorMatrixControl
				//colorMatrixControl.BackColor = SystemColors.Control;
				colorMatrixControl.Location = new Point( 227, 7 );
				colorMatrixControl.Size = new Size( 179, 190 );
				colorMatrixControl.TabIndex = 14;
				// triangleControl
				triangleControl.Location = new Point( 432, 0 );
				triangleControl.Size = new Size( 16, 204 );
				triangleControl.TabIndex = 12;
				// brightnessControl
				brightnessControl.Location = new Point( 415, 7 );
				brightnessControl.Size = new Size( 14, 190 );
				brightnessControl.TabIndex = 20;
				
				// selectedColorPanel
				selectedColorPanel.BackColor = SystemColors.Desktop;
				selectedColorPanel.BorderStyle = BorderStyle.Fixed3D;
				selectedColorPanel.Location = new Point( 227, 202 );
				selectedColorPanel.Size = new Size( 60, 42 );
				selectedColorPanel.TabIndex = 10;
				
				ClientSize = new Size( 448, 332 ); // 300
				Controls.Add( hueTextBox );
				Controls.Add( satTextBox );
				Controls.Add( briTextBox );
				Controls.Add( redTextBox );
				Controls.Add( greenTextBox );
				Controls.Add( blueTextBox );
				
				Controls.Add( defineColoursButton );
				Controls.Add( okButton );
				Controls.Add( cancelButton );
				Controls.Add( helpButton );
				Controls.Add( addColoursButton );
				
				Controls.Add( baseColorControl );
				Controls.Add( colorMatrixControl );
				Controls.Add( brightnessControl );
				Controls.Add( triangleControl );
				
				Controls.Add( colorBaseLabel );
				Controls.Add( greenLabel );
				Controls.Add( blueLabel );
				Controls.Add( redLabel );
				Controls.Add( briLabel );
				Controls.Add( hueLabel );
				Controls.Add( satLabel );
				
				Controls.Add( selectedColorPanel );
				
				ResumeLayout( false );
				
				brightnessControl.ColorToShow = selectedColorPanel.BackColor;
				
				redTextBox.Text = selectedColorPanel.BackColor.R.ToString( );
				greenTextBox.Text = selectedColorPanel.BackColor.G.ToString( );
				blueTextBox.Text = selectedColorPanel.BackColor.B.ToString( );
				
				HSB hsb = HSB.RGB2HSB( selectedColorPanel.BackColor );
				hueTextBox.Text = hsb.hue.ToString( );
				satTextBox.Text = hsb.sat.ToString( );
				briTextBox.Text = hsb.bri.ToString( );
				
				if ( !colorDialog.AllowFullOpen )
					defineColoursButton.Enabled = false;
				
				if ( !colorDialog.ShowHelp )
					helpButton.Enabled = false;
				
				if ( colorDialog.FullOpen )
					DoButtonDefineColours( );
				
				defineColoursButton.Click += new EventHandler( OnClickButtonDefineColours );
				addColoursButton.Click += new EventHandler( OnClickButtonAddColours );
				helpButton.Click += new EventHandler( OnClickHelpButton );
				cancelButton.Click += new EventHandler( OnClickCancelButton );
				okButton.Click += new EventHandler( OnClickOkButton );
				
				hueTextBox.KeyPress += new KeyPressEventHandler( OnKeyPressTextBoxes );
				satTextBox.KeyPress += new KeyPressEventHandler( OnKeyPressTextBoxes );
				briTextBox.KeyPress += new KeyPressEventHandler( OnKeyPressTextBoxes );
				redTextBox.KeyPress += new KeyPressEventHandler( OnKeyPressTextBoxes );
				greenTextBox.KeyPress += new KeyPressEventHandler( OnKeyPressTextBoxes );
				blueTextBox.KeyPress += new KeyPressEventHandler( OnKeyPressTextBoxes );
				
				SetStyle( ControlStyles.DoubleBuffer, true );
			}
			
			public Panel SelectedColorPanel
			{
				set {
					selectedColorPanel = value;
				}
				
				get {
					return selectedColorPanel;
				}
			}
			
			public BrightnessControl BrightnessControl
			{
				set {
					brightnessControl = value;
				}
				
				get {
					return brightnessControl;
				}
			}
			
			public TextBox HueTextBox
			{
				set {
					hueTextBox = value;
				}
				
				get {
					return hueTextBox;
				}
			}
			
			public ColorMatrixControl ColorMatrixControl
			{
				set {
					colorMatrixControl = value;
				}
				
				get {
					return colorMatrixControl;
				}
			}
			
			public TriangleControl TriangleControl
			{
				set {
					triangleControl = value;
				}
				
				get {
					return triangleControl;
				}
			}
			
			public TextBox RedTextBox
			{
				set {
					redTextBox = value;
				}
				
				get {
					return redTextBox;
				}
			}
			
			public TextBox GreenTextBox
			{
				set {
					greenTextBox = value;
				}
				
				get {
					return greenTextBox;
				}
			}
			
			public BaseColorControl BaseColorControl
			{
				set {
					baseColorControl = value;
				}
				
				get {
					return baseColorControl;
				}
			}
			
			public TextBox BlueTextBox
			{
				set {
					blueTextBox = value;
				}
				
				get {
					return blueTextBox;
				}
			}
			
			public TextBox SatTextBox
			{
				set {
					satTextBox = value;
				}
				
				get {
					return satTextBox;
				}
			}
			
			public TextBox BriTextBox
			{
				set {
					briTextBox = value;
				}
				
				get {
					return briTextBox;
				}
			}
			
			public ColorDialog ColorDialog
			{
				set {
					colorDialog = value;
				}
				
				get {
					return colorDialog;
				}
			}
			
			void OnClickCancelButton( object sender, EventArgs e )
			{
				colorDialog.form.Controls.Remove( this );
				colorDialog.form.DialogResult = DialogResult.Cancel;
			}
			
			void OnClickOkButton( object sender, EventArgs e )
			{
				colorDialog.form.Controls.Remove( this );
				colorDialog.form.DialogResult = DialogResult.OK;
			}
			
			void OnClickButtonDefineColours( object sender, EventArgs e )
			{
				DoButtonDefineColours( );
			}
			
			private void DoButtonDefineColours( )
			{
				defineColoursButton.Enabled = false;
				
				colorDialog.FullOpen = true;
				
				colorMatrixControl.ColorToShow = baseColorControl.ColorToShow;
				
				colorDialog.form.ClientSize = new Size( 448, 332 );
			}
			
			void OnClickButtonAddColours( object sender, EventArgs e )
			{
				baseColorControl.SetUserColor( selectedColorPanel.BackColor );
			}
			
			// FIXME: Is this correct ?
			void OnClickHelpButton( object sender, EventArgs e )
			{
				colorDialog.OnHelpRequest( e );
			}

			// not working 100 %, S.W.F.TextBox isn't finished yet
			void OnKeyPressTextBoxes( object sender, KeyPressEventArgs e )
			{
				// accept only '0', '1', ... , '9'
				// 48 = '0', 57 = '9'
				if ( e.KeyChar < (char)48 || e.KeyChar > (char)57 )
					e.Handled = true;
				
				TextChangedTextBoxes( sender );
			}
			
			// not working 100 %, S.W.F.TextBox isn't finished yet
			void TextChangedTextBoxes( object sender )
			{
				if ( ( (TextBox)sender ).Text.Length == 0 )
					return;
				
				int val;
				
				if ( sender == hueTextBox )
				{
					val = System.Convert.ToInt32( hueTextBox.Text );
					
					if ( val > 240 )
					{
						val = 240;
						hueTextBox.Text = val.ToString( );
					}
					else
					if ( val < 0 )
					{
						val = 0;
						hueTextBox.Text = val.ToString( );
					}
					
					UpdateFromHSBTextBoxes( );
					
					UpdateControls( selectedColorPanel.BackColor );
				}
				else
				if ( sender == satTextBox )
				{
					val = System.Convert.ToInt32( satTextBox.Text );
					
					if ( val > 239 )
					{
						val = 239;
						satTextBox.Text = val.ToString( );
					}
					else
					if ( val < 0 )
					{
						val = 0;
						satTextBox.Text = val.ToString( );
					}
					
					UpdateFromHSBTextBoxes( );
					
					UpdateControls( selectedColorPanel.BackColor );
				}
				else
				if ( sender == briTextBox )
				{
					val = System.Convert.ToInt32( briTextBox.Text );
					
					if ( val > 239 )
					{
						val = 239;
						briTextBox.Text = val.ToString( );
					}
					else
					if ( val < 0 )
					{
						val = 0;
						briTextBox.Text = val.ToString( );
					}
					
					UpdateFromHSBTextBoxes( );
					
					UpdateControls( selectedColorPanel.BackColor );
				}
				else
				if ( sender == redTextBox )
				{
					val = System.Convert.ToInt32( redTextBox.Text );
					
					if ( val > 255 )
					{
						val = 255;
						redTextBox.Text = val.ToString( );
					}
					else
					if ( val < 0 )
					{
						val = 0;
						redTextBox.Text = val.ToString( );
					}
					
					UpdateFromRGBTextBoxes( );
				}
				else
				if ( sender == greenTextBox )
				{
					val = System.Convert.ToInt32( greenTextBox.Text );
					
					if ( val > 255 )
					{
						val = 255;
						greenTextBox.Text = val.ToString( );
					}
					else
					if ( val < 0 )
					{
						val = 0;
						greenTextBox.Text = val.ToString( );
					}
					
					UpdateFromRGBTextBoxes( );
				}
				else
				if ( sender == blueTextBox )
				{
					val = System.Convert.ToInt32( blueTextBox.Text );
					
					if ( val > 255 )
					{
						val = 255;
						blueTextBox.Text = val.ToString( );
					}
					else
					if ( val < 0 )
					{
						val = 0;
						blueTextBox.Text = val.ToString( );
					}
					
					UpdateFromRGBTextBoxes( );
				}
			}
			
			public void UpdateControls( Color color )
			{
				colorDialog.Color = color;
				selectedColorPanel.BackColor = color;
				colorMatrixControl.ColorToShow = color;
				brightnessControl.ColorToShow = color;
				triangleControl.ColorToShow = color;
			}
			
			public void UpdateRGBTextBoxes( Color color )
			{
				redTextBox.Text = color.R.ToString( );
				greenTextBox.Text = color.G.ToString( );
				blueTextBox.Text = color.B.ToString( );
			}
			
			public void UpdateHSBTextBoxes( Color color )
			{
				HSB hsb = HSB.RGB2HSB( color );
				
				hueTextBox.Text = hsb.hue.ToString( );
				satTextBox.Text = hsb.sat.ToString( );
				briTextBox.Text = hsb.bri.ToString( );
			}
			
			public void UpdateFromHSBTextBoxes( )
			{
				Color col = HSB.HSB2RGB( System.Convert.ToInt32( hueTextBox.Text ),
										System.Convert.ToInt32( satTextBox.Text ),
										System.Convert.ToInt32( briTextBox.Text ) );
				
				selectedColorPanel.BackColor = col;
				UpdateRGBTextBoxes( col );
			}
			
			public void UpdateFromRGBTextBoxes( )
			{
				Color col = Color.FromArgb( System.Convert.ToInt32( redTextBox.Text ),
										   System.Convert.ToInt32( greenTextBox.Text ),
										   System.Convert.ToInt32( blueTextBox.Text ) );
				
				selectedColorPanel.BackColor = col;
				
				UpdateHSBTextBoxes( col );
				
				UpdateFromHSBTextBoxes( );
				
				UpdateControls( col );
			}
		}
		
		internal struct HSB
		{
			public int hue;
			public int sat;
			public int bri;
			
			public static HSB RGB2HSB( Color color )
			{
				HSB hsb = new HSB( );
				
				hsb.hue = (int)( ( color.GetHue( ) / 360.0f ) * 241 );
				hsb.sat = (int)( color.GetSaturation( ) * 241 );
				hsb.bri = (int)( color.GetBrightness( ) * 240 );
				
				if ( hsb.hue > 240 ) hsb.hue = 240;
				if ( hsb.sat > 240 ) hsb.sat = 240;
				if ( hsb.bri > 239 ) hsb.bri = 239;
				
				return hsb;
			}
			
			// not using ControlPaint HBS2Color, this algo is more precise
			public static Color HSB2RGB( int hue, int saturation, int brightness )
			{
				if ( hue > 240 )
					hue = 240;
				else
				if ( hue < 0 )
					hue = 0;
				
				if ( saturation > 240 )
					saturation = 240;
				else
				if ( saturation < 0 )
					saturation = 0;
				
				if ( brightness > 239 )
					brightness = 239;
				else
				if ( brightness < 0 )
					brightness = 0;
				
				float H = hue / 240.0f;
				float S = saturation / 240.0f;
				float L = brightness / 239.0f;
				
				float r = 0, g = 0, b = 0;
				float d1, d2;
				
				if ( L == 0 )
				{
					r = g =  b = 0;
				}
				else
				{
					if ( S == 0 )
					{
						r = g = b = L;
					}
					else
					{
						d2 = ( L <= 0.5f ) ? L * ( 1.0f + S ) : L + S - ( L * S );
						d1 = 2.0f * L - d2;
						
						float[] d3 = new float[] { H + 1.0f / 3.0f , H, H - 1.0f / 3.0f };
						float[] rgb = new float[] { 0,0,0 };
						
						for ( int i = 0; i < 3; i++ )
						{
							if ( d3[ i ] < 0 )
								d3[ i ] += 1.0f;
							if ( d3[ i ] > 1.0f )
								d3[ i ] -= 1.0f;
							
							if ( 6.0f * d3[ i ] < 1.0f )
								rgb[ i ] = d1 + ( d2 - d1 ) * d3[ i ] * 6.0f;
							else
							if ( 2.0f * d3[ i ] < 1.0f )
								rgb[ i ] = d2;
							else
							if ( 3.0f * d3[ i ] < 2.0f )
								rgb[ i ] = ( d1 + ( d2 - d1 ) * ( ( 2.0f / 3.0f ) - d3[ i ] ) * 6.0f );
							else
								rgb[ i ] = d1;
						}
						
						r = rgb[ 0 ];
						g = rgb[ 1 ];
						b = rgb[ 2 ];
					}
				}
				
				r = 255.0f * r;
				g = 255.0f * g;
				b = 255.0f * b;
				
				if ( r < 1 )
					r = 0.0f;
				else
				if ( r > 255.0f )
					r = 255.0f;
				
				if ( g < 1 )
					g = 0.0f;
				else
				if ( g > 255.0f )
					g = 255.0f;
				
				if ( b < 1 )
					b = 0.0f;
				else
				if ( b > 255.0f )
					b = 255.0f;
				
				return Color.FromArgb( (int)r, (int)g, (int)b );
			}
			
			public static int Brightness( Color color )
			{
				return (int)( color.GetBrightness( ) * 240 );
			}
			
			public static void GetHueSaturation( Color color, out int hue, out int sat )
			{
				hue = (int)( ( color.GetHue( ) / 360.0f ) * 241 );
				sat = (int)( color.GetSaturation( ) * 241 );
			}
			
			// only for testing
			// there are some small glitches, but it is still better than ControlPaint implementation
			public static void TestColor( Color color )
			{
				Console.WriteLine( "Color: " + color );
				HSB hsb = HSB.RGB2HSB( color );
				Console.WriteLine( "RGB2HSB: " + hsb.hue + ", " + hsb.sat + ", " + hsb.bri );
				Console.WriteLine( "HSB2RGB: " + HSB.HSB2RGB( hsb.hue, hsb.sat, hsb.bri ) );
				Console.WriteLine( );
			}
		}
		
		internal class BaseColorControl : Control
		{
			private Panel[] colorPanel;
			
			private Panel[] userColorPanel;
			
			private Label userColorLabel;
			private Label baseColorLabel;
			
			private bool panelSelected = false;
			
			private Panel selectedBaseColourPanel;
			
			private int currentlyUsedUserColorPanel = 0;
			private int[] customColors = null;
			
			private ColorDialogPanel colorDialogPanel = null;
			
			public BaseColorControl( ColorDialogPanel colorDialogPanel )
			{
				this.colorDialogPanel = colorDialogPanel;
				
				userColorPanel = new Panel[ 16 ];
				userColorPanel[ 0 ] = new Panel( );
				userColorPanel[ 1 ] = new Panel( );
				userColorPanel[ 2 ] = new Panel( );
				userColorPanel[ 3 ] = new Panel( );
				userColorPanel[ 4 ] = new Panel( );
				userColorPanel[ 5 ] = new Panel( );
				userColorPanel[ 6 ] = new Panel( );
				userColorPanel[ 7 ] = new Panel( );
				userColorPanel[ 8 ] = new Panel( );
				userColorPanel[ 9 ] = new Panel( );
				userColorPanel[ 10 ] = new Panel( );
				userColorPanel[ 11 ] = new Panel( );
				userColorPanel[ 12 ] = new Panel( );
				userColorPanel[ 13 ] = new Panel( );
				userColorPanel[ 14 ] = new Panel( );
				userColorPanel[ 15 ] = new Panel( );
				
				colorPanel = new Panel[ 48 ];
				colorPanel[ 0 ] = new Panel( );
				colorPanel[ 1 ] = new Panel( );
				colorPanel[ 2 ] = new Panel( );
				colorPanel[ 3 ] = new Panel( );
				colorPanel[ 4 ] = new Panel( );
				colorPanel[ 5 ] = new Panel( );
				colorPanel[ 6 ] = new Panel( );
				colorPanel[ 7 ] = new Panel( );
				colorPanel[ 8 ] = new Panel( );
				colorPanel[ 9 ] = new Panel( );
				colorPanel[ 10 ] = new Panel( );
				colorPanel[ 11 ] = new Panel( );
				colorPanel[ 12 ] = new Panel( );
				colorPanel[ 13 ] = new Panel( );
				colorPanel[ 14 ] = new Panel( );
				colorPanel[ 15 ] = new Panel( );
				colorPanel[ 16 ] = new Panel( );
				colorPanel[ 17 ] = new Panel( );
				colorPanel[ 18 ] = new Panel( );
				colorPanel[ 19 ] = new Panel( );
				colorPanel[ 20 ] = new Panel( );
				colorPanel[ 21 ] = new Panel( );
				colorPanel[ 22 ] = new Panel( );
				colorPanel[ 23 ] = new Panel( );
				colorPanel[ 24 ] = new Panel( );
				colorPanel[ 25 ] = new Panel( );
				colorPanel[ 26 ] = new Panel( );
				colorPanel[ 27 ] = new Panel( );
				colorPanel[ 28 ] = new Panel( );
				colorPanel[ 29 ] = new Panel( );
				colorPanel[ 30 ] = new Panel( );
				colorPanel[ 31 ] = new Panel( );
				colorPanel[ 32 ] = new Panel( );
				colorPanel[ 33 ] = new Panel( );
				colorPanel[ 34 ] = new Panel( );
				colorPanel[ 35 ] = new Panel( );
				colorPanel[ 36 ] = new Panel( );
				colorPanel[ 37 ] = new Panel( );
				colorPanel[ 38 ] = new Panel( );
				colorPanel[ 39 ] = new Panel( );
				colorPanel[ 40 ] = new Panel( );
				colorPanel[ 41 ] = new Panel( );
				colorPanel[ 42 ] = new Panel( );
				colorPanel[ 43 ] = new Panel( );
				colorPanel[ 44 ] = new Panel( );
				colorPanel[ 45 ] = new Panel( );
				colorPanel[ 46 ] = new Panel( ); //Black
				colorPanel[ 47 ] = new Panel( );
				
				baseColorLabel = new Label( );
				userColorLabel = new Label( );
				
				SuspendLayout( );
				
				// colorPanel1
				colorPanel[ 0 ].BackColor = Color.FromArgb( ( (Byte)( 255 ) ), ( (Byte)( 128 ) ), ( (Byte)( 138 ) ) );
				colorPanel[ 0 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 0 ].Location = new Point( 4, 19 );
				colorPanel[ 0 ].Size = new Size( 20, 17 );
				colorPanel[ 0 ].TabIndex = 51;
				colorPanel[ 0 ].Click += new EventHandler( OnClickPanel );
				// colorPanel2
				colorPanel[ 1 ].BackColor = Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 128 ) ), ( (Byte)( 64 ) ) );
				colorPanel[ 1 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 1 ].Location = new Point( 55, 128 );
				colorPanel[ 1 ].Size = new Size( 20, 17 );
				colorPanel[ 1 ].TabIndex = 92;
				colorPanel[ 1 ].Click += new EventHandler( OnClickPanel );
				// colorPanel3
				colorPanel[ 2 ].BackColor = Color.Gray;
				colorPanel[ 2 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 2 ].Location = new Point( 80, 128 );
				colorPanel[ 2 ].Size = new Size( 20, 17 );
				colorPanel[ 2 ].TabIndex = 93;
				colorPanel[ 2 ].Click += new EventHandler( OnClickPanel );
				// colorPanel4
				colorPanel[ 3 ].BackColor = Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 0 ) ), ( (Byte)( 255 ) ) );
				colorPanel[ 3 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 3 ].Location = new Point( 180, 85 );
				colorPanel[ 3 ].Size = new Size( 20, 17 );
				colorPanel[ 3 ].TabIndex = 98;
				colorPanel[ 3 ].Click += new EventHandler( OnClickPanel );
				// colorPanel5
				colorPanel[ 4 ].BackColor = Color.Silver;
				colorPanel[ 4 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 4 ].Location = new Point( 130, 128 );
				colorPanel[ 4 ].Size = new Size( 20, 17 );
				colorPanel[ 4 ].TabIndex = 95;
				colorPanel[ 4 ].Click += new EventHandler( OnClickPanel );
				// colorPanel6
				colorPanel[ 5 ].BackColor = Color.FromArgb( ( (Byte)( 64 ) ), ( (Byte)( 128 ) ), ( (Byte)( 128 ) ) );
				colorPanel[ 5 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 5 ].Location = new Point( 105, 128 );
				colorPanel[ 5 ].Size = new Size( 20, 17 );
				colorPanel[ 5 ].TabIndex = 94;
				colorPanel[ 5 ].Click += new EventHandler( OnClickPanel );
				// colorPanel7
				colorPanel[ 6 ].BackColor = Color.White;
				colorPanel[ 6 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 6 ].Location = new Point( 180, 128 );
				colorPanel[ 6 ].Size = new Size( 20, 17 );
				colorPanel[ 6 ].TabIndex = 97;
				colorPanel[ 6 ].Click += new EventHandler( OnClickPanel );
				// colorPanel8
				colorPanel[ 7 ].BackColor = Color.FromArgb( ( (Byte)( 64 ) ), ( (Byte)( 0 ) ), ( (Byte)( 64 ) ) );
				colorPanel[ 7 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 7 ].Location = new Point( 155, 128 );
				colorPanel[ 7 ].Size = new Size( 20, 17 );
				colorPanel[ 7 ].TabIndex = 96;
				colorPanel[ 7 ].Click += new EventHandler( OnClickPanel );
				// colorPanel9
				colorPanel[ 8 ].BackColor = Color.FromArgb( ( (Byte)( 255 ) ), ( (Byte)( 128 ) ), ( (Byte)( 64 ) ) );
				colorPanel[ 8 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 8 ].Location = new Point( 29, 63 );
				colorPanel[ 8 ].Size = new Size( 20, 17 );
				colorPanel[ 8 ].TabIndex = 68;
				colorPanel[ 8 ].Click += new EventHandler( OnClickPanel );
				// colorPanel10
				colorPanel[ 9 ].BackColor = Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 64 ) ), ( (Byte)( 64 ) ) );
				colorPanel[ 9 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 9 ].Location = new Point( 4, 63 );
				colorPanel[ 9 ].Size = new Size( 20, 17 );
				colorPanel[ 9 ].TabIndex = 67;
				colorPanel[ 9 ].Click += new EventHandler( OnClickPanel );
				// colorPanel11
				colorPanel[ 10 ].BackColor = Color.Teal;
				colorPanel[ 10 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 10 ].Location = new Point( 80, 63 );
				colorPanel[ 10 ].Size = new Size( 20, 17 );
				colorPanel[ 10 ].TabIndex = 70;
				colorPanel[ 10 ].Click += new EventHandler( OnClickPanel );
				// colorPanel12
				colorPanel[ 11 ].BackColor = Color.Lime;
				colorPanel[ 11 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 11 ].Location = new Point( 55, 63 );
				colorPanel[ 11 ].Size = new Size( 20, 17 );
				colorPanel[ 11 ].TabIndex = 69;
				colorPanel[ 11 ].Click += new EventHandler( OnClickPanel );
				// colorPanel13
				colorPanel[ 12 ].BackColor = Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 128 ) ), ( (Byte)( 255 ) ) );
				colorPanel[ 12 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 12 ].Location = new Point( 130, 63 );
				colorPanel[ 12 ].Size = new Size( 20, 17 );
				colorPanel[ 12 ].TabIndex = 72;
				colorPanel[ 12 ].Click += new EventHandler( OnClickPanel );
				// colorPanel14
				colorPanel[ 13 ].BackColor = Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 64 ) ), ( (Byte)( 128 ) ) );
				colorPanel[ 13 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 13 ].Location = new Point( 105, 63 );
				colorPanel[ 13 ].Size = new Size( 20, 17 );
				colorPanel[ 13 ].TabIndex = 71;
				colorPanel[ 13 ].Click += new EventHandler( OnClickPanel );
				// colorPanel15
				colorPanel[ 14 ].BackColor = Color.FromArgb( ( (Byte)( 255 ) ), ( (Byte)( 0 ) ), ( (Byte)( 128 ) ) );
				colorPanel[ 14 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 14 ].Location = new Point( 180, 63 );
				colorPanel[ 14 ].Size = new Size( 20, 17 );
				colorPanel[ 14 ].TabIndex = 74;
				colorPanel[ 14 ].Click += new EventHandler( OnClickPanel );
				// colorPanel16
				colorPanel[ 15 ].BackColor = Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 255 ) ), ( (Byte)( 0 ) ) );
				colorPanel[ 15 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 15 ].Location = new Point( 55, 41 );
				colorPanel[ 15 ].Size = new Size( 20, 17 );
				colorPanel[ 15 ].TabIndex = 61;
				colorPanel[ 15 ].Click += new EventHandler( OnClickPanel );
				// colorPanel17
				colorPanel[ 16 ].BackColor = Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 255 ) ), ( (Byte)( 64 ) ) );
				colorPanel[ 16 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 16 ].Location = new Point( 80, 41 );
				colorPanel[ 16 ].Size = new Size( 20, 17 );
				colorPanel[ 16 ].TabIndex = 62;
				colorPanel[ 16 ].Click += new EventHandler( OnClickPanel );
				// colorPanel18
				colorPanel[ 17 ].BackColor = Color.Red;
				colorPanel[ 17 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 17 ].Location = new Point( 4, 41 );
				colorPanel[ 17 ].Size = new Size( 20, 17 );
				colorPanel[ 17 ].TabIndex = 59;
				colorPanel[ 17 ].Click += new EventHandler( OnClickPanel );
				// colorPanel19
				colorPanel[ 18 ].BackColor = Color.FromArgb( ( (Byte)( 255 ) ), ( (Byte)( 128 ) ), ( (Byte)( 0 ) ) );
				colorPanel[ 18 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 18 ].Location = new Point( 29, 85 );
				colorPanel[ 18 ].Size = new Size( 20, 17 );
				colorPanel[ 18 ].TabIndex = 75;
				colorPanel[ 18 ].Click += new EventHandler( OnClickPanel );
				// colorPanel20
				colorPanel[ 19 ].BackColor = Color.FromArgb( ( (Byte)( 255 ) ), ( (Byte)( 128 ) ), ( (Byte)( 255 ) ) );
				colorPanel[ 19 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 19 ].Location = new Point( 180, 19 );
				colorPanel[ 19 ].Size = new Size( 20, 17 );
				colorPanel[ 19 ].TabIndex = 58;
				colorPanel[ 19 ].Click += new EventHandler( OnClickPanel );
				// colorPanel21
				colorPanel[ 20 ].BackColor = Color.Fuchsia;
				colorPanel[ 20 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 20 ].Location = new Point( 180, 41 );
				colorPanel[ 20 ].Size = new Size( 20, 17 );
				colorPanel[ 20 ].TabIndex = 66;
				colorPanel[ 20 ].Click += new EventHandler( OnClickPanel );
				// colorPanel22
				colorPanel[ 21 ].BackColor = Color.Aqua;
				colorPanel[ 21 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 21 ].Location = new Point( 105, 41 );
				colorPanel[ 21 ].Size = new Size( 20, 17 );
				colorPanel[ 21 ].TabIndex = 63;
				colorPanel[ 21 ].Click += new EventHandler( OnClickPanel );
				// colorPanel23
				colorPanel[ 22 ].BackColor = Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 255 ) ), ( (Byte)( 128 ) ) );
				colorPanel[ 22 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 22 ].Location = new Point( 55, 19 );
				colorPanel[ 22 ].Size = new Size( 20, 17 );
				colorPanel[ 22 ].TabIndex = 53;
				colorPanel[ 22 ].Click += new EventHandler( OnClickPanel );
				// colorPanel24
				colorPanel[ 23 ].BackColor = Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 255 ) ), ( (Byte)( 255 ) ) );
				colorPanel[ 23 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 23 ].Location = new Point( 105, 19 );
				colorPanel[ 23 ].Size = new Size( 20, 17 );
				colorPanel[ 23 ].TabIndex = 55;
				colorPanel[ 23 ].Click += new EventHandler( OnClickPanel );
				// colorPanel25
				colorPanel[ 24 ].BackColor = Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 128 ) ), ( (Byte)( 255 ) ) );
				colorPanel[ 24 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 24 ].Location = new Point( 130, 19 );
				colorPanel[ 24 ].Size = new Size( 20, 17 );
				colorPanel[ 24 ].TabIndex = 56;
				colorPanel[ 24 ].Click += new EventHandler( OnClickPanel );
				// colorPanel26
				colorPanel[ 25 ].BackColor = Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 64 ) ), ( (Byte)( 0 ) ) );
				colorPanel[ 25 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 25 ].Location = new Point( 29, 107 );
				colorPanel[ 25 ].Size = new Size( 20, 17 );
				colorPanel[ 25 ].TabIndex = 83;
				colorPanel[ 25 ].Click += new EventHandler( OnClickPanel );
				// colorPanel27
				colorPanel[ 26 ].BackColor = Color.FromArgb( ( (Byte)( 64 ) ), ( (Byte)( 0 ) ), ( (Byte)( 0 ) ) );
				colorPanel[ 26 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 26 ].Location = new Point( 4, 107 );
				colorPanel[ 26 ].Size = new Size( 20, 17 );
				colorPanel[ 26 ].TabIndex = 82;
				colorPanel[ 26 ].Click += new EventHandler( OnClickPanel );
				// colorPanel28
				colorPanel[ 27 ].BackColor = Color.Maroon;
				colorPanel[ 27 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 27 ].Location = new Point( 4, 85 );
				colorPanel[ 27 ].Size = new Size( 20, 17 );
				colorPanel[ 27 ].TabIndex = 81;
				colorPanel[ 27 ].Click += new EventHandler( OnClickPanel );
				// colorPanel29
				colorPanel[ 28 ].BackColor = Color.Purple;
				colorPanel[ 28 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 28 ].Location = new Point( 155, 85 );
				colorPanel[ 28 ].Size = new Size( 20, 17 );
				colorPanel[ 28 ].TabIndex = 80;
				colorPanel[ 28 ].Click += new EventHandler( OnClickPanel );
				// colorPanel30
				colorPanel[ 29 ].BackColor = Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 0 ) ), ( (Byte)( 160 ) ) );
				colorPanel[ 29 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 29 ].Location = new Point( 130, 85 );
				colorPanel[ 29 ].Size = new Size( 20, 17 );
				colorPanel[ 29 ].TabIndex = 79;
				colorPanel[ 29 ].Click += new EventHandler( OnClickPanel );
				// colorPanel31
				colorPanel[ 30 ].BackColor = Color.Blue;
				colorPanel[ 30 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 30 ].Location  = new Point( 105, 85 );
				colorPanel[ 30 ].Size = new Size( 20, 17 );
				colorPanel[ 30 ].TabIndex = 78;
				colorPanel[ 30 ].Click += new EventHandler( OnClickPanel );
				// colorPanel32
				colorPanel[ 31 ].BackColor = Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 128 ) ), ( (Byte)( 64 ) ) );
				colorPanel[ 31 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 31 ].Location = new Point( 80, 85 );
				colorPanel[ 31 ].Size = new Size( 20, 17 );
				colorPanel[ 31 ].TabIndex = 77;
				colorPanel[ 31 ].Click += new EventHandler( OnClickPanel );
				// colorPanel33
				colorPanel[ 32 ].BackColor = Color.Green;
				colorPanel[ 32 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 32 ].Location = new Point( 55, 85 );
				colorPanel[ 32 ].Size = new Size( 20, 17 );
				colorPanel[ 32 ].TabIndex = 76;
				colorPanel[ 32 ].Click += new EventHandler( OnClickPanel );
				// colorPanel34
				colorPanel[ 33 ].BackColor = Color.Yellow;
				colorPanel[ 33 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 33 ].Location = new Point( 29, 41 );
				colorPanel[ 33 ].Size = new Size( 20, 17 );
				colorPanel[ 33 ].TabIndex = 60;
				colorPanel[ 33 ].Click += new EventHandler( OnClickPanel );
				// colorPanel35
				colorPanel[ 34 ].BackColor = Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 128 ) ), ( (Byte)( 192 ) ) );
				colorPanel[ 34 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 34 ].Location = new Point( 155, 41 );
				colorPanel[ 34 ].Size = new Size( 20, 17 );
				colorPanel[ 34 ].TabIndex = 65;
				colorPanel[ 34 ].Click += new EventHandler( OnClickPanel );
				// colorPanel36
				colorPanel[ 35 ].BackColor = Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 128 ) ), ( (Byte)( 192 ) ) );
				colorPanel[ 35 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 35 ].Location = new Point( 130, 41 );
				colorPanel[ 35 ].Size = new Size( 20, 17 );
				colorPanel[ 35 ].TabIndex = 64;
				colorPanel[ 35 ].Click += new EventHandler( OnClickPanel );
				// colorPanel37
				colorPanel[ 36 ].BackColor = Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 0 ) ), ( (Byte)( 64 ) ) );
				colorPanel[ 36 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 36 ].Location = new Point( 155, 63 );
				colorPanel[ 36 ].Size = new Size( 20, 17 );
				colorPanel[ 36 ].TabIndex = 73;
				colorPanel[ 36 ].Click += new EventHandler( OnClickPanel );
				// colorPanel38
				colorPanel[ 37 ].BackColor = Color.FromArgb( ( (Byte)( 255 ) ), ( (Byte)( 128 ) ), ( (Byte)( 192 ) ) );
				colorPanel[ 37 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 37 ].Location = new Point( 155, 19 );
				colorPanel[ 37 ].Size = new Size( 20, 17 );
				colorPanel[ 37 ].TabIndex = 57;
				colorPanel[ 37 ].Click += new EventHandler( OnClickPanel );
				// colorPanel39
				colorPanel[ 38 ].BackColor = Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 255 ) ), ( (Byte)( 128 ) ) );
				colorPanel[ 38 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 38 ].Location = new Point( 80, 19 );
				colorPanel[ 38 ].Size = new Size( 20, 17 );
				colorPanel[ 38 ].TabIndex = 54;
				colorPanel[ 38 ].Click += new EventHandler( OnClickPanel );
				// colorPanel40
				colorPanel[ 39 ].BackColor = Color.FromArgb( ( (Byte)( 255 ) ), ( (Byte)( 255 ) ), ( (Byte)( 128 ) ) );
				colorPanel[ 39 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 39 ].Location = new Point( 29, 19 );
				colorPanel[ 39 ].Size = new Size( 20, 17 );
				colorPanel[ 39 ].TabIndex = 52;
				colorPanel[ 39 ].Click += new EventHandler( OnClickPanel );
				// colorPanel41
				colorPanel[ 40 ].BackColor = Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 64 ) ), ( (Byte)( 0 ) ) );
				colorPanel[ 40 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 40 ].Location = new Point( 55, 107 );
				colorPanel[ 40 ].Size = new Size( 20, 17 );
				colorPanel[ 40 ].TabIndex = 84;
				colorPanel[ 40 ].Click += new EventHandler( OnClickPanel );
				// colorPanel42
				colorPanel[ 41 ].BackColor = Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 64 ) ), ( (Byte)( 64 ) ) );
				colorPanel[ 41 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 41 ].Location = new Point( 80, 107 );
				colorPanel[ 41 ].Size = new Size( 20, 17 );
				colorPanel[ 41 ].TabIndex = 85;
				colorPanel[ 41 ].Click += new EventHandler( OnClickPanel );
				// colorPanel43
				colorPanel[ 42 ].BackColor = Color.Navy;
				colorPanel[ 42 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 42 ].Location = new Point( 105, 107 );
				colorPanel[ 42 ].Size = new Size( 20, 17 );
				colorPanel[ 42 ].TabIndex = 86;
				colorPanel[ 42 ].Click += new EventHandler( OnClickPanel );
				// colorPanel44
				colorPanel[ 43 ].BackColor = Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 0 ) ), ( (Byte)( 64 ) ) );
				colorPanel[ 43 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 43 ].Location = new Point( 130, 107 );
				colorPanel[ 43 ].Size = new Size( 20, 17 );
				colorPanel[ 43 ].TabIndex = 87;
				colorPanel[ 43 ].Click += new EventHandler( OnClickPanel );
				// colorPanel45
				colorPanel[ 44 ].BackColor = Color.FromArgb( ( (Byte)( 64 ) ), ( (Byte)( 0 ) ), ( (Byte)( 64 ) ) );
				colorPanel[ 44 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 44 ].Location = new Point( 155, 107 );
				colorPanel[ 44 ].Size = new Size( 20, 17 );
				colorPanel[ 44 ].TabIndex = 88;
				colorPanel[ 44 ].Click += new EventHandler( OnClickPanel );
				// colorPanel46
				colorPanel[ 45 ].BackColor = Color.FromArgb( ( (Byte)( 64 ) ), ( (Byte)( 0 ) ), ( (Byte)( 128 ) ) );
				colorPanel[ 45 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 45 ].Location = new Point( 180, 107 );
				colorPanel[ 45 ].Size = new Size( 20, 17 );
				colorPanel[ 45 ].TabIndex = 89;
				colorPanel[ 45 ].Click += new EventHandler( OnClickPanel );
				// colorPanel47
				colorPanel[ 46 ].BackColor = Color.Black;
				colorPanel[ 46 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 46 ].Location = new Point( 4, 128 );
				colorPanel[ 46 ].Size = new Size( 20, 17 );
				colorPanel[ 46 ].TabIndex = 90;
				colorPanel[ 46 ].Click += new EventHandler( OnClickPanel );
				// colorPanel48
				colorPanel[ 47 ].BackColor = Color.Olive;
				colorPanel[ 47 ].BorderStyle = BorderStyle.Fixed3D;
				colorPanel[ 47 ].Location = new Point( 29, 128 );
				colorPanel[ 47 ].Size = new Size( 20, 17 );
				colorPanel[ 47 ].TabIndex = 91;
				colorPanel[ 47 ].Click += new EventHandler( OnClickPanel );
				
				// userColorPane1
				userColorPanel[ 0 ].BackColor = Color.White;
				userColorPanel[ 0 ].BorderStyle = BorderStyle.Fixed3D;
				userColorPanel[ 0 ].Location = new Point( 4, 184 );
				userColorPanel[ 0 ].Size = new Size( 20, 17 );
				userColorPanel[ 0 ].TabIndex = 99;
				userColorPanel[ 0 ].Click += new EventHandler( OnClickPanel );
				// userColorPanel2
				userColorPanel[ 1 ].BackColor = Color.White;
				userColorPanel[ 1 ].BorderStyle = BorderStyle.Fixed3D;
				userColorPanel[ 1 ].Location = new Point( 4, 207 );
				userColorPanel[ 1 ].Size = new Size( 20, 17 );
				userColorPanel[ 1 ].TabIndex = 108;
				userColorPanel[ 1 ].Click += new EventHandler( OnClickPanel );
				// userColorPanel13
				userColorPanel[ 2 ].BackColor = Color.White;
				userColorPanel[ 2 ].BorderStyle = BorderStyle.Fixed3D;
				userColorPanel[ 2 ].Location = new Point( 29, 184 );
				userColorPanel[ 2 ].Size = new Size( 20, 17 );
				userColorPanel[ 2 ].TabIndex = 100;
				userColorPanel[ 2 ].Click += new EventHandler( OnClickPanel );
				// userColorPanel4
				userColorPanel[ 3 ].BackColor = Color.White;
				userColorPanel[ 3 ].BorderStyle = BorderStyle.Fixed3D;
				userColorPanel[ 3 ].Location = new Point( 29, 207 );
				userColorPanel[ 3 ].Size = new Size( 20, 17 );
				userColorPanel[ 3 ].TabIndex = 109;
				userColorPanel[ 3 ].Click += new EventHandler( OnClickPanel );
				// userColorPanel5
				userColorPanel[ 4 ].BackColor = Color.White;
				userColorPanel[ 4 ].BorderStyle = BorderStyle.Fixed3D;
				userColorPanel[ 4 ].Location = new Point( 55, 184 );
				userColorPanel[ 4 ].Size = new Size( 20, 17 );
				userColorPanel[ 4 ].TabIndex = 101;
				userColorPanel[ 4 ].Click += new EventHandler( OnClickPanel );
				// userColorPanel6
				userColorPanel[ 5 ].BackColor = Color.White;
				userColorPanel[ 5 ].BorderStyle = BorderStyle.Fixed3D;
				userColorPanel[ 5 ].Location = new Point( 55, 207 );
				userColorPanel[ 5 ].Size = new Size( 20, 17 );
				userColorPanel[ 5 ].TabIndex = 110;
				userColorPanel[ 5 ].Click += new EventHandler( OnClickPanel );
				// userColorPanel7
				userColorPanel[ 6 ].BackColor = Color.White;
				userColorPanel[ 6 ].BorderStyle = BorderStyle.Fixed3D;
				userColorPanel[ 6 ].Location = new Point( 80, 184 );
				userColorPanel[ 6 ].Size = new Size( 20, 17 );
				userColorPanel[ 6 ].TabIndex = 102;
				userColorPanel[ 6 ].Click += new EventHandler( OnClickPanel );
				// userColorPanel8
				userColorPanel[ 7 ].BackColor = Color.White;
				userColorPanel[ 7 ].BorderStyle = BorderStyle.Fixed3D;
				userColorPanel[ 7 ].Location = new Point( 80, 207 );
				userColorPanel[ 7 ].Size = new Size( 20, 17 );
				userColorPanel[ 7 ].TabIndex = 111;
				userColorPanel[ 7 ].Click += new EventHandler( OnClickPanel );
				// userColorPanel9
				userColorPanel[ 8 ].BackColor = Color.White;
				userColorPanel[ 8 ].BorderStyle = BorderStyle.Fixed3D;
				userColorPanel[ 8 ].Location = new Point( 105, 184 );
				userColorPanel[ 8 ].Size = new Size( 20, 17 );
				userColorPanel[ 8 ].TabIndex = 103;
				userColorPanel[ 8 ].Click += new EventHandler( OnClickPanel );
				// userColorPanel10
				userColorPanel[ 9 ].BackColor = Color.White;
				userColorPanel[ 9 ].BorderStyle = BorderStyle.Fixed3D;
				userColorPanel[ 9 ].Location = new Point( 105, 207 );
				userColorPanel[ 9 ].Size = new Size( 20, 17 );
				userColorPanel[ 9 ].TabIndex = 112;
				userColorPanel[ 9 ].Click += new EventHandler( OnClickPanel );
				// userColorPanel11
				userColorPanel[ 10 ].BackColor = Color.White;
				userColorPanel[ 10 ].BorderStyle = BorderStyle.Fixed3D;
				userColorPanel[ 10 ].Location = new Point( 130, 184 );
				userColorPanel[ 10 ].Size = new Size( 20, 17 );
				userColorPanel[ 10 ].TabIndex = 105;
				userColorPanel[ 10 ].Click += new EventHandler( OnClickPanel );
				// userColorPanel2
				userColorPanel[ 11 ].BackColor = Color.White;
				userColorPanel[ 11 ].BorderStyle = BorderStyle.Fixed3D;
				userColorPanel[ 11 ].Location = new Point( 130, 207 );
				userColorPanel[ 11 ].Size = new Size( 20, 17 );
				userColorPanel[ 11 ].TabIndex = 113;
				userColorPanel[ 11 ].Click += new EventHandler( OnClickPanel );
				// userColorPanel13
				userColorPanel[ 12 ].BackColor = Color.White;
				userColorPanel[ 12 ].BorderStyle = BorderStyle.Fixed3D;
				userColorPanel[ 12 ].Location = new Point( 155, 184 );
				userColorPanel[ 12 ].Size = new Size( 20, 17 );
				userColorPanel[ 12 ].TabIndex = 106;
				userColorPanel[ 12 ].Click += new EventHandler( OnClickPanel );
				// userColorPanel4
				userColorPanel[ 13 ].BackColor = Color.White;
				userColorPanel[ 13 ].BorderStyle = BorderStyle.Fixed3D;
				userColorPanel[ 13 ].Location = new Point( 155, 207 );
				userColorPanel[ 13 ].Size = new Size( 20, 17 );
				userColorPanel[ 13 ].TabIndex = 114;
				userColorPanel[ 13 ].Click += new EventHandler( OnClickPanel );
				// userColorPanel15
				userColorPanel[ 14 ].BackColor = Color.White;
				userColorPanel[ 14 ].BorderStyle = BorderStyle.Fixed3D;
				userColorPanel[ 14 ].Location = new Point( 180, 184 );
				userColorPanel[ 14 ].Size = new Size( 20, 17 );
				userColorPanel[ 14 ].TabIndex = 107;
				userColorPanel[ 14 ].Click += new EventHandler( OnClickPanel );
				// userColorPanel16
				userColorPanel[ 15 ].BackColor = Color.White;
				userColorPanel[ 15 ].BorderStyle = BorderStyle.Fixed3D;
				userColorPanel[ 15 ].Location = new Point( 180, 207 );
				userColorPanel[ 15 ].Size = new Size( 20, 17 );
				userColorPanel[ 15 ].TabIndex = 115;
				userColorPanel[ 15 ].Click += new EventHandler( OnClickPanel );
				
				// baseColorLabel
				baseColorLabel.Location = new Point( 2, 0 );
				baseColorLabel.Size = new Size( 200, 12 );
				baseColorLabel.TabIndex = 5;
				baseColorLabel.Text = Locale.GetText( "Base Colours" ) + ":";
				// userColorLabel
				userColorLabel.FlatStyle = FlatStyle.System;
				userColorLabel.Location = new Point( 2, 164 );
				userColorLabel.Size = new Size( 200, 14 );
				userColorLabel.TabIndex = 104;
				userColorLabel.Text = Locale.GetText( "User Colors" ) + ":";
				
				Controls.Add( userColorPanel[ 7 ] );
				Controls.Add( userColorPanel[ 6 ] );
				Controls.Add( userColorPanel[ 5 ] );
				Controls.Add( userColorPanel[ 4 ] );
				Controls.Add( userColorPanel[ 3 ] );
				Controls.Add( userColorPanel[ 2 ] );
				Controls.Add( userColorPanel[ 1 ] );
				Controls.Add( userColorPanel[ 0 ] );
				Controls.Add( userColorPanel[ 15 ] );
				Controls.Add( userColorPanel[ 14 ] );
				Controls.Add( userColorPanel[ 13 ] );
				Controls.Add( userColorPanel[ 12 ] );
				Controls.Add( userColorPanel[ 11 ] );
				Controls.Add( userColorPanel[ 10 ] );
				Controls.Add( userColorPanel[ 9 ] );
				Controls.Add( userColorPanel[ 8 ] );
				
				Controls.Add( colorPanel[ 0 ] );
				Controls.Add( colorPanel[ 3 ] );
				Controls.Add( colorPanel[ 6 ] );
				Controls.Add( colorPanel[ 7 ] );
				Controls.Add( colorPanel[ 4 ] );
				Controls.Add( colorPanel[ 5 ] );
				Controls.Add( colorPanel[ 2 ] );
				Controls.Add( colorPanel[ 1 ] );
				Controls.Add( colorPanel[ 47 ] );
				Controls.Add( colorPanel[ 46 ] );
				Controls.Add( colorPanel[ 45 ] );
				Controls.Add( colorPanel[ 44 ] );
				Controls.Add( colorPanel[ 43 ] );
				Controls.Add( colorPanel[ 42 ] );
				Controls.Add( colorPanel[ 41 ] );
				Controls.Add( colorPanel[ 40 ] );
				Controls.Add( colorPanel[ 25 ] );
				Controls.Add( colorPanel[ 26 ] );
				Controls.Add( colorPanel[ 27 ] );
				Controls.Add( colorPanel[ 28 ] );
				Controls.Add( colorPanel[ 29 ] );
				Controls.Add( colorPanel[ 30 ] );
				Controls.Add( colorPanel[ 31 ] );
				Controls.Add( colorPanel[ 32 ] );
				Controls.Add( colorPanel[ 18 ] );
				Controls.Add( colorPanel[ 14 ] );
				Controls.Add( colorPanel[ 36 ] );
				Controls.Add( colorPanel[ 12 ] );
				Controls.Add( colorPanel[ 13 ] );
				Controls.Add( colorPanel[ 10 ] );
				Controls.Add( colorPanel[ 11 ] );
				Controls.Add( colorPanel[ 8 ] );
				Controls.Add( colorPanel[ 9 ] );
				Controls.Add( colorPanel[ 20 ] );
				Controls.Add( colorPanel[ 34 ] );
				Controls.Add( colorPanel[ 35 ] );
				Controls.Add( colorPanel[ 21 ] );
				Controls.Add( colorPanel[ 16 ] );
				Controls.Add( colorPanel[ 15 ] );
				Controls.Add( colorPanel[ 33 ] );
				Controls.Add( colorPanel[ 17 ] );
				Controls.Add( colorPanel[ 19 ] );
				Controls.Add( colorPanel[ 37 ] );
				Controls.Add( colorPanel[ 24 ] );
				Controls.Add( colorPanel[ 23 ] );
				Controls.Add( colorPanel[ 38 ] );
				Controls.Add( colorPanel[ 22 ] );
				Controls.Add( colorPanel[ 39 ] );
				
				Controls.Add( userColorLabel );
				Controls.Add( baseColorLabel );
				
				Size = new Size( 212, 238 );
				ResumeLayout( false );
				
				selectedBaseColourPanel = colorPanel[ 46 ];  // default, Black
				
				CheckIfColorIsInPanel( );
				
				panelSelected = false;
				
				SetStyle( ControlStyles.DoubleBuffer, true );
				SetStyle( ControlStyles.AllPaintingInWmPaint, true );
				SetStyle( ControlStyles.UserPaint, true );
			}
			
			private void CheckIfColorIsInPanel( )
			{
				if ( colorDialogPanel.ColorDialog.Color != Color.Black )
				{
					// check if we have a panel with a BackColor = ColorDialog.Color...
					for ( int i = 0; i < colorPanel.Length; i++ )
					{
						if ( colorPanel[ i ].BackColor == colorDialogPanel.ColorDialog.Color )
						{
							selectedBaseColourPanel = colorPanel[ i ];
							break;
						}
					}
				}
			}
			
			void OnClickPanel( object sender, EventArgs e )
			{
				panelSelected = true;
				
				selectedBaseColourPanel = (Panel)sender;
				
				TriangleControl.CurrentBrightness = HSB.Brightness( selectedBaseColourPanel.BackColor );
				
				colorDialogPanel.UpdateControls( selectedBaseColourPanel.BackColor );
				colorDialogPanel.UpdateRGBTextBoxes( selectedBaseColourPanel.BackColor );
				colorDialogPanel.UpdateHSBTextBoxes( selectedBaseColourPanel.BackColor );
				
				Invalidate( );
				
				Update( );
			}
			
			protected override void OnPaint( PaintEventArgs e )
			{
				e.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush (BackColor), ClientRectangle );
				
				ControlPaint.DrawBorder(
					e.Graphics,
					new Rectangle( selectedBaseColourPanel.Location.X - 3, selectedBaseColourPanel.Location.Y - 3, selectedBaseColourPanel.Size.Width + 6, selectedBaseColourPanel.Size.Height + 6 ),
					Color.Black,
					ButtonBorderStyle.Dotted
				);
				
				ControlPaint.DrawBorder(
					e.Graphics,
					new Rectangle( selectedBaseColourPanel.Location.X - 1, selectedBaseColourPanel.Location.Y - 1, selectedBaseColourPanel.Size.Width + 2, selectedBaseColourPanel.Size.Height + 2 ),
					Color.Black,
					ButtonBorderStyle.Solid
				);

				base.OnPaint( e );
			}
			
			public Color ColorToShow
			{
				get {
					return selectedBaseColourPanel.BackColor;
				}
			}
			
			public void SetUserColor( Color col )
			{
				userColorPanel[ currentlyUsedUserColorPanel ].BackColor = col;
				
				// check if this.customColors already exists
				if ( customColors == null )
				{
					customColors = new int[ 16 ];
					int white = Color.White.ToArgb( );
					
					for ( int i = 0; i < customColors.Length; i++ )
						customColors[ i ] = white;
				}
				
				customColors[ currentlyUsedUserColorPanel ] = col.ToArgb( );
				
				// update ColorDialog dialog property
				colorDialogPanel.ColorDialog.CustomColors = customColors;
				
				currentlyUsedUserColorPanel++;
				if ( currentlyUsedUserColorPanel > 15 )
					currentlyUsedUserColorPanel = 0;
			}
			
			public void SetCustomColors( )
			{
				int[] customColors = colorDialogPanel.ColorDialog.CustomColors;
				
				for ( int i = 0; i < customColors.Length; i++ )
				{
					userColorPanel[ i ].BackColor = Color.FromArgb( customColors[ i ] );
				}
			}
		}
		
		internal class ColorMatrixControl : Panel //PictureBox
		{
			internal class DrawingBitmap
			{
				private Bitmap bitmap;
				
				public DrawingBitmap( )
				{
					bitmap = new Bitmap( 180, 191 );
					
					float hueadd = 241.0f / 178.0f;
					float satsub = 241.0f / 189.0f;
					float satpos = 240.0f;
					
					// paint the matrix to the bitmap
					for ( int height = 0; height < 191; height++ )
					{
						float huepos = 0.0f;
						
						for ( int width = 0; width < 180; width++ )
						{
							HSB hsb = new HSB( );
							
							hsb.hue = (int)huepos;
							hsb.sat = (int)satpos;
							hsb.bri = 120; // paint it with 120 to get a nice bitmap
							
							bitmap.SetPixel( width, height, HSB.HSB2RGB( hsb.hue, hsb.sat, hsb.bri ) );
							
							huepos += hueadd;
						}
						
						satpos -= satsub;
					}
				}
				
				public Bitmap Bitmap
				{
					set {
						bitmap = value;
					}
					
					get {
						return bitmap;
					}
				}
			}
			
			internal class CrossCursor
			{
				private Bitmap bitmap;
				
				private Color cursorColor;
				
				public CrossCursor( )
				{
					bitmap = new Bitmap( 22, 22 );
					
					cursorColor = Color.Black;
					
					Draw( );
				}
				
				public void Draw( )
				{
					Pen pen = new Pen(ThemeEngine.Current.ResPool.GetSolidBrush (cursorColor), 3 );
					
					Graphics graphics = Graphics.FromImage( bitmap );
					
					graphics.DrawLine( pen, 11, 0, 11, 7 );
					graphics.DrawLine( pen, 11, 14, 11, 21 );
					graphics.DrawLine( pen, 0, 11, 7, 11 );
					graphics.DrawLine( pen, 14, 11, 21, 11 );
				}
				
				public Bitmap Bitmap
				{
					set {
						bitmap = value;
					}
					
					get {
						return bitmap;
					}
				}
				
				public Color CursorColor
				{
					set {
						cursorColor = value;
					}
					
					get {
						return cursorColor;
					}
				}
			}
			
			private DrawingBitmap drawingBitmap = new DrawingBitmap( );
			
			private CrossCursor crossCursor = new CrossCursor();
			
			private bool mouseButtonDown = false;
			
			private bool drawCross = true;
			
			private Color color;
			
			private int currentXPos;
			private int currentYPos;
			
			private const float xstep = 240.0f/178.0f;
			private const float ystep = 240.0f/189.0f;
			
			private ColorDialogPanel colorDialogPanel;
			
			public ColorMatrixControl( ColorDialogPanel colorDialogPanel )
			{
				this.colorDialogPanel = colorDialogPanel;
				
				SuspendLayout( );
				
				BorderStyle = BorderStyle.Fixed3D;
				Location = new Point( 0, 0 );
				Size = new Size( 179, 190 );
				TabIndex = 0;
				TabStop = false;
				//BackColor = SystemColors.Control;
				Size = new Size( 179, 190 );
				
				ResumeLayout( false );
				
				SetStyle( ControlStyles.DoubleBuffer, true );
				SetStyle( ControlStyles.AllPaintingInWmPaint, true );
				SetStyle( ControlStyles.UserPaint, true );
			}
			
			protected override void OnPaint( PaintEventArgs e )
			{
				Draw( e );
				
				base.OnPaint( e );
			}
			
			private void Draw( PaintEventArgs e )
			{
				Bitmap bmp = new Bitmap( drawingBitmap.Bitmap );
				
				e.Graphics.DrawImage( bmp, 0, 0 );
				
				// drawCross is false if the mouse gets moved...
				if ( drawCross )
				{
					e.Graphics.DrawImage( crossCursor.Bitmap, currentXPos - 11 , currentYPos - 11 );
				}
			}
			
			protected override void OnMouseDown( MouseEventArgs e )
			{
				mouseButtonDown = true;
				currentXPos = e.X;
				currentYPos = e.Y;
				if ( drawCross )
				{
					drawCross = false;
					Invalidate( );
					Update( );
				}
				
				UpdateControls( );
				
				base.OnMouseDown( e );
			}
			
			protected override void OnMouseMove( MouseEventArgs e )
			{
				if ( mouseButtonDown )
					if ( ( e.X < 178 && e.X >= 0 ) && ( e.Y < 189 && e.Y >= 0 ) ) // 177 189
					{
						currentXPos = e.X;
						currentYPos = e.Y;
						UpdateControls( );
					}
				
				base.OnMouseMove( e );
			}
			
			protected override void OnMouseUp( MouseEventArgs e )
			{
				mouseButtonDown = false;
				drawCross = true;
				Invalidate( );
				Update( );
			}
			
			public Color ColorToShow
			{
				set {
					color = value;
					
					HSB hsb = HSB.RGB2HSB( color );
					
					currentXPos = (int)( (float)hsb.hue / xstep );
					currentYPos = 189 - (int)( (float)hsb.sat / ystep );
					
					if ( currentXPos < 0 )
						currentXPos = 0;
					if ( currentYPos < 0 )
						currentYPos = 0;
					
					Invalidate( );
					Update( );
					
					UpdateControls( );
				}
			}
			
			private Color GetColorFromHSB( )
			{
				int hue = (int)( (float)currentXPos * xstep );
				int sat = 240 - ( (int)( (float)currentYPos * ystep ) );
				int bri = TriangleControl.CurrentBrightness;
				
				return HSB.HSB2RGB( hue, sat, bri );
			}
			
			private void UpdateControls( )
			{
				Color tmpColor = GetColorFromHSB( );
				
				// update the brightness control
				colorDialogPanel.BrightnessControl.ShowColor( (int)( (float)currentXPos * xstep ), 240 - ( (int)( (float)currentYPos * ystep ) ) );
				
				// update saturation text box
				int satvalue = ( 240 - ( (int)( (float)currentYPos * ystep ) ) );
				satvalue = satvalue == 240 ? 239 : satvalue;
				colorDialogPanel.SatTextBox.Text = satvalue.ToString( );
				
				// update hue text box
				colorDialogPanel.HueTextBox.Text = ( (int)( (float)currentXPos * xstep ) ).ToString( );
				
				// update the main selected color panel
				colorDialogPanel.SelectedColorPanel.BackColor = tmpColor;
				
				// and finally the rgb text boxes
				colorDialogPanel.UpdateRGBTextBoxes( tmpColor );
			}
		}
		
		
		internal class BrightnessControl : Panel
		{
			internal class DrawingBitmap
			{
				private Bitmap bitmap;
				
				public DrawingBitmap( )
				{
					bitmap = new Bitmap( 14, 190 );
				}
				
				public Bitmap Bitmap
				{
					set {
						bitmap = value;
					}
					
					get {
						return bitmap;
					}
				}
				
				// only hue and saturation are needed.
				// color will be computed with an iteration
				public void Draw( int hue, int sat )
				{
					float brisub = 240.0f / 190.0f;
					float bri = 240.0f;
					
					for ( int height = 0; height < 190; height++ )
					{
						for ( int width = 0; width < 14; width++ )
						{
							Color pixcolor = HSB.HSB2RGB( hue, sat, (int)bri );
							bitmap.SetPixel( width, height, pixcolor );
						}
						bri = bri - brisub;
					}
				}
			}
			
			private const float step = 240.0f/189.0f;
			
			private DrawingBitmap bitmap;
			
			private Color color;
			
			private ColorDialogPanel colorDialogPanel;
			
			public BrightnessControl( ColorDialogPanel colorDialogPanel )
			{
				this.colorDialogPanel = colorDialogPanel;
				
				SuspendLayout( );
				
				BorderStyle = BorderStyle.Fixed3D;
				Location = new Point( 0, 0 );
				Size = new Size( 14, 190 );
				TabIndex = 0;
				TabStop = false;
				Size = new Size( 14, 190 );
				ResumeLayout( false );
				
				bitmap = new DrawingBitmap( );
				
				SetStyle( ControlStyles.DoubleBuffer, true );
				SetStyle( ControlStyles.AllPaintingInWmPaint, true );
				SetStyle( ControlStyles.UserPaint, true );
			}
			
			
			protected override void OnPaint( PaintEventArgs e )
			{
				e.Graphics.DrawImage( bitmap.Bitmap, 0, 0 );
				
				base.OnPaint( e );
			}
			
			protected override void OnMouseDown( MouseEventArgs e )
			{
				colorDialogPanel.TriangleControl.TrianglePosition = (int)( (float)( 189 - e.Y ) * step );
				
				base.OnMouseDown( e );
			}
			
			// this one is for ColorMatrixControl
			public void ShowColor( int hue, int sat )
			{
				bitmap.Draw( hue, sat );
				Invalidate( );
				Update( );
			}
			
			// this one for the other controls
			public Color ColorToShow
			{
				set {
					int hue, sat;
					HSB.GetHueSaturation( value, out hue, out sat );
					bitmap.Draw( hue, sat );
					Invalidate( );
					Update( );
				}
			}
		}
		
		
		internal class TriangleControl : Panel
		{
			private bool mouseButtonDown = false;
			
			private int currentTrianglePosition = 195;
//			private Rectangle clipRectangle;
			
			private const float briStep = 239.0f/186.0f;
			
			private static int currentBrightness = 0;
			
			private ColorDialogPanel colorDialogPanel;
			
			public TriangleControl( ColorDialogPanel colorDialogPanel )
			{
				this.colorDialogPanel = colorDialogPanel;
				
				Size = new Size( 16, 203 );
				
				SetStyle( ControlStyles.DoubleBuffer, true );
				SetStyle( ControlStyles.AllPaintingInWmPaint, true );
				SetStyle( ControlStyles.UserPaint, true );
			}
			
			public static int CurrentBrightness
			{
				set {
					currentBrightness = value;
				}
				
				get {
					return currentBrightness;
				}
			}
			
			protected override void OnPaint( PaintEventArgs e )
			{
				Draw( e );
				
				base.OnPaint( e );
			}
			
			private void Draw( PaintEventArgs e )
			{
				e.Graphics.FillRectangle( new SolidBrush( SystemColors.Control ), new Rectangle( 0, 0, 16, 203 ) );
				
				Point[] trianglePoints = new Point[ 3 ]
				{
					new Point( 0, currentTrianglePosition ),
					new Point( 8, currentTrianglePosition - 8 ),
					new Point( 8, currentTrianglePosition + 8 )
				};
				
				e.Graphics.FillPolygon( ThemeEngine.Current.ResPool.GetSolidBrush (Color.Black ), trianglePoints );
			}
			
			protected override void OnMouseDown( MouseEventArgs e )
			{
				if ( e.Y > 195 || e.Y < 9 ) return; // helper until Cursor.Clip works
				
				mouseButtonDown = true;
				currentTrianglePosition = e.Y;
				
				// Cursor.Clip doesn't yet work in Managed.Windows.Forms
//				clipRectangle = Cursor.Clip;
//				Point p = Location;
//				p.Y += 8;
//				Size s = Size;
//				s.Width -= 5;
//				s.Height -= 16;
//				Cursor.Clip = new Rectangle( Parent.PointToScreen( p ), s );
				
				colorDialogPanel.BriTextBox.Text = TrianglePosition.ToString( );
				colorDialogPanel.UpdateFromHSBTextBoxes( );
				
				Invalidate( );
				Update( );
				
				base.OnMouseDown( e );
			}
			
			protected override void OnMouseMove( MouseEventArgs e )
			{
				if ( mouseButtonDown )
					if ( e.Y < 196 && e.Y > 8 )
					{
						currentTrianglePosition = e.Y;
						
						colorDialogPanel.BriTextBox.Text = TrianglePosition.ToString( );
						colorDialogPanel.UpdateFromHSBTextBoxes( );
						
						Invalidate( );
						Update( );
					}
				
				base.OnMouseMove( e );
			}
			
			protected override void OnMouseUp( MouseEventArgs e )
			{
				mouseButtonDown = false;
//				Cursor.Clip = clipRectangle;
				
				base.OnMouseUp( e );
			}
			
			public int TrianglePosition
			{
				get {
					float tmp = (float)( currentTrianglePosition - 9 );
					tmp = tmp * briStep;
					
					int retval = 239 - (int)tmp;
					
					TriangleControl.CurrentBrightness = retval;
					
					return retval;
				}
				
				set {
					float tmp = (float)value / briStep;
					currentTrianglePosition = 186 - (int)tmp + 9;
					
					colorDialogPanel.BriTextBox.Text = TrianglePosition.ToString( );
					colorDialogPanel.UpdateFromHSBTextBoxes( );
					
					Invalidate( );
					Update( );
				}
			}
			
			public Color ColorToShow
			{
				set {
					TrianglePosition = HSB.Brightness( value );
				}
			}
		}
		#endregion
	}
}
