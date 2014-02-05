// StreamUtils.cs
//
// Copyright 2005 John Reilly
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
//
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

using System;
using System.IO;

namespace ICSharpCode.SharpZipLib.Core
{
	/// <summary>
	/// Provides simple <see cref="Stream"/>" utilities.
	/// </summary>
	public sealed class StreamUtils
	{
		/// <summary>
		/// Read from a <see cref="Stream"/> ensuring all the required data is read.
		/// </summary>
		/// <param name="stream">The stream to read.</param>
		/// <param name="buffer">The buffer to fill.</param>
		/// <seealso cref="ReadFully(Stream,byte[],int,int)"/>
		static public void ReadFully(Stream stream, byte[] buffer)
		{
			ReadFully(stream, buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Read from a <see cref="Stream"/>" ensuring all the required data is read.
		/// </summary>
		/// <param name="stream">The stream to read data from.</param>
		/// <param name="buffer">The buffer to store data in.</param>
		/// <param name="offset">The offset at which to begin storing data.</param>
		/// <param name="count">The number of bytes of data to store.</param>
		/// <exception cref="ArgumentNullException">Required parameter is null</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> and or <paramref name="count"/> are invalid.</exception>
		/// <exception cref="EndOfStreamException">End of stream is encountered before all the data has been read.</exception>
		static public void ReadFully(Stream stream, byte[] buffer, int offset, int count)
		{
			if ( stream == null ) {
				throw new ArgumentNullException("stream");
			}

			if ( buffer == null ) {
				throw new ArgumentNullException("buffer");
			}

			// Offset can equal length when buffer and count are 0.
			if ( (offset < 0) || (offset > buffer.Length) ) {
				throw new ArgumentOutOfRangeException("offset");
			}

			if ( (count < 0) || (offset + count > buffer.Length) ) {
				throw new ArgumentOutOfRangeException("count");
			}

			while ( count > 0 ) {
				int readCount = stream.Read(buffer, offset, count);
				if ( readCount <= 0 ) {
					throw new EndOfStreamException();
				}
				offset += readCount;
				count -= readCount;
			}
		}

		/// <summary>
		/// Copy the contents of one <see cref="Stream"/> to another.
		/// </summary>
		/// <param name="source">The stream to source data from.</param>
		/// <param name="destination">The stream to write data to.</param>
		/// <param name="buffer">The buffer to use during copying.</param>
		static public void Copy(Stream source, Stream destination, byte[] buffer)
		{
			if (source == null) {
				throw new ArgumentNullException("source");
			}

			if (destination == null) {
				throw new ArgumentNullException("destination");
			}

			if (buffer == null) {
				throw new ArgumentNullException("buffer");
			}

			// Ensure a reasonable size of buffer is used without being prohibitive.
			if (buffer.Length < 128) {
				throw new ArgumentException("Buffer is too small", "buffer");
			}

			bool copying = true;

			while (copying) {
				int bytesRead = source.Read(buffer, 0, buffer.Length);
				if (bytesRead > 0) {
					destination.Write(buffer, 0, bytesRead);
				}
				else {
					destination.Flush();
					copying = false;
				}
			}
		}

		/// <summary>
		/// Copy the contents of one <see cref="Stream"/> to another.
		/// </summary>
		/// <param name="source">The stream to source data from.</param>
		/// <param name="destination">The stream to write data to.</param>
		/// <param name="buffer">The buffer to use during copying.</param>
		/// <param name="progressHandler">The <see cref="ProgressHandler">progress handler delegate</see> to use.</param>
		/// <param name="updateInterval">The minimum <see cref="TimeSpan"/> between progress updates.</param>
		/// <param name="sender">The source for this event.</param>
		/// <param name="name">The name to use with the event.</param>
		/// <remarks>This form is specialised for use within #Zip to support events during archive operations.</remarks>
		static public void Copy(Stream source, Stream destination,
			byte[] buffer, ProgressHandler progressHandler, TimeSpan updateInterval, object sender, string name)
		{
			Copy(source, destination, buffer, progressHandler, updateInterval, sender, name, -1);
		}

		/// <summary>
		/// Copy the contents of one <see cref="Stream"/> to another.
		/// </summary>
		/// <param name="source">The stream to source data from.</param>
		/// <param name="destination">The stream to write data to.</param>
		/// <param name="buffer">The buffer to use during copying.</param>
		/// <param name="progressHandler">The <see cref="ProgressHandler">progress handler delegate</see> to use.</param>
		/// <param name="updateInterval">The minimum <see cref="TimeSpan"/> between progress updates.</param>
		/// <param name="sender">The source for this event.</param>
		/// <param name="name">The name to use with the event.</param>
		/// <param name="fixedTarget">A predetermined fixed target value to use with progress updates.
		/// If the value is negative the target is calculated by looking at the stream.</param>
		/// <remarks>This form is specialised for use within #Zip to support events during archive operations.</remarks>
		static public void Copy(Stream source, Stream destination,
			byte[] buffer,
			ProgressHandler progressHandler, TimeSpan updateInterval,
			object sender, string name, long fixedTarget)
		{
			if (source == null) {
				throw new ArgumentNullException("source");
			}

			if (destination == null) {
				throw new ArgumentNullException("destination");
			}

			if (buffer == null) {
				throw new ArgumentNullException("buffer");
			}

			// Ensure a reasonable size of buffer is used without being prohibitive.
			if (buffer.Length < 128) {
				throw new ArgumentException("Buffer is too small", "buffer");
			}

			if (progressHandler == null) {
				throw new ArgumentNullException("progressHandler");
			}

			bool copying = true;

			DateTime marker = DateTime.Now;
			long processed = 0;
			long target = 0;

			if (fixedTarget >= 0) {
				target = fixedTarget;
			}
			else if (source.CanSeek) {
				target = source.Length - source.Position;
			}

			// Always fire 0% progress..
			ProgressEventArgs args = new ProgressEventArgs(name, processed, target);
			progressHandler(sender, args);

			bool progressFired = true;

			while (copying) {
				int bytesRead = source.Read(buffer, 0, buffer.Length);
				if (bytesRead > 0) {
					processed += bytesRead;
					progressFired = false;
					destination.Write(buffer, 0, bytesRead);
				}
				else {
					destination.Flush();
					copying = false;
				}

				if (DateTime.Now - marker > updateInterval) {
					progressFired = true;
					marker = DateTime.Now;
					args = new ProgressEventArgs(name, processed, target);
					progressHandler(sender, args);

					copying = args.ContinueRunning;
				}
			}

			if (!progressFired) {
				args = new ProgressEventArgs(name, processed, target);
				progressHandler(sender, args);
			}
		}

		/// <summary>
		/// Initialise an instance of <see cref="StreamUtils"></see>
		/// </summary>
		private StreamUtils()
		{
			// Do nothing.
		}
	}
}
