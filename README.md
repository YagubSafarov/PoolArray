# PoolArray
Array Pool implementation


Usage:

    PoolArray<int> intArrayPoolFactory = new PoolArray<int>();  // Create instance

    intArrayPoolFactory.Init(100);  // Init array length (can set in the constructor)
    
    intArrayPoolFactory[index]  // Call like array
    
    intArrayPoolFactory.Dispose() //  Dispose 
