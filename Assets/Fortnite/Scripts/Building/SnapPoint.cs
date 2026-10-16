using UnityEngine;
using static BuildingManager;

public abstract class SnapPoint
{
    public Vector3 Position;
    public Quaternion Rotation;

    public Vector3Int GridCoordinate;
    public bool IsSupported = false;

    public abstract bool IsAvailable(SelectedBuildingPiece selectedPiece);

    public abstract void TakeSlot(SelectedBuildingPiece selectedPiece);

    public abstract void FreeSlot(SelectedBuildingPiece selectedPiece);
}