using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StudyFlow_Api.DTOs;
using StudyFlow_Api.Models;

namespace StudyFlow_Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly AppdbContext _db;
        private readonly IMapper _mapper;
        public SubjectController(AppdbContext db, IMapper mapper)
        {
            _db = db;
            _mapper=mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SubjectDTO>>),StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<SubjectDTO>>>> GetAll()
        {
            var subjects = await _db.Subjects.ToListAsync();

            return Ok(ApiResponse<IEnumerable<SubjectDTO>>.Ok( "Data retrieved successfully", _mapper.Map<IEnumerable<SubjectDTO>>(subjects)));
        }

        [HttpGet("{id}")]

        public async Task<ActionResult<ApiResponse<SubjectDTO>>> GetById(int id)
        {
            if (id<=0)
            {
                return BadRequest(ApiResponse<object>.BadRequest("please enter id greater than 0"));
            }
            var subjectbyId = await _db.Subjects.FirstOrDefaultAsync(e=>e.Id==id);
            if (subjectbyId==null)
            { 
            return NotFound(ApiResponse<object>.NotFound($"subject with id : {id} not found"));
            }

            return Ok(ApiResponse<SubjectDTO>.Ok("Data retrived successfully", _mapper.Map<SubjectDTO>(subjectbyId)));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<SubjectDTO>>> CreateSubject(CreateSubjectDTO createSubjectDTO)
        {
            if (createSubjectDTO==null)
            {
                return BadRequest(ApiResponse<object>.BadRequest("please enter data"));
            }
            if (string.IsNullOrWhiteSpace(createSubjectDTO.SubjectName))
            {
                return BadRequest(ApiResponse<object>.BadRequest("please enter Subject Name"));
            }
            createSubjectDTO.SubjectName=createSubjectDTO.SubjectName.Trim();
            if (await _db.Subjects.AnyAsync(e=>e.SubjectName.Trim().ToLower()==createSubjectDTO.SubjectName.ToLower()))
            {
                return BadRequest(ApiResponse<object>.BadRequest("there are already subject with this name!"));
            }
            
            var newSubject=_mapper.Map<Subject>(createSubjectDTO);
            await _db.AddAsync(newSubject);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id=newSubject.Id},ApiResponse<SubjectDTO>.CreatedAt("Created successfully", _mapper.Map<SubjectDTO>(newSubject)));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<SubjectDTO>>>UpdateSubject(int id,UpdateSubjectDTO updateSubjectDTO)
        {
            if (id<=0)
            {
                return BadRequest(ApiResponse<object>.BadRequest("please enter id greater than 0"));
            }
            if (updateSubjectDTO==null)
            {
                return BadRequest(ApiResponse<object>.BadRequest("please enter data"));
            }

            if (id!=updateSubjectDTO.Id)
            {
                return BadRequest(ApiResponse<object>.BadRequest("entered id not match any exist subject"));
            }
            if (string.IsNullOrWhiteSpace(updateSubjectDTO.SubjectName))
            {
                return BadRequest(ApiResponse<object>.BadRequest("please enter Subject Name"));
            }
            var subject = await _db.Subjects.FirstOrDefaultAsync(e => e.Id==id);
            if (subject==null)
            {
                return NotFound(ApiResponse<object>.NotFound($"subject with id : {id} not found"));
            }

            updateSubjectDTO.SubjectName=updateSubjectDTO.SubjectName.Trim();

            if (await _db.Subjects.AnyAsync(e => e.SubjectName.Trim().ToLower()==updateSubjectDTO.SubjectName.ToLower()&&e.Id!=id))
            {
                return BadRequest(ApiResponse<object>.BadRequest("there are already subject with this name!"));
            }
            
            
            _mapper.Map(updateSubjectDTO, subject);
            await _db.SaveChangesAsync();

            return Ok(ApiResponse<SubjectDTO>.Ok("Data Updated successfully", _mapper.Map<SubjectDTO>(subject)));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteSubject(int id )
        {
            if (id<=0)
            {
                return BadRequest(ApiResponse<object>.BadRequest("please enter id greater than 0"));
            }
           
            var subject = await _db.Subjects.FirstOrDefaultAsync(e => e.Id==id);
            if (subject==null)
            {
                return NotFound(ApiResponse<object>.NotFound($"subject with id : {id} not found"));
            }
            if (await _db.Assignments.AnyAsync(e => e.SubjectId==id))
            {
                return Conflict(ApiResponse<object>.Conflict("This subject has assignments and cannot be deleted."));
            }
            _db.Subjects.Remove(subject);
            await _db.SaveChangesAsync();
            return NoContent();
        }

    }
}
