#ifndef __MONO_PROFLOG_H__
#define __MONO_PROFLOG_H__

#include <mono/utils/mono-compiler.h>

#define BUF_ID 0x4D504C01
#define LOG_HEADER_ID 0x4D505A01
#define LOG_VERSION_MAJOR 0
#define LOG_VERSION_MINOR 4
#define LOG_DATA_VERSION 4
/*
 * Changes in data versions:
 * version 2: added offsets in heap walk
 * version 3: added GC roots
 * version 4: added sample/statistical profiling
 */

enum {
	TYPE_ALLOC,
	TYPE_GC,
	TYPE_METADATA,
	TYPE_METHOD,
	TYPE_EXCEPTION,
	TYPE_MONITOR,
	TYPE_HEAP,
	TYPE_SAMPLE,
	/* extended type for TYPE_HEAP */
	TYPE_HEAP_START  = 0 << 4,
	TYPE_HEAP_END    = 1 << 4,
	TYPE_HEAP_OBJECT = 2 << 4,
	TYPE_HEAP_ROOT   = 3 << 4,
	/* extended type for TYPE_METADATA */
	TYPE_START_LOAD   = 1 << 4,
	TYPE_END_LOAD     = 2 << 4,
	TYPE_START_UNLOAD = 3 << 4,
	TYPE_END_UNLOAD   = 4 << 4,
	TYPE_LOAD_ERR     = 1 << 7,
	TYPE_CLASS     = 1,
	TYPE_IMAGE     = 2,
	TYPE_ASSEMBLY  = 3,
	TYPE_DOMAIN    = 4,
	TYPE_THREAD    = 5,
	/* extended type for TYPE_GC */
	TYPE_GC_EVENT  = 1 << 4,
	TYPE_GC_RESIZE = 2 << 4,
	TYPE_GC_MOVE   = 3 << 4,
	TYPE_GC_HANDLE_CREATED   = 4 << 4,
	TYPE_GC_HANDLE_DESTROYED = 5 << 4,
	/* extended type for TYPE_METHOD */
	TYPE_LEAVE     = 1 << 4,
	TYPE_ENTER     = 2 << 4,
	TYPE_EXC_LEAVE = 3 << 4,
	TYPE_JIT       = 4 << 4,
	/* extended type for TYPE_EXCEPTION */
	TYPE_THROW        = 0 << 4,
	TYPE_CLAUSE       = 1 << 4,
	TYPE_EXCEPTION_BT = 1 << 7,
	/* extended type for TYPE_ALLOC */
	TYPE_ALLOC_BT  = 1 << 4,
	/* extended type for TYPE_MONITOR */
	TYPE_MONITOR_BT  = 1 << 7,
	/* extended type for TYPE_SAMPLE */
	TYPE_SAMPLE_HIT    = 0 << 4,
	TYPE_SAMPLE_USYM   = 1 << 4,
	TYPE_SAMPLE_UBIN   = 2 << 4,
	TYPE_END
};

enum {
	SAMPLE_CYCLES = 1,
	SAMPLE_INSTRUCTIONS,
	SAMPLE_CACHE_MISSES,
	SAMPLE_CACHE_REFS,
	SAMPLE_BRANCHES,
	SAMPLE_BRANCH_MISSES,
	SAMPLE_LAST
};

typedef struct _LogBuffer LogBuffer;

LogBuffer* ensure_logbuf (int bytes) MONO_INTERNAL;

int enter_log (LogBuffer *lb, const char *str) MONO_INTERNAL;
void exit_log (LogBuffer *lb) MONO_INTERNAL;

void emit_byte (LogBuffer *logbuffer, int value) MONO_INTERNAL;
void emit_value (LogBuffer *logbuffer, int value) MONO_INTERNAL;
void emit_time (LogBuffer *logbuffer, uint64_t value) MONO_INTERNAL;
void emit_svalue (LogBuffer *logbuffer, int64_t value) MONO_INTERNAL;
void emit_uvalue (LogBuffer *logbuffer, uint64_t value) MONO_INTERNAL;
void emit_ptr (LogBuffer *logbuffer, void *ptr) MONO_INTERNAL;
void emit_method (LogBuffer *logbuffer, void *method) MONO_INTERNAL;
void emit_obj (LogBuffer *logbuffer, void *ptr) MONO_INTERNAL;
void emit_string (LogBuffer *logbuffer, const char *str) MONO_INTERNAL;

#endif /* __MONO_PROFLOG_H__ */
