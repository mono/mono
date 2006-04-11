thisdir = tools
SUBDIRS = \
	al cilc corcompare ictool mono-xsd security wsdl genxs sqlsharp		\
	disco soapsuds browsercaps-updater monop gacutil	\
	resgen macpack mkbundle dtd2rng dtd2xsd mjs prj2make mono-service \
	mono-shlib-cop compiler-tester mono-xmltool

net_1_1_bootstrap_SUBDIRS = gacutil security resgen
net_2_0_bootstrap_SUBDIRS = resgen

net_2_0_SUBDIRS = corcompare wsdl compiler-tester monop xbuild resgen

DIST_ONLY_SUBDIRS = xbuild

include ../build/rules.make

DISTFILES = \
	assemblies.xml			\
	mono-win32-setup-dark.bmp	\
	mono-win32-setup-light.bmp	\
	mono-win32-setup.nsi		\
	scan-tests.pl			\
	tinderbox/smtp.c		\
	tinderbox/tinderbox.sh

test-local run-test-local run-test-ondotnet-local all-local install-local uninstall-local:
	@:

dist-local: dist-default

clean-local:
	rm -f *.exe *.dll *.pdb cormissing.xml
