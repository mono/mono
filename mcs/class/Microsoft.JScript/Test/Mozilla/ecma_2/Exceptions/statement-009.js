/* -*- Mode: C++; tab-width: 2; indent-tabs-mode: nil; c-basic-offset: 2 -*- */
/**
   File Name:          12.9-1-n.js
   ECMA Section:       12.9 The return statement
   Description:

   Author:             christine@netscape.com
   Date:               12 november 1997
*/
var SECTION = "12.9-1-n";
var VERSION = "ECMA_1";
var TITLE   = "The return statement";

startTest();
writeHeaderToLog( SECTION + " The return statement");

var result = "Failed";
var exception = "No exception thrown";
var expect = "Passed";

try {
  eval("return;");
} catch ( e ) {
  result = expect;
  exception = e.toString();
}

new TestCase(
  SECTION,
  "return outside of a function" +
  " (threw " + exception +")",
  expect,
  result );

test();

