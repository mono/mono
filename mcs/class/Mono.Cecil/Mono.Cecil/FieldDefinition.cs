//
// FieldDefinition.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// (C) 2005 Jb Evain
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

namespace Mono.Cecil {

	using Mono.Cecil;
	using Mono.Cecil.Binary;

	public sealed class FieldDefinition : FieldReference, IMemberDefinition,
		ICustomAttributeProvider, IHasMarshalSpec, IHasConstant {

		FieldAttributes m_attributes;

		CustomAttributeCollection m_customAttrs;

		bool m_hasInfo;
		uint m_offset;

		RVA m_rva;
		byte [] m_initVal;

		bool m_hasConstant;
		object m_const;

		MarshalSpec m_marshalDesc;

		public bool HasLayoutInfo {
			get { return m_hasInfo; }
		}

		public uint Offset {
			get { return m_offset; }
			set {
				m_hasInfo = true;
				m_offset = value;
			}
		}

		public RVA RVA {
			get { return m_rva; }
			set { m_rva = value; }
		}

		public byte [] InitialValue {
			get { return m_initVal; }
			set { m_initVal = value; }
		}

		public FieldAttributes Attributes {
			get { return m_attributes; }
			set { m_attributes = value; }
		}

		public bool HasConstant {
			get { return m_hasConstant; }
		}

		public object Constant {
			get { return m_const; }
			set {
				m_hasConstant = true;
				m_const = value;
			}
		}

		public CustomAttributeCollection CustomAttributes {
			get {
				if (m_customAttrs == null)
					m_customAttrs = new CustomAttributeCollection (this);

				return m_customAttrs;
			}
		}

		public MarshalSpec MarshalSpec {
			get { return m_marshalDesc; }
			set { m_marshalDesc = value; }
		}

		#region FieldAttributes

		public bool IsLiteral {
			get { return (m_attributes & FieldAttributes.Literal) != 0; }
			set {
				if (value)
					m_attributes |= FieldAttributes.Literal;
				else
					m_attributes &= ~FieldAttributes.Literal;
			}
		}

		public bool IsReadOnly {
			get { return (m_attributes & FieldAttributes.InitOnly) != 0; }
			set {
				if (value)
					m_attributes |= FieldAttributes.InitOnly;
				else
					m_attributes &= ~FieldAttributes.InitOnly;
			}
		}

		public bool IsRuntimeSpecialName {
			get { return (m_attributes & FieldAttributes.RTSpecialName) != 0; }
			set {
				if (value)
					m_attributes |= FieldAttributes.RTSpecialName;
				else
					m_attributes &= ~FieldAttributes.RTSpecialName;
			}
		}

		public bool IsSpecialName {
			get { return (m_attributes & FieldAttributes.SpecialName) != 0; }
			set {
				if (value)
					m_attributes |= FieldAttributes.SpecialName;
				else
					m_attributes &= ~FieldAttributes.SpecialName;
			}
		}

		public bool IsStatic {
			get { return (m_attributes & FieldAttributes.Static) != 0; }
			set {
				if (value)
					m_attributes |= FieldAttributes.Static;
				else
					m_attributes &= ~FieldAttributes.Static;
			}
		}

		#endregion

		public FieldDefinition (string name, TypeReference fieldType,
			FieldAttributes attrs) : base (name, fieldType)
		{
			m_attributes = attrs;
		}

		public FieldDefinition Clone ()
		{
			return Clone (this, new ImportContext (NullReferenceImporter.Instance, this.DeclaringType));
		}

		internal static FieldDefinition Clone (FieldDefinition field, ImportContext context)
		{
			FieldDefinition nf = new FieldDefinition (
				field.Name,
				context.Import (field.FieldType),
				field.Attributes);

			if (field.HasConstant)
				nf.Constant = field.Constant;
			if (field.MarshalSpec != null)
				nf.MarshalSpec = field.MarshalSpec;
			if (field.RVA != RVA.Zero)
				nf.InitialValue = field.InitialValue;
			else
				nf.InitialValue = new byte [0];
			if (field.HasLayoutInfo)
				nf.Offset = field.Offset;

			foreach (CustomAttribute ca in field.CustomAttributes)
				nf.CustomAttributes.Add (CustomAttribute.Clone (ca, context));

			return nf;
		}

		public override void Accept (IReflectionVisitor visitor)
		{
			visitor.VisitFieldDefinition (this);

			if (this.MarshalSpec != null)
				this.MarshalSpec.Accept (visitor);

			this.CustomAttributes.Accept (visitor);
		}
	}
}
