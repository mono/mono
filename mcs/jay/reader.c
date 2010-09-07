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
static char sccsid[] = "@(#)reader.c	5.7 (Berkeley) 1/20/91";
#endif /* not lint */

#include "defs.h"

/*  The line size must be a positive integer.  One hundred was chosen	*/
/*  because few lines in Yacc input grammars exceed 100 characters.	*/
/*  Note that if a line exceeds LINESIZE characters, the line buffer	*/
/*  will be expanded to accomodate it.					*/

#define LINESIZE 100

char *cache;
int cinc, cache_size;

int ntags, tagmax;
char **tag_table;

char saw_eof;
char *cptr, *line;
int linesize;

bucket *goal;
int prec;
int gensym;
char last_was_action;

int maxitems;
bucket **pitem;

int maxrules;
bucket **plhs;

int maxmethods;

int name_pool_size;
char *name_pool;

char *line_format = "\t\t\t\t\t// line %d \"%s\"\n";
char *default_line_format = "\t\t\t\t\t// line %d\n";


cachec(c)
int c;
{
    assert(cinc >= 0);
    if (cinc >= cache_size)
    {
	cache_size += 256;
	cache = REALLOC(cache, cache_size);
	if (cache == 0) no_space();
    }
    cache[cinc] = c;
    ++cinc;
}


get_line()
{
    register FILE *f = input_file;
    register int c;
    register int i;

    if (saw_eof || (c = getc(f)) == EOF)
    {
	if (line) { FREE(line); line = 0; }
	cptr = 0;
	saw_eof = 1;
	return;
    }

    if (line == 0 || linesize != (LINESIZE + 1))
    {
	if (line) FREE(line);
	linesize = LINESIZE + 1;
	line = MALLOC(linesize);
	if (line == 0) no_space();
    }

    i = 0;
    ++lineno;
    for (;;)
    {
	line[i]  =  c;
	if (c == '\n') { cptr = line; return; }
	if (++i >= linesize)
	{
	    linesize += LINESIZE;
	    line = REALLOC(line, linesize);
	    if (line ==  0) no_space();
	}
	c = getc(f);
	if (c ==  EOF)
	{
	    line[i] = '\n';
	    saw_eof = 1;
	    cptr = line;
	    return;
	}
    }
}


char *
dup_line()
{
    register char *p, *s, *t;

    if (line == 0) return (0);
    s = line;
    while (*s != '\n') ++s;
    p = MALLOC(s - line + 1);
    if (p == 0) no_space();

    s = line;
    t = p;
    while ((*t++ = *s++) != '\n') continue;
    return (p);
}


skip_comment()
{
    register char *s;

    int st_lineno = lineno;
    char *st_line = dup_line();
    char *st_cptr = st_line + (cptr - line);

    s = cptr + 2;
    for (;;)
    {
	if (*s == '*' && s[1] == '/')
	{
	    cptr = s + 2;
	    FREE(st_line);
	    return;
	}
	if (*s == '\n')
	{
	    get_line();
	    if (line == 0)
		unterminated_comment(st_lineno, st_line, st_cptr);
	    s = cptr;
	}
	else
	    ++s;
    }
}


int
nextc()
{
    register char *s;

    if (line == 0)
    {
	get_line();
	if (line == 0)
	    return (EOF);
    }

    s = cptr;
    for (;;)
    {
	switch (*s)
	{
	case '\n':
	    get_line();
	    if (line == 0) return (EOF);
	    s = cptr;
	    break;

	case ' ':
	case '\t':
	case '\f':
	case '\r':
	case '\v':
	case ',':
	case ';':
	    ++s;
	    break;

	case '\\':
	    cptr = s;
	    return ('%');

	case '/':
	    if (s[1] == '*')
	    {
		cptr = s;
		skip_comment();
		s = cptr;
		break;
	    }
	    else if (s[1] == '/')
	    {
		get_line();
		if (line == 0) return (EOF);
		s = cptr;
		break;
	    }
	    /* fall through */

	default:
	    cptr = s;
	    return (*s);
	}
    }
}


int
keyword()
{
    register int c;
    char *t_cptr = cptr;

    c = *++cptr;
    if (isalpha(c))
    {
	cinc = 0;
	for (;;)
	{
	    if (isalpha(c))
	    {
		if (isupper(c)) c = tolower(c);
		cachec(c);
	    }
	    else if (isdigit(c) || c == '_' || c == '.' || c == '$')
		cachec(c);
	    else
		break;
	    c = *++cptr;
	}
	cachec(NUL);

	if (strcmp(cache, "token") == 0 || strcmp(cache, "term") == 0)
	    return (TOKEN);
	if (strcmp(cache, "type") == 0)
	    return (TYPE);
	if (strcmp(cache, "left") == 0)
	    return (LEFT);
	if (strcmp(cache, "right") == 0)
	    return (RIGHT);
	if (strcmp(cache, "nonassoc") == 0 || strcmp(cache, "binary") == 0)
	    return (NONASSOC);
	if (strcmp(cache, "start") == 0)
	    return (START);
    }
    else
    {
	++cptr;
	if (c == '{')
	    return (TEXT);
	if (c == '%' || c == '\\')
	    return (MARK);
	if (c == '<')
	    return (LEFT);
	if (c == '>')
	    return (RIGHT);
	if (c == '0')
	    return (TOKEN);
	if (c == '2')
	    return (NONASSOC);
    }
    syntax_error(lineno, line, t_cptr);
    /*NOTREACHED*/
}


copy_text(f)
FILE *f;
{
    register int c;
    int quote;
    int need_newline = 0;
    int t_lineno = lineno;
    char *t_line = dup_line();
    char *t_cptr = t_line + (cptr - line - 2);

    if (*cptr == '\n')
    {
	get_line();
	if (line == 0)
	    unterminated_text(t_lineno, t_line, t_cptr);
    }
    fprintf(f, line_format, lineno, input_file_name);

loop:
    c = *cptr++;
    switch (c)
    {
    case '\n':
    next_line:
	putc('\n', f);
	need_newline = 0;
	get_line();
	if (line) goto loop;
	unterminated_text(t_lineno, t_line, t_cptr);

    case '\'':
    case '"':
	{
	    int s_lineno = lineno;
	    char *s_line = dup_line();
	    char *s_cptr = s_line + (cptr - line - 1);

	    quote = c;
	    putc(c, f);
	    for (;;)
	    {
		c = *cptr++;
		putc(c, f);
		if (c == quote)
		{
		    need_newline = 1;
		    FREE(s_line);
		    goto loop;
		}
		if (c == '\n')
		    unterminated_string(s_lineno, s_line, s_cptr);
		if (c == '\\')
		{
		    c = *cptr++;
		    putc(c, f);
		    if (c == '\n')
		    {
			get_line();
			if (line == 0)
			    unterminated_string(s_lineno, s_line, s_cptr);
		    }
		}
	    }
	}

    case '/':
	putc(c, f);
	need_newline = 1;
	c = *cptr;
	if (c == '/')
	{
	    do putc(c, f); while ((c = *++cptr) != '\n');
	    goto next_line;
	}
	if (c == '*')
	{
	    int c_lineno = lineno;
	    char *c_line = dup_line();
	    char *c_cptr = c_line + (cptr - line - 1);

	    putc('*', f);
	    ++cptr;
	    for (;;)
	    {
		c = *cptr++;
		putc(c, f);
		if (c == '*' && *cptr == '/')
		{
		    putc('/', f);
		    ++cptr;
		    FREE(c_line);
		    goto loop;
		}
		if (c == '\n')
		{
		    get_line();
		    if (line == 0)
			unterminated_comment(c_lineno, c_line, c_cptr);
		}
	    }
	}
	need_newline = 1;
	goto loop;

    case '%':
    case '\\':
	if (*cptr == '}')
	{
	    if (need_newline) putc('\n', f);
	    ++cptr;
	    FREE(t_line);
	    return;
	}
	/* fall through */

    default:
	putc(c, f);
	need_newline = 1;
	goto loop;
    }
}

int
hexval(c)
int c;
{
    if (c >= '0' && c <= '9')
	return (c - '0');
    if (c >= 'A' && c <= 'F')
	return (c - 'A' + 10);
    if (c >= 'a' && c <= 'f')
	return (c - 'a' + 10);
    return (-1);
}


bucket *
get_literal()
{
    register int c, quote;
    register int i;
    register int n;
    register char *s;
    register bucket *bp;
    int s_lineno = lineno;
    char *s_line = dup_line();
    char *s_cptr = s_line + (cptr - line);

    quote = *cptr++;
    cinc = 0;
    for (;;)
    {
	c = *cptr++;
	if (c == quote) break;
	if (c == '\n') unterminated_string(s_lineno, s_line, s_cptr);
	if (c == '\\')
	{
	    char *c_cptr = cptr - 1;

	    c = *cptr++;
	    switch (c)
	    {
	    case '\n':
		get_line();
		if (line == 0) unterminated_string(s_lineno, s_line, s_cptr);
		continue;

	    case '0': case '1': case '2': case '3':
	    case '4': case '5': case '6': case '7':
		n = c - '0';
		c = *cptr;
		if (IS_OCTAL(c))
		{
		    n = (n << 3) + (c - '0');
		    c = *++cptr;
		    if (IS_OCTAL(c))
		    {
			n = (n << 3) + (c - '0');
			++cptr;
		    }
		}
		if (n > MAXCHAR) illegal_character(c_cptr);
		c = n;
	    	break;

	    case 'x':
		c = *cptr++;
		n = hexval(c);
		if (n < 0 || n >= 16)
		    illegal_character(c_cptr);
		for (;;)
		{
		    c = *cptr;
		    i = hexval(c);
		    if (i < 0 || i >= 16) break;
		    ++cptr;
		    n = (n << 4) + i;
		    if (n > MAXCHAR) illegal_character(c_cptr);
		}
		c = n;
		break;

	    case 'a': c = 7; break;
	    case 'b': c = '\b'; break;
	    case 'f': c = '\f'; break;
	    case 'n': c = '\n'; break;
	    case 'r': c = '\r'; break;
	    case 't': c = '\t'; break;
	    case 'v': c = '\v'; break;
	    }
	}
	cachec(c);
    }
    FREE(s_line);

    n = cinc;
    s = MALLOC(n);
    if (s == 0) no_space();
    
    for (i = 0; i < n; ++i)
	s[i] = cache[i];

    cinc = 0;
    if (n == 1)
	cachec('\'');
    else
	cachec('"');

    for (i = 0; i < n; ++i)
    {
	c = ((unsigned char *)s)[i];
	if (c == '\\' || c == cache[0])
	{
	    cachec('\\');
	    cachec(c);
	}
	else if (isprint(c))
	    cachec(c);
	else
	{
	    cachec('\\');
	    switch (c)
	    {
	    case 7: cachec('a'); break;
	    case '\b': cachec('b'); break;
	    case '\f': cachec('f'); break;
	    case '\n': cachec('n'); break;
	    case '\r': cachec('r'); break;
	    case '\t': cachec('t'); break;
	    case '\v': cachec('v'); break;
	    default:
		cachec(((c >> 6) & 7) + '0');
		cachec(((c >> 3) & 7) + '0');
		cachec((c & 7) + '0');
		break;
	    }
	}
    }

    if (n == 1)
	cachec('\'');
    else
	cachec('"');

    cachec(NUL);
    bp = lookup(cache);
    bp->class = TERM;
    if (n == 1 && bp->value == UNDEFINED)
	bp->value = *(unsigned char *)s;
    FREE(s);

    return (bp);
}


int
is_reserved(name)
char *name;
{
    char *s;

    if (strcmp(name, ".") == 0 ||
	    strcmp(name, "$accept") == 0 ||
	    strcmp(name, "$end") == 0)
	return (1);

    if (name[0] == '$' && name[1] == '$' && isdigit(name[2]))
    {
	s = name + 3;
	while (isdigit(*s)) ++s;
	if (*s == NUL) return (1);
    }

    return (0);
}


bucket *
get_name()
{
    register int c;

    cinc = 0;
    for (c = *cptr; IS_IDENT(c); c = *++cptr)
	cachec(c);
    cachec(NUL);

    if (is_reserved(cache)) used_reserved(cache);

    return (lookup(cache));
}


int
get_number()
{
    register int c;
    register int n;

    n = 0;
    for (c = *cptr; isdigit(c); c = *++cptr)
	n = 10*n + (c - '0');

    return (n);
}


char *
get_tag(int emptyOk)
{
    register int c;
    register int i;
    register char *s;
    int t_lineno = lineno;
    char *t_line = dup_line();
    char *t_cptr = t_line + (cptr - line);

    ++cptr;
    c = nextc();
    if (c == EOF) unexpected_EOF();
    if (emptyOk && c == '>') {
      ++cptr; return 0;	// 0 indicates empty tag if emptyOk
    }
    if (!isalpha(c) && c != '_' && c != '$')
	illegal_tag(t_lineno, t_line, t_cptr);

    cinc = 0;
    do { cachec(c); c = *++cptr; } while (IS_IDENT(c));
    cachec(NUL);

    c = nextc();
    if (c == EOF) unexpected_EOF();
    if (c != '>')
	illegal_tag(t_lineno, t_line, t_cptr);
    ++cptr;

    for (i = 0; i < ntags; ++i)
    {
	if (strcmp(cache, tag_table[i]) == 0)
	    return (tag_table[i]);
    }

    if (ntags >= tagmax)
    {
	tagmax += 16;
	tag_table = (char **)
			(tag_table ? REALLOC(tag_table, tagmax*sizeof(char *))
				   : MALLOC(tagmax*sizeof(char *)));
	if (tag_table == 0) no_space();
    }

    s = MALLOC(cinc);
    if  (s == 0) no_space();
    strcpy(s, cache);
    tag_table[ntags] = s;
    ++ntags;
    FREE(t_line);
    return (s);
}


declare_tokens(assoc)
int assoc;
{
    register int c;
    register bucket *bp;
    int value;
    char *tag = 0;

    if (assoc != TOKEN) ++prec;

    c = nextc();
    if (c == EOF) unexpected_EOF();
    if (c == '<')
    {
	tag = get_tag(0);
	c = nextc();
	if (c == EOF) unexpected_EOF();
    }

    for (;;)
    {
	if (isalpha(c) || c == '_' || c == '.' || c == '$')
	    bp = get_name();
	else if (c == '\'' || c == '"')
	    bp = get_literal();
	else
	    return;

	if (bp == goal) tokenized_start(bp->name);
	bp->class = TERM;

	if (tag)
	{
	    if (bp->tag && tag != bp->tag)
		retyped_warning(bp->name);
	    bp->tag = tag;
	}

	if (assoc != TOKEN)
	{
	    if (bp->prec && prec != bp->prec)
		reprec_warning(bp->name);
	    bp->assoc = assoc;
	    bp->prec = prec;
	}

	c = nextc();
	if (c == EOF) unexpected_EOF();
	value = UNDEFINED;
	if (isdigit(c))
	{
	    value = get_number();
	    if (bp->value != UNDEFINED && value != bp->value)
		revalued_warning(bp->name);
	    bp->value = value;
	    c = nextc();
	    if (c == EOF) unexpected_EOF();
	}
    }
}


declare_types()
{
    register int c;
    register bucket *bp;
    char *tag;

    c = nextc();
    if (c == EOF) unexpected_EOF();
    if (c != '<') syntax_error(lineno, line, cptr);
    tag = get_tag(0);

    for (;;)
    {
	c = nextc();
	if (isalpha(c) || c == '_' || c == '.' || c == '$')
	    bp = get_name();
	else if (c == '\'' || c == '"')
	    bp = get_literal();
	else
	    return;

	if (bp->tag && tag != bp->tag)
	    retyped_warning(bp->name);
	bp->tag = tag;
    }
}


declare_start()
{
    register int c;
    register bucket *bp;

    c = nextc();
    if (c == EOF) unexpected_EOF();
    if (!isalpha(c) && c != '_' && c != '.' && c != '$')
	syntax_error(lineno, line, cptr);
    bp = get_name();
    if (bp->class == TERM)
	terminal_start(bp->name);
    if (goal && goal != bp)
	restarted_warning();
    goal = bp;
}


read_declarations()
{
    register int c, k;

    cache_size = 256;
    cache = MALLOC(cache_size);
    if (cache == 0) no_space();

    for (;;)
    {
	c = nextc();
	if (c == EOF) unexpected_EOF();
	if (c != '%') syntax_error(lineno, line, cptr);
	switch (k = keyword())
	{
	case MARK:
	    return;

	case TEXT:
	    copy_text(prolog_file);
	    break;

	case TOKEN:
	case LEFT:
	case RIGHT:
	case NONASSOC:
	    declare_tokens(k);
	    break;

	case TYPE:
	    declare_types();
	    break;

	case START:
	    declare_start();
	    break;
	}
    }
}


initialize_grammar()
{
    nitems = 4;
    maxitems = 300;
    pitem = (bucket **) MALLOC(maxitems*sizeof(bucket *));
    if (pitem == 0) no_space();
    pitem[0] = 0;
    pitem[1] = 0;
    pitem[2] = 0;
    pitem[3] = 0;

	nmethods = 0;
    nrules = 3;
    maxrules = 100;
    plhs = (bucket **) MALLOC(maxrules*sizeof(bucket *));
    if (plhs == 0) no_space();
    plhs[0] = 0;
    plhs[1] = 0;
    plhs[2] = 0;
    rprec = (short *) MALLOC(maxrules*sizeof(short));
    if (rprec == 0) no_space();
    rprec[0] = 0;
    rprec[1] = 0;
    rprec[2] = 0;
    rassoc = (char *) MALLOC(maxrules*sizeof(char));
    if (rassoc == 0) no_space();
    rassoc[0] = TOKEN;
    rassoc[1] = TOKEN;
    rassoc[2] = TOKEN;
}


expand_items()
{
    maxitems += 300;
    pitem = (bucket **) REALLOC(pitem, maxitems*sizeof(bucket *));
    if (pitem == 0) no_space();
}


expand_rules()
{
    maxrules += 100;
    plhs = (bucket **) REALLOC(plhs, maxrules*sizeof(bucket *));
    if (plhs == 0) no_space();
    rprec = (short *) REALLOC(rprec, maxrules*sizeof(short));
    if (rprec == 0) no_space();
    rassoc = (char *) REALLOC(rassoc, maxrules*sizeof(char));
    if (rassoc == 0) no_space();
}


advance_to_start()
{
    register int c;
    register bucket *bp;
    char *s_cptr;
    int s_lineno;

    for (;;)
    {
	c = nextc();
	if (c != '%') break;
	s_cptr = cptr;
	switch (keyword())
	{
	case MARK:
	    no_grammar();

	case TEXT:
	    copy_text(local_file);
	    break;

	case START:
	    declare_start();
	    break;

	default:
	    syntax_error(lineno, line, s_cptr);
	}
    }

    c = nextc();
    if (!isalpha(c) && c != '_' && c != '.' && c != '_')
	syntax_error(lineno, line, cptr);
    bp = get_name();
    if (goal == 0)
    {
	if (bp->class == TERM)
	    terminal_start(bp->name);
	goal = bp;
    }

    s_lineno = lineno;
    c = nextc();
    if (c == EOF) unexpected_EOF();
    if (c != ':') syntax_error(lineno, line, cptr);
    start_rule(bp, s_lineno);
    ++cptr;
}


start_rule(bp, s_lineno)
register bucket *bp;
int s_lineno;
{
    if (bp->class == TERM)
	terminal_lhs(s_lineno);
    bp->class = NONTERM;
    if (nrules >= maxrules)
	expand_rules();
    plhs[nrules] = bp;
    rprec[nrules] = UNDEFINED;
    rassoc[nrules] = TOKEN;
}


end_rule()
{
    register int i;

    if (!last_was_action && plhs[nrules]->tag)
    {
	for (i = nitems - 1; pitem[i]; --i) continue;
	if (pitem[i+1] == 0 || pitem[i+1]->tag != plhs[nrules]->tag)
	    default_action_warning();	/** if classes don't match exactly **/
    }					/** bug: could be superclass... **/

    last_was_action = 0;
    if (nitems >= maxitems) expand_items();
    pitem[nitems] = 0;
    ++nitems;
    ++nrules;
}


insert_empty_rule()
{
    register bucket *bp, **bpp;

    assert(cache);
    sprintf(cache, "$$%d", ++gensym);
    bp = make_bucket(cache);
    last_symbol->next = bp;
    last_symbol = bp;
    bp->tag = plhs[nrules]->tag;
    bp->class = NONTERM;

    if ((nitems += 2) > maxitems)
	expand_items();
    bpp = pitem + nitems - 1;
    *bpp-- = bp;
    while (bpp[0] = bpp[-1]) --bpp;

    if (++nrules >= maxrules)
	expand_rules();
    plhs[nrules] = plhs[nrules-1];
    plhs[nrules-1] = bp;
    rprec[nrules] = rprec[nrules-1];
    rprec[nrules-1] = 0;
    rassoc[nrules] = rassoc[nrules-1];
    rassoc[nrules-1] = TOKEN;
}


add_symbol()
{
    register int c;
    register bucket *bp;
    int s_lineno = lineno;

    c = *cptr;
    if (c == '\'' || c == '"')
	bp = get_literal();
    else
	bp = get_name();

    c = nextc();
    if (c == ':')
    {
	end_rule();
	start_rule(bp, s_lineno);
	++cptr;
	return;
    }

    if (last_was_action)
	insert_empty_rule();
    last_was_action = 0;

    if (++nitems > maxitems)
	expand_items();
    pitem[nitems-1] = bp;
}


copy_action()
{
    register int c;
    register int i, n;
    int depth;
    int quote;
    char *tag;
    FILE *f = action_file;
    int a_lineno = lineno;
    char *a_line = dup_line();
    char *a_cptr = a_line + (cptr - line);
	char buffer [10000];
	int len = 0;
	int comment_lines = 0;
	char *mbody;
	memset (buffer, 0, 10000);

    if (last_was_action)
	insert_empty_rule();
    last_was_action = 1;

    fprintf(f, "case %d:\n", nrules - 2);
    if (*cptr == '=') ++cptr;

    n = 0;

    for (i = nitems - 1; pitem[i]; --i) ++n;

    depth = 0;
loop:
    c = *cptr;
    if (c == '$')
    {
	if (cptr[1] == '<')
	{
	    int d_lineno = lineno;
	    char *d_line = dup_line();
	    char *d_cptr = d_line + (cptr - line);

	    ++cptr;
	    tag = get_tag(1);
	    c = *cptr;
	    if (c == '$')
	    {   
			if (tag && strcmp(tag, "Object")) {
				len += sprintf(buffer + len, "((%s)yyVal)", tag);
			} else {
				strcat (buffer + len, "yyVal");
				len += 5;
			}
		++cptr;
		FREE(d_line);
		goto loop;
	    }
	    else if (isdigit(c))
	    {
		i = get_number();
		if (i > n) dollar_warning(d_lineno, i);
		if (tag && strcmp(tag, "Object"))
			len += sprintf(buffer + len, "((%s)yyVals[%d+yyTop])", tag, i - n);
		else
			len += sprintf(buffer + len, "yyVals[%d+yyTop]", i - n);
		FREE(d_line);
		goto loop;
	    }
	    else if (c == '-' && isdigit(cptr[1]))
	    {
		++cptr;
		i = -get_number() - n;
		if (tag && strcmp(tag, "Object"))
			len += sprintf(buffer + len, "((%s)yyVals[%d+yyTop])", tag, i);
		else
			len += sprintf(buffer + len, "yyVals[%d+yyTop]", i);
		FREE(d_line);
		goto loop;
	    }
	    else
		dollar_error(d_lineno, d_line, d_cptr);
	}
	else if (cptr[1] == '$')
	{
	    if (ntags && plhs[nrules]->tag == 0)
		untyped_lhs();
		strcat (buffer, "yyVal");
		len += 5;
	    cptr += 2;
	    goto loop;
	}
	else if (isdigit(cptr[1]))
	{
	    ++cptr;
	    i = get_number();
	    if (ntags)
	    {
		if (i <= 0 || i > n)
		    unknown_rhs(i);
		tag = pitem[nitems + i - n - 1]->tag;
		if (tag == 0)
		    untyped_rhs(i, pitem[nitems + i - n - 1]->name),
		    len += sprintf(buffer + len, "yyVals[%d+yyTop]", i - n);
		else if (strcmp(tag, "Object"))
		    len += sprintf(buffer + len, "((%s)yyVals[%d+yyTop])", tag, i - n);
		else
		    len += sprintf(buffer + len, "yyVals[%d+yyTop]", i - n);
	    }
	    else
	    {
		if (i > n)
		    dollar_warning(lineno, i);

		len += sprintf(buffer + len,"yyVals[%d+yyTop]", i - n);
	    }
	    goto loop;
	}
	else if (cptr[1] == '-')
	{
	    cptr += 2;
	    i = get_number();
	    if (ntags)
		unknown_rhs(-i);
	    len += sprintf(buffer + len, "yyVals[%d+yyTop]", -i - n);
	    goto loop;
	}
    }
    if (isalpha(c) || c == '_' || c == '$')
    {
	do
	{
	    buffer[len++] = c;
	    c = *++cptr;
	} while (isalnum(c) || c == '_' || c == '$');
	goto loop;
    }
	buffer[len++] = c;
    ++cptr;
    switch (c)
    {
    case '\n':
    next_line:
	get_line();
	if (line) goto loop;
	unterminated_action(a_lineno, a_line, a_cptr);

    case ';':
	if (depth > 0) goto loop;
	break;

    case '{':
	++depth;
	goto loop;

    case '}':
	if (--depth > 0) goto loop;
	break;

    case '\'':
    case '"':
	{
	    int s_lineno = lineno;
	    char *s_line = dup_line();
	    char *s_cptr = s_line + (cptr - line - 1);

	    quote = c;
	    for (;;)
	    {
		c = *cptr++;
		buffer[len++] = c;
		if (c == quote)
		{
		    FREE(s_line);
		    goto loop;
		}
		if (c == '\n')
		    unterminated_string(s_lineno, s_line, s_cptr);
		if (c == '\\')
		{
		    c = *cptr++;
		    buffer[len++] = c;
		    if (c == '\n')
		    {
			get_line();
			if (line == 0)
			    unterminated_string(s_lineno, s_line, s_cptr);
		    }
		}
	    }
	}

    case '/':
	c = *cptr;
	if (c == '/')
	{
	    buffer[len++] = '*';
	    while ((c = *++cptr) != '\n')
	    {
			if (c == '*' && cptr[1] == '/'){
				buffer[len++] = '*';
				buffer[len++] = ' ';
			} else {
				buffer[len++] = c;
			}
		}
	    buffer[len++] = '*';
		buffer[len++] = '/';
		buffer[len++] = '\n';
	    goto next_line;
	}
	if (c == '*')
	{
	    int c_lineno = lineno;
	    char *c_line = dup_line();
	    char *c_cptr = c_line + (cptr - line - 1);

	    buffer[len++] = '*';
	    ++cptr;
	    for (;;)
	    {
		c = *cptr++;
		buffer[len++] = c;
		if (c == '*' && *cptr == '/')
		{
		    buffer[len++] = '/';
		    ++cptr;
		    FREE(c_line);
		    goto loop;
		}
		if (c == '\n')
		{
			++comment_lines;
		    get_line();
		    if (line == 0)
			unterminated_comment(c_lineno, c_line, c_cptr);
		}
	    }
	}
	goto loop;

    default:
	goto loop;
    }

	if (comment_lines > 0)
		comment_lines++;

	if ((lineno - (a_lineno + comment_lines)) > 2)
	{
		char mname[20];
		char line_define[256];

		sprintf(mname, "case_%d()", nrules - 2);

		putc(' ', f); putc(' ', f);
		fputs(mname, f);
		fprintf(f, ";");
		if (nmethods == 0)
		{
			maxmethods = 100;
			methods = NEW2(maxmethods, char *);
		}
		else if (nmethods == maxmethods)
		{
			maxmethods += 500;
			methods = REALLOC (methods, maxmethods*sizeof(char *));
		}

		sprintf(line_define, line_format, a_lineno, input_file_name);

		mbody = NEW2(5+strlen(line_define)+1+strlen(mname)+strlen(buffer)+1, char);
		strcpy(mbody, "void ");
		strcat(mbody, mname);
		strcat(mbody, "\n");
		strcat(mbody, line_define);
		strcat(mbody, buffer);
		methods[nmethods++] = mbody;
	}
	else
	{
	    fprintf(f, line_format, lineno, input_file_name);
		putc(' ', f); putc(' ', f);
		fwrite(buffer, 1, len, f);
	}

	fprintf(f, "\n  break;\n");
}


int
mark_symbol()
{
    register int c;
    register bucket *bp;

    c = cptr[1];
    if (c == '%' || c == '\\')
    {
	cptr += 2;
	return (1);
    }

    if (c == '=')
	cptr += 2;
    else if ((c == 'p' || c == 'P') &&
	     ((c = cptr[2]) == 'r' || c == 'R') &&
	     ((c = cptr[3]) == 'e' || c == 'E') &&
	     ((c = cptr[4]) == 'c' || c == 'C') &&
	     ((c = cptr[5], !IS_IDENT(c))))
	cptr += 5;
    else
	syntax_error(lineno, line, cptr);

    c = nextc();
    if (isalpha(c) || c == '_' || c == '.' || c == '$')
	bp = get_name();
    else if (c == '\'' || c == '"')
	bp = get_literal();
    else
    {
	syntax_error(lineno, line, cptr);
	/*NOTREACHED*/
    }

    if (rprec[nrules] != UNDEFINED && bp->prec != rprec[nrules])
	prec_redeclared();

    rprec[nrules] = bp->prec;
    rassoc[nrules] = bp->assoc;
    return (0);
}


read_grammar()
{
    register int c;

    initialize_grammar();
    advance_to_start();

    for (;;)
    {
	c = nextc();
	if (c == EOF) break;
	if (isalpha(c) || c == '_' || c == '.' || c == '$' || c == '\'' ||
		c == '"')
	    add_symbol();
	else if (c == '{' || c == '=')
	    copy_action();
	else if (c == '|')
	{
	    end_rule();
	    start_rule(plhs[nrules-1], 0);
	    ++cptr;
	}
	else if (c == '%')
	{
	    if (mark_symbol()) break;
	}
	else
	    syntax_error(lineno, line, cptr);
    }
    end_rule();
}


free_tags()
{
    register int i;

    if (tag_table == 0) return;

    for (i = 0; i < ntags; ++i)
    {
	assert(tag_table[i]);
	FREE(tag_table[i]);
    }
    FREE(tag_table);
}


pack_names()
{
    register bucket *bp;
    register char *p, *s, *t;

    name_pool_size = 13;  /* 13 == sizeof("$end") + sizeof("$accept") */
    for (bp = first_symbol; bp; bp = bp->next)
	name_pool_size += strlen(bp->name) + 1;
    name_pool = MALLOC(name_pool_size);
    if (name_pool == 0) no_space();

    strcpy(name_pool, "$accept");
    strcpy(name_pool+8, "$end");
    t = name_pool + 13;
    for (bp = first_symbol; bp; bp = bp->next)
    {
	p = t;
	s = bp->name;
	while (*t++ = *s++) continue;
	FREE(bp->name);
	bp->name = p;
    }
}


check_symbols()
{
    register bucket *bp;

    if (goal->class == UNKNOWN)
	undefined_goal(goal->name);

    for (bp = first_symbol; bp; bp = bp->next)
    {
	if (bp->class == UNKNOWN)
	{
	    undefined_symbol_warning(bp->name);
	    bp->class = TERM;
	}
    }
}


pack_symbols()
{
    register bucket *bp;
    register bucket **v;
    register int i, j, k, n;

    nsyms = 2;
    ntokens = 1;
    for (bp = first_symbol; bp; bp = bp->next)
    {
	++nsyms;
	if (bp->class == TERM) ++ntokens;
    }
    start_symbol = ntokens;
    nvars = nsyms - ntokens;

    symbol_name = (char **) MALLOC(nsyms*sizeof(char *));
    if (symbol_name == 0) no_space();
    symbol_value = (short *) MALLOC(nsyms*sizeof(short));
    if (symbol_value == 0) no_space();
    symbol_prec = (short *) MALLOC(nsyms*sizeof(short));
    if (symbol_prec == 0) no_space();
    symbol_assoc = MALLOC(nsyms);
    if (symbol_assoc == 0) no_space();

    v = (bucket **) MALLOC(nsyms*sizeof(bucket *));
    if (v == 0) no_space();

    v[0] = 0;
    v[start_symbol] = 0;

    i = 1;
    j = start_symbol + 1;
    for (bp = first_symbol; bp; bp = bp->next)
    {
	if (bp->class == TERM)
	    v[i++] = bp;
	else
	    v[j++] = bp;
    }
    assert(i == ntokens && j == nsyms);

    for (i = 1; i < ntokens; ++i)
	v[i]->index = i;

    goal->index = start_symbol + 1;
    k = start_symbol + 2;
    while (++i < nsyms)
	if (v[i] != goal)
	{
	    v[i]->index = k;
	    ++k;
	}

    goal->value = 0;
    k = 1;
    for (i = start_symbol + 1; i < nsyms; ++i)
    {
	if (v[i] != goal)
	{
	    v[i]->value = k;
	    ++k;
	}
    }

    k = 0;
    for (i = 1; i < ntokens; ++i)
    {
	n = v[i]->value;
	if (n > 256)
	{
	    for (j = k++; j > 0 && symbol_value[j-1] > n; --j)
		symbol_value[j] = symbol_value[j-1];
	    symbol_value[j] = n;
	}
    }

    if (v[1]->value == UNDEFINED)
	v[1]->value = 256;

    j = 0;
    n = 257;
    for (i = 2; i < ntokens; ++i)
    {
	if (v[i]->value == UNDEFINED)
	{
	    while (j < k && n == symbol_value[j])
	    {
		while (++j < k && n == symbol_value[j]) continue;
		++n;
	    }
	    v[i]->value = n;
	    ++n;
	}
    }

    symbol_name[0] = name_pool + 8;
    symbol_value[0] = 0;
    symbol_prec[0] = 0;
    symbol_assoc[0] = TOKEN;
    for (i = 1; i < ntokens; ++i)
    {
	symbol_name[i] = v[i]->name;
	symbol_value[i] = v[i]->value;
	symbol_prec[i] = v[i]->prec;
	symbol_assoc[i] = v[i]->assoc;
    }
    symbol_name[start_symbol] = name_pool;
    symbol_value[start_symbol] = -1;
    symbol_prec[start_symbol] = 0;
    symbol_assoc[start_symbol] = TOKEN;
    for (++i; i < nsyms; ++i)
    {
	k = v[i]->index;
	symbol_name[k] = v[i]->name;
	symbol_value[k] = v[i]->value;
	symbol_prec[k] = v[i]->prec;
	symbol_assoc[k] = v[i]->assoc;
    }

    FREE(v);
}


pack_grammar()
{
    register int i, j;
    int assoc, prec;

    ritem = (short *) MALLOC(nitems*sizeof(short));
    if (ritem == 0) no_space();
    rlhs = (short *) MALLOC(nrules*sizeof(short));
    if (rlhs == 0) no_space();
    rrhs = (short *) MALLOC((nrules+1)*sizeof(short));
    if (rrhs == 0) no_space();
    rprec = (short *) REALLOC(rprec, nrules*sizeof(short));
    if (rprec == 0) no_space();
    rassoc = REALLOC(rassoc, nrules);
    if (rassoc == 0) no_space();

    ritem[0] = -1;
    ritem[1] = goal->index;
    ritem[2] = 0;
    ritem[3] = -2;
    rlhs[0] = 0;
    rlhs[1] = 0;
    rlhs[2] = start_symbol;
    rrhs[0] = 0;
    rrhs[1] = 0;
    rrhs[2] = 1;

    j = 4;
    for (i = 3; i < nrules; ++i)
    {
	rlhs[i] = plhs[i]->index;
	rrhs[i] = j;
	assoc = TOKEN;
	prec = 0;
	while (pitem[j])
	{
	    ritem[j] = pitem[j]->index;
	    if (pitem[j]->class == TERM)
	    {
		prec = pitem[j]->prec;
		assoc = pitem[j]->assoc;
	    }
	    ++j;
	}
	ritem[j] = -i;
	++j;
	if (rprec[i] == UNDEFINED)
	{
	    rprec[i] = prec;
	    rassoc[i] = assoc;
	}
    }
    rrhs[i] = j;

    FREE(plhs);
    FREE(pitem);
}


print_grammar()
{
    register int i, j, k;
    int spacing;
    register FILE *f = verbose_file;

    if (!vflag) return;

    k = 1;
    for (i = 2; i < nrules; ++i)
    {
	if (rlhs[i] != rlhs[i-1])
	{
	    if (i != 2) fprintf(f, "\n");
	    fprintf(f, "%4d  %s :", i - 2, symbol_name[rlhs[i]]);
	    spacing = strlen(symbol_name[rlhs[i]]) + 1;
	}
	else
	{
	    fprintf(f, "%4d  ", i - 2);
	    j = spacing;
	    while (--j >= 0) putc(' ', f);
	    putc('|', f);
	}

	while (ritem[k] >= 0)
	{
	    fprintf(f, " %s", symbol_name[ritem[k]]);
	    ++k;
	}
	++k;
	putc('\n', f);
    }
}


reader()
{
    create_symbol_table();
    read_declarations();
    read_grammar();
    free_symbol_table();
    free_tags();
    pack_names();
    check_symbols();
    pack_symbols();
    pack_grammar();
    free_symbols();
    print_grammar();
}
