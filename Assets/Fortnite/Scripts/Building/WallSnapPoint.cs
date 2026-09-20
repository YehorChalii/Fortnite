using UnityEngine;

public class WallSnapPoint : SnapPoint
{
    private bool _isWallPlaced = false;

    public override bool IsAvailable(BuildingManager.SelectedBuildingPiece buildingPiece)
    {
        return buildingPiece == BuildingManager.SelectedBuildingPiece.Wall && !_isWallPlaced;
    }

    public override void TakeSlot(BuildingManager.SelectedBuildingPiece buildingPiece)
    {
        if (buildingPiece == BuildingManager.SelectedBuildingPiece.Wall) _isWallPlaced = true;
    }

    public override void FreeSlot(BuildingManager.SelectedBuildingPiece buildingPiece)
    {
        if (buildingPiece == BuildingManager.SelectedBuildingPiece.Wall) _isWallPlaced = false;
    }
}
