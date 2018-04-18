/*
 * Copyright (c) 1989 The Regents of the University of California.
 * All rights reserved.
 *
 * This code is derived from software contributed to Berkeley by
 * Robert Paul Corbett.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. All advertising materials mentioning features or use of this software
 *    must display the following acknowledgement:
 *	This product includes software developed by the University of
 *	California, Berkeley and its contributors.
 * 4. Neither the name of the University nor the names of its contributors
 *    may be used to endorse or promote products derived from this software
 *    without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE REGENTS AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE REGENTS OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 */

#ifndef lint
static char sccsid[] = "@(#)output.c	5.7 (Berkeley) 5/24/93";
#endif /* not lint */

#include "defs.h"
#include <string.h>

static int nvectors;
static int nentries;
static short **froms;
static short **tos;
static short *tally;
static short *width;
static short *state_count;
static short *order;
static short *base;
static short *pos;
static int maxtable;
static short *table;
static short *check;
static int lowzero;
static int high;
extern int csharp;

static void
free_itemsets (void);

static void
free_reductions (void);

static void
free_shifts (void);

static void
goto_actions (void);

static void
output_actions (void);

static void
output_base (void);

static void
output_check (void);

static void
output_debug (void);

static void
output_defines (const char *prefix);

static void
output_rule_data (void);

static void
output_semantic_actions (void);

static void
output_stored_text (FILE *file, const char *name);

static void
output_table (void);

static void
output_trailing_text (void);

static void
save_column (int symbol, int default_state);

static void
sort_actions (void);

static void 
output_yydefred (void);

static int
default_goto (int symbol);

static int
matching_vector (int vector);

static int
pack_vector (int vector);

static void
token_actions (void);

static void
pack_table (void);

void
output (void)
{
  int lno = 0;
  char buf [128];

  free_itemsets();
  free_shifts();
  free_reductions();

  while (fgets(buf, sizeof buf, stdin) != NULL) {
    char * cp;
    ++ lno;
    if (buf[strlen(buf)-1] != '\n')
      fprintf(stderr, "jay: line %d is too long\n", lno), done(1);
    switch (buf[0]) {
    case '#':	continue;
    case 't':	if (!tflag) fputs("//t", output_file);
    case '.':	break;
    default:
      cp = strtok(buf, " \t\r\n");
      if (cp) {
        if (strcmp(cp, "actions") == 0) output_semantic_actions();
        else if (strcmp(cp, "debug") == 0) output_debug();
        else if (strcmp(cp, "epilog") == 0) output_trailing_text();
        else if (strcmp(cp, "prolog") == 0)
		output_stored_text(prolog_file, prolog_file_name);
        else if (strcmp(cp, "local") == 0)
		output_stored_text(local_file, local_file_name);
        else if (strcmp(cp, "tables") == 0)
	  output_rule_data(), output_yydefred(), output_actions();
        else if (strcmp(cp, "tokens") == 0)
		output_defines(strtok(NULL, "\r\n"));
        else
          fprintf(stderr, "jay: unknown call (%s) in line %d\n", cp, lno);
      }
      continue;
    }
    fputs(buf+1, output_file), ++ outline;
  }
  free_parser();
}

static void
output_rule_data (void)
{
    register int i;
    register int j;

	fprintf(output_file, "/*\n All more than 3 lines long rules are wrapped into a method\n*/\n");

    for (i = 0; i < nmethods; ++i)
	{
		fprintf(output_file, "%s", methods[i]);
		FREE(methods[i]);
		fprintf(output_file, "\n\n");
	}
	FREE(methods);

	fprintf(output_file, default_line_format, ++outline + 1);

    fprintf(output_file, "  %s static %s short [] yyLhs  = {%16d,",
	   csharp ? "" : " protected",
	   csharp ? "readonly" : "final",
	    symbol_value[start_symbol]);

    j = 10;
    for (i = 3; i < nrules; i++)
    {
	if (j >= 10)
	{
	    ++outline;
	    putc('\n', output_file);
	    j = 1;
	}
        else
	    ++j;

        fprintf(output_file, "%5d,", symbol_value[rlhs[i]]);
    }
    outline += 2;
    fprintf(output_file, "\n  };\n");

    fprintf(output_file, "  %s static %s short [] yyLen = {%12d,",
	   csharp ? "" : "protected",
	   csharp ? "readonly" : "final",
	   2);

    j = 10;
    for (i = 3; i < nrules; i++)
    {
	if (j >= 10)
	{
	    ++outline;
	    putc('\n', output_file);
	    j = 1;
	}
	else
	  j++;

        fprintf(output_file, "%5d,", rrhs[i + 1] - rrhs[i] - 1);
    }
    outline += 2;
    fprintf(output_file, "\n  };\n");
}

static void
output_yydefred (void)
{
    register int i, j;

    fprintf(output_file, "  %s static %s short [] yyDefRed = {%13d,",
	   csharp ? "" : "protected",
	   csharp ? "readonly" : "final",	   
	    (defred[0] ? defred[0] - 2 : 0));

    j = 10;
    for (i = 1; i < nstates; i++)
    {
	if (j < 10)
	    ++j;
	else
	{
	    ++outline;
	    putc('\n', output_file);
	    j = 1;
	}

	fprintf(output_file, "%5d,", (defred[i] ? defred[i] - 2 : 0));
    }

    outline += 2;
    fprintf(output_file, "\n  };\n");
}

static void
output_actions (void)
{
    nvectors = 2*nstates + nvars;

    froms = NEW2(nvectors, short *);
    tos = NEW2(nvectors, short *);
    tally = NEW2(nvectors, short);
    width = NEW2(nvectors, short);

    token_actions();
    FREE(lookaheads);
    FREE(LA);
    FREE(LAruleno);
    FREE(accessing_symbol);

    goto_actions();
    FREE(goto_map + ntokens);
    FREE(from_state);
    FREE(to_state);

    sort_actions();
    pack_table();
    output_base();
    output_table();
    output_check();
}

static void
token_actions (void)
{
    register int i, j;
    register int shiftcount, reducecount;
    register int max, min;
    register short *actionrow, *r, *s;
    register action *p;

    actionrow = NEW2(2*ntokens, short);
    for (i = 0; i < nstates; ++i)
    {
	if (parser[i])
	{
	    for (j = 0; j < 2*ntokens; ++j)
	    actionrow[j] = 0;

	    shiftcount = 0;
	    reducecount = 0;
	    for (p = parser[i]; p; p = p->next)
	    {
		if (p->suppressed == 0)
		{
		    if (p->action_code == SHIFT)
		    {
			++shiftcount;
			actionrow[p->symbol] = p->number;
		    }
		    else if (p->action_code == REDUCE && p->number != defred[i])
		    {
			++reducecount;
			actionrow[p->symbol + ntokens] = p->number;
		    }
		}
	    }

	    tally[i] = shiftcount;
	    tally[nstates+i] = reducecount;
	    width[i] = 0;
	    width[nstates+i] = 0;
	    if (shiftcount > 0)
	    {
		froms[i] = r = NEW2(shiftcount, short);
		tos[i] = s = NEW2(shiftcount, short);
		min = MAXSHORT;
		max = 0;
		for (j = 0; j < ntokens; ++j)
		{
		    if (actionrow[j])
		    {
			if (min > symbol_value[j])
			    min = symbol_value[j];
			if (max < symbol_value[j])
			    max = symbol_value[j];
			*r++ = symbol_value[j];
			*s++ = actionrow[j];
		    }
		}
		width[i] = max - min + 1;
	    }
	    if (reducecount > 0)
	    {
		froms[nstates+i] = r = NEW2(reducecount, short);
		tos[nstates+i] = s = NEW2(reducecount, short);
		min = MAXSHORT;
		max = 0;
		for (j = 0; j < ntokens; ++j)
		{
		    if (actionrow[ntokens+j])
		    {
			if (min > symbol_value[j])
			    min = symbol_value[j];
			if (max < symbol_value[j])
			    max = symbol_value[j];
			*r++ = symbol_value[j];
			*s++ = actionrow[ntokens+j] - 2;
		    }
		}
		width[nstates+i] = max - min + 1;
	    }
	}
    }
    FREE(actionrow);
}

static void
goto_actions (void)
{
    register int i, j, k;

    state_count = NEW2(nstates, short);

    k = default_goto(start_symbol + 1);
    fprintf(output_file, "  protected static %s short [] yyDgoto  = {%14d,", csharp ? "readonly" : "final", k);
    save_column(start_symbol + 1, k);

    j = 10;
    for (i = start_symbol + 2; i < nsyms; i++)
    {
	if (j >= 10)
	{
	    ++outline;
	    putc('\n', output_file);
	    j = 1;
	}
	else
	    ++j;

	k = default_goto(i);
	fprintf(output_file, "%5d,", k);
	save_column(i, k);
    }

    outline += 2;
    fprintf(output_file, "\n  };\n");
    FREE(state_count);
}

static int
default_goto (int symbol)
{
    register int i;
    register int m;
    register int n;
    register int default_state;
    register int max;

    m = goto_map[symbol];
    n = goto_map[symbol + 1];

    if (m == n) return (0);

    for (i = 0; i < nstates; i++)
	state_count[i] = 0;

    for (i = m; i < n; i++)
	state_count[to_state[i]]++;

    max = 0;
    default_state = 0;
    for (i = 0; i < nstates; i++)
    {
	if (state_count[i] > max)
	{
	    max = state_count[i];
	    default_state = i;
	}
    }

    return (default_state);
}

static void
save_column (int symbol, int default_state)
{
    register int i;
    register int m;
    register int n;
    register short *sp;
    register short *sp1;
    register short *sp2;
    register int count;
    register int symno;

    m = goto_map[symbol];
    n = goto_map[symbol + 1];

    count = 0;
    for (i = m; i < n; i++)
    {
	if (to_state[i] != default_state)
	    ++count;
    }
    if (count == 0) return;

    symno = symbol_value[symbol] + 2*nstates;

    froms[symno] = sp1 = sp = NEW2(count, short);
    tos[symno] = sp2 = NEW2(count, short);

    for (i = m; i < n; i++)
    {
	if (to_state[i] != default_state)
	{
	    *sp1++ = from_state[i];
	    *sp2++ = to_state[i];
	}
    }

    tally[symno] = count;
    width[symno] = sp1[-1] - sp[0] + 1;
}

static void
sort_actions (void)
{
  register int i;
  register int j;
  register int k;
  register int t;
  register int w;

  order = NEW2(nvectors, short);
  nentries = 0;

  for (i = 0; i < nvectors; i++)
    {
      if (tally[i] > 0)
	{
	  t = tally[i];
	  w = width[i];
	  j = nentries - 1;

	  while (j >= 0 && (width[order[j]] < w))
	    j--;

	  while (j >= 0 && (width[order[j]] == w) && (tally[order[j]] < t))
	    j--;

	  for (k = nentries - 1; k > j; k--)
	    order[k + 1] = order[k];

	  order[j + 1] = i;
	  nentries++;
	}
    }
}

static void
pack_table (void)
{
    register int i;
    register int place;
    register int state;

    base = NEW2(nvectors, short);
    pos = NEW2(nentries, short);

    maxtable = 1000;
    table = NEW2(maxtable, short);
    check = NEW2(maxtable, short);

    lowzero = 0;
    high = 0;

    for (i = 0; i < maxtable; i++)
	check[i] = -1;

    for (i = 0; i < nentries; i++)
    {
	state = matching_vector(i);

	if (state < 0)
	    place = pack_vector(i);
	else
	    place = base[state];

	pos[i] = place;
	base[order[i]] = place;
    }

    for (i = 0; i < nvectors; i++)
    {
	if (froms[i])
	    FREE(froms[i]);
	if (tos[i])
	    FREE(tos[i]);
    }

    FREE(froms);
    FREE(tos);
    FREE(pos);
}


/*  The function matching_vector determines if the vector specified by	*/
/*  the input parameter matches a previously considered	vector.  The	*/
/*  test at the start of the function checks if the vector represents	*/
/*  a row of shifts over terminal symbols or a row of reductions, or a	*/
/*  column of shifts over a nonterminal symbol.  Berkeley Yacc does not	*/
/*  check if a column of shifts over a nonterminal symbols matches a	*/
/*  previously considered vector.  Because of the nature of LR parsing	*/
/*  tables, no two columns can match.  Therefore, the only possible	*/
/*  match would be between a row and a column.  Such matches are	*/
/*  unlikely.  Therefore, to save time, no attempt is made to see if a	*/
/*  column matches a previously considered vector.			*/
/*									*/
/*  Matching_vector is poorly designed.  The test could easily be made	*/
/*  faster.  Also, it depends on the vectors being in a specific	*/
/*  order.								*/

static int
matching_vector (int vector)
{
    register int i;
    register int j;
    register int k;
    register int t;
    register int w;
    register int match;
    register int prev;

    i = order[vector];
    if (i >= 2*nstates)
	return (-1);

    t = tally[i];
    w = width[i];

    for (prev = vector - 1; prev >= 0; prev--)
    {
	j = order[prev];
	if (width[j] != w || tally[j] != t)
	    return (-1);

	match = 1;
	for (k = 0; match && k < t; k++)
	{
	    if (tos[j][k] != tos[i][k] || froms[j][k] != froms[i][k])
		match = 0;
	}

	if (match)
	    return (j);
    }

    return (-1);
}

static int
pack_vector (int vector)
{
    register int i, j, k, l;
    register int t;
    register int loc;
    register int ok;
    register short *from;
    register short *to;
    int newmax;

    i = order[vector];
    t = tally[i];
    assert(t);

    from = froms[i];
    to = tos[i];

    j = lowzero - from[0];
    for (k = 1; k < t; ++k)
	if (lowzero - from[k] > j)
	    j = lowzero - from[k];
    for (;; ++j)
    {
	if (j == 0)
	    continue;
	ok = 1;
	for (k = 0; ok && k < t; k++)
	{
	    loc = j + from[k];
	    if (loc >= maxtable)
	    {
		if (loc >= MAXTABLE)
		    fatal("maximum table size exceeded");

		newmax = maxtable;
		do { newmax += 200; } while (newmax <= loc);
		table = (short *) REALLOC(table, newmax*sizeof(short));
		if (table == 0) no_space();
		check = (short *) REALLOC(check, newmax*sizeof(short));
		if (check == 0) no_space();
		for (l  = maxtable; l < newmax; ++l)
		{
		    table[l] = 0;
		    check[l] = -1;
		}
		maxtable = newmax;
	    }

	    if (check[loc] != -1)
		ok = 0;
	}
	for (k = 0; ok && k < vector; k++)
	{
	    if (pos[k] == j)
		ok = 0;
	}
	if (ok)
	{
	    for (k = 0; k < t; k++)
	    {
		loc = j + from[k];
		table[loc] = to[k];
		check[loc] = from[k];
		if (loc > high) high = loc;
	    }

	    while (check[lowzero] != -1)
		++lowzero;

	    return (j);
	}
    }
}

static void
output_base (void)
{
    register int i, j;

    fprintf(output_file, "  protected static %s short [] yySindex = {%13d,", csharp? "readonly":"final", base[0]);

    j = 10;
    for (i = 1; i < nstates; i++)
    {
	if (j >= 10)
	{
	    ++outline;
	    putc('\n', output_file);
	    j = 1;
	}
	else
	    ++j;

	fprintf(output_file, "%5d,", base[i]);
    }

    outline += 2;
    fprintf(output_file, "\n  };\n  protected static %s short [] yyRindex = {%13d,",
	   csharp ? "readonly" : "final",
	    base[nstates]);

    j = 10;
    for (i = nstates + 1; i < 2*nstates; i++)
    {
	if (j >= 10)
	{
	    ++outline;
	    putc('\n', output_file);
	    j = 1;
	}
	else
	    ++j;

	fprintf(output_file, "%5d,", base[i]);
    }

    outline += 2;
    fprintf(output_file, "\n  };\n  protected static %s short [] yyGindex = {%13d,",
	   csharp ? "readonly" : "final",
	    base[2*nstates]);

    j = 10;
    for (i = 2*nstates + 1; i < nvectors - 1; i++)
    {
	if (j >= 10)
	{
	    ++outline;
	    putc('\n', output_file);
	    j = 1;
	}
	else
	    ++j;

	fprintf(output_file, "%5d,", base[i]);
    }

    outline += 2;
    fprintf(output_file, "\n  };\n");
    FREE(base);
}

static void
output_table (void)
{
    register int i;
    register int j;

    fprintf(output_file, "  protected static %s short [] yyTable = {%14d,", csharp ? "readonly" : "final", table[0]);

    j = 10;
    for (i = 1; i <= high; i++)
    {
	if (j >= 10)
	{
	    ++outline;
	    putc('\n', output_file);
	    j = 1;
	}
	else
	    ++j;

	fprintf(output_file, "%5d,", table[i]);
    }

    outline += 2;
    fprintf(output_file, "\n  };\n");
    FREE(table);
}

static void
output_check (void)
{
    register int i;
    register int j;

    fprintf(output_file, "  protected static %s short [] yyCheck = {%14d,",
	   csharp ? "readonly" : "final",
	    check[0]);

    j = 10;
    for (i = 1; i <= high; i++)
    {
	if (j >= 10)
	{
	    ++outline;
	    putc('\n', output_file);
	    j = 1;
	}
	else
	    ++j;

	fprintf(output_file, "%5d,", check[i]);
    }

    outline += 2;
    fprintf(output_file, "\n  };\n");
    FREE(check);
}

static int
is_C_identifier (const char *name)
{
    const char *s;
    register int c;

    s = name;
    c = *s;
    if (c == '"')
    {
	c = *++s;
	if (!isalpha(c) && c != '_' && c != '$')
	    return (0);
	while ((c = *++s) != '"')
	{
	    if (!isalnum(c) && c != '_' && c != '$')
		return (0);
	}
	return (1);
    }

    if (!isalpha(c) && c != '_' && c != '$')
	return (0);
    while ((c = *++s))
    {
	if (!isalnum(c) && c != '_' && c != '$')
	    return (0);
    }
    return (1);
}

static void
output_defines (const char *prefix)
{
    register int c, i;
    const char *s;

    for (i = 2; i < ntokens; ++i)
    {
	s = symbol_name[i];
	if (is_C_identifier(s))
	{
	    if (prefix)
	        fprintf(output_file, "  %s ", prefix);
	    c = *s;
	    if (c == '"')
	    {
		while ((c = *++s) != '"')
		{
		    putc(c, output_file);
		}
	    }
	    else
	    {
		do
		{
		    putc(c, output_file);
		}
		while ((c = *++s));
	    }
	    ++outline;
	    fprintf(output_file, " = %d%s\n", symbol_value[i], csharp ? ";" : ";");
	}
    }

    ++outline;
    fprintf(output_file, "  %s yyErrorCode = %d%s\n", prefix ? prefix : "", symbol_value[1], csharp ? ";" : ";");
}

static void
output_stored_text (FILE *file, const char *name)
{
    register int c;
    register FILE *in;

    fflush(file);
    in = fopen(name, "r");
    if (in == NULL)
	open_error(name);
    if ((c = getc(in)) != EOF) {
      if (c ==  '\n')
	++outline;
      putc(c, output_file);
      while ((c = getc(in)) != EOF)
      {
	if (c == '\n')
	    ++outline;
    	putc(c, output_file);
      }
      fprintf(output_file, default_line_format, ++outline + 1);
    }
    fclose(in);
}

static void
output_debug (void)
{
    register int i, j, k, max;
    char **symnam, *s;
    const char * prefix = tflag ? "" : "//t";

    ++outline;
    fprintf(output_file, "  protected %s int yyFinal = %d;\n", csharp ? "const" : "static final", final_state);

      ++outline;
	  fprintf(output_file, "%s // Put this array into a separate class so it is only initialized if debugging is actually used\n", prefix);
	  fprintf(output_file, "%s // Use MarshalByRefObject to disable inlining\n", prefix);
	  fprintf(output_file, "%s class YYRules %s {\n", prefix, csharp ? ": MarshalByRefObject" : "");
      fprintf(output_file, "%s  public static %s string [] yyRule = {\n", prefix, csharp ? "readonly" : "final");
      for (i = 2; i < nrules; ++i)
      {
	  fprintf(output_file, "%s    \"%s :", prefix, symbol_name[rlhs[i]]);
	  for (j = rrhs[i]; ritem[j] > 0; ++j)
	  {
	      s = symbol_name[ritem[j]];
	      if (s[0] == '"')
	      {
		  fprintf(output_file, " \\\"");
		  while (*++s != '"')
		  {
		      if (*s == '\\')
		      {
			  if (s[1] == '\\')
			      fprintf(output_file, "\\\\\\\\");
			  else
			      fprintf(output_file, "\\\\%c", s[1]);
			  ++s;
		      }
		      else
			  putc(*s, output_file);
		  }
		  fprintf(output_file, "\\\"");
	      }
	      else if (s[0] == '\'')
	      {
		  if (s[1] == '"')
		      fprintf(output_file, " '\\\"'");
		  else if (s[1] == '\\')
		  {
		      if (s[2] == '\\')
			  fprintf(output_file, " '\\\\\\\\");
		      else
			  fprintf(output_file, " '\\\\%c", s[2]);
		      s += 2;
		      while (*++s != '\'')
			  putc(*s, output_file);
		      putc('\'', output_file);
		  }
		  else
		      fprintf(output_file, " '%c'", s[1]);
	      }
	      else
		  fprintf(output_file, " %s", s);
	  }
	  ++outline;
	  fprintf(output_file, "\",\n");
      }
      ++ outline;
      fprintf(output_file, "%s  };\n", prefix);
	  fprintf(output_file, "%s public static string getRule (int index) {\n", prefix);
	  fprintf(output_file, "%s    return yyRule [index];\n", prefix);
	  fprintf(output_file, "%s }\n", prefix);
	  fprintf(output_file, "%s}\n", prefix);

    max = 0;
    for (i = 2; i < ntokens; ++i)
	if (symbol_value[i] > max)
	    max = symbol_value[i];

	/* need yyNames for yyExpecting() */

      fprintf(output_file, "  protected static %s string [] yyNames = {", csharp ? "readonly" : "final");
      symnam = (char **) MALLOC((max+1)*sizeof(char *));
      if (symnam == 0) no_space();
  
      /* Note that it is  not necessary to initialize the element	*/
      /* symnam[max].							*/
      for (i = 0; i < max; ++i)
	  symnam[i] = 0;
      for (i = ntokens - 1; i >= 2; --i)
	  symnam[symbol_value[i]] = symbol_name[i];
      symnam[0] = (char*)"end-of-file";
  
      j = 70; fputs("    ", output_file);
      for (i = 0; i <= max; ++i)
      {
	  if ((s = symnam[i]))
	  {
	      if (s[0] == '"')
	      {
		  k = 7;
		  while (*++s != '"')
		  {
		      ++k;
		      if (*s == '\\')
		      {
			  k += 2;
			  if (*++s == '\\')
			      ++k;
		      }
		  }
		  j += k;
		  if (j > 70)
		  {
		      ++outline;
		      fprintf(output_file, "\n    ");
		      j = k;
		  }
		  fprintf(output_file, "\"\\\"");
		  s = symnam[i];
		  while (*++s != '"')
		  {
		      if (*s == '\\')
		      {
			  fprintf(output_file, "\\\\");
			  if (*++s == '\\')
			      fprintf(output_file, "\\\\");
			  else
			      putc(*s, output_file);
		      }
		      else
			  putc(*s, output_file);
		  }
		  fprintf(output_file, "\\\"\",");
	      }
	      else if (s[0] == '\'')
	      {
		  if (s[1] == '"')
		  {
		      j += 7;
		      if (j > 70)
		      {
			  ++outline;
		      	  fprintf(output_file, "\n    ");
			  j = 7;
		      }
		      fprintf(output_file, "\"'\\\"'\",");
		  }
		  else
		  {
		      k = 5;
		      while (*++s != '\'')
		      {
			  ++k;
			  if (*s == '\\')
			  {
			      k += 2;
			      if (*++s == '\\')
				  ++k;
			  }
		      }
		      j += k;
		      if (j > 70)
		      {
			  ++outline;
		      	  fprintf(output_file, "\n    ");
			  j = k;
		      }
		      fprintf(output_file, "\"'");
		      s = symnam[i];
		      while (*++s != '\'')
		      {
			  if (*s == '\\')
			  {
			      fprintf(output_file, "\\\\");
			      if (*++s == '\\')
				  fprintf(output_file, "\\\\");
			      else
				  putc(*s, output_file);
			  }
			  else
			      putc(*s, output_file);
		      }
		      fprintf(output_file, "'\",");
		  }
	      }
	      else
	      {
		  k = strlen(s) + 3;
		  j += k;
		  if (j > 70)
		  {
		      ++outline;
		      fprintf(output_file, "\n    ");
		      j = k;
		  }
		  putc('"', output_file);
		  do { putc(*s, output_file); } while (*++s);
		  fprintf(output_file, "\",");
	      }
	  }
	  else
	  {
	      j += 5;
	      if (j > 70)
	      {
		  ++outline;
		  fprintf(output_file, "\n    ");
		  j = 5;
	      }
	      fprintf(output_file, "null,");
	  }
      }
      outline += 2;
      fprintf(output_file, "\n  };\n");
      FREE(symnam);
}

static void
output_trailing_text (void)
{
    register int c, last;
    register FILE *in;

    if (line == 0)
	return;

    in = input_file;
    c = *cptr;
    if (c == '\n')
    {
	++lineno;
	if ((c = getc(in)) == EOF)
	    return;
        ++outline;
	fprintf(output_file, line_format, lineno, input_file_name);
	if (c == '\n')
	    ++outline;
	putc(c, output_file);
	last = c;
    }
    else
    {
	++outline;
	fprintf(output_file, line_format, lineno, input_file_name);
	do { putc(c, output_file); } while ((c = *++cptr) != '\n');
	++outline;
	putc('\n', output_file);
	last = '\n';
    }

    while ((c = getc(in)) != EOF)
    {
	if (c == '\n')
	    ++outline;
	putc(c, output_file);
	last = c;
    }

    if (last != '\n')
    {
	++outline;
	putc('\n', output_file);
    }
    fprintf(output_file, default_line_format, ++outline + 1);
}

static void
output_semantic_actions (void)
{
    register int c, last;

    fclose(action_file);
    action_file = fopen(action_file_name, "r");
    if (action_file == NULL)
	open_error(action_file_name);

    if ((c = getc(action_file)) == EOF)
	return;

    last = c;
    if (c == '\n')
	++outline;
    putc(c, output_file);
    while ((c = getc(action_file)) != EOF)
    {
	if (c == '\n')
	    ++outline;
	putc(c, output_file);
	last = c;
    }

    if (last != '\n')
    {
	++outline;
	putc('\n', output_file);
    }

    fprintf(output_file, default_line_format, ++outline + 1);
}

static void
free_itemsets (void)
{
    register core *cp, *next;

    FREE(state_table);
    for (cp = first_state; cp; cp = next)
    {
	next = cp->next;
	FREE(cp);
    }
}

static void
free_shifts (void)
{
    register shifts *sp, *next;

    FREE(shift_table);
    for (sp = first_shift; sp; sp = next)
    {
	next = sp->next;
	FREE(sp);
    }
}

static void
free_reductions (void)
{
    register reductions *rp, *next;

    FREE(reduction_table);
    for (rp = first_reduction; rp; rp = next)
    {
	next = rp->next;
	FREE(rp);
    }
}
