# PoolArray
Array Pool implementation


Usage:

    PoolArray<int> intArray = new PoolArray<int>();  // Create instance

    intArray.Init(100);  // Set array with length (can set in the constructor)
    
    intArray[index]  // Call like array
    
    intArray.Dispose() //  Dispose 
