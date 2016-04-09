/*
 * domain.c: MonoDomain functions
 *
 * Author:
 *	Dietmar Maurer (dietmar@ximian.com)
 *	Patrik Torstensson
 *
 * Copyright 2001-2003 Ximian, Inc (http://www.ximian.com)
 * Copyright 2004-2009 Novell, Inc (http://www.novell.com)
 * Copyright 2011-2012 Xamarin, Inc (http://www.xamarin.com)
 */

#include <config.h>
#include <glib.h>
#include <string.h>
#include <sys/stat.h>

#include <mono/metadata/gc-internals.h>

#include <mono/utils/atomic.h>
#include <mono/utils/mono-compiler.h>
#include <mono/utils/mono-logger-internals.h>
#include <mono/utils/mono-membar.h>
#include <mono/utils/mono-counters.h>
#include <mono/utils/hazard-pointer.h>
#include <mono/utils/mono-tls.h>
#include <mono/utils/mono-mmap.h>
#include <mono/utils/mono-threads.h>
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
#include <mono/metadata/runtime.h>
#include <metadata/threads.h>
#include <metadata/profiler-private.h>
#include <mono/metadata/coree.h>

//#define DEBUG_DOMAIN_UNLOAD 1

/* we need to use both the Tls* functions and __thread because
 * some archs may generate faster jit code with one meachanism
 * or the other (we used to do it because tls slots were GC-tracked,
 * but we can't depend on this).
 */
static MonoNativeTlsKey appdomain_thread_id;

#ifdef MONO_HAVE_FAST_TLS

MONO_FAST_TLS_DECLARE(tls_appdomain);

#define GET_APPDOMAIN() ((MonoDomain*)MONO_FAST_TLS_GET(tls_appdomain))

#define SET_APPDOMAIN(x) do { \
	MonoThreadInfo *info; \
	MONO_FAST_TLS_SET (tls_appdomain,x); \
	mono_native_tls_set_value (appdomain_thread_id, x); \
	mono_gc_set_current_thread_appdomain (x); \
	info = mono_thread_info_current (); \
	if (info) \
		mono_thread_info_tls_set (info, TLS_KEY_DOMAIN, (x));	\
} while (FALSE)

#else /* !MONO_HAVE_FAST_TLS */

#define GET_APPDOMAIN() ((MonoDomain *)mono_native_tls_get_value (appdomain_thread_id))
#define SET_APPDOMAIN(x) do {						\
		MonoThreadInfo *info;								\
		mono_native_tls_set_value (appdomain_thread_id, x);	\
		mono_gc_set_current_thread_appdomain (x);		\
		info = mono_thread_info_current ();				\
		if (info)												 \
			mono_thread_info_tls_set (info, TLS_KEY_DOMAIN, (x));	\
	} while (FALSE)

#endif

#define GET_APPCONTEXT() (mono_thread_internal_current ()->current_appcontext)
#define SET_APPCONTEXT(x) MONO_OBJECT_SETREF (mono_thread_internal_current (), current_appcontext, (x))

static guint16 appdomain_list_size = 0;
static guint16 appdomain_next = 0;
static MonoDomain **appdomains_list = NULL;
static MonoImage *exe_image;
static gboolean debug_domain_unload;

gboolean mono_dont_free_domains;

#define mono_appdomains_lock() mono_coop_mutex_lock (&appdomains_mutex)
#define mono_appdomains_unlock() mono_coop_mutex_unlock (&appdomains_mutex)
static MonoCoopMutex appdomains_mutex;

static MonoDomain *mono_root_domain = NULL;

/* some statistics */
static int max_domain_code_size = 0;
static int max_domain_code_alloc = 0;
static int total_domain_code_alloc = 0;

/* AppConfigInfo: Information about runtime versions supported by an 
 * aplication.
 */
typedef struct {
	GSList *supported_runtimes;
	char *required_runtime;
	int configuration_count;
	int startup_count;
} AppConfigInfo;

static const MonoRuntimeInfo *current_runtime = NULL;

/* This is the list of runtime versions supported by this JIT.
 */
static const MonoRuntimeInfo supported_runtimes[] = {
	{"v4.0.30319","4.5", { {4,0,0,0}, {10,0,0,0}, {4,0,0,0}, {4,0,0,0} } },
	{"mobile",    "2.1", { {2,0,5,0}, {10,0,0,0}, {2,0,5,0}, {2,0,5,0} } },
	{"moonlight", "2.1", { {2,0,5,0}, { 9,0,0,0}, {3,5,0,0}, {3,0,0,0} } },
};


/* The stable runtime version */
#define DEFAULT_RUNTIME_VERSION "v4.0.30319"

/* Callbacks installed by the JIT */
static MonoCreateDomainFunc create_domain_hook;
static MonoFreeDomainFunc free_domain_hook;

/* AOT cache configuration */
static MonoAotCacheConfig aot_cache_config;

static void
get_runtimes_from_exe (const char *exe_file, MonoImage **exe_image, const MonoRuntimeInfo** runtimes);

static const MonoRuntimeInfo*
get_runtime_by_version (const char *version);

MonoNativeTlsKey
mono_domain_get_tls_key (void)
{
	return appdomain_thread_id;
}

gint32
mono_domain_get_tls_offset (void)
{
	int offset = -1;

#ifdef HOST_WIN32
	if (appdomain_thread_id)
		offset = appdomain_thread_id;
#else
	MONO_THREAD_VAR_OFFSET (tls_appdomain, offset);
#endif
	return offset;
}

#define ALIGN_TO(val,align) ((((guint64)val) + ((align) - 1)) & ~((align) - 1))
#define ALIGN_PTR_TO(ptr,align) (gpointer)((((gssize)(ptr)) + (align - 1)) & (~(align - 1)))

static LockFreeMempool*
lock_free_mempool_new (void)
{
	return g_new0 (LockFreeMempool, 1);
}

static void
lock_free_mempool_free (LockFreeMempool *mp)
{
	LockFreeMempoolChunk *chunk, *next;

	chunk = mp->chunks;
	while (chunk) {
		next = (LockFreeMempoolChunk *)chunk->prev;
		mono_vfree (chunk, mono_pagesize ());
		chunk = next;
	}
	g_free (mp);
}

/*
 * This is async safe
 */
static LockFreeMempoolChunk*
lock_free_mempool_chunk_new (LockFreeMempool *mp, int len)
{
	LockFreeMempoolChunk *chunk, *prev;
	int size;

	size = mono_pagesize ();
	while (size - sizeof (LockFreeMempoolChunk) < len)
		size += mono_pagesize ();
	chunk = (LockFreeMempoolChunk *)mono_valloc (0, size, MONO_MMAP_READ|MONO_MMAP_WRITE);
	g_assert (chunk);
	chunk->mem = (guint8 *)ALIGN_PTR_TO ((char*)chunk + sizeof (LockFreeMempoolChunk), 16);
	chunk->size = ((char*)chunk + size) - (char*)chunk->mem;
	chunk->pos = 0;

	/* Add to list of chunks lock-free */
	while (TRUE) {
		prev = mp->chunks;
		if (InterlockedCompareExchangePointer ((volatile gpointer*)&mp->chunks, chunk, prev) == prev)
			break;
	}
	chunk->prev = prev;

	return chunk;
}

/*
 * This is async safe
 */
static gpointer
lock_free_mempool_alloc0 (LockFreeMempool *mp, guint size)
{
	LockFreeMempoolChunk *chunk;
	gpointer res;
	int oldpos;

	// FIXME: Free the allocator

	size = ALIGN_TO (size, 8);
	chunk = mp->current;
	if (!chunk) {
		chunk = lock_free_mempool_chunk_new (mp, size);
		mono_memory_barrier ();
		/* Publish */
		mp->current = chunk;
	}

	/* The code below is lock-free, 'chunk' is shared state */
	oldpos = InterlockedExchangeAdd (&chunk->pos, size);
	if (oldpos + size > chunk->size) {
		chunk = lock_free_mempool_chunk_new (mp, size);
		g_assert (chunk->pos + size <= chunk->size);
		res = chunk->mem;
		chunk->pos += size;
		mono_memory_barrier ();
		mp->current = chunk;
	} else {
		res = (char*)chunk->mem + oldpos;
	}

	return res;
}

void
mono_install_create_domain_hook (MonoCreateDomainFunc func)
{
	create_domain_hook = func;
}

void
mono_install_free_domain_hook (MonoFreeDomainFunc func)
{
	free_domain_hook = func;
}

/**
 * mono_string_equal:
 * @s1: First string to compare
 * @s2: Second string to compare
 *
 * Compares two `MonoString*` instances ordinally for equality.
 *
 * Returns FALSE if the strings differ.
 */
gboolean
mono_string_equal (MonoString *s1, MonoString *s2)
{
	int l1 = mono_string_length (s1);
	int l2 = mono_string_length (s2);

	if (s1 == s2)
		return TRUE;
	if (l1 != l2)
		return FALSE;

	return memcmp (mono_string_chars (s1), mono_string_chars (s2), l1 * 2) == 0; 
}

/**
 * mono_string_hash:
 * @s: the string to hash
 *
 * Compute the hash for a `MonoString*`
 * Returns the hash for the string.
 */
guint
mono_string_hash (MonoString *s)
{
	const guint16 *p = mono_string_chars (s);
	int i, len = mono_string_length (s);
	guint h = 0;

	for (i = 0; i < len; i++) {
		h = (h << 5) - h + *p;
		p++;
	}

	return h;	
}

static gboolean
mono_ptrarray_equal (gpointer *s1, gpointer *s2)
{
	int len = GPOINTER_TO_INT (s1 [0]);
	if (len != GPOINTER_TO_INT (s2 [0]))
		return FALSE;

	return memcmp (s1 + 1, s2 + 1, len * sizeof(gpointer)) == 0; 
}

static guint
mono_ptrarray_hash (gpointer *s)
{
	int i;
	int len = GPOINTER_TO_INT (s [0]);
	guint hash = 0;
	
	for (i = 1; i < len; i++)
		hash += GPOINTER_TO_UINT (s [i]);

	return hash;	
}

/*
 * Allocate an id for domain and set domain->domain_id.
 * LOCKING: must be called while holding appdomains_mutex.
 * We try to assign low numbers to the domain, so it can be used
 * as an index in data tables to lookup domain-specific info
 * with minimal memory overhead. We also try not to reuse the
 * same id too quickly (to help debugging).
 */
static int
domain_id_alloc (MonoDomain *domain)
{
	int id = -1, i;
	if (!appdomains_list) {
		appdomain_list_size = 2;
		appdomains_list = (MonoDomain **)mono_gc_alloc_fixed (appdomain_list_size * sizeof (void*), MONO_GC_DESCRIPTOR_NULL, MONO_ROOT_SOURCE_DOMAIN, "domains list");
	}
	for (i = appdomain_next; i < appdomain_list_size; ++i) {
		if (!appdomains_list [i]) {
			id = i;
			break;
		}
	}
	if (id == -1) {
		for (i = 0; i < appdomain_next; ++i) {
			if (!appdomains_list [i]) {
				id = i;
				break;
			}
		}
	}
	if (id == -1) {
		MonoDomain **new_list;
		int new_size = appdomain_list_size * 2;
		if (new_size >= (1 << 16))
			g_assert_not_reached ();
		id = appdomain_list_size;
		new_list = (MonoDomain **)mono_gc_alloc_fixed (new_size * sizeof (void*), MONO_GC_DESCRIPTOR_NULL, MONO_ROOT_SOURCE_DOMAIN, "domains list");
		memcpy (new_list, appdomains_list, appdomain_list_size * sizeof (void*));
		mono_gc_free_fixed (appdomains_list);
		appdomains_list = new_list;
		appdomain_list_size = new_size;
	}
	domain->domain_id = id;
	appdomains_list [id] = domain;
	appdomain_next++;
	if (appdomain_next > appdomain_list_size)
		appdomain_next = 0;
	return id;
}

static gsize domain_gc_bitmap [sizeof(MonoDomain)/4/32 + 1];
static MonoGCDescriptor domain_gc_desc = MONO_GC_DESCRIPTOR_NULL;
static guint32 domain_shadow_serial = 0L;

/**
 * mono_domain_create:
 *
 * Creates a new application domain, the unmanaged representation
 * of the actual domain.   Usually you will want to create the
 *
 * Application domains provide an isolation facilty for assemblies.   You
 * can load assemblies and execute code in them that will not be visible
 * to other application domains.   This is a runtime-based virtualization
 * technology.
 *
 * It is possible to unload domains, which unloads the assemblies and
 * data that was allocated in that domain.
 *
 * When a domain is created a mempool is allocated for domain-specific
 * structures, along a dedicated code manager to hold code that is
 * associated with the domain.
 *
 * Returns: New initialized MonoDomain, with no configuration or assemblies
 * loaded into it.
 */
MonoDomain *
mono_domain_create (void)
{
	MonoDomain *domain;
	guint32 shadow_serial;
  
	mono_appdomains_lock ();
	shadow_serial = domain_shadow_serial++;
  
	if (!domain_gc_desc) {
		unsigned int i, bit = 0;
		for (i = G_STRUCT_OFFSET (MonoDomain, MONO_DOMAIN_FIRST_OBJECT); i < G_STRUCT_OFFSET (MonoDomain, MONO_DOMAIN_FIRST_GC_TRACKED); i += sizeof (gpointer)) {
			bit = i / sizeof (gpointer);
			domain_gc_bitmap [bit / 32] |= (gsize) 1 << (bit % 32);
		}
		domain_gc_desc = mono_gc_make_descr_from_bitmap ((gsize*)domain_gc_bitmap, bit + 1);
	}
	mono_appdomains_unlock ();

#ifdef HAVE_BOEHM_GC
	/*
	 * Boehm doesn't like roots inside GC allocated objects, and alloc_fixed returns
	 * a GC_MALLOC-ed object, contrary to the api docs. This causes random crashes when
	 * running the corlib test suite.
	 * To solve this, we pass a NULL descriptor, and don't register roots.
	 */
	domain = (MonoDomain *)mono_gc_alloc_fixed (sizeof (MonoDomain), NULL, MONO_ROOT_SOURCE_DOMAIN, "domain object");
#else
	domain = (MonoDomain *)mono_gc_alloc_fixed (sizeof (MonoDomain), domain_gc_desc, MONO_ROOT_SOURCE_DOMAIN, "domain object");
	mono_gc_register_root ((char*)&(domain->MONO_DOMAIN_FIRST_GC_TRACKED), G_STRUCT_OFFSET (MonoDomain, MONO_DOMAIN_LAST_GC_TRACKED) - G_STRUCT_OFFSET (MonoDomain, MONO_DOMAIN_FIRST_GC_TRACKED), MONO_GC_DESCRIPTOR_NULL, MONO_ROOT_SOURCE_DOMAIN, "misc domain fields");
#endif
	domain->shadow_serial = shadow_serial;
	domain->domain = NULL;
	domain->setup = NULL;
	domain->friendly_name = NULL;
	domain->search_path = NULL;

	mono_profiler_appdomain_event (domain, MONO_PROFILE_START_LOAD);

	domain->mp = mono_mempool_new ();
	domain->code_mp = mono_code_manager_new ();
	domain->lock_free_mp = lock_free_mempool_new ();
	domain->env = mono_g_hash_table_new_type ((GHashFunc)mono_string_hash, (GCompareFunc)mono_string_equal, MONO_HASH_KEY_VALUE_GC, MONO_ROOT_SOURCE_DOMAIN, "domain environment variables table");
	domain->domain_assemblies = NULL;
	domain->assembly_bindings = NULL;
	domain->assembly_bindings_parsed = FALSE;
	domain->class_vtable_array = g_ptr_array_new ();
	domain->proxy_vtable_hash = g_hash_table_new ((GHashFunc)mono_ptrarray_hash, (GCompareFunc)mono_ptrarray_equal);
	domain->static_data_array = NULL;
	mono_jit_code_hash_init (&domain->jit_code_hash);
	domain->ldstr_table = mono_g_hash_table_new_type ((GHashFunc)mono_string_hash, (GCompareFunc)mono_string_equal, MONO_HASH_KEY_VALUE_GC, MONO_ROOT_SOURCE_DOMAIN, "domain string constants table");
	domain->num_jit_info_tables = 1;
	domain->jit_info_table = mono_jit_info_table_new (domain);
	domain->jit_info_free_queue = NULL;
	domain->finalizable_objects_hash = g_hash_table_new (mono_aligned_addr_hash, NULL);
	domain->ftnptrs_hash = g_hash_table_new (mono_aligned_addr_hash, NULL);

	mono_coop_mutex_init_recursive (&domain->lock);

	mono_os_mutex_init_recursive (&domain->assemblies_lock);
	mono_os_mutex_init_recursive (&domain->jit_code_hash_lock);
	mono_os_mutex_init_recursive (&domain->finalizable_objects_hash_lock);

	domain->method_rgctx_hash = NULL;

	mono_appdomains_lock ();
	domain_id_alloc (domain);
	mono_appdomains_unlock ();

#ifndef DISABLE_PERFCOUNTERS
	mono_perfcounters->loader_appdomains++;
	mono_perfcounters->loader_total_appdomains++;
#endif

	mono_debug_domain_create (domain);

	if (create_domain_hook)
		create_domain_hook (domain);

	mono_profiler_appdomain_loaded (domain, MONO_PROFILE_OK);
	
	return domain;
}

/**
 * mono_init_internal:
 * 
 * Creates the initial application domain and initializes the mono_defaults
 * structure.
 * This function is guaranteed to not run any IL code.
 * If exe_filename is not NULL, the method will determine the required runtime
 * from the exe configuration file or the version PE field.
 * If runtime_version is not NULL, that runtime version will be used.
 * Either exe_filename or runtime_version must be provided.
 *
 * Returns: the initial domain.
 */
static MonoDomain *
mono_init_internal (const char *filename, const char *exe_filename, const char *runtime_version)
{
	static MonoDomain *domain = NULL;
	MonoAssembly *ass = NULL;
	MonoImageOpenStatus status = MONO_IMAGE_OK;
	const MonoRuntimeInfo* runtimes [G_N_ELEMENTS (supported_runtimes) + 1];
	int n, dummy;

#ifdef DEBUG_DOMAIN_UNLOAD
	debug_domain_unload = TRUE;
#endif

	if (domain)
		g_assert_not_reached ();

#ifdef HOST_WIN32
	/* Avoid system error message boxes. */
	SetErrorMode (SEM_FAILCRITICALERRORS | SEM_NOOPENFILEERRORBOX);
#endif

#ifndef HOST_WIN32
	wapi_init ();
#endif

#ifndef DISABLE_PERFCOUNTERS
	mono_perfcounters_init ();
#endif
	mono_counters_init ();

	mono_counters_register ("Max native code in a domain", MONO_COUNTER_INT|MONO_COUNTER_JIT, &max_domain_code_size);
	mono_counters_register ("Max code space allocated in a domain", MONO_COUNTER_INT|MONO_COUNTER_JIT, &max_domain_code_alloc);
	mono_counters_register ("Total code space allocated", MONO_COUNTER_INT|MONO_COUNTER_JIT, &total_domain_code_alloc);

	mono_gc_base_init ();
	mono_thread_info_attach (&dummy);

	MONO_FAST_TLS_INIT (tls_appdomain);
	mono_native_tls_alloc (&appdomain_thread_id, NULL);

	mono_coop_mutex_init_recursive (&appdomains_mutex);

	mono_metadata_init ();
	mono_images_init ();
	mono_assemblies_init ();
	mono_classes_init ();
	mono_loader_init ();
	mono_reflection_init ();
	mono_runtime_init_tls ();

	/* FIXME: When should we release this memory? */
	MONO_GC_REGISTER_ROOT_FIXED (appdomains_list, MONO_ROOT_SOURCE_DOMAIN, "domains list");

	domain = mono_domain_create ();
	mono_root_domain = domain;

	SET_APPDOMAIN (domain);
	
	/* Get a list of runtimes supported by the exe */
	if (exe_filename != NULL) {
		/*
		 * This function will load the exe file as a MonoImage. We need to close it, but
		 * that would mean it would be reloaded later. So instead, we save it to
		 * exe_image, and close it during shutdown.
		 */
		get_runtimes_from_exe (exe_filename, &exe_image, runtimes);
#ifdef HOST_WIN32
		if (!exe_image) {
			exe_image = mono_assembly_open_from_bundle (exe_filename, NULL, FALSE);
			if (!exe_image)
				exe_image = mono_image_open (exe_filename, NULL);
		}
		mono_fixup_exe_image (exe_image);
#endif
	} else if (runtime_version != NULL) {
		runtimes [0] = get_runtime_by_version (runtime_version);
		runtimes [1] = NULL;
	}

	if (runtimes [0] == NULL) {
		const MonoRuntimeInfo *default_runtime = get_runtime_by_version (DEFAULT_RUNTIME_VERSION);
		runtimes [0] = default_runtime;
		runtimes [1] = NULL;
		g_print ("WARNING: The runtime version supported by this application is unavailable.\n");
		g_print ("Using default runtime: %s\n", default_runtime->runtime_version); 
	}

	/* The selected runtime will be the first one for which there is a mscrolib.dll */
	for (n = 0; runtimes [n] != NULL && ass == NULL; n++) {
		current_runtime = runtimes [n];
		ass = mono_assembly_load_corlib (current_runtime, &status);
		if (status != MONO_IMAGE_OK && status != MONO_IMAGE_ERROR_ERRNO)
			break;

	}
	
	if ((status != MONO_IMAGE_OK) || (ass == NULL)) {
		switch (status){
		case MONO_IMAGE_ERROR_ERRNO: {
			char *corlib_file = g_build_filename (mono_assembly_getrootdir (), "mono", current_runtime->framework_version, "mscorlib.dll", NULL);
			g_print ("The assembly mscorlib.dll was not found or could not be loaded.\n");
			g_print ("It should have been installed in the `%s' directory.\n", corlib_file);
			g_free (corlib_file);
			break;
		}
		case MONO_IMAGE_IMAGE_INVALID:
			g_print ("The file %s/mscorlib.dll is an invalid CIL image\n",
				 mono_assembly_getrootdir ());
			break;
		case MONO_IMAGE_MISSING_ASSEMBLYREF:
			g_print ("Missing assembly reference in %s/mscorlib.dll\n",
				 mono_assembly_getrootdir ());
			break;
		case MONO_IMAGE_OK:
			/* to suppress compiler warning */
			break;
		}
		
		exit (1);
	}
	mono_defaults.corlib = mono_assembly_get_image (ass);

	mono_defaults.object_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "Object");

	mono_defaults.void_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "Void");

	mono_defaults.boolean_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "Boolean");

	mono_defaults.byte_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "Byte");

	mono_defaults.sbyte_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "SByte");

	mono_defaults.int16_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "Int16");

	mono_defaults.uint16_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "UInt16");

	mono_defaults.int32_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "Int32");

	mono_defaults.uint32_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "UInt32");

	mono_defaults.uint_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "UIntPtr");

	mono_defaults.int_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "IntPtr");

	mono_defaults.int64_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "Int64");

	mono_defaults.uint64_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "UInt64");

	mono_defaults.single_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "Single");

	mono_defaults.double_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "Double");

	mono_defaults.char_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "Char");

	mono_defaults.string_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "String");

	mono_defaults.enum_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "Enum");

	mono_defaults.array_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "Array");

	mono_defaults.delegate_class = mono_class_load_from_name (
		mono_defaults.corlib, "System", "Delegate");

	mono_defaults.multicastdelegate_class = mono_class_load_from_name (
		mono_defaults.corlib, "System", "MulticastDelegate");

	mono_defaults.asyncresult_class = mono_class_load_from_name (
		mono_defaults.corlib, "System.Runtime.Remoting.Messaging", 
		"AsyncResult");

	mono_defaults.manualresetevent_class = mono_class_load_from_name (
		mono_defaults.corlib, "System.Threading", "ManualResetEvent");

	mono_defaults.typehandle_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "RuntimeTypeHandle");

	mono_defaults.methodhandle_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "RuntimeMethodHandle");

	mono_defaults.fieldhandle_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "RuntimeFieldHandle");

	mono_defaults.systemtype_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "Type");

	mono_defaults.monotype_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "MonoType");

	mono_defaults.runtimetype_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "RuntimeType");

	mono_defaults.exception_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "Exception");

	mono_defaults.threadabortexception_class = mono_class_load_from_name (
                mono_defaults.corlib, "System.Threading", "ThreadAbortException");

	mono_defaults.thread_class = mono_class_load_from_name (
                mono_defaults.corlib, "System.Threading", "Thread");

	mono_defaults.internal_thread_class = mono_class_load_from_name (
                mono_defaults.corlib, "System.Threading", "InternalThread");

	mono_defaults.appdomain_class = mono_class_load_from_name (
                mono_defaults.corlib, "System", "AppDomain");

#ifndef DISABLE_REMOTING
	mono_defaults.transparent_proxy_class = mono_class_load_from_name (
                mono_defaults.corlib, "System.Runtime.Remoting.Proxies", "TransparentProxy");

	mono_defaults.real_proxy_class = mono_class_load_from_name (
                mono_defaults.corlib, "System.Runtime.Remoting.Proxies", "RealProxy");

	mono_defaults.marshalbyrefobject_class =  mono_class_load_from_name (
	        mono_defaults.corlib, "System", "MarshalByRefObject");

	mono_defaults.iremotingtypeinfo_class = mono_class_load_from_name (
	        mono_defaults.corlib, "System.Runtime.Remoting", "IRemotingTypeInfo");

#endif

	mono_defaults.mono_method_message_class = mono_class_load_from_name (
                mono_defaults.corlib, "System.Runtime.Remoting.Messaging", "MonoMethodMessage");

	mono_defaults.field_info_class = mono_class_load_from_name (
		mono_defaults.corlib, "System.Reflection", "FieldInfo");

	mono_defaults.method_info_class = mono_class_load_from_name (
		mono_defaults.corlib, "System.Reflection", "MethodInfo");

	mono_defaults.stringbuilder_class = mono_class_load_from_name (
		mono_defaults.corlib, "System.Text", "StringBuilder");

	mono_defaults.math_class = mono_class_load_from_name (
	        mono_defaults.corlib, "System", "Math");

	mono_defaults.stack_frame_class = mono_class_load_from_name (
	        mono_defaults.corlib, "System.Diagnostics", "StackFrame");

	mono_defaults.stack_trace_class = mono_class_load_from_name (
	        mono_defaults.corlib, "System.Diagnostics", "StackTrace");

	mono_defaults.marshal_class = mono_class_load_from_name (
	        mono_defaults.corlib, "System.Runtime.InteropServices", "Marshal");

	mono_defaults.typed_reference_class = mono_class_load_from_name (
	        mono_defaults.corlib, "System", "TypedReference");

	mono_defaults.argumenthandle_class = mono_class_load_from_name (
	        mono_defaults.corlib, "System", "RuntimeArgumentHandle");

	mono_defaults.monitor_class = mono_class_load_from_name (
	        mono_defaults.corlib, "System.Threading", "Monitor");
	/*
	Not using GENERATE_TRY_GET_CLASS_WITH_CACHE_DECL as this type is heavily checked by sgen when computing finalization.
	*/
	mono_defaults.critical_finalizer_object = mono_class_try_load_from_name (mono_defaults.corlib,
			"System.Runtime.ConstrainedExecution", "CriticalFinalizerObject");

	mono_assembly_load_friends (ass);

	mono_defaults.handleref_class = mono_class_load_from_name (
		mono_defaults.corlib, "System.Runtime.InteropServices", "HandleRef");

	mono_defaults.attribute_class = mono_class_load_from_name (
		mono_defaults.corlib, "System", "Attribute");

	mono_defaults.customattribute_data_class = mono_class_load_from_name (
		mono_defaults.corlib, "System.Reflection", "CustomAttributeData");

	mono_class_init (mono_defaults.array_class);
	mono_defaults.generic_nullable_class = mono_class_load_from_name (
		mono_defaults.corlib, "System", "Nullable`1");
	mono_defaults.generic_ilist_class = mono_class_load_from_name (
	        mono_defaults.corlib, "System.Collections.Generic", "IList`1");
	mono_defaults.generic_ireadonlylist_class = mono_class_load_from_name (
	        mono_defaults.corlib, "System.Collections.Generic", "IReadOnlyList`1");

	mono_defaults.threadpool_wait_callback_class = mono_class_load_from_name (
		mono_defaults.corlib, "System.Threading", "_ThreadPoolWaitCallback");

	mono_defaults.threadpool_perform_wait_callback_method = mono_class_get_method_from_name (
		mono_defaults.threadpool_wait_callback_class, "PerformWaitCallback", 0);

	domain->friendly_name = g_path_get_basename (filename);

	mono_profiler_appdomain_name (domain, domain->friendly_name);

	return domain;
}

/**
 * mono_init:
 * 
 * Creates the initial application domain and initializes the mono_defaults
 * structure.
 *
 * This function is guaranteed to not run any IL code.
 * The runtime is initialized using the default runtime version.
 *
 * Returns: the initial domain.
 */
MonoDomain *
mono_init (const char *domain_name)
{
	return mono_init_internal (domain_name, NULL, DEFAULT_RUNTIME_VERSION);
}

/**
 * mono_init_from_assembly:
 * @domain_name: name to give to the initial domain
 * @filename: filename to load on startup
 *
 * Used by the runtime, users should use mono_jit_init instead.
 *
 * Creates the initial application domain and initializes the mono_defaults
 * structure.
 * This function is guaranteed to not run any IL code.
 * The runtime is initialized using the runtime version required by the
 * provided executable. The version is determined by looking at the exe 
 * configuration file and the version PE field)
 *
 * Returns: the initial domain.
 */
MonoDomain *
mono_init_from_assembly (const char *domain_name, const char *filename)
{
	return mono_init_internal (domain_name, filename, NULL);
}

/**
 * mono_init_version:
 * 
 * Used by the runtime, users should use mono_jit_init instead.
 * 
 * Creates the initial application domain and initializes the mono_defaults
 * structure.
 *
 * This function is guaranteed to not run any IL code.
 * The runtime is initialized using the provided rutime version.
 *
 * Returns: the initial domain.
 */
MonoDomain *
mono_init_version (const char *domain_name, const char *version)
{
	return mono_init_internal (domain_name, NULL, version);
}

/**
 * mono_cleanup:
 *
 * Cleans up all metadata modules. 
 */
void
mono_cleanup (void)
{
	mono_close_exe_image ();

	mono_defaults.corlib = NULL;

	mono_config_cleanup ();
	mono_loader_cleanup ();
	mono_classes_cleanup ();
	mono_assemblies_cleanup ();
	mono_debug_cleanup ();
	mono_images_cleanup ();
	mono_metadata_cleanup ();

	mono_native_tls_free (appdomain_thread_id);
	mono_coop_mutex_destroy (&appdomains_mutex);

#ifndef HOST_WIN32
	wapi_cleanup ();
#endif
}

void
mono_close_exe_image (void)
{
	if (exe_image)
		mono_image_close (exe_image);
}

/**
 * mono_get_root_domain:
 *
 * The root AppDomain is the initial domain created by the runtime when it is
 * initialized.  Programs execute on this AppDomain, but can create new ones
 * later.   Currently there is no unmanaged API to create new AppDomains, this
 * must be done from managed code.
 *
 * Returns: the root appdomain, to obtain the current domain, use mono_domain_get ()
 */
MonoDomain*
mono_get_root_domain (void)
{
	return mono_root_domain;
}

/**
 * mono_domain_get:
 *
 * This method returns the value of the current MonoDomain that this thread
 * and code are running under.   To obtain the root domain use
 * mono_get_root_domain() API.
 *
 * Returns: the current domain
 */
MonoDomain *
mono_domain_get ()
{
	return GET_APPDOMAIN ();
}

void
mono_domain_unset (void)
{
	SET_APPDOMAIN (NULL);
}

void
mono_domain_set_internal_with_options (MonoDomain *domain, gboolean migrate_exception)
{
	MonoInternalThread *thread;

	if (mono_domain_get () == domain)
		return;

	SET_APPDOMAIN (domain);
	SET_APPCONTEXT (domain->default_context);

	if (migrate_exception) {
		thread = mono_thread_internal_current ();
		if (!thread->abort_exc)
			return;

		g_assert (thread->abort_exc->object.vtable->domain != domain);
		MONO_OBJECT_SETREF (thread, abort_exc, mono_get_exception_thread_abort ());
		g_assert (thread->abort_exc->object.vtable->domain == domain);
	}
}

/**
 * mono_domain_set_internal:
 * @domain: the new domain
 *
 * Sets the current domain to @domain.
 */
void
mono_domain_set_internal (MonoDomain *domain)
{
	mono_domain_set_internal_with_options (domain, TRUE);
}

/**
 * mono_domain_foreach:
 * @func: function to invoke with the domain data
 * @user_data: user-defined pointer that is passed to the supplied @func fo reach domain
 *
 * Use this method to safely iterate over all the loaded application
 * domains in the current runtime.   The provided @func is invoked with a
 * pointer to the MonoDomain and is given the value of the @user_data
 * parameter which can be used to pass state to your called routine.
 */
void
mono_domain_foreach (MonoDomainFunc func, gpointer user_data)
{
	int i, size;
	MonoDomain **copy;

	/*
	 * Create a copy of the data to avoid calling the user callback
	 * inside the lock because that could lead to deadlocks.
	 * We can do this because this function is not perf. critical.
	 */
	mono_appdomains_lock ();
	size = appdomain_list_size;
	copy = (MonoDomain **)mono_gc_alloc_fixed (appdomain_list_size * sizeof (void*), MONO_GC_DESCRIPTOR_NULL, MONO_ROOT_SOURCE_DOMAIN, "temporary domains list");
	memcpy (copy, appdomains_list, appdomain_list_size * sizeof (void*));
	mono_appdomains_unlock ();

	for (i = 0; i < size; ++i) {
		if (copy [i])
			func (copy [i], user_data);
	}

	mono_gc_free_fixed (copy);
}

/**
 * mono_domain_assembly_open:
 * @domain: the application domain
 * @name: file name of the assembly
 *
 * fixme: maybe we should integrate this with mono_assembly_open ??
 */
MonoAssembly *
mono_domain_assembly_open (MonoDomain *domain, const char *name)
{
	MonoDomain *current;
	MonoAssembly *ass;
	GSList *tmp;

	mono_domain_assemblies_lock (domain);
	for (tmp = domain->domain_assemblies; tmp; tmp = tmp->next) {
		ass = (MonoAssembly *)tmp->data;
		if (strcmp (name, ass->aname.name) == 0) {
			mono_domain_assemblies_unlock (domain);
			return ass;
		}
	}
	mono_domain_assemblies_unlock (domain);

	if (domain != mono_domain_get ()) {
		current = mono_domain_get ();

		mono_domain_set (domain, FALSE);
		ass = mono_assembly_open (name, NULL);
		mono_domain_set (current, FALSE);
	} else {
		ass = mono_assembly_open (name, NULL);
	}

	return ass;
}

static void
unregister_vtable_reflection_type (MonoVTable *vtable)
{
	MonoObject *type = (MonoObject *)vtable->type;

	if (type->vtable->klass != mono_defaults.monotype_class)
		MONO_GC_UNREGISTER_ROOT_IF_MOVING (vtable->type);
}

/**
 * mono_domain_free:
 * @domain: the domain to release
 * @force: if true, it allows the root domain to be released (used at shutdown only).
 *
 * This releases the resources associated with the specific domain.
 * This is a low-level function that is invoked by the AppDomain infrastructure
 * when necessary.
 */
void
mono_domain_free (MonoDomain *domain, gboolean force)
{
	int code_size, code_alloc;
	GSList *tmp;
	gpointer *p;

	if ((domain == mono_root_domain) && !force) {
		g_warning ("cant unload root domain");
		return;
	}

	if (mono_dont_free_domains)
		return;

	mono_profiler_appdomain_event (domain, MONO_PROFILE_START_UNLOAD);

	mono_debug_domain_unload (domain);

	mono_appdomains_lock ();
	appdomains_list [domain->domain_id] = NULL;
	mono_appdomains_unlock ();

	/* must do this early as it accesses fields and types */
	if (domain->special_static_fields) {
		mono_alloc_special_static_data_free (domain->special_static_fields);
		g_hash_table_destroy (domain->special_static_fields);
		domain->special_static_fields = NULL;
	}

	/*
	 * We must destroy all these hash tables here because they
	 * contain references to managed objects belonging to the
	 * domain.  Once we let the GC clear the domain there must be
	 * no more such references, or we'll crash if a collection
	 * occurs.
	 */
	mono_g_hash_table_destroy (domain->ldstr_table);
	domain->ldstr_table = NULL;

	mono_g_hash_table_destroy (domain->env);
	domain->env = NULL;

	mono_reflection_cleanup_domain (domain);

	/* This must be done before type_hash is freed */
	if (domain->class_vtable_array) {
		int i;
		for (i = 0; i < domain->class_vtable_array->len; ++i)
			unregister_vtable_reflection_type ((MonoVTable *)g_ptr_array_index (domain->class_vtable_array, i));
	}

	if (domain->type_hash) {
		mono_g_hash_table_destroy (domain->type_hash);
		domain->type_hash = NULL;
	}
	if (domain->type_init_exception_hash) {
		mono_g_hash_table_destroy (domain->type_init_exception_hash);
		domain->type_init_exception_hash = NULL;
	}

	for (tmp = domain->domain_assemblies; tmp; tmp = tmp->next) {
		MonoAssembly *ass = (MonoAssembly *)tmp->data;
		mono_assembly_release_gc_roots (ass);
	}

	/* Have to zero out reference fields since they will be invalidated by the clear_domain () call below */
	for (p = (gpointer*)&domain->MONO_DOMAIN_FIRST_OBJECT; p < (gpointer*)&domain->MONO_DOMAIN_FIRST_GC_TRACKED; ++p)
		*p = NULL;

	/* This needs to be done before closing assemblies */
	mono_gc_clear_domain (domain);

	/* Close dynamic assemblies first, since they have no ref count */
	for (tmp = domain->domain_assemblies; tmp; tmp = tmp->next) {
		MonoAssembly *ass = (MonoAssembly *)tmp->data;
		if (!ass->image || !image_is_dynamic (ass->image))
			continue;
		mono_trace (G_LOG_LEVEL_INFO, MONO_TRACE_ASSEMBLY, "Unloading domain %s[%p], assembly %s[%p], ref_count=%d", domain->friendly_name, domain, ass->aname.name, ass, ass->ref_count);
		if (!mono_assembly_close_except_image_pools (ass))
			tmp->data = NULL;
	}

	for (tmp = domain->domain_assemblies; tmp; tmp = tmp->next) {
		MonoAssembly *ass = (MonoAssembly *)tmp->data;
		if (!ass)
			continue;
		if (!ass->image || image_is_dynamic (ass->image))
			continue;
		mono_trace (G_LOG_LEVEL_INFO, MONO_TRACE_ASSEMBLY, "Unloading domain %s[%p], assembly %s[%p], ref_count=%d", domain->friendly_name, domain, ass->aname.name, ass, ass->ref_count);
		if (!mono_assembly_close_except_image_pools (ass))
			tmp->data = NULL;
	}

	for (tmp = domain->domain_assemblies; tmp; tmp = tmp->next) {
		MonoAssembly *ass = (MonoAssembly *)tmp->data;
		if (ass)
			mono_assembly_close_finish (ass);
	}
	g_slist_free (domain->domain_assemblies);
	domain->domain_assemblies = NULL;

	/* 
	 * Send this after the assemblies have been unloaded and the domain is still in a 
	 * usable state.
	 */
	mono_profiler_appdomain_event (domain, MONO_PROFILE_END_UNLOAD);

	if (free_domain_hook)
		free_domain_hook (domain);

	/* FIXME: free delegate_hash_table when it's used */
	if (domain->search_path) {
		g_strfreev (domain->search_path);
		domain->search_path = NULL;
	}
	domain->create_proxy_for_type_method = NULL;
	domain->private_invoke_method = NULL;
	domain->default_context = NULL;
	domain->out_of_memory_ex = NULL;
	domain->null_reference_ex = NULL;
	domain->stack_overflow_ex = NULL;
	domain->ephemeron_tombstone = NULL;
	domain->entry_assembly = NULL;

	g_free (domain->friendly_name);
	domain->friendly_name = NULL;
	g_ptr_array_free (domain->class_vtable_array, TRUE);
	domain->class_vtable_array = NULL;
	g_hash_table_destroy (domain->proxy_vtable_hash);
	domain->proxy_vtable_hash = NULL;
	if (domain->static_data_array) {
		mono_gc_free_fixed (domain->static_data_array);
		domain->static_data_array = NULL;
	}
	mono_internal_hash_table_destroy (&domain->jit_code_hash);

	/*
	 * There might still be jit info tables of this domain which
	 * are not freed.  Since the domain cannot be in use anymore,
	 * this will free them.
	 */
	mono_thread_hazardous_try_free_all ();
	if (domain->aot_modules)
		mono_jit_info_table_free (domain->aot_modules);
	g_assert (domain->num_jit_info_tables == 1);
	mono_jit_info_table_free (domain->jit_info_table);
	domain->jit_info_table = NULL;
	g_assert (!domain->jit_info_free_queue);

	/* collect statistics */
	code_alloc = mono_code_manager_size (domain->code_mp, &code_size);
	total_domain_code_alloc += code_alloc;
	max_domain_code_alloc = MAX (max_domain_code_alloc, code_alloc);
	max_domain_code_size = MAX (max_domain_code_size, code_size);

	if (debug_domain_unload) {
		mono_mempool_invalidate (domain->mp);
		mono_code_manager_invalidate (domain->code_mp);
	} else {
#ifndef DISABLE_PERFCOUNTERS
		mono_perfcounters->loader_bytes -= mono_mempool_get_allocated (domain->mp);
#endif
		mono_mempool_destroy (domain->mp);
		domain->mp = NULL;
		mono_code_manager_destroy (domain->code_mp);
		domain->code_mp = NULL;
	}
	lock_free_mempool_free (domain->lock_free_mp);
	domain->lock_free_mp = NULL;

	g_hash_table_destroy (domain->finalizable_objects_hash);
	domain->finalizable_objects_hash = NULL;
	if (domain->method_rgctx_hash) {
		g_hash_table_destroy (domain->method_rgctx_hash);
		domain->method_rgctx_hash = NULL;
	}
	if (domain->generic_virtual_cases) {
		g_hash_table_destroy (domain->generic_virtual_cases);
		domain->generic_virtual_cases = NULL;
	}
	if (domain->generic_virtual_thunks) {
		g_hash_table_destroy (domain->generic_virtual_thunks);
		domain->generic_virtual_thunks = NULL;
	}
	if (domain->ftnptrs_hash) {
		g_hash_table_destroy (domain->ftnptrs_hash);
		domain->ftnptrs_hash = NULL;
	}
	if (domain->method_to_dyn_method) {
		g_hash_table_destroy (domain->method_to_dyn_method);
		domain->method_to_dyn_method = NULL;
	}

	mono_os_mutex_destroy (&domain->finalizable_objects_hash_lock);
	mono_os_mutex_destroy (&domain->assemblies_lock);
	mono_os_mutex_destroy (&domain->jit_code_hash_lock);

	mono_coop_mutex_destroy (&domain->lock);

	domain->setup = NULL;

	mono_gc_deregister_root ((char*)&(domain->MONO_DOMAIN_FIRST_GC_TRACKED));

	/* FIXME: anything else required ? */

	mono_gc_free_fixed (domain);

#ifndef DISABLE_PERFCOUNTERS
	mono_perfcounters->loader_appdomains--;
#endif

	if (domain == mono_root_domain)
		mono_root_domain = NULL;
}

/**
 * mono_domain_get_by_id:
 * @domainid: the ID
 *
 * Returns: the domain for a specific domain id.
 */
MonoDomain * 
mono_domain_get_by_id (gint32 domainid) 
{
	MonoDomain * domain;

	mono_appdomains_lock ();
	if (domainid < appdomain_list_size)
		domain = appdomains_list [domainid];
	else
		domain = NULL;
	mono_appdomains_unlock ();

	return domain;
}

/*
 * mono_domain_get_id:
 *
 * A domain ID is guaranteed to be unique for as long as the domain
 * using it is alive. It may be reused later once the domain has been
 * unloaded.
 *
 * Returns: The unique ID for @domain.
 */
gint32
mono_domain_get_id (MonoDomain *domain)
{
	return domain->domain_id;
}

/*
 * mono_domain_get_friendly_name:
 *
 * The returned string's lifetime is the same as @domain's. Consider
 * copying it if you need to store it somewhere.
 *
 * Returns: The friendly name of @domain. Can be NULL if not yet set.
 */
const char *
mono_domain_get_friendly_name (MonoDomain *domain)
{
	return domain->friendly_name;
}

/*
 * mono_domain_alloc:
 *
 * LOCKING: Acquires the domain lock.
 */
gpointer
mono_domain_alloc (MonoDomain *domain, guint size)
{
	gpointer res;

	mono_domain_lock (domain);
#ifndef DISABLE_PERFCOUNTERS
	mono_perfcounters->loader_bytes += size;
#endif
	res = mono_mempool_alloc (domain->mp, size);
	mono_domain_unlock (domain);

	return res;
}

/*
 * mono_domain_alloc0:
 *
 * LOCKING: Acquires the domain lock.
 */
gpointer
mono_domain_alloc0 (MonoDomain *domain, guint size)
{
	gpointer res;

	mono_domain_lock (domain);
#ifndef DISABLE_PERFCOUNTERS
	mono_perfcounters->loader_bytes += size;
#endif
	res = mono_mempool_alloc0 (domain->mp, size);
	mono_domain_unlock (domain);

	return res;
}

gpointer
mono_domain_alloc0_lock_free (MonoDomain *domain, guint size)
{
	return lock_free_mempool_alloc0 (domain->lock_free_mp, size);
}

/*
 * mono_domain_code_reserve:
 *
 * LOCKING: Acquires the domain lock.
 */
void*
mono_domain_code_reserve (MonoDomain *domain, int size)
{
	gpointer res;

	mono_domain_lock (domain);
	res = mono_code_manager_reserve (domain->code_mp, size);
	mono_domain_unlock (domain);

	return res;
}

/*
 * mono_domain_code_reserve_align:
 *
 * LOCKING: Acquires the domain lock.
 */
void*
mono_domain_code_reserve_align (MonoDomain *domain, int size, int alignment)
{
	gpointer res;

	mono_domain_lock (domain);
	res = mono_code_manager_reserve_align (domain->code_mp, size, alignment);
	mono_domain_unlock (domain);

	return res;
}

/*
 * mono_domain_code_commit:
 *
 * LOCKING: Acquires the domain lock.
 */
void
mono_domain_code_commit (MonoDomain *domain, void *data, int size, int newsize)
{
	mono_domain_lock (domain);
	mono_code_manager_commit (domain->code_mp, data, size, newsize);
	mono_domain_unlock (domain);
}

#if defined(__native_client_codegen__) && defined(__native_client__)
/*
 * Given the temporary buffer (allocated by mono_domain_code_reserve) into which
 * we are generating code, return a pointer to the destination in the dynamic 
 * code segment into which the code will be copied when mono_domain_code_commit
 * is called.
 * LOCKING: Acquires the domain lock.
 */
void *
nacl_domain_get_code_dest (MonoDomain *domain, void *data)
{
	void *dest;
	mono_domain_lock (domain);
	dest = nacl_code_manager_get_code_dest (domain->code_mp, data);
	mono_domain_unlock (domain);
	return dest;
}

/* 
 * Convenience function which calls mono_domain_code_commit to validate and copy
 * the code. The caller sets *buf_base and *buf_size to the start and size of
 * the buffer (allocated by mono_domain_code_reserve), and *code_end to the byte
 * after the last instruction byte. On return, *buf_base will point to the start
 * of the copied in the code segment, and *code_end will point after the end of 
 * the copied code.
 */
void
nacl_domain_code_validate (MonoDomain *domain, guint8 **buf_base, int buf_size, guint8 **code_end)
{
	guint8 *tmp = nacl_domain_get_code_dest (domain, *buf_base);
	mono_domain_code_commit (domain, *buf_base, buf_size, *code_end - *buf_base);
	*code_end = tmp + (*code_end - *buf_base);
	*buf_base = tmp;
}

#else

/* no-op versions of Native Client functions */

void *
nacl_domain_get_code_dest (MonoDomain *domain, void *data)
{
	return data;
}

void
nacl_domain_code_validate (MonoDomain *domain, guint8 **buf_base, int buf_size, guint8 **code_end)
{
}

#endif

/*
 * mono_domain_code_foreach:
 * Iterate over the code thunks of the code manager of @domain.
 * 
 * The @func callback MUST not take any locks. If it really needs to, it must respect
 * the locking rules of the runtime: http://www.mono-project.com/Mono:Runtime:Documentation:ThreadSafety 
 * LOCKING: Acquires the domain lock.
 */

void
mono_domain_code_foreach (MonoDomain *domain, MonoCodeManagerFunc func, void *user_data)
{
	mono_domain_lock (domain);
	mono_code_manager_foreach (domain->code_mp, func, user_data);
	mono_domain_unlock (domain);
}


void 
mono_context_set (MonoAppContext * new_context)
{
	SET_APPCONTEXT (new_context);
}

/**
 * mono_context_get:
 *
 * Returns: the current Mono Application Context.
 */
MonoAppContext * 
mono_context_get (void)
{
	return GET_APPCONTEXT ();
}

/**
 * mono_context_get_id:
 * @context: the context to operate on.
 *
 * Context IDs are guaranteed to be unique for the duration of a Mono
 * process; they are never reused.
 *
 * Returns: The unique ID for @context.
 */
gint32
mono_context_get_id (MonoAppContext *context)
{
	return context->context_id;
}

/**
 * mono_context_get_domain_id:
 * @context: the context to operate on.
 *
 * Returns: The ID of the domain that @context was created in.
 */
gint32
mono_context_get_domain_id (MonoAppContext *context)
{
	return context->domain_id;
}

/* LOCKING: the caller holds the lock for this domain */
void
mono_domain_add_class_static_data (MonoDomain *domain, MonoClass *klass, gpointer data, guint32 *bitmap)
{
	/* The first entry in the array is the index of the next free slot
	 * and the total size of the array
	 */
	int next;
	if (domain->static_data_array) {
		int size = GPOINTER_TO_INT (domain->static_data_array [1]);
		next = GPOINTER_TO_INT (domain->static_data_array [0]);
		if (next >= size) {
			/* 'data' is allocated by alloc_fixed */
			gpointer *new_array = (gpointer *)mono_gc_alloc_fixed (sizeof (gpointer) * (size * 2), MONO_GC_ROOT_DESCR_FOR_FIXED (size * 2), MONO_ROOT_SOURCE_DOMAIN, "static field list");
			mono_gc_memmove_aligned (new_array, domain->static_data_array, sizeof (gpointer) * size);
			size *= 2;
			new_array [1] = GINT_TO_POINTER (size);
			mono_gc_free_fixed (domain->static_data_array);
			domain->static_data_array = new_array;
		}
	} else {
		int size = 32;
		gpointer *new_array = (gpointer *)mono_gc_alloc_fixed (sizeof (gpointer) * size, MONO_GC_ROOT_DESCR_FOR_FIXED (size), MONO_ROOT_SOURCE_DOMAIN, "static field list");
		next = 2;
		new_array [0] = GINT_TO_POINTER (next);
		new_array [1] = GINT_TO_POINTER (size);
		domain->static_data_array = new_array;
	}
	domain->static_data_array [next++] = data;
	domain->static_data_array [0] = GINT_TO_POINTER (next);
}

/**
 * mono_get_corlib:
 *
 * Use this function to get the `MonoImage*` for the mscorlib.dll assembly
 *
 * Returns: The MonoImage for mscorlib.dll
 */
MonoImage*
mono_get_corlib (void)
{
	return mono_defaults.corlib;
}

/**
 * mono_get_object_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.Object`.
 *
 * Returns: The `MonoClass*` for the `System.Object` type.
 */
MonoClass*
mono_get_object_class (void)
{
	return mono_defaults.object_class;
}

/**
 * mono_get_byte_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.Byte`.
 *
 * Returns: The `MonoClass*` for the `System.Byte` type.
 */
MonoClass*
mono_get_byte_class (void)
{
	return mono_defaults.byte_class;
}

/**
 * mono_get_void_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.Void`.
 *
 * Returns: The `MonoClass*` for the `System.Void` type.
 */
MonoClass*
mono_get_void_class (void)
{
	return mono_defaults.void_class;
}

/**
 * mono_get_boolean_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.Boolean`.
 *
 * Returns: The `MonoClass*` for the `System.Boolean` type.
 */
MonoClass*
mono_get_boolean_class (void)
{
	return mono_defaults.boolean_class;
}

/**
 * mono_get_sbyte_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.SByte`.
 *
 * Returns: The `MonoClass*` for the `System.SByte` type.
 */
MonoClass*
mono_get_sbyte_class (void)
{
	return mono_defaults.sbyte_class;
}

/**
 * mono_get_int16_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.Int16`.
 *
 * Returns: The `MonoClass*` for the `System.Int16` type.
 */
MonoClass*
mono_get_int16_class (void)
{
	return mono_defaults.int16_class;
}

/**
 * mono_get_uint16_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.UInt16`.
 *
 * Returns: The `MonoClass*` for the `System.UInt16` type.
 */
MonoClass*
mono_get_uint16_class (void)
{
	return mono_defaults.uint16_class;
}

/**
 * mono_get_int32_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.Int32`.
 *
 * Returns: The `MonoClass*` for the `System.Int32` type.
 */
MonoClass*
mono_get_int32_class (void)
{
	return mono_defaults.int32_class;
}

/**
 * mono_get_uint32_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.UInt32`.
 *
 * Returns: The `MonoClass*` for the `System.UInt32` type.
 */
MonoClass*
mono_get_uint32_class (void)
{
	return mono_defaults.uint32_class;
}

/**
 * mono_get_intptr_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.IntPtr`.
 *
 * Returns: The `MonoClass*` for the `System.IntPtr` type.
 */
MonoClass*
mono_get_intptr_class (void)
{
	return mono_defaults.int_class;
}

/**
 * mono_get_uintptr_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.UIntPtr`.
 *
 * Returns: The `MonoClass*` for the `System.UIntPtr` type.
 */
MonoClass*
mono_get_uintptr_class (void)
{
	return mono_defaults.uint_class;
}

/**
 * mono_get_int64_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.Int64`.
 *
 * Returns: The `MonoClass*` for the `System.Int64` type.
 */
MonoClass*
mono_get_int64_class (void)
{
	return mono_defaults.int64_class;
}

/**
 * mono_get_uint64_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.UInt64`.
 *
 * Returns: The `MonoClass*` for the `System.UInt64` type.
 */
MonoClass*
mono_get_uint64_class (void)
{
	return mono_defaults.uint64_class;
}

/**
 * mono_get_single_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.Single` (32-bit floating points).
 *
 * Returns: The `MonoClass*` for the `System.Single` type.
 */
MonoClass*
mono_get_single_class (void)
{
	return mono_defaults.single_class;
}

/**
 * mono_get_double_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.Double` (64-bit floating points).
 *
 * Returns: The `MonoClass*` for the `System.Double` type.
 */
MonoClass*
mono_get_double_class (void)
{
	return mono_defaults.double_class;
}

/**
 * mono_get_char_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.Char`.
 *
 * Returns: The `MonoClass*` for the `System.Char` type.
 */
MonoClass*
mono_get_char_class (void)
{
	return mono_defaults.char_class;
}

/**
 * mono_get_string_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.String`.
 *
 * Returns: The `MonoClass*` for the `System.String` type.
 */
MonoClass*
mono_get_string_class (void)
{
	return mono_defaults.string_class;
}

/**
 * mono_get_enum_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.Enum`.
 *
 * Returns: The `MonoClass*` for the `System.Enum` type.
 */
MonoClass*
mono_get_enum_class (void)
{
	return mono_defaults.enum_class;
}

/**
 * mono_get_array_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.Array`.
 *
 * Returns: The `MonoClass*` for the `System.Array` type.
 */
MonoClass*
mono_get_array_class (void)
{
	return mono_defaults.array_class;
}

/**
 * mono_get_thread_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.Threading.Thread`.
 *
 * Returns: The `MonoClass*` for the `System.Threading.Thread` type.
 */
MonoClass*
mono_get_thread_class (void)
{
	return mono_defaults.thread_class;
}

/**
 * mono_get_exception_class:
 *
 * Use this function to get the `MonoClass*` that the runtime is using for `System.Exception`.
 *
 * Returns: The `MonoClass*` for the `` type.
 */
MonoClass*
mono_get_exception_class (void)
{
	return mono_defaults.exception_class;
}


static char* get_attribute_value (const gchar **attribute_names, 
					const gchar **attribute_values, 
					const char *att_name)
{
	int n;
	for (n=0; attribute_names[n] != NULL; n++) {
		if (strcmp (attribute_names[n], att_name) == 0)
			return g_strdup (attribute_values[n]);
	}
	return NULL;
}

static void start_element (GMarkupParseContext *context, 
                           const gchar         *element_name,
			   const gchar        **attribute_names,
			   const gchar        **attribute_values,
			   gpointer             user_data,
			   GError             **error)
{
	AppConfigInfo* app_config = (AppConfigInfo*) user_data;
	
	if (strcmp (element_name, "configuration") == 0) {
		app_config->configuration_count++;
		return;
	}
	if (strcmp (element_name, "startup") == 0) {
		app_config->startup_count++;
		return;
	}
	
	if (app_config->configuration_count != 1 || app_config->startup_count != 1)
		return;
	
	if (strcmp (element_name, "requiredRuntime") == 0) {
		app_config->required_runtime = get_attribute_value (attribute_names, attribute_values, "version");
	} else if (strcmp (element_name, "supportedRuntime") == 0) {
		char *version = get_attribute_value (attribute_names, attribute_values, "version");
		app_config->supported_runtimes = g_slist_append (app_config->supported_runtimes, version);
	}
}

static void end_element   (GMarkupParseContext *context,
                           const gchar         *element_name,
			   gpointer             user_data,
			   GError             **error)
{
	AppConfigInfo* app_config = (AppConfigInfo*) user_data;
	
	if (strcmp (element_name, "configuration") == 0) {
		app_config->configuration_count--;
	} else if (strcmp (element_name, "startup") == 0) {
		app_config->startup_count--;
	}
}

static const GMarkupParser 
mono_parser = {
	start_element,
	end_element,
	NULL,
	NULL,
	NULL
};

static AppConfigInfo *
app_config_parse (const char *exe_filename)
{
	AppConfigInfo *app_config;
	GMarkupParseContext *context;
	char *text;
	gsize len;
	const char *bundled_config;
	char *config_filename;

	bundled_config = mono_config_string_for_assembly_file (exe_filename);

	if (bundled_config) {
		text = g_strdup (bundled_config);
		len = strlen (text);
	} else {
		config_filename = g_strconcat (exe_filename, ".config", NULL);

		if (!g_file_get_contents (config_filename, &text, &len, NULL)) {
			g_free (config_filename);
			return NULL;
		}
		g_free (config_filename);
	}

	app_config = g_new0 (AppConfigInfo, 1);

	context = g_markup_parse_context_new (&mono_parser, (GMarkupParseFlags)0, app_config, NULL);
	if (g_markup_parse_context_parse (context, text, len, NULL)) {
		g_markup_parse_context_end_parse (context, NULL);
	}
	g_markup_parse_context_free (context);
	g_free (text);
	return app_config;
}

static void 
app_config_free (AppConfigInfo* app_config)
{
	char *rt;
	GSList *list = app_config->supported_runtimes;
	while (list != NULL) {
		rt = (char*)list->data;
		g_free (rt);
		list = g_slist_next (list);
	}
	g_slist_free (app_config->supported_runtimes);
	g_free (app_config->required_runtime);
	g_free (app_config);
}


static const MonoRuntimeInfo*
get_runtime_by_version (const char *version)
{
	int n;
	int max = G_N_ELEMENTS (supported_runtimes);
	int vlen;

	if (!version)
		return NULL;

	for (n=0; n<max; n++) {
		if (strcmp (version, supported_runtimes[n].runtime_version) == 0)
			return &supported_runtimes[n];
	}
	
	vlen = strlen (version);
	if (vlen >= 4 && version [1] - '0' >= 4) {
		for (n=0; n<max; n++) {
			if (strncmp (version, supported_runtimes[n].runtime_version, 4) == 0)
				return &supported_runtimes[n];
		}
	}
	
	return NULL;
}

static void
get_runtimes_from_exe (const char *exe_file, MonoImage **exe_image, const MonoRuntimeInfo** runtimes)
{
	AppConfigInfo* app_config;
	char *version;
	const MonoRuntimeInfo* runtime = NULL;
	MonoImage *image = NULL;
	
	app_config = app_config_parse (exe_file);
	
	if (app_config != NULL) {
		/* Check supportedRuntime elements, if none is supported, fail.
		 * If there are no such elements, look for a requiredRuntime element.
		 */
		if (app_config->supported_runtimes != NULL) {
			int n = 0;
			GSList *list = app_config->supported_runtimes;
			while (list != NULL) {
				version = (char*) list->data;
				runtime = get_runtime_by_version (version);
				if (runtime != NULL)
					runtimes [n++] = runtime;
				list = g_slist_next (list);
			}
			runtimes [n] = NULL;
			app_config_free (app_config);
			return;
		}
		
		/* Check the requiredRuntime element. This is for 1.0 apps only. */
		if (app_config->required_runtime != NULL) {
			runtimes [0] = get_runtime_by_version (app_config->required_runtime);
			runtimes [1] = NULL;
			app_config_free (app_config);
			return;
		}
		app_config_free (app_config);
	}
	
	/* Look for a runtime with the exact version */
	image = mono_assembly_open_from_bundle (exe_file, NULL, FALSE);

	if (image == NULL)
		image = mono_image_open (exe_file, NULL);

	if (image == NULL) {
		/* The image is wrong or the file was not found. In this case return
		 * a default runtime and leave to the initialization method the work of
		 * reporting the error.
		 */
		runtimes [0] = get_runtime_by_version (DEFAULT_RUNTIME_VERSION);
		runtimes [1] = NULL;
		return;
	}

	*exe_image = image;

	runtimes [0] = get_runtime_by_version (image->version);
	runtimes [1] = NULL;
}


/**
 * mono_get_runtime_info:
 *
 * Returns: the version of the current runtime instance.
 */
const MonoRuntimeInfo*
mono_get_runtime_info (void)
{
	return current_runtime;
}

/**
 * mono_framework_version:
 *
 * Return the major version of the framework curently executing.
 */
int
mono_framework_version (void)
{
	return current_runtime->framework_version [0] - '0';
}

void
mono_enable_debug_domain_unload (gboolean enable)
{
	debug_domain_unload = enable;
}

MonoAotCacheConfig *
mono_get_aot_cache_config (void)
{
	return &aot_cache_config;
}

void
mono_domain_lock (MonoDomain *domain)
{
	mono_locks_coop_acquire (&domain->lock, DomainLock);
}

void
mono_domain_unlock (MonoDomain *domain)
{
	mono_locks_coop_release (&domain->lock, DomainLock);
}
