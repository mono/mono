#include <glib.h>
#include <mono/jit/jit.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/mono-gc.h>
#include <mono/metadata/class.h>

int main()
{
	MonoDomain* domain = mono_jit_init_version ("Unity Root Domain", "v2.0.50727");
	MonoMethodDesc* desc = mono_method_desc_new("Lucas:.ctor()",1);

        MonoAssembly* ass = mono_domain_assembly_open (mono_domain_get (), "lucas.dll");
        MonoImage* img = mono_assembly_get_image(ass);
	printf("image %d\n",img);
        MonoMethod* m = mono_method_desc_search_in_image(desc,img);
	printf("method %d\n",m);
        MonoObject* exc;
	MonoObject* newinst = mono_object_new(mono_domain_get(), mono_method_get_class(m));
        MonoObject* ret = mono_runtime_invoke(m,newinst,0,&exc);
	printf ("Exception: %d\n",exc);
	if (exc)
	{
		MonoException* exc2 = (MonoException*) exc;
		printf ("exc msg: %s\n",mono_class_get_name(mono_object_get_class(exc)));
	}
	printf ("ret: %d\n",ret);
	return 0;
}
