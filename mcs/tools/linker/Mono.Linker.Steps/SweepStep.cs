//
// SweepStep.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// (C) 2006 Jb Evain
// (C) 2007 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Mono.Linker.Steps {

	public class SweepStep : BaseStep {

		AssemblyDefinition [] assemblies;
		HashSet<AssemblyDefinition> resolvedTypeReferences;

		protected override void Process ()
		{
			assemblies = Context.GetAssemblies ();
			foreach (var assembly in assemblies) {
				SweepAssembly (assembly);
				if (Annotations.GetAction (assembly) == AssemblyAction.Copy) {
					// Copy assemblies can still contain Type references with
					// type forwarders from Delete assemblies
					// thus try to resolve all the type references and see
					// if some changed the scope. if yes change the action to Save
					if (ResolveAllTypeReferences (assembly))
						Annotations.SetAction (assembly, AssemblyAction.Save);
				}
			}
		}

		void SweepAssembly (AssemblyDefinition assembly)
		{
			if (Annotations.GetAction (assembly) != AssemblyAction.Link)
				return;

			if (!IsMarkedAssembly (assembly)) {
				RemoveAssembly (assembly);
				return;
			}

			var types = new List<TypeDefinition> ();

			foreach (TypeDefinition type in assembly.MainModule.Types) {
				if (Annotations.IsMarked (type)) {
					SweepType (type);
					types.Add (type);
					continue;
				}

				if (type.Name == "<Module>")
					types.Add (type);
			}

			assembly.MainModule.Types.Clear ();
			foreach (TypeDefinition type in types)
				assembly.MainModule.Types.Add (type);
		}

		bool IsMarkedAssembly (AssemblyDefinition assembly)
		{
			return Annotations.IsMarked (assembly.MainModule);
		}

		void RemoveAssembly (AssemblyDefinition assembly)
		{
			Annotations.SetAction (assembly, AssemblyAction.Delete);

			SweepReferences (assembly);
		}

		void SweepReferences (AssemblyDefinition target)
		{
			foreach (var assembly in assemblies)
				SweepReferences (assembly, target);
		}

		void SweepReferences (AssemblyDefinition assembly, AssemblyDefinition target)
		{
			if (assembly == target)
				return;

			var references = assembly.MainModule.AssemblyReferences;
			for (int i = 0; i < references.Count; i++) {
				var reference = references [i];
				var r = Context.Resolver.Resolve (reference);
				if (!AreSameReference (r.Name, target.Name))
					continue;

				references.RemoveAt (i);
				// Removing the reference does not mean it will be saved back to disk!
				// That depends on the AssemblyAction set for the `assembly`
				switch (Annotations.GetAction (assembly)) {
				case AssemblyAction.Copy:
					// Copy means even if "unlinked" we still want that assembly to be saved back 
					// to disk (OutputStep) without the (removed) reference
					Annotations.SetAction (assembly, AssemblyAction.Save);
					ResolveAllTypeReferences (assembly);
					break;

				case AssemblyAction.Save:
				case AssemblyAction.Link:
					ResolveAllTypeReferences (assembly);
					break;
				}
				return;
			}
		}

		bool ResolveAllTypeReferences (AssemblyDefinition assembly)
		{
			if (resolvedTypeReferences == null)
				resolvedTypeReferences = new HashSet<AssemblyDefinition> ();
			if (resolvedTypeReferences.Contains (assembly))
				return false;
			resolvedTypeReferences.Add (assembly);

			var hash = new Dictionary<TypeReference,IMetadataScope> ();
			bool changes = false;

			foreach (TypeReference tr in assembly.MainModule.GetTypeReferences ()) {
				if (hash.ContainsKey (tr))
					continue;
				var td = tr.Resolve ();
				IMetadataScope scope = tr.Scope;
				// at this stage reference might include things that can't be resolved
				// and if it is (resolved) it needs to be kept only if marked (#16213)
				if ((td != null) && Annotations.IsMarked (td)) {
					scope = assembly.MainModule.Import (td).Scope;
					if (tr.Scope != scope)
						changes = true;
					hash.Add (tr, scope);
				}
			}
			if (assembly.MainModule.HasExportedTypes) {
				foreach (var et in assembly.MainModule.ExportedTypes) {
					var td = et.Resolve ();
					IMetadataScope scope = et.Scope;
					if ((td != null) && Annotations.IsMarked (td)) {
						scope = assembly.MainModule.Import (td).Scope;
						hash.Add (td, scope);
					}
				}
			}

			// Resolve everything first before updating scopes.
			// If we set the scope to null, then calling Resolve() on any of its
			// nested types would crash.

			foreach (var e in hash) {
				e.Key.Scope = e.Value;
			}

			return changes;
		}

		void SweepType (TypeDefinition type)
		{
			if (type.HasFields)
				SweepCollection (type.Fields);

			if (type.HasMethods)
				SweepCollection (type.Methods);

			if (type.HasNestedTypes)
				SweepNestedTypes (type);
		}

		void SweepNestedTypes (TypeDefinition type)
		{
			for (int i = 0; i < type.NestedTypes.Count; i++) {
				var nested = type.NestedTypes [i];
				if (Annotations.IsMarked (nested)) {
					SweepType (nested);
				} else {
					type.NestedTypes.RemoveAt (i--);
				}
			}
		}

		void SweepCollection (IList list)
		{
			for (int i = 0; i < list.Count; i++)
				if (!Annotations.IsMarked ((IMetadataTokenProvider) list [i]))
					list.RemoveAt (i--);
		}

		static bool AreSameReference (AssemblyNameReference a, AssemblyNameReference b)
		{
			if (a == b)
				return true;

			if (a.Name != b.Name)
				return false;

			if (a.Version > b.Version)
				return false;

			return true;
		}
	}
}
