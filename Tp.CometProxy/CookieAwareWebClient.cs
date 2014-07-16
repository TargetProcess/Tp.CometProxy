// 
// Copyright (c) 2005-2011 TargetProcess. All rights reserved.
// TargetProcess proprietary/confidential. Use is subject to license terms. Redistribution of this file is strictly forbidden.
// 

using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading;

namespace Tp.CometProxy
{
	internal class CookieAwareWebClient : WebClient
	{
		private CookieContainer _container = new CookieContainer();

		public CookieContainer CookieContainer
		{
			get { return _container; }
			set { _container = value; }
		}

		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest request = base.GetWebRequest(address);
			request.Timeout = Timeout.Infinite;
			var httpWebRequest = request as HttpWebRequest;
			if (httpWebRequest != null)
			{
				httpWebRequest.CookieContainer = CookieContainer;
			}
			return request;
		}

		public void Login(string url, string login, string password)
		{
			UploadValues(url, "POST",
			             new NameValueCollection
			             	{
			             		{"UserName", login},
			             		{"Password", password},
			             		{"btnLogin", "Sign%20In"},
			             		{"__EVENTTARGET", ""},
			             		{"__EVENTARGUMENT", ""},
			             		{"scriptManager", "mainPanel|btnLogin"}
			             	});
		}
	}
}