using NoSQLproject.Models;

namespace NoSQLproject.Repositories.Interfaces
{
    public interface IPasswordResetRepository
    {
        void Create(PasswordResetToken token);
        PasswordResetToken? GetByToken(string token);
        void MarkUsed(string id);
        void InvalidateAllForUser(string userId); // optional: cleanup
    }
}
