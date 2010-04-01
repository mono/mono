//
// System.CodeDom.Compiler CompilerInfo class
//
// Author:
// 	Marek Safar (marek.safar@seznam.cz)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (c) 2004,2005 Novell, Inc. (http://www.novell.com)
//

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

#if NET_2_0

using System.Configuration;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Permissions;

namespace System.CodeDom.Compiler {

	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	public sealed class CompilerInfo
	{
		internal string Languages;
		internal string Extensions;
		internal string TypeName;
		internal int WarningLevel;
		internal string CompilerOptions;
		internal Dictionary <string, string> ProviderOptions;
		
		bool inited;
		Type type;

		internal CompilerInfo ()
		{
		}

		internal void Init ()
		{
			if (inited)
				return;

			inited = true;
			type = Type.GetType (TypeName);
			if (type == null)
				return;

			if (!typeof (CodeDomProvider).IsAssignableFrom (type))
				type = null;
		}

		public Type CodeDomProviderType {
			get {
				if (type == null) {
					type = Type.GetType (TypeName, false);
#if CONFIGURATION_DEP
					if (type == null)
						throw new ConfigurationErrorsException ("Unable to locate compiler type '" + TypeName + "'");
#endif
				}
				
				return type;
			}
		}

		
		public bool IsCodeDomProviderTypeValid {
			get { return type != null; }
		}

		public CompilerParameters CreateDefaultCompilerParameters ()
		{
			CompilerParameters cparams = new CompilerParameters ();
			if (CompilerOptions == null)
				cparams.CompilerOptions = String.Empty;
			else
				cparams.CompilerOptions = CompilerOptions;
			cparams.WarningLevel = WarningLevel;

			return cparams;
		}

		public CodeDomProvider CreateProvider ()
		{
			return CreateProvider (ProviderOptions);
		}

#if NET_4_0
		public		
#endif
		CodeDomProvider CreateProvider (IDictionary<string, string> providerOptions)
		{
			Type providerType = CodeDomProviderType;
			if (providerOptions != null && providerOptions.Count > 0) {
				ConstructorInfo ctor = providerType.GetConstructor (new [] { typeof (IDictionary <string, string>) });
				if (ctor != null)
					return (CodeDomProvider) ctor.Invoke (new object[] { providerOptions });
			}
			
			return (CodeDomProvider) Activator.CreateInstance (providerType);
		}

		public override bool Equals (object o)
		{
			if (!(o is CompilerInfo))
				return false;

			CompilerInfo c = (CompilerInfo) o;
			return c.TypeName == TypeName;
		}

		public override int GetHashCode ()
		{
			return TypeName.GetHashCode ();
		}

		public string [] GetExtensions ()
		{
			return Extensions.Split (';');
		}

		public string [] GetLanguages ()
		{
			return Languages.Split (';');
		}
	}
}
#endif

