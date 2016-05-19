namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Text;
    using System.Drawing;
    using System.Resources;
    using System.Reflection;
    using System.Diagnostics;
    using System.Collections;
    using System.Drawing.Text;
    using System.Globalization;
    using System.Windows.Forms;
    using System.Drawing.Design;
    using System.ComponentModel;
    using System.Drawing.Drawing2D;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Design;
    using System.Collections.Specialized;
    using System.Runtime.InteropServices;
    using System.ComponentModel.Design.Serialization;
    using Microsoft.Win32;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;
    using System.Windows.Forms.Design;

    //



    #region WorkflowTheme Enums
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum ThemeType
    {
        Default = 0,
        System = 1,
        UserDefined = 2
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum DesignerGeometry
    {
        Rectangle = 0,
        RoundedRectangle = 1
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum TextQuality
    {
        Aliased = 0,
        AntiAliased = 1,
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum DesignerSize
    {
        //Please note that this enum is used to access array and hence we need to 
        //change all the arrays before changing this enum
        Small = 0,
        Medium = 1,
        Large = 2
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum DesignerContentAlignment
    {
        Left = 1,
        Top = 2,
        Right = 4,
        Bottom = 8,
        Center = 16,
        TopLeft = Left + Top,
        TopCenter = Center + Top,
        TopRight = Right + Top,
        CenterLeft = Left + Center,
        CenterRight = Right + Center,
        BottomLeft = Left + Bottom,
        BottomCenter = Center + Bottom,
        BottomRight = Right + Bottom,
        Fill = 32
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum LineAnchor
    {
        None = 0,
        Arrow = 1,
        ArrowAnchor = 2,
        Diamond = 3,
        DiamondAnchor = 4,
        Round = 5,
        RoundAnchor = 6,
        Rectangle = 7,
        RectangleAnchor = 8,
        RoundedRectangle = 9,
        RoundedRectangleAnchor = 10
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum AmbientProperty
    {
        DesignerSize = 0,
        OperatingSystemSetting = 1
    }
    #endregion

    #region Class ActivityDesignerThemeAttribute
    [AttributeUsage(AttributeTargets.Class)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ActivityDesignerThemeAttribute : Attribute
    {
        private Type designerThemeType = null;
        private string xml = String.Empty;

        public ActivityDesignerThemeAttribute(Type designerThemeType)
        {
            this.designerThemeType = designerThemeType;
        }

        public Type DesignerThemeType
        {
            get
            {
                return this.designerThemeType;
            }
        }

        public string Xml
        {
            get
            {
                return this.xml;
            }

            set
            {
                this.xml = value;
            }
        }
    }
    #endregion

    #region Class WorkflowTheme
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowTheme : IDisposable
    {
        #region Static Members
        private static readonly string WorkflowThemesSubKey = "Themes";
        private const string ThemeTypeKey = "ThemeType";
        private const string ThemePathKey = "ThemeFilePath";
        private const string ThemeResourceNS = "System.Workflow.ComponentModel.Design.ActivityDesignerThemes.";
        internal const string DefaultThemeFileExtension = "*.wtm";
        internal static string DefaultNamespace = typeof(WorkflowTheme).Namespace.Replace(".", "_");
        private static IUIService uiService = null; //cached session-wide ui service (for getting environment font)
        private static Font defaultFont = null;
        #endregion

        #region Members and Initialization
        private static WorkflowTheme currentTheme = null;
        private static bool enableChangeNotification = true;
        public static event System.EventHandler ThemeChanged;

        private ThemeType themeType = ThemeType.UserDefined;
        private string name = String.Empty;
        private string version = "1.0";
        private string description = DR.GetString(DR.DefaultThemeDescription);
        private string filePath = String.Empty;
        private ThemeCollection designerThemes = new ThemeCollection();
        private bool readOnly = false;

        static WorkflowTheme()
        {
            WorkflowTheme.currentTheme = LoadThemeSettingFromRegistry();
            if (WorkflowTheme.currentTheme != null)
                WorkflowTheme.currentTheme.ReadOnly = true;
        }

        public WorkflowTheme()
        {
            this.filePath = WorkflowTheme.GenerateThemeFilePath();
            if (this.filePath != null && this.filePath.Length > 0)
                this.name = Path.GetFileNameWithoutExtension(this.filePath);
        }

        ~WorkflowTheme()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            //Dispose all the members first before doing anything with the resources
            foreach (DesignerTheme designerTheme in this.designerThemes)
                ((IDisposable)designerTheme).Dispose();
            this.designerThemes.Clear();
        }
        #endregion

        #region Static Properties and Methods
        internal static IUIService UIService
        {
            get
            {
                return WorkflowTheme.uiService;
            }
            set
            {
                WorkflowTheme.uiService = value;
                WorkflowTheme.defaultFont = null; //clear cached font
                WorkflowTheme.CurrentTheme.AmbientTheme.UpdateFont();
            }
        }

        internal static Font GetDefaultFont()
        {
            if (WorkflowTheme.defaultFont == null)
            {
                if (WorkflowTheme.UIService != null)
                    WorkflowTheme.defaultFont = WorkflowTheme.UIService.Styles["DialogFont"] as Font;

                if (WorkflowTheme.defaultFont == null)
                    WorkflowTheme.defaultFont = Control.DefaultFont;
            }

            return defaultFont;
        }

        public static string RegistryKeyPath
        {
            get
            {
                return DesignerHelpers.DesignerPerUserRegistryKey + "\\" + WorkflowThemesSubKey;
            }
        }

        public static WorkflowTheme CurrentTheme
        {
            get
            {
                if (WorkflowTheme.currentTheme == null)
                {
                    WorkflowTheme.currentTheme = CreateStandardTheme(ThemeType.Default);
                    WorkflowTheme.currentTheme.ReadOnly = true;
                }

                return WorkflowTheme.currentTheme;
            }

            set
            {
                if (WorkflowTheme.currentTheme == value)
                    return;

                if (value == null)
                    throw new ArgumentNullException("value");

                WorkflowTheme oldTheme = WorkflowTheme.currentTheme;

                WorkflowTheme.currentTheme = value;
                WorkflowTheme.currentTheme.ReadOnly = true;

                if (WorkflowTheme.EnableChangeNotification)
                {
                    //We dont want to dispose the standard themes here
                    if (oldTheme != null)
                    {
                        ((IDisposable)oldTheme).Dispose();
                        oldTheme = null;
                    }

                    FireThemeChange();
                }
            }
        }

        public static bool EnableChangeNotification
        {
            get
            {
                return WorkflowTheme.enableChangeNotification;
            }

            set
            {
                if (WorkflowTheme.enableChangeNotification == value)
                    return;
                WorkflowTheme.enableChangeNotification = value;
            }
        }

        public static string LookupPath
        {
            get
            {
                string path = string.Empty;
                try
                {
                    path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    if (String.IsNullOrEmpty(path))
                    {
                        Debug.Assert(false, "Install directory key is missing!");
                        path = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
                    }

                    path = Path.Combine(path, "Windows Workflow Foundation" + Path.DirectorySeparatorChar + "Themes");
                    path += Path.DirectorySeparatorChar;
                }
                catch
                {
                }

                Debug.Assert(path != null && path.Length > 0);
                return path;
            }
        }

        public static string GenerateThemeFilePath()
        {
            string path = LookupPath;
            string tempThemePath = Path.Combine(path, DR.GetString(DR.MyFavoriteTheme) + ".wtm");
            for (int i = 1; File.Exists(tempThemePath); i++)
            {
                tempThemePath = Path.Combine(path, DR.GetString(DR.MyFavoriteTheme) + i.ToString(CultureInfo.InvariantCulture) + ".wtm");
            }
            return tempThemePath;
        }

        public static WorkflowTheme LoadThemeSettingFromRegistry()
        {
            WorkflowTheme loadedTheme = null;

            RegistryKey themeKey = Registry.CurrentUser.OpenSubKey(WorkflowTheme.RegistryKeyPath);
            if (themeKey != null)
            {
                ThemeType themeType = ThemeType.Default;
                try
                {
                    object registryValue = themeKey.GetValue(WorkflowTheme.ThemeTypeKey);
                    if (registryValue is string)
                        themeType = (ThemeType)Enum.Parse(typeof(ThemeType), (string)registryValue, true);

                    if (themeType == ThemeType.UserDefined)
                    {
                        registryValue = themeKey.GetValue(WorkflowTheme.ThemePathKey);
                        string themePath = (registryValue is string) ? (string)registryValue : String.Empty;
                        if (File.Exists(themePath))
                        {
                            string extension = Path.GetExtension(themePath);
                            if (extension.Equals(WorkflowTheme.DefaultThemeFileExtension.Replace("*", ""), StringComparison.Ordinal))
                                loadedTheme = WorkflowTheme.Load(themePath);
                        }
                    }
                }
                catch
                {
                    //We eat the exception
                }
                finally
                {
                    if (loadedTheme == null)
                    {
                        if (themeType == ThemeType.UserDefined)
                            themeType = ThemeType.Default;
                        loadedTheme = CreateStandardTheme(themeType);
                    }

                    themeKey.Close();
                }
            }

            return loadedTheme;
        }

        public static void SaveThemeSettingToRegistry()
        {
            RegistryKey themeKey = Registry.CurrentUser.CreateSubKey(WorkflowTheme.RegistryKeyPath);
            if (themeKey != null)
            {
                try
                {
                    themeKey.SetValue(WorkflowTheme.ThemeTypeKey, WorkflowTheme.CurrentTheme.themeType);
                    if (WorkflowTheme.CurrentTheme.themeType == ThemeType.UserDefined)
                        themeKey.SetValue(WorkflowTheme.ThemePathKey, WorkflowTheme.CurrentTheme.FilePath);
                    else
                        themeKey.SetValue(WorkflowTheme.ThemePathKey, String.Empty);
                }
                catch
                {
                    //We eat the exception
                }
                finally
                {
                    themeKey.Close();
                }
            }
        }

        public static WorkflowTheme CreateStandardTheme(ThemeType standardThemeType)
        {
            WorkflowTheme theme = null;
            if (standardThemeType == ThemeType.Default)
            {
                theme = new WorkflowTheme();
                theme.AmbientTheme.UseDefaultFont();
            }
            else if (standardThemeType == ThemeType.System)
            {
                theme = new WorkflowTheme();
                theme.AmbientTheme.UseOperatingSystemSettings = true;
            }
            else
            {
                return null;
            }

            string[] standardThemeParams = StandardThemes[standardThemeType] as string[];
            Debug.Assert(standardThemeParams != null);
            if (standardThemeParams != null)
            {
                theme.Name = standardThemeParams[0];
                theme.themeType = standardThemeType;
                theme.Description = standardThemeParams[1];
                theme.FilePath = LookupPath;
            }
            return theme;
        }

        public static IDictionary<ThemeType, string[]> StandardThemes
        {
            get
            {
                //DO NOT CHANGE THE ORDER OF THE THEMES ADDED BELOW
                Dictionary<ThemeType, string[]> standardThemes = new Dictionary<ThemeType, string[]>();
                standardThemes.Add(ThemeType.Default, new string[] { DR.GetString(DR.DefaultTheme), DR.GetString(DR.DefaultThemeDescription) });
                standardThemes.Add(ThemeType.System, new string[] { DR.GetString(DR.OSTheme), DR.GetString(DR.SystemThemeDescription) });
                return standardThemes;
            }
        }

        internal static void FireThemeChange()
        {
            if (WorkflowTheme.ThemeChanged != null)
                WorkflowTheme.ThemeChanged(WorkflowTheme.currentTheme, EventArgs.Empty);
        }
        #endregion

        #region Load/Save
        public static WorkflowTheme Load(string themeFilePath)
        {
            DesignerSerializationManager serializationManager = new DesignerSerializationManager();
            using (serializationManager.CreateSession())
                return WorkflowTheme.Load(serializationManager, themeFilePath);
        }

        public static WorkflowTheme Load(IDesignerSerializationManager serializationManager, string themeFilePath)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");

            WorkflowTheme theme = null;
            if (themeFilePath != null && File.Exists(themeFilePath))
            {
                XmlReader xmlReader = XmlReader.Create(themeFilePath);
                ThemeSerializationProvider themeSerializationProvider = new ThemeSerializationProvider();

                try
                {
                    serializationManager.AddSerializationProvider(themeSerializationProvider);
                    WorkflowMarkupSerializer xomlSerializer = new WorkflowMarkupSerializer();
                    theme = xomlSerializer.Deserialize(serializationManager, xmlReader) as WorkflowTheme;
                }
                finally
                {
                    serializationManager.RemoveSerializationProvider(themeSerializationProvider);
                    xmlReader.Close();
                }

                if (theme != null)
                    theme.filePath = themeFilePath;
            }

            return theme;
        }

        public void Save(string themeFilePath)
        {
            if (themeFilePath == null || themeFilePath.Length == 0)
                throw new ArgumentException(DR.GetString(DR.ThemePathNotValid), "themeFilePath");

            DesignerSerializationManager dsManager = new DesignerSerializationManager();
            using (dsManager.CreateSession())
            {
                WorkflowMarkupSerializationManager serializationManager = new WorkflowMarkupSerializationManager(dsManager);
                XmlWriter streamWriter = null;
                ThemeSerializationProvider themeSerializationProvider = new ThemeSerializationProvider();

                try
                {
                    string directory = Path.GetDirectoryName(themeFilePath);
                    if (directory.Length > 0 && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    streamWriter = Helpers.CreateXmlWriter(themeFilePath);
                    serializationManager.AddSerializationProvider(themeSerializationProvider);
                    WorkflowMarkupSerializer xomlSerializer = new WorkflowMarkupSerializer();
                    xomlSerializer.Serialize(serializationManager, streamWriter, this);
                }
                finally
                {
                    serializationManager.RemoveSerializationProvider(themeSerializationProvider);
                    if (streamWriter != null)
                        streamWriter.Close();
                }

                this.filePath = themeFilePath;
            }
        }
        #endregion

        #region a-la ICloneable Members
        public WorkflowTheme Clone()
        {
            WorkflowTheme theme = null;

            DesignerSerializationManager serializationManager = new DesignerSerializationManager();
            using (serializationManager.CreateSession())
            {
                ThemeSerializationProvider themeSerializationProvider = new ThemeSerializationProvider();
                StringWriter stringWriter = new StringWriter(new StringBuilder(), CultureInfo.InvariantCulture);
                StringReader stringReader = null;

                try
                {
                    ((IDesignerSerializationManager)serializationManager).AddSerializationProvider(themeSerializationProvider);
                    WorkflowMarkupSerializer xomlSerializer = new WorkflowMarkupSerializer();
                    using (XmlWriter xmlWriter = Helpers.CreateXmlWriter(stringWriter))
                        xomlSerializer.Serialize(serializationManager, xmlWriter, this);

                    stringReader = new StringReader(stringWriter.ToString());
                    using (XmlReader xmlReader = XmlReader.Create(stringReader))
                        theme = xomlSerializer.Deserialize(serializationManager, xmlReader) as WorkflowTheme;
                }
                finally
                {
                    ((IDesignerSerializationManager)serializationManager).RemoveSerializationProvider(themeSerializationProvider);
                    stringReader.Close();
                    stringWriter.Close();
                }
            }

            if (theme != null)
            {
                theme.filePath = this.filePath;

                //Now we go thru all the designer themes and call Initialize on them
                foreach (DesignerTheme designerTheme in theme.DesignerThemes)
                    designerTheme.Initialize();
            }

            return theme;
        }
        #endregion

        #region Public Properties and Methods
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.name = value;
            }
        }

        public string Version
        {
            get
            {
                return this.version;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.version = value;
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.description = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string FilePath
        {
            get
            {
                return this.filePath;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.filePath = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ContainingFileDirectory
        {
            get
            {
                string directory = String.Empty;
                if (this.filePath.Length > 0)
                {
                    try
                    {
                        directory = Path.GetDirectoryName(this.filePath) + Path.DirectorySeparatorChar;
                    }
                    catch
                    {
                    }
                }

                return directory;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public IList DesignerThemes
        {
            get
            {
                return this.designerThemes;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AmbientTheme AmbientTheme
        {
            get
            {
                return GetTheme(typeof(WorkflowView)) as AmbientTheme;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ActivityDesignerTheme GetDesignerTheme(ActivityDesigner designer)
        {
            if (designer == null)
                throw new ArgumentNullException("designer");

            return GetTheme(designer.GetType()) as ActivityDesignerTheme;
        }

        internal DesignerTheme GetTheme(Type designerType)
        {
            bool wasReadOnly = ReadOnly;
            DesignerTheme designerTheme = (this.designerThemes.Contains(designerType.FullName)) ? this.designerThemes[designerType.FullName] : null;

            try
            {
                ReadOnly = false;

                if (designerTheme == null || (designerTheme.DesignerType != null && !designerType.Equals(designerTheme.DesignerType)))
                {
                    //This means that the two types are not equal and hence we need to replace the theme
                    bool replaceTheme = (designerTheme != null);

                    AttributeCollection attributeCollection = TypeDescriptor.GetAttributes(designerType);
                    ActivityDesignerThemeAttribute themeAttrib = attributeCollection[typeof(ActivityDesignerThemeAttribute)] as ActivityDesignerThemeAttribute;
                    if (themeAttrib == null)
                        throw new InvalidOperationException(DR.GetString(DR.Error_ThemeAttributeMissing, designerType.FullName));

                    if (themeAttrib.DesignerThemeType == null)
                        throw new InvalidOperationException(DR.GetString(DR.Error_ThemeTypeMissing, designerType.FullName));

                    if (themeAttrib.Xml.Length > 0)
                    {
                        //First check if the theme initializer is obtained from resource as a manifest
                        Stream stream = designerType.Assembly.GetManifestResourceStream(designerType, themeAttrib.Xml);
                        if (stream == null)
                            stream = designerType.Assembly.GetManifestResourceStream(WorkflowTheme.ThemeResourceNS + themeAttrib.Xml);

                        //Check if the theme initializer is obtained from file
                        XmlReader textReader = (stream != null) ? XmlReader.Create(stream) : null;
                        if (textReader == null)
                            textReader = XmlReader.Create(new StringReader(themeAttrib.Xml));

                        if (textReader != null)
                        {
                            DesignerSerializationManager serializationManager = new DesignerSerializationManager();
                            using (serializationManager.CreateSession())
                            {
                                ThemeSerializationProvider themeSerializationProvider = new ThemeSerializationProvider();

                                try
                                {
                                    ((IDesignerSerializationManager)serializationManager).AddSerializationProvider(themeSerializationProvider);
                                    ((IDesignerSerializationManager)serializationManager).Context.Push(this);
                                    WorkflowMarkupSerializer xomlSerializer = new WorkflowMarkupSerializer();
                                    designerTheme = xomlSerializer.Deserialize(serializationManager, textReader) as DesignerTheme;

                                    if (designerTheme != null && !themeAttrib.DesignerThemeType.IsAssignableFrom(designerTheme.GetType()))
                                    {
                                        ((IDesignerSerializationManager)serializationManager).ReportError(new WorkflowMarkupSerializationException(DR.GetString(DR.ThemeTypesMismatch, new object[] { themeAttrib.DesignerThemeType.FullName, designerTheme.GetType().FullName })));
                                        designerTheme = null;
                                    }

                                    if (serializationManager.Errors.Count > 0)
                                    {
                                        string errors = String.Empty;
                                        foreach (object ex in serializationManager.Errors)
                                            errors += ex.ToString() + @"\n";
                                        Debug.WriteLine(errors);
                                    }
                                }
                                finally
                                {
                                    //In some cases the type resolution throws an exception when we try to create a illegal theme type, 
                                    //this is the reason we need to catch it and proceed further
                                    ((IDesignerSerializationManager)serializationManager).RemoveSerializationProvider(themeSerializationProvider);
                                    textReader.Close();
                                }
                            }
                        }
                    }

                    if (designerTheme == null)
                    {
                        try
                        {
                            designerTheme = Activator.CreateInstance(themeAttrib.DesignerThemeType, new object[] { this }) as DesignerTheme;
                        }
                        catch
                        {
                            //If an exception is thrown here then surely we can not create designerTheme specified by the user so we create a default theme
                            //and give it to the user
                            designerTheme = new ActivityDesignerTheme(this);
                        }
                    }

                    designerTheme.DesignerType = designerType;
                    designerTheme.ApplyTo = designerType.FullName;
                    designerTheme.Initialize();
                    if (replaceTheme)
                        this.designerThemes.Remove(designerType.FullName);
                    this.designerThemes.Add(designerTheme);
                }

                if (designerTheme.DesignerType == null)
                    designerTheme.DesignerType = designerType;
            }
            finally
            {
                ReadOnly = wasReadOnly;
            }

            return designerTheme;
        }

        internal void AmbientPropertyChanged(AmbientProperty ambientProperty)
        {
            foreach (DesignerTheme theme in this.designerThemes)
            {
                bool oldReadOnly = this.ReadOnly;
                this.ReadOnly = false;
                theme.OnAmbientPropertyChanged(ambientProperty);
                this.ReadOnly = oldReadOnly;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ThemeType Type
        {
            get
            {
                return this.themeType;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ReadOnly
        {
            get
            {
                return this.readOnly;
            }

            set
            {
                this.readOnly = value;
            }
        }
        #endregion

        #region Class ThemeCollection
        private class ThemeCollection : KeyedCollection<string, DesignerTheme>
        {
            protected override string GetKeyForItem(DesignerTheme item)
            {
                Debug.Assert(item.ApplyTo != null && item.ApplyTo.Length > 0);
                return item.ApplyTo;
            }
        }
        #endregion
    }
    #endregion

    #region Class DesignerTheme
    [DesignerSerializer(typeof(ThemeSerializer), typeof(WorkflowMarkupSerializer))]
    [TypeConverter(typeof(ThemeTypeConverter))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class DesignerTheme : IDisposable, IPropertyValueProvider
    {
        #region Members and Initialization
        private WorkflowTheme workflowTheme = null;
        private Type designerType = null;
        private string designerTypeName = String.Empty;

        protected DesignerTheme(WorkflowTheme theme)
        {
            this.workflowTheme = theme;
        }

        ~DesignerTheme()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Initialize()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
        }
        #endregion

        #region Properties and Methods
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        protected WorkflowTheme ContainingTheme
        {
            get
            {
                return this.workflowTheme;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public virtual Type DesignerType
        {
            get
            {
                return this.designerType;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.designerType = value;
            }
        }

        [Browsable(false)]
        public virtual string ApplyTo
        {
            get
            {
                return this.designerTypeName;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.designerTypeName = value;
            }
        }

        public virtual void OnAmbientPropertyChanged(AmbientProperty ambientProperty)
        {
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ReadOnly
        {
            get
            {
                if (this.workflowTheme != null)
                    return this.workflowTheme.ReadOnly;
                else
                    return false;
            }
            internal set
            {
                if (this.workflowTheme != null)
                    this.workflowTheme.ReadOnly = value;
            }
        }
        #endregion

        #region IPropertyValueProvider Members
        ICollection IPropertyValueProvider.GetPropertyValues(ITypeDescriptorContext context)
        {
            //You can filter the enum values here by using EnumFilterConverter
            return GetPropertyValues(context);
        }

        internal virtual ICollection GetPropertyValues(ITypeDescriptorContext context)
        {
            //You can filter the enum values here by using EnumFilterConverter
            return new object[] { };
        }
        #endregion

        #region Class ThemeSerializer
        private class ThemeSerializer : WorkflowMarkupSerializer
        {
            protected override object CreateInstance(WorkflowMarkupSerializationManager serializationManager, Type type)
            {
                if (typeof(DesignerTheme).IsAssignableFrom(type))
                    return Activator.CreateInstance(type, new object[] { serializationManager.Context[typeof(WorkflowTheme)] });
                else
                    return base.CreateInstance(serializationManager, type);
            }
        }
        #endregion

    }
    #endregion

    #region Class ActivityDesignerTheme
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityDesignerTheme : DesignerTheme
    {
        #region Constants
        private static readonly Size[] DesignerSizes = new Size[] { new Size(90, 40), new Size(130, 41), new Size(110, 50) };
        private static readonly Size[] ImageSizes = new Size[] { new Size(16, 16), new Size(16, 16), new Size(24, 24) };
        #endregion

        #region Members and Initialization
        //Public members
        private string designerImagePath = String.Empty;
        private Color foreColor = Color.Black;
        private Color borderColor = Color.Black;
        private DashStyle borderStyle = DashStyle.Solid;
        private Color backColorStart = Color.White;
        private Color backColorEnd = Color.Empty;
        private LinearGradientMode backgroundStyle = LinearGradientMode.Horizontal;

        //Temporary members
        private Image designerImage;
        private Pen foregroundPen;
        private Pen borderPen;
        private Brush foregroundBrush;
        private Brush backgroundBrush;
        private Rectangle backgroundBrushRect;

        public ActivityDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this.designerImage != null)
                {
                    this.designerImage.Dispose();
                    this.designerImage = null;
                }

                if (this.foregroundPen != null)
                {
                    this.foregroundPen.Dispose();
                    this.foregroundPen = null;
                }

                if (this.foregroundBrush != null)
                {
                    this.foregroundBrush.Dispose();
                    this.foregroundBrush = null;
                }

                if (this.borderPen != null)
                {
                    this.borderPen.Dispose();
                    this.borderPen = null;
                }

                if (this.backgroundBrush != null)
                {
                    this.backgroundBrush.Dispose();
                    this.backgroundBrush = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            if (ContainingTheme.AmbientTheme.UseOperatingSystemSettings)
                ApplySystemColors();
        }
        #endregion

        #region Publicly browsable Properties
        [DispId(1)]
        [SRDescription("ImageDesc", DR.ResourceSet)]
        [SRCategory("ForegroundCategory", DR.ResourceSet)]
        [Editor(typeof(ImageBrowserEditor), typeof(UITypeEditor))]
        public virtual string DesignerImagePath
        {
            get
            {
                return this.designerImagePath;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                if (value != null && value.Length > 0 && value.Contains(Path.DirectorySeparatorChar.ToString()) && Path.IsPathRooted(value))
                {
                    value = DesignerHelpers.GetRelativePath(ContainingTheme.ContainingFileDirectory, value);

                    if (!DesignerHelpers.IsValidImageResource(this, ContainingTheme.ContainingFileDirectory, value))
                        throw new InvalidOperationException(DR.GetString(DR.Error_InvalidImageResource));
                }

                this.designerImagePath = value;
                if (this.designerImage != null)
                {
                    this.designerImage.Dispose();
                    this.designerImage = null;
                }
            }
        }

        [DispId(2)]
        [SRDescription("ForeColorDesc", DR.ResourceSet)]
        [SRCategory("ForegroundCategory", DR.ResourceSet)]
        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ColorPickerConverter))]
        public virtual Color ForeColor
        {
            get
            {
                return this.foreColor;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.foreColor = value;

                if (this.foregroundPen != null)
                {
                    this.foregroundPen.Dispose();
                    this.foregroundPen = null;
                }

                if (this.foregroundBrush != null)
                {
                    this.foregroundBrush.Dispose();
                    this.foregroundBrush = null;
                }
            }
        }

        [DispId(3)]
        [SRDescription("BorderColorDesc", DR.ResourceSet)]
        [SRCategory("ForegroundCategory", DR.ResourceSet)]
        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ColorPickerConverter))]
        public virtual Color BorderColor
        {
            get
            {
                return this.borderColor;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.borderColor = value;
                if (this.borderPen != null)
                {
                    this.borderPen.Dispose();
                    this.borderPen = null;
                }
            }
        }

        [DispId(4)]
        [SRDescription("BorderStyleDesc", DR.ResourceSet)]
        [SRCategory("ForegroundCategory", DR.ResourceSet)]
        [TypeConverter(typeof(FilteredEnumConverter))]
        public virtual DashStyle BorderStyle
        {
            get
            {
                return this.borderStyle;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                if (value == DashStyle.Custom)
                    throw new Exception(DR.GetString(DR.CustomStyleNotSupported));

                this.borderStyle = value;
                if (this.borderPen != null)
                {
                    this.borderPen.Dispose();
                    this.borderPen = null;
                }
            }
        }

        [DispId(5)]
        [SRDescription("BackColorStartDesc", DR.ResourceSet)]
        [SRCategory("BackgroundCategory", DR.ResourceSet)]
        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ColorPickerConverter))]
        public virtual Color BackColorStart
        {
            get
            {
                return this.backColorStart;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.backColorStart = value;
                if (this.backgroundBrush != null)
                {
                    this.backgroundBrush.Dispose();
                    this.backgroundBrush = null;
                }
            }
        }

        [DispId(6)]
        [SRDescription("BackColorEndDesc", DR.ResourceSet)]
        [SRCategory("BackgroundCategory", DR.ResourceSet)]
        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ColorPickerConverter))]
        public virtual Color BackColorEnd
        {
            get
            {
                return this.backColorEnd;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.backColorEnd = value;
                if (this.backgroundBrush != null)
                {
                    this.backgroundBrush.Dispose();
                    this.backgroundBrush = null;
                }
            }
        }

        [DispId(7)]
        [SRDescription("BackgroundStyleDesc", DR.ResourceSet)]
        [SRCategory("BackgroundCategory", DR.ResourceSet)]
        public virtual LinearGradientMode BackgroundStyle
        {
            get
            {
                return this.backgroundStyle;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.backgroundStyle = value;
                if (this.backgroundBrush != null)
                {
                    this.backgroundBrush.Dispose();
                    this.backgroundBrush = null;
                }
            }
        }
        #endregion

        #region Helper Properties
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Pen ForegroundPen
        {
            get
            {
                if (this.foregroundPen == null)
                    this.foregroundPen = new Pen(this.foreColor, BorderWidth);
                return this.foregroundPen;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Brush ForegroundBrush
        {
            get
            {
                if (this.foregroundBrush == null)
                    this.foregroundBrush = new SolidBrush(this.foreColor);
                return this.foregroundBrush;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Pen BorderPen
        {
            get
            {
                if (this.borderPen == null)
                {
                    this.borderPen = new Pen(this.borderColor, BorderWidth);
                    this.borderPen.DashStyle = this.borderStyle;
                }
                return this.borderPen;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Brush GetBackgroundBrush(Rectangle rectangle)
        {
            if (this.backgroundBrush == null || this.backgroundBrushRect != rectangle)
            {
                if (this.backgroundBrush != null)
                    this.backgroundBrush.Dispose();
                this.backgroundBrushRect = rectangle;

                if (this.backColorStart == this.backColorEnd)
                    this.backgroundBrush = new SolidBrush(this.backColorStart);
                else
                    this.backgroundBrush = new LinearGradientBrush(this.backgroundBrushRect, this.backColorStart, this.backColorEnd, this.backgroundStyle);
            }
            return this.backgroundBrush;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Size Size
        {
            get
            {
                return ActivityDesignerTheme.DesignerSizes[(int)ContainingTheme.AmbientTheme.DesignerSize];
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public DesignerGeometry DesignerGeometry
        {
            get
            {
                if (ContainingTheme.AmbientTheme.DrawRounded)
                    return DesignerGeometry.RoundedRectangle;
                else
                    return DesignerGeometry.Rectangle;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Image DesignerImage
        {
            get
            {
                if (this.designerImage == null && this.designerImagePath.Length > 0)
                    this.designerImage = DesignerHelpers.GetImageFromPath(this, ContainingTheme.ContainingFileDirectory, this.designerImagePath);
                return this.designerImage;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Size ImageSize
        {
            get
            {
                return ActivityDesignerTheme.ImageSizes[(int)ContainingTheme.AmbientTheme.DesignerSize];
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Font Font
        {
            get
            {
                return ContainingTheme.AmbientTheme.Font;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Font BoldFont
        {
            get
            {
                return ContainingTheme.AmbientTheme.BoldFont;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int BorderWidth
        {
            get
            {
                return ContainingTheme.AmbientTheme.BorderWidth;
            }
        }

        public override void OnAmbientPropertyChanged(AmbientProperty ambientProperty)
        {
            if (ambientProperty == AmbientProperty.DesignerSize)
            {
                ForeColor = this.foreColor;
                BorderColor = this.borderColor;
            }
            else if (ambientProperty == AmbientProperty.OperatingSystemSetting)
            {
                ApplySystemColors();
            }
        }

        private void ApplySystemColors()
        {
            ForeColor = SystemColors.ControlText;
            BorderColor = SystemColors.ControlDark;
            BackColorStart = SystemColors.Control;
            BackColorEnd = SystemColors.ControlLight;
        }
        #endregion

        #region IPropertyValueProvider Implementation
        internal override ICollection GetPropertyValues(ITypeDescriptorContext context)
        {
            object[] values = new object[] { };
            if (string.Equals(context.PropertyDescriptor.Name, "BorderStyle", StringComparison.Ordinal))
                values = new object[] { DashStyle.Solid, DashStyle.Dash, DashStyle.DashDot, DashStyle.DashDotDot, DashStyle.Dot };

            return values;
        }
        #endregion
    }
    #endregion

    #region Class CompositeDesignerTheme
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class CompositeDesignerTheme : ActivityDesignerTheme
    {
        #region Static Members
        internal static readonly Pen ExpandButtonForegoundPen = new Pen(Color.Black, 1);
        internal static readonly Pen ExpandButtonBorderPen = new Pen(Color.FromArgb(123, 154, 181), 1);

        private static readonly Size[] ExpandButtonSizes = new Size[] { new Size(8, 8), new Size(8, 8), new Size(12, 12) };
        private static readonly Size[] ConnectorSizes = new Size[] { new Size(15, 30), new Size(15, 19), new Size(25, 50) };
        #endregion

        #region Members and Constructor
        //Members
        private DesignerContentAlignment watermarkAlignment = DesignerContentAlignment.BottomRight;
        private string watermarkImagePath = String.Empty;
        private bool dropShadow = false;
        private LineAnchor startCap = LineAnchor.None;
        private LineAnchor endCap = LineAnchor.ArrowAnchor;

        //Temporary variables
        private Brush expandButtonBackBrush;
        private Rectangle expandButtonRectangle = Rectangle.Empty;
        private Image watermarkImage;

        public CompositeDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this.expandButtonBackBrush != null)
                {
                    this.expandButtonBackBrush.Dispose();
                    this.expandButtonBackBrush = null;
                }

                if (this.watermarkImage != null)
                {
                    this.watermarkImage.Dispose();
                    this.watermarkImage = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            if (ContainingTheme.AmbientTheme.UseOperatingSystemSettings)
                ApplySystemColors();
        }

        #endregion

        #region Publicly browsable Properties
        [DispId(8)]
        [SRDescription("WatermarkDesc", DR.ResourceSet)]
        [SRCategory("BackgroundCategory", DR.ResourceSet)]
        [Editor(typeof(ImageBrowserEditor), typeof(UITypeEditor))]
        public virtual string WatermarkImagePath
        {
            get
            {
                return this.watermarkImagePath;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                if (!String.IsNullOrEmpty(value) && value.Contains(Path.DirectorySeparatorChar.ToString()) && Path.IsPathRooted(value))
                {
                    value = DesignerHelpers.GetRelativePath(ContainingTheme.ContainingFileDirectory, value);

                    if (!DesignerHelpers.IsValidImageResource(this, ContainingTheme.ContainingFileDirectory, value))
                        throw new InvalidOperationException(DR.GetString(DR.Error_InvalidImageResource));
                }

                this.watermarkImagePath = value;
                if (this.watermarkImage != null)
                {
                    this.watermarkImage.Dispose();
                    this.watermarkImage = null;
                }
            }
        }

        [DispId(9)]
        [DefaultValue(DesignerContentAlignment.BottomRight)]
        [SRDescription("WatermarkAlignmentDesc", DR.ResourceSet)]
        [SRCategory("BackgroundCategory", DR.ResourceSet)]
        public virtual DesignerContentAlignment WatermarkAlignment
        {
            get
            {
                return this.watermarkAlignment;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.watermarkAlignment = value;
            }
        }

        [DefaultValue(false)]
        [DispId(10)]
        [SRDescription("DropShadowDesc", DR.ResourceSet)]
        [SRCategory("BackgroundCategory", DR.ResourceSet)]
        public virtual bool ShowDropShadow
        {
            get
            {
                return this.dropShadow;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.dropShadow = value;
            }
        }

        [DefaultValue(LineAnchor.None)]
        [DispId(11)]
        [SRDescription("ConnectorStartCapDesc", DR.ResourceSet)]
        [SRCategory("ForegroundCategory", DR.ResourceSet)]
        public virtual LineAnchor ConnectorStartCap
        {
            get
            {
                return this.startCap;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.startCap = value;
            }
        }

        [DefaultValue(LineAnchor.ArrowAnchor)]
        [DispId(12)]
        [SRDescription("ConnectorEndCapDesc", DR.ResourceSet)]
        [SRCategory("ForegroundCategory", DR.ResourceSet)]
        public virtual LineAnchor ConnectorEndCap
        {
            get
            {
                return this.endCap;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.endCap = value;
            }
        }
        #endregion

        #region Helper Methods
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public virtual Size ConnectorSize
        {
            get
            {
                if (DesignerType != null && typeof(FreeformActivityDesigner).IsAssignableFrom(DesignerType))
                {
                    int connectorSize = CompositeDesignerTheme.ConnectorSizes[(int)ContainingTheme.AmbientTheme.DesignerSize].Height;
                    return new Size(connectorSize, connectorSize);
                }
                else
                {
                    return CompositeDesignerTheme.ConnectorSizes[(int)ContainingTheme.AmbientTheme.DesignerSize];
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public virtual Size ExpandButtonSize
        {
            get
            {
                return CompositeDesignerTheme.ExpandButtonSizes[(int)ContainingTheme.AmbientTheme.DesignerSize];
            }
        }

        public Brush GetExpandButtonBackgroundBrush(Rectangle rectangle)
        {
            if (this.expandButtonBackBrush == null || this.expandButtonRectangle != rectangle)
            {
                if (this.expandButtonBackBrush != null)
                    this.expandButtonBackBrush.Dispose();
                this.expandButtonRectangle = rectangle;
                this.expandButtonBackBrush = new LinearGradientBrush(this.expandButtonRectangle, Color.White, Color.FromArgb(173, 170, 156), LinearGradientMode.ForwardDiagonal);
            }
            return this.expandButtonBackBrush;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Image WatermarkImage
        {
            get
            {
                if (this.watermarkImage == null && this.watermarkImagePath.Length > 0)
                    this.watermarkImage = DesignerHelpers.GetImageFromPath(this, ContainingTheme.ContainingFileDirectory, this.watermarkImagePath);
                return this.watermarkImage;
            }
        }

        public override void OnAmbientPropertyChanged(AmbientProperty ambientProperty)
        {
            base.OnAmbientPropertyChanged(ambientProperty);

            if (ambientProperty == AmbientProperty.OperatingSystemSetting)
                ApplySystemColors();
        }

        private void ApplySystemColors()
        {
            //the root designers should be transparent...
            BackColorStart = Color.Empty;
            BackColorEnd = Color.Empty;
        }
        #endregion
    }
    #endregion

    #region Class ActivityPreviewDesignerTheme
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityPreviewDesignerTheme : CompositeDesignerTheme
    {
        #region Static Members
        internal static readonly Bitmap LeftScrollImage = DR.GetImage(DR.MoveLeft) as Bitmap;
        internal static readonly Bitmap LeftScrollImageUp = DR.GetImage(DR.MoveLeftUp) as Bitmap;
        internal static readonly Bitmap RightScrollImage = DR.GetImage(DR.MoveRight) as Bitmap;
        internal static readonly Bitmap RightScrollImageUp = DR.GetImage(DR.MoveRightUp) as Bitmap;
        internal static readonly Bitmap PreviewButtonImage = DR.GetImage(DR.PreviewModeIcon) as Bitmap;
        internal static readonly Bitmap EditButtonImage = DR.GetImage(DR.EditModeIcon) as Bitmap;
        internal static readonly Bitmap PreviewImage = DR.GetImage(DR.PreviewIndicator) as Bitmap;

        private static readonly Size[] ItemSizes = new Size[] { new Size(20, 20), new Size(20, 20), new Size(30, 30) };
        private static readonly Size[] PreviewButtonSizes = new Size[] { new Size(16, 16), new Size(16, 16), new Size(20, 20) };
        private static readonly Size[] PreviewWindowSizes = new Size[] { new Size(172, 120), new Size(172, 120), new Size(212, 160) };
        private const int DefaultItemCount = 5;
        #endregion

        #region Members
        private Color previewForeColor = Color.WhiteSmoke;
        private Color previewBackColor = Color.White;
        private Color previewBorderColor = Color.Gray;

        //Temporary members
        private Brush previewForegroundBrush;
        private Brush previewBackgroundBrush;
        private Pen previewBorderPen;

        public ActivityPreviewDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this.previewForegroundBrush != null)
                {
                    this.previewForegroundBrush.Dispose();
                    this.previewForegroundBrush = null;
                }

                if (this.previewBackgroundBrush != null)
                {
                    this.previewBackgroundBrush.Dispose();
                    this.previewBackgroundBrush = null;
                }

                if (this.previewBorderPen != null)
                {
                    this.previewBorderPen.Dispose();
                    this.previewBorderPen = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            if (ContainingTheme.AmbientTheme.UseOperatingSystemSettings)
                ApplySystemColors();
        }
        #endregion

        #region Publicly browsable Properties
        [DispId(13)]
        [SRDescription("PreviewForeColorDesc", DR.ResourceSet)]
        [SRCategory("ForegroundCategory", DR.ResourceSet)]
        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ColorPickerConverter))]
        public Color PreviewForeColor
        {
            get
            {
                return this.previewForeColor;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.previewForeColor = value;
                if (this.previewForegroundBrush != null)
                {
                    this.previewForegroundBrush.Dispose();
                    this.previewForegroundBrush = null;
                }
            }
        }

        [DispId(14)]
        [SRDescription("PreviewBackColorDesc", DR.ResourceSet)]
        [SRCategory("BackgroundCategory", DR.ResourceSet)]
        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ColorPickerConverter))]
        public Color PreviewBackColor
        {
            get
            {
                return this.previewBackColor;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.previewBackColor = value;
                if (this.previewBackgroundBrush != null)
                {
                    this.previewBackgroundBrush.Dispose();
                    this.previewBackgroundBrush = null;
                }
            }
        }

        [DispId(15)]
        [SRDescription("PreviewBorderColorDesc", DR.ResourceSet)]
        [SRCategory("ForegroundCategory", DR.ResourceSet)]
        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ColorPickerConverter))]
        public Color PreviewBorderColor
        {
            get
            {
                return this.previewBorderColor;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.previewBorderColor = value;
                if (this.previewBorderPen != null)
                {
                    this.previewBorderPen.Dispose();
                    this.previewBorderPen = null;
                }
            }
        }
        #endregion

        #region Helper Properties
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        internal Size PreviewItemSize
        {
            get
            {
                return ActivityPreviewDesignerTheme.ItemSizes[(int)ContainingTheme.AmbientTheme.DesignerSize];
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        internal int PreviewItemCount
        {
            get
            {
                return ActivityPreviewDesignerTheme.DefaultItemCount;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        internal Size PreviewWindowSize
        {
            get
            {
                return ActivityPreviewDesignerTheme.PreviewWindowSizes[(int)ContainingTheme.AmbientTheme.DesignerSize];
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        internal Size PreviewButtonSize
        {
            get
            {
                return ActivityPreviewDesignerTheme.PreviewButtonSizes[(int)ContainingTheme.AmbientTheme.DesignerSize];
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        internal Pen PreviewBorderPen
        {
            get
            {
                if (this.previewBorderPen == null)
                    this.previewBorderPen = new Pen(this.previewBorderColor, BorderWidth);
                return this.previewBorderPen;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        internal Brush PreviewForegroundBrush
        {
            get
            {
                if (this.previewForegroundBrush == null)
                    this.previewForegroundBrush = new SolidBrush(this.previewForeColor);
                return this.previewForegroundBrush;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        internal Brush PreviewBackgroundBrush
        {
            get
            {
                if (this.previewBackgroundBrush == null)
                    this.previewBackgroundBrush = new SolidBrush(this.previewBackColor);
                return this.previewBackgroundBrush;
            }
        }

        public override void OnAmbientPropertyChanged(AmbientProperty ambientProperty)
        {
            base.OnAmbientPropertyChanged(ambientProperty);

            if (ambientProperty == AmbientProperty.DesignerSize)
                PreviewBorderColor = this.previewBorderColor;
            else if (ambientProperty == AmbientProperty.OperatingSystemSetting)
                ApplySystemColors();
        }

        private void ApplySystemColors()
        {
            PreviewForeColor = SystemColors.ButtonFace;
            PreviewBackColor = SystemColors.Window;
            PreviewBorderColor = SystemColors.ControlDarkDark;
            BorderColor = SystemColors.ControlDarkDark;
        }
        #endregion
    }
    #endregion

    #region Class AmbientTheme
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class AmbientTheme : DesignerTheme
    {
        #region Constants which wont change
        internal const float WatermarkTransparency = 0.25f;
        internal const int ArcDiameter = 8;
        internal const int DropShadowWidth = 4;

        internal static Color TransparentColor = Color.FromArgb(255, 0, 255);
        internal static readonly Image ConfigErrorImage = DR.GetImage(DR.ConfigError);
        internal static readonly Image ScrollIndicatorImage = DR.GetImage(DR.ArrowLeft);
        internal static readonly Image DropIndicatorImage = DR.GetImage(DR.DropShapeShort);
        internal static readonly Image LockImage = DR.GetImage(DR.PreviewIndicator);
        internal static readonly Image ReadOnlyImage = DR.GetImage(DR.ReadOnly);

        internal static readonly Pen SmartTagBorderPen = new Pen(Color.Black, 1);
        internal static readonly Pen MagnifierPen = new Pen(Color.Black, 2);
        internal static readonly Pen WorkflowBorderPen = new Pen(Color.FromArgb(127, 157, 185), 1);
        internal static readonly Brush WorkspaceBackgroundBrush = new SolidBrush(Color.FromArgb(234, 234, 236));
        internal static readonly Brush FadeBrush = new SolidBrush(Color.FromArgb(120, 255, 255, 255));
        internal static readonly Brush DisabledBrush = new SolidBrush(Color.FromArgb(40, Color.Gray));
        internal static readonly Brush PageShadowBrush = new SolidBrush(Color.FromArgb(75, 75, 75));

        internal const float ScrollIndicatorTransparency = 0.7f;
        internal static readonly Size DragImageMargins = new Size(4, 4);
        internal static readonly Size DragImageTextSize = new Size(100, 60);
        internal static readonly Size DragImageIconSize = new Size(16, 16);

        internal const int MinZoom = 10;
        internal const int MaxZoom = 400;
        internal const int ScrollUnit = 25;
        internal const int MinShadowDepth = 0;
        internal const int MaxShadowDepth = 8;

        private static float[] fontSizes = null;
        private static readonly Size[] GridSizes = new Size[] { new Size(30, 30), new Size(40, 40), new Size(60, 60) };
        private static readonly Size[] MarginSizes = new Size[] { new Size(2, 2), new Size(4, 4), new Size(6, 6) };
        private static readonly Size[] SelectionSizes = new Size[] { new Size(2, 2), new Size(4, 4), new Size(6, 6) };
        private static readonly Size[] GlyphSizes = new Size[] { new Size(10, 10), new Size(14, 14), new Size(18, 18) };
        private static readonly Size[] ScrollIndicatorSizes = new Size[] { new Size(24, 24), new Size(32, 32), new Size(40, 40) };
        private static readonly Size[] DropIndicatorSizes = new Size[] { new Size(8, 8), new Size(12, 12), new Size(16, 16) };
        private static readonly Size[] MagnifierSizes = new Size[] { new Size(50, 50), new Size(100, 100), new Size(150, 150) };
        private static readonly int[] BorderWidths = new int[] { 1, 1, 3 };

        private const int DefaultShadowDepth = 6;
        #endregion

        private static float[] FontSizes
        {
            get
            {
                if (fontSizes == null)
                    fontSizes = new float[] { WorkflowTheme.GetDefaultFont().SizeInPoints - 2.0f, WorkflowTheme.GetDefaultFont().SizeInPoints, WorkflowTheme.GetDefaultFont().SizeInPoints + 2.0f };
                return fontSizes;
            }
        }

        #region Members
        //General category
        private bool useOperatingSystemSettings = false;
        private bool showConfigErrors = true;
        private bool drawShadow = false;

        //Advanced
        private bool drawGrayscale = false;
        private Color dropIndicatorColor = Color.Green;
        private Color selectionForeColor = Color.Blue;
        private Color selectionPatternColor = Color.DarkGray;
        private Color foreColor = Color.Gray;
        private Color backColor = Color.White;
        private Color commentIndicatorColor = Color.FromArgb(49, 198, 105);
        private Color readonlyIndicatorColor = Color.Gray;
        private DesignerContentAlignment watermarkAlignment = DesignerContentAlignment.BottomRight;
        private string watermarkImagePath = String.Empty;
        private bool useDefaultFont = false;

        //Grid
        private bool showGrid = false;
        private DashStyle gridStyle = DashStyle.Dash;
        private Color gridColor = Color.FromArgb(192, 192, 192);

        //Font
        private string fontName = WorkflowTheme.GetDefaultFont().FontFamily.Name;
        private TextQuality textQuality = TextQuality.Aliased;

        //Designer
        private DesignerSize designerStyle = DesignerSize.Medium;
        private bool drawRounded = true;
        private bool showDesignerBorder = true;
        #endregion

        #region Resource Members
        private Font font;
        private Font boldFont;
        private Pen foregroundPen;
        private Pen selectionForegroundPen;
        private Pen selectionPatternPen;
        private Pen majorGridPen;
        private Pen minorGridPen;
        private Pen dropIndicatorPen;
        private Pen commentIndicatorPen;
        private Brush backgroundBrush;
        private Brush foregroundBrush;
        private Brush selectionForegroundBrush;
        private Brush dropIndicatorBrush;
        private Brush commentIndicatorBrush;
        private Brush readonlyIndicatorBrush;
        private Brush majorGridBrush;
        private Brush minorGridBrush;
        private Image watermarkImage;
        #endregion

        public AmbientTheme(WorkflowTheme theme)
            : base(theme)
        {
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                //We stop listening to OS setting events
                UseOperatingSystemSettings = false;

                if (this.font != null)
                {
                    this.font.Dispose();
                    this.font = null;
                }

                if (this.boldFont != null)
                {
                    this.boldFont.Dispose();
                    this.boldFont = null;
                }

                if (this.watermarkImage != null)
                {
                    this.watermarkImage.Dispose();
                    this.watermarkImage = null;
                }

                if (this.foregroundPen != null)
                {
                    this.foregroundPen.Dispose();
                    this.foregroundPen = null;
                }

                if (this.foregroundBrush != null)
                {
                    this.foregroundBrush.Dispose();
                    this.foregroundBrush = null;
                }

                if (this.backgroundBrush != null)
                {
                    this.backgroundBrush.Dispose();
                    this.backgroundBrush = null;
                }

                if (this.dropIndicatorPen != null)
                {
                    this.dropIndicatorPen.Dispose();
                    this.dropIndicatorPen = null;
                }

                if (this.selectionPatternPen != null)
                {
                    this.selectionPatternPen.Dispose();
                    this.selectionPatternPen = null;
                }

                if (this.selectionForegroundPen != null)
                {
                    this.selectionForegroundPen.Dispose();
                    this.selectionForegroundPen = null;
                }

                if (this.majorGridPen != null)
                {
                    this.majorGridPen.Dispose();
                    this.majorGridPen = null;
                }

                if (this.majorGridBrush != null)
                {
                    this.majorGridBrush.Dispose();
                    this.majorGridBrush = null;
                }

                if (this.minorGridPen != null)
                {
                    this.minorGridPen.Dispose();
                    this.minorGridPen = null;
                }

                if (this.minorGridBrush != null)
                {
                    this.minorGridBrush.Dispose();
                    this.minorGridBrush = null;
                }

                if (this.commentIndicatorPen != null)
                {
                    this.commentIndicatorPen.Dispose();
                    this.commentIndicatorPen = null;
                }

                if (this.commentIndicatorBrush != null)
                {
                    this.commentIndicatorBrush.Dispose();
                    this.commentIndicatorBrush = null;
                }

                if (this.readonlyIndicatorBrush != null)
                {
                    this.readonlyIndicatorBrush.Dispose();
                    this.readonlyIndicatorBrush = null;
                }

                if (this.dropIndicatorBrush != null)
                {
                    this.dropIndicatorBrush.Dispose();
                    this.dropIndicatorBrush = null;
                }

                if (this.selectionForegroundBrush != null)
                {
                    this.selectionForegroundBrush.Dispose();
                    this.selectionForegroundBrush = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            if (this.useOperatingSystemSettings)
                ApplySystemColors();
        }

        #region Publicly browsable Properties

        #region General Properties
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool UseOperatingSystemSettings
        {
            get
            {
                return this.useOperatingSystemSettings;
            }

            internal set
            {
                this.useOperatingSystemSettings = value;
                if (this.useOperatingSystemSettings)
                {
                    SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(OnOperatingSystemSettingsChanged);
                    OnOperatingSystemSettingsChanged(this, new UserPreferenceChangedEventArgs(UserPreferenceCategory.Color));
                }
                else
                {
                    SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(OnOperatingSystemSettingsChanged);
                }
            }
        }

        [DispId(1)]
        [SRDescription("FontDesc", DR.ResourceSet)]
        [SRCategory("WorkflowAppearanceCategory", DR.ResourceSet)]
        [TypeConverter(typeof(FontFamilyConverter))]
        public virtual string FontName
        {
            get
            {
                return this.fontName;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                if (value == null || value.Length == 0)
                    throw new Exception(DR.GetString(DR.EmptyFontFamilyNotSupported));

                try
                {
                    Font font = new Font(value, FontSize);
                    if (font != null)
                        font.Dispose();
                }
                catch (Exception e)
                {
                    throw new Exception(DR.GetString(DR.FontFamilyNotSupported, value), e);
                }

                this.fontName = value;
                if (this.font != null)
                {
                    this.font.Dispose();
                    this.font = null;
                }

                if (this.boldFont != null)
                {
                    this.boldFont.Dispose();
                    this.boldFont = null;
                }
            }
        }

        [DefaultValue(TextQuality.Aliased)]
        [DispId(2)]
        [SRDescription("TextQualityDesc", DR.ResourceSet)]
        [SRCategory("WorkflowAppearanceCategory", DR.ResourceSet)]
        public virtual TextQuality TextQuality
        {
            get
            {
                return this.textQuality;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.textQuality = value;
            }
        }

        [DispId(3)]
        [DefaultValue(true)]
        [SRDescription("ShowConfigErrorDesc", DR.ResourceSet)]
        [SRCategory("WorkflowAppearanceCategory", DR.ResourceSet)]
        public virtual bool ShowConfigErrors
        {
            get
            {
                return this.showConfigErrors;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.showConfigErrors = value;
            }
        }

        [DefaultValue(false)]
        [DispId(6)]
        [SRDescription("GrayscaleWorkflowDesc", DR.ResourceSet)]
        [SRCategory("WorkflowAppearanceCategory", DR.ResourceSet)]
        public virtual bool DrawGrayscale
        {
            get
            {
                return this.drawGrayscale;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.drawGrayscale = value;
            }
        }
        #endregion

        #region Workflow Foreground Properties
        [DispId(7)]
        [SRDescription("DropHiliteDesc", DR.ResourceSet)]
        [SRCategory("ForegroundCategory", DR.ResourceSet)]
        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ColorPickerConverter))]
        public virtual Color DropIndicatorColor
        {
            get
            {
                return this.dropIndicatorColor;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.dropIndicatorColor = value;

                if (this.dropIndicatorPen != null)
                {
                    this.dropIndicatorPen.Dispose();
                    this.dropIndicatorPen = null;
                }

                if (this.dropIndicatorBrush != null)
                {
                    this.dropIndicatorBrush.Dispose();
                    this.dropIndicatorBrush = null;
                }
            }
        }

        [DispId(8)]
        [SRDescription("SelectionForegroundDesc", DR.ResourceSet)]
        [SRCategory("ForegroundCategory", DR.ResourceSet)]
        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ColorPickerConverter))]
        public virtual Color SelectionForeColor
        {
            get
            {
                return this.selectionForeColor;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.selectionForeColor = value;

                if (this.selectionForegroundPen != null)
                {
                    this.selectionForegroundPen.Dispose();
                    this.selectionForegroundPen = null;
                }

                if (this.selectionForegroundBrush != null)
                {
                    this.selectionForegroundBrush.Dispose();
                    this.selectionForegroundBrush = null;
                }
            }
        }

        [DispId(9)]
        [SRDescription("SelectionPatternDesc", DR.ResourceSet)]
        [SRCategory("ForegroundCategory", DR.ResourceSet)]
        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ColorPickerConverter))]
        public virtual Color SelectionPatternColor
        {
            get
            {
                return this.selectionPatternColor;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.selectionPatternColor = value;

                if (this.selectionPatternPen != null)
                {
                    this.selectionPatternPen.Dispose();
                    this.selectionPatternPen = null;
                }
            }
        }

        [DispId(10)]
        [SRDescription("WorkflowForegroundDesc", DR.ResourceSet)]
        [SRCategory("ForegroundCategory", DR.ResourceSet)]
        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ColorPickerConverter))]
        public virtual Color ForeColor
        {
            get
            {
                return this.foreColor;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.foreColor = value;

                if (this.foregroundPen != null)
                {
                    this.foregroundPen.Dispose();
                    this.foregroundPen = null;
                }

                if (this.foregroundBrush != null)
                {
                    this.foregroundBrush.Dispose();
                    this.foregroundBrush = null;
                }
            }
        }

        [DispId(11)]
        [SRDescription("CommentColorDesc", DR.ResourceSet)]
        [SRCategory("ForegroundCategory", DR.ResourceSet)]
        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ColorPickerConverter))]
        public virtual Color CommentIndicatorColor
        {
            get
            {
                return this.commentIndicatorColor;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.commentIndicatorColor = value;

                if (this.commentIndicatorPen != null)
                {
                    this.commentIndicatorPen.Dispose();
                    this.commentIndicatorPen = null;
                }

                if (this.commentIndicatorBrush != null)
                {
                    this.commentIndicatorBrush.Dispose();
                    this.commentIndicatorBrush = null;
                }
            }
        }

        [DispId(12)]
        [SRDescription("LockColorDesc", DR.ResourceSet)]
        [SRCategory("ForegroundCategory", DR.ResourceSet)]
        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ColorPickerConverter))]
        public virtual Color ReadonlyIndicatorColor
        {
            get
            {
                return this.readonlyIndicatorColor;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.readonlyIndicatorColor = value;

                if (this.readonlyIndicatorBrush != null)
                {
                    this.readonlyIndicatorBrush.Dispose();
                    this.readonlyIndicatorBrush = null;
                }
            }
        }
        #endregion

        #region Workflow Background Properties
        [DispId(13)]
        [SRDescription("WorkflowBackgroundDesc", DR.ResourceSet)]
        [SRCategory("BackgroundCategory", DR.ResourceSet)]
        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ColorPickerConverter))]
        public virtual Color BackColor
        {
            get
            {
                return this.backColor;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.backColor = value;

                if (this.backgroundBrush != null)
                {
                    this.backgroundBrush.Dispose();
                    this.backgroundBrush = null;
                }
            }
        }

        [DefaultValue(false)]
        [DispId(14)]
        [SRDescription("WorkflowShadowDesc", DR.ResourceSet)]
        [SRCategory("WorkflowAppearanceCategory", DR.ResourceSet)]
        public virtual bool DrawShadow
        {
            get
            {
                return this.drawShadow;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.drawShadow = value;
            }
        }


        [DispId(15)]
        [SRDescription("WorkflowWatermarkDesc", DR.ResourceSet)]
        [SRCategory("BackgroundCategory", DR.ResourceSet)]
        [Editor(typeof(ImageBrowserEditor), typeof(UITypeEditor))]
        public virtual string WatermarkImagePath
        {
            get
            {
                return this.watermarkImagePath;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                if (!String.IsNullOrEmpty(value) && value.Contains(Path.DirectorySeparatorChar.ToString()) && Path.IsPathRooted(value))
                {
                    value = DesignerHelpers.GetRelativePath(ContainingTheme.ContainingFileDirectory, value);

                    if (!DesignerHelpers.IsValidImageResource(this, ContainingTheme.ContainingFileDirectory, value))
                        throw new InvalidOperationException(DR.GetString(DR.Error_InvalidImageResource));
                }

                this.watermarkImagePath = value;
                if (this.watermarkImage != null)
                {
                    this.watermarkImage.Dispose();
                    this.watermarkImage = null;
                }
            }
        }

        [DispId(16)]
        [DefaultValue(DesignerContentAlignment.BottomRight)]
        [SRDescription("WatermarkAlignmentDesc", DR.ResourceSet)]
        [SRCategory("BackgroundCategory", DR.ResourceSet)]
        public virtual DesignerContentAlignment WatermarkAlignment
        {
            get
            {
                return this.watermarkAlignment;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.watermarkAlignment = value;
            }
        }

        [DefaultValue(false)]
        [DispId(17)]
        [SRDescription("ShowGridDesc", DR.ResourceSet)]
        [SRCategory("BackgroundCategory", DR.ResourceSet)]
        public virtual bool ShowGrid
        {
            get
            {
                return this.showGrid;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.showGrid = value;
            }
        }

        [DefaultValue(DashStyle.Dash)]
        [DispId(18)]
        [SRDescription("GridStyleDesc", DR.ResourceSet)]
        [SRCategory("BackgroundCategory", DR.ResourceSet)]
        public virtual DashStyle GridStyle
        {
            get
            {
                return this.gridStyle;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.gridStyle = value;

                if (this.majorGridPen != null)
                {
                    this.majorGridPen.Dispose();
                    this.majorGridPen = null;
                }

                if (this.minorGridPen != null)
                {
                    this.minorGridPen.Dispose();
                    this.minorGridPen = null;
                }
            }
        }

        [DispId(19)]
        [SRDescription("GridColorDesc", DR.ResourceSet)]
        [SRCategory("BackgroundCategory", DR.ResourceSet)]
        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ColorPickerConverter))]
        public virtual Color GridColor
        {
            get
            {
                return this.gridColor;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.gridColor = value;

                if (this.majorGridPen != null)
                {
                    this.majorGridPen.Dispose();
                    this.majorGridPen = null;
                }

                if (this.majorGridBrush != null)
                {
                    this.majorGridBrush.Dispose();
                    this.majorGridBrush = null;
                }

                if (this.minorGridPen != null)
                {
                    this.minorGridPen.Dispose();
                    this.minorGridPen = null;
                }

                if (this.minorGridBrush != null)
                {
                    this.minorGridBrush.Dispose();
                    this.minorGridBrush = null;
                }
            }
        }
        #endregion

        #endregion

        #region Activity Appearance Properties
        //

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(DesignerSize.Medium)]
        [DispId(20)]
        [SRDescription("DesignerSizeDesc", DR.ResourceSet)]
        [SRCategory("ActivityAppearanceCategory", DR.ResourceSet)]
        public virtual DesignerSize DesignerSize
        {
            get
            {
                return this.designerStyle;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.designerStyle = value;
                ContainingTheme.AmbientPropertyChanged(AmbientProperty.DesignerSize);
            }
        }

        [DefaultValue(true)]
        [DispId(21)]
        [SRDescription("DrawRoundedDesignersDesc", DR.ResourceSet)]
        [SRCategory("ActivityAppearanceCategory", DR.ResourceSet)]
        public virtual bool DrawRounded
        {
            get
            {
                return this.drawRounded;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.drawRounded = value;
            }
        }

        [DefaultValue(true)]
        [DispId(24)]
        [SRDescription("DesignerBorderDesc", DR.ResourceSet)]
        [SRCategory("ActivityAppearanceCategory", DR.ResourceSet)]
        public virtual bool ShowDesignerBorder
        {
            get
            {
                return this.showDesignerBorder;
            }

            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                this.showDesignerBorder = value;
            }
        }
        #endregion

        #region Helper Properties and Methods
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public virtual Size Margin
        {
            get
            {
                return AmbientTheme.MarginSizes[(int)this.designerStyle];
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public virtual Size SelectionSize
        {
            get
            {
                return AmbientTheme.SelectionSizes[(int)this.designerStyle];
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public virtual Size GlyphSize
        {
            get
            {
                return AmbientTheme.GlyphSizes[(int)this.designerStyle];
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        internal Size ScrollIndicatorSize
        {
            get
            {
                return AmbientTheme.ScrollIndicatorSizes[(int)this.designerStyle];
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        internal Size DropIndicatorSize
        {
            get
            {
                return AmbientTheme.DropIndicatorSizes[(int)this.designerStyle];
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        internal Size MagnifierSize
        {
            get
            {
                return AmbientTheme.MagnifierSizes[(int)this.designerStyle];
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        internal int ShadowDepth
        {
            get
            {
                return ((this.drawShadow) ? AmbientTheme.DefaultShadowDepth : 0);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public virtual Size GridSize
        {
            get
            {
                return AmbientTheme.GridSizes[(int)this.designerStyle];
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public virtual int BorderWidth
        {
            get
            {
                return AmbientTheme.BorderWidths[(int)ContainingTheme.AmbientTheme.DesignerSize];
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Pen MajorGridPen
        {
            get
            {
                if (this.majorGridPen == null)
                {
                    this.majorGridPen = new Pen(this.gridColor, 1);
                    this.majorGridPen.DashStyle = DashStyle.Dash;
                }
                return this.majorGridPen;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Brush MajorGridBrush
        {
            get
            {
                if (this.majorGridBrush == null)
                    this.majorGridBrush = new SolidBrush(this.gridColor);
                return this.majorGridBrush;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Pen MinorGridPen
        {
            get
            {
                if (this.minorGridPen == null)
                {
                    Color minorGridColor = Color.FromArgb(this.gridColor.A, Math.Min(this.gridColor.R + 32, 255), Math.Min(this.gridColor.G + 32, 255), Math.Min(this.gridColor.B + 32, 255));
                    this.minorGridPen = new Pen(minorGridColor, 1);
                    this.minorGridPen.DashStyle = DashStyle.Dot;
                }
                return this.minorGridPen;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        internal Brush MinorGridBrush
        {
            get
            {
                if (this.minorGridBrush == null)
                {
                    Color minorGridColor = Color.FromArgb(this.gridColor.A, Math.Min(this.gridColor.R + 32, 255), Math.Min(this.gridColor.G + 32, 255), Math.Min(this.gridColor.B + 32, 255));
                    this.minorGridBrush = new SolidBrush(minorGridColor);
                }
                return this.minorGridBrush;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Pen SelectionPatternPen
        {
            get
            {
                if (this.selectionPatternPen == null)
                {
                    this.selectionPatternPen = new Pen(this.selectionPatternColor, 1);
                    this.selectionPatternPen.DashStyle = DashStyle.Dot;
                }

                return this.selectionPatternPen;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Pen SelectionForegroundPen
        {
            get
            {
                if (this.selectionForegroundPen == null)
                    this.selectionForegroundPen = new Pen(this.selectionForeColor, 1);
                return this.selectionForegroundPen;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Brush SelectionForegroundBrush
        {
            get
            {
                if (this.selectionForegroundBrush == null)
                    this.selectionForegroundBrush = new SolidBrush(this.selectionForeColor);
                return this.selectionForegroundBrush;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Pen DropIndicatorPen
        {
            get
            {
                if (this.dropIndicatorPen == null)
                    this.dropIndicatorPen = new Pen(this.dropIndicatorColor, BorderWidth);
                return this.dropIndicatorPen;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Brush DropIndicatorBrush
        {
            get
            {
                if (this.dropIndicatorBrush == null)
                    this.dropIndicatorBrush = new SolidBrush(this.dropIndicatorColor);
                return this.dropIndicatorBrush;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Pen ForegroundPen
        {
            get
            {
                if (this.foregroundPen == null)
                {
                    this.foregroundPen = new Pen(this.foreColor, 1);
                    this.foregroundPen.DashStyle = DashStyle.Dot;
                }
                return this.foregroundPen;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Pen CommentIndicatorPen
        {
            get
            {
                if (this.commentIndicatorPen == null)
                    this.commentIndicatorPen = new Pen(this.commentIndicatorColor, 1);
                return this.commentIndicatorPen;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Brush CommentIndicatorBrush
        {
            get
            {
                if (this.commentIndicatorBrush == null)
                    this.commentIndicatorBrush = new SolidBrush(Color.FromArgb(40, this.commentIndicatorColor));
                return this.commentIndicatorBrush;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Brush ReadonlyIndicatorBrush
        {
            get
            {
                if (this.readonlyIndicatorBrush == null)
                    this.readonlyIndicatorBrush = new SolidBrush(Color.FromArgb(20, this.readonlyIndicatorColor));
                return this.readonlyIndicatorBrush;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Brush ForegroundBrush
        {
            get
            {
                if (this.foregroundBrush == null)
                    this.foregroundBrush = new SolidBrush(this.foreColor);
                return this.foregroundBrush;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Brush BackgroundBrush
        {
            get
            {
                if (this.backgroundBrush == null)
                    this.backgroundBrush = new SolidBrush(this.backColor);
                return this.backgroundBrush;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Image WorkflowWatermarkImage
        {
            get
            {
                if (this.watermarkImage == null && this.watermarkImagePath.Length > 0)
                    this.watermarkImage = DesignerHelpers.GetImageFromPath(this, ContainingTheme.ContainingFileDirectory, this.watermarkImagePath);
                return this.watermarkImage;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Font Font
        {
            get
            {
                if (this.font == null)
                {
                    Debug.Assert(this.fontName != null && this.fontName.Length > 0);
                    if (this.fontName == null || this.fontName.Length == 0)
                        this.fontName = WorkflowTheme.GetDefaultFont().FontFamily.Name;

                    ArrayList supportedFonts = new ArrayList(AmbientTheme.SupportedFonts);
                    Debug.Assert(supportedFonts.Contains(this.fontName));
                    if (!supportedFonts.Contains(this.fontName))
                        this.fontName = WorkflowTheme.GetDefaultFont().FontFamily.Name;

                    this.font = new Font(this.fontName, FontSize);
                }
                return this.font;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Font BoldFont
        {
            get
            {
                if (this.boldFont == null)
                {
                    Debug.Assert(this.fontName != null && this.fontName.Length > 0);
                    if (this.fontName == null || this.fontName.Length == 0)
                        this.fontName = WorkflowTheme.GetDefaultFont().FontFamily.Name;

                    ArrayList supportedFonts = new ArrayList(AmbientTheme.SupportedFonts);
                    Debug.Assert(supportedFonts.Contains(this.fontName));
                    if (!supportedFonts.Contains(this.fontName))
                        this.fontName = WorkflowTheme.GetDefaultFont().FontFamily.Name;

                    this.boldFont = new Font(this.fontName, FontSize, FontStyle.Bold);
                }
                return this.boldFont;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private float FontSize
        {
            get
            {
                //

                if (this.useOperatingSystemSettings)
                    return SystemInformation.MenuFont.SizeInPoints;
                else
                    return AmbientTheme.FontSizes[(int)this.DesignerSize];
            }
        }

        internal void UseDefaultFont()
        {
            this.useDefaultFont = true;
        }

        internal void UpdateFont()
        {
            if (this.useDefaultFont)
            {
                bool oldReadOnly = ReadOnly;
                ReadOnly = false;
                this.FontName = WorkflowTheme.GetDefaultFont().FontFamily.Name;
                ReadOnly = oldReadOnly;
            }
        }

        public override void OnAmbientPropertyChanged(AmbientProperty ambientProperty)
        {
            base.OnAmbientPropertyChanged(ambientProperty);

            if (ambientProperty == AmbientProperty.DesignerSize)
            {
                //We set the same properties again so that the pens and brushes are reset to 
                //consider the new designer style for creation
                DropIndicatorColor = this.dropIndicatorColor;
                FontName = this.fontName;
            }
            else if (ambientProperty == AmbientProperty.OperatingSystemSetting)
            {
                //Apply the system colors
                ApplySystemColors();
            }
        }

        internal static string[] SupportedFonts
        {
            get
            {
                ArrayList fontNames = new ArrayList();
                foreach (FontFamily family in FontFamily.Families)
                    fontNames.Add(family.Name);
                fontNames.Sort(CaseInsensitiveComparer.Default);
                return ((string[])fontNames.ToArray(typeof(string)));
            }
        }

        private void OnOperatingSystemSettingsChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Color || e.Category == UserPreferenceCategory.VisualStyle)
            {
                ContainingTheme.AmbientPropertyChanged(AmbientProperty.OperatingSystemSetting);
                WorkflowTheme.FireThemeChange();
            }
        }

        private void ApplySystemColors()
        {
            DropIndicatorColor = SystemColors.HotTrack;
            SelectionForeColor = SystemColors.Highlight;
            SelectionPatternColor = SystemColors.Highlight;
            ForeColor = SystemColors.WindowText;
            CommentIndicatorColor = SystemColors.GrayText;
            ReadonlyIndicatorColor = SystemColors.GrayText;
            BackColor = SystemColors.Window;
            GridColor = SystemColors.InactiveBorder;
            FontName = WorkflowTheme.GetDefaultFont().FontFamily.Name;
        }
        #endregion

        #region IPropertyValueProvider Implementation
        internal override ICollection GetPropertyValues(ITypeDescriptorContext context)
        {
            object[] values = new object[] { };
            if (string.Equals(context.PropertyDescriptor.Name, "GridStyle", StringComparison.Ordinal))
                values = new object[] { DashStyle.Solid, DashStyle.Dash, DashStyle.Dot };
            return values;
        }
        #endregion
    }
    #endregion

    #region TypeConverters, TypeDescriptors, PropertyDescriptors, UITypeEditors

    #region Class ThemeTypeDescriptor
    internal sealed class ThemeTypeConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && context.PropertyDescriptor != null)
                return String.Empty;
            else
                return base.ConvertTo(context, culture, value, destinationType);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection srcProperties = base.GetProperties(context, value, attributes);
            return srcProperties.Sort(new PropertyDescriptorSorter());
        }

        #region Class PropertyDescriptorSorter
        private sealed class PropertyDescriptorSorter : IComparer
        {
            int IComparer.Compare(object obj1, object obj2)
            {
                PropertyDescriptor property1 = obj1 as PropertyDescriptor;
                PropertyDescriptor property2 = obj2 as PropertyDescriptor;
                DispIdAttribute prop1DispID = property1.Attributes[typeof(DispIdAttribute)] as DispIdAttribute;
                DispIdAttribute prop2DispID = property2.Attributes[typeof(DispIdAttribute)] as DispIdAttribute;

                if (prop1DispID == null)
                    return 1;
                else if (prop2DispID == null)
                    return -1;
                else
                    return prop1DispID.Value - prop2DispID.Value;
            }
        }
        #endregion
    }
    #endregion

    #region Class ColorPickerConverter
    internal sealed class ColorPickerConverter : ColorConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }
    #endregion

    #region Class FilteredEnumConverter
    internal sealed class FilteredEnumConverter : PropertyValueProviderTypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return Enum.Parse(context.PropertyDescriptor.PropertyType, (string)value);
        }
    }
    #endregion

    #region Class FontFamilyConverter
    internal sealed class FontFamilyConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new TypeConverter.StandardValuesCollection(AmbientTheme.SupportedFonts);
        }
    }
    #endregion

    #region Class ImageBrowserEditor
    internal sealed class ImageBrowserEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.AddExtension = true;
            fileDialog.DefaultExt = WorkflowTheme.DefaultThemeFileExtension;
            fileDialog.CheckFileExists = true;
            fileDialog.Filter = DR.GetString(DR.ImageFileFilter);
            if (fileDialog.ShowDialog() == DialogResult.OK)
                return fileDialog.FileName;
            else
                return value;
        }
    }
    #endregion

    #region Class ColorPickerEditor
    internal sealed class ColorPickerEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return (context != null && context.PropertyDescriptor != null);
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            base.PaintValue(e);

            if (e.Value is Color)
            {
                Color selectedColor = (Color)e.Value;
                if (selectedColor != Color.Empty)
                {
                    using (Brush fillBrush = new SolidBrush(selectedColor))
                        e.Graphics.FillRectangle(fillBrush, e.Bounds);
                }
            }
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.AllowFullOpen = true;
            colorDialog.FullOpen = true;
            colorDialog.AnyColor = true;
            colorDialog.Color = (value is Color) ? (Color)value : Color.White;
            if (colorDialog.ShowDialog() == DialogResult.OK)
                return colorDialog.Color;
            else
                return value;
        }
    }
    #endregion

    #endregion

    #region Serializers

    #region Class ThemeSerializationProvider
    internal sealed class ThemeSerializationProvider : WorkflowMarkupSerializationProvider
    {
        #region IDesignerSerializationProvider Members
        public override object GetSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType)
        {
            if (serializerType.IsAssignableFrom(typeof(WorkflowMarkupSerializer)))
            {
                if (typeof(System.Drawing.Color) == objectType)
                    return new ColorMarkupSerializer();
                else if (typeof(Size) == objectType)
                    return new SizeMarkupSerializer();
            }

            return base.GetSerializer(manager, currentSerializer, objectType, serializerType);
        }
        #endregion
    }
    #endregion

    #endregion
}
