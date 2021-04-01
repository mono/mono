using System;
using System.Diagnostics;
using System.Linq;

class Program {
    static void Main () {
        const int iterations = Benchmark.IterationCount, 
            warming_iterations = Benchmark.WarmingIterationCount;
    
        WakeUp ();

        Console.WriteLine ($">>> Warming with {warming_iterations} iterations...");

        var b = new Benchmark ();
        var timings = new TimeSpan[iterations];

        for (var i = 0; i < warming_iterations; i++)
            b.runIteration ();

        b.reset ();

        Console.WriteLine ($">>> Running {iterations} iterations...");

        var sw = new Stopwatch ();
        for (int i = 0; i < iterations; i++) {
            sw.Restart ();
            b.Step ();
            timings[i] = sw.Elapsed;
        }

        Console.WriteLine ($">>> Elapsed {timings.Sum (t => t.TotalMilliseconds)}ms");
        Console.WriteLine ($">>> ms/iter avg = {timings.Average (t => t.TotalMilliseconds)}");
        Console.WriteLine ($">>> min = {timings.Min (t => t.TotalMilliseconds)}");
        Console.WriteLine ($">>> max = {timings.Max (t => t.TotalMilliseconds)}");

        if (Debugger.IsAttached) {
            Console.WriteLine (">>> Press enter to exit.");
            Console.ReadLine ();
        }
    }

    static void WakeUp () {        
    }
}

public partial class Benchmark {
    public void Step () {
        for (int i = 0; i < InnerIterationCount; i++)
            runIteration ();
    }
}