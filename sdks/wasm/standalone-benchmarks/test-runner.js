if (process.argv.length != 4) {
  console.log("usage: node test-runner.js assemblyName packagerOutputDirectory");
  process.exit(1);
}

var root = process.argv[3];
var assemblyName = process.argv[2];

var vm = require("vm");
var fs = require("fs");
var path = require("path");

var context = Object.create(null);

// Necessary because dotnet.js decides we're v8/sm shell without it
context.process = process;

// dotnet.js assumes __dirname and similar globals will be available but they aren't
//  unless you're the top-level script being run from the command line
context.require = require;
context.console = console;
context.setTimeout = setTimeout;
context.setInterval = setInterval;
context.clearInterval = clearInterval;

context.App = {
  init: function () {
    console.log("Priming interpreter...");
    var wu = context.Module.mono_bind_static_method ("[" + assemblyName + "] Program:WakeUp");
    wu();
    console.log("Sleeping to allow time for tiered JIT...");
    setTimeout(function () {
      var f = context.Module.cwrap ('mono_wasm_enable_on_demand_gc', 'void', []);
      f ();
      console.log("Running benchmark...");
      context.Module.mono_call_assembly_entry_point (assemblyName, []);
      console.log("Benchmark complete.");
    }, 5000);
  }
};
context.global = context;

var contextOptions = {};
vm.createContext(context, contextOptions);

function loadAndRun (filename) {
  var virtualFilename, virtualDirname, absoluteRoot;
  if ((root[0] === "/") || (root[1] === ":"))
    absoluteRoot = root;
  else
    absoluteRoot = path.normalize(path.join(process.cwd(), root));

  virtualFilename = path.join(absoluteRoot, filename);
  virtualDirname = path.dirname(virtualFilename);

  var runOptions = {};
  var text = fs.readFileSync(virtualFilename);
  console.log("Compiling " + filename);
  var script = new vm.Script(text, virtualFilename);
  console.log("Running " + filename);

  context.__filename = virtualFilename;
  context.__dirname = virtualDirname;

  script.runInContext(context, runOptions);
};

loadAndRun("mono-config.js");
loadAndRun("runtime.js");
loadAndRun("dotnet.js");