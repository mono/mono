// cs0245.cs: Destructors and object.Finalize cannot be called directly. Consider calling IDisposable.Dispose if available
// Line: 6

class Sample {
        void Close () {
                this.Finalize();
        }
}

