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
// Gerry Shaw (gerry_shaw@yahoo.com)

namespace SourceForge.NAnt {
    using System;
    using System.IO;

    /// <summary>
    /// Stores the file name and line number in a file.
    /// </summary>
    public class Location {
        string _fileName;
        int _lineNumber;
        int _columnNumber;

        public static readonly Location UnknownLocation = new Location();

        /// <summary>
        /// Creates a location consisting of a file name and line number.
        ///</summary>
        public Location(string fileName, int lineNumber, int columnNumber) {
            Uri uri = new Uri(fileName);
            string strfileName = uri.LocalPath;  // convert from URI syntax to local path
            Init(strfileName, lineNumber, columnNumber);
        }

        /// <summary>
        /// Creates a location consisting of a file name but no line number.
        ///</summary>
        public Location(string fileName) {
            Init(fileName, 0, 0);
        }

        /// <summary>
        /// Creates an "unknown" location.
        ///</summary>
        private Location() {
            Init(null, 0, 0);
        }

        /// <summary>
        /// Private Init function.
        ///</summary>
        private void Init(string fileName, int lineNumber, int columnNumber) {
            _fileName = fileName;
            _lineNumber = lineNumber;
            _columnNumber = columnNumber;
        }

        /// <summary>
        /// Returns the file name, line number and a trailing space. An error
        /// message can be appended easily. For unknown locations, returns
        /// an empty string.
        ///</summary>
        public override string ToString() {
            string message = "";

            if (_fileName != null) {
                message += _fileName;

                if (_lineNumber != 0) {
                    message += ":";
                    message += _lineNumber.ToString();
                }

                message += ":";
            }

            return message;
        }
    }
}
