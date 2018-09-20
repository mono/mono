//
//  openssl-x509-revoked.h
//  MonoOpenSSL
//
//  Created by Martin Baulig on 3/23/16.
//  Copyright © 2016 Xamarin. All rights reserved.
//

#ifndef __openssl__openssl_x509_revoked__
#define __openssl__openssl_x509_revoked__

#include <stdio.h>
#include "openssl-ssl.h"
#include "openssl-x509-crl.h"

MonoOpenSSLX509Revoked *
mono_tls_x509_revoked_new (MonoOpenSSLX509Crl *owner, X509_REVOKED *revoked);

void
mono_tls_x509_revoked_free (MonoOpenSSLX509Revoked *revoked);

int
mono_tls_x509_revoked_get_serial_number (MonoOpenSSLX509Revoked *revoked, char *buffer, int size);

int64_t
mono_tls_x509_revoked_get_revocation_date (MonoOpenSSLX509Revoked *revoked);

int
mono_tls_x509_revoked_get_reason (MonoOpenSSLX509Revoked *revoked);

int
mono_tls_x509_revoked_get_sequence (MonoOpenSSLX509Revoked *revoked);

#endif /* __openssl__openssl_x509_revoked__ */
