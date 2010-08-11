using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Mono.CodeContracts.Rewrite {
	public struct AssemblyRef {

		public struct TwoStreams {

			public TwoStreams (Stream assembly, Stream symbols)
				: this ()
			{
				this.Assembly = assembly;
				this.Symbols = symbols;
			}

			public Stream Assembly { get; private set; }
			public Stream Symbols { get; private set; }

		}

		public AssemblyRef (string filename)
			: this ()
		{
			this.Filename = filename;
		}

		public AssemblyRef (TwoStreams streams)
			: this ()
		{
			this.Streams = streams;
		}

		public string Filename { get; private set; }
		public TwoStreams Streams { get; private set; }

		public bool IsFilename
		{
			get
			{
				return this.Filename != null;
			}
		}

		public bool IsStream
		{
			get
			{
				return this.Streams.Assembly != null;
			}
		}

		public bool IsSet
		{
			get
			{
				return this.Filename != null || this.Streams.Assembly != null;
			}
		}

		public static implicit operator AssemblyRef (string filename)
		{
			return new AssemblyRef (filename);
		}

		public static implicit operator AssemblyRef (Stream stream)
		{
			return new AssemblyRef (new TwoStreams (stream, null));
		}

	}
}
