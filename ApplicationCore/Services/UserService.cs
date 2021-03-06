using System;
using System.IO;
using System.Linq;
using ApplicationCore.Entity;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace ApplicationCore.Services
{
    public class UserService : IUserServices
    {
        private readonly IBaseRepository<Class> _classRepository;
        private readonly ICloudPhotoHandler _cloudPhotoHandler;
        private readonly IBaseRepository<User> _userRepository;
        private readonly IBaseRepository<AdminAccount> _adminAccountRepository;

        public UserService(IBaseRepository<User> userRepository,
            ICloudPhotoHandler cloudPhotoHandler, IBaseRepository<Class> classRepository,
            IBaseRepository<AdminAccount> adminAccountRepository)
        {
            _userRepository = userRepository;
            _cloudPhotoHandler = cloudPhotoHandler;
            _classRepository = classRepository;
            _adminAccountRepository = adminAccountRepository;
        }

        public User GetUserById(int id)
        {
            var found = _userRepository.GetFirst(user =>
                user.Id == id);

            if (found is null)
                return null;

            return found;
        }

        public User GetUserByEmail(string email)
        {
            var found = _userRepository.GetFirst(user =>
                user.Email == email);

            if (found is null)
                return null;

            return found;
        }

        public User GetUserByStudentIdentification(string studentIdentification)
        {
            var found = _userRepository.GetFirst(user => user.StudentIdentification == studentIdentification);
            if (found is null) return null;
            return found;
        }

        public ClassRole GetUserRoleInClass(int userId, int classId)
        {
            var foundClass =
                _classRepository
                    .GetFirst(cl => cl.Id == classId,
                        cl => cl.Include(c => c.MainTeacher));
            var foundUser = _userRepository.GetFirst(user => user.Id == userId,
                user => user.Include(u => u.ClassStudentsAccounts)
                    .Include(u => u.ClassTeachersAccounts));
            if (foundUser is null)
                return ClassRole.NotAMember;

            if (foundClass.MainTeacher == foundUser) return ClassRole.Teacher;

            if (foundUser.ClassTeachersAccounts.FirstOrDefault(c => c.ClassId == classId) is not null)
                return ClassRole.Teacher;
            if (foundUser.ClassStudentsAccounts.FirstOrDefault(c => c.ClassId == classId) is not null)
                return ClassRole.Student;
            return ClassRole.NotAMember;
        }

        public bool IsUserAdmin(int adminId)
        {
            var foundAdmin = _adminAccountRepository.GetFirst(a => a.Id == adminId);

            return (foundAdmin is not null);
        }

        public bool IsUserSuperAdmin(int adminId)
        {
            var foundAdmin = _adminAccountRepository.GetFirst(a => a.Id == adminId);

            return (foundAdmin is not null && foundAdmin.IsSuperAdmin);
        }


        public User UpdateUser(int id, string newFirstname, string newLastname, string newStudentIdentification,
            string newEmail)
        {
            var found = _userRepository.GetFirst(user => user.Id == id);
            if (found is null) return null;

            if (!string.IsNullOrEmpty(newFirstname)) found.FirstName = newFirstname;
            if (!string.IsNullOrEmpty(newLastname)) found.LastName = newLastname;

            if (!string.IsNullOrEmpty(newStudentIdentification))
            {
                var foundExistedStudent =
                    _userRepository.GetFirst(user => user.StudentIdentification == newStudentIdentification);
                if (foundExistedStudent is not null)
                    throw new ApplicationException(
                        $"Student with student Id {newStudentIdentification} already exists");

                found.StudentIdentification = newStudentIdentification;
            }

            if (!string.IsNullOrEmpty(newEmail))
            {
                var foundExistedStudent = _userRepository.GetFirst(user => user.Email == newEmail);
                if (foundExistedStudent is not null)
                    throw new ApplicationException($"User with {newEmail} email already exists");
                found.Email = newEmail;
                found.IsEmailConfirmed = false;
            }


            var result = _userRepository.Update(found);
            return result;
        }

        public User UpdateUserPassword(int userId, string oldPassword, string newPassword)
        {
            var found = _userRepository.GetFirst(user => user.Id == userId);
            if (found is null)
                throw new ApplicationException("User does not exist");

            if (!found.IsPasswordNotSet)
            {
                var validOldPassword =
                    PasswordHelper.CheckPasswordHash(oldPassword, found.PasswordHash, found.PasswordSalt);
                if (!validOldPassword)
                    return null;
            }

            PasswordHelper.HashPassword(newPassword, out var newSalt, out var newHash);
            found.PasswordHash = newHash;
            found.PasswordSalt = newSalt;
            found.IsPasswordNotSet = false;

            _ = _userRepository.Update(found);
            return found;
        }

        public User UpdateUserPassword(int userId, string newPassword)
        {
            var found = _userRepository.GetFirst(user => user.Id == userId);
            if (found is null)
                throw new ApplicationException("User does not exist");


            PasswordHelper.HashPassword(newPassword, out var newSalt, out var newHash);
            found.PasswordHash = newHash;
            found.PasswordSalt = newSalt;
            found.IsPasswordNotSet = false;

            _ = _userRepository.Update(found);
            return found;
        }

        public string UpdateUserAvatar(int id, Stream newPicture)
        {
            var found = _userRepository.GetFirst(user => user.Id == id);
            if (found is null)
                return null;
            var resultUrl = _cloudPhotoHandler.Upload(newPicture);

            if (string.IsNullOrEmpty(resultUrl)) return null;

            found.ProfilePictureUrl = resultUrl;
            _userRepository.Update(found);

            return resultUrl;
        }
    }
}