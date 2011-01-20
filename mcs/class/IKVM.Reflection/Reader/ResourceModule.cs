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
using System;
using System.Collections.Generic;

namespace IKVM.Reflection.Reader
{
	sealed class ResourceModule : Module
	{
		private readonly Assembly assembly;
		private readonly string scopeName;
		private readonly string location;

		internal ResourceModule(Assembly assembly, string scopeName, string location)
			: base(assembly.universe)
		{
			this.assembly = assembly;
			this.scopeName = scopeName;
			this.location = location;
		}

		public override int MDStreamVersion
		{
			get { throw new NotSupportedException(); }
		}

		public override bool IsResource()
		{
			return true;
		}

		public override Assembly Assembly
		{
			get { return assembly; }
		}

		public override string FullyQualifiedName
		{
			get { return location ?? "<Unknown>"; }
		}

		public override string Name
		{
			get { return location == null ? "<Unknown>" : System.IO.Path.GetFileName(location); }
		}

		public override string ScopeName
		{
			get { return scopeName; }
		}

		public override Guid ModuleVersionId
		{
			get { throw new NotSupportedException(); }
		}

		public override Type ResolveType(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw new NotSupportedException();
		}

		public override MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw new NotSupportedException();
		}

		public override FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw new NotSupportedException();
		}

		public override MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
		{
			throw new NotSupportedException();
		}

		public override string ResolveString(int metadataToken)
		{
			throw new NotSupportedException();
		}

		public override Type[] __ResolveOptionalParameterTypes(int metadataToken)
		{
			throw new NotSupportedException();
		}

		public override AssemblyName[] __GetReferencedAssemblies()
		{
			throw new NotSupportedException();
		}

		internal override Type FindType(TypeName typeName)
		{
			return null;
		}

		internal override void GetTypesImpl(List<Type> list)
		{
		}

		internal override Type GetModuleType()
		{
			throw new InvalidOperationException();
		}

		internal override ByteReader GetBlob(int blobIndex)
		{
			throw new InvalidOperationException();
		}
	}
}
