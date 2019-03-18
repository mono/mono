using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NUnitConsoleRunner
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string[] args = new string[] { "../DbLinq-Sqlite-Sqlserver.nunit" };
            NUnit.Gui.AppEntry.Main(args);
        }
    }
}
