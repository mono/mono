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
char copyright[] =
"@(#) Copyright (c) 1989 The Regents of the University of California.\n\
 All rights reserved.\n";
#endif /* not lint */

#ifndef lint
static char sccsid[] = "@(#)main.c	5.5 (Berkeley) 5/24/93";
#endif /* not lint */

#include <signal.h>
#include "defs.h"

char tflag;
char vflag;
int csharp = 0;

char *file_prefix = "y";
char *myname = "yacc";
char *temp_form = "yacc.XXXXXXX";

int lineno;
int outline;

char *action_file_name;
char *input_file_name = "";
char *prolog_file_name;
char *local_file_name;
char *verbose_file_name;

FILE *action_file;	/*  a temp file, used to save actions associated    */
			/*  with rules until the parser is written	    */
FILE *input_file;	/*  the input file				    */
FILE *prolog_file;	/*  temp files, used to save text until all	    */
FILE *local_file;	/*  symbols have been defined			    */
FILE *verbose_file;	/*  y.output					    */

int nitems;
int nrules;
int nsyms;
int ntokens;
int nvars;
int nmethods;

int   start_symbol;
char  **symbol_name;
short *symbol_value;
short *symbol_prec;
char  *symbol_assoc;
char  **methods;

short *ritem;
short *rlhs;
short *rrhs;
short *rprec;
char  *rassoc;
short **derives;
char *nullable;

#if defined(_WIN32) && !defined(__CYGWIN32__) && !defined(__CYGWIN__)
extern char* mktemp();
#define mkstemp mktemp
#endif

extern char *getenv();

done(k)
int k;
{
    if (action_file) { fclose(action_file); unlink(action_file_name); }
    if (prolog_file) { fclose(prolog_file); unlink(prolog_file_name); }
    if (local_file) { fclose(local_file); unlink(local_file_name); }
    exit(k);
}


void
onintr(signo)
	int signo;
{
    done(1);
}


set_signals()
{
#ifdef SIGINT
    if (signal(SIGINT, SIG_IGN) != SIG_IGN)
	signal(SIGINT, onintr);
#endif
#ifdef SIGTERM
    if (signal(SIGTERM, SIG_IGN) != SIG_IGN)
	signal(SIGTERM, onintr);
#endif
#ifdef SIGHUP
    if (signal(SIGHUP, SIG_IGN) != SIG_IGN)
	signal(SIGHUP, onintr);
#endif
}


usage()
{
    fprintf(stderr, "usage: %s [-tvcp] [-b file_prefix] filename\n", myname);
    exit(1);
}

void
print_skel_dir(void)
{
    printf ("%s\n", SKEL_DIRECTORY);
    exit (0);
}

getargs(argc, argv)
int argc;
char *argv[];
{
    register int i;
    register char *s;

    if (argc > 0) myname = argv[0];
    for (i = 1; i < argc; ++i)
    {
	s = argv[i];
	if (*s != '-') break;
	switch (*++s)
	{
	case '\0':
	    input_file = stdin;
	    if (i + 1 < argc) usage();
	    return;

        case '-':
            ++i;
            goto no_more_options;

	case 'b':
	    if (*++s)
		 file_prefix = s;
	    else if (++i < argc)
		file_prefix = argv[i];
	    else
		usage();
	    continue;

        case 't':
            tflag = 1;
            break;

	case 'p':
            print_skel_dir ();
            break;

	case 'c':
	    csharp = 1;
	    line_format = "#line %d \"%s\"\n";
	    default_line_format = "#line default\n";
	    break;
	    
	case 'v':
	    vflag = 1;
	    break;

	default:
	    usage();
	}

	for (;;)
	{
	    switch (*++s)
	    {
	    case '\0':
		goto end_of_option;

	    case 't':
		tflag = 1;
		break;

	    case 'v':
		vflag = 1;
		break;

            case 'p':
                print_skel_dir ();
                break;

            case 'c':
		csharp = 1;
	        line_format = "#line %d \"%s\"\n";
        	default_line_format = "#line default\n";

		break;

	    default:
		usage();
	    }
	}
end_of_option:;
    }

no_more_options:;
    if (i + 1 != argc) usage();
    input_file_name = argv[i];
}


char *
allocate(n)
unsigned n;
{
    register char *p;

    p = NULL;
    if (n)
    {
	p = CALLOC(1, n);
	if (!p) no_space();
    }
    return (p);
}


create_file_names()
{
    int i, len;
    char *tmpdir;

#if defined(_WIN32) && !defined(__CYGWIN32__) && !defined(__CYGWIN__)
    tmpdir = ".";
#else
    tmpdir = getenv("TMPDIR");
    if (tmpdir == 0) tmpdir = getenv ("TMP");
    if (tmpdir == 0) tmpdir = getenv ("TEMP");
    if (tmpdir == 0) tmpdir = "/tmp";
#endif

    len = strlen(tmpdir);
    i = len + 13;
    if (len && tmpdir[len-1] != '/')
	++i;

    action_file_name = MALLOC(i);
    if (action_file_name == 0) no_space();
    prolog_file_name = MALLOC(i);
    if (prolog_file_name == 0) no_space();
    local_file_name = MALLOC(i);
    if (local_file_name == 0) no_space();

    strcpy(action_file_name, tmpdir);
    strcpy(prolog_file_name, tmpdir);
    strcpy(local_file_name, tmpdir);

    if (len && tmpdir[len - 1] != '/')
    {
	action_file_name[len] = '/';
	prolog_file_name[len] = '/';
	local_file_name[len] = '/';
	++len;
    }

    strcpy(action_file_name + len, temp_form);
    strcpy(prolog_file_name + len, temp_form);
    strcpy(local_file_name + len, temp_form);

    action_file_name[len + 5] = 'a';
    prolog_file_name[len + 5] = 'p';
    local_file_name[len + 5] = 'l';

    mkstemp(action_file_name);
    mkstemp(prolog_file_name);
    mkstemp(local_file_name);

    len = strlen(file_prefix);

    if (vflag)
    {
	verbose_file_name = MALLOC(len + 8);
	if (verbose_file_name == 0)
	    no_space();
	strcpy(verbose_file_name, file_prefix);
	strcpy(verbose_file_name + len, VERBOSE_SUFFIX);
    }
}


open_files()
{
    create_file_names();

    if (input_file == 0)
    {
	input_file = fopen(input_file_name, "r");
	if (input_file == 0)
	    open_error(input_file_name);
    }

    action_file = fopen(action_file_name, "w");
    if (action_file == 0)
	open_error(action_file_name);

    prolog_file = fopen(prolog_file_name, "w");
    if (prolog_file == 0)
	open_error(prolog_file_name);

    local_file = fopen(local_file_name, "w");
    if (local_file == 0)
	open_error(local_file_name);

    if (vflag)
    {
	verbose_file = fopen(verbose_file_name, "w");
	if (verbose_file == 0)
	    open_error(verbose_file_name);
    }
}


int
main(argc, argv)
int argc;
char *argv[];
{
    set_signals();
    getargs(argc, argv);
    open_files();
    reader();
    lr0();
    lalr();
    make_parser();
    verbose();
    output();
    done(0);
    /*NOTREACHED*/
}
