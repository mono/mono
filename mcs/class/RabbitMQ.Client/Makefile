thisdir = class/RabbitMQ.Client
SUBDIRS = src/apigen docs/specs src/client
#SUBDIRS = src/client
include ../../build/rules.make

DISTFILES = \
	src/util/BlockingCell.cs \
	src/util/DebugUtil.cs \
	src/util/Either.cs \
	src/util/IntAllocator.cs \
	src/util/NetworkBinaryReader.cs \
	src/util/NetworkBinaryWriter.cs \
	src/util/Semaphore.cs \
	src/util/SharedQueue.cs \
	src/util/XmlUtil.cs \
	src/apigen/AmqpClass.cs \
	src/apigen/AmqpEntity.cs \
	src/apigen/AmqpField.cs \
	src/apigen/AmqpMethod.cs \
	src/apigen/Apigen.cs \
	src/apigen/Makefile \
	docs/specs/amqp0-8.stripped.xml \
	docs/specs/amqp0-9.stripped.xml \
	docs/specs/qpid-amqp.0-8.stripped.xml \
	docs/specs/Makefile

all-local install-local clean-local test-local run-test-local run-test-ondotnet-local uninstall-local doc-update-local:

dist-local: dist-default

