TOP=$(realpath $(CURDIR)/../..)
include $(TOP)/sdks/paths.mk

run: $(CURDIR)/net_4_x_Mono.Debugger.Soft_test.dll $(TOP)/sdks/out/desktop-x86_64/bin/mono-sgen
	MONO_TESTS_IN_PROGRESS="yes" MONO_PATH="$(CURDIR)/../out/desktop-bcl/net_4_x" $(TOP)/sdks/out/desktop-x86_64/bin/mono-sgen --debug $(CURDIR)/../out/desktop-bcl/net_4_x/nunit-lite-console.exe $(CURDIR)/net_4_x_Mono.Debugger.Soft_test.dll -exclude=NotOnMac,MacNotWorking,NotWorking,CAS  -format:nunit2

.PHONY: $(CURDIR)/net_4_x_Mono.Debugger.Soft_test.dll
$(CURDIR)/net_4_x_Mono.Debugger.Soft_test.dll: $(TOP)/mcs/class/lib/net_4_x/tests/net_4_x_Mono.Debugger.Soft_test.dll
	cp $(TOP)/mcs/class/lib/net_4_x/tests/net_4_x_Mono.Debugger.Soft_test.dll $(CURDIR)/net_4_x_Mono.Debugger.Soft_test.dll
	cp $(TOP)/mcs/class/lib/net_4_x/nunit* $(CURDIR)/../out/desktop-bcl/net_4_x

.PHONY: $(TOP)/mcs/class/lib/net_4_x/tests/net_4_x_Mono.Debugger.Soft_test.dll
$(TOP)/mcs/class/lib/net_4_x/tests/net_4_x_Mono.Debugger.Soft_test.dll:
	$(MAKE) PROFILE='net_4_x' -C $(TOP)/mcs/class/Mono.Debugger.Soft test

