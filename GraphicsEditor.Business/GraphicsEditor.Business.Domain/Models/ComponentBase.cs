using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphicsEditor.Business.Domain.Models
{
    public abstract class ComponentBase : IComponent
    {
        public ComponentBase()
        {
            this.SelectionArea = new Rectangle();
            this.ResizeArea = new Rectangle();

            this.SelectionArea.Stroke = Brushes.Transparent;
            this.SelectionArea.Fill = Brushes.Transparent;
            this.SelectionArea.StrokeDashArray = new DoubleCollection() { 5, 5 };

            this.ResizeArea.Width = 20;
            this.ResizeArea.Height = 20;
            this.ResizeArea.Stroke = Brushes.Transparent;
            this.ResizeArea.Fill = Brushes.Transparent;
        }

        // represents the clickable rectangle around a component for selection
        public Shape SelectionArea
        {
            get;
            protected set;
        }

        // represents the clickable resize corner of a component in the bottom right of the selection area
        public Shape ResizeArea
        {
            get;
            protected set;
        }

        public void DisplaySelectionArea()
        {
            this.SelectionArea.Stroke = Brushes.Black;
            this.ResizeArea.Stroke = Brushes.Black;
        }

        public void HideSelectionArea()
        {
            this.SelectionArea.Stroke = Brushes.Transparent;
            this.ResizeArea.Stroke = Brushes.Transparent;
        }

        public abstract void Add(IComponent Component);

        public abstract void Remove(IComponent Component);

        public abstract void Move(Vector translation);

        public abstract void Resize(Vector translation);
    }
}