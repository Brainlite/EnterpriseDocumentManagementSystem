using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using EnterpriseDocumentManagementSystem.Api.Models;

namespace EnterpriseDocumentManagementSystem.Api.Services;

public class MockAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly List<User> _mockUsers;

    public MockAuthService(IConfiguration configuration, IMapper mapper)
    {
        _configuration = configuration;
        _mapper = mapper;
        
        // Validate JWT configuration on service initialization
        ValidateJwtConfiguration();
        
        _mockUsers = new List<User>
        {
            new()
            {
                Id = "1",
                Email = "viewer@example.com",
                Password = "viewer123",
                Role = UserRoles.Viewer,
                Name = "John Viewer"
            },
            new()
            {
                Id = "2",
                Email = "contributor@example.com",
                Password = "contributor123",
                Role = UserRoles.Contributor,
                Name = "Jane Contributor"
            },
            new()
            {
                Id = "3",
                Email = "manager@example.com",
                Password = "manager123",
                Role = UserRoles.Manager,
                Name = "Bob Manager"
            },
            new()
            {
                Id = "4",
                Email = "admin@example.com",
                Password = "admin123",
                Role = UserRoles.Admin,
                Name = "Alice Admin"
            },
            new()
            {
                Id = "5",
                Email = "admin@company.com",
                Password = "Admin@123",
                Role = UserRoles.Admin,
                Name = "Admin"
            }
        };
    }

    private void ValidateJwtConfiguration()
    {
        var secret = _configuration["Jwt:Secret"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("JWT Secret is not configured in appsettings.json");
        }

        if (secret.Length < 32)
        {
            throw new InvalidOperationException("JWT Secret must be at least 32 characters long");
        }
    }

    public LoginResponse? AuthenticateUser(string email, string password)
    {
        var user = _mockUsers.FirstOrDefault(u => 
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && 
            u.Password == password);

        if (user == null)
            return null;

        var token = GenerateJwtToken(user);
        var userPayload = _mapper.Map<UserPayload>(user);
        
        // Using record type with positional parameters
        return new LoginResponse(token, userPayload);
    }

    public UserPayload? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secret = _configuration["Jwt:Secret"] 
                ?? throw new InvalidOperationException("JWT Secret is not configured");
            var key = Encoding.UTF8.GetBytes(secret);
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"] 
                    ?? throw new InvalidOperationException("JWT Issuer is not configured"),
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"] 
                    ?? throw new InvalidOperationException("JWT Audience is not configured"),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            
            // Using record type with positional parameters
            return new UserPayload(
                Sub: jwtToken.Claims.First(x => x.Type == "sub").Value,
                Email: jwtToken.Claims.First(x => x.Type == "email").Value,
                Role: jwtToken.Claims.First(x => x.Type == "role").Value,
                Name: jwtToken.Claims.First(x => x.Type == "name").Value
            );
        }
        catch
        {
            return null;
        }
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var secret = _configuration["Jwt:Secret"] 
            ?? throw new InvalidOperationException("JWT Secret is not configured");
        var key = Encoding.UTF8.GetBytes(secret);
        
        var claims = new List<Claim>
        {
            new("sub", user.Id),
            new("email", user.Email),
            new("role", user.Role),
            new("name", user.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8),
            Issuer = _configuration["Jwt:Issuer"] 
                ?? throw new InvalidOperationException("JWT Issuer is not configured"),
            Audience = _configuration["Jwt:Audience"] 
                ?? throw new InvalidOperationException("JWT Audience is not configured"),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public IEnumerable<User> GetAllMockUsers()
    {
        return _mockUsers.Select(u => new User
        {
            Id = u.Id,
            Email = u.Email,
            Role = u.Role,
            Name = u.Name,
            Password = "***" // Masked for security
        });
    }
}
