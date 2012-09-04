// 
// System.EnterpriseServices.RegistrationHelper.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

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
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[Guid("89a86e7b-c229-4008-9baa-2f5c8411d7e0")]
	public sealed class RegistrationHelper : MarshalByRefObject, IRegistrationHelper {

		#region Constructors

		public RegistrationHelper ()
		{
		}

		#endregion

		#region Methods

		public void InstallAssembly (string assembly, ref string application, ref string tlb, InstallationFlags installFlags)
		{
			application = String.Empty;
			tlb = String.Empty;

			InstallAssembly (assembly, ref application, null, ref tlb, installFlags);
		}

		[MonoTODO]
		public void InstallAssembly (string assembly, ref string application, string partition, ref string tlb, InstallationFlags installFlags)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void InstallAssemblyFromConfig ([MarshalAs(UnmanagedType.IUnknown)] ref RegistrationConfig regConfig)
		{
			throw new NotImplementedException ();
		}

		public void UninstallAssembly (string assembly, string application)
		{
			UninstallAssembly (assembly, application, null);
		}

		[MonoTODO]
		public void UninstallAssembly (string assembly, string application, string partition)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void UninstallAssemblyFromConfig ([MarshalAs(UnmanagedType.IUnknown)] ref RegistrationConfig regConfig)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
