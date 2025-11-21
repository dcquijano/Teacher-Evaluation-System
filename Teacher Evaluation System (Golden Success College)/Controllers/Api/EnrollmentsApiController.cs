using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Teacher_Evaluation_System__Golden_Success_College_.Data;
using Teacher_Evaluation_System__Golden_Success_College_.Models;
using Teacher_Evaluation_System__Golden_Success_College_.ViewModels;

namespace Teacher_Evaluation_System__Golden_Success_College_.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsApiController : ControllerBase
    {
        private readonly Teacher_Evaluation_System__Golden_Success_College_Context _context;

        public EnrollmentsApiController(Teacher_Evaluation_System__Golden_Success_College_Context context)
        {
            _context = context;
        }

        // GET: api/EnrollmentsApi
        [HttpGet]
        public async Task<IActionResult> GetEnrollments()
        {
            var enrollments = await _context.Enrollment
                .Include(e => e.Student)
                .Include(e => e.Subject)
                .Include(e => e.Teacher)
                .ToListAsync();

            var data = enrollments.Select(e => new
            {
                enrollmentId = e.EnrollmentId,
                studentId = e.StudentId,
                studentName = e.Student?.FullName,
                subjectId = e.SubjectId,
                subjectName = e.Subject?.SubjectName,
                teacherId = e.TeacherId,
                teacherName = e.Teacher?.FullName
            });

            return Ok(new
            {
                success = true,
                data = data
            });
        }

        // GET: api/EnrollmentsApi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEnrollment(int id)
        {
            var enrollment = await _context.Enrollment
                .Include(e => e.Student)
                .Include(e => e.Subject)
                .Include(e => e.Teacher)
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            if (enrollment == null)
                return NotFound(new { success = false, message = "Enrollment not found" });

            return Ok(new
            {
                success = true,
                data = new
                {
                    enrollmentId = enrollment.EnrollmentId,
                    studentId = enrollment.StudentId,
                    studentName = enrollment.Student?.FullName,
                    subjectId = enrollment.SubjectId,
                    subjectName = enrollment.Subject?.SubjectName,
                    teacherId = enrollment.TeacherId,
                    teacherName = enrollment.Teacher?.FullName
                }
            });
        }

        // POST: api/EnrollmentsApi
        [HttpPost]
        public async Task<IActionResult> PostEnrollment([FromBody] EnrollmentCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data" });

            var createdEnrollments = new List<Enrollment>();

            foreach (var subjectId in model.SubjectIds)
            {
                var subject = await _context.Subject.FindAsync(subjectId);
                if (subject == null) continue;

                // Prevent duplicate enrollment
                if (_context.Enrollment.Any(e => e.StudentId == model.StudentId && e.SubjectId == subjectId))
                    continue;

                var enrollment = new Enrollment
                {
                    StudentId = model.StudentId,
                    SubjectId = subjectId,
                    TeacherId = subject.TeacherId
                };

                _context.Enrollment.Add(enrollment);
                createdEnrollments.Add(enrollment);
            }

            await _context.SaveChangesAsync();

            var data = createdEnrollments.Select(e => new
            {
                enrollmentId = e.EnrollmentId,
                studentId = e.StudentId,
                studentName = _context.Student.FirstOrDefault(s => s.StudentId == e.StudentId)?.FullName,
                subjectId = e.SubjectId,
                subjectName = _context.Subject.FirstOrDefault(s => s.SubjectId == e.SubjectId)?.SubjectName,
                teacherId = e.TeacherId,
                teacherName = _context.Teacher.FirstOrDefault(t => t.TeacherId == e.TeacherId)?.FullName
            });

            return Ok(new
            {
                success = true,
                message = "Enrollment(s) created successfully",
                data = data
            });
        }

        // PUT: api/EnrollmentsApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEnrollment(int id, Enrollment enrollment)
        {
            if (id != enrollment.EnrollmentId)
                return BadRequest(new { success = false, message = "Enrollment ID mismatch" });

            var existing = await _context.Enrollment.FindAsync(id);
            if (existing == null)
                return NotFound(new { success = false, message = "Enrollment not found" });

            existing.StudentId = enrollment.StudentId;
            existing.SubjectId = enrollment.SubjectId;
            existing.TeacherId = enrollment.TeacherId;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Enrollment updated successfully",
                data = new
                {
                    enrollmentId = existing.EnrollmentId,
                    studentId = existing.StudentId,
                    studentName = _context.Student.FirstOrDefault(s => s.StudentId == existing.StudentId)?.FullName,
                    subjectId = existing.SubjectId,
                    subjectName = _context.Subject.FirstOrDefault(s => s.SubjectId == existing.SubjectId)?.SubjectName,
                    teacherId = existing.TeacherId,
                    teacherName = _context.Teacher.FirstOrDefault(t => t.TeacherId == existing.TeacherId)?.FullName
                }
            });
        }

        // DELETE: api/EnrollmentsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrollment(int id)
        {
            var enrollment = await _context.Enrollment.FindAsync(id);
            if (enrollment == null)
                return NotFound(new { success = false, message = "Enrollment not found" });

            _context.Enrollment.Remove(enrollment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Enrollment deleted successfully"
            });
        }
    }
}
