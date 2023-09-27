//Copyright © 2023 Takashi Yoshinaga
//This code is provided as a sample and is not responsible for any errors.

using UnityEngine;
using UnityEngine.Video;
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GridMesh : MonoBehaviour
{
    [SerializeField]
    VideoClip _depthVideo;
    [SerializeField]
    VideoClip _colorVideo;

    [SerializeField]
    int _xSteps = 200; // Number of vertices in x direction
    [SerializeField]
    int _ySteps = 200; // Number of vertices in y direction
    [SerializeField]
    float _depthScale=1.5f; // Scale of depth. 1.0 is the original depth(0~1).

    //private variables
    VideoPlayer _depthPlayer;
    VideoPlayer _colorPlayer;
    bool _isDepthPlayerReady = false;
    bool _isColorPlayerReady = false;
    MeshRenderer _meshRenderer;

    void Start()
    {
        InitializeVideoPlayer();
        InitializeMesh();
        WaitForVideoPlayerReady();
    }
    private void Update() {
        if(_meshRenderer!=null){
            _meshRenderer.material.SetFloat("_DepthScale", _depthScale);
        }
    }
    void InitializeVideoPlayer(){
        uint width=_depthVideo.width;
        uint height=_depthVideo.height;
        _depthPlayer = gameObject.AddComponent<VideoPlayer>();
        _depthPlayer.playOnAwake = false;
        _depthPlayer.isLooping = true;
        _depthPlayer.renderMode = VideoRenderMode.RenderTexture;
        _depthPlayer.targetTexture = new RenderTexture((int)width, (int)height, 0);
        _depthPlayer.clip = _depthVideo;
        _depthPlayer.Prepare();
     
        width=_colorVideo.width;
        height=_colorVideo.height;
        _colorPlayer = gameObject.AddComponent<VideoPlayer>();
        _colorPlayer.playOnAwake = false;
        _colorPlayer.isLooping = true;
        _colorPlayer.renderMode = VideoRenderMode.RenderTexture;
        _colorPlayer.targetTexture = new RenderTexture((int)width, (int)height, 0);
        _colorPlayer.clip = _colorVideo;
        _colorPlayer.Prepare();
    }
    void InitializeMesh(){
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        _meshRenderer = gameObject.GetComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        Vector3[] vertices = new Vector3[_xSteps * _ySteps];
        Vector2[] uv = new Vector2[_xSteps * _ySteps];
        int[] triangles = new int[(_xSteps - 1) * (_ySteps - 1) * 6];

        for (int y = 0, i = 0; y < _ySteps; y++)
        {
            for (int x = 0; x < _xSteps; x++, i++)
            {
                vertices[i] = new Vector3((float)x / (_xSteps - 1) , (float)y / (_ySteps - 1) ,0);
                uv[i] = new Vector2((float)x / (_xSteps - 1), (float)y / (_ySteps - 1));
            }
        }

        for (int y = 0, ti = 0, vi = 0; y < _ySteps - 1; y++, vi++)
        {
            for (int x = 0; x < _xSteps - 1; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + _xSteps;
                triangles[ti + 5] = vi + _xSteps + 1;
            }
        }
        mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        _meshRenderer.material.SetTexture("_Depth", _depthPlayer.targetTexture);
        _meshRenderer.material.SetTexture("_Color", _colorPlayer.targetTexture);
        _meshRenderer.material.SetFloat("_DepthScale", _depthScale);
       
        float aspect = (float)_depthVideo.width / (float)_depthVideo.height;
        if(aspect>1f){
            transform.localScale = new Vector3(aspect, 1f, 1f);
        }else{
            transform.localScale = new Vector3(1f, 1f/aspect, 1f);
        }
    }
    void PlayBack(){
        if(_isDepthPlayerReady && _isColorPlayerReady){
            _depthPlayer.Play();
            _colorPlayer.Play();
        }
    }
    void WaitForVideoPlayerReady(){
       _depthPlayer.prepareCompleted += (VideoPlayer vp) => {
            _isDepthPlayerReady = true;
            PlayBack();
        };
        _colorPlayer.prepareCompleted += (VideoPlayer vp) => {
            _isColorPlayerReady = true;
            PlayBack();
        };
    }
}

