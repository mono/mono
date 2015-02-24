#!/bin/bash
#
# This script is used to generate a root CA, intermediate CA, and server cert, to be used in 
# the Client and Server applications.

# OSX:
CnfOrig=/System/Library/OpenSSL/openssl.cnf
#
# Ubuntu:
# CnfOrig=/usr/lib/ssl/openssl.cnf
#
# Red Hat: 
# CnfOrig=/etc/pki/tls/openssl.cnf


##### Create a scratch directory

mkdir junkca
cd junkca

##### Create the Root CA

mkdir -p junkCA/{certs,crl,newcerts,private}
touch junkCA/index.txt
echo "01" > junkCA/crlnumber

# Using "junkCA" as the default directory instead of "demoCA"
# Also un-commenting the password lines, to make password entry automatic
cat $CnfOrig | sed 's/demoCA/junkCA/g' | sed 's/# input_password/input_password/' | sed 's/# output_password/output_password/' | sed 's/cacert.pem/ca.crt/g' | sed 's/cakey.pem/ca.key/g' > junkCA/openssl.cnf

openssl req -config junkCA/openssl.cnf -newkey rsa:2048 -nodes -keyout junkCA/private/ca.key -out junkCA/ca.csr -subj "/C=AU/ST=SomeState/O=Internet Widgits Pty Ltd/CN=junkca.junkdomain"
openssl ca -config junkCA/openssl.cnf -create_serial -notext -out junkCA/ca.crt -days 10950 -batch -keyfile junkCA/private/ca.key -selfsign -extensions v3_ca -infiles junkCA/ca.csr

##### Create the Intermediate CA

mkdir -p junkIntermediate/{certs,crl,newcerts,private}
touch junkIntermediate/index.txt
echo "01" > junkIntermediate/crlnumber

cat junkCA/openssl.cnf | sed 's/junkCA/junkIntermediate/g' | sed 's/ca.key/intermediate.key/g' | sed 's/ca.crt/intermediate.crt/g' > junkIntermediate/openssl.cnf

cat >> junkIntermediate/openssl.cnf << EOF

[ www_junk_cert ]
basicConstraints = CA:FALSE
keyUsage = digitalSignature, keyEncipherment, keyAgreement
subjectKeyIdentifier = hash
authorityKeyIdentifier = keyid,issuer
extendedKeyUsage = serverAuth
subjectAltName = DNS:www.junkca.junkdomain

EOF

openssl req -config junkIntermediate/openssl.cnf -newkey rsa:2048 -nodes -keyout junkIntermediate/private/intermediate.key -out junkIntermediate/intermediate.csr -subj "/C=AU/ST=SomeState/O=Internet Widgits Pty Ltd/CN=intermediate.junkca.junkdomain"
openssl ca -config junkCA/openssl.cnf -notext -out junkIntermediate/intermediate.crt -days 10950 -batch -keyfile junkCA/private/ca.key -cert junkCA/ca.crt -extensions v3_ca -in junkIntermediate/intermediate.csr

# randomly generated once
echo "CCED516A2DF30025" > junkIntermediate/serial

cat junkIntermediate/intermediate.crt junkCA/ca.crt > chain.crt

##### Create the intermediately signed certificate
mkdir www
openssl req -newkey rsa:2048 -keyout www/www.key -out www/www.csr -nodes -subj "/C=AU/ST=SomeState/O=Internet Widgits Pty Ltd/CN=www.junkca.junkdomain"
openssl ca -config junkIntermediate/openssl.cnf -notext -days 10950 -batch -keyfile junkIntermediate/private/intermediate.key -cert junkIntermediate/intermediate.crt -extensions www_junk_cert -in www/www.csr -out www/www.crt

##### Create the directly signed certificate
mkdir junkDirect
openssl req -newkey rsa:2048 -keyout junkDirect/junkDirect.key -out junkDirect/junkDirect.csr -nodes -subj "/C=AU/ST=SomeState/O=Internet Widgits Pty Ltd/CN=junkDirect.junkca.junkdomain"
openssl ca -config junkCA/openssl.cnf -notext -days 10950 -batch -keyfile junkCA/private/ca.key -cert junkCA/ca.crt -in junkDirect/junkDirect.csr -out junkDirect/junkDirect.crt
cat junkCA/ca.crt >> junkDirect/junkDirect.crt

if [ "`openssl verify -CAfile chain.crt www/www.crt`" = "www/www.crt: OK" ] ; then
    echo ""
    echo ""
    openssl pkcs12 -passout pass: -export -out www/www.pfx -inkey www/www.key -in www/www.crt -certfile junkIntermediate/intermediate.crt
    openssl pkcs12 -passout pass: -export -out junkDirect/junkDirect.pfx -inkey junkDirect/junkDirect.key -in junkDirect/junkDirect.crt -certfile junkCA/ca.crt
    openssl pkcs12 -passout pass: -export -out junkIntermediate/intermediate.pfx -inkey junkIntermediate/private/intermediate.key -in junkIntermediate/intermediate.crt 
    openssl pkcs12 -passout pass: -export -out junkCA/ca.pfx -inkey junkCA/private/ca.key -in junkCA/ca.crt 
    cd ..
    cp junkca/junkCA/ca.crt .
    cp junkca/junkCA/ca.pfx .
    cp junkca/junkCA/private/ca.key .

    cp junkca/junkIntermediate/intermediate.crt .
    cp junkca/junkIntermediate/intermediate.pfx .
    cp junkca/junkIntermediate/private/intermediate.key .

    cp junkca/www/www.crt ./intermed_signed.crt
    cp junkca/www/www.pfx ./intermed_signed.pfx
    cp junkca/www/www.key ./intermed_signed.key

    cp junkca/junkDirect/junkDirect.crt ./directly_signed.crt
    cp junkca/junkDirect/junkDirect.pfx ./directly_signed.pfx
    cp junkca/junkDirect/junkDirect.key ./directly_signed.key
    
    cat intermed_signed.crt junkca/chain.crt > ./full_chain.crt
    rm -rf junkca
    echo "Done, OK"
    echo ""
else
    echo ""
    echo "There was a problem."
    echo ""
fi
