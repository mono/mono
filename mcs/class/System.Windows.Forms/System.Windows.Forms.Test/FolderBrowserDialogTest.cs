//
// Test application for the FolderBrowserDialog.cs class implementation
//
// Author:
//  (c) 2003 Jordi Mas i Hernàndez, jmas@softcatala.org
//

using System;
using System.Windows.Forms;
using System.IO;

public class FolderBrowserDialogTest : System.Windows.Forms.Form
{        
    
    // The main entry point for the application.
    static void Main() 
    {
        new FolderBrowserDialogTest();
    }


    // Constructor.
    public FolderBrowserDialogTest()
    {  	    	
    	FolderBrowserDialog folderBrowserDlg = new FolderBrowserDialog();       			
    			
    			
		// Show default class values			
		Console.WriteLine("Default values---");								  				
	  	Console.WriteLine("ShowNewFolderButton " +  folderBrowserDlg.ShowNewFolderButton);							
	  	Console.WriteLine("RootFolder " +  folderBrowserDlg.RootFolder);							   	
	  	
	  	folderBrowserDlg.Description = "Select a file";
		
		DialogResult result = folderBrowserDlg.ShowDialog();
        if( result == DialogResult.OK )        
            Console.WriteLine("Selected folder: " + folderBrowserDlg.SelectedPath);
                
    }    

}