//
// general.c
//
// Copyright (c) 2003 Alexandre Pigolkine
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial 
// portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT 
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
// OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Authors:
//   Alexandre Pigolkine(pigolkine@gmx.de)
//

#include "gdip_main.h"
#include <dlfcn.h>

// Startup / shutdown

struct startupInput
{
	unsigned int version;             
	void       * ptr; 
	int          threadOpt;
	int          codecOpt;
};


struct startupOutput
{
	void *hook;
	void *unhook;
};

Display *GDIP_display = 0;
int      closeDisplay = 0;

static void * x11drvHandle = 0;

static void _load_x11drv ()
{
	if (x11drvHandle == 0) {
		x11drvHandle = dlopen ("libx11drv.dll.so", 1);
		if (x11drvHandle == 0) {
			x11drvHandle = dlopen ("/usr/local/lib/libx11drv.dll.so", 1);
		}
		if (x11drvHandle == 0) {
			x11drvHandle = dlopen ("/usr/lib/libx11drv.dll.so", 1);
		}
	}
}

static void _unload_x11drv ()
{
	if (x11drvHandle != 0) {
		dlclose (x11drvHandle);
	}
}

Display *_get_wine_display ()
{
	Display * result = 0;
	_load_x11drv ();
	if (x11drvHandle != 0) {
		Display **addr = dlsym(x11drvHandle,"gdi_display");
		if (addr) {
			result = *addr;
		}
	}
	return result;
}

Status GdiplusStartup(unsigned long *token, const struct startupInput *input, struct startupOutput *output)
{
	GDIP_display = _get_wine_display ();
	if (GDIP_display == 0){
		GDIP_display = XOpenDisplay(0);
		closeDisplay = 1;
	}
	//printf ("GdiplusStartup. GDIP_Display %p\n", GDIP_display);
	initializeGdipWin32 ();
	*token = 1;
	return Ok;
}

void GdiplusShutdown(unsigned long * token)
{
	if (closeDisplay) {
		XCloseDisplay(GDIP_display);
	}
	_unload_x11drv ();
}


// Memory
void *GdipAlloc (int size)
{
	return malloc (size);
}

void GdipFree (void * ptr)
{
	free(ptr);
}

