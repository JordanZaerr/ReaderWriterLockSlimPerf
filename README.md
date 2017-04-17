# ReaderWriterLockSlimPerf
Quick spike to test multi-threaded locking perf in response to [this article](https://blogs.msdn.microsoft.com/pedram/2007/10/07/a-performance-comparison-of-readerwriterlockslim-with-readerwriterlock/) that someone sent me.

Spawns 1000 tasks that aquire a lock, sleep 10ms, and then release the lock.

This only tests the read lock of the `ReaderWriterLockSlim`.

![Perf Chart](ReaderWriterLock.jpg)

Note: I am aware that the lock keyword compiles to basically the following.
```
bool lockTaken = false;
try
{
  Monitor.Enter(obj, ref lockTaken);
  action();
}
finally
{
  if (lockTaken)
    Monitor.Exit(obj);
}
```
