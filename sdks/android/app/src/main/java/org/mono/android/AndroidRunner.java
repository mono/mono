package org.mono.android;

import android.app.Instrumentation;

import android.content.Context;
import android.content.pm.ApplicationInfo;
import android.content.res.AssetManager;

import android.os.Bundle;
import android.os.Build;

import android.util.Log;

import java.io.File;
import java.io.InputStream;
import java.io.OutputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.FileNotFoundException;

import org.mono.android.AndroidTestRunner.R;

public class AndroidRunner extends Instrumentation
{
	static AndroidRunner inst;

	String testsuite;
	boolean waitForLLDB;

	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		testsuite = savedInstanceState.getString ("TestSuite");
		waitForLLDB = savedInstanceState.getString ("WaitForLLDB").equals ("True");

		super.onCreate (savedInstanceState);

		start ();
	}

	@Override
	public void onStart ()
	{
		super.onStart ();

		AndroidRunner.inst = this;

		Context context = getContext ();

		String filesDir    = context.getFilesDir ().getAbsolutePath ();
		String cacheDir    = context.getCacheDir ().getAbsolutePath ();
		String nativeLibraryDir = context.getApplicationInfo ().nativeLibraryDir;
		String assemblyDir = filesDir + "/assemblies";
		String lldbDir = filesDir + "/lldb";

		//XXX copy stuff
		Log.w ("MONO", "DOING THE COPYING!2");

		AssetManager am = context.getAssets ();

		new File (assemblyDir).mkdir ();
		copyAssetDir (am, "asm", assemblyDir);

		new File (lldbDir).mkdir ();
		copyAssetDir (am, Build.CPU_ABI, lldbDir);

		new File (filesDir + "/mono").mkdir ();
		new File (filesDir + "/mono/2.1").mkdir ();
		copyAssetDir (am, "mconfig", filesDir + "/mono/2.1");

		if (waitForLLDB) {
			Log.w ("MONO", "Launching lldb-server " + Build.CPU_ABI);

			String lldbServerFile = lldbDir + "/lldb-server";

			new File(lldbServerFile).setExecutable (true);

			// launch LLDB server in background
			ProcessBuilder pb = new ProcessBuilder (
				lldbServerFile,
				"platform",
				"--server",
				"--listen", "127.0.0.1:6101",
				"--log-channels", "lldb process:gdb-remote packets");

			try {
				final Process p = pb.start ();
			} catch (IOException ioe) {
				Log.e ("MONO", "Error when launching lldb-server", ioe);

				runOnMainSync (new Runnable () {
					public void run() {
						finish (1, null);
					}
				});

				return;
			}
		}

		if (testsuite.startsWith("debugger:")) {
			runTests (filesDir, cacheDir, nativeLibraryDir, assemblyDir, "tests/" + testsuite.substring(9), true, false, waitForLLDB);
		} else if (testsuite.startsWith("profiler:")) {
			runTests (filesDir, cacheDir, nativeLibraryDir, assemblyDir, "tests/" + testsuite.substring(9), false, true, waitForLLDB);
		} else {
			runTests (filesDir, cacheDir, nativeLibraryDir, assemblyDir, "tests/" + testsuite, false, false, waitForLLDB);
		}

		runOnMainSync (new Runnable () {
			public void run() {
				finish (0, null);
			}
		});
	}

	void copyAssetDir (AssetManager am, String path, String outpath) {
		Log.w ("MONO", "EXTRACTING: " + path);
		try {
			String[] res = am.list (path);
			for (int i = 0; i < res.length; ++i) {
				String fromFile = path + "/" + res [i];
				String toFile = outpath + "/" + res [i];

				InputStream fromStream;
				try {
					fromStream = am.open (fromFile);
				} catch (FileNotFoundException e) {
					// am.list() returns directories, we need to process them too
					new File (toFile).mkdirs ();
					copyAssetDir (am, fromFile, toFile);
					continue;
				}

				Log.w ("MONO", "\tCOPYING " + fromFile + " to " + toFile);
				copy (fromStream, new FileOutputStream (toFile));
			}
		} catch (Exception e) {
			Log.w ("MONO", "WTF", e);
		}
	}

	void copy (InputStream in, OutputStream out) throws IOException {
		byte[] buff = new byte [1024];
		for (int len = in.read (buff); len != -1; len = in.read (buff)) {
			out.write (buff, 0, len);
		}
		in.close ();
		out.close ();
	}

	native void runTests (String filesDir, String cacheDir, String dataDir, String assemblyDir, String assemblyName, boolean isDebugger, boolean isProfiler, boolean waitForLLDB);

	static void WriteLineToInstrumentation (String line)
	{
		Bundle b = new Bundle();
		b.putString(Instrumentation.REPORT_KEY_STREAMRESULT, line + "\n");
		AndroidRunner.inst.sendStatus(0, b);
	}

	static {
		System.loadLibrary("runtime-bootstrap");
	}
}
