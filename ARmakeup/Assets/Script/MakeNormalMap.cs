using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;//파일저장
using System;
using UnityEngine.UI;
public class MakeNormalMap : MonoBehaviour
{

    [DllImport("TestCuda", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern void make_wrapimg1([MarshalAs(UnmanagedType.LPStr)] string path);
    public GameObject facemask;
    public Material[] materials;
    public Camera cam;//스크린샷 찍을 카메라


    private Vector3[] vertices;
    private Vector2[] uv_points;
    private int[] tri;
    private String dir_path;
    // Start is called before the first frame update

    void Start()
    {
        vertices = new Vector3[68];
        uv_points = new Vector2[68];
        tri = new int[273];
        dir_path = Application.persistentDataPath;//Application.dataPath + "/Model";


        Debug.Log(dir_path);

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnUnwrap()
    {
        make_wrapimg1(dir_path);
        
    }
    
    public void OnSetting()
    {
        
        Mesh mesh = facemask.GetComponent<MeshFilter>().mesh;
        int j = 0;

        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            tri[i] = mesh.triangles[i];
        }

        foreach (Vector3 vertice in mesh.vertices)
        {
            Vector3 world_v = facemask.transform.TransformPoint(vertice);
            vertices[j] = world_v;
            //vertices[j] = cam.WorldToScreenPoint(vertices[j]);
            //vertices[j].y = Screen.height - vertices[j].y;
            vertices[j] = cam.WorldToViewportPoint(vertices[j]);
            int h, w;
            w = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<webcam>().camTexture.width;
            h = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<webcam>().camTexture.height;
            vertices[j].x = vertices[j].x * w;
            vertices[j].y = h-vertices[j].y * h;

            j++;
        }
        //FacePoint.text = Screen.height.ToString() + "/" + vertices[0].x.ToString();
        j = 0;

        foreach (Vector2 vertice in mesh.uv)
        { //uv 468개의 좌표
            uv_points[j] = new Vector2(vertice.x, (float)1.0 - vertice.y);
            j++;

        }
        j = 0;
        Debug.Log("call");
        Textfilewrite("/FaceVertices.txt", 3);
        Textfilewrite("/FaceUV.txt", 2);
        Textfilewrite("/FacePolygon.txt", 1);
        Debug.Log("call2");

        StartCoroutine(Imgfilewrite());

        
        //Texture2D normalTexture = imgfileload("/unwrap.png");
        //NormalMap(normalTexture);
        //state = false;

    }

    public void Textfilewrite(String filename, int type)
    {
        
        String path = dir_path + filename;
        Debug.Log(path);
        FileStream file;
        //if (!File.Exists(path))

        file = new FileStream(path, FileMode.Create, FileAccess.Write);

        StreamWriter sw = new StreamWriter(file);
        String str;
        if (type == 3)
        {
            for (int j = 0; j < vertices.Length; j++)
            {
                str = (vertices[j].x).ToString() + " "
                    + (vertices[j].y).ToString();
                sw.WriteLine(str);
            }
        }
        else if (type == 2)
        {
            int w = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<webcam>().camTexture.width;
            int h = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<webcam>().camTexture.height;
            for (int j = 0; j < uv_points.Length; j++)
            {
                str = (uv_points[j].x * w).ToString() + " "
                    + (uv_points[j].y * h).ToString();
                sw.WriteLine(str);
                
            }
        }
        else if (type == 1)
        {
            for (int j = 0; j < tri.Length; j++)
            {
                str = tri[j].ToString();
                sw.WriteLine(str);
                
            }
        }

        sw.Close();
        file.Close();

    }

    public IEnumerator Imgfilewrite()
    {
        GameObject UI = GameObject.Find("Panel");
        GameObject mask = GameObject.FindGameObjectWithTag("FaceMask");

        yield return new WaitForSeconds(0.5f);
        
        String path =dir_path;
        String filename;
        filename = path +"/sourceImg.png";
        GameObject obj= GameObject.Find("Main Camera");
        WebCamTexture cam= obj.GetComponent<webcam>().camTexture;
        Texture2D texture = new Texture2D(cam.width, cam.height); ;
        texture.SetPixels32(cam.GetPixels32());
        texture.Apply();
        Debug.Log(filename);

        byte[] bytes = texture.EncodeToPNG();
        DestroyImmediate(texture);
        File.WriteAllBytes(filename, bytes);

        

    }
    public void OnMakeNormalmap()
    {
        OnUnwrap();
        Texture2D sourceTexture;
        try
        {
            sourceTexture = imgfileload("/unwrap.png");
            MakingNormalMap(sourceTexture);
        }
        catch(FileNotFoundException)
        {
            
        }
        Texture2D NormalMap = imgfileload("/NormalMap.png");
    }
    private Texture2D imgfileload(String filename)
    {
        byte[] byte_texture = File.ReadAllBytes(dir_path + filename);
        Texture2D texture = new Texture2D(0, 0);
        if (byte_texture.Length > 0)
        {
            texture.LoadImage(byte_texture);
        }
        else
        {
            Debug.LogError("not found imgfile, filename : " + filename);
        }
        return texture;
    }
    private void MakingNormalMap(Texture2D source, float strength = (float)1.0) //노말맵으로 변환함수
    {
        strength = Mathf.Clamp(strength, 9.0F, 15.0F);

        Texture2D normalTexture;
        float xLeft;
        float xRight;
        float yUp;
        float yDown;
        float yDelta;
        float xDelta;

        normalTexture = new Texture2D(source.width, source.height, TextureFormat.ARGB32, true);

        for (int y = 0; y < normalTexture.height; y++)
        {
            for (int x = 0; x < normalTexture.width; x++)
            {
                xLeft = source.GetPixel(x - 1, y).grayscale * strength;
                xRight = source.GetPixel(x + 1, y).grayscale * strength;
                yUp = source.GetPixel(x, y - 1).grayscale * strength;
                yDown = source.GetPixel(x, y + 1).grayscale * strength;
                xDelta = ((xLeft - xRight) + 1) * 0.5f;
                yDelta = ((yUp - yDown) + 1) * 0.5f;
                normalTexture.SetPixel(x, y, new Color(xDelta, yDelta, 1.0f, yDelta));//오류 발생시(범프매핑) 4번째 인자 xDelta
            }
        }
        normalTexture.Apply();

        //Code for exporting the image to assets folder
        File.WriteAllBytes(dir_path + "/NormalMap.png", normalTexture.EncodeToPNG());
        

    }
}
