namespace System.Xml.Utils {
	static partial class Res {












public const string Xml_UserException = @"{0}";
public const string Xml_ErrorFilePosition = @"An error occurred at {0}({1},{2}).";
public const string Xml_InvalidOperation = @"Operation is not valid due to the current state of the object.";


public const string Xml_EndOfInnerExceptionStack = @"--- End of inner exception stack trace ---";


public const string XPath_UnclosedString = @"String literal was not closed.";
public const string XPath_ScientificNotation = @"Scientific notation is not allowed.";
public const string XPath_UnexpectedToken = @"Unexpected token '{0}' in the expression.";
public const string XPath_NodeTestExpected = @"Expected a node test, found '{0}'.";
public const string XPath_EofExpected = @"Expected end of the expression, found '{0}'.";
public const string XPath_TokenExpected = @"Expected token '{0}', found '{1}'.";
public const string XPath_InvalidAxisInPattern = @"Only 'child' and 'attribute' axes are allowed in a pattern outside predicates.";
public const string XPath_PredicateAfterDot = @"Abbreviated step '.' cannot be followed by a predicate. Use the full form 'self::node()[predicate]' instead.";
public const string XPath_PredicateAfterDotDot = @"Abbreviated step '..' cannot be followed by a predicate. Use the full form 'parent::node()[predicate]' instead.";
public const string XPath_NArgsExpected = @"Function '{0}()' must have {1} argument(s).";
public const string XPath_NOrMArgsExpected = @"Function '{0}()' must have {1} or {2} argument(s).";
public const string XPath_AtLeastNArgsExpected = @"Function '{0}()' must have at least {1} argument(s).";

public const string XPath_AtMostMArgsExpected = @"Function '{0}()' must have no more than {2} arguments.";
public const string XPath_NodeSetArgumentExpected = @"Argument {1} of function '{0}()' cannot be converted to a node-set.";
public const string XPath_NodeSetExpected = @"Expression must evaluate to a node-set.";
public const string XPath_RtfInPathExpr = @"To use a result tree fragment in a path expression, first convert it to a node-set using the msxsl:node-set() function.";



public const string Xslt_WarningAsError = @"Warning as Error: {0}";
public const string Xslt_InputTooComplex = @"The stylesheet is too complex.";
public const string Xslt_CannotLoadStylesheet = @"Cannot load the stylesheet object referenced by URI '{0}', because the provided XmlResolver returned an object of type '{1}'. One of Stream, XmlReader, and IXPathNavigable types was expected.";
public const string Xslt_WrongStylesheetElement = @"Stylesheet must start either with an 'xsl:stylesheet' or an 'xsl:transform' element, or with a literal result element that has an 'xsl:version' attribute, where prefix 'xsl' denotes the 'http://www.w3.org/1999/XSL/Transform' namespace.";
public const string Xslt_WdXslNamespace = @"The 'http://www.w3.org/TR/WD-xsl' namespace is no longer supported.";
public const string Xslt_NotAtTop = @"'{0}' element children must precede all other children of the '{1}' element.";
public const string Xslt_UnexpectedElement = @"'{0}' cannot be a child of the '{1}' element.";
public const string Xslt_NullNsAtTopLevel = @"Top-level element '{0}' may not have a null namespace URI.";
public const string Xslt_TextNodesNotAllowed = @"'{0}' element cannot have text node children.";
public const string Xslt_NotEmptyContents = @"The contents of '{0}' must be empty.";
public const string Xslt_InvalidAttribute = @"'{0}' is an invalid attribute for the '{1}' element.";
public const string Xslt_MissingAttribute = @"Missing mandatory attribute '{0}'.";
public const string Xslt_InvalidAttrValue = @"'{1}' is an invalid value for the '{0}' attribute.";
public const string Xslt_BistateAttribute = @"The value of the '{0}' attribute must be '{1}' or '{2}'.";
public const string Xslt_CharAttribute = @"The value of the '{0}' attribute must be a single character.";
public const string Xslt_CircularInclude = @"Stylesheet '{0}' cannot directly or indirectly include or import itself.";
public const string Xslt_SingleRightBraceInAvt = @"The right curly brace in an attribute value template '{0}' outside an expression must be doubled.";
public const string Xslt_VariableCntSel2 = @"The variable or parameter '{0}' cannot have both a 'select' attribute and non-empty content.";
public const string Xslt_KeyCntUse = @"'xsl:key' has a 'use' attribute and has non-empty content, or it has empty content and no 'use' attribute.";
public const string Xslt_DupTemplateName = @"'{0}' is a duplicate template name.";
public const string Xslt_BothMatchNameAbsent = @"'xsl:template' must have either a 'match' attribute or a 'name' attribute, or both.";
public const string Xslt_InvalidVariable = @"The variable or parameter '{0}' is either not defined or it is out of scope.";
public const string Xslt_DupGlobalVariable = @"The variable or parameter '{0}' was duplicated with the same import precedence.";
public const string Xslt_DupLocalVariable = @"The variable or parameter '{0}' was duplicated within the same scope.";
public const string Xslt_DupNsAlias = @"Namespace URI '{0}' is declared to be an alias for multiple different namespace URIs with the same import precedence.";
public const string Xslt_EmptyAttrValue = @"The value of the '{0}' attribute cannot be empty.";
public const string Xslt_EmptyNsAlias = @"The value of the '{0}' attribute cannot be empty. Use '#default' to specify the default namespace.";
public const string Xslt_UnknownXsltFunction = @"'{0}()' is an unknown XSLT function.";
public const string Xslt_UnsupportedXsltFunction = @"'{0}()' is an unsupported XSLT function.";
public const string Xslt_NoAttributeSet = @"A reference to attribute set '{0}' cannot be resolved. An 'xsl:attribute-set' of this name must be declared at the top level of the stylesheet.";
public const string Xslt_UndefinedKey = @"A reference to key '{0}' cannot be resolved. An 'xsl:key' of this name must be declared at the top level of the stylesheet.";
public const string Xslt_CircularAttributeSet = @"Circular reference in the definition of attribute set '{0}'.";
public const string Xslt_InvalidCallTemplate = @"The named template '{0}' does not exist.";
public const string Xslt_InvalidPrefix = @"Prefix '{0}' is not defined.";
public const string Xslt_ScriptXsltNamespace = @"Script block cannot implement the XSLT namespace.";
public const string Xslt_ScriptInvalidLanguage = @"Scripting language '{0}' is not supported.";
public const string Xslt_ScriptMixedLanguages = @"All script blocks implementing the namespace '{0}' must use the same language.";

public const string Xslt_ScriptCompileException = @"Error occurred while compiling the script: {0}";
public const string Xslt_ScriptNotAtTop = @"Element '{0}' must precede script code.";
public const string Xslt_AssemblyNameHref = @"'msxsl:assembly' must have either a 'name' attribute or an 'href' attribute, but not both.";
public const string Xslt_ScriptAndExtensionClash = @"Cannot have both an extension object and a script implementing the same namespace '{0}'.";
public const string Xslt_NoDecimalFormat = @"Decimal format '{0}' is not defined.";
public const string Xslt_DecimalFormatSignsNotDistinct = @"The '{0}' and '{1}' attributes of 'xsl:decimal-format' must have distinct values.";
public const string Xslt_DecimalFormatRedefined = @"The '{0}' attribute of 'xsl:decimal-format' cannot be redefined with a value of '{1}'.";
public const string Xslt_UnknownExtensionElement = @"'{0}' is not a recognized extension element.";
public const string Xslt_ModeWithoutMatch = @"An 'xsl:template' element without a 'match' attribute cannot have a 'mode' attribute.";
public const string Xslt_ModeListEmpty = @"List of modes in 'xsl:template' element can't be empty. ";
public const string Xslt_ModeListDup = @"List of modes in 'xsl:template' element can't contain duplicates ('{0}'). ";
public const string Xslt_ModeListAll = @"List of modes in 'xsl:template' element can't contain token '#all' together with any other value. ";
public const string Xslt_PriorityWithoutMatch = @"An 'xsl:template' element without a 'match' attribute cannot have a 'priority' attribute.";
public const string Xslt_InvalidApplyImports = @"An 'xsl:apply-imports' element can only occur within an 'xsl:template' element with a 'match' attribute, and cannot occur within an 'xsl:for-each' element.";
public const string Xslt_DuplicateWithParam = @"Value of parameter '{0}' cannot be specified more than once within a single 'xsl:call-template' or 'xsl:apply-templates' element.";
public const string Xslt_ReservedNS = @"Elements and attributes cannot belong to the reserved namespace '{0}'.";
public const string Xslt_XmlnsAttr = @"An attribute with a local name 'xmlns' and a null namespace URI cannot be created.";
public const string Xslt_NoWhen = @"An 'xsl:choose' element must have at least one 'xsl:when' child.";
public const string Xslt_WhenAfterOtherwise = @"'xsl:when' must precede the 'xsl:otherwise' element.";
public const string Xslt_DupOtherwise = @"An 'xsl:choose' element can have only one 'xsl:otherwise' child.";
public const string Xslt_AttributeRedefinition = @"Attribute '{0}' of 'xsl:output' cannot be defined more than once with the same import precedence.";
public const string Xslt_InvalidMethod = @"'{0}' is not a supported output method. Supported methods are 'xml', 'html', and 'text'.";
public const string Xslt_InvalidEncoding = @"'{0}' is not a supported encoding name.";
public const string Xslt_InvalidLanguage = @"'{0}' is not a supported language identifier.";
public const string Xslt_InvalidCompareOption = @"String comparison option(s) '{0}' are either invalid or cannot be used together.";
public const string Xslt_KeyNotAllowed = @"The 'key()' function cannot be used in 'use' and 'match' attributes of 'xsl:key' element.";
public const string Xslt_VariablesNotAllowed = @"Variables cannot be used within this expression.";
public const string Xslt_CurrentNotAllowed = @"The 'current()' function cannot be used in a pattern.";
public const string Xslt_DocumentFuncProhibited = @"Execution of the 'document()' function was prohibited. Use the XsltSettings.EnableDocumentFunction property to enable it.";
public const string Xslt_ScriptsProhibited = @"Execution of scripts was prohibited. Use the XsltSettings.EnableScript property to enable it.";
public const string Xslt_ItemNull = @"Extension functions cannot return null values.";
public const string Xslt_NodeSetNotNode = @"Cannot convert a node-set which contains zero nodes or more than one node to a single node.";
public const string Xslt_UnsupportedClrType = @"Extension function parameters or return values which have Clr type '{0}' are not supported.";
public const string Xslt_NotYetImplemented = @"'{0}' is not yet implemented.";
public const string Xslt_SchemaDeclaration = @"'{0}' declaration is not permitted in non-schema aware processor.";
public const string Xslt_SchemaAttribute = @"Attribute '{0}' is not permitted in basic XSLT processor (http://www.w3.org/TR/xslt20/#dt-basic-xslt-processor).";
public const string Xslt_SchemaAttributeValue = @"Value '{1}' of attribute '{0}' is not permitted in basic XSLT processor (http://www.w3.org/TR/xslt20/#dt-basic-xslt-processor).";
public const string Xslt_ElementCntSel = @"The element '{0}' cannot have both a 'select' attribute and non-empty content.";
public const string Xslt_PerformSortCntSel = @"The element 'xsl:perform-sort' cannot have 'select' attribute any content other than 'xsl:sort' and 'xsl:fallback' instructions.";
public const string Xslt_RequiredAndSelect = @"Mandatory parameter '{0}' must be empty and must not have a 'select' attribute.";
public const string Xslt_NoSelectNoContent = @"Element '{0}' must have either 'select' attribute or non-empty content.";
public const string Xslt_NonTemplateTunnel = @"Stylesheet or function parameter '{0}' cannot have attribute 'tunnel'.";
public const string Xslt_RequiredOnFunction = @"The 'required' attribute must not be specified for parameter '{0}'. Function parameters are always mandatory. ";
public const string Xslt_ExcludeDefault = @"Value '#default' is used within the 'exclude-result-prefixes' attribute and the parent element of this attribute has no default namespace.";
public const string Xslt_CollationSyntax = @"The value of an 'default-collation' attribute contains no recognized collation URI.";
public const string Xslt_AnalyzeStringDupChild = @"'xsl:analyze-string' cannot have second child with name '{0}'.";
public const string Xslt_AnalyzeStringChildOrder = @"When both 'xsl:matching-string' and 'xsl:non-matching-string' elements are present, 'xsl:matching-string' element must come first.";
public const string Xslt_AnalyzeStringEmpty = @"'xsl:analyze-string' must contain either 'xsl:matching-string' or 'xsl:non-matching-string' elements or both.";
public const string Xslt_SortStable = @"Only the first 'xsl:sort' element may have 'stable' attribute.";
public const string Xslt_InputTypeAnnotations = @"It is an error if there is a stylesheet module in the stylesheet that specifies 'input-type-annotations'=""strip"" and another stylesheet module that specifies 'input-type-annotations'=""preserve"".";


public const string Coll_BadOptFormat = @"Collation option '{0}' is invalid. Options must have the following format: <option-name>=<option-value>.";
public const string Coll_Unsupported = @"The collation '{0}' is not supported.";
public const string Coll_UnsupportedLanguage = @"Collation language '{0}' is not supported.";
public const string Coll_UnsupportedOpt = @"Unsupported option '{0}' in collation.";
public const string Coll_UnsupportedOptVal = @"Collation option '{0}' cannot have the value '{1}'.";
public const string Coll_UnsupportedSortOpt = @"Unsupported sort option '{0}' in collation.";


public const string Qil_Validation = @"QIL Validation Error! '{0}'.";


public const string XmlIl_TooManyParameters = @"Functions may not have more than 65535 parameters.";
public const string XmlIl_BadXmlState = @"An item of type '{0}' cannot be constructed within a node of type '{1}'.";
public const string XmlIl_BadXmlStateAttr = @"Attribute and namespace nodes cannot be added to the parent element after a text, comment, pi, or sub-element node has already been added.";
public const string XmlIl_NmspAfterAttr = @"Namespace nodes cannot be added to the parent element after an attribute node has already been added.";
public const string XmlIl_NmspConflict = @"Cannot construct namespace declaration xmlns{0}{1}='{2}'. Prefix '{1}' is already mapped to namespace '{3}'.";
public const string XmlIl_CantResolveEntity = @"Cannot query the data source object referenced by URI '{0}', because the provided XmlResolver returned an object of type '{1}'. Only Stream, XmlReader, and IXPathNavigable data source objects are currently supported.";
public const string XmlIl_NoDefaultDocument = @"Query requires a default data source, but no default was supplied to the query engine.";
public const string XmlIl_UnknownDocument = @"Data source '{0}' cannot be located.";
public const string XmlIl_UnknownParam = @"Supplied XsltArgumentList does not contain a parameter with local name '{0}' and namespace '{1}'.";
public const string XmlIl_UnknownExtObj = @"Cannot find a script or an extension object associated with namespace '{0}'.";
public const string XmlIl_CantStripNav = @"White space cannot be stripped from input documents that have already been loaded. Provide the input document as an XmlReader instead.";
public const string XmlIl_ExtensionError = @"An error occurred during a call to extension function '{0}'. See InnerException for a complete description of the error.";
public const string XmlIl_TopLevelAttrNmsp = @"XmlWriter cannot process the sequence returned by the query, because it contains an attribute or namespace node.";
public const string XmlIl_NoExtensionMethod = @"Extension object '{0}' does not contain a matching '{1}' method that has {2} parameter(s).";
public const string XmlIl_AmbiguousExtensionMethod = @"Ambiguous method call. Extension object '{0}' contains multiple '{1}' methods that have {2} parameter(s).";
public const string XmlIl_NonPublicExtensionMethod = @"Method '{1}' of extension object '{0}' cannot be called because it is not public.";
public const string XmlIl_GenericExtensionMethod = @"Method '{1}' of extension object '{0}' cannot be called because it is generic.";
public const string XmlIl_ByRefType = @"Method '{1}' of extension object '{0}' cannot be called because it has one or more ByRef parameters.";
public const string XmlIl_DocumentLoadError = @"An error occurred while loading document '{0}'. See InnerException for a complete description of the error.";


public const string Xslt_CompileError = @"XSLT compile error at {0}({1},{2}). See InnerException for details.";
public const string Xslt_CompileError2 = @"XSLT compile error.";
public const string Xslt_UnsuppFunction = @"'{0}()' is an unsupported XSLT function.";
public const string Xslt_NotFirstImport = @"'xsl:import' instructions must precede all other element children of an 'xsl:stylesheet' element.";
public const string Xslt_UnexpectedKeyword = @"'{0}' cannot be a child of the '{1}' element.";
public const string Xslt_InvalidContents = @"The contents of '{0}' are invalid.";
public const string Xslt_CantResolve = @"Cannot resolve the referenced document '{0}'.";
public const string Xslt_SingleRightAvt = @"Right curly brace in the attribute value template '{0}' must be doubled.";
public const string Xslt_OpenBracesAvt = @"The braces are not closed in AVT expression '{0}'.";
public const string Xslt_OpenLiteralAvt = @"The literal in AVT expression is not correctly closed '{0}'.";
public const string Xslt_NestedAvt = @"AVT cannot be nested in AVT '{0}'.";
public const string Xslt_EmptyAvtExpr = @"XPath Expression in AVT cannot be empty: '{0}'.";
public const string Xslt_InvalidXPath = @"'{0}' is an invalid XPath expression.";
public const string Xslt_InvalidQName = @"'{0}' is an invalid QName.";
public const string Xslt_NoStylesheetLoaded = @"No stylesheet was loaded.";
public const string Xslt_TemplateNoAttrib = @"The 'xsl:template' instruction must have the 'match' and/or 'name' attribute present.";
public const string Xslt_DupVarName = @"Variable or parameter '{0}' was duplicated within the same scope.";
public const string Xslt_WrongNumberArgs = @"XSLT function '{0}()' has the wrong number of arguments.";
public const string Xslt_NoNodeSetConversion = @"Cannot convert the operand to a node-set.";
public const string Xslt_NoNavigatorConversion = @"Cannot convert the operand to 'Result tree fragment'.";
public const string Xslt_FunctionFailed = @"Function '{0}()' has failed.";
public const string Xslt_InvalidFormat = @"Format cannot be empty.";
public const string Xslt_InvalidFormat1 = @"Format '{0}' cannot have digit symbol after zero digit symbol before a decimal point.";
public const string Xslt_InvalidFormat2 = @"Format '{0}' cannot have zero digit symbol after digit symbol after decimal point.";
public const string Xslt_InvalidFormat3 = @"Format '{0}' has two pattern separators.";
public const string Xslt_InvalidFormat4 = @"Format '{0}' cannot end with a pattern separator.";
public const string Xslt_InvalidFormat5 = @"Format '{0}' cannot have two decimal separators.";
public const string Xslt_InvalidFormat8 = @"Format string should have at least one digit or zero digit.";
public const string Xslt_ScriptCompileErrors = @"Script compile errors:\n{0}";
public const string Xslt_ScriptInvalidPrefix = @"Cannot find the script or external object that implements prefix '{0}'.";
public const string Xslt_ScriptDub = @"Namespace '{0}' has a duplicate implementation.";
public const string Xslt_ScriptEmpty = @"The 'msxsl:script' element cannot be empty.";
public const string Xslt_DupDecimalFormat = @"Decimal format '{0}' has a duplicate declaration.";
public const string Xslt_CircularReference = @"Circular reference in the definition of variable '{0}'.";
public const string Xslt_InvalidExtensionNamespace = @"Extension namespace cannot be 'null' or an XSLT namespace URI.";
public const string Xslt_InvalidModeAttribute = @"An 'xsl:template' element without a 'match' attribute cannot have a 'mode' attribute.";
public const string Xslt_MultipleRoots = @"There are multiple root elements in the output XML.";
public const string Xslt_ApplyImports = @"The 'xsl:apply-imports' instruction cannot be included within the content of an 'xsl:for-each' instruction or within an 'xsl:template' instruction without the 'match' attribute.";
public const string Xslt_Terminate = @"Transform terminated: '{0}'.";
public const string Xslt_InvalidPattern = @"'{0}' is an invalid XSLT pattern.";


public const string Xslt_EmptyTagRequired = @"The tag '{0}' must be empty.";
public const string Xslt_WrongNamespace = @"The wrong namespace was used for XSL. Use 'http://www.w3.org/1999/XSL/Transform'.";
public const string Xslt_InvalidFormat6 = @"Format '{0}' has both  '*' and '_' which is invalid. ";
public const string Xslt_InvalidFormat7 = @"Format '{0}' has '{1}' which is invalid.";
public const string Xslt_ScriptMixLang = @"Multiple scripting languages for the same namespace is not supported.";
public const string Xslt_ScriptInvalidLang = @"The scripting language '{0}' is not supported.";
public const string Xslt_InvalidExtensionPermitions = @"Extension object should not have wider permissions than the caller of the AddExtensionObject(). If wider permissions are needed, wrap the extension object. ";
public const string Xslt_InvalidParamNamespace = @"Parameter cannot belong to XSLT namespace.";
public const string Xslt_DuplicateParametr = @"Duplicate parameter: '{0}'.";
public const string Xslt_VariableCntSel = @"The '{0}' variable has both a select attribute of '{1}' and non-empty contents.";
}
}
