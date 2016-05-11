typedef gboolean (*GrepEntriesFunction) (EntryStream *stream, int num_nums, long nums [], int num_vtables, long vtables [],
		gboolean dump_all, gboolean pause_times, gboolean color_output, unsigned long long first_entry_to_consider);

gboolean
sgen_binary_protocol_grep_entries (EntryStream *stream, int num_nums, long nums [], int num_vtables, long vtables [],
                        gboolean dump_all, gboolean pause_times, gboolean color_output, unsigned long long first_entry_to_consider);
gboolean
sgen_binary_protocol_grep_entries32p (EntryStream *stream, int num_nums, long nums [], int num_vtables, long vtables [],
                        gboolean dump_all, gboolean pause_times, gboolean color_output, unsigned long long first_entry_to_consider);
gboolean
sgen_binary_protocol_grep_entries64p (EntryStream *stream, int num_nums, long nums [], int num_vtables, long vtables [],
                        gboolean dump_all, gboolean pause_times, gboolean color_output, unsigned long long first_entry_to_consider);
