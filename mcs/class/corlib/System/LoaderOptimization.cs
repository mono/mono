//
// System.LoaderOptimization.cs
//
// Author:
//   Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System
{
	public enum LoaderOptimization
	{
		NotSpecified = 0,
		SingleDomain = 1,
		MultiDomain = 2,
		MultiDomainHost = 3,
		DomainMask = 3,
		DisallowBindings = 4
	}
}
