PREFIX	= /work/asgard
MONO	= $(PREFIX)/INSTALL/bin/mono

all:	mcs gmcs

mcs:
	$(MAKE) -C $(PREFIX)/mono/mcs/mcs MCS_FLAGS='/debug /define:MCS_DEBUG'

gmcs:
	$(MAKE) -C $(PREFIX)/mono/mcs/gmcs PROFILE=net_2_0 DEBUG_FLAGS='/debug /define:MCS_DEBUG'

test:
	$(MONO) --debug $(PREFIX)/mono/mcs/class/lib/net_2_0/compiler-tester.exe positive 'martin-test-*.cs' $(PREFIX)/mono/mcs/gmcs/gmcs.exe known-issues martin.log

test-all:	test
	$(MAKE) -C $(PREFIX)/mono/mcs/tests test-everything

