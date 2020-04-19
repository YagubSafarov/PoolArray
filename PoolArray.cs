using System.Collections.Generic;

public class PoolArray<T> : System.IDisposable
{
    // private static System.Action<int, int> OnDestroy; NOT OPTIMAL
    public class Ref
    {
        public int length = -1;
        public int index = -1;
    }


    private static bool memoryAutoControl = true;
    private static int maxSize = 100;
    private static int optimalSize = maxSize / 2;
    private static Dictionary<int, List<T[]>> pool = new Dictionary<int, List<T[]>>();
    private static Dictionary<int, List<bool>> free = new Dictionary<int, List<bool>>();
    private static Dictionary<int, List<PoolArray<T>>> users = new Dictionary<int, List<PoolArray<T>>>();
    private static Dictionary<int, int> usage = new Dictionary<int, int>();

    private static int GetPoolSize()
    {
        int factor = 0;

        foreach (var v in pool.Values)
        {
            factor += v.Count;
        }

        return factor;
    }

    public static void Destroy(int length, int index, bool removeAll)
    {
        if (removeAll)
        {
            pool.Remove(length);
            free.Remove(length);
            users.Remove(length);
            usage.Remove(length);
            return;
        }

        pool[length].RemoveAt(index);
        free[length].RemoveAt(index);


        for (int i = index + 1, iMax = users[length].Count; i < iMax; i++)
        {
            if (users[length][i] != null)
            {
                users[length][i].OnDestroyedFromPool(length, index);
            }
        }
    }

    public static void Create(int length)
    {
        if (pool.ContainsKey(length))
        {
            pool[length].Add(new T[length]);
            free[length].Add(true);
            users[length].Add(null);
            usage[length]++;
        }
        else
        {
            pool.Add(length, new List<T[]> { new T[length] });
            free.Add(length, new List<bool> { true });
            users.Add(length, new List<PoolArray<T>> { null });
            usage.Add(length, 1);
        }
    }

    public static void CheckSize()
    {
        int size = GetPoolSize();
        if (maxSize >= size)
            return;

        int totalRemove = maxSize - optimalSize;

        for (int i = 0; i < totalRemove; i++)
        {
            if (!TryRemoveOptimal())
                break;
        }
    }

    public static bool TryRemoveOptimal()
    {
        int minUsageLength = -1;
        int minUsageIndex = -1;
        bool removeAll = false;

        int minUsageValue = 999999;

        foreach (var pair in usage)
        {
            if (pair.Value < minUsageValue)
            {
                int freeIndex = GetFreeIndex(pair.Key);
                if (freeIndex >= 0)
                {
                    minUsageLength = pair.Key;
                    minUsageIndex = freeIndex;
                    minUsageValue = pair.Value;
                    removeAll = free[pair.Key].Count < 2;
                }
            }
        }

        if (minUsageLength >= 0)
        {
            Destroy(minUsageLength, minUsageIndex, removeAll);
            return true;
        }
        return false;
    }

    private static int GetFreeIndex(int length)
    {
        if (free.ContainsKey(length))
        {
            for (int i = 0, iMax = free[length].Count; i < iMax; i++)
            {
                if (free[length][i])
                {
                    return i;
                }
            }

        }
        return -1;
    }

    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 

    private Ref current = new Ref();
    public int Length => current.length;

    public PoolArray()
    {
    }


    public PoolArray(int length)
    {
        Init(length);
    }

    private void OnDestroyedFromPool(int length, int index)
    {
        if (current.length != length || current.index < index)
            return;

        current.index--;
    }

    public T this[int index]
    {
        get
        {
            return pool[current.length][current.index][index];
        }
        set
        {
            pool[current.length][current.index][index] = value;
        }
    }

    public T[] ToArray()
    {
        return pool[current.length][current.index];
    }

    public void Init(int length)
    {
        if (current.length >= 0)
            Dispose();

        current.length = length;
        current.index = GetFreeIndex(length);

        if (current.index < 0)
        {
            Create(length);
            current.index = GetFreeIndex(current.length);
        }

        free[length][current.index] = false;
        users[length][current.index] = this;
        usage[length]++;
    }

    public void Dispose()
    {
        users[current.length][current.index] = null;
        free[current.length][current.index] = true;

        current.length = -1;
        current.index = -1;

        if (memoryAutoControl)
            CheckSize();
    }
}
