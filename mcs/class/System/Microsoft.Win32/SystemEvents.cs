//
// SystemEvents.cs
//
// Author:
//  Johannes Roith (johannes@jroith.de)
//
// (C) 2002 Johannes Roith
//
namespace Microsoft.Win32 {

	/// <summary>
	/// </summary>
public sealed class SystemEvents : System.EventArgs{

public static void InvokeOnEventsThread(System.Delegate method)
{

}

public static event System.EventHandler DisplaySettingsChanged;
public static event System.EventHandler EventsThreadShutdown;
public static event System.EventHandler InstalledFontsChanged;
public static event System.EventHandler LowMemory;
public static event System.EventHandler PaletteChanged;
public static event System.EventHandler PowerModeChanged;
public static event System.EventHandler SessionEnded;
public static event System.EventHandler SessionEnding;
public static event System.EventHandler TimeChanged;
public static event System.EventHandler TimerElapsed;
public static event System.EventHandler UserPreferenceChanged;
public static event System.EventHandler UserPreferenceChanging;

}

}
