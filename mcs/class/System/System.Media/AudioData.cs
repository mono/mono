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
		bool stopped;

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
		int data_len;
		long data_offset;
		AudioFormat format;

		public WavData (Stream data) {
			stream = data;
			byte[] buffer = new byte [12 + 32];
			int idx;
			
			// Read Chunk ID + Format
			int c = stream.Read (buffer, 0, 12);
			if (c != 12 ||
				buffer [0] != 'R' || buffer [1] != 'I' || buffer [2] != 'F' || buffer [3] != 'F' ||
				buffer [8] != 'W' || buffer [9] != 'A' || buffer [10] != 'V' || buffer [11] != 'E') {
				throw new Exception ("incorrect format" + c);
			}

			// Read SubChunk 1 ID + Size => Must be 'fmt ' !
			c = stream.Read (buffer, 0, 8);
			if (c == 8 && buffer [0] == 'f' && buffer [1] == 'm' && buffer [2] == 't' && buffer [3] == ' ') {
				int sub_chunk_1_size = buffer [4];
				sub_chunk_1_size |= buffer [5] << 8;
				sub_chunk_1_size |= buffer [6] << 16;
				sub_chunk_1_size |= buffer [7] << 24;

				// Read SubChunk 1 Data
				c = stream.Read (buffer, 0, sub_chunk_1_size);
				if (sub_chunk_1_size == c)
				{
					idx = 0;
					int compression = buffer [idx++] | (buffer [idx++] << 8);
					if (compression != 1)
						throw new Exception ("incorrect format (not PCM)");
					channels = (short)(buffer [idx++] | (buffer [idx++] << 8));
					sample_rate = buffer [idx++];
					sample_rate |= buffer [idx++] << 8;
					sample_rate |= buffer [idx++] << 16;
					sample_rate |= buffer [idx++] << 24;
					int byte_rate = buffer [idx++];
					byte_rate |= buffer [idx++] << 8;
					byte_rate |= buffer [idx++] << 16;
					byte_rate |= buffer [idx++] << 24;
//					int block_align = buffer [idx++] | (buffer [idx++] << 8);
					idx += 2; //because, the above line is commented out
					int sign_bits = buffer [idx++] | (buffer [idx++] << 8);

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

				} else {
					throw new Exception ("Error: Can't Read "+sub_chunk_1_size+" bytes from stream ("+c+" bytes read");
				}
			} else {
				throw new Exception ("incorrect format (fmt)");
			}

			// Read SubChunk 2 ID + Size => Could be 'fact' or 'data' !
			c = stream.Read (buffer, 0, 8);
			if (c == 8) {
				// If SubChunk 2 ID = fact
				if (buffer [0] == 'f' && buffer [1] == 'a' && buffer [2] == 'c' && buffer [3] == 't') {
					// Read Data
					int sub_chunk_2_size = buffer [4];
					sub_chunk_2_size |= buffer [5] << 8;
					sub_chunk_2_size |= buffer [6] << 16;
					sub_chunk_2_size |= buffer [7] << 24;

					c = stream.Read (buffer, 0, sub_chunk_2_size);

					// Don't care about this data !

					// If there is a fact Chunck, read the next subChunk Id and size (should be data !)
					c = stream.Read (buffer, 0, 8);
				}

				if (buffer [0] == 'd' && buffer [1] == 'a' && buffer [2] == 't' && buffer [3] == 'a') {
					// Read Data
					int sub_chunk_2_size = buffer [4];
					sub_chunk_2_size |= buffer [5] << 8;
					sub_chunk_2_size |= buffer [6] << 16;
					sub_chunk_2_size |= buffer [7] << 24;

					data_len = sub_chunk_2_size;
					data_offset = stream.Position;
				} else { 
					throw new Exception ("incorrect format (data/fact chunck)");
				}
			}
		}

		public override void Play (AudioDevice dev) {
			int    fragment_played = 0;
			int    total_data_played = 0;
			int    chunk_size        = (int)dev.ChunkSize;
			int    count             = data_len;
			byte[] buffer            = new byte [data_len];
			byte[] chunk_to_play     = new byte [chunk_size];

			// Read only wave data, don't care about file header here !
			stream.Position = data_offset;
			stream.Read (buffer, 0, data_len); 

			while (!IsStopped && count >= 0){
				// Copy one chunk from buffer
				Buffer.BlockCopy(buffer, total_data_played, chunk_to_play, 0, chunk_size);
				// play that chunk, !!! the size pass to alsa the number of fragment, a fragment is a sample per channel !!!
				fragment_played = dev.PlaySample (chunk_to_play, chunk_size / (frame_divider * channels));

				// If alsa played something, inc the total data played and dec the data to be played
				if (fragment_played > 0) {
					total_data_played  += (fragment_played * frame_divider * channels);
					count              -= (fragment_played * frame_divider * channels);
				}
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
		int data_len ;
//		int data_offset;
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
						int    fragment_played = 0;
			int    total_data_played = 0;
			int    chunk_size        = (int)dev.ChunkSize;
			int    count             = data_len;
			byte[] buffer            = new byte [data_len];
			byte[] chunk_to_play     = new byte [chunk_size];
			
			// Read only Au data, don't care about file header here !
			stream.Position = 0; //(long)data_offset;
			stream.Read (buffer, 0, data_len); 
			
			while (!IsStopped && count >= 0){
				// Copy one chunk from buffer
				Buffer.BlockCopy(buffer, total_data_played, chunk_to_play, 0, chunk_size);
				// play that chunk, !!! the size pass to alsa the number of fragment, a fragment is a sample per channel !!!
				fragment_played = dev.PlaySample (chunk_to_play, chunk_size / (frame_divider * channels));
				
				// If alsa played something, inc the total data played and dec the data to be played
				if (fragment_played > 0) {
					total_data_played  += (fragment_played * frame_divider * channels);
					count              -= (fragment_played * frame_divider * channels);
				}
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


