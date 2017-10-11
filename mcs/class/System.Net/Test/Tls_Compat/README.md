Note: If you have problems running on OSX because the State or Province field is not the same even though it appears to be, follow the advice here to fix this known defect:

http://thornelabs.net/2013/05/30/openssl-ca-signing-error-field-needed-to-be-the-same-in-the-ca-certificate.html

```
To fix this simply change the string_mask parameter to utf8only in /System/Library/OpenSSL/openssl.cnf on OS X or create the CSR on a Linux box.
```
