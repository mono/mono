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

	MONO_COUNTER_CAT_THREAD,
	MONO_COUNTER_CAT_THREADPOOL,
	MONO_COUNTER_CAT_SYS,

	MONO_COUNTER_CAT_CUSTOM,

	MONO_COUNTER_CAT_MAX
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
	MONO_COUNTER_UNIT_BYTES, /* Quantity of bytes the counter represent */
	MONO_COUNTER_UNIT_TIME,  /* This is a timestap in 100n units */
	MONO_COUNTER_UNIT_EVENTS, /* Number of times the given event happens */
	MONO_COUNTER_UNIT_CONFIG, /* Configuration knob of the runtime */
	MONO_COUNTER_UNIT_PERCENTAGE, /* Percentage of something */

	MONO_COUNTER_UNIT_MAX
} MonoCounterUnit;

typedef enum {
	MONO_COUNTER_CONSTANT = 1, /* This counter doesn't change. Agent will only send it once */
	MONO_COUNTER_MONOTONIC, /* This counter value always increase/decreases over time */
	MONO_COUNTER_VARIABLE, /* This counter value can be anything on each sampling */

	MONO_COUNTER_VARIANCE_MAX
} MonoCounterVariance;

typedef struct _MonoCounter MonoCounter;

struct _MonoCounter {
	MonoCounter *next;
	const char *name;
	void *addr;
	MonoCounterType type;
	MonoCounterCategory category;
	MonoCounterUnit unit;
	MonoCounterVariance variance;
	int callback_style;
	gboolean is_synthetic;
	void *user_arg;
};

typedef gboolean (*CountersEnumCallback) (const char *category, const char *name);
typedef gboolean (*CountersDataSourceForeach) (CountersEnumCallback);
typedef MonoCounter* (*CountersDataSourceGet) (const char *, const char *name);

/*
Limitations:
	The old-style string counter type won't work as they cannot be safely sampled during execution.

TODO:
	Size-bounded String counter.
	Dynamic category registration.
	MonoCounter size diet once we're done with the above.

FIXME
	The naming schema for the counters sucks, cleanup and unify with mono-counters.h
*/
void* mono_counters_new (MonoCounterCategory category, const char *name, MonoCounterType type, MonoCounterUnit unit, MonoCounterVariance variance) MONO_INTERNAL;
MonoCounter* mono_counters_register_full (MonoCounterCategory category, const char *name, MonoCounterType type, MonoCounterUnit unit, MonoCounterVariance variance, void *addr) MONO_INTERNAL;

void* mono_counters_new_synt (MonoCounterCategory category, const char *name, MonoCounterType type, MonoCounterUnit unit, MonoCounterVariance variance, void *addr) MONO_INTERNAL;
void* mono_counters_new_synt_func (MonoCounterCategory category, const char *name, MonoCounterType type, MonoCounterUnit unit, MonoCounterVariance variance, void *fun_addr, void *user_arg) MONO_INTERNAL;

MonoCounter* mono_counters_get (const char *category, const char* name) MONO_INTERNAL;
int mono_counters_sample (MonoCounter* counter, char* buffer, int size) MONO_INTERNAL;
int mono_counters_size   (MonoCounter* counter) MONO_INTERNAL;
void mono_counters_foreach (CountersEnumCallback cb) MONO_INTERNAL;

void mono_counters_add_data_source (CountersDataSourceGet get_cb, CountersDataSourceForeach foreach_cb) MONO_INTERNAL;

MonoCounterCategory mono_counters_category_name_to_id (const char* name) MONO_INTERNAL;
const char* mono_counters_category_id_to_name (MonoCounterCategory id) MONO_INTERNAL;
void mono_counters_free_counter (MonoCounter* counter) MONO_INTERNAL;

/* Helpers */

/*
These helpers provide a typed struct and a bunch of helper functions:
Structs: IntCounter LongCounter, WordCounter, DoubleCounter
factory functions: mono_counters_new_int, mono_counters_new_const_int
update functions: mono_counters_int_set, mono_counters_int_inc, mono_counters_int_dec
read functions: mono_counters_int_get
*/
#define MK_COUNTER_HELPERS(TYPE, NAME, CAPITALIZED_NAME,COUNTER_TYPE) \
typedef struct { TYPE *pointer; } CAPITALIZED_NAME ##Counter;	\
static void mono_counters_ ## NAME ## _set (CAPITALIZED_NAME ##Counter counter, TYPE value) { (*counter.pointer) += value; }	\
static TYPE mono_counters_ ## NAME ## _get (CAPITALIZED_NAME ##Counter counter) { return *counter.pointer; }	\
static void mono_counters_ ## NAME ## _inc (CAPITALIZED_NAME ##Counter counter) { (*counter.pointer) += 1; }	\
static void mono_counters_ ## NAME ## _dec (CAPITALIZED_NAME ##Counter counter) { (*counter.pointer) -= 1; }	\
static CAPITALIZED_NAME ##Counter mono_counters_new_ ## NAME (MonoCounterCategory category, const char *name, MonoCounterUnit unit, MonoCounterVariance variance) {	\
	CAPITALIZED_NAME ##Counter c = { mono_counters_new(category, name, MONO_COUNTER_TYPE_ ## COUNTER_TYPE, unit, variance) };	\
	return c;	\
}	\
static void mono_counters_new_const_ ## NAME (MonoCounterCategory category, const char *name, MonoCounterUnit unit, TYPE value) {	\
	CAPITALIZED_NAME ##Counter c = { mono_counters_new(category, name, MONO_COUNTER_TYPE_ ## COUNTER_TYPE, unit, MONO_COUNTER_CONSTANT) };	\
	mono_counters_ ## NAME ## _set (c, value);	\
}	\

MK_COUNTER_HELPERS(int,int,Int,INT)
MK_COUNTER_HELPERS(gint64,long,Long,LONG)
MK_COUNTER_HELPERS(ssize_t,word,Word,WORD)
MK_COUNTER_HELPERS(double,double,Double,DOUBLE)


#endif
