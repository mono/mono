
#include <windows.h>

int PASCAL WinMain( HINSTANCE hInstance,
    HINSTANCE hPrevInstance,
    LPSTR lpszCmdLine,
    int nCmdShow )
{
	mono_start(hInstance, hPrevInstance, lpszCmdLine, nCmdShow);
	return 0;
}

