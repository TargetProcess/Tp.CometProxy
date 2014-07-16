// 
// Copyright (c) 2005-2011 TargetProcess. All rights reserved.
// TargetProcess proprietary/confidential. Use is subject to license terms. Redistribution of this file is strictly forbidden.
// 

using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Client;
using log4net;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace Tp.CometProxy
{
	public class Connection : IDisposable
	{
		private readonly string _url;
		private readonly string _login;
		private readonly string _password;
		private readonly HubConnection _connection;
		private readonly IDictionary<string, Proxy> _proxies;
		private readonly ILog _log;
		public Connection(string url, string login, string password)
		{
			_url = url;
			_login = login;
			_password = password;
			_connection = new HubConnection(_url + "notifications");
			_proxies = new Dictionary<string, Proxy>();
			_proxies["slice"] = new Proxy(_connection.CreateHubProxy("slice"));
			_proxies["resource"] = new Proxy(_connection.CreateHubProxy("resource"));
			_log = LogManager.GetLogger("CometProxy");
		}

		public void Start()
		{
			var client = new CookieAwareWebClient();
			client.Login(_url + "/login.aspx", _login, _password);
			_connection.CookieContainer = client.CookieContainer;
			try
			{
				_connection.Start().Wait();
				_log.InfoFormat("{0} - connection started", _url);
				Console.WriteLine("{0} - connection started", _url);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}
		}

		public Proxy Get(string hub)
		{
			return _proxies[hub];
		}

		public void Dispose()
		{
			_log.InfoFormat("Closing connection: {0}", _url);
			if (_connection.State == ConnectionState.Connected)
			{
				foreach (var proxy in _proxies.Values)
				{
					proxy.Dispose();
				}
				_connection.Stop();
			}
			_log.Info("Connection closed");
		}
	}
}