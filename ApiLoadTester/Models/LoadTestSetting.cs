namespace Resgrid.ApiLoadTester.Models
{
	public class LoadTestSetting
	{
		public RequestTypes RequestType { get; set; }
		public string BaseUrl { get; set; }
		public string ActionPath { get; set; }
		public string FilePath { get; set; }
		public string Headers { get; set; }
		public int Threads { get; set; }
		public int Iterations { get; set; }
	}
}