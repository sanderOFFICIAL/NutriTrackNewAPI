using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Models;
using NutriTrackAPI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NutriTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register/user")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
                string uid = decodedToken.Uid;

                if (await _context.Users.AnyAsync(u => u.user_uid == uid))
                {
                    return BadRequest(new { message = "User already registered." });
                }

                var user = new User
                {
                    user_uid = uid,
                    nickname = request.nickname,
                    profile_picture = request.profile_picture,
                    profile_description = request.profile_description,
                    gender = request.gender,
                    height = request.height,
                    current_weight = request.current_weight,
                    created_at = DateTime.UtcNow,
                    last_login = DateTime.UtcNow,
                    is_active = true,
                    activity_level = request.activity_level,
                    birth_year = request.birth_year
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User registered successfully.", user_uid = uid });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }


        [HttpPost("register/consultant")]
        public async Task<IActionResult> RegisterConsultant([FromBody] RegisterConsultantRequest request)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
                string uid = decodedToken.Uid;

                if (await _context.Consultants.AnyAsync(c => c.consultant_uid == uid))
                {
                    return BadRequest(new { message = "Consultant already registered." });
                }

                var consultant = new Consultant
                {
                    consultant_uid = uid,
                    nickname = request.nickname,
                    profile_picture = request.profile_picture,
                    profile_description = request.profile_description,
                    experience_years = request.experience_years,
                    max_clients = request.max_clients,
                    created_at = DateTime.Today,
                    last_login = DateTime.Today,
                    is_active = true,
                    gender = request.gender,
                };

                _context.Consultants.Add(consultant);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Consultant registered successfully.", user_uid = uid });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }

        [HttpPost("register/admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminRequest request)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
                string uid = decodedToken.Uid;

                if (await _context.Admins.AnyAsync(a => a.admin_uid == uid))
                {
                    return BadRequest(new { message = "Admin already registered." });
                }

                var admin = new Admin
                {
                    admin_uid = uid,
                    registration_date = DateTime.Today,
                    name = request.name,
                    email = request.email,
                    phone_number = request.phone_number
                };

                _context.Admins.Add(admin);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Admin registered successfully.", user_uid = uid });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }

        [HttpPost("login/user")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
                string uid = decodedToken.Uid;

                var user = await _context.Users.FirstOrDefaultAsync(u => u.user_uid == uid);

                if (user == null)
                {
                    return Unauthorized(new { message = "User not found." });
                }

                user.last_login = DateTime.Today;
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    user.user_uid,
                    user.nickname,
                });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }

        [HttpPost("login/consultant")]
        public async Task<IActionResult> LoginConsultant([FromBody] ConsultantLoginRequest request)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
                string uid = decodedToken.Uid;

                var consultant = await _context.Consultants.FirstOrDefaultAsync(c => c.consultant_uid == uid);

                if (consultant == null)
                {
                    return Unauthorized(new { message = "Consultant not found." });
                }

                consultant.last_login = DateTime.Today;
                _context.Entry(consultant).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    consultant.consultant_uid,
                    consultant.nickname,
                });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }

        [HttpPost("login/admin")]
        public async Task<IActionResult> LoginAdmin([FromBody] AdminLoginRequest request)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
                string uid = decodedToken.Uid;

                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.admin_uid == uid);

                if (admin == null)
                {
                    return Unauthorized(new { message = "Admin not found." });
                }

                return Ok(new
                {
                    admin.admin_uid,
                    admin.name,
                    admin.email,
                    admin.phone_number
                });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }
    }


    public class RegisterUserRequest
    {
        public required string idToken { get; set; }
        public required string nickname { get; set; }
        public required string profile_picture { get; set; }
        public required string profile_description { get; set; }
        public required string gender { get; set; }
        public int? height { get; set; }
        public double? current_weight { get; set; }
        public ActivityLevel activity_level { get; set; }
        public int birth_year { get; set; }
    }


    public class RegisterConsultantRequest
    {
        public required string idToken { get; set; }
        public required string nickname { get; set; }
        public required string profile_picture { get; set; }
        public required string profile_description { get; set; }
        public int experience_years { get; set; }
        public int max_clients { get; set; }
        public required string gender { get; set; }
    }
    public class RegisterAdminRequest
    {
        public required string idToken { get; set; }
        public required string name { get; set; }
        public required string email { get; set; }
        public required string phone_number { get; set; }
    }

    public class UserLoginRequest
    {
        public required string idToken { get; set; }
    }

    public class ConsultantLoginRequest
    {
        public required string idToken { get; set; }
    }

    public class AdminLoginRequest
    {
        public required string idToken { get; set; }
    }

}