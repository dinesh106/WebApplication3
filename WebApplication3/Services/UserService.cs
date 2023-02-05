using Microsoft.EntityFrameworkCore;
using WebApplication3.Model;
using WebApplication3.ViewModels;

namespace WebApplication3.Services
{
    public class UserService
	{
        private readonly AuthDbContext _context;

        public UserService(AuthDbContext context)
        {
            _context = context;
        }

        public List<Register> GetAll()
        {
            return _context.Users.ToList();
        }

        public Register? GetUserById(string id)
        {
            Register? employee = _context.Users.FirstOrDefault(x => x.Email.Equals(id));
            return employee;
        }

        public void AddUser(Register user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void UpdateUser(Register employee)
        {
            _context.Users.Update(employee);
            _context.SaveChanges();
        }
    }
}
