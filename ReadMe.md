# Using FlatBuffers in Unity Prevent GC Alloc

## Why

* In some sample code, you may see

```csharp
        var builder = new Google.FlatBuffers.FlatBufferBuilder(1);
        // ... do sth
```

* The above code is the worst way to prevent GC.Alloc.
* Whenever the length of the ByteBuffer's previous byte array is less than the currently used length, the previous byte array is GC'd and a new byte array is allocated.
* Create a builder with the full used length and store it to be used multiple times if needed.
* Creating a string from bytes will result in a GC.Alloc that stores the string you will use multiple times.

## Profiler

* GC.Alloc of new builder(1), GrowFront multiple times.
![TestNewBiulder](./ReadMe/TestNewBiulder.png)

* GC.Alloc of use stored builder.
![TestStoreBuilder](./ReadMe/TestStoreBuilder.png)
