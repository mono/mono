using System.Diagnostics;
using System.Reflection;

[assembly: AssemblyProduct ("Product")]
[assembly: AssemblyCompany ("Company")]
[assembly: AssemblyDescription ("Description")]
[assembly: AssemblyCopyright ("Copyright")]
[assembly: AssemblyTrademark ("Trademark")]
[assembly: AssemblyVersion ("5.4.3.1")]
[assembly: AssemblyFileVersion ("8.9")]

class C
{
	public static int Main ()
	{
		var loc = Assembly.GetExecutingAssembly ().Location;
		var fv = FileVersionInfo.GetVersionInfo (loc);

		if (fv.ProductName != "Product")
			return 1;

		if (fv.CompanyName != "Company")
			return 2;

//		if (fv.Comments != "Description")
//			return 3;

		if (fv.LegalCopyright != "Copyright")
			return 4;

		if (fv.LegalTrademarks != "Trademark")
			return 5;

		if (fv.ProductVersion != "8.9")
			return 6;

		return 0;
	}
}