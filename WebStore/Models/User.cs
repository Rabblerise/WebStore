using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace WebStore.Models
{
    public class User
    {
        public string? Id { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
    }
}
