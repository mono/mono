// Decompiled with JetBrains decompiler
// Type: System.Web.ModelBinding.DisplayAttributeAdapter
// Assembly: System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: 0C6883DC-A0C9-4219-BD64-501FA87809D2
// Assembly location: C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Web.dll

using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Web.Globalization;

namespace System.Web.ModelBinding
{
  internal sealed class DisplayAttributeAdapter
  {
    private DisplayAttribute _displayAttribute;

    public DisplayAttributeAdapter(DisplayAttribute displayAttribute)
    {
      if (displayAttribute == null)
        throw new ArgumentNullException(nameof (displayAttribute));
      this._displayAttribute = displayAttribute;
    }

    public string GetDescription()
    {
      return this.GetLocalizedString(this._displayAttribute.Description) ?? this._displayAttribute.GetDescription();
    }

    public string GetShortName()
    {
      return this.GetLocalizedString(this._displayAttribute.ShortName) ?? this._displayAttribute.GetShortName();
    }

    public string GetPrompt()
    {
      return this.GetLocalizedString(this._displayAttribute.Prompt) ?? this._displayAttribute.GetPrompt();
    }

    public string GetName()
    {
      return this.GetLocalizedString(this._displayAttribute.Name) ?? this._displayAttribute.GetName();
    }

    public int? GetOrder()
    {
      return this._displayAttribute.GetOrder();
    }

    private string GetLocalizedString(string name)
    {
      if (this._displayAttribute.ResourceType != (Type) null)
        return (string) null;
      return StringLocalizerProviders.DataAnnotationStringLocalizerProvider.GetLocalizedString(Thread.CurrentThread.CurrentUICulture, name);
    }
  }
}
