//Copyright © 2023 Takashi Yoshinaga
//This code is provided as a sample and is not responsible for any errors.

using UnityEngine;
using UnityEngine.Video;

public class MainScript : MonoBehaviour
{
    [SerializeField]
    GridMesh _gridMesh;
    [SerializeField]
    VideoClip _colorVideo;

    [SerializeField]
    VideoClip _depthVideo;
    
    [SerializeField]
    int _xSteps = 200; // Number of vertices in x direction
    [SerializeField]
    int _ySteps = 200; // Number of vertices in y direction
    [SerializeField]
    float _depthScale = 1.5f; // Scale of depth. 1.0 is the original depth(0~1).

    private VideoPlayer _depthPlayer;
    private VideoPlayer _colorPlayer;


    bool _isDepthPlayerReady = false;
    bool _isColorPlayerReady = false;


    // Start is called before the first frame update
    void Start()
    {
        InitializeVideoPlayer();
        _gridMesh.InitializeGridMesh(_depthPlayer.targetTexture, _colorPlayer.targetTexture, _xSteps, _ySteps, _depthScale);
        StartVideoAfterReady();
    }

    //Comment out the following Update() if you don't change the depth scale in runtime.
    void Update(){
        if(_gridMesh==null) return;
        _gridMesh.SetDepthScale(_depthScale);
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
    void PlayBack(){
        if(_isDepthPlayerReady && _isColorPlayerReady){
            _depthPlayer.Play();
            _colorPlayer.Play();
        }
    }
   
    void StartVideoAfterReady(){
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
