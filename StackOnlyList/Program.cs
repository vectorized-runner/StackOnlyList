using System;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

namespace StackOnlyList
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			
			// Don't care if NUnit is optimized or not
			var config = new ManualConfig()
			             .WithOptions(ConfigOptions.DisableOptimizationsValidator)
			             // .AddValidator(JitOptimizationsValidator.DontFailOnError)
			             .AddLogger(ConsoleLogger.Default)
			             .AddExporter(RPlotExporter.Default, CsvExporter.Default)
			             .AddColumnProvider(DefaultColumnProviders.Instance);

			var summary = BenchmarkRunner.Run<Benchmarks_VersusList>(config);
		}
	}
}