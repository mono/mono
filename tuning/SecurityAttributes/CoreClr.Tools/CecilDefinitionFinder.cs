using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace CoreClr.Tools
{
	public class CecilDefinitionFinder
	{
		private readonly IEnumerable<AssemblyDefinition> _assemblies;

		public CecilDefinitionFinder(IEnumerable<AssemblyDefinition> assemblies)
		{
			_assemblies = assemblies;
		}
		public CecilDefinitionFinder(AssemblyDefinition assembly)
		{
			_assemblies = new List<AssemblyDefinition>() { assembly };
		}

		public TypeDefinition FindType(string fullname)
		{
			return _assemblies.Select(assembly => assembly.MainModule.Types[fullname]).FirstOrDefault(t => t != null);
		}

		public TypeDefinition GetType(string fullname)
		{
			var r = FindType(fullname);
			if (r==null)
				Console.WriteLine("Type not found: "+fullname);
			return r;
		}

		public MethodDefinition FindMethod(string signature)
		{
			int pos = signature.IndexOf(" ");
			if (pos == -1)
				throw new ArgumentException();

			string tmp = signature.Substring(pos + 1);

			pos = tmp.IndexOf("::");
			if (pos == -1)
				throw new ArgumentException();

			string typeName = tmp.Substring(0, pos);

			int parpos = tmp.IndexOf("(");
			if (parpos == -1)
				throw new ArgumentException();

			string methodName = tmp.Substring(pos + 2, parpos - pos - 2);

			TypeDefinition type = FindType(typeName);
			if (type == null)
				return null;

			return methodName.StartsWith(".c")
				? FindMethod(type.Constructors, signature)
				: FindMethod(type.Methods, signature);
		}

		public IEnumerable<MethodDefinition> FindMethods(IEnumerable<string> signatures)
		{
			var alreadyreturned = new HashSet<MethodDefinition>();
			foreach (var signature in signatures)
			{
				var found = FindMethod(signature);
				if (alreadyreturned.Contains(found))
					throw new Exception("Double entry: " + found.ToString());
				
				if (found==null)
				{
					Console.WriteLine("Method not found: "+signature);
					continue;
				}
				alreadyreturned.Add(found);
					//ThrowNotFoundException(signature);
				yield return found;
			}
		}

		private void ThrowNotFoundException(string signature)
		{
			throw new ArgumentException(string.Format("'{0}' not found in assembly '{1}'", signature, _assemblies));
		}

		public IMemberDefinition FindTargetOf(SecurityAttributeDescriptor descriptor)
		{
			switch (descriptor.Target)
			{
				case TargetKind.Type:
					return FindType(descriptor.Signature);
				case TargetKind.Method:
					return FindMethod(descriptor.Signature);
				default:
					throw new ArgumentException();
			}
		}

		static MethodDefinition FindMethod(IEnumerable methods, string signature)
		{
			foreach (MethodDefinition method in methods)
				if (GetFullName(method) == signature)
					return method;

			return null;
		}

		static string GetFullName(MethodReference method)
		{
			return MethodSignatureProvider.SignatureFor(method);
		}
	}
}