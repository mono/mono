/**
* \file
*/

#ifndef _MONO_METADATA_LOADER_INTERNALS_H_
#define _MONO_METADATA_LOADER_INTERNALS_H_

#include <glib.h>
#include <mono/metadata/object-forward.h>
#include <mono/utils/mono-forward.h>
#include <mono/utils/mono-error.h>

typedef struct _MonoLoadedImages MonoLoadedImages;
typedef struct _MonoAssemblyLoadContext MonoAssemblyLoadContext;

#ifdef ENABLE_NETCORE
/* FIXME: this probably belongs somewhere else */
struct _MonoAssemblyLoadContext {
	MonoDomain *domain;
	MonoLoadedImages *loaded_images;
#if 0
	GSList *loaded_assemblies;
	MonoCoopMutex assemblies_lock;
#endif
};
#endif /* ENABLE_NETCORE */

enum {
	MONO_LOADED_IMAGES_ALC = 0,  /* For assemblies */
	MONO_LOADED_IMAGES_ASSEMBLY = 1, /* for files & netmodules */
};

struct _MonoLoadedImages {
	union {
		MonoAssemblyLoadContext *alc; /* NULL if global */
		MonoAssembly *assembly; /* for netmodules */
	} owner;
	guint8 owner_kind;
	GHashTable *loaded_images_hashes [4];
};



gpointer
mono_lookup_pinvoke_call_internal (MonoMethod *method, MonoError *error);

#ifdef ENABLE_NETCORE
void
mono_set_pinvoke_search_directories (int dir_count, char **dirs);
#endif

void
mono_loaded_images_init (MonoLoadedImages *li, guint8 owner_kind, gpointer owner);

void
mono_loaded_images_cleanup (MonoLoadedImages *li, gboolean shutdown);

void
mono_loaded_images_free (MonoLoadedImages *li);

#ifdef ENABLE_NETCORE
void
mono_alc_init (MonoAssemblyLoadContext *alc, MonoDomain *domain, gboolean default_alc);

void
mono_alc_cleanup (MonoAssemblyLoadContext *alc);
#endif /* ENABLE_NETCORE */

#ifdef ENABLE_NETCORE
static inline MonoDomain *
mono_alc_domain (MonoAssemblyLoadContext *alc)
{
	return alc->domain;
}
#endif /* ENABLE_NETCORE */

MonoLoadedImages *
mono_alc_get_loaded_images (MonoAssemblyLoadContext *alc);

#endif
