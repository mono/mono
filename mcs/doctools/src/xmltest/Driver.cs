using System;
using System.Xml.Serialization;

using Mono.Doc.Core;

namespace Mono.Doc.XmlTest
{
	public class Driver
	{
		public Driver()
		{
		}

		public static void Main(string[] args)
		{
			MonodocFile file  = new MonodocFile();
			XmlSerializer ser = new XmlSerializer(file.GetType());

			// a class
			ClassDoc hashtable = new ClassDoc("System.Collections.Hashtable");
			hashtable.Assembly = "corlib";

			file.Types.Add(hashtable);

			// an interface
			InterfaceDoc icollection = new InterfaceDoc("System.Collections.ICollection");
			icollection.Assembly     = "corlib";

			file.Types.Add(icollection);

			ser.Serialize(Console.Out, file);
			Console.WriteLine("\n\n");
		}
	}
}
