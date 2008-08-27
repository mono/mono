thisdir = tools
SUBDIRS = \
	al cilc corcompare culevel ictool mono-xsd security wsdl genxs \
	disco soapsuds browsercaps-updater monop gacutil linker	tuner \
	resgen macpack mkbundle dtd2rng dtd2xsd mjs prj2make mono-service \
	mono-shlib-cop compiler-tester mono-xmltool installvst installutil

net_1_1_bootstrap_SUBDIRS = gacutil security resgen culevel
net_2_0_bootstrap_SUBDIRS = resgen

net_2_0_SUBDIRS = \
	al corcompare mono-xsd wsdl compiler-tester monop xbuild resgen \
	mono-service mkbundle sgen security mconfig installutil nunitreport sqlsharp \
	gacutil
net_2_1_raw_SUBDIRS = compiler-tester
net_2_1_SUBDIRS = compiler-tester tuner

DIST_ONLY_SUBDIRS = xbuild sgen mconfig nunitreport sqlsharp

include ../build/rules.make

DISTFILES = \
	assemblies.xml			\
	mono-win32-setup-dark.bmp	\
	mono-win32-setup-light.bmp	\
	mono-win32-setup.nsi		\
	scan-tests.pl			\
	tinderbox/smtp.c		\
	tinderbox/tinderbox.sh		\
	gensources.sh			\
	removecomments.sh

test-local run-test-local run-test-ondotnet-local all-local install-local uninstall-local:
	@:

dist-local: dist-default

clean-local:
	rm -f *.exe *.dll *.pdb cormissing.xml
