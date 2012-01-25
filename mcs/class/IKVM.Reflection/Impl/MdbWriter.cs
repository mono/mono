/*
  Copyright (C) 2009 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
#if MONO
using System;
using System.Collections.Generic;
using Mono.CompilerServices.SymbolWriter;
using IKVM.Reflection.Emit;

namespace IKVM.Reflection.Impl
{
	sealed class Method : IMethodDef
	{
		internal int token;
		internal string name;
		internal SymbolDocumentWriter document;
		internal int[] offsets;
		internal int[] lines;
		internal int[] columns;
		internal List<string> variables = new List<string>();

		public string Name
		{
			get { return name; }
		}

		public int Token
		{
			get { return token; }
		}
	}

	sealed class SymbolDocumentWriter : System.Diagnostics.SymbolStore.ISymbolDocumentWriter
	{
		internal readonly string url;
		internal SourceFileEntry source;

		internal SymbolDocumentWriter(string url)
		{
			this.url = url;
		}

		public void SetCheckSum(Guid algorithmId, byte[] checkSum)
		{
		}

		public void SetSource(byte[] source)
		{
		}
	}

	sealed class MdbWriter : ISymbolWriterImpl
	{
		private readonly ModuleBuilder moduleBuilder;
		private readonly Dictionary<int, Method> methods = new Dictionary<int, Method>();
		private readonly Dictionary<string, SymbolDocumentWriter> documents = new Dictionary<string, SymbolDocumentWriter>();
		private Method currentMethod;

		internal MdbWriter(ModuleBuilder moduleBuilder)
		{
			this.moduleBuilder = moduleBuilder;
		}

		public byte[] GetDebugInfo(ref IMAGE_DEBUG_DIRECTORY idd)
		{
			return Empty<byte>.Array;
		}

		public void RemapToken(int oldToken, int newToken)
		{
			if (methods.ContainsKey(oldToken))
			{
				methods[oldToken].token = newToken;
			}
		}

		public void Close()
		{
			MonoSymbolWriter writer = new MonoSymbolWriter(moduleBuilder.FullyQualifiedName);

			foreach (Method method in methods.Values)
			{
				if (method.document != null)
				{
					if (method.document.source == null)
					{
						method.document.source = new SourceFileEntry(writer.SymbolFile, method.document.url);
					}
					ICompileUnit file = new CompileUnitEntry(writer.SymbolFile, method.document.source);
					SourceMethodBuilder smb = writer.OpenMethod(file, 0, method);
					for (int i = 0; i < method.offsets.Length; i++)
					{
						smb.MarkSequencePoint(method.offsets[i], method.document.source, method.lines[i], method.columns[i], false);
					}
					for (int i = 0; i < method.variables.Count; i++)
					{
						writer.DefineLocalVariable(i, method.variables[i]);
					}
					writer.CloseMethod();
				}
			}

			writer.WriteSymbolFile(moduleBuilder.ModuleVersionId);
		}

		public System.Diagnostics.SymbolStore.ISymbolDocumentWriter DefineDocument(string url, Guid language, Guid languageVendor, Guid documentType)
		{
			SymbolDocumentWriter writer;
			if (!documents.TryGetValue(url, out writer))
			{
				writer = new SymbolDocumentWriter(url);
				documents.Add(url, writer);
			}
			return writer;
		}

		public void OpenMethod(System.Diagnostics.SymbolStore.SymbolToken method)
		{
			throw new NotImplementedException();
		}

		public void OpenMethod(System.Diagnostics.SymbolStore.SymbolToken token, MethodBase mb)
		{
			Method method = new Method();
			method.token = token.GetToken();
			method.name = mb.Name;
			methods.Add(token.GetToken(), method);
			currentMethod = method;
		}

		public void CloseMethod()
		{
			currentMethod = null;
		}

		public void DefineLocalVariable(string name, System.Reflection.FieldAttributes attributes, byte[] signature, System.Diagnostics.SymbolStore.SymAddressKind addrKind, int addr1, int addr2, int addr3, int startOffset, int endOffset)
		{
		}

		public void DefineLocalVariable2(string name, FieldAttributes attributes, int signature, System.Diagnostics.SymbolStore.SymAddressKind addrKind, int addr1, int addr2, int addr3, int startOffset, int endOffset)
		{
			currentMethod.variables.Add(name);
		}

		public void DefineSequencePoints(System.Diagnostics.SymbolStore.ISymbolDocumentWriter document, int[] offsets, int[] lines, int[] columns, int[] endLines, int[] endColumns)
		{
			currentMethod.document = (SymbolDocumentWriter)document;
			currentMethod.offsets = offsets;
			currentMethod.lines = lines;
			currentMethod.columns = columns;
		}

		public void DefineParameter(string name, System.Reflection.ParameterAttributes attributes, int sequence, System.Diagnostics.SymbolStore.SymAddressKind addrKind, int addr1, int addr2, int addr3)
		{
		}

		public void DefineField(System.Diagnostics.SymbolStore.SymbolToken parent, string name, System.Reflection.FieldAttributes attributes, byte[] signature, System.Diagnostics.SymbolStore.SymAddressKind addrKind, int addr1, int addr2, int addr3)
		{
		}

		public void DefineGlobalVariable(string name, System.Reflection.FieldAttributes attributes, byte[] signature, System.Diagnostics.SymbolStore.SymAddressKind addrKind, int addr1, int addr2, int addr3)
		{
		}

		public void OpenNamespace(string name)
		{
		}

		public void CloseNamespace()
		{
		}

		public void UsingNamespace(string fullName)
		{
		}

		public int OpenScope(int startOffset)
		{
			return 0;
		}

		public void CloseScope(int endOffset)
		{
		}

		public void SetMethodSourceRange(System.Diagnostics.SymbolStore.ISymbolDocumentWriter startDoc, int startLine, int startColumn, System.Diagnostics.SymbolStore.ISymbolDocumentWriter endDoc, int endLine, int endColumn)
		{
		}

		public void SetScopeRange(int scopeID, int startOffset, int endOffset)
		{
		}

		public void SetSymAttribute(System.Diagnostics.SymbolStore.SymbolToken parent, string name, byte[] data)
		{
		}

		public void SetUserEntryPoint(System.Diagnostics.SymbolStore.SymbolToken entryMethod)
		{
		}

		public void SetUnderlyingWriter(IntPtr underlyingWriter)
		{
			throw new InvalidOperationException();
		}

		public void Initialize(IntPtr emitter, string filename, bool fFullBuild)
		{
			throw new InvalidOperationException();
		}
	}
}
#endif // MONO
