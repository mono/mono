using System;
using System.Xml.Serialization;

using Mono.Doc.Core;

namespace Mono.Doc.XmlTest
{
	public class Driver
	{
		private static string assemblyFile = 
			@"D:\projects\mcs\doctools\build\System.Xml.dll";
		private static string sampleClass = "System.Xml.XmlDocument";
		private static string sampleEnum  = "System.Xml.WriteState";

		public Driver()
		{
		}

		public static void Main(string[] args)
		{
			AssemblyLoader loader = new AssemblyLoader(assemblyFile);
			Type           t      = loader.Assembly.GetType(sampleClass, true, false);
			MonodocFile    file   = new MonodocFile();
			XmlSerializer  ser    = new XmlSerializer(file.GetType());
			ClassDoc       aClass = new ClassDoc(t, loader);

			file.Types.Add(aClass);

			EnumDoc anEnum = new EnumDoc(loader.Assembly.GetType(sampleEnum, true, false), loader);

			file.Types.Add(anEnum);

			// serialize to stdout for now
			ser.Serialize(Console.Out, file);
			Console.WriteLine();
		}
	}
}
