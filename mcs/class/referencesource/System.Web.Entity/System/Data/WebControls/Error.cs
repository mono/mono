namespace System.Web.UI.WebControls
{
  internal static class Error
  {
    internal static Exception ArgumentNull(string paramName)
    {
      return (Exception) new ArgumentNullException(paramName);
    }

    internal static Exception ArgumentOutOfRange(string paramName)
    {
      return (Exception) new ArgumentOutOfRangeException(paramName);
    }

    internal static Exception NotImplemented()
    {
      return (Exception) new NotImplementedException();
    }

    internal static Exception NotSupported()
    {
      return (Exception) new NotSupportedException();
    }
  }
}