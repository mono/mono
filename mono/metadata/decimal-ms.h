/**
 * \file
 */

#ifndef __MONO_DECIMAL_MS_H__
#define __MONO_DECIMAL_MS_H__

typedef struct {
    // Decimal.cs treats the first two shorts as one long
    // And they seriable the data so we need to little endian
    // seriliazation
    // The wReserved overlaps with Variant's vt member
#if G_BYTE_ORDER != G_LITTLE_ENDIAN
    union {
        struct {
            uint8_t sign;
            uint8_t scale;
        } u;
        uint16_t signscale;
    } u;
    uint16_t reserved;
#else
    uint16_t reserved;
    union {
        struct {
            uint8_t scale;
            uint8_t sign;
        } u;
        uint16_t signscale;
    } u;
#endif
    uint32_t Hi32;
    union {
        struct {
            uint32_t Lo32;
            uint32_t Mid32;
        } v;
        uint64_t Lo64;
    } v;
} MonoDecimal;

typedef enum {
	MONO_DECIMAL_CMP_LT=-1,
	MONO_DECIMAL_CMP_EQ,
	MONO_DECIMAL_CMP_GT
} MonoDecimalCompareResult;
	
MonoDecimalCompareResult
mono_decimal_compare (MonoDecimal *left, MonoDecimal *right, MonoError *error);

void
mono_decimal_init_single   (MonoDecimal *_this, float value, MonoError *error);

void
mono_decimal_init_double   (MonoDecimal *_this, double value, MonoError *error);

void
mono_decimal_floor         (MonoDecimal *d, MonoError *error);

int32_t
mono_decimal_get_hash_code (MonoDecimal *d, MonoError *error);

void
mono_decimal_multiply      (MonoDecimal *d1, MonoDecimal *d2, MonoError *error);

void
mono_decimal_round         (MonoDecimal *d, int32_t decimals, MonoError *error);

void
mono_decimal_tocurrency    (MonoDecimal *decimal, MonoError *error);

double
mono_decimal_to_double     (MonoDecimal d, MonoError *error);

int32_t
mono_decimal_to_int32      (MonoDecimal d, MonoError *error);

float
mono_decimal_to_float      (MonoDecimal d, MonoError *error);

void
mono_decimal_truncate      (MonoDecimal *d, MonoError *error);

void
mono_decimal_addsub        (MonoDecimal *left, MonoDecimal *right, uint8_t sign, MonoError *error);

void
mono_decimal_divide        (MonoDecimal *left, MonoDecimal *right, MonoError *error);

int
mono_decimal_from_number   (void *from, MonoDecimal *target, MonoError *error);

#endif
