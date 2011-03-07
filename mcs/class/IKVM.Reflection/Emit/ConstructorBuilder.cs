/*
  Copyright (C) 2008 Jeroen Frijters

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

namespace IKVM.Reflection.Emit
{
	public sealed class ConstructorBuilder : ConstructorInfo
	{
		private readonly MethodBuilder methodBuilder;

		internal ConstructorBuilder(MethodBuilder mb)
		{
			this.methodBuilder = mb;
		}

		public override bool Equals(object obj)
		{
			ConstructorBuilder other = obj as ConstructorBuilder;
			return other != null && other.methodBuilder.Equals(methodBuilder);
		}

		public override int GetHashCode()
		{
			return methodBuilder.GetHashCode();
		}

		public void __SetSignature(Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			methodBuilder.SetSignature(returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
		}

		public ParameterBuilder DefineParameter(int position, ParameterAttributes attributes, string strParamName)
		{
			return methodBuilder.DefineParameter(position, attributes, strParamName);
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			methodBuilder.SetCustomAttribute(customBuilder);
		}

		public void SetCustomAttribute(ConstructorInfo con,	byte[] binaryAttribute)
		{
			methodBuilder.SetCustomAttribute(con, binaryAttribute);
		}

		public void __AddDeclarativeSecurity(CustomAttributeBuilder customBuilder)
		{
			methodBuilder.__AddDeclarativeSecurity(customBuilder);
		}

		public void AddDeclarativeSecurity(System.Security.Permissions.SecurityAction securityAction, System.Security.PermissionSet permissionSet)
		{
			methodBuilder.AddDeclarativeSecurity(securityAction, permissionSet);
		}

		public void SetImplementationFlags(MethodImplAttributes attributes)
		{
			methodBuilder.SetImplementationFlags(attributes);
		}

		public ILGenerator GetILGenerator()
		{
			return methodBuilder.GetILGenerator();
		}

		public ILGenerator GetILGenerator(int streamSize)
		{
			return methodBuilder.GetILGenerator(streamSize);
		}

		public void __ReleaseILGenerator()
		{
			methodBuilder.__ReleaseILGenerator();
		}

		public override CallingConventions CallingConvention
		{
			get { return methodBuilder.CallingConvention; }
		}

		public override MethodAttributes Attributes
		{
			get { return methodBuilder.Attributes; }
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return methodBuilder.GetMethodImplementationFlags();
		}

		public Type ReturnType
		{
			get { return methodBuilder.ReturnType; }
		}

		internal override int ParameterCount
		{
			get { return methodBuilder.ParameterCount; }
		}

		public override Type DeclaringType
		{
			get { return methodBuilder.DeclaringType; }
		}

		public override string Name
		{
			get { return methodBuilder.Name; }
		}

		public override int MetadataToken
		{
			get { return methodBuilder.MetadataToken; }
		}

		public override Module Module
		{
			get { return methodBuilder.Module; }
		}

		public Module GetModule()
		{
			return methodBuilder.GetModule();
		}

		public MethodToken GetToken()
		{
			return methodBuilder.GetToken();
		}

		public override MethodBody GetMethodBody()
		{
			return methodBuilder.GetMethodBody();
		}

		public bool InitLocals
		{
			get { return methodBuilder.InitLocals; }
			set { methodBuilder.InitLocals = value; }
		}

		internal override MethodInfo GetMethodInfo()
		{
			return methodBuilder;
		}

		internal override MethodInfo GetMethodOnTypeDefinition()
		{
			return methodBuilder;
		}

		internal override MethodSignature MethodSignature
		{
			get { return methodBuilder.MethodSignature; }
		}

		internal override int ImportTo(ModuleBuilder module)
		{
			return module.ImportMember(methodBuilder);
		}
	}
}
