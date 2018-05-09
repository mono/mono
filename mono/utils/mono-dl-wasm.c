#include <config.h>

#if defined (HOST_WASM)

#include "mono/utils/mono-dl.h"
#include "mono/utils/mono-embed.h"
#include "mono/utils/mono-path.h"

#include <stdlib.h>
#include <stdio.h>
#include <ctype.h>
#include <string.h>
#include <glib.h>

typedef struct MonoDlWasmFunctionEntry
{
	char *name;
	void *ptr;
	struct MonoDlWasmFunctionEntry *next;

} MonoDlWasmFunctionEntry;

typedef struct MonoDlWasmLibraryEntry
{
	char *name;
	MonoDlWasmFunctionEntry *functions;
	struct MonoDlWasmLibraryEntry *next;

} MonoDlWasmLibraryEntry;


static MonoDlWasmLibraryEntry* mono_dl_wasm_entries = NULL;


static MonoDlWasmLibraryEntry *
mono_dl_wasm_get_library_entry (const char* name, int create)
{
	if (name == NULL)
		name = "__Internal";

	MonoDlWasmLibraryEntry *entry = mono_dl_wasm_entries;
	while (entry)
	{
		if (0 == strcmp (name, entry->name))
			return entry;
		entry = entry->next;
	}

	if (!create)
		return NULL;

	entry = (MonoDlWasmLibraryEntry *)g_malloc0 (sizeof (MonoDlWasmFunctionEntry));
	entry->next = mono_dl_wasm_entries;
	entry->name = g_strdup (name);
	entry->functions = NULL;
	mono_dl_wasm_entries = entry;
	return entry;
}

void mono_dl_wasm_add_internal_pinvoke (const char *library_name, const char *name, void *ptr)
{
	struct MonoDlWasmLibraryEntry *library = mono_dl_wasm_get_library_entry (library_name, TRUE);

	MonoDlWasmFunctionEntry *entry = (MonoDlWasmFunctionEntry *)g_malloc0 (sizeof (MonoDlWasmFunctionEntry));
	entry->next = library->functions;
	entry->name = g_strdup (name);
	entry->ptr = ptr;
	library->functions = entry;
}


const char *
mono_dl_get_so_prefix (void)
{
	return "";
}

const char **
mono_dl_get_so_suffixes (void)
{
	static const char *suffixes[] = {
		".wasm", //we only recognize .wasm files for DSOs.
		"",
	};
	return suffixes;
}

int
mono_dl_get_executable_path (char *buf, int buflen)
{
	strncpy (buf, "/managed", buflen); //This is a packaging convertion that our tooling should enforce
	return 0;
}

const char*
mono_dl_get_system_dir (void)
{
	return NULL;
}


void*
mono_dl_lookup_symbol (MonoDl *module, const char *name)
{
	if (name == NULL)
		return NULL;
	MonoDlWasmLibraryEntry *library = (MonoDlWasmLibraryEntry *) module->handle;

	MonoDlWasmFunctionEntry *entry = library->functions;
	while (entry)
	{
		if (0 == strcmp (name, entry->name))
			return entry->ptr;
		entry = entry->next;
	}
	
	return NULL;
}

char*
mono_dl_current_error_string (void)
{
	return g_strdup ("");
}


int
mono_dl_convert_flags (int flags)
{
	return flags;
}

void *
mono_dl_open_file (const char *file, int flags)
{
	return mono_dl_wasm_get_library_entry (file, FALSE);
}

void
mono_dl_close_handle (MonoDl *module)
{
	//nothing to do
}

#endif
