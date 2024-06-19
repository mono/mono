thisdir = class/System.Web.Mobile
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Mobile.dll

OTHER_RES = \
	resources/System.Web.UI.MobileControls.Command.bmp \
	resources/System.Web.UI.MobileControls.CompareValidator.bmp \
	resources/System.Web.UI.MobileControls.CustomValidator.bmp \
	resources/System.Web.UI.MobileControls.DeviceSpecific.bmp \
	resources/System.Web.UI.MobileControls.Form.bmp \
	resources/System.Web.UI.MobileControls.Image.bmp \
	resources/System.Web.UI.MobileControls.Label.bmp \
	resources/System.Web.UI.MobileControls.Link.bmp \
	resources/System.Web.UI.MobileControls.List.bmp \
	resources/System.Web.UI.MobileControls.ObjectList.bmp \
	resources/System.Web.UI.MobileControls.Panel.bmp \
	resources/System.Web.UI.MobileControls.PhoneCall.bmp \
	resources/System.Web.UI.MobileControls.RangeValidator.bmp \
	resources/System.Web.UI.MobileControls.RegularExpressionValidator.bmp \
	resources/System.Web.UI.MobileControls.RequiredFieldValidator.bmp \
	resources/System.Web.UI.MobileControls.SelectionList.bmp \
	resources/System.Web.UI.MobileControls.StyleSheet.bmp \
	resources/System.Web.UI.MobileControls.TextBox.bmp \
	resources/System.Web.UI.MobileControls.TextView.bmp \
	resources/System.Web.UI.MobileControls.ValidationSummary.bmp


RESOURCE_DEFS = \
	System.Web.Resources.Mobile,resources/System.Web.Mobile.resx \
	System.Web.Resources.Mobile.Adapters,resources/System.Web.Mobile.Adapters.resx 
	
LIB_REFS = mscorlib System System.Configuration System.Data System.Design System.Drawing System.Drawing.Design System.Web System.Web.RegularExpressions \
 	System.Windows.Forms System.Xml 

KEYFILE = ../msfinal.pub
LIB_MCS_FLAGS = \
	-delaysign \
	-nowarn:612,618,672	\
	$(OTHER_RES:%=/resource:%)

include ../../build/library.make
