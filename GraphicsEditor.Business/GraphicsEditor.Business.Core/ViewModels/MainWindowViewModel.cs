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
using GraphicsEditor.Business.Domain.Models;
using GraphicsEditor.Business.Core.Extensions;

namespace GraphicsEditor.Business.Core.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private bool isMouseDown = false;
        private bool isCtrlDown = false;
        private bool isShiftDown = false;
        private bool hoverOverAnyElement = false;

        private Dictionary<Shape, ComponentBase> shapes;

        private List<Shape> selection;

        private Shape currentShape;

        private Point clickedMousePosition;
        private Point mousePosition;

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
            isMouseDown = false;
            currentShape = null;
        }

        private void ExecuteMouseDown(MouseEventArgs e)
        {
            isMouseDown = true;

            if (!hoverOverAnyElement)
            {
                this.clearSelection();
            }
        }

        // mouse move event of the canvas
        private void ExecuteMouseMove(MouseEventArgs e)
        {
            // update position
            mousePosition = e.GetPosition(null);

            // we are not interested in mouse move events if the mouse is not pressed
            // we just want the current mouse position if we are dragging something (selection != null) so lets exit here
            if (!isMouseDown || selection.Count > 0)
            {
                return;
            }

            if (this.currentShape == null)
            {               
                var shape = new Ellipse
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Margin = new Thickness(mousePosition.X - 10, mousePosition.Y - 10, 0, 0),
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Transparent
                };

                var leaf = new Leaf(shape);

                // register events on the selectionArea
                leaf.SelectionArea.MouseDown += ElementMouseDown;
                leaf.SelectionArea.MouseUp += ElementMouseUp;
                leaf.SelectionArea.MouseMove += ElementMove;

                leaf.SelectionArea.MouseEnter += ElementMouseEnter;
                leaf.SelectionArea.MouseLeave += ElementMouseLeave;

                // make the selectionarea invisible
                leaf.SelectionArea.Stroke = Brushes.Transparent;

                // register the relationship between the selectionArea of the shape and the element in the hierachy
                shapes.Add(leaf.SelectionArea, leaf);

                // add both elements to the canvas
                this.Elements.Add(shape);
                this.Elements.Add(leaf.SelectionArea);

                this.currentShape = leaf.SelectionArea;
            }

            if (currentShape != null)
            {
                // find component for selection
                ComponentBase component;
                shapes.TryGetValue(currentShape, out component);

                Point topRight = new Point(currentShape.Margin.Left, currentShape.Margin.Top);

                Vector toBottomRight = new Vector(currentShape.Width, currentShape.Height);
                Vector topRightToMouse = Point.Subtract(mousePosition, topRight);
                Vector translation;

                // force square if shift down
                if (isShiftDown)
                {
                    topRightToMouse.Y = topRightToMouse.X;
                }

                translation = Vector.Subtract(topRightToMouse, toBottomRight);

                // get the current absolute position of the bottom right corner of the selectionArea
                component.Resize(translation);
            }
        }


        #endregion

        #region ExecuteKeyUpDown
        private void ExecuteKeyDown(KeyEventArgs e)
        {
           switch(e.Key)
           {
               case Key.LeftCtrl:
                   isCtrlDown = true;
                   break;

               case Key.LeftShift:
                   isShiftDown = true;
                   break;
           }
        }
        private void ExecuteKeyUp(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.LeftCtrl:
                    isCtrlDown = false;
                    break;

                case Key.LeftShift:
                    isShiftDown = false;
                    break;
            }
        }
        #endregion

        // these methods are only registered on the selection rectangles
        private void ElementMouseDown(object sender, MouseButtonEventArgs e)
        {
            clickedMousePosition = e.GetPosition(null);
            var shape = sender as Shape;

            if (selection.Contains(shape))
            {
                return;
            }

            shape.Selected();
            selection.Add(shape);
        }

        private void ElementMouseUp(object sender, MouseButtonEventArgs e)
        {
            var shape = sender as Shape;

            if (!isCtrlDown)
            {
                this.clearSelection();
            }
        }

        private void ElementMove(object sender, MouseEventArgs e)
        {
            // just hovering over an element? get out of here
            if (!isMouseDown)
            {
                return;
            }

            Vector translation = Point.Subtract(mousePosition, clickedMousePosition);

            foreach(Shape shape in selection)
            {
                ComponentBase component;
                shapes.TryGetValue(shape, out component);

                component.Move(translation);
            }

            clickedMousePosition = mousePosition;
        }

        // disable the selection on leave
        private void ElementMouseLeave(object sender, MouseEventArgs e)
        {
            this.hoverOverAnyElement = false;
            var shape = sender as Shape;

            // if shape is selected, do not remove visual selection from shape
            if (selection.Contains(shape))
            {
                return;
            }

            shape.DeSelected();
        }

        // display the selection on hover
        private void ElementMouseEnter(object sender, MouseEventArgs e)
        {
            this.hoverOverAnyElement = true;
            (sender as Shape).Selected();
        }


        private void ExecuteGroupSelectionCommand()
        {

        }

        private void ExecuteUnGroupSelectionCommand()
        {

        }

        private void clearSelection()
        {
            foreach (var el in this.selection)
            {
                el.DeSelected();
            }

            this.selection.Clear();
        }

        public MainWindowViewModel() 
        {
            // holds all elements displayed in the canvas
            this.Elements = new ObservableCollection<UIElement>();

            // holds the currently selected elements
            this.selection = new List<Shape>();

            // holds the relationships between selection rectangles and elements in the hierachy
            this.shapes = new Dictionary<Shape, ComponentBase>();

            // mouse commands
            this.MouseUpCommand = new RelayCommand<MouseEventArgs>(this.ExecuteMouseUp);
            this.MouseDownCommand = new RelayCommand<MouseEventArgs>(this.ExecuteMouseDown);
            this.MouseMoveCommand = new RelayCommand<MouseEventArgs>(this.ExecuteMouseMove);

            // key commands
            this.KeyDownCommand = new RelayCommand<KeyEventArgs>(this.ExecuteKeyDown);
            this.KeyUpCommand = new RelayCommand<KeyEventArgs>(this.ExecuteKeyUp);

            // button commands
            this.GroupSelectionCommand = new RelayCommand(this.ExecuteGroupSelectionCommand);
            this.UnGroupSelectionCommand = new RelayCommand(this.ExecuteUnGroupSelectionCommand);
        }
    }
}
