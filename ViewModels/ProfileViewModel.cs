using System.ComponentModel.DataAnnotations;

namespace mvc_user_profile.ViewModels
{
    public class ProfileViewModel
    {
        public string Username { get; set; }
        public string Email { get; set; }        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Token { get; set; }
            
    }
}