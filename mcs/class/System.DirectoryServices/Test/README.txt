Additional steps for configuring secure tests run:

1. Set login.config.url.1 parameter in JAVA_HOME\lib\security\java.security
to \mcs\class\System.DirectoryServices\Test\java.login.sun.config (for sun jvm)
or \mcs\class\System.DirectoryServices\Test\java.login.ibm.config (for ibm jvm)

2. Copy krb5.conf.example to JAVA_HOME\lib\security\krb5.conf
