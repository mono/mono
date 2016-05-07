//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Core.Presentation.Themes
{

    sealed partial class StringResourceDictionary
    {
        static StringResourceDictionary instance;

        public StringResourceDictionary()
        {
            InitializeComponent();
        }

        public static StringResourceDictionary Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new StringResourceDictionary();
                }
                return instance;
            }
        }

        public string GetString(string key)
        {
            return (string)(this.Contains(key) ? this[key] : null);
        }

        public string GetString(string key, string defaultValue)
        {
            return (string)(this.Contains(key) ? this[key] : defaultValue);
        }

    }
}
