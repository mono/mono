//
// System.Drawing.Bitmap.cs
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//
// Authors:
// 	Alexandre Pigolkine ( pigolkine@gmx.de)
//

using System;
using System.Drawing;
using System.Configuration;
using System.Collections.Specialized;
using System.Reflection;
using System.Drawing.Text;
using System.Threading;

namespace System.Drawing {
internal class Factories {
	internal static string DefaultImplementationNamespace = "Cairo";
	private static string FactoryClassNameBase = "System.Drawing.Win32Impl.";
	private static Assembly FactoryAssembly;

	static object CreateObjectOfType (string type)
	{
		object obj = FactoryAssembly.CreateInstance(FactoryClassNameBase + type);
		if( obj == null) {
			string msg = String.Format ("Factory {0}{1} is not implemented", FactoryClassNameBase, type);
			System.Console.WriteLine(msg);
			throw new NotImplementedException(msg);
		}
		return obj;
	}

	internal static IBitmapFactory GetBitmapFactory ()
	{
		return CreateObjectOfType("BitmapFactory") as IBitmapFactory;
	}

	internal static IFontFactory GetFontFactory ()
	{
		return CreateObjectOfType("FontFactory") as IFontFactory;
	}

	internal static IGraphicsFactory GetGraphicsFactory ()
	{
		return CreateObjectOfType("GraphicsFactory") as IGraphicsFactory;
	}

	internal static IPenFactory GetPenFactory ()
	{
		return CreateObjectOfType("PenFactory") as IPenFactory;
	}

	internal static ISolidBrushFactory GetSolidBrushFactory ()
	{
		return CreateObjectOfType("SolidBrushFactory") as ISolidBrushFactory;
	}
	
	internal static IFontCollectionFactory GetFontCollectionFactory()
	{
		return CreateObjectOfType("FontCollectionFactory") as IFontCollectionFactory;
	}

	internal static IFontFamilyFactory GetFontFamilyFactory()
	{
		return CreateObjectOfType("FontFamilyFactory") as IFontFamilyFactory;
	}

	//
	// Initializes the implementation that will be used to provide
	// System.Drawing facilities.  Currently Cairo and Wine
	// implementations are available.
	//
	// If Windows.Forms has been initialized, the Domain
	// information will have the key Mono.Running.Windows.Forms
	// set to true and we will make this the default.  Otherwise
	// the Cairo-based implementation is picked.
	// 
	// A factory can be forced by setting the "SystemDrawingImpl"
	// environment variable to either Win32Impl or Cairo.
	//
	// This can also be controlled by the <system.drawing> entry
	// in the application configuration directory.
	//
	static Factories ()
	{
		FactoryAssembly = Assembly.GetExecutingAssembly();
		string implNamespace = null;

		if (Thread.GetDomain ().GetData ("Mono.Running.Windows.Forms") != null)
			implNamespace = "Win32Impl";

		string s = Environment.GetEnvironmentVariable ("SystemDrawingImpl");
		if (s != null)
			implNamespace = s;
		
		if (implNamespace == null){
			implNamespace = Factories.DefaultImplementationNamespace;
			NameValueCollection sysdrawconfig = (NameValueCollection) ConfigurationSettings.GetConfig("system.drawing");
			if (sysdrawconfig != null) {
				if (sysdrawconfig["Implementation"] != null) {
					implNamespace = sysdrawconfig["Implementation"];
				}
				else {
					Console.WriteLine("sysdrawconfig[\"Implementation\"] is null");
				}
			}
		}
		FactoryClassNameBase = "System.Drawing." + implNamespace + ".";
	}
}
}
