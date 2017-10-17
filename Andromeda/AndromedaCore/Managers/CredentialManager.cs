using System.DirectoryServices.AccountManagement;
using AndromedaCore.Infrastructure;
using AndromedaCore.Model;
using AndromedaCore.View;
using AndromedaCore.ViewModel;

namespace AndromedaCore.Managers
{
    public class CredentialManager
    {
        private static IWindowService _windowService;

        public CredentialManager(IWindowService windowService)
        {
            _windowService = windowService;
        }

        public static CredToken RequestCredentials()
        {
            var loginWindowViewModel = new LoginWindowViewModel();
            _windowService.ShowDialog<LoginWindow>(loginWindowViewModel);

            if (loginWindowViewModel.WasCanceled)
            {
                loginWindowViewModel.Dispose();
                return null;
            }

            var credtoken = new CredToken(loginWindowViewModel.Domain, loginWindowViewModel.Username, SecureStringHelper.BuildSecureString(loginWindowViewModel.Password));

            return credtoken;
        }

        
        public static bool ValidateCredentials(string domain, string user, string pass)
        {
            using (PrincipalContext context = new PrincipalContext(ContextType.Domain, domain))
            {
                return context.ValidateCredentials(user, pass);
            }
        }
    }
}
