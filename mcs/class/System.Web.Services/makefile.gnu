topdir = ../..

LIBRARY = ../lib/System.Web.Services.dll

LIB_LIST = list
LIB_FLAGS = -r corlib -r System.Xml -r System.EnterpriseServices -r System.Web -r System

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=./Test*

export MONO_PATH_PREFIX = ../lib:

include ../library.make
