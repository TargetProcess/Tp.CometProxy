using System;
using System.Threading;

namespace Tp.CometProxy.Example
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			const string url = "https://xxx.tpondemand.com/";
			const string login = "user";
			const string password = "passwrod";

			var subscriptionData = new {ResourceType = "userstory", Filter = "?Id is 83213"};

			var subscription = new Subscription("some subscription", subscriptionData)
			{
				ClientId = "resource/test"
			};

			using (var connection = new Connection(url, login, password))
			{
				connection.Start();
				using(Proxy hub = connection.Get("resource"))
				{

					hub.Subscribe(subscription, result => Console.WriteLine(result));

					hub.Wait("some subscription", 10, 60*1000);
				}
			}
		}
	}
}