/* GNOME DB library
 * Copyright (C) 1999-2002 The GNOME Foundation.
 *
 * AUTHORS:
 *      Rodrigo Moya <rodrigo@gnome-db.org>
 *      Daniel Morgan <danmorg@sc.rr.com>
 *
 * This Library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Library General Public License as
 * published by the Free Software Foundation; either version 2 of the
 * License, or (at your option) any later version.
 *
 * This Library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Library General Public License for more details.
 *
 * You should have received a copy of the GNU Library General Public
 * License along with this Library; see the file COPYING.LIB.  If not,
 * write to the Free Software Foundation, Inc., 59 Temple Place - Suite 330,
 * Boston, MA 02111-1307, USA.
 */

#if !defined(__gnome_db_sqleditor_h__)
#  define __gnome_db_sqleditor_h__

#include <gtk/gtkvbox.h>

G_BEGIN_DECLS

#define GNOME_DB_TYPE_SQL_EDITOR      (gnome_db_sql_editor_get_type())
#define GNOME_DB_SQL_EDITOR(obj)      (G_TYPE_CHECK_INSTANCE_CAST (obj, GNOME_DB_TYPE_SQL_EDITOR, GnomeDbSqlEditor))
#define GNOME_DB_SQL_EDITOR_CLASS(klass)    (G_TYPE_CHECK_CLASS_CAST (klass, GNOME_DB_TYPE_SQL_EDITOR, GnomeDbSqlEditor))
#define GNOME_DB_IS_SQL_EDITOR(obj)         (G_TYPE_CHECK_INSTANCE_TYPE (obj, GNOME_DB_TYPE_SQL_EDITOR))
#define GNOME_DB_IS_SQL_EDITOR_CLASS(klass) (G_TYPE_CHECK_CLASS_TYPE ((klass), GNOME_DB_TYPE_SQL_EDITOR))

typedef struct _GnomeDbSqlEditor        GnomeDbSqlEditor;
typedef struct _GnomeDbSqlEditorClass   GnomeDbSqlEditorClass;
typedef struct _GnomeDbSqlEditorPrivate GnomeDbSqlEditorPrivate;


struct _GnomeDbSqlEditor {
	GtkVBox parent;
	GnomeDbSqlEditorPrivate *priv;
};

struct _GnomeDbSqlEditorClass {
	GtkVBoxClass parent_class;
	void (* text_changed) (GnomeDbSqlEditor *sql_editor);
};

/** GnomeDbSqlEditorCommand
 *
 * #gnome_db_sql_editor_get_all_commands returns 
 * a #GList of #GnomeDbEditorCommand
 * 
 * offset is the offset in the GnomeDbEditor where the SQL Command ends.
 * sql_statement is the SQL statement to be executed
 *
 * A SQL Command structure contains:
 * the offset in the GnomeDbSqlEditor where the sql_statement is located.
 */
typedef struct _GnomeDbSqlEditorCommand GnomeDbSqlEditorCommand;
struct _GnomeDbSqlEditorCommand {
	gint offset;
	gchar *sql_statement;
	gint error; /* 0 = no error; currently, not being used. */
};

typedef gboolean	(*GnomeDbSqlEditorRunFunc)	(GnomeDbSqlEditorCommand *cmd,
							 gpointer  user_data);

GType            gnome_db_sql_editor_get_type (void);
GtkWidget       *gnome_db_sql_editor_new (void);

GList		*gnome_db_sql_editor_get_all_commands (GnomeDbSqlEditor *sql_editor,
						gboolean run_at_pref );

gboolean	gnome_db_sql_editor_foreach_command (GnomeDbSqlEditor *sql_editor,
			gboolean run_at_pref, 
			GnomeDbSqlEditorRunFunc run_command, 
			gpointer user_data);

GnomeDbSqlEditorCommand *gnome_db_sql_editor_get_command_at_cursor (GnomeDbSqlEditor *sql_editor);

GtkTextView 	*gnome_db_sql_editor_get_text_view (GnomeDbSqlEditor *sql_editor);

GtkTextBuffer	*gnome_db_sql_editor_get_text_buffer (GnomeDbSqlEditor *sql_editor);

gboolean	gnome_db_sql_editor_load_from_file (GnomeDbSqlEditor *sql_editor, gchar *filename);
gboolean	gnome_db_sql_editor_save_to_file (GnomeDbSqlEditor *sql_editor, gchar *filename);
void		gnome_db_sql_editor_use_syntax_hi_lighting (GnomeDbSqlEditor *sql_editor, 
					gboolean hi_lighting);

void gnome_db_sql_editor_debug_setup(GnomeDbSqlEditor *ed, GtkTextView *dtv);
void gnome_db_sql_editor_debug(GnomeDbSqlEditor *ed, gchar *text);


G_END_DECLS

#endif
