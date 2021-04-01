const puppeteer = require('puppeteer');
var fs = require("fs");
var http = require('http');

var finalhandler = require('finalhandler');
var serveStatic = require('serve-static');

if (process.argv.length == 3) {
	console.log ("Usage: gen-profile.js <output file> <dir>.");
	process.exit ()
}
var output_file = process.argv [2];
var serving_dir = process.argv [3];
var port = 8088;

// Start a server as wasm cannot work with file:// urls.
var serve = serveStatic(serving_dir);

var server = http.createServer(function(req, res) {
  var done = finalhandler(req, res);
  serve(req, res, done);
});
server.listen(port);

// Run the app through chrome
(async () => {
	const browser = await puppeteer.launch();
	const page = await browser.newPage();
	page.on('console', msg => console.log('LOG:', msg.text()));
	await page.goto ('http://localhost:' + port);
	await page.waitFor(1000);
	const data = await page.mainFrame ().evaluate('Module.coverage_profile_data');
	fs.writeFile (output_file, Buffer.from (data), function(err) {
		if (err)
			console.log ("ERR: " + err);
	});

	await browser.close();
	server.close ();
})();


