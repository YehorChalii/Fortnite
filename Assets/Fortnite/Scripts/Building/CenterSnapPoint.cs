using UnityEngine;

public class CenterSnapPoint : SnapPoint
{
    private bool _isRampPlaced = false;
    private bool _isFloorPlaced = false;

    public override bool IsAvailable(BuildingManager.SelectedBuildingPiece buildingPiece)
    {
        if(buildingPiece == BuildingManager.SelectedBuildingPiece.Ramp)
        {
            return !_isRampPlaced;
        }
        if (buildingPiece == BuildingManager.SelectedBuildingPiece.Floor)
        {
            return !_isFloorPlaced;
        }
        else return false;
    }

    public override void TakeSlot(BuildingManager.SelectedBuildingPiece buildingPiece)
    {
        if (buildingPiece == BuildingManager.SelectedBuildingPiece.Ramp) _isRampPlaced = true;
        if (buildingPiece == BuildingManager.SelectedBuildingPiece.Floor) _isFloorPlaced = true;
    }

    public override void FreeSlot(BuildingManager.SelectedBuildingPiece buildingPiece)
    {
        if (buildingPiece == BuildingManager.SelectedBuildingPiece.Ramp) _isRampPlaced = false;
        if (buildingPiece == BuildingManager.SelectedBuildingPiece.Floor) _isFloorPlaced = false;
    }
}
