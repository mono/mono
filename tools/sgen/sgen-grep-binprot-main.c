/*
 * sgen-grep-binprot-main.c: Binary protocol entries reader 
 *
 * Copyright (C) 2016 Xamarin Inc
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include <stdio.h>
#include <stdlib.h>
#include <assert.h>
#include <glib.h>
#include <unistd.h>
#include <fcntl.h>
#include "sgen-entry-stream.h"
#include "sgen-grep-binprot.h"

/* FIXME Add grepers for specific endianness */
GrepEntriesFunction grepers [] = {
	sgen_binary_protocol_grep_entries32p, /* We have header, structures are packed, 32 bit word */
	sgen_binary_protocol_grep_entries64p, /* We have header, structures are packed, 64 bit word */
	sgen_binary_protocol_grep_entries /* No header, uses default word size and structure layout */
};

int
main (int argc, char *argv[])
{
	int num_args = argc - 1;
	int num_nums = 0;
	int num_vtables = 0;
	int i;
	long nums [num_args];
	long vtables [num_args];
	gboolean dump_all = FALSE;
	gboolean color_output = FALSE;
	gboolean pause_times = FALSE;
	const char *input_path = NULL;
	int input_file;
	EntryStream stream;
	unsigned long long first_entry_to_consider = 0;

	for (i = 0; i < num_args; ++i) {
		char *arg = argv [i + 1];
		char *next_arg = argv [i + 2];
		if (!strcmp (arg, "--all")) {
			dump_all = TRUE;
		} else if (!strcmp (arg, "--pause-times")) {
			pause_times = TRUE;
		} else if (!strcmp (arg, "-v") || !strcmp (arg, "--vtable")) {
			vtables [num_vtables++] = strtoul (next_arg, NULL, 16);
			++i;
		} else if (!strcmp (arg, "-s") || !strcmp (arg, "--start-at")) {
			first_entry_to_consider = strtoull (next_arg, NULL, 10);
			++i;
		} else if (!strcmp (arg, "-c") || !strcmp (arg, "--color")) {
			color_output = TRUE;
		} else if (!strcmp (arg, "-i") || !strcmp (arg, "--input")) {
			input_path = next_arg;
			++i;
		} else if (!strcmp (arg, "--help")) {
			printf (
				"\n"
				"Usage:\n"
				"\n"
				"\tsgen-grep-binprot [options] [pointer...]\n"
				"\n"
				"Examples:\n"
				"\n"
				"\tsgen-grep-binprot --all </tmp/binprot\n"
				"\tsgen-grep-binprot --input /tmp/binprot --color 0xdeadbeef\n"
				"\n"
				"Options:\n"
				"\n"
				"\t--all                    Print all entries.\n"
				"\t--color, -c              Highlight matches in color.\n"
				"\t--help                   You're looking at it.\n"
				"\t--input FILE, -i FILE    Read input from FILE instead of standard input.\n"
				"\t--pause-times            Print GC pause times.\n"
				"\t--start-at N, -s N       Begin filtering at the Nth entry.\n"
				"\t--vtable PTR, -v PTR     Search for vtable pointer PTR.\n"
				"\n");
			return 0;
		} else {
			nums [num_nums++] = strtoul (arg, NULL, 16);
		}
	}

	if (dump_all)
		assert (!pause_times);
	if (pause_times)
		assert (!dump_all);

	input_file = input_path ? open (input_path, O_RDONLY) : STDIN_FILENO;
	init_stream (&stream, input_file);
	for (i = 0; i < sizeof (grepers) / sizeof (GrepEntriesFunction); i++) {
		if (grepers [i] (&stream, num_nums, nums, num_vtables, vtables, dump_all,
				pause_times, color_output, first_entry_to_consider)) {
			/* Success */
			break;
		}
		reset_stream (&stream);
	}
	close_stream (&stream);
	if (input_path)
		close (input_file);

	return 0;
}
