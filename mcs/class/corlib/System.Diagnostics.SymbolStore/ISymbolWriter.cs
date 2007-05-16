//
// System.Diagnostics.SymbolStore.ISymbolWriter
//
// Author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (c) 2002 Duco Fijma
// 

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Reflection;

#if NET_2_0
using System.Runtime.InteropServices;
#endif

namespace System.Diagnostics.SymbolStore
{
#if NET_2_0
	[ComVisible (true)]
#endif
	public interface ISymbolWriter {
		void Close ();
		void CloseMethod ();
		void CloseNamespace ();
		void CloseScope (int endOffset);
		ISymbolDocumentWriter DefineDocument(
			string url,
			Guid language,
			Guid languageVendor,
			Guid documentType);
		void DefineField (
			SymbolToken parent,
			string name,
			FieldAttributes attributes,
			byte[] signature,
			SymAddressKind addrKind,
			int addr1,
			int addr2,
			int addr3);
		void DefineGlobalVariable (
			string name,
			FieldAttributes attributes,
			byte[] signature,
			SymAddressKind addrKind,
			int addr1,
			int addr2,
			int addr3);
		void DefineLocalVariable (
			string name,
			FieldAttributes attributes,
			byte[] signature,
			SymAddressKind addrKind,
			int addr1,
			int addr2,
			int addr3,
			int startOffset,
			int endOffset);
		void DefineParameter (
			string name,
			ParameterAttributes attributes,
			int sequence,
			SymAddressKind addrKind,
			int addr1,
			int addr2,
			int addr3);
		void DefineSequencePoints (
			ISymbolDocumentWriter document,
			int[] offsets,
			int[] lines,
			int[] columns,
			int[] endLines,
			int[] endColumns);
		void Initialize (IntPtr emitter, string filename, bool fFullBuild);
		void OpenMethod (SymbolToken method);
		void OpenNamespace (string name);
		int OpenScope (int startOffset);
		void SetMethodSourceRange (
			ISymbolDocumentWriter startDoc,
			int startLine,
			int startColumn,
			ISymbolDocumentWriter endDoc,
			int endLine,
			int endColumn);
		void SetScopeRange (int scopeID, int startOffset, int endOffset);
		void SetSymAttribute (SymbolToken parent, string name, byte[] data);
		void SetUnderlyingWriter (IntPtr underlyingWriter);
		void SetUserEntryPoint (SymbolToken entryMethod);
		void UsingNamespace (string fullName);
	}
}
