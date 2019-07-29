//describe, beforeAll, it, expect - are the Jasmine default methods
//karmaHTML is the karma-html package object with the access to all its features
 
describe("The WebAssembly Zip Test Suite",function(){
    
    const DEFAULT_TIMEOUT = 1000;
    const DEFAULT_WS_TIMEOUT = 5000;

    beforeAll(function(done){
      //load DOM custom matchers from karma-jasmine-dom package
      jasmine.addMatchers(DOMCustomMatchers);
      
      //lets open our 'http-spec.html' file in the browser by 'index' tag as you specified in 'karma.conf.js'
      karmaHTML.zipspec.open();
      
      //karmaHTML.zipspec.onstatechange fires when the Document is loaded
      //now the tests can be executed on the DOM
      karmaHTML.zipspec.onstatechange = function(ready){
        //if the #Document is ready, fire tests
        //the done() callback is the jasmine native async-support function
        if(ready) {
          karmaHTML.zipspec.document.onRuntimeDone = function ()
          {
            done();
          }

        }
      };

    });
    
    it('ZipGetEntryReadMode: entry for foo.txt should exist', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipGetEntryReadMode", ["foo.txt"]).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.isDefined(result, "result should not be undefined.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);

    it('ZipGetEntryReadMode: entry for foobar.txt should not exist', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipGetEntryReadMode", ["foobar.txt"]).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.isUndefined(result, "result should be undefined but it seems we have an entry.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);

    it('ZipGetEntryCreateMode: accessing entry in Create Mode should fail.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipGetEntryCreateMode", []).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              done.fail("CreateMode: should fail in this case with - System.NotSupportedException: Cannot access entries in Create mode.")
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done(error)

      );
      
    }, DEFAULT_TIMEOUT);

    it('ZipGetEntryUpdateMode: entry for foo.txt should exist', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipGetEntryUpdateMode", ["foo.txt"]).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.isDefined(result, "result should not be undefined.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);

    it('ZipGetEntryUpdateMode: entry for foobar.txt should not exist', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipGetEntryUpdateMode", ["foobar.txt"]).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.isUndefined(result, "result should be undefined but it seems we have an entry.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);

    it('ZipGetEntryOpen: should open entry foo.txt ', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipGetEntryOpen", []).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.isDefined(result, "result should be the opend entry.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);    

    it('ZipOpenAndReopenEntry: reopening an entry should fail.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipOpenAndReopenEntry", []).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              done.fail("Reopening entry: should fail in this case with - System.IO.IOException: Entries cannot be opened multiple times in Update mode.")
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done(error)

      );
      
    }, DEFAULT_TIMEOUT);

    it('ZipOpenCloseAndReopenEntry: opening, closing and then reopening should not fail.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipOpenCloseAndReopenEntry", []).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.isTrue(result, "result should be true the opened entry.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);

    it('ZipDeleteEntryCheckEntries: should delete entry.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipDeleteEntryCheckEntries", []).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.isUndefined(result, "result should be undefined.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);

    it('ZipGetEntryDeleteUpdateMode: should delete entry in update mode.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipGetEntryDeleteUpdateMode", []).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.isUndefined(result, "result should be undefined.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);    

    it('ZipCreateArchive: should create archive.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipCreateArchive", []).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.equal(result.length, 3, "result length doesn't match 3");
              assert.isDefined(result[0], "result[0] should be an object");
              assert.isDefined(result[1], "result[1] should be an object");
              assert.equal(result[2], "foo", "result[2] doesn't match text foo");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);    

    it('ZipEnumerateEntriesModifiedTime: should enumerate entry modified time.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipEnumerateEntriesModifiedTime", []).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            var resultNow = new Date();
            try {
              assert.equal(result.length, 3, "result length doesn't match 3");
              assert.equal(result[0], resultNow.getFullYear() , "result[0] doesn't match current year");
              // Month is a zero-based value (where zero indicates the first month of the year)
              assert.equal(result[1], resultNow.getMonth() + 1, "result[1] doesn't match current month");
              assert.equal(result[2], resultNow.getDate(), "result[2] doesn't match current day");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);    

    it('ZipEnumerateArchiveDefaultLastWriteTime: should return archive last write time.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipEnumerateArchiveDefaultLastWriteTime", []).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.equal(result, 0, "result does not match .NET ticks");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);    

    it('ZipGetArchiveEntryStreamLengthPositionReadMode: should return archive entry length values.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipGetArchiveEntryStreamLengthPositionReadMode", []).then(
        (result) => 
        {
            //console.log("we are here: " + result);
            try {
              assert.equal(result.length, 2, "result does not match expected result values");
              assert.equal(result[0], 0, "result[0] does not match expected result position: 0");
              assert.equal(result[1], 425, "result[0] does not match expected result length: 425");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);    

    it('ZipGetArchiveEntryStreamLengthPositionUpdateMode: should return archive entry length values.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipGetArchiveEntryStreamLengthPositionUpdateMode", []).then(
        (result) => 
        {
            try {
              assert.equal(result.length, 4, "result does not match expected result values");
              assert.equal(result[0], 0, "result[0] does not match expected result position: 0");
              assert.equal(result[1], 425, "result[0] does not match expected result length: 425");
              assert.equal(result[2], 857, "result[0] does not match expected result length.");
              assert.equal(result[3], 0, "result[0] does not match expected result position.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);    

    it('ZipEnumerateEntriesReadMode: should enumerate archive entries.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipEnumerateEntriesReadMode", []).then(
        (result) => 
        {
            try {
              assert.equal(result.length, 5, "result does not match expected result values");
              assert.equal(result[0], "bar.txt", "result[0] does not match expected result.");
              assert.equal(result[1], "foo.txt", "result[1] does not match expected result.");
              assert.equal(result[2], "foobar/", "result[2] does not match expected result.");
              assert.equal(result[3], "foobar/bar.txt", "result[3] does not match expected.");
              assert.equal(result[4], "foobar/foo.txt", "result[4] does not match expected.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);    

    it('ZipEnumerateEntriesUpdateMode: should enumerate archive entries.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipEnumerateEntriesUpdateMode", []).then(
        (result) => 
        {
            try {
              assert.equal(result.length, 5, "result does not match expected result values");
              assert.equal(result[0], "bar.txt", "result[0] does not match expected result.");
              assert.equal(result[1], "foo.txt", "result[1] does not match expected result.");
              assert.equal(result[2], "foobar/", "result[2] does not match expected result.");
              assert.equal(result[3], "foobar/bar.txt", "result[3] does not match expected.");
              assert.equal(result[4], "foobar/foo.txt", "result[4] does not match expected.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);    

    it('ZipEnumerateEntriesCreateMode: should not enumerate archive entries in Create Mode.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipEnumerateEntriesCreateMode", []).then(
        (result) => 
        {
            try {
              done.fail("CreateMode: should fail in this case with - System.InvalidOperationException.")
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done(error)

      );
      
    }, DEFAULT_TIMEOUT);  
    
    it('ZipUpdateEmptyArchive: should allow update on empty stream.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:ZipUpdateEmptyArchive", []).then(
        (result) => 
        {
            try {
              assert.isTrue(result, "result does not match expected.");
              done()
            } catch (e) {
              done.fail(e);
            }
        },
        (error) => done.fail(error)

      );
      
    }, DEFAULT_TIMEOUT);    
    
    it('Compress: should compress string.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      var result = _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:Compress", ["Hello"]);
      try {
        assert.isString(result, "result does not match expected.");
        done()
      } catch (e) {
        done.fail(e);
      }
      
    }, DEFAULT_TIMEOUT);    

    it('CompressDecompress: should compress and decompress to equal string.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      var result = _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:Compress", ["Hello"]);
      result = _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:Decompress", [result]);
      try {
        assert.equal(result, "Hello", "result does not match expected.");
        done()
      } catch (e) {
        done.fail(e);
      }
      
    }, DEFAULT_TIMEOUT);    

    it('CompressDecompressLarge: should compress and decompress to equal large string.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      var lorum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum porttitor elit ac dui ullamcorper pellentesque. Etiam elementum vel dolor vitae luctus. Vestibulum fringilla varius cursus. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Donec tempus fringilla velit, sed imperdiet ipsum consequat id. Duis nec justo vel lacus convallis tristique. Mauris luctus erat vitae justo sagittis bibendum. Nullam justo justo, dictum porta augue ut, pretium malesuada velit.  Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Aliquam pretium, ex eu vestibulum egestas, nisi sapien semper eros, vitae facilisis elit nisi sed erat. Cras ultricies auctor tempor. Ut euismod magna ac tellus viverra varius. Phasellus ut lectus dapibus, sagittis erat vel, tristique lectus. Praesent dictum fermentum mauris eget consequat. Donec cursus mauris sagittis lectus dictum, a tincidunt dolor semper. Integer dapibus neque et elit volutpat.";
      var result = _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:Compress", [lorum]);
      result = _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:Decompress", [result]);
      try {
        assert.equal(result, lorum, "result does not match expected.");
        done()
      } catch (e) {
        done.fail(e);
      }
      
    }, DEFAULT_TIMEOUT); 

    it('CompressDecompressVeryLarge: should compress and decompress to equal very large string.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      var lorum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum porttitor elit ac dui ullamcorper pellentesque. Etiam elementum vel dolor vitae luctus. Vestibulum fringilla varius cursus. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Donec tempus fringilla velit, sed imperdiet ipsum consequat id. Duis nec justo vel lacus convallis tristique. Mauris luctus erat vitae justo sagittis bibendum. Nullam justo justo, dictum porta augue ut, pretium malesuada velit.  Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Aliquam pretium, ex eu vestibulum egestas, nisi sapien semper eros, vitae facilisis elit nisi sed erat. Cras ultricies auctor tempor. Ut euismod magna ac tellus viverra varius. Phasellus ut lectus dapibus, sagittis erat vel, tristique lectus. Praesent dictum fermentum mauris eget consequat. Donec cursus mauris sagittis lectus dictum, a tincidunt dolor semper. Integer dapibus neque et elit volutpat.";
      var result = _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:Compress", [lorum.repeat(1000)]);
      result = _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:Decompress", [result]);
      try {
        assert.equal(result, lorum.repeat(1000), "result does not match expected.");
        done()
      } catch (e) {
        done.fail(e);
      }
      
    }, DEFAULT_TIMEOUT);    

    it('CompressGZip: should compress string.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      var result = _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:CompressGZip", ["Hello GZip"]);
      try {
        assert.isString(result, "result does not match expected.");
        done()
      } catch (e) {
        done.fail(e);
      }
      
    }, DEFAULT_TIMEOUT);    

    it('CompressDecompressGZip: should compress and decompress to equal string.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      
      var result = _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:CompressGZip", ["Hello GZip"]);
      result = _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:DecompressGZip", [result]);
      try {
        assert.equal(result, "Hello GZip", "result does not match expected.");
        done()
      } catch (e) {
        done.fail(e);
      }
      
    }, DEFAULT_TIMEOUT);    

    it('CompressDecompressGZipLarge: should compress and decompress to equal large string.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      var lorum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum porttitor elit ac dui ullamcorper pellentesque. Etiam elementum vel dolor vitae luctus. Vestibulum fringilla varius cursus. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Donec tempus fringilla velit, sed imperdiet ipsum consequat id. Duis nec justo vel lacus convallis tristique. Mauris luctus erat vitae justo sagittis bibendum. Nullam justo justo, dictum porta augue ut, pretium malesuada velit.  Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Aliquam pretium, ex eu vestibulum egestas, nisi sapien semper eros, vitae facilisis elit nisi sed erat. Cras ultricies auctor tempor. Ut euismod magna ac tellus viverra varius. Phasellus ut lectus dapibus, sagittis erat vel, tristique lectus. Praesent dictum fermentum mauris eget consequat. Donec cursus mauris sagittis lectus dictum, a tincidunt dolor semper. Integer dapibus neque et elit volutpat.";
      var result = _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:CompressGZip", [lorum]);
      result = _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:DecompressGZip", [result]);
      try {
        assert.equal(result, lorum, "result does not match expected.");
        done()
      } catch (e) {
        done.fail(e);
      }
      
    }, DEFAULT_TIMEOUT); 

    it('CompressDecompressGZipVeryLarge: should compress and decompress to equal very large string.', (done) => {
      //karmaHTML.zipspec.document gives the access to the Document object of 'http-spec.html' file
      var _document = karmaHTML.zipspec.document;
      var lorum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum porttitor elit ac dui ullamcorper pellentesque. Etiam elementum vel dolor vitae luctus. Vestibulum fringilla varius cursus. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Donec tempus fringilla velit, sed imperdiet ipsum consequat id. Duis nec justo vel lacus convallis tristique. Mauris luctus erat vitae justo sagittis bibendum. Nullam justo justo, dictum porta augue ut, pretium malesuada velit.  Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Aliquam pretium, ex eu vestibulum egestas, nisi sapien semper eros, vitae facilisis elit nisi sed erat. Cras ultricies auctor tempor. Ut euismod magna ac tellus viverra varius. Phasellus ut lectus dapibus, sagittis erat vel, tristique lectus. Praesent dictum fermentum mauris eget consequat. Donec cursus mauris sagittis lectus dictum, a tincidunt dolor semper. Integer dapibus neque et elit volutpat.";
      var result = _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:CompressGZip", [lorum.repeat(1000)]);
      result = _document.Module.BINDING.call_static_method("[ZipTestSuite]TestSuite.Program:DecompressGZip", [result]);
      try {
        assert.equal(result, lorum.repeat(1000), "result does not match expected.");
        done()
      } catch (e) {
        done.fail(e);
      }
      
    }, DEFAULT_TIMEOUT);    
    
  });
