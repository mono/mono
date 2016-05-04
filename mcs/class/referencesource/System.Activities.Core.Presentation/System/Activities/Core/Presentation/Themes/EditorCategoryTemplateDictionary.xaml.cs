//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Core.Presentation.Themes
{
    using System.Windows;
    using System.Runtime;
    using System.Activities.Presentation.View;

    sealed partial class EditorCategoryTemplateDictionary
    {
        static EditorCategoryTemplateDictionary instance;

        public EditorCategoryTemplateDictionary()
        {
            InitializeComponent();
        }

        public static EditorCategoryTemplateDictionary Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new EditorCategoryTemplateDictionary();
                }
                return instance;
            }
        }

        public DataTemplate GetCategoryTemplate(string templateName)
        {
            DataTemplate result = null;
            if (this.Contains(templateName))
            {
                if (!(this[templateName] is DataTemplate))
                {
                    Fx.Assert(false, "'" + templateName + "' is not a DataTemplate");
                }
                result = this[templateName] as DataTemplate;
            }
            else
            {
                Fx.Assert(false, "DataTemplate '" + templateName + "' not found");
            }
            return result;
        }

        public string GetCategoryTitle(string categoryName)
        {
            string result = string.Empty;
            if (this.Contains(categoryName))
            {
                if (!(this[categoryName] is string))
                {
                    Fx.Assert(false, "'" + categoryName + "' is not a string");
                }
                result = this[categoryName] as string;
            }
            else
            {
                Fx.Assert(false, "Category title for '" + categoryName + "' not found");
            }
            return result;
        }

        public object GetCategoryImage(string imageName)
        {
            object result = null;
            if (this.Contains(imageName))
            {
                result = this[imageName];
            }
            else
            {
                Fx.Assert(false, "No resource with key '" + imageName + "'");
            }
            return result;
        }
    }
}
