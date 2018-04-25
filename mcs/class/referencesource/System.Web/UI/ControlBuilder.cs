//------------------------------------------------------------------------------
// <copyright file="ControlBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI {
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Instrumentation;
    using System.Web.RegularExpressions;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    /// <devdoc>
    /// Implementation of a generic control builder used by all controls and child objects
    /// </devdoc>
    public class ControlBuilder {

        public readonly static string DesignerFilter = "__designer";
        private readonly static string ItemTypeProperty = "ItemType";

#if DEBUG
        private bool _initCalled;
#endif

        // Parses a databinding expression (e.g. <%# i+1 %>
        private readonly static Regex databindRegex = new DataBindRegex();
        internal readonly static Regex expressionBuilderRegex = new ExpressionBuilderRegex();
        private readonly static Regex bindExpressionRegex = new BindExpressionRegex();
        private readonly static Regex bindParametersRegex = new BindParametersRegex();
        private readonly static Regex bindItemExpressionRegex = new BindItemExpressionRegex();
        private readonly static Regex bindItemParametersRegex = new BindItemParametersRegex();
        private readonly static Regex evalExpressionRegex = new EvalExpressionRegex();
        private readonly static Regex formatStringRegex = new FormatStringRegex();

        private Type _controlType;
        private string _tagName;
        private string _skinID;
        private ArrayList _subBuilders;
        private ControlBuilderParseTimeData _parseTimeData;
        private IServiceProvider _serviceProvider;

        // 
        private ArrayList _eventEntries;
        private ArrayList _simplePropertyEntries;
        private ArrayList _complexPropertyEntries;
        private ArrayList _templatePropertyEntries;
        private ArrayList _boundPropertyEntries;

        //A placeholder dictionary passed to ControlBuilderInterceptor methods.
        private IDictionary _additionalState; 

        private PropertyDescriptor _bindingContainerDescriptor;

        // NOTE: All bool fields in this class should be using the
        // below flags to conserve memory.

        // const masks into the BitVector32
        private const int parseComplete                 = 0x00000001;
        private const int needsTagAttributeComputed     = 0x00000002;
        private const int needsTagAttribute             = 0x00000004;
        private const int doneInitObjectOptimizations   = 0x00000008;
        private const int isICollection                 = 0x00000010;
        private const int isIParserAccessor             = 0x00000020;
        private const int hasFilteredSimpleProps        = 0x00000040;
        private const int hasFilteredComplexProps       = 0x00000080;
        private const int hasFilteredTemplateProps      = 0x00000100;
        private const int hasFilteredBoundProps         = 0x00000200;
        private const int hasTwoWayBoundProps           = 0x00000400;
        private const int triedFieldToControlBinding    = 0x00000800;
        private const int hasFieldToControlBinding      = 0x00001000;
        private const int controlTypeIsControl          = 0x00002000; // Indicates that the type specified in _controlType derives from Control
        private const int entriesSorted                 = 0x00004000;
        private const int applyTheme                    = 0x00008000;
        #pragma warning disable 0649
        private SimpleBitVector32 flags;
        #pragma warning restore 0649


        /// <devdoc>
        /// </devdoc>
        public virtual Type BindingContainerType {
            get {
                if (NamingContainerBuilder == null) {
                    return typeof(System.Web.UI.Control);
                }

                Type ctrlType = NamingContainerBuilder.ControlType;

                Debug.Assert(ctrlType != null, "Control type is null.");
                Debug.Assert(typeof(INamingContainer).IsAssignableFrom(ctrlType), String.Format(CultureInfo.InvariantCulture, "NamingContainerBuilder.Control type {0} is not an INamingContainer", ctrlType.FullName));

                // Recursively lookup if the NamingContainerBuilder.ControlType is an INonBindingContainer
                if (typeof(INonBindingContainer).IsAssignableFrom(ctrlType)) {
                    return NamingContainerBuilder.BindingContainerType;
                }

                return NamingContainerBuilder.ControlType;
            }
        }

        public virtual ControlBuilder BindingContainerBuilder {
            get {
                if (NamingContainerBuilder != null) {

                    Type ctrlType = NamingContainerBuilder.ControlType;

                    Debug.Assert(ctrlType != null, "Control type is null.");
                    Debug.Assert(typeof(INamingContainer).IsAssignableFrom(ctrlType), String.Format(CultureInfo.InvariantCulture, "NamingContainerBuilder.Control type {0} is not an INamingContainer", ctrlType.FullName));

                    // Recursively lookup if the NamingContainerBuilder.ControlType is an INonBindingContainer
                    if (typeof(INonBindingContainer).IsAssignableFrom(ctrlType)) {
                        return NamingContainerBuilder.BindingContainerBuilder;
                    }
                }

                return NamingContainerBuilder;
            }
        }

        /// <devdoc>
        ///If there is a ModelType property set on BindingContainer, returns the type corresponding to it
        /// </devdoc>
        public virtual String ItemType {
            get {
                ControlBuilder bindingContainerBuilder = BindingContainerBuilder;
                if (bindingContainerBuilder != null) {
                    Debug.Assert(bindingContainerBuilder is TemplateBuilder, "Assert failure in ControlBuilder class, there's a scenario where BindingContainerBuilder is not a TemplateBuilder, is someone asking for ModelType out of a Data Binding Context??");
                    if (bindingContainerBuilder.BindingContainerBuilder != null) {
                        return (from object propertyEntry in bindingContainerBuilder.BindingContainerBuilder.SimplePropertyEntriesInternal
                                let simplePropertyEntry = propertyEntry as SimplePropertyEntry
                                where simplePropertyEntry != null && simplePropertyEntry.Name.Equals(ItemTypeProperty, StringComparison.OrdinalIgnoreCase)
                                select (string)simplePropertyEntry.Value).FirstOrDefault();
                    }
                }
                return null;
            }
        }

        internal ICollection EventEntries {
            get {
                // If there are no entries, return a static empty collection
                if (_eventEntries == null)
                    return EmptyCollection.Instance;

                return _eventEntries;
            }
        }

        private ArrayList EventEntriesInternal {
            get {
                // Create the ArrayList on demand
                if (_eventEntries == null)
                    _eventEntries = new ArrayList();

                return _eventEntries;
            }
        }

        internal ICollection SimplePropertyEntries {
            get {
                // If there are no entries, return a static empty collection
                if (_simplePropertyEntries == null)
                    return EmptyCollection.Instance;

                return _simplePropertyEntries;
            }
        }

        internal ArrayList SimplePropertyEntriesInternal {
            get {
                // Create the ArrayList on demand
                if (_simplePropertyEntries == null)
                    _simplePropertyEntries = new ArrayList();

                return _simplePropertyEntries;
            }
        }

        public ICollection ComplexPropertyEntries {
            get {
                // If there are no entries, return a static empty collection
                if (_complexPropertyEntries == null)
                    return EmptyCollection.Instance;

                return _complexPropertyEntries;
            }
        }

        private ArrayList ComplexPropertyEntriesInternal {
            get {
                // Create the ArrayList on demand
                if (_complexPropertyEntries == null)
                    _complexPropertyEntries = new ArrayList();

                return _complexPropertyEntries;
            }
        }

        public ICollection TemplatePropertyEntries {
            get {
                // If there are no entries, return a static empty collection
                if (_templatePropertyEntries == null)
                    return EmptyCollection.Instance;

                return _templatePropertyEntries;
            }
        }

        private ArrayList TemplatePropertyEntriesInternal {
            get {
                // Create the ArrayList on demand
                if (_templatePropertyEntries == null)
                    _templatePropertyEntries = new ArrayList();

                return _templatePropertyEntries;
            }
        }

        internal ICollection BoundPropertyEntries {
            get {
                // If there are no entries, return a static empty collection
                if (_boundPropertyEntries == null)
                    return EmptyCollection.Instance;

                return _boundPropertyEntries;
            }
        }

        private ArrayList BoundPropertyEntriesInternal {
            get {
                // Create the ArrayList on demand
                if (_boundPropertyEntries == null)
                    _boundPropertyEntries = new ArrayList();

                return _boundPropertyEntries;
            }
        }

        internal bool HasFilteredBoundEntries {
            get {
                return flags[hasFilteredBoundProps];
            }
        }

        internal bool IsNoCompile {
            get {
                return flags[parseComplete];
            }
        }

        internal string SkinID {
            get {
                return _skinID;
            }
            set {
                _skinID = value;
            }
        }

        internal IDictionary AdditionalState {
            get {
                if (_additionalState == null) {
                    _additionalState = new Dictionary<object, object>();
                }
                return _additionalState;
            }
        }

        /// <devdoc>
        /// Return the type of the control that this builder creates
        /// </devdoc>
        public Type ControlType {
            get {
                return _controlType;
            }
        }


        public IFilterResolutionService CurrentFilterResolutionService {
            get {
                if (ServiceProvider != null) {
                    return (IFilterResolutionService)ServiceProvider.GetService(typeof(IFilterResolutionService));
                }
                else {
                    // If there is no ServiceProvider, use the TemplateControl (VSWhidbey 551431)
                    return TemplateControl;
                }
            }
        }


        /// <devdoc>
        /// Return the type that will be used by codegen to declare the control
        /// </devdoc>
        public virtual Type DeclareType {
            get {
                return _controlType;
            }
        }

        /// <devdoc>
        /// Gets the IDesignerHost if we are in design mode. This is used to load
        /// config through IWebApplication rather than the RuntimeConfig object.
        /// </devdoc>
        private IDesignerHost DesignerHost {
            get {
                if (InDesigner && ParseTimeData != null) {
                    TemplateParser parser = ParseTimeData.Parser;
                    if (parser != null) {
                        return parser.DesignerHost;
                    }
                }
                return null;
            }
        }

        /// <devdoc>
        ///
        /// </devdoc>
        private ControlBuilder DefaultPropertyBuilder {
            get {
                return ParseTimeData.DefaultPropertyBuilder;
            }
        }


        public IThemeResolutionService ThemeResolutionService {
            get {
                if (ServiceProvider != null) {
                    return (IThemeResolutionService)ServiceProvider.GetService(typeof(IThemeResolutionService));
                }
                else {
                    // If there is no ServiceProvider, use the TemplateControl (VSWhidbey 551431)
                    return TemplateControl as IThemeResolutionService;
                }
            }
        }

        /// <devdoc>
        ///
        /// </devdoc>
        private EventDescriptorCollection EventDescriptors {
            get {
                if (ParseTimeData.EventDescriptors == null) {
                    ParseTimeData.EventDescriptors = TargetFrameworkUtil.GetEvents(_controlType);
                }

                return ParseTimeData.EventDescriptors;
            }
        }

        internal string Filter {
            get {
                return ParseTimeData.Filter;
            }
            set {
                ParseTimeData.Filter = value;
            }
        }


        /// <devdoc>
        ///
        /// </devdoc>
        protected bool FChildrenAsProperties {
            get {
                return ParseTimeData.ChildrenAsProperties;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected bool FIsNonParserAccessor {
            get {
                return ParseTimeData.IsNonParserAccessor;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual bool HasAspCode {
            get {
                return ParseTimeData.HasAspCode;
            }
        }

        /*
         * Return the ID of the control that this builder creates
         */

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string ID {
            get {
                return ParseTimeData.ID;
            }
            set {
                ParseTimeData.ID = value;
            }
        }

        internal bool IsGeneratedID {
            get {
                return ParseTimeData.IsGeneratedID;
            }
            set {
                ParseTimeData.IsGeneratedID = value;
            }
        }

        private bool IgnoreControlProperty {
            get {
                return ParseTimeData.IgnoreControlProperties;
            }
        }


        /// <devdoc>
        ///    <para> InDesigner property gets used by control builders so that they can behave
        ///         differently if needed. </para>
        /// </devdoc>
        protected bool InDesigner {
            get {
                // If  we're in no-compile page mode, return false
                if (IsNoCompile)
                    return false;

                // Simply return null when called without a Parser.
                // This is in the codepath from TemplateBuilder.NeedsTagInnerText()
                // to determine designer source code preservation behavior by tools.
                if (Parser == null)
                    return false;

                return Parser.FInDesigner;
            }
        }


        /// <devdoc>
        ///    <para> InPageTheme property indicates if the control builder is used to generate page themes.</para>
        /// </devdoc>
        protected bool InPageTheme {
            get {
                return Parser is PageThemeParser;
            }
        }

        internal bool IsControlSkin {
            get {
                return ParentBuilder is FileLevelPageThemeBuilder;
            }
        }

        /// <devdoc>
        ///
        /// </devdoc>
        private bool IsHtmlControl {
            get {
                return ParseTimeData.IsHtmlControl;
            }
        }

        /// <devdoc>
        /// The source file line number at which this builder is defined
        /// </devdoc>
        internal int Line {
            get {
                return ParseTimeData.Line;
            }
            set {
                ParseTimeData.Line = value;
            }
        }

        public bool Localize {
            get {
                if (ParseTimeData != null) {
                    return ParseTimeData.Localize;
                }

                return true;
            }
        }

        /// <devdoc>
        ///
        /// </devdoc>
        private ControlBuilder NamingContainerBuilder {
            get {
                if (ParseTimeData.NamingContainerSearched) {
                    return ParseTimeData.NamingContainerBuilder;
                }

                if (ParentBuilder == null || ParentBuilder.ControlType == null) {
                    ParseTimeData.NamingContainerBuilder = null;
                }
                else if (typeof(INamingContainer).IsAssignableFrom(ParentBuilder.ControlType)) {
                    ParseTimeData.NamingContainerBuilder = ParentBuilder;
                }
                else {
                    ParseTimeData.NamingContainerBuilder = ParentBuilder.NamingContainerBuilder;
                }

                ParseTimeData.NamingContainerSearched = true;
                return ParseTimeData.NamingContainerBuilder;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// Return the type of the naming container of the control that this builder creates
        /// </devdoc>
        public Type NamingContainerType {
            get {
                if (NamingContainerBuilder == null) {
                    return typeof(System.Web.UI.Control);
                }

                return NamingContainerBuilder.ControlType;
            }
        }

        /// <devdoc>
        ///
        /// </devdoc>
        internal CompilationMode CompilationMode {
            get {
                return Parser.CompilationMode;
            }
        }

        /// <devdoc>
        ///
        /// </devdoc>
        internal ControlBuilder ParentBuilder {
            get {
                return ParseTimeData.ParentBuilder;
            }
        }


        /// <devdoc>
        ///
        /// </devdoc>
        protected internal TemplateParser Parser {
            get {
                return ParseTimeData.Parser;
            }
        }

        /// <devdoc>
        ///
        /// </devdoc>
        private ControlBuilderParseTimeData ParseTimeData {
            get {
                if (_parseTimeData == null) {
                    if (IsNoCompile) {
                        throw new InvalidOperationException(SR.GetString(SR.ControlBuilder_ParseTimeDataNotAvailable));
                    }

                    _parseTimeData = new ControlBuilderParseTimeData();
                }

                return _parseTimeData;
            }
        }

        /// <devdoc>
        ///
        /// </devdoc>
        private PropertyDescriptorCollection PropertyDescriptors {
            get {
                if (ParseTimeData.PropertyDescriptors == null) {
                    ParseTimeData.PropertyDescriptors = TargetFrameworkUtil.GetProperties(_controlType);
                }

                return ParseTimeData.PropertyDescriptors;
            }
        }

        private StringSet PropertyEntries {
            get {
                if (ParseTimeData.PropertyEntries == null) {
                    ParseTimeData.PropertyEntries = new CaseInsensitiveStringSet();
                }

                return ParseTimeData.PropertyEntries;
            }
        }

        /// <devdoc>
        ///
        /// </devdoc>
        public ArrayList SubBuilders {
            get {
                if (_subBuilders == null) {
                    _subBuilders = new ArrayList();
                }

                return _subBuilders;
            }
        }

        public IServiceProvider ServiceProvider {
            get {
                return _serviceProvider;
            }
        }

        /// <devdoc>
        ///
        /// </devdoc>
        private bool SupportsAttributes {
            get {
                return ParseTimeData.SupportsAttributes;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string TagName {
            get {
                return _tagName;
            }
        }

        /// <devdoc>
        /// The name of the source file in which this builder is defined
        /// </devdoc>
        internal VirtualPath VirtualPath {
            get {
                return ParseTimeData.VirtualPath;
            }
            set {
                ParseTimeData.VirtualPath = value;
            }
        }

        public string PageVirtualPath {
            get {
                return System.Web.VirtualPath.GetVirtualPathString(VirtualPath);
            }
        }

        /// <devdoc>
        /// The template control that hosts the current control. For example, it is the usercontrol for controls defined in a usercontrol file.
        /// This property is available only in non-compiled cases.
        /// </devdoc>
        internal TemplateControl TemplateControl {
            get {
                HttpContext context = HttpContext.Current;
                if (context == null) {
                    return null;
                }

                return context.TemplateControl;
            }
        }

        private void AddBoundProperty(string filter, string name, string expressionPrefix,
            string expression, ExpressionBuilder expressionBuilder, object parsedExpressionData, string fieldName, string formatString, bool twoWayBound, bool encode, int line = 0, int column = 0) {
            AddBoundProperty(filter, name, expressionPrefix, expression, expressionBuilder, parsedExpressionData, false, fieldName, formatString, twoWayBound, encode, line, column);
        }

        /// <devdoc>
        ///
        /// </devdoc>
        private void AddBoundProperty(string filter, string name, string expressionPrefix,
            string expression, ExpressionBuilder expressionBuilder, object parsedExpressionData, bool generated, string fieldName, 
            string formatString, bool twoWayBound, bool encode, int line = 0, int column = 0) {

            Debug.Assert(!String.IsNullOrEmpty(name));

            string controlID = ParseTimeData.ID;

            // Get the IDesignerHost in case we need it to find ExpressionBuilders
            IDesignerHost host = DesignerHost;

            if (String.IsNullOrEmpty(expressionPrefix)) {
                // This is a databinding entry
                if (String.IsNullOrEmpty(controlID)) {
                    if (CompilationMode == CompilationMode.Never) {
                        throw new HttpException(SR.GetString(SR.NoCompileBinding_requires_ID, _controlType.Name, fieldName));
                    }
                    if (twoWayBound) {
                        throw new HttpException(SR.GetString(SR.TwoWayBinding_requires_ID, _controlType.Name, fieldName));
                    }
                }

                Debug.Assert(ControlType != null, "ControlType should not be null if we're adding a property entry");
                // We only support databindings on objects that have an event named "DataBinding"
                if (!flags[controlTypeIsControl]) {
                    if (TargetFrameworkUtil.GetEvent(ControlType, "DataBinding") == null) {
                        throw new InvalidOperationException(SR.GetString(SR.ControlBuilder_DatabindingRequiresEvent, _controlType.FullName));
                    }
                }
            }
            else {
                // If we don't have an expression builder yet, go get it
                if (expressionBuilder == null) {
                    expressionBuilder = ExpressionBuilder.GetExpressionBuilder(expressionPrefix, VirtualPath, host);
                }
            }

            Debug.Assert(!(String.IsNullOrEmpty(expressionPrefix) ^ (expressionBuilder == null)), "expressionBuilder should be non-null iff expressionPrefix is non-empty");

            // Set up a BoundPropertyEntry since we know this is an expression
            BoundPropertyEntry entry = new BoundPropertyEntry();

            entry.Filter = filter;
            entry.Expression = expression;
            entry.ExpressionBuilder = expressionBuilder;
            entry.ExpressionPrefix = expressionPrefix;
            entry.Generated = generated;
            entry.FieldName = fieldName;
            entry.FormatString = formatString;
            entry.ControlType = _controlType;
            entry.ControlID = controlID;
            entry.TwoWayBound = twoWayBound;
            entry.ParsedExpressionData = parsedExpressionData;
            entry.IsEncoded = encode;
            entry.Line = line;
            entry.Column = column;
            
            FillUpBoundPropertyEntry(entry, name);

            // Check for duplicate bound property entries and throws if it finds one.
            // This is done here rather than on AddBoundProperty(BoundPropertyEntry entry) since
            // that overload can be called by other control builders in two-way binding scenarios.
            // In that case it is valid to have duplicate bound property entries since they are on
            // the BindableTemplateBuilder, not the control's ControlBuilder.
            foreach (BoundPropertyEntry bpe in BoundPropertyEntriesInternal) {
                if (String.Equals(bpe.Name, entry.Name, StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(bpe.Filter, entry.Filter, StringComparison.OrdinalIgnoreCase)) {
                    string fullPropertyName = entry.Name;
                    if (!String.IsNullOrEmpty(entry.Filter)) {
                        fullPropertyName = entry.Filter + ":" + fullPropertyName;
                    }
                    throw new InvalidOperationException(SR.GetString(SR.ControlBuilder_CannotHaveMultipleBoundEntries, fullPropertyName, ControlType));
                }
            }

            // Add these to the bound entries
            AddBoundProperty(entry);
        }

        private void AddBoundProperty(BoundPropertyEntry entry) {
            // Add these to the bound entries
            AddEntry(BoundPropertyEntriesInternal, entry);

            if (entry.TwoWayBound) {
                // Remember that this builder has some two-way entries
                flags[hasTwoWayBoundProps] = true;
            }
        }

        // Attach the TargetFrameworkProvider to enable designer Multi-Targeting
        private void AttachTypeDescriptionProvider(object obj) {
            if (InDesigner && (obj != null) && (_serviceProvider != null)) {
                TypeDescriptionProviderService tdpService = _serviceProvider.GetService(typeof(TypeDescriptionProviderService))
                    as TypeDescriptionProviderService;
                if (tdpService != null) {
                    TypeDescriptor.AddProvider(tdpService.GetProvider(obj), obj);
                }
            }
        }

        private void FillUpBoundPropertyEntry(BoundPropertyEntry entry, string name) {

            // Grab a member info corresponding to the property
            string objectModelName;

            MemberInfo memberInfo = PropertyMapper.GetMemberInfo(_controlType, name, out objectModelName);
            entry.Name = objectModelName;

            // If we got a memberInfo
            if (memberInfo != null) {

                if (memberInfo is PropertyInfo) {
                    // If it's a property, make sure it is persistable
                    PropertyInfo propInfo = ((PropertyInfo)memberInfo);

                    if (propInfo.GetSetMethod() == null) {
                        if (!SupportsAttributes) {
                            throw new HttpException(SR.GetString(SR.Property_readonly, name));
                        }
                        else {
                            // If the property is readonly, fall back to using SetAttribute
                            if (entry.TwoWayBound) {
                                entry.ReadOnlyProperty = true;
                            }
                            else {
                                entry.UseSetAttribute = true;
                            }
                        }
                    }
                    else {
                        // The property is settable, so we can use it
                        entry.PropertyInfo = propInfo;
                        entry.Type = propInfo.PropertyType;
                    }

                }
                else {
                    // If it's a field, just grab the type
                    Debug.Assert(memberInfo is FieldInfo);
                    entry.Type = ((FieldInfo)memberInfo).FieldType;
                }
            }
                // If we didn't find a member, we need to use the IAttributeAccessor
            else {
                if (!SupportsAttributes) {
                    throw new HttpException(SR.GetString(SR.Type_doesnt_have_property, _controlType.FullName, name));
                }
                else {
                    if (entry.TwoWayBound) {
                        throw new InvalidOperationException(SR.GetString(SR.ControlBuilder_TwoWayBindingNonProperty, name, ControlType.Name));
                    }
                    entry.Name = name;
                    entry.UseSetAttribute = true;
                }
            }

            // Make sure we have parsed expression data
            if (entry.ParsedExpressionData == null) {
                entry.ParseExpression(new ExpressionBuilderContext(VirtualPath));
            }

            if (!Parser.IgnoreParseErrors && entry.ParsedExpressionData == null) {
                // Disallow empty expressions (VSWhidbey 234273)
                if (Util.IsWhiteSpaceString(entry.Expression)) {

                    throw new HttpException(
                        SR.GetString(SR.Empty_expression));
                }
            }
        }


        /// <devdoc>
        /// </devdoc>
        private void AddCollectionItem(ControlBuilder builder) {
            // Just save the builder and filter and add it to the complex entries
            ComplexPropertyEntry entry = new ComplexPropertyEntry(true);

            entry.Builder = builder;
            entry.Filter = String.Empty;
            AddEntry(ComplexPropertyEntriesInternal, entry);
        }


        /// <devdoc>
        /// </devdoc>
        private void AddComplexProperty(string filter, string name, ControlBuilder builder) {

            // VSWhidbey 281887 Do not ignore complex properties, since templates could be defined inside collections
            // , databinding or code can be placed inside templates.
            /*
            if (IgnoreControlProperty) {
                return;
            }
            */

            Debug.Assert(!String.IsNullOrEmpty(name));
            Debug.Assert(builder != null);

            // Look for a MemberInfo
            string objectModelName = String.Empty;
            MemberInfo memberInfo = PropertyMapper.GetMemberInfo(_controlType, name, out objectModelName);

            // Initialize the entry
            ComplexPropertyEntry entry = new ComplexPropertyEntry();

            entry.Builder = builder;
            entry.Filter = filter;
            entry.Name = objectModelName;

            Type memberType = null;

            if (memberInfo != null) {
                if (memberInfo is PropertyInfo) {
                    PropertyInfo propInfo = ((PropertyInfo)memberInfo);

                    entry.PropertyInfo = propInfo;
                    if (propInfo.GetSetMethod() == null) {
                        entry.ReadOnly = true;
                    }

                    // Check if the property is themeable and persistable
                    ValidatePersistable(propInfo, false, false, false, filter);
                    memberType = propInfo.PropertyType;
                }
                else {
                    Debug.Assert(memberInfo is FieldInfo);
                    memberType = ((FieldInfo)memberInfo).FieldType;
                }

                entry.Type = memberType;
            }
            else {
                throw new HttpException(SR.GetString(SR.Type_doesnt_have_property, _controlType.FullName, name));
            }

            // Add the entry to the complex entries
            AddEntry(ComplexPropertyEntriesInternal, entry);
        }

        /// <devdoc>
        ///
        /// </devdoc>
        private void AddEntry(ArrayList entries, PropertyEntry entry) {
            // Only allow setting the ID property of a control using a simple property (e.g. ID="Button1").
            // This restricts the user from using databinding, expressions, implicit expressions,
            // or inner string properties to set the ID.
            if (String.Equals(entry.Name, "ID", StringComparison.OrdinalIgnoreCase) &&
                flags[controlTypeIsControl] &&
                !(entry is SimplePropertyEntry)) {
                throw new HttpException(SR.GetString(SR.ControlBuilder_IDMustUseAttribute));
            }

            // Remember the item index to perform a stable sort.
            entry.Index = entries.Count;

            // We used to sort the entries here via an insertion-type sort
            // But it's faster just to sort before we use the entries.
            entries.Add(entry);
        }

        /// <devdoc>
        //
        //// </devdoc>
        private void AddProperty(string filter, string name, string value, bool mainDirectiveMode) {
            Debug.Assert(!String.IsNullOrEmpty(name));

            //Second check is a hack to make the intellisense work with strongly typed controls. Existence of ModelType property
            //should force creation of code to make the intellisense to work, so far this is the only property
            //that is required to be identified at design time. 
            //If there's atleast one more property, this hack should be removed and another way should be figured out.
            if (IgnoreControlProperty && !name.Equals(ItemTypeProperty, StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            string objectModelName = String.Empty;
            MemberInfo memberInfo = null;

            // This _controlType can be null if we're using a StringPropertyBuilder that has designer expandos
            if (_controlType != null) {
                if (String.Equals(name, BaseTemplateCodeDomTreeGenerator.skinIDPropertyName, StringComparison.OrdinalIgnoreCase) &&
                    flags[controlTypeIsControl]) {

                    // Make sure there isn't filter for skinID property.
                    if (!String.IsNullOrEmpty(filter)) {
                        throw new InvalidOperationException(SR.GetString(SR.Illegal_Device, BaseTemplateCodeDomTreeGenerator.skinIDPropertyName));
                    }

                    SkinID = value;
                    return;
                }

                memberInfo = PropertyMapper.GetMemberInfo(_controlType, name, out objectModelName);
            }

            if (memberInfo != null) {

                // Found a property on the object, so start building a simple property setter
                SimplePropertyEntry entry = new SimplePropertyEntry();

                entry.Filter = filter;
                entry.Name = objectModelName;
                entry.PersistedValue = value;

                Type memberType = null;

                if (memberInfo is PropertyInfo) {
                    PropertyInfo propInfo = ((PropertyInfo)memberInfo);

                    entry.PropertyInfo = propInfo;

                    // If the property is read-only
                    if (propInfo.GetSetMethod() == null) {
                        if (!SupportsAttributes) {
                            // If it doesn't support attributes, throw an exception
                            throw new HttpException(SR.GetString(SR.Property_readonly, name));
                        }
                        else {
                            // Otherwise, use the attribute accessor
                            entry.UseSetAttribute = true;

                            // Use the original casing of the name from the parsed data
                            entry.Name = name;
                        }
                    }

                    ValidatePersistable(propInfo, entry.UseSetAttribute, mainDirectiveMode, true, filter);
                    memberType = propInfo.PropertyType;
                }
                else {
                    Debug.Assert(memberInfo is FieldInfo);
                    memberType = ((FieldInfo)memberInfo).FieldType;
                }

                entry.Type = memberType;
                if (entry.UseSetAttribute) {
                    entry.Value = value;
                }
                else {
                    // Get the actual value for the property and store it in the entry
                    object objectValue = PropertyConverter.ObjectFromString(memberType, memberInfo, value);

                    DesignTimePageThemeParser themeParser = Parser as DesignTimePageThemeParser;
                    if (themeParser != null) {
                        object[] attrs = memberInfo.GetCustomAttributes(typeof(UrlPropertyAttribute), true);
                        if (attrs.Length > 0) {
                            string url = objectValue.ToString();
                            // Do not combine the url if it's apprelative, let controls resolve the url.
                            if (UrlPath.IsRelativeUrl(url) && !UrlPath.IsAppRelativePath(url)) {
                                objectValue = themeParser.ThemePhysicalPath + url;
                            }
                        }
                    }

                    entry.Value = objectValue;

                    // 

                    if (memberType.IsEnum) {
                        if (objectValue == null) {
                            throw new HttpException(SR.GetString(SR.Invalid_enum_value, value, name, entry.Type.FullName));
                        }

                        entry.PersistedValue = Enum.Format(memberType, objectValue, "G");
                    }
                    else if (memberType == typeof(Boolean)) {
                        // 
                        if (objectValue == null) {
                            entry.Value = true;
                        }
                    }
                }

                AddEntry(SimplePropertyEntriesInternal, entry);
            }
            else {
                bool foundEvent = false;

                // Check if the property is actually an event handler
                if (StringUtil.StringStartsWithIgnoreCase(name, "on")) {
                    string eventName = name.Substring(2);
                    EventDescriptor eventDesc = EventDescriptors.Find(eventName, true);

                    if (eventDesc != null) {
                        if (InPageTheme) {
                            throw new HttpException(SR.GetString(SR.Property_theme_disabled, eventName, ControlType.FullName));
                        }

                        if (value != null)
                            value = value.Trim();

                        if (String.IsNullOrEmpty(value)) {
                            throw new HttpException(SR.GetString(SR.Event_handler_cant_be_empty, name));
                        }

                        if (filter.Length > 0) {
                            throw new HttpException(SR.GetString(SR.Events_cant_be_filtered, filter, name));
                        }

                        foundEvent = true;

                        // First, give the PageParserFilter a chance to handle the event hookup
                        if (!Parser.PageParserFilterProcessedEventHookupAttribute(ID, eventDesc.Name, value)) {
                            // Make sure event handlers are allowed. In no-compile pages, they aren't. (VSWhidbey 450297)
                            Parser.OnFoundEventHandler(name);

                            EventEntry entry = new EventEntry();

                            entry.Name = eventDesc.Name;
                            entry.HandlerType = eventDesc.EventType;
                            entry.HandlerMethodName = value;
                            EventEntriesInternal.Add(entry);
                        }
                    }
                }

                // If we didn't find an eventhandler, we need to use the IAttributeAccessor
                if (!foundEvent) {
                    // Allow the designer filter expandos for simple attributes
                    if (!SupportsAttributes && (filter != DesignerFilter)) {
                        if (_controlType != null) {
                            throw new HttpException(SR.GetString(SR.Type_doesnt_have_property, _controlType.FullName, name));
                        }
                        else {
                            throw new HttpException(SR.GetString(SR.Property_doesnt_have_property, TagName, name));
                        }
                    }

                    SimplePropertyEntry entry = new SimplePropertyEntry();

                    entry.Filter = filter;
                    entry.Name = name;
                    entry.PersistedValue = value;
                    entry.UseSetAttribute = true;
                    entry.Value = value;
                    AddEntry(SimplePropertyEntriesInternal, entry);
                }
            }
        }


        /// <devdoc>
        /// </devdoc>
        private void AddTemplateProperty(string filter, string name, TemplateBuilder builder) {

            /* Do not ignore template properties since we do want to generate IDs for controls defined
               inside SingleInstanceTemplates. VSWhidbey 243341
            if (IgnoreControlProperty) {
                return;
            }
            */

            Debug.Assert(!String.IsNullOrEmpty(name));
            Debug.Assert(builder != null);

            // Look for a MemberInfo
            string objectModelName = String.Empty;
            MemberInfo memberInfo = PropertyMapper.GetMemberInfo(_controlType, name, out objectModelName);

            // Setup a template entry
            bool bindableTemplate = builder is BindableTemplateBuilder;
            TemplatePropertyEntry entry = new TemplatePropertyEntry(bindableTemplate);

            entry.Builder = builder;
            entry.Filter = filter;
            entry.Name = objectModelName;

            Type memberType = null;

            if (memberInfo != null) {
                if (memberInfo is PropertyInfo) {

                    PropertyInfo propInfo = ((PropertyInfo)memberInfo);

                    entry.PropertyInfo = propInfo;

                    ValidatePersistable(propInfo, false, false, false, filter);

                    // Check the attribute on the property to see if it has a ContainerType
                    TemplateContainerAttribute templateAttrib = (TemplateContainerAttribute)Attribute.GetCustomAttribute(propInfo, typeof(TemplateContainerAttribute), false);

                    if (templateAttrib != null) {
                        if (!typeof(INamingContainer).IsAssignableFrom(templateAttrib.ContainerType)) {
                            throw new HttpException(SR.GetString(SR.Invalid_template_container, name, templateAttrib.ContainerType.FullName));
                        }

                        // If it had one, make sure the builder knows what type it is
                        builder.SetControlType(templateAttrib.ContainerType);
                    }

                    entry.Type = propInfo.PropertyType;
                }
                else {
                    Debug.Assert(memberInfo is FieldInfo);
                    memberType = ((FieldInfo)memberInfo).FieldType;
                }

                entry.Type = memberType;
            }
            else {
                throw new HttpException(SR.GetString(SR.Type_doesnt_have_property, _controlType.FullName, name));
            }

            // Add it to the template entries
            AddEntry(TemplatePropertyEntriesInternal, entry);
        }

        /// <devdoc>
        ///
        /// </devdoc>
        internal void AddSubBuilder(object o) {
            SubBuilders.Add(o);
        }

        internal bool HasTwoWayBoundProperties {
            get {
                return flags[hasTwoWayBoundProps];
            }
        }


        /// <devdoc>
        ///
        /// </devdoc>
        public virtual bool AllowWhitespaceLiterals() {
            return true;
        }


        /// <devdoc>
        ///
        /// </devdoc>
        public virtual void AppendLiteralString(string s) {
            // Ignore null strings
            if (s == null) {
                return;
            }

            // If we are not building a control, or if our children define
            // properties, we should not get literal strings.  Ignore whitespace
            // ones, and fail for others
            if (FIsNonParserAccessor || FChildrenAsProperties) {
                // If there is a default property, delegate to its builder
                if (DefaultPropertyBuilder != null) {
                    DefaultPropertyBuilder.AppendLiteralString(s);
                    return;
                }

                s = s.Trim();
                if (FChildrenAsProperties) {
                    // Throw a better error message if the content start with the '<' char.
                    if (s.StartsWith("<", StringComparison.OrdinalIgnoreCase)) {
                        throw new HttpException(SR.GetString(SR.Literal_content_not_match_property, _controlType.FullName, s));
                    }
                }

                if (s.Length != 0) {
                    throw new HttpException(SR.GetString(SR.Literal_content_not_allowed, _controlType.FullName, s));
                }

                return;
            }

            // Ignore literals that are just whitespace if the control does not want them
            if ((AllowWhitespaceLiterals() == false) && Util.IsWhiteSpaceString(s))
                return;

            // A builder can specify its strings need to be html decoded
            if (HtmlDecodeLiterals()) {
                s = HttpUtility.HtmlDecode(s);
            }

            // If the last builder is a DataBoundLiteralControlBuilder, add the string
            // to it instead of to our list of sub-builders
            // But if page instrumentation is enabled, the strings need to be treated
            // separately, so don't combine them.
            DataBoundLiteralControlBuilder dataBoundBuilder = null;
            if (!PageInstrumentationService.IsEnabled) {
                object lastBuilder = GetLastBuilder();
                dataBoundBuilder = lastBuilder as DataBoundLiteralControlBuilder;
            }

            if (dataBoundBuilder != null) {
                Debug.Assert(!InDesigner, "!InDesigner");
                dataBoundBuilder.AddLiteralString(s);
            }
            else {
                AddSubBuilder(s);
            }
        }


        /// <devdoc>
        ///
        /// </devdoc>
        public virtual void AppendSubBuilder(ControlBuilder subBuilder) {
            // Tell the sub builder that it's about to be appended to its parent
            subBuilder.OnAppendToParentBuilder(this);
            if (FChildrenAsProperties) {
                // Don't allow code blocks when properties are expected (ASURT 97838)
                if (subBuilder is CodeBlockBuilder) {
                    throw new HttpException(SR.GetString(SR.Code_not_supported_on_not_controls));
                }

                // If there is a default property, delegate to its builder
                if (DefaultPropertyBuilder != null) {
                    DefaultPropertyBuilder.AppendSubBuilder(subBuilder);
                    return;
                }

                // The tagname is the property name
                string propName = subBuilder.TagName;

                if (subBuilder is TemplateBuilder) {
                    TemplateBuilder tplBuilder = (TemplateBuilder)subBuilder;

                    AddTemplateProperty(tplBuilder.Filter, propName, tplBuilder);
                }
                else if (subBuilder is CollectionBuilder) {
                    // If there are items in the collection, add them
                    if ((subBuilder.SubBuilders != null) && (subBuilder.SubBuilders.Count > 0)) {
                        IEnumerator subBuilders = subBuilder.SubBuilders.GetEnumerator();

                        while (subBuilders.MoveNext()) {
                            ControlBuilder builder = (ControlBuilder)subBuilders.Current;

                            subBuilder.AddCollectionItem(builder);
                        }

                        subBuilder.SubBuilders.Clear();
                        AddComplexProperty(subBuilder.Filter, propName, subBuilder);
                    }
                }
                else if (subBuilder is StringPropertyBuilder) {
                    // Trim this so whitespace doesn't matter inside a tag?
                    string text = ((StringPropertyBuilder)subBuilder).Text.Trim();

                    if (!String.IsNullOrEmpty(text)) {
                        // Make sure we haven't set this property in the attributes already (special case for TextBox and similar things)
                        AddComplexProperty(subBuilder.Filter, propName, subBuilder);
                    }
                }
                else {
                    AddComplexProperty(subBuilder.Filter, propName, subBuilder);
                }

                return;
            }

            CodeBlockBuilder codeBlockBuilder = subBuilder as CodeBlockBuilder;

            if (codeBlockBuilder != null) {
                // Don't allow code blocks inside non-control tags (ASURT 76719)
                if (ControlType != null && !flags[controlTypeIsControl]) {
                    throw new HttpException(SR.GetString(SR.Code_not_supported_on_not_controls));
                }

                // Is it a databinding expression?  <%# ... %>
                if (codeBlockBuilder.BlockType == CodeBlockType.DataBinding) {
                    // Bind statements are not allowed as DataBoundLiterals inside any template.
                    // Outside a template, they should be treated as calls to page code.
                    Match match;

                    if ((match = bindExpressionRegex.Match(codeBlockBuilder.Content, 0)).Success || (match = bindItemExpressionRegex.Match(codeBlockBuilder.Content, 0)).Success) {
                        ControlBuilder currentBuilder = this;

                        while (currentBuilder != null && !(currentBuilder is TemplateBuilder)) {
                            currentBuilder = currentBuilder.ParentBuilder;
                        }

                        if (currentBuilder != null && currentBuilder.ParentBuilder != null && currentBuilder is TemplateBuilder) {
                            throw new HttpException(SR.GetString(SR.DataBoundLiterals_cant_bind));
                        }
                    }

                    if (InDesigner) {
                        // In the designer, don't use the fancy multipart DataBoundLiteralControl,
                        // which breaks a number of things (ASURT 82925,86738).  Instead, use the
                        // simpler DesignerDataBoundLiteralControl, and do standard databinding
                        // on its Text property.
                        IDictionary attribs = new ParsedAttributeCollection();

                        attribs.Add("Text", "<%#" + codeBlockBuilder.Content + "%>");
                        subBuilder = CreateBuilderFromType(Parser, this, typeof(DesignerDataBoundLiteralControl),
                            null, null, attribs, codeBlockBuilder.Line, codeBlockBuilder.PageVirtualPath);
                    }
                    else {
                        // Get the last builder, and check if it's a DataBoundLiteralControlBuilder
                        object lastBuilder = GetLastBuilder();
                        DataBoundLiteralControlBuilder dataBoundBuilder = lastBuilder as DataBoundLiteralControlBuilder;

                        // If not, then we need to create one.  Otherwise, just append to the
                        // existing one
                        bool fNewDataBoundLiteralControl = false;

                        if (dataBoundBuilder == null) {
                            dataBoundBuilder = new DataBoundLiteralControlBuilder();
                            dataBoundBuilder.Init(Parser, this, typeof(DataBoundLiteralControl), null, null, null);
                            dataBoundBuilder.Line = codeBlockBuilder.Line;
                            dataBoundBuilder.VirtualPath = codeBlockBuilder.VirtualPath;
                            fNewDataBoundLiteralControl = true;

                            // If the previous builder was a string, add it as the first
                            // entry in the composite control.
                            // But if instrumentation is enabled, the strings need to be
                            // treated separately, so don't combine them
                            if (!PageInstrumentationService.IsEnabled) {
                                string s = lastBuilder as string;

                                if (s != null) {
                                    SubBuilders.RemoveAt(SubBuilders.Count - 1);
                                    dataBoundBuilder.AddLiteralString(s);
                                }
                            }
                        }

                        dataBoundBuilder.AddDataBindingExpression(codeBlockBuilder);
                        if (!fNewDataBoundLiteralControl)
                            return;

                        subBuilder = dataBoundBuilder;
                    }
                }
                else {
                    // Set a flag if there is at least one block of ASP code
                    ParseTimeData.HasAspCode = true;
                }
            }

            if (FIsNonParserAccessor) {
                throw new HttpException(SR.GetString(SR.Children_not_supported_on_not_controls));
            }

            AddSubBuilder(subBuilder);
        }

        /// <devdoc>
        /// Builds all child ControlBuilders of this ControlBuilder.
        /// Only used in the no-compile scenario.
        /// </devdoc>
        internal virtual void BuildChildren(object parentObj) {
            // Create all the children
            if (_subBuilders != null) {
                IEnumerator en = _subBuilders.GetEnumerator();

                for (int i = 0; en.MoveNext(); i++) {
                    object childObj;
                    object cur = en.Current;

                    if (cur is string) {
                        childObj = new LiteralControl((string)cur);
                    }
                    else if (cur is CodeBlockBuilder) {
                        if (InDesigner) {
                            CodeBlockBuilder cbb = (CodeBlockBuilder)cur;
                            string code;
                            switch (cbb.BlockType) {
                                case CodeBlockType.Code:
                                    code = "<%" + cbb.Content + "%>";
                                    break;
                                case CodeBlockType.Expression:
                                    code = "<%=" + cbb.Content + "%>";
                                    break;
                                case CodeBlockType.EncodedExpression:
                                    code = "<%:" + cbb.Content + "%>";
                                    break;
                                case CodeBlockType.DataBinding:
                                    code = "<%#" + (cbb.IsEncoded ? ":" : "") + cbb.Content + "%>";
                                    break;
                                default:
                                    Debug.Fail("Invalid value for CodeBlockType enum");
                                    code = null;
                                    break;
                            }
                            childObj = new LiteralControl(code);
                        }
                        else {
                            // In case this function is called at runtime, we want to be consistent with the past behavior
                            continue;
                        }
                    }
                    else {
                        ControlBuilder controlBuilder = (ControlBuilder)cur;

                        Debug.Assert(controlBuilder.ServiceProvider == null);
                        controlBuilder.SetServiceProvider(ServiceProvider);
                        try {
                            childObj = controlBuilder.BuildObject(flags[applyTheme]);

                            // If it's a user control, call its InitializeAsUserControl
                            if (!InDesigner) {
                                UserControl uc = childObj as UserControl;

                                if (uc != null) {
                                    Control parent = parentObj as Control;

                                    Debug.Assert(parent != null);
                                    uc.InitializeAsUserControl(parent.Page);
                                }
                            }
                        } finally {
                            controlBuilder.SetServiceProvider(null);
                        }
                    }

                    Debug.Assert(childObj != null);
                    Debug.Assert(typeof(IParserAccessor).IsAssignableFrom(parentObj.GetType()));
                    ((IParserAccessor)parentObj).AddParsedSubObject(childObj);
                }
            }
        }


        /// <devdoc>
        /// This code is only used in the no-compile mode.
        /// It is used at design-time and when the user calls Page.ParseControl.
        /// </devdoc>
        public virtual object BuildObject() {
            return BuildObjectInternal();
        }

        // Helper which sets themebility (This will only ever be true in the designer)
        internal object BuildObject(bool shouldApplyTheme) {
            if (flags[applyTheme] != shouldApplyTheme)
                flags[applyTheme] = shouldApplyTheme;
            return BuildObject();
        }

        internal object BuildObjectInternal() {
            // Can't assert these anymore since we've discarded this information by the time we call this method
            // Debug.Assert(InDesigner || CompilationMode == CompilationMode.Never, "Expected to be in designer mode.");
            // Since getting the ConstructorNeedsTagAttribute is very expensive, cache
            // the result in a flag
            if (!flags[needsTagAttributeComputed]) {
                // If it has a ConstructorNeedsTagAttribute, it needs a tag name
                ConstructorNeedsTagAttribute cnta = (ConstructorNeedsTagAttribute)TargetFrameworkUtil.GetAttributes(ControlType)[typeof(ConstructorNeedsTagAttribute)];

                if (cnta != null && cnta.NeedsTag) {
                    flags[needsTagAttribute] = true;
                }

                // Remember that we have cached it
                flags[needsTagAttributeComputed] = true;
            }

            Object obj;

            if (flags[needsTagAttribute]) {
                // Create the object, using its ctor that takes the tag name
                Object[] args = new Object[] { TagName };

                obj = HttpRuntime.CreatePublicInstance(_controlType, args);
            }
            else {
                // Create the object
                obj = HttpRuntime.FastCreatePublicInstance(_controlType);
            }

            if (flags[applyTheme]) obj = GetThemedObject(obj);

            AttachTypeDescriptionProvider(obj);
            RenderTraceListener.CurrentListeners.ShareTraceData(this, obj);
            InitObject(obj);
            return obj;
        }


        /// <devdoc>
        /// Called when the parser is done with parsing for this ControlBuilder.
        /// </devdoc>
        public virtual void CloseControl() {
        }

        internal static ParsedAttributeCollection ConvertDictionaryToParsedAttributeCollection(IDictionary attribs) {
            if (attribs is ParsedAttributeCollection) {
                return (ParsedAttributeCollection)attribs;
            }

            // Assert here so we know our own code is never passing in a plain IDictionary
            // System.Web.Mobile does this, so we don't assert
            // Debug.Assert(false);
            ParsedAttributeCollection newAttribs = new ParsedAttributeCollection();

            foreach (DictionaryEntry entry in attribs) {
                newAttribs.AddFilteredAttribute(String.Empty, entry.Key.ToString(), entry.Value.ToString());
            }

            return newAttribs;
        }

        internal ControlBuilder CreateChildBuilder(string filter, string tagName, IDictionary attribs, TemplateParser parser, ControlBuilder parentBuilder, string id, int line, VirtualPath virtualPath, ref Type childType, bool defaultProperty) {
            ControlBuilder subBuilder;

            if (FChildrenAsProperties) {
                // If there is a default property, delegate to its builder
                if (DefaultPropertyBuilder != null) {
                    // check if a property exists for this tag
                    PropertyInfo pInfo = TargetFrameworkUtil.GetProperty(_controlType, tagName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase);

                    if (pInfo != null) {
                        subBuilder = GetChildPropertyBuilder(tagName, attribs, ref childType, parser, false);

                        // If items have already been added to the default builder, throw an exception
                        if (DefaultPropertyBuilder.SubBuilders.Count > 0) {
                            ParseChildrenAttribute pca = null;
                            object[] attrs = TargetFrameworkUtil.GetCustomAttributes(ControlType, typeof(ParseChildrenAttribute), /*inherit*/ true);

                            pca = (ParseChildrenAttribute)attrs[0];
                            Debug.Assert(pca != null);
                            throw new HttpException(SR.GetString(SR.Cant_use_default_items_and_filtered_collection, _controlType.FullName, pca.DefaultProperty));
                        }

                        // Can't use the default property builder anymore since we have filtered collections
                        ParseTimeData.DefaultPropertyBuilder = null;
                    }
                    else {
                        subBuilder = DefaultPropertyBuilder.CreateChildBuilder(filter, tagName, attribs, parser, parentBuilder, id, line, virtualPath, ref childType, false /*defaultProperty*/);
                    }
                }
                else {
                    subBuilder = GetChildPropertyBuilder(tagName, attribs, ref childType, parser, defaultProperty);
                }
            }
            else {
                string fullTagName = Util.CreateFilteredName(filter, tagName);

                childType = GetChildControlType(fullTagName, attribs);
                if (childType == null) {
                    return null;
                }

                // We have to pass in the fullTagName since these will be actual registered controls
                subBuilder = CreateBuilderFromType(parser, parentBuilder, childType, fullTagName,
                    id, attribs, line, PageVirtualPath);
            }

            if (subBuilder == null) {
                return null;
            }

            subBuilder.Filter = filter;
            subBuilder.SetParentBuilder((parentBuilder != null) ? parentBuilder : this);
            return subBuilder;
        }


        /// <devdoc>
        /// Create a ControlBuilder for a given tag
        /// </devdoc>
        public static ControlBuilder CreateBuilderFromType(TemplateParser parser, ControlBuilder parentBuilder, Type type, string tagName, string id, IDictionary attribs, int line, string sourceFileName) {
            ControlBuilder builder = CreateBuilderFromType(type);

            // 

            builder.Line = line;
            builder.VirtualPath = System.Web.VirtualPath.CreateAllowNull(sourceFileName);

            // Initialize the builder
            builder.Init(parser, parentBuilder, type, tagName, id, attribs);
            return builder;
        }

#if !DONTUSEFACTORYGENERATOR
        // Cache instances of IWebObjectFactory for each control Type, which allow us
        // to instantiate the builders very efficiently, compared to calling
        // GetCustomAttributes and Activator.CreateInstance on every call.
        private static FactoryGenerator s_controlBuilderFactoryGenerator;
#endif // DONTUSEFACTORYGENERATOR
        private static Hashtable s_controlBuilderFactoryCache;

        private static ControlBuilder CreateBuilderFromType(Type type) {
            // Create the factory generator on demand
            if (s_controlBuilderFactoryCache == null) {
#if !DONTUSEFACTORYGENERATOR
                s_controlBuilderFactoryGenerator = new FactoryGenerator();
#endif // DONTUSEFACTORYGENERATOR

                // Create the factory cache
                s_controlBuilderFactoryCache = Hashtable.Synchronized(new Hashtable());

                // Seed the cache with a few types that we don't want to expose as public (they
                // need to be public for FactoryGenerator to be used).
                s_controlBuilderFactoryCache[typeof(Content)] = new ContentBuilderInternalFactory();
                s_controlBuilderFactoryCache[typeof(ContentPlaceHolder)] = new ContentPlaceHolderBuilderFactory();
            }

            // First, check if it's cached
            IWebObjectFactory factory = (IWebObjectFactory)s_controlBuilderFactoryCache[type];

            if (factory == null) {
                // Check whether the control's class exposes a custom builder type
                ControlBuilderAttribute cba = GetControlBuilderAttribute(type);

                if (cba != null) {
                    // Make sure the type has the correct base class (ASURT 123677)
                    Util.CheckAssignableType(typeof(ControlBuilder), cba.BuilderType);
#if !DONTUSEFACTORYGENERATOR
                    if (cba.BuilderType.IsPublic) {
                        // If the builder type is public, codegen a fast factory for it
                        factory = s_controlBuilderFactoryGenerator.CreateFactory(cba.BuilderType);
                    }
                    else {
                        Debug.Assert(false, "The type " + cba.BuilderType.Name + " should be made public for better performance.");
#endif // DONTUSEFACTORYGENERATOR

                        // It's not public, so we must stick with slower reflection
                        factory = new ReflectionBasedControlBuilderFactory(cba.BuilderType);
#if !DONTUSEFACTORYGENERATOR
                    }
#endif // DONTUSEFACTORYGENERATOR
                }
                else {
                    // use a factory that creates generic builders (i.e. ControlBuilder's)
                    factory = s_defaultControlBuilderFactory;
                }

                // Cache the factory
                s_controlBuilderFactoryCache[type] = factory;
            }

            return (ControlBuilder) factory.CreateInstance();
        }

        private static ControlBuilderAttribute GetControlBuilderAttribute(Type controlType) {
            // Check whether the control's class exposes a custom builder type
            ControlBuilderAttribute cba = null;
            object[] attrs = TargetFrameworkUtil.GetCustomAttributes(controlType, typeof(ControlBuilderAttribute), /*inherit*/ true);

            if ((attrs != null) && (attrs.Length > 0)) {
                Debug.Assert(attrs[0] is ControlBuilderAttribute);
                cba = (ControlBuilderAttribute)attrs[0];
            }

            return cba;
        }

        private ControlBuilder GetChildPropertyBuilder(string tagName, IDictionary attribs, ref Type childType, TemplateParser templateParser, bool defaultProperty) {
            Debug.Assert(FChildrenAsProperties, "ChildrenAsProperties");

            // Parse the device filter if any
            // The child is supposed to be a property, so look for it
            PropertyInfo pInfo = TargetFrameworkUtil.GetProperty(_controlType, tagName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase);

            if (pInfo == null) {
                throw new HttpException(SR.GetString(SR.Type_doesnt_have_property, _controlType.FullName, tagName));
            }

            // Get its type
            childType = pInfo.PropertyType;

            ControlBuilder builder = null;

            // If it's a collection, return the collection builder
            if (typeof(ICollection).IsAssignableFrom(childType)) {
                // Check whether the prop has an IgnoreUnknownContentAttribute
                IgnoreUnknownContentAttribute attr = (IgnoreUnknownContentAttribute)Attribute.GetCustomAttribute(pInfo, typeof(IgnoreUnknownContentAttribute), true);

                builder = new CollectionBuilder(attr != null /*ignoreUnknownContent*/);
            }
            else if (typeof(ITemplate).IsAssignableFrom(childType)) {
                bool useBindableTemplate = false;
                bool allowMultipleInstances = true;
                object[] containerAttrs = pInfo.GetCustomAttributes(typeof(TemplateContainerAttribute), /*inherits*/ false);

                if ((containerAttrs != null) && (containerAttrs.Length > 0)) {
                    Debug.Assert(containerAttrs[0] is TemplateContainerAttribute);
                    useBindableTemplate = (((TemplateContainerAttribute)containerAttrs[0]).BindingDirection == BindingDirection.TwoWay);
                }

                allowMultipleInstances = Util.IsMultiInstanceTemplateProperty(pInfo);

                if (useBindableTemplate) {  // If it's a bindable template, return the bindable template builder
                    builder = new BindableTemplateBuilder();
                }
                else {  // If it's a template, return the template builder
                    builder = new TemplateBuilder();
                }

                if (builder is TemplateBuilder) {
                    ((TemplateBuilder)builder).AllowMultipleInstances = allowMultipleInstances;
                    // If we're in the designer, set a reference to the designer host
                    // so we can get to a filter resolution service later
                    if (InDesigner) {
                        ((TemplateBuilder)builder).SetDesignerHost(templateParser.DesignerHost);
                    }
                }
            }
            else if (childType == typeof(string)) {
                PersistenceModeAttribute persistenceAttr = (PersistenceModeAttribute)Attribute.GetCustomAttribute(pInfo, typeof(PersistenceModeAttribute), true);

                if (((persistenceAttr == null) || (persistenceAttr.Mode == PersistenceMode.Attribute)) && !defaultProperty) {
                    // If the property is supposed to be declared as an attribute on a control tag, throw if it was declared as an inner property
                    // We don't throw if we are simply building the DefaultPropertyBuilder.
                    throw new HttpException(SR.GetString(SR.ControlBuilder_CannotHaveComplexString, _controlType.FullName, tagName));
                }
                builder = new StringPropertyBuilder();
            }

            if (builder != null) {
                builder.Line = Line;
                builder.VirtualPath = VirtualPath;

                // Initialize the builder
                builder.Init(Parser, (ControlBuilder)this, null, tagName, null, attribs);
                return builder;
            }

            // Otherwise, simply return the builder for the property
            builder = CreateBuilderFromType(Parser, this, childType, tagName, null,
                attribs, Line, PageVirtualPath);
            return builder;
        }


        /// <devdoc>
        /// When overridden, returns the control type of any parsed child controls
        /// </devdoc>
        public virtual Type GetChildControlType(string tagName, IDictionary attribs) {
            return null;
        }

        /// <devdoc>
        /// Convenience method for grabbing the set of property entries that we need to set
        /// on an instance of the object this controlbuilder builds.
        /// </devdoc>
        internal ICollection GetFilteredPropertyEntrySet(ICollection entries) {
            // If we encounter the default value, put it in the table if it doesn't already exist
            // If we encounter the filter value, replace whatevers in the table.
            IDictionary filteredEntries = new HybridDictionary(true);
            IFilterResolutionService filterResolutionService = CurrentFilterResolutionService;

            if (filterResolutionService != null) {
                foreach (PropertyEntry entry in entries) {
                    if (!filteredEntries.Contains(entry.Name)) {
                        String filter = entry.Filter;
                        // empty filter always matches.
                        if (String.IsNullOrEmpty(filter) || filterResolutionService.EvaluateFilter(filter)) {
                            filteredEntries[entry.Name] = entry;
                        }
                    }
                }
            }
            else {
                // If there isn't a filter resolution service, just add anything from the default filter
                foreach (PropertyEntry entry in entries) {
                    if (String.IsNullOrEmpty(entry.Filter)) {
                        filteredEntries[entry.Name] = entry;
                    }
                }
            }

            return filteredEntries.Values;
        }

        // Check if any of the entries have a filter
        private bool HasFilteredEntries(ICollection entries) {
            foreach (PropertyEntry entry in entries) {
                if (entry.Filter.Length > 0)
                    return true;
            }

            // None of the entries are filtered
            return false;
        }

        /// <devdoc>
        /// Return the last sub builder added to this builder
        /// </devdoc>
        internal object GetLastBuilder() {
            if (SubBuilders.Count == 0) {
                return null;
            }

            return SubBuilders[SubBuilders.Count - 1];
        }


        public ObjectPersistData GetObjectPersistData() {
            return new ObjectPersistData(this, Parser.RootBuilder.BuiltObjects);
        }


        /// <devdoc>
        /// Does this control have a body.  e.g. <foo/> doesn't.
        /// </devdoc>
        public virtual bool HasBody() {
            return true;
        }


        public virtual bool HtmlDecodeLiterals() {
            return false;
        }


        /// <devdoc>
        /// @param parser The instance of the parser that is controlling us.
        /// @param tagName The name of the tag to be built.  This is necessary
        ///      to allow a builder to support multiple tag types.
        /// @param attribs IDictionary which holds all the attributes of
        ///      the tag.  It is immutable.
        /// @param type Type of the control that this builder will create.
        /// </devdoc>
        public virtual void Init(TemplateParser parser, ControlBuilder parentBuilder, Type type, string tagName, string id, IDictionary attribs) {
#if DEBUG
            Debug.Assert(!_initCalled, "ControlBuilder.Init() should never be called more than once on the same ControlBuilder.");
            _initCalled = true;
#endif
            if (parser != null && parser.ControlBuilderInterceptor != null) {
                parser.ControlBuilderInterceptor.PreControlBuilderInit(this, parser, parentBuilder, type, tagName, id, attribs, AdditionalState);
            }
            ParseTimeData.Parser = parser;
            ParseTimeData.ParentBuilder = parentBuilder;
            if (parser != null) {
                ParseTimeData.IgnoreControlProperties = parser.IgnoreControlProperties;
            }

            _tagName = tagName;
            if (type != null) {
                _controlType = type;
                flags[controlTypeIsControl] = typeof(Control).IsAssignableFrom(_controlType);

                ID = id;

                // Try to get a ParseChildrenAttribute from the object
                ParseChildrenAttribute pca = GetParseChildrenAttribute(type);

                // Is this a builder for an object that implements IParserAccessor?
                if (!typeof(IParserAccessor).IsAssignableFrom(type)) {
                    ParseTimeData.IsNonParserAccessor = true;

                    // Non controls never have children
                    ParseTimeData.ChildrenAsProperties = true;
                }
                else {
                    // Check if the nested tags define properties, as opposed to children
                    if (pca != null) {
                        ParseTimeData.ChildrenAsProperties = pca.ChildrenAsProperties;
                    }
                }

                if (FChildrenAsProperties) {
                    // Check if there is a default property
                    if (pca != null && pca.DefaultProperty.Length != 0) {
                        Type subType = null;

                        // Create a builder for the default prop
                        // Default property is always the default filter
                        ParseTimeData.DefaultPropertyBuilder = CreateChildBuilder(String.Empty, pca.DefaultProperty, null/*attribs*/, parser, null, null /*id*/, Line, VirtualPath, ref subType, true /*defaultProperty*/);
                        Debug.Assert(DefaultPropertyBuilder != null, pca.DefaultProperty);
                    }
                }

                // Check if the object is an HtmlControl
                ParseTimeData.IsHtmlControl = typeof(HtmlControl).IsAssignableFrom(_controlType);

                // Check if the object supports attributes
                ParseTimeData.SupportsAttributes = typeof(IAttributeAccessor).IsAssignableFrom(_controlType);
            }
            else {
                flags[controlTypeIsControl] = false;
            }

            // Process the attributes, if any
            if (attribs != null) {
                // This could be called by anyone, so if it's not the parser that's calling us
                // we have to copy all the attribs values over to a ParsedAttributeCollection
                // in the default filter
                PreprocessAttributes(ConvertDictionaryToParsedAttributeCollection(attribs));
            }

            // Check if the same control with identical skinID is already defined as a control skin.
            // If so, fails now instead of infinite recursion during runtime or designtime rendering.
            // VSWhidbey 531782.
            if (InPageTheme) {
                ControlBuilder builder = ((PageThemeParser)parser).CurrentSkinBuilder;
                if (builder != null && 
                    builder.ControlType == ControlType && 
                    String.Equals(builder.SkinID, SkinID, StringComparison.OrdinalIgnoreCase)) {
                        throw new InvalidOperationException(SR.GetString(SR.Cannot_set_recursive_skin, builder.ControlType.Name));
                }
            }
        }

        // Cache the custom ParseChildrenAttribute for each type to avoid having to call
        // GetCustomAttributes any more than necessary (it's extremely slow)
        private static ParseChildrenAttribute s_markerParseChildrenAttribute = new ParseChildrenAttribute();

        private static Hashtable s_parseChildrenAttributeCache = new Hashtable();

        private static ParseChildrenAttribute GetParseChildrenAttribute(Type controlType) {
            // First, see if we have it cached for this type
            ParseChildrenAttribute pca = (ParseChildrenAttribute)s_parseChildrenAttributeCache[controlType];

            if (pca == null) {
                // Try to get a ParseChildrenAttribute from the type
                object[] attrs = TargetFrameworkUtil.GetCustomAttributes(controlType, typeof(ParseChildrenAttribute), /*inherit*/ true);

                if ((attrs != null) && (attrs.Length > 0)) {
                    Debug.Assert(attrs[0] is ParseChildrenAttribute);
                    pca = (ParseChildrenAttribute)attrs[0];
                }

                // If it doesn't have one, use a default as a marker
                if (pca == null)
                    pca = s_markerParseChildrenAttribute;

                // Cache the ParseChildrenAttribute
                lock (s_parseChildrenAttributeCache.SyncRoot) {
                    s_parseChildrenAttributeCache[controlType] = pca;
                }
            }

            // If it's the marker one, just return null
            if (pca == s_markerParseChildrenAttribute)
                return null;

            return pca;
        }

        private void DoInitObjectOptimizations(object obj) {
            // Cache whether it's an ICollection, since IsAssignableFrom is expensive
            flags[isICollection] = typeof(ICollection).IsAssignableFrom(ControlType);

            // Cache whether it's an IParserAccessor, since IsAssignableFrom is expensive
            flags[isIParserAccessor] = typeof(IParserAccessor).IsAssignableFrom(obj.GetType());
            if (_simplePropertyEntries != null) {
                flags[hasFilteredSimpleProps] = HasFilteredEntries(_simplePropertyEntries);
            }

            if (_complexPropertyEntries != null) {
                flags[hasFilteredComplexProps] = HasFilteredEntries(_complexPropertyEntries);
            }

            if (_templatePropertyEntries != null) {
                flags[hasFilteredTemplateProps] = HasFilteredEntries(_templatePropertyEntries);
            }

            if (_boundPropertyEntries != null) {
                flags[hasFilteredBoundProps] = HasFilteredEntries(_boundPropertyEntries);
            }
        }

        internal virtual object GetThemedObject(object obj) {
            Control control = obj as Control;

            if (control == null) return obj;

            IThemeResolutionService themeService = ThemeResolutionService;

            if (themeService != null) {
                if (!String.IsNullOrEmpty(SkinID)) {
                    control.SkinID = SkinID;
                }

                // Apply the theme builders before we run the regular builders
                ThemeProvider themeProvider = themeService.GetStylesheetThemeProvider();
                SkinBuilder themeBuilder = null;

                if (themeProvider != null) {
                    themeBuilder = themeProvider.GetSkinBuilder(control);
                    if (themeBuilder != null) {
                        try {
                            themeBuilder.SetServiceProvider(ServiceProvider);
                            return themeBuilder.ApplyTheme();
                        } finally {
                            themeBuilder.SetServiceProvider(null);
                        }
                    }
                }
            }

            return control;
        }

        /// <devdoc>
        /// Sets all the properties of a built object corresponding to this ControlBuilder.
        /// This code is only used in the no-compile and designer modes
        /// </devdoc>
        internal virtual void InitObject(object obj) {
            // Can't assert these anymore since we've discarded this information by the time we call this method
            // Debug.Assert(InDesigner || CompilationMode == CompilationMode.Never, "Expected to be in designer mode.");
            // Make sure we initialize the property entries in the right order
            EnsureEntriesSorted();

            // Do some expensive one time pre-computations on demand
            if (!flags[doneInitObjectOptimizations]) {
                DoInitObjectOptimizations(obj);
                flags[doneInitObjectOptimizations] = true;
            }

            Control control = obj as Control;
            if (control != null) {
                if (InDesigner) {
                    control.SetDesignMode();
                }

                if (SkinID != null) {
                    control.SkinID = SkinID;
                }

                // Need to apply stylesheet on the controls in non-compiled pages.
                if (!InDesigner && TemplateControl != null) {
                    control.ApplyStyleSheetSkin(TemplateControl.Page);
                }
            }

            InitSimpleProperties(obj);
            if (flags[isICollection]) {
                InitCollectionsComplexProperties(obj);
            }
            else {
                InitComplexProperties(obj);
            }

            if (InDesigner) {
                if (control != null) {
                    if (Parser.DesignTimeDataBindHandler != null) {
                        control.DataBinding += Parser.DesignTimeDataBindHandler;
                    }

                    // Set a reference to the control builder that created this object
                    control.SetControlBuilder(this);
                }

                Parser.RootBuilder.BuiltObjects[obj] = this;
            }

            InitBoundProperties(obj);

            if (flags[isIParserAccessor]) {
                // Build the children
                BuildChildren(obj);
            }

            InitTemplateProperties(obj);

            if (control != null)
                BindFieldToControl(control);

            // 



        }

        private void InitSimpleProperties(object obj) {
            // Don't do anything if there are no entries
            if (_simplePropertyEntries == null)
                return;

            // If there are no filters in the picture, use the entries as is
            ICollection entries;

            if (flags[hasFilteredSimpleProps])
                entries = GetFilteredPropertyEntrySet(SimplePropertyEntries);
            else
                entries = SimplePropertyEntries;

            // Now that we have the proper set, set all the entries
            foreach (SimplePropertyEntry entry in entries) {
                SetSimpleProperty(entry, obj);
            }
        }

        internal void SetSimpleProperty(SimplePropertyEntry entry, object obj) {
            if (entry.UseSetAttribute) {
                ((IAttributeAccessor)obj).SetAttribute(entry.Name, entry.Value.ToString());
            }
            else {
                try {
                    PropertyMapper.SetMappedPropertyValue(obj, entry.Name, entry.Value, InDesigner);
                }
                catch (Exception e) {
                    throw new HttpException(SR.GetString(SR.Cannot_set_property, entry.PersistedValue, entry.Name), e);
                }
            }
        }

        private void InitCollectionsComplexProperties(object obj) {
            // Don't do anything if there are no entries
            if (_complexPropertyEntries == null)
                return;

            foreach (ComplexPropertyEntry entry in ComplexPropertyEntries) {
                try {
                    ControlBuilder controlBuilder = ((ComplexPropertyEntry)entry).Builder;
                    Debug.Assert(((ComplexPropertyEntry)entry).IsCollectionItem, "The entry should be a collection entry, instead it's a " + entry.GetType());
                    object objValue;

                    Debug.Assert(controlBuilder.ServiceProvider == null);
                    controlBuilder.SetServiceProvider(ServiceProvider);
                    try {
                        objValue = controlBuilder.BuildObject(flags[applyTheme]);
                    } finally {
                        controlBuilder.SetServiceProvider(null);
                    }

                    object[] parameters = new object[1];
                    parameters[0] = objValue;
                    MethodInfo methodInfo = ControlType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { objValue.GetType() }, null);
                    if (methodInfo == null) {
                        throw new InvalidOperationException(SR.GetString(SR.ControlBuilder_CollectionHasNoAddMethod, TagName));
                    }
                    Util.InvokeMethod(methodInfo, obj, parameters);
                }
                catch (Exception ex) {
                    throw new HttpException(SR.GetString(SR.Cannot_add_value_not_collection, TagName, ex.Message), ex);
                }
            }
        }

        private void InitComplexProperties(object obj) {
            // Don't do anything if there are no entries
            if (_complexPropertyEntries == null)
                return;

            // If there are no filters in the picture, use the entries as is
            ICollection entries;

            if (flags[hasFilteredComplexProps])
                entries = GetFilteredPropertyEntrySet(ComplexPropertyEntries);
            else
                entries = ComplexPropertyEntries;

            foreach (ComplexPropertyEntry entry in entries) {
                if (entry.ReadOnly) {
                    try {
                        object objectValue = FastPropertyAccessor.GetProperty(obj, entry.Name, InDesigner);

                        entry.Builder.SetServiceProvider(ServiceProvider);
                        try {
                            // We must push the theme flag to child complex objects so they are init'd properly
                            
                            // DevDiv Bug 59351
                            // Set applytheme only when necessary.
                            if (entry.Builder.flags[applyTheme] != flags[applyTheme]) {
                                entry.Builder.flags[applyTheme] = flags[applyTheme];
                            }
                            entry.Builder.InitObject(objectValue);
                        }
                        finally {
                            entry.Builder.SetServiceProvider(null);
                        }
                    }
                    catch (Exception e) {
                        throw new HttpException(SR.GetString(SR.Cannot_init, entry.Name, e.Message), e);
                    }
                }
                else {
                    try {
                        ControlBuilder controlBuilder = entry.Builder;

                        Debug.Assert(controlBuilder.ServiceProvider == null);
                        object objectValue = null;
                        controlBuilder.SetServiceProvider(ServiceProvider);
                        try {
                            objectValue = controlBuilder.BuildObject(flags[applyTheme]);
                        }
                        finally {
                            controlBuilder.SetServiceProvider(null);
                        }

                        // Use the FastPropertyAccessor to assign the value
                        FastPropertyAccessor.SetProperty(obj, entry.Name, objectValue, InDesigner);
                    }
                    catch (Exception e) {
                        throw new HttpException(SR.GetString(SR.Cannot_set_property, TagName, entry.Name), e);
                    }
                }
            }
        }

        private void InitBoundProperties(object obj) {
            // Don't do anything if there are no entries
            if (_boundPropertyEntries == null)
                return;

            DataBindingCollection dataBindings = null;
            IAttributeAccessor attributeAccessor = null;

            // If there are no filters in the picture, use the entries as is
            ICollection entries;

            if (flags[hasFilteredBoundProps])
                entries = GetFilteredPropertyEntrySet(BoundPropertyEntries);
            else
                entries = BoundPropertyEntries;

            foreach (BoundPropertyEntry entry in entries) {

                if (entry.TwoWayBound && this is BindableTemplateBuilder) {
                    if (InDesigner) {
                        // Skip two-way entries for BindableTemplateBuilders in designer
                        continue;
                    }
                }

                InitBoundProperty(obj, entry, ref dataBindings, ref attributeAccessor);
            }
        }

        private void InitBoundProperty(object obj, BoundPropertyEntry entry,
            ref DataBindingCollection dataBindings, ref IAttributeAccessor attributeAccessor) {

            string expressionPrefix = entry.ExpressionPrefix == null ? String.Empty : entry.ExpressionPrefix.Trim();
            // If we're in the designer, add the bound properties to the collections
            if (InDesigner) {
                if (String.IsNullOrEmpty(expressionPrefix)) {
                    if (dataBindings == null && obj is IDataBindingsAccessor) {
                        dataBindings = ((IDataBindingsAccessor)obj).DataBindings;
                    }

                    dataBindings.Add(new DataBinding(entry.Name, entry.Type, entry.Expression.Trim()));
                }
                else {
                    if (obj is IExpressionsAccessor) {
                        string expression = entry.Expression == null ? String.Empty : entry.Expression.Trim();
                        ((IExpressionsAccessor)obj).Expressions.Add(new ExpressionBinding(entry.Name, entry.Type, expressionPrefix, expression, entry.Generated, entry.ParsedExpressionData));
                    }
                }
            }
            // If we're in no-compile mode, set the values for expressions that support evaluate
            else {
                if (!String.IsNullOrEmpty(expressionPrefix)) {
                    ExpressionBuilder eb = entry.ExpressionBuilder;
                    Debug.Assert(eb != null, "Did not expect null expression builder");
                    if (eb.SupportsEvaluate) {
                        string name = entry.Name;

                        // DevDiv Bugs 160497: Create the expression context with whatever information we have.
                        // We used to always use the TemplateControl one, but sometimes it's null, so we should
                        // fall back to the VirtualPath one if we can.
                        ExpressionBuilderContext expressionContext;
                        if (TemplateControl != null) {
                            expressionContext = new ExpressionBuilderContext(TemplateControl);
                        }
                        else {
                            expressionContext = new ExpressionBuilderContext(VirtualPath);
                        }
                        object value = eb.EvaluateExpression(obj, entry,
                            entry.ParsedExpressionData, expressionContext);

                        if (entry.UseSetAttribute) {
                            if (attributeAccessor == null) {
                                Debug.Assert(obj is IAttributeAccessor);
                                attributeAccessor = (IAttributeAccessor)obj;
                            }

                            attributeAccessor.SetAttribute(name, value.ToString());
                        }
                        else {
                            try {
                                PropertyMapper.SetMappedPropertyValue(obj, name, value, InDesigner);
                            }
                            catch (Exception e) {
                                throw new HttpException(SR.GetString(SR.Cannot_set_property, entry.ExpressionPrefix + ":" + entry.Expression, name), e);
                            }
                        }
                    }
                    else {
                        Debug.Fail("Got a ExpressionBuilder that does not support Evaluate in a non-compiled page");
                    }
                }
                else {
                    // no-compile Bind property handling
                    ((Control)obj).DataBinding += new EventHandler(DataBindingMethod);
                }
            }
        }

        private void DataBindingMethod(object sender, EventArgs e) {
            /*System.Web.UI.WebControls.DropDownList dataBindingExpressionBuilderTarget;
            dataBindingExpressionBuilderTarget = ((System.Web.UI.WebControls.DropDownList)(sender));
            System.Web.UI.IDataItemContainer Container;
            Container = ((System.Web.UI.IDataItemContainer)(dataBindingExpressionBuilderTarget.BindingContainer));
            if ((this.Page.GetDataItem() != null)) {
                dataBindingExpressionBuilderTarget.SelectedValue = System.Convert.ToString(this.Eval("FavVegetable"));
            }*/

            bool isBindableTemplateBuilder = this is BindableTemplateBuilder;
            bool isTemplateBuilder = this is TemplateBuilder;
            bool firstEntry = true;
            object evalValue;
            Control containerControl = null;

            ICollection entries;

            // If there are no filters in the picture, use the entries as is
            if (!flags[hasFilteredBoundProps]) {
                entries = BoundPropertyEntries;
            }
            else {
                Debug.Assert(ServiceProvider == null);
                Debug.Assert(TemplateControl != null, "TemplateControl should not be null in no-compile pages. We need it for the FilterResolutionService.");

                ServiceContainer container = new ServiceContainer();
                container.AddService(typeof(IFilterResolutionService), TemplateControl);

                try {
                    SetServiceProvider(container);
                    entries = GetFilteredPropertyEntrySet(BoundPropertyEntries);
                }
                finally {
                    SetServiceProvider(null);
                }
            }            
                
            foreach (BoundPropertyEntry entry in entries) {
                // Skip all one-way entries.  No-compile supported only on Bind statements.
                // Skip two-way entries if it's a BindableTemplateBuilder or the two way entry is read only
                if ((entry.TwoWayBound && (isBindableTemplateBuilder || entry.ReadOnlyProperty))
                    || (!entry.TwoWayBound && isTemplateBuilder))
                    continue;

                // We only care about databinding entries here
                if (!entry.IsDataBindingEntry)
                    continue;

                Debug.Assert(!entry.UseSetAttribute, "Two-way binding is not supported on expandos - this should have been prevented in ControlBuilder");

                if (firstEntry) {
                    firstEntry = false;

                    Debug.Assert(entry.ControlType.IsInstanceOfType(sender), "The DataBinding event sender was not of type " + entry.ControlType.Name);
                    if (_bindingContainerDescriptor == null) {
                        _bindingContainerDescriptor = TargetFrameworkUtil.GetProperties(typeof(Control))["BindingContainer"];
                    }
                    object container = _bindingContainerDescriptor.GetValue(sender);
                    containerControl = container as Control;
                    if (containerControl.Page.GetDataItem() == null) {
                        break; // nothing to do if GetDataItem is null
                    }
                }

                evalValue = containerControl.TemplateControl.Eval(entry.FieldName, entry.FormatString);

                string objectModelName;
                MemberInfo memberInfo = PropertyMapper.GetMemberInfo(entry.ControlType, entry.Name, out objectModelName);                        
                // If destination is property:
                //     If destination type is string:
                //         {{target}}.{{targetPropertyName}} = System.Convert.ToString( {{value}} );
                //     Else If destination type is reference type:
                //         {{target}}.{{targetPropertyName}} = ( {{destinationType}} ) {{value}};
                //     Else destination type is value type:
                //         {{target}}.{{targetPropertyName}} = ( {{destinationType}} ) ({value});
                if (entry.Type.IsValueType && evalValue == null) {
                    continue;
                }

                object convertedValue = evalValue;
                if (entry.Type == typeof(string)) {
                    convertedValue = System.Convert.ToString(evalValue, CultureInfo.CurrentCulture);
                }
                else if (evalValue != null && !entry.Type.IsAssignableFrom(evalValue.GetType())) {
                    convertedValue = PropertyConverter.ObjectFromString(entry.Type, memberInfo, System.Convert.ToString(evalValue, CultureInfo.CurrentCulture));
                }

                PropertyMapper.SetMappedPropertyValue(sender, objectModelName, convertedValue, InDesigner);
            }
        }
        
        private void InitTemplateProperties(object obj) {
            // Don't do anything if there are no entries
            if (_templatePropertyEntries == null)
                return;

            object[] parameters = new object[1];

            // If there are no filters in the picture, use the entries as is
            ICollection entries;

            if (flags[hasFilteredTemplateProps])
                entries = GetFilteredPropertyEntrySet(TemplatePropertyEntries);
            else
                entries = TemplatePropertyEntries;

            foreach (TemplatePropertyEntry entry in entries) {
                try {
                    ControlBuilder controlBuilder = ((TemplatePropertyEntry)entry).Builder;

                    Debug.Assert(controlBuilder.ServiceProvider == null);
                    controlBuilder.SetServiceProvider(ServiceProvider);
                    try {
                        parameters[0] = controlBuilder.BuildObject(flags[applyTheme]);
                    }
                    finally {
                        controlBuilder.SetServiceProvider(null);
                    }

                    MethodInfo methodInfo = entry.PropertyInfo.GetSetMethod();

                    Debug.Assert(methodInfo != null);
                    Util.InvokeMethod(methodInfo, obj, parameters);
                }
                catch (Exception e) {
                    throw new HttpException(SR.GetString(SR.Cannot_set_property, TagName, entry.Name), e);
                }
            }
        }

        // If the page has a field which name matches the ID of this control,
        // assign the control to the field.  This matches what we do for compiled
        // pages (VSWhidbey 252411)
        private void BindFieldToControl(Control control) {

            // If we tried before and did not find a field, don't try again
            if (flags[triedFieldToControlBinding] && !flags[hasFieldToControlBinding])
                return;

            flags[triedFieldToControlBinding] = true;

            TemplateControl templateControl = TemplateControl;
            if (templateControl == null)
                return;

            Type templateControlType = TemplateControl.GetType();

            // This logic only needs to be checked once
            if (!flags[hasFieldToControlBinding]) {
                // This doesn't apply to designer scenarios.  It's only for no-compile pages.
                if (InDesigner)
                    return;

                // Nothing to bind if the control doesn't have an ID
                if (control.ID == null)
                    return;

                // If the TemplateControl is a built in class (Page or UserControl),
                // there is no point in looking for fields.
                if (templateControlType.Assembly == typeof(HttpRuntime).Assembly)
                    return;
            }

            // Try to find a field named after the ID in the TemplateControl
            FieldInfo fieldInfo = TargetFrameworkUtil.GetField(templateControl.GetType(), control.ID,
                BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            // If we couldn't find a field or it doesn't qualify, give up
            if (fieldInfo == null || fieldInfo.IsPrivate || !fieldInfo.FieldType.IsAssignableFrom(control.GetType())) {
                return;
            }

            // Everything is in place, so set the field to the control
            fieldInfo.SetValue(templateControl, control);

            // Remember that it was successful so we know we should try again next time
            flags[hasFieldToControlBinding] = true;
        }


        /// <devdoc>
        /// Returns true is it needs SetTagInnerText() to be called.
        /// </devdoc>
        public virtual bool NeedsTagInnerText() {
            return false;
        }


        /// <devdoc>
        /// This method is used to tell the builder that it's about to be appended to its parent.
        /// </devdoc>
        public virtual void OnAppendToParentBuilder(ControlBuilder parentBuilder) {
            // If we have a default property, add it to ourselves
            if (DefaultPropertyBuilder != null) {
                ControlBuilder defaultPropBuilder = DefaultPropertyBuilder;

                // Need to make it null to avoid infinite recursion
                ParseTimeData.DefaultPropertyBuilder = null;
                AppendSubBuilder(defaultPropBuilder);
            }

            if (!(this is BindableTemplateBuilder)) {
                ControlBuilder currentBuilder = this;

                while (currentBuilder != null && !(currentBuilder is BindableTemplateBuilder)) {
                    currentBuilder = currentBuilder.ParentBuilder;
                }

                if (currentBuilder != null && currentBuilder is BindableTemplateBuilder) {

                    // Add all the TwoWay BoundPropertyEntry's to the BindableTemplateBuilder
                    foreach (BoundPropertyEntry entry in BoundPropertyEntries) {
                        if (entry.TwoWayBound) {
                            ((BindableTemplateBuilder)currentBuilder).AddBoundProperty(entry);
                        }
                    }
                }
            }
        }

        /// <devdoc>
        /// Prepares this ControlBuilder and all of it's subbuilders for
        /// use in no-compile pages (minimizing the memory usage).
        /// This makes the use of all parse-time properties invalid.
        /// </devdoc>
        internal virtual void PrepareNoCompilePageSupport() {
            // Clear out all the flags and cached data from parse time
            flags[parseComplete] = true;
            _parseTimeData = null;

            // Remove any property entry lists that aren't being used
            if ((_eventEntries != null) && (_eventEntries.Count == 0)) {
                _eventEntries = null;
            }

            if ((_simplePropertyEntries != null) && (_simplePropertyEntries.Count == 0)) {
                _simplePropertyEntries = null;
            }

            if (_complexPropertyEntries != null) {
                if (_complexPropertyEntries.Count == 0) {
                    _complexPropertyEntries = null;
                }
                else {
                    foreach (BuilderPropertyEntry entry in _complexPropertyEntries) {
                        if (entry.Builder != null)
                            entry.Builder.PrepareNoCompilePageSupport();
                    }
                }
            }

            if (_templatePropertyEntries != null) {
                if (_templatePropertyEntries.Count == 0) {
                    _templatePropertyEntries = null;
                }
                else {
                    foreach (BuilderPropertyEntry entry in _templatePropertyEntries) {
                        if (entry.Builder != null)
                            entry.Builder.PrepareNoCompilePageSupport();
                    }
                }
            }

            if ((_boundPropertyEntries != null) && (_boundPropertyEntries.Count == 0)) {
                _boundPropertyEntries = null;
            }

            if (_subBuilders != null) {
                if (_subBuilders.Count > 0) {
                    foreach (Object builderObj in _subBuilders) {
                        ControlBuilder builder = builderObj as ControlBuilder;

                        if (builder != null)
                            builder.PrepareNoCompilePageSupport();
                    }
                }
                else {
                    _subBuilders = null;
                }
            }

            // Sort the entry here to make sure we don't run into a race condition if we try to
            // do it later on demand (VSWhidbey 551431)
            EnsureEntriesSorted();
        }

        /// <devdoc>
        /// If the control has a Property which matches the name of the
        /// attribute, create an PropertyEntry for it, that will
        /// be used at BuildControl time.
        /// </devdoc>
        internal void PreprocessAttribute(string filter, string attribname, string attribvalue, bool mainDirectiveMode, int line = 0, int column = 0) {
            Match match;

            // Treat a null value as an empty string
            if (attribvalue == null) {
                attribvalue = String.Empty;
            }

            if ((match = databindRegex.Match(attribvalue, 0)).Success) {

                // Don't process databinding expressions during updatable precomp, because we're only
                // generating the base class, and only need the Type and ID of the controls (VSWhidbey 470549)
                if (BuildManager.PrecompilingForUpdatableDeployment)
                    return;

                Group codeGroup = match.Groups["code"];
                // Use it to calculate the column where the code starts,
                // which improves the debugging experience (VSWhidbey 87172)
                column += codeGroup.Index;

                string code = codeGroup.Value;
                bool encode = match.Groups["encode"].Success;

                bool isParsedBindingStatement = false;
                bool isTwoWayBindingStatement = false;
                bool isBindItemStatement = false;
                if (!InDesigner) {
                    if ((match = bindExpressionRegex.Match(code, 0)).Success) {
                        isParsedBindingStatement = true;
                        isTwoWayBindingStatement = true;
                    }
                    else if ((match = bindItemExpressionRegex.Match(code, 0)).Success) {
                        isParsedBindingStatement = true;
                        isTwoWayBindingStatement = true;
                        isBindItemStatement = true;
                    }
                    // Treat it as a binding statement for skin files so the expression
                    // format will be checked first.
                    else if ((CompilationMode == CompilationMode.Never || InPageTheme) && 
                        (match = evalExpressionRegex.Match(code, 0)).Success) {
                        isParsedBindingStatement = true;
                    }
                }

                // Is it a two-way binding statement or eval in no-compile?
                if (isParsedBindingStatement) {
                    string paramString = match.Groups["params"].Value;

                    if (!isBindItemStatement) {
                        if (!(match = bindParametersRegex.Match(paramString, 0)).Success) {
                            throw new HttpException(SR.GetString(SR.BadlyFormattedBind));
                        }
                    }
                    else if (!(match = bindItemParametersRegex.Match(paramString, 0)).Success) {
                        throw new HttpException(SR.GetString(SR.BadlyFormattedBindItem));
                    }

                    string fieldName = match.Groups["fieldName"].Value;
                    string formatString = String.Empty;
                    Group formatStringGroup = match.Groups["formatString"];

                    if (formatStringGroup != null) {
                        formatString = formatStringGroup.Value;
                    }

                    if (formatString.Length > 0) {
                        if (!(match = formatStringRegex.Match(formatString, 0)).Success) {
                            throw new HttpException(SR.GetString(SR.BadlyFormattedBind));
                        }
                    }

                    // Pass the code expression to AddBoundProperty since the Eval expression needs to be compiled in skin files.
                    // The bind expression needs to call without code
                    if (InPageTheme && !isTwoWayBindingStatement) {
                        AddBoundProperty(filter, attribname, String.Empty, code, null /*expressionBuilder*/, null /*parsedExpressionData*/, String.Empty, String.Empty, false, encode, line, column);
                        return;
                    }

                    AddBoundProperty(filter, attribname, String.Empty, code, null /*expressionBuilder*/, null /*parsedExpressionData*/, fieldName, formatString, isTwoWayBindingStatement, encode, line, column);
                    return;
                }
                else {
                    // First, give the PageParserFilter a chance to handle the databinding
                    if (!Parser.PageParserFilterProcessedDataBindingAttribute(ID, attribname, code)) {
                        // If it's a non compiled page and it's not a Bind or Eval statement, fail
                        Parser.EnsureCodeAllowed();

                        // Get the piece of code and add the property
                        AddBoundProperty(filter, attribname, String.Empty, code, null /*expressionBuilder*/, null /*parsedExpressionData*/, String.Empty, String.Empty, false, encode, line, column);
                    }
                    return;
                }
            }
            else if ((match = expressionBuilderRegex.Match(attribvalue, 0)).Success) {
                if (InPageTheme) {
                    throw new HttpParseException(SR.GetString(SR.ControlBuilder_ExpressionsNotAllowedInThemes));
                }

                // Don't process expression builders during updatable precomp, because we're only
                // generating the base class, and only need the Type and ID of the controls (VSWhidbey 434350)
                if (BuildManager.PrecompilingForUpdatableDeployment)
                    return;
                
                string code = match.Groups["code"].Value.Trim();

                int indexOfColon = code.IndexOf(':');
                if (indexOfColon == -1) {
                    throw new HttpParseException(SR.GetString(SR.InvalidExpressionSyntax, attribvalue));
                }

                string expressionPrefix = code.Substring(0, indexOfColon).Trim();
                string expressionCode = code.Substring(indexOfColon + 1).Trim();
                if (expressionPrefix.Length == 0) {
                    throw new HttpParseException(SR.GetString(SR.MissingExpressionPrefix, attribvalue));
                }
                if (expressionCode.Length == 0) {
                    throw new HttpParseException(SR.GetString(SR.MissingExpressionValue, attribvalue));
                }

                // If it's a non compiled page, fail if the expressiom builder has SupportsEvaluate==false
                ExpressionBuilder expressionBuilder = null;
                if (CompilationMode == CompilationMode.Never) {
                    expressionBuilder = ExpressionBuilder.GetExpressionBuilder(expressionPrefix, Parser.CurrentVirtualPath);
                    if ((expressionBuilder != null) && !expressionBuilder.SupportsEvaluate) {
                        throw new InvalidOperationException(SR.GetString(SR.Cannot_evaluate_expression, expressionPrefix + ":" + expressionCode));
                    }
                }

                AddBoundProperty(filter, attribname, expressionPrefix, expressionCode, expressionBuilder, null /*parsedExpressionData*/, String.Empty, String.Empty, false, encode : false );
                return;
            }

            AddProperty(filter, attribname, attribvalue, mainDirectiveMode);
        }

        /// <devdoc>
        /// Indicates whether this ControlBuilder should allow meta:localize and meta:resourcekey
        /// attributes on the tag. We only allow this for tags that represent controls and tags
        /// that represent collection items.
        /// </devdoc>
        private bool IsValidForImplicitLocalization() {
            if (flags[controlTypeIsControl]) {
                // If this is a control, we can localize
                return true;
            }

            if (ParentBuilder == null) {
                // We must have a parent builder
                return false;
            }

            // If we have a parent builder, check that we are a collection item either through our
            // immediate parent, or our parent's default property builder, which is our effective
            // parent.
            if (ParentBuilder.DefaultPropertyBuilder != null) {
                return typeof(ICollection).IsAssignableFrom(ParentBuilder.DefaultPropertyBuilder.ControlType);
            }
            else {
                return typeof(ICollection).IsAssignableFrom(ParentBuilder.ControlType);
            }
        }

        /// <devdoc>
        /// Process implicit resources if the control has a meta:resourcekey attribute
        /// </devdoc>
        internal void ProcessImplicitResources(ParsedAttributeCollection attribs) {

            // Check if meta:localize="false" was specified.  Always do this since we need it at design-time
            string localize = (string)((IDictionary)attribs)["meta:localize"];
            if (localize != null) {
                // Depending on the control type, don't allow meta:localize (e.g. ITemplate case) (VSWhidbey 276398, 454894)
                if (!IsValidForImplicitLocalization()) {
                    throw new InvalidOperationException(SR.GetString(SR.meta_localize_notallowed, TagName));
                }

                bool parseResult;
                if (!Boolean.TryParse(localize, out parseResult)) {
                    throw new HttpException(SR.GetString(SR.ControlBuilder_InvalidLocalizeValue, localize));
                }
                ParseTimeData.Localize = parseResult;
            }
            else {
                ParseTimeData.Localize = true;
            }

            // Check whether a resource key was specified
            string keyPrefix = (string) ((IDictionary)attribs)["meta:resourcekey"];

            // Remove all meta attributes from the collection (VSWhidbey 230192)
            attribs.ClearFilter("meta");

            if (keyPrefix == null)
                return;

            // Depending on the control type, don't allow meta:reskey (e.g. ITemplate case) (VSWhidbey 276398, 454894)
            if (!IsValidForImplicitLocalization()) {
                throw new InvalidOperationException(SR.GetString(SR.meta_reskey_notallowed, TagName));
            }
            Debug.Assert(_controlType != null, "If we get here then the tag type must be either an ICollection or a Control, so how can it be null?");

            // Restrict resource keys the same way as we restrict ID's (VSWhidbey 256438)
            if (!System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(keyPrefix)) {
                throw new HttpException(SR.GetString(SR.Invalid_resourcekey, keyPrefix));
            }

            if (!ParseTimeData.Localize) {
                // If we have a key prefix (from meta:resourcekey) but we also have
                // meta:localize=false, we throw.
                throw new HttpException(SR.GetString(SR.meta_localize_error));
            }

            ParseTimeData.ResourceKeyPrefix = keyPrefix;

            // Try to get the implicit resources for this specific Page
            IImplicitResourceProvider implicitResourceProvider;
            if (Parser.FInDesigner && Parser.DesignerHost != null) {
                implicitResourceProvider = (IImplicitResourceProvider)Parser.DesignerHost.GetService(typeof(IImplicitResourceProvider));
            }
            else {
                implicitResourceProvider = Parser.GetImplicitResourceProvider();
            }

            // If the Page has resources, get the specific ones for this meta:resourcekey
            ICollection tagResources = null;
            if (implicitResourceProvider != null)
                tagResources = implicitResourceProvider.GetImplicitResourceKeys(keyPrefix);

            if (tagResources != null) {
                // Get the IDesignerHost in case we need it to find ExpressionBuilders
                IDesignerHost host = DesignerHost;

                // Note: this code expect that the "resources" expression builder be
                // registered in config.  If the user removes it, they will get an error.
                ExpressionBuilder resourcesExpressionBuilder = ExpressionBuilder.GetExpressionBuilder("resources", Parser.CurrentVirtualPath, host);
                bool usingStandardResources = typeof(ResourceExpressionBuilder) == resourcesExpressionBuilder.GetType();
                foreach (ImplicitResourceKey entry in tagResources) {

                    // Put together the complete resource key, as would appear in an explicit resource
                    string fullResourceKey = keyPrefix + "." + entry.Property;
                    if (entry.Filter.Length > 0)
                        fullResourceKey = entry.Filter + ':' + fullResourceKey;

                    // Replace '.' with '-', since that's what AddBoundProperty expects
                    string property = entry.Property.Replace('.', '-');

                    object parsedExpressionData = null;
                    string expression;
                    if (usingStandardResources) {
                        // If we're using the standard System.Web.Compilation.ResourceExpressionBuilder
                        // we can optimized the parsed data.
                        parsedExpressionData = ResourceExpressionBuilder.ParseExpression(fullResourceKey);
                        expression = String.Empty;
                    }
                    else {
                        expression = fullResourceKey;
                    }

                    AddBoundProperty(entry.Filter, property, "resources",
                        expression, resourcesExpressionBuilder, parsedExpressionData, true, String.Empty, String.Empty, false, encode:false);
                }
            }
        }

        /// <devdoc>
        /// Preprocess all the attributes at parse time, so that we'll be left
        /// with as little work as possible when we build the control.
        /// </devdoc>
        private void PreprocessAttributes(ParsedAttributeCollection attribs) {

            ProcessImplicitResources(attribs);

            //Since the attribute column values are only used for generating line pragmas at design time for intellisense to work,
            //we populate that only at design time so that runtime memory usage is not affected.
            bool isDesignerMode = BuildManagerHost.InClientBuildManager;
            IDictionary<String, Pair> attributeValuePositions = null;

            if (isDesignerMode) {
                //This dictionary indicates the column values at which the attribute value expressions begin and 
                //that is used for generating line pragmas at design time for intellisense.
                attributeValuePositions = attribs.AttributeValuePositionsDictionary;
            }

            // Preprocess all the attributes
            foreach (FilteredAttributeDictionary filteredAttributes in attribs.GetFilteredAttributeDictionaries()) {
                string filter = filteredAttributes.Filter;

                foreach (DictionaryEntry attribute in filteredAttributes) {
                    string name = attribute.Key.ToString();
                    string value = attribute.Value.ToString();
                    int column = 0; 
                    int line = 0;
                    if (isDesignerMode && attributeValuePositions.ContainsKey(name)) {
                        line = (int)attributeValuePositions[name].First;
                        column = (int)attributeValuePositions[name].Second;
                    }

                    PreprocessAttribute(filter, name, value, false, line, column);
                }
            }
        }

        // 
        public /* internal */ void SetServiceProvider(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
        }

        internal void EnsureEntriesSorted() {
            // Always perform the sorting only once, even in derived classes, 
            // so as to avoid concurrency issues (DevDiv bugs 203787).
            if (!flags[entriesSorted]) {
                flags[entriesSorted] = true;
                SortEntries();
            }
        }

        internal virtual void SortEntries() {
            // Don't sort the entries in a collection builder
            if (this is CollectionBuilder) {
                return;
            }

            FilteredPropertyEntryComparer comparer = null;

            ProcessAndSortPropertyEntries(_boundPropertyEntries, ref comparer);
            ProcessAndSortPropertyEntries(_complexPropertyEntries, ref comparer);
            ProcessAndSortPropertyEntries(_simplePropertyEntries, ref comparer);
            ProcessAndSortPropertyEntries(_templatePropertyEntries, ref comparer);
        }
        

        internal void ProcessAndSortPropertyEntries(ArrayList propertyEntries, 
            ref FilteredPropertyEntryComparer comparer) {

            if (propertyEntries != null && propertyEntries.Count > 1) {
                HybridDictionary dictionary = new HybridDictionary(propertyEntries.Count, true);
                int index = 0;

                // Determine the order of the entry based on location of the first entry with the same name
                foreach (PropertyEntry entry in propertyEntries) {
                    object o = dictionary[entry.Name];
                    if (o != null) {
                        entry.Order = (int)o;
                    }
                    else {
                        entry.Order = index;
                        dictionary.Add(entry.Name, index++);
                    }
                }

                if (comparer == null) {
                    comparer = new FilteredPropertyEntryComparer(CurrentFilterResolutionService);
                }
                propertyEntries.Sort(comparer);
            }
        }

        /// <devdoc>
        ///
        /// </devdoc>
        internal void SetControlType(Type controlType) {
            _controlType = controlType;
            if (_controlType != null) {
                flags[controlTypeIsControl] = typeof(Control).IsAssignableFrom(_controlType);
            }
            else {
                flags[controlTypeIsControl] = false;
            }
        }

        /// <devdoc>
        /// Set the ControlBuilder that's the parent of this ControlBuilder
        /// </devdoc>
        internal virtual void SetParentBuilder(ControlBuilder parentBuilder) {
            ParseTimeData.ParentBuilder = parentBuilder;
            if ((ParseTimeData.FirstNonThemableProperty != null) && (parentBuilder is FileLevelPageThemeBuilder)) {
                throw new InvalidOperationException(SR.GetString(SR.Property_theme_disabled, ParseTimeData.FirstNonThemableProperty.Name, ControlType.FullName));
            }
        }

        // 
        public string GetResourceKey() {

            // This should only be used in the designer
            Debug.Assert(InDesigner);

            return ParseTimeData.ResourceKeyPrefix;
        }

        // 
        public void SetResourceKey(string resourceKey) {

            // This should only be used in the designer
            Debug.Assert(InDesigner);

            SimplePropertyEntry entry = new SimplePropertyEntry();
            entry.Filter = "meta";
            entry.Name = "resourcekey";
            entry.Value = resourceKey;
            entry.PersistedValue = resourceKey;
            entry.UseSetAttribute = true;
            entry.Type = typeof(string);

            AddEntry(SimplePropertyEntriesInternal, entry);
        }


        /// <devdoc>
        ///  Give the builder the raw inner text of the tag.
        /// </devdoc>
        public virtual void SetTagInnerText(string text) {
        }

        /// <devdoc>
        ///  Give the ControlBuilder a chance to look at and modify the tree
        /// </devdoc>
        public virtual void ProcessGeneratedCode(
            CodeCompileUnit codeCompileUnit,
            CodeTypeDeclaration baseType,
            CodeTypeDeclaration derivedType,
            CodeMemberMethod buildMethod,
            CodeMemberMethod dataBindingMethod) { }

        /// <devdoc>
        /// Make sure the given property with the specified context (using SetAttribute to set the value or a directive property) is persistable
        /// throwing otherwise
        /// </devdoc>
        private void ValidatePersistable(PropertyInfo propInfo, bool usingSetAttribute,
            bool mainDirectiveMode, bool simplePropertyEntry, string filter) {

            // Get the appropriate PropertyDescriptorCollection.  If it's for our own type, just
            // call our PropertyDescriptors property, which caches it.  Otherwise (for sub properties)
            // get it directly without caching (less common case).
            PropertyDescriptorCollection propertyDescriptors;

            // Use the current control type if it derives from propInfo.DeclaringType
            bool useCurrentControlType = propInfo.DeclaringType.IsAssignableFrom(_controlType);

            if (useCurrentControlType) {
                propertyDescriptors = PropertyDescriptors;
            }
            else {
                // See comments below regarding when we check sub-properties for validity.
                propertyDescriptors = TargetFrameworkUtil.GetProperties(propInfo.DeclaringType);
            }

            PropertyDescriptor propDesc = propertyDescriptors[propInfo.Name];

            if (propDesc != null) {
                if (useCurrentControlType) {
                    // These checks are only done for top-level properties (e.g. Text="hello").
                    // We don't do it for sub-properties (e.g. Font-Name="Arial") since it would
                    // break backwards compatibility with v1.1 (where we did not do these checks).

                    // If it's an HtmlControl, check the HtmlControlPersistableAttribute to see if the property is persistable
                    if (IsHtmlControl) {
                        if (propDesc.Attributes.Contains(HtmlControlPersistableAttribute.No)) {
                            throw new HttpException(SR.GetString(SR.Property_Not_Persistable, propDesc.Name));
                        }
                    }
                    // Otherwise, if we're not using the attribute accessor, we're not processing the main directive
                    // check if the property is persistable
                    else if (!usingSetAttribute && !mainDirectiveMode && propDesc.Attributes.Contains(DesignerSerializationVisibilityAttribute.Hidden)) {
                        throw new HttpException(SR.GetString(SR.Property_Not_Persistable, propDesc.Name));
                    }
                }

                // These checks are done for both top-level properties, as well as sub-properties.
                // Backwards compatibility is satisfied since the Filterable() and Themeable()
                // attributes are new in v2.0.

                // Make sure the property is filterable if there is a filter
                if (!FilterableAttribute.IsPropertyFilterable(propDesc) && !String.IsNullOrEmpty(filter)) {
                    throw new InvalidOperationException(SR.GetString(SR.Illegal_Device, propDesc.Name));
                }

                if (InPageTheme && (ParseTimeData.FirstNonThemableProperty == null)) {
                    // For simple properties, don't validate if it's a customAttribute
                    if (!simplePropertyEntry || !usingSetAttribute) {
                        ThemeableAttribute attr = (ThemeableAttribute)propDesc.Attributes[typeof(ThemeableAttribute)];
                        if (attr != null && !attr.Themeable) {
                            if (this.ParentBuilder != null) {
                                if (ParentBuilder is FileLevelPageThemeBuilder) {
                                    throw new InvalidOperationException(SR.GetString(SR.Property_theme_disabled, propDesc.Name, ControlType.FullName));
                                }
                            }
                            else {
                                ParseTimeData.FirstNonThemableProperty = propDesc;
                            }
                        }
                    }
                }
            }
        }

        // Default factory used create base ControlBuilder objects
        private static IWebObjectFactory s_defaultControlBuilderFactory = new DefaultControlBuilderFactory();

        private class DefaultControlBuilderFactory : IWebObjectFactory {
            object IWebObjectFactory.CreateInstance() {
                return new ControlBuilder();
            }
        }

        // Factories used when we cannot generate a fast factory (e.g. because the ControlBuilder
        // type is internal).
        private class ReflectionBasedControlBuilderFactory : IWebObjectFactory {
            private Type _builderType;

            internal ReflectionBasedControlBuilderFactory(Type builderType) {
                _builderType = builderType;
            }

            object IWebObjectFactory.CreateInstance() {
                return (ControlBuilder)HttpRuntime.CreateNonPublicInstance(_builderType);
            }
        }

        /// <devdoc>
        /// Space-saving class used to store variables used only during parse and codegen time.
        /// All these are cleared when the ControlBuilder is used in a no-compile page.
        /// </devdoc>
        private sealed class ControlBuilderParseTimeData {

            // const masks into the BitVector32
            private const int childrenAsProperties = 0x00000001;
            private const int hasAspCode = 0x00000002;
            private const int isHtmlControl = 0x00000004;
            private const int isNonParserAccessor = 0x00000008;
            private const int namingContainerSearched = 0x00000010;
            private const int supportsAttributes = 0x00000020;
            private const int isGeneratedID = 0x00000040;
            private const int localize = 0x00000080;
            private const int ignoreControlProperties = 0x00000100;
            #pragma warning disable 0649
            private SimpleBitVector32 flags;
            #pragma warning restore 0649

            internal bool ChildrenAsProperties {
                get { return flags[childrenAsProperties]; }
                set { flags[childrenAsProperties] = value; }
            }

            internal ControlBuilder DefaultPropertyBuilder;

            internal EventDescriptorCollection EventDescriptors;

            internal string Filter;

            internal bool HasAspCode {
                get { return flags[hasAspCode]; }
                set { flags[hasAspCode] = value; }
            }

            internal bool IsHtmlControl {
                get { return flags[isHtmlControl]; }
                set { flags[isHtmlControl] = value; }
            }

            internal bool IgnoreControlProperties {
                get { return flags[ignoreControlProperties]; }
                set { flags[ignoreControlProperties] = value; }
            }

            internal bool IsNonParserAccessor {
                get { return flags[isNonParserAccessor]; }
                set { flags[isNonParserAccessor] = value; }
            }

            internal bool IsGeneratedID {
                get { return flags[isGeneratedID]; }
                set { flags[isGeneratedID] = value; }
            }

            internal string ID;

            internal int Line;

            internal bool Localize {
                get { return flags[localize]; }
                set { flags[localize] = value; }
            }

            internal bool NamingContainerSearched {
                get { return flags[namingContainerSearched]; }
                set { flags[namingContainerSearched] = value; }
            }

            internal ControlBuilder NamingContainerBuilder;

            internal ControlBuilder ParentBuilder;

            internal TemplateParser Parser;

            internal PropertyDescriptorCollection PropertyDescriptors;

            internal StringSet PropertyEntries;

            internal bool SupportsAttributes {
                get { return flags[supportsAttributes]; }
                set { flags[supportsAttributes] = value; }
            }

            internal VirtualPath VirtualPath;

            internal PropertyDescriptor FirstNonThemableProperty;

            internal string ResourceKeyPrefix;
        }

        internal sealed class FilteredPropertyEntryComparer : IComparer {
            IFilterResolutionService _filterResolutionService;

            public FilteredPropertyEntryComparer(IFilterResolutionService filterResolutionService) {
                _filterResolutionService = filterResolutionService;
            }

            int IComparer.Compare(object o1, object o2) {
                if (o1 == o2) {
                    return 0;
                }

                if (o1 == null) {
                    return 1;
                }

                if (o2 == null) {
                    return -1;
                }

                Debug.Assert(o1 is PropertyEntry);
                Debug.Assert(o2 is PropertyEntry);

                PropertyEntry entry1 = (PropertyEntry)o1;
                PropertyEntry entry2 = (PropertyEntry)o2;

                // Compare the order of the item to make the sorting stable.
                int compareValue = entry1.Order - entry2.Order;

                if (compareValue == 0) {
                    if (_filterResolutionService == null) {
                        if (String.IsNullOrEmpty(entry1.Filter)) {
                            if ((entry2.Filter != null) && (entry2.Filter.Length > 0)) {
                                compareValue = 1;
                            }
                            else {
                                compareValue = 0;
                            }
                        }
                        else {
                            if (String.IsNullOrEmpty(entry2.Filter)) {
                                compareValue = -1;
                            }
                            else {
                                compareValue = 0;
                            }
                        }
                    }
                    else {
                        string filter1 = (entry1.Filter.Length == 0) ? "Default" : entry1.Filter;
                        string filter2 = (entry2.Filter.Length == 0) ? "Default" : entry2.Filter;

                        compareValue = _filterResolutionService.CompareFilters(filter1, filter2);
                    }

                    // Compare the index of the item in the array to make the sorting stable.
                    if (compareValue == 0) {
                        return entry1.Index - entry2.Index;
                    }
                }

                return compareValue;
            }
        }
    }
}
