#include <mono/jit/jit.h>
#include <stdio.h>
#include <semaphore.h>
#include <string.h>
#include <ctype.h>
/*
 * The Mono and WINE header files have overlapping definitions in the
 * header files. Since we are only using a few functions and definitions
 * define them here to avoid conflicts.
 */
#define __stdcall __attribute__((__stdcall__))
#define PASCAL      __stdcall

typedef int INT;
typedef unsigned int UINT;
typedef char CHAR;
typedef CHAR *LPSTR;
typedef void* HINSTANCE;

HINSTANCE applicationInstance = NULL;

/*
 * unresolved symbols when linking w/o pthread (testing):
 *   pthread_kill
 *   sem_post
 *   sem_init
 *   sem_wait
 *   sem_destroy
 */
#if 0
int pthread_kill (pthread_t thread, int signo)
{
  printf ("pthread_kill\n");
  return 0;
}

int sem_init (sem_t *sem, int pshared, unsigned int value)
{
  printf ("sem_init\n");
  return 0;
}

int sem_post (sem_t * sem)
{
  printf ("sem_post\n");
  return 0;
}

int sem_wait (sem_t * sem)
{
  printf ("sem_wait\n");
  return 0;
}

int sem_destroy(sem_t * sem)
{
  printf ("sem_destroy\n");
  return 0;
}
#endif

/* not defined in the public headers but we it to load the DLL mappings */
void mono_config_parse (const char *filename);

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
	int i;

	printf ("opening assembly \"%s\", argc %d\n", main_args->file, main_args->argc);
	for( i = 0; i < main_args->argc; i++) {
		printf ("param %d  = \"%s\"\n", i, main_args->argv[i]);
	}
	assembly = mono_domain_assembly_open (main_args->domain,
					      main_args->file);
	if (!assembly) {
		printf("error opening assembly\n");
		return;
	}
	/*
	 * mono_jit_exec() will run the Main() method in the assembly.
	 * The return value needs to be looked up from
	 * System.Environment.ExitCode.
	 */
	printf ("executing assembly\n");
	mono_jit_exec (main_args->domain, assembly, main_args->argc,
		       main_args->argv);
}

#define MAX_ARGV_IDX 1024

int PASCAL WinMain (HINSTANCE hInstance, HINSTANCE hPrevInstance,
		    LPSTR lpszCmdLine, int nCmdShow)
{
	MonoDomain *domain = NULL;
	MainThreadArgs main_args;
	int retval = 0;
	size_t len;
	char *CommandLine;
	char *argv [MAX_ARGV_IDX] = { 0};
	int argc = 0;
	char *argv_ptr = 0;
	char *argv_start;
	char *argv_end;
	int isInsideQuote = 0;

	applicationInstance = hInstance;

	printf ("parsing configuration file\n");
	mono_config_parse (NULL);

	printf ("initializing JIT engine\n");
	domain = mono_jit_init (lpszCmdLine);

	printf ("parsing command line \"%s\"\n", lpszCmdLine);
	len = strlen(lpszCmdLine);
	CommandLine = (char*)malloc(len + 1);
	memset(CommandLine, 0,len + 1);
	strncpy(CommandLine, lpszCmdLine, len);

	argv_start = CommandLine;
	argv_end = CommandLine + len;

	// FIXME: remove "" from arguments
	for( argv_ptr = argv_start; argv_ptr < argv_end; ) {
		if( argc == MAX_ARGV_IDX) {
			break;
		}
		if( isspace(*argv_ptr)) {
			if( !isInsideQuote){
				// parameter completed
				if( argv_start != argv_ptr) {
					argv[argc] = argv_start;
					++argc;
					*argv_ptr = '\0';
					++argv_ptr;
				}
				while( argv_ptr <= argv_end && isspace(*argv_ptr) ) ++argv_ptr;
				argv_start = argv_ptr;
			}
			else {
				++argv_ptr;
			}
		}
		else {
			if(*argv_ptr == '\"') {
				isInsideQuote = isInsideQuote ? 0 : 1;
			}
			++argv_ptr;
		}
	}
	if( argv_start != argv_ptr && argc < MAX_ARGV_IDX) {
		argv[argc] = argv_start;
		++argc;
		*argv_ptr = '\0';
	}

	if( argc > 0) {
		main_args.domain=domain;
		if( 0 == strcmp(argv[0],"debug")) {
			argv[0] = "/usr/local/bin/Interpreter.exe";
		}
		main_args.file=argv[0];
		main_args.argc=argc;
		main_args.argv=argv;
		printf ("runtime exec managed code\n");

		mono_runtime_exec_managed_code (domain, main_thread_handler,
						&main_args);
	}
	else {
		printf ("no command line parameters.\n");
	}
	free(CommandLine);

	printf ("calling JIT cleanup\n");
	mono_jit_cleanup (domain);

	return retval;
}
