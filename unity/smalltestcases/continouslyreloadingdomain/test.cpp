#include <glib.h>
#include <mono/jit/jit.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/mono-gc.h>

int main()
{
	MonoDomain* domain = mono_jit_init_version ("Unity Root Domain", "v2.0.50727");
	MonoMethodDesc* desc = mono_method_desc_new("Lucas:Doit()",1);

	for (;;)
	{
        MonoDomain* child = mono_domain_create_appdomain("Unity Child Domain", NULL);
        mono_domain_set (child, FALSE);

        MonoAssembly* ass = mono_domain_assembly_open (mono_domain_get (), "lucas.dll");
        MonoImage* img = mono_assembly_get_image(ass);
        MonoMethod* m = mono_method_desc_search_in_image(desc,img);
        MonoObject* exc;
        MonoObject* ret = mono_runtime_invoke(m,0,0,&exc);
        mono_domain_set(mono_get_root_domain(), 0);
        mono_domain_unload(child);
        mono_gc_collect(mono_gc_max_generation());
	}

	return 0;
}
