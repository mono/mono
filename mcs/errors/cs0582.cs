// cs0582.cs: Conditional not valid on interface members
// Line: 5

interface Interface {
        [System.Diagnostics.ConditionalAttribute("DEBUG")]
        void Method ();
}

