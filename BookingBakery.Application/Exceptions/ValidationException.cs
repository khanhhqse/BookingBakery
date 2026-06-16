namespace BookingBakery.Application.Exceptions
{
    public class ValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("Dữ liệu không hợp lệ.")
        {
            Errors = errors;
        }

        public ValidationException(string field, string message)
            : this(new Dictionary<string, string[]> { { field, new[] { message } } })
        {
        }
    }
}