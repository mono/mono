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
#include "gdbcore.h"
#include "expression.h"
#include "value.h"
#include "demangle.h"
#include "parser-defs.h"
#include "language.h"
#include "symfile.h"
#include "objfiles.h"
#include "gdb_string.h"
#include "csharp-lang.h"
#include "c-lang.h"
#include "annotate.h"
#include <ctype.h>

static void csharp_print_value_fields (struct type *type, char *valaddr, CORE_ADDR address,
				       struct ui_file *stream, int format, int recurse,
				       enum val_prettyprint pretty);

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
    case TYPE_CODE_CSHARP_STRING:
      addr = address + TYPE_CSHARP_ARRAY_LENGTH_OFFSET (type);
      length = read_memory_integer (addr, TYPE_CSHARP_ARRAY_LENGTH_BYTESIZE (type));

      addr = address + TYPE_CSHARP_ARRAY_DATA_OFFSET (type);

      return val_print_string (addr, length, 2, stream);

    case TYPE_CODE_PTR:
      addr = unpack_pointer (type, valaddr);
      target_type = check_typedef (TYPE_TARGET_TYPE (type));

      if (deref_ref && addr != 0)
	{
	  struct value *newval;
	  int retval;

	  newval = allocate_value (target_type);
	  retval = csharp_val_print (target_type, (char *) newval, embedded_offset,
				     addr, stream, format, deref_ref, recurse,
				     pretty);
	  release_value (newval);

	  return retval;
	}
      break;

    default:
      break;
    }

  return c_val_print (type, valaddr, embedded_offset, address, stream,
		      format, deref_ref, recurse, pretty);
}

/* Table mapping opcodes into strings for printing operators
   and precedences of the operators.  */

const struct op_print csharp_op_print_tab[] =
{
  {NULL, 0, 0, 0}
};
