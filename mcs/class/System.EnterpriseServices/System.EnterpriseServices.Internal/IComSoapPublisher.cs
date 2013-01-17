// System.EnterpriseServices.Internal.IComSoapPublisher.cs
//
// Author:
//   Alejandro Sánchez Acosta (raciel@es.gnu.org)
//
// Copyright (C) 2002 Alejandro Sánchez Acosta
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

namespace System.EnterpriseServices.Internal
{
	[Guid("d8013eee-730b-45e2-ba24-874b7242c425")]
	public interface IComSoapPublisher
	{
		[DispId(6)]
		void CreateMailBox (
			[MarshalAs(UnmanagedType.BStr)] string RootMailServer,
			[MarshalAs(UnmanagedType.BStr)] string MailBox,
			[MarshalAs(UnmanagedType.BStr)] out string SmtpName,
			[MarshalAs(UnmanagedType.BStr)] out string Domain,
			[MarshalAs(UnmanagedType.BStr)] out string PhysicalPath,
			[MarshalAs(UnmanagedType.BStr)] out string Error);

		[DispId(4)]
		void CreateVirtualRoot (
			[MarshalAs(UnmanagedType.BStr)] string Operation,
			[MarshalAs(UnmanagedType.BStr)] string FullUrl,
			[MarshalAs(UnmanagedType.BStr)] out string BaseUrl,
			[MarshalAs(UnmanagedType.BStr)] out string VirtualRoot,
			[MarshalAs(UnmanagedType.BStr)] out string PhysicalPath,
			[MarshalAs(UnmanagedType.BStr)] out string Error);

		[DispId(7)]
		void DeleteMailBox (
			[MarshalAs(UnmanagedType.BStr)] string RootMailServer,
			[MarshalAs(UnmanagedType.BStr)] string MailBox,
			[MarshalAs(UnmanagedType.BStr)] out string Error);

		[DispId(5)]
		void DeleteVirtualRoot (
			[MarshalAs(UnmanagedType.BStr)] string RootWebServer,
			[MarshalAs(UnmanagedType.BStr)] string FullUrl,
			[MarshalAs(UnmanagedType.BStr)] out string Error);

		[DispId(13)]
		void GacInstall ([MarshalAs(UnmanagedType.BStr)] string AssemblyPath);

		[DispId(14)]
		void GacRemove ([MarshalAs(UnmanagedType.BStr)] string AssemblyPath);

		[DispId(15)]
		void GetAssemblyNameForCache (
			[MarshalAs(UnmanagedType.BStr)] string TypeLibPath,
			[MarshalAs(UnmanagedType.BStr)] out string CachePath);

		[return: MarshalAs(UnmanagedType.BStr)]
		[DispId(10)]
		string GetTypeNameFromProgId (
			[MarshalAs(UnmanagedType.BStr)] string AssemblyPath,
			[MarshalAs(UnmanagedType.BStr)] string ProgId);

		[DispId(9)]
		void ProcessClientTlb (
			[MarshalAs(UnmanagedType.BStr)] string ProgId,
			[MarshalAs(UnmanagedType.BStr)] string SrcTlbPath,
			[MarshalAs(UnmanagedType.BStr)] string PhysicalPath,
			[MarshalAs(UnmanagedType.BStr)] string VRoot,
			[MarshalAs(UnmanagedType.BStr)] string BaseUrl,
			[MarshalAs(UnmanagedType.BStr)] string Mode,
			[MarshalAs(UnmanagedType.BStr)] string Transport,
			[MarshalAs(UnmanagedType.BStr)] out string AssemblyName,
			[MarshalAs(UnmanagedType.BStr)] out string TypeName,
			[MarshalAs(UnmanagedType.BStr)] out string Error);

		[DispId(8)]
		void ProcessServerTlb (
			[MarshalAs(UnmanagedType.BStr)] string ProgId,
			[MarshalAs(UnmanagedType.BStr)] string SrcTlbPath,
			[MarshalAs(UnmanagedType.BStr)] string PhysicalPath,
			[MarshalAs(UnmanagedType.BStr)] string Operation,
			[MarshalAs(UnmanagedType.BStr)] out string AssemblyName,
			[MarshalAs(UnmanagedType.BStr)] out string TypeName,
			[MarshalAs(UnmanagedType.BStr)] out string Error);

		[DispId(11)]
		void RegisterAssembly ([MarshalAs(UnmanagedType.BStr)] string AssemblyPath);

		[DispId(12)]
		void UnRegisterAssembly ([MarshalAs(UnmanagedType.BStr)] string AssemblyPath);
	}
}
