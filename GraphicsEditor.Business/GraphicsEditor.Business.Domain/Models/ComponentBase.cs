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
            this.ResizeRectangle = new Rectangle();

            this.SelectionArea.Fill = Brushes.Transparent;
            this.SelectionArea.StrokeDashArray = new DoubleCollection() { 5, 5 };

            this.ResizeRectangle.Width = 20;
            this.ResizeRectangle.Height = 20;
            this.ResizeRectangle.Fill = Brushes.Transparent;
            this.ResizeRectangle.StrokeThickness = 1;
        }

        public Shape SelectionArea
        {
            get;
            protected set;
        }

        public Shape ResizeRectangle
        {
            get;
            protected set;
        }

        public void DisplaySelectionArea()
        {
            this.SelectionArea.Stroke = Brushes.Black;
            this.ResizeRectangle.Stroke = Brushes.Black;
        }

        public void HideSelectionArea()
        {
            this.SelectionArea.Stroke = Brushes.Transparent;
            this.ResizeRectangle.Stroke = Brushes.Transparent;
        }

        public abstract void Add(IComponent Component);

        public abstract void Remove(IComponent Component);

        public abstract void Move(Vector translation);

        public abstract void Resize(Vector translation);
    }
}