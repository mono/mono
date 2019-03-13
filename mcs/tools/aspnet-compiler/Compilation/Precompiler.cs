// Decompiled with JetBrains decompiler
// Type: System.Web.Compilation.Precompiler
// Assembly: aspnet_compiler, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// MVID: B2801FB6-66A2-4131-A8D2-5C31DC70E77B
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\aspnet_compiler.exe

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;

namespace System.Web.Compilation
{
  internal class Precompiler
  {
    private static int maxLineLength = 80;
    private static readonly char[] invalidVirtualPathChars = new char[2]
    {
      '*',
      '?'
    };
    private static ClientBuildManager _client;
    private static string _sourcePhysicalDir;
    private static string _metabasePath;
    private static string _sourceVirtualDir;
    private static string _targetPhysicalDir;
    private static string _keyFile;
    private static string _keyContainer;
    private static PrecompilationFlags _precompilationFlags;
    private static bool _showErrorStack;
    private static List<string> _excludedVirtualPaths;
    private const int leftMargin = 14;

    public static int Main(string[] args)
    {
      Precompiler._excludedVirtualPaths = new List<string>();
      bool flag = false;
      try
      {
        Precompiler.maxLineLength = Console.BufferWidth;
      }
      catch
      {
      }
      Precompiler.SetThreadUICulture();
      for (int index = 0; index < args.Length; ++index)
      {
        string lower = args[index].ToLower(CultureInfo.InvariantCulture);
        if (lower == "-nologo" || lower == "/nologo")
          flag = true;
      }
      if (!flag)
      {
        Console.WriteLine(string.Format((IFormatProvider) CultureInfo.CurrentCulture, CompilerResources.brand_text, new object[1]
        {
          (object) "4.7.3056.0"
        }));
        Console.WriteLine(CompilerResources.header_text);
        Console.WriteLine(CompilerResources.copyright);
        Console.WriteLine();
      }
      if (args.Length == 0)
      {
        Console.WriteLine(CompilerResources.short_usage_text);
        return 1;
      }
      if (!Precompiler.ValidateArgs(args))
        return 1;
      try
      {
        if (Precompiler._sourceVirtualDir == null)
          Precompiler._sourceVirtualDir = Precompiler._metabasePath;
        ClientBuildManagerParameter parameter = new ClientBuildManagerParameter();
        parameter.PrecompilationFlags = Precompiler._precompilationFlags;
        parameter.StrongNameKeyFile = Precompiler._keyFile;
        parameter.StrongNameKeyContainer = Precompiler._keyContainer;
        parameter.ExcludedVirtualPaths.AddRange((IEnumerable<string>) Precompiler._excludedVirtualPaths);
        Precompiler._client = new ClientBuildManager(Precompiler._sourceVirtualDir, Precompiler._sourcePhysicalDir, Precompiler._targetPhysicalDir, parameter);
        Precompiler._client.PrecompileApplication((ClientBuildManagerCallback) new Precompiler.CBMCallback());
        return 0;
      }
      catch (FileLoadException ex)
      {
        if ((Precompiler._precompilationFlags & PrecompilationFlags.DelaySign) != PrecompilationFlags.Default && (int) typeof (FileLoadException).GetProperty("HResult", BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true).Invoke((object) ex, (object[]) null) == -2146233318)
        {
          Precompiler.DumpErrors((Exception) new FileLoadException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, CompilerResources.Strongname_failure, new object[1]
          {
            (object) ex.FileName
          }), ex.FileName, (Exception) ex));
          return 1;
        }
        Precompiler.DumpErrors((Exception) ex);
      }
      catch (Exception ex)
      {
        Precompiler.DumpErrors(ex);
      }
      return 1;
    }

    private static void SetThreadUICulture()
    {
      Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentUICulture.GetConsoleFallbackUICulture();
      if (Console.OutputEncoding.CodePage == 65001 || Console.OutputEncoding.CodePage == Thread.CurrentThread.CurrentUICulture.TextInfo.OEMCodePage || Console.OutputEncoding.CodePage == Thread.CurrentThread.CurrentUICulture.TextInfo.ANSICodePage)
        return;
      Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
    }

    private static void DisplayUsage()
    {
      Console.WriteLine(CompilerResources.usage);
      Console.WriteLine("aspnet_compiler [-?] [-m metabasePath | -v virtualPath [-p physicalDir]]");
      Console.WriteLine("                [[-u] [-f] [-d] [-fixednames] targetDir] [-c]");
      Console.WriteLine("                [-x excludeVirtualPath [...]]");
      Console.WriteLine("                [[-keyfile file | -keycontainer container]");
      Console.WriteLine("                     [-aptca] [-delaySign]]");
      Console.WriteLine("                [-errorstack]");
      Console.WriteLine();
      Precompiler.DisplaySwitchWithHelp("-?", CompilerResources.questionmark_help);
      Precompiler.DisplaySwitchWithHelp("-m", CompilerResources.m_help);
      Precompiler.DisplaySwitchWithHelp("-v", CompilerResources.v_help);
      Precompiler.DisplaySwitchWithHelp("-p", CompilerResources.p_help);
      Precompiler.DisplaySwitchWithHelp("-u", CompilerResources.u_help);
      Precompiler.DisplaySwitchWithHelp("-f", CompilerResources.f_help);
      Precompiler.DisplaySwitchWithHelp("-d", CompilerResources.d_help);
      Precompiler.DisplaySwitchWithHelp("targetDir", CompilerResources.targetDir_help);
      Precompiler.DisplaySwitchWithHelp("-c", CompilerResources.c_help);
      Precompiler.DisplaySwitchWithHelp("-x", CompilerResources.x_help);
      Precompiler.DisplaySwitchWithHelp("-keyfile", CompilerResources.keyfile_help);
      Precompiler.DisplaySwitchWithHelp("-keycontainer", CompilerResources.keycontainer_help);
      Precompiler.DisplaySwitchWithHelp("-aptca", CompilerResources.aptca_help);
      Precompiler.DisplaySwitchWithHelp("-delaysign", CompilerResources.delaysign_help);
      Precompiler.DisplaySwitchWithHelp("-fixednames", CompilerResources.fixednames_help);
      Precompiler.DisplaySwitchWithHelp("-nologo", CompilerResources.nologo_help);
      Precompiler.DisplaySwitchWithHelp("-errorstack", CompilerResources.errorstack_help);
      Console.WriteLine();
      Console.WriteLine(CompilerResources.examples);
      Console.WriteLine();
      Precompiler.DisplayWordWrappedString(CompilerResources.example1);
      Console.WriteLine("    aspnet_compiler -m /LM/W3SVC/1/Root/MyApp c:\\MyTarget");
      Console.WriteLine("    aspnet_compiler -v /MyApp c:\\MyTarget");
      Console.WriteLine();
      Precompiler.DisplayWordWrappedString(CompilerResources.example2);
      Console.WriteLine("    aspnet_compiler -v /MyApp");
      Console.WriteLine();
      Precompiler.DisplayWordWrappedString(CompilerResources.example3);
      Console.WriteLine("    aspnet_compiler -v /MyApp -p c:\\myapp c:\\MyTarget");
      Console.WriteLine();
    }

    private static void DisplaySwitchWithHelp(string switchString, string stringHelpString)
    {
      Console.Write(switchString);
      Precompiler.DisplayWordWrappedString(stringHelpString, switchString.Length, 14);
    }

    private static void DisplayWordWrappedString(string s)
    {
      Precompiler.DisplayWordWrappedString(s, 0, 0);
    }

    private static void DisplayWordWrappedString(string s, int currentOffset, int leftMargin)
    {
      string[] strArray = s.Split(' ');
      bool flag = true;
      foreach (string str in strArray)
      {
        int length = str.Length;
        if (!flag)
          ++length;
        if (currentOffset + length >= Precompiler.maxLineLength)
        {
          Console.WriteLine();
          currentOffset = 0;
          flag = true;
        }
        if (flag)
        {
          for (; currentOffset < leftMargin; ++currentOffset)
            Console.Write(' ');
        }
        else
        {
          Console.Write(' ');
          ++currentOffset;
        }
        Console.Write(str);
        currentOffset += str.Length;
        flag = false;
      }
      Console.WriteLine();
    }

    private static string GetNextArgument(string[] args, ref int index)
    {
      if (index != args.Length - 1)
        return args[++index];
      Console.WriteLine(string.Format((IFormatProvider) CultureInfo.CurrentCulture, CompilerResources.missing_arg, new object[1]
      {
        (object) args[index]
      }));
      return (string) null;
    }

    private static bool ValidateArgs(string[] args)
    {
      if (args.Length == 0)
        return false;
      for (int index = 0; index < args.Length; ++index)
      {
        string str = args[index];
        if ((int) str[0] != 47 && (int) str[0] != 45)
        {
          if (Precompiler._targetPhysicalDir == null)
          {
            Precompiler._targetPhysicalDir = str;
            Precompiler._targetPhysicalDir = Precompiler.GetFullPath(Precompiler._targetPhysicalDir);
            if (Precompiler._targetPhysicalDir == null)
              return false;
          }
          else
          {
            Precompiler.DumpError("1001", string.Format((IFormatProvider) CultureInfo.CurrentCulture, CompilerResources.unexpected_param, new object[1]
            {
              (object) str
            }));
            return false;
          }
        }
        else
        {
          switch (str.Substring(1).ToLower(CultureInfo.InvariantCulture))
          {
            case "?":
              Precompiler.DisplayUsage();
              return false;
            case "aptca":
              Precompiler._precompilationFlags |= PrecompilationFlags.AllowPartiallyTrustedCallers;
              continue;
            case "c":
              Precompiler._precompilationFlags |= PrecompilationFlags.Clean;
              continue;
            case "d":
              Precompiler._precompilationFlags |= PrecompilationFlags.ForceDebug;
              continue;
            case "delaysign":
              Precompiler._precompilationFlags |= PrecompilationFlags.DelaySign;
              continue;
            case "errorstack":
              Precompiler._showErrorStack = true;
              continue;
            case "f":
              Precompiler._precompilationFlags |= PrecompilationFlags.OverwriteTarget;
              continue;
            case "fixednames":
              Precompiler._precompilationFlags |= PrecompilationFlags.FixedNames;
              continue;
            case "keycontainer":
              Precompiler._keyContainer = Precompiler.GetNextArgument(args, ref index);
              if (Precompiler._keyContainer == null)
                return false;
              continue;
            case "keyfile":
              Precompiler._keyFile = Precompiler.GetNextArgument(args, ref index);
              if (Precompiler._keyFile == null)
                return false;
              if (!File.Exists(Precompiler._keyFile))
              {
                Precompiler.DumpError("1012", string.Format((IFormatProvider) CultureInfo.CurrentCulture, CompilerResources.invalid_keyfile, new object[1]
                {
                  (object) Precompiler._keyFile
                }));
                return false;
              }
              Precompiler._keyFile = Path.GetFullPath(Precompiler._keyFile);
              continue;
            case "m":
              Precompiler._metabasePath = Precompiler.GetNextArgument(args, ref index);
              if (Precompiler._metabasePath == null)
                return false;
              continue;
            case "nologo":
              continue;
            case "p":
              Precompiler._sourcePhysicalDir = Precompiler.GetNextArgument(args, ref index);
              if (Precompiler._sourcePhysicalDir == null)
                return false;
              Precompiler._sourcePhysicalDir = Precompiler.GetFullPath(Precompiler._sourcePhysicalDir);
              if (Precompiler._sourcePhysicalDir == null)
                return false;
              if (!Directory.Exists(Precompiler._sourcePhysicalDir))
              {
                Precompiler.DumpError("1003", string.Format((IFormatProvider) CultureInfo.CurrentCulture, CompilerResources.dir_not_exist, new object[1]
                {
                  (object) Precompiler._sourcePhysicalDir
                }));
                return false;
              }
              continue;
            case "u":
              Precompiler._precompilationFlags |= PrecompilationFlags.Updatable;
              continue;
            case "v":
              Precompiler._sourceVirtualDir = Precompiler.GetNextArgument(args, ref index);
              if (Precompiler._sourceVirtualDir == null)
                return false;
              if (!Precompiler.IsValidVirtualPath(Precompiler._sourceVirtualDir))
              {
                Precompiler.DumpError("1011", string.Format((IFormatProvider) CultureInfo.CurrentCulture, CompilerResources.invalid_vpath, new object[1]
                {
                  (object) Precompiler._sourceVirtualDir
                }));
                return false;
              }
              continue;
            case "x":
              string nextArgument = Precompiler.GetNextArgument(args, ref index);
              if (nextArgument == null)
                return false;
              Precompiler._excludedVirtualPaths.Add(nextArgument);
              continue;
            default:
              Precompiler.DumpError("1004", string.Format((IFormatProvider) CultureInfo.CurrentCulture, CompilerResources.unknown_switch, new object[1]
              {
                (object) str
              }));
              return false;
          }
        }
      }
      if (Precompiler._sourceVirtualDir == null == (Precompiler._metabasePath == null))
      {
        Precompiler.DumpError("1005", CompilerResources.need_m_or_v);
        return false;
      }
      if (Precompiler._sourcePhysicalDir != null && Precompiler._metabasePath != null)
      {
        Precompiler.DumpError("1006", CompilerResources.no_m_and_p);
        return false;
      }
      if ((Precompiler._precompilationFlags & PrecompilationFlags.Updatable) != PrecompilationFlags.Default && Precompiler._targetPhysicalDir == null)
      {
        Precompiler.DumpError("1007", string.Format((IFormatProvider) CultureInfo.CurrentCulture, CompilerResources.flag_requires_target, new object[1]
        {
          (object) "u"
        }));
        return false;
      }
      if ((Precompiler._precompilationFlags & PrecompilationFlags.OverwriteTarget) != PrecompilationFlags.Default && Precompiler._targetPhysicalDir == null)
      {
        Precompiler.DumpError("1008", string.Format((IFormatProvider) CultureInfo.CurrentCulture, CompilerResources.flag_requires_target, new object[1]
        {
          (object) "f"
        }));
        return false;
      }
      if ((Precompiler._precompilationFlags & PrecompilationFlags.ForceDebug) != PrecompilationFlags.Default && Precompiler._targetPhysicalDir == null)
      {
        Precompiler.DumpError("1009", string.Format((IFormatProvider) CultureInfo.CurrentCulture, CompilerResources.flag_requires_target, new object[1]
        {
          (object) "d"
        }));
        return false;
      }
      if (Precompiler._keyFile != null && Precompiler._targetPhysicalDir == null)
      {
        Precompiler.DumpError("1017", string.Format((IFormatProvider) CultureInfo.CurrentCulture, CompilerResources.flag_requires_target, new object[1]
        {
          (object) "keyfile"
        }));
        return false;
      }
      if (Precompiler._keyContainer != null && Precompiler._targetPhysicalDir == null)
      {
        Precompiler.DumpError("1018", string.Format((IFormatProvider) CultureInfo.CurrentCulture, CompilerResources.flag_requires_target, new object[1]
        {
          (object) "keycontainer"
        }));
        return false;
      }
      if ((Precompiler._precompilationFlags & PrecompilationFlags.FixedNames) != PrecompilationFlags.Default && Precompiler._targetPhysicalDir == null)
      {
        Precompiler.DumpError("1019", string.Format((IFormatProvider) CultureInfo.CurrentCulture, CompilerResources.flag_requires_target, new object[1]
        {
          (object) "fixednames"
        }));
        return false;
      }
      if ((Precompiler._precompilationFlags & PrecompilationFlags.DelaySign) != PrecompilationFlags.Default)
      {
        if (Precompiler._keyFile == null && Precompiler._keyContainer == null)
        {
          Precompiler.DumpError("1013", CompilerResources.invalid_delaysign);
          return false;
        }
        if (Precompiler._targetPhysicalDir == null)
        {
          Precompiler.DumpError("1015", string.Format((IFormatProvider) CultureInfo.CurrentCulture, CompilerResources.flag_requires_target, new object[1]
          {
            (object) "delaysign"
          }));
          return false;
        }
      }
      if ((Precompiler._precompilationFlags & PrecompilationFlags.AllowPartiallyTrustedCallers) != PrecompilationFlags.Default)
      {
        if (Precompiler._keyFile == null && Precompiler._keyContainer == null)
        {
          Precompiler.DumpError("1014", CompilerResources.invalid_aptca);
          return false;
        }
        if (Precompiler._targetPhysicalDir == null)
        {
          Precompiler.DumpError("1016", string.Format((IFormatProvider) CultureInfo.CurrentCulture, CompilerResources.flag_requires_target, new object[1]
          {
            (object) "aptca"
          }));
          return false;
        }
      }
      return true;
    }

    private static bool IsValidVirtualPath(string virtualPath)
    {
      return virtualPath != null && virtualPath.IndexOfAny(Precompiler.invalidVirtualPathChars) < 0;
    }

    private static string GetFullPath(string path)
    {
      try
      {
        return Path.GetFullPath(path);
      }
      catch
      {
        Precompiler.DumpError("1010", string.Format((IFormatProvider) CultureInfo.CurrentCulture, CompilerResources.invalid_path, new object[1]
        {
          (object) path
        }));
        return (string) null;
      }
    }

    private static void DumpErrors(Exception exception)
    {
      Exception formattableException = Precompiler.GetFormattableException(exception);
      if (formattableException != null)
        exception = formattableException;
      if (!(exception is HttpCompileException) && !(exception is HttpParseException))
      {
        if (exception is ConfigurationException)
        {
          ConfigurationException configurationException = (ConfigurationException) exception;
          Precompiler.DumpError(configurationException.Filename, configurationException.Line, false, "ASPCONFIG", configurationException.BareMessage);
        }
        else
          Precompiler.DumpError((string) null, 0, false, "ASPRUNTIME", exception.Message);
      }
      if (!Precompiler._showErrorStack)
        return;
      Precompiler.DumpExceptionStack(exception);
    }

    private static Exception GetFormattableException(Exception e)
    {
      if (e is HttpCompileException || e is HttpParseException || e is ConfigurationException)
        return e;
      Exception innerException = e.InnerException;
      if (innerException == null)
        return (Exception) null;
      return Precompiler.GetFormattableException(innerException);
    }

    private static void DumpCompileError(CompilerError error)
    {
      Precompiler.DumpError(error.FileName, error.Line, error.IsWarning, error.ErrorNumber, error.ErrorText);
    }

    private static void DumpExceptionStack(Exception e)
    {
      Exception innerException = e.InnerException;
      if (innerException != null)
        Precompiler.DumpExceptionStack(innerException);
      string str = "[" + e.GetType().Name + "]";
      if (e.Message != null && e.Message.Length > 0)
        str = str + ": " + e.Message;
      Console.WriteLine();
      Console.WriteLine(str);
      if (e.StackTrace == null)
        return;
      Console.WriteLine(e.StackTrace);
    }

    private static void DumpError(string errorNumber, string message)
    {
      Precompiler.DumpError((string) null, 0, false, errorNumber, message);
    }

    private static void DumpError(string filename, int line, bool warning, string errorNumber, string message)
    {
      if (filename != null)
      {
        Console.Write(filename);
        Console.Write("(" + (object) line + "): ");
      }
      if (warning)
        Console.Write("warning ");
      else
        Console.Write("error ");
      Console.Write(errorNumber + ": ");
      Console.WriteLine(message);
    }

    private class CBMCallback : ClientBuildManagerCallback
    {
      public override void ReportCompilerError(CompilerError error)
      {
        Precompiler.DumpCompileError(error);
      }

      public override void ReportParseError(ParserError error)
      {
        Precompiler.DumpError(error.VirtualPath, error.Line, false, "ASPPARSE", error.ErrorText);
      }

      public override void ReportProgress(string message)
      {
      }

      public override object InitializeLifetimeService()
      {
        return (object) null;
      }
    }
  }
}
