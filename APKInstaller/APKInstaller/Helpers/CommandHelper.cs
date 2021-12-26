using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace APKInstaller.Helpers
{
    internal class CommandHelper
    {
        /// <summary>
        /// Executes a shell command on the remote device
        /// </summary>
        /// </param>
        /// <param name="command">The command to execute</param>
        /// <param name="rcvr">The shell output receiver</param>
        public static void ExecuteShellCommand(string command)
        {
            try
            {
                ExecuteShellCommandAsync(command, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1)
                {
                    throw ex.InnerException;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public static async Task ExecuteShellCommandAsync(string command, CancellationToken cancellationToken)
        {
            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                Arguments = command,
                CreateNoWindow = true
            };

            using Process process = Process.Start(start);

            process.EnableRaisingEvents = true;

            try
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    // Previously, we would loop while reader.Peek() >= 0. Turns out that this would
                    // break too soon in certain cases (about every 10 loops, so it appears to be a timing
                    // issue). Checking for reader.ReadLine() to return null appears to be much more robust
                    // -- one of the integration test fetches output 1000 times and found no truncations.
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        string line = await reader.ReadLineAsync().ConfigureAwait(false);

                        if (line == null)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}
