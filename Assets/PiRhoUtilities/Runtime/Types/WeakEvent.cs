using System;

namespace PiRhoSoft.Utilities
{
	public class WeakEvent
	{
		private event Action Event;

		public void Subscribe<T>(T target, Action<T> callback) where T : class
		{
			var reference = new WeakReference(target, false);
			var handler = (Action)null;

			handler = () =>
			{
				if (reference.Target is T t)
					callback(t);
				else
					Event -= handler;
			};

			Event += handler;
		}

		public void Trigger()
		{
			Event?.Invoke();
		}
	}

	public class WeakEvent<TArgs>
	{
		private event Action<TArgs> Event;

		public void Subscribe<T>(T target, Action<T, TArgs> callback) where T : class
		{
			var reference = new WeakReference(target, false);
			var handler = (Action<TArgs>)null;

			handler = args =>
			{
				if (reference.Target is T t)
					callback(t, args);
				else
					Event -= handler;
			};

			Event += handler;
		}

		public void Trigger(TArgs args)
		{
			Event?.Invoke(args);
		}
	}
}
