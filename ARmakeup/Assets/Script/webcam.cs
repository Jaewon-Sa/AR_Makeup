using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System.Linq;
using System.Diagnostics;
using System.IO;
using UnityEngine.Video;

public class webcam : MonoBehaviour
{
    public struct faceconfidence
    {
        public double lip_confidence;//입술
        public double lefteye_confidence;//왼쪽눈
        public double righteye_confidence;//오른쪽눈
        public double nose_confidence;//코
        public double outline_confidence;//윤곽
        public double all_confidence;//전체
        public int lightangle;//현재 사용안함 
        public float faceangle;
        public int faceangledir;
    };
    //에디터에서 재생 시 캔버스, 디스플레이화면 해상도를 웹캠 해상도에 맞춰서 빌드해야 얼굴마스크와 웹캠에서의 얼굴이 잘 맞아 떨어진다. 
    public RawImage display;
    public WebCamTexture camTexture;
    public int currentIndex = 0;
    public GameObject Panel;//UI 버튼
    public Light mainlight;

    private Color[] img;
    Vector3[] vertices;

    Transform[] landTransform;
    private int check = 0;
    [DllImport("TestCuda", CallingConvention = CallingConvention.Cdecl)]
    private static extern void setpath([MarshalAs(UnmanagedType.LPStr)] string path);//파일경로 잡아주기

    [DllImport("TestCuda", CallingConvention = CallingConvention.Cdecl)]
    private static extern faceconfidence facelandmarks(ref Color32[] rawimg, int height, int width, [In, Out]float[] landmarks, bool changeLight_angle=false);//웹캠 프레임 전달
    private string path;
    //float time = 0f;
    float[] landmarks;
    
    Vector2 vector2;

    private GameObject mask;//화장을 입혀줄 게임오브젝트
    private Mesh mesh;//
    private MeshFilter facemesh;//mask 오브젝트의 메쉬필터

    private Color32[] frame;
    private Texture2D frametexture;

    private Texture2D tex;
    private bool change_light_angle;
    public GameObject set_light_angle;
    
    void Start()
    {
        
        WebCamDevice[] devices = WebCamTexture.devices;
       
        //UnityEngine.Debug.Log(devices[0].name);
        
        vertices = new Vector3[68];
        mask = GameObject.FindGameObjectWithTag("FaceMask");
        facemesh = mask.GetComponent<MeshFilter>();
        mesh = new Mesh();
        Makemesh();

        landmarks = Enumerable.Repeat<float>(1, 136).ToArray<float>();//얼굴 랜드마크 배열 초기화

        path = Application.streamingAssetsPath + "/";// path = 프로젝트파일경로/Assets/Model/
        //UnityEngine.Debug.Log(path);
        if (camTexture != null)
        {
            display.material.mainTexture = null;
            camTexture.Stop();
            camTexture = null;
        }
        WebCamDevice device = WebCamTexture.devices[currentIndex];
        
        camTexture = new WebCamTexture(device.name,1280,720,60);
        

       //display.material.mainTexture = camTexture;

        camTexture.Play();
        frame = new Color32[camTexture.width*camTexture.height];
        frametexture = new Texture2D(camTexture.width, camTexture.height);

        camTexture.requestedHeight = 1280;
        camTexture.requestedWidth = 720;
        Screen.SetResolution(1280, 720, true);
        setpath(path);
        //if (path_TF == false)
        //    UnityEngine.Debug.Log("file not found error: Please check the path again");
        //else
        //    UnityEngine.Debug.Log("Success");



        tex = new Texture2D(camTexture.width, camTexture.height);
        change_light_angle = false;


    }
    void Update()
    {

       
        
        if (camTexture.isPlaying)
        {
            if (check == 1)
            {
                facedetect();    
                check = 0;
                UpdateFaceMesh();
               


            }
            
        }
        check++;
    }

    void facedetect()
    {
        Onlook();
        Stopwatch watch = new Stopwatch();
        watch.Start();
        Color32[] rawImg = camTexture.GetPixels32();//frame

        tex.SetPixels32(rawImg);
        tex.Apply();
        display.material.mainTexture = tex as Texture;
 
        faceconfidence info;

        if (change_light_angle)
        {
            
            info = facelandmarks(ref rawImg, camTexture.width, camTexture.height, landmarks, true);
            UnityEngine.Debug.Log(info.all_confidence);
            if (info.all_confidence > 0.45)
            {
              
                UnityEngine.Debug.Log(info.faceangle);
                UnityEngine.Debug.Log(info.faceangledir);
                float Xangle, Yangle;
                Xangle = 30;
                
                if (info.faceangledir != 0)
                {
                    if (Mathf.Abs(info.faceangle) < 30)
                        Yangle = 15;
                    else
                        Yangle = 15 + info.faceangle;
                    
                }
                    
                   
                else
                {
                    Xangle = 30;
                    Yangle = 15;
                }
                    
                mainlight.transform.rotation = Quaternion.Euler(Xangle, Yangle, 0);
            }
            
        }

        else
        {
           info = facelandmarks(ref rawImg, camTexture.width, camTexture.height, landmarks);
            int Xangle = 30;
            int Yangle = 15;
            mainlight.transform.rotation = Quaternion.Euler(Xangle, Yangle, 0);
        }
        frametexture.SetPixels32(rawImg);


        watch.Stop();
        //UnityEngine.Debug.Log(info.lightangle);
       
        //UnityEngine.Debug.Log(change_light_angle);
        if (info.all_confidence < 0.55)
        {
            mask.SetActive(false);
            
        }
        else if(Panel.activeSelf)
        {
            mask.SetActive(true);
        }
        else
        {
            
            return;
        }
        //UnityEngine.Debug.Log(watch.ElapsedMilliseconds + " ms detect");
        
    }
    public void OnChangeLightAngle()
    {
        if (change_light_angle)
        {
            change_light_angle = false;
            set_light_angle.GetComponent<Image>().color = new Color(255, 255, 255);
        }
        else
        {
            change_light_angle = true;
            set_light_angle.GetComponent<Image>().color = new Color(0, 255, 0);
        }
           
    }

    void UpdateFaceMesh()
    {
        GameObject a = GameObject.Find("Canvas");
        float w = a.GetComponent<CanvasScaler>().referenceResolution.x;
        float h = a.GetComponent<CanvasScaler>().referenceResolution.y;
        if (mask.activeSelf)
        {
            Mesh Face_mesh;
            Face_mesh = facemesh.mesh;
            for (int i = 0; i < 68; i++)
            {
               
                float x = Mathf.Abs(vertices[i].x - (landmarks[i * 2] - camTexture.width / 2)) / camTexture.width;
                float y = Mathf.Abs(vertices[i].y - (camTexture.height - landmarks[i * 2 + 1] - camTexture.height / 2))/camTexture.height;
                float r = x * 100 + y * 100;
                    
                
                if (r<1.3)//움직인 비율계산
                {
                    
                    vertices[i].x = vertices[i].x;
                    vertices[i].y = vertices[i].y;
                    vertices[i].z = 0f;
                }
                else
                {
                    vertices[i].x = landmarks[i * 2] - camTexture.width / 2;
                    vertices[i].y = camTexture.height - landmarks[i * 2 + 1] - camTexture.height / 2;
                    vertices[i].z = 0f;
                }

                if (47 < i)
                {

                    if (60 < i && i < 68)
                    {
                        vertices[i].z = vertices[i].z+7.0f;
                    }
                    

                }

            }
            if(Mathf.Abs((vertices[66].y - vertices[57].y)*1/6)> Mathf.Abs((vertices[62].y - vertices[66].y) / 2))
            {
                vertices[65].y = vertices[63].y;
                vertices[66].y = vertices[62].y;
                vertices[67].y = vertices[61].y;
            }
            Face_mesh.vertices = vertices;
            Face_mesh.RecalculateBounds();
            Face_mesh.RecalculateNormals();
            
            //mask.GetComponent<MeshRenderer>().material = material;
           
            
        }

    }
    void Makemesh()
    {
        Vector3[] vertices=new Vector3[68];
        Vector2[] uv = new Vector2[68];
        Vector2[] libUV = new Vector2[] {new Vector2(178f,406f),new Vector2(185f, 488f), new Vector2(191f, 603f),new Vector2(231f,697f),
            new Vector2(269f, 754f),new Vector2(314f, 805f), new Vector2(357f, 839f), new Vector2(438f, 876f),
            new Vector2(512f, 887f),new Vector2(588f, 876f), new Vector2(664f, 839f), new Vector2(708f, 805f),
            new Vector2(751f, 754f),new Vector2(775f, 697f), new Vector2(813f, 603f), new Vector2(834f, 488f),
            new Vector2(843f,406f), new Vector2(236f, 306f), new Vector2(283f, 278f), new Vector2(335f, 263f),
            new Vector2(389f,259f), new Vector2(452f, 267f), new Vector2(571f, 267f), new Vector2(631f, 259f),
            new Vector2(688f,263f), new Vector2(740f, 278f), new Vector2(788f, 306f), new Vector2(512f, 358f),
            new Vector2(512f,411f), new Vector2(512f, 480f), new Vector2(512f, 539f), new Vector2(432f, 577f),
            new Vector2(463f,611f), new Vector2(512f, 617f), new Vector2(564f, 611f), new Vector2(591f, 577f),
            new Vector2(272f,385f), new Vector2(326f, 354f), new Vector2(377f, 353f), new Vector2(435f, 398f),
            new Vector2(375f,418f), new Vector2(318f, 419f), new Vector2(586f, 398f), new Vector2(644f, 354f),
            new Vector2(699f,353f), new Vector2(749f, 385f), new Vector2(706f, 418f), new Vector2(649f, 419f),
            new Vector2(390f,710f), new Vector2(425f, 681f), new Vector2(469f, 669f), new Vector2(512f, 668f),
            new Vector2(554f,669f),new Vector2(598f,681f),new Vector2(633f,710f),new Vector2(594f,736f),
            new Vector2(554f,750f),new Vector2(512f,755f),new Vector2(476f,750f),new Vector2(428f,736f),
            new Vector2(399f,710f),new Vector2(474f,710f),new Vector2(512f,710f),new Vector2(540f,710f),
            new Vector2(621f,710f),new Vector2(540f,710f),new Vector2(512f,710f),new Vector2(474f,710f)};

        int[] triangles = new int[] {60,48,49,67,66,58,
                                        59,48,60,57,58,66,
                                        60,49,61,65,57,66,
                                        60,67,59,56,57,65,
                                        49,50,61,55,56,65,
                                        58,59,67,64,55,65,
                                        50,51,61,54,55,64,
                                        51,62,61,52,53,63,
                                        51,52,62,53,64,63,
                                        52,63,62,53,54,64,
        0,17,36,17,18,36,18,37,36,
        18,19,37,19,20,37,37,20,38,
        20,21,38,38,21,39,21,27,39,
        39,27,28,0,36,1,1,36,41,
        40,39,28,1,41,2,
        2,41,31,41,40,31,40,28,31,
        31,28,30,2,31,3,31,30,32,
        32,30,33,31,32,49,49,32,50,
        32,33,50,50,33,51,3,31,49,
        3,49,48,3,48,4,4,48,5,5,48,59,
        5,59,6,6,59,58,6,58,7,7,58,57,
        7,57,8,27,22,42,27,42,28,22,23,42,
        42,23,43,43,23,24,43,24,44,44,24,25,
        44,25,45,45,25,26,45,26,16,46,45,15,
        45,16,15,28,42,47,30,28,35,
        33,30,34,34,30,35,35,28,47,35,47,46,
        35,46,14,46,15,14,51,33,52,33,34,52,
        52,34,53,53,34,35,35,13,53,35,14,13,
        53,13,54,54,13,12,55,54,12,55,12,11,
        55,11,10,56,55,9,9,55,10,57,56,8,8,56,9,
        21,22,27
        };
        int num = 0;
        for (int i = 0; i < uv.Length; i++)
        {
            
            uv[i].x = libUV[num].x / 1024.0f;
            uv[i].y = 1 - (libUV[num].y / 1024.0f);
            //UnityEngine.Debug.Log(uv[i].x);
            //UnityEngine.Debug.Log(uv[i].y);
            num++;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        //메쉬 uv 넣기
        
        //mesh.RecalculateBounds();
        //mesh.RecalculateNormals();
        facemesh.mesh = mesh;

    }
    public void Onlook()
    {
        GameObject a=GameObject.Find("Canvas");
        
        a.SetActive(false); a.SetActive(true);
    }
}
