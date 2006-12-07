all: on x509build.exe

x509build.exe: x509build.cs
	gmcs x509build.cs

PKITS_data.zip:
	wget http://csrc.nist.gov/pki/testing/PKITS_data.zip

pkits.ldif: PKITS_data.zip
	unzip -o PKITS_data.zip
	touch pkits.ldif

register: pkits.ldif
	certmgr -add -c Trust certs/TrustAnchorRootCertificate.crt
	certmgr -add -c CA certs/GoodCACert.crt
	certmgr -add -c CA certs/BadSignedCACert.crt
	certmgr -add -c CA certs/DSACACert.crt
	certmgr -add -c CA certs/DSAParametersInheritedCACert.crt
	certmgr -add -c CA certs/BadnotBeforeDateCACert.crt
	certmgr -add -c CA certs/BadnotAfterDateCACert.crt
	certmgr -add -c CA certs/NameOrderingCACert.crt
	certmgr -add -c CA certs/UIDCACert.crt
	certmgr -add -c CA certs/RFC3280MandatoryAttributeTypesCACert.crt
	certmgr -add -c CA certs/RFC3280OptionalAttributeTypesCACert.crt
	certmgr -add -c CA certs/UTF8StringEncodedNamesCACert.crt
	certmgr -add -c CA certs/RolloverfromPrintableStringtoUTF8StringCACert.crt
	certmgr -add -c CA certs/UTF8StringCaseInsensitiveMatchCACert.crt
	certmgr -add -c CA certs/BasicSelfIssuedNewKeyCACert.crt
	certmgr -add -c CA certs/BasicSelfIssuedNewKeyOldWithNewCACert.crt
	certmgr -add -c CA certs/BasicSelfIssuedCRLSigningKeyCACert.crt
	certmgr -add -c CA certs/BasicSelfIssuedCRLSigningKeyCRLCert.crt
	certmgr -add -c CA certs/MissingbasicConstraintsCACert.crt
	certmgr -add -c CA certs/basicConstraintsCriticalcAFalseCACert.crt
	certmgr -add -c CA certs/basicConstraintsNotCriticalCACert.crt
	certmgr -add -c CA certs/basicConstraintsNotCriticalcAFalseCACert.crt
	certmgr -add -c CA certs/pathLenConstraint0CACert.crt
	certmgr -add -c CA certs/pathLenConstraint0subCACert.crt
	certmgr -add -c CA certs/pathLenConstraint0subCA2Cert.crt
	certmgr -add -c CA certs/pathLenConstraint6CACert.crt
	certmgr -add -c CA certs/pathLenConstraint6subCA0Cert.crt
	certmgr -add -c CA certs/pathLenConstraint6subsubCA00Cert.crt
	certmgr -add -c CA certs/pathLenConstraint6subCA1Cert.crt
	certmgr -add -c CA certs/pathLenConstraint6subsubCA11Cert.crt
	certmgr -add -c CA certs/pathLenConstraint6subsubsubCA11XCert.crt
	certmgr -add -c CA certs/pathLenConstraint6subCA4Cert.crt
	certmgr -add -c CA certs/pathLenConstraint6subsubCA41Cert.crt
	certmgr -add -c CA certs/pathLenConstraint6subsubsubCA41XCert.crt
	certmgr -add -c Trust certs/pathLenConstraint0SelfIssuedCACert.crt
	certmgr -add -c CA certs/pathLenConstraint1CACert.crt
	certmgr -add -c Trust certs/pathLenConstraint1SelfIssuedCACert.crt
	certmgr -add -c CA certs/pathLenConstraint1subCACert.crt
	certmgr -add -c CA certs/keyUsageCriticalkeyCertSignFalseCACert.crt
	certmgr -add -c CA certs/keyUsageNotCriticalkeyCertSignFalseCACert.crt
	certmgr -add -c CA certs/keyUsageNotCriticalCACert.crt
	certmgr -add -c CA certs/keyUsageCriticalcRLSignFalseCACert.crt
	certmgr -add -c CA certs/keyUsageNotCriticalcRLSignFalseCACert.crt
	certmgr -add -c CA certs/BasicSelfIssuedOldKeyCACert.crt
	certmgr -add -c CA certs/BasicSelfIssuedOldKeyNewWithOldCACert.crt
	certmgr -add -c CA certs/NoCRLCACert.crt
	certmgr -add -c CA certs/RevokedsubCACert.crt
	certmgr -add -c CA certs/BadCRLSignatureCACert.crt
	certmgr -add -c CA certs/BadCRLIssuerNameCACert.crt
	certmgr -add -c CA certs/WrongCRLCACert.crt
	certmgr -add -c CA certs/TwoCRLsCACert.crt
	certmgr -add -c CA certs/UnknownCRLEntryExtensionCACert.crt
	certmgr -add -c CA certs/UnknownCRLExtensionCACert.crt
	certmgr -add -c CA certs/OldCRLnextUpdateCACert.crt
	certmgr -add -c CA certs/pre2000CRLnextUpdateCACert.crt
	certmgr -add -c CA certs/GeneralizedTimeCRLnextUpdateCACert.crt
	certmgr -add -c CA certs/NegativeSerialNumberCACert.crt
	certmgr -add -c CA certs/LongSerialNumberCACert.crt
	certmgr -add -c CA certs/SeparateCertificateandCRLKeysCertificateSigningCACert.crt
	certmgr -add -c CA certs/SeparateCertificateandCRLKeysCRLSigningCert.crt
	certmgr -add -c CA certs/SeparateCertificateandCRLKeysCA2CertificateSigningCACert.crt
	certmgr -add -c CA certs/SeparateCertificateandCRLKeysCA2CRLSigningCert.crt

	# general (all sections)
	certmgr -add -crl Trust crls/TrustAnchorRootCRL.crl
	certmgr -add -crl CA crls/GoodCACRL.crl
	# section 4.1
	certmgr -add -crl CA crls/DSACACRL.crl
	certmgr -add -crl CA crls/DSAParametersInheritedCACRL.crl
	# section 4.2
	certmgr -add -crl CA crls/BadnotBeforeDateCACRL.crl
	certmgr -add -crl CA crls/BadnotAfterDateCACRL.crl
	# section 4.3
	certmgr -add -crl CA crls/UIDCACRL.crl
	certmgr -add -crl CA crls/RFC3280MandatoryAttributeTypesCACRL.crl
	certmgr -add -crl CA crls/RFC3280OptionalAttributeTypesCACRL.crl
	certmgr -add -crl CA crls/UTF8StringEncodedNamesCACRL.crl
	certmgr -add -crl CA crls/RolloverfromPrintableStringtoUTF8StringCACRL.crl
	certmgr -add -crl CA crls/UTF8StringCaseInsensitiveMatchCACRL.crl
	# section 4.4
	certmgr -add -crl CA crls/TwoCRLsCAGoodCRL.crl
	certmgr -add -crl CA crls/TwoCRLsCABadCRL.crl
	certmgr -add -crl CA crls/UnknownCRLEntryExtensionCACRL.crl
	certmgr -add -crl CA crls/UnknownCRLExtensionCACRL.crl
	certmgr -add -crl CA crls/OldCRLnextUpdateCACRL.crl
	certmgr -add -crl CA crls/pre2000CRLnextUpdateCACRL.crl
	certmgr -add -crl CA crls/GeneralizedTimeCRLnextUpdateCACRL.crl
	certmgr -add -crl CA crls/NegativeSerialNumberCACRL.crl
	certmgr -add -crl CA crls/LongSerialNumberCACRL.crl
	# section 4.5
	certmgr -add -crl CA crls/BasicSelfIssuedOldKeyCACRL.crl
	certmgr -add -crl CA crls/BasicSelfIssuedNewKeyCACRL.crl
	certmgr -add -crl CA crls/BasicSelfIssuedOldKeySelfIssuedCertCRL.crl
	certmgr -add -crl CA crls/BasicSelfIssuedCRLSigningKeyCACRL.crl
	certmgr -add -crl CA crls/BasicSelfIssuedCRLSigningKeyCRLCertCRL.crl
	# section 4.6
	certmgr -add -crl CA crls/MissingbasicConstraintsCACRL.crl
	certmgr -add -crl CA crls/basicConstraintsCriticalcAFalseCACRL.crl
	certmgr -add -crl CA crls/basicConstraintsNotCriticalCACRL.crl
	certmgr -add -crl CA crls/basicConstraintsNotCriticalcAFalseCACRL.crl
	certmgr -add -crl CA crls/pathLenConstraint0CACRL.crl
	certmgr -add -crl CA crls/pathLenConstraint0subCACRL.crl
	certmgr -add -crl CA crls/pathLenConstraint0subCA2CRL.crl
	certmgr -add -crl CA crls/pathLenConstraint6CACRL.crl
	certmgr -add -crl CA crls/pathLenConstraint6subCA0CRL.crl
	certmgr -add -crl CA crls/pathLenConstraint6subsubCA00CRL.crl
	certmgr -add -crl CA crls/pathLenConstraint6subCA1CRL.crl
	certmgr -add -crl CA crls/pathLenConstraint6subsubCA11CRL.crl
	certmgr -add -crl CA crls/pathLenConstraint6subsubsubCA11XCRL.crl
	certmgr -add -crl CA crls/pathLenConstraint6subCA4CRL.crl
	certmgr -add -crl CA crls/pathLenConstraint6subsubCA41CRL.crl
	certmgr -add -crl CA crls/pathLenConstraint6subsubsubCA41XCRL.crl
	certmgr -add -crl CA crls/pathLenConstraint1CACRL.crl
	certmgr -add -crl CA crls/pathLenConstraint1subCACRL.crl
	# section 4.7
	certmgr -add -crl CA crls/keyUsageCriticalkeyCertSignFalseCACRL.crl
	certmgr -add -crl CA crls/keyUsageNotCriticalkeyCertSignFalseCACRL.crl
	certmgr -add -crl CA crls/keyUsageNotCriticalCACRL.crl
	certmgr -add -crl CA crls/keyUsageCriticalcRLSignFalseCACRL.crl
	certmgr -add -crl CA crls/keyUsageNotCriticalcRLSignFalseCACRL.crl

on: register
	echo go > hint

off:
	rm -f hint

clean: off
	rm -rf certpairs
	rm -rf certs
	rm -rf crls
	rm -rf pkcs12
	rm -rf smime
	rm -f pkits.ldif
	rm -f pkits.schema
	rm -f ReadMe.txt
	rm -r x509build.exe*

clean-all: clean
	rm -f PKITS_data.zip
