// NAnt - A .NET build tool
// Copyright (C) 2001 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Ian MacLean (ian_maclean@another.com)

namespace SourceForge.NAnt {

    using System;
    using System.Xml;
    using System.Threading;

    /// <summary>
    /// A task for sleeping a short period of time, useful when a build or deployment process
    /// requires an interval between tasks.
    /// </summary>

    [TaskName("sleep")]
    public class SleepTask : Task   {

        /// <summary>hours to to add to the sleep time</summary>
        [TaskAttribute("hours")]
        string _hours = null;

        /// <summary>minutes to add to the sleep time</summary>
        [TaskAttribute("minutes")]
        string _minutes = 0.ToString();

        /// <summary>seconds to add to the sleep time</summary>
        [TaskAttribute("seconds")]
        string _seconds = 0.ToString();

        /// <summary>milliseconds to add to the sleep time</summary>
        [TaskAttribute("milliseconds")]
        string _milliseconds = 0.ToString();

        /// <summary>flag controlling whether to break the build on an error</summary>
        [TaskAttribute("failonerror")]
        [BooleanValidator()]
        string _failonerror = Boolean.FalseString;

        // Attribute properties
        public int Hours                  { get { return Convert.ToInt32(_hours); } }
        public int Minutes                { get { return Convert.ToInt32(_minutes); } }
        public int Seconds                { get { return Convert.ToInt32(_seconds); } }
        public int Milliseconds           { get { return Convert.ToInt32(_milliseconds); } }
        public bool FailOnError           { get { return Convert.ToBoolean(_failonerror); } }

        ///return time to sleep
        private int GetSleepTime() {
            return ((((int) Hours * 60) + Minutes) * 60 + Seconds) * 1000 + Milliseconds;
        }

        ///<summary> return time to sleep </summary>
        ///<param name="millis"> </param>
        private void DoSleep(int millis ) {
            Thread.Sleep(millis);
        }

        /// <summary>
        ///  verify parameters
        ///</summary>
        ///<param name="taskNode"> taskNode used to define this task instance </param>
        protected override void InitializeTask(XmlNode taskNode) {
            if (GetSleepTime() < 0) {
                throw new BuildException("Negative sleep periods are not supported", Location);
            }
        }

        protected override void ExecuteTask() {
            int sleepTime = GetSleepTime();
            Log.WriteLine(LogPrefix + "sleeping for {0} milliseconds", sleepTime);
            DoSleep(sleepTime);
        }
    }
}


