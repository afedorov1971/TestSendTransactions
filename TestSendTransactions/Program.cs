using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TestSendTransactions
{
	internal class Program
	{
		private enum Option
		{
			Exit = 0,
			Run = 1,
			SelectType = 2,
			SelectIterationsNumber = 3,
			SelectCallsPerSecond = 4
		}
		private static Option SelectOption(RunOptions runOptions)
		{
			while (true)
			{
				Console.Clear();
				Console.WriteLine("Choose an option:");
				Console.WriteLine($"1) Select what to send({runOptions.SendTypeMessage})");
				Console.WriteLine($"2) Set number of iterations({runOptions.NumberOfIterations})");
				Console.WriteLine($"3) Set calls per second({runOptions.CallsPerSecond})");
				Console.WriteLine("4) Run");
				Console.WriteLine("5) Exit");
				Console.WriteLine();
				Console.Write("Select an option: ");

				Option? choice = Console.ReadLine() switch
				{
					"1" => Option.SelectType,
					"2" => Option.SelectIterationsNumber,
					"3" => Option.SelectCallsPerSecond,
					"4" => Option.Run,
					"5" => Option.Exit,
					_ => null
				};

				if (choice.HasValue)
				{
					return choice.Value;
				}
			}
		}

		private static bool SelectSendType(bool currentValue)
		{
			Console.Clear();
			Console.WriteLine("Choose an option:");
			Console.WriteLine("1) Send transactions");
			Console.WriteLine("2) Send multi search transactions");
			Console.WriteLine("3) Back to main menu");
			Console.WriteLine();
			Console.Write("Select an option: ");

			return Console.ReadLine() switch
			{
				"1" => true,
				"2" => false,
				_ => currentValue
			};
		}


		private static int SelectNumberOfIterations(int currentValue)
		{
			while (true)
			{
				Console.Clear();
				Console.Write($"Number of iterations[1..1000000]({currentValue}):");
				var result = Console.ReadLine();
				if (int.TryParse(result, out var aValue) && aValue is > 0 and <= 1000000)
				{
					return aValue;
				}
			}
		}

		private static int SelectCallsPerSecond(int currentValue)
		{
			while (true)
			{
				Console.Clear();
				Console.Write($"Calls per second[1..1000]({currentValue}):");
				var result = Console.ReadLine();
				if (int.TryParse(result, out var aValue) && aValue is > 0 and <= 1000)
				{
					return aValue;
				}
			}
		}

		private class RunOptions
		{
			public string Endpoint => SendTransactions ? "transaction" : "transactions";

			public string SendTypeMessage => SendTransactions ? "transactions" : "multi search transactions";

			public int NumberOfIterations { get; set; } = 50;
			public int CallsPerSecond { get; set; } = 200;

			public bool SendTransactions { get; set; } = true;
		}

		private static bool CanRun(RunOptions options)
		{
			do
			{
				var choice = SelectOption(options);
				switch (choice)
				{
					case Option.SelectType:
						options.SendTransactions = SelectSendType(options.SendTransactions);
						break;
					case Option.SelectIterationsNumber:
						options.NumberOfIterations = SelectNumberOfIterations(options.NumberOfIterations);
						break;
					case Option.SelectCallsPerSecond:
						options.CallsPerSecond = SelectCallsPerSecond(options.CallsPerSecond);
						break;
					case Option.Run:
						return true;
					case Option.Exit:
						return false;
				}
			} while (true);
		}

		static void Main(string[] args)
		{
			var defaultUrl = "http://localhost/TestWebService/";

			var httpClient = new HttpClient
			{
				BaseAddress = new Uri(defaultUrl)
			};

			var options = new RunOptions();

			while (CanRun(options))
			{

				var endpoint = options.Endpoint;

				for (var j = 0; j < options.NumberOfIterations; j++)
				{
					var watch = new System.Diagnostics.Stopwatch();

					watch.Start();

					Parallel.For(0, options.CallsPerSecond, (_,_) =>
					{
						var res = httpClient.GetStringAsync(endpoint).Result;
						Console.WriteLine(res);
					});
					
					watch.Stop();

					var elapsedMilliseconds = watch.ElapsedMilliseconds;

					Console.WriteLine($"Execution Time: {elapsedMilliseconds} msec");

					if (elapsedMilliseconds < 1000)
					{
						Thread.Sleep(TimeSpan.FromMilliseconds(1000 - elapsedMilliseconds));
					}
				}
			}
		}
	}
}
