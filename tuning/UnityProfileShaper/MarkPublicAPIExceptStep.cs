//
// ResolveFromAssemblyStep.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// (C) 2006 Jb Evain
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Mono.Cecil;
using UnityProfileShaper;

namespace Mono.Linker.Steps
{
	public class MarkPublicApiExceptStep : ResolveStep
	{
		AssemblyDefinition _assembly;
		string _file;

		public MarkPublicApiExceptStep(string assembly)
		{
			_file = assembly;
		}

		public MarkPublicApiExceptStep(AssemblyDefinition assembly)
		{
			_assembly = assembly;
		}

		public override void Process(LinkContext context)
		{
			if (_assembly != null)
			{
				context.SafeLoadSymbols(_assembly);
				context.Resolver.CacheAssembly(_assembly);
			}

			AssemblyDefinition assembly = _assembly ?? context.Resolve(_file);

			if (assembly.Kind != AssemblyKind.Dll)
				throw new InvalidOperationException("MarkPublicAPIExceptStep should only be applied to dll's");

			Annotations.SetAction(assembly, AssemblyAction.Link);

			foreach (TypeDefinition type in assembly.MainModule.Types)
			{
				if (!type.IsPublic) continue;
	
				if (IsInExceptionList(type)) continue;
				Annotations.Mark(type, "MarkPublicAPIExceptStep");

				if (type.HasFields)
					MarkFields(type.Fields, type);
				if (type.HasMethods)
					MarkMethods(type.Methods, type);
				if (type.HasConstructors)
					MarkMethods(type.Constructors, type);
			}
		}

		private static List<Regex> s_ExceptionListRegexes;
		List<Regex> GetExceptionListReGetRegexes()
		{
			if (s_ExceptionListRegexes==null)
			{
				var lines = Tools.ReadLinesFromDataFileWithoutPlus("MarkEverythingExceptTheseTypes.txt");
				s_ExceptionListRegexes = CreateRegexes(new List<string>(lines));
			}
			return s_ExceptionListRegexes;
		}

		private static List<Regex> s_ExceptionListOverrideRegexes;
		List<Regex> GetExceptionListOverrideRegexes()
		{
			if (s_ExceptionListOverrideRegexes == null)
			{
				s_ExceptionListOverrideRegexes = CreateRegexes(new List<string>(Tools.ReadLinesFromDataFileWithPlus("MarkEverythingExceptTheseTypes.txt")));
			}
			return s_ExceptionListOverrideRegexes;
		}

		private List<Regex> CreateRegexes(List<string> patterns)
		{
			var l = new List<Regex>();
			foreach (var s in patterns)
				l.Add(new Regex(s, RegexOptions.Compiled));
			return l;
		}

		private bool IsInExceptionList(TypeDefinition type)
		{
			var name = type.FullName;
			return IsInExceptionList(name);
		}
		private bool IsInExceptionList(MethodDefinition method)
		{
			var name = method.ToString();
			return IsInExceptionList(name);
		}

		private bool IsInExceptionList(string name)
		{
			foreach (var regex in GetExceptionListOverrideRegexes())
				if (regex.Match(name).Success)
					return false;

			foreach(var regex in GetExceptionListReGetRegexes())
				if (regex.Match(name).Success) 
					return true;

			return false;
		}

		static void MarkFields(ICollection fields, TypeDefinition markedby)
		{
			foreach (FieldDefinition field in fields)
			{
				if (field.IsPublic)
					Annotations.Mark(field, markedby);
			}
				
		}

		void MarkMethods(ICollection methods, TypeDefinition markedby)
		{
			foreach (MethodDefinition method in methods)
				if (method.IsPublic)
					if (!IsInExceptionList(method))
						MarkMethod(method, MethodAction.ForceParse, markedby);
		}

		static void MarkMethod(MethodDefinition method, MethodAction action, TypeDefinition markedby)
		{
			Annotations.Mark(method, markedby);
			Annotations.SetAction(method, action);
		}
	}
}
