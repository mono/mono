//
// ILGenerator.pns.cs
//
// Authors:
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
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

#if !MONO_FEATURE_SRE

namespace System.Reflection.Emit
{
	public class ILGenerator
	{
		ILGenerator ()
		{
		}

		public int ILOffset {
			get	{
				throw new PlatformNotSupportedException ();
			}
		}

		public virtual void BeginCatchBlock (Type exceptionType)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void BeginExceptFilterBlock ()
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual Label BeginExceptionBlock ()
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void BeginFaultBlock ()
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void BeginFinallyBlock ()
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void BeginScope ()
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual LocalBuilder DeclareLocal (Type localType)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual LocalBuilder DeclareLocal (Type localType, bool pinned)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual Label DefineLabel ()
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void Emit (OpCode opcode)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void Emit (OpCode opcode, byte arg)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void Emit (OpCode opcode, double arg)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void Emit (OpCode opcode, short arg)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void Emit (OpCode opcode, int arg)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void Emit (OpCode opcode, long arg)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void Emit (OpCode opcode, ConstructorInfo con)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void Emit (OpCode opcode, Label label)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void Emit (OpCode opcode, Label[] labels)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void Emit (OpCode opcode, LocalBuilder local)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void Emit (OpCode opcode, SignatureHelper signature)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void Emit (OpCode opcode, FieldInfo field)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void Emit (OpCode opcode, MethodInfo meth)
		{
			throw new PlatformNotSupportedException ();
		}

		[CLSCompliant (false)]
		public void Emit (OpCode opcode, sbyte arg)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void Emit (OpCode opcode, float arg)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void Emit (OpCode opcode, string str)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void Emit (OpCode opcode, Type cls)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void EmitCall (OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void EmitCalli (OpCode opcode, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void EmitWriteLine (LocalBuilder localBuilder)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void EmitWriteLine (FieldInfo fld)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void EmitWriteLine (string value)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void EndExceptionBlock ()
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void EndScope ()
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void MarkLabel (Label loc)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void ThrowException (Type excType)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual void UsingNamespace (string usingNamespace)
		{
			throw new PlatformNotSupportedException ();
		}

	}
}

#endif
