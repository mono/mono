using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Permissions;

namespace System.Media {
    /// <include file='doc\systemsounds.uex' path='docs/doc[@for="SystemSounds"]/*' />
    [HostProtection(UI = true)]
    public sealed class SystemSounds {
        static volatile SystemSound asterisk;
        static volatile SystemSound beep;
        static volatile SystemSound exclamation;
        static volatile SystemSound hand;
        static volatile SystemSound question;

        private SystemSounds() {
        }

        /// <include file='doc\systemsounds.uex' path='docs/doc[@for="SystemSounds.Asterisk"]/*' />
        public static SystemSound Asterisk {
            get {
                if (asterisk == null) {
                    asterisk = new SystemSound(NativeMethods.MB_ICONASTERISK);
                }
                return asterisk;
            }
        }

        /// <include file='doc\systemsounds.uex' path='docs/doc[@for="SystemSounds.Beep"]/*' />
        public static SystemSound Beep {
            get {
                if (beep == null) {
                    beep = new SystemSound(0);
                }
                return beep;
            }
        }

        /// <include file='doc\systemsounds.uex' path='docs/doc[@for="SystemSounds.Exclamation"]/*' />
        public static SystemSound Exclamation {
            get {
                if (exclamation == null) {
                    exclamation = new SystemSound(NativeMethods.MB_ICONEXCLAMATION);
                }
                return exclamation;
            }
        }

        /// <include file='doc\systemsounds.uex' path='docs/doc[@for="SystemSounds.Hand"]/*' />
        public static SystemSound Hand {
            get {
                if (hand == null) {
                    hand = new SystemSound(NativeMethods.MB_ICONHAND);
                }
                return hand;
            }
        }

        /// <include file='doc\systemsounds.uex' path='docs/doc[@for="SystemSounds.Question"]/*' />
        public static SystemSound Question {
            get {
                if (question == null) {
                    question = new SystemSound(NativeMethods.MB_ICONQUESTION);
                }
                return question;
            }
        }
        private class NativeMethods {
           // Constructor added because of FxCop rules
           private NativeMethods() {}

           internal const int MB_ICONHAND = 0x000010,
           MB_ICONQUESTION = 0x000020,
           MB_ICONEXCLAMATION = 0x000030,
           MB_ICONASTERISK = 0x000040;
        }
    }

    /// <include file='doc\systemsounds.uex' path='docs/doc[@for="SystemSound.SystemSound"]/*' />
    [HostProtection(UI = true)]
    public class SystemSound {
        private int soundType;
        internal SystemSound(int soundType) {
            this.soundType = soundType;
        }

        /// <include file='doc\systemsounds.uex' path='docs/doc[@for="SystemSound.Play"]/*' />
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts")]
        public void Play() {
            IntSecurity.UnmanagedCode.Assert();
            try {
                SafeNativeMethods.MessageBeep(soundType);
            } finally {
                System.Security.CodeAccessPermission.RevertAssert();
            }
        }

        private class SafeNativeMethods {
            // Constructor added because of FxCop rules
            private SafeNativeMethods() {}

            [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
            [ResourceExposure(ResourceScope.None)]
            internal static extern bool MessageBeep(int type);
        }
    }
}
