#include <mono/jit/jit.h>

int mono_start(int hInstance, int hPrevInstance, char* lpszCmdLine, int nCmdShow)
{
	MonoDomain *domain;
	MonoAssembly *assembly;

	domain = mono_jit_init (lpszCmdLine);
	assembly = mono_domain_assembly_open (domain, lpszCmdLine);
	mono_jit_exec (domain, assembly, 0, 0);
	mono_profiler_shutdown ();
	mono_jit_cleanup (domain);
}
