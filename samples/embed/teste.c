#include <mono/jit/jit.h>
#include <mono/metadata/environment.h>

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

typedef struct
{
	MonoDomain *domain;
	const char *file;
	int argc;
	char **argv;
} MainThreadArgs;

static void main_thread_handler (gpointer user_data)
{
	MainThreadArgs *main_args=(MainThreadArgs *)user_data;
	MonoAssembly *assembly;

	assembly = mono_domain_assembly_open (main_args->domain,
					      main_args->file);
	if (!assembly)
		exit (2);
	/*
	 * mono_jit_exec() will run the Main() method in the assembly.
	 * The return value needs to be looked up from
	 * System.Environment.ExitCode.
	 */
	mono_jit_exec (main_args->domain, assembly, main_args->argc,
		       main_args->argv);
}


int 
main(int argc, char* argv[]) {
	MonoDomain *domain;
	const char *file;
	int retval;
	MainThreadArgs main_args;
	
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

	main_args.domain=domain;
	main_args.file=file;
	main_args.argc=argc-1;
	main_args.argv=argv+1;
	
	mono_runtime_exec_managed_code (domain, main_thread_handler,
					&main_args);
	
	retval=mono_environment_exitcode_get ();
	
	mono_jit_cleanup (domain);
	return retval;
}

