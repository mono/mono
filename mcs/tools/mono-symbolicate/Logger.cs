using System;

namespace Mono
{
	public class Logger
	{
		public enum Level
		{
			Debug = 1,
			Warning = 2,
			Error = 3,
			None = 4,
		}

		Level level;
		Action<string> logAction;

		public Logger (Level level, Action<string> logAction)
		{
			this.level = level;
			this.logAction = logAction;
		}

		public void LogDebug (string str, params string[] vals)
		{
			Log (Level.Debug, "Debug: " + str, vals);
		}

		public void LogWarning (string str, params string[] vals)
		{
			Log (Level.Warning, "Warning: " + str, vals);
		}

		public void LogError (string str, params string[] vals)
		{
			Log (Level.Error, "Error: " + str, vals);
		}

		private void Log (Level msgLevel, string str, params string[] vals)
		{
			if ((int) level > (int) msgLevel)
				return;

			logAction (string.Format (str, vals));
		}
	}
}

