thisdir = tools

net_1_1_dirs := \
	al			\
	cilc		\
	culevel		\
	ictool		\
	mono-xsd	\
	security	\
	wsdl		\
	genxs		\
	disco		\
	soapsuds	\
	browsercaps-updater	\
	monop		\
	gacutil		\
	cil-strip	\
	resgen		\
	macpack		\
	mkbundle	\
	dtd2rng		\
	dtd2xsd		\
	mjs			\
	prj2make	\
	mono-service	\
	monodoc		\
	mod		\
	compiler-tester	\
	installvst		\
	installutil

net_2_0_dirs := \
	al			\
	corcompare	\
	linker		\
	tuner		\
	csharplib	\
	csharp		\
	mono-xsd	\
	wsdl		\
	mono-xmltool	\
	compiler-tester	\
	monop		\
	xbuild		\
	resgen		\
	mdoc		\
	mono-service	\
	mono-shlib-cop	\
	mkbundle	\
	sgen		\
	security	\
	mconfig		\
	installutil	\
	nunitreport	\
	pdb2mdb		\
	sqlsharp	\
	sqlmetal	\
	gacutil		\
	svcutil		\
	lc

net_2_1_dirs := \
	compiler-tester
	
net_1_1_bootstrap_SUBDIRS = gacutil security resgen culevel
net_1_1_SUBDIRS := $(net_1_1_dirs)
net_2_0_bootstrap_SUBDIRS = resgen
net_2_0_SUBDIRS := $(net_2_0_dirs)
net_2_1_raw_SUBDIRS := $(net_2_1_dirs)
net_2_1_SUBDIRS := $(net_2_1_dirs)
net_4_0_bootstrap_SUBDIRS = resgen
net_4_0_SUBDIRS := $(net_2_0_dirs)

SUBDIRS = $(net_1_1_SUBDIRS) $(net_2_0_SUBDIRS) $(net_2_1_SUBDIRS)

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

test-local run-test-local run-test-ondotnet-local all-local install-local uninstall-local doc-update-local:
	@:

dist-local: dist-default

clean-local:
	rm -f *.exe *.dll *.pdb cormissing.xml
