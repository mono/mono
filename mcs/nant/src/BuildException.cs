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
// Gerry Shaw (gerry_shaw@yahoo.com)
// Ian MacLean (ian_maclean@another.com)

namespace SourceForge.NAnt {

    using System;

    /// <summary>
    /// Thrown whenever an error occurs during the build.
    /// </summary>
    public class BuildException : ApplicationException {

        private Location _location = Location.UnknownLocation;

        /// <summary>
        /// Constructs a build exception with no descriptive information.
        /// </summary>
        public BuildException() : base() {
        }

        /// <summary>
        /// Constructs an exception with a descriptive message.
        /// </summary>
        public BuildException(String message) : base(message) {
        }

        /// <summary>
        /// Constructs an exception with a descriptive message and an
        /// instance of the Exception that is the cause of the current Exception.
        /// </summary>
        public BuildException(String message, Exception e) : base(message, e) {
        }

        /// <summary>
        /// Constructs an exception with a descriptive message and location
        /// in the build file that caused the exception.
        /// </summary>
        /// <param name="location">Location in the build file where the exception occured.</param>
        public BuildException(String message, Location location) : base(message) {
            _location = location;
        }

        /// <summary>
        /// Constructs an exception with the given descriptive message, the
        /// location in the build file and an instance of the Exception that
        /// is the cause of the current Exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="location">Location in the build file where the exception occured.</param>
        /// <param name="e">An instance of Exception that is the cause of the current Exception.</param>
        public BuildException(String message, Location location, Exception e) : base(message, e) {
            _location = location;
        }

        public override string Message {
            get {
                string message = base.Message;

                // only include location string if not empty
                string locationString = _location.ToString();
                if (locationString != String.Empty) {
                    message = locationString + " " + message;
                }
                return message;
            }
        }
    }
}
