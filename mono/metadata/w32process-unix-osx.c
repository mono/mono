/**
 * \file
 */

#include "w32process.h"
#include "w32process-unix-internals.h"

#ifdef USE_OSX_BACKEND

#include <errno.h>
#include <unistd.h>
#include <sys/time.h>
#include <sys/proc.h>
#include <sys/sysctl.h>
#include <sys/utsname.h>
#include <mach-o/dyld.h>
#include <mach-o/getsect.h>
#include <dlfcn.h>

/* sys/resource.h (for rusage) is required when using osx 10.3 (but not 10.4) */
#ifdef __APPLE__
#include <TargetConditionals.h>
#include <sys/resource.h>
#ifdef HAVE_LIBPROC_H
/* proc_name */
#include <libproc.h>
#endif
#endif

#include "utils/mono-logger-internals.h"
#include "icall-decl.h"

gchar*
mono_w32process_get_name (pid_t pid)
{
	gchar *ret = NULL;

#if defined (__mono_ppc__) || !defined (TARGET_OSX)
	size_t size;
	struct kinfo_proc *pi;
	gint mib[] = { CTL_KERN, KERN_PROC, KERN_PROC_PID, pid };

	if (sysctl(mib, 4, NULL, &size, NULL, 0) < 0)
		return(ret);

	if ((pi = g_malloc (size)) == NULL)
		return(ret);

	if (sysctl (mib, 4, pi, &size, NULL, 0) < 0) {
		if (errno == ENOMEM) {
			g_free (pi);
			mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER_PROCESS, "%s: Didn't allocate enough memory for kproc info", __func__);
		}
		return(ret);
	}

	if (strlen (pi->kp_proc.p_comm) > 0)
		ret = g_strdup (pi->kp_proc.p_comm);

	g_free (pi);
#else
	gchar buf[256];
	gint res;

	/* No proc name on OSX < 10.5 nor ppc nor iOS */
	memset (buf, '\0', sizeof(buf));
	res = proc_name (pid, buf, sizeof(buf));
	if (res == 0) {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER_PROCESS, "%s: proc_name failed, error (%d) \"%s\"", __func__, errno, g_strerror (errno));
		return NULL;
	}

	// Fixes proc_name triming values to 15 characters #32539
	if (strlen (buf) >= MAXCOMLEN - 1) {
		gchar path_buf [PROC_PIDPATHINFO_MAXSIZE];
		gchar *name_buf;
		gint path_len;

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
#endif

	return ret;
}

gchar*
mono_w32process_get_path (pid_t pid)
{
#if defined(__mono_ppc__) || !defined(TARGET_OSX)
	return mono_w32process_get_name (pid);
#else
	gchar buf [PROC_PIDPATHINFO_MAXSIZE];
	gint res;

	res = proc_pidpath (pid, buf, sizeof (buf));
	if (res <= 0)
		return NULL;
	if (buf [0] == '\0')
		return NULL;
	return g_strdup (buf);
#endif
}

struct mono_dyld_image_info
{
	const void *header_addr;
	const void *data_section_start;
	const void *data_section_end;
	const char *name;
	guint64 order;
	struct mono_dyld_image_info *limbo_next;
};

static MonoConcurrentHashTable *images;
static mono_mutex_t images_mutex;
static volatile gpointer limbo_head;

static void
mono_dyld_image_info_free (struct mono_dyld_image_info *info)
{
	g_free ((void *) info->name);
	g_free (info);
}

static int
sort_modules_by_load_order (gconstpointer a, gconstpointer b)
{
	MonoW32ProcessModule *ma = (MonoW32ProcessModule *) a;
	MonoW32ProcessModule *mb = (MonoW32ProcessModule *) b;
	return ma->inode == mb->inode ? 0 : ma->inode < mb->inode ? -1 : 1;
}

static void
iter_images (gpointer key, gpointer val, gpointer user_data)
{
	GSList **data = (GSList **) user_data;
	struct mono_dyld_image_info *info = (struct mono_dyld_image_info *) val;

	MonoW32ProcessModule *mod = g_new0 (MonoW32ProcessModule, 1);
	mod->address_start = GINT_TO_POINTER (info->data_section_start);
	mod->address_end = GINT_TO_POINTER (info->data_section_end);
	mod->perms = g_strdup ("r--p");
	mod->address_offset = 0;
	mod->device = makedev (0, 0);
	mod->inode = info->order;
	mod->filename = g_strdup (info->name);

	*data = g_slist_prepend (*data, mod);
}

static void
clear_limbo_list (void)
{
	gpointer head = mono_atomic_load_ptr (&limbo_head); /* load-relaxed */
	if (head) {
		head = mono_atomic_xchg_ptr (&limbo_head, NULL); /* xchg acquire */
		struct mono_dyld_image_info *list = (struct mono_dyld_image_info *) head;
		while (list) {
			struct mono_dyld_image_info *cur = list;
			list = list->limbo_next;
			mono_dyld_image_info_free (cur);
		}
	}
}

GSList *
mono_w32process_get_modules (pid_t pid)
{
	GSList *ret = NULL;
	MONO_ENTER_GC_SAFE;

	if (pid != getpid ())
		goto done;

	mono_os_mutex_lock (&images_mutex);
	mono_conc_hashtable_foreach_snapshot (images, mono_hazard_pointer_get (), iter_images, &ret);
	mono_os_mutex_unlock (&images_mutex);

	ret = g_slist_sort (ret, &sort_modules_by_load_order);

	clear_limbo_list ();

done:
	MONO_EXIT_GC_SAFE;
	return ret;
}

static void
free_if_locked (int lock_status, struct mono_dyld_image_info *info)
{
	if (!info) return;
	if (lock_status) {
		mono_dyld_image_info_free (info);
		return;
	}
	gpointer head = NULL;
	do {
		head = mono_atomic_load_ptr (&limbo_head); /* load-relaxed */
		info->limbo_next = (struct mono_dyld_image_info *) head;
	} while (mono_atomic_cas_ptr (&limbo_head, info, head) != head); /* strong cas release on success, relaxed on failure */
}

static guint64 dyld_order = 0;

static void
image_added (const struct mach_header *hdr32, intptr_t vmaddr_slide)
{
	#if SIZEOF_VOID_P == 8
	const struct mach_header_64 *hdr64 = (const struct mach_header_64 *)hdr32;
	const struct section_64 *sec = getsectbynamefromheader_64 (hdr64, SEG_DATA, SECT_DATA);
	#else
	const struct section *sec = getsectbynamefromheader (hdr32, SEG_DATA, SECT_DATA);
	#endif
	Dl_info dlinfo;
	if (!dladdr (hdr32, &dlinfo)) return;
	if (sec == NULL) return;

	struct mono_dyld_image_info *info = g_new0 (struct mono_dyld_image_info, 1);
	info->header_addr = hdr32;
	info->data_section_start = GINT_TO_POINTER (sec->addr);
	info->data_section_end = GINT_TO_POINTER (sec->addr + sec->size);
	info->name = g_strdup (dlinfo.dli_fname);
	info->order = dyld_order;
	++dyld_order;

	int lock_status = mono_os_mutex_trylock (&images_mutex);
	gpointer old = mono_conc_hashtable_insert (images, (gpointer) hdr32, info);
	struct mono_dyld_image_info *old_info = (struct mono_dyld_image_info *) old;
	free_if_locked (lock_status, old_info);
	if (lock_status)
		mono_os_mutex_unlock (&images_mutex);
}

static void
image_removed (const struct mach_header *hdr32, intptr_t vmaddr_slide)
{
	int lock_status = mono_os_mutex_trylock (&images_mutex);
	gpointer old = mono_conc_hashtable_remove (images, (gpointer) hdr32);
	struct mono_dyld_image_info *old_info = (struct mono_dyld_image_info *) old;
	free_if_locked (lock_status, old_info);
	if (lock_status)
		mono_os_mutex_unlock (&images_mutex);
}

void
mono_w32process_platform_init_once (void)
{
	images = mono_conc_hashtable_new (NULL, NULL);
	mono_os_mutex_init (&images_mutex);
	_dyld_register_func_for_add_image (&image_added);
	_dyld_register_func_for_remove_image (&image_removed);
}

#else

MONO_EMPTY_SOURCE_FILE (w32process_unix_osx);

#endif
