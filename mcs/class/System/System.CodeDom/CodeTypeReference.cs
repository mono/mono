//
// System.CodeDom CodeTypeReferenceExpression Class implementation
//
// Author:
//   Daniel Stodden (stodden@in.tum.de)
//   Marek Safar (marek.safar@seznam.cz)
//
// (C) 2001 Ximian, Inc.
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

using System.Runtime.InteropServices;
using System.Text;

namespace System.CodeDom
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeTypeReference : CodeObject
	{
		private string baseType;
		private CodeTypeReference arrayElementType;
		private int arrayRank;
		private bool isInterface;
		//bool needsFixup;

#if NET_2_0
		CodeTypeReferenceCollection typeArguments;
		CodeTypeReferenceOptions referenceOptions;
#endif

		//
		// Constructors
		//

#if NET_2_0
		public CodeTypeReference ()
		{
		}
#endif

#if NET_2_0
		[MonoTODO("We should parse basetype from right to left in 2.0 profile.")]
#endif
		public CodeTypeReference (string baseType)
		{
			Parse (baseType);
		}

#if NET_2_0
		[MonoTODO("We should parse basetype from right to left in 2.0 profile.")]
#endif
		public CodeTypeReference (Type baseType)
		{
#if NET_2_0
			if (baseType == null) {
				throw new ArgumentNullException ("baseType");
			}

			if (baseType.IsGenericParameter) {
				this.baseType = baseType.Name;
				this.referenceOptions = CodeTypeReferenceOptions.GenericTypeParameter;
			}
			else if (baseType.IsGenericTypeDefinition)
				this.baseType = baseType.FullName;
			else if (baseType.IsGenericType) {
				this.baseType = baseType.GetGenericTypeDefinition ().FullName;
				foreach (Type arg in baseType.GetGenericArguments ()) {
					if (arg.IsGenericParameter)
						TypeArguments.Add (new CodeTypeReference (new CodeTypeParameter (arg.Name)));
					else
						TypeArguments.Add (new CodeTypeReference (arg));
				}
			}
			else
#endif
			if (baseType.IsArray) {
				this.arrayRank = baseType.GetArrayRank ();
				this.arrayElementType = new CodeTypeReference (baseType.GetElementType ());
				this.baseType = arrayElementType.BaseType;
			} else {
				Parse (baseType.FullName);
			}
			this.isInterface = baseType.IsInterface;
		}

		public CodeTypeReference( CodeTypeReference arrayElementType, int arrayRank )
		{
			this.baseType = null;
			this.arrayRank = arrayRank;
			this.arrayElementType = arrayElementType;
		}

#if NET_2_0
		[MonoTODO("We should parse basetype from right to left in 2.0 profile.")]
#endif
		public CodeTypeReference( string baseType, int arrayRank )
			: this (new CodeTypeReference (baseType), arrayRank)
		{
		}

#if NET_2_0
		public CodeTypeReference( CodeTypeParameter typeParameter ) :
			this (typeParameter.Name)
		{
			this.referenceOptions = CodeTypeReferenceOptions.GenericTypeParameter;
		}

		public CodeTypeReference( string typeName, CodeTypeReferenceOptions referenceOptions ) :
			this (typeName)
		{
			this.referenceOptions = referenceOptions;
		}

		public CodeTypeReference( Type type, CodeTypeReferenceOptions referenceOptions ) :
			this (type)
		{
			this.referenceOptions = referenceOptions;
		}

		public CodeTypeReference( string typeName, params CodeTypeReference[] typeArguments ) :
			this (typeName)
		{
			TypeArguments.AddRange (typeArguments);
			if (this.baseType.IndexOf ('`') < 0)
				this.baseType += "`" + TypeArguments.Count;
		}
#endif

		//
		// Properties
		//

		public CodeTypeReference ArrayElementType
		{
			get {
				return arrayElementType;
			}
			set {
				arrayElementType = value;
			}
		}
		
		public int ArrayRank {
			get {
				return arrayRank;
			}
			set {
				arrayRank = value;
			}
		}

		public string BaseType {
			get {
				if (arrayElementType != null && arrayRank > 0) {
					return arrayElementType.BaseType;
				}

				if (baseType == null)
					return String.Empty;

				return baseType;
			}
			set {
				baseType = value;
			}
		}

		internal bool IsInterface {
			get { return isInterface; }
		}

		private void Parse (string baseType)
		{
			if (baseType == null || baseType.Length == 0) {
				this.baseType = typeof (void).FullName;
				return;
			}

#if NET_2_0
			int array_start = baseType.IndexOf ('[');
			if (array_start == -1) {
				this.baseType = baseType;
				return;
			}

			int array_end = baseType.LastIndexOf (']');
			if (array_end < array_start) {
				this.baseType = baseType;
				return;
			}

			int lastAngle = baseType.LastIndexOf ('>');
			if (lastAngle != -1 && lastAngle > array_end) {
				this.baseType = baseType;
				return;
			}
			
			string[] args = baseType.Substring (array_start + 1, array_end - array_start - 1).Split (',');

			if ((array_end - array_start) != args.Length) {
				this.baseType = baseType.Substring (0, array_start);
				int escapeCount = 0;
				int scanPos = array_start;
				StringBuilder tb = new StringBuilder();
				while (scanPos < baseType.Length) {
					char currentChar = baseType[scanPos];
					
					switch (currentChar) {
						case '[':
							if (escapeCount > 1 && tb.Length > 0) {
								tb.Append (currentChar);
							}
							escapeCount++;
							break;
						case ']':
							escapeCount--;
							if (escapeCount > 1 && tb.Length > 0) {
								tb.Append (currentChar);
							}

							if (tb.Length != 0 && (escapeCount % 2) == 0) {
								TypeArguments.Add (tb.ToString ());
								tb.Length = 0;
							}
							break;
						case ',':
							if (escapeCount > 1) {
								// skip anything after the type name until we 
								// reach the next separator
								while (scanPos + 1 < baseType.Length) {
									if (baseType[scanPos + 1] == ']') {
										break;
									}
									scanPos++;
								}
							} else if (tb.Length > 0) {
								CodeTypeReference typeArg = new CodeTypeReference (tb.ToString ());
								TypeArguments.Add (typeArg);
								tb.Length = 0;
							}
							break;
						default:
							tb.Append (currentChar);
							break;
					}
					scanPos++;
				}
			} else {
				arrayElementType = new CodeTypeReference (baseType.Substring (0, array_start));
				arrayRank = args.Length;
			}
#else
			int array_start = baseType.LastIndexOf ('[');
			if (array_start == -1) {
				this.baseType = baseType;
				return;
			}

			int array_end = baseType.LastIndexOf (']');
			if (array_end < array_start) {
				this.baseType = baseType;
				return;
			}

			string[] args = baseType.Substring (array_start + 1, array_end - array_start - 1).Split (',');

			bool isArray = true;
			foreach (string arg in args) {
				if (arg.Length != 0) {
					isArray = false;
					break;
				}
			}
			if (isArray) {
				arrayElementType = new CodeTypeReference (baseType.Substring (0, array_start));
				arrayRank = args.Length;
			} else {
				this.baseType = baseType;
			}
#endif
		}

#if NET_2_0
		[ComVisible (false)]
		public CodeTypeReferenceOptions Options {
			get {
				return referenceOptions;
			}
			set {
				referenceOptions = value;
			}
		}

		[ComVisible (false)]
		public CodeTypeReferenceCollection TypeArguments {
			get {
				if (typeArguments == null)
					typeArguments = new CodeTypeReferenceCollection ();
				return typeArguments;
			}
		}
#endif

	}
}
