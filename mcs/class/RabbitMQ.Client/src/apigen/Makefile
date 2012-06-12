thisdir := class/RabbitMQ.Client/src/apigen
SUBDIRS =

include ../../../../build/rules.make

PROGRAM = RabbitMQ.Client.Apigen.exe

LOCAL_MCS_FLAGS = /r:System.dll     \
                  /r:System.Xml.dll \
                  /main:RabbitMQ.Client.Apigen.Apigen

include ../../../../build/executable.make
