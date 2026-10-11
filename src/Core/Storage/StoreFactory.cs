using Core.Abstractions;

namespace Core.Storage;

public static class StoreFactory
{
    public static IOrderStore CreateStore(string[] args, string defaultJsonPath)
    {
        bool useFile = args.Contains("--file");
        bool useCache = args.Contains("--cache");

        IOrderStore store = useFile
            ? new FileOrderStore(defaultJsonPath)
            : new InMemoryOrderStore(SampleData.GetOrders());

        if (useCache)
        {
            store = new CachingOrderStore(store);
        }

        return store;
    }
}