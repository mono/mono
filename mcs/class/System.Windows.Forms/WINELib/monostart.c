#include <mono/jit/jit.h>
#include <mono/metadata/debug-helpers.h>
#include <stdio.h>

static unsigned int application_instance;
MonoMethod *wndproc_method = NULL;

// wrapped in the Mono class, enables the embedded application
// to get the HINSTANCE
int GetInstance()
{
	return application_instance;
}

// WndProc for registered class, calls embeded Mono WndProc function
unsigned long __attribute__((__stdcall__)) WndProc(unsigned int hWnd, unsigned int msg, unsigned int wParam, long lParam)
{
	MonoObject *return_object = NULL;
	gpointer params[4];

	printf("WndProc begin\n");

	params[0] = &hWnd;
	params[1] = &msg;
	params[2] = &wParam;
	params[3] = &lParam;

	return_object = mono_runtime_invoke(wndproc_method, NULL, params, NULL);
	printf("WndProc end\n");

	return (long) &return_object->vtable;
}

// uses Mono embedding API to execute application
int mono_start(unsigned int hInstance, unsigned int hPrevInstance, char* lpszCmdLine, int nCmdShow)
{
	MonoDomain *domain = NULL;
	MonoAssembly *assembly = NULL;
	MonoMethodDesc* desc = NULL;
	MonoImage *image = NULL;
  
	application_instance = hInstance;

      	printf("initializing JIT engine\n");	
	domain = mono_jit_init(lpszCmdLine);

	// helper to allow embedded application to get the HINSTANCE
	// (not sure if we need this in the latest Win32 API's)
	//printf("adding internal calls\n");
	//mono_add_internal_call ("Application::GetInstance", 
	//			GetInstance);

	printf("opening assembly\n");
	assembly = mono_domain_assembly_open (domain, lpszCmdLine);

	// setup WNDPROC method in embedded application
	desc = mono_method_desc_new ("System.Windows.Forms.Application:_ApplicationWndProc", TRUE);
	printf("finding method(s)\n");
	image = mono_image_loaded ("System.Windows.Forms");

	if (image && desc)
	  wndproc_method = mono_method_desc_search_in_image (desc, image);

	if (!wndproc_method) {
	  printf("error: Application:_ApplicationWndProc not found\n");
	  return 1;
	}

	printf("executing assembly\n");
	mono_jit_exec(domain, assembly, 0, 0);
	
	printf("calling JIT cleanup\n");
	mono_jit_cleanup(domain);

	return 0;
}
