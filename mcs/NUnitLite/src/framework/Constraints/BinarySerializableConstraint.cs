// ***********************************************************************
// Copyright (c) 2008 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

#if !NETCF && !SILVERLIGHT
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace NUnit.Framework.Constraints
{
    /// <summary>
    /// BinarySerializableConstraint tests whether 
    /// an object is serializable in binary format.
    /// </summary>
    public class BinarySerializableConstraint : Constraint
    {
        readonly BinaryFormatter serializer = new BinaryFormatter();

        /// <summary>
        /// Test whether the constraint is satisfied by a given value
        /// </summary>
        /// <param name="actual">The value to be tested</param>
        /// <returns>True for success, false for failure</returns>
        public override bool Matches(object actual)
        {
            this.actual = actual;

            if (actual == null)
                throw new ArgumentException();

            MemoryStream stream = new MemoryStream();

            try
            {
                serializer.Serialize(stream, actual);

                stream.Seek(0, SeekOrigin.Begin);

                object value = serializer.Deserialize(stream);

                return value != null;
            }
            catch (SerializationException)
            {
                return false;
            }
        }

        /// <summary>
        /// Write the constraint description to a MessageWriter
        /// </summary>
        /// <param name="writer">The writer on which the description is displayed</param>
        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.Write("binary serializable");
        }

        /// <summary>
        /// Write the actual value for a failing constraint test to a
        /// MessageWriter. The default implementation simply writes
        /// the raw value of actual, leaving it to the writer to
        /// perform any formatting.
        /// </summary>
        /// <param name="writer">The writer on which the actual value is displayed</param>
        public override void WriteActualValueTo(MessageWriter writer)
        {
            writer.Write("<{0}>", actual.GetType().Name);
        }

        /// <summary>
        /// Returns the string representation
        /// </summary>
        protected override string GetStringRepresentation()
        {
            return "<binaryserializable>";
        }
    }
}
#endif