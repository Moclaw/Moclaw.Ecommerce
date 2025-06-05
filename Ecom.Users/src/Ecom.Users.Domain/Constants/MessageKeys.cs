using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecom.Users.Domain.Constants
{
    public struct MessageKeys
    {
        // General messages
        public const string Success = "Success";
        public const string Error = "Something went wrong";
        public const string NotFound = "Not found";
        public const string Conflict = "Conflict occurred";
        public const string NoContent = "No content available";
        public const string BadRequest = "Bad request";
        public const string Unauthorized = "Unauthorized";
        public const string Forbidden = "Forbidden";
        public const string InternalServerError = "Internal server error";

        // User-related messages
        public const string UserNotFound = "User not found";
        
        // Auth messages
        public const string UserUpdatedSuccessfully = "User updated successfully";
        public const string AccessDenied = "Access denied: You can only update your own information";
        public const string CurrentPasswordIncorrect = "Current password is incorrect";
        public const string EmailAlreadyExists = "Email already exists";
        public const string InvalidUserId = "Invalid user ID";
        public const string OperationCancelled = "Operation cancelled";
        
        // Registration messages
        public const string RegisterSuccess = "User registered successfully";
        public const string PasswordMismatch = "Password and confirmation password do not match";
        public const string UserNameTaken = "Username is already taken";
        public const string EmailRequired = "Email is required";
        public const string PasswordRequired = "Password is required";
        
        // Token messages
        public const string TokenRefreshedSuccessfully = "Token refreshed successfully";
        public const string InvalidToken = "Invalid token";
        public const string TokenExpired = "Token expired";
        
        // Login messages
        public const string LoginSuccess = "Login successful";
        public const string InvalidCredentials = "Invalid email or password";
        public const string AccountLocked = "Account is locked";
    }
}
