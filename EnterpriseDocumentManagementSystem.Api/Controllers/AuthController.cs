using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterpriseDocumentManagementSystem.Api.Attributes;
using EnterpriseDocumentManagementSystem.Api.Extensions;
using EnterpriseDocumentManagementSystem.Api.Models;
using EnterpriseDocumentManagementSystem.Api.Services;

namespace EnterpriseDocumentManagementSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MockAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(MockAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Authenticates a user with email and password
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            _logger.LogDebug("Login attempt for email: {Email}", request.Email);
            
            // FluentValidation automatically validates the request
            // If validation fails, ModelState will contain errors
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login validation failed for email: {Email}", request.Email);
                return BadRequest(ModelState);
            }

            var result = _authService.AuthenticateUser(request.Email, request.Password);
            
            if (result == null)
            {
                _logger.LogWarning("Login failed: Invalid credentials for email: {Email}", request.Email);
                return Unauthorized(new { message = "Invalid email or password" });
            }

            _logger.LogInformation("User logged in successfully: {Email}, Role: {Role}", request.Email, result.User.Role);
            return Ok(result);
        }

        /// <summary>
        /// Returns the current authenticated user's information
        /// </summary>
        /// <returns>User information from the JWT token</returns>
        [HttpGet("me")]
        [RequireAuth]
        [ProducesResponseType(typeof(UserPayload), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetCurrentUser()
        {
            var user = HttpContext.GetCurrentUser();
            return Ok(user);
        }

        /// <summary>
        /// Logs out the current user (client-side token disposal)
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            // No server-side state to clear - client should dispose of the token
            return Ok(new { message = "Logged out successfully. Please dispose of your token on the client side." });
        }

        /// <summary>
        /// Returns a list of available users
        /// </summary>
        /// <returns>List of users</returns>
        [HttpGet("users")]
        [RequireAuth]
        [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetUsers()
        {
            return Ok(_authService.GetAllMockUsers());
        }
    }
}
