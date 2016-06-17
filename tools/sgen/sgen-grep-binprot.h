#ifndef SGEN_GREP_BINPROT_H
#define SGEN_GREP_BINPROT_H

typedef gboolean GrepEntriesFunction(
	EntryStream *stream,
	int num_nums,
	long nums [],
	int num_patterns,
	const char *patterns [],
	int num_vtables,
	long vtables [],
	gboolean dump_all,
	gboolean pause_times,
	gboolean color_output,
	unsigned long long first_entry_to_consider);

GrepEntriesFunction
	sgen_binary_protocol_grep_entries,
	sgen_binary_protocol_grep_entries32p,
	sgen_binary_protocol_grep_entries64p;

#endif /* SGEN_GREP_BINPROT_H */
