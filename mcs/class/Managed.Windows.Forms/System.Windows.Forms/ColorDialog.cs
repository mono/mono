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

// COMPLETE

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
			get
			{
				return color;
			}
			
			set
			{
				color = value;
			}
		}
		
		[DefaultValue(true)]
		public virtual bool AllowFullOpen
		{
			get
			{
				return allowFullOpen;
			}
			
			set
			{
				allowFullOpen = value;
			}
		}
		
		// Currently AnyColor internally is always true
		// Does really anybody still use 256 or less colors ???
		// Naw, cairo only supports 24bit anyway - pdb
		[DefaultValue(false)]
		public virtual bool AnyColor
		{
			get
			{
				return anyColor;
			}
			
			set
			{
				anyColor = value;
			}
		}
		
		[DefaultValue(false)]
		public virtual bool FullOpen
		{
			get
			{
				return fullOpen;
			}
			
			set
			{
				fullOpen = value;
			}
		}
		
		[Browsable(false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int[] CustomColors
		{
			get
			{
				return customColors;
			}
			
			set
			{
				customColors = value;
			}
		}
		
		[DefaultValue(false)]
		public virtual bool ShowHelp
		{
			get
			{
				return showHelp;
			}
			
			set
			{
				showHelp = value;
			}
		}
		
		[DefaultValue(false)]
		public virtual bool SolidColorOnly
		{
			get
			{
				return solidColorOnly;
			}
			
			set
			{
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
			get
			{
				// MS Internal
				return (IntPtr)GetHashCode( );
			}
		}
		
		protected virtual int Options
		{
			get
			{
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
				baseColorControl.Location = new Point( 3, 6 );
				baseColorControl.Size = new Size( 212, 231 );
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
				set
				{
					selectedColorPanel = value;
				}
				
				get
				{
					return selectedColorPanel;
				}
			}
			
			public BrightnessControl BrightnessControl
			{
				set
				{
					brightnessControl = value;
				}
				
				get
				{
					return brightnessControl;
				}
			}
			
			public TextBox HueTextBox
			{
				set
				{
					hueTextBox = value;
				}
				
				get
				{
					return hueTextBox;
				}
			}
			
			public ColorMatrixControl ColorMatrixControl
			{
				set
				{
					colorMatrixControl = value;
				}
				
				get
				{
					return colorMatrixControl;
				}
			}
			
			public TriangleControl TriangleControl
			{
				set
				{
					triangleControl = value;
				}
				
				get
				{
					return triangleControl;
				}
			}
			
			public TextBox RedTextBox
			{
				set
				{
					redTextBox = value;
				}
				
				get
				{
					return redTextBox;
				}
			}
			
			public TextBox GreenTextBox
			{
				set
				{
					greenTextBox = value;
				}
				
				get
				{
					return greenTextBox;
				}
			}
			
			public BaseColorControl BaseColorControl
			{
				set
				{
					baseColorControl = value;
				}
				
				get
				{
					return baseColorControl;
				}
			}
			
			public TextBox BlueTextBox
			{
				set
				{
					blueTextBox = value;
				}
				
				get
				{
					return blueTextBox;
				}
			}
			
			public TextBox SatTextBox
			{
				set
				{
					satTextBox = value;
				}
				
				get
				{
					return satTextBox;
				}
			}
			
			public TextBox BriTextBox
			{
				set
				{
					briTextBox = value;
				}
				
				get
				{
					return briTextBox;
				}
			}
			
			public ColorDialog ColorDialog
			{
				set
				{
					colorDialog = value;
				}
				
				get
				{
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
			internal class SmallColorControl : Control
			{
				private Color color;
				
				private bool isSelected = false;
				private bool hasFocus = false;
				
				public SmallColorControl( Color color )
				{
					this.color = color;
					
					Size = new Size( 26, 23 );
					
					SetStyle( ControlStyles.DoubleBuffer, true );
					SetStyle( ControlStyles.AllPaintingInWmPaint, true );
					SetStyle( ControlStyles.UserPaint, true );
					SetStyle( ControlStyles.Selectable, true );
				}
				
				public bool IsSelected
				{
					set
					{
						isSelected = value;
						Invalidate( );
						Update( );
					}
					
					get
					{
						return isSelected;
					}
				}
				
				public Color Color
				{
					set
					{
						color = value;
						Invalidate( );
						Update( );
					}
					
					get
					{
						return color;
					}
				}
				
				protected override void OnPaint( PaintEventArgs pe )
				{
					base.OnPaint( pe );
					
					pe.Graphics.FillRectangle( ThemeEngine.Current.ResPool.GetSolidBrush( SystemColors.Control ), 0, 0, 26, 23 );
					
					ControlPaint.DrawBorder3D( pe.Graphics, new Rectangle( 3, 3, 20, 18 ) );
					
					pe.Graphics.FillRectangle( ThemeEngine.Current.ResPool.GetSolidBrush( color ),
								  new Rectangle( 4, 4, 16, 14 ) );
					
					if ( isSelected )
					{
						pe.Graphics.DrawRectangle( new Pen( ThemeEngine.Current.ResPool.GetSolidBrush( Color.Black ) ),
									  new Rectangle( 2, 2, 20, 18 ) );
					}
					
					if ( hasFocus && isSelected )
					{
						ControlPaint.DrawFocusRectangle(
							pe.Graphics, new Rectangle( 0, 0, 25, 23 )
						);
					}
				}
				
				protected override void OnLostFocus( EventArgs e )
				{
					hasFocus = false;
					
					Invalidate( );
					Update( );
					
					base.OnLostFocus( e );
				}
				
				protected override void OnMouseUp( MouseEventArgs e )
				{
					isSelected = true;
					
					hasFocus = true;
					
					Invalidate( );
					Update( );
					
					base.OnMouseUp( e );
				}
			}
			
			private SmallColorControl[] smallColorControl;
			
			private SmallColorControl[] userSmallColorControl;
			
			private Label userColorLabel;
			private Label baseColorLabel;
			
			private bool panelSelected = false;
			
			private SmallColorControl selectedSmallColorControl;
			
			private int currentlyUsedUserSmallColorControl = 0;
			private int[] customColors = null;
			
			private ColorDialogPanel colorDialogPanel = null;
			
			public BaseColorControl( ColorDialogPanel colorDialogPanel )
			{
				this.colorDialogPanel = colorDialogPanel;
				
				userSmallColorControl = new SmallColorControl[ 16 ];
				userSmallColorControl[ 0 ] = new SmallColorControl( Color.White );
				userSmallColorControl[ 1 ] = new SmallColorControl( Color.White );
				userSmallColorControl[ 2 ] = new SmallColorControl( Color.White );
				userSmallColorControl[ 3 ] = new SmallColorControl( Color.White );
				userSmallColorControl[ 4 ] = new SmallColorControl( Color.White );
				userSmallColorControl[ 5 ] = new SmallColorControl( Color.White );
				userSmallColorControl[ 6 ] = new SmallColorControl( Color.White );
				userSmallColorControl[ 7 ] = new SmallColorControl( Color.White );
				userSmallColorControl[ 8 ] = new SmallColorControl( Color.White );
				userSmallColorControl[ 9 ] = new SmallColorControl( Color.White );
				userSmallColorControl[ 10 ] = new SmallColorControl( Color.White );
				userSmallColorControl[ 11 ] = new SmallColorControl( Color.White );
				userSmallColorControl[ 12 ] = new SmallColorControl( Color.White );
				userSmallColorControl[ 13 ] = new SmallColorControl( Color.White );
				userSmallColorControl[ 14 ] = new SmallColorControl( Color.White );
				userSmallColorControl[ 15 ] = new SmallColorControl( Color.White );
				
				smallColorControl = new SmallColorControl[ 48 ];
				smallColorControl[ 0 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 255 ) ), ( (Byte)( 128 ) ), ( (Byte)( 138 ) ) ) );
				smallColorControl[ 1 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 128 ) ), ( (Byte)( 64 ) ) ) );
				smallColorControl[ 2 ] = new SmallColorControl( Color.Gray );
				smallColorControl[ 3 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 0 ) ), ( (Byte)( 255 ) ) ) );
				smallColorControl[ 4 ] = new SmallColorControl( Color.Silver );
				smallColorControl[ 5 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 64 ) ), ( (Byte)( 128 ) ), ( (Byte)( 128 ) ) ) );
				smallColorControl[ 6 ] = new SmallColorControl( Color.White );
				smallColorControl[ 7 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 64 ) ), ( (Byte)( 0 ) ), ( (Byte)( 64 ) ) ) );
				smallColorControl[ 8 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 255 ) ), ( (Byte)( 128 ) ), ( (Byte)( 64 ) ) ) );
				smallColorControl[ 9 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 64 ) ), ( (Byte)( 64 ) ) ) );
				smallColorControl[ 10 ] = new SmallColorControl( Color.Teal );
				smallColorControl[ 11 ] = new SmallColorControl( Color.Lime );
				smallColorControl[ 12 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 128 ) ), ( (Byte)( 255 ) ) ) );
				smallColorControl[ 13 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 64 ) ), ( (Byte)( 128 ) ) ) );
				smallColorControl[ 14 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 255 ) ), ( (Byte)( 0 ) ), ( (Byte)( 128 ) ) ) );
				smallColorControl[ 15 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 255 ) ), ( (Byte)( 0 ) ) ) );
				smallColorControl[ 16 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 255 ) ), ( (Byte)( 64 ) ) ) );
				smallColorControl[ 17 ] = new SmallColorControl( Color.Red );
				smallColorControl[ 18 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 255 ) ), ( (Byte)( 128 ) ), ( (Byte)( 0 ) ) ) );
				smallColorControl[ 19 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 255 ) ), ( (Byte)( 128 ) ), ( (Byte)( 255 ) ) ) );
				smallColorControl[ 20 ] = new SmallColorControl( Color.Fuchsia );
				smallColorControl[ 21 ] = new SmallColorControl( Color.Aqua );
				smallColorControl[ 22 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 255 ) ), ( (Byte)( 128 ) ) ) );
				smallColorControl[ 23 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 255 ) ), ( (Byte)( 255 ) ) ) );
				smallColorControl[ 24 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 128 ) ), ( (Byte)( 255 ) ) ) );
				smallColorControl[ 25 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 64 ) ), ( (Byte)( 0 ) ) ) );
				smallColorControl[ 26 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 64 ) ), ( (Byte)( 0 ) ), ( (Byte)( 0 ) ) ) );
				smallColorControl[ 27 ] = new SmallColorControl( Color.Maroon );
				smallColorControl[ 28 ] = new SmallColorControl( Color.Purple );
				smallColorControl[ 29 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 0 ) ), ( (Byte)( 160 ) ) ) );
				smallColorControl[ 30 ] = new SmallColorControl( Color.Blue );
				smallColorControl[ 31 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 128 ) ), ( (Byte)( 64 ) ) ) );
				smallColorControl[ 32 ] = new SmallColorControl( Color.Green );
				smallColorControl[ 33 ] = new SmallColorControl( Color.Yellow );
				smallColorControl[ 34 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 128 ) ), ( (Byte)( 192 ) ) ) );
				smallColorControl[ 35 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 128 ) ), ( (Byte)( 192 ) ) ) );
				smallColorControl[ 36 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 128 ) ), ( (Byte)( 0 ) ), ( (Byte)( 64 ) ) ) );
				smallColorControl[ 37 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 255 ) ), ( (Byte)( 128 ) ), ( (Byte)( 192 ) ) ) );
				smallColorControl[ 38 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 255 ) ), ( (Byte)( 128 ) ) ) );
				smallColorControl[ 39 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 255 ) ), ( (Byte)( 255 ) ), ( (Byte)( 128 ) ) ) );
				smallColorControl[ 40 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 64 ) ), ( (Byte)( 0 ) ) ) );
				smallColorControl[ 41 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 64 ) ), ( (Byte)( 64 ) ) ) );
				smallColorControl[ 42 ] = new SmallColorControl( Color.Navy );
				smallColorControl[ 43 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 0 ) ), ( (Byte)( 0 ) ), ( (Byte)( 64 ) ) ) );
				smallColorControl[ 44 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 64 ) ), ( (Byte)( 0 ) ), ( (Byte)( 64 ) ) ) );
				smallColorControl[ 45 ] = new SmallColorControl( Color.FromArgb( ( (Byte)( 64 ) ), ( (Byte)( 0 ) ), ( (Byte)( 128 ) ) ) );
				smallColorControl[ 46 ] = new SmallColorControl( Color.Black ); //Black
				smallColorControl[ 47 ] = new SmallColorControl( Color.Olive );
				
				baseColorLabel = new Label( );
				userColorLabel = new Label( );
				
				SuspendLayout( );
				
				// colorPanel1
				smallColorControl[ 0 ].Location = new Point( 0, 15 );
				smallColorControl[ 0 ].TabIndex = 51;
				smallColorControl[ 0 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel2
				smallColorControl[ 1 ].Location = new Point( 50, 130 );
				smallColorControl[ 1 ].TabIndex = 92;
				smallColorControl[ 1 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel3
				smallColorControl[ 2 ].Location = new Point( 75, 130 );
				smallColorControl[ 2 ].TabIndex = 93;
				smallColorControl[ 2 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel4
				smallColorControl[ 3 ].Location = new Point( 175, 84 );
				smallColorControl[ 3 ].TabIndex = 98;
				smallColorControl[ 3 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel5
				smallColorControl[ 4 ].Location = new Point( 125, 130 );
				smallColorControl[ 4 ].TabIndex = 95;
				smallColorControl[ 4 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel6
				smallColorControl[ 5 ].Location = new Point( 100, 130 );
				smallColorControl[ 5 ].TabIndex = 94;
				smallColorControl[ 5 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel7
				smallColorControl[ 6 ].Location = new Point( 175, 130 );
				smallColorControl[ 6 ].TabIndex = 97;
				smallColorControl[ 6 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel8
				smallColorControl[ 7 ].Location = new Point( 150, 130 );
				smallColorControl[ 7 ].TabIndex = 96;
				smallColorControl[ 7 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel9
				smallColorControl[ 8 ].Location = new Point( 25, 61 );
				smallColorControl[ 8 ].TabIndex = 68;
				smallColorControl[ 8 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel10
				smallColorControl[ 9 ].Location = new Point( 0, 61 );
				smallColorControl[ 9 ].TabIndex = 67;
				smallColorControl[ 9 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel11
				smallColorControl[ 10 ].Location = new Point( 75, 61 );
				smallColorControl[ 10 ].TabIndex = 70;
				smallColorControl[ 10 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel12
				smallColorControl[ 11 ].Location = new Point( 50, 61 );
				smallColorControl[ 11 ].TabIndex = 69;
				smallColorControl[ 11 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel13
				smallColorControl[ 12 ].Location = new Point( 125, 61 );
				smallColorControl[ 12 ].TabIndex = 72;
				smallColorControl[ 12 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel14
				smallColorControl[ 13 ].Location = new Point( 100, 61 );
				smallColorControl[ 13 ].TabIndex = 71;
				smallColorControl[ 13 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel15
				smallColorControl[ 14 ].Location = new Point( 175, 61 );
				smallColorControl[ 14 ].TabIndex = 74;
				smallColorControl[ 14 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel16
				smallColorControl[ 15 ].Location = new Point( 50, 38 );
				smallColorControl[ 15 ].TabIndex = 61;
				smallColorControl[ 15 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel17
				smallColorControl[ 16 ].Location = new Point( 75, 38 );
				smallColorControl[ 16 ].TabIndex = 62;
				smallColorControl[ 16 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel18
				smallColorControl[ 17 ].Location = new Point( 0, 38 );
				smallColorControl[ 17 ].TabIndex = 59;
				smallColorControl[ 17 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel19
				smallColorControl[ 18 ].Location = new Point( 25, 84 );
				smallColorControl[ 18 ].TabIndex = 75;
				smallColorControl[ 18 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel20
				smallColorControl[ 19 ].Location = new Point( 175, 15 );
				smallColorControl[ 19 ].TabIndex = 58;
				smallColorControl[ 19 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel21
				smallColorControl[ 20 ].Location = new Point( 175, 38 );
				smallColorControl[ 20 ].TabIndex = 66;
				smallColorControl[ 20 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel22
				smallColorControl[ 21 ].Location = new Point( 100, 38 );
				smallColorControl[ 21 ].TabIndex = 63;
				smallColorControl[ 21 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel23
				smallColorControl[ 22 ].Location = new Point( 50, 15 );
				smallColorControl[ 22 ].TabIndex = 53;
				smallColorControl[ 22 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel24
				smallColorControl[ 23 ].Location = new Point( 100, 15 );
				smallColorControl[ 23 ].TabIndex = 55;
				smallColorControl[ 23 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel25
				smallColorControl[ 24 ].Location = new Point( 125, 15 );
				smallColorControl[ 24 ].TabIndex = 56;
				smallColorControl[ 24 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel26
				smallColorControl[ 25 ].Location = new Point( 25, 107 );
				smallColorControl[ 25 ].TabIndex = 83;
				smallColorControl[ 25 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel27
				smallColorControl[ 26 ].Location = new Point( 0, 107 );
				smallColorControl[ 26 ].TabIndex = 82;
				smallColorControl[ 26 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel28
				smallColorControl[ 27 ].Location = new Point( 0, 84 );
				smallColorControl[ 27 ].TabIndex = 81;
				smallColorControl[ 27 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel29
				smallColorControl[ 28 ].Location = new Point( 150, 84 );
				smallColorControl[ 28 ].TabIndex = 80;
				smallColorControl[ 28 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel30
				smallColorControl[ 29 ].Location = new Point( 125, 84 );
				smallColorControl[ 29 ].TabIndex = 79;
				smallColorControl[ 29 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel31
				smallColorControl[ 30 ].Location  = new Point( 100, 84 );
				smallColorControl[ 30 ].TabIndex = 78;
				smallColorControl[ 30 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel32
				smallColorControl[ 31 ].Location = new Point( 75, 84 );
				smallColorControl[ 31 ].TabIndex = 77;
				smallColorControl[ 31 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel33
				smallColorControl[ 32 ].Location = new Point( 50, 84 );
				smallColorControl[ 32 ].TabIndex = 76;
				smallColorControl[ 32 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel34
				smallColorControl[ 33 ].Location = new Point( 25, 38 );
				smallColorControl[ 33 ].TabIndex = 60;
				smallColorControl[ 33 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel35
				smallColorControl[ 34 ].Location = new Point( 150, 38 );
				smallColorControl[ 34 ].TabIndex = 65;
				smallColorControl[ 34 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel36
				smallColorControl[ 35 ].Location = new Point( 125, 38 );
				smallColorControl[ 35 ].TabIndex = 64;
				smallColorControl[ 35 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel37
				smallColorControl[ 36 ].Location = new Point( 150, 61 );
				smallColorControl[ 36 ].TabIndex = 73;
				smallColorControl[ 36 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel38
				smallColorControl[ 37 ].Location = new Point( 150, 15 );
				smallColorControl[ 37 ].TabIndex = 57;
				smallColorControl[ 37 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel39
				smallColorControl[ 38 ].Location = new Point( 75, 15 );
				smallColorControl[ 38 ].TabIndex = 54;
				smallColorControl[ 38 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel40
				smallColorControl[ 39 ].Location = new Point( 25, 15 );
				smallColorControl[ 39 ].TabIndex = 52;
				smallColorControl[ 39 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel41
				smallColorControl[ 40 ].Location = new Point( 50, 107 );
				smallColorControl[ 40 ].TabIndex = 84;
				smallColorControl[ 40 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel42
				smallColorControl[ 41 ].Location = new Point( 75, 107 );
				smallColorControl[ 41 ].TabIndex = 85;
				smallColorControl[ 41 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel43
				smallColorControl[ 42 ].Location = new Point( 100, 107 );
				smallColorControl[ 42 ].TabIndex = 86;
				smallColorControl[ 42 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel44
				smallColorControl[ 43 ].Location = new Point( 125, 107 );
				smallColorControl[ 43 ].TabIndex = 87;
				smallColorControl[ 43 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel45
				smallColorControl[ 44 ].Location = new Point( 150, 107 );
				smallColorControl[ 44 ].TabIndex = 88;
				smallColorControl[ 44 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel46
				smallColorControl[ 45 ].Location = new Point( 175, 107 );
				smallColorControl[ 45 ].TabIndex = 89;
				smallColorControl[ 45 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel47
				smallColorControl[ 46 ].Location = new Point( 0, 130 );
				smallColorControl[ 46 ].TabIndex = 90;
				smallColorControl[ 46 ].Click += new EventHandler( OnSmallColorControlClick );
				// colorPanel48
				smallColorControl[ 47 ].Location = new Point( 25, 130 );
				smallColorControl[ 47 ].TabIndex = 91;
				smallColorControl[ 47 ].Click += new EventHandler( OnSmallColorControlClick );
				
				// userColorPane1
				userSmallColorControl[ 0 ].Location = new Point( 0, 180 );
				userSmallColorControl[ 0 ].TabIndex = 99;
				userSmallColorControl[ 0 ].Click += new EventHandler( OnSmallColorControlClick );
				// userColorPanel2
				userSmallColorControl[ 1 ].Location = new Point( 0, 203 );
				userSmallColorControl[ 1 ].TabIndex = 108;
				userSmallColorControl[ 1 ].Click += new EventHandler( OnSmallColorControlClick );
				// userColorPanel13
				userSmallColorControl[ 2 ].Location = new Point( 25, 180 );
				userSmallColorControl[ 2 ].TabIndex = 100;
				userSmallColorControl[ 2 ].Click += new EventHandler( OnSmallColorControlClick );
				// userColorPanel4
				userSmallColorControl[ 3 ].Location = new Point( 25, 203 );
				userSmallColorControl[ 3 ].TabIndex = 109;
				userSmallColorControl[ 3 ].Click += new EventHandler( OnSmallColorControlClick );
				// userColorPanel5
				userSmallColorControl[ 4 ].Location = new Point( 50, 180 );
				userSmallColorControl[ 4 ].TabIndex = 101;
				userSmallColorControl[ 4 ].Click += new EventHandler( OnSmallColorControlClick );
				// userColorPanel6
				userSmallColorControl[ 5 ].Location = new Point( 50, 203 );
				userSmallColorControl[ 5 ].TabIndex = 110;
				userSmallColorControl[ 5 ].Click += new EventHandler( OnSmallColorControlClick );
				// userColorPanel7
				userSmallColorControl[ 6 ].Location = new Point( 75, 180 );
				userSmallColorControl[ 6 ].TabIndex = 102;
				userSmallColorControl[ 6 ].Click += new EventHandler( OnSmallColorControlClick );
				// userColorPanel8
				userSmallColorControl[ 7 ].Location = new Point( 75, 203 );
				userSmallColorControl[ 7 ].TabIndex = 111;
				userSmallColorControl[ 7 ].Click += new EventHandler( OnSmallColorControlClick );
				// userColorPanel9
				userSmallColorControl[ 8 ].Location = new Point( 100, 180 );
				userSmallColorControl[ 8 ].TabIndex = 103;
				userSmallColorControl[ 8 ].Click += new EventHandler( OnSmallColorControlClick );
				// userColorPanel10
				userSmallColorControl[ 9 ].Location = new Point( 100, 203 );
				userSmallColorControl[ 9 ].TabIndex = 112;
				userSmallColorControl[ 9 ].Click += new EventHandler( OnSmallColorControlClick );
				// userColorPanel11
				userSmallColorControl[ 10 ].Location = new Point( 125, 180 );
				userSmallColorControl[ 10 ].TabIndex = 105;
				userSmallColorControl[ 10 ].Click += new EventHandler( OnSmallColorControlClick );
				// userColorPanel2
				userSmallColorControl[ 11 ].Location = new Point( 125, 203 );
				userSmallColorControl[ 11 ].TabIndex = 113;
				userSmallColorControl[ 11 ].Click += new EventHandler( OnSmallColorControlClick );
				// userColorPanel13
				userSmallColorControl[ 12 ].Location = new Point( 150, 180 );
				userSmallColorControl[ 12 ].TabIndex = 106;
				userSmallColorControl[ 12 ].Click += new EventHandler( OnSmallColorControlClick );
				// userColorPanel4
				userSmallColorControl[ 13 ].Location = new Point( 150, 203 );
				userSmallColorControl[ 13 ].TabIndex = 114;
				userSmallColorControl[ 13 ].Click += new EventHandler( OnSmallColorControlClick );
				// userColorPanel15
				userSmallColorControl[ 14 ].Location = new Point( 175, 180 );
				userSmallColorControl[ 14 ].TabIndex = 107;
				userSmallColorControl[ 14 ].Click += new EventHandler( OnSmallColorControlClick );
				// userColorPanel16
				userSmallColorControl[ 15 ].Location = new Point( 175, 203 );
				userSmallColorControl[ 15 ].TabIndex = 115;
				userSmallColorControl[ 15 ].Click += new EventHandler( OnSmallColorControlClick );
				
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
				
				Controls.Add( userSmallColorControl[ 7 ] );
				Controls.Add( userSmallColorControl[ 6 ] );
				Controls.Add( userSmallColorControl[ 5 ] );
				Controls.Add( userSmallColorControl[ 4 ] );
				Controls.Add( userSmallColorControl[ 3 ] );
				Controls.Add( userSmallColorControl[ 2 ] );
				Controls.Add( userSmallColorControl[ 1 ] );
				Controls.Add( userSmallColorControl[ 0 ] );
				Controls.Add( userSmallColorControl[ 15 ] );
				Controls.Add( userSmallColorControl[ 14 ] );
				Controls.Add( userSmallColorControl[ 13 ] );
				Controls.Add( userSmallColorControl[ 12 ] );
				Controls.Add( userSmallColorControl[ 11 ] );
				Controls.Add( userSmallColorControl[ 10 ] );
				Controls.Add( userSmallColorControl[ 9 ] );
				Controls.Add( userSmallColorControl[ 8 ] );
				
				Controls.Add( smallColorControl[ 0 ] );
				Controls.Add( smallColorControl[ 3 ] );
				Controls.Add( smallColorControl[ 6 ] );
				Controls.Add( smallColorControl[ 7 ] );
				Controls.Add( smallColorControl[ 4 ] );
				Controls.Add( smallColorControl[ 5 ] );
				Controls.Add( smallColorControl[ 2 ] );
				Controls.Add( smallColorControl[ 1 ] );
				Controls.Add( smallColorControl[ 47 ] );
				Controls.Add( smallColorControl[ 46 ] );
				Controls.Add( smallColorControl[ 45 ] );
				Controls.Add( smallColorControl[ 44 ] );
				Controls.Add( smallColorControl[ 43 ] );
				Controls.Add( smallColorControl[ 42 ] );
				Controls.Add( smallColorControl[ 41 ] );
				Controls.Add( smallColorControl[ 40 ] );
				Controls.Add( smallColorControl[ 25 ] );
				Controls.Add( smallColorControl[ 26 ] );
				Controls.Add( smallColorControl[ 27 ] );
				Controls.Add( smallColorControl[ 28 ] );
				Controls.Add( smallColorControl[ 29 ] );
				Controls.Add( smallColorControl[ 30 ] );
				Controls.Add( smallColorControl[ 31 ] );
				Controls.Add( smallColorControl[ 32 ] );
				Controls.Add( smallColorControl[ 18 ] );
				Controls.Add( smallColorControl[ 14 ] );
				Controls.Add( smallColorControl[ 36 ] );
				Controls.Add( smallColorControl[ 12 ] );
				Controls.Add( smallColorControl[ 13 ] );
				Controls.Add( smallColorControl[ 10 ] );
				Controls.Add( smallColorControl[ 11 ] );
				Controls.Add( smallColorControl[ 8 ] );
				Controls.Add( smallColorControl[ 9 ] );
				Controls.Add( smallColorControl[ 20 ] );
				Controls.Add( smallColorControl[ 34 ] );
				Controls.Add( smallColorControl[ 35 ] );
				Controls.Add( smallColorControl[ 21 ] );
				Controls.Add( smallColorControl[ 16 ] );
				Controls.Add( smallColorControl[ 15 ] );
				Controls.Add( smallColorControl[ 33 ] );
				Controls.Add( smallColorControl[ 17 ] );
				Controls.Add( smallColorControl[ 19 ] );
				Controls.Add( smallColorControl[ 37 ] );
				Controls.Add( smallColorControl[ 24 ] );
				Controls.Add( smallColorControl[ 23 ] );
				Controls.Add( smallColorControl[ 38 ] );
				Controls.Add( smallColorControl[ 22 ] );
				Controls.Add( smallColorControl[ 39 ] );
				
				Controls.Add( userColorLabel );
				Controls.Add( baseColorLabel );
				
				Size = new Size( 212, 238 );
				ResumeLayout( false );
				
				selectedSmallColorControl = smallColorControl[ 46 ];  // default, Black
				selectedSmallColorControl.IsSelected = true;
				
				CheckIfColorIsInPanel( );
				
				panelSelected = false;
			}
			
			private void CheckIfColorIsInPanel( )
			{
				if ( colorDialogPanel.ColorDialog.Color != Color.Black )
				{
					// check if we have a panel with a BackColor = ColorDialog.Color...
					for ( int i = 0; i < smallColorControl.Length; i++ )
					{
						if ( smallColorControl[ i ].BackColor == colorDialogPanel.ColorDialog.Color )
						{
							selectedSmallColorControl = smallColorControl[ i ];
							break;
						}
					}
				}
			}
			
			void OnSmallColorControlClick( object sender, EventArgs e )
			{
				panelSelected = true;
				
				// previous selected smallcolorcontrol
				if ( selectedSmallColorControl != (SmallColorControl)sender )
					selectedSmallColorControl.IsSelected = false;
				
				selectedSmallColorControl = (SmallColorControl)sender;
				
				TriangleControl.CurrentBrightness = HSB.Brightness( selectedSmallColorControl.Color );
				
				colorDialogPanel.UpdateControls( selectedSmallColorControl.Color );
				colorDialogPanel.UpdateRGBTextBoxes( selectedSmallColorControl.Color );
				colorDialogPanel.UpdateHSBTextBoxes( selectedSmallColorControl.Color );
			}
			
			public Color ColorToShow
			{
				get
				{
					return selectedSmallColorControl.Color;
				}
			}
			
			public void SetUserColor( Color col )
			{
				userSmallColorControl[ currentlyUsedUserSmallColorControl ].Color = col;
				
				// check if this.customColors already exists
				if ( customColors == null )
				{
					customColors = new int[ 16 ];
					int white = Color.White.ToArgb( );
					
					for ( int i = 0; i < customColors.Length; i++ )
						customColors[ i ] = white;
				}
				
				customColors[ currentlyUsedUserSmallColorControl ] = col.ToArgb( );
				
				// update ColorDialog dialog property
				colorDialogPanel.ColorDialog.CustomColors = customColors;
				
				currentlyUsedUserSmallColorControl++;
				if ( currentlyUsedUserSmallColorControl > 15 )
					currentlyUsedUserSmallColorControl = 0;
			}
			
			public void SetCustomColors( )
			{
				int[] customColors = colorDialogPanel.ColorDialog.CustomColors;
				
				for ( int i = 0; i < customColors.Length; i++ )
				{
					userSmallColorControl[ i ].Color = Color.FromArgb( customColors[ i ] );
				}
			}
		}
		
		internal class ColorMatrixControl : Panel
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
					set
					{
						bitmap = value;
					}
					
					get
					{
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
					Pen pen = new Pen( ThemeEngine.Current.ResPool.GetSolidBrush( cursorColor ), 3 );
					
					Graphics graphics = Graphics.FromImage( bitmap );
					
					graphics.DrawLine( pen, 11, 0, 11, 7 );
					graphics.DrawLine( pen, 11, 14, 11, 21 );
					graphics.DrawLine( pen, 0, 11, 7, 11 );
					graphics.DrawLine( pen, 14, 11, 21, 11 );
				}
				
				public Bitmap Bitmap
				{
					set
					{
						bitmap = value;
					}
					
					get
					{
						return bitmap;
					}
				}
				
				public Color CursorColor
				{
					set
					{
						cursorColor = value;
					}
					
					get
					{
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
				set
				{
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
					set
					{
						bitmap = value;
					}
					
					get
					{
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
				set
				{
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
				set
				{
					currentBrightness = value;
				}
				
				get
				{
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
				
				e.Graphics.FillPolygon( ThemeEngine.Current.ResPool.GetSolidBrush( Color.Black ), trianglePoints );
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
				get
				{
					float tmp = (float)( currentTrianglePosition - 9 );
					tmp = tmp * briStep;
					
					int retval = 239 - (int)tmp;
					
					TriangleControl.CurrentBrightness = retval;
					
					return retval;
				}
				
				set
				{
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
				set
				{
					TrianglePosition = HSB.Brightness( value );
				}
			}
		}
		#endregion
	}
}
