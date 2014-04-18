using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace GraphicsEditor.Business.Domain.Models
{
    public abstract class ComponentBase : IComponent
    {
        public Shape SelectionArea
        {
            get;
            protected set;
        }

        public abstract void Add(IComponent Component);

        public abstract void Remove(IComponent Component);

        public abstract void Move(Vector translation);

        public abstract void Resize(Vector translation);
    }
}