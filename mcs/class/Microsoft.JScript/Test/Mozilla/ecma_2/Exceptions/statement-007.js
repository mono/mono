/* -*- Mode: C++; tab-width: 2; indent-tabs-mode: nil; c-basic-offset: 2 -*- */
/**
   File Name:          statement-007.js
   Corresponds To:     12.7-1-n.js
   ECMA Section:       12.7 The continue statement
   Description:

   Author:             christine@netscape.com
   Date:               12 november 1997
*/
var SECTION = "statement-007";
var VERSION = "JS1_4";
var TITLE   = "The continue statement";

startTest();
writeHeaderToLog( SECTION + " "+ TITLE);

var result = "Failed";
var exception = "No exception thrown";
var expect = "Passed";

try {
  eval("continue;");
} catch ( e ) {
  result = expect;
  exception = e.toString();
}

new TestCase(
  SECTION,
  "continue outside of an iteration statement" +
  " (threw " + exception +")",
  expect,
  result );

test();

