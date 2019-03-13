using System.Globalization;
using System.Resources;
using System.Threading;

namespace System.Web.Mobile
{
  internal class SR
  {
    internal const string ConfigSect_MissingAttr = "ConfigSect_MissingAttr";
    internal const string ConfigSect_UnknownAttr = "ConfigSect_UnknownAttr";
    internal const string ConfigSect_UnrecognizedXML = "ConfigSect_UnrecognizedXML";
    internal const string ConfigSect_MissingValue = "ConfigSect_MissingValue";
    internal const string ConfigSect_InvalidBooleanAttr = "ConfigSect_InvalidBooleanAttr";
    internal const string ConfigSect_InvalidIntegerAttr = "ConfigSect_InvalidIntegerAttr";
    internal const string DevCapSect_EmptyClass = "DevCapSect_EmptyClass";
    internal const string DevCapSect_ExtraCompareDelegator = "DevCapSect_ExtraCompareDelegator";
    internal const string DevCapSect_ExtraArgumentDelegator = "DevCapSect_ExtraArgumentDelegator";
    internal const string DevCapSect_NoTypeInfo = "DevCapSect_NoTypeInfo";
    internal const string DevCapSect_NoCapabilityEval = "DevCapSect_NoCapabilityEval";
    internal const string DevCapSect_MustSpecify = "DevCapSect_MustSpecify";
    internal const string DevCapSect_ComparisonAlreadySpecified = "DevCapSect_ComparisonAlreadySpecified";
    internal const string DevCapSect_UnableAddDelegate = "DevCapSect_UnableAddDelegate";
    internal const string DevCapSect_UnrecognizedTag = "DevCapSect_UnrecognizedTag";
    internal const string DevFiltDict_FoundLoop = "DevFiltDict_FoundLoop";
    internal const string MobCap_DelegateNameNoValue = "MobCap_DelegateNameNoValue";
    internal const string MobCap_CantFindCapability = "MobCap_CantFindCapability";
    internal const string MobileRedirect_RedirectNotAllowed = "MobileRedirect_RedirectNotAllowed";
    internal const string FactoryGenerator_Error_FactoryInterface = "FactoryGenerator_Error_FactoryInterface";
    internal const string RTL = "RTL";
    internal const string Category_Action = "Category_Action";
    internal const string Category_Appearance = "Category_Appearance";
    internal const string Category_Behavior = "Category_Behavior";
    internal const string Category_Data = "Category_Data";
    internal const string Category_Navigation = "Category_Navigation";
    internal const string Category_Paging = "Category_Paging";
    internal const string Category_Style = "Category_Style";
    internal const string MSHTMLHost_Not_Implemented = "MSHTMLHost_Not_Implemented";
    internal const string AdRotator_AdvertisementFile = "AdRotator_AdvertisementFile";
    internal const string AdRotator_ImageKey = "AdRotator_ImageKey";
    internal const string AdRotator_NavigateUrlKey = "AdRotator_NavigateUrlKey";
    internal const string AdRotator_KeywordFilter = "AdRotator_KeywordFilter";
    internal const string AdRotator_AdCreated = "AdRotator_AdCreated";
    internal const string BaseValidator_ControlToValidate = "BaseValidator_ControlToValidate";
    internal const string BaseValidator_Display = "BaseValidator_Display";
    internal const string BaseValidator_ErrorMessage = "BaseValidator_ErrorMessage";
    internal const string Calendar_FirstDayOfWeek = "Calendar_FirstDayOfWeek";
    internal const string Calendar_SelectedDate = "Calendar_SelectedDate";
    internal const string Calendar_SelectionMode = "Calendar_SelectionMode";
    internal const string Calendar_ShowDayHeader = "Calendar_ShowDayHeader";
    internal const string Calendar_VisibleDate = "Calendar_VisibleDate";
    internal const string Calendar_CalendarEntryText = "Calendar_CalendarEntryText";
    internal const string Calendar_OnSelectionChanged = "Calendar_OnSelectionChanged";
    internal const string PhoneCall_AlternateFormat = "PhoneCall_AlternateFormat";
    internal const string PhoneCall_AlternateUrl = "PhoneCall_AlternateUrl";
    internal const string PhoneCall_PhoneNumber = "PhoneCall_PhoneNumber";
    internal const string PhoneCall_SoftkeyLabel = "PhoneCall_SoftkeyLabel";
    internal const string Command_CommandArgument = "Command_CommandArgument";
    internal const string Command_CommandName = "Command_CommandName";
    internal const string Command_CausesValidation = "Command_CausesValidation";
    internal const string Command_OnClick = "Command_OnClick";
    internal const string Command_OnItemCommand = "Command_OnItemCommand";
    internal const string Command_SoftkeyLabel = "Command_SoftkeyLabel";
    internal const string Command_Format = "Command_Format";
    internal const string CompareValidator_ControlToCompare = "CompareValidator_ControlToCompare";
    internal const string CompareValidator_Operator = "CompareValidator_Operator";
    internal const string CompareValidator_Type = "CompareValidator_Type";
    internal const string CompareValidator_ValueToCompare = "CompareValidator_ValueToCompare";
    internal const string Control_AccessKey = "Control_AccessKey";
    internal const string CustomValidator_OnServerValidate = "CustomValidator_OnServerValidate";
    internal const string FontInfo_Bold = "FontInfo_Bold";
    internal const string FontInfo_Italic = "FontInfo_Italic";
    internal const string FontInfo_Name = "FontInfo_Name";
    internal const string FontInfo_Size = "FontInfo_Size";
    internal const string Form_OnActivate = "Form_OnActivate";
    internal const string Form_OnDeactivate = "Form_OnDeactivate";
    internal const string Form_OnPaginated = "Form_OnPaginated";
    internal const string Form_Action = "Form_Action";
    internal const string Form_Method = "Form_Method";
    internal const string Form_PagerStyle = "Form_PagerStyle";
    internal const string Form_Title = "Form_Title";
    internal const string Image_AlternateText = "Image_AlternateText";
    internal const string Image_AutoFormat = "Image_AutoFormat";
    internal const string Image_Coverage = "Image_Coverage";
    internal const string Image_ImageUrl = "Image_ImageUrl";
    internal const string Image_NavigateUrl = "Image_NavigateUrl";
    internal const string Image_SoftkeyLabel = "Image_SoftkeyLabel";
    internal const string Link_NavigateUrl = "Link_NavigateUrl";
    internal const string Link_SoftkeyLabel = "Link_SoftkeyLabel";
    internal const string List_DataMember = "List_DataMember";
    internal const string List_DataSource = "List_DataSource";
    internal const string List_DataTextField = "List_DataTextField";
    internal const string List_DataValueField = "List_DataValueField";
    internal const string List_Decoration = "List_Decoration";
    internal const string List_ItemsAsLinks = "List_ItemsAsLinks";
    internal const string List_Items = "List_Items";
    internal const string List_OnItemCommand = "List_OnItemCommand";
    internal const string List_OnItemDataBind = "List_OnItemDataBind";
    internal const string MobileControl_Alignment = "MobileControl_Alignment";
    internal const string MobileControl_BackColor = "MobileControl_BackColor";
    internal const string MobileControl_BreakAfter = "MobileControl_BreakAfter";
    internal const string MobileControl_Font = "MobileControl_Font";
    internal const string MobileControl_InfiniteTemplateRecursion = "MobileControl_InfiniteTemplateRecursion";
    internal const string MobileControl_StyleReference = "MobileControl_StyleReference";
    internal const string MobileControl_Wrapping = "MobileControl_Wrapping";
    internal const string ObjectList_DataSource = "ObjectList_DataSource";
    internal const string ObjectList_LabelField = "ObjectList_LabelField";
    internal const string ObjectList_TableFields = "ObjectList_TableFields";
    internal const string ObjectList_AutoGenerateFields = "ObjectList_AutoGenerateFields";
    internal const string ObjectList_Fields = "ObjectList_Fields";
    internal const string ObjectList_Commands = "ObjectList_Commands";
    internal const string ObjectList_OnItemCommand = "ObjectList_OnItemCommand";
    internal const string ObjectList_OnItemSelect = "ObjectList_OnItemSelect";
    internal const string ObjectList_OnItemDataBind = "ObjectList_OnItemDataBind";
    internal const string ObjectList_OnShowItemCommands = "ObjectList_OnShowItemCommands";
    internal const string ObjectList_DefaultCommand = "ObjectList_DefaultCommand";
    internal const string ObjectList_DetailsCommandText = "ObjectList_DetailsCommandText";
    internal const string ObjectList_BackCommandText = "ObjectList_BackCommandText";
    internal const string ObjectList_MoreText = "ObjectList_MoreText";
    internal const string ObjectList_CommandStyle = "ObjectList_CommandStyle";
    internal const string ObjectList_LabelStyle = "ObjectList_LabelStyle";
    internal const string PagedControl_ItemCount = "PagedControl_ItemCount";
    internal const string PagedControl_ItemsPerPage = "PagedControl_ItemsPerPage";
    internal const string PagedControl_OnLoadItems = "PagedControl_OnLoadItems";
    internal const string PagerStyle_NextPageText = "PagerStyle_NextPageText";
    internal const string PagerStyle_PageLabel = "PagerStyle_PageLabel";
    internal const string PagerStyle_PreviousPageText = "PagerStyle_PreviousPageText";
    internal const string Panel_Paginate = "Panel_Paginate";
    internal const string RangeValidator_MaximumValue = "RangeValidator_MaximumValue";
    internal const string RangeValidator_MinimumValue = "RangeValidator_MinimumValue";
    internal const string RangeValidator_Type = "RangeValidator_Type";
    internal const string RegularExpressionValidator_ValidationExpression = "RegularExpressionValidator_ValidationExpression";
    internal const string RequiredFieldValidator_InitialValue = "RequiredFieldValidator_InitialValue";
    internal const string SelectionList_OnSelectedIndexChanged = "SelectionList_OnSelectedIndexChanged";
    internal const string SelectionList_Rows = "SelectionList_Rows";
    internal const string SelectionList_SelectType = "SelectionList_SelectType";
    internal const string Input_Title = "Input_Title";
    internal const string Style_Alignment = "Style_Alignment";
    internal const string Style_BackColor = "Style_BackColor";
    internal const string Style_Font = "Style_Font";
    internal const string Style_ForeColor = "Style_ForeColor";
    internal const string Style_Name = "Style_Name";
    internal const string Style_Reference = "Style_Reference";
    internal const string Style_Wrapping = "Style_Wrapping";
    internal const string StyleSheet_ReferencePath = "StyleSheet_ReferencePath";
    internal const string TextBox_Numeric = "TextBox_Numeric";
    internal const string TextBox_MaxLength = "TextBox_MaxLength";
    internal const string TextBox_OnTextChanged = "TextBox_OnTextChanged";
    internal const string TextBox_Password = "TextBox_Password";
    internal const string TextBox_Size = "TextBox_Size";
    internal const string TextControl_Text = "TextControl_Text";
    internal const string TextView_Text = "TextView_Text";
    internal const string ValidationSummary_BackLabel = "ValidationSummary_BackLabel";
    internal const string ValidationSummary_FormToValidate = "ValidationSummary_FormToValidate";
    internal const string ValidationSummary_HeaderText = "ValidationSummary_HeaderText";
    internal const string BaseValidator_ControlToValidateBlank = "BaseValidator_ControlToValidateBlank";
    internal const string BaseValidator_ControlNotFound = "BaseValidator_ControlNotFound";
    internal const string BaseValidator_BadControlType = "BaseValidator_BadControlType";
    internal const string PhoneCall_InvalidPhoneNumberFormat = "PhoneCall_InvalidPhoneNumberFormat";
    internal const string PhoneCall_EmptyPhoneNumber = "PhoneCall_EmptyPhoneNumber";
    internal const string CompareValidator_BadCompareControl = "CompareValidator_BadCompareControl";
    internal const string ControlAdapter_BackLabel = "ControlAdapter_BackLabel";
    internal const string ControlAdapter_GoLabel = "ControlAdapter_GoLabel";
    internal const string ControlAdapter_OKLabel = "ControlAdapter_OKLabel";
    internal const string ControlAdapter_MoreLabel = "ControlAdapter_MoreLabel";
    internal const string ControlAdapter_OptionsLabel = "ControlAdapter_OptionsLabel";
    internal const string ControlAdapter_NextLabel = "ControlAdapter_NextLabel";
    internal const string ControlAdapter_PreviousLabel = "ControlAdapter_PreviousLabel";
    internal const string ControlAdapter_LinkLabel = "ControlAdapter_LinkLabel";
    internal const string ControlAdapter_PhoneCallLabel = "ControlAdapter_PhoneCallLabel";
    internal const string ControlAdapter_InvalidDefaultLabel = "ControlAdapter_InvalidDefaultLabel";
    internal const string ControlsConfig_NoDeviceConfigRegistered = "ControlsConfig_NoDeviceConfigRegistered";
    internal const string DataSourceHelper_MissingDataMember = "DataSourceHelper_MissingDataMember";
    internal const string DataSourceHelper_DataSourceWithoutDataMember = "DataSourceHelper_DataSourceWithoutDataMember";
    internal const string DeviceSpecific_OnlyChoiceElementsAllowed = "DeviceSpecific_OnlyChoiceElementsAllowed";
    internal const string DeviceSpecificChoice_OverridingPropertyNotFound = "DeviceSpecificChoice_OverridingPropertyNotFound";
    internal const string DeviceSpecificChoice_OverridingPropertyTypeCast = "DeviceSpecificChoice_OverridingPropertyTypeCast";
    internal const string DeviceSpecificChoice_OverridingPropertyNotDeclarable = "DeviceSpecificChoice_OverridingPropertyNotDeclarable";
    internal const string DeviceSpecificChoice_InvalidPropertyOverride = "DeviceSpecificChoice_InvalidPropertyOverride";
    internal const string DeviceSpecificChoice_PropertyNotAnAttribute = "DeviceSpecificChoice_PropertyNotAnAttribute";
    internal const string DeviceSpecificChoice_ChoiceOnlyExistInDeviceSpecific = "DeviceSpecificChoice_ChoiceOnlyExistInDeviceSpecific";
    internal const string DeviceSpecificChoice_CantFindFilter = "DeviceSpecificChoice_CantFindFilter";
    internal const string ErrorFormatterPage_ServerError = "ErrorFormatterPage_ServerError";
    internal const string ErrorFormatterPage_MiscErrorMessage = "ErrorFormatterPage_MiscErrorMessage";
    internal const string ErrorFormatterPage_File = "ErrorFormatterPage_File";
    internal const string ErrorFormatterPage_Line = "ErrorFormatterPage_Line";
    internal const string Form_NestedForms = "Form_NestedForms";
    internal const string Form_PropertyNotAccessible = "Form_PropertyNotAccessible";
    internal const string Form_PropertyNotSettable = "Form_PropertyNotSettable";
    internal const string Form_InvalidSubControlType = "Form_InvalidSubControlType";
    internal const string Form_InvalidControlToPaginateForm = "Form_InvalidControlToPaginateForm";
    internal const string IndividualDeviceConfig_TypeMustSupportInterface = "IndividualDeviceConfig_TypeMustSupportInterface";
    internal const string IndividualDeviceConfig_ControlWithIncorrectPageAdapter = "IndividualDeviceConfig_ControlWithIncorrectPageAdapter";
    internal const string LiteralTextParser_InvalidTagFormat = "LiteralTextParser_InvalidTagFormat";
    internal const string MobileControl_MustBeInMobilePage = "MobileControl_MustBeInMobilePage";
    internal const string MobileControl_MustBeInForm = "MobileControl_MustBeInForm";
    internal const string MobileControl_NoTemplatesDefined = "MobileControl_NoTemplatesDefined";
    internal const string MobileControl_NoMultipleDeviceSpecifics = "MobileControl_NoMultipleDeviceSpecifics";
    internal const string MobileControl_NoCustomAttributes = "MobileControl_NoCustomAttributes";
    internal const string MobileControl_InvalidAccessKey = "MobileControl_InvalidAccessKey";
    internal const string MobileControl_InnerTextCannotContainTags = "MobileControl_InnerTextCannotContainTags";
    internal const string MobileControl_TextCannotContainNewlines = "MobileControl_TextCannotContainNewlines";
    internal const string MobileControlBuilder_ControlMustBeTopLevelOfPage = "MobileControlBuilder_ControlMustBeTopLevelOfPage";
    internal const string MobileControlBuilder_StyleMustBeInStyleSheet = "MobileControlBuilder_StyleMustBeInStyleSheet";
    internal const string MobileControlsSectionHandler_UnknownElementName = "MobileControlsSectionHandler_UnknownElementName";
    internal const string MobileControlsSectionHandler_DeviceConfigNotFound = "MobileControlsSectionHandler_DeviceConfigNotFound";
    internal const string MobileControlsSectionHandler_CantCreateMethodOnClass = "MobileControlsSectionHandler_CantCreateMethodOnClass";
    internal const string MobileControlsSectionHandler_TypeNotFound = "MobileControlsSectionHandler_TypeNotFound";
    internal const string MobileControlsSectionHandler_DuplicatedDeviceName = "MobileControlsSectionHandler_DuplicatedDeviceName";
    internal const string MobileControlsSectionHandler_NotAssignable = "MobileControlsSectionHandler_NotAssignable";
    internal const string MobileControlsSectionHandler_CircularReference = "MobileControlsSectionHandler_CircularReference";
    internal const string MobileErrorInfo_Unknown = "MobileErrorInfo_Unknown";
    internal const string MobileErrorInfo_CompilationErrorType = "MobileErrorInfo_CompilationErrorType";
    internal const string MobileErrorInfo_CompilationErrorMiscTitle = "MobileErrorInfo_CompilationErrorMiscTitle";
    internal const string MobileErrorInfo_CompilationErrorDescription = "MobileErrorInfo_CompilationErrorDescription";
    internal const string MobileErrorInfo_ParserErrorType = "MobileErrorInfo_ParserErrorType";
    internal const string MobileErrorInfo_ParserErrorMiscTitle = "MobileErrorInfo_ParserErrorMiscTitle";
    internal const string MobileErrorInfo_ParserErrorDescription = "MobileErrorInfo_ParserErrorDescription";
    internal const string MobileErrorInfo_SourceObject = "MobileErrorInfo_SourceObject";
    internal const string MobileListItemCollection_ViewStateManagementError = "MobileListItemCollection_ViewStateManagementError";
    internal const string MobilePage_InvalidApplicationUrl = "MobilePage_InvalidApplicationUrl";
    internal const string MobilePage_AtLeastOneFormInPage = "MobilePage_AtLeastOneFormInPage";
    internal const string MobilePage_FormNotFound = "MobilePage_FormNotFound";
    internal const string MobilePage_RequiresSessionState = "MobilePage_RequiresSessionState";
    internal const string MobileTypeNameConverter_TypeNotResolved = "MobileTypeNameConverter_TypeNotResolved";
    internal const string MobileTypeNameConverter_UnsupportedValueType = "MobileTypeNameConverter_UnsupportedValueType";
    internal const string ObjectList_FieldNotFound = "ObjectList_FieldNotFound";
    internal const string ObjectList_ItemTitle = "ObjectList_ItemTitle";
    internal const string ObjectList_SelectedIndexTooSmall = "ObjectList_SelectedIndexTooSmall";
    internal const string ObjectList_SelectedIndexTooBig = "ObjectList_SelectedIndexTooBig";
    internal const string ObjectList_CannotSetViewModeWithNoSelectedItem = "ObjectList_CannotSetViewModeWithNoSelectedItem";
    internal const string ObjectList_MustBeInListModeToClearSelectedIndex = "ObjectList_MustBeInListModeToClearSelectedIndex";
    internal const string ObjectList_MustBeInDetailsModeToGetDetails = "ObjectList_MustBeInDetailsModeToGetDetails";
    internal const string ObjectList_MustHaveOneOrMoreFields = "ObjectList_MustHaveOneOrMoreFields";
    internal const string ObjectListField_DataFieldNotFound = "ObjectListField_DataFieldNotFound";
    internal const string ObjectListField_DataBoundText = "ObjectListField_DataBoundText";
    internal const string PagedControl_ItemCountCantBeNegative = "PagedControl_ItemCountCantBeNegative";
    internal const string PagedControl_ItemsPerPageCantBeNegative = "PagedControl_ItemsPerPageCantBeNegative";
    internal const string PagerStyle_NextPageText_DefaultValue = "PagerStyle_NextPageText_DefaultValue";
    internal const string PagerStyle_PreviousPageText_DefaultValue = "PagerStyle_PreviousPageText_DefaultValue";
    internal const string RangeValidator_RangeOverlap = "RangeValidator_RangeOverlap";
    internal const string SessionViewState_InvalidSessionStateHistory = "SessionViewState_InvalidSessionStateHistory";
    internal const string SessionViewState_ExpiredOrCookieless = "SessionViewState_ExpiredOrCookieless";
    internal const string SelectionList_AdapterNotHandlingLoadPostData = "SelectionList_AdapterNotHandlingLoadPostData";
    internal const string Style_DuplicateName = "Style_DuplicateName";
    internal const string Style_NameChangeCauseCircularLoop = "Style_NameChangeCauseCircularLoop";
    internal const string Style_ReferenceCauseCircularLoop = "Style_ReferenceCauseCircularLoop";
    internal const string Style_StyleNotFound = "Style_StyleNotFound";
    internal const string Style_EmptyName = "Style_EmptyName";
    internal const string Style_StyleNotFoundOnGenericUserControl = "Style_StyleNotFoundOnGenericUserControl";
    internal const string Style_CircularReference = "Style_CircularReference";
    internal const string Style_UnregisteredProperty = "Style_UnregisteredProperty";
    internal const string Style_ErrorMessageTitle = "Style_ErrorMessageTitle";
    internal const string StyleSheet_DuplicateName = "StyleSheet_DuplicateName";
    internal const string StyleSheet_MustContainID = "StyleSheet_MustContainID";
    internal const string StyleSheet_NoStyleSheetInExternalFile = "StyleSheet_NoStyleSheetInExternalFile";
    internal const string StyleSheet_PropertyNotAccessible = "StyleSheet_PropertyNotAccessible";
    internal const string StyleSheet_PropertyNotSettable = "StyleSheet_PropertyNotSettable";
    internal const string StyleSheet_InvalidStyleName = "StyleSheet_InvalidStyleName";
    internal const string StyleSheet_StyleAlreadyOwned = "StyleSheet_StyleAlreadyOwned";
    internal const string StyleSheet_LoopReference = "StyleSheet_LoopReference";
    internal const string Theme_Not_Supported_On_MobileControls = "Theme_Not_Supported_On_MobileControls";
    internal const string Feature_Not_Supported_On_MobilePage = "Feature_Not_Supported_On_MobilePage";
    internal const string TextBox_NotNegativeNumber = "TextBox_NotNegativeNumber";
    internal const string SelectionList_OutOfRange = "SelectionList_OutOfRange";
    internal const string ValidationSummary_InvalidFormToValidate = "ValidationSummary_InvalidFormToValidate";
    internal const string Validator_ValueBadType = "Validator_ValueBadType";
    internal const string AppliedDeviceFiltersDialog_Title = "AppliedDeviceFiltersDialog_Title";
    internal const string AppliedDeviceFiltersDialog_AppliedDeviceFilters = "AppliedDeviceFiltersDialog_AppliedDeviceFilters";
    internal const string AppliedDeviceFiltersDialog_AssociatedItemsWillBeLost = "AppliedDeviceFiltersDialog_AssociatedItemsWillBeLost";
    internal const string AppliedDeviceFiltersDialog_ApplyDeviceFilter = "AppliedDeviceFiltersDialog_ApplyDeviceFilter";
    internal const string AppliedDeviceFiltersDialog_AvailableDeviceFilters = "AppliedDeviceFiltersDialog_AvailableDeviceFilters";
    internal const string AppliedDeviceFiltersDialog_Argument = "AppliedDeviceFiltersDialog_Argument";
    internal const string AppliedDeviceFiltersDialog_DuplicateChoices = "AppliedDeviceFiltersDialog_DuplicateChoices";
    internal const string AppliedDeviceFiltersDialog_DefaultFilterAlreadyApplied = "AppliedDeviceFiltersDialog_DefaultFilterAlreadyApplied";
    internal const string BaseTemplatedMobileComponentEditor_TemplateModeErrorMessage = "BaseTemplatedMobileComponentEditor_TemplateModeErrorMessage";
    internal const string BaseTemplatedMobileComponentEditor_TemplateModeErrorTitle = "BaseTemplatedMobileComponentEditor_TemplateModeErrorTitle";
    internal const string Category_DeviceSpecific = "Category_DeviceSpecific";
    internal const string DeviceFilter_DefaultChoice = "DeviceFilter_DefaultChoice";
    internal const string DeviceFilter_NoChoice = "DeviceFilter_NoChoice";
    internal const string DeviceFilterEditorDialog_Header = "DeviceFilterEditorDialog_Header";
    internal const string DeviceFilterEditorDialog_DeviceFilters = "DeviceFilterEditorDialog_DeviceFilters";
    internal const string DeviceFilterEditorDialog_NewDeviceFilter = "DeviceFilterEditorDialog_NewDeviceFilter";
    internal const string DeviceFilterEditorDialog_UnnamedFilter = "DeviceFilterEditorDialog_UnnamedFilter";
    internal const string DeviceFilterEditorDialog_DuplicateName = "DeviceFilterEditorDialog_DuplicateName";
    internal const string DeviceFilterEditorDialog_IllegalDefaultName = "DeviceFilterEditorDialog_IllegalDefaultName";
    internal const string DeviceFilterEditorDialog_Argument = "DeviceFilterEditorDialog_Argument";
    internal const string DeviceFilterEditorDialog_Attributes = "DeviceFilterEditorDialog_Attributes";
    internal const string DeviceFilterEditorDialog_Method = "DeviceFilterEditorDialog_Method";
    internal const string DeviceFilterEditorDialog_TypeGl = "DeviceFilterEditorDialog_TypeGl";
    internal const string DeviceFilterEditorDialog_Equality = "DeviceFilterEditorDialog_Equality";
    internal const string DeviceFilterEditorDialog_Compare = "DeviceFilterEditorDialog_Compare";
    internal const string DeviceFilterEditorDialog_Evaluator = "DeviceFilterEditorDialog_Evaluator";
    internal const string DeviceFilterEditorDialog_TypeTxt = "DeviceFilterEditorDialog_TypeTxt";
    internal const string DeviceFilterEditorDialog_Title = "DeviceFilterEditorDialog_Title";
    internal const string DeviceFilterEditorDialog_InvalidFilter = "DeviceFilterEditorDialog_InvalidFilter";
    internal const string DeviceFilterEditorDialog_DuplicateNames = "DeviceFilterEditorDialog_DuplicateNames";
    internal const string DeviceFilterEditorDialog_WebConfigMissingOnOpen = "DeviceFilterEditorDialog_WebConfigMissingOnOpen";
    internal const string DeviceFilterEditorDialog_WebConfigMissing = "DeviceFilterEditorDialog_WebConfigMissing";
    internal const string DeviceFilterEditorDialog_WebConfigParsingError = "DeviceFilterEditorDialog_WebConfigParsingError";
    internal const string DeviceFilterNode_DefaultFilterName = "DeviceFilterNode_DefaultFilterName";
    internal const string DeviceSpecific_DefaultMessage = "DeviceSpecific_DefaultMessage";
    internal const string DeviceSpecific_TemplateEditingMessage = "DeviceSpecific_TemplateEditingMessage";
    internal const string DeviceSpecific_DuplicateWarningMessage = "DeviceSpecific_DuplicateWarningMessage";
    internal const string DeviceSpecific_PropNotSet = "DeviceSpecific_PropNotSet";
    internal const string EditableTreeList_Rename = "EditableTreeList_Rename";
    internal const string EditableTreeList_AddName = "EditableTreeList_AddName";
    internal const string EditableTreeList_AddDescription = "EditableTreeList_AddDescription";
    internal const string EditableTreeList_MoveUpName = "EditableTreeList_MoveUpName";
    internal const string EditableTreeList_MoveUpDescription = "EditableTreeList_MoveUpDescription";
    internal const string EditableTreeList_MoveDownName = "EditableTreeList_MoveDownName";
    internal const string EditableTreeList_MoveDownDescription = "EditableTreeList_MoveDownDescription";
    internal const string EditableTreeList_DeleteName = "EditableTreeList_DeleteName";
    internal const string EditableTreeList_DeleteDescription = "EditableTreeList_DeleteDescription";
    internal const string PropertyOverridesDialog_AppliedDeviceFilters = "PropertyOverridesDialog_AppliedDeviceFilters";
    internal const string PropertyOverridesDialog_Title = "PropertyOverridesDialog_Title";
    internal const string PropertyOverridesDialog_DeviceSpecificProperties = "PropertyOverridesDialog_DeviceSpecificProperties";
    internal const string PropertyOverridesDialog_InvalidPropertyValue = "PropertyOverridesDialog_InvalidPropertyValue";
    internal const string PropertyOverridesDialog_NotICloneable = "PropertyOverridesDialog_NotICloneable";
    internal const string PropertyOverridesDialog_DuplicateChoices = "PropertyOverridesDialog_DuplicateChoices";
    internal const string GenericDialog_OKBtnCaption = "GenericDialog_OKBtnCaption";
    internal const string GenericDialog_CancelBtnCaption = "GenericDialog_CancelBtnCaption";
    internal const string GenericDialog_HelpBtnCaption = "GenericDialog_HelpBtnCaption";
    internal const string GenericDialog_CloseBtnCaption = "GenericDialog_CloseBtnCaption";
    internal const string GenericDialog_Edit = "GenericDialog_Edit";
    internal const string ImageUrlPicker_ImageCaption = "ImageUrlPicker_ImageCaption";
    internal const string ImageUrlPicker_ImageFilter = "ImageUrlPicker_ImageFilter";
    internal const string ListGeneralPage_Title = "ListGeneralPage_Title";
    internal const string ListGeneralPage_DataGroupLabel = "ListGeneralPage_DataGroupLabel";
    internal const string ListGeneralPage_PagingGroupLabel = "ListGeneralPage_PagingGroupLabel";
    internal const string ListGeneralPage_AppearanceGroupLabel = "ListGeneralPage_AppearanceGroupLabel";
    internal const string ListGeneralPage_DataSourceCaption = "ListGeneralPage_DataSourceCaption";
    internal const string ListGeneralPage_DataMemberCaption = "ListGeneralPage_DataMemberCaption";
    internal const string ListGeneralPage_DataTextFieldCaption = "ListGeneralPage_DataTextFieldCaption";
    internal const string ListGeneralPage_DataValueFieldCaption = "ListGeneralPage_DataValueFieldCaption";
    internal const string ListGeneralPage_ItemCountCaption = "ListGeneralPage_ItemCountCaption";
    internal const string ListGeneralPage_ItemsPerPageCaption = "ListGeneralPage_ItemsPerPageCaption";
    internal const string ListGeneralPage_DecorationCaption = "ListGeneralPage_DecorationCaption";
    internal const string ListGeneralPage_DecorationNone = "ListGeneralPage_DecorationNone";
    internal const string ListGeneralPage_DecorationBulleted = "ListGeneralPage_DecorationBulleted";
    internal const string ListGeneralPage_DecorationNumbered = "ListGeneralPage_DecorationNumbered";
    internal const string ListGeneralPage_SelectTypeCaption = "ListGeneralPage_SelectTypeCaption";
    internal const string ListGeneralPage_SelectTypeDropDown = "ListGeneralPage_SelectTypeDropDown";
    internal const string ListGeneralPage_SelectTypeListBox = "ListGeneralPage_SelectTypeListBox";
    internal const string ListGeneralPage_SelectTypeRadio = "ListGeneralPage_SelectTypeRadio";
    internal const string ListGeneralPage_SelectTypeMultiSelectListBox = "ListGeneralPage_SelectTypeMultiSelectListBox";
    internal const string ListGeneralPage_SelectTypeCheckBox = "ListGeneralPage_SelectTypeCheckBox";
    internal const string ListGeneralPage_RowsCaption = "ListGeneralPage_RowsCaption";
    internal const string ListGeneralPage_NoneComboEntry = "ListGeneralPage_NoneComboEntry";
    internal const string ListGeneralPage_UnboundComboEntry = "ListGeneralPage_UnboundComboEntry";
    internal const string ListGeneralPage_PrivateMemberMessage = "ListGeneralPage_PrivateMemberMessage";
    internal const string ListGeneralPage_PrivateMemberCaption = "ListGeneralPage_PrivateMemberCaption";
    internal const string ListItemsPage_Title = "ListItemsPage_Title";
    internal const string ListItemsPage_ItemsAsLinksCaption = "ListItemsPage_ItemsAsLinksCaption";
    internal const string ListItemsPage_ItemListGroupLabel = "ListItemsPage_ItemListGroupLabel";
    internal const string ListItemsPage_DefaultItemText = "ListItemsPage_DefaultItemText";
    internal const string ListItemsPage_ItemCaption = "ListItemsPage_ItemCaption";
    internal const string ListItemsPage_NewItemCaption = "ListItemsPage_NewItemCaption";
    internal const string ListItemsPage_ItemValueCaption = "ListItemsPage_ItemValueCaption";
    internal const string ListItemsPage_ItemSelectedCaption = "ListItemsPage_ItemSelectedCaption";
    internal const string MarkupSchema_HTML32 = "MarkupSchema_HTML32";
    internal const string MarkupSchema_cHTML10 = "MarkupSchema_cHTML10";
    internal const string MobileControl_FormPanelContainmentErrorMessage = "MobileControl_FormPanelContainmentErrorMessage";
    internal const string MobileControl_StrictlyFormPanelContainmentErrorMessage = "MobileControl_StrictlyFormPanelContainmentErrorMessage";
    internal const string MobileControl_TopPageContainmentErrorMessage = "MobileControl_TopPageContainmentErrorMessage";
    internal const string MobileControl_MobilePageErrorMessage = "MobileControl_MobilePageErrorMessage";
    internal const string MobileControl_NonHtmlSchemaErrorMessage = "MobileControl_NonHtmlSchemaErrorMessage";
    internal const string MobileControl_DefaultErrorMessage = "MobileControl_DefaultErrorMessage";
    internal const string MobileControl_SettingGenericChoiceDescription = "MobileControl_SettingGenericChoiceDescription";
    internal const string MobileControl_DeviceSpecificPropsDescription = "MobileControl_DeviceSpecificPropsDescription";
    internal const string MobileControl_AppliedDeviceFiltersDescription = "MobileControl_AppliedDeviceFiltersDescription";
    internal const string MobileControl_InnerTextCannotContainTagsDesigner = "MobileControl_InnerTextCannotContainTagsDesigner";
    internal const string MobileControl_UserControlWarningMessage = "MobileControl_UserControlWarningMessage";
    internal const string MobileWebFormDesigner_NonMobileControlOnPageWarning = "MobileWebFormDesigner_NonMobileControlOnPageWarning";
    internal const string MobileWebFormDesigner_NonMobileControlCreatedWarning = "MobileWebFormDesigner_NonMobileControlCreatedWarning";
    internal const string MobileWebFormDesigner_MessageBoxTitle = "MobileWebFormDesigner_MessageBoxTitle";
    internal const string NavigateUrlConverter_SelectURITarget = "NavigateUrlConverter_SelectURITarget";
    internal const string ObjectListCommandsPage_Title = "ObjectListCommandsPage_Title";
    internal const string ObjectListCommandsPage_DefaultCommandCaption = "ObjectListCommandsPage_DefaultCommandCaption";
    internal const string ObjectListCommandsPage_CommandListGroupLabel = "ObjectListCommandsPage_CommandListGroupLabel";
    internal const string ObjectListCommandsPage_DataGroupLabel = "ObjectListCommandsPage_DataGroupLabel";
    internal const string ObjectListCommandsPage_DefaultCommandName = "ObjectListCommandsPage_DefaultCommandName";
    internal const string ObjectListCommandsPage_CommandNameCaption = "ObjectListCommandsPage_CommandNameCaption";
    internal const string ObjectListCommandsPage_NewCommandBtnCaption = "ObjectListCommandsPage_NewCommandBtnCaption";
    internal const string ObjectListCommandsPage_PropertiesGroupLabel = "ObjectListCommandsPage_PropertiesGroupLabel";
    internal const string ObjectListCommandsPage_TextCaption = "ObjectListCommandsPage_TextCaption";
    internal const string ObjectListCommandsPage_EmptyNameError = "ObjectListCommandsPage_EmptyNameError";
    internal const string ObjectListCommandsPage_ErrorMessageTitle = "ObjectListCommandsPage_ErrorMessageTitle";
    internal const string ObjectListFieldsPage_Title = "ObjectListFieldsPage_Title";
    internal const string ObjectListFieldsPage_AutoGenerateFieldsCaption = "ObjectListFieldsPage_AutoGenerateFieldsCaption";
    internal const string ObjectListFieldsPage_FieldListGroupLabel = "ObjectListFieldsPage_FieldListGroupLabel";
    internal const string ObjectListFieldsPage_DefaultFieldName = "ObjectListFieldsPage_DefaultFieldName";
    internal const string ObjectListFieldsPage_FieldNameCaption = "ObjectListFieldsPage_FieldNameCaption";
    internal const string ObjectListFieldsPage_NewFieldBtnCaption = "ObjectListFieldsPage_NewFieldBtnCaption";
    internal const string ObjectListFieldsPage_PropertiesGroupLabel = "ObjectListFieldsPage_PropertiesGroupLabel";
    internal const string ObjectListFieldsPage_DataFieldCaption = "ObjectListFieldsPage_DataFieldCaption";
    internal const string ObjectListFieldsPage_NoneComboEntry = "ObjectListFieldsPage_NoneComboEntry";
    internal const string ObjectListFieldsPage_DataFormatStringCaption = "ObjectListFieldsPage_DataFormatStringCaption";
    internal const string ObjectListFieldsPage_TitleCaption = "ObjectListFieldsPage_TitleCaption";
    internal const string ObjectListFieldsPage_VisibleCaption = "ObjectListFieldsPage_VisibleCaption";
    internal const string ObjectListFieldsPage_EmptyNameError = "ObjectListFieldsPage_EmptyNameError";
    internal const string ObjectListFieldsPage_ErrorMessageTitle = "ObjectListFieldsPage_ErrorMessageTitle";
    internal const string ObjectListGeneralPage_Title = "ObjectListGeneralPage_Title";
    internal const string ObjectListGeneralPage_AppearanceGroupLabel = "ObjectListGeneralPage_AppearanceGroupLabel";
    internal const string ObjectListGeneralPage_DataGroupLabel = "ObjectListGeneralPage_DataGroupLabel";
    internal const string ObjectListGeneralPage_PagingGroupLabel = "ObjectListGeneralPage_PagingGroupLabel";
    internal const string ObjectListGeneralPage_NoneComboEntry = "ObjectListGeneralPage_NoneComboEntry";
    internal const string ObjectListGeneralPage_UnboundComboEntry = "ObjectListGeneralPage_UnboundComboEntry";
    internal const string ObjectListGeneralPage_BackCommandTextCaption = "ObjectListGeneralPage_BackCommandTextCaption";
    internal const string ObjectListGeneralPage_DetailsCommandTextCaption = "ObjectListGeneralPage_DetailsCommandTextCaption";
    internal const string ObjectListGeneralPage_MoreTextCaption = "ObjectListGeneralPage_MoreTextCaption";
    internal const string ObjectListGeneralPage_DataSourceCaption = "ObjectListGeneralPage_DataSourceCaption";
    internal const string ObjectListGeneralPage_DataMemberCaption = "ObjectListGeneralPage_DataMemberCaption";
    internal const string ObjectListGeneralPage_LabelFieldCaption = "ObjectListGeneralPage_LabelFieldCaption";
    internal const string ObjectListGeneralPage_PrivateDataSourceMessage = "ObjectListGeneralPage_PrivateDataSourceMessage";
    internal const string ObjectListGeneralPage_TableFieldsGroupLabel = "ObjectListGeneralPage_TableFieldsGroupLabel";
    internal const string ObjectListGeneralPage_TableFieldsAvailableListLabel = "ObjectListGeneralPage_TableFieldsAvailableListLabel";
    internal const string ObjectListGeneralPage_TableFieldsSelectedListLabel = "ObjectListGeneralPage_TableFieldsSelectedListLabel";
    internal const string PropertyBuilderVerb = "PropertyBuilderVerb";
    internal const string Security_ReturnUrlCannotBeAbsolute = "Security_ReturnUrlCannotBeAbsolute";
    internal const string StylesEditorDialog_DuplicateStyleException = "StylesEditorDialog_DuplicateStyleException";
    internal const string StylesEditorDialog_DuplicateStyleNames = "StylesEditorDialog_DuplicateStyleNames";
    internal const string StylesEditorDialog_Title = "StylesEditorDialog_Title";
    internal const string StylesEditorDialog_PreviewText = "StylesEditorDialog_PreviewText";
    internal const string StylesEditorDialog_StyleListGroupLabel = "StylesEditorDialog_StyleListGroupLabel";
    internal const string StylesEditorDialog_AvailableStylesCaption = "StylesEditorDialog_AvailableStylesCaption";
    internal const string StylesEditorDialog_AddBtnCation = "StylesEditorDialog_AddBtnCation";
    internal const string StylesEditorDialog_DefinedStylesCaption = "StylesEditorDialog_DefinedStylesCaption";
    internal const string StylesEditorDialog_StylePropertiesGroupLabel = "StylesEditorDialog_StylePropertiesGroupLabel";
    internal const string StylesEditorDialog_TypeCaption = "StylesEditorDialog_TypeCaption";
    internal const string StylesEditorDialog_SampleCaption = "StylesEditorDialog_SampleCaption";
    internal const string StylesEditorDialog_PropertiesCaption = "StylesEditorDialog_PropertiesCaption";
    internal const string StylesEditorDialog_DeleteStyleMessage = "StylesEditorDialog_DeleteStyleMessage";
    internal const string StylesEditorDialog_DeleteStyleCaption = "StylesEditorDialog_DeleteStyleCaption";
    internal const string StylesEditorDialog_EmptyName = "StylesEditorDialog_EmptyName";
    internal const string StyleSheet_DefaultMessage = "StyleSheet_DefaultMessage";
    internal const string StyleSheet_TemplateEditingMessage = "StyleSheet_TemplateEditingMessage";
    internal const string StyleSheet_DuplicateStyleNamesMessage = "StyleSheet_DuplicateStyleNamesMessage";
    internal const string StyleSheet_DuplicateWarningMessage = "StyleSheet_DuplicateWarningMessage";
    internal const string StyleSheet_RefCycleErrorMessage = "StyleSheet_RefCycleErrorMessage";
    internal const string StyleSheet_PropNotSet = "StyleSheet_PropNotSet";
    internal const string StyleSheet_StylesCaption = "StyleSheet_StylesCaption";
    internal const string StyleSheet_TemplateStyleDescription = "StyleSheet_TemplateStyleDescription";
    internal const string StyleSheet_StylesEditorVerb = "StyleSheet_StylesEditorVerb";
    internal const string StyleSheet_SettingTemplatingStyleChoiceDescription = "StyleSheet_SettingTemplatingStyleChoiceDescription";
    internal const string StyleSheet_SettingGenericStyleChoiceDescription = "StyleSheet_SettingGenericStyleChoiceDescription";
    internal const string Stylesheet_EditBtnCaption = "Stylesheet_EditBtnCaption";
    internal const string StyleSheetRefUrlEditor_Filter = "StyleSheetRefUrlEditor_Filter";
    internal const string StyleSheetRefUrlEditor_Caption = "StyleSheetRefUrlEditor_Caption";
    internal const string TemplateableDesigner_SetTemplatesFilterVerb = "TemplateableDesigner_SetTemplatesFilterVerb";
    internal const string TemplateableDesigner_SettingTemplatingChoiceDescription = "TemplateableDesigner_SettingTemplatingChoiceDescription";
    internal const string TemplateableDesigner_SettingGenericChoiceDescription = "TemplateableDesigner_SettingGenericChoiceDescription";
    internal const string TemplateableDesigner_TemplateChoiceDescription = "TemplateableDesigner_TemplateChoiceDescription";
    internal const string TemplatingOptionsDialog_Title = "TemplatingOptionsDialog_Title";
    internal const string TemplatingOptionsDialog_FilterCaption = "TemplatingOptionsDialog_FilterCaption";
    internal const string TemplatingOptionsDialog_EditBtnCaption = "TemplatingOptionsDialog_EditBtnCaption";
    internal const string TemplatingOptionsDialog_SchemaCaption = "TemplatingOptionsDialog_SchemaCaption";
    internal const string TemplatingOptionsDialog_HTMLSchemaFriendly = "TemplatingOptionsDialog_HTMLSchemaFriendly";
    internal const string TemplatingOptionsDialog_CHTMLSchemaFriendly = "TemplatingOptionsDialog_CHTMLSchemaFriendly";
    internal const string Toolbox_TabName = "Toolbox_TabName";
    internal const string UrlPicker_DefaultFilter = "UrlPicker_DefaultFilter";
    internal const string UrlPicker_DefaultCaption = "UrlPicker_DefaultCaption";
    internal const string TemplateFrame_HeaderFooterTemplates = "TemplateFrame_HeaderFooterTemplates";
    internal const string TemplateFrame_ItemTemplates = "TemplateFrame_ItemTemplates";
    internal const string TemplateFrame_ContentTemplate = "TemplateFrame_ContentTemplate";
    internal const string TemplateFrame_SeparatorTemplate = "TemplateFrame_SeparatorTemplate";
    internal const string TemplateFrame_IllFormedWarning = "TemplateFrame_IllFormedWarning";
    internal const string UrlPath_EmptyPathHasNoDirectory = "UrlPath_EmptyPathHasNoDirectory";
    internal const string UrlPath_PathMustBeRooted = "UrlPath_PathMustBeRooted";
    internal const string UrlPath_PhysicalPathNotAllowed = "UrlPath_PhysicalPathNotAllowed";
    internal const string UrlPath_CannotExitUpTopDirectory = "UrlPath_CannotExitUpTopDirectory";
    internal const string UnsettableComboBox_NotSetText = "UnsettableComboBox_NotSetText";
    internal const string UnsettableComboBox_NotSetCompactText = "UnsettableComboBox_NotSetCompactText";
    internal const string ValidationSummary_ErrorMessage = "ValidationSummary_ErrorMessage";
    internal const string WebConfig_FileNotFoundException = "WebConfig_FileNotFoundException";
    internal const string WebConfig_FileLoadException = "WebConfig_FileLoadException";
    private static SR loader;
    private ResourceManager resources;

    internal SR()
    {
      this.resources = new ResourceManager("System.Web.Mobile", this.GetType().Assembly);
    }

    private static SR GetLoader()
    {
      if (SR.loader == null)
      {
        SR sr = new SR();
        Interlocked.CompareExchange<SR>(ref SR.loader, sr, (SR) null);
      }
      return SR.loader;
    }

    private static CultureInfo Culture
    {
      get
      {
        return (CultureInfo) null;
      }
    }

    public static ResourceManager Resources
    {
      get
      {
        return SR.GetLoader().resources;
      }
    }

    public static string GetString(string name, params object[] args)
    {
      SR loader = SR.GetLoader();
      if (loader == null)
        return (string) null;
      string format = loader.resources.GetString(name, SR.Culture);
      if (args == null || args.Length == 0)
        return format;
      for (int index = 0; index < args.Length; ++index)
      {
        string str = args[index] as string;
        if (str != null && str.Length > 1024)
          args[index] = (object) (str.Substring(0, 1021) + "...");
      }
      return string.Format((IFormatProvider) CultureInfo.CurrentCulture, format, args);
    }

    public static string GetString(string name)
    {
      SR loader = SR.GetLoader();
      if (loader == null)
        return (string) null;
      return loader.resources.GetString(name, SR.Culture);
    }

    public static string GetString(string name, out bool usedFallback)
    {
      usedFallback = false;
      return SR.GetString(name);
    }

    public static object GetObject(string name)
    {
      SR loader = SR.GetLoader();
      if (loader == null)
        return (object) null;
      return loader.resources.GetObject(name, SR.Culture);
    }
  }
}
