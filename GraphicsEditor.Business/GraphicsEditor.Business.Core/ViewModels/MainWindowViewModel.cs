using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GraphicsEditor.Business.Domain.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

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

        private Composite selection;
  
        private ComponentBase resizingElement;
        private ComponentBase drawingElement;

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
            //this.selectedAreas = new List<Shape>();

            // holds the relationships between selection rectangles and elements in the hierachy
            this.shapeComponentRelationships = new Dictionary<Shape, ComponentBase>();

            this.selection = new Composite();

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

            this.CanvasElements.Add(selection.SelectionArea);
            this.CanvasElements.Add(selection.ResizeRectangle);
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
            this.drawingElement = null;
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
            // we just want the current mouse position if we are dragging something (this.selection.Children.Count > 0) so lets exit here
            if (!this.isMouseDown || this.selection.Children.Count > 0)
            {
                return;
            }

            if (this.drawingElement == null && this.resizingElement == null)
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

                leaf.ResizeRectangle.MouseDown += this.ElementEnableResizing;
                leaf.ResizeRectangle.MouseUp += this.ElementDisableResizing;
                leaf.ResizeRectangle.MouseEnter += this.ElementMouseEnter;
                leaf.ResizeRectangle.MouseLeave += this.ElementMouseLeave;

                // register the relationship between the selectionArea of the shape and the element in the hierachy
                this.shapeComponentRelationships.Add(leaf.SelectionArea, leaf);
                this.shapeComponentRelationships.Add(leaf.ResizeRectangle, leaf);

                // add all 3 elements to the canvas
                this.CanvasElements.Add(shape);
                this.CanvasElements.Add(leaf.SelectionArea);
                this.CanvasElements.Add(leaf.ResizeRectangle);

                this.drawingElement = leaf;
                this.resizingElement = leaf;
            }

            if (this.resizingElement != null)
            {
                Point topLeft = new Point(this.resizingElement.SelectionArea.Margin.Left, this.resizingElement.SelectionArea.Margin.Top);

                Vector topLeftToBottomRight = new Vector(this.resizingElement.SelectionArea.Width, this.resizingElement.SelectionArea.Height);
                Vector topLeftToMouse = Point.Subtract(trackedMousePosition, topLeft);

                // force 1:1 aspect ratio
                if (this.isShiftDown)
                {
                    topLeftToMouse.Y = topLeftToMouse.X;
                }

                Vector translation = Vector.Subtract(topLeftToMouse, topLeftToBottomRight);

                // get the current absolute position of the bottom right corner of the selectionArea
                this.resizingElement.Resize(translation);
            }
        }

        private void ElementDisableResizing(object sender, MouseButtonEventArgs e)
        {
            this.resizingElement = null;
        }

        private void ElementEnableResizing(object sender, MouseButtonEventArgs e)
        {
            var shape = sender as Shape;
            this.shapeComponentRelationships.TryGetValue(shape, out this.resizingElement);
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

            ComponentBase leaf;
            shapeComponentRelationships.TryGetValue(sender as Shape, out leaf);

            if (this.selection.Children.Contains(leaf))
            {
                // CtrlDown -> modifying selection
                if (isCtrlDown)
                {
                    this.selection.Remove(leaf);
                }
            }
            else
            {
                this.selection.Add(leaf);
            }
        }

        private void ElementMouseUp(object sender, MouseButtonEventArgs e)
        {
            ComponentBase leaf;
            shapeComponentRelationships.TryGetValue(sender as Shape, out leaf);

            // if ctrl is not pressed when releasing the mouse button, discard the current selection
            if (!this.isCtrlDown)
            {
                this.clearSelection();
            }
        }

        private void ElementMove(object sender, MouseEventArgs e)
        {
            // just hovering over an element? get out of here
            if (!this.isMouseDown)
            {
                return;
            }

            Vector translation = Point.Subtract(this.trackedMousePosition, this.clickedMousePosition);

            this.selection.Move(translation);

            this.clickedMousePosition = this.trackedMousePosition;
        }

        // hide the selection area on mouse leave
        private void ElementMouseLeave(object sender, MouseEventArgs e)
        {
            this.hoverOverAnyElement = false;

            ComponentBase leaf;
            shapeComponentRelationships.TryGetValue(sender as Shape, out leaf);

            leaf.HideSelectionArea();
        }

        // display the selection area on hover
        private void ElementMouseEnter(object sender, MouseEventArgs e)
        {
            this.hoverOverAnyElement = true;

            ComponentBase leaf;
            shapeComponentRelationships.TryGetValue(sender as Shape, out leaf);

            if (this.selection.Children.Contains(leaf))
            {
                return;
            }

            leaf.DisplaySelectionArea();
        }

        #endregion

        #region Grouping Commands

        private void ExecuteGroupSelectionCommand()
        {
            if (this.selection.Children.Count == 1)
            {
                System.Diagnostics.Debug.WriteLine("Cannot group 1 item.");
                //return;
            }

            Composite composition = new Composite();

            foreach (ComponentBase leaf in this.selection.Children)
            {
                this.shapeComponentRelationships.Remove(leaf.SelectionArea);
                this.shapeComponentRelationships.Remove(leaf.ResizeRectangle);

                this.CanvasElements.Remove(leaf.SelectionArea);
                this.CanvasElements.Remove(leaf.ResizeRectangle);

                composition.Add(leaf);
            }

            composition.SelectionArea.MouseDown += this.ElementMouseDown;
            composition.SelectionArea.MouseUp += this.ElementMouseUp;
            composition.SelectionArea.MouseMove += this.ElementMove;
            composition.SelectionArea.MouseEnter += this.ElementMouseEnter;
            composition.SelectionArea.MouseLeave += this.ElementMouseLeave;

            composition.ResizeRectangle.MouseDown += this.ElementEnableResizing;
            composition.ResizeRectangle.MouseUp += this.ElementDisableResizing;
            composition.ResizeRectangle.MouseEnter += this.ElementMouseEnter;
            composition.ResizeRectangle.MouseLeave += this.ElementMouseLeave;

            this.shapeComponentRelationships.Add(composition.SelectionArea, composition);
            this.shapeComponentRelationships.Add(composition.ResizeRectangle, composition);

            this.CanvasElements.Add(composition.SelectionArea);
            this.CanvasElements.Add(composition.ResizeRectangle);

            this.clearSelection();

            this.selection.Add(composition);
        }

        private void ExecuteUnGroupSelectionCommand()
        {
            List<ComponentBase> selectAfterUngroup = new List<ComponentBase>();

            foreach (ComponentBase component in this.selection.Children)
            {
                if (component is Composite)
                    {
                        foreach (ComponentBase leaf in (component as Composite).Children)
                        {
                            this.shapeComponentRelationships.Add(leaf.SelectionArea, leaf);
                            this.shapeComponentRelationships.Add(leaf.ResizeRectangle, leaf);

                            this.CanvasElements.Add(leaf.SelectionArea);
                            this.CanvasElements.Add(leaf.ResizeRectangle);

                            selectAfterUngroup.Add(leaf);
                        }

                        this.shapeComponentRelationships.Remove(component.SelectionArea);
                        this.shapeComponentRelationships.Remove(component.ResizeRectangle);

                        this.CanvasElements.Remove(component.SelectionArea);
                        this.CanvasElements.Remove(component.ResizeRectangle);
                    }
            }

            this.clearSelection();

            selectAfterUngroup.ForEach(E => this.selection.Children.Add(E));
        }

        #endregion

        private void clearSelection()
        {
            this.selection.Children.Clear();
        }
    }
}
