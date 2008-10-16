thisdir = class/monodoc
SUBDIRS = 
include ../../build/rules.make

LIBRARY = monodoc.dll
LIBRARY_PACKAGE = none

LIB_MCS_FLAGS = \
	/codepage:utf8                                                    \
	/nowarn:169,164,162,168,219                                       \
	/r:Commons.Xml.Relaxng                                            \
	/resource:Resources/base.css,base.css                             \
	/resource:Resources/ecmaspec-html-css.xsl,ecmaspec-html-css.xsl   \
	/resource:Resources/ecmaspec-html.xsl,ecmaspec-html.xsl           \
	/resource:Resources/ecmaspec.css,ecmaspec.css                     \
	/resource:Resources/helper.js,helper.js                           \
	/resource:Resources/home.html,home.html                           \
	/resource:Resources/Lminus.gif                                    \
	/resource:Resources/Lplus.gif                                     \
	/resource:Resources/mdoc-html-utils.xsl,mdoc-html-utils.xsl       \
	/resource:Resources/mdoc-sections-css.xsl,mdoc-sections-css.xsl   \
	/resource:Resources/mdoc-sections.xsl,mdoc-sections.xsl           \
	/resource:Resources/mono-ecma-css.js,mono-ecma-css.js             \
	/resource:Resources/mono-ecma-css.xsl,mono-ecma-css.xsl           \
	/resource:Resources/mono-ecma-impl.xsl,mono-ecma-impl.xsl         \
	/resource:Resources/mono-ecma.css,mono-ecma.css                   \
	/resource:Resources/mono-ecma.xsl,mono-ecma.xsl                   \
	/r:ICSharpCode.SharpZipLib                                        \
	/r:$(corlib)                                                      \
	/r:System.dll                                                     \
	/r:System.Web                                                     \
	/r:System.Web.Services                                            \
	/r:System.Xml.dll

include ../../build/library.make

