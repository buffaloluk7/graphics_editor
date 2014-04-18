using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Linq;
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

        private Dictionary<Shape, Vector> selection;

        private Shape currentShape;

        private Point mousePosition;

        public ObservableCollection<string> Selection
        {
            get;
            set;
        }

        public ObservableCollection<UIElement> Elements
        {
            get;
            set;
        }

        #region Commands

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

        public RelayCommand GroupSelectionCommand
        {
            get;
            private set;
        }

        public RelayCommand UnGroupSelectionCommand
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
            if (!mouseDown || this.selection.Count > 0)
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
                var holder = new ShapeBase(currentShape);

                shapes.Add(holder);

                // register events on our shape
                currentShape.MouseDown += ElementMouseDown;
                currentShape.MouseUp += ElementMouseUp;
                currentShape.MouseMove += ElementMove;
            }

            if (currentShape != null)
            {
                var w = mousePosition.X - currentShape.Margin.Left;

                currentShape.Width = (w < 0) ? currentShape.Width : w;

                if (shiftDown)
                {
                    var h = mousePosition.X - currentShape.Margin.Left;
                    currentShape.Height = (h < 0) ? currentShape.Height : h;
                }
                else
                {
                    var h = mousePosition.Y - currentShape.Margin.Top;
                    currentShape.Height = (h < 0) ? currentShape.Height : h;
                }
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
            // search clicked element in hierachie -> how ??
            // outline all elements of composition -> how ?? :D
            var clickedShape = sender as Shape;

            if (ctrlDown)
            {
                if (this.selection.ContainsKey(clickedShape))
                {
                    this.removeFromSelection(clickedShape);
                    return;
                }
            }
            else
            {
                this.clearSelection();
            }

            this.addToSelection(clickedShape);
        }

        private void ElementMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ctrlDown)
            {
                return;
            }

            this.clearSelection();
        }

          // move event of every element in the canvas
        private void ElementMove(object sender, MouseEventArgs e)
        {
            if (ctrlDown)
            {
                return;
            }

            if (this.selection.Count > 1)
            {
                return;
            }

            if (this.selection.Count > 0)
            {
                foreach(var el in selection)
                {
                    Point newPos = Point.Add(mousePosition, el.Value);
                    el.Key.Margin = new Thickness(newPos.X, newPos.Y, 0, 0);
                }
            }
        }

        private void removeFromSelection(Shape shape)
        {
            this.selection.Remove(shape);
            this.Selection.Remove(shape.ToString());
            shape.Stroke = Brushes.Black;
        }

        private void addToSelection(Shape shape)
        {
            Point origin = new Point(shape.Margin.Left, shape.Margin.Top);
            Vector vectorToOrigin = Point.Subtract(origin, mousePosition);

            this.selection.Add(shape, vectorToOrigin);
            this.Selection.Add(shape.ToString());
            shape.Stroke = Brushes.Red;
        }

        private void clearSelection()
        {
            foreach (var el in this.selection)
            {
                el.Key.Stroke = Brushes.Black;
            }

            this.selection.Clear();
            this.Selection.Clear();
        }

        private void ExecuteGroupSelectionCommand()
        {
            Composite composite = new Composite();

            foreach(var el in this.selection)
            {
                var component = this.shapes.FirstOrDefault(S => (S as ShapeBase).Shape == el.Key);

                this.shapes.Remove(component);
                composite.Add(component);
            }

            shapes.Add(composite);
        }

        private void ExecuteUnGroupSelectionCommand()
        {

        }

        public MainWindowViewModel() 
        {
            this.Elements = new ObservableCollection<UIElement>();
            this.Selection = new ObservableCollection<string>();
            this.shapes = new List<IComponent>();
            this.selection = new Dictionary<Shape, Vector>();

            this.MouseUpCommand = new RelayCommand<MouseEventArgs>(this.ExecuteMouseUp);
            this.MouseDownCommand = new RelayCommand<MouseEventArgs>(this.ExecuteMouseDown);
            this.MouseMoveCommand = new RelayCommand<MouseEventArgs>(this.ExecuteMouseMove);

            this.KeyDownCommand = new RelayCommand<KeyEventArgs>(this.ExecuteKeyDown);
            this.KeyUpCommand = new RelayCommand<KeyEventArgs>(this.ExecuteKeyUp);

            this.GroupSelectionCommand = new RelayCommand(this.ExecuteGroupSelectionCommand);
            this.UnGroupSelectionCommand = new RelayCommand(this.ExecuteUnGroupSelectionCommand);
        }
    }
}
