//
// Test application for the SaveFileDialogText class implementation
//
// Author:
//   Jordi Mas i Hernàndez, jmas@softcatala.org
//
using System;
using System.IO;
using System.Windows.Forms;

class SaveFileDialogText : Form 
{
	
	public SaveFileDialogText  () : base (){
		
	}
	
	static public int Main (String[] args){
		
		Stream outStream;
		SaveFileDialog saveDlg = new SaveFileDialog();
		
		saveDlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"  ;
		saveDlg.FilterIndex =2;
		saveDlg.RestoreDirectory =true;
		
		Console.WriteLine ("Default class values------" +  saveDlg.CreatePrompt);
		Console.WriteLine ("CreatePrompt->" +  saveDlg.CreatePrompt);
		Console.WriteLine ("OverwritePrompt->" +  saveDlg.OverwritePrompt);		
		
		if(saveDlg.ShowDialog() == DialogResult.OK)	{
			
		 if((outStream = saveDlg.OpenFile()) != null) {
         		
         	 Console.WriteLine ("Selected File->" +  saveDlg.FileName);		
		 }
		}
		return 0;
	}
}
