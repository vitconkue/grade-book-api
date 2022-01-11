using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApplicationCore.Entity;
using ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;

namespace ApplicationCore.Services
{
    public class UserJwtAuthService : IUserJwtAuthService
    {
        private readonly ILogger<UserJwtAuthService> _logger;
        private readonly IBaseRepository<User> _repository;
        private readonly IBaseRepository<AccountConfirmationRequest> _confirmationRepository;

        public UserJwtAuthService(
            ILogger<UserJwtAuthService> logger,
            IBaseRepository<User> repository, IBaseRepository<AccountConfirmationRequest> confirmationRepository)
        {
            _logger = logger;
            _repository = repository;
            _confirmationRepository = confirmationRepository;
        }

        public string TryGetToken(string email, string password)
        {
            var foundUser =
                _repository.GetFirst(user => user.Email == email);

            if (foundUser is null)
                return null;
            var success = PasswordHelper
                .CheckPasswordHash(password, foundUser.PasswordHash, foundUser.PasswordSalt);

            if (!success)
                return null;

            return GenerateJwtToken(foundUser);
        }

        public string TryGetTokenWithoutPassword(string email)
        {
            var foundUser =
                _repository.GetFirst(user => user.Email == email);
            if (foundUser is null)
                return null;
            return GenerateJwtToken(foundUser);
        }

        public User CreateNewUser(string password, string email, string firstName, string lastName,
            string profilePictureUrl, string defaultProfilePictureHex)
        {
            var foundUser = _repository.GetFirst(user => user.Email == email);

            if (foundUser is not null) throw new ApplicationException("Existed user");
            var userToAdd = new User(email, firstName, lastName, password, profilePictureUrl, defaultProfilePictureHex);
            _repository.Insert(userToAdd);

            return userToAdd;
        }

        public AccountConfirmationRequest CreateNewConfirmationRequest(string email)
        {
            var foundUser = _repository.GetFirst(user => user.Email == email);

            var newConfirmationRequest = new AccountConfirmationRequest(foundUser);

            _confirmationRepository.Insert(newConfirmationRequest);

            return newConfirmationRequest;

        }


        private string GenerateJwtToken(User user)
        {
            var hourToRefresher = 48;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("3aacfb02-b67b-4923-8a2d-21a103902b91"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("ID", user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddHours(hourToRefresher)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }
    }
}