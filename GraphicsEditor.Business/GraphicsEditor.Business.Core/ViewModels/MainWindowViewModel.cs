using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphicsEditor.Business.Core.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private bool mouseDown = false;
        private bool ctrlDown = false;
        private bool shiftDown = false;

        private List<IComponent> shapes;

        private Shape selection;
        private Shape currentShape;

        private Point mousePosition;

        Vector vectorToOrigin;

        public ObservableCollection<UIElement> Elements
        {
            get;
            set;
        }

        #region RelayCommands

        public RelayCommand<MouseEventArgs> MouseUpCommand
        {
            get;
            private set;
        }

        public RelayCommand<MouseEventArgs> MouseDownCommand
        {
            get;
            private set;
        }

        public RelayCommand<MouseEventArgs> MouseMoveCommand
        {
            get;
            private set;
        }

        public RelayCommand<KeyEventArgs> KeyUpCommand
        {
            get;
            private set;
        }

        public RelayCommand<KeyEventArgs> KeyDownCommand
        {
            get;
            private set;
        }

        #endregion

        #region ExecuteMouse

        private void ExecuteMouseUp(MouseEventArgs e)
        {
            mouseDown = false;
            currentShape = null;
        }

        private void ExecuteMouseDown(MouseEventArgs e)
        {
            mouseDown = true;
        }

        // mouse move event of the canvas
        private void ExecuteMouseMove(MouseEventArgs e)
        {
            mousePosition = e.GetPosition(null);

            // we are not interested in mouse move events if the mouse is not pressed
            // we just want the current mouse position if we are dragging something (selection != null) so lets exit here
            if (!mouseDown || selection != null)
            {
                return;
            }

            if (currentShape == null)
            {               
                currentShape = new Ellipse
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Margin = new Thickness(mousePosition.X - 10, mousePosition.Y - 10, 0, 0),
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Transparent
                };

                this.Elements.Add(currentShape);

                // register events on our shape
                currentShape.MouseDown += ElementMouseDown;
                currentShape.MouseUp += ElementMouseUp;
                currentShape.MouseMove += ElementMove;
            }

            if (currentShape != null)
            {
                currentShape.Width = mousePosition.X - currentShape.Margin.Left;
                currentShape.Height = mousePosition.Y - currentShape.Margin.Top;
            }
        }
        #endregion

        #region ExecuteKeyUpDown
        private void ExecuteKeyDown(KeyEventArgs e)
        {
           switch(e.Key)
           {
               case Key.LeftCtrl:
                   ctrlDown = true;
                   break;

               case Key.LeftShift:
                   shiftDown = true;
                   break;
           }
        }
        private void ExecuteKeyUp(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.LeftCtrl:
                    ctrlDown = false;
                    break;

                case Key.LeftShift:
                    shiftDown = false;
                    break;
            }
        }
        #endregion

        private void ElementMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.selection = (Shape)sender;
            (sender as Shape).Stroke = Brushes.Red;

            Point origin = new Point(selection.Margin.Left, selection.Margin.Top);
            vectorToOrigin = Point.Subtract(origin, mousePosition);
        }

        private void ElementMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.selection = null;
            (sender as Shape).Stroke = Brushes.Black;

            vectorToOrigin = new Vector(0, 0);
        }

        // move event of every element in the canvas
        private void ElementMove(object sender, MouseEventArgs e)
        {
            if (selection != null)
            {
                Point newPos = Point.Add(mousePosition, vectorToOrigin);
                selection.Margin = new Thickness(newPos.X, newPos.Y, 0, 0);
            }
        }

        public MainWindowViewModel() 
        {
            this.Elements = new ObservableCollection<UIElement>();
            this.shapes = new List<IComponent>();

            this.MouseUpCommand = new RelayCommand<MouseEventArgs>(this.ExecuteMouseUp);
            this.MouseDownCommand = new RelayCommand<MouseEventArgs>(this.ExecuteMouseDown);
            this.MouseMoveCommand = new RelayCommand<MouseEventArgs>(this.ExecuteMouseMove);

            this.KeyDownCommand = new RelayCommand<KeyEventArgs>(this.ExecuteKeyDown);
            this.KeyUpCommand = new RelayCommand<KeyEventArgs>(this.ExecuteKeyUp);
        }
    }
}
