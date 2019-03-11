//
// SizeCheckStep.cs
//
// Author:
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright (c) 2019 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using Mono.Cecil;
using System.Collections.Generic;

namespace Mono.Linker.Steps
{
	public class SizeCheckStep : BaseStep
	{
		readonly Dictionary<string, int> namespace_sizes;
		readonly Dictionary<string, int> type_sizes;
		readonly Dictionary<string, int> method_sizes;

		public SizeCheckStep ()
		{
			namespace_sizes = new Dictionary<string, int> ();
			type_sizes = new Dictionary<string, int> ();
			method_sizes = new Dictionary<string, int> ();
		}

		protected override void ProcessAssembly (AssemblyDefinition assembly)
		{
			foreach (TypeDefinition type in assembly.MainModule.Types) {
				ProcessType (type);
			}
		}

		protected override void EndProcess ()
		{
			base.EndProcess ();
			Report ();
		}

		struct SizeEntry : IComparable<SizeEntry>
		{
			public readonly string Name;
			public readonly int Size;

			public SizeEntry (string name, int size)
			{
				Name = name;
				Size = size;
			}

			public int CompareTo (SizeEntry obj)
			{
				return Size.CompareTo (obj.Size);
			}
		}

		void Report ()
		{
			var sorted_ns = new SortedSet<SizeEntry> ();
			var sorted_type = new SortedSet<SizeEntry> ();
			foreach (var ns in namespace_sizes)
				sorted_ns.Add (new SizeEntry (ns.Key, ns.Value));
			foreach (var type in type_sizes)
				sorted_type.Add (new SizeEntry (type.Key, type.Value));

			Console.Error.WriteLine ();
			Console.Error.WriteLine ("SIZE REPORT");

			foreach (var ns in sorted_ns)
				Console.Error.WriteLine ($"NS: {ns.Name} {ns.Size}");

			Console.Error.WriteLine ();
			foreach (var ns in sorted_type)
				Console.Error.WriteLine ($"TYPE: {ns.Name} {ns.Size}");

			Console.Error.WriteLine ();
			Console.Error.WriteLine ();

		}

		void ProcessType (TypeDefinition type)
		{
			if (!Annotations.IsMarked (type))
				return;

			foreach (var method in type.Methods)
				ProcessMethod (method);

			foreach (var nested in type.NestedTypes)
				ProcessType (nested);
		}

		void ProcessMethod (MethodDefinition method)
		{
			if (!Annotations.IsMarked (method))
				return;
			if (!method.HasBody)
				return;

			if (method_sizes.ContainsKey (method.FullName))
				return;

			method_sizes.Add (method.FullName, method.Body.CodeSize);

			if (namespace_sizes.TryGetValue (method.DeclaringType.Namespace, out var ns))
				namespace_sizes[method.DeclaringType.Namespace] = ns + method.Body.CodeSize;
			else
				namespace_sizes.Add (method.DeclaringType.Namespace, method.Body.CodeSize);

			if (type_sizes.TryGetValue (method.DeclaringType.FullName, out var ts))
				type_sizes[method.DeclaringType.FullName] = ts + method.Body.CodeSize;
			else
				type_sizes.Add (method.DeclaringType.FullName, method.Body.CodeSize);

		}
	}
}
