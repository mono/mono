/*
 * sgen-entry-stream.h: EntryStream definitions
 *
 * Copyright (C) 2016 Xamarin Inc
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

typedef struct {
	int file;
	char *buffer;
	const char *end;
	const char *pos;
} EntryStream;

void init_stream (EntryStream *stream, int file);
void reset_stream (EntryStream *stream);
void close_stream (EntryStream *stream);
gboolean refill_stream (EntryStream *in, size_t size);
ssize_t read_stream (EntryStream *stream, void *out, size_t size);
