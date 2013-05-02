//
// mac-reachability.c: System.Net.NetworkingInformation.NetworkChange
// implementation for Mac OS X using SystemConfiguration's
// NetworkReachability API.
//
// Authors:
//  Aaron Bockover (abock@xamarin.com)
//
// Copyright (c) 2013 Xamarin, Inc. (http://www.xamarin.com)
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

#if HAVE_CONFIG_H
#include "config.h"
#endif

int mono_sc_reachability_enabled (void);

#if defined(PLATFORM_MACOSX) || defined(TARGET_IOS)

int
mono_sc_reachability_enabled (void)
{
	return 1;
}

#include <SystemConfiguration/SCNetworkReachability.h>
#include <netinet/in.h>

typedef void (*mono_sc_reachability_callback)(int);

typedef struct {
	SCNetworkReachabilityRef reachability;
	mono_sc_reachability_callback callback;
} mono_sc_reachability;

mono_sc_reachability * mono_sc_reachability_new (mono_sc_reachability_callback callback);
void mono_sc_reachability_free (mono_sc_reachability *reachability);
int mono_sc_reachability_is_available (mono_sc_reachability *reachability);

static int
_mono_sc_reachability_is_available (SCNetworkReachabilityFlags flags)
{
	return (flags & kSCNetworkFlagsReachable) && (flags & kSCNetworkFlagsConnectionRequired) == 0;
}

static void
_mono_sc_reachability_callback (SCNetworkReachabilityRef target, SCNetworkReachabilityFlags flags, void *user)
{
	mono_sc_reachability *reachability;
	
	if (user == NULL) {
		return;
	}

	reachability = (mono_sc_reachability *)user;
	if (reachability->callback == NULL) {
		return;
	}

	reachability->callback (_mono_sc_reachability_is_available (flags));
}

mono_sc_reachability *
mono_sc_reachability_new (mono_sc_reachability_callback callback)
{
	struct sockaddr_in zero;
	SCNetworkReachabilityRef reachability;
	SCNetworkReachabilityContext context;
	mono_sc_reachability *instance;

	if (callback == NULL) {
		return NULL;
	}

	bzero (&zero, sizeof (zero));
	zero.sin_len = sizeof (zero);
	zero.sin_family = AF_INET;

	reachability = SCNetworkReachabilityCreateWithAddress (NULL, (const struct sockaddr *)&zero);
	if (reachability == NULL) {
		return NULL;
	}

	instance = (mono_sc_reachability *)malloc (sizeof (mono_sc_reachability));
	instance->reachability = reachability;
	instance->callback = callback;

	bzero (&context, sizeof (context));
	context.info = instance;

	if (!SCNetworkReachabilitySetCallback (reachability, _mono_sc_reachability_callback, &context) ||
		!SCNetworkReachabilityScheduleWithRunLoop (reachability, CFRunLoopGetCurrent (), kCFRunLoopDefaultMode)) {
		mono_sc_reachability_free (instance);
		return NULL;
	}

	return instance;
}

void
mono_sc_reachability_free (mono_sc_reachability *reachability)
{
	if (reachability != NULL) {
		if (reachability->reachability != NULL) {
			SCNetworkReachabilityUnscheduleFromRunLoop (reachability->reachability,
				CFRunLoopGetCurrent (), kCFRunLoopDefaultMode);
			CFRelease (reachability->reachability);
			reachability->reachability = NULL;
		}

		reachability->callback = NULL;
		free (reachability);
		reachability = NULL;
	}
}

int
mono_sc_reachability_is_available (mono_sc_reachability *reachability)
{
	SCNetworkReachabilityFlags flags;
	return reachability != NULL && reachability->reachability != NULL &&
		SCNetworkReachabilityGetFlags (reachability->reachability, &flags) &&
		_mono_sc_reachability_is_available (flags);
}

#else

int
mono_sc_reachability_enabled (void)
{
	return 0;
}

#endif
