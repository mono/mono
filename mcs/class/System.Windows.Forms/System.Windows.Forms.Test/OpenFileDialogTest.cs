//
// Test application for the OpenFileDialogText class implementation
//
// Author:
//   Jordi Mas i Hernàndez, jmas@softcatala.org
//
using System;
using System.IO;
using System.Windows.Forms;

class OpenFileDialogText : Form 
{
	
	public OpenFileDialogText  () : base (){
		
	}
	
	static public int Main (String[] args){
				
		FileDialog fileDlg = new OpenFileDialog();
		
		fileDlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"  ;
		fileDlg.FilterIndex =2;
		fileDlg.RestoreDirectory =true;
		
		Console.WriteLine ("Default class values------");
		
		Console.WriteLine ("fileName->" +  fileDlg.FileName);
		Console.WriteLine ("checkFileExists->" +  fileDlg.CheckFileExists);
		Console.WriteLine ("defaultExt->" +  fileDlg.DefaultExt);
		Console.WriteLine ("filterIndex->" +  fileDlg.FilterIndex);
		Console.WriteLine ("filter->" +  fileDlg.Filter);
		Console.WriteLine ("checkPathExists->" +  fileDlg.CheckPathExists);
		Console.WriteLine ("dereferenceLinks->" +  fileDlg.DereferenceLinks);
		Console.WriteLine ("showHelp->" +  fileDlg.ShowHelp);
		Console.WriteLine ("title->" +  fileDlg.Title);
		Console.WriteLine ("initialDirectory->" +  fileDlg.InitialDirectory);
		Console.WriteLine ("restoreDirectory->" +  fileDlg.RestoreDirectory);
		Console.WriteLine ("validateNames->" + fileDlg.ValidateNames);
		
			
		if(fileDlg.ShowDialog() == DialogResult.OK)	{      		
         	Console.WriteLine ("Selected File->" +  fileDlg.FileName);		
		}
		
		return 0;
	}
}
