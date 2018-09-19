//
//  openssl-x509-crl.h
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/23/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#ifndef __openssl__openssl_x509_crl__
#define __openssl__openssl_x509_crl__

#include <stdio.h>
#include "openssl-ssl.h"
#include "openssl-x509.h"

MonoOpenSSLX509Crl *
mono_openssl_x509_crl_from_data (const void *buf, int len, MonoOpenSSLX509Format format);

MonoOpenSSLX509Crl *
mono_openssl_x509_crl_ref (MonoOpenSSLX509Crl *crl);

int
mono_openssl_x509_crl_free (MonoOpenSSLX509Crl *crl);

MonoOpenSSLX509Revoked *
mono_openssl_x509_crl_get_by_cert (MonoOpenSSLX509Crl *crl, X509 *x509);

MonoOpenSSLX509Revoked *
mono_openssl_x509_crl_get_by_serial (MonoOpenSSLX509Crl *crl, void *serial, int len);

int
mono_openssl_x509_crl_get_revoked_count (MonoOpenSSLX509Crl *crl);

MonoOpenSSLX509Revoked *
mono_openssl_x509_crl_get_revoked (MonoOpenSSLX509Crl *crl, int index);

int64_t
mono_openssl_x509_crl_get_last_update (MonoOpenSSLX509Crl *crl);

int64_t
mono_openssl_x509_crl_get_next_update (MonoOpenSSLX509Crl *crl);

int64_t
mono_openssl_x509_crl_get_version (MonoOpenSSLX509Crl *crl);

MonoOpenSSLX509Name *
mono_openssl_x509_crl_get_issuer (MonoOpenSSLX509Crl *crl);

#endif /* __openssl__openssl_x509_crl__ */
