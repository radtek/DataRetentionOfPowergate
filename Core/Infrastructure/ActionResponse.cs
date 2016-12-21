namespace DataRetention.Core.Infrastructure
{
    /// <summary>
    /// Provides a standardised means fopr functions that need to return data to also include operation success, failure and error messages
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ActionResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Response { get; set; }
    }

    /// <summary>
    /// Provides a standardised means fopr functions to return success/failure and error messages
    /// </summary>
    public class ActionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
