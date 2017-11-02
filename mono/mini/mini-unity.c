#include <config.h>
#include <mono/mini/mini.h>

#ifdef WIN32

extern LONG CALLBACK seh_vectored_exception_handler(EXCEPTION_POINTERS* ep);
LONG mono_unity_seh_handler(EXCEPTION_POINTERS* ep)
{
#if defined(TARGET_X86) || defined(TARGET_AMD64)
	return seh_vectored_exception_handler(ep);
#else
	g_assert_not_reached();
#endif
}

int (*gUnhandledExceptionHandler)(EXCEPTION_POINTERS*) = NULL;

void mono_unity_set_unhandled_exception_handler(void* handler)
{
	gUnhandledExceptionHandler = handler;
}

#endif // WIN32

extern gboolean unity_shutting_down;

MONO_API void
mono_unity_jit_cleanup (MonoDomain *domain)
{
	unity_shutting_down = TRUE;
	mono_jit_cleanup (domain);
}
