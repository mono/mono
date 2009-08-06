/*
 Copyright (c) 2003-2006 Niels Kokholm and Peter Sestoft
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

// C5 example: job queue 2004-11-22

// Compile with 
//   csc /r:C5.dll Anagrams.cs 

using System;
using C5;
using SCG = System.Collections.Generic;

class MyJobQueueTest {
  public static void Main(String[] args) {
    JobQueue jq = new JobQueue();
    // One user submits three jobs at time=27
    Rid rid1 = jq.Submit(new Ip("62.150.83.11"), 27),
        rid2 = jq.Submit(new Ip("62.150.83.11"), 27),
        rid3 = jq.Submit(new Ip("62.150.83.11"), 27);
    // One job is executed
    jq.ExecuteOne();
    // Another user submits two jobs at time=55
    Rid rid4 = jq.Submit(new Ip("130.225.17.5"), 55),
        rid5 = jq.Submit(new Ip("130.225.17.5"), 55);
    // One more job is executed
    jq.ExecuteOne();
    // The first user tries to cancel his first and last job
    jq.Cancel(rid1);
    jq.Cancel(rid3);
    // The remaining jobs are executed
    while (jq.ExecuteOne() != null) { } 
  }
}

class JobQueue {
  private readonly IPriorityQueue<Job> jobQueue;
  private readonly IDictionary<Rid,IPriorityQueueHandle<Job>> jobs;
  private readonly HashBag<Ip> userJobs;

  public JobQueue() {
    this.jobQueue = new IntervalHeap<Job>();
    this.jobs = new HashDictionary<Rid,IPriorityQueueHandle<Job>>();
    this.userJobs = new HashBag<Ip>();
  }

  public Rid Submit(Ip ip, int time) {
    int jobCount = userJobs.ContainsCount(ip);
    Rid rid = new Rid();
    Job job = new Job(rid, ip, time + 60 * jobCount);
    IPriorityQueueHandle<Job> h = null;
    jobQueue.Add(ref h, job);
    userJobs.Add(ip);
    jobs.Add(rid, h);
    Console.WriteLine("Submitted {0}", job);    
    return rid;
  }

  public Job ExecuteOne() {
    if (!jobQueue.IsEmpty) {
      Job job = jobQueue.DeleteMin();
      userJobs.Remove(job.ip);
      jobs.Remove(job.rid);
      Console.WriteLine("Executed {0}", job);    
      return job;
    } else
      return null;
  }

  public void Cancel(Rid rid) {
    IPriorityQueueHandle<Job> h;
    if (jobs.Remove(rid, out h)) {
      Job job = jobQueue.Delete(h);
      userJobs.Remove(job.ip);
      Console.WriteLine("Cancelled {0}", job);
    }
  }
}

class Job : IComparable<Job> {
  public readonly Rid rid;
  public readonly Ip ip;
  public readonly int time;

  public Job(Rid rid, Ip ip, int time) {
    this.rid = rid;
    this.ip = ip;
    this.time = time;
  }

  public int CompareTo(Job that) {
    return this.time - that.time;
  }

  public override String ToString() {
    return rid.ToString();
  }
}

class Rid { 
  private readonly int ridNumber;
  private static int nextRid = 1;
  public Rid() {
    ridNumber = nextRid++;
  }
  public override String ToString() {
    return "rid=" + ridNumber;
  }
}

class Ip { 
  public readonly String ipString;

  public Ip(String ipString) {
    this.ipString = ipString;
  }

  public override int GetHashCode() {
    return ipString.GetHashCode();
  }

  public override bool Equals(Object that) {
    return 
      that != null 
      && that is Ip 
      && this.ipString.Equals(((Ip)that).ipString);
  }
}
 

