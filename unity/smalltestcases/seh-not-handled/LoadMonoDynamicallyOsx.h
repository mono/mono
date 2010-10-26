#include "MonoTypes.h"
// define a ProcPtr type for each API
#define DO_API(r,n,p)	typedef r (*fp##n##Type) p;
#include "MonoFunctions.h"

// declare storage for each API's function pointers
#define DO_API(r,n,p)	fp##n##Type n = NULL;
#include "MonoFunctions.h"

void* gMonoModule;

void SetupMono()
{
	gMonoModule = dlopen("../../../builds/embedruntimes/osx/libmono.0.dylib",RTLD_LAZY );
	
	// Search for the functions we want by name.
	bool funcsOK = true;
#define DO_API(r,n,p)	n = (fp##n##Type) dlsym(gMonoModule, #n); if( !n ) { funcsOK = false; printf("Fail: %s\n",#n); }
#include "MonoFunctions.h"

	if( !funcsOK )
	{

		dlclose( gMonoModule );
		gMonoModule = NULL;
	}
}

void CleanupMono()
{
	dlclose(gMonoModule);
}
