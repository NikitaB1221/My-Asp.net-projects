using ASP111.Data;
using ASP111.Models.User;
using ASP111.Services.Hash;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Text.RegularExpressions;

namespace ASP111.Controllers
{
    public class UserController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IHashService _md5HashService;


        public UserController(DataContext dataContext, IHashService md5HashService)
        {
            _dataContext = dataContext;
            _md5HashService = md5HashService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SignUp(SignUpFormModel? formModel)
        {
            SignUpViewModel viewModel;
            if (Request.Method == "POST" && formModel != null)
            {
                // передача формы
                viewModel = ValidateSignUpForm(formModel);
                viewModel.FormModel = formModel;

                HttpContext.Session.SetString("FormData", System.Text.Json.JsonSerializer.Serialize(viewModel));
                return RedirectToAction(nameof(SignUp));
            }
            else
            {
                if (HttpContext.Session.Keys.Contains("FormData"))
                {
                    String? data = HttpContext.Session.GetString("FormData");
                    if (data is not null)
                    {
                        viewModel = System.Text.Json.JsonSerializer
                            .Deserialize<SignUpViewModel>(data)!;
                    }
                    else
                    {
                        viewModel = new();
                        viewModel.FormModel = null;
                    }
                    HttpContext.Session.Remove("FormData");
                }
                else
                {
                    // первый заход - начало заполнения формы
                    viewModel = new();
                    viewModel.FormModel = null;  // нечего проверять
                }
            }
            return View(viewModel);  // передаем модель в представление
        }

        private String? ValidateLogin(SignUpFormModel formModel)
        {
            if (String.IsNullOrEmpty(formModel.Login))
            {
                return "Логин не может быть пустым";
            }
            else if (formModel.Login.Length < 3)
            {
                return "Логин слишком короткий (3 символа минимум)";
            }
            else if (_dataContext.Users.Any(u => u.Login == formModel.Login))
            {
                return "Данный логин уже занят";
            }
            else
            {
                return null;  // все проверки логина пройдены
            }
        }
        private String? ValidatePassword(SignUpFormModel formModel)
        {
            if (String.IsNullOrEmpty(formModel.Password))
            {
                return "Пароль не может быть пустым";
            }
            else if (formModel.Password.Length < 3)
            {
                return "Пароль слишком короткий (3 символа минимум)";
            }
            else if (!Regex.IsMatch(formModel.Password, @"\d"))
            {
                return "Пароль должен содержать цифры";
            }
            else
            {
                return null;  // все проверки пароля пройдены
            }
        }
        private String? ValidateRepeatPassword(SignUpFormModel formModel)
        {
            if (formModel.Password is not null)
            {
                if (!formModel.Password.Equals(formModel.RepeatPassword))
                {
                    return "Пароли не совпадают";
                }
                else
                {
                    return null;
                }
            }
            else
                return "Сначала введите пароль";
        }
        private String? ValidateRealName(SignUpFormModel formModel)
        {
            if (!String.IsNullOrEmpty(formModel.RealName) && !Regex.IsMatch(formModel.RealName, @"^[a-zA-Z]+$"))  //  ^ - начало строки, [a-zA-Z] - символы от a до z (маленькие и большие буквы), + - один или более раз, $ - конец строки
            {
                return "Имя должно содержать только латинские буквы ";
            }
            else
            {
                return null;  // все проверки пароля пройдены
            }
        }
        private String? ValidateAvatar(SignUpFormModel formModel)
        {
            /* При приема файла важно:
             * - проверить допустимые расширения (тип)
             */
            if (formModel.Avatar is null)
            {
                return null;
            }
            else if (formModel.Avatar.Length > 1048576)
            {
                return "Файл слишком большой (макс 1 МБ)";
            }
            else if (!Regex.IsMatch(Path.GetExtension(formModel.Avatar.FileName), @"^.*\.(jpg|jpeg|png|gif)$", RegexOptions.IgnoreCase))
            {
                return "Не подходящий тмп расширения Файла(Допустимые расширения: jpg, jpeg, png, gif)";
            }
            else
            {
                return null;
            }
        }
        private String? ValidateEmail(SignUpFormModel formModel)
        {
            if (string.IsNullOrEmpty(formModel.Email))
            {
                return "Введите Email";
            }
            else if (!Regex.IsMatch(formModel.Email, @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$"))
            {
                return "Некорректный ввод Email";
            }
            else
            {
                return null;
            }
        }
        private String? ValidateConfirm(SignUpFormModel formModel)
        {
            if (formModel.IsAgree is not true)
            {
                return "Отсутствует согласие на соблюдения правил сайта";
            }
            else
            {
                return null;
            }
        }



        private SignUpViewModel ValidateSignUpForm(SignUpFormModel formModel)
        {
            SignUpViewModel viewModel = new();
            viewModel.LoginMessage = ValidateLogin(formModel);
            viewModel.PasswordMessage = ValidatePassword(formModel);
            viewModel.RepeatMessage = ValidateRepeatPassword(formModel);
            viewModel.RealNameMessage = ValidateRealName(formModel);
            viewModel.EmailMessage = ValidateEmail(formModel);
            viewModel.ConfirmMessage = ValidateConfirm(formModel);


            String nameAvatar = null!;
            viewModel.AvatarMessage = ValidateAvatar(formModel);
            String ext = formModel.Avatar is null ? "" : Path.GetExtension(formModel.Avatar.FileName);
            nameAvatar = Guid.NewGuid().ToString() + ext;

            // сохранение файла
            if (formModel.Avatar is not null && viewModel.AvatarMessage is null)  // файл передан
            {
                // определяем расширение файла
                //String ext = Path.GetExtension(formModel.Avatar.FileName);
                // проверить расширение на перечень допустимых
                //
                // формируем имя для файла
                //nameAvatar = Guid.NewGuid().ToString() + ext;

                formModel.Avatar.CopyTo(
                    new FileStream("wwwroot/avatars/" + nameAvatar, FileMode.Create));
            }


            if (viewModel.LoginMessage is null &&
                viewModel.PasswordMessage is null &&
                viewModel.RepeatMessage is null &&
                viewModel.RealNameMessage is null &&
                viewModel.EmailMessage is null)
            {
                _dataContext.Users.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Login = formModel.Login,
                    PasswordHash = _md5HashService.GetHash(formModel.RepeatPassword),
                    Email = formModel.Email!,
                    CreatedDt = DateTime.Now,
                    Name = formModel.RealName!,
                    Avatar = nameAvatar

                });
                _dataContext.SaveChanges();
                TempData["SuccessMessage"] = "Регистрация прошла успешно. Войдите в систему.";
            }
            return viewModel;
        }
    }
}