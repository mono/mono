/*
 * gmisc.c: Misc functions with no place to go (right now)
 *
 * Author:
 *   Aaron Bockover (abockover@novell.com)
 *
 * (C) 2006 Novell, Inc.
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

#include <config.h>
#include <stdlib.h>
#include <glib.h>
#include <pthread.h>

#ifdef HAVE_PWD_H
#include <pwd.h>
#endif

#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif

extern char **environ;

static GHashTable *env;

static pthread_mutex_t env_lock = PTHREAD_MUTEX_INITIALIZER;

static void
g_getenv_init (void)
{
	pthread_mutex_lock (&env_lock);

	// Raced
	if (env) {
		pthread_mutex_unlock (&env_lock);
		return;
	}

	env = g_hash_table_new (g_str_hash, g_str_equal);

	char **head = (char **) environ;
	for (int i = 0; head [i] != NULL; i++) {
		char *line = g_strdup (head [i]);
		int p = 0;
		while (line [p] != '\0' && line [p] != '=')
			p++;
		if (line [p] != '=')
			continue;
		line [p] = '\0';
		// Now line is a buffer of memory where the prefix
		// is the key, and the value is after a NULL
		char *key = line;

		// Can make this strdup if we want to free, not leak
		char *value = &line [p + 1];

		g_hash_table_insert (env, key, value);
	}

	pthread_mutex_unlock (&env_lock);
}

// FIXME: add refcounting to this, so the
// memory isn't leaked on overwrite
const gchar *
g_getenv(const gchar *variable)
{
	if (!env)
		g_getenv_init ();

	pthread_mutex_lock (&env_lock);
	gchar *res = g_hash_table_lookup (env, (gpointer) variable);
	pthread_mutex_unlock (&env_lock);
	return res;
}

gboolean
g_setenv(const gchar *variable, const gchar *value, gboolean overwrite)
{
	if (!env)
		g_getenv_init ();

	pthread_mutex_lock (&env_lock);

	gchar *curr = g_hash_table_lookup (env, (gpointer) variable);
	if (!overwrite && curr != NULL) {
		pthread_mutex_unlock (&env_lock);
		return FALSE;
	}

	int result = setenv(variable, value, overwrite) == 0;
	if (result)
		g_hash_table_insert (env, (gpointer) variable, (gpointer) value);
	pthread_mutex_unlock (&env_lock);

	return result;
}

void
g_unsetenv(const gchar *variable)
{
	if (!env)
		g_getenv_init ();

	pthread_mutex_lock (&env_lock);
	unsetenv(variable);
	g_hash_table_remove (env, (gpointer) variable);
	pthread_mutex_unlock (&env_lock);
}

gchar*
g_win32_getlocale(void)
{
	return NULL;
}

gboolean
g_path_is_absolute (const char *filename)
{
	g_return_val_if_fail (filename != NULL, FALSE);

	return (*filename == '/');
}

static pthread_mutex_t pw_lock = PTHREAD_MUTEX_INITIALIZER;
static const gchar *home_dir;
static const gchar *user_name;

static void
get_pw_data (void)
{
#ifdef HAVE_GETPWUID_R
	struct passwd pw;
	struct passwd *result;
	char buf [4096];
#endif

	if (user_name != NULL)
		return;

	pthread_mutex_lock (&pw_lock);
	if (user_name != NULL) {
		pthread_mutex_unlock (&pw_lock);
		return;
	}

	home_dir = g_getenv ("HOME");
	user_name = g_getenv ("USER");

#ifdef HAVE_GETPWUID_R
	if (home_dir == NULL || user_name == NULL) {
		if (getpwuid_r (getuid (), &pw, buf, 4096, &result) == 0) {
			if (home_dir == NULL)
				home_dir = g_strdup (pw.pw_dir);
			if (user_name == NULL)
				user_name = g_strdup (pw.pw_name);
		}
	}
#endif

	if (user_name == NULL)
		user_name = "somebody";
	if (home_dir == NULL)
		home_dir = "/";

	pthread_mutex_unlock (&pw_lock);
}

const gchar *
g_get_home_dir (void)
{
	get_pw_data ();
	return home_dir;
}

const char *
g_get_user_name (void)
{
	get_pw_data ();
	return user_name;
}

static const char *tmp_dir;

static pthread_mutex_t tmp_lock = PTHREAD_MUTEX_INITIALIZER;

const gchar *
g_get_tmp_dir (void)
{
	if (tmp_dir == NULL){
		pthread_mutex_lock (&tmp_lock);
		if (tmp_dir == NULL){
			tmp_dir = g_getenv ("TMPDIR");
			if (tmp_dir == NULL){
				tmp_dir = g_getenv ("TMP");
				if (tmp_dir == NULL){
					tmp_dir = g_getenv ("TEMP");
					if (tmp_dir == NULL)
						tmp_dir = "/tmp";
				}
			}
		}
		pthread_mutex_unlock (&tmp_lock);
	}
	return tmp_dir;
}

