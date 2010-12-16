/*
  Copyright (C) 2010 Jeroen Frijters

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
using System;
using System.Runtime.InteropServices;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	public sealed class __StandAloneMethodSig
	{
		private readonly bool unmanaged;
		private readonly CallingConvention unmanagedCallingConvention;
		private readonly CallingConventions callingConvention;
		private readonly Type returnType;
		private readonly Type[] parameterTypes;
		private readonly Type[] optionalParameterTypes;

		internal __StandAloneMethodSig(bool unmanaged, CallingConvention unmanagedCallingConvention, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes)
		{
			this.unmanaged = unmanaged;
			this.unmanagedCallingConvention = unmanagedCallingConvention;
			this.callingConvention = callingConvention;
			this.returnType = returnType;
			this.parameterTypes = parameterTypes;
			this.optionalParameterTypes = optionalParameterTypes;
		}

		public bool IsUnmanaged
		{
			get { return unmanaged; }
		}

		public CallingConventions CallingConvention
		{
			get { return callingConvention; }
		}

		public CallingConvention UnmanagedCallingConvention
		{
			get { return unmanagedCallingConvention; }
		}

		public Type ReturnType
		{
			get { return returnType; }
		}

		public Type[] ParameterTypes
		{
			get { return Util.Copy(parameterTypes); }
		}

		public Type[] OptionalParameterTypes
		{
			get { return Util.Copy(optionalParameterTypes); }
		}
	}
}
