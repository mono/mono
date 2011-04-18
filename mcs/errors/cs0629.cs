// CS0629: Conditional member `DerivedClass.Show(int)' cannot implement interface member `IFace.Show(int)'
// Line: 12

interface IFace
{
        void Show (int arg);
}

class DerivedClass: IFace
{
        [System.Diagnostics.Conditional("DEBUG")]
        public void Show (int arg) {}
}
