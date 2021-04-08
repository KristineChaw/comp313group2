using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GetAccredited.Models
{
    /// <summary>
    /// Provides extension methods to some classes.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns all representatives of an organization.
        /// </summary>
        /// <param name="userMgr"></param>
        /// <param name="organizationId"></param>
        /// <returns>List of representatives of the organization with the specified <c>organizationId</c></returns>
        public static async Task<IEnumerable<ApplicationUser>> GetRepresentativesByOrganization(
            this UserManager<ApplicationUser> userMgr, string organizationId)
        {
            return (await userMgr.GetUsersInRoleAsync(Utility.ROLE_REP))
                .Where(r => r.OrganizationId == organizationId);
        }

        /// <summary>
        /// Indicates whether <c>email</c> is already used by a user.
        /// </summary>
        /// <param name="userMgr"></param>
        /// <param name="email"></param>
        /// <returns>true if the email is already taken; otherwise, false.</returns>
        public static bool IsEmailTaken(this UserManager<ApplicationUser> userMgr, string email)
        {
            return userMgr.Users.Where(u => u.Email != null && u.Email != "").Where(u => u.Email == email).Any();
        }

        /// <summary>
        /// Returns the role of <c>user</c>
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetRole(this ClaimsPrincipal user)
        {
            if (user.IsInRole(Utility.ROLE_STUDENT)) return Utility.ROLE_STUDENT;
            if (user.IsInRole(Utility.ROLE_REP)) return Utility.ROLE_REP;
            return Utility.ROLE_ADMIN;
        }

        /// <summary>
        /// Indicates whether <c>user</c> is an admin.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole(Utility.ROLE_ADMIN);

        /// <summary>
        /// Indicates whether <c>user</c> is a representative.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool IsRepresentative(this ClaimsPrincipal user) => user.IsInRole(Utility.ROLE_REP);

        /// <summary>
        /// Indicates whether <c>user</c> is a student.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool IsStudent(this ClaimsPrincipal user) => user.IsInRole(Utility.ROLE_STUDENT);
    }
}