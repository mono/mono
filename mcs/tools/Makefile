thisdir = tools
SUBDIRS = al cilc corcompare ictool mono-xsd security wsdl genxs SqlSharp disco soapsuds browsercaps-updater monop gacutil mono-rpm-helpers

net_1_1_bootstrap_SUBDIRS = security

net_2_0_SUBDIRS = wsdl

include ../build/rules.make

PROGRAM_INSTALL_DIR = $(prefix)/bin

DISTFILES = \
	assemblies.xml			\
	mono-win32-setup-dark.bmp	\
	mono-win32-setup-light.bmp	\
	mono-win32-setup.nsi		\
	scan-tests.pl			\
	tinderbox/smtp.c		\
	tinderbox/tinderbox.sh

test-local run-test-local all-local install-local:

dist-local: dist-default

