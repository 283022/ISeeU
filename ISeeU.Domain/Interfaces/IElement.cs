namespace ISeeU.Domain.Interfaces;

public interface IElement
{
    string Name { get; }
    int ProcessId { get; }
    int ControlType { get; }
    //Rect BoundingRectangle { get; }
    string[] GetSupportedProperties();
}

//class Rect need for Painting Element on a board

/*
public struct Rect
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
*/