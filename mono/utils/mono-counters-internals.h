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
	MONO_COUNTER_TYPE_DOUBLE,

	MONO_COUNTER_TYPE_MAX
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
Limitations:
	The old-style string counter type won't work as they cannot be safely sampled during execution.

TODO:
	Size-bounded String counter.
	Sampler function that take user data arguments (could we use them for user perf counters?)
	Dynamic category registration.
	MonoCounter size diet once we're done with the above.
*/
void*
mono_counters_new (MonoCounterCategory category, const char *name, MonoCounterType type, MonoCounterUnit unit, MonoCounterVariance variance) MONO_INTERNAL;

MonoCounter*
mono_counters_register_full (MonoCounterCategory category, const char *name, MonoCounterType type, MonoCounterUnit unit, MonoCounterVariance variance, void *addr) MONO_INTERNAL;

#define mono_counters_new_int(cat,name,unit,variance) mono_counters_new(cat,name,MONO_COUNTER_TYPE_INT,unit,variance)
#define mono_counters_new_word(cat,name,unit,variance) mono_counters_new(cat,name,MONO_COUNTER_TYPE_WORD,unit,variance)
#define mono_counters_new_long(cat,name,unit,variance) mono_counters_new(cat,name,MONO_COUNTER_TYPE_LONG,unit,variance)
#define mono_counters_new_double(cat,name,unit,variance) mono_counters_new(cat,name,MONO_COUNTER_TYPE_double,unit,variance)

#define mono_counters_new_int_const(cat,name,unit,value) do { int *__ptr = mono_counters_new(cat,name,MONO_COUNTER_TYPE_INT,unit,variance); *__ptr = value; } while (0)
#define mono_counters_new_word_const(cat,name,unit,value) do { ssize_t *__ptr = mono_counters_new(cat,name,MONO_COUNTER_TYPE_INT,unit,variance); *__ptr = value; } while (0)
#define mono_counters_new_long_const(cat,name,unit,value) do { gint64 *__ptr = mono_counters_new(cat,name,MONO_COUNTER_TYPE_INT,unit,variance); *__ptr = value; } while (0)
#define mono_counters_new_double_const(cat,name,unit,value) do { double *__ptr = mono_counters_new(cat,name,MONO_COUNTER_TYPE_INT,unit,variance); *__ptr = value; } while (0)


#endif
