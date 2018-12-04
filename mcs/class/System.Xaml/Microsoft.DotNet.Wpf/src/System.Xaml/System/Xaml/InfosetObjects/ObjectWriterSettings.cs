using System;
using System.Collections.Generic;
using System.Text;

namespace System.Xaml
{
#if SILVERLIGHTXAML
    internal
#else
    public
#endif
    class ObjectWriterSettings
    {
        public EventHandler<XamlCreatedObjectEventArgs> ObjectCreatedHandler { get; set; }
        public Object RootObjectInstance { get; set; }
        public bool IgnoreCanConvert { get; set; }
        public System.Windows.Markup.INameScope NameScope { get; set; }
    }
}
