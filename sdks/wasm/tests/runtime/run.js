// -*- mode: js; js-indent-level: 4; -*-
//
// Run runtime test suites in chrome using puppeteer
//

const puppeteer = require('puppeteer');
var fs = require("fs");
var http = require('http');
var finalhandler = require('finalhandler');
var serveStatic = require('serve-static');

if (process.argv.length < 4) {
	console.log ("Usage: run.js <wasm dir> <arguments to runtime-tests.js>.");
	process.exit ()
}

var serving_dir = process.argv [2];
var args = []
for (var i = 3; i < process.argv.length; ++i) {
	args.push (process.argv [i]);
}
var port = 8088;

// Start a server as wasm cannot work with file:// urls.
var serve = serveStatic(serving_dir, {
	setHeaders: function(res, path, stat) {
		if (path.endsWith ('.wasm'))
			res.setHeader ('Content-Type', 'application/wasm');
  }
});

var server = http.createServer(function(req, res) {
	var done = finalhandler(req, res);
	serve(req, res, done);
});
server.listen(port);

// Pass arguments to the child using a query string, i.e. arg=arg1&arg=arg2
uri = "http://localhost:" + port;
for (var i = 0; i < args.length; ++i) {
	if (i > 0)
		uri += "&";
	else
		uri += "?";
	uri += "arg=" + encodeURIComponent (args [i]);
}
console.log ("URI: " + uri);

// Run the app through chrome
(async () => {
	const browser = await puppeteer.launch();
	const page = await browser.newPage();
	page.on('console', function (msg) {
		// runtime-tests.js emits this on exit
		if (msg.text ().startsWith ("WASM EXIT")) {
			code = parseInt (msg.text ().substring ("WASM EXIT".length));
			browser.disconnect ();
			process.exit (code);
		}
		console.log('LOG:', msg.text());
	});
	page.on("pageerror", function(err) {  
		s = err.toString();
		console.log("Page error: " + s);
		browser.disconnect ();
		process.exit (1);
	});
	await page.goto (encodeURI (uri));
	await page.waitFor(1000000);
	await browser.close();
	server.close ();
	process.exit (1);
})();


