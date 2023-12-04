using System;
using System.Collections.Generic;

namespace PiRhoSoft.Utilities
{
	public class ClassPoolInfo
	{
		public Type Type;
		public bool IsRegistered;
		public int ReservedCount;
		public int FreeCount;
	}

	public static class ClassPool
	{
		public const int DEFAULT_CAPACITY = 10;
		public const int DEFAULT_GROWTH = 5;
	}

	public class ClassPool<TYpe>
	{
		private int _growth;
		private Stack<TYpe> _freeList;
		private Func<TYpe> _creator;
		private int _reservedCount;

		public int ReservedCount => _reservedCount;
		public int FreeCount => _freeList.Count;

		public ClassPool(Func<TYpe> creator, int capacity = ClassPool.DEFAULT_CAPACITY, int growth = ClassPool.DEFAULT_GROWTH)
		{
			_growth = growth;
			_freeList = new Stack<TYpe>(capacity);
			_creator = creator;
			_reservedCount = 0;

			for (var i = 0; i < capacity; i++)
            {
                Release(_creator());
            }
        }

		public TYpe Reserve()
		{
			if (_freeList.Count == 0)
			{
				if (_growth > 0)
				{
					for (var i = 0; i < _growth; i++)
                    {
                        Release(_creator());
                    }
                }
				else
				{
					return default;
				}
			}

			_reservedCount++;
			return _freeList.Pop();
		}

		public void Release(TYpe item)
		{
			_reservedCount--;
			_freeList.Push(item);
		}

		public ClassPoolInfo GetPoolInfo()
		{
			return new ClassPoolInfo
			{
				Type = typeof(TYpe),
				IsRegistered = true,
				ReservedCount = ReservedCount,
				FreeCount = FreeCount
			};
		}
	}
}
