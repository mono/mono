// CS1026: Unexpected symbol `;', expecting `)'
// Line: 10

using System;

class Test {
        static void Main ()
        {
                string uri = "http://localhost/";
                int default_port = (uri.StartsWith ("http://") ? 80 : -1;
        }
}
