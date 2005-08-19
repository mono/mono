/* -*- Mode: C++; tab-width: 2; indent-tabs-mode: nil; c-basic-offset: 2 -*- */
/**
   File Name:          call-1.js
   Section:            Function.prototype.call
   Description:


   Author:             christine@netscape.com
   Date:               12 november 1997
*/
var SECTION = "call-1";
var VERSION = "ECMA_2";
var TITLE   = "Function.prototype.call";

startTest();
writeHeaderToLog( SECTION + " "+ TITLE);

new TestCase( SECTION,
	      "ToString.call( this, this )",
	      GLOBAL,
	      ToString.call( this, this ) );

new TestCase( SECTION,
	      "ToString.call( Boolean, Boolean.prototype )",
	      "false",
	      ToString.call( Boolean, Boolean.prototype ) );

new TestCase( SECTION,
	      "ToString.call( Boolean, Boolean.prototype.valueOf() )",
	      "false",
	      ToString.call( Boolean, Boolean.prototype.valueOf() ) );

test();

function ToString( obj ) {
  return obj +"";
}
