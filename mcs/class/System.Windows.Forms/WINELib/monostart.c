#include <mono/jit/jit.h>
#include <mono/metadata/debug-helpers.h>
#include <stdio.h>

// TODO: need better way of dealing with WNDPROC callback in Mono
// application
static unsigned int application_instance;
MonoMethod *wndproc_method = NULL;

// wrapped in the Mono class, enables the embedded application
// to get the HINSTANCE
 int GetApplicationInstance()
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

// this function was copied from jit/mono.c
static MonoMethod *
find_method_in_assembly (MonoAssembly *assembly, MonoMethodDesc *mdesc)
{
	MonoAssembly **ptr;
	MonoMethod *method;

	method = mono_method_desc_search_in_image (mdesc, assembly->image);

	if (method)
		return method;
  
	for (ptr = assembly->image->references; ptr && *ptr; ptr++) {
		method = find_method_in_assembly (*ptr, mdesc);
		if (method)
			return method;
  	}
  
	return NULL;
}

// uses Mono embedding API to execute application
int mono_start(unsigned int hInstance, unsigned int hPrevInstance, char* lpszCmdLine, int nCmdShow)
{
	MonoDomain *domain = NULL;
	MonoAssembly *assembly = NULL;
	MonoMethodDesc* desc = NULL;
  
	application_instance = hInstance;

      	printf("initializing JIT engine\n");	
	domain = mono_jit_init(lpszCmdLine);

	// helper to allow embedded application to get the HINSTANCE
	// (not sure if we need this in the latest Win32 API's)
	//printf("adding internal calls\n");
	//mono_add_internal_call ("Window::GetApplicationInstance", 
	//			GetApplicationInstance);

	printf("opening assembly\n");
	assembly = mono_domain_assembly_open (domain, lpszCmdLine);

	// setup WNDPROC method in embedded application
	printf("setting up mono_method(s)\n");
	desc = mono_method_desc_new ("System.Windows.Forms.Application:_ApplicationWndProc", FALSE);
	printf("created new desc\n");

	printf("finding method(s)\n");
	wndproc_method = find_method_in_assembly(assembly, desc);

	if (wndproc_method == NULL) {
		printf("_ApplicationWndProc not found");
		return -1;
	}
	
	printf("executing assembly\n");
	mono_jit_exec(domain, assembly, 0, 0);
	
	printf("calling JIT cleanup\n");
	mono_jit_cleanup(domain);

	return 0;
}
