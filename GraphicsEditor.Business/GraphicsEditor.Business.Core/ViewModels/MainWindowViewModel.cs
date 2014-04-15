using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GraphicsEditor.Business.Core.Common;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphicsEditor.Business.Core.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public ObservableCollection<UIElement> Elements
        {
            get;
            set;
        }

        public RelayCommand BackgroundCommand
        {
            get;
            private set;
        }

        public MouseRelayCommand MouseUpCommand
        {
            get;
            private set;
        }

        public MainWindowViewModel() 
        {
            this.Elements = new ObservableCollection<UIElement>();
            this.BackgroundCommand = new RelayCommand(onBackgroundCommandExecuted);
            this.MouseUpCommand = new MouseRelayCommand(param => ExecuteMouseUp((MouseEventArgs)param));
        }

        private void ExecuteMouseUp(MouseEventArgs e)
        {
            Console.WriteLine("Mouse Move : " + e.GetPosition((IInputElement)e.Source));
        }

        private void onBackgroundCommandExecuted()
        {
            Ellipse e = new Ellipse
            {
                Stroke = Brushes.Black,
                StrokeThickness = 50,
                Margin = new Thickness(10.0, 10.0, 0, 0)
            };
            this.Elements.Add(e);
        }
    }
}
