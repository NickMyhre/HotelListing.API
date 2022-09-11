using HotelListing.API.Core.Contracts;
using HotelListing.API.Core.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthManager _authManager;
        //logger injection to allow custom logging
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthManager authManager, ILogger<AccountController> logger)
        {
            this._authManager = authManager;
            this._logger = logger;
        }

        // api/Account/register
        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Register([FromBody] ApiUserDto apiUserDto)
        {
            //////NOTE: Logging here is not necessary. Logging and exception handling is dealt with through the ExceptionMiddleware class./////
            
            //log if someone attempts to register
            _logger.LogInformation($"Registration Attempt for {apiUserDto.Email}");
            try
            {
                var errors = await _authManager.Register(apiUserDto);

                //if errors exist
                if (errors.Any())
                {
                    //iterate through errors
                    foreach (var error in errors)
                    {
                        //add errors to model state
                        //modelstate handles errors and model state (model being the ApiUseDto data type)
                        //model state is an object that used to hold errors for bad requests
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    //return bad request with model state that contains errors
                    return BadRequest(ModelState);
                }
                return Ok();
            }
            catch (Exception ex)
            {

                //log error for Register Action and include email
                _logger.LogError(ex, $"Something Went Wrong in the {nameof(Register)} - User Registration attempt for {apiUserDto.Email}");
                //message returned to client
                return Problem($"Something Went Wrong in the {nameof(Register)}. Please contact support", statusCode: 500);
            }

        }

        // api/Account/admin
        [HttpPost]
        [Route("admin")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> RegisterAdmin([FromBody] ApiUserDto apiUserDto)
        {
            var errors = await _authManager.Register(apiUserDto);

            //if errors exist
            if (errors.Any())
            {
                //iterate through errors
                foreach (var error in errors)
                {
                    //add errors to model state
                    //modelstate handles errors and model state (model being the ApiUseDto data type)
                    //model state is an object that used to hold errors for bad requests
                    ModelState.AddModelError(error.Code, error.Description);
                }
                //return bad request with model state that contains errors
                return BadRequest(ModelState);
            }
            return Ok();
        }





        // api/Account/Login
        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            var authResponse = await _authManager.Login(loginDto);

            if (authResponse == null)
            {
                return Unauthorized();
            }
            //return ok response with token and user id if authenticated/authorized
            return Ok(authResponse);
        }


        // api/Account/Login
        [HttpPost]
        [Route("refreshtoken")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> RefreshToken([FromBody] AuthResponseDto request)
        {
            var authResponse = await _authManager.VerifyRefreshToken(request);

            if (authResponse == null)
            {
                return Unauthorized();
            }
            //return ok response with token and user id if authenticated/authorized
            return Ok(authResponse);
        }
    }
}

