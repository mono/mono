//
// Test application for the PrinttDialogTest class implementation
//
// Author:
//   Jordi Mas i Hernàndez, jmas@softcatala.org
//

using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Printing;


class PrintDialogTest : Form	{
				
	public PrintDialogTest(): base() {
				
    }
        
    public static void Main(string[] args){
    	
    	PrintDialog printDlg = new PrintDialog();
    	
    	Console.WriteLine ("Default class values------");		
		Console.WriteLine ("AllowPrintToFile->" +  printDlg.AllowPrintToFile);
		Console.WriteLine ("AllowSelection->" +  printDlg.AllowSelection);
		Console.WriteLine ("AllowSomePages->" +  printDlg.AllowSomePages);
		Console.WriteLine ("ShowHelp->" +  printDlg.ShowHelp);
		Console.WriteLine ("ShowNetwork->" +  printDlg.ShowNetwork);
		Console.WriteLine ("PrintToFile->" +  printDlg.PrintToFile);
						
        // Declare the PrintDocument object.
    	PrintDocument docToPrint = 	new PrintDocument();	               	
        
        printDlg.Document = docToPrint;
        DialogResult result = printDlg.ShowDialog();

        // If the result is OK then print the document.
        if (result==DialogResult.OK){
			//docToPrint.Print();
			Console.WriteLine ("Copies->" +  printDlg.PrinterSettings.Copies);
        }
		
	}

}

