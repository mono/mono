using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SwitchMode
{
	class Program
	{
		static void Main (string [] args) {
			string file = args [0];
			string mode = args [1];
			XElement config = XElement.Load(file,LoadOptions.PreserveWhitespace);
			
			XElement appSettings = config.Element("appSettings");
			if (appSettings == null) {
				appSettings = new XElement ("appSettings");
				config.Add (appSettings);
			}
			var results = from el in appSettings.Elements ()
						  where el.Attribute ("key").Value == "onlyClients"
						  select el;
			XElement onlyClients;
			if (results.Count() == 1)
				onlyClients = results.First ();
			else if (results.Count() == 0) {
				onlyClients = new XElement ("add");
				onlyClients.SetAttributeValue ("key", "onlyClients");
				appSettings.Add (onlyClients);
			}
			else 
				throw new Exception ("Too many onlyClients appSettings clauses");
			if (mode == "client")
				onlyClients.SetAttributeValue("value","true");
			else if (mode == "inproc")
				onlyClients.SetAttributeValue ("value", "false");
			else {
				throw new Exception ("Unrecognized mode: " + mode);
			}
			config.Save (file,SaveOptions.DisableFormatting);
			Console.WriteLine ("Successfully switched to mode : " + mode);
		}
	}
}
