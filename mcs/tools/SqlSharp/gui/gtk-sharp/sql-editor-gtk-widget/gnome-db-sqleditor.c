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

#include <gtk/gtk.h>
#include <stdio.h>
#include <string.h>
#include <stdlib.h> 
#include "gnome-db-sqleditor.h"

#define PARENT_TYPE GTK_TYPE_VBOX

#define FILE_BUFFER_SIZE 8192

static GObjectClass *parent_class = NULL;

struct _GnomeDbSqlEditorPrivate
{
	/* text tags for syntax hi-lighted text */
	GtkTextTag * freecomment_tag;
	GtkTextTag * linecomment_tag; 
	GtkTextTag * singlequotedconstant_tag;
	GtkTextTag * sql_tag;
	GtkTextTag * normaltext_tag;

	/* determine if something has changed beyond a line */
        /* updating one line is faster than the whole buffer */
	gint line_last_changed;
	gint last_freecomment_count;

	/* settings */
	gboolean use_hi_lighting;
	gchar *family;

	/* widgets */
	GtkWidget *scroll;
	GtkWidget *text_view;

	/* debug text_view widget */
	GtkWidget *debug_text_view;
};

static gchar *sql_keywords[] = {
	"DELETE",
	"FROM",
	"SELECT",
	"UPDATE",
	"SET",
	"INSERT",
	"INTO",
	"VALUES",
	"WHERE",
	"COUNT",
	"SUM",
	"MAX",
	"MIN",
	"AVG",
	"DROP",
	"ALTER",
	"CREATE",
	"VIEW",
	"TABLE",
	"AS",
	"AND",
	"OR",
	"ORDER",
	"GROUP",
	"BY",
	"HAVING",
	"IS",
	"NULL",
	"NOT",
	"COMMIT",
	"ROLLBACK",
	"EXISTS",
	"IN",
	NULL
};

static void
put_cursor_at_command (GnomeDbSqlEditorPrivate *priv,
			GnomeDbSqlEditorCommand *cmd)
{
	GtkTextView *tv;
	GtkTextIter iter;
	GtkTextBuffer *buffer;

	if(!priv)
		return;

	if(!priv->text_view)
		return;

	if(!cmd)
		return;

	if(!cmd->sql_statement)
		return;

	buffer = gtk_text_view_get_buffer (GTK_TEXT_VIEW(priv->text_view));
	if(!buffer)
		return;

	gtk_text_buffer_get_start_iter(buffer, &iter);	
	gtk_text_iter_set_offset(&iter, cmd->offset);

	tv = (GtkTextView *) priv->text_view;

	gtk_text_view_scroll_to_iter(tv, &iter, 0.3, FALSE, 0.0, 0.0);
}

static void
apply_text_by_tag_name(GtkTextBuffer *buffer, 
		       const GtkTextIter *start_iter, 
		       gint start, gint end, gchar *tag_name)
{
	GtkTextIter begin_iter, end_iter;
	
	begin_iter = *start_iter;
	end_iter = *start_iter;

	gtk_text_iter_set_line_offset (	&begin_iter, start );							
	gtk_text_iter_set_line_offset (	&end_iter, end );							
	
	gtk_text_buffer_remove_tag_by_name (buffer, "normaltext", 
			&begin_iter, &end_iter);

	gtk_text_buffer_apply_tag_by_name (buffer, tag_name, 
			&begin_iter, &end_iter);	
}

static void
apply_text_by_tag_offsets (GtkTextBuffer *buffer, 
			   GtkTextIter *start_iter, 
			   gint start_offset, gint end_offset, 
			   GtkTextTag *apply_tag,
			   GtkTextTag *remove_tag )
{
	GtkTextIter begin_iter, end_iter;
	
	begin_iter = *start_iter;
	end_iter = *start_iter;

	gtk_text_iter_set_line_offset (	&begin_iter, start_offset );							
	gtk_text_iter_set_line_offset (	&end_iter, end_offset );							
	
	gtk_text_buffer_remove_tag (buffer, remove_tag, 
			&begin_iter, &end_iter);

	gtk_text_buffer_apply_tag (buffer, apply_tag, 
			&begin_iter, &end_iter);	
}

static void
apply_text_by_tag (GtkTextBuffer *buffer, 
		   GtkTextTag *apply_tag,
		   GtkTextTag *remove_tag,
		   GtkTextIter *start_iter, 
		   GtkTextIter *end_iter )
{
	gtk_text_buffer_remove_tag (buffer, remove_tag, 
			start_iter, end_iter);

	gtk_text_buffer_apply_tag (buffer, apply_tag, 
			start_iter, end_iter);	
}

/* is word a SQL keyword? */
static gboolean
is_text_sql(const gchar *text, gint begin, gint end)
{
	gint i;
	gint text_len;

	if(!text)
		return FALSE;

	if(begin < 0)
		return FALSE;

	if(end < 1)
		return FALSE;

	text_len = end - begin;
        if(text_len < 1)
		return FALSE;

	if(*(text) == '\0')
		return FALSE;

	if(*(text + begin) > 0) /* first character is not the NUL terminator */
	{
		for (i = 0; sql_keywords[i] != NULL; i++) 
		{
			if(strlen(sql_keywords[i]) == text_len)
			{
				if( !g_ascii_strncasecmp(
					text + begin, sql_keywords[i],
					text_len ) )
				{
					return TRUE;
				}
			}
		}
	}
	else
		return FALSE;

	return FALSE;
}

/* does the character at offset in the GtkTextIter has
 * this text tag applied?
 */
static gboolean 
char_has_tag(const GtkTextIter *iter, GtkTextTag *tag, gint char_offset_in_line)
{
	GtkTextIter offset_iter;

	offset_iter = *iter;
	gtk_text_iter_set_line_offset (	&offset_iter, char_offset_in_line );							

	return gtk_text_iter_has_tag(&offset_iter, tag);
}

/* this is where SQL syntax hi-lighting takes place
 */
static void
syntax_hi_light_text (GtkTextBuffer *buffer, gpointer user_data)
{
	GtkTextIter start_iter, end_iter, iter, insert_iter;
	GtkTextIter match_start1, match_end1, match_start2, match_end2;
	gint char_count = 0;
	gint hyphen = 0, single_quotes = 0;
	gchar *text = NULL;
	gint i, start_con = 0, end_con = 0, line = 0;
	gint freecomment_count = 0;
	gint start_word = -1;
	GtkTextMark *insert_mark;
	GnomeDbSqlEditorPrivate *priv = NULL;

	/* check arguments */
	if(!buffer)
		return;

	if(!user_data)
		return;

	/* get private data */
	priv = (GnomeDbSqlEditorPrivate *) user_data;

	/* get cursor insertion point to determine if line changed 
	 * to a different line
         */
	insert_mark = gtk_text_buffer_get_insert(buffer);
	gtk_text_buffer_get_iter_at_mark(buffer,&insert_iter,insert_mark);
	line = gtk_text_iter_get_line(&insert_iter);

	/* get the starting and ending text iterators */
	gtk_text_buffer_get_iter_at_offset (buffer, &start_iter, 0);
	char_count = gtk_text_buffer_get_char_count (buffer);
	gtk_text_buffer_get_iter_at_offset (buffer, &end_iter, char_count);

	/* since line is not same - redo all */
	if(line != priv->line_last_changed)
	{
	        /* remove all previously applied tags */
		gtk_text_buffer_remove_all_tags (
			buffer, &start_iter, &end_iter);  

	        /* apply the entire buffer to the normaltext tag */
		gtk_text_buffer_apply_tag (
			buffer, priv->normaltext_tag, &start_iter, &end_iter); 
	}
	else /* just worry about current insertion line */
	{
		/* get start iter */
		if(gtk_text_iter_starts_line(&insert_iter))
		{
			start_iter = insert_iter;
		}
		else
		{
			start_iter = insert_iter;
			gtk_text_iter_set_line_offset(&start_iter,0);
		}
		/* get end iter */	
		gtk_text_iter_forward_to_line_end(&end_iter);
		char_count = gtk_text_iter_get_chars_in_line(&start_iter);

	        /* remove all previously applied tags */
		gtk_text_buffer_remove_all_tags (
			buffer, &start_iter, &end_iter);  

	        /* apply the entire buffer to the normaltext tag */
		gtk_text_buffer_apply_tag (
			buffer, priv->normaltext_tag, &start_iter, &end_iter); 		

		/* get the starting and ending text iterators */
		gtk_text_buffer_get_iter_at_offset (buffer, &start_iter, 0);
		char_count = gtk_text_buffer_get_char_count (buffer);
		gtk_text_buffer_get_iter_at_offset (buffer, &end_iter, char_count);
	}

        /*  ------------------------------------
         *  Free Comments (sort of like c style) 
         *  ------------------------------------
         *  except in SQL, a c like comment occurs within
         *  a SQL statement
         */ 
	while(gtk_text_iter_is_end(&start_iter) == FALSE)
	{	
		if(gtk_text_iter_forward_search(
			&start_iter, "/*", GTK_TEXT_SEARCH_TEXT_ONLY, 
			&match_start1, &match_end1, &end_iter) == TRUE)
		{
		        /* beginning of free comment found */ 
			freecomment_count++;
			if(gtk_text_iter_forward_search(
				&match_end1, "*/", GTK_TEXT_SEARCH_TEXT_ONLY, 
				&match_start2, &match_end2, &end_iter) == TRUE)
			{
				/* ending of free comment found, now hi-light comment */
 				gtk_text_buffer_apply_tag (
					buffer, priv->freecomment_tag, 
					&match_start1, &match_end2);
				gtk_text_iter_forward_chars(&match_end2, 1);
				start_iter = match_end2;
			}
			else
			{
				/* if no end found, hi-light to the end, to let the user know 
                                 * the ending asterisk slash is missing 
                                 */
				apply_text_by_tag (
					buffer, 
					priv->freecomment_tag, 
					priv->normaltext_tag,
					&match_start1, &end_iter);
				break;
			}
		}
		else
			break;
	}

	/* if free comments is different than last time,
	 * invalidate line_last_changed - causes 
	 * a complete redo (instead hi-lighting just the current line -
	 * do the whole buffer)
	 * THIS IS JUST AN ATTEMPT FOR SPEED
	 */
	if(freecomment_count != priv->last_freecomment_count)
	{
		priv->line_last_changed = -1;
	}

        /*********************************************************************
	 * See if the following needs hi-lighting:
	 * - Line Comments (sort of like C++ slash slash comments 
         *   but uses hypen hyphen and it is based at the beginning of a line)
	 * - Single-Quoted Constants ( WHERE COL1 = 'ABC' )
	 * - SQL keywords (SELECT, FROM, WHERE, UPDATE, etc)
	 *********************************************************************/
	if(line != priv->line_last_changed)
	{
		gtk_text_buffer_get_iter_at_offset (buffer, &start_iter, 0);
	}
	else
	{
		if(gtk_text_iter_starts_line(&insert_iter))
		{
			start_iter = insert_iter;
		}
		else
		{
			start_iter = insert_iter;
			gtk_text_iter_set_line_offset(&start_iter,0);
		}
	}

	/* get starting and ending iters and character count of line */       
	char_count = gtk_text_buffer_get_char_count (buffer);
	gtk_text_buffer_get_iter_at_offset (buffer, &end_iter, char_count);

        /* for each line, look for line comments, constants, and keywoards */   
	do 
	{	
		iter = start_iter;
		gtk_text_iter_forward_to_line_end(&iter);
		text = gtk_text_buffer_get_text(buffer, 
			&start_iter, &iter, FALSE);

		/* line comment */
		char_count = gtk_text_iter_get_chars_in_line(&start_iter);
		hyphen = 0; 
		for(i = 0; i < char_count - 1; i++)
		{
			switch( *(text + i) )
			{
				case '-':
					if(hyphen == 1)
					{
						hyphen = 2;
						/* line comment found */
						i = char_count;

						apply_text_by_tag (
							buffer, 
							priv->freecomment_tag, 
							priv->normaltext_tag,
							&start_iter, &iter);
					}
					else
					{
						hyphen = 1;
					}
					break;
				case ' ':
					/* continue */
					break;
				default:
					/* this line is not line commented */
					i = char_count; /* to break out of for loop */
					break;
			}
		}
		/* if not line commented, 
		 * look for singled quoted constants and keywords
                 */
		if(hyphen < 2)
		{
			if(gtk_text_iter_is_end(&start_iter) == TRUE)
				break;
			start_word = -1;
			single_quotes = 0;
			for(i = 0; i < char_count; i++)
			{
				match_start1 = start_iter;
				match_end1 = start_iter;

				if(gtk_text_iter_is_end(&match_end1) == TRUE)
					break;
				if(char_has_tag(&start_iter, priv->freecomment_tag, 
						i) == FALSE)
				{
					if(single_quotes == 0 && start_word == -1)
					{		
						switch( *(text + i) )
						{
							case '\'':
								single_quotes = 1;
								start_con = i + 1;
								break;
							default:
								if(g_ascii_isalpha(*(text + i)))
								{
									start_word = i;
								}
								break;
						}
					}
					else if (single_quotes == 1)
					{
						switch( *(text + i) )
						{
							case '\'':
								/* single quoted constant */
								end_con = i;
		
								/* get starting and
								 * ending of constant 
								 * excluding quotes
								 */
								apply_text_by_tag_offsets(
									buffer,&start_iter,
									start_con, i,
									priv->singlequotedconstant_tag,
									priv->normaltext_tag);

								single_quotes = 0;
								break;
							default:
								break;
							}
					}
					else if(start_word != -1)
					{	/* is character alphabetic, numeric, or '_' */
						if(g_ascii_isalnum(*(text + i)) || (*(text + i) == '_'))
						{
							/* continue */
						}
						else
						{
							/* using start_word and i offsets, get word */
							if(is_text_sql(text, start_word, i) == TRUE)
							{
								/* word is a SQL keyword, hi-light word */							
								apply_text_by_tag_offsets(
									buffer, &start_iter,
									start_word, i,
									priv->sql_tag,
									priv->normaltext_tag);
							}
							start_word = -1;
							switch( *(text + i) )
							{
								case '\'':
									single_quotes = 1;
									start_con = i + 1;
									break;
								default:
									break;
							}
						}

					}
				} 
			}
			if( start_word != -1)
			{
				if (is_text_sql(text, start_word, i) == TRUE)
				{	
					/* word is a SQL keyword, hi-light word */						
					apply_text_by_tag_offsets(
						buffer, &start_iter,
						start_word, i,
						priv->sql_tag,
						priv->normaltext_tag);
				}
			}
		}
		g_free(text);
	} while (gtk_text_iter_forward_line(&start_iter) );
	

	/* POOR ATTEMPTS AT SPEED - last_freecomment_count 
	 * and line_last_changed 
	 */
	priv->last_freecomment_count = freecomment_count;
	priv->line_last_changed = line;
}

/* text buffer signal "changed" 
 */
static void
text_changed_cb (GtkTextBuffer *buffer, gpointer user_data)
{
	GnomeDbSqlEditorPrivate *priv;

	if(!buffer)
		return;

	if(!user_data)
		return;

	priv = (GnomeDbSqlEditorPrivate *) user_data;

	if(priv->use_hi_lighting == TRUE)
	{
		syntax_hi_light_text (buffer, user_data);
	}
}


static void
parse_chars (gchar *text, gchar **end_ptr, gchar **tag, gboolean start)
{
	gint i;
	gchar *next_token;

	*tag = NULL;
	*end_ptr = NULL;

	/* SQL keywords */
	for (i = 0; sql_keywords[i] != NULL; i++) {
		if (!g_ascii_strncasecmp (text, sql_keywords[i], strlen (sql_keywords[i]))) {
			*end_ptr = text + strlen (sql_keywords[i]);
			*tag = "sql";
			return;
		}
	}

	/* not at the start of a TAG, find the next one */
	for (i = 0; sql_keywords[i] != NULL; i++) {
		next_token = strstr (text, sql_keywords[i]);
		if (next_token) {
			if (*end_ptr)
				*end_ptr = (*end_ptr < next_token) ? *end_ptr : next_token;
			else
				*end_ptr = next_token;
		}
	}
}

/* gnome-db sql window's original text_changed_cb */
static void
text_changed_cb2 (GtkTextBuffer *buffer, gpointer user_data)
{
	GtkTextIter start_iter, next_iter, tmp_iter;
	gchar *text, *start_ptr, *end_ptr, *tag;

	GnomeDbSqlEditorPrivate *priv;

	if(!buffer)
		return;

	if(!user_data)
		return;

	priv = (GnomeDbSqlEditorPrivate *) user_data;

	g_return_if_fail (priv != NULL);

	/* parse the text buffer to higlight SQL keywords */
	gtk_text_buffer_get_iter_at_offset (buffer, &start_iter, 0);
	next_iter = start_iter;
	while (gtk_text_iter_forward_line (&next_iter)) {
		gboolean start = TRUE;

		start_ptr = text = gtk_text_iter_get_text (&start_iter, &next_iter);
		do {
			parse_chars (start_ptr, &end_ptr, &tag, start);
			start = FALSE;
			if (end_ptr) {
				tmp_iter = start_iter;
				gtk_text_iter_forward_chars (&tmp_iter, end_ptr - start_ptr);
			}
			else
				tmp_iter = next_iter;

			if (tag) {
				gtk_text_buffer_apply_tag_by_name (
					buffer, tag, &start_iter, &tmp_iter);
			}

			start_iter = tmp_iter;
			start_ptr = end_ptr;
		} while (end_ptr);

		g_free (text);
		start_iter = next_iter;
	}
}

static gboolean
load_editor_from_file(GtkTextBuffer *buffer, gchar *filename)
{
	GtkTextIter iter;
	char text[FILE_BUFFER_SIZE];
	int char_count;
	FILE *infile;

	if(!buffer)
		return FALSE;

	if(!filename)
		return FALSE;

	infile = fopen (filename, "r");
  
	if (infile) 
	{
		gtk_text_buffer_get_iter_at_offset (buffer, 
					&iter, 0);

		while (1)
		{
			char_count = fread (text, 1, 
					FILE_BUFFER_SIZE, infile);

			gtk_text_buffer_insert (buffer, &iter, 
					text, char_count);
	
			if (char_count < FILE_BUFFER_SIZE)
				break;
		}
		fclose (infile);
	}
	else
		return FALSE;

	return TRUE;
}

static gboolean
save_editor_to_file(GtkTextBuffer *buffer, gchar *filename)
{
	GtkTextIter start_iter, end_iter;
	gchar *text;
	gint char_count = 0, char_offset = 0;

	FILE *outfile;

	if(!buffer)
		return FALSE;

	if(!filename)
		return FALSE;

	outfile = fopen (filename, "w");
  
	if (outfile) 
	{
		gtk_text_buffer_get_iter_at_offset (buffer, &start_iter, char_offset);
		char_count = gtk_text_buffer_get_char_count (buffer);
		if(char_count > FILE_BUFFER_SIZE)
			char_count = FILE_BUFFER_SIZE;
		char_offset += char_count;
		gtk_text_buffer_get_iter_at_offset (buffer, &end_iter, char_offset - 1);
		text = gtk_text_buffer_get_text(buffer, 
			&start_iter, &end_iter, FALSE);
		while (1)
		{
			/* strip NUL characters */
			fwrite (text, 1, char_count - 1, outfile);

			if(gtk_text_iter_is_end(&end_iter))
				break;
			if(char_count < FILE_BUFFER_SIZE)
				break;

			gtk_text_buffer_get_iter_at_offset (buffer, &start_iter, char_offset);
			char_count = gtk_text_buffer_get_char_count (buffer);
			if(char_count > FILE_BUFFER_SIZE)
				char_count = FILE_BUFFER_SIZE;
			char_offset += char_count;
			gtk_text_buffer_get_iter_at_offset (buffer, &end_iter, char_offset - 1);
			text = gtk_text_buffer_get_text(buffer, 
				&start_iter, &end_iter, FALSE);

		}
		fclose (outfile);
	}
  	else
		return FALSE;

	return TRUE;
}

/* create_sql_command_list
 *
 * this function takes the text in GtkTextBuffer and
 * creates a GList of GnomeDbSqlEditorCommands which
 * contain each SQL statement and the offset into the GtkTextBuffer.
 *
 * runat is where should the first SQL statement start at:
 *    TRUE  = beginning of the editor
 *    FAKSE = current SQL statement (offset)
 */
static GList *
create_sql_command_list(GnomeDbSqlEditor *sql_editor, 
			GnomeDbSqlEditorCommand *current_cmd)
{
	GList *list = NULL;
	GtkTextBuffer *buffer;
	GnomeDbSqlEditorCommand *cmd;

	GtkTextIter start_iter, end_iter, iter;
	GtkTextIter match_start1, match_end1;
	GtkTextIter statement_begin_iter;
	gint char_count;
	gint hyphen, single_quotes;
	gchar *text;
	gint i, start_con, end_con;

	gint start_word;



	GnomeDbSqlEditorPrivate *priv;
	gboolean other;

	g_return_val_if_fail (GNOME_DB_IS_SQL_EDITOR (sql_editor), NULL);
	priv = sql_editor->priv;

	buffer = gnome_db_sql_editor_get_text_buffer (sql_editor);

	/* parse the buffer for SQL statements into a GList */

	/* if current_cmd exists, then the GList must begin at the
	 * cursor position where a SQL statement is located.  Otherwise,
	 * start at the beginning of the buffer.
	 */
	if(current_cmd != NULL)
	{
		gtk_text_buffer_get_iter_at_offset (buffer, &start_iter, 
			cmd->offset);
	}
	else
	{
		gtk_text_buffer_get_iter_at_offset (buffer, &start_iter, 0);
	}

	/* get starting and ending iters and character count of line */       
	char_count = gtk_text_buffer_get_char_count (buffer);
	gtk_text_buffer_get_iter_at_offset (buffer, &end_iter, char_count);
	statement_begin_iter = start_iter;
	other = FALSE;
        /* for each line, look for line comments, constants, and keywoards */   
	do 
	{	
		iter = start_iter;
		gtk_text_iter_forward_to_line_end(&iter);
		text = gtk_text_buffer_get_text(buffer, 
			&start_iter, &iter, FALSE);

		/* line comment */
		char_count = gtk_text_iter_get_chars_in_line(&start_iter);
		if(hyphen == 2)
			statement_begin_iter = start_iter;
			
		hyphen = 0; 
		
		for(i = 0; i < char_count - 1; i++)
		{
			switch( *(text + i) )
			{
				case '-':
					if(hyphen == 1)
					{
						hyphen = 2;
						/* line comment found */
						i = char_count;
						if(other == TRUE)
						{
							/* parser error -
							 * a line comment
							 * found, but
							 * the last SQL
							 * statment was not
							 * terminated with ;
							 */
							/* create new editor command struct */
							cmd = g_new0(GnomeDbSqlEditorCommand,1);

							/* get text from beginning of
							 * supposed SQL statement 
							 */
							cmd->sql_statement = gtk_text_buffer_get_text(buffer, 
								&statement_begin_iter,
								&start_iter, 
								FALSE);
							/* fill cmd struct */
							cmd->offset = gtk_text_iter_get_offset(
								&start_iter);
							/* using cmd, put cursor at end of
							 * problem SQL statement in text view 
							 */			
							put_cursor_at_command (
								priv,
								cmd);
							/* FIXME: parser error handling */
							gnome_db_sql_editor_debug(sql_editor, "*** parser error begin ***");
							gnome_db_sql_editor_debug(sql_editor, cmd->sql_statement);
							gnome_db_sql_editor_debug(sql_editor, "*** parser error end ***");
							list = g_list_append (list, (gpointer) cmd);
							return list; 
						}
					}
					else
					{
						hyphen = 1;
					}
					break;
				case ' ':
					/* continue */
					break;
				default:
					other = TRUE; /* not a comment nor white space */
					/* this line is not line commented */
					i = char_count; /* to break out of for loop */
					break;
			}
		}
		/* if not line commented, 
		 * look for singled quoted constants and keywords
                 */
		if(hyphen < 2)
		{
			if(gtk_text_iter_is_end(&start_iter) == TRUE)
				break;
			start_word = -1;
			single_quotes = 0;
			for(i = 0; i < char_count; i++)
			{
				match_start1 = start_iter;
				match_end1 = start_iter;

				if(gtk_text_iter_is_end(&match_end1) == TRUE)
					break;
				
				if(char_has_tag(&start_iter, priv->freecomment_tag, 
						i) == FALSE)
				{
					if(single_quotes == 0 && start_word == -1)
					{		
						switch( *(text + i) )
						{
							case '\'':
								single_quotes = 1;
								start_con = i + 1;
								break;
							case ';':
								/* end of SQL statement */												
								/* create new editor command struct */
								cmd = g_new0(GnomeDbSqlEditorCommand,1);

								/* get text from beginning of
								 * supposed SQL statement 
								 */
								match_end1 = start_iter;
								gtk_text_iter_set_line_offset(&match_end1,
										i + 1);
								cmd->sql_statement = gtk_text_buffer_get_text(buffer, 
									&statement_begin_iter,
									&match_end1, 
									FALSE);

								/* fill cmd struct */
								cmd->offset = gtk_text_iter_get_offset(
									&match_end1);
								gnome_db_sql_editor_debug(sql_editor, "*** parsed SQL begin ***");
								gnome_db_sql_editor_debug(sql_editor, cmd->sql_statement);
								gnome_db_sql_editor_debug(sql_editor, "*** parsed SQL end ***");
								/* add cmd struct to list */
								list = g_list_append(
									list, 
									(gpointer) cmd);
								other = FALSE;
								statement_begin_iter = match_end1;
								break;
							default:
								if(g_ascii_isalpha(*(text + i)))
								{
									start_word = i;
								}
								break;
						}
					}
					else if (single_quotes == 1)
					{
						switch( *(text + i) )
						{
							case '\'':
								/* single quoted constant */
								end_con = i;
		
								/* get starting and
								 * ending of constant 
								 * excluding quotes
								 */
								single_quotes = 0;
								break;
							default:
								break;
							}
					}
					else if(start_word != -1)
					{	/* alphabetic, numeric, or '_' */
						if(g_ascii_isalnum(*(text + i)) || (*(text + i) == '_'))
						{
							/* continue */
						}
						else
						{
							/* using start_word and i offsets, get word */
							if(is_text_sql(text, start_word,i) == TRUE)
							{
								/* word is a SQL keyword, hi-light word */							
							}
							start_word = -1;
							switch( *(text + i) )
							{
								case '\'':
									single_quotes = 1;
									start_con = i + 1;
									break;
								case ';':
									/* end of SQL statement */												
									/* create new editor command struct */
									cmd = g_new0(GnomeDbSqlEditorCommand,1);	

									/* get text from beginning of
									 * supposed SQL statement 
									 */
									match_end1 = start_iter;
									gtk_text_iter_set_line_offset(&match_end1,
										i + 1);
									cmd->sql_statement = gtk_text_buffer_get_text(buffer, 
										&statement_begin_iter,
										&match_end1, 
										FALSE);

									/* fill cmd struct */
									cmd->offset = gtk_text_iter_get_offset(
										&match_end1);
									gnome_db_sql_editor_debug(sql_editor, "*** parsed SQL begin ***");
									gnome_db_sql_editor_debug(sql_editor, cmd->sql_statement);
									gnome_db_sql_editor_debug(sql_editor, "*** parsed SQL end ***");
									/* add cmd struct to list */
									list = g_list_append(
										list, 
										(gpointer) cmd);
									other = FALSE;
									statement_begin_iter = match_end1;
									break;
								default:
									break;
							}
						}

					}
				} 
			}
			if( start_word != -1)
			{

			}
		}
		g_free(text);
	} while (gtk_text_iter_forward_line(&start_iter));

	return list;
}

static void
setup_sql_editor (GnomeDbSqlEditor *sql_editor)
{
	GtkTextBuffer *buffer;

	g_return_if_fail (GNOME_DB_IS_SQL_EDITOR (sql_editor));
	
	/* fill private struct for GnomeDbSqlEditor */
	sql_editor->priv->line_last_changed = -1, 
	sql_editor->priv->last_freecomment_count = 0;
   
	buffer = gtk_text_view_get_buffer (
			GTK_TEXT_VIEW (
				sql_editor->priv->text_view));     

        /* --------- Text Tag definitions ------- */
        
        /* SQL Keywords - SELECT FROM WHERE, etc */   
	sql_editor->priv->sql_tag = gtk_text_buffer_create_tag (
				buffer, "sql",
				    "foreground", "blue",
				    "weight", PANGO_WEIGHT_NORMAL,
				    "style", PANGO_STYLE_NORMAL,
                                    "scale", PANGO_SCALE_LARGE,
				    "family", sql_editor->priv->family,
				NULL);

        /* anything else is normaltext */
	sql_editor->priv->normaltext_tag = gtk_text_buffer_create_tag (
				buffer, "normaltext",
				    "foreground", "black",
				    "weight", PANGO_WEIGHT_NORMAL,
				    "style", PANGO_STYLE_NORMAL,
                                    "scale", PANGO_SCALE_LARGE,
				    "family", sql_editor->priv->family,
				NULL);

        /* c like free comment - used within a SQL statement */
	sql_editor->priv->freecomment_tag = gtk_text_buffer_create_tag (
				buffer, "freecomment",
				    "foreground", "darkgreen",
				    "weight", PANGO_WEIGHT_LIGHT,
				    "style", PANGO_STYLE_ITALIC,
                        /*            "scale", PANGO_SCALE_LARGE,  */
				    "family", sql_editor->priv->family,
				NULL);

        /* c++ like line comment, but using two hyphens */
	sql_editor->priv->linecomment_tag = gtk_text_buffer_create_tag (
				buffer, "linecomment",
				    "foreground", "darkgreen",
				    "weight", PANGO_WEIGHT_LIGHT,
				    "style", PANGO_STYLE_ITALIC,
                                    "scale", PANGO_SCALE_LARGE, 
				    "family", sql_editor->priv->family,
				NULL);

        /* single quoted constant - WHERE COL1 = 'ABC' */
	sql_editor->priv->singlequotedconstant_tag = gtk_text_buffer_create_tag (
				buffer, "singlequotedconstant",
				    "foreground", "red", 
				    "weight", PANGO_WEIGHT_NORMAL,
				    "style", PANGO_STYLE_NORMAL,
                                    "scale", PANGO_SCALE_LARGE, 
				    "family", sql_editor->priv->family,
				NULL);

	/* internal signals */
	g_signal_connect (G_OBJECT (buffer),
		"changed", G_CALLBACK (text_changed_cb), sql_editor->priv);
}

static void
gnome_db_sql_editor_finalize (GObject *object)
{
	GnomeDbSqlEditor *sql_editor = (GnomeDbSqlEditor *) object;

	g_return_if_fail (GNOME_DB_IS_SQL_EDITOR (sql_editor));

	/* free memory */
	g_free (sql_editor->priv->family);
	g_free (sql_editor->priv);
	sql_editor->priv = NULL;

	parent_class->finalize (object);
}

static void
gnome_db_sql_editor_class_init (GnomeDbSqlEditorClass *klass)
{
	GObjectClass *object_class = G_OBJECT_CLASS (klass);

	parent_class = g_type_class_peek_parent (klass);

	object_class->finalize = gnome_db_sql_editor_finalize;
}

static void
gnome_db_sql_editor_init (GnomeDbSqlEditor *sql_editor, GnomeDbSqlEditorClass *klass)
{
	g_return_if_fail (GNOME_DB_IS_SQL_EDITOR (sql_editor));

	/* allocate the internal structure */
	sql_editor->priv = g_new0 (GnomeDbSqlEditorPrivate, 1);

	/* set up widgets */
	sql_editor->priv->scroll = gtk_scrolled_window_new (NULL, NULL);
	gtk_box_pack_start (GTK_BOX (sql_editor), 
			sql_editor->priv->scroll, 1, 1, 0);

	sql_editor->priv->text_view = gtk_text_view_new ();
	gtk_container_add (GTK_CONTAINER (sql_editor->priv->scroll), 
				sql_editor->priv->text_view);

	/* fill any settings - default */
	sql_editor->priv->use_hi_lighting = TRUE;
	sql_editor->priv->family = g_strdup("courier"); /* font family */

	/* for debugging */
	sql_editor->priv->debug_text_view = NULL;

	/* set up the sql editor */
	setup_sql_editor (sql_editor);
}

/**
 * gnome_db_sql_editor_new
 *
 * Creates a new #GnomeDbSqlEditor widget.  
 *
 * The #GnomeDbSqlEditor widget
 * is a scrollable text view widget with syntax hi-lighting for SQL
 * and allows for parsing of the text buffer into SQL statements
 * that can be executed or retrieved into a list. 
 *
 * Returns: a #GtkWdiget that is a #GnomeDbSqlEditor.
 */
GtkWidget *
gnome_db_sql_editor_new (void)
{
	GnomeDbSqlEditor *sql_editor;

	sql_editor = g_object_new (GNOME_DB_TYPE_SQL_EDITOR, NULL);
	return GTK_WIDGET (sql_editor);
}

GType
gnome_db_sql_editor_get_type (void)
{
	static GType type = 0;

	if (!type) {
		static const GTypeInfo info = {
			sizeof (GnomeDbSqlEditorClass),
			(GBaseInitFunc) NULL,
			(GBaseFinalizeFunc) NULL,
			(GClassInitFunc) gnome_db_sql_editor_class_init,
			NULL,
			NULL,
			sizeof (GnomeDbSqlEditor),
			0,
			(GInstanceInitFunc) gnome_db_sql_editor_init
		};
		type = g_type_register_static (PARENT_TYPE, "GnomeDbSqlEditor", &info, 0);
	}
	return type;
}

/**
 * gnome_db_sql_editor_get_all_commands
 * @sql_editor: a #GnomeDbSqlEditor widget.
 * @run_at_pref: a #gboolean indicating where to start the
 *               listing of SQL statements:
 *               TRUE = beginning of text view 
 *               FALSE = current SQL statement
 *
 * Returns a list of the SQL statements from a
 * #GnomeDbSqlEditor widget. The returned value is a list of gchar strings
 * which represent each of SQL statement.  
 *
 * Returns: a #GList of #GnomeDbSqlEditorCommand. This list should 
 * be freed (by calling #g_list_free) when no longer needed, 
 * #g_free each #GnomeDbSqlEditorCommand, and #g_free
 * each #gchar *sql_command.)
 */
GList
*gnome_db_sql_editor_get_all_commands (GnomeDbSqlEditor *sql_editor,
						gboolean run_at_pref )
{
	GList *list;
	GnomeDbSqlEditorCommand *cmd;
	g_return_val_if_fail (GNOME_DB_IS_SQL_EDITOR (sql_editor),NULL);

	if(run_at_pref == FALSE)
	{
		cmd = gnome_db_sql_editor_get_command_at_cursor(sql_editor);
		if(cmd == NULL)
		{
			/* parser error */
			return NULL;
		}
		else
		{
			list = create_sql_command_list(sql_editor, cmd);
			if(!list)
				return NULL; /* Error in parsing SQL statements */
		}
	}
	else
	{
		list = create_sql_command_list(sql_editor, NULL);
		if(!list)
			return NULL; /* Error in parsing SQL statements */
	}

	return list;
}

/**
 * gnome_db_sql_editor_foreach_command
 * @sql_editor: a #GnomeDbSqlEditor widget.
 * @run_at_pref: a gboolean where indicating where to start the
 *               running of SQL statements:
 *               TRUE = beginning of text view 
 *               FALSE = current SQL statement
 * @run_command: a function pointer to the function to call for each SQL 
 * statement.  This function must be the function type #GnomeDbSqlEditorRunFunc
 * @user_data: optionally any user_data you want to pass along, otherwise NULL.
 *
 * The idea use of this function is to call a function to execute each
 * SQL statement.  The run_command function should return TRUE on success
 * and FALSE on failure.  This is so function 
 * #gnome_db_sql_editor_foreach_command can break on an error.
 *
 * Returns: a #gboolean indicating success or failure.
 * TRUE indicating success while FALSE indicating failure.
 *
 */
gboolean
gnome_db_sql_editor_foreach_command (GnomeDbSqlEditor *sql_editor,
			gboolean run_at_pref, 
			GnomeDbSqlEditorRunFunc run_command, 
			gpointer user_data)
{
	GList *list;
	char *sql;
	gboolean result;
	GnomeDbSqlEditorCommand *cmd;

	g_return_val_if_fail (GNOME_DB_IS_SQL_EDITOR (sql_editor),FALSE);

	g_return_val_if_fail (run_command != NULL, FALSE);

	if(run_at_pref == FALSE)
	{
		cmd = gnome_db_sql_editor_get_command_at_cursor(sql_editor);
		if(cmd == NULL)
		{
			/* parser error */
			return FALSE;
		}
		else
		{
			list = create_sql_command_list(sql_editor, cmd);
			if(!list)
				return FALSE; /* Error in parsing SQL statements */
		}
	}
	else
	{
		list = create_sql_command_list(sql_editor, NULL);
		if(!list)
			return FALSE; /* Error in parsing SQL statements */
	}

	while (list)
	{
		GList *next = list->next;

		/* a result of FALSE indicates failure, and thus must stop */

		cmd = (GnomeDbSqlEditorCommand *) list->data;
		g_return_val_if_fail (cmd != NULL, FALSE);

		sql = (gchar *) cmd->sql_statement;
		g_return_val_if_fail (sql != NULL, FALSE);

		put_cursor_at_command (sql_editor->priv, cmd);

		result = (*run_command) (cmd, user_data);
		if(result == FALSE)
		{
			return FALSE;
		}
		else
		{
			list = next;
		}
	}
	return TRUE;
}

/**
 * gnome_db_sql_editor_get_command_at_cursor
 * @sql_editor: a #GnomeDbSqlEditor widget.
 *
 * Get the SQL statement at the cursor.
 *
 * Returns: a #GnomeDbSqlEditorCommand that contains the SQL statement
 * and offset of the SQL statement in the editor that is at the cursor.
 *
 */
GnomeDbSqlEditorCommand *
gnome_db_sql_editor_get_command_at_cursor (GnomeDbSqlEditor *sql_editor)
{
	GtkTextIter start_iter, end_iter, iter, insert_iter, statement_begin_iter;
	GtkTextIter match_start1, match_end1;
	gint char_count;
	gint hyphen, single_quotes;
	gchar *text;
	gint i, start_con, end_con, line;
	gint start_word = -1;
	GtkTextMark *insert_mark;
	GnomeDbSqlEditorPrivate *priv;
	GnomeDbSqlEditorCommand *cmd;
	GtkTextIter begin_statement_iter, end_statement_iter;
	gboolean begin_found = FALSE, end_found = FALSE;
	GtkTextBuffer *buffer;
	gboolean other = FALSE;

	g_return_val_if_fail (GNOME_DB_IS_SQL_EDITOR (sql_editor),FALSE);

	priv = sql_editor->priv;

	/* -- ----------------------------- --
	 * -- get cursor location in editor --
	 * -- ----------------------------- --
	 */
	buffer = gnome_db_sql_editor_get_text_buffer (sql_editor);

	insert_mark = gtk_text_buffer_get_insert(buffer);
	gtk_text_buffer_get_iter_at_mark(buffer,&insert_iter,insert_mark);
	line = gtk_text_iter_get_line(&insert_iter);

	start_iter = insert_iter;
	
	/* -- -------------------------------------------------------
	 * -- look for semi-colon - indicates end of SQL statement -- 
	 * -- -------------------------------------------------------
	 */

	/* get starting and ending iters and character count of line */       
	char_count = gtk_text_buffer_get_char_count (buffer);
	gtk_text_buffer_get_iter_at_offset (buffer, &end_iter, char_count);
	statement_begin_iter = start_iter;
	other = FALSE;

        /* for each line, look for line comments, constants, and keywoards */   
		iter = start_iter;
		gtk_text_iter_forward_to_line_end(&iter);
		text = gtk_text_buffer_get_text(buffer, 
			&start_iter, &iter, FALSE);

		/* line comment */
		char_count = gtk_text_iter_get_chars_in_line(&start_iter);
		if(hyphen == 2)
			statement_begin_iter = start_iter;
			
		hyphen = 0; 
		
		for(i = 0; i < char_count - 1; i++)
		{
			switch( *(text + i) )
			{
				case '-':
					if(hyphen == 1)
					{
						hyphen = 2;
						/* line comment found */
						i = char_count;
						if(other == TRUE)
						{
							/* parser error -
							 * a line comment
							 * found (user error)
							 */
							/* FIXME: do proper 
							 * user error handling 
							 */
							g_free(text);
							return FALSE;
						}
					}
					else
					{
						hyphen = 1;
					}
					break;
				case ' ':
					/* continue */
					break;
				default:
					other = TRUE; /* not a comment nor white space */
					/* this line is not line commented */
					i = char_count; /* to break out of for loop */
					break;
			}
		}
		/* if not line commented, 
		 * look for singled quoted constants and keywords
                 */
		if(hyphen < 2)
		{
			if(gtk_text_iter_is_end(&start_iter) == FALSE)
			{
			
			start_word = -1;
			single_quotes = 0;
			for(i = 0; i < char_count; i++)
			{
				match_start1 = start_iter;
				match_end1 = start_iter;

				if(gtk_text_iter_is_end(&match_end1) == TRUE)
					break;
				
				if(char_has_tag(&start_iter, priv->freecomment_tag, 
						i) == FALSE)
				{
					if(single_quotes == 0 && start_word == -1)
					{		
						switch( *(text + i) )
						{
							case '\'':
								single_quotes = 1;
								start_con = i + 1;
								break;
							case ';':
								/* end of SQL statement */												
								gtk_text_iter_set_line_offset(&end_statement_iter,
										i + 1);
								end_found = TRUE;
								i = char_count;
								break;
							default:
								if(g_ascii_isalpha(*(text + i)))
								{
									start_word = i;
								}
								break;
						}
					}
					else if (single_quotes == 1)
					{
						switch( *(text + i) )
						{
							case '\'':
								/* single quoted constant */
								end_con = i;
		
								/* get starting and
								 * ending of constant 
								 * excluding quotes
								 */
								single_quotes = 0;
								break;
							default:
								break;
							}
					}
					else if(start_word != -1)
					{	/* alphabetic, numeric, or '_' */
						if(g_ascii_isalnum(*(text + i)) || (*(text + i) == '_'))
						{
							/* continue */
						}
						else
						{
							/* using start_word and i offsets, get word */
							if(is_text_sql(text, start_word,i) == TRUE)
							{
								/* word is a SQL keyword, hi-light word */							
							}
							start_word = -1;
							switch( *(text + i) )
							{
								case '\'':
									single_quotes = 1;
									start_con = i + 1;
									break;
								case ';':
									gtk_text_iter_set_line_offset(&end_statement_iter,
										i + 1);
									end_found = TRUE;
									i = char_count;
									break;
								default:
									break;
							}
						}

					}
				} 
			}
			}
			if( start_word != -1)
			{

			}
		}
		g_free(text);

	/* -- ------------------------------------------------------------- 
	 * -- if ; not found, we need to find it - go forward to find it -- 
	 * -- -------------------------------------------------------------
	 */
	if(end_found == FALSE)
	{

	/* get starting and ending iters and character count of line */       
	char_count = gtk_text_buffer_get_char_count (buffer);
	gtk_text_buffer_get_iter_at_offset (buffer, &end_iter, char_count);
	statement_begin_iter = start_iter;
	other = FALSE;

        /* for each line, look for line comments, constants, and keywoards */   
	gtk_text_iter_forward_line(&start_iter);
	do 
	{	
		iter = start_iter;
		gtk_text_iter_forward_to_line_end(&iter);
		text = gtk_text_buffer_get_text(buffer, 
			&start_iter, &iter, FALSE);

		/* line comment */
		char_count = gtk_text_iter_get_chars_in_line(&start_iter);
		if(hyphen == 2)
			statement_begin_iter = start_iter;
			
		hyphen = 0; 
		
		for(i = 0; i < char_count - 1; i++)
		{
			switch( *(text + i) )
			{
				case '-':
					if(hyphen == 1)
					{
						hyphen = 2;
						/* line comment found */
						i = char_count;
						if(other == TRUE)
						{
							/* parser error -
							 * a line comment
							 * found, but
							 * the last SQL
							 * statment was not
							 * terminated with ;
							 */
							/* create new editor command struct */
							cmd = g_new0(GnomeDbSqlEditorCommand,1);

							/* get text from beginning of
							 * supposed SQL statement 
							 */
							cmd->sql_statement = gtk_text_buffer_get_text(buffer, 
								&statement_begin_iter,
								&start_iter, 
								FALSE);
							/* fill cmd struct */
							cmd->offset = gtk_text_iter_get_offset(
								&start_iter);
							/* using cmd, put cursor at end of
							 * problem SQL statement in text view 
							 */			
							put_cursor_at_command (
								priv,
								cmd);
							/* FIXME: parser error handling */
							gnome_db_sql_editor_debug(sql_editor, "*** parser error begin ***");
							gnome_db_sql_editor_debug(sql_editor, cmd->sql_statement);
							gnome_db_sql_editor_debug(sql_editor, "*** parser error end ***");
							return cmd;
						}
					}
					else
					{
						hyphen = 1;
					}
					break;
				case ' ':
					/* continue */
					break;
				default:
					other = TRUE; /* not a comment nor white space */
					/* this line is not line commented */
					i = char_count; /* to break out of for loop */
					break;
			}
		}
		/* if not line commented, 
		 * look for singled quoted constants and keywords
                 */
		if(hyphen < 2)
		{
			if(gtk_text_iter_is_end(&start_iter) == FALSE)
			{
			
			start_word = -1;
			single_quotes = 0;
			for(i = 0; i < char_count; i++)
			{
				match_start1 = start_iter;
				match_end1 = start_iter;

				if(gtk_text_iter_is_end(&match_end1) == TRUE)
					break;
				
				if(char_has_tag(&start_iter, priv->freecomment_tag, 
						i) == FALSE)
				{
					if(single_quotes == 0 && start_word == -1)
					{		
						switch( *(text + i) )
						{
							case '\'':
								single_quotes = 1;
								start_con = i + 1;
								break;
							case ';':
								/* end of SQL statement */												
								gtk_text_iter_set_line_offset(&end_statement_iter,
										i + 1);
								end_found = TRUE;
								i = char_count;
								break;
							default:
								if(g_ascii_isalpha(*(text + i)))
								{
									start_word = i;
								}
								break;
						}
					}
					else if (single_quotes == 1)
					{
						switch( *(text + i) )
						{
							case '\'':
								/* single quoted constant */
								end_con = i;
		
								/* get starting and
								 * ending of constant 
								 * excluding quotes
								 */
								single_quotes = 0;
								break;
							default:
								break;
							}
					}
					else if(start_word != -1)
					{	/* alphabetic, numeric, or '_' */
						if(g_ascii_isalnum(*(text + i)) || (*(text + i) == '_'))
						{
							/* continue */
						}
						else
						{
							/* using start_word and i offsets, get word */
							if(is_text_sql(text, start_word,i) == TRUE)
							{
								/* word is a SQL keyword, hi-light word */							
							}
							start_word = -1;
							switch( *(text + i) )
							{
								case '\'':
									single_quotes = 1;
									start_con = i + 1;
									break;
								case ';':
									/* end of SQL statement */												
									gtk_text_iter_set_line_offset(&end_statement_iter,
											i + 1);
									end_found = TRUE;
									i = char_count;
									break;
								default:
									break;
							}
						}

					}
				} 
			}
			}
			if( start_word != -1)
			{

			}
		}
		g_free(text);
	} while (gtk_text_iter_forward_line(&start_iter) &&
		end_found == FALSE);

	}

	/* -- ---------------------------------------------------------------
	 * -- go backward to find the beginning of the SQL statement by 
         * -- finding the ending of another SQL statement (; found) 
	 * -- or line comment found
	 * -- ---------------------------------------------------------------
         */
	/* get starting and ending iters and character count of line */       
	char_count = gtk_text_buffer_get_char_count (buffer);
	gtk_text_buffer_get_iter_at_offset (buffer, &end_iter, char_count);
	statement_begin_iter = start_iter;
	other = FALSE;

        /* for each line, look for line comments, constants, and keywoards */   
	gtk_text_iter_backward_line(&start_iter);
	do 
	{	
		iter = start_iter;
		gtk_text_iter_forward_to_line_end(&iter);
		text = gtk_text_buffer_get_text(buffer, 
			&start_iter, &iter, FALSE);

		/* line comment */
		char_count = gtk_text_iter_get_chars_in_line(&start_iter);
		if(hyphen == 2)
			statement_begin_iter = start_iter;
			
		hyphen = 0; 
		
		for(i = 0; i < char_count - 1; i++)
		{
			switch( *(text + i) )
			{
				case '-':
					if(hyphen == 1)
					{
						hyphen = 2;
						/* line comment found */
						i = char_count;
						if(other == TRUE)
						{
							/* beginning found
							 * now, get the next line
							 * since we don't won't
							 * to include the
							 * line commented line
							 * in the SQL statement
							 */
							gtk_text_iter_forward_line(&start_iter);
							begin_statement_iter = start_iter;
							begin_found = TRUE;
							i = char_count;
						}
					}
					else
					{
						hyphen = 1;
					}
					break;
				case ' ':
					/* continue */
					break;
				default:
					other = TRUE; /* not a comment nor white space */
					/* this line is not line commented */
					i = char_count; /* to break out of for loop */
					break;
			}
		}
		/* if not line commented, 
		 * look for singled quoted constants and keywords
                 */
		if(hyphen < 2)
		{
			if(gtk_text_iter_is_end(&start_iter) == TRUE)
				break;
			start_word = -1;
			single_quotes = 0;
			for(i = 0; i < char_count; i++)
			{
				match_start1 = start_iter;
				match_end1 = start_iter;

				if(gtk_text_iter_is_end(&match_end1) == TRUE)
					break;
				
				if(char_has_tag(&start_iter, priv->freecomment_tag, 
						i) == FALSE)
				{
					if(single_quotes == 0 && start_word == -1)
					{		
						switch( *(text + i) )
						{
							case '\'':
								single_quotes = 1;
								start_con = i + 1;
								break;
							case ';':
								/* end of another
								 * SQL statement 
								 * or in this case
								 * the beginning
								 * the SQL statement
								 * we were trying
								 * to find.
								 */
								gtk_text_iter_forward_line(&start_iter);
								begin_statement_iter = start_iter;
								begin_found = TRUE;
								i = char_count;

								break;
							default:
								if(g_ascii_isalpha(*(text + i)))
								{
									start_word = i;
								}
								break;
						}
					}
					else if (single_quotes == 1)
					{
						switch( *(text + i) )
						{
							case '\'':
								/* single quoted constant */
								end_con = i;
		
								/* get starting and
								 * ending of constant 
								 * excluding quotes
								 */
								single_quotes = 0;
								break;
							default:
								break;
							}
					}
					else if(start_word != -1)
					{	/* alphabetic, numeric, or '_' */
						if(g_ascii_isalnum(*(text + i)) || (*(text + i) == '_'))
						{
							/* continue */
						}
						else
						{
							/* using start_word and i offsets, get word */
							if(is_text_sql(text, start_word,i) == TRUE)
							{
								/* word is a SQL keyword, hi-light word */							
							}
							start_word = -1;
							switch( *(text + i) )
							{
								case '\'':
									single_quotes = 1;
									start_con = i + 1;
									break;
								case ';':
									gtk_text_iter_forward_line(&start_iter);
									begin_statement_iter = start_iter;
									i = char_count;
									begin_found = TRUE;
									break;
								default:
									break;
							}
						}

					}
				} 
			}
			if( start_word != -1)
			{

			}
		}
		g_free(text);
	} while (gtk_text_iter_backward_line(&start_iter) &&
		begin_found == FALSE);

	/* -- ---------------------------------------------------------- --
	 * -- Final Results: either we have SQL statement or we don't    --
	 * -- ---------------------------------------------------------- --
	 */   
	if(begin_found == TRUE && end_found == TRUE)
	{
		/* we 'should' have ourselves the currsent SQL statement here */
		cmd = g_new0(GnomeDbSqlEditorCommand,1);

		cmd->sql_statement = gtk_text_buffer_get_text(buffer, 
			&begin_statement_iter,
			&end_statement_iter,
			FALSE);

		/* fill cmd struct */
		cmd->offset = gtk_text_iter_get_offset(
				&end_statement_iter);

		gnome_db_sql_editor_debug(sql_editor, "*** parsed cursor SQL begin ***");
		gnome_db_sql_editor_debug(sql_editor, cmd->sql_statement);
		gnome_db_sql_editor_debug(sql_editor, "*** parsed cursor SQL end ***");
	}
	else
	{
		/* no SQL statement found at current cursor position */
		gnome_db_sql_editor_debug(sql_editor, "*** no cursor SQL found ***");
		return NULL;
	}

	return cmd;
}

/**
 * gnome_db_sql_editor_get_text_view
 * @sql_editor: a #GnomeDbSqlEditor widget.
 *
 * Returns: a #GtkTextView that's in a #GnomeDbSqlEditor widget.
 *
 */
GtkTextView *
gnome_db_sql_editor_get_text_view (GnomeDbSqlEditor *sql_editor)
{
	g_return_val_if_fail (GNOME_DB_IS_SQL_EDITOR (sql_editor),NULL);

	return GTK_TEXT_VIEW(sql_editor->priv->text_view);
}

/**
 * gnome_db_sql_editor_get_text_buffer
 * @sql_editor: a #GnomeDbSqlEditor widget.
 *
 * Returns: the #GtkTextBuffer that's in the #GtkTextView that's in the
 * #GnomeDbSqlEditor widget. 
 *
 */
GtkTextBuffer *
gnome_db_sql_editor_get_text_buffer (GnomeDbSqlEditor *sql_editor)
{
	GtkTextBuffer* buffer = NULL;

	g_return_val_if_fail (GNOME_DB_IS_SQL_EDITOR (sql_editor),NULL);

	buffer = gtk_text_view_get_buffer (GTK_TEXT_VIEW (sql_editor->priv->text_view));

	return buffer;
}

/**
 * gnome_db_sql_editor_load_from_file
 * @sql_editor: a #GnomeDbSqlEditor widget.
 * @filename: the file name to load the text from into the #GnomeDbSqlEditor.
 *
 * Returns the #gboolean to indicate success or failure.
 * TRUE is successful while FALSE is failure.
 *
 */
gboolean
gnome_db_sql_editor_load_from_file (GnomeDbSqlEditor *sql_editor, gchar *filename)
{
	GtkTextBuffer *buffer;

	g_return_val_if_fail (GNOME_DB_IS_SQL_EDITOR (sql_editor), FALSE);

	if(!filename)
		return FALSE;

	buffer = gnome_db_sql_editor_get_text_buffer (sql_editor);
	if(!buffer)
		return FALSE;

	return load_editor_from_file(buffer, filename);
}

/**
 * gnome_db_sql_editor_save_to_file
 * @sql_editor: a #GnomeDbSqlEditor widget.
 * @filename: a filename
 *
 * Returns the #gboolean indicating success or failure.
 *
 */
gboolean
gnome_db_sql_editor_save_to_file (GnomeDbSqlEditor *sql_editor, gchar *filename)
{
	GtkTextBuffer *buffer;

	g_return_val_if_fail (GNOME_DB_IS_SQL_EDITOR (sql_editor),FALSE);

	if(!filename)
		return FALSE;

	buffer = gnome_db_sql_editor_get_text_buffer (sql_editor);
	if(!buffer)
		return FALSE;

	return save_editor_to_file(buffer, filename);
}

/**
 * gnome_db_sql_editor_use_syntax_hi_lighting
 * @sql_editor: a #GnomeDbSqlEditor widget.
 * @setting: a #gboolean indicating:
 *    TRUE = use syntax hi-lighting
 *    FALSE = do not use syntax hi-lighting
 *
 */
void
gnome_db_sql_editor_use_syntax_hi_lighting (GnomeDbSqlEditor *sql_editor, 
					gboolean setting)
{
	GtkTextBuffer *buffer;
	GtkTextIter start_iter, end_iter;
	gint char_count;
	
	g_return_if_fail (GNOME_DB_IS_SQL_EDITOR (sql_editor));

	if(!sql_editor->priv)
		return;

	sql_editor->priv->use_hi_lighting = setting;

	buffer = gnome_db_sql_editor_get_text_buffer (sql_editor);

	if(setting == TRUE)
	{
		/* hi-light */
		syntax_hi_light_text (buffer, (gpointer) sql_editor->priv);
	}
	else
	{
		/* un hi-light */
		gtk_text_buffer_get_iter_at_offset (buffer, &start_iter, 0);
		char_count = gtk_text_buffer_get_char_count (buffer);
		gtk_text_buffer_get_iter_at_offset (buffer, &end_iter, char_count);
		gtk_text_buffer_remove_all_tags (
			buffer, &start_iter, &end_iter);  
	}	
}

/**
 * gnome_db_sql_editor_set_editable
 * @sql_editor: a #GnomeDbSqlEditor widget.
 * @setting: a #gboolean idicating editable or not editable.
 *
 * Sets a #GnomeDbSqlEditor to editable or not.
 * TRUE = editable 
 * FALSE = not editable
 *
 * When a #GnomeDbSqlEditor is created, its default editable state is editable.
 */
void
gnome_db_sql_editor_set_editable (GnomeDbSqlEditor *sql_editor, gboolean setting)
{
	g_return_if_fail (GNOME_DB_IS_SQL_EDITOR (sql_editor));

	gtk_text_view_set_editable (GTK_TEXT_VIEW(sql_editor->priv->text_view),
					setting);
}

/**
 * gnome_db_sql_editor_set_text
 * @sql_editor: a #GnomeDbSqlEditor widget.
 * @text: UTF-8 text to insert
 * @len: length of #text in bytes.
 *
 * Deletes current contents of #buffer and inserts #text instead.
 * If #len is -1, #text must be nul-terminated. #text must be valid UTF-8.
 *
 * This is just a wrapper call to gtk_text_buffer_set_text.
 */
void
gnome_db_sql_editor_set_text (GnomeDbSqlEditor *sql_editor, 
					gchar *text, gint len)
{
	GtkTextBuffer *buffer;

	g_return_if_fail (GNOME_DB_IS_SQL_EDITOR (sql_editor));

	g_return_if_fail (text != NULL);

	buffer = gnome_db_sql_editor_get_text_buffer (sql_editor);

	gtk_text_buffer_set_text (buffer, text, len);
}

/**
 * gnome_db_sql_editor_insert_text
 * @sql_editor: a #GnomeDbSqlEditor widget.
 * @iter: #GtkTextIter representing a position in the buffer
 * @text: UTF-8 text to insert
 * @len: length of #text in bytes.
 *
 * Inserts len bytes of text at position iter.  If len is -1, text must be
 * nul-terminated and will be inserted in its entirety.  Emits the
 * "insert_text" signal; insertion actually occurs in the default handler
 * for the signal.  inter is invalidated when isertion occurs (because the
 * buffer contents change), but the default signal handler revalidates it to
 * point to the end of the inserted text.
 *
 * This is just a wrapper call to gtk_text_buffer_insert.
 */
void
gnome_db_sql_editor_insert_text (GnomeDbSqlEditor *sql_editor,
				 GtkTextIter *iter, 
				 gchar *text, gint len)
{
	GtkTextBuffer *buffer;

	g_return_if_fail (GNOME_DB_IS_SQL_EDITOR (sql_editor));

	g_return_if_fail (iter != NULL);

	g_return_if_fail (text != NULL);

	buffer = gnome_db_sql_editor_get_text_buffer (sql_editor);

	gtk_text_buffer_insert (buffer, iter, text, len);
}

/**
 * gnome_db_sql_editor_get_text
 * @sql_editor: a #GnomeDbSqlEditor widget.
 * @start: start of range in the #GtkTextBuffer
 * @end: end of range in the #GtkTextBuffer
 *
 * Returns the text in the range [start,end]. 
 *
 * Returns: an allocated UTF-8 encoded string.
 *
 * This is a wrapper call to #gtk_text_buffer_get_text.
 */
gchar *
gnome_db_sql_editor_get_text (GnomeDbSqlEditor *sql_editor,
				 GtkTextIter *start, GtkTextIter *end )
{
	GtkTextBuffer *buffer;

	g_return_val_if_fail (GNOME_DB_IS_SQL_EDITOR (sql_editor),NULL);

	g_return_val_if_fail (start != NULL,NULL);

	g_return_val_if_fail (end != NULL,NULL);

	buffer = gnome_db_sql_editor_get_text_buffer (sql_editor);

	if(!buffer)
		return FALSE;

	return gtk_text_buffer_get_text (buffer, start, end, FALSE);
}

/* debug functions -- these will be removed */
void gnome_db_sql_editor_debug_setup(GnomeDbSqlEditor *ed, GtkTextView *dtv)
{
	g_return_if_fail (GNOME_DB_IS_SQL_EDITOR (ed));
	ed->priv->debug_text_view = GTK_WIDGET(dtv);
}

/* gnome_db_sql_editor_debug(sql_editor, text);
 *
 */
void gnome_db_sql_editor_debug(GnomeDbSqlEditor *ed, gchar *text)
{
	gint char_count;
	GtkTextBuffer *buffer;
	GtkTextIter iter;

	g_return_if_fail (GNOME_DB_IS_SQL_EDITOR (ed));

	if(!ed->priv)
		return;

	if(!ed->priv->text_view)
		return;

	if(!ed->priv->debug_text_view)
		return;

	buffer = gtk_text_view_get_buffer (
			GTK_TEXT_VIEW (
				ed->priv->debug_text_view));

	if(text) 
	{
		if(*text > 0)
		{
			char_count = gtk_text_buffer_get_char_count(buffer);
			char_count = MAX(0, char_count - 1);
			gtk_text_buffer_get_iter_at_offset (buffer, &iter, char_count);
			gtk_text_buffer_insert (buffer, &iter, text, -1);	
		}
	}
	char_count = gtk_text_buffer_get_char_count(buffer);
	char_count = MAX(0, char_count - 1);
	gtk_text_buffer_get_iter_at_offset (buffer, &iter, char_count);
	gtk_text_buffer_insert (buffer, &iter, "\n", -1); 
}
