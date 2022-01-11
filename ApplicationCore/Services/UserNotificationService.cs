using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Entity;
using ApplicationCore.Interfaces;
using ApplicationCore.Specifications;
using Microsoft.EntityFrameworkCore;

namespace ApplicationCore.Services
{
    public class UserNotificationService : IUserNotificationService
    {
        private readonly IBaseRepository<Assignment> _assignmentRepository;
        private readonly IBaseRepository<UserNotification> _notificationRepository;
        private readonly IBaseRepository<User> _userRepository;
        private readonly IBaseRepository<AssignmentGradeReviewRequest> _reviewRepository;
        private readonly IBaseRepository<Class> _classRepository;


        public UserNotificationService(IBaseRepository<Assignment> assignmentRepository,
            IBaseRepository<UserNotification> notificationRepository, IBaseRepository<User> userRepository,
            IBaseRepository<AssignmentGradeReviewRequest> reviewRepository, IBaseRepository<Class> classRepository)
        {
            _assignmentRepository = assignmentRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _reviewRepository = reviewRepository;
            _classRepository = classRepository;
        }

        public List<UserNotification> ReadPagedUserNotification(int userId, int pageNumber,
            int numOfNotificationPerPage)
        {
            var listNotification = _notificationRepository
                .List(new UserNotificationWithUserIdAndPagingSpec(userId, numOfNotificationPerPage, pageNumber));

            return listNotification.ToList();
        }

        public void SetUserNotificationAsViewed(int userId)
        {
            var foundUser =
                _userRepository
                    .GetFirst(u => u.Id == userId,
                        u => u.Include(user => user.UserNotifications));
            foundUser.SetAllNotificationRead(true);
            _userRepository.Update(foundUser);
        }

        public int CountNotViewedNotification(int userId)
        {
            return _notificationRepository.Count(n => !n.IsViewed && n.UserId == userId);
        }

        public void AddNewFinalizedGradeCompositionNotification(int assignmentId)
        {
            var foundAssignment = _assignmentRepository.GetFirst(a => a.Id == assignmentId,
                a => a.Include(ass => ass.Class)
                    .ThenInclude(c => c.ClassStudentsAccounts)
                    .ThenInclude(cs => cs.Student)
            );

            var studentAccounts = foundAssignment.Class.ClassStudentsAccounts;

            var newNotifications = (
                from studentAccount in studentAccounts // to all student account in class 
                select studentAccount.Student
                into user
                where !string.IsNullOrEmpty(user.StudentIdentification)
                select new UserNotification
                {
                    NotificationType = NotificationType.NewFinalizedGradeComposition,
                    Assignment = foundAssignment,
                    AssignmentId = foundAssignment.Id,
                    Class = foundAssignment.Class,
                    ClassId = foundAssignment.Class.Id,
                    AssignmentGradeReviewRequest = null,
                    User = user,
                    UserId = user.Id,
                    DateTime = DateTime.Now,
                    IsViewed = false
                }).ToList();

            _notificationRepository.InsertRange(newNotifications);
        }

        public void AddNewGradeRequestNotification(int requestId)
        {
            var foundRequest = _reviewRepository.GetFirst(r => r.Id == requestId,
                r =>
                    r.Include(re => re.StudentAssignmentGrade)
                        .ThenInclude(sGrade => sGrade.Assignment)
                        .ThenInclude(a => a.Class)
            );

            var classOfRequest =
                _classRepository.GetFirst(c => c.Id == foundRequest.StudentAssignmentGrade.Assignment.Class.Id,
                    c => c.Include(cl => cl.MainTeacher)
                        .Include(cl => cl.ClassTeachersAccounts)
                        .ThenInclude(ct => ct.Teacher));

            var teachersList = classOfRequest.GetAllTeacher();

            var newNotifications = teachersList.Select(teacherAccount => new UserNotification
                {
                    NotificationType = NotificationType.NewGradeReviewRequest,
                    Assignment = foundRequest.StudentAssignmentGrade.Assignment,
                    AssignmentId = foundRequest.StudentAssignmentGrade.Assignment.Id,
                    Class = classOfRequest,
                    ClassId = classOfRequest.Id,
                    AssignmentGradeReviewRequest = foundRequest,
                    User = teacherAccount,
                    UserId = teacherAccount.Id
                })
                .ToList();

            _notificationRepository.InsertRange(newNotifications);
        }

        public void AddNewGradeReviewReplyNotification(int replyId)
        {
            throw new NotImplementedException();
        }

        public void AddAcceptedOrRejectedGradeReviewNotification(int requestId)
        {
            throw new NotImplementedException();
        }
    }
}