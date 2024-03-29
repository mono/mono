/**
 * \file
 * Support for the portable PDB symbol file format
 *
 *
 * Author:
 *	Mono Project (http://www.mono-project.com)
 *
 * Copyright 2015 Xamarin Inc (http://www.xamarin.com)
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#ifndef __MONO_METADATA_DEBUG_MONO_PPDB_H__
#define __MONO_METADATA_DEBUG_MONO_PPDB_H__

#include <config.h>
#include <mono/metadata/metadata-internals.h>
#include <mono/metadata/mono-debug.h>

MonoPPDBFile*
mono_ppdb_load_file (MonoImage *image, const guint8 *raw_contents, int size);

void
mono_ppdb_close (MonoDebugHandle *handle);

MonoDebugMethodInfo *
mono_ppdb_lookup_method (MonoDebugHandle *handle, MonoMethod *method);

MonoDebugSourceLocation *
mono_ppdb_lookup_location (MonoDebugMethodInfo *minfo, uint32_t offset);

void
mono_ppdb_get_seq_points (MonoDebugMethodInfo *minfo, char **source_file, GPtrArray **source_file_list, int **source_files, MonoSymSeqPoint **seq_points, int *n_seq_points);

MonoDebugLocalsInfo*
mono_ppdb_lookup_locals (MonoDebugMethodInfo *minfo);

MonoDebugMethodAsyncInfo*
mono_ppdb_lookup_method_async_debug_info (MonoDebugMethodInfo *minfo);

MonoImage *
mono_ppdb_get_image (MonoPPDBFile *ppdb);

char *
mono_ppdb_get_sourcelink (MonoDebugHandle *handle);

gboolean 
mono_ppdb_is_embedded (MonoPPDBFile *ppdb);

gboolean
mono_get_pe_debug_info_full (MonoImage *image, guint8 *out_guid, gint32 *out_age, gint32 *out_timestamp, guint8 **ppdb_data,
                                int *ppdb_uncompressed_size, int *ppdb_compressed_size, char **pdb_path, GArray *pdb_checksum_hash_type, GArray *pdb_checksum);
#endif
