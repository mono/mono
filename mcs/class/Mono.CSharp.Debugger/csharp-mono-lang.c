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

/* Local functions */

extern void _initialize_csharp_mono_language (void);

const struct language_defn csharp_mono_language_defn =
{
  "csharp-mono",		/* Language name */
  language_csharp_mono,
  c_builtin_types,
  range_check_off,
  type_check_off,
  case_sensitive_on,
  c_parse,
  c_error,
  evaluate_subexp_standard,
  c_printchar,			/* Print a character constant */
  c_printstr,			/* Function to print string constant */
  csharp_emit_char,		/* Function to print a single character */
  c_create_fundamental_type,	/* Create fundamental type in this language */
  csharp_print_type,		/* Print a type using appropriate syntax */
  csharp_val_print,		/* Print a value using appropriate syntax */
  csharp_value_print,		/* Print a top-level value */
  {"", "", "", ""},		/* Binary format info */
  {"0%lo", "0", "o", ""},	/* Octal format info */
  {"%ld", "", "d", ""},		/* Decimal format info */
  {"0x%lx", "0x", "x", ""},	/* Hex format info */
  csharp_op_print_tab,		/* expression operators for printing */
  0,				/* not c-style arrays */
  0,				/* String lower bound */
  &builtin_type_char,		/* Type of string elements */
  LANG_MAGIC
};

void
_initialize_csharp_mono_language (void)
{
  add_language (&csharp_mono_language_defn);
}

