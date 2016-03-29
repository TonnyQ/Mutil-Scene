using System;
using System.Diagnostics;

namespace Util
{
	public class StopTime : IDisposable
	{
		private string _tag;
		private Stopwatch _time;

		public StopTime(string tag)
		{
			_tag = tag;
			_time = Stopwatch.StartNew ();
		}

		public void Dispose()
		{
			_time.Stop ();
			UnityEngine.Debug.Log (string.Concat (_tag, " cost time : ", _time.ElapsedMilliseconds));
		}
	}
}

