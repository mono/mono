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

#include "defs.h"
#include "symtab.h"
#include "gdbtypes.h"
#include "expression.h"
#include "parser-defs.h"
#include "language.h"
#include "gdbtypes.h"
#include "symtab.h"
#include "symfile.h"
#include "objfiles.h"
#include "gdb_string.h"
#include "value.h"
#include "csharp-lang.h"
#include "c-lang.h"
#include "gdbcore.h"
#include <ctype.h>

/* Print the character C on STREAM as part of the contents of a literal
   string whose delimiter is QUOTER.  Note that that format for printing
   characters and strings is language specific. */

void
csharp_emit_char (int c, struct ui_file *stream, int quoter)
{
  switch (c)
    {
    case '\\':
    case '\'':
      fprintf_filtered (stream, "\\%c", c);
      break;
    case '\b':
      fputs_filtered ("\\b", stream);
      break;
    case '\t':
      fputs_filtered ("\\t", stream);
      break;
    case '\n':
      fputs_filtered ("\\n", stream);
      break;
    case '\f':
      fputs_filtered ("\\f", stream);
      break;
    case '\r':
      fputs_filtered ("\\r", stream);
      break;
    default:
      if (isprint (c))
	fputc_filtered (c, stream);
      else
	fprintf_filtered (stream, "\\u%.4x", (unsigned int) c);
      break;
    }
}

int
csharp_value_print (struct value *val, struct ui_file *stream, int format,
		    enum val_prettyprint pretty)
{
  struct type *type;
  CORE_ADDR address;
  int i;
  char *name;

  type = VALUE_TYPE (val);
  address = VALUE_ADDRESS (val) + VALUE_OFFSET (val);

  /* If it's type String, print it */

  if (TYPE_CODE (type) == TYPE_CODE_PTR
      && TYPE_TARGET_TYPE (type)
      && TYPE_TAG_NAME (TYPE_TARGET_TYPE (type))
      && strcmp (TYPE_TAG_NAME (TYPE_TARGET_TYPE (type)), "MonoString") == 0
      && (format == 0 || format == 's')
      && address != 0
      && value_as_address (val) != 0)
    {
      struct value *data_val;
      CORE_ADDR data;
      struct value *count_val;
      unsigned long count;
      struct value *mark;

      mark = value_mark ();	/* Remember start of new values */

      data_val = value_struct_elt (&val, NULL, "Chars", NULL, NULL);
      data = VALUE_ADDRESS (data_val) + VALUE_OFFSET (data_val);

      count_val = value_struct_elt (&val, NULL, "Length", NULL, NULL);
      count = value_as_address (count_val);

      value_free_to_mark (mark);	/* Release unnecessary values */

      val_print_string (data, count, 2, stream);

      return 0;
    }

  return (val_print (type, VALUE_CONTENTS (val), 0, address,
		     stream, format, 1, 0, pretty));
}


/* Print data of type TYPE located at VALADDR (within GDB), which came from
   the inferior at address ADDRESS, onto stdio stream STREAM according to
   FORMAT (a letter or 0 for natural format).  The data at VALADDR is in
   target byte order.

   If the data are a string pointer, returns the number of string characters
   printed.

   If DEREF_REF is nonzero, then dereference references, otherwise just print
   them like pointers.

   The PRETTY parameter controls prettyprinting.  */

int
csharp_val_print (struct type *type, char *valaddr, int embedded_offset,
		  CORE_ADDR address, struct ui_file *stream, int format,
		  int deref_ref, int recurse, enum val_prettyprint pretty)
{
  register unsigned int i = 0;	/* Number of characters printed */
  struct type *target_type;
  LONGEST length;
  CORE_ADDR addr;

  CHECK_TYPEDEF (type);
  switch (TYPE_CODE (type))
    {
#if 0
    case TYPE_CODE_CSHARP_STRING:
      addr = address + TYPE_CSHARP_STRING_LENGTH_OFFSET (type);
      length = read_memory_integer (addr, TYPE_CSHARP_STRING_LENGTH_BYTESIZE (type));

      addr = address + TYPE_CSHARP_STRING_DATA_OFFSET (type);

      return val_print_string (addr, length, 2, stream);

    case TYPE_CODE_PTR:
      addr = unpack_pointer (type, valaddr);
      target_type = check_typedef (TYPE_TARGET_TYPE (type));

      if (deref_ref && addr != 0)
	{
	  if (TYPE_CODE (target_type) == TYPE_CODE_CSHARP_STRING)
	    return csharp_val_print (target_type, NULL, 0, addr, stream,
				     format, deref_ref, recurse, pretty);
	  else
	    return 0;
	}

      return c_val_print (type, valaddr, embedded_offset, address, stream,
			  format, deref_ref, recurse, pretty);
#endif

    default:
      return c_val_print (type, valaddr, embedded_offset, address, stream,
			  format, deref_ref, recurse, pretty);
    }

  return 0;
}

/* Table mapping opcodes into strings for printing operators
   and precedences of the operators.  */

const struct op_print csharp_op_print_tab[] =
{
  {NULL, 0, 0, 0}
};
