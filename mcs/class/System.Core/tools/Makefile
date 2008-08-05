all: hashwrap.exe

wrappers: hashwrap.exe
	cd ../System.Security.Cryptography; \
	mono --debug ../tools/hashwrap.exe MD5Cng MD5 MD5CryptoServiceProvider \
		SHA1Cng SHA1 SHA1Managed \
		SHA256Cng SHA256 SHA256Managed \
		SHA384Cng SHA384 SHA384Managed \
		SHA512Cng SHA512 SHA512Managed \
		SHA256CryptoServiceProvider SHA256 SHA256Managed \
		SHA384CryptoServiceProvider SHA384 SHA384Managed \
		SHA512CryptoServiceProvider SHA512 SHA512Managed; \
	cd ../tools;

hashwrap.exe: hashwrap.cs
	mcs -debug hashwrap.cs

clean:
	rm -f *.exe*
