//
// file:	ictool.cs
// author:	Dan Lewis (dihlewis@yahoo.co.uk)
// 		(C) 2002
//
// description:
//
// Tool for generating C prototypes and structures suitable for use by the runtime
// from a list of supplied assemblies. See ictool-config.xml for configuration details.
//

using System;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Collections;

public class ICTool {
	public static void Main (string[] args) {
		string filename = "ictool-config.xml";
		if (args.Length == 1) {
			filename = args[0];
		}
		else if (args.Length > 1) {
			Console.Error.WriteLine ("Usage: ictool.exe [config.xml]");
			Environment.Exit (-1);
		}

		try {
			Stream config = File.OpenRead (filename);
			Configure (config);
		}
		catch (Exception e) {
			Console.Error.WriteLine ("Error: could not read configuration file.");
			Console.Error.WriteLine (e);
			Environment.Exit (-1);
		}

		EmitPrototypes ();
		EmitStructures ();
	}

	// private

	private static void EmitPrototypes () {
		StreamWriter methods_file = GetOutputFile ("methods");
		StreamWriter map_file = GetOutputFile ("map");

		// includes

		methods_file.WriteLine ("#include \"{0}\"\n", output_files["types"]);
		map_file.WriteLine ("#include \"{0}\"\n", output_files["methods"]);

		map_file.Write (
			"static gpointer icall_map [] = {\n\t"
		);

		ArrayList map_lines = new ArrayList ();

		BindingFlags binding =
			BindingFlags.DeclaredOnly |
			BindingFlags.Instance |
			BindingFlags.Static |
			BindingFlags.Public |
			BindingFlags.NonPublic;

		foreach (Type type in types.Values) {
			bool has_icall = false;
			MethodInfo[] methods = type.GetMethods (binding);

			foreach (MethodInfo method in methods) {
				if (IsInternalCall (method)) {
					has_icall = true;
					break;
				}
			}

			if (!has_icall)
				continue;

			methods_file.WriteLine ("\n/* {0} */\n", type.FullName);
			//map_lines.Add (String.Format ("\n/* {0} */\n", type.FullName));
			
			foreach (MethodInfo method in methods) {
				if (!IsInternalCall (method))
					continue;

				// function name

				string func_name = String.Format ("ves_icall_{0}_{1}",
					
					type.FullName,
					method.Name
				);

				func_name = func_name.Replace ('.', '_');

				// map file

				map_lines.Add (String.Format (
					"\"{0}::{1}\", {2}",

					type.FullName.Replace ('.', '_'),
					method.Name,
					func_name
				));

				// methods file

				ArrayList args = new ArrayList ();

				// FIXME: return types that are structs need to be inserted
				// into the argument list as a destination pointer

				// object/value instance pointer
				
				if (IsInstanceMethod (method)) {
					args.Add (String.Format (
						"{0}{1}",

						peer_map.GetPeer (method.DeclaringType).GetTypedef (1),
						"this"
					));
				}

				// arguments

				foreach (ParameterInfo param in method.GetParameters ()) {
					Type arg_type = param.ParameterType;

					int refs = 0;
					if (arg_type.IsByRef) {
						arg_type = arg_type.GetElementType ();
						++ refs;
					}

					Peer arg_peer = peer_map.GetPeer (arg_type);
					if (!arg_peer.IsValueType)
						++ refs;

					args.Add (String.Format ("{0}{1}", arg_peer.GetTypedef (refs), param.Name));
				}

				Peer ret = peer_map.GetPeer (method.ReturnType);
				methods_file.WriteLine ("static {0}", ret.GetTypedef (ret.IsValueType ? 0 : 1));
				methods_file.WriteLine ("{0} ({1});",
					
					func_name,
					Join (", ", args)
				);
				methods_file.WriteLine ();
			}

		}

		methods_file.Close ();

		// write map file and close it

		map_file.Write (
			"{0}\n}};\n", Join (",\n\t", map_lines)
		);

		map_file.Close ();
	}

	private static bool IsInternalCall (MethodInfo meth) {
		return (meth.GetMethodImplementationFlags () & MethodImplAttributes.InternalCall) != 0;
	}

	private static bool IsInstanceMethod (MethodInfo meth) {
		return (meth.CallingConvention & CallingConventions.HasThis) != 0;
	}

	private static void EmitStructures () {
		StreamWriter file = GetOutputFile ("types");

		// build dependency graph
		
		DependencyGraph dg = new DependencyGraph ();
		foreach (Peer peer in peer_map.Peers) {
			dg.AddNode (peer);

			// peer depends on nearest base

			if (peer.NearestBase != null)
				dg.AddEdge (peer.NearestBase, peer);

			// peer depends on any value types used for fields

			foreach (PeerField field in peer.Fields) {
				if (field.Peer.IsValueType)
					dg.AddEdge (field.Peer, peer);
			}
		}

		// write structures in order

		foreach (Peer peer in dg.TopologicalSort ()) {
			if (peer.IsOpaque)
				continue;

			if (peer.IsEnum) {
				file.WriteLine ("typedef {0} {1};", peer.UnderlyingPeer.Name, peer.Name);
				file.WriteLine ("enum _{0} {{", peer.Name);

				ArrayList enum_lines = new ArrayList ();
				foreach (string name in peer.EnumConstants.Keys) {
					enum_lines.Add (String.Format ("\t{0}_{1} = {2}",
						peer.Name,
						name,
						peer.EnumConstants[name]
					));
				}
				
				file.WriteLine ("{0}\n}};\n", Join (",\n", enum_lines));
			}
			else {
				file.WriteLine ("typedef struct _{0} {{", peer.Name);

				// base type
				
				if (peer.NearestBase != null) {
					file.WriteLine ("\t{0} __base;", peer.NearestBase.Name);
					file.WriteLine ();
				}

				// fields
				
				foreach (PeerField field in peer.Fields) {
					bool use_struct = true;
					if (field.Peer.IsValueType || field.Peer.IsOpaque)
						use_struct = false;
				
					file.WriteLine ("\t{0}{1}{2};",
						use_struct ? "struct _" : "",
						field.Peer.GetTypedef (field.Peer.IsValueType ? 0 : 1),
						field.Name
					);
				}

				file.WriteLine ("}} {0};\n", peer.Name);
			}
		}
	}

	private static void LoadAssemblies () {
		types = new Hashtable ();

		foreach (string filename in assemblies) {
			Assembly assembly;

			// find assembly

			FileInfo info = null;
			foreach (string path in assembly_paths) {
				info = new FileInfo (Path.Combine (path, filename));
				if (info.Exists)
					break;
			}

			if (!info.Exists) {
				Console.Error.WriteLine ("Error: assembly {0} not found.", filename);
				Environment.Exit (-1);
			}

			// load assembly

			assembly = Assembly.LoadFrom (info.FullName);

			// load types

			ArrayList loaded_types;
			
			try {
				loaded_types = new ArrayList (assembly.GetTypes ());
			}
			catch (ReflectionTypeLoadException e) {
				loaded_types = new ArrayList ();
				foreach (Type type in e.Types) {
					if (type != null)
						loaded_types.Add (type);
				}

				foreach (Exception f in e.LoaderExceptions) {
					if (f is TypeLoadException) {
						Console.Error.WriteLine ("Warning: {0} could not be loaded from assembly {1}.",
							((TypeLoadException)f).TypeName,
							filename
						);
					}
					else
						Console.Error.WriteLine (f);
				}
			}

			// add to type dictionary

			foreach (Type type in loaded_types) {
				if (!types.Contains (type.FullName))
					types.Add (type.FullName, type);
			}
		}
	}

	private static void Configure (Stream input) {
		XmlDocument doc = new XmlDocument ();
		doc.Load (input);

		// assemblies

		assembly_paths = new ArrayList ();
		assembly_paths.Add (".");

		foreach (XmlNode node in doc.SelectNodes ("config/assemblypath")) {
			assembly_paths.Add (node.Attributes["path"].Value);
		}

		assemblies = new ArrayList ();
		foreach (XmlNode node in doc.SelectNodes ("config/assembly")) {
			assemblies.Add (node.Attributes["file"].Value);
		}

		LoadAssemblies ();

		// outputfiles

		output_path = ".";
		XmlNode path_node = doc.SelectSingleNode ("config/outputpath");
		if (path_node != null)
			output_path = path_node.Attributes["path"].Value;

		output_files = new Hashtable ();
		output_includes = new Hashtable ();
		foreach (XmlNode node in doc.SelectNodes ("config/outputfile")) {
			string name = node.Attributes["name"].Value;
			output_files.Add (name, node.Attributes["file"].Value);

			foreach (XmlNode child in node.ChildNodes) {
				if (child.Name == "include")
					output_includes[name] = child.InnerText;
			}
		}

		// typemap

		peer_map = new PeerMap ();
		foreach (XmlNode node in doc.SelectNodes ("config/typemap/namespace")) {
			string ns = node.Attributes["name"].Value;

			foreach (XmlNode child in node.ChildNodes) {
				if (child.Name == "type") {
					string name = child.Attributes["name"].Value;
					string peer_name = child.Attributes["peer"].Value;

					bool opaque = false;
					if (child.Attributes["opaque"] != null && child.Attributes["opaque"].Value == "true")
						opaque = true;

					String fullname = String.Format ("{0}.{1}", ns, name);
					
					Type type;
					if (child.Attributes["default"] != null && child.Attributes["default"].Value == "true")
						type = Type.GetType (fullname);
					else
						type = (Type)types [fullname];

					if (type != null)
						peer_map.Add (new Peer (type, peer_name, opaque));
				}
			}
		}

		peer_map.ResolvePeers ();
	}

	private static StreamWriter GetOutputFile (string name) {
		string filename = Path.Combine (output_path, (string)output_files[name]);
		StreamWriter file = File.CreateText (filename);
		file.AutoFlush = true;

		file.Write (

// (verbatim string)
		
@"/**
 *  {0}
 *
 *  This file was automatically generated on {1} by ictool.exe from
 *  the following assemblies:
 *    {2}
 */
 
",

			output_files[name],
			DateTime.Now.ToString ("d"),
			Join (", ", assemblies)
		);

		if (output_includes.Contains (name)) {
			file.WriteLine (output_includes [name]);
			file.WriteLine ();
		}

		return file;
	}

	private static string Join (string separator, ICollection values) {
		// note to microsoft: please implement this in String :)

		string[] strs = new string[values.Count];

		int i = 0;
		foreach (object value in values)
			strs[i ++] = value.ToString ();

		return String.Join (separator, strs);
	}
	
	private static ArrayList assembly_paths;
	private static ArrayList assemblies;
	private static string output_path;
	private static Hashtable output_files;
	private static Hashtable output_includes;
	private static PeerMap peer_map;
	private static Hashtable types;
}
