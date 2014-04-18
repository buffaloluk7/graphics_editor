using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphicsEditor.Business.Core.Extensions
{
    public static class ShapeExtension
    {
        public static void DisplaySelectionArea(this Shape shape)
        {
            shape.Stroke = Brushes.Black;
        }

        public static void HideSelectionArea(this Shape shape)
        {
            shape.Stroke = Brushes.Transparent;
        }
    }
}
