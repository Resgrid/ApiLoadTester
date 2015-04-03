using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Resgrid.ApiLoadTester.Helpers;
using Resgrid.ApiLoadTester.Models;
using RestSharp;
using Rug.Cmd;

namespace Resgrid.ApiLoadTester.Workers
{
	public class LoadWorker
	{
		private static long _minRequestTime = -1;
		private static long _maxRequestTime = -1;
		private static long _averageTime = 0;
		private static int _successes = 0;
		private static int _failures = 0;
		private static List<Task> _workers = new List<Task>(); 

		public static void RunLoadTest(LoadTestSetting setting)
		{

			for (int i = 0; i < setting.Threads; i++)
			{
				var id = i + 1;
				Task task = new Task(() => LoadTestWorker(setting, id));
				_workers.Add(task);
				task.Start();
			}

			Stopwatch masterSW = new Stopwatch();
			masterSW.Start();

			Task.WaitAll(_workers.ToArray());

			masterSW.Stop();
			Thread.Sleep(250);

			RC.WriteLine(ConsoleColorExt.Yellow, "==========================================");
			RC.WriteLine(ConsoleColorExt.Yellow, "=               RUN REPORT               =");
			RC.WriteLine(ConsoleColorExt.Yellow, "==========================================");
			RC.WriteLine(ConsoleColorExt.Yellow, string.Format("Successful Operations: {0}", _successes));
			RC.WriteLine(ConsoleColorExt.Yellow, string.Format("Failed Operations: {0}", _failures));
			RC.WriteLine(ConsoleColorExt.Yellow, string.Format("Quickest Operation: {0}", _minRequestTime));
			RC.WriteLine(ConsoleColorExt.Yellow, string.Format("Slowest Operation: {0}", _maxRequestTime));
			RC.WriteLine(ConsoleColorExt.Yellow, string.Format("Average Operation: {0}", (_averageTime / (setting.Threads * setting.Iterations))));

			RC.WriteLine(ConsoleColorExt.Yellow, string.Format("Total Operation Time: {0}ms/{1}m", masterSW.ElapsedMilliseconds, TimeSpan.FromMilliseconds(masterSW.ElapsedMilliseconds).TotalMinutes.ToString("F")));


			RC.WriteLine(ConsoleColorExt.Black, string.Format("##teamcity[SuccessfulApiCalls '{0}']", _successes));
			RC.WriteLine(ConsoleColorExt.Black, string.Format("##teamcity[FailedApiCalls '{0}']", _failures));
			RC.WriteLine(ConsoleColorExt.Black, string.Format("##teamcity[QuickestApiCall '{0}']", _minRequestTime));
			RC.WriteLine(ConsoleColorExt.Black, string.Format("##teamcity[SlowestApiCall '{0}']", _maxRequestTime));
			RC.WriteLine(ConsoleColorExt.Black, string.Format("##teamcity[AverageApiCall '{0}']", (_averageTime / (setting.Threads * setting.Iterations))));
			RC.WriteLine(ConsoleColorExt.Black, string.Format("##teamcity[TotalTime '{0}']", masterSW.ElapsedMilliseconds));
		}

		private static void LoadTestWorker(LoadTestSetting setting, int threadId)
		{
			RC.WriteLine(ConsoleVerbosity.Normal, ConsoleColorExt.Gray, string.Format("Thread {0}: Starting Worker", threadId));

			int count = 0;

			while (count < setting.Iterations)
			{
				count++;

				try
				{
					var client = new RestClient(setting.BaseUrl);
					var request = new RestRequest(setting.ActionPath, RequestTypeConverter.ConvertToMethod(setting.RequestType));

					var headers = HeaderConverter.ConvertHeaderItems(setting.Headers);
					
					if (headers != null && headers.Any())
					{
						foreach (var header in headers)
						{
							request.AddHeader(header.Item1, header.Item2);
						}
					}

					if (setting.RequestType == RequestTypes.Post)
					{
						string data = System.IO.File.ReadAllText(setting.FilePath);

						if (!String.IsNullOrWhiteSpace(data))
							request.AddBody(data);
					}

					Stopwatch sw = new Stopwatch();

					sw.Start();
					var response = client.Execute(request);
					sw.Stop();

					if (_minRequestTime == -1 || sw.ElapsedMilliseconds < _minRequestTime)
						_minRequestTime = sw.ElapsedMilliseconds;

					if (_minRequestTime == -1 || sw.ElapsedMilliseconds > _maxRequestTime)
						_maxRequestTime = sw.ElapsedMilliseconds;

					_averageTime += sw.ElapsedMilliseconds;

					if (String.IsNullOrWhiteSpace(response.ErrorMessage) && response.Content != null)
					{
						_successes++;
						RC.WriteLine(ConsoleVerbosity.Normal, ConsoleColorExt.Green,
							string.Format("Thread {0}: Request successfully completed in {1}ms ({2}/{3})", threadId, sw.ElapsedMilliseconds,
								count, setting.Iterations));
					}
					else
					{
						_failures++;
						RC.WriteLine(ConsoleVerbosity.Normal, ConsoleColorExt.Red,
							string.Format("Thread {0}: Request unsuccessfully completed in {1}ms ({2}/{3})", threadId, sw.ElapsedMilliseconds,
								count, setting.Iterations));
					}

				}
				catch (Exception ex)
				{
					RC.WriteException(0, "Exception in Worker", ex);
					_failures++;
				}
			}

			RC.WriteLine(ConsoleVerbosity.Normal, ConsoleColorExt.Gray, string.Format("Thread {0}: Finished {1} Iterations", threadId, count));
		}
	}
}
