using AutoMapper;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Core.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListing.API.Core.Repository
{
    public class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper;
        //contains methods that handle authenticating/authorizing ApiUsers
        private readonly UserManager<ApiUser> _userManager;
        //need to inject this to access key from config file
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthManager> _logger;
        private ApiUser _user;

        private const string _loginProvider = "HotelListingApi";
        private const string _refreshToken = "RefreshToken";

        public AuthManager(IMapper mapper, UserManager<ApiUser> userManager, IConfiguration configuration, ILogger<AuthManager> logger)
        {
            this._mapper = mapper;
            this._userManager = userManager;
            this._configuration = configuration;
            this._logger = logger;
        }

        public async Task<string> CreateRefreshToken()
        {
            //remove any tokens that exist
            await _userManager.RemoveAuthenticationTokenAsync(_user, _loginProvider, _refreshToken);
            //create new token
            var newRefreshToken = await _userManager.GenerateUserTokenAsync(_user, _loginProvider,
                _refreshToken);
            //set the token in the database provided by identity
            var result = await _userManager.SetAuthenticationTokenAsync(_user, _loginProvider, _refreshToken, newRefreshToken);
            //return the token
            return newRefreshToken;
        }

        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            //log login
            _logger.LogInformation($"Looking for user with email: {loginDto.Email}");
            //find user
            _user = await _userManager.FindByEmailAsync(loginDto.Email);
            //check if null before passing to checkpasswordasync method to prevent null exception
            if (_user == null)
            {
                _logger.LogWarning($"User with email: {loginDto.Email} not found");
                return null;
            }
            //check password
            bool isValidUser = await _userManager.CheckPasswordAsync(_user, loginDto.Password);
             
            if(isValidUser == false)
            {
                return null;
            }
            //generate token after checks
            var token = await GenerateToken();
            _logger.LogInformation($"Token generated for user with email: {loginDto.Email} | Token: {token}");
            //return AuthResponseDto object with token and user Id
            return new AuthResponseDto
            {
                Token = token,
                UserId = _user.Id,
                RefreshToken = await CreateRefreshToken()
            };
        }

        public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
        {
            //map Dto to ApiUser object
            _user = _mapper.Map<ApiUser>(userDto);
            //set username to email
            _user.UserName = userDto.Email;

            //attempt to create user with usermanager
            //password is recieved from client as plaintext
            //encryption only happens after recieving pasword using Identity framework
            var result = await _userManager.CreateAsync(_user, userDto.Password);

            //add user to user role if successful
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(_user, "User");
            }
            //return errors or null if successful
            return result.Errors;
        }

        public async Task<IEnumerable<IdentityError>> RegisterAdmin(ApiUserDto userDto)
        {
            //map Dto to ApiUser object
            _user = _mapper.Map<ApiUser>(userDto);
            //set username to email
            _user.UserName = userDto.Email;

            //attempt to create user with usermanager
            //password is recieved from client as plaintext
            //encryption only happens after recieving pasword using Identity framework
            var result = await _userManager.CreateAsync(_user, userDto.Password);

            //add user to user role if successful
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(_user, "Administrator");
            }
            //return errors or null if successful
            return result.Errors;
        }

        public async Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request)
        {
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            //read content from authresponsedto object
            var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(request.Token);
            //get username from token
            // question mark at end makes expression return null rather than throw exception
            var username = tokenContent.Claims.ToList().FirstOrDefault(q => q.Type == JwtRegisteredClaimNames.Email)?.Value;

            //find the user by the username from the token
            _user = await _userManager.FindByNameAsync(username);

            //check if exists
            if(_user ==null || _user.Id != request.UserId)
            {
                return null;
            }

            var isValidRefreshToken = await _userManager.VerifyUserTokenAsync(_user, _loginProvider, _refreshToken, request.RefreshToken);

            if (isValidRefreshToken)
            {
                var token = await GenerateToken();
                return new AuthResponseDto
                {
                    Token = token,
                    UserId = _user.Id,
                    RefreshToken = await CreateRefreshToken()
                };
            }
            //
            await _userManager.UpdateSecurityStampAsync(_user);
            return null;
        }

        private async Task<string> GenerateToken()
        {
            //create security key
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));

            //create credentials using security key 
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //get roles for the user
            var roles = await _userManager.GetRolesAsync(_user);

            //get all claims to the roles from a list of strings(x is the list of strings)
            //a claim being everything that the role has access to
            //in other words, get all the claims for all roles for the user
            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();

            //get a users specific claims
            var userClaims = await _userManager.GetClaimsAsync(_user);

            var claims = new List<Claim>
            {
                //default claims
                new Claim(JwtRegisteredClaimNames.Sub, _user.Email),             //allows entity that takes token to be represented by 2nd param
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //security measure, GUID changes each token
                new Claim(JwtRegisteredClaimNames.Email, _user.Email),           //specify email of user 
                new Claim("uid", _user.Id)                                       //include userid in token (not a good idea sometimes)
            }
            //union with user claims from DB and role claims for the user
            .Union(userClaims).Union(roleClaims);

            //create token with config information, claims, and signing credentials, (token is a string)
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: credentials
                );
            //token generated here and returned
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
