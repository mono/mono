#include <mono/jit/jit.h>

/*
 * Very simple mono embedding example.
 * Compile with: 
 * 	gcc -o teste teste.c `pkg-config --cflags --libs mono` -lm
 * 	mcs test.cs
 * Run with:
 * 	./teste test.exe
 */

static MonoString*
gimme () {
	return mono_string_new (mono_domain_get (), "All your monos are belong to us!");
}

int 
main(int argc, char* argv[]) {
	MonoDomain *domain;
	MonoAssembly *assembly;
	const char *file;
	int retval;

	if (argc < 2){
		fprintf (stderr, "Please provide an assembly to load");
		return 1;
	}
	file = argv [1];
	/*
	 * mono_jit_init() creates a domain: each assembly is
	 * loaded and run in a MonoDomain.
	 */
	domain = mono_jit_init (file);
	/*
	 * We add our special internal call, so that C# code
	 * can call us back.
	 */
	mono_add_internal_call ("Mono::gimme", gimme);
	assembly = mono_domain_assembly_open (domain, file);
	if (!assembly)
		return 2;
	/*
	 * mono_jit_exec() will run the Main() method in the assembly
	 * and return the value.
	 */
	retval = mono_jit_exec (domain, assembly, argc - 1, argv + 1);
	mono_jit_cleanup (domain);
	return retval;
}

