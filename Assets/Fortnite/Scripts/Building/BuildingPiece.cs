using UnityEngine;

public class BuildingPiece : MonoBehaviour
{
    public SnapPoint SnapPoint { get; private set; }
    public BuildingManager.SelectedBuildingPiece BuildingPieceType { get; private set; }
    public BuildingManager.BuildingMaterial BuildingMaterial { get; private set; }

    public void AssignSnapPoint(SnapPoint snapPoint, BuildingManager.SelectedBuildingPiece buildingPiece, BuildingManager.BuildingMaterial buildingMaterial)
    {
        SnapPoint = snapPoint;
        BuildingPieceType = buildingPiece;
        BuildingMaterial = buildingMaterial;
    }
}
