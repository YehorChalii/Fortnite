using UnityEngine;

public class CenterSnapPoint : SnapPoint
{
    private bool _isRampPlaced = false;
    private bool _isFloorPlaced = false;

    public override bool IsAvailable(SelectedBuildingPiece buildingPiece)
    {
        if(buildingPiece == SelectedBuildingPiece.Ramp)
        {
            return !_isRampPlaced;
        }
        if (buildingPiece == SelectedBuildingPiece.Floor)
        {
            return !_isFloorPlaced;
        }
        else return false;
    }

    public override void TakeSlot(SelectedBuildingPiece buildingPiece)
    {
        if (buildingPiece == SelectedBuildingPiece.Ramp) _isRampPlaced = true;
        if (buildingPiece == SelectedBuildingPiece.Floor) _isFloorPlaced = true;
    }

    public override void FreeSlot(SelectedBuildingPiece buildingPiece)
    {
        if (buildingPiece == SelectedBuildingPiece.Ramp) _isRampPlaced = false;
        if (buildingPiece == SelectedBuildingPiece.Floor) _isFloorPlaced = false;
    }
}
