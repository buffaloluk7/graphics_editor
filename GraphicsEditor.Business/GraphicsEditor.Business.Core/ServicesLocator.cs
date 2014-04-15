using GraphicsEditor.Business.Core.ViewModels;
using Luvi.Service.Navigation;
using Ninject;

namespace GraphicsEditor.Business.Core
{
    public class ServicesLocator
    {
        private IKernel kernel;

        public void Register(IKernel kernel, INavigationService navigationService)
        {
            this.kernel = kernel;
            
            // Services
            this.kernel.Bind<INavigationService>().ToConstant(navigationService);

            // Viewmodels
            this.kernel.Bind<MainWindowViewModel>().ToSelf().InTransientScope();
        }

        public MainWindowViewModel MainWindow
        {
            get
            {
                if (this.kernel != null)
                {
                    return this.kernel.Get<MainWindowViewModel>();
                }
                return null;
            }
        }
    }
}
