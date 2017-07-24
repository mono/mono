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

G_BEGIN_DECLS
typedef int(*MonoReadMemoryCallback)(
    void* userData,
    const void* address,
    void* buffer,
    size_t size,
    size_t* readSize);

// petele: todo: move this structure into a mono header
typedef struct _MonoStackFrameDetails
{
    char* methodName;
    size_t methodNameLen;
    char* signature;
    size_t signatureLen;
    char* assemblyName;
    size_t assemblyNameLen;
    char* sourceFile;
    size_t sourceFileLen;
    int lineNo;
} MonoStackFrameDetails;

struct _MonoOutOfProcessParameters
{
    MonoReadMemoryCallback readMemoryCallback;
    void* userData;
};

typedef struct _MonoOutOfProcessParameters MonoOutOfProcessParameters;

G_END_DECLS

typedef const void * gooppointer;

gooppointer oop_fetch_ptr(
    const MonoOutOfProcessParameters* oopCfg,
    gooppointer ptr)
{
    gpointer out = NULL;
    if (!oopCfg->readMemoryCallback(oopCfg->userData, ptr, &out, sizeof(out), NULL))
        return NULL;
    return out;
}

#define oop_copy(oopCfg, storage, address) (oopCfg->readMemoryCallback(oopCfg->userData, (address), &(storage), sizeof(storage), NULL))
#define oop_member_address(type, base, member) ((const gint8*)(base) + offsetof(type, member))

MonoBoolean oop_copy_jit_info_table(
    const MonoOutOfProcessParameters* oopCfg,
    const MonoDomain* domain,
    MonoJitInfoTable* table)
{
    gooppointer srcTable = oop_fetch_ptr(oopCfg, oop_member_address(MonoDomain, domain, jit_info_table));
    return oopCfg->readMemoryCallback(oopCfg->userData, srcTable, table, sizeof(MonoJitInfoTable), NULL);
}

static int oop_jit_info_table_index(
    const MonoOutOfProcessParameters* oopCfg,
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

        chunkPtr = oop_fetch_ptr(oopCfg, chunks + pos);
        if (chunkPtr == NULL)
            return error;

        last_code_end = oop_fetch_ptr(oopCfg, oop_member_address(MonoJitInfoTableChunk, chunkPtr, last_code_end));
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
    const MonoOutOfProcessParameters* oopCfg,
    const MonoJitInfo** chunk_data,
    int num_elements,
    const gint8 *addr)
{
    static const int error = 0x7fffffff;

    MonoJitInfo ji;
    int left = 0, right = num_elements;

    while (left < right) {
        int pos = (left + right) / 2;
        const gint8 *code_end;

        if (!oop_copy(oopCfg, ji, oop_fetch_ptr(oopCfg, chunk_data + pos)))
            return error;

        code_end = (const gint8*)ji.code_start + ji.code_size;

        if (addr < code_end)
            right = pos;
        else
            left = pos + 1;
    }

    g_assert(left == right);

    return left;
}

static const MonoJitInfo*
oop_jit_info_table_find(
    const MonoOutOfProcessParameters* oopCfg,
    const MonoDomain *domain,
    const char *addr)
{
    const MonoJitInfoTable* tablePtr;
    const MonoJitInfoTableChunk** chunkListPtr;
    MonoJitInfoTableChunk chunk;
    const MonoJitInfo* jiPtr;
    MonoJitInfo ji;
    int chunk_pos, pos;
    
    // Get the domain's jit_info_table pointer.
    tablePtr = oop_fetch_ptr(oopCfg, oop_member_address(MonoDomain, domain, jit_info_table));
    if (tablePtr == NULL)
        return NULL;

    int num_chunks = 0;
    if (!oop_copy(oopCfg, num_chunks, oop_member_address(MonoJitInfoTable, tablePtr, num_chunks)))
        return NULL;

    // Get the chunk array
    chunkListPtr = (const MonoJitInfoTableChunk**) oop_member_address(MonoJitInfoTable, tablePtr, chunks);

    chunk_pos = oop_jit_info_table_index(oopCfg, chunkListPtr, num_chunks, (const gint8*)addr);
    if (chunk_pos > num_chunks)
        return NULL;

    if (!oop_copy(oopCfg, chunk, oop_fetch_ptr(oopCfg, chunkListPtr + chunk_pos)))
        return NULL;

    pos = oop_jit_info_table_chunk_index(oopCfg, (const MonoJitInfo**) chunk.data, chunk.num_elements, addr);
    if (pos > chunk.num_elements)
        return NULL;

    /* We now have a position that's very close to that of the
    first element whose end address is higher than the one
    we're looking for.  If we don't have the exact position,
    then we have a position below that one, so we'll just
    search upward until we find our element. */
    do {
        if (!oop_copy(oopCfg, chunk, oop_fetch_ptr(oopCfg, chunkListPtr + chunk_pos)))
            return NULL;

        while (pos < chunk.num_elements) {
            jiPtr = (const MonoJitInfo*)oop_fetch_ptr(oopCfg, chunk.data + pos);
            if (!oop_copy(oopCfg, ji, jiPtr))
                return NULL;

            ++pos;

            if (ji.d.method == NULL) {
                continue;
            }
            if ((gint8*)addr >= (gint8*)ji.code_start
                && (gint8*)addr < (gint8*)ji.code_start + ji.code_size) {
                return jiPtr;
            }

            /* If we find a non-tombstone element which is already
            beyond what we're looking for, we have to end the
            search. */
            if ((gint8*)addr < (gint8*)ji.code_start)
                goto not_found;
        }

        ++chunk_pos;
        pos = 0;
    } while (chunk_pos < num_chunks);

not_found:
    return NULL;
}

int oop_read_string(
    const MonoOutOfProcessParameters* oopCfg,
    char* buf,
    size_t* bufLen,
    const char* src)
{
    size_t read = 0;
    int ret = oopCfg->readMemoryCallback(oopCfg->userData, src, buf, *bufLen, &read);

    *bufLen = read;
    return ret ? ret : read > 0;
}

int
mono_oop_get_stack_frame_details(
    const MonoDomain* domain,
    const void* frameAddress,
    MonoReadMemoryCallback readMemoryCallback,
    void* userData,
    MonoStackFrameDetails* frameDetails)
{
    const MonoJitInfo* ji;

    MonoOutOfProcessParameters oopCfg;
    oopCfg.readMemoryCallback = readMemoryCallback;
    oopCfg.userData = userData;

    ji = oop_jit_info_table_find(&oopCfg, domain, (const char*) frameAddress);
    if (ji)
    {
        const MonoMethod* method = oop_fetch_ptr(&oopCfg, oop_member_address(MonoJitInfo, ji, d.method));

        oop_read_string(&
            oopCfg, 
            frameDetails->methodName,
            &frameDetails->methodNameLen,
            oop_fetch_ptr(&oopCfg, oop_member_address(MonoMethod, method, name)));

        return 1;
    }

    return 0;
}
