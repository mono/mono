#include <mono/jit/jit.h>
#include <stdio.h>
#include <semaphore.h>

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

int PASCAL WinMain (HINSTANCE hInstance, HINSTANCE hPrevInstance, 
		    LPSTR lpszCmdLine, int nCmdShow)
{
	MonoDomain *domain = NULL;
	MonoAssembly *assembly = NULL;
	int retval = 0;

	applicationInstance = hInstance;

	printf ("parsing configuration file\n");
	mono_config_parse (NULL);

	printf ("initializing JIT engine\n");	
	domain = mono_jit_init (lpszCmdLine);
	
	printf ("opening assembly\n");
	assembly = mono_domain_assembly_open (domain, lpszCmdLine);
	
	if (!assembly){
		printf("error opening assembly\n");
		return 1;
	}

	printf ("executing assembly\n");
	retval = mono_jit_exec (domain, assembly, 0, 0);
	
	printf ("calling JIT cleanup\n");
	mono_jit_cleanup (domain);
	
	return retval;
}
