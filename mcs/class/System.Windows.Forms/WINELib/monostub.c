#include <mono/jit/jit.h>
#include <stdio.h>

//
// The Mono and WINE header files have overlapping definitions in the
// header files. Since we are only using a few functions and definitions
// define them here to avoid conflicts.
//
// these are defined in jit.h
//  typedef long LONG;
//  typedef unsigned long DWORD;
//  typedef unsigned short WORD;
//  typedef UINT HANDLE;
//  typedef HINSTANCE HMODULE;

#define __stdcall __attribute__((__stdcall__))
#define CALLBACK    __stdcall
#define WINAPI      __stdcall
#define PASCAL      __stdcall

typedef int INT;
typedef unsigned int UINT;
typedef char CHAR;
typedef CHAR *LPSTR;
typedef const CHAR *LPCSTR;

typedef UINT WPARAM;
typedef LONG LPARAM;
typedef LONG LRESULT;
typedef WORD ATOM;

typedef void* HWND;
typedef void* HINSTANCE;
typedef void* HICON;
typedef void* HCURSOR;
typedef void* HBRUSH;

typedef LRESULT (CALLBACK *WNDPROC) (HWND, UINT, WPARAM, LPARAM);

typedef struct
{
	UINT style;
	WNDPROC lpfnWndProc;
	INT cbClsExtra;
	INT cbWndExtra;
	HINSTANCE hInstance;
	HICON hIcon;
	HCURSOR hCursor;
	HBRUSH hbrBackground;
	LPCSTR lpszMenuName;
	LPCSTR lpszClassName;
} WNDCLASSA;

ATOM WINAPI RegisterClassA (const WNDCLASSA *);
HMODULE WINAPI GetModuleHandleA (LPCSTR);
INT WINAPI MessageBoxExA (HWND, LPCSTR, LPCSTR, UINT, WORD);

HINSTANCE applicationInstance = NULL;

// register WNDCLASS for use in embeded application, this is a work around
// for Bugzilla item #29548
int PASCAL MonoRegisterClass (UINT style, WNDPROC lpfnWndProc, INT cbClsExtra,
			      INT cbWndExtra, HINSTANCE hInstance, HICON hIcon,
			      HCURSOR hCursor, HBRUSH hbrBackground, 
			      LPCSTR lpszMenuName, LPCSTR lpszClassName)
{
	WNDCLASSA wc;
	int retval = 0;

	wc.lpszClassName = lpszClassName;
	wc.lpfnWndProc = lpfnWndProc;
	wc.style = style;
	wc.hInstance = applicationInstance;
	wc.hIcon = hIcon;
	wc.hCursor = hCursor;
	wc.hbrBackground = hbrBackground;
	wc.lpszMenuName = lpszMenuName;
	wc.cbClsExtra = cbClsExtra;
	wc.cbWndExtra = cbWndExtra;
	
	retval = RegisterClassA (&wc);
	
	return retval;
}

int PASCAL WinMain (HINSTANCE hInstance, HINSTANCE hPrevInstance, 
		    LPSTR lpszCmdLine, int nCmdShow)
{
	MonoDomain *domain = NULL;
	MonoAssembly *assembly = NULL;
	int retval = 0;

	applicationInstance = hInstance;

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
