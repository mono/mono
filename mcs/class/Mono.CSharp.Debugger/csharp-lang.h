/* -*- mode: C; c-basic-offset: 2 -*-

   C# language support for Mono.
   Copyright 2002 Ximian, Inc.

   This program is free software; you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation; either version 2 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with this program; if not, write to the Free Software
   Foundation, Inc., 59 Temple Place - Suite 330,
   Boston, MA 02111-1307, USA.  */

#ifndef CSHARP_LANG_H
#define CSHARP_LANG_H

extern const struct op_print csharp_op_print_tab[];

extern void csharp_emit_char (int c, struct ui_file *stream, int quoter);

extern void csharp_print_type (struct type *type, char *varstring,
			       struct ui_file *stream, int show, int level);

extern int csharp_val_print (struct type *type, char *valaddr, int embedded_offset,
			     CORE_ADDR address, struct ui_file *stream, int format,
			     int deref_ref, int recurse, enum val_prettyprint pretty);

extern int csharp_value_print (struct value *val, struct ui_file *stream, int format,
			       enum val_prettyprint pretty);

#endif
