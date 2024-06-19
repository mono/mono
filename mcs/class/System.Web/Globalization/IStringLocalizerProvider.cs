using System.Globalization;

namespace System.Web.Globalization
{
  public interface IStringLocalizerProvider
  {
    string GetLocalizedString(CultureInfo culture, string name, params object[] arguments);
  }
}