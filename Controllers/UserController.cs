using ASP111.Data;
using ASP111.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace ASP111.Controllers
{
    public class UserController : Controller
    {
        private readonly DataContext _dataContext;

        public UserController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public IActionResult Index()
        {
            return View();
        }
        public ViewResult SignUp(SignUpFormModel? formModel)
        {
            SignUpViewModel viewModel = new SignUpViewModel();
            if (Request.Method == "POST" && formModel != null)
            {
                //  передача формы
                viewModel = ValidateSignUpForm(formModel);
                viewModel.FormModel = formModel;
            }
            else
            {
                viewModel = new();
                viewModel.FormModel = null;
            }
            return View(viewModel);
        }
        private SignUpViewModel ValidateSignUpForm(SignUpFormModel formModel)
        {
            SignUpViewModel viewModel = new SignUpViewModel();

            if (String.IsNullOrEmpty(formModel.Login))
            {
                viewModel.LoginMessage = "Login can't be empty";
            }
            else if (formModel.Login.Length < 3)
            {
                viewModel.LoginMessage = "Login is too short (3 symbol min)";
            }
            else if (_dataContext.Users.Any(u => u.Login == formModel.Login))
            {
                viewModel.LoginMessage = "This login is already in use";
            }
            else
            {
                viewModel.LoginMessage = null;
            }

            if (String.IsNullOrEmpty(formModel.Password))
            {
                viewModel.PasswordMessage = "Password can't be empty";
            }
            else if (formModel.Password.Length < 4)
            {
                viewModel.PasswordMessage = "Password is too short (4 symbol min)";
            }
            else if (!Regex.IsMatch(formModel.Password, @"\d"))
            {
                viewModel.PasswordMessage = "Password must contain numbers";
            }
            else
            {
                viewModel.PasswordMessage = null;
            }


            if (formModel.Avatar != null) // File transferred
            {
                if (formModel.Avatar.Length > 1048576)
                {
                    viewModel.AvatarMessage = "File size exceeds the limit (Maximum 1 MB)";
                }
                String ext = Path.GetExtension(formModel.Avatar.FileName);
                String name = Guid.NewGuid().ToString() + ext;

                formModel.Avatar.CopyTo(new FileStream("wwwroot/avatars" + name, FileMode.Create));
            }


            return viewModel;
        }
    }
}
