// CS0245: Destructors and object.Finalize cannot be called directly. Consider calling IDisposable.Dispose if available
// Line: 7

class Sample {
        void Close ()
        {
                this.Finalize();
        }
}
