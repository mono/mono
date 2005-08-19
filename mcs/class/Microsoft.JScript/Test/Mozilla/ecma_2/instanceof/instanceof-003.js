/**
    File Name:          instanceof-003.js
    ECMA Section:
    Description:        http://bugzilla.mozilla.org/show_bug.cgi?id=7635

js> function Foo() {}
js> theproto = {};
[object Object]
js> Foo.prototype = theproto
[object Object]
js> theproto instanceof Foo
true

I think this should be 'false'


    Author:             christine@netscape.com
    Date:               12 november 1997

Modified to conform to ECMA3
https://bugzilla.mozilla.org/show_bug.cgi?id=281606
*/
    var SECTION = "instanceof-003";
    var VERSION = "ECMA_2";
    var TITLE   = "instanceof operator";
    var BUGNUMBER ="http://bugzilla.mozilla.org/show_bug.cgi?id=7635";

    startTest();

    function Foo() {};
    theproto = {};
    Foo.prototype = theproto;

    AddTestCase(
        "function Foo() = {}; theproto = {}; Foo.prototype = theproto; " +
            "theproto instanceof Foo",
        false,
        theproto instanceof Foo );


    var o = {};

// https://bugzilla.mozilla.org/show_bug.cgi?id=281606
try
{
    AddTestCase(
        "o = {}; o instanceof o",
        "error",
        o instanceof o );
}
catch(e)
{
    AddTestCase(
        "o = {}; o instanceof o",
        "error",
        "error" );
}

    test();
