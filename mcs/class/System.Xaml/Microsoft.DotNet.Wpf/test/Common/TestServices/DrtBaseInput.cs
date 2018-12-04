using System;                       // InvalidOperationException
using System.Windows;               // UIElement, etc.
using System.Windows.Input ;        // Key
using MS.Internal;                  // PointUtil
using System.Windows.Automation;    // AutomationElement
using System.Windows.Media;         // Matrix
using System.Text;                  // StringBuilder
using System.Runtime.InteropServices; // DllImport
using System.ComponentModel;        // Win32Exception

namespace DRT
{
    // base class for a DRT application
    public abstract partial class DrtBase
    {
        #region Input

        /// <summary>
        /// Move the mouse to the specified position within the given element.  x and y are
        /// coordinates within the element, (0,0) is upper left, (1,1) is lower right.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="x">horizontal position:  0.0 = left edge,  1.0 = right edge</param>
        /// <param name="y">vertical position:    0.0 = top edge,   1.0 = bottom edge</param>
        public void MoveMouse(AutomationElement e, double x, double y)
        {
            Rect rc = e.Current.BoundingRectangle;
            MoveMouse(new Point(rc.Left + x * rc.Width, rc.Top + y * rc.Height));
        }

        string Identify(UIElement target)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(target.GetType().Name);
            sb.Append(".");
            sb.Append(target.GetHashCode().ToString());

            string name = null;
            FrameworkElement fe;
            if ((fe = target as FrameworkElement) != null)
            {
                name = fe.Name;
            }

            if (!String.IsNullOrEmpty(name))
            {
                sb.Append("\"");
                sb.Append(name);
                sb.Append("\"");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Click the mouse (at its current position).  Performs sanity checks before sending input.
        /// </summary>
        public void ClickMouse()
        {
            MouseButtonDown();
            MouseButtonUp();
        }

        /// <summary>
        /// Click the mouse (at its current position) using the second mouse button (respect SwapButtons).  Performs sanity checks before sending input.
        /// </summary>
        public void ClickMouseSecondButton()
        {
            MouseSecondButtonDown();
            MouseSecondButtonUp();
        }

        /// <summary>
        /// Press primary mouse button (respects SwapButtons).  Performs sanity checks before sending input.
        /// </summary>
        public void MouseButtonDown()
        {
            SendMouseButton(MouseButton.Left, true);
        }

        /// <summary>
        /// Release primary mouse button (respects SwapButtons).  Performs sanity checks before sending input.
        /// </summary>
        public void MouseButtonUp()
        {
            SendMouseButton(MouseButton.Left, false);
        }

        /// <summary>
        /// Press second mouse button down -- respect SwapButtons.  Performs sanity checks before sending input.
        /// </summary>
        public void MouseSecondButtonDown()
        {
            SendMouseButton(MouseButton.Right, true);
        }

        /// <summary>
        /// Release second mouse button -- respect SwapButtons.  Performs sanity checks before sending input.
        /// </summary>
        public void MouseSecondButtonUp()
        {
            SendMouseButton(MouseButton.Right, false);
        }

        public void MouseWheelUp()
        {
            SendMouseWheel(120);
        }

        public void MouseWheelDown()
        {
            SendMouseWheel(-120);
        }

        /// <summary>
        /// Press or release a key.  Performs sanity checks before sending input.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="press"></param>
        public void SendKeyboardInput(Key key, bool press)
        {
            SendKeyStrokes(new KeyStatePair(key, press));
        }

        /// <summary>
        /// Press and release the given key.
        /// </summary>
        /// <param name="key"></param>
        public void PressKey(Key key)
        {
            SendKeyStrokes(new KeyStatePair(key, true), new KeyStatePair(key, false));
        }

        /// <summary>
        /// Type an entire string.
        /// </summary>
        /// <param name="key"></param>
        public void SendString(string s)
        {
            EnsureResetInputStateHandler();
            PrepareToSendInput();

            foreach (char c in s)
            {
                Key key = KeyFromChar(c);

                if (IsUpper(c))
                    Input.SendKeyboardInput(Key.LeftShift, true);   // press Shift

                Input.SendKeyboardInput(key, true);                 // press
                Input.SendKeyboardInput(key, false);                // release

                if (IsUpper(c))
                    Input.SendKeyboardInput(Key.LeftShift, false);  // release Shift
            }
        }

        private bool IsUpper(char c)
        {
            if (c == ':') return true;
            if (c == '?') return true;
            if (c == '_') return true;
            return Char.IsUpper(c);
        }

        private Key KeyFromChar(Char c)
        {
            if ('a' <= c && c <= 'z')   return (Key)((int)(c - 'a') + Key.A);
            if ('A' <= c && c <= 'Z')   return (Key)((int)(c - 'A') + Key.A);
            if ('0' <= c && c <= '9')   return (Key)((int)(c - '0') + Key.D0);
            if (c == ';' || c == ':') return Key.OemSemicolon;
            if (c == '/' || c == '?') return Key.Divide;
            if (c == '-' || c == '_') return Key.Subtract;
            if (c == ' ') return Key.Space;
            throw new NotSupportedException("Can't handle that char yet - please extend");
        }

        // NOTE: the following three functions are virtual to allow subclasses to hook
        //       these and do additional work and/or checks.

        /// <summary>
        ///     Moves the mouse to the specified screen coordinate.
        /// </summary>
        /// <param name="pt">The mouse position in screen coordinates.</param>
        public virtual void MoveMouse(Point ptScreen)
        {
            EnsureResetInputStateHandler();
            PrepareToSendInput();
            Input.MoveTo(ptScreen);
        }

        public virtual void SendKeyStrokes(params KeyStatePair[] keyStatePairs)
        {
            EnsureResetInputStateHandler();
            PrepareToSendInput();

            for (int i = 0; i < keyStatePairs.Length; i++)
            {
                Input.SendKeyboardInput(keyStatePairs[i].Key, keyStatePairs[i].Press);
            }
        }

        public virtual void SendMouseButton(MouseButton button, bool press)
        {
            EnsureResetInputStateHandler();

            PrepareToSendInput();
            bool swapButtons = SystemParameters.SwapButtons;
            if (button == MouseButton.Left)
            {
                if (press)
                {
                    Input.SendMouseInput(0, 0, 0, swapButtons ? SendMouseInputFlags.RightDown : SendMouseInputFlags.LeftDown);
                }
                else
                {
                    Input.SendMouseInput(0, 0, 0, swapButtons ? SendMouseInputFlags.RightUp : SendMouseInputFlags.LeftUp);
                }
            }
            else if (button == MouseButton.Right)
            {
                if (press)
                {
                    Input.SendMouseInput(0, 0, 0, swapButtons ? SendMouseInputFlags.LeftDown : SendMouseInputFlags.RightDown);
                }
                else
                {
                    Input.SendMouseInput(0, 0, 0, swapButtons ? SendMouseInputFlags.LeftUp : SendMouseInputFlags.RightUp);
                }
            }
            else
            {
                // TODO: support other mouse buttons
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Scroll the mouse wheel by the given amount.
        /// </summary>
        /// <param name="amount"></param>
        protected virtual void SendMouseWheel(int amount)
        {
            EnsureResetInputStateHandler();
            PrepareToSendInput();

            Input.SendMouseInput(0, 0, amount, SendMouseInputFlags.Wheel);
        }

        private void EnsureResetInputStateHandler()
        {
            if (_resetInputState == null)
            {
                _resetInputState = new EventHandler(ResetInputStateHandler);
                ResetInputState += _resetInputState;
            }
        }

        private void ResetInputStateHandler(object sender, EventArgs e)
        {
            if (Mouse.Captured != null)
            {
                ConsoleOut.WriteLine("INVALID CONFIGURATION: Mouse capture left on, resetting.");
                Mouse.Capture(null);
            }

            bool swapButtons = SystemParameters.SwapButtons;
            if (Mouse.RightButton != MouseButtonState.Released)
            {
                ConsoleOut.WriteLine("INVALID CONFIGURATION: Right mouse button is down, resetting.");
                Input.SendMouseInput(0, 0, 0, swapButtons ? SendMouseInputFlags.LeftUp : SendMouseInputFlags.RightUp);
            }

            if (Mouse.MiddleButton != MouseButtonState.Released)
            {
                ConsoleOut.WriteLine("INVALID CONFIGURATION: Right mouse button is down, resetting.");
                Input.SendMouseInput(0, 0, 0, SendMouseInputFlags.MiddleUp);
            }

            if (Mouse.LeftButton != MouseButtonState.Released)
            {
                ConsoleOut.WriteLine("INVALID CONFIGURATION: Left mouse button is down, resetting.");
                Input.SendMouseInput(0, 0, 0, swapButtons ? SendMouseInputFlags.RightUp : SendMouseInputFlags.LeftUp);
            }
        }

        private EventHandler _resetInputState;

        #endregion
    }
    //
    // Consider combining all uses of UnsafeNativeMethods.cs, NativeMethods.cs, etc... from
    // specific DRTs and putting them in a shared version under devtest\drts\drtbase but for
    // now just define w/in this module.
    //
    internal class InternalUnsafeNativeMethods
    {
        [DllImport( "user32.dll", ExactSpelling = true, CharSet = CharSet.Auto )]
        public static extern int GetSystemMetrics( int nIndex );

        //
        // SendInput related
        //

        [StructLayout( LayoutKind.Sequential )]
        public struct INPUT
        {
            public int type;
            public INPUTUNION union;
        };

        [StructLayout( LayoutKind.Explicit )]
        public struct INPUTUNION
        {
            [FieldOffset( 0 )]
            public MOUSEINPUT mouseInput;
            [FieldOffset( 0 )]
            public KEYBDINPUT keyboardInput;
        };

        [StructLayout( LayoutKind.Sequential )]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        };

        [StructLayout( LayoutKind.Sequential )]
        public struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        };

        [DllImport( "user32.dll", SetLastError = true )]
        public static extern int SendInput( int nInputs, ref INPUT mi, int cbSize );

        [DllImport( "user32.dll", CharSet = CharSet.Auto )]
        public static extern int MapVirtualKey( int nVirtKey, int nMapType );
    }

    internal class InternalNativeMethods
    {
        public const int SM_SWAPBUTTON = 23;
        public const int SM_XVIRTUALSCREEN = 76;
        public const int SM_YVIRTUALSCREEN = 77;
        public const int SM_CXVIRTUALSCREEN = 78;
        public const int SM_CYVIRTUALSCREEN = 79;

        //
        // SendInput related
        //

        public const int VK_SHIFT = 0x10;
        public const int VK_CONTROL = 0x11;
        public const int VK_MENU = 0x12;

        public const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const int KEYEVENTF_KEYUP = 0x0002;
        public const int KEYEVENTF_UNICODE = 0x0004;
        public const int KEYEVENTF_SCANCODE = 0x0008;

        public const int MOUSEEVENTF_VIRTUALDESK = 0x4000;

        public const int INPUT_MOUSE = 0;
        public const int INPUT_KEYBOARD = 1;
    }

    /// <summary>
    /// Flags for Input.SendMouseInput, indicate whether movent took place,
    /// or whether buttons were pressed or released.
    /// </summary>
    [Flags]
    public enum SendMouseInputFlags
    {
        /// <summary>Specifies that the pointer moved.</summary>
        Move = 0x0001,
        /// <summary>Specifies that the left button was pressed.</summary>
        LeftDown = 0x0002,
        /// <summary>Specifies that the left button was released.</summary>
        LeftUp = 0x0004,
        /// <summary>Specifies that the right button was pressed.</summary>
        RightDown = 0x0008,
        /// <summary>Specifies that the right button was released.</summary>
        RightUp = 0x0010,
        /// <summary>Specifies that the middle button was pressed.</summary>
        MiddleDown = 0x0020,
        /// <summary>Specifies that the middle button was released.</summary>
        MiddleUp = 0x0040,
        /// <summary>Specifies that the x button was pressed.</summary>
        XDown = 0x0080,
        /// <summary>Specifies that the x button was released. </summary>
        XUp = 0x0100,
        /// <summary>Specifies that the wheel was moved</summary>
        Wheel = 0x0800,
        /// <summary>Specifies that x, y are absolute, not relative</summary>
        Absolute = 0x8000,
    };

    // Consider removing the Input class and rolling these methods into DrtBase as
    // general helpers for doing input

    /// <summary>
    /// Provides methods for sending mouse and keyboard input
    /// </summary>
    public sealed class Input
    {
        //------------------------------------------------------
        //
        //  Constructors
        //
        //------------------------------------------------------

        #region Constructors

        // Static class - Private to prevent creation
        Input()
        {
        }

        #endregion Constructors

        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        /// <summary>
        /// Inject pointer input into the system
        /// </summary>
        /// <param name="x">x coordinate of pointer, if Move flag specified</param>
        /// <param name="y">y coordinate of pointer, if Move flag specified</param>
        /// <param name="data">wheel movement, or mouse X button, depending on flags</param>
        /// <param name="flags">flags to indicate which type of input occurred - move, button press/release, wheel move, etc.</param>
        /// <remarks>x, y are in pixels. If Absolute flag used, are relative to desktop origin.</remarks>
        public static void SendMouseInput( double x, double y, int data, SendMouseInputFlags flags )
        {
            int intflags = (int)flags;

            if ( ( intflags & (int)SendMouseInputFlags.Absolute ) != 0 )
            {
                int vscreenWidth = InternalUnsafeNativeMethods.GetSystemMetrics( InternalNativeMethods.SM_CXVIRTUALSCREEN );
                int vscreenHeight = InternalUnsafeNativeMethods.GetSystemMetrics( InternalNativeMethods.SM_CYVIRTUALSCREEN );
                int vscreenLeft = InternalUnsafeNativeMethods.GetSystemMetrics( InternalNativeMethods.SM_XVIRTUALSCREEN );
                int vscreenTop = InternalUnsafeNativeMethods.GetSystemMetrics( InternalNativeMethods.SM_YVIRTUALSCREEN );

                // Absolute input requires that input is in 'normalized' coords - with the entire
                // desktop being (0,0)...(65535,65536). Need to convert input x,y coords to this
                // first.
                //
                // In this normalized world, any pixel on the screen corresponds to a block of values
                // of normalized coords - eg. on a 1024x768 screen,
                // y pixel 0 corresponds to range 0 to 85.333,
                // y pixel 1 corresponds to range 85.333 to 170.666,
                // y pixel 2 correpsonds to range 170.666 to 256 - and so on.
                // Doing basic scaling math - (x-top)*65536/Width - gets us the start of the range.
                // However, because int math is used, this can end up being rounded into the wrong
                // pixel. For example, if we wanted pixel 1, we'd get 85.333, but that comes out as
                // 85 as an int, which falls into pixel 0's range - and that's where the pointer goes.
                // To avoid this, we add on half-a-"screen pixel"'s worth of normalized coords - to
                // push us into the middle of any given pixel's range - that's the 65536/(Width*2)
                // part of the formula. So now pixel 1 maps to 85+42 = 127 - which is comfortably
                // in the middle of that pixel's block.
                // The key ting here is that unlike points in coordinate geometry, pixels take up
                // space, so are often better treated like rectangles - and if you want to target
                // a particular pixel, target its rectangle's midpoint, not its edge.
                x = ( ( x - vscreenLeft ) * 65536 ) / vscreenWidth + 65536 / ( vscreenWidth * 2 );
                y = ( ( y - vscreenTop ) * 65536 ) / vscreenHeight + 65536 / ( vscreenHeight * 2 );

                intflags |= InternalNativeMethods.MOUSEEVENTF_VIRTUALDESK;
            }

            InternalUnsafeNativeMethods.INPUT mi = new InternalUnsafeNativeMethods.INPUT();
            mi.type = InternalNativeMethods.INPUT_MOUSE;
            mi.union.mouseInput.dx = (int)x;
            mi.union.mouseInput.dy = (int)y;
            mi.union.mouseInput.mouseData = data;
            mi.union.mouseInput.dwFlags = intflags;
            mi.union.mouseInput.time = 0;
            mi.union.mouseInput.dwExtraInfo = new IntPtr( 0 );
            //Console.WriteLine("Sending");
            if ( InternalUnsafeNativeMethods.SendInput( 1, ref mi, Marshal.SizeOf( mi ) ) == 0 )
                throw new Win32Exception( Marshal.GetLastWin32Error() );
        }


        /// <summary>
        /// Inject keyboard input into the system
        /// </summary>
        /// <param name="key">indicates the key pressed or released. Can be one of the constants defined in the Key enum</param>
        /// <param name="press">true to inject a key press, false to inject a key release</param>
        public static void SendKeyboardInput( Key key, bool press )
        {
            InternalUnsafeNativeMethods.INPUT ki = new InternalUnsafeNativeMethods.INPUT();
            ki.type = InternalNativeMethods.INPUT_KEYBOARD;
            ki.union.keyboardInput.wVk = (short)KeyInterop.VirtualKeyFromKey( key );
            ki.union.keyboardInput.wScan = (short)InternalUnsafeNativeMethods.MapVirtualKey( ki.union.keyboardInput.wVk, 0 );
            int dwFlags = 0;
            if ( ki.union.keyboardInput.wScan > 0 )
                dwFlags |= InternalNativeMethods.KEYEVENTF_SCANCODE;
            if ( !press )
                dwFlags |= InternalNativeMethods.KEYEVENTF_KEYUP;
            ki.union.keyboardInput.dwFlags = dwFlags;
            if ( IsExtendedKey( key ) )
            {
                ki.union.keyboardInput.dwFlags |= InternalNativeMethods.KEYEVENTF_EXTENDEDKEY;
            }
            ki.union.keyboardInput.time = 0;
            ki.union.keyboardInput.dwExtraInfo = new IntPtr( 0 );
            if ( InternalUnsafeNativeMethods.SendInput( 1, ref ki, Marshal.SizeOf( ki ) ) == 0 )
                throw new Win32Exception( Marshal.GetLastWin32Error() );
        }

        /// <summary>
        /// Move the mouse to an element.
        /// </summary>
        /// <param name="el">The element that the mouse will move to</param>
        /// <exception cref="NoClickablePointException">If there is not clickable point for the element</exception>
        public static void MoveTo( AutomationElement el )
        {
            if ( el == null )
            {
                throw new ArgumentNullException( "el" );
            }
            MoveTo( el.GetClickablePoint() );
        }

        /// <summary>
        /// Move the mouse to a point.
        /// </summary>
        /// <param name="pt">The point that the mouse will move to.</param>
        /// <remarks>pt are in pixels that are relative to desktop origin.</remarks>
        public static void MoveTo( Point pt )
        {
            Input.SendMouseInput( pt.X, pt.Y, 0, SendMouseInputFlags.Move | SendMouseInputFlags.Absolute );
        }

        /// <summary>
        /// Move the mouse to an element and click on it.  The primary mouse button will be used
        /// this is usually the left button except if the mouse buttons are swaped.
        /// </summary>
        /// <param name="el">The element to click on</param>
        /// <exception cref="NoClickablePointException">If there is not clickable point for the element</exception>
        public static void MoveToAndClick( AutomationElement el )
        {
            if ( el == null )
            {
                throw new ArgumentNullException( "el" );
            }
            MoveToAndClick( el.GetClickablePoint() );
        }

        /// <summary>
        /// Move the mouse to a point and click.  The primary mouse button will be used
        /// this is usually the left button except if the mouse buttons are swaped.
        /// </summary>
        /// <param name="pt">The point to click at</param>
        /// <remarks>pt are in pixels that are relative to desktop origin.</remarks>
        public static void MoveToAndClick( Point pt )
        {
            Input.SendMouseInput( pt.X, pt.Y, 0, SendMouseInputFlags.Move | SendMouseInputFlags.Absolute );

            // send SendMouseInput works in term of the phisical mouse buttons, therefore we need
            // to check to see if the mouse buttons are swapped because this method need to use the primary
            // mouse button.
            if ( InternalUnsafeNativeMethods.GetSystemMetrics( InternalNativeMethods.SM_SWAPBUTTON ) == 0 )
            {
                // the mouse buttons are not swaped the primary is the left
                Input.SendMouseInput( pt.X, pt.Y, 0, SendMouseInputFlags.LeftDown | SendMouseInputFlags.Absolute );
                Input.SendMouseInput( pt.X, pt.Y, 0, SendMouseInputFlags.LeftUp | SendMouseInputFlags.Absolute );
            }
            else
            {
                // the mouse buttons are swaped so click the right button which as actually the primary
                Input.SendMouseInput( pt.X, pt.Y, 0, SendMouseInputFlags.RightDown | SendMouseInputFlags.Absolute );
                Input.SendMouseInput( pt.X, pt.Y, 0, SendMouseInputFlags.RightUp | SendMouseInputFlags.Absolute );
            }
        }

        #endregion Public Methods


        //------------------------------------------------------
        //
        //  Private Methods
        //
        //------------------------------------------------------

        #region Private Methods

        private static bool IsExtendedKey( Key key )
        {
            // From the SDK:
            // The extended-key flag indicates whether the keystroke message originated from one of
            // the additional keys on the enhanced keyboard. The extended keys consist of the ALT and
            // CTRL keys on the right-hand side of the keyboard; the INS, DEL, HOME, END, PAGE UP,
            // PAGE DOWN, and arrow keys in the clusters to the left of the numeric keypad; the NUM LOCK
            // key; the BREAK (CTRL+PAUSE) key; the PRINT SCRN key; and the divide (/) and ENTER keys in
            // the numeric keypad. The extended-key flag is set if the key is an extended key.
            //
            // - docs appear to be incorrect. Use of Spy++ indicates that break is not an extended key.
            // Also, menu key and windows keys also appear to be extended.
            return key == Key.RightAlt
                || key == Key.RightCtrl
                || key == Key.NumLock
                || key == Key.Insert
                || key == Key.Delete
                || key == Key.Home
                || key == Key.End
                || key == Key.Prior
                || key == Key.Next
                || key == Key.Up
                || key == Key.Down
                || key == Key.Left
                || key == Key.Right
                || key == Key.Apps
                || key == Key.RWin
                || key == Key.LWin;

            // Note that there are no distinct values for the following keys:
            // numpad divide
            // numpad enter
        }

        #endregion Private Methods
    }

}
