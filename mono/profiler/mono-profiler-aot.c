/*
 * mono-profiler-aot.c: Ahead of Time Compiler Profiler for Mono.
 *
 *
 * Copyright 2008-2009 Novell, Inc (http://www.novell.com)
 *
 * This profiler collects profiling information usable by the Mono AOT compiler
 * to generate better code. It saves the information into files under ~/.mono. 
 * The AOT compiler can load these files during compilation.
 * Currently, only the order in which methods were compiled is saved, 
 * allowing more efficient function ordering in the AOT files.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include <config.h>

#include "mono-profiler-aot.h"

#include <mono/metadata/profiler.h>
#include <mono/metadata/tokentype.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/class-internals.h>
#include <mono/utils/mono-os-mutex.h>
#include <string.h>
#include <errno.h>
#include <stdlib.h>
#include <glib.h>
#include <sys/stat.h>

#ifdef HOST_WIN32
#include <direct.h>
#endif

struct _MonoProfiler {
	GHashTable *classes;
	GHashTable *images;
	GPtrArray *methods;
	FILE *outfile;
	int id;
	char *outfile_name;
};

static mono_mutex_t mutex;
static gboolean verbose;

static void
prof_jit_enter (MonoProfiler *prof, MonoMethod *method)
{
}

static void
prof_jit_leave (MonoProfiler *prof, MonoMethod *method, int result)
{
	MonoImage *image = mono_class_get_image (mono_method_get_class (method));

	if (!image->assembly || method->wrapper_type)
		return;

	mono_os_mutex_lock (&mutex);
	g_ptr_array_add (prof->methods, method);
	mono_os_mutex_unlock (&mutex);
}

static void
prof_shutdown (MonoProfiler *prof);

static void
usage (int do_exit)
{
	printf ("AOT profiler.\n");
	printf ("Usage: mono --profile=aot[:OPTION1[,OPTION2...]] program.exe\n");
	printf ("Options:\n");
	printf ("\thelp                 show this usage info\n");
	printf ("\toutput=FILENAME      write the data to file FILENAME (required)\n");
	printf ("\tverbose              print diagnostic info\n");
	if (do_exit)
		exit (1);
}

static const char*
match_option (const char* p, const char *opt, char **rval)
{
	int len = strlen (opt);
	if (strncmp (p, opt, len) == 0) {
		if (rval) {
			if (p [len] == '=' && p [len + 1]) {
				const char *opt = p + len + 1;
				const char *end = strchr (opt, ',');
				char *val;
				int l;
				if (end == NULL) {
					l = strlen (opt);
				} else {
					l = end - opt;
				}
				val = (char *) g_malloc (l + 1);
				memcpy (val, opt, l);
				val [l] = 0;
				*rval = val;
				return opt + l;
			}
			if (p [len] == 0 || p [len] == ',') {
				*rval = NULL;
				return p + len + (p [len] == ',');
			}
			usage (1);
		} else {
			if (p [len] == 0)
				return p + len;
			if (p [len] == ',')
				return p + len + 1;
		}
	}
	return p;
}

void
mono_profiler_startup (const char *desc);

/* the entry point */
void
mono_profiler_startup (const char *desc)
{
	MonoProfiler *prof;
	const char *p;
	const char *opt;
	char *outfile_name;

	p = desc;
	if (strncmp (p, "aot", 3))
		usage (1);
	p += 3;
	if (*p == ':')
		p++;
	for (; *p; p = opt) {
		char *val;
		if (*p == ',') {
			opt = p + 1;
			continue;
		}
		if ((opt = match_option (p, "help", NULL)) != p) {
			usage (0);
			continue;
		}
		if ((opt = match_option (p, "verbose", NULL)) != p) {
			verbose = TRUE;
			continue;
		}
		if ((opt = match_option (p, "output", &val)) != p) {
			outfile_name = val;
			continue;
		}
		fprintf (stderr, "mono-profiler-aot: Unknown option: '%s'.\n", p);
		exit (1);
	}

	if (!outfile_name) {
		fprintf (stderr, "mono-profiler-aot: The 'output' argument is required.\n");
		exit (1);
	}

	prof = g_new0 (MonoProfiler, 1);
	prof->images = g_hash_table_new (NULL, NULL);
	prof->classes = g_hash_table_new (NULL, NULL);
	prof->methods = g_ptr_array_new ();
	prof->outfile_name = outfile_name;

	mono_os_mutex_init (&mutex);

	mono_profiler_install (prof, prof_shutdown);

	mono_profiler_install_jit_compile (prof_jit_enter, prof_jit_leave);

	mono_profiler_set_events (MONO_PROFILE_JIT_COMPILATION);
}

static void
emit_byte (MonoProfiler *prof, guint8 value)
{
	fwrite (&value, 1, 1, prof->outfile);
}

static void
emit_int32 (MonoProfiler *prof, int value)
{
	// FIXME: Endianness
	fwrite (&value, 4, 1, prof->outfile);
}

static void
emit_string (MonoProfiler *prof, const char *str)
{
	int len = strlen (str);

	emit_int32 (prof, len);
	fwrite (str, len, 1, prof->outfile);
}

static void
emit_record (MonoProfiler *prof, AotProfRecordType type, int id)
{
	emit_byte (prof, type);
	emit_int32 (prof, id);
}

static int
add_image (MonoProfiler *prof, MonoImage *image)
{
	int id = GPOINTER_TO_INT (g_hash_table_lookup (prof->images, image));
	if (id)
		return id - 1;

	id = prof->id ++;
	emit_record (prof, AOTPROF_RECORD_IMAGE, id);
	emit_string (prof, image->assembly->aname.name);
	emit_string (prof, image->guid);
	g_hash_table_insert (prof->images, image, GINT_TO_POINTER (id + 1));
	return id;
}

static int
add_class (MonoProfiler *prof, MonoClass *klass);

static int
add_type (MonoProfiler *prof, MonoType *type)
{
	switch (type->type) {
#if 0
	case MONO_TYPE_SZARRAY: {
		int eid = add_type (prof, &type->data.klass->byval_arg);
		if (eid == -1)
			return -1;
		int id = prof->id ++;
		emit_record (prof, AOTPROF_RECORD_TYPE, id);
		emit_byte (prof, MONO_TYPE_SZARRAY);
		emit_int32 (prof, id);
		return id;
	}
#endif
	case MONO_TYPE_BOOLEAN:
	case MONO_TYPE_CHAR:
	case MONO_TYPE_I1:
	case MONO_TYPE_U1:
	case MONO_TYPE_I2:
	case MONO_TYPE_U2:
	case MONO_TYPE_I4:
	case MONO_TYPE_U4:
	case MONO_TYPE_I8:
	case MONO_TYPE_U8:
	case MONO_TYPE_R4:
	case MONO_TYPE_R8:
	case MONO_TYPE_I:
	case MONO_TYPE_U:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_STRING:
	case MONO_TYPE_CLASS:
	case MONO_TYPE_VALUETYPE:
	case MONO_TYPE_GENERICINST:
		return add_class (prof, mono_class_from_mono_type (type));
	default:
		return -1;
	}
}

static int
add_ginst (MonoProfiler *prof, MonoGenericInst *inst)
{
	int i, id;
	int *ids;

	// FIXME: Cache
	ids = g_malloc0 (inst->type_argc * sizeof (int));
	for (i = 0; i < inst->type_argc; ++i) {
		MonoType *t = inst->type_argv [i];
		ids [i] = add_type (prof, t);
		if (ids [i] == -1) {
			g_free (ids);
			return -1;
		}
	}
	id = prof->id ++;
	emit_record (prof, AOTPROF_RECORD_GINST, id);
	emit_int32 (prof, inst->type_argc);
	for (i = 0; i < inst->type_argc; ++i)
		emit_int32 (prof, ids [i]);
	g_free (ids);

	return id;
}

static int
add_class (MonoProfiler *prof, MonoClass *klass)
{
	int id, inst_id = -1, image_id;
	char *name;

	id = GPOINTER_TO_INT (g_hash_table_lookup (prof->classes, klass));
	if (id)
		return id - 1;

	image_id = add_image (prof, klass->image);

	if (mono_class_is_ginst (klass)) {
		MonoGenericContext *ctx = mono_class_get_context (klass);
		inst_id = add_ginst (prof, ctx->class_inst);
		if (inst_id == -1)
			return -1;
	}

	if (klass->nested_in)
		name = g_strdup_printf ("%s.%s/%s", klass->nested_in->name_space, klass->nested_in->name, klass->name);
	else
		name = g_strdup_printf ("%s.%s", klass->name_space, klass->name);

	id = prof->id ++;
	emit_record (prof, AOTPROF_RECORD_TYPE, id);
	emit_byte (prof, MONO_TYPE_CLASS);
	emit_int32 (prof, image_id);
	emit_int32 (prof, inst_id);
	emit_string (prof, name);
	g_free (name);
	g_hash_table_insert (prof->classes, klass, GINT_TO_POINTER (id + 1));
	return id;
}

static void
add_method (MonoProfiler *prof, MonoMethod *m)
{
	MonoError error;
	MonoMethodSignature *sig;
	char *s;

	sig = mono_method_signature_checked (m, &error);
	g_assert (mono_error_ok (&error));

	int class_id = add_class (prof, m->klass);
	if (class_id == -1)
		return;
	int inst_id = -1;

	if (m->is_inflated) {
		MonoGenericContext *ctx = mono_method_get_context (m);
		if (ctx->method_inst)
			inst_id = add_ginst (prof, ctx->method_inst);
	}
	int id = prof->id ++;
	emit_record (prof, AOTPROF_RECORD_METHOD, id);
	emit_int32 (prof, class_id);
	emit_int32 (prof, inst_id);
	emit_int32 (prof, sig->param_count);
	emit_string (prof, m->name);
	s = mono_signature_full_name (sig);
	emit_string (prof, s);
	g_free (s);
	if (verbose)
		printf ("%s %d\n", mono_method_full_name (m, 1), id);
}

/* called at the end of the program */
static void
prof_shutdown (MonoProfiler *prof)
{
	FILE *outfile;
	int mindex;
	char magic [32];

	printf ("Creating output file: %s\n", prof->outfile_name);

	if (prof->outfile_name [0] == '#') {
		int fd = strtol (prof->outfile_name + 1, NULL, 10);
		outfile = fdopen (fd, "a");
	} else {
		outfile = fopen (prof->outfile_name, "w+");
	}
	if (!outfile) {
		fprintf (stderr, "Unable to create output file '%s': %s.\n", prof->outfile_name, strerror (errno));
		return;
	}
	prof->outfile = outfile;

	gint32 version = (AOT_PROFILER_MAJOR_VERSION << 16) | AOT_PROFILER_MINOR_VERSION;
	sprintf (magic, AOT_PROFILER_MAGIC);
	fwrite (magic, strlen (magic), 1, outfile);
	emit_int32 (prof, version);

	GHashTable *all_methods = g_hash_table_new (NULL, NULL);
	for (mindex = 0; mindex < prof->methods->len; ++mindex) {
	    MonoMethod *m = (MonoMethod*)g_ptr_array_index (prof->methods, mindex);

		if (!mono_method_get_token (m))
			continue;

		if (g_hash_table_lookup (all_methods, m))
			continue;
		g_hash_table_insert (all_methods, m, m);

		add_method (prof, m);
	}
	emit_record (prof, AOTPROF_RECORD_NONE, 0);

	fclose (outfile);

	g_hash_table_destroy (all_methods);
	g_hash_table_destroy (prof->classes);
	g_hash_table_destroy (prof->images);
	g_ptr_array_free (prof->methods, TRUE);
	g_free (prof->outfile_name);
}
