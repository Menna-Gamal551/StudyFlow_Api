using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyFlow_Api.Data;
using StudyFlow_Api.Data.DTOs;
using StudyFlow_Api.Data.Models;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Text;
using static StudyFlow_Api.SD;

namespace StudyFlow_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AppdbContext _db;

        public AssignmentController(IMapper mapper, AppdbContext db)
        {
            _mapper=mapper;
            _db=db;
        }


        //[HttpGet]
        //public async Task<ActionResult<ApiResponse<IEnumerable<AssignmentDTO>>>> GetAll()
        //{
        //    var assignments = await _db.Assignments.Include(e => e.Subject).ToListAsync();


        //    return Ok(ApiResponse<IEnumerable<AssignmentDTO>>.Ok("Data retrived successfully", _mapper.Map<List<AssignmentDTO>>(assignments)));
        //}
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AssignmentDTO>>> GetById(int id)
        {
            if (id<=0)
            {
                return BadRequest(ApiResponse<object>.BadRequest("please enter id greater than 0"));
            }
            var assignment = await _db.Assignments.Include(e => e.Subject).FirstOrDefaultAsync(e => e.Id==id);
            if (assignment==null)
            {
                return NotFound(ApiResponse<object>.NotFound($"Assignment with id : {id} not found"));
            }
           
            return Ok(ApiResponse<AssignmentDTO>.Ok("Data retrived successfully", _mapper.Map<AssignmentDTO>(assignment)));
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<AssignmentDTO>>>> GetAssignments(string ?filterby,string ?filterquery,string?sortedby, 
            string? sortOrder = "asc",int page=1,int pageSize=5)
        {
            var assignmentQuery =  _db.Assignments.AsQueryable();
            filterquery = filterquery?.Trim();
            if (page<1) page=1;
            if (pageSize<1) pageSize=5;
            if (pageSize>100) pageSize=100;

            if (!string.IsNullOrWhiteSpace(filterby)&&!string.IsNullOrWhiteSpace(filterquery))
            {
                switch (filterby.ToLower())
                {
                    case "subjectname":

                        assignmentQuery=assignmentQuery.Where(f => f.Subject.SubjectName.ToLower().Trim().Contains(filterquery.ToLower()));
                        break;

                    case "status":
                        if (Enum.TryParse<SD.Status>(filterquery, true, out var status)&&Enum.IsDefined(typeof(SD.Status), status))
                            assignmentQuery=assignmentQuery.Where(f => f.Status==status);
                        else
                        {
                            return BadRequest(ApiResponse<object>.BadRequest("Invalid filtering or filtering by"));
                        }
                        break;

                    case "priority":
                        if (Enum.TryParse<SD.Priority>(filterquery, true, out var priority)&&Enum.IsDefined(typeof(SD.Priority), priority))
                            assignmentQuery=assignmentQuery.Where(f => f.Priority==priority);
                        else
                        {
                            return BadRequest(ApiResponse<object>.BadRequest("Invalid filtering or filtering by"));
                        }
                        break;

                    default:
                        return BadRequest(ApiResponse<object>.BadRequest("Invalid filtering or filtering by"));


                }
            }
            else if (string.IsNullOrWhiteSpace(filterby)&&string.IsNullOrWhiteSpace(filterquery))
            {
                assignmentQuery=assignmentQuery.OrderBy(e => e.Id);
            }
            else 
            {
                return BadRequest(ApiResponse<object>.BadRequest("Invalid filtering or filtering by"));

            }
            if (!string.IsNullOrWhiteSpace(sortedby))
            {
               
                if (sortOrder?.ToLower()!="desc"&&sortOrder?.ToLower()!="asc")
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Invalid sorting "));
                }

                var isDescending = sortOrder.ToLower()=="desc";
                assignmentQuery=  sortedby.ToLower() switch
                {
                    "subjectid" => isDescending ? assignmentQuery.OrderByDescending(e => e.SubjectId) : assignmentQuery.OrderBy(e => e.SubjectId),
                    "status" => isDescending ? assignmentQuery.OrderByDescending(e => e.Status) : assignmentQuery.OrderBy(e => e.Status),
                    "priority" => isDescending ? assignmentQuery.OrderByDescending(e => e.Priority) : assignmentQuery.OrderBy(e => e.Priority),
                    "duedate" => isDescending ? assignmentQuery.OrderByDescending(e => e.DueDate).ThenBy(e => e.Id) : assignmentQuery.OrderBy(e => e.DueDate).ThenBy(e => e.Id),
                    _ => assignmentQuery.OrderBy(e=>e.Id)
                }; 
            }

            var skip = (page-1)*pageSize;
            var totalAssignment = await assignmentQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double) totalAssignment /pageSize);

            assignmentQuery= assignmentQuery.Skip(skip).Take(pageSize);
            var assignments = await assignmentQuery.Include(e => e.Subject).ToListAsync();

            var dtoResponseAssignment= _mapper.Map<List<AssignmentDTO>>(assignments);

            var messagebuilder = new StringBuilder();
            messagebuilder.Append($"a successful Retrived {dtoResponseAssignment.Count} assignment/s ");
            messagebuilder.Append($"(Page {page} of {totalPages}),{totalAssignment} total records ");
            if (!string.IsNullOrEmpty(filterby)&&!string.IsNullOrEmpty(filterquery))
            {
                messagebuilder.Append($"filtered by {filterby}: '{filterquery}'");
            }
            if (!string.IsNullOrEmpty(sortedby))
            {
                messagebuilder.Append($"sorted by {sortedby}: '{sortOrder?.ToLower()??"asc"}' ");
            }

            Response.Headers.Append("X-Pagination-CurrentPage", page.ToString());
            Response.Headers.Append("X-Pagination-PageSize", pageSize.ToString());
            Response.Headers.Append("X-Pagination-TotalPages", totalPages.ToString());
            Response.Headers.Append("X-Pagination-TotalAssignments", totalAssignment.ToString());

            return Ok(ApiResponse<IEnumerable<AssignmentDTO>>.Ok(messagebuilder.ToString(), dtoResponseAssignment));
        }
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<AssignmentDTO>>>> GetUpcomingAssignments()
        {

            var currentDate = DateTime.UtcNow;
            var expireDate = currentDate.AddDays(7);

            var assignments = await  _db.Assignments.Include(e=>e.Subject).Where(e =>e.DueDate>=currentDate&& e.DueDate<=expireDate && e.Status!=SD.Status.Completed).ToListAsync();
               
            return Ok(ApiResponse<IEnumerable<AssignmentDTO>>.Ok("Data retrived successfully", _mapper.Map<List<AssignmentDTO>>(assignments)));
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<AssignmentDTO>>>> GetOverdueAssignments()
        {

            var currentDate = DateTime.UtcNow;
            

            var assignments = await _db.Assignments.Include(e => e.Subject).Where(e => e.DueDate<currentDate && e.Status!=SD.Status.Completed).ToListAsync();

            return Ok(ApiResponse<IEnumerable<AssignmentDTO>>.Ok("Data retrived successfully", _mapper.Map<List<AssignmentDTO>>(assignments)));
        }

        [HttpPost]

        public async Task<ActionResult<ApiResponse<AssignmentDTO>>> CreateAssignment(CreateAssignmentDTO createAssignmentDTO)
        {
            if (createAssignmentDTO == null)
            {
                return BadRequest(ApiResponse<object>.BadRequest("Please provide assignment data."));
            }

            if (string.IsNullOrWhiteSpace(createAssignmentDTO.Title))
            {
                return BadRequest(ApiResponse<object>.BadRequest("Assignment title is required."));
            }

            createAssignmentDTO.Title = createAssignmentDTO.Title.Trim();

            if (createAssignmentDTO.SubjectId <= 0)
            {
                return BadRequest(ApiResponse<object>.BadRequest("Please provide a valid Subject ID."));
            }

            var subjectExists = await _db.Subjects.AnyAsync(e => e.Id == createAssignmentDTO.SubjectId);

            if (!subjectExists)
            {
                return NotFound(ApiResponse<object>.NotFound($"Subject with ID {createAssignmentDTO.SubjectId} was not found."));
            }

            if (await _db.Assignments.AnyAsync(e => e.Title.Trim().ToLower() == createAssignmentDTO.Title.ToLower()
                && e.SubjectId == createAssignmentDTO.SubjectId))
            {
                return Conflict(ApiResponse<object>.Conflict("An assignment with this title already exists in this subject."));
            }

            if (!Enum.IsDefined(typeof(SD.Priority), createAssignmentDTO.Priority))
            {
                return BadRequest(ApiResponse<object>.BadRequest("Invalid priority value. Allowed values are Low, Medium, and High."));
            }

            if (!Enum.IsDefined(typeof(SD.Status), createAssignmentDTO.Status))
            {
                return BadRequest(ApiResponse<object>.BadRequest("Invalid status value. Allowed values are Pending, InProgress, and Completed."));
            }

            var assignment = _mapper.Map<Assignment>(createAssignmentDTO);

            _db.Assignments.Add(assignment);
            await _db.SaveChangesAsync();

            var createdAssignment = await _db.Assignments.Include(e => e.Subject).FirstAsync(e => e.Id == assignment.Id);

            var assignmentDTO = _mapper.Map<AssignmentDTO>(createdAssignment);

            return CreatedAtAction(nameof(GetById), new { id = assignment.Id }, ApiResponse<AssignmentDTO>.CreatedAt(
                    "Assignment created successfully.", assignmentDTO));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<AssignmentDTO>>> UpdateAssignment( int id, UpdateAssignmentDTO updateAssignmentDTO)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse<object>.BadRequest(
                    "Please enter an ID greater than 0."));
            }

            if (updateAssignmentDTO == null)
            {
                return BadRequest(ApiResponse<object>.BadRequest(
                    "Please provide assignment data."));
            }

            if (id != updateAssignmentDTO.Id)
            {
                return BadRequest(ApiResponse<object>.BadRequest(
                    "The entered ID does not match the assignment ID."));
            }

            if (string.IsNullOrWhiteSpace(updateAssignmentDTO.Title))
            {
                return BadRequest(ApiResponse<object>.BadRequest(  "Assignment title is required."));
            }

            updateAssignmentDTO.Title = updateAssignmentDTO.Title.Trim();

            if (!Enum.IsDefined(typeof(SD.Priority), updateAssignmentDTO.Priority))
            {
                return BadRequest(ApiResponse<object>.BadRequest( "Invalid priority value."));
            }

            if (!Enum.IsDefined(typeof(SD.Status), updateAssignmentDTO.Status))
            {
                return BadRequest(ApiResponse<object>.BadRequest( "Invalid status value."));
            }

            var assignment = await _db.Assignments.FirstOrDefaultAsync(e => e.Id == id);

            if (assignment == null)
            {
                return NotFound(ApiResponse<object>.NotFound(  $"Assignment with ID {id} was not found."));
            }

            var subject = await _db.Subjects.FirstOrDefaultAsync(e => e.Id == updateAssignmentDTO.SubjectId);

            if (subject == null)
            {
                return NotFound(ApiResponse<object>.NotFound( $"Subject with ID {updateAssignmentDTO.SubjectId} was not found."));
            }

            if (await _db.Assignments.AnyAsync(e => e.Title.Trim().ToLower() == updateAssignmentDTO.Title.ToLower()))
            {
                return Conflict(ApiResponse<object>.Conflict( "An assignment with this title already exists in this subject."));
            }

            _mapper.Map(updateAssignmentDTO, assignment);

            assignment.Subject = subject;
             
            await _db.SaveChangesAsync();

            

            return Ok(ApiResponse<AssignmentDTO>.Ok( "Assignment updated successfully.", _mapper.Map<AssignmentDTO>(assignment)));
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            if (id<=0)
            {
                return BadRequest(ApiResponse<object>.BadRequest("please enter id greater than 0"));
            }
            var assignment = await _db.Assignments.FirstOrDefaultAsync(e => e.Id==id);
            if (assignment==null)
            {
                return NotFound(ApiResponse<object>.NotFound($"Assignment with id : {id} not found"));

            }
            _db.Assignments.Remove(assignment);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
