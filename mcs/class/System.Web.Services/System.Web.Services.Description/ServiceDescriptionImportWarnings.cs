// 
// System.Web.Services.Description.ServiceDescriptionImportWarnings.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	[Serializable]
	public enum ServiceDescriptionImportWarnings {
		NoCodeGenerated = 0x1,
		NoMethodsGenerated = 0x20,
		OptionalExtensionsIgnored = 0x2,
		RequiredExtensionsIgnored = 0x4,
		UnsupportedBindingsIgnored = 0x10,
		UnsupportedOperationsIgnored = 0x8
	}
}
