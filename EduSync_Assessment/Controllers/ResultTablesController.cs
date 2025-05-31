﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSync_Assessment.Data;
using EduSync_Assessment.Models;
using EduSync_Assessment.DTO; 

namespace EduSync_Assessment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultTablesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResultTablesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ResultTables/by-instructor/{instructorId}
        [HttpGet("by-instructor/{instructorId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetResultsByInstructor(Guid instructorId)
        {
            var results = await _context.ResultTables
                .Include(r => r.Assessment)
                    .ThenInclude(a => a.Course)
                .Include(r => r.User)
                .Where(r => r.Assessment.Course.InstructorId == instructorId)
                .Select(r => new
                {
                    StudentName = r.User.Name,
                    StudentEmail = r.User.Email,
                    AssessmentTitle = r.Assessment.Title,
                    CourseTitle = r.Assessment.Course.Title,
                    Score = r.Score,
                    MaxScore = r.Assessment.MaxScore,
                    AttemptDate = r.AttemptDate
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/ResultTables
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResultReadDto>>> GetResultTables()
        {
            var resultDtos = await _context.ResultTables
                .Include(r => r.Assessment)
                    .ThenInclude(a => a.Course)
                .Include(r => r.User)
                .Select(r => new ResultReadDto
                {
                    ResultId = r.ResultId,
                    AssessmentId = r.AssessmentId,
                    UserId = r.UserId,
                    Score = r.Score,
                    AttemptDate = r.AttemptDate
                })
                .ToListAsync();

            return Ok(resultDtos);
        }

        // GET: api/ResultTables/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ResultReadDto>> GetResultTable(Guid id)
        {
            var result = await _context.ResultTables
                .Include(r => r.Assessment)
                    .ThenInclude(a => a.Course)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
            {
                return NotFound();
            }

            var dto = new ResultReadDto
            {
                ResultId = result.ResultId,
                AssessmentId = result.AssessmentId,
                UserId = result.UserId,
                Score = result.Score,
                AttemptDate = result.AttemptDate
            };

            return Ok(dto);
        }

        // POST: api/ResultTables
        [HttpPost]
        public async Task<ActionResult<ResultReadDto>> PostResultTable(ResultCreateDto dto)
        {
            var newResult = new ResultTable
            {
                ResultId = Guid.NewGuid(),
                AssessmentId = dto.AssessmentId,
                UserId = dto.UserId,
                Score = dto.Score,
                AttemptDate = dto.AttemptDate
            };

            _context.ResultTables.Add(newResult);
            await _context.SaveChangesAsync();

            var readDto = new ResultReadDto
            {
                ResultId = newResult.ResultId,
                AssessmentId = newResult.AssessmentId,
                UserId = newResult.UserId,
                Score = newResult.Score,
                AttemptDate = newResult.AttemptDate
            };

            return CreatedAtAction(nameof(GetResultTable), new { id = newResult.ResultId }, readDto);
        }

        // PUT: api/ResultTables/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutResultTable(Guid id, ResultCreateDto dto)
        {
            var result = await _context.ResultTables.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            result.AssessmentId = dto.AssessmentId;
            result.UserId = dto.UserId;
            result.Score = dto.Score;
            result.AttemptDate = dto.AttemptDate;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/ResultTables/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResultTable(Guid id)
        {
            var result = await _context.ResultTables.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }
                
            _context.ResultTables.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ResultTableExists(Guid id)
        {
            return _context.ResultTables.Any(e => e.ResultId == id);
        }
    }
}
