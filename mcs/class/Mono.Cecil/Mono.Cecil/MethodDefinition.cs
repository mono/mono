//
// MethodDefinition.cs
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

	using Mono.Cecil.Binary;
	using Mono.Cecil.Cil;

	public sealed class MethodDefinition : MethodReference, IMemberDefinition,
		IHasSecurity, ICustomAttributeProvider {

		public const string Cctor = ".cctor";
		public const string Ctor = ".ctor";

		MethodAttributes m_attributes;
		MethodImplAttributes m_implAttrs;
		MethodSemanticsAttributes m_semAttrs;
		SecurityDeclarationCollection m_secDecls;
		CustomAttributeCollection m_customAttrs;

		ModuleDefinition m_module;

		MethodBody m_body;
		RVA m_rva;
		OverrideCollection m_overrides;
		PInvokeInfo m_pinvoke;
		readonly ParameterDefinition m_this;

		public MethodAttributes Attributes {
			get { return m_attributes; }
			set { m_attributes = value; }
		}

		public MethodImplAttributes ImplAttributes {
			get { return m_implAttrs; }
			set { m_implAttrs = value; }
		}

		public MethodSemanticsAttributes SemanticsAttributes {
			get { return m_semAttrs; }
			set { m_semAttrs = value; }
		}

		public override TypeReference DeclaringType {
			get { return base.DeclaringType; }
			set {
				base.DeclaringType = value;
				TypeDefinition t = value as TypeDefinition;
				if (t != null)
					m_module = t.Module;
			}
		}

		public SecurityDeclarationCollection SecurityDeclarations {
			get {
				if (m_secDecls == null)
					m_secDecls = new SecurityDeclarationCollection (this);

				return m_secDecls;
			}
		}

		public CustomAttributeCollection CustomAttributes {
			get {
				if (m_customAttrs == null)
					m_customAttrs = new CustomAttributeCollection (this);

				return m_customAttrs;
			}
		}

		public RVA RVA {
			get { return m_rva; }
			set { m_rva = value; }
		}

		public MethodBody Body {
			get {
				LoadBody ();
				return m_body;
			}
			set { m_body = value; }
		}

		public PInvokeInfo PInvokeInfo {
			get { return m_pinvoke; }
			set { m_pinvoke = value; }
		}

		public OverrideCollection Overrides {
			get {
				if (m_overrides == null)
					m_overrides = new OverrideCollection (this);

				return m_overrides;
			}
		}

		public ParameterDefinition This {
			get { return m_this; }
		}

		public bool IsAbstract {
			get { return (m_attributes & MethodAttributes.Abstract) != 0; }
			set {
				if (value)
					m_attributes |= MethodAttributes.Abstract;
				else
					m_attributes &= ~MethodAttributes.Abstract;
			}
		}

		public bool IsFinal {
			get { return (m_attributes & MethodAttributes.Final) != 0; }
			set {
				if (value)
					m_attributes |= MethodAttributes.Final;
				else
					m_attributes &= ~MethodAttributes.Final;
			}
		}

		public bool IsHideBySignature {
			get { return (m_attributes & MethodAttributes.HideBySig) != 0; }
			set {
				if (value)
					m_attributes |= MethodAttributes.HideBySig;
				else
					m_attributes &= ~MethodAttributes.HideBySig;
			}
		}

		public bool IsNewSlot {
			get { return (m_attributes & MethodAttributes.VtableLayoutMask) == MethodAttributes.NewSlot; }
			set {
				if (value)
					m_attributes |= (MethodAttributes.VtableLayoutMask & MethodAttributes.NewSlot);
				else
					m_attributes &= ~(MethodAttributes.VtableLayoutMask & MethodAttributes.NewSlot);
			}
		}

		public bool IsRuntimeSpecialName {
			get { return (m_attributes & MethodAttributes.RTSpecialName) != 0; }
			set {
				if (value)
					m_attributes |= MethodAttributes.RTSpecialName;
				else
					m_attributes &= ~MethodAttributes.RTSpecialName;
			}
		}

		public bool IsInternalCall {
			get { return MethodImplAttributes.InternalCall == (m_implAttrs & MethodImplAttributes.InternalCall); }
			set {
				if (value)
					m_implAttrs |= MethodImplAttributes.InternalCall;
				else
					m_implAttrs &= ~MethodImplAttributes.InternalCall;
			}
		}

		public bool IsRuntime {
			get { return MethodImplAttributes.Runtime == (m_implAttrs & MethodImplAttributes.Runtime); }
			set {
				if (value)
					m_implAttrs |= MethodImplAttributes.Runtime;
				else
					m_implAttrs &= ~MethodImplAttributes.Runtime;
			}
		}

		public bool IsSpecialName {
			get { return (m_attributes & MethodAttributes.SpecialName) != 0; }
			set {
				if (value)
					m_attributes |= MethodAttributes.SpecialName;
				else
					m_attributes &= ~MethodAttributes.SpecialName;
			}
		}

		public bool IsStatic {
			get { return (m_attributes & MethodAttributes.Static) != 0; }
			set {
				if (value)
					m_attributes |= MethodAttributes.Static;
				else
					m_attributes &= ~MethodAttributes.Static;
				this.HasThis = !value;
			}
		}

		public bool IsVirtual {
			get { return (m_attributes & MethodAttributes.Virtual) != 0; }
			set {
				if (value)
					m_attributes |= MethodAttributes.Virtual;
				else
					m_attributes &= ~MethodAttributes.Virtual;
			}
		}

		public bool IsConstructor {
			get {
				return this.IsRuntimeSpecialName && this.IsSpecialName &&
					(this.Name == Cctor || this.Name == Ctor);
			}
		}

		public bool HasBody {
			get {
				return (m_attributes & MethodAttributes.Abstract) == 0 &&
					(m_attributes & MethodAttributes.PInvokeImpl) == 0 &&
					(m_implAttrs & MethodImplAttributes.InternalCall) == 0 &&
					(m_implAttrs & MethodImplAttributes.Native) == 0 &&
					(m_implAttrs & MethodImplAttributes.Unmanaged) == 0 &&
					(m_implAttrs & MethodImplAttributes.Runtime) == 0;
			}
		}

		public MethodDefinition (string name, RVA rva,
			MethodAttributes attrs, MethodImplAttributes implAttrs,
			bool hasThis, bool explicitThis, MethodCallingConvention callConv) :
			base (name, hasThis, explicitThis, callConv)
		{
			m_rva = rva;
			m_attributes = attrs;
			m_implAttrs = implAttrs;

			if (!IsStatic)
				m_this = new ParameterDefinition ("this", 0, (ParameterAttributes) 0, null);
		}

		internal MethodDefinition (string name, MethodAttributes attrs) : base (name)
		{
			m_attributes = attrs;

			this.HasThis = !this.IsStatic;
			if (!IsStatic)
				m_this = new ParameterDefinition ("this", 0, (ParameterAttributes) 0, null);
		}

		public MethodDefinition (string name, MethodAttributes attrs, TypeReference returnType) :
			this (name, attrs)
		{
			this.ReturnType.ReturnType = returnType;
		}

		internal void LoadBody ()
		{
			if (m_body == null && this.HasBody) {
				m_body = new MethodBody (this);
				if (m_module != null && m_rva != RVA.Zero)
					m_module.Controller.Reader.Code.VisitMethodBody (m_body);
			}
		}

		public MethodDefinition Clone ()
		{
			return Clone (this, new ImportContext (NullReferenceImporter.Instance, this));
		}

		internal static MethodDefinition Clone (MethodDefinition meth, ImportContext context)
		{
			MethodDefinition nm = new MethodDefinition (
				meth.Name,
				RVA.Zero,
				meth.Attributes,
				meth.ImplAttributes,
				meth.HasThis,
				meth.ExplicitThis,
				meth.CallingConvention);

			context.GenericContext.Method = nm;

			foreach (GenericParameter p in meth.GenericParameters)
				nm.GenericParameters.Add (GenericParameter.Clone (p, context));

			nm.ReturnType.ReturnType = context.Import (meth.ReturnType.ReturnType);

			if (meth.ReturnType.HasConstant)
				nm.ReturnType.Constant = meth.ReturnType.Constant;

			if (meth.ReturnType.MarshalSpec != null)
				nm.ReturnType.MarshalSpec = meth.ReturnType.MarshalSpec;

			foreach (CustomAttribute ca in meth.ReturnType.CustomAttributes)
				nm.ReturnType.CustomAttributes.Add (CustomAttribute.Clone (ca, context));

			if (meth.PInvokeInfo != null)
				nm.PInvokeInfo = meth.PInvokeInfo; // TODO: import module ?
			foreach (ParameterDefinition param in meth.Parameters)
				nm.Parameters.Add (ParameterDefinition.Clone (param, context));
			foreach (MethodReference ov in meth.Overrides)
				nm.Overrides.Add (context.Import (ov));
			foreach (CustomAttribute ca in meth.CustomAttributes)
				nm.CustomAttributes.Add (CustomAttribute.Clone (ca, context));
			foreach (SecurityDeclaration sec in meth.SecurityDeclarations)
				nm.SecurityDeclarations.Add (SecurityDeclaration.Clone (sec));

			if (meth.Body != null)
				nm.Body = MethodBody.Clone (meth.Body, nm, context);

			return nm;
		}

		public override void Accept (IReflectionVisitor visitor)
		{
			visitor.VisitMethodDefinition (this);

			this.GenericParameters.Accept (visitor);
			this.Parameters.Accept (visitor);

			if (this.PInvokeInfo != null)
				this.PInvokeInfo.Accept (visitor);

			this.SecurityDeclarations.Accept (visitor);
			this.Overrides.Accept (visitor);
			this.CustomAttributes.Accept (visitor);
		}
	}
}
