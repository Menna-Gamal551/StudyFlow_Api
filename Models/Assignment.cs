
namespace StudyFlow_Api.Models
{
    public class Assignment
    {
        
        public int Id { get; set; }
        
        public required string Title { get; set; }=string.Empty;
        public string ?Description { get; set; } 
        public DateTime? DueDate { get; set; }
        public SD.Priority Priority { get; set; } = SD.Priority.Low;
        public SD.Status Status { get; set; } = SD.Status.Pending;
        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

    }
}
