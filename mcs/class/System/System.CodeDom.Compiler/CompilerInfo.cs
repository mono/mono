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
namespace System.CodeDom.Compiler
{
	public sealed class CompilerInfo
	{
		internal string Languages;
		internal string Extensions;
		internal string TypeName;
		internal int WarningLevel;
		internal string CompilerOptions;
		bool inited;
		Type type;

		internal CompilerInfo ()
		{
		}

		void Init ()
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
			get { return type; }
		}

		public bool IsCodeDomProviderTypeValid {
			get { return type != null; }
		}

		public CodeDomProvider CreateProvider ()
		{
			return (CodeDomProvider) Activator.CreateInstance (type);
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

