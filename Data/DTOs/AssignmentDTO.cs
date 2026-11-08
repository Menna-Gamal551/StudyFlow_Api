using StudyFlow_Api.Models;

namespace StudyFlow_Api.Data.DTOs
{
    public class AssignmentDTO
    {
        public int Id { get; set; }

        public required string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public SD.Priority Priority { get; set; } = SD.Priority.Low;
        public SD.Status Status { get; set; } = SD.Status.Pending;
        public string SubjectName { get; set; } = string.Empty;

    }
}
