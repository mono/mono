/*
 * lldb.c: Mono support for LLDB.
 *
 * Author:
 *   Zoltan Varga (vargaz@gmail.com)
 *
 * Copyright 2016 Xamarin, Inc (http://www.xamarin.com)
 */

#include "config.h"
#include "mini.h"
#include "lldb.h"

#if !defined(DISABLE_JIT) && !defined(DISABLE_LLDB)

typedef enum {
	ENTRY_CODE_REGION = 1,
	ENTRY_METHOD = 2,
	ENTRY_TRAMPOLINE = 3,
	ENTRY_UNLOAD_CODE_REGION = 4
} EntryType;

/* One data packet sent from the runtime to the debugger */
typedef struct {
	/* Pointer to the next entry */
	guint64 next_addr;
	/* The type of data pointed to by ADDR */
	/* One of the ENTRY_ constants */
	guint32 type;
	guint64 size;
	gpointer addr;
} DebugEntry;

typedef struct
{
	/* (MAJOR << 16) | MINOR */
	guint32 version;
	/* Keep these as pointers so accessing them is atomic */
	DebugEntry *entry;
	/* List of all entries */
	DebugEntry *all_entries;
} JitDescriptor;

/*
 * Represents a memory region used for code.
 */
typedef struct {
	/*
	 * OBJFILE_MAGIC. This is needed to make it easier for lldb to
	 * create object files from this packet.
	 */
	char magic [32];
	guint64 start;
	guint32 size;
	int id;
} CodeRegionEntry;

typedef struct {
	int id;
} UnloadCodeRegionEntry;

/*
 * Represents a managed method
 */
typedef struct {
	guint64 code;
	int id;
	/* The id of the codegen region which contains CODE */
	int region_id;
	int code_size;
} MethodEntry;

/*
 * Represents a trampoline
 */
typedef struct {
	guint64 code;
	int id;
	/* The id of the codegen region which contains CODE */
	int region_id;
	int code_size;
} TrampolineEntry;

#define MAJOR_VERSION 1
#define MINOR_VERSION 0

static const char* OBJFILE_MAGIC = { "MONO_JIT_OBJECT_FILE" };

JitDescriptor __mono_jit_debug_descriptor = { (MAJOR_VERSION << 16) | MINOR_VERSION };

static gboolean enabled;
static int id_generator;
static GHashTable *codegen_regions;
static DebugEntry *last_entry;
static mono_mutex_t mutex;
static GHashTable *dyn_codegen_regions;

#define lldb_lock() mono_os_mutex_lock (&mutex)
#define lldb_unlock() mono_os_mutex_unlock (&mutex)

void MONO_NEVER_INLINE __mono_jit_debug_register_code (void);

/* The native debugger puts a breakpoint in this function. */
void MONO_NEVER_INLINE
__mono_jit_debug_register_code (void)
{
	/* Make sure that even compilers that ignore __noinline__ don't inline this */
#if defined(__GNUC__)
	asm ("");
#endif
}

/*
 * Functions to encode protocol data
 */

typedef struct {
	guint8 *buf, *p, *end;
} Buffer;

static inline void
buffer_init (Buffer *buf, int size)
{
	buf->buf = (guint8 *)g_malloc (size);
	buf->p = buf->buf;
	buf->end = buf->buf + size;
}

static inline int
buffer_len (Buffer *buf)
{
	return buf->p - buf->buf;
}

static inline void
buffer_make_room (Buffer *buf, int size)
{
	if (buf->end - buf->p < size) {
		int new_size = buf->end - buf->buf + size + 32;
		guint8 *p = (guint8 *)g_realloc (buf->buf, new_size);
		size = buf->p - buf->buf;
		buf->buf = p;
		buf->p = p + size;
		buf->end = buf->buf + new_size;
	}
}

static inline void
buffer_add_byte (Buffer *buf, guint8 val)
{
	buffer_make_room (buf, 1);
	buf->p [0] = val;
	buf->p++;
}

static inline void
buffer_add_short (Buffer *buf, guint32 val)
{
	buffer_make_room (buf, 2);
	buf->p [0] = (val >> 8) & 0xff;
	buf->p [1] = (val >> 0) & 0xff;
	buf->p += 2;
}

static inline void
buffer_add_int (Buffer *buf, guint32 val)
{
	buffer_make_room (buf, 4);
	buf->p [0] = (val >> 24) & 0xff;
	buf->p [1] = (val >> 16) & 0xff;
	buf->p [2] = (val >> 8) & 0xff;
	buf->p [3] = (val >> 0) & 0xff;
	buf->p += 4;
}

static inline void
buffer_add_long (Buffer *buf, guint64 l)
{
	buffer_add_int (buf, (l >> 32) & 0xffffffff);
	buffer_add_int (buf, (l >> 0) & 0xffffffff);
}

static inline void
buffer_add_id (Buffer *buf, int id)
{
	buffer_add_int (buf, (guint64)id);
}

static inline void
buffer_add_data (Buffer *buf, guint8 *data, int len)
{
	buffer_make_room (buf, len);
	memcpy (buf->p, data, len);
	buf->p += len;
}

static inline void
buffer_add_string (Buffer *buf, const char *str)
{
	int len;

	if (str == NULL) {
		buffer_add_int (buf, 0);
	} else {
		len = strlen (str);
		buffer_add_int (buf, len);
		buffer_add_data (buf, (guint8*)str, len);
	}
}

static inline void
buffer_add_buffer (Buffer *buf, Buffer *data)
{
	buffer_add_data (buf, data->buf, buffer_len (data));
}

static inline void
buffer_free (Buffer *buf)
{
	g_free (buf->buf);
}

typedef struct {
	gpointer code;
	gpointer region_start;
	guint32 region_size;
	gboolean found;
} UserData;

static int
find_code_region (void *data, int csize, int size, void *user_data)
{
	UserData *ud = user_data;

	if ((char*)ud->code >= (char*)data && (char*)ud->code < (char*)data + csize) {
		ud->region_start = data;
		ud->region_size = csize;
		ud->found = TRUE;
		return 1;
	}
	return 0;
}

static void
add_entry (EntryType type, Buffer *buf)
{
	DebugEntry *entry;
	guint8 *data;
	int size = buffer_len (buf);

	data = g_malloc (size);
	memcpy (data, buf->buf, size);

	entry = g_malloc0 (sizeof (DebugEntry));
	entry->type = type;
	entry->addr = data;
	entry->size = size;

	mono_memory_barrier ();

	lldb_lock ();

	/* The debugger can read the list of entries asynchronously, so this has to be async safe */
	// FIXME: Make sure this is async safe
	if (last_entry) {
		last_entry->next_addr = (guint64)(gsize) (entry);
		last_entry = entry;
	} else {
		last_entry = entry;
		__mono_jit_debug_descriptor.all_entries = entry;
	}

	__mono_jit_debug_descriptor.entry = entry;
	__mono_jit_debug_register_code ();

	lldb_unlock ();
}

/*
 * register_codegen_region:
 *
 * Register a codegen region with the debugger if needed.
 * Return a region id.
 */
static int
register_codegen_region (gpointer region_start, int region_size, gboolean dynamic)
{
	CodeRegionEntry *region_entry;
	int id;
	Buffer tmp_buf;
	Buffer *buf = &tmp_buf;

	if (dynamic) {
		lldb_lock ();
		id = ++id_generator;
		lldb_unlock ();
	} else {
		lldb_lock ();
		if (!codegen_regions)
			codegen_regions = g_hash_table_new (NULL, NULL);
		id = GPOINTER_TO_INT (g_hash_table_lookup (codegen_regions, region_start));
		if (id) {
			lldb_unlock ();
			return id;
		}
		id = ++id_generator;
		g_hash_table_insert (codegen_regions, region_start, GINT_TO_POINTER (id));
		lldb_unlock ();
	}

	buffer_init (buf, 128);

	region_entry = (CodeRegionEntry*)buf->p;
	buf->p += sizeof (CodeRegionEntry);
	memset (region_entry, 0, sizeof (CodeRegionEntry));
	strcpy (region_entry->magic, OBJFILE_MAGIC);
	region_entry->id = id;
	region_entry->start = (gsize)region_start;
	region_entry->size = (gsize)region_size;

	add_entry (ENTRY_CODE_REGION, buf);
	buffer_free (buf);
	return id;
}

static void
emit_unwind_info (GSList *unwind_ops, Buffer *buf)
{
	int ret_reg;
	int nunwind_ops;
	GSList *l;

	ret_reg = mono_unwind_get_dwarf_pc_reg ();
	g_assert (ret_reg < 256);

	/* We use the unencoded version of the unwind info to make it easier to decode */
	nunwind_ops = 0;
	for (l = unwind_ops; l; l = l->next) {
		MonoUnwindOp *op = l->data;

		/* lldb can't handle these */
		if (op->op == DW_CFA_mono_advance_loc)
			break;
		nunwind_ops ++;
	}

	buffer_add_byte (buf, ret_reg);
	buffer_add_int (buf, nunwind_ops);
	for (l = unwind_ops; l; l = l->next) {
		MonoUnwindOp *op = l->data;

		if (op->op == DW_CFA_mono_advance_loc)
			break;
		buffer_add_int (buf, op->op);
		buffer_add_int (buf, op->when);
		buffer_add_int (buf, mono_hw_reg_to_dwarf_reg (op->reg));
		buffer_add_int (buf, op->val);
	}
}

void
mono_lldb_init (const char *options)
{
	enabled = TRUE;
	mono_os_mutex_init_recursive (&mutex);
}

void
mono_lldb_save_method_info (MonoCompile *cfg)
{
	MethodEntry *entry;
	UserData udata;
	int region_id;
	Buffer tmpbuf;
	Buffer *buf = &tmpbuf;

	if (!enabled)
		return;

	/* Find the codegen region which contains the code */
	memset (&udata, 0, sizeof (udata));
	udata.code = cfg->native_code;
	if (cfg->method->dynamic) {
		mono_code_manager_foreach (cfg->dynamic_info->code_mp, find_code_region, &udata);
		g_assert (udata.found);

		region_id = register_codegen_region (udata.region_start, udata.region_size, TRUE);

		lldb_lock ();
		if (!dyn_codegen_regions)
			dyn_codegen_regions = g_hash_table_new (NULL, NULL);
		g_hash_table_insert (dyn_codegen_regions, cfg->method, GINT_TO_POINTER (region_id));
		lldb_unlock ();
	} else {
		mono_domain_code_foreach (cfg->domain, find_code_region, &udata);
		g_assert (udata.found);

		region_id = register_codegen_region (udata.region_start, udata.region_size, FALSE);
	}

	buffer_init (buf, 256);

	entry = (MethodEntry*)buf->p;
	buf->p += sizeof (MethodEntry);
	entry->id = ++id_generator;
	entry->region_id = region_id;
	entry->code = (gsize)cfg->native_code;
	entry->code_size = cfg->code_size;

	emit_unwind_info (cfg->unwind_ops, buf);

	char *s = g_strdup_printf ("%s_%s", cfg->method->klass->name, cfg->method->name);
	buffer_add_string (buf, s);
	g_free (s);

	add_entry (ENTRY_METHOD, buf);
	buffer_free (buf);
}

void
mono_lldb_remove_method (MonoDomain *domain, MonoMethod *method, MonoJitDynamicMethodInfo *info)
{
	int region_id;
	UnloadCodeRegionEntry *entry;
	Buffer tmpbuf;
	Buffer *buf = &tmpbuf;

	g_assert (method->dynamic);

	lldb_lock ();
	region_id = GPOINTER_TO_INT (g_hash_table_lookup (dyn_codegen_regions, method));
	g_hash_table_remove (dyn_codegen_regions, method);
	lldb_unlock ();

	buffer_init (buf, 256);

	entry = (UnloadCodeRegionEntry*)buf->p;
	buf->p += sizeof (UnloadCodeRegionEntry);
	entry->id = region_id;

	add_entry (ENTRY_UNLOAD_CODE_REGION, buf);
	buffer_free (buf);

	/* The method is associated with the code region, so it doesn't have to be unloaded */
}

void
mono_lldb_save_trampoline_info (MonoTrampInfo *info)
{
	TrampolineEntry *entry;
	UserData udata;
	int region_id;
	Buffer tmpbuf;
	Buffer *buf = &tmpbuf;

	if (!enabled)
		return;

	/* Find the codegen region which contains the code */
	memset (&udata, 0, sizeof (udata));
	udata.code = info->code;
	mono_global_codeman_foreach (find_code_region, &udata);
	if (!udata.found)
		mono_domain_code_foreach (mono_get_root_domain (), find_code_region, &udata);
	g_assert (udata.found);

	region_id = register_codegen_region (udata.region_start, udata.region_size, FALSE);

	buffer_init (buf, 1024);

	entry = (TrampolineEntry*)buf->p;
	buf->p += sizeof (TrampolineEntry);
	entry->id = ++id_generator;
	entry->region_id = region_id;
	entry->code = (gsize)info->code;
	entry->code_size = info->code_size;

	emit_unwind_info (info->unwind_ops, buf);

	buffer_add_string (buf, info->name);

	add_entry (ENTRY_TRAMPOLINE, buf);
	buffer_free (buf);
}

/*
DESIGN:

Communication:
Similar to the gdb jit interface. The runtime communicates with a plugin running inside lldb.
- The runtime allocates a data packet, points a symbol with a well known name at it.
- It calls a dummy function with a well known name.
- The plugin sets a breakpoint at this function, causing the runtime to be suspended.
- The plugin reads the data pointed to by the other symbol and processes it.

The data packets are kept in a list, so lldb can read all of them after attaching.
Lldb will associate an object file with each mono codegen region.

Packet design:
- use a flat byte array so the whole data can be read in one operation.
- use 64 bit ints for pointers.
*/

#else

void
mono_lldb_init (const char *options)
{
	g_error ("lldb support has been disabled at configure time.");
}

void
mono_lldb_save_method_info (MonoCompile *cfg)
{
}

void
mono_lldb_save_trampoline_info (MonoTrampInfo *info)
{
}

void
mono_lldb_remove_method (MonoDomain *domain, MonoMethod *method, MonoJitDynamicMethodInfo *info)
{
}

#endif
