//
// System.Windows.Forms.FontDialog.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@raytek.com)
//   Implemented by Jordi Mas i Hernàndez (jmas@softcatala.org)
//
// (C) 2002-2003 Ximian, Inc
//
using System.Drawing;
using System.Runtime.InteropServices;
namespace System.Windows.Forms 
{	
	public class FontDialog : CommonDialog{        	        	
	        private bool	bAllowScriptChange;
	        private bool	bAllowSimulations;							
		  	private bool	bAllowVectorFonts;							
		  	private bool	bAllowVerticalFonts;							
		  	private Color	color = Color.Black;							
		  	private bool	bFixedPitchOnly;							
		  	private Font	font;							
		  	private bool	bFontMustExist;							
		  	private int		nMaxSize;							
		  	private int		nMinSize;								  	
		  	private bool	bScriptsOnly;							
		  	private bool	bShowApply;								  	
		  	private bool	bShowColor;							
		  	private bool	bShowEffects;							
		  	private bool	bShowHelp;

			protected static readonly object EventApply;
	
			//
			//  --- Constructor
			//		
			public FontDialog()	{
				defaultValues();
			}
			
			internal void defaultValues(){		
				font = new Font("Microsoft Sans Serif", 8);
				bAllowScriptChange = true;
	        	bAllowSimulations  = true;							
		  		bAllowVectorFonts  = true;							
		  		bAllowVerticalFonts  = true;							
		  		color = Color.Black;							
		  		bFixedPitchOnly = false;								  		
		  		bFontMustExist = false;
		  		nMaxSize = 0 ;							
		  		nMinSize = 0;								  	
		  		bScriptsOnly = false;							
		  		bShowApply = true;								  	
		  		bShowColor = false;							
		  		bShowEffects = true;							
		  		bShowHelp = false;				
			}
	
			//
			//  --- Public Properties
			//		
			public bool AllowScriptChange {
				get { return bAllowScriptChange;  }
				set { bAllowScriptChange = value; }
			}
			
			public bool AllowSimulations {
				get { return bAllowSimulations;  }
				set { bAllowSimulations = value; }
			}
			
			public bool AllowVectorFonts {
				get { return bAllowVectorFonts;  }
				set { bAllowVectorFonts = value; }
			}
			
			public bool AllowVerticalFonts{
				get { return bAllowVerticalFonts;  }
				set { bAllowVerticalFonts = value; }
			}
			
			public Color Color {
				get { return color;  }
				set { color = value; }
			}			
			
			public bool FixedPitchOnly {
				get { return bFixedPitchOnly;  }
				set { bFixedPitchOnly = value; }
			}
			
			public Font Font {
				get { return font;  }
				set { font = value; }
			}
			
			public bool FontMustExist {
				get { return bFontMustExist;  }
				set { bFontMustExist = value; }
			}
			
			public int MaxSize{
				get { return nMaxSize;  }
				set { nMaxSize = value; }
			}
			
			public int MinSize {
				get { return nMinSize;  }
				set { nMinSize = value; }
			}
			
			public bool ScriptsOnly {
				get { return bScriptsOnly;  }
				set { bScriptsOnly = value; }
			}
			
			public bool ShowApply {
				get { return bShowApply;  }
				set { bShowApply = value; }
			}
			
			public bool ShowColor {
				get { return bShowColor;  }
				set { bShowColor = value; }
			}
			
			public bool ShowEffects{
				get { return bShowEffects;  }
				set { bShowEffects = value; }
			}
			
			public bool ShowHelp {
				get { return bShowHelp;  }
				set { bShowHelp = value; }
			}
	
			
			//  --- Public Methods			
			public override void Reset(){				
				defaultValues();
			}

			public override string ToString(){				
				return base.ToString();
			}
	
			protected virtual void OnApply(EventArgs e){
				if (Apply != null)
					Apply (this, e);
			}
			//
			//  --- Public Events
			//
			[MonoTODO]
			public event EventHandler Apply;
	

			[MonoTODO]
			protected override bool RunDialog(IntPtr hWndOwner){	
				Dialog.Run();			
				return false;
			}
			[MonoTODO]
			internal override Gtk.Dialog CreateDialog(){
				return new Gtk.FontSelectionDialog("");
			}
		 }
		 
		 
}
