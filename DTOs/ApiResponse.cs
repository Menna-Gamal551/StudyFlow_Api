namespace StudyFlow_Api.DTOs
{
    public class ApiResponse<TData>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public object? Errors { get; set; }
        public TData? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<TData> Create(bool success, int statuscode, string message, TData? Data = default, object? error = null)
        {
            return new ApiResponse<TData> { Success = success, StatusCode = statuscode, Message = message, Data = Data, Errors = error };

        }
        public static ApiResponse<TData> NotFound(string message = "Resource Not Found") => Create(false, 404, message);
        public static ApiResponse<TData> Conflict(string message) => Create(false, 409, message);
        public static ApiResponse<TData> BadRequest(string message, object? error = null) => Create(false, 400, message, error: error);
        public static ApiResponse<TData> Error(int statuscode, string message, object? error = null) => Create(false, statuscode, message, error: error);


        public static ApiResponse<TData> Ok(string message, TData tdata) => Create(true, 200, message, tdata);
        public static ApiResponse<TData> CreatedAt(string message, TData tdata) => Create(true, 201, message, tdata);
        public static ApiResponse<TData> NoContent(string message = "Operation Completed Successfully") => Create(true, 204, message);

    }
}
