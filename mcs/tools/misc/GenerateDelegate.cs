/**
 * Namespace: com.mastergaurav.utils
 * Class:     GenerateDelegate
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.IO;

namespace com.mastergaurav.Utils
{
	public class GenerateDelegate
	{
		public static string TargetDirectory = String.Empty;
		public static string NamespaceName   = String.Empty;

		public static readonly string PROLOGUE = "/**\n * Namespace: ";
		public static readonly string DETAILS   = " *\n * Author: Gaurav Vaish\n" +
		                                          " * Maintainer: gvaish@iitk.ac.in\n" +
		                                          " * Contact:  <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>\n" +
		                                          " * Implementation: yes\n" +
		                                          " * Status: 100%\n" +
		                                          " *\n" +
		                                          " * (C) Gaurav Vaish (2002)\n" +
		                                          " */\n\n";
		public static readonly string USING     = "using System;\n" +
		                                          "using System.Web;\n" +
		                                          "using System.Web.UI;\n\n";

		public static readonly string NAMESPACE = "namespace ";

		public static string AskForNamespace()
		{
			string nm = String.Empty;
			System.Console.Write("Enter the name of the namespace: ");
			nm = System.Console.ReadLine();
			return nm;
		}

		public static string AskForMore()
		{
			string del = String.Empty;
			System.Console.Write("Enter the name of the delegate (end to stop): ");
			del = System.Console.ReadLine();
			if(del == String.Empty || del == "end")
				return String.Empty;
			return del;
		}

		public static void Generate(string delName)
		{
			string fileName = TargetDirectory + "\\" + delName + "EventHandler.cs";
			System.Console.Write("File: ");//, fileName);
			System.Console.Write(fileName);
			System.Console.Write("\tGenerating");

			StreamWriter writer;
			try
			{
				Stream stream = new FileStream(fileName, FileMode.Truncate, FileAccess.Write);
				writer = new StreamWriter(stream);
			} catch(FileNotFoundException)
			{
				writer = File.CreateText(fileName);
			}
			if(writer == null)
			{
				System.Console.WriteLine("Null writer...\n");
				return;
			}
			writer.Write(PROLOGUE);
			writer.Write(NamespaceName + "\n");
			writer.Write(DETAILS);
			writer.Write(NAMESPACE);
			writer.Write(NamespaceName + "\n");
			writer.Write("{\n");
			writer.Write("\tpublic delegate void ");
			writer.Write(delName);
			writer.Write("EventHandler(object sender, ");
			writer.Write(delName);
			writer.Write("EventArgs e);\n");
			writer.Write("}");

			writer.Flush();
			writer.Close();

			System.Console.WriteLine("\tGenerated\n");
		}

		public static string GetTargetDir()
		{
			System.Console.Write("Enter target directory: ");
			return System.Console.ReadLine();
		}

		public static void Usage(bool wrong)
		{
			if(wrong)
			{
				System.Console.WriteLine("Wrong # arguments.");
			}
			System.Console.WriteLine("Usage: GenerateDelegate [target-dir] [namespace] [delegate1 [delegate2 [...]]]");
		}

		public static bool IsHelp(string arg)
		{
			return (arg == "-h" || arg == "--help");
		}

		public static bool IsDirectory(string dirName)
		{
			FileAttributes attrs;
			try
			{
				attrs = File.GetAttributes(dirName);
				if( (attrs & FileAttributes.Directory) != FileAttributes.Directory)
				{
					Usage(true);
					return false;
				}
			}catch(Exception e)
			{
				System.Console.WriteLine("Exception: {0}", e.ToString());
				return false;
			}
			return true;
		}

		public static void Main(string[] args)
		{
			if(args.Length == 1 && IsHelp(args[0]))
			{
				Usage(false);
				return;
			}

			if(args.Length == 0)
			{
				TargetDirectory = GetTargetDir();
				while(TargetDirectory.EndsWith("\\"))
				{
					TargetDirectory = TargetDirectory.Substring(0, TargetDirectory.Length - 1);
				}
			} else
			{
				while(args[0].EndsWith("\\"))
				{
					args[0] = args[0].Substring(0, args[0].Length - 1);
				}
				TargetDirectory = args[0];
			}

			if(!IsDirectory(TargetDirectory))
				return;

			if(args.Length > 1)
			{
				NamespaceName = args[1];
			} else
			{
				NamespaceName = AskForNamespace();
			}

			if(args.Length > 2)
			{
				int i=0;
				foreach(string currArg in args)
				{
					if(i != 0)
					{
						Generate(currArg);
					}
					i++;
				}
			}
			string delegateName = String.Empty;
			while((delegateName = AskForMore()) != String.Empty)
			{
				Generate(delegateName);
			}
		}
	}
}
