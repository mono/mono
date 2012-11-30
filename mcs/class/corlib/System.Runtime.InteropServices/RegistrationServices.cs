//
// System.Runtime.InteropServices.RegistrationServices.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Reflection;

namespace System.Runtime.InteropServices
{
	
#if !MOBILE	
	[ComVisible(true)]
	[Guid ("475e398f-8afa-43a7-a3be-f4ef8d6787c9")]
	[ClassInterface (ClassInterfaceType.None)]
	public class RegistrationServices : IRegistrationServices
	{
		private static Guid guidManagedCategory = new Guid("{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}");

		public RegistrationServices ()
		{
		}

		public virtual Guid GetManagedCategoryGuid ()
		{
			return guidManagedCategory;
		}

		public virtual string GetProgIdForType (Type type)
		{
			return Marshal.GenerateProgIdForType(type);
		}

		[MonoTODO ("implement")]
		public virtual Type[] GetRegistrableTypesInAssembly (Assembly assembly)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public virtual bool RegisterAssembly (Assembly assembly, AssemblyRegistrationFlags flags)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public virtual void RegisterTypeForComClients (Type type, ref Guid g)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public virtual bool TypeRepresentsComType (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public virtual bool TypeRequiresRegistration (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public virtual bool UnregisterAssembly (Assembly assembly)
		{
			throw new NotImplementedException ();
		}

		[ComVisible(false)]
		[MonoTODO ("implement")]
		public virtual int RegisterTypeForComClients(Type type, RegistrationClassContext classContext, RegistrationConnectionType flags)
		{
			throw new NotImplementedException ();
		}
		
		[ComVisible(false)]
		[MonoTODO ("implement")]
		public virtual void UnregisterTypeForComClients(int cookie)
		{
			throw new NotImplementedException ();
		}
		
	}
#endif
}
