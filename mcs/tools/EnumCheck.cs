/**
 * Namespace: System.Web
 * Class:     EnumCheck
 *
 * Author:  Gaurav Vaish
 * Contact: <gvaish@iitk.ac.in>
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Reflection;

namespace Mono.Enumerations
{
	public class EnumCheck
	{
		private string className;
		private Type   type;

		public static string basePath = @"C:\WINNT\Microsoft.NET\Framework\v1.0.3705";
		//public static string basePath = "file:///C:/WINNT/Microsoft.NET/Framework/v1.0.3705";
		private static readonly string[] assemblies = {
			"mscorlib",
			"System.Web",
			"System",
			"System.Drawing",
			"System.Security",
			"System.Windows.Forms",
			"System.XML",
			"System.Data",
			"System.Design",
			"System.Enterpriseservices",
			"System.Management",
			"System.Messaging",
			"System.Runtime.Remoting",
			"System.ServiceProcess",
			"System.Web.RegularExpressions",
			"System.Web.Services"
		};

		public EnumCheck(string className)
		{
			this.className = className;
		}

		public void Display()
		{
			LoadType();
			if(type == null || !type.IsEnum)
			{
				System.Console.Write("-->Failed to load the enumeration: " + className);
				return;
			}
			Array ar = Enum.GetValues(type);
			System.Console.WriteLine("-->Enumeration: {0}", type.ToString());
			for(int i=0; i < ar.Length; i++)
			{
				Enum b = (Enum)ar.GetValue(i);
				System.Console.Write(" {0}", Enum.Format(type, b, "G"));
				System.Console.WriteLine(" ({0}) ", Enum.Format(type, b, "D"));
			}
		}

		private void LoadType()
		{
			type = null;
			foreach(string assemblyName in assemblies)
			{
				try
				{
					Assembly assembly;
					/*if(assemblyName == "mscorlib")
					{
						assembly = Assembly.LoadFrom(assemblyName);
					} else
					{*/
						assembly = Assembly.LoadFrom(basePath + "\\" + assemblyName + ".dll");
					//}
					System.Console.WriteLine("\tSearching in: {0}", assembly.ToString());
					foreach(Type t in assembly.GetTypes())
					{
						if(!t.IsEnum)
							continue;
						string name = null;
						if(Type.GetType(className + "," + assemblyName) != null)
							name = Type.GetType(className + "," + assemblyName).ToString();
						if(name == t.ToString())
						{
							type = t;
							break;
						}
					}
				} catch(BadImageFormatException)
				{
				} catch(ReflectionTypeLoadException)
				{
				} catch(ArgumentException)
				{
				}
				if(type != null)
					return;
			}
		}

		public static void PrintUsage()
		{
			System.Console.WriteLine("Usage:");
			System.Console.WriteLine("EnumCheck [<enum> [<enum> [... ] ] ]");
			System.Console.WriteLine("");
			System.Console.WriteLine("enum := <namespace>[.<subnamespace>[...]].enum_name");
			System.Console.WriteLine("");
		}

		public static void Main(string[] args)
		{
			if(args.Length > 0 && (args[0] == "--help" || args[0] == "-h"))
			{
				PrintUsage();
				return;
			}
			EnumCheck check = null;
			string bdir;
			System.Console.WriteLine("Enter directory to search assemblies");
			System.Console.Write("[{0}]: ", basePath);
			bdir = System.Console.ReadLine();
			while(bdir.EndsWith("/") || bdir.EndsWith("\\"))
			{
				bdir = bdir.Substring(0, bdir.Length - 1);
			}
			if(bdir != "")
			{
				basePath = bdir;
			}
			if(args.Length != 0)
			{
				foreach(string clName in args)
				{
					check = new EnumCheck(clName);
					check.Display();
					System.Console.WriteLine("\n");
				}
			}
			while(true)
			{
				System.Console.Write("Enter the name of the Enumeration (end to stop): ");
				string clName = System.Console.ReadLine();
				if(clName == "stop" || clName == "end" || clName.Length == 0)
					break;
				check = new EnumCheck(clName);
				check.Display();
				System.Console.WriteLine("\n");
			}
		}
	}
}
