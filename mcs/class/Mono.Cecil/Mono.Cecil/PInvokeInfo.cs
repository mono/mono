//
// PInvokeInfo.cs
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

	public sealed class PInvokeInfo : IReflectionVisitable {

		MethodDefinition m_meth;

		PInvokeAttributes m_attributes;
		string m_entryPoint;
		ModuleReference m_module;

		public MethodDefinition Method {
			get { return m_meth; }
		}

		public PInvokeAttributes Attributes {
			get { return m_attributes; }
			set { m_attributes = value; }
		}

		public string EntryPoint {
			get { return m_entryPoint; }
			set { m_entryPoint = value; }
		}

		public ModuleReference Module {
			get { return m_module; }
			set { m_module = value; }
		}

		#region PInvokeAttributes

		public bool IsNoMangle {
			get { return (m_attributes & PInvokeAttributes.NoMangle) != 0; }
			set {
				if (value)
					m_attributes |= PInvokeAttributes.NoMangle;
				else
					m_attributes &= ~PInvokeAttributes.NoMangle;
			}
		}

		public bool IsCharSetNotSpec {
			get { return (m_attributes & PInvokeAttributes.CharSetNotSpec) != 0; }
			set {
				PInvokeAttributes masked = (PInvokeAttributes.CharSetMask & PInvokeAttributes.CharSetNotSpec)
				if (value)
					m_attributes |= masked;
				else
					m_attributes &= masked;
			}
		}

		public bool IsCharSetAnsi {
			get { return (m_attributes & PInvokeAttributes.CharSetAnsi) != 0; }
			set {
				PInvokeAttributes masked = (PInvokeAttributes.CharSetMask & PInvokeAttributes.CharSetAnsi)
				if (value)
					m_attributes |= masked;
				else
					m_attributes &= masked;
			}
		}

		public bool IsCharSetUnicode {
			get { return (m_attributes & PInvokeAttributes.CharSetUnicode) != 0; }
			set {
				PInvokeAttributes masked = (PInvokeAttributes.CharSetMask & PInvokeAttributes.CharSetUnicode)
				if (value)
					m_attributes |= masked;
				else
					m_attributes &= masked;
			}
		}

		public bool IsCharSetAuto {
			get { return (m_attributes & PInvokeAttributes.CharSetAuto) != 0; }
			set {
				PInvokeAttributes masked = (PInvokeAttributes.CharSetMask & PInvokeAttributes.CharSetAuto)
				if (value)
					m_attributes |= masked;
				else
					m_attributes &= masked;
			}
		}

		public bool SupportsLastError {
			get { return (m_attributes & PInvokeAttributes.SupportsLastError) != 0; }
			set {
				PInvokeAttributes masked = (PInvokeAttributes.CharSetMask & PInvokeAttributes.SupportsLastError)
				if (value)
					m_attributes |= masked;
				else
					m_attributes &= masked;
			}
		}

		public bool IsCallConvWinapi {
			get { return (m_attributes & PInvokeAttributes.CallConvWinapi) != 0; }
			set {
				PInvokeAttributes masked = (PInvokeAttributes.CallConvMask & PInvokeAttributes.CallConvWinapi)
				if (value)
					m_attributes |= masked;
				else
					m_attributes &= masked;
			}
		}

		public bool IsCallConvCdecl {
			get { return (m_attributes & PInvokeAttributes.CallConvCdecl) != 0; }
			set {
				PInvokeAttributes masked = (PInvokeAttributes.CallConvMask & PInvokeAttributes.CallConvCdecl)
				if (value)
					m_attributes |= masked;
				else
					m_attributes &= masked;
			}
		}

		public bool IsCallConvStdCall {
			get { return (m_attributes & PInvokeAttributes.CallConvStdCall) != 0; }
			set {
				PInvokeAttributes masked = (PInvokeAttributes.CallConvMask & PInvokeAttributes.CallConvStdCall)
				if (value)
					m_attributes |= masked;
				else
					m_attributes &= masked;
			}
		}

		public bool IsCallConvThiscall {
			get { return (m_attributes & PInvokeAttributes.CallConvThiscall) != 0; }
			set {
				PInvokeAttributes masked = (PInvokeAttributes.CallConvMask & PInvokeAttributes.CallConvThiscall)
				if (value)
					m_attributes |= masked;
				else
					m_attributes &= masked;
			}
		}

		public bool IsCallConvFastcall {
			get { return (m_attributes & PInvokeAttributes.CallConvFastcall) != 0; }
			set {
				PInvokeAttributes masked = (PInvokeAttributes.CallConvMask & PInvokeAttributes.CallConvFastcall)
				if (value)
					m_attributes |= masked;
				else
					m_attributes &= masked;
			}
		}

		#endregion

		public PInvokeInfo (MethodDefinition meth)
		{
			m_meth = meth;
		}

		public PInvokeInfo (MethodDefinition meth, PInvokeAttributes attrs,
			string entryPoint, ModuleReference mod) : this (meth)
		{
			m_attributes = attrs;
			m_entryPoint = entryPoint;
			m_module = mod;
		}

		public void Accept (IReflectionVisitor visitor)
		{
			visitor.VisitPInvokeInfo (this);
		}
	}
}
