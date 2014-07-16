// 
// Copyright (c) 2005-2011 TargetProcess. All rights reserved.
// TargetProcess proprietary/confidential. Use is subject to license terms. Redistribution of this file is strictly forbidden.
// 

namespace Tp.CometProxy
{
	public class Subscription
	{
		public Subscription(string id, dynamic parameters)
		{
			Id = id;
			Parameters = parameters;
		}

		public string Id { get; private set; }
		public string ClientId { get; set; }
		public dynamic Parameters { get; private set; }
	}
}