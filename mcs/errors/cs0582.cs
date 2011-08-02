// CS0582: Conditional not valid on interface members
// Line: 5

interface Interface {
        [System.Diagnostics.ConditionalAttribute("DEBUG")]
        void Method ();
}

