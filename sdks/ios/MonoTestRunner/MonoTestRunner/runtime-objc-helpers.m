//
//  runtime-objc-helpers.m
//  MonoTestRunner
//
//  Created by Rodrigo Kumpera on 4/19/17.
//  Copyright © 2017 Rodrigo Kumpera. All rights reserved.
//

#import <Foundation/Foundation.h>

//FIXME conditionalize this properly
#define MONOTOUCH

const char *
runtime_get_bundle_path (void)
{
    NSBundle *main_bundle = [NSBundle mainBundle];
    NSString *bundle_path;
    
    bundle_path = [main_bundle bundlePath];
    
    const char *result = strdup ([bundle_path UTF8String]);
    
    return result;
}


///All the following functions are used by the BCL
void
xamarin_log (const unsigned short *unicodeMessage)
{
	// COOP: no managed memory access: any mode.
	int length = 0;
	const unsigned short *ptr = unicodeMessage;
	while (*ptr++)
		length += sizeof (unsigned short);
	NSString *msg = [[NSString alloc] initWithBytes: unicodeMessage length: length encoding: NSUTF16LittleEndianStringEncoding];

#if TARGET_OS_WATCH && defined (__arm__) // maybe make this configurable somehow?
	const char *utf8 = [msg UTF8String];
	int len = strlen (utf8);
	fwrite (utf8, 1, len, stdout);
	if (len == 0 || utf8 [len - 1] != '\n')
		fwrite ("\n", 1, 1, stdout);
	fflush (stdout);
#else
	NSLog (@"%@", msg);
#endif
}

#if defined (MONOTOUCH)
// called from mono-extensions/mcs/class/corlib/System/Environment.iOS.cs
const char *
xamarin_GetFolderPath (int folder)
{
	// COOP: no managed memory access: any mode.
	// NSUInteger-based enum (and we do not want corlib exposed to 32/64 bits differences)
	NSSearchPathDirectory dd = (NSSearchPathDirectory) folder;
	NSURL *url = [[[NSFileManager defaultManager] URLsForDirectory:dd inDomains:NSUserDomainMask] lastObject];
	NSString *path = [url path];
	return strdup ([path UTF8String]);
}
#endif /* defined (MONOTOUCH) */

void*
xamarin_timezone_get_data (const char *name, int *size)
{
	// COOP: no managed memory access: any mode.
	NSTimeZone *tz = nil;
	if (name) {
		NSString *n = [[NSString alloc] initWithUTF8String: name];
		tz = [[NSTimeZone alloc] initWithName:n];
	} else {
		tz = [NSTimeZone localTimeZone];
	}
	NSData *data = [tz data];
	*size = [data length];
	void* result = malloc (*size);
	memcpy (result, data.bytes, *size);
	return result;
}

char**
xamarin_timezone_get_names (int *count)
{
	// COOP: no managed memory access: any mode.
	NSArray *array = [NSTimeZone knownTimeZoneNames];
	*count = array.count;
	char** result = (char**) malloc (sizeof (char*) * (*count));
	for (int i = 0; i < *count; i++) {
		NSString *s = [array objectAtIndex: i];
		result [i] = strdup (s.UTF8String);
	}
	return result;
}
