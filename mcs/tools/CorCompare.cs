// CorCompare
//
// Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak

using System;
using System.Reflection;
using System.Collections;
using System.Text;
using System.IO;

namespace Mono.CorCompare
{
	class XMLUtil{
		public static string ToXML(
			ArrayList list, 
			string itemWrap,
			string listWrap)
		{
			if (null == itemWrap){
				throw new ArgumentNullException("itemWrap");
			}
			if (null == listWrap){
				throw new ArgumentNullException("listWrap");
			}
			StringBuilder output = new StringBuilder();
			output.Append("<"+listWrap+">");
			foreach(object o in list){
				output.Append("\n<"+itemWrap+">");
				output.Append(o.ToString());
				output.Append("</"+itemWrap+">");
			}
			output.Append("\n</"+listWrap+">");
			return output.ToString();
		}
	}
	class CorCompare {
		public static void Main(string[] args) {
			if (args.Length < 1) {
				Console.WriteLine ("Usage: CorCompare assembly_to_compare");
				return;
			}
			Assembly monoAsmbl = null;
			try{
				monoAsmbl = Assembly.LoadFrom(args[0]);
			}
			catch(FileNotFoundException)
			{
				Console.WriteLine("Could not find corlib file: {0}", args[0]);
				return;
			}

			Assembly msAsmbl = Assembly.GetAssembly(typeof (System.Object));
			Type[] mscorlibTypes = msAsmbl.GetTypes();
			Type[] monocorlibTypes;
			monocorlibTypes = monoAsmbl.GetTypes();
			ArrayList TypesList = new ArrayList();
			foreach(Type subt in monocorlibTypes){
				TypesList.Add(subt.FullName);
			}
			TypesList.Sort();
			
			ArrayList MissingTypes = new ArrayList();
			bool foundit = false;
			foreach(Type t in mscorlibTypes) {
				if (t.IsPublic) {
					foundit = (TypesList.BinarySearch(t.FullName) >= 0);
					if (!foundit){
						MissingTypes.Add(t.FullName);
					}
				}
			}
			Console.WriteLine(XMLUtil.ToXML(MissingTypes, "Type", "MissingTypes"));
		}
	}

}
