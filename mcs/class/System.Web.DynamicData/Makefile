thisdir = class/System.Web.DynamicData
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.DynamicData.dll
LIB_MCS_FLAGS = \
	        /r:System.dll \
	        /r:System.Core.dll \
	        /r:System.ComponentModel.DataAnnotations.dll \
		/r:System.Data.dll \
		/r:System.Data.Linq.dll \
	        /r:System.Drawing.dll \
	        /r:System.Web.dll \
	        /r:System.Web.Extensions.dll \
		/r:System.Web.Abstractions.dll \
		/r:System.Web.Routing.dll

ifeq (2.0, $(FRAMEWORK_VERSION))
# This is a .NET 3.5 only assembly, but built during the 2.0 build
LIB_MCS_FLAGS += -d:NET_3_5
endif

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_MCS_FLAGS += -r:System.Web.ApplicationServices.dll
endif

# Use genresources.sh to generate the list below
TEST_RESOURCE_FILES = \
	Test/WebPages/DynamicData/Content/FilterUserControl.ascx,MonoTests.WebPages.DynamicData.Content.FilterUserControl.ascx	\
	Test/WebPages/DynamicData/Content/FilterUserControl.ascx.cs,MonoTests.WebPages.DynamicData.Content.FilterUserControl.ascx.cs	\
	Test/WebPages/DynamicData/Content/GridViewPager.ascx,MonoTests.WebPages.DynamicData.Content.GridViewPager.ascx	\
	Test/WebPages/DynamicData/Content/GridViewPager.ascx.cs,MonoTests.WebPages.DynamicData.Content.GridViewPager.ascx.cs	\
	Test/WebPages/DynamicData/Content/Images/Back.gif,MonoTests.WebPages.DynamicData.Content.Images.Back.gif	\
	Test/WebPages/DynamicData/Content/Images/header_back.gif,MonoTests.WebPages.DynamicData.Content.Images.header_back.gif	\
	Test/WebPages/DynamicData/Content/Images/PgFirst.gif,MonoTests.WebPages.DynamicData.Content.Images.PgFirst.gif	\
	Test/WebPages/DynamicData/Content/Images/PgLast.gif,MonoTests.WebPages.DynamicData.Content.Images.PgLast.gif	\
	Test/WebPages/DynamicData/Content/Images/PgNext.gif,MonoTests.WebPages.DynamicData.Content.Images.PgNext.gif	\
	Test/WebPages/DynamicData/Content/Images/PgPrev.gif,MonoTests.WebPages.DynamicData.Content.Images.PgPrev.gif	\
	Test/WebPages/DynamicData/Content/Images/plus.gif,MonoTests.WebPages.DynamicData.Content.Images.plus.gif	\
	Test/WebPages/DynamicData/FieldTemplates/Boolean.ascx,MonoTests.WebPages.DynamicData.FieldTemplates.Boolean.ascx	\
	Test/WebPages/DynamicData/FieldTemplates/Boolean.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates.Boolean.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates/Boolean_Edit.ascx,MonoTests.WebPages.DynamicData.FieldTemplates.Boolean_Edit.ascx	\
	Test/WebPages/DynamicData/FieldTemplates/Boolean_Edit.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates.Boolean_Edit.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates/Children.ascx,MonoTests.WebPages.DynamicData.FieldTemplates.Children.ascx	\
	Test/WebPages/DynamicData/FieldTemplates/Children.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates.Children.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates/CustomColor.ascx,MonoTests.WebPages.DynamicData.FieldTemplates.CustomColor.ascx	\
	Test/WebPages/DynamicData/FieldTemplates/CustomColor.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates.CustomColor.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates/CustomFieldTemplate.ascx,MonoTests.WebPages.DynamicData.FieldTemplates.CustomFieldTemplate.ascx	\
	Test/WebPages/DynamicData/FieldTemplates/CustomFieldTemplate.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates.CustomFieldTemplate.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates/DateTime.ascx,MonoTests.WebPages.DynamicData.FieldTemplates.DateTime.ascx	\
	Test/WebPages/DynamicData/FieldTemplates/DateTime.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates.DateTime.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates/DateTime_Edit.ascx,MonoTests.WebPages.DynamicData.FieldTemplates.DateTime_Edit.ascx	\
	Test/WebPages/DynamicData/FieldTemplates/DateTime_Edit.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates.DateTime_Edit.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates/Decimal_Edit.ascx,MonoTests.WebPages.DynamicData.FieldTemplates.Decimal_Edit.ascx	\
	Test/WebPages/DynamicData/FieldTemplates/Decimal_Edit.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates.Decimal_Edit.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates/ForeignKey.ascx,MonoTests.WebPages.DynamicData.FieldTemplates.ForeignKey.ascx	\
	Test/WebPages/DynamicData/FieldTemplates/ForeignKey.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates.ForeignKey.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates/ForeignKey_Edit.ascx,MonoTests.WebPages.DynamicData.FieldTemplates.ForeignKey_Edit.ascx	\
	Test/WebPages/DynamicData/FieldTemplates/ForeignKey_Edit.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates.ForeignKey_Edit.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates/Integer_Edit.ascx,MonoTests.WebPages.DynamicData.FieldTemplates.Integer_Edit.ascx	\
	Test/WebPages/DynamicData/FieldTemplates/Integer_Edit.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates.Integer_Edit.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates/MultilineText_Edit.ascx,MonoTests.WebPages.DynamicData.FieldTemplates.MultilineText_Edit.ascx	\
	Test/WebPages/DynamicData/FieldTemplates/MultilineText_Edit.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates.MultilineText_Edit.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates/MyCustomUIHintTemplate_Text.ascx,MonoTests.WebPages.DynamicData.FieldTemplates.MyCustomUIHintTemplate_Text.ascx	\
	Test/WebPages/DynamicData/FieldTemplates/MyCustomUIHintTemplate_Text.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates.MyCustomUIHintTemplate_Text.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/Boolean.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.Boolean.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/Boolean.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.Boolean.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/Byte.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.Byte.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/Byte[].ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.Byte[].ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/Byte.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.Byte.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/Byte[].ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.Byte[].ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/Char.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.Char.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/Char.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.Char.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/FooEmpty.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.FooEmpty.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/FooEmpty.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.FooEmpty.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/ICollection.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.ICollection.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/ICollection.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.ICollection.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/Int16.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.Int16.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/Int16.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.Int16.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/Int32.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.Int32.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/Int32.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.Int32.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/Int64.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.Int64.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/Int64.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.Int64.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/MonoTests.Common.FooEmpty.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.MonoTests.Common.FooEmpty.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/MonoTests.Common.FooEmpty.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.MonoTests.Common.FooEmpty.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/Object.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.Object.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/Object.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.Object.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/SByte.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.SByte.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/SByte.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.SByte.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/String.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.String.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/String.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.String.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Boolean.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Boolean.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Boolean.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Boolean.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Byte.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Byte.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Byte[].ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Byte[].ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Byte.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Byte.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Byte[].ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Byte[].ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Char.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Char.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Char.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Char.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Collections.Generic.List\`1[System.String].ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Collections.Generic.List\`1[System.String].ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Collections.Generic.List\`1[System.String].ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Collections.Generic.List\`1[System.String].ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Collections.ICollection.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Collections.ICollection.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Collections.ICollection.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Collections.ICollection.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Int16.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Int16.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Int16.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Int16.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Int32.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Int32.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Int32.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Int32.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Int64.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Int64.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Int64.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Int64.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Object.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Object.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.Object.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.Object.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.SByte.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.SByte.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.SByte.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.SByte.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.String.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.String.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.String.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.String.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.UInt16.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.UInt16.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.UInt16.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.UInt16.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.UInt32.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.UInt32.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.UInt32.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.UInt32.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.UInt64.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.UInt64.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/System.UInt64.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.System.UInt64.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/UInt16.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.UInt16.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/UInt16.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.UInt16.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/UInt32.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.UInt32.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/UInt32.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.UInt32.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/UInt64.ascx,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.UInt64.ascx	\
	Test/WebPages/DynamicData/FieldTemplates_NonDefault/UInt64.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault.UInt64.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates/PlainControlTemplate.ascx,MonoTests.WebPages.DynamicData.FieldTemplates.PlainControlTemplate.ascx	\
	Test/WebPages/DynamicData/FieldTemplates/PlainControlTemplate.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates.PlainControlTemplate.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates/Text.ascx,MonoTests.WebPages.DynamicData.FieldTemplates.Text.ascx	\
	Test/WebPages/DynamicData/FieldTemplates/Text.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates.Text.ascx.cs	\
	Test/WebPages/DynamicData/FieldTemplates/Text_Edit.ascx,MonoTests.WebPages.DynamicData.FieldTemplates.Text_Edit.ascx	\
	Test/WebPages/DynamicData/FieldTemplates/Text_Edit.ascx.cs,MonoTests.WebPages.DynamicData.FieldTemplates.Text_Edit.ascx.cs	\
	Test/WebPages/DynamicData/PageTemplates/Details.aspx,MonoTests.WebPages.DynamicData.PageTemplates.Details.aspx	\
	Test/WebPages/DynamicData/PageTemplates/Details.aspx.cs,MonoTests.WebPages.DynamicData.PageTemplates.Details.aspx.cs	\
	Test/WebPages/DynamicData/PageTemplates/Edit.aspx,MonoTests.WebPages.DynamicData.PageTemplates.Edit.aspx	\
	Test/WebPages/DynamicData/PageTemplates/Edit.aspx.cs,MonoTests.WebPages.DynamicData.PageTemplates.Edit.aspx.cs	\
	Test/WebPages/DynamicData/PageTemplates/Insert.aspx,MonoTests.WebPages.DynamicData.PageTemplates.Insert.aspx	\
	Test/WebPages/DynamicData/PageTemplates/Insert.aspx.cs,MonoTests.WebPages.DynamicData.PageTemplates.Insert.aspx.cs	\
	Test/WebPages/DynamicData/PageTemplates/List.aspx,MonoTests.WebPages.DynamicData.PageTemplates.List.aspx	\
	Test/WebPages/DynamicData/PageTemplates/List.aspx.cs,MonoTests.WebPages.DynamicData.PageTemplates.List.aspx.cs	\
	Test/WebPages/DynamicData/PageTemplates/ListDetails.aspx,MonoTests.WebPages.DynamicData.PageTemplates.ListDetails.aspx	\
	Test/WebPages/DynamicData/PageTemplates/ListDetails.aspx.cs,MonoTests.WebPages.DynamicData.PageTemplates.ListDetails.aspx.cs	\
	Test/WebPages/DynamicData/web.config,MonoTests.WebPages.DynamicData.web.config	\
	Test/WebPages/Global.asax,MonoTests.WebPages.Global.asax	\
	Test/WebPages/ListView_DynamicControl_01.aspx,MonoTests.WebPages.ListView_DynamicControl_01.aspx	\
	Test/WebPages/ListView_DynamicControl_01.aspx.cs,MonoTests.WebPages.ListView_DynamicControl_01.aspx.cs	\
	Test/WebPages/ListView_DynamicControl_02.aspx,MonoTests.WebPages.ListView_DynamicControl_02.aspx	\
	Test/WebPages/ListView_DynamicControl_02.aspx.cs,MonoTests.WebPages.ListView_DynamicControl_02.aspx.cs	\
	Test/WebPages/ListView_DynamicControl_03.aspx,MonoTests.WebPages.ListView_DynamicControl_03.aspx	\
	Test/WebPages/ListView_DynamicControl_03.aspx.cs,MonoTests.WebPages.ListView_DynamicControl_03.aspx.cs	\
	Test/WebPages/ListView_DynamicControl_04.aspx,MonoTests.WebPages.ListView_DynamicControl_04.aspx	\
	Test/WebPages/ListView_DynamicControl_04.aspx.cs,MonoTests.WebPages.ListView_DynamicControl_04.aspx.cs	\
	Test/WebPages/ListView_DynamicControl_05.aspx,MonoTests.WebPages.ListView_DynamicControl_05.aspx	\
	Test/WebPages/ListView_DynamicControl_05.aspx.cs,MonoTests.WebPages.ListView_DynamicControl_05.aspx.cs	\
	Test/WebPages/ListView_DynamicControl_06.aspx,MonoTests.WebPages.ListView_DynamicControl_06.aspx	\
	Test/WebPages/ListView_DynamicControl_06.aspx.cs,MonoTests.WebPages.ListView_DynamicControl_06.aspx.cs	\
	Test/WebPages/ListView_DynamicControl_07.aspx,MonoTests.WebPages.ListView_DynamicControl_07.aspx	\
	Test/WebPages/ListView_DynamicControl_07.aspx.cs,MonoTests.WebPages.ListView_DynamicControl_07.aspx.cs	\
	Test/WebPages/ListView_DynamicControl_08.aspx,MonoTests.WebPages.ListView_DynamicControl_08.aspx	\
	Test/WebPages/ListView_DynamicControl_08.aspx.cs,MonoTests.WebPages.ListView_DynamicControl_08.aspx.cs	\
	Test/WebPages/ListView_DynamicControl_09.aspx,MonoTests.WebPages.ListView_DynamicControl_09.aspx	\
	Test/WebPages/ListView_DynamicControl_09.aspx.cs,MonoTests.WebPages.ListView_DynamicControl_09.aspx.cs	\
	Test/WebPages/ListView_DynamicControl_10.aspx,MonoTests.WebPages.ListView_DynamicControl_10.aspx	\
	Test/WebPages/ListView_DynamicControl_10.aspx.cs,MonoTests.WebPages.ListView_DynamicControl_10.aspx.cs	\
	Test/WebPages/DynamicValidator_01.aspx,MonoTests.WebPages.DynamicValidator_01.aspx			\
	Test/WebPages/DynamicValidator_01.aspx.cs,MonoTests.WebPages.DynamicValidator_01.aspx.cs		\
	Test/WebPages/DynamicValidator_02.aspx,MonoTests.WebPages.DynamicValidator_02.aspx			\
	Test/WebPages/DynamicValidator_02.aspx.cs,MonoTests.WebPages.DynamicValidator_02.aspx.cs		\
	Test/WebPages/Site.css,MonoTests.WebPages.Site.css	\
	Test/WebPages/Site.master,MonoTests.WebPages.Site.master	\
	Test/WebPages/Site.master.cs,MonoTests.WebPages.Site.master.cs	\
	Test/WebPages/web.config.2.0,MonoTests.WebPages.web.config.2.0 \
	Test/WebPages/web.config.4.0,MonoTests.WebPages.web.config.4.0

NUNIT_RESOURCE_FILES = $(TEST_RESOURCE_FILES)

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -r:System.ComponentModel.DataAnnotations.dll -r:System.Configuration.dll $(NUNIT_RESOURCE_FILES:%=/resource:%) -r:SystemWebTestShim.dll

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
TEST_MCS_FLAGS += -r:System.Web.ApplicationServices.dll
endif

EXTRA_DISTFILES = $(foreach resource,$(TEST_RESOURCE_FILES), $(shell echo $(subst \`,\\\`,$(resource)) | cut -d ',' -f 1))

# This is a .NET 3.5+ assembly
VALID_PROFILE := $(filter net_2_0 net_4_0, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Web.DynamicData.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
