using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace CoreClr.Tools
{
	public class AssemblySecurityVerifier
	{
		public class ErrorEventArgs : EventArgs
		{
			public ErrorEventArgs(string message)
			{
				Message = message;
			}

			public string Message { get; private set; }
		}

		readonly AssemblyDefinition _assembly;

		public AssemblySecurityVerifier(string assemblyPath)
		{
			_assembly = AssemblyFactory.GetAssembly(assemblyPath);
		}

		public EventHandler<ErrorEventArgs> Error;

		public void Verify(IEnumerable<SecurityAttributeDescriptor> descriptors)
		{	
			var descriptorLookup = descriptors.ToLookup(d => d.Signature);

			foreach (TypeDefinition type in _assembly.MainModule.Types)
			{
				AssertDescriptorMatches(descriptorLookup, type.FullName, type);
				foreach (MethodDefinition method in type.AllMethodsAndConstructors())
					AssertDescriptorMatches(descriptorLookup, SignatureFor(method), method);
			}
		}
        public void Verify(List<CecilSecurityAttributeDescriptor> instructions)
        {
            Verify(instructions.Select(i => new SecurityAttributeDescriptor(i.SecurityAttributeType, GetTargetKind(i.Member), SignatureFor(i.Member))));
        }

	    private string SignatureFor(IMemberDefinition member)
	    {
	        var method = member as MethodDefinition;
            if (method!=null) return SignatureFor(method);

            var type = member as TypeDefinition;
            if (type != null) return type.FullName;
            
            throw new ArgumentException();
	    }

	    private TargetKind GetTargetKind(IMemberDefinition member)
	    {
            if (member is MethodDefinition) return TargetKind.Method;
            if (member is TypeDefinition) return TargetKind.Type;
            throw new ArgumentException();
	    }


	    static string SignatureFor(MethodDefinition method)
		{
			return MethodSignatureProvider.SignatureFor(method);
		}

		void AssertDescriptorMatches(ILookup<string, SecurityAttributeDescriptor> descriptors, string signature, ICustomAttributeProvider element)
		{
			if (descriptors.Contains(signature))
				AssertContainsAttribute(element, descriptors[signature].Single().AttributeTypeName);
			else
				AssertContainsNoAttribute(element);
		}

		void AssertContainsAttribute(ICustomAttributeProvider provider, string attributeType)
		{
			if (!ContainsAttribute(provider, attributeType))
				OnError(string.Format("Expecting '{0}' on '{1}'.", attributeType, provider));
		}

		void AssertContainsNoAttribute(ICustomAttributeProvider provider)
		{
			foreach (var attributeType in SecurityAttributeTypeNames.All)
				if (ContainsAttribute(provider, attributeType))
					OnError(string.Format("Unexpected attribute '{0}' on '{1}'", attributeType, provider));
		}

		private bool ContainsAttribute(ICustomAttributeProvider provider, string attributeType)
		{
			var customAttributes = from CustomAttribute a in provider.CustomAttributes
								   select a.Constructor.DeclaringType.FullName;
			return customAttributes.Contains(attributeType);
		}

		private void OnError(string failureMessage)
		{
			if (Error == null)
				throw new InvalidOperationException(failureMessage);
			Error(this, new ErrorEventArgs(failureMessage));
		}


	}
}
