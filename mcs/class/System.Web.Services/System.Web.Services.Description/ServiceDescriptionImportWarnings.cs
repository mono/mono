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
		NoCodeGenerated,
		NoMethodsGenerated,
		OptionalExtensionsIgnored,
		RequiredExtensionsIgnored,
		UnsupportedBindingsIgnored,
		UnsupportedOperationsIgnored
	}
}
