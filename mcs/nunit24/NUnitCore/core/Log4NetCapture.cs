// ****************************************************************
// Copyright 2008, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.IO;
using System.Reflection;
using BF = System.Reflection.BindingFlags;

namespace NUnit.Core
{
	/// <summary>
	/// Proxy class for operations on a real log4net appender,
	/// allowing NUnit to work with multiple versions of log4net
	/// and to fail gracefully if no log4net assembly is present.
	/// </summary>
	public class Log4NetCapture : LogCapture
	{
		private Assembly log4netAssembly;
		private Type appenderType;
		private Type basicConfiguratorType;

		private object appender;
		private bool isInitialized;

		// Layout codes that work for versions from 
		// log4net 1.2.0.30714 to 1.2.10:
		//
		//	%a = domain friendly name
		//	%c = logger name (%c{1} = last component )
		//	%d = date and time
		//	%d{ABSOLUTE} = time only
		//	%l = source location of the error
		//	%m = message
		//	%n = newline
		//	%p = level
		//	%r = elapsed milliseconds since program start
		//	%t = thread
		//	%x = nested diagnostic content (NDC)
		private static readonly string logFormat =
			"%d{ABSOLUTE} %-5p [%4t] %c{1} [%x]- %m%n";

		protected override void StartCapture()
		{
			if ( IsInitialized )
			{
				string threshold = DefaultThreshold;
				if ( !SetLoggingThreshold( threshold ) )
					SetLoggingThreshold( "Error" );

				SetAppenderTextWriter( this.Writer );
				ConfigureAppender();
			}
		}

		protected override void StopCapture()
		{
			if ( appender != null )
			{
				SetLoggingThreshold( "Off" );
				SetAppenderTextWriter( null );
			}
		}

		#region Helpers
		private bool IsInitialized
		{
			get
			{
				if ( isInitialized )
					return true;

				try
				{
					log4netAssembly = Assembly.Load( "log4net" );
					if ( log4netAssembly == null ) return false;

					appenderType = log4netAssembly.GetType( 
						"log4net.Appender.TextWriterAppender", false, false );
					if ( appenderType == null ) return false;

					basicConfiguratorType = log4netAssembly.GetType( 
						"log4net.Config.BasicConfigurator", false, false );
					if ( basicConfiguratorType == null ) return false;

					appender = TryCreateAppender();
					if ( appender == null ) return false;

					SetAppenderLogFormat( logFormat );

					isInitialized = true;
				}
				catch
				{
				}

				return isInitialized;
			}
		}

		private Assembly TryLoadLog4NetAssembly()
		{
			Assembly assembly = null;

			try
			{
				assembly = Assembly.Load( "log4net" );
			}
			catch
			{
				return null; 
			}

			return assembly;
		}

		/// <summary>
		/// Attempt to create a TextWriterAppender using reflection,
		/// failing silently if it is not possible.
		/// </summary>
		private object TryCreateAppender()
		{
			ConstructorInfo ctor = appenderType.GetConstructor( Type.EmptyTypes );
			object appender = ctor.Invoke( new object[0] );

			return appender;
		}

		private void SetAppenderLogFormat( string logFormat )
		{
			Type patternLayoutType = log4netAssembly.GetType( 
				"log4net.Layout.PatternLayout", false, false );
			if ( patternLayoutType == null ) return;

			ConstructorInfo ctor = patternLayoutType.GetConstructor( new Type[] { typeof(string) } );
			if ( ctor != null )
			{
				object patternLayout = ctor.Invoke( new object[] { logFormat } );

				if ( patternLayout != null )
				{
					PropertyInfo prop = appenderType.GetProperty( "Layout", BF.Public | BF.Instance | BF.SetProperty );
					if ( prop != null )
						prop.SetValue( appender, patternLayout, null );
				}
			} 
		}

		private bool SetLoggingThreshold( string threshold )
		{
			PropertyInfo prop = appenderType.GetProperty( "Threshold", BF.Public | BF.Instance | BF.SetProperty );
			if ( prop == null ) return false;

			Type levelType = prop.PropertyType;
			FieldInfo levelField = levelType.GetField( threshold, BF.Public | BF.Static | BF.IgnoreCase );
			if ( levelField == null ) return false;

			object level = levelField.GetValue( null );
			prop.SetValue( appender, level, null );
			return true;
		}

		private void SetAppenderTextWriter( TextWriter writer )
		{
			PropertyInfo prop = appenderType.GetProperty( "Writer", BF.Instance | BF.Public | BF.SetProperty );
			if ( prop != null )
				prop.SetValue( appender, writer, null );
		}

		private void ConfigureAppender()
		{
			MethodInfo configureMethod = basicConfiguratorType.GetMethod( "Configure", new Type[] { appenderType } );
			if ( configureMethod != null )
				configureMethod.Invoke( null, new object[] { appender } );
		}
		#endregion
	}
}
