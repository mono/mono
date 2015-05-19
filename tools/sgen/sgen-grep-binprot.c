#include <stdio.h>
#include <stdlib.h>
#include <assert.h>
#include <glib.h>
#include <unistd.h>
#include <fcntl.h>

#define SGEN_BINARY_PROTOCOL
#define MONO_INTERNAL

#include <mono/sgen/sgen-protocol.h>

#define SGEN_PROTOCOL_EOF	255

#define TYPE(t)		((t) & 0x7f)
#define WORKER(t)	((t) & 0x80)

#define MAX_ENTRY_SIZE (1 << 10)
#define BUFFER_SIZE (1 << 20)

typedef struct {
	int file;
	char *buffer;
	const char *end;
	const char *pos;
} EntryStream;

static void
init_stream (EntryStream *stream, int file)
{
	stream->file = file;
	stream->buffer = g_malloc0 (BUFFER_SIZE);
	stream->end = stream->buffer + BUFFER_SIZE;
	stream->pos = stream->end;
}

static void
close_stream (EntryStream *stream)
{
	g_free (stream->buffer);
}

static gboolean
refill_stream (EntryStream *in, size_t size)
{
	size_t remainder = in->end - in->pos;
	ssize_t refilled;
	g_assert (size > 0);
	g_assert (in->pos >= in->buffer);
	if (in->pos + size <= in->end)
		return TRUE;
	memmove (in->buffer, in->pos, remainder);
	in->pos = in->buffer;
	refilled = read (in->file, in->buffer + remainder, BUFFER_SIZE - remainder);
	if (refilled < 0)
		return FALSE;
	g_assert (refilled + remainder <= BUFFER_SIZE);
	in->end = in->buffer + refilled + remainder;
	return in->end - in->buffer >= size;
}

static ssize_t
read_stream (EntryStream *stream, void *out, size_t size)
{
	if (refill_stream (stream, size)) {
		memcpy (out, stream->pos, size);
		stream->pos += size;
		return size;
	}
	return 0;
}

static int
read_entry (EntryStream *stream, void *data)
{
	unsigned char type;
	ssize_t size;

	if (read_stream (stream, &type, 1) <= 0)
		return SGEN_PROTOCOL_EOF;
	switch (TYPE (type)) {

#define BEGIN_PROTOCOL_ENTRY0(method) \
	case PROTOCOL_ID(method): size = 0; break;
#define BEGIN_PROTOCOL_ENTRY1(method,t1,f1) \
	case PROTOCOL_ID(method): size = sizeof (PROTOCOL_STRUCT(method)); break;
#define BEGIN_PROTOCOL_ENTRY2(method,t1,f1,t2,f2) \
	case PROTOCOL_ID(method): size = sizeof (PROTOCOL_STRUCT(method)); break;
#define BEGIN_PROTOCOL_ENTRY3(method,t1,f1,t2,f2,t3,f3) \
	case PROTOCOL_ID(method): size = sizeof (PROTOCOL_STRUCT(method)); break;
#define BEGIN_PROTOCOL_ENTRY4(method,t1,f1,t2,f2,t3,f3,t4,f4) \
	case PROTOCOL_ID(method): size = sizeof (PROTOCOL_STRUCT(method)); break;
#define BEGIN_PROTOCOL_ENTRY5(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5) \
	case PROTOCOL_ID(method): size = sizeof (PROTOCOL_STRUCT(method)); break;
#define BEGIN_PROTOCOL_ENTRY6(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5,t6,f6) \
	case PROTOCOL_ID(method): size = sizeof (PROTOCOL_STRUCT(method)); break;

#define BEGIN_PROTOCOL_ENTRY_HEAVY0(method) \
	BEGIN_PROTOCOL_ENTRY0 (method)
#define BEGIN_PROTOCOL_ENTRY_HEAVY1(method,t1,f1) \
	BEGIN_PROTOCOL_ENTRY1 (method,t1,f1)
#define BEGIN_PROTOCOL_ENTRY_HEAVY2(method,t1,f1,t2,f2) \
	BEGIN_PROTOCOL_ENTRY2 (method,t1,f1,t2,f2)
#define BEGIN_PROTOCOL_ENTRY_HEAVY3(method,t1,f1,t2,f2,t3,f3) \
	BEGIN_PROTOCOL_ENTRY3 (method,t1,f1,t2,f2,t3,f3)
#define BEGIN_PROTOCOL_ENTRY_HEAVY4(method,t1,f1,t2,f2,t3,f3,t4,f4) \
	BEGIN_PROTOCOL_ENTRY4 (method,t1,f1,t2,f2,t3,f3,t4,f4)
#define BEGIN_PROTOCOL_ENTRY_HEAVY5(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5) \
	BEGIN_PROTOCOL_ENTRY5 (method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5)
#define BEGIN_PROTOCOL_ENTRY_HEAVY6(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5,t6,f6) \
	BEGIN_PROTOCOL_ENTRY6 (method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5,t6,f6)

#define FLUSH()

#define DEFAULT_PRINT()
#define CUSTOM_PRINT(_)

#define IS_ALWAYS_MATCH(_)
#define MATCH_INDEX(_)
#define IS_VTABLE_MATCH(_)

#define END_PROTOCOL_ENTRY
#define END_PROTOCOL_ENTRY_HEAVY

#include <mono/sgen/sgen-protocol-def.h>

	default: assert (0);
	}

	if (size) {
		size_t size_read = read_stream (stream, data, size);
		g_assert (size_read == size);
	}

	return (int)type;
}

static gboolean
is_always_match (int type)
{
	switch (TYPE (type)) {
#define BEGIN_PROTOCOL_ENTRY0(method) \
	case PROTOCOL_ID(method):
#define BEGIN_PROTOCOL_ENTRY1(method,t1,f1) \
	case PROTOCOL_ID(method):
#define BEGIN_PROTOCOL_ENTRY2(method,t1,f1,t2,f2) \
	case PROTOCOL_ID(method):
#define BEGIN_PROTOCOL_ENTRY3(method,t1,f1,t2,f2,t3,f3) \
	case PROTOCOL_ID(method):
#define BEGIN_PROTOCOL_ENTRY4(method,t1,f1,t2,f2,t3,f3,t4,f4) \
	case PROTOCOL_ID(method):
#define BEGIN_PROTOCOL_ENTRY5(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5) \
	case PROTOCOL_ID(method):
#define BEGIN_PROTOCOL_ENTRY6(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5,t6,f6) \
	case PROTOCOL_ID(method):

#define BEGIN_PROTOCOL_ENTRY_HEAVY0(method) \
	BEGIN_PROTOCOL_ENTRY0 (method)
#define BEGIN_PROTOCOL_ENTRY_HEAVY1(method,t1,f1) \
	BEGIN_PROTOCOL_ENTRY1 (method,t1,f1)
#define BEGIN_PROTOCOL_ENTRY_HEAVY2(method,t1,f1,t2,f2) \
	BEGIN_PROTOCOL_ENTRY2 (method,t1,f1,t2,f2)
#define BEGIN_PROTOCOL_ENTRY_HEAVY3(method,t1,f1,t2,f2,t3,f3) \
	BEGIN_PROTOCOL_ENTRY3 (method,t1,f1,t2,f2,t3,f3)
#define BEGIN_PROTOCOL_ENTRY_HEAVY4(method,t1,f1,t2,f2,t3,f3,t4,f4) \
	BEGIN_PROTOCOL_ENTRY4 (method,t1,f1,t2,f2,t3,f3,t4,f4)
#define BEGIN_PROTOCOL_ENTRY_HEAVY5(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5) \
	BEGIN_PROTOCOL_ENTRY5 (method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5)
#define BEGIN_PROTOCOL_ENTRY_HEAVY6(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5,t6,f6) \
	BEGIN_PROTOCOL_ENTRY6 (method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5,t6,f6)

#define FLUSH()

#define DEFAULT_PRINT()
#define CUSTOM_PRINT(_)

#define IS_ALWAYS_MATCH(is_always_match) \
		return is_always_match;
#define MATCH_INDEX(_)
#define IS_VTABLE_MATCH(_)

#define END_PROTOCOL_ENTRY
#define END_PROTOCOL_ENTRY_HEAVY

#include <mono/sgen/sgen-protocol-def.h>

	default: assert (0);
	}
}

#define WORKER_PREFIX(t)	(WORKER ((t)) ? "w" : " ")

enum { NO_COLOR = -1 };

typedef struct {
	int type;
	const char *name;
	void *data;
	/* The index of the ANSI color with which to highlight
	 * this entry, or NO_COLOR for no highlighting.
	 */
	int color;
} PrintEntry;


#define TYPE_INT 0
#define TYPE_LONGLONG 1
#define TYPE_SIZE 2
#define TYPE_POINTER 3
#define TYPE_BOOL 4

static void
print_entry_content (int entries_size, PrintEntry *entries, gboolean color_output)
{
	int i;
	for (i = 0; i < entries_size; ++i) {
		printf ("%s%s ", i == 0 ? "" : " ", entries [i].name);
		if (color_output && entries [i].color != NO_COLOR)
			/* Set foreground color, excluding black & white. */
			printf ("\x1B[%dm", 31 + (entries [i].color % 6));
		switch (entries [i].type) {
		case TYPE_INT:
			printf ("%d", *(int*) entries [i].data);
			break;
		case TYPE_LONGLONG:
			printf ("%lld", *(long long*) entries [i].data);
			break;
		case TYPE_SIZE:
			printf ("%lu", *(size_t*) entries [i].data);
			break;
		case TYPE_POINTER:
			printf ("%p", *(gpointer*) entries [i].data);
			break;
		case TYPE_BOOL:
			printf ("%s", *(gboolean*) entries [i].data ? "true" : "false");
			break;
		default:
			assert (0);
		}
		if (color_output && entries [i].color != NO_COLOR)
			/* Reset foreground color to default. */
			printf ("\x1B[0m");
	}
}

static int
index_color (int index, int num_nums, int *match_indices)
{
	int result;
	for (result = 0; result < num_nums + 1; ++result)
		if (index == match_indices [result])
			return result;
	return NO_COLOR;
}

static void
print_entry (int type, void *data, int num_nums, int *match_indices, gboolean color_output)
{
	const char *always_prefix = is_always_match (type) ? "  " : "";
	printf ("%s%s ", WORKER_PREFIX (type), always_prefix);

	switch (TYPE (type)) {

#define BEGIN_PROTOCOL_ENTRY0(method) \
	case PROTOCOL_ID(method): { \
		const int pes_size G_GNUC_UNUSED = 0; \
		PrintEntry pes [1] G_GNUC_UNUSED; \
		printf ("%s", #method + strlen ("binary_protocol_"));
#define BEGIN_PROTOCOL_ENTRY1(method,t1,f1) \
	case PROTOCOL_ID(method): { \
		PROTOCOL_STRUCT (method) *entry = data; \
		const int pes_size G_GNUC_UNUSED = 1; \
		PrintEntry pes [1] G_GNUC_UNUSED; \
		pes [0].type = t1; \
		pes [0].name = #f1; \
		pes [0].data = &entry->f1; \
		pes [0].color = index_color(0, num_nums, match_indices); \
		printf ("%s ", #method + strlen ("binary_protocol_"));
#define BEGIN_PROTOCOL_ENTRY2(method,t1,f1,t2,f2) \
	case PROTOCOL_ID(method): { \
		PROTOCOL_STRUCT (method) *entry = data; \
		const int pes_size G_GNUC_UNUSED = 2; \
		PrintEntry pes [2] G_GNUC_UNUSED; \
		pes [0].type = t1; \
		pes [0].name = #f1; \
		pes [0].data = &entry->f1; \
		pes [0].color = index_color(0, num_nums, match_indices); \
		pes [1].type = t2; \
		pes [1].name = #f2; \
		pes [1].data = &entry->f2; \
		pes [1].color = index_color(1, num_nums, match_indices); \
		printf ("%s ", #method + strlen ("binary_protocol_"));
#define BEGIN_PROTOCOL_ENTRY3(method,t1,f1,t2,f2,t3,f3) \
	case PROTOCOL_ID(method): { \
		PROTOCOL_STRUCT (method) *entry = data; \
		const int pes_size G_GNUC_UNUSED = 3; \
		PrintEntry pes [3] G_GNUC_UNUSED; \
		pes [0].type = t1; \
		pes [0].name = #f1; \
		pes [0].data = &entry->f1; \
		pes [0].color = index_color(0, num_nums, match_indices); \
		pes [1].type = t2; \
		pes [1].name = #f2; \
		pes [1].data = &entry->f2; \
		pes [1].color = index_color(1, num_nums, match_indices); \
		pes [2].type = t3; \
		pes [2].name = #f3; \
		pes [2].data = &entry->f3; \
		pes [2].color = index_color(2, num_nums, match_indices); \
		printf ("%s ", #method + strlen ("binary_protocol_"));
#define BEGIN_PROTOCOL_ENTRY4(method,t1,f1,t2,f2,t3,f3,t4,f4) \
	case PROTOCOL_ID(method): { \
		PROTOCOL_STRUCT (method) *entry = data; \
		const int pes_size G_GNUC_UNUSED = 4; \
		PrintEntry pes [4] G_GNUC_UNUSED; \
		pes [0].type = t1; \
		pes [0].name = #f1; \
		pes [0].data = &entry->f1; \
		pes [0].color = index_color(0, num_nums, match_indices); \
		pes [1].type = t2; \
		pes [1].name = #f2; \
		pes [1].data = &entry->f2; \
		pes [1].color = index_color(1, num_nums, match_indices); \
		pes [2].type = t3; \
		pes [2].name = #f3; \
		pes [2].data = &entry->f3; \
		pes [2].color = index_color(2, num_nums, match_indices); \
		pes [3].type = t4; \
		pes [3].name = #f4; \
		pes [3].data = &entry->f4; \
		pes [3].color = index_color(3, num_nums, match_indices); \
		printf ("%s ", #method + strlen ("binary_protocol_"));
#define BEGIN_PROTOCOL_ENTRY5(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5) \
	case PROTOCOL_ID(method): { \
		PROTOCOL_STRUCT (method) *entry = data; \
		const int pes_size G_GNUC_UNUSED = 5; \
		PrintEntry pes [5] G_GNUC_UNUSED; \
		pes [0].type = t1; \
		pes [0].name = #f1; \
		pes [0].data = &entry->f1; \
		pes [0].color = index_color(0, num_nums, match_indices); \
		pes [1].type = t2; \
		pes [1].name = #f2; \
		pes [1].data = &entry->f2; \
		pes [1].color = index_color(1, num_nums, match_indices); \
		pes [2].type = t3; \
		pes [2].name = #f3; \
		pes [2].data = &entry->f3; \
		pes [2].color = index_color(2, num_nums, match_indices); \
		pes [3].type = t4; \
		pes [3].name = #f4; \
		pes [3].data = &entry->f4; \
		pes [3].color = index_color(3, num_nums, match_indices); \
		pes [4].type = t5; \
		pes [4].name = #f5; \
		pes [4].data = &entry->f5; \
		pes [4].color = index_color(4, num_nums, match_indices); \
		printf ("%s ", #method + strlen ("binary_protocol_"));
#define BEGIN_PROTOCOL_ENTRY6(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5,t6,f6) \
	case PROTOCOL_ID(method): { \
		PROTOCOL_STRUCT (method) *entry = data; \
		const int pes_size G_GNUC_UNUSED = 6; \
		PrintEntry pes [6] G_GNUC_UNUSED; \
		pes [0].type = t1; \
		pes [0].name = #f1; \
		pes [0].data = &entry->f1; \
		pes [0].color = index_color(0, num_nums, match_indices); \
		pes [1].type = t2; \
		pes [1].name = #f2; \
		pes [1].data = &entry->f2; \
		pes [1].color = index_color(1, num_nums, match_indices); \
		pes [2].type = t3; \
		pes [2].name = #f3; \
		pes [2].data = &entry->f3; \
		pes [2].color = index_color(2, num_nums, match_indices); \
		pes [3].type = t4; \
		pes [3].name = #f4; \
		pes [3].data = &entry->f4; \
		pes [3].color = index_color(3, num_nums, match_indices); \
		pes [4].type = t5; \
		pes [4].name = #f5; \
		pes [4].data = &entry->f5; \
		pes [4].color = index_color(4, num_nums, match_indices); \
		pes [5].type = t6; \
		pes [5].name = #f6; \
		pes [5].data = &entry->f6; \
		pes [5].color = index_color(5, num_nums, match_indices); \
		printf ("%s ", #method + strlen ("binary_protocol_"));

#define BEGIN_PROTOCOL_ENTRY_HEAVY0(method) \
	BEGIN_PROTOCOL_ENTRY0 (method)
#define BEGIN_PROTOCOL_ENTRY_HEAVY1(method,t1,f1) \
	BEGIN_PROTOCOL_ENTRY1 (method,t1,f1)
#define BEGIN_PROTOCOL_ENTRY_HEAVY2(method,t1,f1,t2,f2) \
	BEGIN_PROTOCOL_ENTRY2 (method,t1,f1,t2,f2)
#define BEGIN_PROTOCOL_ENTRY_HEAVY3(method,t1,f1,t2,f2,t3,f3) \
	BEGIN_PROTOCOL_ENTRY3 (method,t1,f1,t2,f2,t3,f3)
#define BEGIN_PROTOCOL_ENTRY_HEAVY4(method,t1,f1,t2,f2,t3,f3,t4,f4) \
	BEGIN_PROTOCOL_ENTRY4 (method,t1,f1,t2,f2,t3,f3,t4,f4)
#define BEGIN_PROTOCOL_ENTRY_HEAVY5(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5) \
	BEGIN_PROTOCOL_ENTRY5 (method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5)
#define BEGIN_PROTOCOL_ENTRY_HEAVY6(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5,t6,f6) \
	BEGIN_PROTOCOL_ENTRY6 (method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5,t6,f6)

#define FLUSH()

#define DEFAULT_PRINT() \
	print_entry_content (pes_size, pes, color_output);
#define CUSTOM_PRINT(print) \
	print;

#define IS_ALWAYS_MATCH(_)
#define MATCH_INDEX(_)
#define IS_VTABLE_MATCH(_)

#define END_PROTOCOL_ENTRY \
		printf ("\n"); \
		break; \
	}
#define END_PROTOCOL_ENTRY_HEAVY \
	END_PROTOCOL_ENTRY

#include <mono/sgen/sgen-protocol-def.h>

	default: assert (0);
	}
}

#undef TYPE_INT
#undef TYPE_LONGLONG
#undef TYPE_SIZE
#undef TYPE_POINTER

#define TYPE_INT int
#define TYPE_LONGLONG long long
#define TYPE_SIZE size_t
#define TYPE_POINTER gpointer

static gboolean
matches_interval (gpointer ptr, gpointer start, int size)
{
	return ptr >= start && (char*)ptr < (char*)start + size;
}

/* Returns the index of the field where a match was found,
 * BINARY_PROTOCOL_NO_MATCH for no match, or
 * BINARY_PROTOCOL_MATCH for a match with no index.
 */
static int
match_index (gpointer ptr, int type, void *data)
{
	switch (TYPE (type)) {

#define BEGIN_PROTOCOL_ENTRY0(method) \
	case PROTOCOL_ID (method): {
#define BEGIN_PROTOCOL_ENTRY1(method,t1,f1) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = data;
#define BEGIN_PROTOCOL_ENTRY2(method,t1,f1,t2,f2) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = data;
#define BEGIN_PROTOCOL_ENTRY3(method,t1,f1,t2,f2,t3,f3) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = data;
#define BEGIN_PROTOCOL_ENTRY4(method,t1,f1,t2,f2,t3,f3,t4,f4) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = data;
#define BEGIN_PROTOCOL_ENTRY5(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = data;
#define BEGIN_PROTOCOL_ENTRY6(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5,t6,f6) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = data;

#define BEGIN_PROTOCOL_ENTRY_HEAVY0(method) \
	BEGIN_PROTOCOL_ENTRY0 (method)
#define BEGIN_PROTOCOL_ENTRY_HEAVY1(method,t1,f1) \
	BEGIN_PROTOCOL_ENTRY1 (method,t1,f1)
#define BEGIN_PROTOCOL_ENTRY_HEAVY2(method,t1,f1,t2,f2) \
	BEGIN_PROTOCOL_ENTRY2 (method,t1,f1,t2,f2)
#define BEGIN_PROTOCOL_ENTRY_HEAVY3(method,t1,f1,t2,f2,t3,f3) \
	BEGIN_PROTOCOL_ENTRY3 (method,t1,f1,t2,f2,t3,f3)
#define BEGIN_PROTOCOL_ENTRY_HEAVY4(method,t1,f1,t2,f2,t3,f3,t4,f4) \
	BEGIN_PROTOCOL_ENTRY4 (method,t1,f1,t2,f2,t3,f3,t4,f4)
#define BEGIN_PROTOCOL_ENTRY_HEAVY5(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5) \
	BEGIN_PROTOCOL_ENTRY5 (method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5)
#define BEGIN_PROTOCOL_ENTRY_HEAVY6(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5,t6,f6) \
	BEGIN_PROTOCOL_ENTRY6 (method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5,t6,f6)

#define FLUSH()

#define DEFAULT_PRINT()
#define CUSTOM_PRINT(_)

#define IS_ALWAYS_MATCH(_)
#define MATCH_INDEX(block) \
		return (block);
#define IS_VTABLE_MATCH(_)

#define END_PROTOCOL_ENTRY \
		break; \
	}
#define END_PROTOCOL_ENTRY_HEAVY \
	END_PROTOCOL_ENTRY

#include <mono/sgen/sgen-protocol-def.h>

	default: assert (0);
	}
}

static gboolean
is_vtable_match (gpointer ptr, int type, void *data)
{
	switch (TYPE (type)) {

#define BEGIN_PROTOCOL_ENTRY0(method) \
	case PROTOCOL_ID (method): {
#define BEGIN_PROTOCOL_ENTRY1(method,t1,f1) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = data;
#define BEGIN_PROTOCOL_ENTRY2(method,t1,f1,t2,f2) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = data;
#define BEGIN_PROTOCOL_ENTRY3(method,t1,f1,t2,f2,t3,f3) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = data;
#define BEGIN_PROTOCOL_ENTRY4(method,t1,f1,t2,f2,t3,f3,t4,f4) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = data;
#define BEGIN_PROTOCOL_ENTRY5(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = data;
#define BEGIN_PROTOCOL_ENTRY6(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5,t6,f6) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = data;

#define BEGIN_PROTOCOL_ENTRY_HEAVY0(method) \
	BEGIN_PROTOCOL_ENTRY0 (method)
#define BEGIN_PROTOCOL_ENTRY_HEAVY1(method,t1,f1) \
	BEGIN_PROTOCOL_ENTRY1 (method,t1,f1)
#define BEGIN_PROTOCOL_ENTRY_HEAVY2(method,t1,f1,t2,f2) \
	BEGIN_PROTOCOL_ENTRY2 (method,t1,f1,t2,f2)
#define BEGIN_PROTOCOL_ENTRY_HEAVY3(method,t1,f1,t2,f2,t3,f3) \
	BEGIN_PROTOCOL_ENTRY3 (method,t1,f1,t2,f2,t3,f3)
#define BEGIN_PROTOCOL_ENTRY_HEAVY4(method,t1,f1,t2,f2,t3,f3,t4,f4) \
	BEGIN_PROTOCOL_ENTRY4 (method,t1,f1,t2,f2,t3,f3,t4,f4)
#define BEGIN_PROTOCOL_ENTRY_HEAVY5(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5) \
	BEGIN_PROTOCOL_ENTRY5 (method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5)
#define BEGIN_PROTOCOL_ENTRY_HEAVY6(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5,t6,f6) \
	BEGIN_PROTOCOL_ENTRY6 (method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5,t6,f6)

#define FLUSH()

#define DEFAULT_PRINT()
#define CUSTOM_PRINT(_)

#define IS_ALWAYS_MATCH(_)
#define MATCH_INDEX(block) \
		return (block);
#define IS_VTABLE_MATCH(_)

#define END_PROTOCOL_ENTRY \
		break; \
	}
#define END_PROTOCOL_ENTRY_HEAVY \
	END_PROTOCOL_ENTRY

#include <mono/sgen/sgen-protocol-def.h>

	default: assert (0);
	}
}

#undef TYPE_INT
#undef TYPE_LONGLONG
#undef TYPE_SIZE
#undef TYPE_POINTER

int
main (int argc, char *argv[])
{
	int type;
	void *data = g_malloc0 (MAX_ENTRY_SIZE);
	int num_args = argc - 1;
	int num_nums = 0;
	int num_vtables = 0;
	int i;
	long nums [num_args];
	long vtables [num_args];
	gboolean dump_all = FALSE;
	gboolean pause_times = FALSE;
	gboolean pause_times_stopped = FALSE;
	gboolean pause_times_concurrent = FALSE;
	gboolean pause_times_finish = FALSE;
	gboolean color_output = FALSE;
	long long pause_times_ts = 0;
	const char *input_path = NULL;
	int input_file;
	EntryStream stream;
	unsigned long long entry_index;
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
	entry_index = 0;
	while ((type = read_entry (&stream, data)) != SGEN_PROTOCOL_EOF) {
		if (entry_index < first_entry_to_consider)
			goto next_entry;
		if (pause_times) {
			switch (type) {
			case PROTOCOL_ID (binary_protocol_world_stopping): {
				PROTOCOL_STRUCT (binary_protocol_world_stopping) *entry = data;
				assert (!pause_times_stopped);
				pause_times_concurrent = FALSE;
				pause_times_finish = FALSE;
				pause_times_ts = entry->timestamp;
				pause_times_stopped = TRUE;
				break;
			}
			case PROTOCOL_ID (binary_protocol_concurrent_finish):
				pause_times_finish = TRUE;
			case PROTOCOL_ID (binary_protocol_concurrent_start):
			case PROTOCOL_ID (binary_protocol_concurrent_update):
				pause_times_concurrent = TRUE;
				break;
			case PROTOCOL_ID (binary_protocol_world_restarted): {
				PROTOCOL_STRUCT (binary_protocol_world_restarted) *entry = data;
				assert (pause_times_stopped);
				printf ("pause-time %d %d %d %lld %lld\n",
						entry->generation,
						pause_times_concurrent,
						pause_times_finish,
						entry->timestamp - pause_times_ts,
						pause_times_ts);
				pause_times_stopped = FALSE;
				break;
			}
			}
		} else {
			int match_indices [num_nums + 1];
			gboolean match = is_always_match (type);
			match_indices [num_nums] = num_nums == 0 ? match_index (NULL, type, data) : BINARY_PROTOCOL_NO_MATCH;
			match = match_indices [num_nums] != BINARY_PROTOCOL_NO_MATCH;
			for (i = 0; i < num_nums; ++i) {
				match_indices [i] = match_index ((gpointer) nums [i], type, data);
				match = match || match_indices [i] != BINARY_PROTOCOL_NO_MATCH;
			}
			if (!match) {
				for (i = 0; i < num_vtables; ++i) {
					if (is_vtable_match ((gpointer) vtables [i], type, data)) {
						match = TRUE;
						break;
					}
				}
			}
			if (match || dump_all)
				printf ("%12lld ", entry_index);
			if (dump_all)
				printf (match ? "* " : "  ");
			if (match || dump_all)
				print_entry (type, data, num_nums, match_indices, color_output);
		}
	next_entry:
		++entry_index;
	}
	close_stream (&stream);
	if (input_path)
		close (input_file);
	g_free (data);

	return 0;
}
