/**
 * \file
 * GC handle type used by profiler, metadata, sgen.
 * They must all use the same values, even if, for example
 * sgen is not coupled to metadata so they are
 * in a common place, outside of metadata.
 */
#ifndef __MONO_INC_MONO_GC_HANDLE_H__
#define __MONO_INC_MONO_GC_HANDLE_H__

// These should match System.Runtime.InteropServices.GCHandleType.
// And mcs/class/Mono.Profiler.Log/Mono.Profiler.Log/LogEnums.cs.
// And mono/sgen/gc-internal-agnostic.h aliases them.
typedef enum {
#define MONO_GC_HANDLE_TYPE_MIN MONO_GC_HANDLE_WEAK // Prefer no duplicates in enum type.
       MONO_GC_HANDLE_WEAK			= 0,
       MONO_GC_HANDLE_WEAK_TRACK_RESURRECTION	= 1,
       MONO_GC_HANDLE_NORMAL			= 2,
       MONO_GC_HANDLE_PINNED			= 3,
       MONO_GC_HANDLE_WEAK_FIELDS		= 4,
       MONO_GC_HANDLE_TYPE_MAX			= 5,
} MonoGCHandleType;

#endif // __MONO_INC_MONO_GC_HANDLE_H__
