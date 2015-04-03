using System;
using System.Collections.Generic;
using System.Linq;

namespace Resgrid.ApiLoadTester.Helpers
{
	public static class HeaderConverter
	{
		public static List<Tuple<string, string>> ConvertHeaderItems(string header)
		{
			if (String.IsNullOrWhiteSpace(header))
				return null;

			var headers = header.Split(char.Parse(","));

			if (headers != null && headers.Any())
				return headers.Select(item => item.Split(char.Parse("|"))).Select(elements => new Tuple<string, string>(elements[0], elements[1])).ToList();

			return null;
		}
	}
}