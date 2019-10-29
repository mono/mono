/*
 * test-mono-embed.c: Unit test for embed mono.
 */

#include "mini/jit.h"
#include "metadata/environment.h"
#include "utils/mono-publib.h"
#include "metadata/mono-config.h"
#include <stdlib.h>

/*
 * Very simple mono embedding example.
 * Compile with: 
 * 	gcc -o teste teste.c `pkg-config --cflags --libs mono-2` -lm
 * 	mcs test.cs
 * Run with:
 * 	./teste test.exe
 */

static MonoString*
gimme () {
	return mono_string_new (mono_domain_get (), "All your monos are belong to us!");
}

static void main_function (MonoDomain *domain, const char *file, int argc, char** argv)
{
	MonoAssembly *assembly;

	assembly = mono_domain_assembly_open (domain, file);
	if (!assembly)
		exit (2);
	/*
	 * mono_jit_exec() will run the Main() method in the assembly.
	 * The return value needs to be looked up from
	 * System.Environment.ExitCode.
	 */
	mono_jit_exec (domain, assembly, argc, argv);
}

static int malloc_count = 0;

static void* custom_malloc(size_t bytes)
{
	++malloc_count;
	return malloc(bytes);
}

#ifdef __cplusplus
extern "C"
#endif
int
test_mono_embed_main (void);

int
test_mono_embed_main (void)
{
    int argc = 2;
    char *argv[] = {
                        (char*)"test-mono-embed.exe",
                        (char*)"test-embed.exe",
                        NULL
                    };
	MonoDomain *domain;
	const char *file;
	int retval;
	
	file = argv [1];

	MonoAllocatorVTable mem_vtable = { MONO_ALLOCATOR_VTABLE_VERSION, custom_malloc, NULL, NULL, NULL };
	mono_set_allocator_vtable (&mem_vtable);

	/*
	 * Load the default Mono configuration file, this is needed
	 * if you are planning on using the dllmaps defined on the
	 * system configuration
	 */
	mono_config_parse (NULL);
	/*
	 * mono_jit_init() creates a domain: each assembly is
	 * loaded and run in a MonoDomain.
	 */
	domain = mono_jit_init (file);
	/*
	 * We add our special internal call, so that C# code
	 * can call us back.
	 */
	mono_add_internal_call ("MonoEmbed::gimme", gimme);

	main_function (domain, file, argc - 1, argv + 1);
	
	retval = mono_environment_exitcode_get ();

	mono_jit_cleanup (domain);

	return retval;
}
