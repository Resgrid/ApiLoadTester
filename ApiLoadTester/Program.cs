using System;
using Fclp;
using Resgrid.ApiLoadTester.Models;
using Resgrid.ApiLoadTester.Workers;
using Rug.Cmd;
using Rug.Cmd.Colors;

namespace Resgrid.ApiLoadTester
{
	class Program
	{
		static void Main(string[] args)
		{
			RC.Theme = ConsoleColorTheme.Load(ConsoleColorDefaultThemes.Colorful);

			CmdHelper.WriteSimpleBanner("Resgrid API Load Tester", ' ', RC.Theme[ConsoleThemeColor.AppBackground], RC.Theme[ConsoleThemeColor.TitleText]);
			RC.WriteLine();

			LoadTestSetting settings = new LoadTestSetting();
			var p = new FluentCommandLineParser();

			p.Setup<int>('r', "requestType").WithDescription("The type of HTTP Request. 1 = GET, 2 = POST").Required()
			 .Callback(type => settings.RequestType = (RequestTypes)type);

			p.Setup<string>('u', "baseUrl").WithDescription("The base url of the API being testing i.e. http://resgrid.com").Required()
			 .Callback(url => settings.BaseUrl = url);

			p.Setup<string>('a', "actionPath").WithDescription("The the path, off the base url, i.e. /customers/get/1").Required()
			 .Callback(act => settings.ActionPath = act);

			p.Setup<string>('f', "file").WithDescription("The the path, off the base url, i.e. /customers/get/1")
			 .Callback(file => settings.FilePath = file);

			p.Setup<string>('h', "headers").WithDescription("Comman seperated list of headers with Key/Value elements seperated by a pipe. Username|myUsername,Password|myPassword")
			 .Callback(headers => settings.Headers = headers);

			p.Setup<int>('t', "threads").WithDescription("The number of threads you want to run rquests with").SetDefault(1)
			 .Callback(threads => settings.Threads = threads);

			p.Setup<int>('i', "iterations").WithDescription("The number of times you want each thread to run").SetDefault(10)
			 .Callback(iterations => settings.Iterations = iterations);

			var result = p.Parse(args);

			if (settings.RequestType == RequestTypes.Post && String.IsNullOrWhiteSpace(settings.FilePath))
				RC.WriteLine(ConsoleColorExt.Red, "You must specify a JSON file to post with the POST request type");
			else
				if (!result.HasErrors)
					LoadWorker.RunLoadTest(settings);
				else
						RC.WriteLine(ConsoleColorExt.Red, result.ErrorText);


#if (DEBUG)
			RC.WriteLine(ConsoleColorExt.White, "Press Enter to Exit...");
			Console.ReadLine();
#endif
		}
	}
}
