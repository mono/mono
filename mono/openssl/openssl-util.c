//
//  openssl-util.c
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/23/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#include "openssl-util.h"
#include <assert.h>
#include <openssl/err.h>
#include <openssl/asn1t.h>

ASN1_ITEM x509_it;

extern int asn1_generalizedtime_to_tm (struct tm *tm, const ASN1_GENERALIZEDTIME *d);

extern int64_t openssl_timegm64 (const struct tm *date);


MONO_API void
mono_openssl_free (void *data)
{
	OPENSSL_free (data);
}

int64_t
mono_openssl_util_asn1_time_to_ticks (ASN1_TIME *time)
{
	ASN1_GENERALIZEDTIME *gtime;
	struct tm tm;
	int64_t epoch;
	int ret;
	
	memset (&tm, 0, sizeof (tm));

	gtime = ASN1_TIME_to_generalizedtime (time, NULL);
	ret = asn1_generalizedtime_to_tm (&tm, gtime);
	ASN1_GENERALIZEDTIME_free (gtime);

	/* FIXME: check the return value in managed code */
	if (ret == 0) {
		return 0;
	}

	epoch = openssl_timegm64 (&tm);

	return epoch;
}

// Copied from crypto/bio/printf.c, takes va_list
int
mono_openssl_debug_printf (BIO *bio, const char *format, va_list args)
{
	char buf[256], *out, out_malloced = 0;
	int out_len, ret;

	out_len = vsnprintf (buf, sizeof(buf), format, args);
	if (out_len < 0) {
		return -1;
	}

	if ((size_t) out_len >= sizeof(buf)) {
		const int requested_len = out_len;
		/* The output was truncated. Note that vsnprintf's return value
		 * does not include a trailing NUL, but the buffer must be sized
		 * for it. */
		out = OPENSSL_malloc (requested_len + 1);
		out_malloced = 1;
		if (out == NULL) {
			OPENSSL_PUT_ERROR(BIO, ERR_R_MALLOC_FAILURE);
			return -1;
		}
		out_len = vsnprintf (out, requested_len + 1, format, args);
		assert(out_len == requested_len);
	} else {
		out = buf;
	}

	ret = BIO_write(bio, out, out_len);
	if (out_malloced) {
		OPENSSL_free(out);
	}

	return ret;
}

void 
CBB_zero(CBB *cbb) 
{
	memset(cbb, 0, sizeof(CBB));
}

static int 
cbb_init(CBB *cbb, uint8_t *buf, size_t cap) 
{
	/* This assumes that |cbb| has already been zeroed. */
	struct cbb_buffer_st *base;

	base = OPENSSL_malloc(sizeof(struct cbb_buffer_st));
	if (base == NULL) {
		return 0;
	}

	base->buf = buf;
	base->len = 0;
	base->cap = cap;
	base->can_resize = 1;

	cbb->base = base;
	cbb->is_top_level = 1;
	return 1;
}

int 
CBB_init(CBB *cbb, size_t initial_capacity) 
{
	CBB_zero(cbb);

	uint8_t *buf = OPENSSL_malloc(initial_capacity);
	if (initial_capacity > 0 && buf == NULL) {
		return 0;
	}

	if (!cbb_init(cbb, buf, initial_capacity)) {
		OPENSSL_free(buf);
		return 0;
	}

	return 1;
}

int 
CBB_init_fixed(CBB *cbb, uint8_t *buf, size_t len) 
{
	CBB_zero(cbb);

	if (!cbb_init(cbb, buf, len)) {
		return 0;
	}

	cbb->base->can_resize = 0;
	return 1;
}

void 
CBB_cleanup(CBB *cbb) 
{
	if (cbb->base) {
		/* Only top-level |CBB|s are cleaned up. Child |CBB|s are non-owning. They
		 * are implicitly discarded when the parent is flushed or cleaned up. 
		 */
		assert(cbb->is_top_level);

		if (cbb->base->can_resize) {
			OPENSSL_free(cbb->base->buf);
		}
		OPENSSL_free(cbb->base);
	}
	cbb->base = NULL;
}

static int 
cbb_buffer_reserve(struct cbb_buffer_st *base, uint8_t **out, size_t len) 
{
	size_t newlen;

	if (base == NULL) {
		return 0;
	}

	newlen = base->len + len;
	if (newlen < base->len) {
		/* Overflow */
		return 0;
	}

	if (newlen > base->cap) {
		size_t newcap = base->cap * 2;
		uint8_t *newbuf;

		if (!base->can_resize) {
			return 0;
		}

		if (newcap < base->cap || newcap < newlen) {
			newcap = newlen;
		}
		newbuf = OPENSSL_realloc(base->buf, newcap);
		if (newbuf == NULL) {
			return 0;
		}

		base->buf = newbuf;
		base->cap = newcap;
	}

	if (out) {
		*out = base->buf + base->len;
	}

	return 1;
}

static int 
cbb_buffer_add(struct cbb_buffer_st *base, uint8_t **out, size_t len) 
{
	if (!cbb_buffer_reserve(base, out, len)) {
		return 0;
	}
	/* This will not overflow or |cbb_buffer_reserve| would have failed. */
	base->len += len;
	return 1;
}

static int 
cbb_buffer_add_u(struct cbb_buffer_st *base, uint32_t v, size_t len_len) 
{
	uint8_t *buf;
	size_t i;

	if (len_len == 0) {
		return 1;
	}
	if (!cbb_buffer_add(base, &buf, len_len)) {
		return 0;
	}

	for (i = len_len - 1; i < len_len; i--) {
		buf[i] = v;
		v >>= 8;
	}
	return 1;
}

int 
CBB_finish(CBB *cbb, uint8_t **out_data, size_t *out_len) 
{
	if (!cbb->is_top_level) {
		return 0;
	}

	if (!CBB_flush(cbb)) {
		return 0;
	}

	if (cbb->base->can_resize && (out_data == NULL || out_len == NULL)) {
		/* |out_data| and |out_len| can only be NULL if the CBB is fixed. */
		return 0;
	}

	if (out_data != NULL) {
		*out_data = cbb->base->buf;
	}

	if (out_len != NULL) {
		*out_len = cbb->base->len;
	}

	cbb->base->buf = NULL;
	CBB_cleanup(cbb);
	return 1;
}

/* CBB_flush recurses and then writes out any pending length prefix. The
 * current length of the underlying base is taken to be the length of the
 * length-prefixed data. 
 */
int 
CBB_flush(CBB *cbb) 
{
	size_t child_start, i, len;

	if (cbb->base == NULL) {
		return 0;
	}

	if (cbb->child == NULL || cbb->child->pending_len_len == 0) {
		return 1;
	}

	child_start = cbb->child->offset + cbb->child->pending_len_len;

	if (!CBB_flush(cbb->child) ||
		child_start < cbb->child->offset ||
		cbb->base->len < child_start) {
		return 0;
	}

	len = cbb->base->len - child_start;

	if (cbb->child->pending_is_asn1) {
		/* For ASN.1 we assume that we'll only need a single byte for the length.
		 * If that turned out to be incorrect, we have to move the contents along
		 * in order to make space. 
		 */
		size_t len_len;
		uint8_t initial_length_byte;

		assert (cbb->child->pending_len_len == 1);

		if (len > 0xfffffffe) {
			/* Too large. */
			return 0;
		} else if (len > 0xffffff) {
			len_len = 5;
			initial_length_byte = 0x80 | 4;
		} else if (len > 0xffff) {
			len_len = 4;
			initial_length_byte = 0x80 | 3;
		} else if (len > 0xff) {
			len_len = 3;
			initial_length_byte = 0x80 | 2;
		} else if (len > 0x7f) {
			len_len = 2;
			initial_length_byte = 0x80 | 1;
		} else {
			len_len = 1;
			initial_length_byte = len;
			len = 0;
		}

		if (len_len != 1) {
			/* We need to move the contents along in order to make space. */
			size_t extra_bytes = len_len - 1;
			if (!cbb_buffer_add(cbb->base, NULL, extra_bytes)) {
				return 0;
			}
			memmove(cbb->base->buf + child_start + extra_bytes,
			cbb->base->buf + child_start, len);
		}
		cbb->base->buf[cbb->child->offset++] = initial_length_byte;
		cbb->child->pending_len_len = len_len - 1;
	}

	for (i = cbb->child->pending_len_len - 1; 
	     i < cbb->child->pending_len_len;
	     i--) {
		cbb->base->buf[cbb->child->offset + i] = len;
		len >>= 8;
	}
	if (len != 0) {
		return 0;
	}

	cbb->child->base = NULL;
	cbb->child = NULL;

	return 1;
}

const uint8_t *
CBB_data(const CBB *cbb) 
{
	assert(cbb->child == NULL);
	return cbb->base->buf + cbb->offset + cbb->pending_len_len;
}

size_t 
CBB_len(const CBB *cbb) 
{
	assert(cbb->child == NULL);
	assert(cbb->offset + cbb->pending_len_len <= cbb->base->len);

	return cbb->base->len - cbb->offset - cbb->pending_len_len;
}

static int 
cbb_add_length_prefixed(CBB *cbb, CBB *out_contents, size_t len_len) 
{
	uint8_t *prefix_bytes;

	if (!CBB_flush(cbb)) {
		return 0;
	}

	size_t offset = cbb->base->len;
	if (!cbb_buffer_add(cbb->base, &prefix_bytes, len_len)) {
		return 0;
	}

	memset(prefix_bytes, 0, len_len);
	memset(out_contents, 0, sizeof(CBB));
	out_contents->base = cbb->base;
	cbb->child = out_contents;
	cbb->child->offset = offset;
	cbb->child->pending_len_len = len_len;
	cbb->child->pending_is_asn1 = 0;

	return 1;
}

int 
CBB_add_u8_length_prefixed(CBB *cbb, CBB *out_contents) 
{
	return cbb_add_length_prefixed(cbb, out_contents, 1);
}

int 
CBB_add_u16_length_prefixed(CBB *cbb, CBB *out_contents) 
{
	return cbb_add_length_prefixed(cbb, out_contents, 2);
}

int 
CBB_add_u24_length_prefixed(CBB *cbb, CBB *out_contents) 
{
	return cbb_add_length_prefixed(cbb, out_contents, 3);
}

int 
CBB_add_asn1(CBB *cbb, CBB *out_contents, uint8_t tag) 
{
	if ((tag & 0x1f) == 0x1f) {
		/* Long form identifier octets are not supported. */
		return 0;
	}

	if (!CBB_flush(cbb) ||
	    !CBB_add_u8(cbb, tag)) {
		return 0;
	}

	size_t offset = cbb->base->len;
	if (!CBB_add_u8(cbb, 0)) {
		return 0;
	}

	memset(out_contents, 0, sizeof(CBB));
	out_contents->base = cbb->base;
	cbb->child = out_contents;
	cbb->child->offset = offset;
	cbb->child->pending_len_len = 1;
	cbb->child->pending_is_asn1 = 1;

	return 1;
}

int 
CBB_add_bytes(CBB *cbb, const uint8_t *data, size_t len) 
{
	uint8_t *dest;

	if (!CBB_flush(cbb) ||
	    !cbb_buffer_add(cbb->base, &dest, len)) {
		return 0;
	}
	memcpy(dest, data, len);
	return 1;
}

int 
CBB_add_space(CBB *cbb, uint8_t **out_data, size_t len) 
{
	if (!CBB_flush(cbb) ||
	    !cbb_buffer_add(cbb->base, out_data, len)) {
		return 0;
	}
	return 1;
}

int 
CBB_reserve(CBB *cbb, uint8_t **out_data, size_t len) 
{
	if (!CBB_flush(cbb) ||
	    !cbb_buffer_reserve(cbb->base, out_data, len)) {
		return 0;
	}
	return 1;
}

int 
CBB_did_write(CBB *cbb, size_t len) 
{
	size_t newlen = cbb->base->len + len;
	if (cbb->child != NULL ||
		newlen < cbb->base->len ||
		newlen > cbb->base->cap) {
		return 0;
	}
	cbb->base->len = newlen;
	return 1;
}

int 
CBB_add_u8(CBB *cbb, uint8_t value) 
{
	if (!CBB_flush(cbb)) {
		return 0;
	}

	return cbb_buffer_add_u(cbb->base, value, 1);
}

int 
CBB_add_u16(CBB *cbb, uint16_t value) 
{
	if (!CBB_flush(cbb)) {
		return 0;
	}

	return cbb_buffer_add_u(cbb->base, value, 2);
}

int 
CBB_add_u24(CBB *cbb, uint32_t value) 
{
	if (!CBB_flush(cbb)) {
		return 0;
	}

	return cbb_buffer_add_u(cbb->base, value, 3);
}

void 
CBB_discard_child(CBB *cbb) 
{
	if (cbb->child == NULL) {
		return;
	}

	cbb->base->len = cbb->child->offset;

	cbb->child->base = NULL;
	cbb->child = NULL;
}

int 
CBB_add_asn1_uint64(CBB *cbb, uint64_t value) 
{
	CBB child;
	size_t i;
	int started = 0;

	if (!CBB_add_asn1(cbb, &child, CBS_ASN1_INTEGER)) {
		return 0;
	}

	for (i = 0; i < 8; i++) {
		uint8_t byte = (value >> 8*(7-i)) & 0xff;
			if (!started) {
				if (byte == 0) {
					/* Don't encode leading zeros. */
					continue;
				}
				/* If the high bit is set, add a padding byte to make it
				 * unsigned. 
				 */
				if ((byte & 0x80) && !CBB_add_u8(&child, 0)) {
					return 0;
				}
				started = 1;
			}
			if (!CBB_add_u8(&child, byte)) {
			return 0;
		}
	}

	/* 0 is encoded as a single 0, not the empty string. */
	if (!started && !CBB_add_u8(&child, 0)) {
		return 0;
	}

	return CBB_flush(cbb);
}

void 
CBS_init(CBS *cbs, const uint8_t *data, size_t len) 
{
	cbs->data = data;
	cbs->len = len;
}

static int 
cbs_get(CBS *cbs, const uint8_t **p, size_t n) 
{
	if (cbs->len < n) {
		return 0;
	}

	*p = cbs->data;
	cbs->data += n;
	cbs->len -= n;
	return 1;
}

int 
CBS_skip(CBS *cbs, size_t len) 
{
	const uint8_t *dummy;
	return cbs_get(cbs, &dummy, len);
}

const uint8_t *
CBS_data(const CBS *cbs) 
{
	return cbs->data;
}

size_t CBS_len(const CBS *cbs) {
  return cbs->len;
}

int 
CBS_stow(const CBS *cbs, uint8_t **out_ptr, size_t *out_len) 
{
	OPENSSL_free(*out_ptr);
	*out_ptr = NULL;
	*out_len = 0;

	if (cbs->len == 0) {
		return 1;
	}

	*out_ptr = BUF_memdup(cbs->data, cbs->len);
	if (*out_ptr == NULL) {
		return 0;
	}

	*out_len = cbs->len;
	return 1;
}

int 
CBS_strdup(const CBS *cbs, char **out_ptr) 
{
	if (*out_ptr != NULL) {
		OPENSSL_free(*out_ptr);
	}

	*out_ptr = BUF_strndup((const char*)cbs->data, cbs->len);
	return (*out_ptr != NULL);
}

int 
CBS_contains_zero_byte(const CBS *cbs) 
{
  return memchr(cbs->data, 0, cbs->len) != NULL;
}

int 
CBS_mem_equal(const CBS *cbs, const uint8_t *data, size_t len) 
{
	if (len != cbs->len) {
		return 0;
	}
	return CRYPTO_memcmp(cbs->data, data, len) == 0;
}

static int 
cbs_get_u(CBS *cbs, uint32_t *out, size_t len) 
{
	uint32_t result = 0;
	size_t i;
	const uint8_t *data;

	if (!cbs_get(cbs, &data, len)) {
		return 0;
	}

	for (i = 0; i < len; i++) {
		result <<= 8;
		result |= data[i];
	}

	*out = result;
	return 1;
}

int 
CBS_get_u8(CBS *cbs, uint8_t *out) 
{
	const uint8_t *v;

	if (!cbs_get(cbs, &v, 1)) {
		return 0;
	}

	*out = *v;
	return 1;
}

int 
CBS_get_u16(CBS *cbs, uint16_t *out) 
{
	uint32_t v;

	if (!cbs_get_u(cbs, &v, 2)) {
		return 0;
	}

	*out = v;
	return 1;
}

int 
CBS_get_u24(CBS *cbs, uint32_t *out) 
{
	return cbs_get_u(cbs, out, 3);
}

int 
CBS_get_u32(CBS *cbs, uint32_t *out) 
{
	return cbs_get_u(cbs, out, 4);
}

int 
CBS_get_last_u8(CBS *cbs, uint8_t *out) 
{
	if (cbs->len == 0) {
		return 0;
	}

	*out = cbs->data[cbs->len - 1];
	cbs->len--;
	return 1;
}

int 
CBS_get_bytes(CBS *cbs, CBS *out, size_t len) 
{
	const uint8_t *v;

	if (!cbs_get(cbs, &v, len)) {
		return 0;
	}

	CBS_init(out, v, len);
	return 1;
}

int 
CBS_copy_bytes(CBS *cbs, uint8_t *out, size_t len) 
{
	const uint8_t *v;

	if (!cbs_get(cbs, &v, len)) {
		return 0;
	}

	memcpy(out, v, len);
	return 1;
}

static int 
cbs_get_length_prefixed(CBS *cbs, CBS *out, size_t len_len) 
{
	uint32_t len;

	if (!cbs_get_u(cbs, &len, len_len)) {
		return 0;
	}

	return CBS_get_bytes(cbs, out, len);
}

int 
CBS_get_u8_length_prefixed(CBS *cbs, CBS *out) 
{
	return cbs_get_length_prefixed(cbs, out, 1);
}

int 
CBS_get_u16_length_prefixed(CBS *cbs, CBS *out) 
{
	return cbs_get_length_prefixed(cbs, out, 2);
}

int 
CBS_get_u24_length_prefixed(CBS *cbs, CBS *out) 
{
	return cbs_get_length_prefixed(cbs, out, 3);
}

static int 
cbs_get_any_asn1_element(CBS *cbs, CBS *out, unsigned *out_tag,
                         size_t *out_header_len, int ber_ok) 
{
	uint8_t tag, length_byte;
	CBS header = *cbs;
	CBS throwaway;

	if (out == NULL) {
		out = &throwaway;
	}

	if (!CBS_get_u8(&header, &tag) ||
	    !CBS_get_u8(&header, &length_byte)) {
		return 0;
	}

	/* ITU-T X.690 section 8.1.2.3 specifies the format for identifiers with a tag
	 * number no greater than 30.
	 *
	 * If the number portion is 31 (0x1f, the largest value that fits in the
	 * allotted bits), then the tag is more than one byte long and the
	 * continuation bytes contain the tag number. This parser only supports tag
	 * numbers less than 31 (and thus single-byte tags). 
	 */
	if ((tag & 0x1f) == 0x1f) {
		return 0;
	}

	if (out_tag != NULL) {
		*out_tag = tag;
	}

	size_t len;
	/* The format for the length encoding is specified in ITU-T X.690 section
	 * 8.1.3. 
	 */
	if ((length_byte & 0x80) == 0) {
		/* Short form length. */
		len = ((size_t) length_byte) + 2;
		if (out_header_len != NULL) {
			*out_header_len = 2;
		}
	} else {
		/* The high bit indicate that this is the long form, while the next 7 bits
		 * encode the number of subsequent octets used to encode the length (ITU-T
		 * X.690 clause 8.1.3.5.b). 
		 */
		const size_t num_bytes = length_byte & 0x7f;
		uint32_t len32;

		if (ber_ok && (tag & CBS_ASN1_CONSTRUCTED) != 0 && num_bytes == 0) {
			/* indefinite length */
			if (out_header_len != NULL) {
				*out_header_len = 2;
			}
			return CBS_get_bytes(cbs, out, 2);
		}

		/* ITU-T X.690 clause 8.1.3.5.c specifies that the value 0xff shall not be
		 * used as the first byte of the length. If this parser encounters that
		 * value, num_bytes will be parsed as 127, which will fail the check below.
		 */
		if (num_bytes == 0 || num_bytes > 4) {
			return 0;
		}

		if (!cbs_get_u(&header, &len32, num_bytes)) {
			return 0;
		}

		/* ITU-T X.690 section 10.1 (DER length forms) requires encoding the length
		 * with the minimum number of octets. 
		 */
		if (len32 < 128) {
			/* Length should have used short-form encoding. */
			return 0;
		}

		if ((len32 >> ((num_bytes-1)*8)) == 0) {
			/* Length should have been at least one byte shorter. */
			return 0;
		}

		len = len32;
		if (len + 2 + num_bytes < len) {
			/* Overflow. */
			return 0;
		}

		len += 2 + num_bytes;
		if (out_header_len != NULL) {
			*out_header_len = 2 + num_bytes;
		}
	}

	return CBS_get_bytes(cbs, out, len);
}

int 
CBS_get_any_asn1_element(CBS *cbs, CBS *out, unsigned *out_tag,
                         size_t *out_header_len) 
{
	return cbs_get_any_asn1_element(cbs, out, out_tag, 
					out_header_len,
				        0 /* DER only */);
}

int 
CBS_get_any_ber_asn1_element(CBS *cbs, CBS *out, unsigned *out_tag,
                             size_t *out_header_len) 
{
	return cbs_get_any_asn1_element(cbs, out, out_tag, 
					out_header_len,
				        1 /* BER allowed */);
}

static int cbs_get_asn1(CBS *cbs, CBS *out, unsigned tag_value,
                        int skip_header) 
{
	size_t header_len;
	unsigned tag;
	CBS throwaway;

	if (out == NULL) {
		out = &throwaway;
	}

	if (!CBS_get_any_asn1_element(cbs, out, &tag, &header_len) ||
	    tag != tag_value) {
		return 0;
	}

	if (skip_header && !CBS_skip(out, header_len)) {
		assert(0);
		return 0;
	}

	return 1;
}

int 
CBS_get_asn1(CBS *cbs, CBS *out, unsigned tag_value) 
{
	return cbs_get_asn1(cbs, out, tag_value, 1 /* skip header */);
}

int 
CBS_get_asn1_element(CBS *cbs, CBS *out, unsigned tag_value) 
{
	return cbs_get_asn1(cbs, out, tag_value, 0 /* include header */);
}

int 
CBS_peek_asn1_tag(const CBS *cbs, unsigned tag_value) 
{
	if (CBS_len(cbs) < 1) {
		return 0;
	}

	return CBS_data(cbs)[0] == tag_value;
}

int 
CBS_get_asn1_uint64(CBS *cbs, uint64_t *out) 
{
	CBS bytes;
	const uint8_t *data;
	size_t i, len;

	if (!CBS_get_asn1(cbs, &bytes, CBS_ASN1_INTEGER)) {
		return 0;
	}

	*out = 0;
	data = CBS_data(&bytes);
	len = CBS_len(&bytes);

	if (len == 0) {
		/* An INTEGER is encoded with at least one octet. */
		return 0;
	}

	if ((data[0] & 0x80) != 0) {
		/* Negative number. */
		return 0;
	}

	if (data[0] == 0 && len > 1 && (data[1] & 0x80) == 0) {
		/* Extra leading zeros. */
		return 0;
	}

	for (i = 0; i < len; i++) {
		if ((*out >> 56) != 0) {
			/* Too large to represent as a uint64_t. */
			return 0;
		}
		*out <<= 8;
		*out |= data[i];
	}

	return 1;
}

int 
CBS_get_optional_asn1(CBS *cbs, CBS *out, int *out_present, unsigned tag) 
{
	int present = 0;

	if (CBS_peek_asn1_tag(cbs, tag)) {
		if (!CBS_get_asn1(cbs, out, tag)) {
			return 0;
		}
		present = 1;
	}

	if (out_present != NULL) {
		*out_present = present;
	}

	return 1;
}

int 
CBS_get_optional_asn1_octet_string(CBS *cbs, CBS *out, int *out_present,
                                   unsigned tag) 
{
	CBS child;
	int present;
	if (!CBS_get_optional_asn1(cbs, &child, &present, tag)) {
		return 0;
	}

	if (present) {
		if (!CBS_get_asn1(&child, out, CBS_ASN1_OCTETSTRING) ||
		    CBS_len(&child) != 0) {
			return 0;
		}
	} else {
		CBS_init(out, NULL, 0);
	}

	if (out_present) {
		*out_present = present;
	}

	return 1;
}

int 
CBS_get_optional_asn1_uint64(CBS *cbs, uint64_t *out, unsigned tag,
                             uint64_t default_value) 
{
	CBS child;
	int present;

	if (!CBS_get_optional_asn1(cbs, &child, &present, tag)) {
		return 0;
	}

	if (present) {
		if (!CBS_get_asn1_uint64(&child, out) ||
			CBS_len(&child) != 0) {
			return 0;
		}
	} else {
		*out = default_value;
	}

	return 1;
}

int 
CBS_get_optional_asn1_bool(CBS *cbs, int *out, unsigned tag,
                           int default_value) 
{
	CBS child, child2;
	int present;

	if (!CBS_get_optional_asn1(cbs, &child, &present, tag)) {
		return 0;
	}

	if (present) {
		uint8_t boolean;

		if (!CBS_get_asn1(&child, &child2, CBS_ASN1_BOOLEAN) ||
		    CBS_len(&child2) != 1 ||
		    CBS_len(&child) != 0) {
			return 0;
		}

		boolean = CBS_data(&child2)[0];
		if (boolean == 0) {
			*out = 0;
		} else if (boolean == 0xff) {
			*out = 1;
		} else {
			return 0;
		}
	} else {
		*out = default_value;
	}
	return 1;
}

int 
CBB_finish_i2d(CBB *cbb, uint8_t **outp) 
{
	assert(cbb->base->can_resize);

	uint8_t *der;
	size_t der_len;

	if (!CBB_finish(cbb, &der, &der_len)) {
		CBB_cleanup(cbb);
		return -1;
	}

	if (der_len > INT_MAX) {
		OPENSSL_free(der);
		return -1;
	}

	if (outp != NULL) {
		if (*outp == NULL) {
			*outp = der;
			der = NULL;
		} else {
			memcpy(*outp, der, der_len);
			*outp += der_len;
		}
	}

	OPENSSL_free(der);

	return (int)der_len;
}

int 
BN_parse_asn1_unsigned(CBS *cbs, BIGNUM *ret) 
{
	CBS child;

	if (!CBS_get_asn1(cbs, &child, CBS_ASN1_INTEGER) ||
	    CBS_len(&child) == 0) {
		OPENSSL_PUT_ERROR(BN, BN_R_BAD_ENCODING);
		return 0;
	}

	if (CBS_data(&child)[0] & 0x80) {
		OPENSSL_PUT_ERROR(BN, BN_R_NEGATIVE_NUMBER);
		return 0;
	}

	/* INTEGERs must be minimal. */
	if (CBS_data(&child)[0] == 0x00 &&
	    CBS_len(&child) > 1 &&
	    !(CBS_data(&child)[1] & 0x80)) {
		OPENSSL_PUT_ERROR(BN, BN_R_BAD_ENCODING);
		return 0;
	}

	return BN_bin2bn(CBS_data(&child), CBS_len(&child), ret) != NULL;
}

int 
BN_parse_asn1_unsigned_buggy(CBS *cbs, BIGNUM *ret) 
{
	CBS child;

	if (!CBS_get_asn1(cbs, &child, CBS_ASN1_INTEGER) ||
		CBS_len(&child) == 0) {
		OPENSSL_PUT_ERROR(BN, BN_R_BAD_ENCODING);
		return 0;
	}

	/*
	 * This function intentionally does not reject negative numbers or non-minimal
	 * encodings. Estonian IDs issued between September 2014 to September 2015 are
	 * broken. See https://crbug.com/532048 and https://crbug.com/534766.
	 *
	 * TODO(davidben): Remove this code and callers in March 2016. 
	 */
	return BN_bin2bn(CBS_data(&child), CBS_len(&child), ret) != NULL;
}

int 
BN_marshal_asn1(CBB *cbb, const BIGNUM *bn) 
{
	/* Negative numbers are unsupported. */
	if (BN_is_negative(bn)) {
		OPENSSL_PUT_ERROR(BN, BN_R_NEGATIVE_NUMBER);
		return 0;
	}

	CBB child;
	if (!CBB_add_asn1(cbb, &child, CBS_ASN1_INTEGER) ||
	    /* The number must be padded with a leading zero if the high bit would
	     * otherwise be set or if |bn| is zero. 
	     */
	    (BN_num_bits(bn) % 8 == 0 && !CBB_add_u8(&child, 0x00)) ||
	    !BN_bn2cbb_padded(&child, BN_num_bytes(bn), bn) ||
	    !CBB_flush(cbb)) {
		OPENSSL_PUT_ERROR(BN, BN_R_ENCODE_ERROR);
		return 0;
	}

	return 1;
}

/*
 * constant_time_select_ulong returns |x| if |v| is 1 and |y| if |v| is 0. Its
 * behavior is undefined if |v| takes any other value. 
 */
static 
BN_ULONG constant_time_select_ulong(int v, BN_ULONG x, BN_ULONG y) 
{
	BN_ULONG mask = v;
	mask--;

	return (~mask & x) | (mask & y);
}

/*
 * constant_time_le_size_t returns 1 if |x| <= |y| and 0 otherwise. |x| and |y|
 * must not have their MSBs set. 
 */
static int 
constant_time_le_size_t(size_t x, size_t y) 
{
	return ((x - y - 1) >> (sizeof(size_t) * 8 - 1)) & 1;
}

/* 
 * read_word_padded returns the |i|'th word of |in|, if it is not out of
 * bounds. Otherwise, it returns 0. It does so without branches on the size of
 * |in|, however it necessarily does not have the same memory access pattern. If
 * the access would be out of bounds, it reads the last word of |in|. |in| must
 * not be zero.
 */
static 
BN_ULONG read_word_padded(const BIGNUM *in, size_t i) 
{
	/* Read |in->d[i]| if valid. Otherwise, read the last word. */
	BN_ULONG l = in->d[constant_time_select_ulong(constant_time_le_size_t(in->dmax, i), 
						      in->dmax - 1, i)];

	/* Clamp to zero if above |d->top|. */
	return constant_time_select_ulong(constant_time_le_size_t(in->top, i), 0, l);
}

int 
BN_bn2bin_padded(uint8_t *out, size_t len, const BIGNUM *in) 
{
	size_t i;
	BN_ULONG l;

	/* Special case for |in| = 0. Just branch as the probability is negligible. */
	if (BN_is_zero(in)) {
		memset(out, 0, len);
		return 1;
	}

	/*
	 * Check if the integer is too big. This case can exit early in non-constant
	 * time. 
	 */
	if ((size_t)in->top > (len + (BN_BYTES - 1)) / BN_BYTES) {
		return 0;
	}

	if ((len % BN_BYTES) != 0) {
		l = read_word_padded(in, len / BN_BYTES);
		if (l >> (8 * (len % BN_BYTES)) != 0) {
			return 0;
		}
	}

	/*
	 * Write the bytes out one by one. Serialization is done without branching on
	 * the bits of |in| or on |in->top|, but if the routine would otherwise read
	 * out of bounds, the memory access pattern can't be fixed. However, for an
	 * RSA key of size a multiple of the word size, the probability of BN_BYTES
	 * leading zero octets is low.
	 *
	 * See Falko Stenzke, "Manager's Attack revisited", ICICS 2010.
	 */
	i = len;
	while (i--) {
		l = read_word_padded(in, i / BN_BYTES);
		*(out++) = (uint8_t)(l >> (8 * (i % BN_BYTES))) & 0xff;
	}
	return 1;
}

int 
BN_bn2cbb_padded(CBB *out, size_t len, const BIGNUM *in) 
{
	uint8_t *ptr;

	return CBB_add_space(out, &ptr, len) && BN_bn2bin_padded(ptr, len, in);
}
