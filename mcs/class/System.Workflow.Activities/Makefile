thisdir = class/System.Workflow.Activities
include ../../build/rules.make
SUBDIRS =

LIBRARY = System.Workflow.Activities.dll


LIB_MCS_FLAGS = /r:$(corlib) /nowarn:114  /r:System.dll /r:System.Workflow.ComponentModel.dll

include ../../build/library.make

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) /r:System.Workflow.Activities.dll /r:System.Workflow.Runtime.dll



