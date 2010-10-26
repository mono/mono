// Injector.cs adapted from Mono.Tuner/InjectSecurityAttributes.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2009 Novell, Inc.
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
using System.IO;
using System.Collections.Generic;
using Mono.Cecil;

namespace CoreClr.Tools
{
	public class Injector
	{
		readonly AssemblyDefinition _assembly;
		readonly CecilDefinitionFinder _cecilDefinitionFinder;

		public Injector(string assemblyPath) : this(AssemblyFactory.GetAssembly(assemblyPath))
		{
		}

		public Injector(AssemblyDefinition assembly)
		{
			_assembly = assembly;
			_cecilDefinitionFinder = new CecilDefinitionFinder(_assembly);
			OutputDirectory = Path.GetDirectoryName(assembly.MainModule.Image.FileInformation.FullName);
		}

		public AssemblyDefinition Assembly
		{
			get { return _assembly; }
		}
		
		public string OutputDirectory { get; set; }

		public string OutputAssemblyFile
		{
			get { return Path.Combine(OutputDirectory, Path.GetFileName(SourceAssemblyFile)); }
		}

		public void InjectAll(IEnumerable<SecurityAttributeDescriptor> descriptors)
		{
			RemoveAllSecurityAttributes();
			ApplySecurityAttributes(descriptors);
			SaveAssembly();
		}

        public void InjectAll(IEnumerable<CecilSecurityAttributeDescriptor> instructions)
        {
            RemoveAllSecurityAttributes();
            ApplySecurityAttributes(instructions);
            SaveAssembly();
        }

	    

	    public void InjectAttributeOn(ICustomAttributeProvider provider, string attributeType)
		{
            //Info("{1}: {0}", provider, attributeType);
			CustomAttributeCollection attributes = provider.CustomAttributes;
			attributes.Add(new CustomAttribute(DefaultConstructorReferenceFor(attributeType)));
		}

		public void SaveAssembly()
		{
			AssemblyFactory.SaveAssembly(_assembly, OutputAssemblyFile);
		}
		
		string SourceAssemblyFile
		{
			get { return _assembly.AssemblyPath(); }
		}

        public UserInterface UI
        {
            get; set;
        }

	    public void RemoveAllSecurityAttributes()
		{
			foreach (TypeDefinition type in _assembly.MainModule.Types)
			{
				RemoveAllSecurityAttributes(type);

				if (type.HasConstructors)
					foreach (MethodDefinition ctor in type.Constructors)
						RemoveAllSecurityAttributes(ctor);

				if (type.HasMethods)
					foreach (MethodDefinition method in type.Methods)
						RemoveAllSecurityAttributes(method);
			}
		}

		void RemoveSecurityDeclarations(IHasSecurity provider)
		{
			// also remove already existing CAS security declarations
			if (provider == null)
				return;

			if (!provider.HasSecurityDeclarations)
				return;

			provider.SecurityDeclarations.Clear();
		}

		void RemoveAllSecurityAttributes(ICustomAttributeProvider provider)
		{
			RemoveSecurityDeclarations(provider as IHasSecurity);

			if (!provider.HasCustomAttributes)
				return;

			CustomAttributeCollection attributes = provider.CustomAttributes;
			for (int i = 0; i < attributes.Count; i++)
			{
				CustomAttribute attribute = attributes[i];
				switch (attribute.Constructor.DeclaringType.FullName)
				{
					case SecurityAttributeTypeNames.SafeCritical:
					case SecurityAttributeTypeNames.Critical:
						attributes.RemoveAt(i--);
						break;
				}
			}
		}

		void ApplySecurityAttributes(IEnumerable<SecurityAttributeDescriptor> descriptors)
		{
            foreach (var descriptor in descriptors)
            {
                Info(descriptor.ToString());
                ApplySecurityAttribute(descriptor);
            }
		}
        private void ApplySecurityAttributes(IEnumerable<CecilSecurityAttributeDescriptor> instructions)
        {
            foreach(var instruction in instructions)
                InjectAttributeOn(instruction.Member, SecurityAttributeTypeNames.AttributeTypeNameFor(instruction.SecurityAttributeType));
        }

	    private void Info(string message, params object[] args)
	    {
            if (UI == null)
                return;
	        UI.Info(message, args);
	    }

	    void ApplySecurityAttribute(SecurityAttributeDescriptor descriptor)
		{
			if (descriptor.Override != SecurityAttributeOverride.None)
				throw new ArgumentException(string.Format("Security attribute overrides are not supported: {0}", descriptor));

			ICustomAttributeProvider provider = FindTargetOf(descriptor);
			if (provider == null)
			{
				Console.Error.WriteLine("Member '{0}' not found in assembly '{1}'.", descriptor.Signature, _assembly.Name);
				return;
			}

			InjectAttributeOn(provider, descriptor.AttributeTypeName);
		}

		readonly IDictionary<string, MethodReference> _ctorReferenceCache = new Dictionary<string, MethodReference>();

		private MethodReference DefaultConstructorReferenceFor(string attributeType)
		{
			MethodReference cached;
			if (_ctorReferenceCache.TryGetValue(attributeType, out cached))
				return cached;

			MethodReference newReference = NewDefaultConstructorReferenceFor(attributeType);
			_ctorReferenceCache.Add(attributeType, newReference);
			return newReference;
		}

		private MethodReference NewDefaultConstructorReferenceFor(string attributeType)
		{
			if (IsProcessingMscorlib())
				// the attributes are defined in mscorlib
				return _assembly.MainModule.Types[attributeType].Constructors.GetConstructor(false, new TypeReference[0]);
			
			var voidType = ImportVoidType();
			var typeReference = new TypeReference(SimpleName(attributeType), Namespace(attributeType), voidType.Scope, false);
			var ctorReference = new MethodReference(".ctor", typeReference, voidType, true, false, MethodCallingConvention.Default);
			_assembly.MainModule.TypeReferences.Add(typeReference);
			_assembly.MainModule.MemberReferences.Add(ctorReference);
			return ctorReference;
		}

		private string Namespace(string attributeType)
		{
			return attributeType.Substring(0, attributeType.LastIndexOf('.'));
		}

		private string SimpleName(string attributeType)
		{
			return attributeType.Substring(attributeType.LastIndexOf('.') + 1);
		}
		
		private bool IsProcessingMscorlib()
		{
			return _assembly.Name.Name.Equals("mscorlib");
		}

		private TypeReference ImportVoidType()
		{
			return _assembly.MainModule.Import(typeof(void));
		}

		ICustomAttributeProvider FindTargetOf(SecurityAttributeDescriptor descriptor)
		{
			return _cecilDefinitionFinder.FindTargetOf(descriptor);
		}

	}
}

