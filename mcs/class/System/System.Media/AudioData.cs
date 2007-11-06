
#if NET_2_0
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Mono.Audio {
 
#if PUBLIC_API
	public
#else
	internal
#endif
	abstract class AudioData {
		protected const int buffer_size = 4096;
		bool stopped = false;

		public abstract int Channels {
			get;
		}

		public abstract int Rate {
			get;
		}

		public abstract AudioFormat Format {
			get;
		}

		public virtual void Setup (AudioDevice dev) {
			dev.SetFormat (Format, Channels, Rate);
		}

		public abstract void Play (AudioDevice dev);

		public virtual bool IsStopped {
			get {
				return stopped;
			}
			set {
				stopped = value;
			}
		}
	}

	/*public enum WavCmpCodes {
		Unknown,
		PCM,
		ADPCM,
	}*/

#if PUBLIC_API
	public
#else
	internal
#endif
	class WavData : AudioData {
		Stream stream;
		short channels;
		ushort frame_divider;
		int sample_rate;
		int data_len = 0;
		AudioFormat format;

		public WavData (Stream data) {
			stream = data;
			byte[] buffer = new byte [12+32];
			int c = stream.Read (buffer, 0, 12 + 32);
			if (c != (12 + 32) || 
					buffer [0] != 'R' || buffer [1] != 'I' || buffer [2] != 'F' || buffer [3] != 'F' ||
					buffer [8] != 'W' || buffer [9] != 'A' || buffer [10] != 'V' || buffer [11] != 'E') {
				throw new Exception ("incorrect format" + c);
			}
			if (buffer [12] != 'f' || buffer [13] != 'm' || buffer [14] != 't' || buffer [15] != ' ') {
				throw new Exception ("incorrect format (fmt)");
			}
			int extra_size = buffer [16];
			extra_size |= buffer [17] << 8;
			extra_size |= buffer [18] << 16;
			extra_size |= buffer [19] << 24;
			int compression = buffer [20] | (buffer [21] << 8);
			if (compression != 1)
				throw new Exception ("incorrect format (not PCM)");
			channels = (short)(buffer [22] | (buffer [23] << 8));
			sample_rate = buffer [24];
			sample_rate |= buffer [25] << 8;
			sample_rate |= buffer [26] << 16;
			sample_rate |= buffer [27] << 24;
			int avg_bytes = buffer [28];
			avg_bytes |= buffer [29] << 8;
			avg_bytes |= buffer [30] << 16;
			avg_bytes |= buffer [31] << 24;
			//int block_align = buffer [32] | (buffer [33] << 8);
			int sign_bits = buffer [34] | (buffer [35] << 8);
			/*Console.WriteLine (extra_size);
			Console.WriteLine (compression);
			Console.WriteLine (channels);
			Console.WriteLine (sample_rate);
			Console.WriteLine (avg_bytes);
			Console.WriteLine (block_align);
			Console.WriteLine (sign_bits);*/
			if (buffer [36] != 'd' || buffer [37] != 'a' || buffer [38] != 't' || buffer [39] != 'a') {
				throw new Exception ("incorrect format (data)");
			}
			int sample_size = buffer [40];
			sample_size |= buffer [41] << 8;
			sample_size |= buffer [42] << 16;
			sample_size |= buffer [43] << 24;
			data_len = sample_size;
			//Console.WriteLine (sample_size);
			switch (sign_bits) {
			case 8:
				frame_divider = 1;
				format = AudioFormat.U8; break;
			case 16:
				frame_divider = 2;
				format = AudioFormat.S16_LE; break;
			default:
				throw new Exception ("bits per sample");
			}
		}

		public override void Play (AudioDevice dev) {
			int read;
			int count = data_len;
			byte[] buffer = new byte [buffer_size];
			stream.Position = 0;
			while (!IsStopped && count >= 0 && (read = stream.Read (buffer, 0, System.Math.Min (buffer.Length, count))) > 0) {
				// FIXME: account for leftover bytes
				dev.PlaySample (buffer, read/frame_divider);
				count -= read;
			}
		}

		public override int Channels {
			get {return channels;}
		}
		public override int Rate {
			get {return sample_rate;}
		}
		public override AudioFormat Format {
			get {return format;}
		}
	}

	// http://en.wikipedia.org/wiki/Au_file_format
#if PUBLIC_API
	public
#else
	internal
#endif
	class AuData : AudioData {
		Stream stream;
		short channels;
		ushort frame_divider;
		int sample_rate;
		int data_len = 0;
		AudioFormat format;

		public AuData (Stream data) {
			stream = data;
			byte[] buffer = new byte [24];
			int c = stream.Read (buffer, 0, 24);
			if (c != 24 || 
					buffer [0] != '.' || buffer [1] != 's' || buffer [2] != 'n' || buffer [3] != 'd') {
				throw new Exception ("incorrect format" + c);
			}
			int data_offset = buffer [7];
			data_offset |= buffer [6] << 8;
			data_offset |= buffer [5] << 16;
			data_offset |= buffer [4] << 24;
			data_len = buffer [11];
			data_len |= buffer [10] << 8;
			data_len |= buffer [9] << 16;
			data_len |= buffer [8] << 24;
			int encoding = buffer [15];
			encoding |= buffer [14] << 8;
			encoding |= buffer [13] << 16;
			encoding |= buffer [12] << 24;
			sample_rate = buffer [19];
			sample_rate |= buffer [18] << 8;
			sample_rate |= buffer [17] << 16;
			sample_rate |= buffer [16] << 24;
			int chans = buffer [23];
			chans |= buffer [22] << 8;
			chans |= buffer [21] << 16;
			chans |= buffer [20] << 24;
			channels = (short)chans;
			if (data_offset < 24 || (chans != 1 && chans != 2)) {
				throw new Exception ("incorrect format offset" + data_offset);
			}
			if (data_offset != 24) {
				for (int l = 24; l < data_offset; ++l)
					stream.ReadByte ();
			}
			switch (encoding) {
			case 1:
				frame_divider = 1;
				format = AudioFormat.MU_LAW; break;
			default:
				throw new Exception ("incorrect format encoding" + encoding);
			}
			if (data_len == -1) {
				data_len = (int)stream.Length - data_offset;
			}
			// Console.WriteLine ("format: {0}, rate: {1}", format, sample_rate);
		}

		public override void Play (AudioDevice dev) {
			int read;
			int count = data_len;
			byte[] buffer = new byte [buffer_size];
			stream.Position = 0;
			while (!IsStopped && count >= 0 && (read = stream.Read (buffer, 0, System.Math.Min (buffer.Length, count))) > 0) {
				// FIXME: account for leftover bytes
				dev.PlaySample (buffer, read/frame_divider);
				count -= read;
			}
		}

		public override int Channels {
			get {return channels;}
		}
		public override int Rate {
			get {return sample_rate;}
		}
		public override AudioFormat Format {
			get {return format;}
		}
	}

}

#endif

