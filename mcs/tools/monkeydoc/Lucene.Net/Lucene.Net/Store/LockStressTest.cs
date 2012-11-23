/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace Mono.Lucene.Net.Store
{
	
	/// <summary> Simple standalone tool that forever acquires &amp; releases a
	/// lock using a specific LockFactory.  Run without any args
	/// to see usage.
	/// 
	/// </summary>
	/// <seealso cref="VerifyingLockFactory">
	/// </seealso>
	/// <seealso cref="LockVerifyServer">
	/// </seealso>
	
	public class LockStressTest
	{
		
		[STAThread]
		public static void  Main(System.String[] args)
		{
			
			if (args.Length != 6)
			{
				System.Console.Out.WriteLine("\nUsage: java Mono.Lucene.Net.Store.LockStressTest myID verifierHostOrIP verifierPort lockFactoryClassName lockDirName sleepTime\n" + "\n" + "  myID = int from 0 .. 255 (should be unique for test process)\n" + "  verifierHostOrIP = host name or IP address where LockVerifyServer is running\n" + "  verifierPort = port that LockVerifyServer is listening on\n" + "  lockFactoryClassName = primary LockFactory class that we will use\n" + "  lockDirName = path to the lock directory (only set for Simple/NativeFSLockFactory\n" + "  sleepTimeMS = milliseconds to pause betweeen each lock obtain/release\n" + "\n" + "You should run multiple instances of this process, each with its own\n" + "unique ID, and each pointing to the same lock directory, to verify\n" + "that locking is working correctly.\n" + "\n" + "Make sure you are first running LockVerifyServer.\n" + "\n");
				System.Environment.Exit(1);
			}
			
			int myID = System.Int32.Parse(args[0]);
			
			if (myID < 0 || myID > 255)
			{
				System.Console.Out.WriteLine("myID must be a unique int 0..255");
				System.Environment.Exit(1);
			}
			
			System.String verifierHost = args[1];
			int verifierPort = System.Int32.Parse(args[2]);
			System.String lockFactoryClassName = args[3];
			System.String lockDirName = args[4];
			int sleepTimeMS = System.Int32.Parse(args[5]);
			
			System.Type c;
			try
			{
				c = System.Type.GetType(lockFactoryClassName);
			}
			catch (System.Exception e)
			{
				throw new System.IO.IOException("unable to find LockClass " + lockFactoryClassName);
			}
			
			LockFactory lockFactory;
			try
			{
				lockFactory = (LockFactory) System.Activator.CreateInstance(c);
			}
			catch (System.UnauthorizedAccessException e)
			{
				throw new System.IO.IOException("IllegalAccessException when instantiating LockClass " + lockFactoryClassName);
			}
			catch (System.InvalidCastException e)
			{
				throw new System.IO.IOException("unable to cast LockClass " + lockFactoryClassName + " instance to a LockFactory");
			}
			catch (System.Exception e)
			{
				throw new System.IO.IOException("InstantiationException when instantiating LockClass " + lockFactoryClassName);
			}
			
			System.IO.DirectoryInfo lockDir = new System.IO.DirectoryInfo(lockDirName);
			
			if (lockFactory is NativeFSLockFactory)
			{
				((NativeFSLockFactory) lockFactory).SetLockDir(lockDir);
			}
			else if (lockFactory is SimpleFSLockFactory)
			{
				((SimpleFSLockFactory) lockFactory).SetLockDir(lockDir);
			}
			
			lockFactory.SetLockPrefix("test");
			
			LockFactory verifyLF = new VerifyingLockFactory((sbyte) myID, lockFactory, verifierHost, verifierPort);
			
			Lock l = verifyLF.MakeLock("test.lock");
			
			while (true)
			{
				
				bool obtained = false;
				
				try
				{
					obtained = l.Obtain(10);
				}
				catch (LockObtainFailedException e)
				{
					System.Console.Out.Write("x");
				}
				
				if (obtained)
				{
					System.Console.Out.Write("l");
					l.Release();
				}
				System.Threading.Thread.Sleep(new System.TimeSpan((System.Int64) 10000 * sleepTimeMS));
			}
		}
	}
}
