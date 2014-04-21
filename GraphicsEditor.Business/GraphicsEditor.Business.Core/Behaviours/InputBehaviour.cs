using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GraphicsEditor.Business.Core.Behaviours
{
    public class InputBehaviour
    {
        #region KeyDown

        public static readonly DependencyProperty KeyDownCommandProperty =
    DependencyProperty.RegisterAttached("KeyDownCommand", typeof(ICommand), typeof(InputBehaviour), new FrameworkPropertyMetadata(new PropertyChangedCallback(KeyDownCommandChanged)));

        private static void KeyDownCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;

            element.KeyDown += element_KeyDown;
        }

        static void element_KeyDown(object sender, KeyEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;

            ICommand command = GetKeyDownCommand(element);

            command.Execute(e);
        }

        public static void SetKeyDownCommand(UIElement element, ICommand value)
        {
            element.SetValue(KeyDownCommandProperty, value);
        }

        public static ICommand GetKeyDownCommand(UIElement element)
        {
            return (ICommand)element.GetValue(KeyDownCommandProperty);
        }

        #endregion

        #region KeyUp

        public static readonly DependencyProperty KeyUpCommandProperty =
    DependencyProperty.RegisterAttached("KeyUpCommand", typeof(ICommand), typeof(InputBehaviour), new FrameworkPropertyMetadata(new PropertyChangedCallback(KeyUpCommandChanged)));

        private static void KeyUpCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)d;

            element.KeyUp += element_KeyUp;
        }

        static void element_KeyUp(object sender, KeyEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;

            ICommand command = GetKeyUpCommand(element);

            command.Execute(e);
        }

        public static void SetKeyUpCommand(UIElement element, ICommand value)
        {
            element.SetValue(KeyUpCommandProperty, value);
        }

        public static ICommand GetKeyUpCommand(UIElement element)
        {
            return (ICommand)element.GetValue(KeyUpCommandProperty);
        }

        #endregion
    }
}
