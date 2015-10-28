/*
 * driver.c: The new mono JIT compiler.
 *
 * Author:
 *   Paolo Molaro (lupus@ximian.com)
 *   Dietmar Maurer (dietmar@ximian.com)
 *
 * (C) 2002-2003 Ximian, Inc.
 * (C) 2003-2006 Novell, Inc.
 */

#include <config.h>
#ifdef HAVE_SIGNAL_H
#include <signal.h>
#endif
#if HAVE_SCHED_SETAFFINITY
#include <sched.h>
#endif
#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif

#include <mono/metadata/assembly.h>
#include <mono/metadata/loader.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/class.h>
#include <mono/metadata/object.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/opcodes.h>
#include <mono/metadata/mono-endian.h>
#include <mono/metadata/tokentype.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/threads.h>
#include <mono/metadata/marshal.h>
#include <mono/metadata/socket-io.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/io-layer/io-layer.h>
#include "mono/metadata/profiler.h"
#include <mono/metadata/profiler-private.h>
#include <mono/metadata/mono-config.h>
#include <mono/metadata/environment.h>
#include <mono/metadata/verify.h>
#include <mono/metadata/verify-internals.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/security-manager.h>
#include <mono/metadata/security-core-clr.h>
#include <mono/metadata/gc-internal.h>
#include <mono/metadata/coree.h>
#include <mono/metadata/attach.h>
#include "mono/utils/mono-counters.h"
#include "mono/utils/mono-hwcap.h"

#include "mini.h"
#include "jit.h"
#include <string.h>
#include <ctype.h>
#include <locale.h>
#include "version.h"
#include "debugger-agent.h"

static FILE *mini_stats_fd;

static void mini_usage (void);

#ifdef HOST_WIN32
/* Need this to determine whether to detach console */
#include <mono/metadata/cil-coff.h>
/* This turns off command line globbing under win32 */
int _CRT_glob = 0;
#endif

typedef void (*OptFunc) (const char *p);

#undef OPTFLAG
#ifdef HAVE_ARRAY_ELEM_INIT
#define MSGSTRFIELD(line) MSGSTRFIELD1(line)
#define MSGSTRFIELD1(line) str##line

static const struct msgstr_t {
#define OPTFLAG(id,shift,name,desc) char MSGSTRFIELD(__LINE__) [sizeof (name) + sizeof (desc)];
#include "optflags-def.h"
#undef OPTFLAG
} opstr = {
#define OPTFLAG(id,shift,name,desc) name "\0" desc,
#include "optflags-def.h"
#undef OPTFLAG
};
static const gint16 opt_names [] = {
#define OPTFLAG(id,shift,name,desc) [(shift)] = offsetof (struct msgstr_t, MSGSTRFIELD(__LINE__)),
#include "optflags-def.h"
#undef OPTFLAG
};

#define optflag_get_name(id) ((const char*)&opstr + opt_names [(id)])
#define optflag_get_desc(id) (optflag_get_name(id) + 1 + strlen (optflag_get_name(id)))

#else /* !HAVE_ARRAY_ELEM_INIT */
typedef struct {
	const char* name;
	const char* desc;
} OptName;

#define OPTFLAG(id,shift,name,desc) {name,desc},
static const OptName 
opt_names [] = {
#include "optflags-def.h"
	{NULL, NULL}
};
#define optflag_get_name(id) (opt_names [(id)].name)
#define optflag_get_desc(id) (opt_names [(id)].desc)

#endif

static const OptFunc
opt_funcs [sizeof (int) * 8] = {
	NULL
};

#ifdef __native_client_codegen__
extern gint8 nacl_align_byte;
#endif
#ifdef __native_client__
extern char *nacl_mono_path;
#endif

#define DEFAULT_OPTIMIZATIONS (	\
	MONO_OPT_PEEPHOLE |	\
	MONO_OPT_CFOLD |	\
	MONO_OPT_INLINE |       \
	MONO_OPT_CONSPROP |     \
	MONO_OPT_COPYPROP |     \
	MONO_OPT_DEADCE |       \
	MONO_OPT_BRANCH |	\
	MONO_OPT_LINEARS |	\
	MONO_OPT_INTRINS |  \
	MONO_OPT_LOOP |  \
	MONO_OPT_EXCEPTION |  \
    MONO_OPT_CMOV |  \
	MONO_OPT_GSHARED |	\
	MONO_OPT_SIMD |	\
	MONO_OPT_ALIAS_ANALYSIS	| \
	MONO_OPT_AOT)

#define EXCLUDED_FROM_ALL (MONO_OPT_SHARED | MONO_OPT_PRECOMP | MONO_OPT_UNSAFE | MONO_OPT_GSHAREDVT | MONO_OPT_FLOAT32)

static guint32
parse_optimizations (guint32 opt, const char* p)
{
	guint32 exclude = 0;
	const char *n;
	int i, invert, len;

	/* Initialize the hwcap module if necessary. */
	mono_hwcap_init ();

	/* call out to cpu detection code here that sets the defaults ... */
	opt |= mono_arch_cpu_optimizations (&exclude);
	opt &= ~exclude;
	if (!p)
		return opt;

	while (*p) {
		if (*p == '-') {
			p++;
			invert = TRUE;
		} else {
			invert = FALSE;
		}
		for (i = 0; i < G_N_ELEMENTS (opt_names) && optflag_get_name (i); ++i) {
			n = optflag_get_name (i);
			len = strlen (n);
			if (strncmp (p, n, len) == 0) {
				if (invert)
					opt &= ~ (1 << i);
				else
					opt |= 1 << i;
				p += len;
				if (*p == ',') {
					p++;
					break;
				} else if (*p == '=') {
					p++;
					if (opt_funcs [i])
						opt_funcs [i] (p);
					while (*p && *p++ != ',');
					break;
				}
				/* error out */
				break;
			}
		}
		if (i == G_N_ELEMENTS (opt_names) || !optflag_get_name (i)) {
			if (strncmp (p, "all", 3) == 0) {
				if (invert)
					opt = 0;
				else
					opt = ~(EXCLUDED_FROM_ALL | exclude);
				p += 3;
				if (*p == ',')
					p++;
			} else {
				fprintf (stderr, "Invalid optimization name `%s'\n", p);
				exit (1);
			}
		}
	}
	return opt;
}

static gboolean
parse_debug_options (const char* p)
{
	MonoDebugOptions *opt = mini_get_debug_options ();

	do {
		if (!*p) {
			fprintf (stderr, "Syntax error; expected debug option name\n");
			return FALSE;
		}

		if (!strncmp (p, "casts", 5)) {
			opt->better_cast_details = TRUE;
			p += 5;
		} else if (!strncmp (p, "mdb-optimizations", 17)) {
			opt->mdb_optimizations = TRUE;
			p += 17;
		} else if (!strncmp (p, "gdb", 3)) {
			opt->gdb = TRUE;
			p += 3;
		} else {
			fprintf (stderr, "Invalid debug option `%s', use --help-debug for details\n", p);
			return FALSE;
		}

		if (*p == ',') {
			p++;
			if (!*p) {
				fprintf (stderr, "Syntax error; expected debug option name\n");
				return FALSE;
			}
		}
	} while (*p);

	return TRUE;
}

typedef struct {
	const char name [6];
	const char desc [18];
	MonoGraphOptions value;
} GraphName;

static const GraphName 
graph_names [] = {
	{"cfg",      "Control Flow",                            MONO_GRAPH_CFG},
	{"dtree",    "Dominator Tree",                          MONO_GRAPH_DTREE},
	{"code",     "CFG showing code",                        MONO_GRAPH_CFG_CODE},
	{"ssa",      "CFG after SSA",                           MONO_GRAPH_CFG_SSA},
	{"optc",     "CFG after IR opts",                       MONO_GRAPH_CFG_OPTCODE}
};

static MonoGraphOptions
mono_parse_graph_options (const char* p)
{
	const char *n;
	int i, len;

	for (i = 0; i < G_N_ELEMENTS (graph_names); ++i) {
		n = graph_names [i].name;
		len = strlen (n);
		if (strncmp (p, n, len) == 0)
			return graph_names [i].value;
	}

	fprintf (stderr, "Invalid graph name provided: %s\n", p);
	exit (1);
}

int
mono_parse_default_optimizations (const char* p)
{
	guint32 opt;

	opt = parse_optimizations (DEFAULT_OPTIMIZATIONS, p);
	return opt;
}

static char*
opt_descr (guint32 flags) {
	GString *str = g_string_new ("");
	int i, need_comma;

	need_comma = 0;
	for (i = 0; i < G_N_ELEMENTS (opt_names); ++i) {
		if (flags & (1 << i) && optflag_get_name (i)) {
			if (need_comma)
				g_string_append_c (str, ',');
			g_string_append (str, optflag_get_name (i));
			need_comma = 1;
		}
	}
	return g_string_free (str, FALSE);
}

static const guint32
opt_sets [] = {
       0,
       MONO_OPT_PEEPHOLE,
       MONO_OPT_BRANCH,
       MONO_OPT_CFOLD,
       MONO_OPT_FCMOV,
       MONO_OPT_ALIAS_ANALYSIS,
#ifdef MONO_ARCH_SIMD_INTRINSICS
       MONO_OPT_SIMD,
       MONO_OPT_SSE2,
       MONO_OPT_SIMD | MONO_OPT_SSE2,
#endif
       MONO_OPT_BRANCH | MONO_OPT_PEEPHOLE | MONO_OPT_INTRINS,
       MONO_OPT_BRANCH | MONO_OPT_PEEPHOLE | MONO_OPT_INTRINS | MONO_OPT_ALIAS_ANALYSIS,
       MONO_OPT_BRANCH | MONO_OPT_PEEPHOLE | MONO_OPT_LINEARS,
       MONO_OPT_BRANCH | MONO_OPT_PEEPHOLE | MONO_OPT_LINEARS | MONO_OPT_COPYPROP,
       MONO_OPT_BRANCH | MONO_OPT_PEEPHOLE | MONO_OPT_LINEARS | MONO_OPT_CFOLD,
       MONO_OPT_BRANCH | MONO_OPT_PEEPHOLE | MONO_OPT_LINEARS | MONO_OPT_COPYPROP | MONO_OPT_CONSPROP | MONO_OPT_DEADCE,
       MONO_OPT_BRANCH | MONO_OPT_PEEPHOLE | MONO_OPT_LINEARS | MONO_OPT_COPYPROP | MONO_OPT_CONSPROP | MONO_OPT_DEADCE | MONO_OPT_ALIAS_ANALYSIS,
       MONO_OPT_BRANCH | MONO_OPT_PEEPHOLE | MONO_OPT_LINEARS | MONO_OPT_COPYPROP | MONO_OPT_CONSPROP | MONO_OPT_DEADCE | MONO_OPT_LOOP | MONO_OPT_INLINE | MONO_OPT_INTRINS,
       MONO_OPT_BRANCH | MONO_OPT_PEEPHOLE | MONO_OPT_LINEARS | MONO_OPT_COPYPROP | MONO_OPT_CONSPROP | MONO_OPT_DEADCE | MONO_OPT_LOOP | MONO_OPT_INLINE | MONO_OPT_INTRINS | MONO_OPT_TAILC,
       MONO_OPT_BRANCH | MONO_OPT_PEEPHOLE | MONO_OPT_LINEARS | MONO_OPT_COPYPROP | MONO_OPT_CONSPROP | MONO_OPT_DEADCE | MONO_OPT_LOOP | MONO_OPT_INLINE | MONO_OPT_INTRINS | MONO_OPT_SSA,
       MONO_OPT_BRANCH | MONO_OPT_PEEPHOLE | MONO_OPT_LINEARS | MONO_OPT_COPYPROP | MONO_OPT_CONSPROP | MONO_OPT_DEADCE | MONO_OPT_LOOP | MONO_OPT_INLINE | MONO_OPT_INTRINS | MONO_OPT_EXCEPTION,
       MONO_OPT_BRANCH | MONO_OPT_PEEPHOLE | MONO_OPT_LINEARS | MONO_OPT_COPYPROP | MONO_OPT_CONSPROP | MONO_OPT_DEADCE | MONO_OPT_LOOP | MONO_OPT_INLINE | MONO_OPT_INTRINS | MONO_OPT_EXCEPTION | MONO_OPT_CMOV,
       MONO_OPT_BRANCH | MONO_OPT_PEEPHOLE | MONO_OPT_LINEARS | MONO_OPT_COPYPROP | MONO_OPT_CONSPROP | MONO_OPT_DEADCE | MONO_OPT_LOOP | MONO_OPT_INLINE | MONO_OPT_INTRINS | MONO_OPT_EXCEPTION | MONO_OPT_ABCREM,
       MONO_OPT_BRANCH | MONO_OPT_PEEPHOLE | MONO_OPT_LINEARS | MONO_OPT_COPYPROP | MONO_OPT_CONSPROP | MONO_OPT_DEADCE | MONO_OPT_LOOP | MONO_OPT_INLINE | MONO_OPT_INTRINS | MONO_OPT_ABCREM,
       MONO_OPT_BRANCH | MONO_OPT_PEEPHOLE | MONO_OPT_LINEARS | MONO_OPT_COPYPROP | MONO_OPT_CONSPROP | MONO_OPT_DEADCE | MONO_OPT_LOOP | MONO_OPT_INLINE | MONO_OPT_INTRINS | MONO_OPT_ABCREM | MONO_OPT_SHARED,
       DEFAULT_OPTIMIZATIONS, 
};

typedef int (*TestMethod) (void);

#if 0
static void
domain_dump_native_code (MonoDomain *domain) {
	// need to poke into the domain, move to metadata/domain.c
	// need to empty jit_info_table and code_mp
}
#endif

static void
mini_regression_step (MonoImage *image, int verbose, int *total_run, int *total,
		guint32 opt_flags,
		GTimer *timer, MonoDomain *domain)
{
	int result, expected, failed, cfailed, run, code_size;
	TestMethod func;
	double elapsed, comp_time, start_time;
	char *n;
	int i;

	mono_set_defaults (verbose, opt_flags);
	n = opt_descr (opt_flags);
	g_print ("Test run: image=%s, opts=%s\n", mono_image_get_filename (image), n);
	g_free (n);
	cfailed = failed = run = code_size = 0;
	comp_time = elapsed = 0.0;

	/* fixme: ugly hack - delete all previously compiled methods */
	if (domain_jit_info (domain)) {
		g_hash_table_destroy (domain_jit_info (domain)->jit_trampoline_hash);
		domain_jit_info (domain)->jit_trampoline_hash = g_hash_table_new (mono_aligned_addr_hash, NULL);
		mono_internal_hash_table_destroy (&(domain->jit_code_hash));
		mono_jit_code_hash_init (&(domain->jit_code_hash));
	}

	g_timer_start (timer);
	if (mini_stats_fd)
		fprintf (mini_stats_fd, "[");
	for (i = 0; i < mono_image_get_table_rows (image, MONO_TABLE_METHOD); ++i) {
		MonoMethod *method = mono_get_method (image, MONO_TOKEN_METHOD_DEF | (i + 1), NULL);
		if (!method)
			continue;
		if (strncmp (method->name, "test_", 5) == 0) {
			MonoCompile *cfg;

			expected = atoi (method->name + 5);
			run++;
			start_time = g_timer_elapsed (timer, NULL);
			comp_time -= start_time;
			cfg = mini_method_compile (method, mono_get_optimizations_for_method (method, opt_flags), mono_get_root_domain (), JIT_FLAG_RUN_CCTORS, 0, -1);
			comp_time += g_timer_elapsed (timer, NULL);
			if (cfg->exception_type == MONO_EXCEPTION_NONE) {
				if (verbose >= 2)
					g_print ("Running '%s' ...\n", method->name);
#ifdef MONO_USE_AOT_COMPILER
				if ((func = mono_aot_get_method (mono_get_root_domain (), method)))
					;
				else
#endif
					func = (TestMethod)(gpointer)cfg->native_code;
				func = (TestMethod)mono_create_ftnptr (mono_get_root_domain (), func);
				result = func ();
				if (result != expected) {
					failed++;
					g_print ("Test '%s' failed result (got %d, expected %d).\n", method->name, result, expected);
				}
				code_size += cfg->code_len;
				mono_destroy_compile (cfg);

			} else {
				cfailed++;
				g_print ("Test '%s' failed compilation.\n", method->name);
			}
			if (mini_stats_fd)
				fprintf (mini_stats_fd, "%f, ",
						g_timer_elapsed (timer, NULL) - start_time);
		}
	}
	if (mini_stats_fd)
		fprintf (mini_stats_fd, "],\n");
	g_timer_stop (timer);
	elapsed = g_timer_elapsed (timer, NULL);
	if (failed > 0 || cfailed > 0){
		g_print ("Results: total tests: %d, failed: %d, cfailed: %d (pass: %.2f%%)\n",
				run, failed, cfailed, 100.0*(run-failed-cfailed)/run);
	} else {
		g_print ("Results: total tests: %d, all pass \n",  run);
	}

	g_print ("Elapsed time: %f secs (%f, %f), Code size: %d\n\n", elapsed,
			elapsed - comp_time, comp_time, code_size);
	*total += failed + cfailed;
	*total_run += run;
}

static int
mini_regression (MonoImage *image, int verbose, int *total_run)
{
	guint32 i, opt;
	MonoMethod *method;
	char *n;
	GTimer *timer = g_timer_new ();
	MonoDomain *domain = mono_domain_get ();
	guint32 exclude = 0;
	int total;

	/* Note: mono_hwcap_init () called in mono_init () before we get here. */
	mono_arch_cpu_optimizations (&exclude);

	if (mini_stats_fd) {
		fprintf (mini_stats_fd, "$stattitle = \'Mono Benchmark Results (various optimizations)\';\n");

		fprintf (mini_stats_fd, "$graph->set_legend(qw(");
		for (opt = 0; opt < G_N_ELEMENTS (opt_sets); opt++) {
			guint32 opt_flags = opt_sets [opt];
			n = opt_descr (opt_flags);
			if (!n [0])
				n = (char *)"none";
			if (opt)
				fprintf (mini_stats_fd, " ");
			fprintf (mini_stats_fd, "%s", n);
		

		}
		fprintf (mini_stats_fd, "));\n");

		fprintf (mini_stats_fd, "@data = (\n");
		fprintf (mini_stats_fd, "[");
	}

	/* load the metadata */
	for (i = 0; i < mono_image_get_table_rows (image, MONO_TABLE_METHOD); ++i) {
		method = mono_get_method (image, MONO_TOKEN_METHOD_DEF | (i + 1), NULL);
		if (!method)
			continue;
		mono_class_init (method->klass);

		if (!strncmp (method->name, "test_", 5) && mini_stats_fd) {
			fprintf (mini_stats_fd, "\"%s\",", method->name);
		}
	}
	if (mini_stats_fd)
		fprintf (mini_stats_fd, "],\n");


	total = 0;
	*total_run = 0;
	if (mono_do_single_method_regression) {
		GSList *iter;

		mini_regression_step (image, verbose, total_run, &total,
				0,
				timer, domain);
		if (total)
			return total;
		g_print ("Single method regression: %d methods\n", g_slist_length (mono_single_method_list));

		for (iter = mono_single_method_list; iter; iter = g_slist_next (iter)) {
			char *method_name;

			mono_current_single_method = iter->data;

			method_name = mono_method_full_name (mono_current_single_method, TRUE);
			g_print ("Current single method: %s\n", method_name);
			g_free (method_name);

			mini_regression_step (image, verbose, total_run, &total,
					0,
					timer, domain);
			if (total)
				return total;
		}
	} else {
		for (opt = 0; opt < G_N_ELEMENTS (opt_sets); ++opt) {
			mini_regression_step (image, verbose, total_run, &total,
					opt_sets [opt] & ~exclude,
					timer, domain);
		}
	}

	if (mini_stats_fd) {
		fprintf (mini_stats_fd, ");\n");
		fflush (mini_stats_fd);
	}

	g_timer_destroy (timer);
	return total;
}

static int
mini_regression_list (int verbose, int count, char *images [])
{
	int i, total, total_run, run;
	MonoAssembly *ass;
	
	total_run =  total = 0;
	for (i = 0; i < count; ++i) {
		ass = mono_assembly_open (images [i], NULL);
		if (!ass) {
			g_warning ("failed to load assembly: %s", images [i]);
			continue;
		}
		total += mini_regression (mono_assembly_get_image (ass), verbose, &run);
		total_run += run;
	}
	if (total > 0){
		g_print ("Overall results: tests: %d, failed: %d, opt combinations: %d (pass: %.2f%%)\n", 
			 total_run, total, (int)G_N_ELEMENTS (opt_sets), 100.0*(total_run-total)/total_run);
	} else {
		g_print ("Overall results: tests: %d, 100%% pass, opt combinations: %d\n", 
			 total_run, (int)G_N_ELEMENTS (opt_sets));
	}
	
	return total;
}

#ifdef MONO_JIT_INFO_TABLE_TEST
typedef struct _JitInfoData
{
	guint start;
	guint length;
	MonoJitInfo *ji;
	struct _JitInfoData *next;
} JitInfoData;

typedef struct
{
	guint start;
	guint length;
	int num_datas;
	JitInfoData *data;
} Region;

typedef struct
{
	int num_datas;
	int num_regions;
	Region *regions;
	int num_frees;
	JitInfoData *frees;
} ThreadData;

static int num_threads;
static ThreadData *thread_datas;
static MonoDomain *test_domain;

static JitInfoData*
alloc_random_data (Region *region)
{
	JitInfoData **data;
	JitInfoData *prev;
	guint prev_end;
	guint next_start;
	guint max_len;
	JitInfoData *d;
	int num_retries = 0;
	int pos, i;

 restart:
	prev = NULL;
	data = &region->data;
	pos = random () % (region->num_datas + 1);
	i = 0;
	while (*data != NULL) {
		if (i++ == pos)
			break;
		prev = *data;
		data = &(*data)->next;
	}

	if (prev == NULL)
		g_assert (*data == region->data);
	else
		g_assert (prev->next == *data);

	if (prev == NULL)
		prev_end = region->start;
	else
		prev_end = prev->start + prev->length;

	if (*data == NULL)
		next_start = region->start + region->length;
	else
		next_start = (*data)->start;

	g_assert (prev_end <= next_start);

	max_len = next_start - prev_end;
	if (max_len < 128) {
		if (++num_retries >= 10)
			return NULL;
		goto restart;
	}
	if (max_len > 1024)
		max_len = 1024;

	d = g_new0 (JitInfoData, 1);
	d->start = prev_end + random () % (max_len / 2);
	d->length = random () % MIN (max_len, next_start - d->start) + 1;

	g_assert (d->start >= prev_end && d->start + d->length <= next_start);

	d->ji = g_new0 (MonoJitInfo, 1);
	d->ji->d.method = (MonoMethod*) 0xABadBabe;
	d->ji->code_start = (gpointer)(gulong) d->start;
	d->ji->code_size = d->length;
	d->ji->cas_inited = 1;	/* marks an allocated jit info */

	d->next = *data;
	*data = d;

	++region->num_datas;

	return d;
}

static JitInfoData**
choose_random_data (Region *region)
{
	int n;
	int i;
	JitInfoData **d;

	g_assert (region->num_datas > 0);

	n = random () % region->num_datas;

	for (d = &region->data, i = 0;
	     i < n;
	     d = &(*d)->next, ++i)
		;

	return d;
}

static Region*
choose_random_region (ThreadData *td)
{
	return &td->regions [random () % td->num_regions];
}

static ThreadData*
choose_random_thread (void)
{
	return &thread_datas [random () % num_threads];
}

static void
free_jit_info_data (ThreadData *td, JitInfoData *free)
{
	free->next = td->frees;
	td->frees = free;

	if (++td->num_frees >= 1000) {
		int i;

		for (i = 0; i < 500; ++i)
			free = free->next;

		while (free->next != NULL) {
			JitInfoData *next = free->next->next;

			//g_free (free->next->ji);
			g_free (free->next);
			free->next = next;

			--td->num_frees;
		}
	}
}

#define NUM_THREADS		8
#define REGIONS_PER_THREAD	10
#define REGION_SIZE		0x10000

#define MAX_ADDR		(REGION_SIZE * REGIONS_PER_THREAD * NUM_THREADS)

#define MODE_ALLOC	1
#define MODE_FREE	2

static void
test_thread_func (ThreadData *td)
{
	int mode = MODE_ALLOC;
	int i = 0;
	gulong lookup_successes = 0, lookup_failures = 0;
	MonoDomain *domain = test_domain;
	int thread_num = (int)(td - thread_datas);
	gboolean modify_thread = thread_num < NUM_THREADS / 2; /* only half of the threads modify the table */

	for (;;) {
		int alloc;
		int lookup = 1;

		if (td->num_datas == 0) {
			lookup = 0;
			alloc = 1;
		} else if (modify_thread && random () % 1000 < 5) {
			lookup = 0;
			if (mode == MODE_ALLOC)
				alloc = (random () % 100) < 70;
			else if (mode == MODE_FREE)
				alloc = (random () % 100) < 30;
		}

		if (lookup) {
			/* modify threads sometimes look up their own jit infos */
			if (modify_thread && random () % 10 < 5) {
				Region *region = choose_random_region (td);

				if (region->num_datas > 0) {
					JitInfoData **data = choose_random_data (region);
					guint pos = (*data)->start + random () % (*data)->length;
					MonoJitInfo *ji;

					ji = mono_jit_info_table_find (domain, (char*)(gulong) pos);

					g_assert (ji->cas_inited);
					g_assert ((*data)->ji == ji);
				}
			} else {
				int pos = random () % MAX_ADDR;
				char *addr = (char*)(gulong) pos;
				MonoJitInfo *ji;

				ji = mono_jit_info_table_find (domain, addr);

				/*
				 * FIXME: We are actually not allowed
				 * to do this.  By the time we examine
				 * the ji another thread might already
				 * have removed it.
				 */
				if (ji != NULL) {
					g_assert (addr >= (char*)ji->code_start && addr < (char*)ji->code_start + ji->code_size);
					++lookup_successes;
				} else
					++lookup_failures;
			}
		} else if (alloc) {
			JitInfoData *data = alloc_random_data (choose_random_region (td));

			if (data != NULL) {
				mono_jit_info_table_add (domain, data->ji);

				++td->num_datas;
			}
		} else {
			Region *region = choose_random_region (td);

			if (region->num_datas > 0) {
				JitInfoData **data = choose_random_data (region);
				JitInfoData *free;

				mono_jit_info_table_remove (domain, (*data)->ji);

				//(*data)->ji->cas_inited = 0; /* marks a free jit info */

				free = *data;
				*data = (*data)->next;

				free_jit_info_data (td, free);

				--region->num_datas;
				--td->num_datas;
			}
		}

		if (++i % 100000 == 0) {
			int j;
			g_print ("num datas %d (%ld - %ld): %d", (int)(td - thread_datas),
				 lookup_successes, lookup_failures, td->num_datas);
			for (j = 0; j < td->num_regions; ++j)
				g_print ("  %d", td->regions [j].num_datas);
			g_print ("\n");
		}

		if (td->num_datas < 100)
			mode = MODE_ALLOC;
		else if (td->num_datas > 2000)
			mode = MODE_FREE;
	}
}

/*
static void
small_id_thread_func (gpointer arg)
{
	MonoThread *thread = mono_thread_current ();
	MonoThreadHazardPointers *hp = mono_hazard_pointer_get ();

	g_print ("my small id is %d\n", (int)thread->small_id);
	mono_hazard_pointer_clear (hp, 1);
	sleep (3);
	g_print ("done %d\n", (int)thread->small_id);
}
*/

static void
jit_info_table_test (MonoDomain *domain)
{
	int i;

	g_print ("testing jit_info_table\n");

	num_threads = NUM_THREADS;
	thread_datas = g_new0 (ThreadData, num_threads);

	for (i = 0; i < num_threads; ++i) {
		int j;

		thread_datas [i].num_regions = REGIONS_PER_THREAD;
		thread_datas [i].regions = g_new0 (Region, REGIONS_PER_THREAD);

		for (j = 0; j < REGIONS_PER_THREAD; ++j) {
			thread_datas [i].regions [j].start = (num_threads * j + i) * REGION_SIZE;
			thread_datas [i].regions [j].length = REGION_SIZE;
		}
	}

	test_domain = domain;

	/*
	for (i = 0; i < 72; ++i)
		mono_thread_create (domain, small_id_thread_func, NULL);

	sleep (2);
	*/

	for (i = 0; i < num_threads; ++i)
		mono_thread_create (domain, test_thread_func, &thread_datas [i]);
}
#endif

enum {
	DO_BENCH,
	DO_REGRESSION,
	DO_SINGLE_METHOD_REGRESSION,
	DO_COMPILE,
	DO_EXEC,
	DO_DRAW,
	DO_DEBUGGER
};

typedef struct CompileAllThreadArgs {
	MonoAssembly *ass;
	int verbose;
	guint32 opts;
	guint32 recompilation_times;
} CompileAllThreadArgs;

static void
compile_all_methods_thread_main_inner (CompileAllThreadArgs *args)
{
	MonoAssembly *ass = args->ass;
	int verbose = args->verbose;
	MonoImage *image = mono_assembly_get_image (ass);
	MonoMethod *method;
	MonoCompile *cfg;
	int i, count = 0, fail_count = 0;

	for (i = 0; i < mono_image_get_table_rows (image, MONO_TABLE_METHOD); ++i) {
		guint32 token = MONO_TOKEN_METHOD_DEF | (i + 1);
		MonoMethodSignature *sig;

		if (mono_metadata_has_generic_params (image, token))
			continue;

		method = mono_get_method (image, token, NULL);
		if (!method)
			continue;
		if ((method->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL) ||
		    (method->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL) ||
		    (method->iflags & METHOD_IMPL_ATTRIBUTE_RUNTIME) ||
		    (method->flags & METHOD_ATTRIBUTE_ABSTRACT))
			continue;

		if (method->klass->generic_container)
			continue;
		sig = mono_method_signature (method);
		if (!sig) {
			char * desc = mono_method_full_name (method, TRUE);
			g_print ("Could not retrieve method signature for %s\n", desc);
			g_free (desc);
			fail_count ++;
			continue;
		}

		if (sig->has_type_parameters)
			continue;

		count++;
		if (verbose) {
			char * desc = mono_method_full_name (method, TRUE);
			g_print ("Compiling %d %s\n", count, desc);
			g_free (desc);
		}
		cfg = mini_method_compile (method, mono_get_optimizations_for_method (method, args->opts), mono_get_root_domain (), 0, 0, -1);
		if (cfg->exception_type != MONO_EXCEPTION_NONE) {
			printf ("Compilation of %s failed with exception '%s':\n", mono_method_full_name (cfg->method, TRUE), cfg->exception_message);
			fail_count ++;
		}
		mono_destroy_compile (cfg);
	}

	if (fail_count)
		exit (1);
}

static void
compile_all_methods_thread_main (CompileAllThreadArgs *args)
{
	guint32 i;
	for (i = 0; i < args->recompilation_times; ++i)
		compile_all_methods_thread_main_inner (args);
}

static void
compile_all_methods (MonoAssembly *ass, int verbose, guint32 opts, guint32 recompilation_times)
{
	CompileAllThreadArgs args;

	args.ass = ass;
	args.verbose = verbose;
	args.opts = opts;
	args.recompilation_times = recompilation_times;

	/* 
	 * Need to create a mono thread since compilation might trigger
	 * running of managed code.
	 */
	mono_thread_create (mono_domain_get (), compile_all_methods_thread_main, &args);

	mono_thread_manage ();
}

/**
 * mono_jit_exec:
 * @assembly: reference to an assembly
 * @argc: argument count
 * @argv: argument vector
 *
 * Start execution of a program.
 */
int 
mono_jit_exec (MonoDomain *domain, MonoAssembly *assembly, int argc, char *argv[])
{
	MonoImage *image = mono_assembly_get_image (assembly);
	MonoMethod *method;
	guint32 entry = mono_image_get_entry_point (image);

	if (!entry) {
		g_print ("Assembly '%s' doesn't have an entry point.\n", mono_image_get_filename (image));
		/* FIXME: remove this silly requirement. */
		mono_environment_exitcode_set (1);
		return 1;
	}

	method = mono_get_method (image, entry, NULL);
	if (method == NULL){
		g_print ("The entry point method could not be loaded\n");
		mono_environment_exitcode_set (1);
		return 1;
	}
	
	return mono_runtime_run_main (method, argc, argv, NULL);
}

typedef struct 
{
	MonoDomain *domain;
	const char *file;
	int argc;
	char **argv;
	guint32 opts;
	char *aot_options;
} MainThreadArgs;

static void main_thread_handler (gpointer user_data)
{
	MainThreadArgs *main_args = user_data;
	MonoAssembly *assembly;

	if (mono_compile_aot) {
		int i, res;

		/* Treat the other arguments as assemblies to compile too */
		for (i = 0; i < main_args->argc; ++i) {
			assembly = mono_domain_assembly_open (main_args->domain, main_args->argv [i]);
			if (!assembly) {
				fprintf (stderr, "Can not open image %s\n", main_args->argv [i]);
				exit (1);
			}
			/* Check that the assembly loaded matches the filename */
			{
				MonoImageOpenStatus status;
				MonoImage *img;

				img = mono_image_open (main_args->argv [i], &status);
				if (img && strcmp (img->name, assembly->image->name)) {
					fprintf (stderr, "Error: Loaded assembly '%s' doesn't match original file name '%s'. Set MONO_PATH to the assembly's location.\n", assembly->image->name, img->name);
					exit (1);
				}
			}
			res = mono_compile_assembly (assembly, main_args->opts, main_args->aot_options);
			if (res != 0) {
				fprintf (stderr, "AOT of image %s failed.\n", main_args->argv [i]);
				exit (1);
			}
		}
	} else {
		assembly = mono_domain_assembly_open (main_args->domain, main_args->file);
		if (!assembly){
			fprintf (stderr, "Can not open image %s\n", main_args->file);
			exit (1);
		}

		/* 
		 * This must be done in a thread managed by mono since it can invoke
		 * managed code.
		 */
		if (main_args->opts & MONO_OPT_PRECOMP)
			mono_precompile_assemblies ();

		mono_jit_exec (main_args->domain, assembly, main_args->argc, main_args->argv);
	}
}

static int
load_agent (MonoDomain *domain, char *desc)
{
	char* col = strchr (desc, ':');	
	char *agent, *args;
	MonoAssembly *agent_assembly;
	MonoImage *image;
	MonoMethod *method;
	guint32 entry;
	MonoArray *main_args;
	gpointer pa [1];
	MonoImageOpenStatus open_status;

	if (col) {
		agent = g_memdup (desc, col - desc + 1);
		agent [col - desc] = '\0';
		args = col + 1;
	} else {
		agent = g_strdup (desc);
		args = NULL;
	}

	agent_assembly = mono_assembly_open (agent, &open_status);
	if (!agent_assembly) {
		fprintf (stderr, "Cannot open agent assembly '%s': %s.\n", agent, mono_image_strerror (open_status));
		g_free (agent);
		return 2;
	}

	/* 
	 * Can't use mono_jit_exec (), as it sets things which might confuse the
	 * real Main method.
	 */
	image = mono_assembly_get_image (agent_assembly);
	entry = mono_image_get_entry_point (image);
	if (!entry) {
		g_print ("Assembly '%s' doesn't have an entry point.\n", mono_image_get_filename (image));
		g_free (agent);
		return 1;
	}

	method = mono_get_method (image, entry, NULL);
	if (method == NULL){
		g_print ("The entry point method of assembly '%s' could not be loaded\n", agent);
		g_free (agent);
		return 1;
	}
	
	mono_thread_set_main (mono_thread_current ());

	if (args) {
		main_args = (MonoArray*)mono_array_new (domain, mono_defaults.string_class, 1);
		mono_array_set (main_args, MonoString*, 0, mono_string_new (domain, args));
	} else {
		main_args = (MonoArray*)mono_array_new (domain, mono_defaults.string_class, 0);
	}

	g_free (agent);

	pa [0] = main_args;
	/* Pass NULL as 'exc' so unhandled exceptions abort the runtime */
	mono_runtime_invoke (method, NULL, pa, NULL);

	return 0;
}

static void
mini_usage_jitdeveloper (void)
{
	int i;
	
	fprintf (stdout,
		 "Runtime and JIT debugging options:\n"
		 "    --breakonex            Inserts a breakpoint on exceptions\n"
		 "    --break METHOD         Inserts a breakpoint at METHOD entry\n"
		 "    --break-at-bb METHOD N Inserts a breakpoint in METHOD at BB N\n"
		 "    --compile METHOD       Just compile METHOD in assembly\n"
		 "    --compile-all=N        Compiles all the methods in the assembly multiple times (default: 1)\n"
		 "    --ncompile N           Number of times to compile METHOD (default: 1)\n"
		 "    --print-vtable         Print the vtable of all used classes\n"
		 "    --regression           Runs the regression test contained in the assembly\n"
		 "    --single-method=OPTS   Runs regressions with only one method optimized with OPTS at any time\n"
		 "    --statfile FILE        Sets the stat file to FILE\n"
		 "    --stats                Print statistics about the JIT operations\n"
		 "    --wapi=hps|semdel|seminfo IO-layer maintenance\n"
		 "    --inject-async-exc METHOD OFFSET Inject an asynchronous exception at METHOD\n"
		 "    --verify-all           Run the verifier on all assemblies and methods\n"
		 "    --full-aot             Avoid JITting any code\n"
		 "    --llvmonly             Use LLVM compiled code only\n"
		 "    --agent=ASSEMBLY[:ARG] Loads the specific agent assembly and executes its Main method with the given argument before loading the main assembly.\n"
		 "    --no-x86-stack-align   Don't align stack on x86\n"
		 "\n"
		 "Other options:\n" 
		 "    --graph[=TYPE] METHOD  Draws a graph of the specified method:\n");
	
	for (i = 0; i < G_N_ELEMENTS (graph_names); ++i) {
		fprintf (stdout, "                           %-10s %s\n", graph_names [i].name, graph_names [i].desc);
	}
}

static void
mini_usage_list_opt (void)
{
	int i;
	
	for (i = 0; i < G_N_ELEMENTS (opt_names); ++i)
		fprintf (stdout, "                           %-10s %s\n", optflag_get_name (i), optflag_get_desc (i));
}

static void
mini_usage (void)
{
	fprintf (stdout,
		"Usage is: mono [options] program [program-options]\n"
		"\n"
		"Development:\n"
		"    --aot[=<options>]      Compiles the assembly to native code\n"
		"    --debug[=<options>]    Enable debugging support, use --help-debug for details\n"
 		"    --debugger-agent=options Enable the debugger agent\n"
		"    --profile[=profiler]   Runs in profiling mode with the specified profiler module\n"
		"    --trace[=EXPR]         Enable tracing, use --help-trace for details\n"
		"    --jitmap               Output a jit method map to /tmp/perf-PID.map\n"
		"    --help-devel           Shows more options available to developers\n"
#ifdef __native_client_codegen__
		"    --nacl-align-mask-off  Turn off Native Client 32-byte alignment mask (for debug only)\n"
#endif
		"\n"
		"Runtime:\n"
		"    --config FILE          Loads FILE as the Mono config\n"
		"    --verbose, -v          Increases the verbosity level\n"
		"    --help, -h             Show usage information\n"
		"    --version, -V          Show version information\n"
		"    --runtime=VERSION      Use the VERSION runtime, instead of autodetecting\n"
		"    --optimize=OPT         Turns on or off a specific optimization\n"
		"                           Use --list-opt to get a list of optimizations\n"
#ifndef DISABLE_SECURITY
		"    --security[=mode]      Turns on the unsupported security manager (off by default)\n"
		"                           mode is one of cas, core-clr, verifiable or validil\n"
#endif
		"    --attach=OPTIONS       Pass OPTIONS to the attach agent in the runtime.\n"
		"                           Currently the only supported option is 'disable'.\n"
		"    --llvm, --nollvm       Controls whenever the runtime uses LLVM to compile code.\n"
	        "    --gc=[sgen,boehm]      Select SGen or Boehm GC (runs mono or mono-sgen)\n"
#ifdef TARGET_OSX
		"    --arch=[32,64]         Select architecture (runs mono32 or mono64)\n"
#endif
#ifdef HOST_WIN32
	        "    --mixed-mode           Enable mixed-mode image support.\n"
#endif
	  );
}

static void
mini_trace_usage (void)
{
	fprintf (stdout,
		 "Tracing options:\n"
		 "   --trace[=EXPR]        Trace every call, optional EXPR controls the scope\n"
		 "\n"
		 "EXPR is composed of:\n"
		 "    all                  All assemblies\n"
		 "    none                 No assemblies\n"
		 "    program              Entry point assembly\n"
		 "    assembly             Specifies an assembly\n"
		 "    wrapper              All wrappers bridging native and managed code\n"
		 "    M:Type:Method        Specifies a method\n"
		 "    N:Namespace          Specifies a namespace\n"
		 "    T:Type               Specifies a type\n"
		 "    E:Type               Specifies stack traces for an exception type\n"
		 "    EXPR                 Includes expression\n"
		 "    -EXPR                Excludes expression\n"
		 "    EXPR,EXPR            Multiple expressions\n"
		 "    disabled             Don't print any output until toggled via SIGUSR2\n");
}

static void
mini_debug_usage (void)
{
	fprintf (stdout,
		 "Debugging options:\n"
		 "   --debug[=OPTIONS]     Enable debugging support, optional OPTIONS is a comma\n"
		 "                         separated list of options\n"
		 "\n"
		 "OPTIONS is composed of:\n"
		 "    casts                Enable more detailed InvalidCastException messages.\n"
		 "    mdb-optimizations    Disable some JIT optimizations which are normally\n"
		 "                         disabled when running inside the debugger.\n"
		 "                         This is useful if you plan to attach to the running\n"
		 "                         process with the debugger.\n");
}

#if defined(MONO_ARCH_ARCHITECTURE)
/* Redefine ARCHITECTURE to include more information */
#undef ARCHITECTURE
#define ARCHITECTURE MONO_ARCH_ARCHITECTURE
#endif

static const char info[] =
#ifdef HAVE_KW_THREAD
	"\tTLS:           __thread\n"
#else
	"\tTLS:           normal\n"
#endif /* HAVE_KW_THREAD */
#ifdef MONO_ARCH_SIGSEGV_ON_ALTSTACK
    "\tSIGSEGV:       altstack\n"
#else
    "\tSIGSEGV:       normal\n"
#endif
#ifdef HAVE_EPOLL
    "\tNotifications: epoll\n"
#elif defined(HAVE_KQUEUE)
    "\tNotification:  kqueue\n"
#else
    "\tNotification:  Thread + polling\n"
#endif
        "\tArchitecture:  " ARCHITECTURE "\n"
	"\tDisabled:      " DISABLED_FEATURES "\n"
	"\tMisc:          "
#ifdef MONO_SMALL_CONFIG
	"smallconfig "
#endif
#ifdef MONO_BIG_ARRAYS
	"bigarrays "
#endif
#if defined(MONO_ARCH_SOFT_DEBUG_SUPPORTED) && !defined(DISABLE_SOFT_DEBUG)
	"softdebug "
#endif
		"\n"
#ifdef MONO_ARCH_LLVM_SUPPORTED
#ifdef ENABLE_LLVM
	"\tLLVM:          yes(" LLVM_VERSION ")\n"
#else
	"\tLLVM:          supported, not enabled.\n"
#endif
#endif
	"";

#ifndef MONO_ARCH_AOT_SUPPORTED
#define error_if_aot_unsupported() do {fprintf (stderr, "AOT compilation is not supported on this platform.\n"); exit (1);} while (0)
#else
#define error_if_aot_unsupported()
#endif

#ifdef HOST_WIN32
BOOL APIENTRY DllMain (HMODULE module_handle, DWORD reason, LPVOID reserved)
{
	if (!mono_gc_dllmain (module_handle, reason, reserved))
		return FALSE;

	switch (reason)
	{
	case DLL_PROCESS_ATTACH:
		mono_install_runtime_load (mini_init);
		break;
	case DLL_PROCESS_DETACH:
		if (coree_module_handle)
			FreeLibrary (coree_module_handle);
		break;
	case DLL_THREAD_DETACH:
		mono_thread_info_detach ();
		break;
	
	}
	return TRUE;
}
#endif

static gboolean enable_debugging;

/*
 * mono_jit_parse_options:
 *
 *   Process the command line options in ARGV as done by the runtime executable. 
 * This should be called before mono_jit_init ().
 */
void
mono_jit_parse_options (int argc, char * argv[])
{
	int i;
	char *trace_options = NULL;
	int mini_verbose = 0;
	guint32 opt;

	/* 
	 * Some options have no effect here, since they influence the behavior of 
	 * mono_main ().
	 */

	opt = mono_parse_default_optimizations (NULL);

	/* FIXME: Avoid code duplication */
	for (i = 0; i < argc; ++i) {
		if (argv [i] [0] != '-')
			break;
 		if (strncmp (argv [i], "--debugger-agent=", 17) == 0) {
			MonoDebugOptions *opt = mini_get_debug_options ();

 			mono_debugger_agent_parse_options (argv [i] + 17);
			opt->mdb_optimizations = TRUE;
			enable_debugging = TRUE;
		} else if (!strcmp (argv [i], "--soft-breakpoints")) {
			MonoDebugOptions *opt = mini_get_debug_options ();

			opt->soft_breakpoints = TRUE;
			opt->explicit_null_checks = TRUE;
		} else if (strncmp (argv [i], "--optimize=", 11) == 0) {
			opt = parse_optimizations (opt, argv [i] + 11);
			mono_set_optimizations (opt);
		} else if (strncmp (argv [i], "-O=", 3) == 0) {
			opt = parse_optimizations (opt, argv [i] + 3);
			mono_set_optimizations (opt);
		} else if (strcmp (argv [i], "--trace") == 0) {
			trace_options = (char*)"";
		} else if (strncmp (argv [i], "--trace=", 8) == 0) {
			trace_options = &argv [i][8];
		} else if (strcmp (argv [i], "--verbose") == 0 || strcmp (argv [i], "-v") == 0) {
			mini_verbose++;
		} else if (strcmp (argv [i], "--breakonex") == 0) {
			MonoDebugOptions *opt = mini_get_debug_options ();

			opt->break_on_exc = TRUE;
		} else if (strcmp (argv [i], "--stats") == 0) {
			mono_counters_enable (-1);
			mono_stats.enabled = TRUE;
			mono_jit_stats.enabled = TRUE;
		} else if (strcmp (argv [i], "--break") == 0) {
			if (i+1 >= argc){
				fprintf (stderr, "Missing method name in --break command line option\n");
				exit (1);
			}
			
			if (!mono_debugger_insert_breakpoint (argv [++i], FALSE))
				fprintf (stderr, "Error: invalid method name '%s'\n", argv [i]);
		} else if (strcmp (argv [i], "--llvm") == 0) {
#ifndef MONO_ARCH_LLVM_SUPPORTED
			fprintf (stderr, "Mono Warning: --llvm not supported on this platform.\n");
#elif !defined(ENABLE_LLVM)
			fprintf (stderr, "Mono Warning: --llvm not enabled in this runtime.\n");
#else
			mono_use_llvm = TRUE;
#endif
		} else {
			fprintf (stderr, "Unsupported command line option: '%s'\n", argv [i]);
			exit (1);
		}
	}

	if (trace_options != NULL) {
		/* 
		 * Need to call this before mini_init () so we can trace methods 
		 * compiled there too.
		 */
		mono_jit_trace_calls = mono_trace_parse_options (trace_options);
		if (mono_jit_trace_calls == NULL)
			exit (1);
	}

	if (mini_verbose)
		mono_set_verbose_level (mini_verbose);
}

static void
mono_set_use_smp (int use_smp)
{
#if HAVE_SCHED_SETAFFINITY
	if (!use_smp) {
		unsigned long proc_mask = 1;
#ifdef GLIBC_BEFORE_2_3_4_SCHED_SETAFFINITY
		sched_setaffinity (getpid(), (gpointer)&proc_mask);
#else
		sched_setaffinity (getpid(), sizeof (unsigned long), (gpointer)&proc_mask);
#endif
	}
#endif
}

static void
switch_gc (char* argv[], const char* target_gc)
{
	GString *path;

	if (!strcmp (mono_gc_get_gc_name (), target_gc)) {
		return;
	}

	path = g_string_new (argv [0]);

	/*Running mono without any argument*/
	if (strstr (argv [0], "-sgen"))
		g_string_truncate (path, path->len - 5);
	else if (strstr (argv [0], "-boehm"))
		g_string_truncate (path, path->len - 6);

	g_string_append_c (path, '-');
	g_string_append (path, target_gc);

#ifdef HAVE_EXECVP
	execvp (path->str, argv);
#else
	fprintf (stderr, "Error: --gc=<NAME> option not supported on this platform.\n");
#endif
}

#ifdef TARGET_OSX

static void
switch_arch (char* argv[], const char* target_arch)
{
	GString *path;
	gsize arch_offset;

	if ((strcmp (target_arch, "32") == 0 && strcmp (ARCHITECTURE, "x86") == 0) ||
		(strcmp (target_arch, "64") == 0 && strcmp (ARCHITECTURE, "amd64") == 0)) {
		return; /* matching arch loaded */
	}

	path = g_string_new (argv [0]);
	arch_offset = path->len -2; /* last two characters */

	/* Remove arch suffix if present */
	if (strstr (&path->str[arch_offset], "32") || strstr (&path->str[arch_offset], "64")) {
		g_string_truncate (path, arch_offset);
	}

	g_string_append (path, target_arch);

	if (execvp (path->str, argv) < 0) {
		fprintf (stderr, "Error: --arch=%s Failed to switch to '%s'.\n", target_arch, path->str);
		exit (1);
	}
}

#endif
/**
 * mono_main:
 * @argc: number of arguments in the argv array
 * @argv: array of strings containing the startup arguments
 *
 * Launches the Mono JIT engine and parses all the command line options
 * in the same way that the mono command line VM would.
 */
int
mono_main (int argc, char* argv[])
{
	MainThreadArgs main_args;
	MonoAssembly *assembly;
	MonoMethodDesc *desc;
	MonoMethod *method;
	MonoCompile *cfg;
	MonoDomain *domain;
	MonoImageOpenStatus open_status;
	const char* aname, *mname = NULL;
	char *config_file = NULL;
	int i, count = 1;
	guint32 opt, action = DO_EXEC, recompilation_times = 1;
	MonoGraphOptions mono_graph_options = 0;
	int mini_verbose = 0;
	gboolean enable_profile = FALSE;
	char *trace_options = NULL;
	char *profile_options = NULL;
	char *aot_options = NULL;
	char *forced_version = NULL;
	GPtrArray *agents = NULL;
	char *attach_options = NULL;
#ifdef MONO_JIT_INFO_TABLE_TEST
	int test_jit_info_table = FALSE;
#endif
#ifdef HOST_WIN32
	int mixed_mode = FALSE;
#endif
#ifdef __native_client__
	gboolean nacl_null_checks_off = FALSE;
#endif

#ifdef MOONLIGHT
#ifndef HOST_WIN32
	/* stdout defaults to block buffering if it's not writing to a terminal, which
	 * happens with our test harness: we redirect stdout to capture it. Force line
	 * buffering in all cases. */
	setlinebuf (stdout);
#endif
#endif

	setlocale (LC_ALL, "");

	if (g_getenv ("MONO_NO_SMP"))
		mono_set_use_smp (FALSE);
	
	g_log_set_always_fatal (G_LOG_LEVEL_ERROR);
	g_log_set_fatal_mask (G_LOG_DOMAIN, G_LOG_LEVEL_ERROR);

	opt = mono_parse_default_optimizations (NULL);

	for (i = 1; i < argc; ++i) {
		if (argv [i] [0] != '-')
			break;
		if (strcmp (argv [i], "--regression") == 0) {
			action = DO_REGRESSION;
		} else if (strncmp (argv [i], "--single-method=", 16) == 0) {
			char *full_opts = g_strdup_printf ("-all,%s", argv [i] + 16);
			action = DO_SINGLE_METHOD_REGRESSION;
			mono_single_method_regression_opt = parse_optimizations (opt, full_opts);
			g_free (full_opts);
		} else if (strcmp (argv [i], "--verbose") == 0 || strcmp (argv [i], "-v") == 0) {
			mini_verbose++;
		} else if (strcmp (argv [i], "--version") == 0 || strcmp (argv [i], "-V") == 0) {
			char *build = mono_get_runtime_build_info ();
			char *gc_descr;

			g_print ("Mono JIT compiler version %s\nCopyright (C) 2002-2014 Novell, Inc, Xamarin Inc and Contributors. www.mono-project.com\n", build);
			g_free (build);
			g_print (info);
			gc_descr = mono_gc_get_description ();
			g_print ("\tGC:            %s\n", gc_descr);
			g_free (gc_descr);
			if (mini_verbose) {
				const char *cerror;
				const char *clibpath;
				mono_init ("mono");
				cerror = mono_check_corlib_version ();
				clibpath = mono_defaults.corlib? mono_image_get_filename (mono_defaults.corlib): "unknown";
				if (cerror) {
					g_print ("The currently installed mscorlib doesn't match this runtime version.\n");
					g_print ("The error is: %s\n", cerror);
					g_print ("mscorlib.dll loaded at: %s\n", clibpath);
					return 1;
				}
			}
			return 0;
		} else if (strcmp (argv [i], "--help") == 0 || strcmp (argv [i], "-h") == 0) {
			mini_usage ();
			return 0;
		} else if (strcmp (argv [i], "--help-trace") == 0){
			mini_trace_usage ();
			return 0;
		} else if (strcmp (argv [i], "--help-devel") == 0){
			mini_usage_jitdeveloper ();
			return 0;
		} else if (strcmp (argv [i], "--help-debug") == 0){
			mini_debug_usage ();
			return 0;
		} else if (strcmp (argv [i], "--list-opt") == 0){
			mini_usage_list_opt ();
			return 0;
		} else if (strncmp (argv [i], "--statfile", 10) == 0) {
			if (i + 1 >= argc){
				fprintf (stderr, "error: --statfile requires a filename argument\n");
				return 1;
			}
			mini_stats_fd = fopen (argv [++i], "w+");
		} else if (strncmp (argv [i], "--optimize=", 11) == 0) {
			opt = parse_optimizations (opt, argv [i] + 11);
		} else if (strncmp (argv [i], "-O=", 3) == 0) {
			opt = parse_optimizations (opt, argv [i] + 3);
		} else if (strcmp (argv [i], "--gc=sgen") == 0) {
			switch_gc (argv, "sgen");
		} else if (strcmp (argv [i], "--gc=boehm") == 0) {
			switch_gc (argv, "boehm");
		}
#ifdef TARGET_OSX
		else if (strcmp (argv [i], "--arch=32") == 0) {
			switch_arch (argv, "32");
		} else if (strcmp (argv [i], "--arch=64") == 0) {
			switch_arch (argv, "64");
		}
#endif
		else if (strcmp (argv [i], "--config") == 0) {
			if (i +1 >= argc){
				fprintf (stderr, "error: --config requires a filename argument\n");
				return 1;
			}
			config_file = argv [++i];
#ifdef HOST_WIN32
		} else if (strcmp (argv [i], "--mixed-mode") == 0) {
			mixed_mode = TRUE;
#endif
		} else if (strcmp (argv [i], "--ncompile") == 0) {
			if (i + 1 >= argc){
				fprintf (stderr, "error: --ncompile requires an argument\n");
				return 1;
			}
			count = atoi (argv [++i]);
			action = DO_BENCH;
		} else if (strcmp (argv [i], "--trace") == 0) {
			trace_options = (char*)"";
		} else if (strncmp (argv [i], "--trace=", 8) == 0) {
			trace_options = &argv [i][8];
		} else if (strcmp (argv [i], "--breakonex") == 0) {
			MonoDebugOptions *opt = mini_get_debug_options ();

			opt->break_on_exc = TRUE;
		} else if (strcmp (argv [i], "--break") == 0) {
			if (i+1 >= argc){
				fprintf (stderr, "Missing method name in --break command line option\n");
				return 1;
			}
			
			if (!mono_debugger_insert_breakpoint (argv [++i], FALSE))
				fprintf (stderr, "Error: invalid method name '%s'\n", argv [i]);
		} else if (strcmp (argv [i], "--break-at-bb") == 0) {
			if (i + 2 >= argc) {
				fprintf (stderr, "Missing method name or bb num in --break-at-bb command line option.");
				return 1;
			}
			mono_break_at_bb_method = mono_method_desc_new (argv [++i], TRUE);
			if (mono_break_at_bb_method == NULL) {
				fprintf (stderr, "Method name is in a bad format in --break-at-bb command line option.");
				return 1;
			}
			mono_break_at_bb_bb_num = atoi (argv [++i]);
		} else if (strcmp (argv [i], "--inject-async-exc") == 0) {
			if (i + 2 >= argc) {
				fprintf (stderr, "Missing method name or position in --inject-async-exc command line option\n");
				return 1;
			}
			mono_inject_async_exc_method = mono_method_desc_new (argv [++i], TRUE);
			if (mono_inject_async_exc_method == NULL) {
				fprintf (stderr, "Method name is in a bad format in --inject-async-exc command line option\n");
				return 1;
			}
			mono_inject_async_exc_pos = atoi (argv [++i]);
		} else if (strcmp (argv [i], "--verify-all") == 0) {
			mono_verifier_enable_verify_all ();
		} else if (strcmp (argv [i], "--full-aot") == 0) {
			mono_aot_only = TRUE;
		} else if (strcmp (argv [i], "--llvmonly") == 0) {
			mono_aot_only = TRUE;
			mono_llvm_only = TRUE;
		} else if (strcmp (argv [i], "--print-vtable") == 0) {
			mono_print_vtable = TRUE;
		} else if (strcmp (argv [i], "--stats") == 0) {
			mono_counters_enable (-1);
			mono_stats.enabled = TRUE;
			mono_jit_stats.enabled = TRUE;
#ifndef DISABLE_AOT
		} else if (strcmp (argv [i], "--aot") == 0) {
			error_if_aot_unsupported ();
			mono_compile_aot = TRUE;
		} else if (strncmp (argv [i], "--aot=", 6) == 0) {
			error_if_aot_unsupported ();
			mono_compile_aot = TRUE;
			aot_options = &argv [i][6];
#endif
		} else if (strncmp (argv [i], "--compile-all=", 14) == 0) {
			action = DO_COMPILE;
			recompilation_times = atoi (argv [i] + 14);
		} else if (strcmp (argv [i], "--compile-all") == 0) {
			action = DO_COMPILE;
		} else if (strncmp (argv [i], "--runtime=", 10) == 0) {
			forced_version = &argv [i][10];
		} else if (strcmp (argv [i], "--jitmap") == 0) {
			mono_enable_jit_map ();
		} else if (strcmp (argv [i], "--profile") == 0) {
			enable_profile = TRUE;
			profile_options = NULL;
		} else if (strncmp (argv [i], "--profile=", 10) == 0) {
			enable_profile = TRUE;
			profile_options = argv [i] + 10;
		} else if (strncmp (argv [i], "--agent=", 8) == 0) {
			if (agents == NULL)
				agents = g_ptr_array_new ();
			g_ptr_array_add (agents, argv [i] + 8);
		} else if (strncmp (argv [i], "--attach=", 9) == 0) {
			attach_options = argv [i] + 9;
		} else if (strcmp (argv [i], "--compile") == 0) {
			if (i + 1 >= argc){
				fprintf (stderr, "error: --compile option requires a method name argument\n");
				return 1;
			}
			
			mname = argv [++i];
			action = DO_BENCH;
		} else if (strncmp (argv [i], "--graph=", 8) == 0) {
			if (i + 1 >= argc){
				fprintf (stderr, "error: --graph option requires a method name argument\n");
				return 1;
			}
			
			mono_graph_options = mono_parse_graph_options (argv [i] + 8);
			mname = argv [++i];
			action = DO_DRAW;
		} else if (strcmp (argv [i], "--graph") == 0) {
			if (i + 1 >= argc){
				fprintf (stderr, "error: --graph option requires a method name argument\n");
				return 1;
			}
			
			mname = argv [++i];
			mono_graph_options = MONO_GRAPH_CFG;
			action = DO_DRAW;
		} else if (strcmp (argv [i], "--debug") == 0) {
			enable_debugging = TRUE;
		} else if (strncmp (argv [i], "--debug=", 8) == 0) {
			enable_debugging = TRUE;
			if (!parse_debug_options (argv [i] + 8))
				return 1;
 		} else if (strncmp (argv [i], "--debugger-agent=", 17) == 0) {
			MonoDebugOptions *opt = mini_get_debug_options ();

 			mono_debugger_agent_parse_options (argv [i] + 17);
			opt->mdb_optimizations = TRUE;
			enable_debugging = TRUE;
		} else if (strcmp (argv [i], "--security") == 0) {
#ifndef DISABLE_SECURITY
			mono_verifier_set_mode (MONO_VERIFIER_MODE_VERIFIABLE);
#else
			fprintf (stderr, "error: --security: not compiled with security manager support");
			return 1;
#endif
		} else if (strncmp (argv [i], "--security=", 11) == 0) {
			/* Note: validil, and verifiable need to be
			   accepted even if DISABLE_SECURITY is defined. */

			if (strcmp (argv [i] + 11, "core-clr") == 0) {
#ifndef DISABLE_SECURITY
				mono_verifier_set_mode (MONO_VERIFIER_MODE_VERIFIABLE);
				mono_security_set_mode (MONO_SECURITY_MODE_CORE_CLR);
#else
				fprintf (stderr, "error: --security: not compiled with CoreCLR support");
				return 1;
#endif
			} else if (strcmp (argv [i] + 11, "core-clr-test") == 0) {
#ifndef DISABLE_SECURITY
				/* fixme should we enable verifiable code here?*/
				mono_security_set_mode (MONO_SECURITY_MODE_CORE_CLR);
				mono_security_core_clr_test = TRUE;
#else
				fprintf (stderr, "error: --security: not compiled with CoreCLR support");
				return 1;
#endif
			} else if (strcmp (argv [i] + 11, "cas") == 0) {
#ifndef DISABLE_SECURITY
				fprintf (stderr, "warning: --security=cas not supported.");
#else
				fprintf (stderr, "error: --security: not compiled with CAS support");
				return 1;
#endif
			} else if (strcmp (argv [i] + 11, "validil") == 0) {
				mono_verifier_set_mode (MONO_VERIFIER_MODE_VALID);
			} else if (strcmp (argv [i] + 11, "verifiable") == 0) {
				mono_verifier_set_mode (MONO_VERIFIER_MODE_VERIFIABLE);
			} else {
				fprintf (stderr, "error: --security= option has invalid argument (cas, core-clr, verifiable or validil)\n");
				return 1;
			}
		} else if (strcmp (argv [i], "--desktop") == 0) {
			mono_gc_set_desktop_mode ();
			/* Put more desktop-specific optimizations here */
		} else if (strcmp (argv [i], "--server") == 0){
			mono_config_set_server_mode (TRUE);
			/* Put more server-specific optimizations here */
		} else if (strcmp (argv [i], "--inside-mdb") == 0) {
			action = DO_DEBUGGER;
		} else if (strncmp (argv [i], "--wapi=", 7) == 0) {
			fprintf (stderr, "--wapi= option no longer supported\n.");
			return 1;
		} else if (strcmp (argv [i], "--no-x86-stack-align") == 0) {
			mono_do_x86_stack_align = FALSE;
#ifdef MONO_JIT_INFO_TABLE_TEST
		} else if (strcmp (argv [i], "--test-jit-info-table") == 0) {
			test_jit_info_table = TRUE;
#endif
		} else if (strcmp (argv [i], "--llvm") == 0) {
#ifndef MONO_ARCH_LLVM_SUPPORTED
			fprintf (stderr, "Mono Warning: --llvm not supported on this platform.\n");
#elif !defined(ENABLE_LLVM)
			fprintf (stderr, "Mono Warning: --llvm not enabled in this runtime.\n");
#else
			mono_use_llvm = TRUE;
#endif
		} else if (strcmp (argv [i], "--nollvm") == 0){
			mono_use_llvm = FALSE;
#ifdef __native_client_codegen__
		} else if (strcmp (argv [i], "--nacl-align-mask-off") == 0){
			nacl_align_byte = -1; /* 0xff */
#endif
#ifdef __native_client__
		} else if (strcmp (argv [i], "--nacl-mono-path") == 0){
			nacl_mono_path = g_strdup(argv[++i]);
		} else if (strcmp (argv [i], "--nacl-null-checks-off") == 0){
			nacl_null_checks_off = TRUE;
#endif
		} else {
			fprintf (stderr, "Unknown command line option: '%s'\n", argv [i]);
			return 1;
		}
	}

#ifdef __native_client_codegen__
	if (g_getenv ("MONO_NACL_ALIGN_MASK_OFF"))
	{
		nacl_align_byte = -1; /* 0xff */
	}
	if (!nacl_null_checks_off) {
		MonoDebugOptions *opt = mini_get_debug_options ();
		opt->explicit_null_checks = TRUE;
	}
#endif

	if (!argv [i]) {
		mini_usage ();
		return 1;
	}

#if !defined(HOST_WIN32) && defined(HAVE_UNISTD_H)
	/*
	 * If we are not embedded, use the mono runtime executable to run managed exe's.
	 */
	{
		char *runtime_path;

		runtime_path = wapi_process_get_path (getpid ());
		if (runtime_path) {
			wapi_process_set_cli_launcher (runtime_path);
			g_free (runtime_path);
		}
	}
#endif

	if (g_getenv ("MONO_XDEBUG"))
		enable_debugging = TRUE;

#ifdef MONO_CROSS_COMPILE
       if (!mono_compile_aot) {
		   fprintf (stderr, "This mono runtime is compiled for cross-compiling. Only the --aot option is supported.\n");
		   exit (1);
       }
#if SIZEOF_VOID_P == 8 && (defined(TARGET_ARM) || defined(TARGET_X86))
       fprintf (stderr, "Can't cross-compile on 64-bit platforms to 32-bit architecture.\n");
       exit (1);
#elif SIZEOF_VOID_P == 4 && (defined(TARGET_ARM64) || defined(TARGET_AMD64))
       fprintf (stderr, "Can't cross-compile on 32-bit platforms to 64-bit architecture.\n");
       exit (1);
#endif
#endif

	if (mono_compile_aot || action == DO_EXEC || action == DO_DEBUGGER) {
		g_set_prgname (argv[i]);
	}

	mono_counters_init ();

	/* Set rootdir before loading config */
	mono_set_rootdir ();

	if (enable_profile)
		mono_profiler_load (profile_options);

	mono_attach_parse_options (attach_options);

	if (trace_options != NULL){
		/* 
		 * Need to call this before mini_init () so we can trace methods 
		 * compiled there too.
		 */
		mono_jit_trace_calls = mono_trace_parse_options (trace_options);
		if (mono_jit_trace_calls == NULL)
			exit (1);
	}

#ifdef DISABLE_JIT
	if (!mono_aot_only) {
		fprintf (stderr, "This runtime has been configured with --enable-minimal=jit, so the --full-aot command line option is required.\n");
		exit (1);
	}
#endif

	if (action == DO_DEBUGGER) {
		enable_debugging = TRUE;
		g_print ("The Mono Debugger is no longer supported.\n");
		return 1;
	} else if (enable_debugging)
		mono_debug_init (MONO_DEBUG_FORMAT_MONO);

#ifdef HOST_WIN32
	if (mixed_mode)
		mono_load_coree (argv [i]);
#endif

	/* Parse gac loading options before loading assemblies. */
	if (mono_compile_aot || action == DO_EXEC || action == DO_DEBUGGER) {
		mono_config_parse (config_file);
	}

	mono_set_defaults (mini_verbose, opt);
	domain = mini_init (argv [i], forced_version);

	mono_gc_set_stack_end (&domain);

	if (agents) {
		int i;

		for (i = 0; i < agents->len; ++i) {
			int res = load_agent (domain, (char*)g_ptr_array_index (agents, i));
			if (res) {
				g_ptr_array_free (agents, TRUE);
				mini_cleanup (domain);
				return 1;
			}
		}

		g_ptr_array_free (agents, TRUE);
	}
	
	switch (action) {
	case DO_SINGLE_METHOD_REGRESSION:
		mono_do_single_method_regression = TRUE;
	case DO_REGRESSION:
		if (mini_regression_list (mini_verbose, argc -i, argv + i)) {
			g_print ("Regression ERRORS!\n");
			mini_cleanup (domain);
			return 1;
		}
		mini_cleanup (domain);
		return 0;
	case DO_BENCH:
		if (argc - i != 1 || mname == NULL) {
			g_print ("Usage: mini --ncompile num --compile method assembly\n");
			mini_cleanup (domain);
			return 1;
		}
		aname = argv [i];
		break;
	case DO_COMPILE:
		if (argc - i != 1) {
			mini_usage ();
			mini_cleanup (domain);
			return 1;
		}
		aname = argv [i];
		break;
	case DO_DRAW:
		if (argc - i != 1 || mname == NULL) {
			mini_usage ();
			mini_cleanup (domain);
			return 1;
		}
		aname = argv [i];
		break;
	default:
		if (argc - i < 1) {
			mini_usage ();
			mini_cleanup (domain);
			return 1;
		}
		aname = argv [i];
		break;
	}

#ifdef MONO_JIT_INFO_TABLE_TEST
	if (test_jit_info_table)
		jit_info_table_test (domain);
#endif

	assembly = mono_assembly_open (aname, &open_status);
	if (!assembly) {
		fprintf (stderr, "Cannot open assembly '%s': %s.\n", aname, mono_image_strerror (open_status));
		mini_cleanup (domain);
		return 2;
	}

	if (trace_options != NULL)
		mono_trace_set_assembly (assembly);

	if (mono_compile_aot || action == DO_EXEC) {
		const char *error;

		//mono_set_rootdir ();

		error = mono_check_corlib_version ();
		if (error) {
			fprintf (stderr, "Corlib not in sync with this runtime: %s\n", error);
			fprintf (stderr, "Loaded from: %s\n",
				mono_defaults.corlib? mono_image_get_filename (mono_defaults.corlib): "unknown");
			fprintf (stderr, "Download a newer corlib or a newer runtime at http://www.mono-project.com/download.\n");
			exit (1);
		}

#ifdef HOST_WIN32
		/* Detach console when executing IMAGE_SUBSYSTEM_WINDOWS_GUI on win32 */
		if (!enable_debugging && !mono_compile_aot && ((MonoCLIImageInfo*)(mono_assembly_get_image (assembly)->image_info))->cli_header.nt.pe_subsys_required == IMAGE_SUBSYSTEM_WINDOWS_GUI)
			FreeConsole ();
#endif

		main_args.domain = domain;
		main_args.file = aname;		
		main_args.argc = argc - i;
		main_args.argv = argv + i;
		main_args.opts = opt;
		main_args.aot_options = aot_options;
#if RUN_IN_SUBTHREAD
		mono_runtime_exec_managed_code (domain, main_thread_handler, &main_args);
#else
		main_thread_handler (&main_args);
		mono_thread_manage ();
#endif

		mini_cleanup (domain);

		/* Look up return value from System.Environment.ExitCode */
		i = mono_environment_exitcode_get ();
		return i;
	} else if (action == DO_COMPILE) {
		compile_all_methods (assembly, mini_verbose, opt, recompilation_times);
		mini_cleanup (domain);
		return 0;
	} else if (action == DO_DEBUGGER) {
		return 1;
	}
	desc = mono_method_desc_new (mname, 0);
	if (!desc) {
		g_print ("Invalid method name %s\n", mname);
		mini_cleanup (domain);
		return 3;
	}
	method = mono_method_desc_search_in_image (desc, mono_assembly_get_image (assembly));
	if (!method) {
		g_print ("Cannot find method %s\n", mname);
		mini_cleanup (domain);
		return 3;
	}

#ifndef DISABLE_JIT
	if (action == DO_DRAW) {
		int part = 0;

		switch (mono_graph_options) {
		case MONO_GRAPH_DTREE:
			part = 1;
			opt |= MONO_OPT_LOOP;
			break;
		case MONO_GRAPH_CFG_CODE:
			part = 1;
			break;
		case MONO_GRAPH_CFG_SSA:
			part = 2;
			break;
		case MONO_GRAPH_CFG_OPTCODE:
			part = 3;
			break;
		default:
			break;
		}

		if ((method->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL) ||
			(method->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL)) {
			MonoMethod *nm;
			nm = mono_marshal_get_native_wrapper (method, TRUE, FALSE);
			cfg = mini_method_compile (nm, opt, mono_get_root_domain (), 0, part, -1);
		}
		else
			cfg = mini_method_compile (method, opt, mono_get_root_domain (), 0, part, -1);
		if ((mono_graph_options & MONO_GRAPH_CFG_SSA) && !(cfg->comp_done & MONO_COMP_SSA)) {
			g_warning ("no SSA info available (use -O=deadce)");
			return 1;
		}
		mono_draw_graph (cfg, mono_graph_options);
		mono_destroy_compile (cfg);

	} else if (action == DO_BENCH) {
		if (mini_stats_fd) {
			const char *n;
			double no_opt_time = 0.0;
			GTimer *timer = g_timer_new ();
			fprintf (mini_stats_fd, "$stattitle = \'Compilations times for %s\';\n", 
				 mono_method_full_name (method, TRUE));
			fprintf (mini_stats_fd, "@data = (\n");
			fprintf (mini_stats_fd, "[");
			for (i = 0; i < G_N_ELEMENTS (opt_sets); i++) {
				opt = opt_sets [i];
				n = opt_descr (opt);
				if (!n [0])
					n = "none";
				fprintf (mini_stats_fd, "\"%s\",", n);
			}
			fprintf (mini_stats_fd, "],\n[");

			for (i = 0; i < G_N_ELEMENTS (opt_sets); i++) {
				int j;
				double elapsed;
				opt = opt_sets [i];
				g_timer_start (timer);
				for (j = 0; j < count; ++j) {
					cfg = mini_method_compile (method, opt, mono_get_root_domain (), 0, 0, -1);
					mono_destroy_compile (cfg);
				}
				g_timer_stop (timer);
				elapsed = g_timer_elapsed (timer, NULL);
				if (!opt)
					no_opt_time = elapsed;
				fprintf (mini_stats_fd, "%f, ", elapsed);
			}
			fprintf (mini_stats_fd, "]");
			if (no_opt_time > 0.0) {
				fprintf (mini_stats_fd, ", \n[");
				for (i = 0; i < G_N_ELEMENTS (opt_sets); i++) 
					fprintf (mini_stats_fd, "%f,", no_opt_time);
				fprintf (mini_stats_fd, "]");
			}
			fprintf (mini_stats_fd, ");\n");
		} else {
			for (i = 0; i < count; ++i) {
				if ((method->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL) ||
					(method->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL))
					method = mono_marshal_get_native_wrapper (method, TRUE, FALSE);

				cfg = mini_method_compile (method, opt, mono_get_root_domain (), 0, 0, -1);
				mono_destroy_compile (cfg);
			}
		}
	} else {
		cfg = mini_method_compile (method, opt, mono_get_root_domain (), 0, 0, -1);
		mono_destroy_compile (cfg);
	}
#endif

	mini_cleanup (domain);
 	return 0;
}

MonoDomain * 
mono_jit_init (const char *file)
{
	return mini_init (file, NULL);
}

/**
 * mono_jit_init_version:
 * @domain_name: the name of the root domain
 * @runtime_version: the version of the runtime to load
 *
 * Use this version when you want to force a particular runtime
 * version to be used.  By default Mono will pick the runtime that is
 * referenced by the initial assembly (specified in @file), this
 * routine allows programmers to specify the actual runtime to be used
 * as the initial runtime is inherited by all future assemblies loaded
 * (since Mono does not support having more than one mscorlib runtime
 * loaded at once).
 *
 * The @runtime_version can be one of these strings: "v1.1.4322" for
 * the 1.1 runtime or "v2.0.50727"  for the 2.0 runtime. 
 *
 * Returns: the MonoDomain representing the domain where the assembly
 * was loaded.
 */
MonoDomain * 
mono_jit_init_version (const char *domain_name, const char *runtime_version)
{
	return mini_init (domain_name, runtime_version);
}

void        
mono_jit_cleanup (MonoDomain *domain)
{
	mono_thread_manage ();

	mini_cleanup (domain);
}

void
mono_jit_set_aot_only (gboolean val)
{
	mono_aot_only = val;
}

void
mono_jit_set_aot_mode (MonoAotMode mode)
{
	mono_aot_mode = mode;
}

/**
 * mono_jit_set_trace_options:
 * @options: string representing the trace options
 *
 * Set the options of the tracing engine. This function can be called before initializing
 * the mono runtime. See the --trace mono(1) manpage for the options format.
 *
 * Returns: #TRUE if the options where parsed and set correctly, #FALSE otherwise.
 */
gboolean
mono_jit_set_trace_options (const char* options)
{
	MonoTraceSpec *trace_opt = mono_trace_parse_options (options);
	if (trace_opt == NULL)
		return FALSE;
	mono_jit_trace_calls = trace_opt;
	return TRUE;
}

/**
 * mono_set_signal_chaining:
 *
 *   Enable/disable signal chaining. This should be called before mono_jit_init ().
 * If signal chaining is enabled, the runtime saves the original signal handlers before
 * installing its own handlers, and calls the original ones in the following cases:
 * - a SIGSEGV/SIGABRT signal received while executing native (i.e. not JITted) code.
 * - SIGPROF
 * - SIGFPE
 * - SIGQUIT
 * - SIGUSR2
 * Signal chaining only works on POSIX platforms.
 */
void
mono_set_signal_chaining (gboolean chain_signals)
{
	mono_do_signal_chaining = chain_signals;
}

/**
 * mono_set_crash_chaining:
 *
 * Enable/disable crash chaining due to signals. When a fatal signal is delivered and
 * Mono doesn't know how to handle it, it will invoke the crash handler. If chrash chaining
 * is enabled, it will first print its crash information and then try to chain with the native handler.
 */
void
mono_set_crash_chaining (gboolean chain_crashes)
{
	mono_do_crash_chaining = chain_crashes;
}
