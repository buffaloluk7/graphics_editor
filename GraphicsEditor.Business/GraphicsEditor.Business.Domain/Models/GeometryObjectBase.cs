using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

public abstract class GeometryObjectBase : ICloneable, IComponent, IDrawable
{
    public Shape Shape
    {
        get;
        protected set;
    }

    public GeometryObjectBase()
    {
        Canvas sketchBoard = new Canvas();
        //sketchBoard.Children.Add(new Canvas() as UIElement);
    }

	public virtual ICloneable Clone()
	{
        return this.MemberwiseClone() as ICloneable;
	}

	public abstract void Add(IComponent component);

	public abstract void Remove(IComponent component);

	public abstract void Draw();

    public void Move(Point location)
    {
        this.Shape.Margin = new Thickness(location.X, location.Y, 0, 0);
    }

    public void Resize(Point size)
    {
        this.Shape.Width = size.X;
        this.Shape.Height = size.Y;
    }
}

