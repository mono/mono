using System.Globalization;
using System.Resources;
using System.Security.Permissions;
using System.Threading;

namespace System.Web.UI.MobileControls.Adapters
{
  [AspNetHostingPermission(SecurityAction.LinkDemand, Unrestricted = true)]
  [AspNetHostingPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
  public class SR
  {
    public const string CalendarAdapterFirstPrompt = "CalendarAdapterFirstPrompt";
    public const string CalendarAdapterOptionPrompt = "CalendarAdapterOptionPrompt";
    public const string CalendarAdapterOptionType = "CalendarAdapterOptionType";
    public const string CalendarAdapterOptionEra = "CalendarAdapterOptionEra";
    public const string CalendarAdapterOptionChooseDate = "CalendarAdapterOptionChooseDate";
    public const string CalendarAdapterOptionChooseWeek = "CalendarAdapterOptionChooseWeek";
    public const string CalendarAdapterOptionChooseMonth = "CalendarAdapterOptionChooseMonth";
    public const string CalendarAdapterTextBoxErrorMessage = "CalendarAdapterTextBoxErrorMessage";
    public const string ChtmlImageAdapterDecimalCodeExpectedAfterGroupChar = "ChtmlImageAdapterDecimalCodeExpectedAfterGroupChar";
    public const string ChtmlPageAdapterRedirectPageContent = "ChtmlPageAdapterRedirectPageContent";
    public const string ChtmlPageAdapterRedirectLinkLabel = "ChtmlPageAdapterRedirectLinkLabel";
    public const string ControlAdapterBasePagePropertyShouldNotBeSet = "ControlAdapterBasePagePropertyShouldNotBeSet";
    public const string FormAdapterMultiControlsAttemptSecondaryUI = "FormAdapterMultiControlsAttemptSecondaryUI";
    public const string MobileTextWriterNotMultiPart = "MobileTextWriterNotMultiPart";
    public const string ObjectListAdapter_InvalidPostedData = "ObjectListAdapter_InvalidPostedData";
    public const string WmlMobileTextWriterBackLabel = "WmlMobileTextWriterBackLabel";
    public const string WmlMobileTextWriterOKLabel = "WmlMobileTextWriterOKLabel";
    public const string WmlMobileTextWriterGoLabel = "WmlMobileTextWriterGoLabel";
    public const string WmlPageAdapterServerError = "WmlPageAdapterServerError";
    public const string WmlPageAdapterStackTrace = "WmlPageAdapterStackTrace";
    public const string WmlPageAdapterPartialStackTrace = "WmlPageAdapterPartialStackTrace";
    public const string WmlPageAdapterMethod = "WmlPageAdapterMethod";
    public const string WmlObjectListAdapterDetails = "WmlObjectListAdapterDetails";
    public const string XhtmlCssHandler_IdNotPresent = "XhtmlCssHandler_IdNotPresent";
    public const string XhtmlCssHandler_StylesheetNotFound = "XhtmlCssHandler_StylesheetNotFound";
    public const string XhtmlObjectListAdapter_InvalidPostedData = "XhtmlObjectListAdapter_InvalidPostedData";
    public const string XhtmlMobileTextWriter_SessionKeyNotSet = "XhtmlMobileTextWriter_SessionKeyNotSet";
    public const string XhtmlMobileTextWriter_CacheKeyNotSet = "XhtmlMobileTextWriter_CacheKeyNotSet";
    private static SR loader;
    private ResourceManager resources;

    public SR()
    {
      this.resources = new ResourceManager("System.Web.UI.MobileControls.Adapters", this.GetType().Assembly);
    }

    private static SR GetLoader()
    {
      if (SR.loader == null)
      {
        SR sr = new SR();
        Interlocked.CompareExchange<SR>(ref SR.loader, sr, (SR) null);
      }
      return SR.loader;
    }

    private static CultureInfo Culture
    {
      get
      {
        return (CultureInfo) null;
      }
    }

    public static string GetString(string name, params object[] args)
    {
      return SR.GetString(SR.Culture, name, args);
    }

    public static string GetString(CultureInfo culture, string name, params object[] args)
    {
      SR loader = SR.GetLoader();
      if (loader == null)
        return (string) null;
      string format = loader.resources.GetString(name, culture);
      if (args == null || args.Length == 0)
        return format;
      for (int index = 0; index < args.Length; ++index)
      {
        string str = args[index] as string;
        if (str != null && str.Length > 1024)
          args[index] = (object) (str.Substring(0, 1021) + "...");
      }
      return string.Format((IFormatProvider) CultureInfo.CurrentCulture, format, args);
    }

    public static string GetString(string name)
    {
      return SR.GetString(SR.Culture, name);
    }

    public static string GetString(CultureInfo culture, string name)
    {
      SR loader = SR.GetLoader();
      if (loader == null)
        return (string) null;
      return loader.resources.GetString(name, culture);
    }

    public static bool GetBoolean(string name)
    {
      return SR.GetBoolean(SR.Culture, name);
    }

    public static bool GetBoolean(CultureInfo culture, string name)
    {
      bool flag = false;
      SR loader = SR.GetLoader();
      if (loader != null)
      {
        object obj = loader.resources.GetObject(name, culture);
        if (obj is bool)
          flag = (bool) obj;
      }
      return flag;
    }

    public static char GetChar(string name)
    {
      return SR.GetChar(SR.Culture, name);
    }

    public static char GetChar(CultureInfo culture, string name)
    {
      char ch = char.MinValue;
      SR loader = SR.GetLoader();
      if (loader != null)
      {
        object obj = loader.resources.GetObject(name, culture);
        if (obj is char)
          ch = (char) obj;
      }
      return ch;
    }

    public static byte GetByte(string name)
    {
      return SR.GetByte(SR.Culture, name);
    }

    public static byte GetByte(CultureInfo culture, string name)
    {
      byte num = 0;
      SR loader = SR.GetLoader();
      if (loader != null)
      {
        object obj = loader.resources.GetObject(name, culture);
        if (obj is byte)
          num = (byte) obj;
      }
      return num;
    }

    public static short GetShort(string name)
    {
      return SR.GetShort(SR.Culture, name);
    }

    public static short GetShort(CultureInfo culture, string name)
    {
      short num = 0;
      SR loader = SR.GetLoader();
      if (loader != null)
      {
        object obj = loader.resources.GetObject(name, culture);
        if (obj is short)
          num = (short) obj;
      }
      return num;
    }

    public static int GetInt(string name)
    {
      return SR.GetInt(SR.Culture, name);
    }

    public static int GetInt(CultureInfo culture, string name)
    {
      int num = 0;
      SR loader = SR.GetLoader();
      if (loader != null)
      {
        object obj = loader.resources.GetObject(name, culture);
        if (obj is int)
          num = (int) obj;
      }
      return num;
    }

    public static long GetLong(string name)
    {
      return SR.GetLong(SR.Culture, name);
    }

    public static long GetLong(CultureInfo culture, string name)
    {
      long num = 0;
      SR loader = SR.GetLoader();
      if (loader != null)
      {
        object obj = loader.resources.GetObject(name, culture);
        if (obj is long)
          num = (long) obj;
      }
      return num;
    }

    public static float GetFloat(string name)
    {
      return SR.GetFloat(SR.Culture, name);
    }

    public static float GetFloat(CultureInfo culture, string name)
    {
      float num = 0.0f;
      SR loader = SR.GetLoader();
      if (loader == null)
      {
        object obj = loader.resources.GetObject(name, culture);
        if (obj is float)
          num = (float) obj;
      }
      return num;
    }

    public static double GetDouble(string name)
    {
      return SR.GetDouble(SR.Culture, name);
    }

    public static double GetDouble(CultureInfo culture, string name)
    {
      double num = 0.0;
      SR loader = SR.GetLoader();
      if (loader == null)
      {
        object obj = loader.resources.GetObject(name, culture);
        if (obj is double)
          num = (double) obj;
      }
      return num;
    }

    public static object GetObject(string name)
    {
      return SR.GetObject(SR.Culture, name);
    }

    public static object GetObject(CultureInfo culture, string name)
    {
      SR loader = SR.GetLoader();
      if (loader == null)
        return (object) null;
      return loader.resources.GetObject(name, culture);
    }
  }
}
