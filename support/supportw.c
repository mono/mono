/*
 * Helper routines for some of the common methods that people P/Invoke
 * on their applications.
 *
 * Authors:
 *   Gonzalo Paniagua (gonzalo@ximian.com)
 *   Miguel de Icaza  (miguel@novell.com)
 *
 * (C) 2005 Novell, Inc.
 *
 */
#include <config.h>
#include <glib.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include "supportw.h"
#include "mono/metadata/assembly.h"
#include "mono/metadata/class.h"
#include "mono/metadata/object.h"
#include "mono/metadata/tabledefs.h"

typedef struct {
	const char *fname;
	void *fnptr;
} FnPtr;

gpointer FindWindowExW        (gpointer hwndParent, gpointer hwndChildAfter,
			       const char *classw, const char *window);

gpointer GetProcessHeap       (void);

static FnPtr functions [] = {
	{ "FindWindowExW", NULL }, /* user32 */
};
#define NFUNCTIONS	(sizeof (functions)/sizeof (FnPtr))

static int swf_registered;

static int
compare_names (const void *key, const void *p)
{
	FnPtr *ptr = (FnPtr *) p;
	return strcmp (key, ptr->fname);
}

static gpointer
get_function (const char *name)
{
	FnPtr *ptr;

	ptr = bsearch (name, functions, NFUNCTIONS, sizeof (FnPtr),
			compare_names);

	if (ptr == NULL) {
		g_warning ("Function '%s' not found.", name);
		return NULL;
	}

	return ptr->fnptr;
}

gboolean
supportw_register_delegate (const char *function_name, void *fnptr)
{
	FnPtr *ptr;

	g_return_val_if_fail (function_name && fnptr, FALSE);

	ptr = bsearch (function_name, functions, NFUNCTIONS, sizeof (FnPtr),
			compare_names);

	if (ptr == NULL) {
		g_warning ("Function '%s' not supported.", function_name);
		return FALSE;
	}

	ptr->fnptr = fnptr;
	return TRUE;
}

#define M_ATTRS (METHOD_ATTRIBUTE_PUBLIC | METHOD_ATTRIBUTE_STATIC)
static gboolean
register_assembly (const char *name, int *registered)
{
/* we can't use mono or wapi funcions in a support lib */
#if 0
	MonoAssembly *assembly;
	MonoImageOpenStatus status;
	MonoImage *image;
	MonoClass *klass;
	MonoMethod *method;
	MonoObject *exc;

	if (*registered)
		return TRUE;

	assembly = mono_assembly_load_with_partial_name (name, &status);
	if (assembly == NULL) {
		g_warning ("Cannot load assembly '%s'.", name);
		return FALSE;
	}

	image = mono_assembly_get_image (assembly);
	klass = mono_class_from_name (image, name, "LibSupport");
	if (klass == NULL) {
		g_warning ("Cannot load class %s.LibSupport", name);
		mono_assembly_close (assembly);
		return FALSE;
	}

	method = mono_class_get_method_from_name_flags (klass, "Register", 0, M_ATTRS);
	if (klass == NULL) {
		g_warning ("Cannot load method Register from klass %s.LibSupport", name);
		mono_assembly_close (assembly);
		return FALSE;
	}

	exc = NULL;
	mono_runtime_invoke (method, NULL, NULL, &exc);
	if (exc != NULL) {
		mono_assembly_close (assembly);
		mono_print_unhandled_exception (exc);
		return FALSE;
	}
	*registered = 1;
	mono_assembly_close (assembly);
	return TRUE;
#else
	return FALSE;
#endif
}

void
supportw_test_all ()
{
	int i;

	register_assembly ("System.Windows.Forms", &swf_registered);
	for (i = 0; i < NFUNCTIONS; i++) {
		FnPtr *ptr = &functions [i];
		if (ptr->fnptr == NULL)
			g_warning ("%s wasn't registered.", ptr->fname);
	}
}

gpointer
FindWindowExW (gpointer hwndParent, gpointer hwndChildAfter, const char *classw, const char *window)
{
	typedef gpointer (*func_type) (gpointer hwndParent, gpointer hwndChildAfter,
					const char *classw, const char *window);
	static func_type func;

	g_return_val_if_fail (register_assembly ("System.Windows.Forms", &swf_registered), NULL);
	if (func == NULL)
		func = (func_type) get_function ("FindWindowExW");

	return func (hwndParent, hwndChildAfter, classw, window);
}

int
SetWindowPos (gpointer hwnd, gpointer hwndInsertAfter, int x, int y, int cx, int cy, unsigned int flags);

int
SetWindowPos (gpointer hwnd, gpointer hwndInsertAfter, int x, int y, int cx, int cy, unsigned int flags)
{
	fprintf (stderr, "SetWindowPos %p %p to [%d,%dx%d,%d] %d\n", hwnd, hwndInsertAfter, x, y, cx, cy, flags);
	return 1;
}

int
SendMessageA (gpointer hwnd, unsigned int msg, gpointer wparam, gpointer lparam);

int
SendMessageA (gpointer hwnd, unsigned int msg, gpointer wparam, gpointer lparam)
{
	fprintf (stderr, "SendMessage (%d, 0x%x, %p, %p)\n", (int) GPOINTER_TO_INT (hwnd), msg, wparam, lparam);
	return 0;
}

int
GetWindowLongA (gpointer hwnd, int a);

int
GetWindowLongA (gpointer hwnd, int a)
{
	return 0;
}
