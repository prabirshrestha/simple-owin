using System;
using System.Threading.Tasks;
using SimpleOwin.Extensions;
using SimpleOwin.Extensions.Stream;

namespace SimpleOwin.Samples.AspNetMono
{
	using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

	public class HelloWorldOwinApp
	{
		public static AppFunc OwinApp()
		{
			return env =>
			{
				env.GetResponseBody()
					.WriteString("Hello world");

				var tcs = new TaskCompletionSource<int>(); 
				tcs.TrySetResult(0);
				return tcs.Task;
			};
		}
	}
}

