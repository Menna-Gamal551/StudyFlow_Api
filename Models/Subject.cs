

namespace StudyFlow_Api.Models
{
    public class Subject
    {
        
        public int Id { get; set; }
       
        public required string SubjectName { get; set; }
        public ICollection<Assignment>?Assignments { get; set; }=new List<Assignment>();
    }
}
