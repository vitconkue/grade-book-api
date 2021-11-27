using System;
using System.Linq;
using ApplicationCore.Interfaces;
using grade_book_api.Requests.ClassRequests;
using grade_book_api.Responses.Class;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace grade_book_api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;
        private readonly ILogger<ClassController> _logger;
        private readonly IUserServices _userServices;

        public ClassController(ILogger<ClassController> logger, IClassService classService, IUserServices userServices)
        {
            _logger = logger;
            _classService = classService;
            _userServices = userServices;
        }

        [HttpGet]
        [Route("{classId}")]
        public IActionResult GetClassDetail(int classId)
        {
            var userId = int.Parse(HttpContext.User.Claims.First(c => c.Type == "ID").Value);
            try
            {
                var foundClass = _classService.GetClassDetail(classId);
                var userRoleInClass = _userServices.GetUserRoleInClass(userId, classId);
                if (userRoleInClass == 0) return BadRequest("User not a member in class");
                var response = new ClassDetailInformationResponse(foundClass, userRoleInClass == 1);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetClassList()
        {
            var userId = int.Parse(HttpContext.User.Claims.First(c => c.Type == "ID").Value);
            try
            {
                var mainTeacherClasses = _classService.GetAllClassWithUserBeingMainTeacher(userId);
                var subTeacherClasses = _classService.GetAllClassWithUserBeingSubTeacher(userId);
                var studentClasses = _classService.GetAllClassWithUserBeingStudent(userId);
                var classesResponse = mainTeacherClasses
                    .Select(cl => new ClassShortInformationResponse(cl, "teacher", cl.MainTeacher)).ToList();
                classesResponse.AddRange(subTeacherClasses.Select(cl =>
                    new ClassShortInformationResponse(cl, "subteacher", cl.MainTeacher)));
                classesResponse.AddRange(studentClasses.Select(cl =>
                    new ClassShortInformationResponse(cl, "student", cl.MainTeacher)));

                return Ok(classesResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        public IActionResult AddNewClass(AddNewClassRequest request)
        {
            var userId = int.Parse(HttpContext.User.Claims.First(c => c.Type == "ID").Value);

            // create new class

            var newAddClass = _classService.AddNewClass(request.Name, request.StartDate, request.Room,
                request.Description, userId);

            return Ok(new ClassShortInformationResponse(newAddClass, "teacher", newAddClass.MainTeacher));
        }

        [HttpGet("{classId}/gradestructure")]
        public IActionResult GetClassGradeStructure(int classId)
        {
            return Ok();
        }

        [HttpPost]
        [Route("student")]
        public IActionResult AddStudentToClass([FromBody] AddStudentToClassRequest request)
        {
            try
            {
                _classService.AddStudentToClass(request.ClassId, request.StudentId);
                return Ok();
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("teacher")]
        public IActionResult AddTeacherToClass([FromBody] AddTeacherToClassRequest request)
        {
            try
            {
                _classService.AddTeacherToClass(request.ClassId, request.TeacherId);
                return Ok();
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}