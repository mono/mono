/**
* \file
*/

#ifndef _MONO_METADATA_LOADER_INTERNALS_H_
#define _MONO_METADATA_LOADER_INTERNALS_H_

#include <glib.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/image.h>
#include <mono/metadata/mempool-internals.h>
#include <mono/metadata/mono-conc-hash.h>
#include <mono/metadata/mono-hash.h>
#include <mono/metadata/object-forward.h>
#include <mono/utils/mono-codeman.h>
#include <mono/utils/mono-coop-mutex.h>
#include <mono/utils/mono-error.h>
#include <mono/utils/mono-forward.h>


G_BEGIN_DECLS

typedef struct _MonoLoadedImages MonoLoadedImages;
typedef struct _MonoAssemblyLoadContext MonoAssemblyLoadContext;
typedef struct _MonoMemoryManager MonoMemoryManager;
typedef struct _MonoSingletonMemoryManager MonoSingletonMemoryManager;

struct _MonoBundledSatelliteAssembly {
	const char *name;
	const char *culture;
	const unsigned char *data;
	unsigned int size;
};

#ifndef DISABLE_DLLMAP
typedef struct _MonoDllMap MonoDllMap;
struct _MonoDllMap {
	char *dll;
	char *target;
	char *func;
	char *target_func;
	MonoDllMap *next;
};
#endif


struct _MonoMemoryManager {
	MonoDomain *domain;
	// Whether the MemoryManager can be unloaded on netcore; should only be set at creation
	gboolean collectible;
	// Whether this is a singleton or generic MemoryManager
	gboolean is_generic;
	// Whether the MemoryManager is in the process of being freed
	gboolean freeing;

	// If taking this with the loader lock, always take this second
	// Currently unused, we take the domain lock instead
	MonoCoopMutex lock;

	MonoMemPool *mp;
	MonoCodeManager *code_mp;

	GPtrArray *class_vtable_array;

	// !!! REGISTERED AS GC ROOTS !!!
	// Hashtables for Reflection handles
	MonoGHashTable *type_hash;
	MonoConcGHashTable *refobject_hash;
	// Maps class -> type initializaiton exception object
	MonoGHashTable *type_init_exception_hash;
	// Maps delegate trampoline addr -> delegate object
	//MonoGHashTable *delegate_hash_table;
	// End of GC roots
};

struct _MonoSingletonMemoryManager {
	MonoMemoryManager memory_manager;

	// Parent ALC, NULL on framework
	MonoAssemblyLoadContext *alc;
};


void
mono_global_loader_data_lock (void);

void
mono_global_loader_data_unlock (void);

gpointer
mono_lookup_pinvoke_call_internal (MonoMethod *method, MonoError *error);

#ifndef DISABLE_DLLMAP
void
mono_dllmap_insert_internal (MonoImage *assembly, const char *dll, const char *func, const char *tdll, const char *tfunc);

void
mono_global_dllmap_cleanup (void);
#endif

void
mono_global_loader_cache_init (void);

void
mono_global_loader_cache_cleanup (void);


static inline MonoDomain *
mono_alc_domain (MonoAssemblyLoadContext *alc)
{
	return mono_domain_get ();
}

MonoLoadedImages *
mono_alc_get_loaded_images (MonoAssemblyLoadContext *alc);

MONO_API void
mono_loader_save_bundled_library (int fd, uint64_t offset, uint64_t size, const char *destfname);

MonoSingletonMemoryManager *
mono_mem_manager_create_singleton (MonoAssemblyLoadContext *alc, MonoDomain *domain, gboolean collectible);

void
mono_mem_manager_free_singleton (MonoSingletonMemoryManager *memory_manager, gboolean debug_unload);

void
mono_mem_manager_free_objects_singleton (MonoSingletonMemoryManager *memory_manager);

void
mono_mem_manager_lock (MonoMemoryManager *memory_manager);

void
mono_mem_manager_unlock (MonoMemoryManager *memory_manager);

void *
mono_mem_manager_alloc (MonoMemoryManager *memory_manager, guint size);

void *
mono_mem_manager_alloc_nolock (MonoMemoryManager *memory_manager, guint size);

void *
mono_mem_manager_alloc0 (MonoMemoryManager *memory_manager, guint size);

void *
mono_mem_manager_alloc0_nolock (MonoMemoryManager *memory_manager, guint size);

void *
mono_mem_manager_code_reserve (MonoMemoryManager *memory_manager, int size);

#define mono_mem_manager_code_reserve(mem_manager, size) (g_cast (mono_mem_manager_code_reserve ((mem_manager), (size))))

void *
mono_mem_manager_code_reserve_align (MonoMemoryManager *memory_manager, int size, int newsize);

void
mono_mem_manager_code_commit (MonoMemoryManager *memory_manager, void *data, int size, int newsize);

void
mono_mem_manager_code_foreach (MonoMemoryManager *memory_manager, MonoCodeManagerFunc func, void *user_data);

char*
mono_mem_manager_strdup (MonoMemoryManager *memory_manager, const char *s);

G_END_DECLS

#endif
