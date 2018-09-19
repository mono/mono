/* Copyright (C) 1995-1998 Eric Young (eay@cryptsoft.com)
 * All rights reserved.
 *
 * This package is an SSL implementation written
 * by Eric Young (eay@cryptsoft.com).
 * The implementation was written so as to conform with Netscapes SSL.
 *
 * This library is free for commercial and non-commercial use as long as
 * the following conditions are aheared to.  The following conditions
 * apply to all code found in this distribution, be it the RC4, RSA,
 * lhash, DES, etc., code; not just the SSL code.  The SSL documentation
 * included with this distribution is covered by the same copyright terms
 * except that the holder is Tim Hudson (tjh@cryptsoft.com).
 *
 * Copyright remains Eric Young's, and as such any Copyright notices in
 * the code are not to be removed.
 * If this package is used in a product, Eric Young should be given attribution
 * as the author of the parts of the library used.
 * This can be in the form of a textual message at program startup or
 * in documentation (online or textual) provided with the package.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. All advertising materials mentioning features or use of this software
 *    must display the following acknowledgement:
 *    "This product includes cryptographic software written by
 *     Eric Young (eay@cryptsoft.com)"
 *    The word 'cryptographic' can be left out if the rouines from the library
 *    being used are not cryptographic related :-).
 * 4. If you include any Windows specific code (or a derivative thereof) from
 *    the apps directory (application code) you must include an acknowledgement:
 *    "This product includes software written by Tim Hudson (tjh@cryptsoft.com)"
 *
 * THIS SOFTWARE IS PROVIDED BY ERIC YOUNG ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHOR OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 *
 * The licence and distribution terms for any publically available version or
 * derivative of this code cannot be changed.  i.e. this code cannot simply be
 * copied and put under another distribution licence
 * [including the GNU Public Licence.] */

#ifndef OPENSSL_HEADER_RSA_H
#define OPENSSL_HEADER_RSA_H

#include <openssl/engine.h>
#include <stdint.h>

#if defined(__cplusplus)
extern "C" {
#endif


/* rsa.h contains functions for handling encryption and signature using RSA. */

/* Allocation and destruction. */

#define RSA_R_BAD_ENCODING		200
#define RSA_R_BAD_E			201
#define RSA_R_BAD_RSA_PARAMETERS	202
#define RSA_R_ENCODE_ERROR		203
#define RSA_R_BAD_VERSION		204

/* RSA_additional_prime contains information about the third, forth etc prime
 * in a multi-prime RSA key. */
typedef struct {
  BIGNUM *prime;
  /* exp is d^{prime-1} mod prime */
  BIGNUM *exp;
  /* coeff is such that r×coeff ≡ 1 mod prime. */
  BIGNUM *coeff;

  /* Values below here are not in the ASN.1 serialisation. */

  /* r is the product of all primes (including p and q) prior to this one. */
  BIGNUM *r;
  /* mont is a |BN_MONT_CTX| modulo |prime|. */
  BN_MONT_CTX *mont;
} RSA_additional_prime;

typedef RSA_additional_prime RSA_additional_prime_st;

/* CHECKED_CAST casts |p| from type |from| to type |to|. */
#define CHECKED_CAST(to, from, p) ((to) (1 ? (p) : (from)0))

/* RSA_additional_prime */
#define sk_RSA_additional_prime_new(comp)                                      	\
  (RSA_additional_prime_st *)sk_new(CHECKED_CAST(                      		\
      stack_cmp_func,                                                          	\
      int (*)(const RSA_additional_prime **a, const RSA_additional_prime **b), 	\
      comp)))

#define sk_RSA_additional_prime_new_null() \
  ((RSA_additional_prime_st *)sk_new_null())

#define sk_RSA_additional_prime_num(sk)                                       	\
  sk_num(CHECKED_CAST(const _STACK *, const RSA_additional_prime_st *, 		\
                      sk))

#define sk_RSA_additional_prime_zero(sk) \
  sk_zero(CHECKED_CAST(_STACK *, RSA_additional_prime_st *, sk));

#define sk_RSA_additional_prime_value(sk, i)                               \
  ((RSA_additional_prime *)sk_value(                                       \
      CHECKED_CAST(const _STACK *, const RSA_additional_prime_st *, 	   \
                   sk),                                                    \
      (i)))

#define sk_RSA_additional_prime_set(sk, i, p)                            \
  ((RSA_additional_prime *)sk_set(                                       \
      CHECKED_CAST(_STACK *, RSA_additional_prime_st *, sk), (i), 	 \
      CHECKED_CAST(void *, RSA_additional_prime *, p)))

#define sk_RSA_additional_prime_free(sk) \
  sk_free(CHECKED_CAST(_STACK *, RSA_additional_prime_st *, sk))

#define sk_RSA_additional_prime_pop_free(sk, free_func)                        	\
  sk_pop_free(CHECKED_CAST(_STACK *, RSA_additional_prime_st *, sk),    	\
              CHECKED_CAST(void (*)(void *), void (*)(RSA_additional_prime *),  \
                           free_func))

#define sk_RSA_additional_prime_insert(sk, p, where)                      	\
  sk_insert(CHECKED_CAST(_STACK *, RSA_additional_prime_st *, sk), 		\
            CHECKED_CAST(void *, RSA_additional_prime *, p), (where))

#define sk_RSA_additional_prime_delete(sk, where) \
  ((RSA_additional_prime *)sk_delete(             \
      CHECKED_CAST(_STACK *, RSA_additional_prime_st *, sk), (where)))

#define sk_RSA_additional_prime_delete_ptr(sk, p)                   	\
  ((RSA_additional_prime *)sk_delete_ptr(                           	\
      CHECKED_CAST(_STACK *, RSA_additional_prime_st *, sk), 		\
      CHECKED_CAST(void *, RSA_additional_prime *, p)))

#define sk_RSA_additional_prime_find(sk, out_index, p)                  \
  sk_find(CHECKED_CAST(_STACK *, RSA_additional_prime_st *, sk), 	\
          (out_index), CHECKED_CAST(void *, RSA_additional_prime *, p))

#define sk_RSA_additional_prime_shift(sk) \
  ((RSA_additional_prime *)sk_shift(      \
      CHECKED_CAST(_STACK *, RSA_additional_prime_st *, sk)))

#define sk_RSA_additional_prime_push(sk, p)                             \
  sk_push(CHECKED_CAST(_STACK *, RSA_additional_prime_st *, sk), 	\
          CHECKED_CAST(void *, RSA_additional_prime *, p))

#define sk_RSA_additional_prime_pop(sk) \
  ((RSA_additional_prime *)sk_pop(      \
      CHECKED_CAST(_STACK *, RSA_additional_prime_st *, sk)))

#define sk_RSA_additional_prime_dup(sk)                   	\
  ((RSA_additional_prime_st *)sk_dup(CHECKED_CAST(		\
      const _STACK *, const RSA_additional_prime_st *, sk)))

#define sk_RSA_additional_prime_sort(sk) \
  sk_sort(CHECKED_CAST(_STACK *, RSA_additional_prime_st *, sk))

#define sk_RSA_additional_prime_is_sorted(sk) 				\
  sk_is_sorted(CHECKED_CAST(const _STACK *,   				\
                            const RSA_additional_prime_st *, sk))

#define sk_RSA_additional_prime_set_cmp_func(sk, comp)                       	\
  ((int (*)(const RSA_additional_prime **a, const RSA_additional_prime **b)) 	\
       sk_set_cmp_func(                                                      	\
           CHECKED_CAST(_STACK *, RSA_additional_prime_st *, sk),     		\
           CHECKED_CAST(stack_cmp_func,                                      	\
                        int (*)(const RSA_additional_prime **a,              	\
                                const RSA_additional_prime **b),             	\
                        comp)))

#define sk_RSA_additional_prime_deep_copy(sk, copy_func, free_func)        	\
  ((RSA_additional_prime_st *)sk_deep_copy(                         		\
      CHECKED_CAST(const _STACK *, const RSA_additional_prime *, 		\
                   sk),                                                    	\
      CHECKED_CAST(void *(*)(void *),                                      	\
                   RSA_additional_prime *(*)(RSA_additional_prime *),      	\
                   copy_func),                                             	\
      CHECKED_CAST(void (*)(void *), void (*)(RSA_additional_prime *),     	\
                   free_func)))

/* RSA_new returns a new, empty RSA object or NULL on error. */
OPENSSL_EXPORT RSA *RSA_new(void);

/* RSA_new_method acts the same as |RSA_new| but takes an explicit |ENGINE|. */
OPENSSL_EXPORT RSA *RSA_new_method(ENGINE *engine);

/* RSA_free decrements the reference count of |rsa| and frees it if the
 * reference count drops to zero. */
OPENSSL_EXPORT void RSA_free(RSA *rsa);

/* RSA_up_ref increments the reference count of |rsa|. */
OPENSSL_EXPORT int RSA_up_ref(RSA *rsa);

void RSA_additional_prime_free(RSA_additional_prime *ap);

/* Key generation. */

/* RSA_generate_key_ex generates a new RSA key where the modulus has size
 * |bits| and the public exponent is |e|. If unsure, |RSA_F4| is a good value
 * for |e|. If |cb| is not NULL then it is called during the key generation
 * process. In addition to the calls documented for |BN_generate_prime_ex|, it
 * is called with event=2 when the n'th prime is rejected as unsuitable and
 * with event=3 when a suitable value for |p| is found.
 *
 * It returns one on success or zero on error. */
OPENSSL_EXPORT int RSA_generate_key_ex(RSA *rsa, int bits, BIGNUM *e,
                                       BN_GENCB *cb);

/* RSA_generate_multi_prime_key acts like |RSA_generate_key_ex| but can
 * generate an RSA private key with more than two primes. */
OPENSSL_EXPORT int RSA_generate_multi_prime_key(RSA *rsa, int bits,
                                                int num_primes, BIGNUM *e,
                                                BN_GENCB *cb);


/* Encryption / Decryption */

/* Padding types for encryption. */
#define RSA_PKCS1_PADDING 1
#define RSA_NO_PADDING 3
#define RSA_PKCS1_OAEP_PADDING 4
/* RSA_PKCS1_PSS_PADDING can only be used via the EVP interface. */
#define RSA_PKCS1_PSS_PADDING 6

/* ASN.1 functions. */

/* RSA_parse_public_key parses a DER-encoded RSAPublicKey structure (RFC 3447)
 * from |cbs| and advances |cbs|. It returns a newly-allocated |RSA| or NULL on
 * error. */
OPENSSL_EXPORT RSA *RSA_parse_public_key(CBS *cbs);

/* RSA_parse_public_key_buggy behaves like |RSA_parse_public_key|, but it
 * tolerates some invalid encodings. Do not use this function. */
OPENSSL_EXPORT RSA *RSA_parse_public_key_buggy(CBS *cbs);

/* RSA_public_key_from_bytes parses |in| as a DER-encoded RSAPublicKey structure
 * (RFC 3447). It returns a newly-allocated |RSA| or NULL on error. */
OPENSSL_EXPORT RSA *RSA_public_key_from_bytes(const uint8_t *in, size_t in_len);

/* RSA_marshal_public_key marshals |rsa| as a DER-encoded RSAPublicKey structure
 * (RFC 3447) and appends the result to |cbb|. It returns one on success and
 * zero on failure. */
OPENSSL_EXPORT int RSA_marshal_public_key(CBB *cbb, const RSA *rsa);

/* RSA_public_key_to_bytes marshals |rsa| as a DER-encoded RSAPublicKey
 * structure (RFC 3447) and, on success, sets |*out_bytes| to a newly allocated
 * buffer containing the result and returns one. Otherwise, it returns zero. The
 * result should be freed with |OPENSSL_free|. */
OPENSSL_EXPORT int RSA_public_key_to_bytes(uint8_t **out_bytes, size_t *out_len,
                                           const RSA *rsa);

/* RSA_parse_private_key parses a DER-encoded RSAPrivateKey structure (RFC 3447)
 * from |cbs| and advances |cbs|. It returns a newly-allocated |RSA| or NULL on
 * error. */
OPENSSL_EXPORT RSA *RSA_parse_private_key(CBS *cbs);

/* RSA_private_key_from_bytes parses |in| as a DER-encoded RSAPrivateKey
 * structure (RFC 3447). It returns a newly-allocated |RSA| or NULL on error. */
OPENSSL_EXPORT RSA *RSA_private_key_from_bytes(const uint8_t *in,
                                               size_t in_len);

/* RSA_marshal_private_key marshals |rsa| as a DER-encoded RSAPrivateKey
 * structure (RFC 3447) and appends the result to |cbb|. It returns one on
 * success and zero on failure. */
OPENSSL_EXPORT int RSA_marshal_private_key(CBB *cbb, const RSA *rsa);

/* RSA_private_key_to_bytes marshals |rsa| as a DER-encoded RSAPrivateKey
 * structure (RFC 3447) and, on success, sets |*out_bytes| to a newly allocated
 * buffer containing the result and returns one. Otherwise, it returns zero. The
 * result should be freed with |OPENSSL_free|. */
OPENSSL_EXPORT int RSA_private_key_to_bytes(uint8_t **out_bytes,
                                            size_t *out_len, const RSA *rsa);

#if defined(__cplusplus)
}  /* extern C */
#endif

#endif  /* OPENSSL_HEADER_RSA_H */
