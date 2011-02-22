thisdir = class/RabbitMQ.Client/docs/specs
SUBDIRS =
include ../../../../build/rules.make

all-local: $(PROFILE)-api-0-9.cs $(PROFILE)-api-0-8.cs $(PROFILE)-api-qpid-0-8.cs

copy:
	cp ../../../lib/net_2_0/apigen-bootstrap.dll ../../src/apigen/.

$(PROFILE)-api-0-9.cs: amqp0-9.stripped.xml
	MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) ../../src/apigen/RabbitMQ.Client.Apigen.exe /n:v0_9 "/apiName:AMQP_0_9" $^ $@

$(PROFILE)-api-0-8.cs: amqp0-8.stripped.xml
	MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) ../../src/apigen/RabbitMQ.Client.Apigen.exe /n:v0_8 "/apiName:AMQP_0_8" $^ $@
	
$(PROFILE)-api-qpid-0-8.cs: qpid-amqp.0-8.stripped.xml
	MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) ../../src/apigen/RabbitMQ.Client.Apigen.exe /n:v0_8qpid "/apiName:AMQP_0_8_QPID" $^ $@

install-local:

clean-local:
	rm -f ../../src/apigen/apigen-bootstrap.dll
	rm -f $(PROFILE)-api*.cs
	
test-local:

run-test-local:

run-test-ondotnet-local:

DISTFILES =

dist-local:

uninstall-local:
