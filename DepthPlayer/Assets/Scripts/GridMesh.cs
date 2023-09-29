//Copyright © 2023 Takashi Yoshinaga
//This code is provided as a sample and is not responsible for any errors.

using UnityEngine;
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GridMesh : MonoBehaviour
{
    Texture _depthTexture;
    Texture _colorTexture;
    MeshRenderer _meshRenderer;

    public void InitializeGridMesh(Texture depthTexture, Texture colorTexture, int xSteps, int ySteps, float depthScale){
        _depthTexture=depthTexture;
        _colorTexture=colorTexture;
        InitializeMesh(xSteps,ySteps,depthScale);
    }
   
    public void SetDepthScale(float depthScale){
        if(_meshRenderer!=null){
            _meshRenderer.material.SetFloat("_DepthScale",depthScale);
        }
    }
    void InitializeMesh(int xSteps,int ySteps, float depthScale){
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        _meshRenderer = gameObject.GetComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        Vector3[] vertices = new Vector3[xSteps * ySteps];
        Vector2[] uv = new Vector2[xSteps * ySteps];
        int[] triangles = new int[(xSteps - 1) * (ySteps - 1) * 6];

        for (int y = 0, i = 0; y < ySteps; y++)
        {
            for (int x = 0; x < xSteps; x++, i++)
            {
                vertices[i] = new Vector3((float)x / (xSteps - 1) , (float)y / (ySteps - 1) ,0);
                uv[i] = new Vector2((float)x / (xSteps - 1), (float)y / (ySteps - 1));
            }
        }

        for (int y = 0, ti = 0, vi = 0; y < ySteps - 1; y++, vi++)
        {
            for (int x = 0; x < xSteps - 1; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSteps;
                triangles[ti + 5] = vi + xSteps + 1;
            }
        }
        mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        _meshRenderer.material.SetTexture("_Depth", _depthTexture);
        _meshRenderer.material.SetTexture("_Color", _colorTexture);
        _meshRenderer.material.SetFloat("_DepthScale", depthScale);
       
        float aspect = (float)_depthTexture.width / (float)_depthTexture.height;
        if(aspect>1f){
            transform.localScale = new Vector3(aspect, 1f, 1f);
        }else{
            transform.localScale = new Vector3(1f, 1f/aspect, 1f);
        }
    }

}

