using UnityEngine;

public abstract class SnapPoint
{
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3Int GridCoordinate { get; set; }
    public bool IsSupported { get; set; } = false;

    public abstract bool IsAvailable(SelectedBuildingPiece selectedPiece);

    public abstract void TakeSlot(SelectedBuildingPiece selectedPiece);

    public abstract void FreeSlot(SelectedBuildingPiece selectedPiece);
}