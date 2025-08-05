using Godot;
using System;

public partial class DrawLines : Node
{
    void Start()
    {
    }

    public void DrawLine(Vector3 a, Vector3 b)
    {
        var mesh_intance = new MeshInstance3D();
        var immediate_mesh = new ImmediateMesh();
        var material = new OrmMaterial3D();

        mesh_intance.Mesh = immediate_mesh;

        immediate_mesh.SurfaceBegin(Mesh.PrimitiveType.Lines, material);
        immediate_mesh.SurfaceAddVertex(a);
        immediate_mesh.SurfaceAddVertex(b);
        immediate_mesh.SurfaceEnd();

        material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
        material.AlbedoColor = Color.Color8(1, 0, 0, 1);

        GetTree().Root.AddChild(mesh_intance);
    }


}
