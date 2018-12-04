using System;
using System.Collections.Generic;
using System.Text;

namespace System.Xaml
{
    public interface IXamlSavedContextProvider
    {
        XamlSavedContext GetSavedContext();
        XamlSavedContext GetSavedContext(object instance);
    }
}
