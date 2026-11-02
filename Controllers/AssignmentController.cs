using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyFlow_Api.DTOs;
using StudyFlow_Api.Models;

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


        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<AssignmentDTO>>>> GetAll()
        {
            var assignments = await _db.Assignments.Include(e => e.Subject).ToListAsync();


            return Ok(ApiResponse<IEnumerable<AssignmentDTO>>.Ok("Data retrived successfully", _mapper.Map<List<AssignmentDTO>>(assignments)));
        }
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
