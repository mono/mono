//
// Enumerates printers installed in the system
//

using System;
using System.Drawing.Printing;

public class EnumPrinters
{
	public static void Main (string[] args)
	{
		PrinterSettings.StringCollection col = System.Drawing.Printing.PrinterSettings.InstalledPrinters;
		
		for (int i = 0; i < col.Count; i++) {
			
			Console.WriteLine ("--- {0}", col[i]);
			PrinterSettings ps = new PrinterSettings ();
			ps.PrinterName = col[i];
			//Console.WriteLine ("    Duplex: {0}", ps.Duplex);
			Console.WriteLine ("    FromPage: {0}", ps.FromPage);
			Console.WriteLine ("    ToPage: {0}", ps.ToPage);
			Console.WriteLine ("    MaximumCopies: {0}", ps.MaximumCopies);
			Console.WriteLine ("    IsDefaultPrinter: {0}", ps.IsDefaultPrinter);
			Console.WriteLine ("    SupportsColor: {0}", ps.SupportsColor);
			Console.WriteLine ("    MaximumPage {0}", ps.MaximumPage);
			Console.WriteLine ("    MinimumPage {0}", ps.MinimumPage);
			Console.WriteLine ("    LandscapeAngle {0}", ps.LandscapeAngle);
			/*				
			for (int p = 0; p < ps.PrinterResolutions.Count; p++) {
				Console.WriteLine ("        PrinterResolutions {0}", ps.PrinterResolutions [p]);
			}*/		
			
			for (int p = 0; p < ps.PaperSizes.Count; p++) {
				Console.WriteLine ("        PaperSize Name [{0}] Kind [{1}] Width {2} Height {3}", 
					ps.PaperSizes [p].PaperName, ps.PaperSizes [p].Kind,
					ps.PaperSizes [p].Width, ps.PaperSizes [p].Height);
			}
		}
	}

}

