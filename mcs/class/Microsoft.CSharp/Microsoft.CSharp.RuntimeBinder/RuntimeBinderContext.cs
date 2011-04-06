//
// RuntimeBinderContext.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using Compiler = Mono.CSharp;

namespace Microsoft.CSharp.RuntimeBinder
{
	sealed class RuntimeBinderContext : Compiler.IMemberContext
	{
		readonly Compiler.ModuleContainer module;
		readonly Type callingType;
		readonly DynamicContext ctx;
		Compiler.TypeSpec callingTypeImported;

		public RuntimeBinderContext (DynamicContext ctx, Compiler.TypeSpec callingType)
		{
			this.ctx = ctx;
			this.module = ctx.Module;
			this.callingTypeImported = callingType;
		}

		public RuntimeBinderContext (DynamicContext ctx, Type callingType)
		{
			this.ctx = ctx;
			this.module = ctx.Module;
			this.callingType = callingType;
		}

		#region IMemberContext Members

		public Compiler.TypeSpec CurrentType {
			get {
				//
				// Delay importing of calling type to be compatible with .net
				// Some libraries are setting it to null which is invalid
				// but the NullReferenceException is thrown only when the context
				// is used and not during initialization
				//
				if (callingTypeImported == null && callingType != null)
					callingTypeImported = ctx.ImportType (callingType);

				return callingTypeImported;
			}
		}

		public Compiler.TypeParameter[] CurrentTypeParameters {
			get { throw new NotImplementedException (); }
		}

		public Compiler.MemberCore CurrentMemberDefinition {
			get {
				return null;
			}
		}

		public bool HasUnresolvedConstraints {
			get {
				return false;
			}
		}

		public bool IsObsolete {
			get {
				// Always true to ignore obsolete attribute checks
				return true;
			}
		}

		public bool IsUnsafe {
			get {
				// Dynamic cannot be used with pointers
				return false;
			}
		}

		public bool IsStatic {
			get {
				throw new NotImplementedException ();
			}
		}

		public Compiler.ModuleContainer Module {
			get {
				return module;
			}
		}

		public string GetSignatureForError ()
		{
			throw new NotImplementedException ();
		}

		public IList<Compiler.MethodSpec> LookupExtensionMethod (Compiler.TypeSpec extensionType, string name, int arity, ref Compiler.NamespaceContainer scope)
		{
			// No extension method lookup in this context
			return null;
		}

		public Compiler.FullNamedExpression LookupNamespaceOrType (string name, int arity, Mono.CSharp.LookupMode mode, Mono.CSharp.Location loc)
		{
			throw new NotImplementedException ();
		}

		public Compiler.FullNamedExpression LookupNamespaceAlias (string name)
		{
			// No namespace aliases in this context
			return null;
		}

		#endregion
	}
}
