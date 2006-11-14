PREFIX	= /work/asgard
MONO	= $(PREFIX)/INSTALL/bin/mono

all:	test-mcs test-gmcs

test-mcs:
	$(MONO) --debug $(PREFIX)/mono/mcs/class/lib/default/compiler-tester.exe positive 'test-*.cs' $(PREFIX)/mono/mcs/mcs/mcs.exe known-issues-mcs mcs.log

test-gmcs:
	$(MONO) --debug $(PREFIX)/mono/mcs/class/lib/net_2_0/compiler-tester.exe positive 'gtest-*.cs' $(PREFIX)/mono/mcs/gmcs/gmcs.exe known-issues-gmcs gmcs.log

