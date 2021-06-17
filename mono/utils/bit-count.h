#ifndef __BIT_COUNT_H__
#define __BIT_COUNT_H__

# include <glib.h>

#if defined (__GNUC__) || defined (__clang__)

static inline guint32 leading_zero_bit_count_32(guint32 v)
{
	if ( __builtin_expect( v == 0, 0 ) ) {
		return 32;
	}

	return __builtin_clz(v);
}

#elif defined (_MSC_VER)

# include <intrin.h>

# pragma intrinsic(_BitScanReverse)

static inline guint32 leading_zero_bit_count_32(guint32 v)
{
	unsigned long lz = 0;

	if ( !_BitScanReverse( &lz, v ) ) {
		return 32;
	}

	return 31 - (guint32)lz;
}

#else

static inline guint32 leading_zero_bit_count_32(guint32 v)
{
	guint32 result;

	if ( !v ) {
		return 32;
	}

	result = 0;
	while ( ( v & (0x80000000 >> result) ) == 0 ) {
		result ++;
	}

	return result;
}

#endif // platform selection

#endif // __BIT_COUNT_H__
