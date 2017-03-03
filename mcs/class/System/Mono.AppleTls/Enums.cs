#if MONO_FEATURE_APPLETLS
// Copyright 2011-2015 Xamarin Inc. All rights reserved.

using ObjCRuntime;

namespace Mono.AppleTls {

	// this is a subset of OSStatus -> SInt32 -> signed int - see CoreFoundation.framework/Headers/CFBase.h
	// values are defined in Security.framework/Headers/SecBase.h 
	enum SecStatusCode {
		Success 							= 0,
		DuplicateItem	 					= -25299,
		Param 								= -50,
	}

	// typedef uint32_t SecTrustResultType;
	// values are defined in Security.framework/Headers/SecTrust.h 
	enum SecTrustResult {
		Unspecified,
	}
}
#endif
