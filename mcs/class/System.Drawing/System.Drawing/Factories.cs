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

internal class Factories {
	internal static string DefaultImplementationNamespace = "Win32Impl";
	private static string FactoryClassNameBase = "System.Drawing.Win32Impl.";
	private static Assembly FactoryAssembly;

	static object CreateObjectOfType( string type) {
		object obj = FactoryAssembly.CreateInstance(FactoryClassNameBase + type);
		if( obj == null) {
			System.Console.WriteLine("Factory {0} is not implemented", FactoryClassNameBase + type );
			throw new NotImplementedException();
		}
		return obj;
	}

	internal static IBitmapFactory GetBitmapFactory () {
		return CreateObjectOfType("BitmapFactory") as IBitmapFactory;
	}

	internal static IFontFactory GetFontFactory () {
		return CreateObjectOfType("FontFactory") as IFontFactory;
	}

	internal static IGraphicsFactory GetGraphicsFactory () {
		return CreateObjectOfType("GraphicsFactory") as IGraphicsFactory;
	}

	internal static IPenFactory GetPenFactory () {
		return CreateObjectOfType("PenFactory") as IPenFactory;
	}

	internal static ISolidBrushFactory GetSolidBrushFactory () {
		return CreateObjectOfType("SolidBrushFactory") as ISolidBrushFactory;
	}

	static Factories ()
	{
		FactoryAssembly = Assembly.GetExecutingAssembly();
		string implNamespace = Environment.GetEnvironmentVariable ("SystemDrawingImpl");
		if (implNamespace == null){
			implNamespace = Factories.DefaultImplementationNamespace;
			NameValueCollection sysdrawconfig = (NameValueCollection) ConfigurationSettings.GetConfig("system.drawing");
			if (sysdrawconfig != null) {
				if (sysdrawconfig["Implementation"] != null) {
					implNamespace = sysdrawconfig["Implementation"];
				}
			}
			else {
				Console.WriteLine("NameValueCollection is null");
			}
		}
		FactoryClassNameBase = "System.Drawing." + implNamespace + ".";
	}
}

