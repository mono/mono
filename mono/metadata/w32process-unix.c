/*
 * process.c: System.Diagnostics.Process support
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *
 * Copyright 2002 Ximian, Inc.
 * Copyright 2002-2006 Novell, Inc.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include <config.h>
#include <glib.h>

#include <stdio.h>
#include <string.h>
#include <pthread.h>
#include <sched.h>
#include <sys/time.h>
#include <errno.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#ifdef HAVE_SIGNAL_H
#include <signal.h>
#endif
#include <sys/time.h>
#include <fcntl.h>
#ifdef HAVE_SYS_PARAM_H
#include <sys/param.h>
#endif
#include <ctype.h>

#ifdef HAVE_SYS_WAIT_H
#include <sys/wait.h>
#endif
#ifdef HAVE_SYS_RESOURCE_H
#include <sys/resource.h>
#endif

#ifdef HAVE_SYS_MKDEV_H
#include <sys/mkdev.h>
#endif

#ifdef HAVE_UTIME_H
#include <utime.h>
#endif

/* sys/resource.h (for rusage) is required when using osx 10.3 (but not 10.4) */
#ifdef __APPLE__
#include <TargetConditionals.h>
#include <sys/resource.h>
#ifdef HAVE_LIBPROC_H
/* proc_name */
#include <libproc.h>
#endif
#endif

#if defined(PLATFORM_MACOSX)
#define USE_OSX_LOADER
#endif

#if ( defined(__OpenBSD__) || defined(__FreeBSD__) ) && defined(HAVE_LINK_H)
#define USE_BSD_LOADER
#endif

#if defined(__HAIKU__)
#define USE_HAIKU_LOADER
#endif

#if defined(USE_OSX_LOADER) || defined(USE_BSD_LOADER)
#include <sys/proc.h>
#include <sys/sysctl.h>
#if !defined(__OpenBSD__)
#include <sys/utsname.h>
#endif
#if defined(__FreeBSD__)
#include <sys/user.h> /* struct kinfo_proc */
#endif
#endif

#ifdef PLATFORM_SOLARIS
/* procfs.h cannot be included if this define is set, but it seems to work fine if it is undefined */
#if _FILE_OFFSET_BITS == 64
#undef _FILE_OFFSET_BITS
#include <procfs.h>
#define _FILE_OFFSET_BITS 64
#else
#include <procfs.h>
#endif
#endif

#if defined(USE_HAIKU_LOADER)
#include <KernelKit.h>
#endif

#include <mono/metadata/w32process.h>
#include <mono/metadata/class.h>
#include <mono/metadata/class-internals.h>
#include <mono/metadata/object.h>
#include <mono/metadata/object-internals.h>
#include <mono/metadata/metadata.h>
#include <mono/metadata/metadata-internals.h>
#include <mono/metadata/exception.h>
#include <mono/io-layer/io-layer.h>
#include <mono/metadata/w32handle.h>
#include <mono/utils/mono-membar.h>
#include <mono/utils/mono-logger-internals.h>
#include <mono/utils/strenc.h>
#include <mono/utils/mono-proclib.h>
#include <mono/utils/mono-path.h>

#ifndef MAXPATHLEN
#define MAXPATHLEN 242
#endif

#define LOGDEBUG(...)
/* define LOGDEBUG(...) g_message(__VA_ARGS__)  */

/* Check if a pid is valid - i.e. if a process exists with this pid. */
static gboolean
is_pid_valid (pid_t pid)
{
	gboolean result = FALSE;

#if defined(HOST_WATCHOS)
	result = TRUE; // TODO: Rewrite using sysctl
#elif defined(PLATFORM_MACOSX) || defined(__OpenBSD__) || defined(__FreeBSD__)
	if (((kill(pid, 0) == 0) || (errno == EPERM)) && pid != 0)
		result = TRUE;
#elif defined(__HAIKU__)
	team_info teamInfo;
	if (get_team_info ((team_id)pid, &teamInfo) == B_OK)
		result = TRUE;
#else
	char *dir = g_strdup_printf ("/proc/%d", pid);
	if (!access (dir, F_OK))
		result = TRUE;
	g_free (dir);
#endif

	return result;
}

static gboolean
process_open_compare (gpointer handle, gpointer user_data)
{
	gboolean res;
	WapiHandle_process *process_handle;
	pid_t wanted_pid, checking_pid;

	g_assert (!WAPI_IS_PSEUDO_PROCESS_HANDLE (handle));

	res = mono_w32handle_lookup (handle, MONO_W32HANDLE_PROCESS, (gpointer*) &process_handle);
	if (!res)
		g_error ("%s: unknown process handle %p", __func__, handle);

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: looking at process %d", __func__, process_handle->id);

	checking_pid = process_handle->id;
	if (checking_pid == 0)
		return FALSE;

	wanted_pid = GPOINTER_TO_UINT (user_data);

	/* It's possible to have more than one process handle with the
	 * same pid, but only the one running process can be
	 * unsignalled.
	 * If the handle is blown away in the window between
	 * returning TRUE here and mono_w32handle_search pinging
	 * the timestamp, the search will continue. */
	return checking_pid == wanted_pid && !mono_w32handle_issignalled (handle);
}

HANDLE
ves_icall_System_Diagnostics_Process_GetProcess_internal (guint32 pid)
{
	gpointer handle;

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: looking for process %d", __func__, pid);

	handle = mono_w32handle_search (MONO_W32HANDLE_PROCESS, process_open_compare, GUINT_TO_POINTER (pid), NULL, TRUE);
	if (handle) {
		/* mono_w32handle_search () already added a ref */
		return handle;
	}

	if (is_pid_valid (pid)) {
		/* Return a pseudo handle for processes we don't have handles for */
		return WAPI_PID_TO_HANDLE (pid);
	} else {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Can't find pid %d", __func__, pid);

		SetLastError (ERROR_PROC_NOT_FOUND);
		return NULL;
	}
}

static gboolean
match_procname_to_modulename (char *procname, char *modulename)
{
	char* lastsep = NULL;
	char* lastsep2 = NULL;
	char* pname = NULL;
	char* mname = NULL;
	gboolean result = FALSE;

	if (procname == NULL || modulename == NULL)
		return (FALSE);

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: procname=\"%s\", modulename=\"%s\"", __func__, procname, modulename);
	pname = mono_path_resolve_symlinks (procname);
	mname = mono_path_resolve_symlinks (modulename);

	if (!strcmp (pname, mname))
		result = TRUE;

	if (!result) {
		lastsep = strrchr (mname, '/');
		if (lastsep)
			if (!strcmp (lastsep+1, pname))
				result = TRUE;
		if (!result) {
			lastsep2 = strrchr (pname, '/');
			if (lastsep2){
				if (lastsep) {
					if (!strcmp (lastsep+1, lastsep2+1))
						result = TRUE;
				} else {
					if (!strcmp (mname, lastsep2+1))
						result = TRUE;
				}
			}
		}
	}

	g_free (pname);
	g_free (mname);

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: result is %d", __func__, result);
	return result;
}

static MonoImage *system_assembly;

static void
stash_system_assembly (MonoObject *obj)
{
	if (!system_assembly)
		system_assembly = obj->vtable->klass->image;
}

//Hand coded version that loads from system
static MonoClass*
mono_class_get_file_version_info_class (void)
{
	static MonoClass *tmp_class;
	MonoClass *klass = tmp_class;
	if (!klass) {
		klass = mono_class_load_from_name (system_assembly, "System.Diagnostics", "FileVersionInfo");
		mono_memory_barrier ();
		tmp_class = klass;
	}
	return klass;
}

static MonoClass*
mono_class_get_process_module_class (void)
{
	static MonoClass *tmp_class;
	MonoClass *klass = tmp_class;
	if (!klass) {
		klass = mono_class_load_from_name (system_assembly, "System.Diagnostics", "ProcessModule");
		mono_memory_barrier ();
		tmp_class = klass;
	}
	return klass;
}

static guint32
unicode_chars (const gunichar2 *str)
{
	guint32 len;

	for (len = 0; str [len] != '\0'; ++len)
		;
	return len;
}

static void
process_set_field_object (MonoObject *obj, const gchar *fieldname,
						  MonoObject *data)
{
	MonoClassField *field;

	LOGDEBUG (g_message ("%s: Setting field %s to object at %p", __func__, fieldname, data));

	field = mono_class_get_field_from_name (mono_object_class (obj),
											fieldname);
	mono_gc_wbarrier_generic_store (((char *)obj) + field->offset, data);
}

static void
process_set_field_string (MonoObject *obj, const gchar *fieldname,
						  const gunichar2 *val, guint32 len, MonoError *error)
{
	MonoClassField *field;
	MonoString *string;

	mono_error_init (error);

	LOGDEBUG (g_message ("%s: Setting field %s to [%s]", __func__, fieldname, g_utf16_to_utf8 (val, len, NULL, NULL, NULL)));

	string = mono_string_new_utf16_checked (mono_object_domain (obj), val, len, error);
	
	field = mono_class_get_field_from_name (mono_object_class (obj),
											fieldname);
	mono_gc_wbarrier_generic_store (((char *)obj) + field->offset, (MonoObject*)string);
}

static void
process_set_field_string_char (MonoObject *obj, const gchar *fieldname,
							   const gchar *val)
{
	MonoClassField *field;
	MonoString *string;

	LOGDEBUG (g_message ("%s: Setting field %s to [%s]", __func__, fieldname, val));

	string = mono_string_new (mono_object_domain (obj), val);
	
	field = mono_class_get_field_from_name (mono_object_class (obj), fieldname);
	mono_gc_wbarrier_generic_store (((char *)obj) + field->offset, (MonoObject*)string);
}

static void
process_set_field_int (MonoObject *obj, const gchar *fieldname,
					   guint32 val)
{
	MonoClassField *field;

	LOGDEBUG (g_message ("%s: Setting field %s to %d", __func__,fieldname, val));
	
	field = mono_class_get_field_from_name (mono_object_class (obj),
					      fieldname);
	*(guint32 *)(((char *)obj) + field->offset)=val;
}

static void
process_set_field_intptr (MonoObject *obj, const gchar *fieldname,
						  gpointer val)
{
	MonoClassField *field;

	LOGDEBUG (g_message ("%s: Setting field %s to %p", __func__, fieldname, val));
	
	field = mono_class_get_field_from_name (mono_object_class (obj),
											fieldname);
	*(gpointer *)(((char *)obj) + field->offset) = val;
}

static void
process_set_field_bool (MonoObject *obj, const gchar *fieldname,
						gboolean val)
{
	MonoClassField *field;

	LOGDEBUG (g_message ("%s: Setting field %s to %s", __func__, fieldname, val ? "TRUE":"FALSE"));
	
	field = mono_class_get_field_from_name (mono_object_class (obj),
											fieldname);
	*(guint8 *)(((char *)obj) + field->offset) = val;
}

#define SFI_COMMENTS		"\\StringFileInfo\\%02X%02X%02X%02X\\Comments"
#define SFI_COMPANYNAME		"\\StringFileInfo\\%02X%02X%02X%02X\\CompanyName"
#define SFI_FILEDESCRIPTION	"\\StringFileInfo\\%02X%02X%02X%02X\\FileDescription"
#define SFI_FILEVERSION		"\\StringFileInfo\\%02X%02X%02X%02X\\FileVersion"
#define SFI_INTERNALNAME	"\\StringFileInfo\\%02X%02X%02X%02X\\InternalName"
#define SFI_LEGALCOPYRIGHT	"\\StringFileInfo\\%02X%02X%02X%02X\\LegalCopyright"
#define SFI_LEGALTRADEMARKS	"\\StringFileInfo\\%02X%02X%02X%02X\\LegalTrademarks"
#define SFI_ORIGINALFILENAME	"\\StringFileInfo\\%02X%02X%02X%02X\\OriginalFilename"
#define SFI_PRIVATEBUILD	"\\StringFileInfo\\%02X%02X%02X%02X\\PrivateBuild"
#define SFI_PRODUCTNAME		"\\StringFileInfo\\%02X%02X%02X%02X\\ProductName"
#define SFI_PRODUCTVERSION	"\\StringFileInfo\\%02X%02X%02X%02X\\ProductVersion"
#define SFI_SPECIALBUILD	"\\StringFileInfo\\%02X%02X%02X%02X\\SpecialBuild"
#define EMPTY_STRING		(gunichar2*)"\000\000"

static void
process_module_string_read (MonoObject *filever, gpointer data,
			    const gchar *fieldname, guchar lang_hi, guchar lang_lo,
			    const gchar *key, MonoError *error)
{
	gchar *lang_key_utf8;
	gunichar2 *lang_key, *buffer;
	UINT chars;

	mono_error_init (error);

	lang_key_utf8 = g_strdup_printf (key, lang_lo, lang_hi, 0x04, 0xb0);

	LOGDEBUG (g_message ("%s: asking for [%s]", __func__, lang_key_utf8));

	lang_key = g_utf8_to_utf16 (lang_key_utf8, -1, NULL, NULL, NULL);

	if (VerQueryValue (data, lang_key, (gpointer *)&buffer, &chars) && chars > 0) {
		LOGDEBUG (g_message ("%s: found %d chars of [%s]", __func__, chars, g_utf16_to_utf8 (buffer, chars, NULL, NULL, NULL)));
		/* chars includes trailing null */
		process_set_field_string (filever, fieldname, buffer, chars - 1, error);
	} else {
		process_set_field_string (filever, fieldname, EMPTY_STRING, 0, error);
	}

	g_free (lang_key);
	g_free (lang_key_utf8);
}

typedef struct {
	const char *name;
	const char *id;
} StringTableEntry;

static StringTableEntry stringtable_entries [] = {
	{ "comments", SFI_COMMENTS },
	{ "companyname", SFI_COMPANYNAME },
	{ "filedescription", SFI_FILEDESCRIPTION },
	{ "fileversion", SFI_FILEVERSION },
	{ "internalname", SFI_INTERNALNAME },
	{ "legalcopyright", SFI_LEGALCOPYRIGHT },
	{ "legaltrademarks", SFI_LEGALTRADEMARKS },
	{ "originalfilename", SFI_ORIGINALFILENAME },
	{ "privatebuild", SFI_PRIVATEBUILD },
	{ "productname", SFI_PRODUCTNAME },
	{ "productversion", SFI_PRODUCTVERSION },
	{ "specialbuild", SFI_SPECIALBUILD }
};

static void
process_module_stringtable (MonoObject *filever, gpointer data,
							guchar lang_hi, guchar lang_lo, MonoError *error)
{
	int i;

	for (i = 0; i < G_N_ELEMENTS (stringtable_entries); ++i) {
		process_module_string_read (filever, data, stringtable_entries [i].name, lang_hi, lang_lo,
									stringtable_entries [i].id, error);
		return_if_nok (error);
	}
}

static void
mono_process_get_fileversion (MonoObject *filever, gunichar2 *filename, MonoError *error)
{
	DWORD verinfohandle;
	VS_FIXEDFILEINFO *ffi;
	gpointer data;
	DWORD datalen;
	guchar *trans_data;
	gunichar2 *query;
	UINT ffi_size, trans_size;
	BOOL ok;
	gunichar2 lang_buf[128];
	guint32 lang, lang_count;

	mono_error_init (error);

	datalen = GetFileVersionInfoSize (filename, &verinfohandle);
	if (datalen) {
		data = g_malloc0 (datalen);
		ok = GetFileVersionInfo (filename, verinfohandle, datalen,
					 data);
		if (ok) {
			query = g_utf8_to_utf16 ("\\", -1, NULL, NULL, NULL);
			if (query == NULL) {
				g_free (data);
				return;
			}
			
			if (VerQueryValue (data, query, (gpointer *)&ffi,
			    &ffi_size)) {
				LOGDEBUG (g_message ("%s: recording assembly: FileName [%s] FileVersionInfo [%d.%d.%d.%d]", __func__, g_utf16_to_utf8 (filename, -1, NULL, NULL, NULL), HIWORD (ffi->dwFileVersionMS), LOWORD (ffi->dwFileVersionMS), HIWORD (ffi->dwFileVersionLS), LOWORD (ffi->dwFileVersionLS)));
	
				process_set_field_int (filever, "filemajorpart", HIWORD (ffi->dwFileVersionMS));
				process_set_field_int (filever, "fileminorpart", LOWORD (ffi->dwFileVersionMS));
				process_set_field_int (filever, "filebuildpart", HIWORD (ffi->dwFileVersionLS));
				process_set_field_int (filever, "fileprivatepart", LOWORD (ffi->dwFileVersionLS));

				process_set_field_int (filever, "productmajorpart", HIWORD (ffi->dwProductVersionMS));
				process_set_field_int (filever, "productminorpart", LOWORD (ffi->dwProductVersionMS));
				process_set_field_int (filever, "productbuildpart", HIWORD (ffi->dwProductVersionLS));
				process_set_field_int (filever, "productprivatepart", LOWORD (ffi->dwProductVersionLS));

				process_set_field_bool (filever, "isdebug", ((ffi->dwFileFlags & ffi->dwFileFlagsMask) & VS_FF_DEBUG) != 0);
				process_set_field_bool (filever, "isprerelease", ((ffi->dwFileFlags & ffi->dwFileFlagsMask) & VS_FF_PRERELEASE) != 0);
				process_set_field_bool (filever, "ispatched", ((ffi->dwFileFlags & ffi->dwFileFlagsMask) & VS_FF_PATCHED) != 0);
				process_set_field_bool (filever, "isprivatebuild", ((ffi->dwFileFlags & ffi->dwFileFlagsMask) & VS_FF_PRIVATEBUILD) != 0);
				process_set_field_bool (filever, "isspecialbuild", ((ffi->dwFileFlags & ffi->dwFileFlagsMask) & VS_FF_SPECIALBUILD) != 0);
			}
			g_free (query);

			query = g_utf8_to_utf16 ("\\VarFileInfo\\Translation", -1, NULL, NULL, NULL);
			if (query == NULL) {
				g_free (data);
				return;
			}
			
			if (VerQueryValue (data, query,
					   (gpointer *)&trans_data,
					   &trans_size)) {
				/* use the first language ID we see
				 */
				if (trans_size >= 4) {
		 			LOGDEBUG (g_message("%s: %s has 0x%0x 0x%0x 0x%0x 0x%0x", __func__, g_utf16_to_utf8 (filename, -1, NULL, NULL, NULL), trans_data[0], trans_data[1], trans_data[2], trans_data[3]));
					lang = (trans_data[0]) |
						(trans_data[1] << 8) |
						(trans_data[2] << 16) |
						(trans_data[3] << 24);
					/* Only give the lower 16 bits
					 * to VerLanguageName, as
					 * Windows gets confused
					 * otherwise
					 */
					lang_count = VerLanguageName (lang & 0xFFFF, lang_buf, 128);
					if (lang_count) {
						process_set_field_string (filever, "language", lang_buf, lang_count, error);
						return_if_nok (error);
					}
					process_module_stringtable (filever, data, trans_data[0], trans_data[1], error);
					return_if_nok (error);
				}
			} else {
				int i;

				for (i = 0; i < G_N_ELEMENTS (stringtable_entries); ++i) {
					/* No strings, so set every field to
					 * the empty string
					 */
					process_set_field_string (filever,
											  stringtable_entries [i].name,
											  EMPTY_STRING, 0, error);
					return_if_nok (error);
				}

				/* And language seems to be set to
				 * en_US according to bug 374600
				 */
				lang_count = VerLanguageName (0x0409, lang_buf, 128);
				if (lang_count) {
					process_set_field_string (filever, "language", lang_buf, lang_count, error);
					return_if_nok (error);
				}
			}
			
			g_free (query);
		}
		g_free (data);
	}
}

static void
process_get_assembly_fileversion (MonoObject *filever, MonoAssembly *assembly)
{
	process_set_field_int (filever, "filemajorpart", assembly->aname.major);
	process_set_field_int (filever, "fileminorpart", assembly->aname.minor);
	process_set_field_int (filever, "filebuildpart", assembly->aname.build);
}

static MonoObject*
get_process_module (MonoAssembly *assembly, MonoClass *proc_class, MonoError *error)
{
	MonoObject *item, *filever;
	MonoDomain *domain = mono_domain_get ();
	char *filename;
	const char *modulename = assembly->aname.name;

	mono_error_init (error);

	/* Build a System.Diagnostics.ProcessModule with the data.
	 */
	item = mono_object_new_checked (domain, proc_class, error);
	return_val_if_nok (error, NULL);
	filever = mono_object_new_checked (domain, mono_class_get_file_version_info_class (), error);
	return_val_if_nok (error, NULL);

	filename = g_strdup_printf ("[In Memory] %s", modulename);

	process_get_assembly_fileversion (filever, assembly);
	process_set_field_string_char (filever, "filename", filename);
	process_set_field_object (item, "version_info", filever);

	process_set_field_intptr (item, "baseaddr", assembly->image->raw_data);
	process_set_field_int (item, "memory_size", assembly->image->raw_data_len);
	process_set_field_string_char (item, "filename", filename);
	process_set_field_string_char (item, "modulename", modulename);

	g_free (filename);

	return item;
}

static MonoObject*
process_add_module (HANDLE process, HMODULE mod, gunichar2 *filename, gunichar2 *modulename, MonoClass *proc_class, MonoError *error)
{
	MonoObject *item, *filever;
	MonoDomain *domain = mono_domain_get ();
	MODULEINFO modinfo;
	BOOL ok;

	mono_error_init (error);

	/* Build a System.Diagnostics.ProcessModule with the data.
	 */
	item = mono_object_new_checked (domain, proc_class, error);
	return_val_if_nok (error, NULL);
	filever = mono_object_new_checked (domain, mono_class_get_file_version_info_class (), error);
	return_val_if_nok (error, NULL);

	mono_process_get_fileversion (filever, filename, error);
	return_val_if_nok (error, NULL);

	process_set_field_string (filever, "filename", filename,
							  unicode_chars (filename), error);
	return_val_if_nok (error, NULL);
	ok = GetModuleInformation (process, mod, &modinfo, sizeof(MODULEINFO));
	if (ok) {
		process_set_field_intptr (item, "baseaddr",
					  modinfo.lpBaseOfDll);
		process_set_field_intptr (item, "entryaddr",
					  modinfo.EntryPoint);
		process_set_field_int (item, "memory_size",
				       modinfo.SizeOfImage);
	}
	process_set_field_string (item, "filename", filename,
							  unicode_chars (filename), error);
	return_val_if_nok (error, NULL);
	process_set_field_string (item, "modulename", modulename,
							  unicode_chars (modulename), error);
	return_val_if_nok (error, NULL);
	process_set_field_object (item, "version_info", filever);

	return item;
}

static GPtrArray*
get_domain_assemblies (MonoDomain *domain)
{
	GSList *tmp;
	GPtrArray *assemblies;

	/* 
	 * Make a copy of the list of assemblies because we can't hold the assemblies
	 * lock while creating objects etc.
	 */
	assemblies = g_ptr_array_new ();
	mono_domain_assemblies_lock (domain);
	for (tmp = domain->domain_assemblies; tmp; tmp = tmp->next) {
		MonoAssembly *ass = (MonoAssembly *)tmp->data;
		if (ass->image->fileio_used)
			continue;
		g_ptr_array_add (assemblies, ass);
	}
	mono_domain_assemblies_unlock (domain);

	return assemblies;
}

static char *
get_process_name_from_proc (pid_t pid)
{
#if defined(USE_BSD_LOADER)
	int mib [6];
	size_t size;
	struct kinfo_proc *pi;
#elif defined(USE_OSX_LOADER)
#if !(!defined (__mono_ppc__) && defined (TARGET_OSX))
	size_t size;
	struct kinfo_proc *pi;
	int mib[] = { CTL_KERN, KERN_PROC, KERN_PROC_PID, pid };
#endif
#else
	FILE *fp;
	char *filename = NULL;
#endif
	char buf[256];
	char *ret = NULL;

#if defined(PLATFORM_SOLARIS)
	filename = g_strdup_printf ("/proc/%d/psinfo", pid);
	if ((fp = fopen (filename, "r")) != NULL) {
		struct psinfo info;
		int nread;

		nread = fread (&info, sizeof (info), 1, fp);
		if (nread == 1) {
			ret = g_strdup (info.pr_fname);
		}

		fclose (fp);
	}
	g_free (filename);
#elif defined(USE_OSX_LOADER)
#if !defined (__mono_ppc__) && defined (TARGET_OSX)
	/* No proc name on OSX < 10.5 nor ppc nor iOS */
	memset (buf, '\0', sizeof(buf));
	proc_name (pid, buf, sizeof(buf));

	// Fixes proc_name triming values to 15 characters #32539
	if (strlen (buf) >= MAXCOMLEN - 1) {
		char path_buf [PROC_PIDPATHINFO_MAXSIZE];
		char *name_buf;
		int path_len;

		memset (path_buf, '\0', sizeof(path_buf));
		path_len = proc_pidpath (pid, path_buf, sizeof(path_buf));

		if (path_len > 0 && path_len < sizeof(path_buf)) {
			name_buf = path_buf + path_len;
			for(;name_buf > path_buf; name_buf--) {
				if (name_buf [0] == '/') {
					name_buf++;
					break;
				}
			}

			if (memcmp (buf, name_buf, MAXCOMLEN - 1) == 0)
				ret = g_strdup (name_buf);
		}
	}

	if (ret == NULL && strlen (buf) > 0)
		ret = g_strdup (buf);
#else
	if (sysctl(mib, 4, NULL, &size, NULL, 0) < 0)
		return(ret);

	if ((pi = g_malloc (size)) == NULL)
		return(ret);

	if (sysctl (mib, 4, pi, &size, NULL, 0) < 0) {
		if (errno == ENOMEM) {
			g_free (pi);
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Didn't allocate enough memory for kproc info", __func__);
		}
		return(ret);
	}

	if (strlen (pi->kp_proc.p_comm) > 0)
		ret = g_strdup (pi->kp_proc.p_comm);

	g_free (pi);
#endif
#elif defined(USE_BSD_LOADER)
#if defined(__FreeBSD__)
	mib [0] = CTL_KERN;
	mib [1] = KERN_PROC;
	mib [2] = KERN_PROC_PID;
	mib [3] = pid;
	if (sysctl(mib, 4, NULL, &size, NULL, 0) < 0) {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: sysctl() failed: %d", __func__, errno);
		return(ret);
	}

	if ((pi = g_malloc (size)) == NULL)
		return(ret);

	if (sysctl (mib, 4, pi, &size, NULL, 0) < 0) {
		if (errno == ENOMEM) {
			g_free (pi);
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Didn't allocate enough memory for kproc info", __func__);
		}
		return(ret);
	}

	if (strlen (pi->ki_comm) > 0)
		ret = g_strdup (pi->ki_comm);
	g_free (pi);
#elif defined(__OpenBSD__)
	mib [0] = CTL_KERN;
	mib [1] = KERN_PROC;
	mib [2] = KERN_PROC_PID;
	mib [3] = pid;
	mib [4] = sizeof(struct kinfo_proc);
	mib [5] = 0;

retry:
	if (sysctl(mib, 6, NULL, &size, NULL, 0) < 0) {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: sysctl() failed: %d", __func__, errno);
		return(ret);
	}

	if ((pi = g_malloc (size)) == NULL)
		return(ret);

	mib[5] = (int)(size / sizeof(struct kinfo_proc));

	if ((sysctl (mib, 6, pi, &size, NULL, 0) < 0) ||
		(size != sizeof (struct kinfo_proc))) {
		if (errno == ENOMEM) {
			g_free (pi);
			goto retry;
		}
		return(ret);
	}

	if (strlen (pi->p_comm) > 0)
		ret = g_strdup (pi->p_comm);

	g_free (pi);
#endif
#elif defined(USE_HAIKU_LOADER)
	image_info imageInfo;
	int32 cookie = 0;

	if (get_next_image_info ((team_id)pid, &cookie, &imageInfo) == B_OK) {
		ret = g_strdup (imageInfo.name);
	}
#else
	memset (buf, '\0', sizeof(buf));
	filename = g_strdup_printf ("/proc/%d/exe", pid);
	if (readlink (filename, buf, 255) > 0) {
		ret = g_strdup (buf);
	}
	g_free (filename);

	if (ret != NULL) {
		return(ret);
	}

	filename = g_strdup_printf ("/proc/%d/cmdline", pid);
	if ((fp = fopen (filename, "r")) != NULL) {
		if (fgets (buf, 256, fp) != NULL) {
			ret = g_strdup (buf);
		}

		fclose (fp);
	}
	g_free (filename);

	if (ret != NULL) {
		return(ret);
	}

	filename = g_strdup_printf ("/proc/%d/stat", pid);
	if ((fp = fopen (filename, "r")) != NULL) {
		if (fgets (buf, 256, fp) != NULL) {
			char *start, *end;

			start = strchr (buf, '(');
			if (start != NULL) {
				end = strchr (start + 1, ')');

				if (end != NULL) {
					ret = g_strndup (start + 1,
							 end - start - 1);
				}
			}
		}

		fclose (fp);
	}
	g_free (filename);
#endif

	return ret;
}

typedef struct
{
	gpointer address_start;
	gpointer address_end;
	char *perms;
	gpointer address_offset;
	guint64 device;
	guint64 inode;
	char *filename;
} WapiProcModule;

static void free_procmodule (WapiProcModule *mod)
{
	if (mod->perms != NULL) {
		g_free (mod->perms);
	}
	if (mod->filename != NULL) {
		g_free (mod->filename);
	}
	g_free (mod);
}

static gint find_procmodule (gconstpointer a, gconstpointer b)
{
	WapiProcModule *want = (WapiProcModule *)a;
	WapiProcModule *compare = (WapiProcModule *)b;
	return want->device == compare->device && want->inode == compare->inode ? 0 : 1;
}

#if defined(USE_OSX_LOADER)
#include <mach-o/dyld.h>
#include <mach-o/getsect.h>

static GSList*
load_modules (void)
{
	GSList *ret = NULL;
	WapiProcModule *mod;
	uint32_t count = _dyld_image_count ();
	int i = 0;

	for (i = 0; i < count; i++) {
#if SIZEOF_VOID_P == 8
		const struct mach_header_64 *hdr;
		const struct section_64 *sec;
#else
		const struct mach_header *hdr;
		const struct section *sec;
#endif
		const char *name;

		name = _dyld_get_image_name (i);
#if SIZEOF_VOID_P == 8
		hdr = (const struct mach_header_64*)_dyld_get_image_header (i);
		sec = getsectbynamefromheader_64 (hdr, SEG_DATA, SECT_DATA);
#else
		hdr = _dyld_get_image_header (i);
		sec = getsectbynamefromheader (hdr, SEG_DATA, SECT_DATA);
#endif

		/* Some dynlibs do not have data sections on osx (#533893) */
		if (sec == 0) {
			continue;
		}

		mod = g_new0 (WapiProcModule, 1);
		mod->address_start = GINT_TO_POINTER (sec->addr);
		mod->address_end = GINT_TO_POINTER (sec->addr+sec->size);
		mod->perms = g_strdup ("r--p");
		mod->address_offset = 0;
		mod->device = makedev (0, 0);
		mod->inode = i;
		mod->filename = g_strdup (name);

		if (g_slist_find_custom (ret, mod, find_procmodule) == NULL) {
			ret = g_slist_prepend (ret, mod);
		} else {
			free_procmodule (mod);
		}
	}

	ret = g_slist_reverse (ret);

	return(ret);
}
#elif defined(USE_BSD_LOADER)
#include <link.h>

static int
load_modules_callback (struct dl_phdr_info *info, size_t size, void *ptr)
{
	if (size < offsetof (struct dl_phdr_info, dlpi_phnum) + sizeof (info->dlpi_phnum))
		return (-1);

	struct dl_phdr_info *cpy = g_calloc (1, sizeof(struct dl_phdr_info));
	if (!cpy)
		return (-1);

	memcpy(cpy, info, sizeof(*info));

	g_ptr_array_add ((GPtrArray *)ptr, cpy);

	return (0);
}

static GSList*
load_modules (void)
{
	GSList *ret = NULL;
	WapiProcModule *mod;
	GPtrArray *dlarray = g_ptr_array_new();
	int i;

	if (dl_iterate_phdr(load_modules_callback, dlarray) < 0)
		return (ret);

	for (i = 0; i < dlarray->len; i++) {
		struct dl_phdr_info *info = g_ptr_array_index (dlarray, i);

		mod = g_new0 (WapiProcModule, 1);
		mod->address_start = (gpointer)(info->dlpi_addr + info->dlpi_phdr[0].p_vaddr);
		mod->address_end = (gpointer)(info->dlpi_addr + info->dlpi_phdr[info->dlpi_phnum - 1].p_vaddr);
		mod->perms = g_strdup ("r--p");
		mod->address_offset = 0;
		mod->inode = i;
		mod->filename = g_strdup (info->dlpi_name);

		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: inode=%d, filename=%s, address_start=%p, address_end=%p",
			__func__, mod->inode, mod->filename, mod->address_start, mod->address_end);

		g_free (info);

		if (g_slist_find_custom (ret, mod, find_procmodule) == NULL) {
			ret = g_slist_prepend (ret, mod);
		} else {
			free_procmodule (mod);
		}
	}

	g_ptr_array_free (dlarray, TRUE);

	ret = g_slist_reverse (ret);

	return(ret);
}
#elif defined(USE_HAIKU_LOADER)
static GSList*
load_modules (void)
{
	GSList *ret = NULL;
	WapiProcModule *mod;
	int32 cookie = 0;
	image_info imageInfo;

	while (get_next_image_info (B_CURRENT_TEAM, &cookie, &imageInfo) == B_OK) {
		mod = g_new0 (WapiProcModule, 1);
		mod->device = imageInfo.device;
		mod->inode = imageInfo.node;
		mod->filename = g_strdup (imageInfo.name);
		mod->address_start = MIN (imageInfo.text, imageInfo.data);
		mod->address_end = MAX ((uint8_t*)imageInfo.text + imageInfo.text_size,
			(uint8_t*)imageInfo.data + imageInfo.data_size);
		mod->perms = g_strdup ("r--p");
		mod->address_offset = 0;

		if (g_slist_find_custom (ret, mod, find_procmodule) == NULL) {
			ret = g_slist_prepend (ret, mod);
		} else {
			free_procmodule (mod);
		}
	}

	ret = g_slist_reverse (ret);

	return ret;
}
#else
static GSList*
load_modules (FILE *fp)
{
	GSList *ret = NULL;
	WapiProcModule *mod;
	char buf[MAXPATHLEN + 1], *p, *endp;
	char *start_start, *end_start, *prot_start, *offset_start;
	char *maj_dev_start, *min_dev_start, *inode_start, prot_buf[5];
	gpointer address_start, address_end, address_offset;
	guint32 maj_dev, min_dev;
	guint64 inode;
	guint64 device;

	while (fgets (buf, sizeof(buf), fp)) {
		p = buf;
		while (g_ascii_isspace (*p)) ++p;
		start_start = p;
		if (!g_ascii_isxdigit (*start_start)) {
			continue;
		}
		address_start = (gpointer)strtoul (start_start, &endp, 16);
		p = endp;
		if (*p != '-') {
			continue;
		}

		++p;
		end_start = p;
		if (!g_ascii_isxdigit (*end_start)) {
			continue;
		}
		address_end = (gpointer)strtoul (end_start, &endp, 16);
		p = endp;
		if (!g_ascii_isspace (*p)) {
			continue;
		}

		while (g_ascii_isspace (*p)) ++p;
		prot_start = p;
		if (*prot_start != 'r' && *prot_start != '-') {
			continue;
		}
		memcpy (prot_buf, prot_start, 4);
		prot_buf[4] = '\0';
		while (!g_ascii_isspace (*p)) ++p;

		while (g_ascii_isspace (*p)) ++p;
		offset_start = p;
		if (!g_ascii_isxdigit (*offset_start)) {
			continue;
		}
		address_offset = (gpointer)strtoul (offset_start, &endp, 16);
		p = endp;
		if (!g_ascii_isspace (*p)) {
			continue;
		}

		while(g_ascii_isspace (*p)) ++p;
		maj_dev_start = p;
		if (!g_ascii_isxdigit (*maj_dev_start)) {
			continue;
		}
		maj_dev = strtoul (maj_dev_start, &endp, 16);
		p = endp;
		if (*p != ':') {
			continue;
		}

		++p;
		min_dev_start = p;
		if (!g_ascii_isxdigit (*min_dev_start)) {
			continue;
		}
		min_dev = strtoul (min_dev_start, &endp, 16);
		p = endp;
		if (!g_ascii_isspace (*p)) {
			continue;
		}

		while (g_ascii_isspace (*p)) ++p;
		inode_start = p;
		if (!g_ascii_isxdigit (*inode_start)) {
			continue;
		}
		inode = (guint64)strtol (inode_start, &endp, 10);
		p = endp;
		if (!g_ascii_isspace (*p)) {
			continue;
		}

		device = makedev ((int)maj_dev, (int)min_dev);
		if ((device == 0) &&
		    (inode == 0)) {
			continue;
		}

		while(g_ascii_isspace (*p)) ++p;
		/* p now points to the filename */

		mod = g_new0 (WapiProcModule, 1);
		mod->address_start = address_start;
		mod->address_end = address_end;
		mod->perms = g_strdup (prot_buf);
		mod->address_offset = address_offset;
		mod->device = device;
		mod->inode = inode;
		mod->filename = g_strdup (g_strstrip (p));

		if (g_slist_find_custom (ret, mod, find_procmodule) == NULL) {
			ret = g_slist_prepend (ret, mod);
		} else {
			free_procmodule (mod);
		}
	}

	ret = g_slist_reverse (ret);

	return(ret);
}
#endif

static gboolean
get_process_modules (gpointer process, gpointer *modules, guint32 size, guint32 *needed)
{
	WapiHandle_process *process_handle;
#if !defined(USE_OSX_LOADER) && !defined(USE_BSD_LOADER)
	FILE *fp;
#endif
	GSList *mods = NULL;
	WapiProcModule *module;
	guint32 count, avail = size / sizeof(gpointer);
	int i;
	pid_t pid;
	char *proc_name = NULL;
	gboolean res;

	/* Store modules in an array of pointers (main module as
	 * modules[0]), using the load address for each module as a
	 * token.  (Use 'NULL' as an alternative for the main module
	 * so that the simple implementation can just return one item
	 * for now.)  Get the info from /proc/<pid>/maps on linux,
	 * /proc/<pid>/map on FreeBSD, other systems will have to
	 * implement /dev/kmem reading or whatever other horrid
	 * technique is needed.
	 */
	if (size < sizeof(gpointer))
		return FALSE;

	if (WAPI_IS_PSEUDO_PROCESS_HANDLE (process)) {
		pid = WAPI_HANDLE_TO_PID (process);
		proc_name = get_process_name_from_proc (pid);
	} else {
		res = mono_w32handle_lookup (process, MONO_W32HANDLE_PROCESS, (gpointer*) &process_handle);
		if (!res) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Can't find process %p", __func__, process);
			return FALSE;
		}

		pid = process_handle->id;
		proc_name = g_strdup (process_handle->proc_name);
	}

#if defined(USE_OSX_LOADER) || defined(USE_BSD_LOADER) || defined(USE_HAIKU_LOADER)
	mods = load_modules ();
	if (!proc_name) {
		modules[0] = NULL;
		*needed = sizeof(gpointer);
		return TRUE;
	}
#else
	fp = open_process_map (pid, "r");
	if (!fp) {
		/* No /proc/<pid>/maps so just return the main module
		 * shortcut for now
		 */
		modules[0] = NULL;
		*needed = sizeof(gpointer);
		g_free (proc_name);
		return TRUE;
	}
	mods = load_modules (fp);
	fclose (fp);
#endif
	count = g_slist_length (mods);

	/* count + 1 to leave slot 0 for the main module */
	*needed = sizeof(gpointer) * (count + 1);

	/*
	 * Use the NULL shortcut, as the first line in
	 * /proc/<pid>/maps isn't the executable, and we need
	 * that first in the returned list. Check the module name
	 * to see if it ends with the proc name and substitute
	 * the first entry with it.  FIXME if this turns out to
	 * be a problem.
	 */
	modules[0] = NULL;
	for (i = 0; i < (avail - 1) && i < count; i++) {
		module = (WapiProcModule *)g_slist_nth_data (mods, i);
		if (modules[0] != NULL)
			modules[i] = module->address_start;
		else if (match_procname_to_modulename (proc_name, module->filename))
			modules[0] = module->address_start;
		else
			modules[i + 1] = module->address_start;
	}

	for (i = 0; i < count; i++) {
		free_procmodule ((WapiProcModule *)g_slist_nth_data (mods, i));
	}
	g_slist_free (mods);
	g_free (proc_name);

	return TRUE;
}

static guint32
get_module_filename (gpointer process, gpointer module,
					 gunichar2 *basename, guint32 size)
{
	gint pid, len;
	gsize bytes;
	gchar *path;
	gunichar2 *proc_path;

	size *= sizeof (gunichar2); /* adjust for unicode characters */

	if (basename == NULL || size == 0)
		return 0;

	pid = GetProcessId (process);

	path = wapi_process_get_path (pid);
	if (path == NULL)
		return 0;

	proc_path = mono_unicode_from_external (path, &bytes);
	g_free (path);

	if (proc_path == NULL)
		return 0;

	len = (bytes / 2);

	/* Add the terminator */
	bytes += 2;

	if (size < bytes) {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Size %d smaller than needed (%ld); truncating", __func__, size, bytes);
		memcpy (basename, proc_path, size);
	} else {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Size %d larger than needed (%ld)", __func__, size, bytes);
		memcpy (basename, proc_path, bytes);
	}

	g_free (proc_path);

	return len;
}

static guint32
get_module_name (gpointer process, gpointer module, gunichar2 *basename, guint32 size, gboolean base)
{
	WapiHandle_process *process_handle;
	pid_t pid;
	gunichar2 *procname;
	char *procname_ext = NULL;
	glong len;
	gsize bytes;
#if !defined(USE_OSX_LOADER) && !defined(USE_BSD_LOADER)
	FILE *fp;
#endif
	GSList *mods = NULL;
	WapiProcModule *found_module;
	guint32 count;
	int i;
	char *proc_name = NULL;
	gboolean res;

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Getting module base name, process handle %p module %p",
		   __func__, process, module);

	size = size * sizeof (gunichar2); /* adjust for unicode characters */

	if (basename == NULL || size == 0)
		return 0;

	if (WAPI_IS_PSEUDO_PROCESS_HANDLE (process)) {
		/* This is a pseudo handle */
		pid = (pid_t)WAPI_HANDLE_TO_PID (process);
		proc_name = get_process_name_from_proc (pid);
	} else {
		res = mono_w32handle_lookup (process, MONO_W32HANDLE_PROCESS, (gpointer*) &process_handle);
		if (!res) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Can't find process %p", __func__, process);
			return 0;
		}

		pid = process_handle->id;
		proc_name = g_strdup (process_handle->proc_name);
	}

	/* Look up the address in /proc/<pid>/maps */
#if defined(USE_OSX_LOADER) || defined(USE_BSD_LOADER) || defined(USE_HAIKU_LOADER)
	mods = load_modules ();
#else
	fp = open_process_map (pid, "r");
	if (fp == NULL) {
		if (errno == EACCES && module == NULL && base == TRUE) {
			procname_ext = get_process_name_from_proc (pid);
		} else {
			/* No /proc/<pid>/maps, so just return failure
			 * for now
			 */
			g_free (proc_name);
			return 0;
		}
	} else {
		mods = load_modules (fp);
		fclose (fp);
	}
#endif
	count = g_slist_length (mods);

	/* If module != NULL compare the address.
	 * If module == NULL we are looking for the main module.
	 * The best we can do for now check it the module name end with the process name.
	 */
	for (i = 0; i < count; i++) {
		found_module = (WapiProcModule *)g_slist_nth_data (mods, i);
		if (procname_ext == NULL &&
			((module == NULL && match_procname_to_modulename (proc_name, found_module->filename)) ||
			 (module != NULL && found_module->address_start == module))) {
			if (base)
				procname_ext = g_path_get_basename (found_module->filename);
			else
				procname_ext = g_strdup (found_module->filename);
		}

		free_procmodule (found_module);
	}

	if (procname_ext == NULL) {
		/* If it's *still* null, we might have hit the
		 * case where reading /proc/$pid/maps gives an
		 * empty file for this user.
		 */
		procname_ext = get_process_name_from_proc (pid);
	}

	g_slist_free (mods);
	g_free (proc_name);

	if (procname_ext) {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Process name is [%s]", __func__,
			   procname_ext);

		procname = mono_unicode_from_external (procname_ext, &bytes);
		if (procname == NULL) {
			/* bugger */
			g_free (procname_ext);
			return 0;
		}

		len = (bytes / 2);

		/* Add the terminator */
		bytes += 2;

		if (size < bytes) {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Size %d smaller than needed (%ld); truncating", __func__, size, bytes);

			memcpy (basename, procname, size);
		} else {
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Size %d larger than needed (%ld)",
				   __func__, size, bytes);

			memcpy (basename, procname, bytes);
		}

		g_free (procname);
		g_free (procname_ext);

		return len;
	}

	return 0;
}

/* Returns an array of System.Diagnostics.ProcessModule */
MonoArray *
ves_icall_System_Diagnostics_Process_GetModules_internal (MonoObject *this_obj, HANDLE process)
{
	MonoError error;
	MonoArray *temp_arr = NULL;
	MonoArray *arr;
	HMODULE mods[1024];
	gunichar2 filename[MAX_PATH];
	gunichar2 modname[MAX_PATH];
	DWORD needed;
	guint32 count = 0, module_count = 0, assembly_count = 0;
	guint32 i, num_added = 0;
	GPtrArray *assemblies = NULL;

	stash_system_assembly (this_obj);

	if (GetProcessId (process) == mono_process_current_pid ()) {
		assemblies = get_domain_assemblies (mono_domain_get ());
		assembly_count = assemblies->len;
	}

	if (get_process_modules (process, mods, sizeof(mods), &needed)) {
		module_count += needed / sizeof(HMODULE);
	}

	count = module_count + assembly_count; 
	temp_arr = mono_array_new_checked (mono_domain_get (), mono_class_get_process_module_class (), count, &error);
	if (mono_error_set_pending_exception (&error))
		return NULL;

	for (i = 0; i < module_count; i++) {
		if (get_module_name (process, mods[i], modname, MAX_PATH, TRUE) &&
				get_module_filename (process, mods[i], filename, MAX_PATH)) {
			MonoObject *module = process_add_module (process, mods[i],
													 filename, modname, mono_class_get_process_module_class (), &error);
			if (!mono_error_ok (&error)) {
				mono_error_set_pending_exception (&error);
				return NULL;
			}
			mono_array_setref (temp_arr, num_added++, module);
		}
	}

	if (assemblies) {
		for (i = 0; i < assembly_count; i++) {
			MonoAssembly *ass = (MonoAssembly *)g_ptr_array_index (assemblies, i);
			MonoObject *module = get_process_module (ass, mono_class_get_process_module_class (), &error);
			if (!mono_error_ok (&error)) {
				mono_error_set_pending_exception (&error);
				return NULL;
			}
			mono_array_setref (temp_arr, num_added++, module);
		}
		g_ptr_array_free (assemblies, TRUE);
	}

	if (count == num_added) {
		arr = temp_arr;
	} else {
		/* shorter version of the array */
		arr = mono_array_new_checked (mono_domain_get (), mono_class_get_process_module_class (), num_added, &error);
		if (mono_error_set_pending_exception (&error))
			return NULL;

		for (i = 0; i < num_added; i++)
			mono_array_setref (arr, i, mono_array_get (temp_arr, MonoObject*, i));
	}

	return arr;
}

void
ves_icall_System_Diagnostics_FileVersionInfo_GetVersionInfo_internal (MonoObject *this_obj, MonoString *filename)
{
	MonoError error;

	stash_system_assembly (this_obj);
	
	mono_process_get_fileversion (this_obj, mono_string_chars (filename), &error);
	if (!mono_error_ok (&error)) {
		mono_error_set_pending_exception (&error);
		return;
	}
	process_set_field_string (this_obj, "filename",
							  mono_string_chars (filename),
							  mono_string_length (filename), &error);
	if (!mono_error_ok (&error)) {
		mono_error_set_pending_exception (&error);
		return;
	}
}

/* Only used when UseShellExecute is false */
static inline gchar *
mono_process_quote_path (const gchar *path)
{
	return g_shell_quote (path);
}

static inline gchar *
mono_process_unquote_application_name (gchar *path)
{
	return path;
}

/* Only used when UseShellExecute is false */
static gboolean
mono_process_complete_path (const gunichar2 *appname, gchar **completed)
{
	gchar *utf8app, *utf8appmemory;
	gchar *found;

	utf8appmemory = g_utf16_to_utf8 (appname, -1, NULL, NULL, NULL);
	utf8app = mono_process_unquote_application_name (utf8appmemory);

	if (g_path_is_absolute (utf8app)) {
		*completed = mono_process_quote_path (utf8app);
		g_free (utf8appmemory);
		return TRUE;
	}

	if (g_file_test (utf8app, G_FILE_TEST_IS_EXECUTABLE) && !g_file_test (utf8app, G_FILE_TEST_IS_DIR)) {
		*completed = mono_process_quote_path (utf8app);
		g_free (utf8appmemory);
		return TRUE;
	}
	
	found = g_find_program_in_path (utf8app);
	if (found == NULL) {
		*completed = NULL;
		g_free (utf8appmemory);
		return FALSE;
	}

	*completed = mono_process_quote_path (found);
	g_free (found);
	g_free (utf8appmemory);
	return TRUE;
}

MonoBoolean
ves_icall_System_Diagnostics_Process_ShellExecuteEx_internal (MonoProcessStartInfo *proc_start_info, MonoProcInfo *process_info)
{
	SHELLEXECUTEINFO shellex = {0};
	gboolean ret;

	shellex.cbSize = sizeof(SHELLEXECUTEINFO);
	shellex.fMask = (gulong)(SEE_MASK_FLAG_DDEWAIT | SEE_MASK_NOCLOSEPROCESS | SEE_MASK_UNICODE);
	shellex.nShow = (gulong)proc_start_info->window_style;
	shellex.nShow = (gulong)((shellex.nShow == 0) ? 1 : (shellex.nShow == 1 ? 0 : shellex.nShow));

	if (proc_start_info->filename != NULL) {
		shellex.lpFile = mono_string_chars (proc_start_info->filename);
	}

	if (proc_start_info->arguments != NULL) {
		shellex.lpParameters = mono_string_chars (proc_start_info->arguments);
	}

	if (proc_start_info->verb != NULL &&
	    mono_string_length (proc_start_info->verb) != 0) {
		shellex.lpVerb = mono_string_chars (proc_start_info->verb);
	}

	if (proc_start_info->working_directory != NULL &&
	    mono_string_length (proc_start_info->working_directory) != 0) {
		shellex.lpDirectory = mono_string_chars (proc_start_info->working_directory);
	}

	if (proc_start_info->error_dialog) {	
		shellex.hwnd = proc_start_info->error_dialog_parent_handle;
	} else {
		shellex.fMask = (gulong)(shellex.fMask | SEE_MASK_FLAG_NO_UI);
	}

	ret = ShellExecuteEx (&shellex);
	if (ret == FALSE) {
		process_info->pid = -GetLastError ();
	} else {
		process_info->process_handle = shellex.hProcess;
		process_info->thread_handle = NULL;
#if !defined(MONO_CROSS_COMPILE)
		process_info->pid = GetProcessId (shellex.hProcess);
#else
		process_info->pid = 0;
#endif
		process_info->tid = 0;
	}

	return ret;
}

static inline void
mono_process_init_startup_info (HANDLE stdin_handle, HANDLE stdout_handle, HANDLE stderr_handle, STARTUPINFO *startinfo)
{
	startinfo->cb = sizeof(STARTUPINFO);
	startinfo->dwFlags = STARTF_USESTDHANDLES;
	startinfo->hStdInput = stdin_handle;
	startinfo->hStdOutput = stdout_handle;
	startinfo->hStdError = stderr_handle;
	return;
}

static gboolean
mono_process_get_shell_arguments (MonoProcessStartInfo *proc_start_info, gunichar2 **shell_path, MonoString **cmd)
{
	gchar *spath = NULL;

	*shell_path = NULL;
	*cmd = proc_start_info->arguments;

	mono_process_complete_path (mono_string_chars (proc_start_info->filename), &spath);
	if (spath != NULL) {
		*shell_path = g_utf8_to_utf16 (spath, -1, NULL, NULL, NULL);
		g_free (spath);
	}

	return (*shell_path != NULL) ? TRUE : FALSE;
}

static gboolean
mono_process_create_process (MonoProcInfo *mono_process_info, gunichar2 *shell_path,
			     MonoString *cmd, guint32 creation_flags, gchar *env_vars,
			     gunichar2 *dir, STARTUPINFO *start_info, PROCESS_INFORMATION *process_info)
{
	gboolean result = FALSE;

	if (mono_process_info->username) {
		guint32 logon_flags = mono_process_info->load_user_profile ? LOGON_WITH_PROFILE : 0;

		result = CreateProcessWithLogonW (mono_string_chars (mono_process_info->username),
						  mono_process_info->domain ? mono_string_chars (mono_process_info->domain) : NULL,
						  (const gunichar2 *)mono_process_info->password,
						  logon_flags,
						  shell_path,
						  cmd ? mono_string_chars (cmd) : NULL,
						  creation_flags,
						  env_vars, dir, start_info, process_info);

	} else {

		result = CreateProcess (shell_path,
					cmd ? mono_string_chars (cmd): NULL,
					NULL,
					NULL,
					TRUE,
					creation_flags,
					env_vars,
					dir,
					start_info,
					process_info);

	}

	return result;
}

MonoBoolean
ves_icall_System_Diagnostics_Process_CreateProcess_internal (MonoProcessStartInfo *proc_start_info, HANDLE stdin_handle,
							     HANDLE stdout_handle, HANDLE stderr_handle, MonoProcInfo *process_info)
{
	gboolean ret;
	gunichar2 *dir;
	STARTUPINFO startinfo={0};
	PROCESS_INFORMATION procinfo;
	gunichar2 *shell_path = NULL;
	gchar *env_vars = NULL;
	MonoString *cmd = NULL;
	guint32 creation_flags;

	mono_process_init_startup_info (stdin_handle, stdout_handle, stderr_handle, &startinfo);

	creation_flags = CREATE_UNICODE_ENVIRONMENT;
	if (proc_start_info->create_no_window)
		creation_flags |= CREATE_NO_WINDOW;
	
	if (mono_process_get_shell_arguments (proc_start_info, &shell_path, &cmd) == FALSE) {
		process_info->pid = -ERROR_FILE_NOT_FOUND;
		return FALSE;
	}

	if (process_info->env_keys) {
		gint i, len; 
		MonoString *ms;
		MonoString *key, *value;
		gunichar2 *str, *ptr;
		gunichar2 *equals16;

		for (len = 0, i = 0; i < mono_array_length (process_info->env_keys); i++) {
			ms = mono_array_get (process_info->env_values, MonoString *, i);
			if (ms == NULL)
				continue;

			len += mono_string_length (ms) * sizeof (gunichar2);
			ms = mono_array_get (process_info->env_keys, MonoString *, i);
			len += mono_string_length (ms) * sizeof (gunichar2);
			len += 2 * sizeof (gunichar2);
		}

		equals16 = g_utf8_to_utf16 ("=", 1, NULL, NULL, NULL);
		ptr = str = g_new0 (gunichar2, len + 1);
		for (i = 0; i < mono_array_length (process_info->env_keys); i++) {
			value = mono_array_get (process_info->env_values, MonoString *, i);
			if (value == NULL)
				continue;

			key = mono_array_get (process_info->env_keys, MonoString *, i);
			memcpy (ptr, mono_string_chars (key), mono_string_length (key) * sizeof (gunichar2));
			ptr += mono_string_length (key);

			memcpy (ptr, equals16, sizeof (gunichar2));
			ptr++;

			memcpy (ptr, mono_string_chars (value), mono_string_length (value) * sizeof (gunichar2));
			ptr += mono_string_length (value);
			ptr++;
		}

		g_free (equals16);
		env_vars = (gchar *) str;
	}
	
	/* The default dir name is "".  Turn that into NULL to mean
	 * "current directory"
	 */
	if (proc_start_info->working_directory == NULL || mono_string_length (proc_start_info->working_directory) == 0)
		dir = NULL;
	else
		dir = mono_string_chars (proc_start_info->working_directory);

	ret = mono_process_create_process (process_info, shell_path, cmd, creation_flags, env_vars, dir, &startinfo, &procinfo);

	g_free (env_vars);
	if (shell_path != NULL)
		g_free (shell_path);

	if (ret) {
		process_info->process_handle = procinfo.hProcess;
		/*process_info->thread_handle=procinfo.hThread;*/
		process_info->thread_handle = NULL;
		if (procinfo.hThread != NULL && procinfo.hThread != INVALID_HANDLE_VALUE)
			CloseHandle (procinfo.hThread);
		process_info->pid = procinfo.dwProcessId;
		process_info->tid = procinfo.dwThreadId;
	} else {
		process_info->pid = -GetLastError ();
	}
	
	return ret;
}

MonoString *
ves_icall_System_Diagnostics_Process_ProcessName_internal (HANDLE process)
{
	MonoError error;
	MonoString *string;
	gunichar2 name[MAX_PATH];
	guint32 len;
	gboolean ok;
	HMODULE mod;
	DWORD needed;

	ok = get_process_modules (process, &mod, sizeof(mod), &needed);
	if (!ok)
		return NULL;

	len = get_module_name (process, mod, name, MAX_PATH, TRUE);

	if (len == 0)
		return NULL;
	
	LOGDEBUG (g_message ("%s: process name is [%s]", __func__, g_utf16_to_utf8 (name, -1, NULL, NULL, NULL)));
	
	string = mono_string_new_utf16_checked (mono_domain_get (), name, len, &error);
	if (!mono_error_ok (&error))
		mono_error_set_pending_exception (&error);
	
	return string;
}

/* Returns an array of pids */
MonoArray *
ves_icall_System_Diagnostics_Process_GetProcesses_internal (void)
{
	MonoError error;
	MonoArray *procs;
	gpointer *pidarray;
	int i, count;

	pidarray = mono_process_list (&count);
	if (!pidarray) {
		mono_set_pending_exception (mono_get_exception_not_supported ("This system does not support EnumProcesses"));
		return NULL;
	}
	procs = mono_array_new_checked (mono_domain_get (), mono_get_int32_class (), count, &error);
	if (mono_error_set_pending_exception (&error)) {
		g_free (pidarray);
		return NULL;
	}
	if (sizeof (guint32) == sizeof (gpointer)) {
		memcpy (mono_array_addr (procs, guint32, 0), pidarray, count * sizeof (gint32));
	} else {
		for (i = 0; i < count; ++i)
			*(mono_array_addr (procs, guint32, i)) = GPOINTER_TO_UINT (pidarray [i]);
	}
	g_free (pidarray);

	return procs;
}

gint64
ves_icall_System_Diagnostics_Process_GetProcessData (int pid, gint32 data_type, gint32 *error)
{
	MonoProcessError perror;
	guint64 res;

	res = mono_process_get_data_with_error (GINT_TO_POINTER (pid), (MonoProcessData)data_type, &perror);
	if (error)
		*error = perror;
	return res;
}
