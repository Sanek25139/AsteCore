using AsteCore.EF.Model;
using AsteCore.EF.Model.Interface;

namespace AsteCore.EF.Services.Interface
{
    public interface IUserService<T> where T : BaseEntity, IUser
    {
        event Action<T?> OnChangeUser;
        T? GetUser();

        Task<bool> SignIn(string login, string password);
        void SignOut();
    }
}
