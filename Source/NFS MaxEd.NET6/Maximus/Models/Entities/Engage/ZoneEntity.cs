namespace Maximus.Models.Entities.Engage;

public class ZoneEntity : BaseEntity
{
    public ZoneEntity(string initialName = "") : base(initialName)
    {
    }
    
    private PointEntity _point;

    public PointEntity Point
    {
        get => _point;
        set => Set(ref _point, value);
    }
    
     private int _radius;
     public int Radius
     {
         get => _radius;
         set => Set(ref _radius, value);
     }

     private int _binIndex;
     public int BinIndex
     {
         get => _binIndex;
         set => Set(ref _binIndex, value);
     }
}