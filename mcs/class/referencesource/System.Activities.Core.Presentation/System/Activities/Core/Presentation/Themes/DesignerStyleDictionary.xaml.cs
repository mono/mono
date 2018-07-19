//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Core.Presentation.Themes
{
    using System.Windows;
    using System.Runtime;

    sealed partial class DesignerStylesDictionary
    {
        static DesignerStylesDictionary instance;
        
        internal DesignerStylesDictionary()
        {
            InitializeComponent();
        }

        static DesignerStylesDictionary Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new DesignerStylesDictionary();
                }
                return instance;
            }
        }

        public static Style SequenceStyle
        {
            get
            {
                var key = "SequenceStyle";
                if (!Instance.Contains(key))
                {
                    throw FxTrace.Exception.ArgumentNull(key);
                }
                var style = (Style)Instance[key];
                if (!style.IsSealed)
                {
                    style.Seal();
                }
                return style;
            }
        }

    }
}
