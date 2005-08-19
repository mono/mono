/**
    File Name:
    ECMA Section:
    Description:        Call Objects



    Author:             christine@netscape.com
    Date:               12 november 1997
*/
    var SECTION = "";
    var VERSION = "ECMA_2";
    var TITLE   = "The Call Constructor";

    startTest();
    writeHeaderToLog( SECTION + " "+ TITLE);

    var b = new Boolean();

    new TestCase( SECTION,
                  "var b = new Boolean(); b instanceof Boolean",
                  true,
                  b instanceof Boolean );

    new TestCase( SECTION,
                  "b instanceof Object",
                  true,
                  b instanceof Object );

    new TestCase( SECTION,
                  "b instanceof Array",
                  false,
                  b instanceof Array );

    new TestCase( SECTION,
                  "true instanceof Boolean",
                  false,
                  true instanceof Boolean );

    new TestCase( SECTION,
                  "Boolean instanceof Object",
                  true,
                  Boolean instanceof Object );
    test();

