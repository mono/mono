/*
 * test-sgen-qsort.c: Our own bzero/memmove.
 *
 * Copyright (C) 2013 Xamarin Inc
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Library General Public
 * License 2.0 as published by the Free Software Foundation;
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Library General Public License for more details.
 *
 * You should have received a copy of the GNU Library General Public
 * License 2.0 along with this library; if not, write to the Free
 * Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 */

/*
 * SGen cannot deal with invalid pointers on the heap or in registered roots.  Sometimes we
 * need to copy or zero out memory in code that might be interrupted by collections.  To
 * guarantee that those operations will not result in invalid pointers, we must do it
 * word-atomically.
 *
 * libc's bzero() and memcpy()/memmove() functions do not guarantee word-atomicity, even in
 * cases where one would assume so.  For instance, some implementations (like Darwin's on
 * x86) have variants of memcpy() using vector instructions.  Those may copy bytewise for
 * the region preceding the first vector-aligned address.  That region could be
 * word-aligned, but it would still be copied byte-wise.
 *
 * All our memory writes here are to "volatile" locations.  This is so that C compilers
 * don't "optimize" our code back to calls to bzero()/memmove().  LLVM, specifically, will
 * do that.
 */

#include <config.h>

#include "metadata/gc-internal.h"

#define ptr_mask ((sizeof (void*) - 1))
#define _toi(ptr) ((size_t)ptr)
#define unaligned_bytes(ptr) (_toi(ptr) & ptr_mask)
#define align_down(ptr) ((void*)(_toi(ptr) & ~ptr_mask))
#define align_up(ptr) ((void*) ((_toi(ptr) + ptr_mask) & ~ptr_mask))
#if SIZEOF_VOID_P == 4
#define bytes_to_words(n)	((size_t)(n) >> 2)
#elif SIZEOF_VOID_P == 8
#define bytes_to_words(n)	((size_t)(n) >> 3)
#else
#error We only support 32 and 64 bit architectures.
#endif

#define BZERO_WORDS(dest,words) do {			\
		void * volatile *__d = (void* volatile*)(dest);		\
		int __n = (words);			\
		int __i;				\
		for (__i = 0; __i < __n; ++__i)		\
			__d [__i] = NULL;		\
	} while (0)

/**
 * mono_gc_bzero:
 * @dest: address to start to clear
 * @size: size of the region to clear
 *
 * Zero @size bytes starting at @dest.
 *
 * Use this to zero memory that can hold managed pointers.
 *
 * FIXME borrow faster code from some BSD libc or bionic
 */
void
mono_gc_bzero (void *dest, size_t size)
{
	volatile char *d = (char*)dest;
	size_t tail_bytes, word_bytes;

	/*
	If we're copying less than a word, just use memset.

	We cannot bail out early if both are aligned because some implementations
	use byte copying for sizes smaller than 16. OSX, on this case.
	*/
	if (size < sizeof(void*)) {
		memset (dest, 0, size);
		return;
	}

	/*align to word boundary */
	while (unaligned_bytes (d) && size) {
		*d++ = 0;
		--size;
	}

	/* copy all words with memmove */
	word_bytes = (size_t)align_down (size);
	switch (word_bytes) {
	case sizeof (void*) * 1:
		BZERO_WORDS (d, 1);
		break;
	case sizeof (void*) * 2:
		BZERO_WORDS (d, 2);
		break;
	case sizeof (void*) * 3:
		BZERO_WORDS (d, 3);
		break;
	case sizeof (void*) * 4:
		BZERO_WORDS (d, 4);
		break;
	default:
		BZERO_WORDS (d, bytes_to_words (word_bytes));
	}

	tail_bytes = unaligned_bytes (size);
	if (tail_bytes) {
		d += word_bytes;
		do {
			*d++ = 0;
		} while (--tail_bytes);
	}
}

#define MEMMOVE_WORDS_UPWARD(dest,src,words) do {	\
		void * volatile *__d = (void* volatile*)(dest);		\
		void **__s = (void**)(src);		\
		int __n = (int)(words);			\
		int __i;				\
		for (__i = 0; __i < __n; ++__i)		\
			__d [__i] = __s [__i];		\
	} while (0)

#define MEMMOVE_WORDS_DOWNWARD(dest,src,words) do {	\
		void * volatile *__d = (void* volatile*)(dest);		\
		void **__s = (void**)(src);		\
		int __n = (int)(words);			\
		int __i;				\
		for (__i = __n - 1; __i >= 0; --__i)	\
			__d [__i] = __s [__i];		\
	} while (0)

/**
 * mono_gc_memmove:
 * @dest: destination of the move
 * @src: source
 * @size: size of the block to move
 *
 * Move @size bytes from @src to @dest.
 * size MUST be a multiple of sizeof (gpointer)
 *
 */
void
mono_gc_memmove (void *dest, const void *src, size_t size)
{
	/*
	If we're copying less than a word we don't need to worry about word tearing
	so we bailout to memmove early.
	*/
	if (size < sizeof(void*)) {
		memmove (dest, src, size);
		return;
	}

	/*
	 * A bit of explanation on why we align only dest before doing word copies.
	 * Pointers to managed objects must always be stored in word aligned addresses, so
	 * even if dest is misaligned, src will be by the same amount - this ensure proper atomicity of reads.
	 *
	 * We don't need to case when source and destination have different alignments since we only do word stores
	 * using memmove, which must handle it.
	 */
	if (dest > src && ((size_t)((char*)dest - (char*)src) < size)) { /*backward copy*/
		volatile char *p = (char*)dest + size;
			char *s = (char*)src + size;
			char *start = (char*)dest;
			char *align_end = MAX((char*)dest, (char*)align_down (p));
			char *word_start;
			size_t bytes_to_memmove;

			while (p > align_end)
				*--p = *--s;

			word_start = align_up (start);
			bytes_to_memmove = p - word_start;
			p -= bytes_to_memmove;
			s -= bytes_to_memmove;
			MEMMOVE_WORDS_DOWNWARD (p, s, bytes_to_words (bytes_to_memmove));

			while (p > start)
				*--p = *--s;
	} else {
		volatile char *d = (char*)dest;
		const char *s = (const char*)src;
		size_t tail_bytes;

		/*align to word boundary */
		while (unaligned_bytes (d)) {
			*d++ = *s++;
			--size;
		}

		/* copy all words with memmove */
		MEMMOVE_WORDS_UPWARD (d, s, bytes_to_words (align_down (size)));

		tail_bytes = unaligned_bytes (size);
		if (tail_bytes) {
			d += (size_t)align_down (size);
			s += (size_t)align_down (size);
			do {
				*d++ = *s++;
			} while (--tail_bytes);
		}
	}
}
