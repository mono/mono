//
// System.Windows.Forms.SaveFileDialog.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
//  Implemented by Jordi Mas i Hernàndez (jmas@softcatala.org)
//
// (C) 2002 Ximian, Inc
//
using System.IO;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public sealed class SaveFileDialog : FileDialog {    	    	
    	
    	private bool createPrompt = false;				
		private bool overwritePrompt = true;

		//
		//  --- Constructor
		//		
		public SaveFileDialog()		{			
			Reset();
		}

		//
		//  --- Public Properties
		//		
		public bool CreatePrompt {
			get {return createPrompt;}			
			set {createPrompt=value;}
		}
		
		public bool OverwritePrompt {
			get {return overwritePrompt;}			
			set {overwritePrompt=value;}
		}

		//
		//  --- Public Methods
		//		
		public Stream OpenFile(){						
			return new FileStream(FileName, FileMode.OpenOrCreate);
		}
		
		internal protected override void initOpenFileName ( ref OPENFILENAME opf )
		{
			base.initOpenFileName ( ref opf );

			if (createPrompt)
				opf.Flags |= (int) (OpenFileDlgFlags.OFN_CREATEPROMPT);
			
			if (overwritePrompt)
				opf.Flags |= (int) (OpenFileDlgFlags.OFN_OVERWRITEPROMPT);
		}
		
		public override void Reset(){
			
			isSave = true;			
			createPrompt = false;				
			overwritePrompt = true;
		}
	 }
}
