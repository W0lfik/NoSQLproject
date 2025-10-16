namespace NoSQLproject.Services.Interfaces
{
    public interface IPasswordResetService
    {
        // sends email if user exists; does NOT reveal whether user exists
        void RequestPasswordReset(string emailOrEmployeeNumber);

        // throws if token invalid/expired/used; updates password on success
        void ResetPassword(string token, string newPassword);
    }
}

