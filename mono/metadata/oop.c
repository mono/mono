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
#include <mono/metadata/mono-config.h>
#include <mono/metadata/threads-types.h>
#include <metadata/threads.h>
#include <metadata/profiler-private.h>
#include <mono/metadata/coree.h>

#ifdef _M_X64
#include <mono/mini/mini.h>
#include <mono/mini/mini-amd64.h>
extern GList* g_dynamic_function_table_begin;
extern SRWLOCK g_dynamic_function_table_lock;
#endif

// petele: todo: move this structure into a mono header
typedef struct _MonoStackFrameDetails
{
    char* methodName;
    size_t methodNameLen;
    char* className;
    size_t classNameLen;
    char* assemblyName;
    size_t assemblyNameLen;
} MonoStackFrameDetails;

typedef gboolean(*ReadMemoryCallback)(void* buffer, gsize* read, const void* address, gsize size, void* userdata);
typedef gboolean(*ReadExceptionCallback)(const void* address, gsize size, void* userdata);

typedef struct _OutOfProcessMono
{
    ReadMemoryCallback readMemory;
    ReadExceptionCallback readException;
    void* userData;
} OutOfProcessMono;

static OutOfProcessMono g_oop = { NULL, NULL };

#define OFFSET_MEMBER(type, base, member) ((gpointer)((gchar*)(base) + offsetof(type, member)))

void read_exception(const void* address, gsize size)
{
    g_assert(g_oop.readException);
    g_oop.readException(address, size, g_oop.userData);
}

gsize read_memory(void* buffer, const void* address, gsize size)
{
    if (!buffer || !size)
        return 0;

    gsize read = 0;
    if (!g_oop.readMemory || !g_oop.readMemory(buffer, &read, address, size, g_oop.userData)) {
        read_exception(address, size);
    }
    
    return read;
}

// Read a null-terminated string out-of-process
gsize read_nt_string(char* buffer, gsize max_size, const void* address)
{
    if (!buffer || !max_size)
        return 0;

    if (!g_oop.readMemory) {
        read_exception(address, 1);
        return 0;
    }

    gsize read = 0;
    if (!g_oop.readMemory(buffer, &read, address, max_size, g_oop.userData)) {
        // Failed to read, but just because we may not have read max_size, we still
        // might be OK if at least one character was read (i.e. the null-terminator)
        if (read == 0)
            read_exception(address, 1);
    }

    // Ensure there's a null-terminator
    buffer[min(read, max_size-1)] = '\0';

    return read;
}

gpointer read_pointer(const void* address)
{
    gpointer ptr = NULL;
    read_memory(&ptr, address, sizeof(ptr));
    return ptr;
}

gint64 read_qword(const void* address)
{
    gint64 v = 0;
    read_memory(&v, address, sizeof(v));
    return v;
}

gint32 read_dword(const void* address)
{
    gint32 v = 0;
    read_memory(&v, address, sizeof(v));
    return v;
}

GList* read_glist_next(GList* list) { return (GList*) read_pointer(OFFSET_MEMBER(GList, list, next)); }
gpointer read_glist_data(GList* list) { return read_pointer(OFFSET_MEMBER(GList, list, data)); }

MONO_API void
mono_unity_oop_init(
    ReadMemoryCallback rmcb, 
    ReadExceptionCallback recb,
    void* userdata) 
{
    g_oop.readMemory = rmcb;
    g_oop.readException = recb;
    g_oop.userData = userdata;
}

MONO_API void
mono_unity_oop_shutdown(void)
{
    memset(&g_oop, 0, sizeof(g_oop));
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
    return;
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

static int oop_jit_info_table_index(
    const MonoJitInfoTableChunk** chunks, // non-local
    int num_chunks,
    const gint8* addr)
{
    static const int error = 0x7fffffff;

    int left = 0, right = num_chunks;

    g_assert(left < right);

    do {
        const MonoJitInfoTableChunk* chunkPtr;
        const gint8* last_code_end;
        int pos = (left + right) / 2;

        chunkPtr = read_pointer(chunks + pos);
        if (chunkPtr == NULL)
            return error;

        last_code_end = read_pointer(OFFSET_MEMBER(MonoJitInfoTableChunk, chunkPtr, last_code_end));
        if (last_code_end == NULL)
            return error;

        if (addr < last_code_end)
            right = pos;
        else
            left = pos + 1;
    } while (left < right);
    g_assert(left == right);

    if (left >= num_chunks)
        return num_chunks - 1;
    return left;
}

static int
oop_jit_info_table_chunk_index(
    const MonoJitInfo** chunk_data,
    int num_elements,
    const gint8 *addr)
{
    const MonoJitInfo* ji;
    int left = 0, right = num_elements;

    while (left < right) {
        int pos = (left + right) / 2;

        const gint8 *code_start;
        const gint8 *code_end;
        int code_size;

        ji = chunk_data[pos];

        code_start = (const gint8*)read_pointer(OFFSET_MEMBER(MonoJitInfo, ji, code_start));
        code_size = read_dword(OFFSET_MEMBER(MonoJitInfo, ji, code_size));
        code_end = code_start + code_size;

        if (addr < code_end)
            right = pos;
        else
            left = pos + 1;
    }

    g_assert(left == right);

    return left;
}

/* This method is an out-of-process version of jit_info_table_find. */
static const MonoJitInfo*
oop_jit_info_table_find(
    const MonoDomain *domain,
    const char *addr, 
    gboolean allow_trampolines)
{
    const MonoJitInfoTable* tablePtr;
    const MonoJitInfoTableChunk** chunkListPtr;
    MonoJitInfoTableChunk chunk;
    MonoJitInfo ji;
    int chunk_pos, pos;

    // Get the domain's jit_info_table pointer.
    tablePtr = read_pointer(OFFSET_MEMBER(MonoDomain, domain, jit_info_table));
    if (tablePtr == NULL)
        return NULL;

    int num_chunks = read_dword(OFFSET_MEMBER(MonoJitInfoTable, tablePtr, num_chunks));

    // Get the chunk array
    chunkListPtr = (const MonoJitInfoTableChunk**)OFFSET_MEMBER(MonoJitInfoTable, tablePtr, chunks);

    chunk_pos = oop_jit_info_table_index(chunkListPtr, num_chunks, (const gint8*)addr);
    if (chunk_pos > num_chunks)
        return NULL;
    
    // read the entire chunk
    read_memory(&chunk, read_pointer(chunkListPtr + chunk_pos), sizeof(MonoJitInfoTableChunk));

    pos = oop_jit_info_table_chunk_index((const MonoJitInfo**)chunk.data, chunk.num_elements, addr);
    if (pos > chunk.num_elements)
        return NULL;

    /* We now have a position that's very close to that of the
    first element whose end address is higher than the one
    we're looking for.  If we don't have the exact position,
    then we have a position below that one, so we'll just
    search upward until we find our element. */
    do {
        read_memory(&chunk, read_pointer(chunkListPtr + chunk_pos), sizeof(MonoJitInfoTableChunk));

        while (pos < chunk.num_elements) {
            read_memory(&ji, chunk.data[pos], sizeof(ji));

            ++pos;

            if (ji.d.method == NULL) {
                continue;
            }
            if ((gint8*)addr >= (gint8*)ji.code_start
                && (gint8*)addr < (gint8*)ji.code_start + ji.code_size) {
                if (ji.is_trampoline && !allow_trampolines) {
                    return NULL;
                }
                return chunk.data[pos-1];
            }

            /* If we find a non-tombstone element which is already
            beyond what we're looking for, we have to end the
            search. */
            if ((gint8*)addr < (gint8*)ji.code_start)
                return NULL;
        }

        ++chunk_pos;
        pos = 0;
    } while (chunk_pos < num_chunks);

    return NULL;
}

MONO_API int
mono_unity_oop_get_stack_frame_details(
    const MonoDomain* domain,
    const void* frameAddress,
    MonoStackFrameDetails* frameDetails)
{
    const MonoJitInfo* ji;

    ji = oop_jit_info_table_find(domain, (const char*)frameAddress, FALSE);
    if (ji)
    {
        const MonoMethod* method = read_pointer(OFFSET_MEMBER(MonoJitInfo, ji, d.method));
        const MonoClass* klass = read_pointer(OFFSET_MEMBER(MonoMethod, method, klass));
        const MonoImage* image = read_pointer(OFFSET_MEMBER(MonoClass, klass, image));
        size_t classNameLen = max(frameDetails->classNameLen, 256);
        char* className = (char*)malloc(classNameLen);
        char* nsName = (char*)malloc(classNameLen);

        frameDetails->methodNameLen = read_nt_string(
            frameDetails->methodName,
            frameDetails->methodNameLen,
            read_pointer(OFFSET_MEMBER(MonoMethod, method, name)));

        if (frameDetails->className && frameDetails->classNameLen > 0) {
            read_nt_string(
                nsName,
                classNameLen,
                read_pointer(OFFSET_MEMBER(MonoClass, klass, name_space)));

            read_nt_string(
                className,
                classNameLen,
                read_pointer(OFFSET_MEMBER(MonoClass, klass, name)));
            
            if (*nsName) {
                frameDetails->classNameLen = sprintf_s(
                    frameDetails->className,
                    frameDetails->classNameLen,
                    "%s.%s",
                    nsName,
                    className);
            } else {
                frameDetails->classNameLen = sprintf_s(
                    frameDetails->className,
                    frameDetails->classNameLen,
                    "%s",
                    className);
            }
        }

        frameDetails->assemblyNameLen = read_nt_string(
            frameDetails->assemblyName,
            frameDetails->assemblyNameLen,
            read_pointer(OFFSET_MEMBER(MonoImage, image, assembly_name)));

        free(className);
        free(nsName);

        return TRUE;
    }

    return FALSE;
}
