namespace Maximus.Models.Entities.Engage;

public class EngageEntity : BaseEntity
{
    
    public EngageEntity(string initialName = "") : base(initialName)
    {
    }
    private PointEntity _point;
    public PointEntity Point
    {
        get => _point;
        set => Set(ref _point, value);
    }

    private int _bounty;
    public int Bounty
    {
        get => _bounty;
        set => Set(ref _bounty, value);
    }
    private int _thresholdSpeed;
    public int ThresholdSpeed
    {
        get => _thresholdSpeed;
        set => Set(ref _thresholdSpeed, value);
    }

    private string _spawnPoint;
    public string SpawnPoint
    {
        get => _spawnPoint;
        set => Set(ref _spawnPoint, value);
    }
}