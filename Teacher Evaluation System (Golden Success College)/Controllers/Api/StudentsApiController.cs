using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Teacher_Evaluation_System__Golden_Success_College_.Data;
using Teacher_Evaluation_System__Golden_Success_College_.Models;
using Teacher_Evaluation_System__Golden_Success_College_.Helper;

namespace Teacher_Evaluation_System__Golden_Success_College_.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsApiController : ControllerBase
    {
        private readonly Teacher_Evaluation_System__Golden_Success_College_Context _context;

        public StudentsApiController(Teacher_Evaluation_System__Golden_Success_College_Context context)
        {
            _context = context;
        }

        // GET: api/StudentsApi
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetStudents()
        {
            var students = await _context.Student
                .Include(s => s.Level)
                .Include(s => s.Role)
                .Include(s => s.Section)
                .ToListAsync();

            var data = students.Select(s => new
            {
                studentId = s.StudentId,
                fullName = s.FullName,
                email = s.Email,
                levelId = s.LevelId,
                levelName = s.Level?.LevelName,
                sectionId = s.SectionId,
                sectionName = s.Section?.SectionName,
                collegeYearLevel = s.CollegeYearLevel,
                roleId = s.RoleId,
                roleName = s.Role?.Name
            });

            return new ApiResponse<IEnumerable<object>>
            {
                Success = true,
                Message = "Students loaded successfully",
                Data = data
            };
        }

        // GET: api/StudentsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> GetStudent(int id)
        {
            var student = await _context.Student
                .Include(s => s.Level)
                .Include(s => s.Role)
                .Include(s => s.Section)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Student not found",
                    Data = null
                });
            }

            var data = new
            {
                studentId = student.StudentId,
                fullName = student.FullName,
                email = student.Email,
                levelId = student.LevelId,
                levelName = student.Level?.LevelName,
                sectionId = student.SectionId,
                sectionName = student.Section?.SectionName,
                collegeYearLevel = student.CollegeYearLevel,
                roleId = student.RoleId,
                roleName = student.Role?.Name
            };

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Student loaded successfully",
                Data = data
            };
        }

        // POST: api/StudentsApi
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> PostStudent([FromBody] StudentDto studentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid data",
                    Data = null
                });
            }

            // Check for duplicate email
            if (await _context.Student.AnyAsync(s => s.Email == studentDto.Email))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Email already exists",
                    Data = null
                });
            }

            var student = new Student
            {
                FullName = studentDto.FullName,
                Email = studentDto.Email,
                Password = !string.IsNullOrEmpty(studentDto.Password)
                    ? PasswordHelper.HashPassword(studentDto.Password)
                    : "",
                LevelId = studentDto.LevelId,
                SectionId = studentDto.SectionId,
                CollegeYearLevel = studentDto.CollegeYearLevel,
                RoleId = 1 // Default to Student role
            };

            // Auto-set CollegeYearLevel based on Level
            var level = await _context.Level.FindAsync(student.LevelId);
            if (level != null && level.LevelName.ToLower().Contains("college"))
            {
                student.CollegeYearLevel = (student.CollegeYearLevel >= 1 && student.CollegeYearLevel <= 4)
                                            ? student.CollegeYearLevel
                                            : 1;
            }
            else
            {
                student.CollegeYearLevel = 0;
            }

            _context.Student.Add(student);
            await _context.SaveChangesAsync();

            // Reload to get navigation properties
            await _context.Entry(student).Reference(s => s.Level).LoadAsync();
            await _context.Entry(student).Reference(s => s.Section).LoadAsync();
            await _context.Entry(student).Reference(s => s.Role).LoadAsync();

            var data = new
            {
                studentId = student.StudentId,
                fullName = student.FullName,
                email = student.Email,
                levelId = student.LevelId,
                levelName = student.Level?.LevelName,
                sectionId = student.SectionId,
                sectionName = student.Section?.SectionName,
                collegeYearLevel = student.CollegeYearLevel,
                roleId = student.RoleId,
                roleName = student.Role?.Name
            };

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Student created successfully",
                Data = data
            };
        }

        // PUT: api/StudentsApi/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> PutStudent(int id, [FromBody] StudentDto studentDto)
        {
            if (id != studentDto.StudentId)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Student ID mismatch",
                    Data = null
                });
            }

            var existingStudent = await _context.Student.AsNoTracking()
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (existingStudent == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Student not found",
                    Data = null
                });
            }

            // Check for duplicate email (excluding current student)
            if (await _context.Student.AnyAsync(s => s.Email == studentDto.Email && s.StudentId != id))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Email already exists",
                    Data = null
                });
            }

            var student = new Student
            {
                StudentId = studentDto.StudentId,
                FullName = studentDto.FullName,
                Email = studentDto.Email,
                LevelId = studentDto.LevelId,
                SectionId = studentDto.SectionId,
                CollegeYearLevel = studentDto.CollegeYearLevel,
                RoleId = 1 // Always Student role
            };

            // Auto-set CollegeYearLevel
            var level = await _context.Level.FindAsync(student.LevelId);
            if (level != null && level.LevelName.ToLower().Contains("college"))
            {
                student.CollegeYearLevel = (student.CollegeYearLevel >= 1 && student.CollegeYearLevel <= 4)
                                            ? student.CollegeYearLevel
                                            : 1;
            }
            else
            {
                student.CollegeYearLevel = 0;
            }

            // Handle password
            if (!string.IsNullOrEmpty(studentDto.Password))
            {
                student.Password = PasswordHelper.HashPassword(studentDto.Password);
            }
            else
            {
                student.Password = existingStudent.Password;
            }

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Student not found",
                        Data = null
                    });
                }
                else
                {
                    throw;
                }
            }

            // Reload to get navigation properties
            await _context.Entry(student).Reference(s => s.Level).LoadAsync();
            await _context.Entry(student).Reference(s => s.Section).LoadAsync();
            await _context.Entry(student).Reference(s => s.Role).LoadAsync();

            var data = new
            {
                studentId = student.StudentId,
                fullName = student.FullName,
                email = student.Email,
                levelId = student.LevelId,
                levelName = student.Level?.LevelName,
                sectionId = student.SectionId,
                sectionName = student.Section?.SectionName,
                collegeYearLevel = student.CollegeYearLevel,
                roleId = student.RoleId,
                roleName = student.Role?.Name
            };

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Student updated successfully",
                Data = data
            };
        }

        // DELETE: api/StudentsApi/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteStudent(int id)
        {
            var student = await _context.Student.FindAsync(id);
            if (student == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Student not found",
                    Data = null
                });
            }

            try
            {
                _context.Student.Remove(student);
                await _context.SaveChangesAsync();

                return new ApiResponse<string>
                {
                    Success = true,
                    Message = "Student deleted successfully",
                    Data = "Deleted"
                };
            }
            catch (DbUpdateException)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Cannot delete student. Student may have enrollments or evaluations.",
                    Data = null
                });
            }
        }

        private bool StudentExists(int id)
        {
            return _context.Student.Any(e => e.StudentId == id);
        }
    }

    // DTO for Student
    public class StudentDto
    {
        public int StudentId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int LevelId { get; set; }
        public int? SectionId { get; set; }
        public int? CollegeYearLevel { get; set; }
    }
}