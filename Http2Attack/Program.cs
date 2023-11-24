using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Collections.Generic;

namespace Http2Attack
{
    class Program
    {
        static int numRequests;
        static int concurrencyLimit;
        static string serverURLStr;
        static uint streamCounter;
        static int waitTime;
        static int delayTime;
        static int sentHeaders, sentRSTs, recvFrames;
        static DateTime headerStart, headerEnd;
        static object lockObject = new object();
        static async Task Main(string[] args)
        {
            // something here
        }
    }
}