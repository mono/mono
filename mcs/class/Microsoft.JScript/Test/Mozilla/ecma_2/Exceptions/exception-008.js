/* -*- Mode: C++; tab-width: 2; indent-tabs-mode: nil; c-basic-offset: 2 -*- */
/**
 *  File Name:          exception-008
 *  ECMA Section:
 *  Description:        Tests for JavaScript Standard Exceptions
 *
 *  SyntaxError.
 *
 *  Author:             christine@netscape.com
 *  Date:               31 August 1998
 */
var SECTION = "exception-008";
var VERSION = "js1_4";
var TITLE   = "Tests for JavaScript Standard Exceptions: SyntaxError";

startTest();
writeHeaderToLog( SECTION + " "+ TITLE);

Syntax_1();

test();

function Syntax_1() {
  result = "failed: no exception thrown";
  exception = null;

  try {
    result = eval("continue;");
  } catch ( e ) {
    result = "passed:  threw exception",
      exception = e.toString();
  } finally {
    new TestCase(
      SECTION,
      "eval(\"continue\") [ exception is " + exception +" ]",
      "passed:  threw exception",
      result );
  }
}
