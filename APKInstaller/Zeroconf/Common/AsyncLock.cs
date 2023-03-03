using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Zeroconf.Common
{
    internal class AsyncLock
    {
        /// <summary>
        /// The logger to use when logging messages.
        /// </summary>
        private readonly ILogger<AsyncLock> logger;

        private readonly SemaphoreSlim m_semaphore;
        private readonly Task<Releaser> m_releaser;

        public AsyncLock(ILogger<AsyncLock> logger = null)
        {
            m_semaphore = new SemaphoreSlim(1);
            m_releaser = Task.FromResult(new Releaser(this));
            this.logger = logger ?? NullLogger<AsyncLock>.Instance;
        }

        public Task<Releaser> LockAsync([CallerMemberName] string callingMethod = null, [CallerFilePath] string path = null, [CallerLineNumber] int line = 0)
        {
            logger.LogDebug($"AsyncLock.LockAsync called by: {callingMethod} in file: {path} : {line}");

            Task wait = m_semaphore.WaitAsync();

            return wait.IsCompleted ?
                m_releaser :
                wait.ContinueWith((_, state) => new Releaser((AsyncLock)state),
                    this, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public readonly struct Releaser : IDisposable
        {
            private readonly AsyncLock m_toRelease;

            internal Releaser(AsyncLock toRelease) { m_toRelease = toRelease; }

            public void Dispose()
            {
                m_toRelease?.m_semaphore.Release();
            }
        }
    }
}
