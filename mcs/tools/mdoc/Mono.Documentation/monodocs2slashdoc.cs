using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using Monodoc;
using Mono.Options;

namespace Mono.Documentation {
public class MDocToMSXDocConverter : MDocCommand {

	public override void Run (IEnumerable<string> args)
	{
		string file = null;
		var p = new OptionSet () {
			{ "o|out=", 
				"The XML {FILE} to generate.\n" + 
				"If not specified, will create a set of files in the curent directory " +
				"based on the //AssemblyInfo/AssemblyName values within the documentation.\n" +
				"Use '-' to write to standard output.",
				v => file = v },
		};
		List<string> directories = Parse (p, args, "export-slashdoc", 
				"[OPTIONS]+ DIRECTORIES",
				"Export mdoc(5) documentation within DIRECTORIES into \n" +
					"Microsoft XML Documentation format files.");
		if (directories == null)
			return;
		Run (file, directories);
	}
	
	public static void Run (string file, IEnumerable<string> dirs)
	{
		Dictionary<string, XmlElement> outputfiles = new Dictionary<string, XmlElement> ();

		XmlDocument nsSummaries = new XmlDocument();
		nsSummaries.LoadXml("<namespaces/>");

		foreach (string dir in dirs)
			Process (dir, outputfiles, nsSummaries, file == null);

		if (outputfiles.Count > 0 && file != null) {
			List<string> files = new List<string> (outputfiles.Keys);
			files.Sort ();
			XmlDocument d = new XmlDocument ();
			d.AppendChild (d.CreateElement ("doc"));
			d.FirstChild.AppendChild (
					d.ImportNode (outputfiles [files [0]].SelectSingleNode ("/doc/assembly"), true));
			XmlElement members = d.CreateElement ("members");
			d.FirstChild.AppendChild (members);
			foreach (string f in files) {
				XmlElement from = (XmlElement) outputfiles [f];
				foreach (XmlNode n in from.SelectNodes ("/doc/members/*"))
					members.AppendChild (d.ImportNode (n, true));
			}
			using (TextWriter tw = file == "-" ? Console.Out : new StreamWriter (file))
				WriteXml (d.DocumentElement, tw);
			return;
		}

		// Write out each of the assembly documents
		foreach (string assemblyName in outputfiles.Keys) {
			XmlElement members = (XmlElement)outputfiles[assemblyName];
			Console.WriteLine(assemblyName + ".xml");
			using(StreamWriter sw = new StreamWriter(assemblyName + ".xml")) {
				WriteXml(members.OwnerDocument.DocumentElement, sw);
			}
		}
	
		// Write out a namespace summaries file.
		Console.WriteLine("NamespaceSummaries.xml");
		using(StreamWriter writer = new StreamWriter("NamespaceSummaries.xml")) {
			WriteXml(nsSummaries.DocumentElement, writer);
		}
	}

	private static void Process (string basepath, Dictionary<string, XmlElement> outputfiles, XmlDocument nsSummaries, bool implicitFiles)
	{
		if (System.Environment.CurrentDirectory == System.IO.Path.GetFullPath(basepath) && implicitFiles) {
			Console.WriteLine("Don't run this tool from your documentation directory, since some files could be accidentally overwritten.");
			return;
		}

		XmlDocument index_doc = new XmlDocument();
		index_doc.Load(Path.Combine(basepath, "index.xml"));
		XmlElement index = index_doc.DocumentElement;
		
		foreach (XmlElement assmbly in index.SelectNodes("Assemblies/Assembly")) {
			string assemblyName = assmbly.GetAttribute("Name");
			if (outputfiles.ContainsKey (assemblyName))
				continue;
			XmlDocument output = new XmlDocument();
			XmlElement output_root = output.CreateElement("doc");
			output.AppendChild(output_root);

			XmlElement output_assembly = output.CreateElement("assembly");
			output_root.AppendChild(output_assembly);
			XmlElement output_assembly_name = output.CreateElement("name");
			output_assembly.AppendChild(output_assembly_name);
			output_assembly_name.InnerText = assemblyName;
		
			XmlElement members = output.CreateElement("members");
			output_root.AppendChild(members);
			
			outputfiles.Add (assemblyName, members);
		}
			
		foreach (XmlElement nsnode in index.SelectNodes("Types/Namespace")) {
			string ns = nsnode.GetAttribute("Name");
			foreach (XmlElement typedoc in nsnode.SelectNodes("Type")) {
				string typename = typedoc.GetAttribute("Name");
				XmlDocument type = new XmlDocument();
				type.Load(Path.Combine(Path.Combine(basepath, ns), typename) + ".xml");
				
				string assemblyname = type.SelectSingleNode("Type/AssemblyInfo/AssemblyName").InnerText;
				XmlElement members = outputfiles [assemblyname];
				if (members == null) continue; // assembly is strangely not listed in the index
				
				//CreateMember(EcmaDoc.GetCref (type.DocumentElement), type.DocumentElement, members);
					
				foreach (XmlElement memberdoc in type.SelectNodes("Type/Members/Member")) {
					//string name = EcmaDoc.GetCref (memberdoc);
					// FIXME
					string name = ns + "." + typename + "." + memberdoc.GetAttribute ("MemberName");
					CreateMember(name, memberdoc, members);
				}
			}
		}
		foreach (XmlElement nsnode in index.SelectNodes("Types/Namespace")) {
			AddNamespaceSummary(nsSummaries, basepath, nsnode.GetAttribute("Name"));
		}
	}
	
	private static void AddNamespaceSummary(XmlDocument nsSummaries, string basepath, string currentNs) {
		foreach (var filename in new [] {
				Path.Combine(basepath, currentNs + ".xml"),
				Path.Combine(basepath, "ns-" + currentNs + ".xml")}) {
			if (File.Exists(filename)) 	{
				XmlDocument nsSummary = new XmlDocument();
				nsSummary.Load(filename);
				XmlElement ns = nsSummaries.CreateElement("namespace");
				nsSummaries.DocumentElement.AppendChild(ns);
				ns.SetAttribute("name", currentNs);
				ns.InnerText = nsSummary.SelectSingleNode("/Namespace/Docs/summary").InnerText;
			}
		}
	}
	
	private static void CreateMember(string name, XmlElement input, XmlElement output) {
		XmlElement member = output.OwnerDocument.CreateElement("member");
		output.AppendChild(member);
		
		member.SetAttribute("name", name);
		
		foreach (XmlNode docnode in input.SelectSingleNode("Docs"))
			member.AppendChild(output.OwnerDocument.ImportNode(docnode, true));
	}

	private static void WriteXml(XmlElement element, System.IO.TextWriter output) {
		XmlTextWriter writer = new XmlTextWriter(output);
		writer.Formatting = Formatting.Indented;
		writer.Indentation = 4;
		writer.IndentChar = ' ';
		element.WriteTo(writer);
		output.WriteLine();	
	}
}

}
