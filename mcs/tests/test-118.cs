using System;
using System.Security.Policy;
using System.Threading;

class Container {

	// LoaderOptimization exists as an enum
	// and LoaderOptimization is also the abbreviation for
	// LoaderOptimizationAttribute
	[LoaderOptimization (LoaderOptimization.SingleDomain)]
	static int Main (string[] args) {
		return 0;
	}
}
