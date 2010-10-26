#include "MonoTypes.h"
// define a ProcPtr type for each API
#define DO_API(r,n,p)	typedef r (*fp##n##Type) p;
#include "MonoFunctions.h"

// declare storage for each API's function pointers
#define DO_API(r,n,p)	fp##n##Type n = NULL;
#include "MonoFunctions.h"

HMODULE gMonoModule;

void SetupMono()
{
	gMonoModule = LoadLibraryW(L"D:\\Massi\\Work\\XBOX-EDITOR\\unity\\External\\Mono\\builds\\embedruntimes\\win32\\mono.dll" );
	
	// Search for the functions we want by name.
	bool funcsOK = true;
#define DO_API(r,n,p)	n = (fp##n##Type) GetProcAddress(gMonoModule, #n); if( !n ) { funcsOK = false; printf("Fail: %s\n",#n); }
#include "MonoFunctions.h"

	if( !funcsOK )
	{

		FreeLibrary( gMonoModule );
		gMonoModule = NULL;
	}
}
void CleanupMono()
{
	::FreeLibrary(gMonoModule);
}