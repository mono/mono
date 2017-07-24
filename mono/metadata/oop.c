/*
 * oop.c: These functions allow us to access the MonoDomain internals for purposes of post-mortem
 * inspection by another process. All data is immutable: these calls are guaranteed to have no 
 * side-effects. These routines are not thread safe. This does not work with AOT modules.
 *
 * Author:
 *	Pete Lewis <pete.lewis@unity3d.com>
 *
 * Copyright 2017 Unity Technologies (http://www.unity3d.com)
 * Copyright 2001-2003 Ximian, Inc (http://www.ximian.com)
 * Copyright 2004-2009 Novell, Inc (http://www.novell.com)
 */

#include <config.h>
#include <glib.h>
#include <string.h>
#include <sys/stat.h>

#include <mono/utils/mono-compiler.h>
#include <mono/utils/mono-logger.h>
#include <mono/utils/mono-membar.h>
#include <mono/utils/mono-counters.h>
#include <mono/metadata/object.h>
#include <mono/metadata/object-internals.h>
#include <mono/metadata/domain-internals.h>
#include <mono/metadata/class-internals.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/metadata-internals.h>
#include <mono/metadata/gc-internals.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/mono-debug-debugger.h>
#include <mono/metadata/mono-config.h>
#include <mono/metadata/threads-types.h>
#include <metadata/threads.h>
#include <metadata/profiler-private.h>
#include <mono/metadata/coree.h>

#ifdef _M_X64
#include <mono/mini/mini-amd64.h>
extern GList* g_dynamic_function_table_begin;
extern SRWLOCK g_dynamic_function_table_lock;
#endif

typedef gboolean(*ReadMemoryCallback)(void* buffer, void* address, gsize size, void* userdata);

typedef struct _OutOfProcessMono
{
    ReadMemoryCallback readMemory;
    void* userData;
} OutOfProcessMono;

static OutOfProcessMono g_oop = { NULL, NULL };

#define OFFSET_MEMBER(type, base, member) ((gpointer)((gchar*)(base) + offsetof(type, member)))

gboolean read_memory(void* buffer, void* address, gsize size)
{
    if (!g_oop.readMemory)
        return FALSE;
    return g_oop.readMemory(buffer, address, size, g_oop.userData);
}

gpointer read_pointer(void* address)
{
    gpointer ptr = NULL;
    if (!read_memory(&ptr, address, sizeof(ptr)))
        return NULL;
    return ptr;
}

gint64 read_qword(void* address)
{
    gint64 v = 0;
    if (!read_memory(&v, address, sizeof(v)))
        return 0;
    return v;
}

gint32 read_dword(void* address)
{
    gint32 v = 0;
    if (!read_memory(&v, address, sizeof(v)))
        return 0;
    return v;
}

GList* read_glist_next(GList* list) { return (GList*) read_pointer(OFFSET_MEMBER(GList, list, next)); }
gpointer read_glist_data(GList* list) { return read_pointer(OFFSET_MEMBER(GList, list, data)); }

MONO_API void
mono_unity_oop_init(
    ReadMemoryCallback rmcb, 
    void* userdata) 
{
    g_oop.readMemory = rmcb;
    g_oop.userData = userdata;
}

#ifdef _M_X64
gboolean TryAcquireSpinWait(PSRWLOCK lock, unsigned int spinWait)
{
    do
    {
        if (TryAcquireSRWLockExclusive(&g_dynamic_function_table_lock))
            return TRUE;
    } while (spinWait--);

    return FALSE;
}
#endif

MONO_API GList*
mono_unity_lock_dynamic_function_access_tables64(unsigned int spinWait) 
{
#ifdef _M_X64
    if (spinWait >= 0x7fffffff) {
        AcquireSRWLockExclusive(&g_dynamic_function_table_lock);
    }
    else if (!TryAcquireSpinWait(&g_dynamic_function_table_lock, spinWait)) {
        return NULL;
    }
    return g_dynamic_function_table_begin;
#else
    return NULL;
#endif
}

MONO_API void
mono_unity_unlock_dynamic_function_access_tables64(void) 
{
#ifdef _M_X64
    ReleaseSRWLockExclusive(&g_dynamic_function_table_lock);
#else
    return NULL;
#endif
}

MONO_API GList*
mono_unity_oop_iterate_dynamic_function_access_tables64(
    GList* current) 
{
#ifdef _M_X64
    if (current != NULL)
        return read_glist_next(current);
    else
        return NULL;
#else
    return NULL;
#endif
}

MONO_API gboolean
mono_unity_oop_get_dynamic_function_access_table64(
    GList* tableEntry,
    gsize* moduleStart,
    gsize* moduleEnd,
    void** functionTable,
    gsize* functionTableSize)
{
#ifdef _M_X64
    if (!tableEntry || !moduleStart || !moduleEnd || !functionTable || !functionTableSize)
        return FALSE;

    const DynamicFunctionTableEntry* entry = read_glist_data(tableEntry);
    *moduleStart = read_qword(OFFSET_MEMBER(DynamicFunctionTableEntry, entry, begin_range));
    *moduleEnd = read_qword(OFFSET_MEMBER(DynamicFunctionTableEntry, entry, end_range));
    *functionTable = read_pointer(OFFSET_MEMBER(DynamicFunctionTableEntry, entry, rt_funcs));
    *functionTableSize = read_dword(OFFSET_MEMBER(DynamicFunctionTableEntry, entry, rt_funcs_max_count));

    return TRUE;
#else
    return FALSE;
#endif
}
