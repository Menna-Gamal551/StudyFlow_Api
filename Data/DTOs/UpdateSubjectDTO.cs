using StudyFlow_Api.Models;

namespace StudyFlow_Api.Data.DTOs
{
    public class UpdateSubjectDTO
    {
        public int Id { get; set; }

        public required string SubjectName { get; set; }
    }
}
