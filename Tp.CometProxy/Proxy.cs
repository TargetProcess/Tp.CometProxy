// 
// Copyright (c) 2005-2011 TargetProcess. All rights reserved.
// TargetProcess proprietary/confidential. Use is subject to license terms. Redistribution of this file is strictly forbidden.
// 

using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Microsoft.AspNet.SignalR.Client;
using log4net;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace Tp.CometProxy
{
	public class Proxy : IDisposable
	{
		private readonly IHubProxy _proxy;
		private readonly IDictionary<string, CountdownEvent> _subscriptions;
		private readonly ILog _log;
		public Proxy(IHubProxy proxy)
		{
			_subscriptions = new Dictionary<string, CountdownEvent>();
			_proxy = proxy;
			_log = LogManager.GetLogger("CometProxy");
		}

		public void Subscribe(Subscription subscription, Action<dynamic> callback)
		{
			_proxy.On("notifyChanged", data =>
			                           	{
			                           		_log.InfoFormat("Notification received: Subscription Id: {0}", subscription.Id);
			                           		_log.DebugFormat("Notification received: Data: {0}", data);
			                           		CountdownEvent ev = _subscriptions[subscription.Id];

			                           		if (callback != null)
											{
												callback(data);
											}
											if (!ev.IsSet)
											{
												ev.Signal();
											}
			                           	});
			_proxy.Invoke("Subscribe", subscription)
				.ContinueWith(task =>
				              	{
				              		if (!task.IsFaulted)
				              		{
				              			_log.InfoFormat("Subscribed successfully. Subscriptions ID: {0}", subscription.Id);
				              			_subscriptions.Add(subscription.Id, new CountdownEvent(Int32.MaxValue));
				              		}
				              		else
				              		{
				              			_log.ErrorFormat("Error during subscribing. Subscription Id: {0}, Exception: {1}", subscription.Id, task.Exception);
				              		}
				              	})
				.Wait();
		}

		public void Unsubscribe(string subscriptionId)
		{
			_proxy.Invoke("Unsubscribe", subscriptionId)
				.ContinueWith(task =>
				              	{
				              		if (!task.IsFaulted)
				              		{
				              			_log.InfoFormat("Unsubscribed successfully. Subscription Id: {0}", subscriptionId);
				              			_subscriptions.Remove(subscriptionId);
				              		}
				              		else
				              		{
				              			_log.ErrorFormat("Error during unsubscribing. Subscription Id: {0}, Exception: {1}", subscriptionId, task.Exception);
				              		}
				              	})
				.Wait();
		}

		public void Wait(string subscriptionId, int numberOfEvents, int timeoutInSeconds)
		{
			_log.InfoFormat("Waiting for notifications. Subscription Id: {0}", subscriptionId);
			var ev = _subscriptions[subscriptionId];
			var evsRaised = ev.InitialCount - ev.CurrentCount;
			if (numberOfEvents <= evsRaised)
			{
				return;
			}
			ev.Reset(numberOfEvents - evsRaised);
			ev.Wait(TimeSpan.FromSeconds(timeoutInSeconds));
			if (!ev.IsSet)
			{
				_log.Error("Not all notifications have been received");
				throw new InvalidOperationException("Not all notifications have been received");
			}
		}

		public void Wait(string subscriptionId)
		{
			Wait(subscriptionId, 1, 3);
		}

		public void Dispose()
		{
			foreach (string subscriptionId in _subscriptions.Keys.ToList())
			{
				Unsubscribe(subscriptionId);
			}
		}
	}
}