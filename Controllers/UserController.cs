﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Models;
using FirebaseAdmin.Auth;
using System.Linq;
using System.Threading.Tasks;

namespace NutriTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPut("update-nickname")]
        public async Task<IActionResult> UpdateNickname([FromBody] UpdateNicknameRequest request)
        {
            FirebaseService.Initialize();
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
            string uid = decodedToken.Uid;

            var user = await _context.Users.FindAsync(uid);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            user.nickname = request.new_nickname;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Nickname updated successfully." });
        }

        [HttpPut("update-profile-picture")]
        public async Task<IActionResult> UpdateProfilePicture([FromBody] UpdateProfilePictureRequest request)
        {
            FirebaseService.Initialize();
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
            string uid = decodedToken.Uid;

            var user = await _context.Users.FindAsync(uid);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            user.profile_picture = request.new_profile_picture;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile picture updated successfully." });
        }

        [HttpPut("update-profile-description")]
        public async Task<IActionResult> UpdateProfileDescription([FromBody] UpdateProfileDescriptionRequest request)
        {
            FirebaseService.Initialize();
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
            string uid = decodedToken.Uid;

            var user = await _context.Users.FindAsync(uid);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            user.profile_description = request.new_profile_description;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile description updated successfully." });
        }

        [HttpPut("update-current-weight")]
        public async Task<IActionResult> UpdateCurrentWeight([FromBody] UpdateCurrentWeightRequest request)
        {
            FirebaseService.Initialize();
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
            string uid = decodedToken.Uid;

            var user = await _context.Users.FindAsync(uid);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            user.current_weight = request.new_current_weight;

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "User current weight updated successfully." });
        }

        [HttpDelete("remove-consultant")]
        public async Task<IActionResult> RemoveConsultant(
    [FromQuery] string idToken,
    [FromQuery] string consultant_uid)
        {
            try
            {
                // 1. Перевірка токену та отримання UID користувача
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                string userUid = decodedToken.Uid;

                // 2. Перевірка, чи є такий зв'язок між користувачем і консультантом
                var userConsultant = await _context.UserConsultants
                    .FirstOrDefaultAsync(uc => uc.user_uid == userUid && uc.consultant_uid == consultant_uid);

                if (userConsultant == null)
                {
                    return NotFound(new { message = "Consultant not found or not assigned to this user." });
                }

                // 3. Видалення зв'язку user-consultant
                _context.UserConsultants.Remove(userConsultant);
                await _context.SaveChangesAsync(); // окремий Save, щоб не було конфлікту далі

                // 4. Зменшення кількості клієнтів у консультанта
                var consultant = await _context.Consultants.FindAsync(consultant_uid);
                if (consultant != null)
                {
                    consultant.current_clients = Math.Max(0, consultant.current_clients - 1);
                    _context.Entry(consultant).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }

                // 5. Видалення всіх "accepted" запитів між цим користувачем і консультантом
                var acceptedRequests = await _context.ConsultantRequests
                    .Where(cr => cr.user_uid == userUid && cr.consultant_uid == consultant_uid && cr.status == "accepted")
                    .ToListAsync();

                if (acceptedRequests.Any())
                {
                    _context.ConsultantRequests.RemoveRange(acceptedRequests);
                    await _context.SaveChangesAsync(); // ОБОВ'ЯЗКОВО!
                }

                return Ok(new { message = "Consultant removed successfully and accepted requests deleted." });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }



        [HttpGet("get-user-by-uid")]
        public async Task<IActionResult> GetUserByUid([FromQuery] string uid)
        {
            try
            {
                var user = await _context.Users.FindAsync(uid);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new
                    {
                        u.user_uid,
                        u.nickname,
                        u.profile_picture,
                        u.profile_description,
                        u.gender,
                        u.height,
                        u.current_weight,
                        u.created_at,
                        u.last_login,
                        u.is_active
                    })
                    .ToListAsync();

                if (!users.Any())
                {
                    return NotFound(new { message = "No users found." });
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // DTOs
    public class UpdateNicknameRequest
    {
        public required string idToken { get; set; }
        public required string new_nickname { get; set; }
    }

    public class UpdateProfilePictureRequest
    {
        public required string idToken { get; set; }
        public required string new_profile_picture { get; set; }
    }

    public class UpdateProfileDescriptionRequest
    {
        public required string idToken { get; set; }
        public required string new_profile_description { get; set; }
    }

    public class UpdateCurrentWeightRequest
    {
        public required string idToken { get; set; }
        public double new_current_weight { get; set; }
    }

    public class RemoveConsultantRequest
    {
        public required string idToken { get; set; }
        public required string consultant_uid { get; set; }
    }
}
