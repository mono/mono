thisdir = class/System.Web.DataVisualization
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.DataVisualization.dll

OTHER_RES = \
	resources/System.Web.UI.DataVisualization.Charting.ChartControl.ico
OTHER_LIB_MCS_FLAGS = -d:FEATURE_PAL -d:SQLRS_CONTROL
	
RESOURCE_DEFS = \
	System.Web.UI.DataVisualization.Charting.Design,resources/System.Web.UI.DataVisualization.Charting.Design.resx \
	System.Web.UI.DataVisualization.Charting.SR,resources/System.Web.UI.DataVisualization.Charting.SR.resx 
	 
LIB_REFS = mscorlib System System.Core System.Data System.Drawing System.Web System.Xml

KEYFILE = ../msfinal.pub
LIB_MCS_FLAGS = \
	-delaysign \
	$(OTHER_LIB_MCS_FLAGS) \
	$(OTHER_RES:%=/resource:%)

include ../../build/library.make
