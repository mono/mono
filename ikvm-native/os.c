/*
  Copyright (C) 2004, 2005 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
#ifdef _WIN32
	#include <windows.h>
	#include "jni.h"

	JNIEXPORT void* JNICALL ikvm_LoadLibrary(char* psz)
	{
		return LoadLibrary(psz);
	}

	JNIEXPORT void JNICALL ikvm_FreeLibrary(HMODULE handle)
	{
		FreeLibrary(handle);
	}

	JNIEXPORT void* JNICALL ikvm_GetProcAddress(HMODULE handle, char* name, jint argc)
	{
#ifdef _WIN64
		return GetProcAddress(handle, name);
#else
		void* pfunc;
		char buf[512];
		if(strlen(name) > sizeof(buf) - 11)
		{
			return 0;
		}
		wsprintf(buf, "_%s@%d", name, argc);
		pfunc = GetProcAddress(handle, buf);
		if (pfunc)
			return pfunc;
		// If we didn't find the mangled name, try the unmangled name (this happens if you have an
		// explicit EXPORT in the linker def).
		return GetProcAddress(handle, name);
#endif
	}
#else
	#include <gmodule.h>
	#include <sys/types.h>
	#include <sys/mman.h>
	#include "jni.h"

	JNIEXPORT void* JNICALL ikvm_LoadLibrary(char* psz)
	{
		return g_module_open(psz, 0);
	}

	JNIEXPORT void JNICALL ikvm_FreeLibrary(GModule* handle)
	{
		g_module_close(handle);
	}

	JNIEXPORT void* JNICALL ikvm_GetProcAddress(GModule* handle, char* name, jint argc)
	{
		void *symbol;

		gboolean res = g_module_symbol(handle, name, &symbol);

		if (res)
			return symbol;
		else
			return NULL;
	}

	JNIEXPORT void* JNICALL ikvm_mmap(int fd, jboolean writeable, jboolean copy_on_write, jlong position, jint size)
	{
		return mmap(0, size, writeable ? PROT_WRITE | PROT_READ : PROT_READ, copy_on_write ? MAP_PRIVATE : MAP_SHARED, fd, position);
	}

	JNIEXPORT int JNICALL ikvm_munmap(void* address, jint size)
	{
		return munmap(address, size);
	}

	JNIEXPORT int JNICALL ikvm_msync(void* address, jint size)
	{
		return msync(address, size, MS_SYNC);
	}
#endif
