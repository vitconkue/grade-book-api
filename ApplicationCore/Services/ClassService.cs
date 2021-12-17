using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Entity;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Services
{
    public class ClassService : IClassService
    {
        private readonly IBaseRepository<Assignment> _assignmentRepository;
        private readonly IBaseRepository<Class> _classRepository;
        private readonly IBaseRepository<ClassStudentsAccount> _classStudentsRepository;
        private readonly IBaseRepository<ClassTeachersAccount> _classTeacherRepository;
        private readonly ILogger<ClassService> _logger;
        private readonly IBaseRepository<User> _userRepository;

        public ClassService(ILogger<ClassService> logger,
            IBaseRepository<Class> classRepository,
            IBaseRepository<User> userRepository,
            IBaseRepository<ClassStudentsAccount> classStudentsRepository,
            IBaseRepository<ClassTeachersAccount> classTeacherRepository,
            IBaseRepository<Assignment> assignmentRepository
            )
        {
            _logger = logger;
            _classRepository = classRepository;
            _userRepository = userRepository;
            _classStudentsRepository = classStudentsRepository;
            _classTeacherRepository = classTeacherRepository;
            _assignmentRepository = assignmentRepository;
        }

        public Class GetClassDetail(int classId)
        {
            var foundClass = _classRepository.GetFirst(cl => cl.Id == classId,
                cl => cl.Include(c => c.MainTeacher)
                    .Include(c => c.ClassStudentsAccounts).ThenInclude(cs => cs.Student)
                    .Include(c => c.ClassTeachersAccounts).ThenInclude(ct => ct.Teacher)
                    .Include(c => c.ClassAssignments));
            if (foundClass is null)
                return null;
            foundClass.ClassAssignments = foundClass.ClassAssignments.OrderBy(a => a.Priority)
                .ThenByDescending(assignment => assignment.Id)
                .ToList();

            return foundClass;
        }

        public List<Class> GetAllClassWithUserBeingMainTeacher(int userId)
        {
            var foundUser = _userRepository.GetFirst(user => user.Id == userId);
            if (foundUser is null)
                throw new ApplicationException("User does not exists");

            var mainTeacherClasses =
                _classRepository.List(cl => cl.MainTeacher == foundUser, null, "MainTeacher");
            return mainTeacherClasses.ToList();
        }

        public List<Class> GetAllClassWithUserBeingSubTeacher(int userId)
        {
            var foundUser =
                _userRepository.GetFirst(user => user.Id == userId,
                    user => user.Include(u => u.ClassTeachersAccounts).ThenInclude(c => c.Class)
                        .ThenInclude(cl => cl.MainTeacher));


            if (foundUser is null)
                throw new ApplicationException("User does not exists");
            return foundUser.ClassTeachersAccounts.Select(ct => ct.Class).ToList();
        }

        public List<Class> GetAllClassWithUserBeingStudent(int userId)
        {
            var foundUser =
                _userRepository.GetFirst(user => user.Id == userId,
                    user => user.Include(u => u.ClassStudentsAccounts).ThenInclude(c => c.Class)
                        .ThenInclude(cl => cl.MainTeacher));


            if (foundUser is null)
                throw new ApplicationException("User does not exists");
            return foundUser.ClassStudentsAccounts.Select(ct => ct.Class).ToList();
        }

        public Class AddNewClass(string name, DateTime startDate, string room, string description, int mainTeacherId)
        {
            var foundUser = _userRepository.GetFirst(user => user.Id == mainTeacherId);
            if (foundUser is null)
                throw new ApplicationException("User does not exists");
            var newClass = new Class
            {
                Name = name,
                StartDate = startDate,
                Room = room,
                Description = description,
                MainTeacher = foundUser
            };
            newClass.InviteStringStudent = GenerateRandomLetterString();
            newClass.InviteStringTeacher = GenerateRandomLetterString();

            _classRepository.Insert(newClass);
            return newClass;
        }

        public void AddStudentAccountToClass(int classId, int studentId)
        {
            var foundUser = GetUserWithClassInformation(studentId);
            if (foundUser is null)
                throw new ApplicationException("User does not exist");
            // check user already a member in class. 
            var availableClass = TryGetAvailableClass(foundUser, classId);


            var newClassStudentRecord = new ClassStudentsAccount
            {
                Class = availableClass,
                ClassId = availableClass.Id,
                Student = foundUser,
                StudentAccountId = foundUser.Id
            };

            _classStudentsRepository.Insert(newClassStudentRecord);
        }

        public void AddTeacherAccountToClass(int classId, int teacherId)
        {
            var foundUser = GetUserWithClassInformation(teacherId);
            if (foundUser is null)
                throw new ApplicationException("User does not exist");
            // check user already a member in class. 
            var availableClass = TryGetAvailableClass(foundUser, classId);


            var newClassTeacherRecord = new ClassTeachersAccount
            {
                Class = availableClass,
                ClassId = availableClass.Id,
                Teacher = foundUser,
                TeacherId = foundUser.Id
            };
            _classTeacherRepository.Insert(newClassTeacherRecord);
        }

        public List<Assignment> GetClassAssignments(int classId)
        {
            var foundClass = _classRepository.GetFirst(cl => cl.Id == classId,
                cl => cl.Include(c => c.ClassAssignments));

            if (foundClass is null)
                return null;
            return foundClass.ClassAssignments.OrderBy(a => a.Priority)
                .ThenByDescending(assignment => assignment.Id)
                .ToList();
        }

        public Assignment AddNewClassAssignment(int classId, string name, int point)
        {
            var foundClass = GetClassDetail(classId);
            var newAssignment = new Assignment {Name = name, Weight = point, Priority = 0, Class = foundClass};


            _assignmentRepository.Insert(newAssignment);

            return newAssignment;
        }

        public bool RemoveAssignment(int assignmentId)
        {
            var foundAssignment = _assignmentRepository.GetFirst(assignment => assignment.Id == assignmentId);
            if (foundAssignment is null)
                return false;
            _assignmentRepository.Delete(foundAssignment);
            return true;
        }

        public Assignment UpdateClassAssignment(int assignmentId, string newName, int newPoint)
        {
            var foundAssignment = _assignmentRepository.GetFirst(assignment => assignment.Id == assignmentId,
                assignment
                    => assignment.Include(a => a.Class));
            if (foundAssignment is null)
                return null;
            foundAssignment.Name = newName;
            foundAssignment.Weight = newPoint;
            _assignmentRepository.Update(foundAssignment);

            return foundAssignment;
        }

        public List<Assignment> UpdateClassAssignmentPriority(int classId, List<int> newOrder)
        {
            var foundClass = GetClassDetail(classId);
            var currentClassAssignments = foundClass.ClassAssignments;

            foreach (var assignment in currentClassAssignments)
            {
                var indexInNewOrder = newOrder.IndexOf(assignment.Id);
                if (indexInNewOrder > -1) assignment.Priority = (indexInNewOrder + 1) * 100;
            }
            
            // TODO: refactor here 

            _classRepository.Update(foundClass);
            return currentClassAssignments.OrderBy(assignment => assignment.Priority)
                .ThenByDescending(assignment => assignment.Id)
                .ToList();
        }

        public List<StudentAssignmentGrade> GetStudentAssignmentGradeAsTeacher(int assignmentId)
        {
            var foundAssignment = _assignmentRepository.GetFirst(a => a.Id == assignmentId,
                a =>
                    a.Include(ass => ass.StudentAssignmentGrades)
                        .ThenInclude(sGrade => sGrade.StudentRecord));

            if (foundAssignment is null)
                return null;
            return foundAssignment.StudentAssignmentGrades.ToList();
        }

        public List<StudentAssignmentGrade> GetStudentAssignmentGradeAsStudent(int assignmentId)
        {
            throw new NotImplementedException();
        }

        public List<StudentRecord> BulkAddStudentToClass(int classId, List<Tuple<string,string>> idNamePairs)
        {
            var foundClass = _classRepository
                .GetFirst(cl => cl.Id == classId, cl => cl.Include(c => c.Students));
            if (foundClass is null)
                return null; 
            foundClass.AddStudents(idNamePairs);
            _classRepository.Update(foundClass);
            return foundClass.Students.ToList();
        }

        public List<StudentAssignmentGrade> BulkAddStudentGradeToAssignment(int assignmentId, List<Tuple<string, int>> idGradePairs)
        {
            var foundAssignment = _assignmentRepository.GetFirst(a => a.Id == assignmentId,
                a => 
                    a.Include(ass => ass.StudentAssignmentGrades)
                        .Include(ass => ass.Class)
                        .ThenInclude(c => c.Students));
            if (foundAssignment is null)
                return null; 
            foundAssignment.AddStudentGrades(idGradePairs);
            _assignmentRepository.Update(foundAssignment);
            return foundAssignment.StudentAssignmentGrades.ToList();
        }

        public List<Assignment> GetAllClassAssignmentWithGradeAsTeacher(int classId)
        {
            var foundClass = _classRepository.GetFirst(cl => cl.Id == classId,
                cl => cl.Include(c => c.ClassAssignments)
                    .ThenInclude(a => a.StudentAssignmentGrades));

            if (foundClass is null)
                return null;
            return foundClass.ClassAssignments.OrderBy(a => a.Priority)
                .ThenByDescending(assignment => assignment.Id)
                .ToList();
        }

        public List<Assignment> GetAllClassAssignmentWithGradeAsStudent(int classId, int userId)
        {
            var foundUser = _userRepository.GetFirst(u => u.Id == userId);
            if (foundUser is null)
                return null;
            var assignments = _assignmentRepository.List(a => a.Class.Id == classId,
                null, a => a.Include(ass => ass.Class)
                    .Include(ass => ass.StudentAssignmentGrades).ThenInclude(sg => sg.StudentRecord)).ToList();

            foreach (var assignment in assignments)
            {
                var sGrades = assignment.StudentAssignmentGrades.ToList();
                sGrades = sGrades.Where(sg =>
                        sg.IsFinalized && sg.StudentRecord.StudentIdentification == foundUser.StudentIdentification)
                    .ToList();
                assignment.SetStudentAssignmentGrades(sGrades);
            }

            return assignments;

        }


        private User GetUserWithClassInformation(int studentId)
        {
            var foundUser = _userRepository.GetFirst(user => user.Id == studentId,
                user => user.Include(u => u.ClassStudentsAccounts).Include(u => u.ClassTeachersAccounts));
            return foundUser;
        }


        private Class TryGetAvailableClass(User foundUser, int classId)
        {
            var foundClass = _classRepository.GetFirst(cl => cl.Id == classId, cl => cl.Include(c => c.MainTeacher));
            if (foundClass is null)
                throw new ApplicationException("Class does not exists");
            if (foundClass.MainTeacher == foundUser)
                throw new ApplicationException("User is currently the main teacher of this class");


            if (foundUser.ClassStudentsAccounts.FirstOrDefault(c => c.ClassId == classId) is not null)
                throw new ApplicationException("User already a student in class");

            if (foundUser.ClassTeachersAccounts.FirstOrDefault(c => c.ClassId == classId) is not null)
                throw new ApplicationException("User is currently a teacher in class");
            return foundClass;
        }

        private string GenerateRandomLetterString()
        {
            var resultGuid = string.Concat(Guid.NewGuid().ToString().Select(c => (char) (c + 17)));
            return resultGuid.Substring(resultGuid.Length - 12);
        }
    }
}