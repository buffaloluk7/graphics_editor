using GraphicsEditor.Business.Core;
using GraphicsEditor.Business.Core.ViewModels;
using GraphicsEditor.Presentation.WPF.Views;
using Luvi.WPF.Service.Navigation;
using Ninject;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GraphicsEditor.Presentation.WPF
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.Navigating += App_Navigating;
        }

        void App_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            var locator = App.Current.Resources["Locator"] as ServicesLocator;

            if (locator == null)
            {
                throw new InvalidCastException("Locator is not of type ServicesLocator");
            }

            Dictionary<Type, Type> viewViewModelMapper = new Dictionary<Type, Type>();
            viewViewModelMapper.Add(typeof(MainWindowViewModel), typeof(MainWindow));

            var navigationService = new NavigationService(viewViewModelMapper);
            locator.Register(new StandardKernel(), navigationService);

            this.Navigating -= App_Navigating;
        }
    }
}
