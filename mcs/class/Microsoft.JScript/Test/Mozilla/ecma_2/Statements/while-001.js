/* -*- Mode: C++; tab-width: 2; indent-tabs-mode: nil; c-basic-offset: 2 -*- */
/**
 *  File Name:          while-001
 *  ECMA Section:
 *  Description:        while statement
 *
 *  Verify that the while statement is not executed if the while expression is
 *  false
 *
 *  Author:             christine@netscape.com
 *  Date:               11 August 1998
 */
var SECTION = "while-001";
var VERSION = "ECMA_2";
var TITLE   = "while statement";

startTest();
writeHeaderToLog( SECTION + " "+ TITLE);

DoWhile();
test();

function DoWhile() {
  result = "pass";

  while (false) {
    result = "fail";
    break;
  }

  new TestCase(
    SECTION,
    "while statement: don't evaluate statement is expression is false",
    "pass",
    result );

}
