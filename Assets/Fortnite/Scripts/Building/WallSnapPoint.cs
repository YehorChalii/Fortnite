using UnityEngine;

public class WallSnapPoint : SnapPoint
{
    private bool _isWallPlaced = false;

    public override bool IsAvailable(SelectedBuildingPiece buildingPiece)
    {
        return buildingPiece == SelectedBuildingPiece.Wall && !_isWallPlaced;
    }

    public override void TakeSlot(SelectedBuildingPiece buildingPiece)
    {
        if (buildingPiece == SelectedBuildingPiece.Wall) _isWallPlaced = true;
    }

    public override void FreeSlot(SelectedBuildingPiece buildingPiece)
    {
        if (buildingPiece == SelectedBuildingPiece.Wall) _isWallPlaced = false;
    }
}
