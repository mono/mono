#include "config.h"
#include "mono/metadata/loader-internals.h"
#include "mono/metadata/loaded-images-internals.h"

#ifdef ENABLE_NETCORE
/* MonoAssemblyLoadContext support only in netcore Mono */

void
mono_alc_init (MonoAssemblyLoadContext *alc, MonoDomain *domain, gboolean default_alc)
{
	MonoLoadedImages *li = g_new0 (MonoLoadedImages, 1);
	mono_loaded_images_init (li, alc);
	alc->domain = domain;
	alc->loaded_images = li;
}

void
mono_alc_cleanup (MonoAssemblyLoadContext *alc)
{
	mono_loaded_images_free (alc->loaded_images);
}


#endif /* ENABLE_NETCORE */
