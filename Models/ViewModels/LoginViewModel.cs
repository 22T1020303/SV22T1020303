using System.ComponentModel.DataAnnotations;

namespace V22T1020381.Admin.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Phải nhập tài khoản")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Phải nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
