#include <windows.h>
#include <stdio.h>
//
// The Mono and WINE header files have overlapping definitions in the
// header files. This file contains the "WinMain" startup code and
// other code that must call functions in the windows.h header file.
// 

int mono_start(unsigned int hInstance, unsigned int hPrevInstance, char* lpszCmdLine, int nCmdShow);
long __attribute__((__stdcall__)) WndProc(unsigned int hWnd, unsigned int msg, unsigned int wParam, long lParam);

// register WNDCLASS for use in embeded application
void register_mono_wine_class(HINSTANCE hInstance)
{
	WNDCLASS wc;

	wc.lpszClassName = "mono_wine_class";
	wc.lpfnWndProc = (WNDPROC) WndProc;
	wc.style = CS_OWNDC | CS_VREDRAW | CS_HREDRAW;
	wc.hInstance = hInstance;
	wc.hIcon = (HICON) LoadIcon( 0, (LPCTSTR) IDI_APPLICATION );
	wc.hCursor = (HCURSOR) LoadCursor( 0, (LPCTSTR) IDC_ARROW );
	wc.hbrBackground = (HBRUSH)( COLOR_WINDOW+1 );
	wc.lpszMenuName = NULL;
	wc.cbClsExtra = 0;
	wc.cbWndExtra = 0;

	RegisterClass(&wc);
}

int PASCAL WinMain( HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpszCmdLine, int nCmdShow )
{
	printf("registering mono_wine_class\n");
	register_mono_wine_class(hInstance);
	return mono_start(hInstance, hPrevInstance, lpszCmdLine, nCmdShow);
}

