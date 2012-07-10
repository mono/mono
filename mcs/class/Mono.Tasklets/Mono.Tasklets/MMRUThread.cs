// COPYRIGHT 2012, Mike Rieker, Beverly, MA, USA, mrieker@nii.net
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

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Mono.Tasklets {

	public abstract class MMRUThread : IDisposable
	{
		public class AlreadyCurrentException : Exception { }
		public class DestructedException     : Exception { }
		public class NotInUThreadException   : Exception { }

		private string name = "";

		[ThreadStatic]
		private static MMRUThread currentUThread = null;

		private UIntPtr mmrUThread = UIntPtr.Zero;  // points to Mono's internal MMRUThread struct

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static Exception ctor (object uThreadObj, UIntPtr stackSize, out UIntPtr mmrUThread_r);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static Exception dtor (UIntPtr mmrUThread);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static Exception start (UIntPtr mmrUThread);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static Exception suspend (UIntPtr mmrUThread, Exception except, bool exiting);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static Exception resume (UIntPtr mmrUThread, Exception except);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static int active (UIntPtr mmrUThread);

		/*
		 * See how many bytes of stack are available for use.
		 */
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static UIntPtr StackLeft ();

		public string Name { get { return name; } }

		/**************************\
		 *  Abstract Definitions  *
		\**************************/

		/*
		 * This is the uthread's main function.
		 * Either override this or MainEx().
		 */
		public virtual void Main ()
		{
			throw new Exception ("neither Main() nor MainEx() overridden");
		}

		/*
		 * You can override this instead of Main() if
		 * your function does not throw any exceptions.
		 */
		public virtual Exception MainEx ()
		{
			try {
				Main ();
			} catch (Exception except) {
				return except;
			}
			return null;
		}

		/******************\
		 *  API Routines  *
		\******************/

		public MMRUThread ()
		{
			Construct (UIntPtr.Zero, "");
		}

		public MMRUThread (string name)
		{
			Construct (UIntPtr.Zero, name);
		}

		public MMRUThread (UIntPtr stackSize)
		{
			Construct (stackSize, "");
		}

		public MMRUThread (UIntPtr stackSize, string name)
		{
			Construct (stackSize, name);
		}

		private void Construct (UIntPtr stackSize, string name)
		{
			Exception except;

			if (name == null) name = "";
			this.name = name;

			except = ctor (this, stackSize, out mmrUThread);
			if (except != null) {
				throw except;
			}
		}

		~MMRUThread ()
		{
			Dispose ();
		}

		public void Dispose ()
		{
			Exception except;

			if (mmrUThread != UIntPtr.Zero) {
				except = dtor (mmrUThread);
				if (except != null) {
					throw except;
				}
				mmrUThread = UIntPtr.Zero;
			}
		}

		/*
		 * Use it like this to call a suspendable method:
		 *
		 *    wrapper = new SomeClassThatDerivesFromMMRUThread ();
		 *    wrapper.Start (wrapper);
		 *    ... returns here when wrapper.Suspend() is called ...
		 */
		public void Start ()
		{
			Exception except = StartEx ();
			if (except != null) {
				throw except;
			}
		}

		/*
		 * This is a microthread giving up use of the CPU for now.
		 * Optionally pass an exception to the Start() or Resume() call.
		 */
		public static void Suspend () { Suspend (null); }
		public static void Suspend (Exception except)
		{
			except = SuspendEx (except);
			if (except != null) {
				throw except;
			}
		}

		/*
		 * Resume a microthread that has suspended.
		 * Optionally pass an exception to the Suspend() call.
		 */
		public void Resume () { Resume (null); }
		public void Resume (Exception except)
		{
			except = ResumeEx (except);
			if (except != null) {
				throw except;
			}
		}

		/*
		 * This is a microthread giving up use of the CPU forever.
		 * Optionally pass an exception to the Start() or Resume() call.
		 */
		public static void Exit () { Exit (null); }
		public static void Exit (Exception except)
		{
			except = ExitEx (except);
			if (except != null) {
				throw except;
			}
		}

		/*
		 * Returns: 0: nothing started or has returned
		 *             Resume() must not be called
		 *             Start() may be called
		 *             Suspend() must not be called
		 *         -1: thread has called Suspend()
		 *             Resume() may be called
		 *             Start() may be called
		 *             Suspend() must not be called
		 *          1: thread is running
		 *             Resume() must not be called
		 *             Start() must not be called
		 *             Suspend() may be called
		 */
		public int Active ()
		{
			if (mmrUThread == UIntPtr.Zero) return 0;  // Dispose()'d
			return active (mmrUThread);
		}

		/*
		 * Get current uthread
		 * Returns null if not a uthread
		 */
		public static MMRUThread Current ()
		{
			return currentUThread;
		}

		/*
		 * Similar to API routines except they 
		 * return exception instead of throwing it.
		 */
		public Exception StartEx ()
		{
			if (mmrUThread == UIntPtr.Zero) {
				return new DestructedException ();
			}
			if (currentUThread != null) {
				return new AlreadyCurrentException ();
			}
			currentUThread = this;
			Exception except = start (mmrUThread);
			currentUThread = null;
			return except;
		}

		public static Exception SuspendEx (Exception except)
		{
			MMRUThread cur = currentUThread;
			if (cur == null) {
				return new NotInUThreadException ();
			}
			return suspend (cur.mmrUThread, except, false);
		}

		public Exception ResumeEx (Exception except)
		{
			if (mmrUThread == UIntPtr.Zero) {
				return new DestructedException ();
			}
			if (currentUThread != null) {
				return new AlreadyCurrentException ();
			}
			currentUThread = this;
			except = resume (mmrUThread, except);
			currentUThread = null;
			return except;
		}

		public static Exception ExitEx (Exception except)
		{
			MMRUThread cur = currentUThread;
			if (cur == null) {
				return new NotInUThreadException ();
			}
			return suspend (cur.mmrUThread, except, true);
		}
	}
}
