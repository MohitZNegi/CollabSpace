namespace CollabSpace.Services
{
    public abstract class BaseService
    {
        protected static async Task ExecuteSequentiallyAsync(
            IEnumerable<Func<Task>> operations)
        {
            foreach (var operation in operations)
            {
                await operation();
            }
        }

        protected static async Task<List<T>> ExecuteSequentiallyAsync<T>(
            IEnumerable<Func<Task<T>>> operations)
        {
            var results = new List<T>();

            foreach (var operation in operations)
            {
                results.Add(await operation());
            }

            return results;
        }
    }
}
