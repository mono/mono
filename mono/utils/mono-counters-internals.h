#ifndef __MONO_COUNTERS_INTERNALS_H__
#define __MONO_COUNTERS_INTERNALS_H__

#include "mono-counters.h"
#include "mono-compiler.h"

typedef enum {
	MONO_COUNTER_CAT_JIT,
	MONO_COUNTER_CAT_GC,
	MONO_COUNTER_CAT_METADATA,
	MONO_COUNTER_CAT_GENERICS,
	MONO_COUNTER_CAT_SECURITY,

	MONO_COUNTER_CAT_REMOTING,
	MONO_COUNTER_CAT_EXC,
	MONO_COUNTER_CAT_THREAD,
	MONO_COUNTER_CAT_THREADPOOL,
	MONO_COUNTER_CAT_IO,
} MonoCounterCategory;

typedef enum {
	MONO_COUNTER_TYPE_INT, /* 4 bytes */
	MONO_COUNTER_TYPE_LONG, /* 8 bytes */
	MONO_COUNTER_TYPE_WORD, /* machine word */
} MonoCounterType;

typedef enum {
	MONO_COUNTER_UNIT_NONE,  /* It's a raw value that needs special handling from the consumer */
	MONO_COUNTER_UNIT_QUANTITY, /* Quantity of the given counter */
	MONO_COUNTER_UNIT_TIME,  /* This is a timestap in 100n units */
	MONO_COUNTER_UNIT_EVENT, /* Number of times the given event happens */
	MONO_COUNTER_UNIT_CONFIG, /* Configuration knob of the runtime */
} MonoCounterUnit;

typedef enum {
	MONO_COUNTER_UNIT_CONSTANT = 1, /* This counter doesn't change. Agent will only send it once */
	MONO_COUNTER_UNIT_MONOTONIC, /* This counter value always increase/decreate over time */
	MONO_COUNTER_UNIT_VARIABLE, /* This counter value can be anything on each sampling */
} MonoCounterVariance;

typedef struct _MonoCounter MonoCounter;
/*
TODO:
	Helpers based on size.
	Helpers for constants.
	String type.
	Sampler function that take user data (could we use them for user perf counters?)
	Dynamic category registration.
	Error handling/assertion
	MonoCounter size diet once we're done with the above.
*/
void*
mono_counters_new (MonoCounterCategory category, const char *name, MonoCounterType type, MonoCounterUnit unit, MonoCounterVariance variance) MONO_INTERNAL;

MonoCounter*
mono_counters_register_full (MonoCounterCategory category, const char *name, MonoCounterType type, MonoCounterUnit unit, MonoCounterVariance variance, void *addr) MONO_INTERNAL;

#endif
