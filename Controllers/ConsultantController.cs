using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Models;
using NutriTrackAPI.Models;

namespace NutriTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultantController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ConsultantController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("send-invite-to-user")]
        public async Task<IActionResult> SendInviteToUser([FromBody] InviteUserRequest request)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
                string uid = decodedToken.Uid;

                var consultant = await _context.Consultants.FirstOrDefaultAsync(c => c.consultant_uid == uid);
                if (consultant == null)
                {
                    return BadRequest(new { message = "Consultant not found." });
                }

                if (consultant.current_clients >= consultant.max_clients)
                {
                    return BadRequest(new { message = "No available slots for new clients." });
                }

                var user = await _context.Users.FindAsync(request.user_uid);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                // Check if an invite has already been sent
                var existingRequest = await _context.ConsultantRequests
                    .FirstOrDefaultAsync(r => r.consultant_uid == uid && r.user_uid == request.user_uid && r.status == "pending");

                if (existingRequest != null)
                {
                    return BadRequest(new { message = "Invite already sent." });
                }

                var consultantRequest = new ConsultantRequest
                {
                    consultant_uid = uid,
                    user_uid = request.user_uid,
                    status = "pending",
                    created_at = DateTime.UtcNow,
                    Consultant = consultant,
                    User = user
                };

                _context.ConsultantRequests.Add(consultantRequest);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Invite sent successfully." });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }

        [HttpPost("user-send-invite")]
        public async Task<IActionResult> UserSendInviteToConsultant([FromBody] UserInviteConsultantRequest request)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
                string uid = decodedToken.Uid;

                var user = await _context.Users.FirstOrDefaultAsync(u => u.user_uid == uid);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                var consultant = await _context.Consultants.FirstOrDefaultAsync(c => c.consultant_uid == request.consultant_uid);
                if (consultant == null)
                {
                    return NotFound(new { message = "Consultant not found." });
                }

                var existingRequest = await _context.ConsultantRequests
                    .FirstOrDefaultAsync(r => r.user_uid == uid && r.consultant_uid == request.consultant_uid && r.status == "pending");

                if (existingRequest != null)
                {
                    return BadRequest(new { message = "Invite already sent." });
                }

                var consultantRequest = new ConsultantRequest
                {
                    consultant_uid = request.consultant_uid,
                    user_uid = uid,
                    status = "pending",
                    created_at = DateTime.UtcNow,
                    Consultant = consultant,
                    User = user
                };

                _context.ConsultantRequests.Add(consultantRequest);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Invite sent to consultant successfully." });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }

        [HttpPost("consultant-respond-invite")]
        public async Task<IActionResult> ConsultantRespondToUserInvite([FromBody] ConsultantRespondToInviteRequest request)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
                string uid = decodedToken.Uid;

                var consultant = await _context.Consultants.FirstOrDefaultAsync(c => c.consultant_uid == uid);
                if (consultant == null)
                {
                    return NotFound(new { message = "Consultant not found." });
                }

                var consultantRequest = await _context.ConsultantRequests
                    .FirstOrDefaultAsync(cr => cr.consultant_uid == uid && cr.user_uid == request.user_uid && cr.status == "pending");

                if (consultantRequest == null)
                {
                    return NotFound(new { message = "Invite not found or already responded to." });
                }

                if (request.is_accepted)
                {
                    if (consultant.current_clients >= consultant.max_clients)
                    {
                        return BadRequest(new { message = "No available slots for new clients." });
                    }

                    consultantRequest.status = "accepted";
                    var user = await _context.Users.FindAsync(request.user_uid);
                    if (user == null)
                    {
                        return NotFound(new { message = "User not found." });
                    }

                    var userConsultant = new UserConsultant
                    {
                        user_uid = request.user_uid,
                        consultant_uid = uid,
                        is_active = true,
                        assignment_date = DateTime.UtcNow,
                        User = user,
                        Consultant = consultant
                    };

                    _context.UserConsultants.Add(userConsultant);
                    consultant.current_clients += 1;
                    _context.Entry(consultant).State = EntityState.Modified;
                }
                else
                {
                    consultantRequest.status = "rejected";
                }

                consultantRequest.responded_at = DateTime.UtcNow;
                _context.Entry(consultantRequest).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Invite response recorded." });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }

        [HttpPost("user-respond-invite")]
        public async Task<IActionResult> RespondToInvite([FromBody] RespondToInviteRequest request)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
                string uid = decodedToken.Uid;

                var consultantRequest = await _context.ConsultantRequests
                    .FirstOrDefaultAsync(cr => cr.user_uid == uid && cr.consultant_uid == request.consultant_uid);

                if (consultantRequest == null)
                {
                    return NotFound(new { message = "Invite not found." });
                }

                if (request.is_accepted)
                {
                    consultantRequest.status = "accepted";

                    var user = await _context.Users.FindAsync(uid);
                    var consultant = await _context.Consultants.FindAsync(request.consultant_uid);

                    if (user == null || consultant == null)
                    {
                        return NotFound(new { message = "User or consultant not found." });
                    }

                    var userConsultant = new UserConsultant
                    {
                        user_uid = uid,
                        consultant_uid = request.consultant_uid,
                        is_active = true,
                        assignment_date = DateTime.UtcNow,
                        User = user,
                        Consultant = consultant
                    };

                    _context.UserConsultants.Add(userConsultant);

                    consultant.current_clients += 1;
                    _context.Entry(consultant).State = EntityState.Modified;
                }
                else
                {
                    consultantRequest.status = "rejected";
                }

                consultantRequest.responded_at = DateTime.UtcNow;

                _context.Entry(consultantRequest).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Invite response recorded." });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }

        }

        [HttpDelete("consultant-remove-user")]
        public async Task<IActionResult> RemoveUser([FromQuery] string idToken, [FromQuery] string user_uid)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                string uid = decodedToken.Uid;

                var consultant = await _context.Consultants.FindAsync(uid);
                if (consultant == null)
                {
                    return BadRequest(new { message = "Unauthorized. Consultant not found." });
                }

                var userConsultant = await _context.UserConsultants
                    .FirstOrDefaultAsync(uc => uc.user_uid == user_uid && uc.consultant_uid == uid);

                if (userConsultant == null)
                {
                    return NotFound(new { message = "User is not assigned to this consultant." });
                }

                _context.UserConsultants.Remove(userConsultant);

                consultant.current_clients = Math.Max(0, consultant.current_clients - 1);
                _context.Entry(consultant).State = EntityState.Modified;

                var activeRequests = await _context.ConsultantRequests
                    .Where(cr => cr.user_uid == user_uid && cr.consultant_uid == uid && cr.status == "accepted")
                    .ToListAsync();

                if (activeRequests.Any())
                {
                    _context.ConsultantRequests.RemoveRange(activeRequests);
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "User removed successfully and associated requests deleted." });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }

        [HttpPut("update-nickname")]
        public async Task<IActionResult> UpdateConsultantNickname([FromBody] UpdateConsultantNicknameRequest request)
        {
            try
            {
                FirebaseService.Initialize();

                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
                string uid = decodedToken.Uid;

                var consultant = await _context.Consultants.FirstOrDefaultAsync(c => c.consultant_uid == uid);
                if (consultant == null)
                {
                    return NotFound(new { message = "Consultant not found." });
                }

                if (!string.IsNullOrEmpty(request.new_nickname))
                {
                    consultant.nickname = request.new_nickname;
                    _context.Entry(consultant).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Nickname updated successfully." });
                }

                return BadRequest(new { message = "New nickname is required." });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }

        [HttpPut("update-profile-picture")]
        public async Task<IActionResult> UpdateConsultantProfilePicture([FromBody] UpdateConsultantProfilePictureRequest request)
        {
            try
            {
                FirebaseService.Initialize();

                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
                string uid = decodedToken.Uid;

                var consultant = await _context.Consultants.FirstOrDefaultAsync(c => c.consultant_uid == uid);
                if (consultant == null)
                {
                    return NotFound(new { message = "Consultant not found." });
                }

                if (!string.IsNullOrEmpty(request.new_profile_picture))
                {
                    consultant.profile_picture = request.new_profile_picture;
                    _context.Entry(consultant).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Profile picture updated successfully." });
                }

                return BadRequest(new { message = "New profile picture is required." });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }


        [HttpPut("update-profile-description")]
        public async Task<IActionResult> UpdateConsultantProfileDescription([FromBody] UpdateConsultantProfileDescriptionRequest request)
        {
            try
            {
                FirebaseService.Initialize();

                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
                string uid = decodedToken.Uid;

                var consultant = await _context.Consultants.FirstOrDefaultAsync(c => c.consultant_uid == uid);
                if (consultant == null)
                {
                    return NotFound(new { message = "Consultant not found." });
                }

                if (!string.IsNullOrEmpty(request.new_profile_description))
                {
                    consultant.profile_description = request.new_profile_description;
                    _context.Entry(consultant).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Profile description updated successfully." });
                }

                return BadRequest(new { message = "New profile description is required." });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }


        [HttpPut("update-max-clients")]
        public async Task<IActionResult> UpdateConsultantMaxClients([FromBody] UpdateConsultantMaxClientsRequest request)
        {
            try
            {
                FirebaseService.Initialize();

                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
                string uid = decodedToken.Uid;

                var consultant = await _context.Consultants.FirstOrDefaultAsync(c => c.consultant_uid == uid);
                if (consultant == null)
                {
                    return NotFound(new { message = "Consultant not found." });
                }

                if (request.new_max_clients.HasValue)
                {
                    consultant.max_clients = request.new_max_clients.Value;
                    _context.Entry(consultant).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Max clients updated successfully." });
                }

                return BadRequest(new { message = "New max clients count is required." });
            }
            catch (FirebaseAuthException ex)
            {
                return Unauthorized(new { message = "Invalid token", error = ex.Message });
            }
        }

        [HttpGet("get-linked-relationships")]
        public async Task<IActionResult> GetLinkedConsultantsAndUsers()
        {
            try
            {
                var relationships = await _context.UserConsultants
                    .Include(uc => uc.User)
                    .Include(uc => uc.Consultant)
                    .Select(uc => new
                    {
                        LinkId = uc.user_consultant_id,
                        UserUid = uc.user_uid,
                        ConsultantUid = uc.consultant_uid,
                        AssignmentDate = uc.assignment_date,
                        IsActive = uc.is_active,
                        User = new
                        {
                            uc.User.user_uid,
                            uc.User.nickname,
                            uc.User.profile_picture,
                            uc.User.gender
                        },
                        Consultant = new
                        {
                            uc.Consultant.consultant_uid,
                            uc.Consultant.nickname,
                            uc.Consultant.profile_picture,
                            uc.Consultant.profile_description,
                            uc.Consultant.experience_years
                        }
                    })
                    .ToListAsync();

                return Ok(relationships);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("get-all-requests")]
        public async Task<IActionResult> GetAllConsultantRequests()
        {
            try
            {
                var requests = await _context.ConsultantRequests
                    .Include(cr => cr.User)
                    .Include(cr => cr.Consultant)
                    .Select(cr => new
                    {
                        RequestId = cr.request_id,
                        UserUid = cr.user_uid,
                        ConsultantUid = cr.consultant_uid,
                        Status = cr.status,
                        CreatedAt = cr.created_at,
                        RespondedAt = cr.responded_at,
                        User = new
                        {
                            cr.User.user_uid,
                            cr.User.nickname,
                            cr.User.profile_picture,
                            cr.User.gender
                        },
                        Consultant = new
                        {
                            cr.Consultant.consultant_uid,
                            cr.Consultant.nickname,
                            cr.Consultant.profile_picture,
                            cr.Consultant.profile_description,
                            cr.Consultant.experience_years
                        }
                    })
                    .ToListAsync();

                return Ok(requests);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("get-all-consultants")]
        public async Task<IActionResult> GetAllConsultants()
        {
            try
            {
                var consultants = await _context.Consultants
                    .Select(c => new
                    {
                        c.consultant_uid,
                        c.nickname,
                        c.profile_picture,
                        c.profile_description,
                        c.experience_years,
                        c.is_active,
                        c.created_at,
                        c.last_login,
                        c.max_clients,
                        c.current_clients,
                        c.gender
                    })
                    .ToListAsync();

                return Ok(consultants);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("get-consultant/{uid}")]
        public async Task<IActionResult> GetConsultantByUid(string uid)
        {
            var consultant = await _context.Consultants
                .FirstOrDefaultAsync(c => c.consultant_uid == uid);

            if (consultant == null)
            {
                return NotFound(new { message = "Consultant not found." });
            }

            return Ok(consultant);
        }


    }

    // DTOs
    public class InviteUserRequest
    {
        public required string idToken { get; set; }
        public required string user_uid { get; set; }
    }

    public class RespondToInviteRequest
    {
        public required string idToken { get; set; }
        public required string consultant_uid { get; set; }
        public bool is_accepted { get; set; }
    }

    public class UpdateConsultantNicknameRequest
    {
        public required string idToken { get; set; }
        public required string new_nickname { get; set; }
    }

    public class UpdateConsultantProfilePictureRequest
    {
        public required string idToken { get; set; }
        public required string new_profile_picture { get; set; }
    }

    public class UpdateConsultantProfileDescriptionRequest
    {
        public required string idToken { get; set; }
        public required string new_profile_description { get; set; }
    }

    public class UpdateConsultantMaxClientsRequest
    {
        public required string idToken { get; set; }
        public int? new_max_clients { get; set; }
    }
    public class RemoveUserRequest
    {
        public required string idToken { get; set; }
        public required string user_uid { get; set; }
    }
    public class ConsultantRespondToInviteRequest
    {
        public required string idToken { get; set; }
        public required string user_uid { get; set; }
        public bool is_accepted { get; set; }
    }
    public class UserInviteConsultantRequest
    {
        public required string idToken { get; set; }
        public required string consultant_uid { get; set; }
    }



}
