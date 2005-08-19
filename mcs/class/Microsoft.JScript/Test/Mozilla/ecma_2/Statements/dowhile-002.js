/* -*- Mode: C++; tab-width: 2; indent-tabs-mode: nil; c-basic-offset: 2 -*- */
/**
 *  File Name:          dowhile-002
 *  ECMA Section:
 *  Description:        do...while statements
 *
 *  Verify that code after a labeled break is not executed.  Verify that
 *  a labeled break breaks you out of the whole labeled block, and not
 *  just the current iteration statement.
 *
 *  Author:             christine@netscape.com
 *  Date:               11 August 1998
 */
var SECTION = "dowhile-002";
var VERSION = "ECMA_2";
var TITLE   = "do...while with a labeled continue statement";

startTest();
writeHeaderToLog( SECTION + " "+ TITLE);

LabeledContinue( 0, 1 );
LabeledContinue( 1, 1 );
LabeledContinue( -1, 1 );
LabeledContinue( 5, 5 );

test();

// The labeled statement contains statements after the labeled break.
// Verify that the statements after the break are not executed.

function LabeledContinue( limit, expect ) {
  i = 0;
  result1 = "pass";
  result2 = "pass";

woohoo: {
    do {
      i++;
      if ( ! (i < limit) ) {
	break woohoo;
	result1 = "fail: evaluated statement after a labeled break";
      }
    } while ( true );

    result2 = "failed:  broke out of loop, but not out of labeled block";
  }

  new TestCase(
    SECTION,
    "do while ( " + i +" < " + limit +" )",
    expect,
    i );

  new TestCase(
    SECTION,
    "breaking out of a do... while loop",
    "pass",
    result1 );


  new TestCase(
    SECTION,
    "breaking out of a labeled do...while loop",
    "pass",
    result2 );
}
