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
#include "valprint.h"
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

  CHECK_TYPEDEF (type);
  if (TYPE_CODE (type) == TYPE_CODE_PTR)
    {
      struct type *target_type = check_typedef (TYPE_TARGET_TYPE (type));
      CORE_ADDR addr = unpack_pointer (type, VALUE_CONTENTS (val));

      if ((addr != 0) &&
	  ((TYPE_CODE (target_type) == TYPE_CODE_CSHARP_STRING) ||
	   (TYPE_CODE (target_type) == TYPE_CODE_CSHARP_ARRAY) ||
	   (TYPE_CODE (target_type) == TYPE_CODE_STRUCT) ||
	   (TYPE_CODE (target_type) == TYPE_CODE_CLASS)))
	{
	  struct value *newval = value_at (target_type, addr, VALUE_BFD_SECTION (val));
	  int retval;

	  retval = val_print (target_type, VALUE_CONTENTS (newval), 0, addr,
			      stream, format, 1, 0, pretty);

	  release_value (newval);

	  return retval;
	}
    }

  return (val_print (type, VALUE_CONTENTS (val), 0, address,
		     stream, format, 1, 0, pretty));
}

void
csharp_print_type (struct type *type, char *varstring, struct ui_file *stream,
		   int show, int level)
{
  int i;

  CHECK_TYPEDEF (type);
  switch (TYPE_CODE (type))
    {
    case TYPE_CODE_CSHARP_STRING:
      fputs_filtered ("string", stream);
      return;

    case TYPE_CODE_CSHARP_ARRAY:
      csharp_print_type (TYPE_TARGET_TYPE (type), varstring, stream, show, level);

      fprintf_filtered (stream, "[");
      for (i = 1; i < TYPE_CSHARP_ARRAY_ARRAY_RANK (type); i++)
	fprintf_filtered (stream, ",");
      fputs_filtered ("]", stream);

      return;

    case TYPE_CODE_PTR:
      if (level == 0)
	csharp_print_type (TYPE_TARGET_TYPE (type), varstring, stream,
			   show, level + 1);
      else
	c_print_type (type, varstring, stream, show, level);

      return;

    default:
      c_print_type (type, varstring, stream, show, level);
      break;
    }
}

/*  Called by various <lang>_val_print routines to print elements of an
   array in the form "<elem1>, <elem2>, <elem3>, ...".

   (FIXME?)  Assumes array element separator is a comma, which is correct
   for all languages currently handled.
   (FIXME?)  Some languages have a notation for repeated array elements,
   perhaps we should try to use that notation when appropriate.
 */

void
csharp_print_array_elements (struct type *type, char *valaddr, CORE_ADDR address,
			     struct ui_file *stream, int format, int deref_ref,
			     int recurse, enum val_prettyprint pretty,
			     unsigned int len, unsigned int i)
{
  unsigned int things_printed = 0;
  struct type *elttype;
  unsigned eltlen;
  /* Position of the array element we are examining to see
     whether it is repeated.  */
  unsigned int rep1;
  /* Number of repetitions we have detected so far.  */
  unsigned int reps;

  elttype = TYPE_TARGET_TYPE (type);
  eltlen = TYPE_LENGTH (check_typedef (elttype));

  annotate_array_section_begin (i, elttype);

  for (; i < len && things_printed < print_max; i++)
    {
      if (i != 0)
	{
	  if (prettyprint_arrays)
	    {
	      fprintf_filtered (stream, ",\n");
	      print_spaces_filtered (2 + 2 * recurse, stream);
	    }
	  else
	    {
	      fprintf_filtered (stream, ", ");
	    }
	}
      wrap_here (n_spaces (2 + 2 * recurse));

      rep1 = i + 1;
      reps = 1;
      while ((rep1 < len) &&
	     !memcmp (valaddr + i * eltlen, valaddr + rep1 * eltlen, eltlen))
	{
	  ++reps;
	  ++rep1;
	}

      if (reps > repeat_count_threshold)
	{
	  val_print (elttype, valaddr + i * eltlen, 0, address + i * eltlen,
		     stream, format, deref_ref, recurse + 1, pretty);
	  annotate_elt_rep (reps);
	  fprintf_filtered (stream, " <repeats %u times>", reps);
	  annotate_elt_rep_end ();

	  i = rep1 - 1;
	  things_printed += repeat_count_threshold;
	}
      else
	{
	  val_print (elttype, valaddr + i * eltlen, 0, address + i * eltlen,
		     stream, format, deref_ref, recurse + 1, pretty);
	  annotate_elt ();
	  things_printed++;
	}
    }
  annotate_array_section_end ();
  if (i < len)
    {
      fprintf_filtered (stream, "...");
    }
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
  struct value *newval;
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

    case TYPE_CODE_CSHARP_ARRAY:
      target_type = check_typedef (TYPE_TARGET_TYPE (type));

      addr = address + TYPE_CSHARP_ARRAY_LENGTH_OFFSET (type);
      length = read_memory_integer (addr, TYPE_CSHARP_ARRAY_LENGTH_BYTESIZE (type));

      addr = address + TYPE_CSHARP_ARRAY_DATA_OFFSET (type);

      newval = allocate_repeat_value (target_type, length);

      VALUE_LVAL (newval) = lval_memory;
      VALUE_ADDRESS (newval) = addr;
      VALUE_LAZY (newval) = 1;

      fprintf_filtered (stream, "{");

      csharp_print_array_elements (type, VALUE_CONTENTS (newval), addr, stream,
				   format, deref_ref, recurse, pretty, length, 0);

      fprintf_filtered (stream, "}");

      release_value (newval);

      return 0;

    case TYPE_CODE_PTR:
      target_type = check_typedef (TYPE_TARGET_TYPE (type));
      addr = unpack_pointer (type, valaddr);

      if ((addr != 0) && (TYPE_CODE (target_type) == TYPE_CODE_CSHARP_STRING))
	{
	  struct value *newval = value_at (target_type, addr, NULL);
	  int retval;

	  retval = val_print (target_type, VALUE_CONTENTS (newval), 0, addr,
			      stream, format, 1, 0, pretty);

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
