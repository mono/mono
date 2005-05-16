//
// System.Security.Permissions.IBuiltInPermission.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2003 Motus Technologies (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

namespace System.Security.Permissions {

	// LAMESPEC: Undocumented interface
	internal interface IBuiltInPermission {
		int GetTokenIndex ();
	}

	// 1_1	2_0	Name
	// 0	0	EnvironmentPermission
	// 1	1	FileDialogPermission
	// 2	2	FileIOPermission
	// 3	3	IsolatedStorageFilePermission
	// 4	4	ReflectionPermission
	// 5	5	RegistryPermission
	// 6	6	SecurityPermission
	// 7	7	UIPermission
	// 8	8	PrincipalPermission
	// N/A	9	HostProtectionPermission (internal)
	// 9	10	PublisherIdentityPermission
	// 10	11	SiteIdentityPermission
	// 11	12	StrongNameIdentityPermission
	// 12	13	UrlIdentityPermission
	// 13	14	ZoneIdentityPermission
	// N/A	15	GacIdentityPermission
	// N/A	16	KeyContainerPermission

	internal enum BuiltInToken {
		Environment = 0,
		FileDialog = 1,
		FileIO = 2,
		IsolatedStorageFile = 3,
		Reflection = 4,
		Registry = 5,
		Security = 6,
		UI = 7,
		Principal = 8,
#if !NET_2_0
		PublisherIdentity = 9,
		SiteIdentity = 10,
		StrongNameIdentity = 11,
		UrlIdentity = 12,
		ZoneIdentity = 13,
#else
		HostProtection = 9,
		PublisherIdentity = 10,
		SiteIdentity = 11,
		StrongNameIdentity = 12,
		UrlIdentity = 13,
		ZoneIdentity = 14,
		GacIdentity = 15,
		KeyContainer = 16,
#endif
	}
}
