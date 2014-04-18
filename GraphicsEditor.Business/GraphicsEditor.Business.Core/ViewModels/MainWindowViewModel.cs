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
        #region Attributes

        private bool isMouseDown = false;
        private bool isCtrlDown = false;
        private bool isShiftDown = false;
        private bool hoverOverAnyElement = false;

        private Dictionary<Shape, ComponentBase> shapeComponentRelationships;

        private List<Shape> selectedAreas;

        private Shape currentShape;

        private Point clickedMousePosition;
        private Point trackedMousePosition;

        #endregion

        public ObservableCollection<UIElement> CanvasElements
        {
            get;
            set;
        }

        public MainWindowViewModel()
        {
            // holds all elements displayed in the canvas
            this.CanvasElements = new ObservableCollection<UIElement>();

            // holds the currently selected elements
            this.selectedAreas = new List<Shape>();

            // holds the relationships between selection rectangles and elements in the hierachy
            this.shapeComponentRelationships = new Dictionary<Shape, ComponentBase>();

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
            this.isMouseDown = false;
            this.currentShape = null;
        }

        private void ExecuteMouseDown(MouseEventArgs e)
        {
            this.isMouseDown = true;

            if (!this.hoverOverAnyElement)
            {
                this.clearSelection();
            }
        }

        // mouse move event of the canvas
        private void ExecuteMouseMove(MouseEventArgs e)
        {
            // update position
            trackedMousePosition = e.GetPosition(null);

            // we are not interested in mouse move events if the mouse is not pressed
            // we just want the current mouse position if we are dragging something (selection != null) so lets exit here
            if (!this.isMouseDown || this.selectedAreas.Count > 0)
            {
                return;
            }

            if (this.currentShape == null)
            {               
                var shape = new Ellipse
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Margin = new Thickness(trackedMousePosition.X - 10, trackedMousePosition.Y - 10, 0, 0),
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Transparent
                };

                var leaf = new Leaf(shape);

                // register events on the selectionArea
                leaf.SelectionArea.MouseDown += this.ElementMouseDown;
                leaf.SelectionArea.MouseUp += this.ElementMouseUp;
                leaf.SelectionArea.MouseMove += this.ElementMove;
                leaf.SelectionArea.MouseEnter += this.ElementMouseEnter;
                leaf.SelectionArea.MouseLeave += this.ElementMouseLeave;

                // make the selectionarea invisible
                leaf.SelectionArea.Stroke = Brushes.Transparent;

                // register the relationship between the selectionArea of the shape and the element in the hierachy
                this.shapeComponentRelationships.Add(leaf.SelectionArea, leaf);

                // add both elements to the canvas
                this.CanvasElements.Add(shape);
                this.CanvasElements.Add(leaf.SelectionArea);

                this.currentShape = leaf.SelectionArea;
            }

            if (currentShape != null)
            {
                // find component for selection
                ComponentBase component;
                this.shapeComponentRelationships.TryGetValue(currentShape, out component);

                Point topRight = new Point(currentShape.Margin.Left, currentShape.Margin.Top);

                Vector toBottomRight = new Vector(currentShape.Width, currentShape.Height);
                Vector topRightToMouse = Point.Subtract(trackedMousePosition, topRight);

                // force aspect ratio if shift is down
                if (this.isShiftDown)
                {
                    topRightToMouse.Y = topRightToMouse.X;
                }

                Vector translation = Vector.Subtract(topRightToMouse, toBottomRight);

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

        
        #region ElementMouseEvents

        // these methods are registered on the selection rectangles

        private void ElementMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.clickedMousePosition = e.GetPosition(null);
            var shape = sender as Shape;

            if (this.selectedAreas.Contains(shape))
            {
                return;
            }

            shape.DisplaySelectionArea();
            this.selectedAreas.Add(shape);
        }

        private void ElementMouseUp(object sender, MouseButtonEventArgs e)
        {
            var shape = sender as Shape;

            if (!this.isCtrlDown)
            {
                this.clearSelection();
            }

            shape.DisplaySelectionArea();
        }

        private void ElementMove(object sender, MouseEventArgs e)
        {
            // just hovering over an element? get out of here
            if (!this.isMouseDown)
            {
                return;
            }

            Vector translation = Point.Subtract(this.trackedMousePosition, this.clickedMousePosition);

            foreach (Shape shape in this.selectedAreas)
            {
                ComponentBase component;
                var success = this.shapeComponentRelationships.TryGetValue(shape, out component);

                if (success)
                {
                    component.Move(translation);
                }
            }

            this.clickedMousePosition = this.trackedMousePosition;
        }

        // hide the selection area on mouse leave
        private void ElementMouseLeave(object sender, MouseEventArgs e)
        {
            this.hoverOverAnyElement = false;
            var shape = sender as Shape;

            // if the shape is selected, do not remove visual selection from shape
            if (this.selectedAreas.Contains(shape))
            {
                return;
            }

            shape.HideSelectionArea();
        }

        // display the selection area on hover
        private void ElementMouseEnter(object sender, MouseEventArgs e)
        {
            this.hoverOverAnyElement = true;
            (sender as Shape).DisplaySelectionArea();
        }

        #endregion

        #region Grouping Commands

        private void ExecuteGroupSelectionCommand()
        {
            Composite composition = new Composite();

            foreach (Shape selectionArea in this.selectedAreas)
            {
                ComponentBase leaf;
                var success = this.shapeComponentRelationships.TryGetValue(selectionArea, out leaf);

                if (success)
                {
                    composition.Add(leaf);
                    this.shapeComponentRelationships.Remove(selectionArea);
                    this.CanvasElements.Remove(leaf.SelectionArea);
                }
            }

            composition.SelectionArea.MouseDown += this.ElementMouseDown;
            composition.SelectionArea.MouseUp += this.ElementMouseUp;
            composition.SelectionArea.MouseMove += this.ElementMove;
            composition.SelectionArea.MouseEnter += this.ElementMouseEnter;
            composition.SelectionArea.MouseLeave += this.ElementMouseLeave;

            this.shapeComponentRelationships.Add(composition.SelectionArea, composition);
            this.CanvasElements.Add(composition.SelectionArea);
        }

        private void ExecuteUnGroupSelectionCommand()
        {
            foreach(Shape shape in selectedAreas)
            {
                ComponentBase composition;
                var success = this.shapeComponentRelationships.TryGetValue(shape, out composition);

                if (success)
                {
                    if (composition is Composite)
                    {
                        foreach(ComponentBase component in (composition as Composite).Children)
                        {
                            this.shapeComponentRelationships.Add(component.SelectionArea, component);
                            this.CanvasElements.Add(component.SelectionArea);
                        }

                        this.shapeComponentRelationships.Remove(composition.SelectionArea);
                        this.CanvasElements.Remove(composition.SelectionArea);
                    }
                }
            }
        }

        #endregion

        private void clearSelection()
        {
            foreach (var el in this.selectedAreas)
            {
                el.HideSelectionArea();
            }

            this.selectedAreas.Clear();
        }
    }
}
