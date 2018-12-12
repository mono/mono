/**
 * \file
 * Culture-sensitive handling
 *
 * Authors:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2003 Ximian, Inc.
 */

#ifndef _MONO_METADATA_LOCALES_H_
#define _MONO_METADATA_LOCALES_H_

#include <glib.h>

#include <mono/metadata/object-internals.h>
#include <mono/metadata/icalls.h>

/* This is a copy of System.Globalization.CompareOptions */
typedef enum {
	CompareOptions_None=0x00,
	CompareOptions_IgnoreCase=0x01,
	CompareOptions_IgnoreNonSpace=0x02,
	CompareOptions_IgnoreSymbols=0x04,
	CompareOptions_IgnoreKanaType=0x08,
	CompareOptions_IgnoreWidth=0x10,
	CompareOptions_StringSort=0x20000000,
	CompareOptions_Ordinal=0x40000000
} MonoCompareOptions;

ICALL_EXPORT int
ves_icall_System_Globalization_CompareInfo_internal_compare (const gunichar2 *str1, gint32 len1,
	const gunichar2 *str2, gint32 len2, gint32 options);

ICALL_EXPORT int
ves_icall_System_Globalization_CompareInfo_internal_index (const gunichar2 *source, gint32 sindex,
	gint32 count, const gunichar2 *value, int value_length, MonoBoolean first);

#define MONO_LOCALE_INVARIANT (0x007F)

#endif /* _MONO_METADATA_FILEIO_H_ */
