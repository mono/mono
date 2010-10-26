#define LOADDYNAMIC 1

#if WIN32
#include "windows.h"
#else
#include <stdlib.h>
#include <dlfcn.h>
#endif
#include "stdio.h"
#if LOADDYNAMIC
	#if WIN32
		#include "LoadMonoDynamically.h"
	#else
		#include "LoadMonoDynamicallyOsx.h"
	#endif
#else
#include <mono/jit/jit.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/mono-gc.h>
#include <mono/metadata/class.h>
#endif

int main()
{
#if LOADDYNAMIC
	SetupMono();
#endif

#if WIN32
	SetEnvironmentVariable(L"MONO_PATH",L"..\\..\\..\\builds\\monodistribution\\lib\\mono\\2.0");
#else
	setenv("MONO_PATH","../../../builds/monodistribution/lib/mono/2.0",1);
#endif
        MonoDomain* domain = mono_jit_init_version ("Unity Root Domain","v2.0.50727");

        //create and set child domain
        MonoDomain* child = mono_domain_create_appdomain("Unity Child Domain",NULL);
        mono_domain_set(child,0);

        //load assembly and call entrypoint
        MonoAssembly* ass = mono_domain_assembly_open (mono_domain_get (),"lucas.exe");
        MonoImage* img = mono_assembly_get_image(ass);
        printf("image %p\n",img);
        MonoMethodDesc* desc = mono_method_desc_new("Main2:Main",1);
        MonoMethod* m = mono_method_desc_search_in_image(desc,img);
        printf("method %p\n",m);
        MonoObject* exc;
        MonoObject* newinst = mono_object_new(mono_domain_get(),mono_method_get_class(m));
        MonoObject* ret = mono_runtime_invoke(m,newinst,0,&exc);

		mono_domain_set(domain,0);
        mono_domain_unload(child);
		mono_runtime_set_shutting_down ();
		mono_threads_set_shutting_down ();
		mono_thread_pool_cleanup ();
		mono_domain_finalize(mono_get_root_domain(),2000);
		mono_runtime_cleanup(mono_get_root_domain());
		CleanupMono();
		while(1){}
        return 0;
}

void massi()
{
	mono_print_thread_dump((void*)0);
}