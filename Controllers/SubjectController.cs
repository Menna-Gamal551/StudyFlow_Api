using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StudyFlow_Api.DTOs;
using StudyFlow_Api.Models;

namespace StudyFlow_Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly AppdbContext _db;
        public SubjectController(AppdbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<Subject>>>> GetAll()
        {
            return Ok();
        }

        [HttpGet("{id}")]

        public async Task<ActionResult<ApiResponse<Subject>>> GetById(int id)
        {
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<Subject>>> CreateSubject()
        {
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<Subject>>>UpdateSubject(int id)
        {
            return Ok();
        }

        [HttpDelete]
        public async Task<ActionResult<ApiResponse<Subject>>> DeleteSubject(int id )
        {
            return Ok();
        }

    }
}
