//
// sourcebeingcompiled.cs: Tracks once-per-source things 
//
// Author:
//   Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2004 Rafael Teixeira.
//

using System;
using System.Collections;
using Mono.Languages;

namespace Mono.MonoBASIC {

	/// <summary>
	///   Keeps track of the once-per-source things in the VB.NET code.
	/// </summary>
	public class SourceBeingCompiled {

		Hashtable imports_clauses;
		Hashtable aliases;
		
		//
		// This class holds the location where a using definition is
		// done, and whether it has been used by the program or not.
		//
		// We use this to flag using clauses for namespaces that do not
		// exist.
		//
		public class ImportsEntry {
			public string Name;
			public bool Used;
			public Location Location;
			
			public ImportsEntry (string name, Location loc)
			{
				Name = name;
				Location = loc;
				Used = false;
			}
		}
		
		public SourceBeingCompiled () { } 

		/// <summary>
		///   Initializes the list of preimported namespaces
		/// </summary>
		public void InitializeImports (ArrayList ImportsList)
		{
			foreach(string preImportedNamespace in ImportsList)
				this.Imports(preImportedNamespace, Location.Null);
		}

		/// <summary>
		///   Records a new namespace for resolving name references
		/// </summary>
		public void Imports (string ns, Location loc)
		{
			if (imports_clauses == null)
				imports_clauses = new CaseInsensitiveHashtable ();

			ImportsEntry ue = new ImportsEntry (ns, loc);
			imports_clauses [ns] = ue;
		}

		public ICollection ImportsTable {
			get {
				return imports_clauses.Values;
			}
		}
		
		public string[] GetNamespacesInScope(string currentNamespace)
		{
			ArrayList list = new ArrayList();
			foreach(ImportsEntry ie in ImportsTable)
				list.Add(ie.Name);
			list.Add(currentNamespace);
			return (string[])list.ToArray(typeof(string));
			
		}

		public void ImportsWithAlias (string alias, string namespace_or_type, Location loc)
		{
			if (aliases == null)
				aliases = new CaseInsensitiveHashtable ();
			
			if (aliases.Contains (alias)){
				Report.Error (1537, loc, "The Imports clause with alias '" + alias +
					      "' appeared previously in this namespace");
				return;
			}
					
			aliases [alias] = namespace_or_type;
		}

		public string LookupAlias (string alias)
		{
			string value = null;

			if (aliases != null)
				value = (string) (aliases [alias]);

			return value;
		}

		/// <summary>
		///   Used to validate that all the using clauses are correct
		///   after we are finished parsing all the files.  
		/// </summary>
		public void VerifyImports ()
		{
			ArrayList unused = new ArrayList ();
			
			foreach (ImportsEntry ue in ImportsTable) {
				if (ue.Used)
					continue;
				unused.Add (ue);
			}

			//
			// If we have unused imports aliases, load all namespaces and check
			// whether it is unused, or it was missing
			//
/* FIXME: why is happening a ghostly NullReferenceException inside TypeManager.GetNamespaces ()?

			if (unused.Count > 0) {
				CaseInsensitiveHashtable namespaces = TypeManager.GetNamespaces ();

				foreach (ImportsEntry ue in unused) {
					if (namespaces.Contains (ue.Name)){
						Report.Warning (6024, ue.Location, "Unused namespace in 'Imports' declaration");
						continue;
					}

					Report.Error (246, ue.Location, "The namespace '" + ue.Name +
						      "' can not be found (missing assembly reference?)");
				}
			}
*/
		}

	}
}
