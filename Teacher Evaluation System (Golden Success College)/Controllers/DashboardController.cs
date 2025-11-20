using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Teacher_Evaluation_System__Golden_Success_College_.Data;

namespace Teacher_Evaluation_System__Golden_Success_College_.Controllers
{
    public class DashboardController : Controller
    {
        private readonly Teacher_Evaluation_System__Golden_Success_College_Context _context;

        public DashboardController(Teacher_Evaluation_System__Golden_Success_College_Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity!.IsAuthenticated)
                return RedirectToAction("Login", "Auth");

            string role = User.FindFirst("RoleName")?.Value ?? "";

            // =============== ADMIN & SUPER ADMIN ==================
            if (role == "Admin" || role == "Super Admin")
            {
                ViewBag.TotalTeachers = await _context.Teacher.CountAsync();
                ViewBag.TotalStudents = await _context.Student.CountAsync();
                ViewBag.TotalEvaluations = await _context.Evaluation.CountAsync();

                return View();   // Model not needed for admin
            }

            // ====================== STUDENT =======================
            if (role == "Student")
            {
                // Get logged-in student ID
                string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdString, out int studentId))
                    return RedirectToAction("AccessDenied", "Auth");

                // Get teacher IDs assigned to this student's enrollments
                var enrolledTeacherIds = await _context.Enrollment
                                                       .Where(e => e.StudentId == studentId)
                                                       .Select(e => e.TeacherId)
                                                       .ToListAsync();

                // Get teachers not yet evaluated by this student
                var evaluatedTeacherIds = await _context.Evaluation
                                                        .Where(e => e.StudentId == studentId)
                                                        .Select(e => e.TeacherId)
                                                        .ToListAsync();

                var teachersToEvaluate = await _context.Teacher
                                                       .Where(t => enrolledTeacherIds.Contains(t.TeacherId)) // only enrolled teachers
                                                       .Where(t => !evaluatedTeacherIds.Contains(t.TeacherId)) // not yet evaluated
                                                       .Include(t => t.Level)
                                                       .ToListAsync();

                return View(teachersToEvaluate);

            }

            // ========== UNKNOWN ROLE → DENIED ACCESS ===============
            return RedirectToAction("AccessDenied", "Auth");
        }
    }
}
