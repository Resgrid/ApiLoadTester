using System;
using Resgrid.ApiLoadTester.Models;
using RestSharp;

namespace Resgrid.ApiLoadTester.Helpers
{
	public static class RequestTypeConverter
	{
		public static Method ConvertToMethod(RequestTypes type)
		{
			switch (type)
			{
				case RequestTypes.Get:
					return Method.GET;
				case RequestTypes.Post:
					return Method.POST;
				default:
					throw new ArgumentOutOfRangeException("type");
			}
		}
	}
}