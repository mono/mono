
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Mono.Audio {

	/* these are the values used by alsa */
#if PUBLIC_API
	public
#else
	internal
#endif
	enum AudioFormat {
		S8,
		U8,
		S16_LE,
		S16_BE,
		U16_LE,
		U16_BE,
		S24_LE,
		S24_BE,
		U24_LE,
		U24_BE,
		S32_LE,
		S32_BE,
		U32_LE,
		U32_BE,
		FLOAT_LE,
		FLOAT_BE,
		FLOAT64_LE,
		FLOAT64_BE,
		IEC958_SUBFRAME_LE,
		IEC958_SUBFRAME_BE,
		MU_LAW,
		A_LAW,
		IMA_ADPCM,
		MPEG,
		GSM
	}

#if PUBLIC_API
	public
#else
	internal
#endif
	class AudioDevice {
		protected uint chunk_size;
		
		static AudioDevice TryAlsa (string name) {
			AudioDevice dev;
			try {
				dev = new AlsaDevice (name);
				return dev;
			} catch {
				return null;
			}
		}

		public static AudioDevice CreateDevice (string name) {
			AudioDevice dev;

			dev = TryAlsa (name);
			/* if no option is found, return a silent device */
			if (dev == null)
				dev = new AudioDevice ();
			return dev;
		}

		public virtual bool SetFormat (AudioFormat format, int channels, int rate) {
			return true;
		}

		public virtual int PlaySample (byte[] buffer, int num_frames) {
			return num_frames;
		}
		
		public virtual void Wait () {
		}
		
		public uint ChunkSize {
			get { return chunk_size; }
		}
	}

	class AlsaDevice: AudioDevice, IDisposable {
		IntPtr handle;
		
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_open (ref IntPtr handle, string pcm_name, int stream, int mode);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_close (IntPtr handle);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_drain (IntPtr handle);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_writei (IntPtr handle, byte[] buf, int size);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_set_params (IntPtr handle, int format, int access, int channels, int rate, int soft_resample, int latency);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_state (IntPtr handle);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_prepare (IntPtr handle);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_wait (IntPtr handle, int timeout);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_resume (IntPtr handle);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params (IntPtr handle, IntPtr param);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_malloc (ref IntPtr param);

		[DllImport ("libasound.so.2")]
		static extern void snd_pcm_hw_params_free (IntPtr param);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_any (IntPtr handle, IntPtr param);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_set_access (IntPtr handle, IntPtr param, int access);
		
		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_set_format (IntPtr handle, IntPtr param, int format);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_set_channels (IntPtr handle, IntPtr param, uint channel);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_set_rate_near (IntPtr handle, IntPtr param, ref uint rate, ref int dir);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_set_period_time_near (IntPtr handle, IntPtr param, ref uint period, ref int dir);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_get_period_size (IntPtr param, ref uint period, ref int dir);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_set_buffer_size_near (IntPtr handle, IntPtr param, ref uint buff_size);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_get_buffer_time_max(IntPtr param, ref uint buffer_time, ref int dir);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_set_buffer_time_near(IntPtr handle, IntPtr param, ref uint BufferTime, ref int dir);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_hw_params_get_buffer_size(IntPtr param, ref uint BufferSize);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_sw_params (IntPtr handle, IntPtr param);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_sw_params_malloc (ref IntPtr param);

		[DllImport ("libasound.so.2")]
		static extern void snd_pcm_sw_params_free (IntPtr param);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_sw_params_current(IntPtr handle, IntPtr param);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_sw_params_set_avail_min(IntPtr handle, IntPtr param, uint frames);

		[DllImport ("libasound.so.2")]
		static extern int snd_pcm_sw_params_set_start_threshold(IntPtr handle, IntPtr param, uint StartThreshold);

		public AlsaDevice (string name) {
			handle = IntPtr.Zero;
			
			if (name == null)
				name = "default";
			int err = snd_pcm_open (ref handle, name, 0, 0);
			if (err < 0)
				throw new Exception ("no open " + err);
		}

		~AlsaDevice () {
			Dispose (false);
		}

		public void Dispose () {
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing) {
			if (disposing) {
				
			}
			if (handle != IntPtr.Zero) {
				snd_pcm_close (handle);
				handle = IntPtr.Zero;
			}
		}

		public override bool SetFormat (AudioFormat format, int channels, int rate) {
			int  alsa_err = -1;
			uint period_time = 0;
			uint period_size = 0;
			uint buffer_size = 0;
			uint buffer_time = 0;
			int dir = 0;
			uint sampling_rate = (uint)rate;
			IntPtr hw_param = IntPtr.Zero;
			IntPtr sw_param = IntPtr.Zero;
			
			try {
				// Alloc hw params structure
				alsa_err = snd_pcm_hw_params_malloc (ref hw_param);

				if (alsa_err == 0) {
					// get current hardware param
					snd_pcm_hw_params_any (handle, hw_param);

					// Set access to SND_PCM_ACCESS_RW_INTERLEAVED
					snd_pcm_hw_params_set_access (handle, hw_param, 3);
					// Set format to the file's format
					snd_pcm_hw_params_set_format (handle, hw_param, (int)format);
					// Set channel to the file's channel number
					snd_pcm_hw_params_set_channels (handle, hw_param, (uint)channels);

					dir = 0;
					// Set the sampling rate to the closest value
					snd_pcm_hw_params_set_rate_near (handle, hw_param, ref sampling_rate, ref dir);

					dir = 0;
					// Get the maximum buffer time allowed by hardware
					snd_pcm_hw_params_get_buffer_time_max (hw_param, ref buffer_time, ref dir);
					// At least, max buffer time = 500ms
					if (buffer_time > 500000) 
						buffer_time = 500000;
					// The optimum time for a period is the quarter of the buffer time
					if (buffer_time > 0)
						period_time = buffer_time / 4;

					dir = 0;
					snd_pcm_hw_params_set_period_time_near (handle, hw_param, ref period_time, ref dir);

					dir = 0;
					snd_pcm_hw_params_set_buffer_time_near (handle, hw_param, ref buffer_time, ref dir);

					// Get the period size in byte
					snd_pcm_hw_params_get_period_size (hw_param, ref period_size, ref dir);
					// Set the chunk size to the periode size
					// a chunk is a piece of wave raw data send to alsa, data are played chunk by chunk !
					chunk_size = period_size;

					snd_pcm_hw_params_get_buffer_size (hw_param, ref buffer_size);

					// Apply hardware params
					snd_pcm_hw_params (handle, hw_param);


				} else {
					hw_param = IntPtr.Zero;
					Console.WriteLine ("failed to alloc Alsa hw param struct");
				}

				alsa_err = snd_pcm_sw_params_malloc (ref sw_param);
				if (alsa_err == 0) {
					// get current software param
					snd_pcm_sw_params_current (handle, sw_param);

					// Alsa becomes ready when there is at least chunk_size bytes (i.e. period) in its ring buffer !
					snd_pcm_sw_params_set_avail_min(handle, sw_param, chunk_size);
					// Alsa starts playing when there is buffer_size (i.e. the buffer is full) bytes in its ring buffer
					snd_pcm_sw_params_set_start_threshold(handle, sw_param, buffer_size);

					// apply software param
					snd_pcm_sw_params(handle, sw_param);
				} else {
					sw_param = IntPtr.Zero;
					Console.WriteLine ("failed to alloc Alsa sw param struct");
				}
			} finally {
				if (hw_param != IntPtr.Zero) {
					snd_pcm_hw_params_free (hw_param);  // free hw params
					hw_param = IntPtr.Zero;
				}
				if (sw_param != IntPtr.Zero) {
					snd_pcm_sw_params_free (sw_param);  // free sw params
					sw_param = IntPtr.Zero;
				}
			}
			return alsa_err == 0;
		}

		public override int PlaySample (byte[] buffer, int num_frames) {
			int result = 0;
			int frames = snd_pcm_writei (handle, buffer, num_frames);
			if (frames == -11 || (frames >= 0 && frames < num_frames)) {
				// Received -EAGAIN (-11) or frames were not all played. Wait for PCM to be ready.
				snd_pcm_wait (handle, 100);
			} else if (frames == -32) {
				// Received -EPIPE (-32), meaning there is a buffer underrun
				result = snd_pcm_prepare (handle);
				if (result < 0) {
					Console.WriteLine ("ALSA xrun: failed to prepare, error code {0}", result);
				}
			} else if (frames == -86) {
				// Received -ESTRPIPE (-86), meaning the driver is suspended. Try to resume it.
				while ((result = snd_pcm_resume (handle)) == -11) {
					// Wait until suspend flag is released
					Thread.Sleep (10);
				}
				if (result < 0) {
					result = snd_pcm_prepare (handle);
				}
				if (result < 0) {
					Console.WriteLine ("ALSA suspend: failed to prepare, error code {0}", result);
				}
			} else if (frames < 0) {
				result = frames;
				Console.WriteLine ("ALSA failed to write, error code {0}", result);
			}
			if (frames >= 0) {
				result = frames;
			}
			return result;
		}
		
		public override void Wait () {
			snd_pcm_drain (handle);
		}
	}

}

