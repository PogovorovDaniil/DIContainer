using DIContainer;
using System;

namespace DIContainerTest
{
    public class Counter
    {
        public int counter;
        public Counter() 
        {
            counter = 1;
        }
        public int GetNextNumber() => counter++;
    }
    public interface ILogger
    {
        void Log(string message);
    }
    public class Logger : ILogger
    {
        private Counter counter;
        public Logger(Counter counter)
        {
            this.counter = counter;
        }
        public void Log(string message)
        {
            Console.WriteLine(counter.GetNextNumber() + "> " + message);
        }
    }

    public class Logger2 : ILogger
    {
        private Counter counter;
        public Logger2(Counter counter)
        {
            this.counter = counter;
        }
        public void Log(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(counter.GetNextNumber() + "> " + message);
            Console.ResetColor();
        }
    }

    public class MyProgram
    {
        private ILogger[] loggers { get; set; }
        public MyProgram(ILogger[] loggers) 
        {
            this.loggers = loggers;
        }

        public void Run()
        {
            foreach(var logger in loggers)
            {
                logger.Log("Hello");
                logger.Log("World");
            }
        }
    }

    internal class Program
    {
        static void Main(string[] _)
        {
            Container container = new Container();
            container.AddSingleton<Counter>();
            container.AddTransient<ILogger, Logger>();
            container.AddTransient<ILogger, Logger2>();
            container.AddEntryPoint<MyProgram>();
            container.Run();
        }
    }
}