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
    public class ShapeBase
    {
        public ShapeBase()
        {

        }

        public Shape Shape
        {
            get;
            private set;
        }

        public ShapeBase Clone(Brush color, int strokeThickness, ShapeType type, Point position)
        {
            var newShapeBase = new ShapeBase();
            Shape newShape = null;

            switch(type)
            {
                case ShapeType.Ellipse:
                    newShape = new Ellipse();
                    break;

                case ShapeType.Rectangle:
                    newShape = new Rectangle();
                    break;

                case ShapeType.Triangle:
                    newShape = new Polygon();
                    (newShape as Polygon).Points = new PointCollection() { new Point(position.X + 5, position.Y), new Point(position.X, position.Y + 10), new Point(position.X + 10, position.Y + 10) };
                    break;
            }

            newShape.Margin = new Thickness(position.X - 10, position.Y - 10, 0, 0);
            newShape.Width = 10;
            newShape.Height = 10;
            newShape.StrokeThickness = strokeThickness;
            newShape.Stroke = color;
            newShape.Fill = Brushes.Transparent;

            newShapeBase.Shape = newShape;

            return newShapeBase;
        }
    }
}
