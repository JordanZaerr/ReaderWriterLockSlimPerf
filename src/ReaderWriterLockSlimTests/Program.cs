using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReaderWriterLockSlimTests
{
    class Program
    {
        static void Main()
        {
            var locks = new ILockingImplementation[] { new RWSlimLock(), new MonitorLock(), new StraightLock()};

            foreach (var impl in locks)
            {
                //This blocks
                Console.WriteLine(RunBenchmark(impl).Result);
                Console.WriteLine();
            }
            Console.WriteLine("Done");
            Console.ReadLine();
        }

        static async Task<BenchmarkResult> RunBenchmark(ILockingImplementation impl)
        {
            var tasks = Enumerable.Range(0, 1000)
                .Select(_ => new Task<long>(() =>
                {
                    var sw = Stopwatch.StartNew();
                    impl.AquireLock();
                    impl.ExecuteCode(() => Thread.Sleep(10));
                    impl.ReleaseLock();
                    sw.Stop();
                    return sw.ElapsedMilliseconds;
                }))
                .ToList();

            var swTotal = Stopwatch.StartNew();

            tasks.ForEach(t => t.Start());

            await Task.WhenAll(tasks);
            swTotal.Stop();

            var results = tasks.Select(t => t.Result).ToList();

            return new BenchmarkResult(impl.Name)
            {
                TotalExecutionTime = swTotal.ElapsedMilliseconds,
                TotalExecutionTimeSum = results.Sum(),
                MinExecutionTime = results.Min(),
                MaxExecutionTime = results.Max()
            };
        }
    }

    public class BenchmarkResult
    {
        private readonly string _name;

        public BenchmarkResult(string name)
        {
            _name = name;
        }
        public double TotalExecutionTime { get; set; }
        public double TotalExecutionTimeSum { get; set; }
        public double MinExecutionTime { get; set; }
        public double MaxExecutionTime { get; set; }
        public override string ToString()
        {
            return $"Locking method: {_name}{Environment.NewLine}"
                   + $"\"Real\" total execution time: {TotalExecutionTime:n0}ms{Environment.NewLine}"
                   + $"Summed execution time of each thread: {TotalExecutionTimeSum:n0}ms{Environment.NewLine}"
                   + $"Min execution time: {MinExecutionTime:n0}ms{Environment.NewLine}"
                   + $"Max execution time: {MaxExecutionTime:n0}ms";
        }
    }

    public class RWSlimLock : ILockingImplementation
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        public void AquireLock()
        {
            _lock.EnterReadLock();
        }

        public void ExecuteCode(Action action)
        {
            action();
        }

        public void ReleaseLock()
        {
            _lock.ExitReadLock();
        }

        public string Name { get; } = "ReaderWriterLockSlim";
    }

    public class MonitorLock : ILockingImplementation
    {
        private readonly object _lock = new object();

        public void AquireLock()
        {
            Monitor.Enter(_lock);
        }

        public void ExecuteCode(Action action)
        {
            action();
        }

        public void ReleaseLock()
        {
            Monitor.Exit(_lock);
        }
        public string Name { get; } = "Monitor";
    }

    public class StraightLock : ILockingImplementation
    {
        private readonly object _lock = new object();
        public void AquireLock()
        {
            //Nothing to do here
        }

        public void ExecuteCode(Action action)
        {
            lock (_lock)
            {
                action();
            }
        }

        public void ReleaseLock()
        {
            //Nothing to do here
        }

        public string Name { get; } = "Lock keyword";
    }

    public interface ILockingImplementation
    {
        string Name { get; }
        void AquireLock();

        void ExecuteCode(Action action);

        void ReleaseLock();
    }
}
