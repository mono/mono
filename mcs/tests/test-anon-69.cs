using System;

public delegate object TargetAccessDelegate (object user_data);

public class SingleSteppingEngine
{
        bool engine_stopped;

        object SendCommand (TargetAccessDelegate target)
        {
                return target (null);
        }

        public void Detach ()
        {
                SendCommand (delegate {
                        if (!engine_stopped) {
                                throw new InvalidOperationException ();
                        }

                        return null;
                });
        }
}

class X
{
        public static void Main ()
        { }
}