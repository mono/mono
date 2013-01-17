// 
// Field.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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

using Mono.Cecil;

namespace Mono.CodeContracts.Static.AST {
	class Field : Member {
		private readonly FieldDefinition definition;

		public Field (FieldDefinition definition) : base (NodeType.Field)
		{
			this.definition = definition;
		}

		#region Overrides of Member
		public override bool IsStatic
		{
			get { return this.definition.IsStatic; }
		}

		public TypeNode FieldType
		{
			get { return TypeNode.Create (this.definition.FieldType); }
		}

		public string Name
		{
			get { return this.definition.Name; }
		}

		public override TypeNode DeclaringType
		{
			get { return TypeNode.Create (this.definition.DeclaringType); }
		}

		public override Module Module
		{
			get { return new Module (this.definition.Module); }
		}

		public override bool IsPublic
		{
			get { return this.definition.IsPublic; }
		}

		public override bool IsPrivate
		{
			get { return this.definition.IsPrivate; }
		}

		public override bool IsAssembly
		{
			get { return this.definition.IsAssembly; }
		}

		public override bool IsFamily
		{
			get { return this.definition.IsFamily; }
		}

		public override bool IsFamilyOrAssembly
		{
			get { return this.definition.IsFamilyOrAssembly; }
		}

		public override bool IsFamilyAndAssembly
		{
			get { return this.definition.IsFamilyAndAssembly; }
		}

		public bool IsReadonly
		{
			get { return this.definition.IsInitOnly; }
		}

		public bool IsCompilerGenerated
		{
			get { return this.definition.IsCompilerControlled; }
		}
		#endregion
	}
}
