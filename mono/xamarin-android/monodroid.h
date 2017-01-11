#ifndef __MONODROID_H
#define __MONODROID_H

/* VS 2010 and later have stdint.h */
#if defined(_MSC_VER)

	#define MONO_API_EXPORT __declspec(dllexport)
	#define MONO_API_IMPORT __declspec(dllimport)

#else   /* defined(_MSC_VER */

	#define MONO_API_EXPORT __attribute__ ((visibility ("default")))
	#define MONO_API_IMPORT

#endif  /* !defined(_MSC_VER) */

#if defined(MONO_DLL_EXPORT)
	#define MONO_API MONO_API_EXPORT
#elif defined(MONO_DLL_IMPORT)
	#define MONO_API MONO_API_IMPORT
#else   /* !defined(MONO_DLL_IMPORT) && !defined(MONO_API_IMPORT) */
	#define MONO_API
#endif  /* MONO_DLL_EXPORT... */

enum FatalExitCodes {
	FATAL_EXIT_CANNOT_FIND_MONO           =  1,
	FATAL_EXIT_ATTACH_JVM_FAILED          =  2,
	FATAL_EXIT_DEBUGGER_CONNECT           =  3,
	FATAL_EXIT_CANNOT_FIND_JNIENV         =  4,
	FATAL_EXIT_CANNOT_FIND_APK            = 10,
	FATAL_EXIT_TRIAL_EXPIRED              = 11,
	FATAL_EXIT_PTHREAD_FAILED             = 12,
	FATAL_EXIT_MISSING_ASSEMBLY           = 13,
	FATAL_EXIT_CANNOT_LOAD_BUNDLE         = 14,
	FATAL_EXIT_CANNOT_FIND_LIBMONOSGEN    = 15,
	FATAL_EXIT_NO_ASSEMBLIES              = 'A',
	FATAL_EXIT_MONO_MISSING_SYMBOLS       = 'B',
	FATAL_EXIT_FORK_FAILED                = 'F',
	FATAL_EXIT_MISSING_INIT               = 'I',
	FATAL_EXIT_MISSING_TIMEZONE_MEMBERS   = 'T',
	FATAL_EXIT_MISSING_ZIPALIGN           = 'Z',
	FATAL_EXIT_OUT_OF_MEMORY              = 'M',
};

#endif  /* defined __MONODROID_H */
