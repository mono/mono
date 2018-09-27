//
//  openssl-x509-chain.h
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/3/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#ifndef __openssl__openssl_x509_chain__
#define __openssl__openssl_x509_chain__

#include <stdio.h>
#include "openssl-ssl.h"
#include "openssl-x509.h"

MonoOpenSSLX509Chain *
mono_uxtls_x509_chain_new (void);

MonoOpenSSLX509Chain *
mono_uxtls_x509_chain_from_certs (STACK_OF(X509) *certs);

STACK_OF(X509) *
mono_uxtls_x509_chain_peek_certs (MonoOpenSSLX509Chain *chain);

int
mono_uxtls_x509_chain_get_count (MonoOpenSSLX509Chain *chain);

X509 *
mono_uxtls_x509_chain_get_cert (MonoOpenSSLX509Chain *chain, int index);

MonoOpenSSLX509Chain *
mono_uxtls_x509_chain_up_ref (MonoOpenSSLX509Chain *chain);

STACK_OF(X509) *
mono_uxtls_x509_chain_get_certs (MonoOpenSSLX509Chain *chain);

int
mono_uxtls_x509_chain_free (MonoOpenSSLX509Chain *chain);

void
mono_uxtls_x509_chain_add_cert (MonoOpenSSLX509Chain *chain, X509 *x509);

#endif /* defined(__openssl__openssl_x509_chain__) */

