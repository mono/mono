/*
 * sgen-grep-binprot.c: Platform specific binary protocol entries reader
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
#include <stdint.h>
#include <inttypes.h>
#include <config.h>
#include "sgen-entry-stream.h"
#include "sgen-grep-binprot.h"

static int file_version = 0;

#ifdef BINPROT_HAS_HEADER
#define PACKED_SUFFIX	p
#else
#define PROTOCOL_STRUCT_ATTR
#define PACKED_SUFFIX
#endif

#ifndef BINPROT_SIZEOF_VOID_P
#define BINPROT_SIZEOF_VOID_P SIZEOF_VOID_P
#define ARCH_SUFFIX
#endif

#if BINPROT_SIZEOF_VOID_P == 4
typedef int32_t mword;
#define MWORD_FORMAT_SPEC_D PRId32
#define MWORD_FORMAT_SPEC_P PRIx32
#ifndef ARCH_SUFFIX
#define ARCH_SUFFIX	32
#endif
#else
typedef int64_t mword;
#define MWORD_FORMAT_SPEC_D PRId64
#define MWORD_FORMAT_SPEC_P PRIx64
#ifndef ARCH_SUFFIX
#define ARCH_SUFFIX	64
#endif
#endif
#define TYPE_SIZE	mword
#define TYPE_POINTER	mword
#include <mono/sgen/sgen-protocol.h>

#define SGEN_PROTOCOL_EOF	255

#define TYPE(t)		((t) & 0x7f)
#define WORKER(t)	((t) & 0x80)

#define MAX_ENTRY_SIZE (1 << 10)

static int
read_entry (EntryStream *stream, void *data, unsigned char *windex)
{
	unsigned char type;
	ssize_t size;

	if (read_stream (stream, &type, 1) <= 0)
		return SGEN_PROTOCOL_EOF;

	if (windex) {
		if (file_version >= 2) {
			if (read_stream (stream, windex, 1) <= 0)
				return SGEN_PROTOCOL_EOF;
		} else {
			*windex = !!(WORKER (type));
		}
	}

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

#define DEFAULT_PRINT()
#define CUSTOM_PRINT(_)

#define IS_ALWAYS_MATCH(_)
#define MATCH_INDEX(_)
#define IS_VTABLE_MATCH(_)

#define END_PROTOCOL_ENTRY
#define END_PROTOCOL_ENTRY_FLUSH
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

#define DEFAULT_PRINT()
#define CUSTOM_PRINT(_)

#define IS_ALWAYS_MATCH(is_always_match) \
		return is_always_match;
#define MATCH_INDEX(_)
#define IS_VTABLE_MATCH(_)

#define END_PROTOCOL_ENTRY
#define END_PROTOCOL_ENTRY_FLUSH
#define END_PROTOCOL_ENTRY_HEAVY

#include <mono/sgen/sgen-protocol-def.h>

	default:
		assert (0);
		return FALSE;
	}
}

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
			printf ("%" MWORD_FORMAT_SPEC_D, *(mword*) entries [i].data);
			break;
		case TYPE_POINTER:
			printf ("0x%" MWORD_FORMAT_SPEC_P, *(mword*) entries [i].data);
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
print_entry (int type, void *data, int num_nums, int *match_indices, gboolean color_output, unsigned char worker_index)
{
	const char *always_prefix = is_always_match (type) ? "  " : "";
	if (worker_index)
		printf ("w%-2d%s ", worker_index, always_prefix);
	else
		printf ("   %s ", always_prefix);

	switch (TYPE (type)) {

#define BEGIN_PROTOCOL_ENTRY0(method) \
	case PROTOCOL_ID(method): { \
		const int pes_size G_GNUC_UNUSED = 0; \
		PrintEntry pes [1] G_GNUC_UNUSED; \
		printf ("%s", &#method [sizeof ("binary_protocol_") - 1]);
#define BEGIN_PROTOCOL_ENTRY1(method,t1,f1) \
	case PROTOCOL_ID(method): { \
		PROTOCOL_STRUCT (method) *entry = (PROTOCOL_STRUCT (method)*)data; \
		const int pes_size G_GNUC_UNUSED = 1; \
		PrintEntry pes [1] G_GNUC_UNUSED; \
		pes [0].type = t1; \
		pes [0].name = #f1; \
		pes [0].data = &entry->f1; \
		pes [0].color = index_color(0, num_nums, match_indices); \
		printf ("%s ", #method + strlen ("binary_protocol_"));
#define BEGIN_PROTOCOL_ENTRY2(method,t1,f1,t2,f2) \
	case PROTOCOL_ID(method): { \
		PROTOCOL_STRUCT (method) *entry = (PROTOCOL_STRUCT (method)*)data; \
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
		PROTOCOL_STRUCT (method) *entry = (PROTOCOL_STRUCT (method)*)data; \
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
		PROTOCOL_STRUCT (method) *entry = (PROTOCOL_STRUCT (method)*)data; \
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
		PROTOCOL_STRUCT (method) *entry = (PROTOCOL_STRUCT (method)*)data; \
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
		PROTOCOL_STRUCT (method) *entry = (PROTOCOL_STRUCT (method)*)data; \
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
#define END_PROTOCOL_ENTRY_FLUSH \
	END_PROTOCOL_ENTRY
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
#define TYPE_SIZE mword
#define TYPE_POINTER mword

static gboolean
matches_interval (mword ptr, mword start, int size)
{
	return ptr >= start && ptr < start + size;
}

/* Returns the index of the field where a match was found,
 * BINARY_PROTOCOL_NO_MATCH for no match, or
 * BINARY_PROTOCOL_MATCH for a match with no index.
 */
static int
match_index (mword ptr, int type, void *data)
{
	switch (TYPE (type)) {

#define BEGIN_PROTOCOL_ENTRY0(method) \
	case PROTOCOL_ID (method): {
#define BEGIN_PROTOCOL_ENTRY1(method,t1,f1) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = (PROTOCOL_STRUCT (method)*)data;
#define BEGIN_PROTOCOL_ENTRY2(method,t1,f1,t2,f2) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = (PROTOCOL_STRUCT (method)*)data;
#define BEGIN_PROTOCOL_ENTRY3(method,t1,f1,t2,f2,t3,f3) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = (PROTOCOL_STRUCT (method)*)data;
#define BEGIN_PROTOCOL_ENTRY4(method,t1,f1,t2,f2,t3,f3,t4,f4) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = (PROTOCOL_STRUCT (method)*)data;
#define BEGIN_PROTOCOL_ENTRY5(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = (PROTOCOL_STRUCT (method)*)data;
#define BEGIN_PROTOCOL_ENTRY6(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5,t6,f6) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = (PROTOCOL_STRUCT (method)*)data;

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

#define DEFAULT_PRINT()
#define CUSTOM_PRINT(_)

#define IS_ALWAYS_MATCH(_)
#define MATCH_INDEX(block) \
		return (block);
#define IS_VTABLE_MATCH(_)

#define END_PROTOCOL_ENTRY \
		break; \
	}
#define END_PROTOCOL_ENTRY_FLUSH \
	END_PROTOCOL_ENTRY
#define END_PROTOCOL_ENTRY_HEAVY \
	END_PROTOCOL_ENTRY

#include <mono/sgen/sgen-protocol-def.h>

	default:
		assert (0);
		return 0;
	}
}

static gboolean
is_vtable_match (mword ptr, int type, void *data)
{
	switch (TYPE (type)) {

#define BEGIN_PROTOCOL_ENTRY0(method) \
	case PROTOCOL_ID (method): {
#define BEGIN_PROTOCOL_ENTRY1(method,t1,f1) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = (PROTOCOL_STRUCT (method)*)data;
#define BEGIN_PROTOCOL_ENTRY2(method,t1,f1,t2,f2) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = (PROTOCOL_STRUCT (method)*)data;
#define BEGIN_PROTOCOL_ENTRY3(method,t1,f1,t2,f2,t3,f3) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = (PROTOCOL_STRUCT (method)*)data;
#define BEGIN_PROTOCOL_ENTRY4(method,t1,f1,t2,f2,t3,f3,t4,f4) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = (PROTOCOL_STRUCT (method)*)data;
#define BEGIN_PROTOCOL_ENTRY5(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = (PROTOCOL_STRUCT (method)*)data;
#define BEGIN_PROTOCOL_ENTRY6(method,t1,f1,t2,f2,t3,f3,t4,f4,t5,f5,t6,f6) \
	case PROTOCOL_ID (method): { \
		PROTOCOL_STRUCT (method) *entry G_GNUC_UNUSED = (PROTOCOL_STRUCT (method)*)data;

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

#define DEFAULT_PRINT()
#define CUSTOM_PRINT(_)

#define IS_ALWAYS_MATCH(_)
#define MATCH_INDEX(block) \
		return (block);
#define IS_VTABLE_MATCH(_)

#define END_PROTOCOL_ENTRY \
		break; \
	}
#define END_PROTOCOL_ENTRY_FLUSH \
	END_PROTOCOL_ENTRY
#define END_PROTOCOL_ENTRY_HEAVY \
	END_PROTOCOL_ENTRY

#include <mono/sgen/sgen-protocol-def.h>

	default:
		assert (0);
		return FALSE;
	}
}

#undef TYPE_INT
#undef TYPE_LONGLONG
#undef TYPE_SIZE
#undef TYPE_POINTER

static gboolean
sgen_binary_protocol_read_header (EntryStream *stream)
{
#ifdef BINPROT_HAS_HEADER
	char data [MAX_ENTRY_SIZE];
	int type = read_entry (stream, data, NULL);
	if (type == SGEN_PROTOCOL_EOF)
		return FALSE;
	if (type == PROTOCOL_ID (binary_protocol_header)) {
		PROTOCOL_STRUCT (binary_protocol_header) * str = (PROTOCOL_STRUCT (binary_protocol_header) *) data;
		if (str->check == PROTOCOL_HEADER_CHECK && str->ptr_size == BINPROT_SIZEOF_VOID_P) {
			if (str->version > PROTOCOL_HEADER_VERSION) {
				fprintf (stderr, "The file contains a newer version %d. We support up to %d. Please update.\n", str->version, PROTOCOL_HEADER_VERSION);
				exit (1);
			}
			file_version = str->version;
			return TRUE;
		}
	}
	return FALSE;
#else
	/*
	 * This implementation doesn't account for the presence of a header,
	 * reading all the entries with the default configuration of the host
	 * machine. It has to be used only after all other implementations
	 * fail to identify a header, for backward compatibility.
	 */
	return TRUE;
#endif
}

#define CONC(A, B) CONC_(A, B)
#define CONC_(A, B) A##B
#define GREP_ENTRIES_FUNCTION_NAME CONC(sgen_binary_protocol_grep_entries, CONC(ARCH_SUFFIX,PACKED_SUFFIX))

gboolean
GREP_ENTRIES_FUNCTION_NAME (EntryStream *stream, int num_nums, long nums [], int num_vtables, long vtables [],
			gboolean dump_all, gboolean pause_times, gboolean color_output, unsigned long long first_entry_to_consider)
{
	int type;
	unsigned char worker_index;
	void *data = g_malloc0 (MAX_ENTRY_SIZE);
	int i;
	gboolean pause_times_stopped = FALSE;
	gboolean pause_times_concurrent = FALSE;
	gboolean pause_times_finish = FALSE;
	long long pause_times_ts = 0;
	unsigned long long entry_index;

	if (!sgen_binary_protocol_read_header (stream))
		return FALSE;

	entry_index = 0;
	while ((type = read_entry (stream, data, &worker_index)) != SGEN_PROTOCOL_EOF) {
		if (entry_index < first_entry_to_consider)
			goto next_entry;
		if (pause_times) {
			switch (type) {
			case PROTOCOL_ID (binary_protocol_world_stopping): {
				PROTOCOL_STRUCT (binary_protocol_world_stopping) *entry = (PROTOCOL_STRUCT (binary_protocol_world_stopping)*)data;
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
				PROTOCOL_STRUCT (binary_protocol_world_restarted) *entry = (PROTOCOL_STRUCT (binary_protocol_world_restarted)*)data;
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
			match_indices [num_nums] = num_nums == 0 ? match_index (0, type, data) : BINARY_PROTOCOL_NO_MATCH;
			match = match_indices [num_nums] != BINARY_PROTOCOL_NO_MATCH;
			for (i = 0; i < num_nums; ++i) {
				match_indices [i] = match_index ((mword) nums [i], type, data);
				match = match || match_indices [i] != BINARY_PROTOCOL_NO_MATCH;
			}
			if (!match) {
				for (i = 0; i < num_vtables; ++i) {
					if (is_vtable_match ((mword) vtables [i], type, data)) {
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
				print_entry (type, data, num_nums, match_indices, color_output, worker_index);
		}
	next_entry:
		++entry_index;
	}
	g_free (data);
	return TRUE;
}
