using AutoMapper;
using IdentityModel.Client;
using IRecharge_API.DAL;
using IRecharge_API.DTO;
using IRecharge_API.Entities;
using IRechargedAPI.BLL.AuthService;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace IRecharge_API.BLL.AuthService
{
    public class AuthManager : IAuthManager
    {
        private readonly APIResponse<object> _response = new APIResponse<object>();
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthManager> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IRechargeDbContext _rechargeDbContext;

        public AuthManager(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<AuthManager> logger,
            IUserRepository userRepository,
            IRechargeDbContext rechargeDbContext
           

        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
            _userRepository = userRepository;
            _rechargeDbContext = rechargeDbContext;
        }



        public async Task<APIResponse<object>> Register(RegisterUserDTO registerUserDTO)
        {
            var response = new APIResponse<object>
            {
                ErrorMessages = new List<string>()
            };

            // Use the DbContext to begin transaction
            var transaction = await _rechargeDbContext.Database.BeginTransactionAsync();

            try
            {

                if (registerUserDTO == null)
                {
                    _logger.LogWarning("RegisterUserDTO is null in Register method");
                    response.ErrorMessages.Add("Registration data is required");
                    return response;
                }

                //Validation Check for DTO properties 
                var validationResults = new List<ValidationResult>();
                var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(registerUserDTO);
                if (!Validator.TryValidateObject(registerUserDTO, validationContext, validationResults, true))
                {
                    _logger.LogWarning($"Validation failed for user registration: {string.Join(", ", validationResults.Select(v => v.ErrorMessage))}");
                    response.IsSuccess = false;
                    response.ErrorMessages.AddRange(validationResults.Select(v => v.ErrorMessage));
                    return response;
                }



                _logger.LogInformation($"Starting registration process for email: {registerUserDTO.Email}");

                // Frist transform the email into a standard format using normalization and Check if user exists
                var normalizedEmail = _userManager.NormalizeEmail(registerUserDTO.Email);
                var userExist = await _userManager.FindByEmailAsync(normalizedEmail);
                if (userExist != null)
                {
                    _logger.LogWarning($"Registration attempt with existing email: {registerUserDTO.Email}");
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("User already exists with this email");
                    return response;
                }



                // Validate password for the user been created 
                var PasswordValidation = IsValidPassword(registerUserDTO.Password);
                if (!string.IsNullOrEmpty(PasswordValidation))
                {
                    _logger.LogInformation($"Password does not meet requirements for email {registerUserDTO.Password}");
                    response.IsSuccess = false;
                    response.ErrorMessages.Add(PasswordValidation);
                    return response;
                }

                var identityUser = new IdentityUser
                {
                    UserName = registerUserDTO.UserName,
                    Email = registerUserDTO.Email,
                    EmailConfirmed = false // require email confirmation that why isset to false until confirmed 
                };


                // Create user with password this will properly hash the user password
                _logger.LogInformation($"Creating identity user for {registerUserDTO.Email}");
                var creationResult = await _userManager.CreateAsync(identityUser, registerUserDTO.Password);
                if (!creationResult.Succeeded)
                {
                    _logger.LogError($"Failed to create user {registerUserDTO.Email}. Errors: {string.Join(", ", creationResult.Errors.Select(e => e.Description))}");
                    await transaction.RollbackAsync();
                    response.IsSuccess = false;
                    response.ErrorMessages.AddRange(creationResult.Errors.Select(e => e.Description));
                    return response;
                }


                // Check if role exists  and Handle role (normalize role name)
                var normalizedRoleName = registerUserDTO.role?.Trim().ToUpper();
                if (string.IsNullOrWhiteSpace(normalizedRoleName))
                {
                    normalizedRoleName = "USER"; // Default role
                }

                _logger.LogInformation($"Processing role {normalizedRoleName} for user {registerUserDTO.Email}");
                var roleExist = await _roleManager.RoleExistsAsync(normalizedRoleName);
                if (!roleExist)
                {
                    _logger.LogInformation($"Creating new role: {normalizedRoleName}");
                    var roleCreationResult = await _roleManager.CreateAsync(new IdentityRole(normalizedRoleName));
                    if (!roleCreationResult.Succeeded)
                    {
                        _logger.LogError($"Failed to create role {normalizedRoleName} for {registerUserDTO.UserName}");
                        await transaction.RollbackAsync();
                        response.IsSuccess = false;
                        response.ErrorMessages.Add("Failed to create role, Please try again");
                        response.ErrorMessages.AddRange(roleCreationResult.Errors.Select(e => e.Description));
                        return response;
                    }
                }



                // Add user to role
                var addToRoleResult = await _userManager.AddToRoleAsync(identityUser, normalizedRoleName);
                if (!addToRoleResult.Succeeded)
                {
                    _logger.LogError($"Failed to add user {registerUserDTO.Email} to role {normalizedRoleName}");
                    await transaction.RollbackAsync();
                    response.IsSuccess = false;
                    response.ErrorMessages.AddRange(addToRoleResult.Errors.Select(e => e.Description));
                    return response;
                }

                // Map RegisterUserDto to domain entities and Create application user record
                _logger.LogInformation($"Creating application record for {registerUserDTO.Email}");
                var userRecord = _mapper.Map<User>(registerUserDTO);
                userRecord.IdentityUserId = identityUser.Id;

                try
                {
                    _userRepository.SaveChange(userRecord);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to save user record for {registerUserDTO.Email}");
                    await transaction.RollbackAsync();
                    response.IsSuccess = false;
                    response.ErrorMessages.Add("Failed to complete registration. Please try again.");
                    return response;
                }

                var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);
                // In a real application, you would send this token to the user's email
                _logger.LogInformation($"Email confirmation token generated for {registerUserDTO.Email}");


                // Commit transaction
                await transaction.CommitAsync();
                _logger.LogInformation($"Successfully registered user {registerUserDTO.Email}");

                // Prepare response
                var authResponse = new AuthReponse
                {
                    UserName = registerUserDTO.UserName,
                    Email = registerUserDTO.Email,
                    PhoneNumber = registerUserDTO.PhoneNumber,
                    WalletBalance = registerUserDTO.WalletBalance,
                    EmailConfirmationToken = emailConfirmationToken // Include this if you want to return it
                };

                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error during registration for email: {registerUserDTO?.Email}");
                await transaction.RollbackAsync();
                response.IsSuccess = false;
                response.ErrorMessages.Add("An unexpected error occurred during registration. Please try again.");
                return response;

            }
        }


        public async Task<APIResponse<object>> Login(LoginDto loginDTO)
        {
            var response = new APIResponse<object>
            {
                ErrorMessages = new List<string>()
            };

            _logger.LogInformation("Logging in user with email {Email}", loginDTO.Email);


            // Validate login DTO
            if (loginDTO == null)
            {
                _logger.LogWarning("LoginDto is null in Login method");
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.ErrorMessages.Add("Login data is required");
                return response;
            }


            // Find user by email
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null)
            {
                _logger.LogWarning("User not found with email {Email}", loginDTO.Email);
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages.Add("Invalid email or password.");
                return response;
            }


            // Verify password
            if (!await _userManager.CheckPasswordAsync(user, loginDTO.Password))
            {
                _logger.LogWarning("Invalid password for email {Email}", loginDTO.Email);
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.ErrorMessages.Add("Invalid email or password.");
                return response;
            }

            // Check roles
            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            if (!roles.Any(r => r == "Reader" || r == "Writer"))
            {
                _logger.LogWarning("User does not have the required role for email {Email}", loginDTO.Email);
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Forbidden;
                response.ErrorMessages.Add("User does not have required role.");
                return response;
            }

            // Generate a JWT token
            var jwtResult = await GenerateJwtToken(user, roles);
            if (jwtResult == null)
            {
                _logger.LogError("Error generating token for email {Email}", loginDTO.Email);
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.ErrorMessages.Add("An error occurred during authentication.");
                return response;
            }

            _logger.LogInformation("User logged in successfully with email {Email}", loginDTO.Email);

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Message = "Login successful.";
            response.ExpiresIn = jwtResult.ExpiresIn;
            response.Token = jwtResult.Token;
            //response.UserId = jwtResult.UserId;

            return response;
        }
    


        private string IsValidPassword(string password)
        {
            // Ensure password has at least one uppercase letter, one digit, and one special character
            var regex = new Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+={}\[\]:;'<>?,./\\-]).+$");
            if (!regex.IsMatch(password))
            {
                return "Password must contain at least one uppercase letter, one number, and one special character.";
            }
            return string.Empty; // Valid password
        }


        private async Task<APIResponse<object>> GenerateJwtToken(IdentityUser user, List<string> roles)
        {
            var response = new APIResponse<object>
            {
                ErrorMessages = new List<string>()
            };

            try
            {
                _logger.LogInformation("Generating JWT token for user {UserId}", user.Id);

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var userClaims = await _userManager.GetClaimsAsync(user);
                var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("uid", user.Id),
                }
                .Union(userClaims)
                .Union(roleClaims);

                var tokenDuration = Convert.ToInt32(_configuration["JwtSettings:DurationInMinutes"]);

                var token = new JwtSecurityToken(
                    issuer: _configuration["JwtSettings:Issuer"],
                    audience: _configuration["JwtSettings:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(tokenDuration),
                    signingCredentials: credentials
                );

                _logger.LogInformation($"JWT token generated successfully for user {user.Id}");

                response.Token = new JwtSecurityTokenHandler().WriteToken(token);
                response.ExpiresIn = token.ValidTo;
                //response.UserId = user.Id;
                response.IsSuccess = true;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for user {UserId}", user.Id);
                response.IsSuccess = false;
                response.ErrorMessages.Add("An error occurred while generating the token.");
                return response;
            }
        }
    }
}
