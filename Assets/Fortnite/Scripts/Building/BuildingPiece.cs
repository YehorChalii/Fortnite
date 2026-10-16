using UnityEngine;

public class BuildingPiece : MonoBehaviour
{
    public SnapPoint SnapPoint { get; private set; }
    public SelectedBuildingPiece BuildingPieceType { get; private set; }
    public BuildingMaterial BuildingMaterial { get; private set; }

    public void AssignSnapPoint(SnapPoint snapPoint, SelectedBuildingPiece buildingPiece, BuildingMaterial buildingMaterial)
    {
        SnapPoint = snapPoint;
        BuildingPieceType = buildingPiece;
        BuildingMaterial = buildingMaterial;
    }
}
