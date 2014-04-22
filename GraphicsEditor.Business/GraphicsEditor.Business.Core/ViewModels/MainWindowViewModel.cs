using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GraphicsEditor.Business.Core.Enums;
using GraphicsEditor.Business.Domain;
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

        private ShapeBase shapeBase;

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

        public ObservableCollection<int> AvailableStrokeThickness
        {
            get;
            private set;
        }

        public ObservableCollection<string> AvailableColors
        {
            get;
            private set;
        }

        public String SelectedShapeColor
        {
            get;
            set;
        }

        public ShapeType SelectedShapeType
        {
            get;
            set;
        }

        public int SelectedStrokeThickness
        {
            get;
            set;
        }

        public MainWindowViewModel()
        {
            // holds all elements displayed in the canvas
            this.CanvasElements = new ObservableCollection<UIElement>();

            // holds the relationship between the clickable selection of an element and the element in the hierachy
            this.shapeComponentRelationships = new Dictionary<Shape, ComponentBase>();

            // all selected elements inside a composition
            this.selection = new Composite();

            this.AvailableStrokeThickness = new ObservableCollection<int>();
            this.AvailableColors = new ObservableCollection<string>();

            // fill comboboxes with values
            typeof(Brushes).GetProperties().ToList().ForEach(I => this.AvailableColors.Add(I.Name));
            Enumerable.Range(1, 10).ToList().ForEach(E => this.AvailableStrokeThickness.Add(E));

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

            // add visual selection area and resize rectangle to canvas
            this.CanvasElements.Add(selection.SelectionArea);
            this.CanvasElements.Add(selection.ResizeArea);

            this.shapeBase = new ShapeBase();
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
                this.selection.Components.Clear();
            }
        }

        // mouse move event of the canvas
        private void ExecuteMouseMove(MouseEventArgs e)
        {
            // update position
            trackedMousePosition = e.GetPosition(null);

            // we are not interested in mouse move events if the mouse is not pressed
            // if we are dragging something (this.selection.Children.Count > 0) exit here (we just needed the updated mouse position)
            if (!this.isMouseDown || this.selection.Components.Count > 0)
            {
                return;
            }

            // draw a new shape
            if (this.drawingElement == null && this.resizingElement == null && this.isMouseDown)
            {
                var brush = new BrushConverter().ConvertFromString(this.SelectedShapeColor) as SolidColorBrush;

                var newShapeBase = shapeBase.Clone(brush, this.SelectedStrokeThickness, this.SelectedShapeType, trackedMousePosition);
                var shape = (newShapeBase as ShapeBase).Shape;

                var leaf = new Leaf(shape);

                // register events on the selectionArea (for dragging, selecting)
                leaf.SelectionArea.MouseDown += this.ElementMouseDown;
                leaf.SelectionArea.MouseUp += this.ElementMouseUp;
                leaf.SelectionArea.MouseMove += this.ElementMove;
                leaf.SelectionArea.MouseEnter += this.ElementMouseEnter;
                leaf.SelectionArea.MouseLeave += this.ElementMouseLeave;

                // register events for resizing
                leaf.ResizeArea.MouseDown += this.ElementEnableResizing;
                leaf.ResizeArea.MouseUp += this.ElementDisableResizing;
                leaf.ResizeArea.MouseEnter += this.ElementMouseEnter;
                leaf.ResizeArea.MouseLeave += this.ElementMouseLeave;

                // register the relationship between the selectionArea of the shape and the element in the hierachy
                this.shapeComponentRelationships.Add(leaf.SelectionArea, leaf);
                this.shapeComponentRelationships.Add(leaf.ResizeArea, leaf);

                // add all 3 elements to the canvas
                this.CanvasElements.Add(shape);
                this.CanvasElements.Add(leaf.SelectionArea);
                this.CanvasElements.Add(leaf.ResizeArea);

                // register the created shape for resizing
                this.resizingElement = this.drawingElement = leaf;
            }

            // resize the clicked shape (this.resizingElement gets set onClick on a resize area)
            if (this.resizingElement != null && this.isMouseDown)
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

               case Key.Delete:
                   this.removeComponent(this.selection);
                   this.selection.Components.Clear();
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

        private void removeComponent(ComponentBase component)
        {
            if (component is Leaf)
            {
                this.CanvasElements.Remove((component as Leaf).Shape);
            }
            else if (component is Composite)
            {
                foreach (var child in (component as Composite).Components)
                {
                    this.removeComponent(child as ComponentBase);
                }
            }

            component.HideSelectionArea();

            // do not remove seletion and resize area of our selection composite
            if (component != this.selection)
            {
                this.CanvasElements.Remove(component.SelectionArea);
                this.CanvasElements.Remove(component.ResizeArea);

                this.shapeComponentRelationships.Remove(component.SelectionArea);
                this.shapeComponentRelationships.Remove(component.ResizeArea);
            }
        }

        #region ElementMouseEvents

        // these methods are registered on the selection rectangles

        private void ElementMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.clickedMousePosition = e.GetPosition(null);

            ComponentBase leaf;
            var success = shapeComponentRelationships.TryGetValue(sender as Shape, out leaf);

            if (!success)
            {
                return;
            }

            if (this.selection.Components.Contains(leaf))
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
            // if ctrl is not pressed when releasing the mouse button, discard the current selection
            if (!this.isCtrlDown)
            {
                this.selection.Components.Clear();
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
            var success = shapeComponentRelationships.TryGetValue(sender as Shape, out leaf);

            if (success)
            {
                // hide the visual highlighting
                leaf.HideSelectionArea();
            }
        }

        // display the selection area on hover
        private void ElementMouseEnter(object sender, MouseEventArgs e)
        {
            this.hoverOverAnyElement = true;

            ComponentBase leaf;
            var success = shapeComponentRelationships.TryGetValue(sender as Shape, out leaf);

            if (this.selection.Components.Contains(leaf))
            {
                return;
            }

            if (success)
            {
                // visually highlight the element we are hovering
                leaf.DisplaySelectionArea();
            }
        }

        #endregion

        #region Grouping Commands

        private void ExecuteGroupSelectionCommand()
        {
            if (this.selection.Components.Count == 1)
            {
                System.Diagnostics.Debug.WriteLine("Cannot group 1 item.");
                return;
            }

            Composite composition = new Composite();

            foreach (ComponentBase leaf in this.selection.Components)
            {
                this.shapeComponentRelationships.Remove(leaf.SelectionArea);
                this.shapeComponentRelationships.Remove(leaf.ResizeArea);

                this.CanvasElements.Remove(leaf.SelectionArea);
                this.CanvasElements.Remove(leaf.ResizeArea);

                composition.Add(leaf);
            }

            composition.SelectionArea.MouseDown += this.ElementMouseDown;
            composition.SelectionArea.MouseUp += this.ElementMouseUp;
            composition.SelectionArea.MouseMove += this.ElementMove;
            composition.SelectionArea.MouseEnter += this.ElementMouseEnter;
            composition.SelectionArea.MouseLeave += this.ElementMouseLeave;

            composition.ResizeArea.MouseDown += this.ElementEnableResizing;
            composition.ResizeArea.MouseUp += this.ElementDisableResizing;
            composition.ResizeArea.MouseEnter += this.ElementMouseEnter;
            composition.ResizeArea.MouseLeave += this.ElementMouseLeave;

            this.shapeComponentRelationships.Add(composition.SelectionArea, composition);
            this.shapeComponentRelationships.Add(composition.ResizeArea, composition);

            this.CanvasElements.Add(composition.SelectionArea);
            this.CanvasElements.Add(composition.ResizeArea);

            this.selection.Components.Clear();

            this.selection.Add(composition);
        }

        private void ExecuteUnGroupSelectionCommand()
        {
            List<ComponentBase> selectAfterUngroup = new List<ComponentBase>();

            foreach (ComponentBase component in this.selection.Components)
            {
                if (component is Composite)
                {
                    foreach (ComponentBase leaf in (component as Composite).Components)
                    {
                        this.shapeComponentRelationships.Add(leaf.SelectionArea, leaf);
                        this.shapeComponentRelationships.Add(leaf.ResizeArea, leaf);

                        this.CanvasElements.Add(leaf.SelectionArea);
                        this.CanvasElements.Add(leaf.ResizeArea);

                        selectAfterUngroup.Add(leaf);
                    }

                    this.shapeComponentRelationships.Remove(component.SelectionArea);
                    this.shapeComponentRelationships.Remove(component.ResizeArea);

                    this.CanvasElements.Remove(component.SelectionArea);
                    this.CanvasElements.Remove(component.ResizeArea);
                }
            }

            this.selection.Components.Clear();

            selectAfterUngroup.ForEach(E => this.selection.Components.Add(E));
        }

        #endregion
    }
}
