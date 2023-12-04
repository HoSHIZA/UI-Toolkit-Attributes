using System;
using System.Collections.Generic;
using System.Linq;

namespace PiRhoSoft.Utilities
{
	public class HierarchyPool<TBaseType>
		where TBaseType : class
	{
		private Dictionary<Type, ISubclassPool> _pools = new Dictionary<Type, ISubclassPool>();

		public void Register<TKeyType, TYpe>(Func<TYpe> creator, int capacity = ClassPool.DEFAULT_CAPACITY, int growth = ClassPool.DEFAULT_GROWTH)
			where TYpe : class, TBaseType
		{
			_pools.Add(typeof(TKeyType), new SubclassPool<TYpe>(creator, capacity, growth));
		}

		public List<ClassPoolInfo> GetPoolInfo()
		{
			return _pools
				.Select(pool => new ClassPoolInfo
				{
					Type = pool.Key,
					IsRegistered = pool.Value == null ? false : pool.Value.IsRegistered,
					ReservedCount = pool.Value == null ? 0 : pool.Value.ReservedCount,
					FreeCount = pool.Value == null ? -1 : pool.Value.FreeCount
				})
				.ToList();
		}

		public TBaseType Reserve(Type type)
		{
			var pool = GetPool(type);
			return pool?.Reserve();
		}

		public TYpe Reserve<TYpe>()
			where TYpe : class, TBaseType
		{
			var pool = GetPool(typeof(TYpe));
			return pool?.Reserve() as TYpe;
		}

		public void Release(TBaseType item)
		{
			var pool = GetPool(item.GetType());
			pool?.Release(item);
		}

		private interface ISubclassPool
		{
			bool IsRegistered { get; }
			int ReservedCount { get; }
			int FreeCount { get; }
			TBaseType Reserve();
			void Release(TBaseType item);
		}

		private class SubclassPool<TYpe> : ClassPool<TYpe>, ISubclassPool
			where TYpe : class, TBaseType
		{
			public bool IsRegistered => true;
			TBaseType ISubclassPool.Reserve() => Reserve();
			void ISubclassPool.Release(TBaseType item) => Release((TYpe)item);

			public SubclassPool(Func<TYpe> creator, int capacity, int growth) : base(creator, capacity, growth) { }
		}

		private class GenericSubclassPool : ClassPool<object>, ISubclassPool
		{
			public bool IsRegistered => false;
			TBaseType ISubclassPool.Reserve() => Reserve() as TBaseType;
			void ISubclassPool.Release(TBaseType item) => Release(item);

			public GenericSubclassPool(Type type) : base(() => Activator.CreateInstance(type)) {}
		}

		private ISubclassPool GetPool(Type type)
		{
			if (!_pools.TryGetValue(type, out var pool))
			{
				// This will not work in builds where unusued types get stripped. It is intended to be used as a
				// fallback during development without having to register all the types.

				// TODO: Figure out exactly what happens and handle when type has not been included in the build.

				if (typeof(TBaseType).IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null)
					pool = new GenericSubclassPool(type);
				else
					pool = null;

				// If the pool can't be created it is still registered so the creation won't be attempted the next time
				// and so it can be seen in GetPoolInfo.
				_pools.Add(type, pool);
			}

			return pool;
		}
	}
}
