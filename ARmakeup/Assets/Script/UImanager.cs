using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
public class UImanager : MonoBehaviour
{

    private GameObject facemask;
    public GameObject GraSlider;
    public Slider GraValue;//그라데이션 조절 혹은 투명도의 값 조절
    public Slider SpecValue;
    public GameObject Alphaslider;
    public Slider AlphaValue;
    public Material[] materials;

    public GameObject mainMenu;

    public GameObject button1;
    public GameObject ColorSetPanel;
    public GameObject ColorView;
    private int switchCount = 2;//시작지점
    private int materialsSize;
    void Start()
    {
        materialsSize = materials.Length;
        facemask = GameObject.FindGameObjectWithTag("FaceMask");
        facemask.GetComponent<MeshRenderer>().material = materials[switchCount];//초기값 설정




    }

    // Update is called once per frame
    void Update()
    {
        if (switchCount == 0)
        {
            UpdateMatShader();
        }
        else if (switchCount == 1)
        {
            UpdateGlossShader();
        }
        else
        {
            facemask.GetComponent<MeshRenderer>().material = materials[switchCount];
        }

    }

    public void sub_click()
    {
        Debug.Log("aad");
        switchCount = 3;
    } 
    
    public void OnclickRed()
    {
        float factor = Mathf.Pow(2, 0);
        Color color = new Color(193/255.0f * factor, 10/255.0f * factor, 0 / 255.0f * factor);
        facemask.GetComponent<MeshRenderer>().material.SetColor("_Color", color);

    }
    public void OnclickPink()
    {
        float factor = Mathf.Pow(2, 0);
        Color color = new Color(186 / 255.0f * factor, 48 / 255.0f * factor, 181 / 255.0f * factor);
        facemask.GetComponent<MeshRenderer>().material.SetColor("_Color", color);

    }
    public void OnclickPantone()
    {
        float factor = Mathf.Pow(2, 0);
        Color color = new Color(198 / 255.0f * factor, 29 / 255.0f * factor, 0 / 255.0f * factor);
        facemask.GetComponent<MeshRenderer>().material.SetColor("_Color", color);

    }
    public void OnclickPlum()
    {
        float factor = Mathf.Pow(2, 0);
        Color color = new Color(145 / 255.0f * factor, 0 / 255.0f * factor, 0 / 255.0f * factor);
        facemask.GetComponent<MeshRenderer>().material.SetColor("_Color", color);

    }
    public void OnclickBrick()
    {
        float factor = Mathf.Pow(2, 0);
        Color color = new Color(72 / 255.0f * factor, 0 / 255.0f * factor, 8 / 255.0f * factor);
        facemask.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        
    }
    public void OnclickPurple()
    {
        float factor = Mathf.Pow(2, 0);
        Color color = new Color(64 / 255.0f * factor, 4 / 255.0f * factor, 125 / 255.0f * factor);
        facemask.GetComponent<MeshRenderer>().material.SetColor("_Color", color);


    }
    public void OnColorView()
    {
        if (ColorView.activeSelf)
        {
            ColorView.SetActive(false);
        }
        else
        {
            ColorView.SetActive(true);
        }
    }
    public void OnMenuClick()
    {
        if (mainMenu.activeSelf)
            mainMenu.SetActive(false);
        else
            mainMenu.SetActive(true);
    }
    public void OnSetGra(bool buttonclick=true)
    {
        Material m;
        m = facemask.GetComponent<MeshRenderer>().material;

        if (buttonclick)
        {
            float check = m.GetFloat("_UsingGra");
            if (check == 1)
            {
                facemask.GetComponent<MeshRenderer>().material.SetFloat("_UsingGra", 0);
                button1.GetComponent<Image>().color = new Color(255, 255, 255);
            }


            else
            {
                facemask.GetComponent<MeshRenderer>().material.SetFloat("_UsingGra", 1);
                button1.GetComponent<Image>().color = new Color(0, 255, 0);

            }
        }
        else
        {
            facemask.GetComponent<MeshRenderer>().material.SetFloat("_UsingGra", 0);
            button1.GetComponent<Image>().color = new Color(255, 255, 255);
        }

    }
    public void OnGlossColorWay()
    {
        switchCount = 1;
        facemask.GetComponent<MeshRenderer>().material = materials[switchCount];

        Texture2D normalTexture = imgfileload("/NormalMap.png");
        facemask.GetComponent<MeshRenderer>().material.SetTexture("_NormalMap", normalTexture);
        OnSetGra(false);
       
        
    }
    public void OnMatColorWay()
    {
        switchCount = 0;
        facemask.GetComponent<MeshRenderer>().material = materials[switchCount];
        OnSetGra(false);

    }
    public void OnChangePaintWay()
    {
        Material m;
        m = facemask.GetComponent<MeshRenderer>().material;
        
        float check = m.GetFloat("_UsingGra");
        if (check == 1)
            facemask.GetComponent<MeshRenderer>().material.SetFloat("_UsingGra", 0);
        
        else
            facemask.GetComponent<MeshRenderer>().material.SetFloat("_UsingGra", 1);

    }


    public void UpdateMatShader()
    {
        Material m;
        m = facemask.GetComponent<MeshRenderer>().material;
        float check=m.GetFloat("_UsingGra");
        if (check == 1)
        {
            GraSlider.SetActive(true);
            Alphaslider.SetActive(false);
            facemask.GetComponent<MeshRenderer>().material.SetFloat("_GraValue", (float)GraValue.value);
            facemask.GetComponent<MeshRenderer>().material.SetFloat("_alphaChennal", (float)0.3);
        }
        else
        {

            Alphaslider.SetActive(true);
            GraSlider.SetActive(false);
            facemask.GetComponent<MeshRenderer>().material.SetFloat("_alphaChennal", (float)AlphaValue.value);

        }

    }
    public void UpdateGlossShader()
    {
        Material m;
        m = facemask.GetComponent<MeshRenderer>().material;
        float check = m.GetFloat("_UsingGra");
        if (check == 1)
        {
            GraSlider.SetActive(true);
            Alphaslider.SetActive(false);
            facemask.GetComponent<MeshRenderer>().material.SetFloat("_GraValue", (float)GraValue.value);
            facemask.GetComponent<MeshRenderer>().material.SetFloat("_alphaChennal", (float)0.3);

        }
        else
        {
            Alphaslider.SetActive(true);
            GraSlider.SetActive(false);
            facemask.GetComponent<MeshRenderer>().material.SetFloat("_alphaChennal", (float)AlphaValue.value);

        }
        facemask.GetComponent<MeshRenderer>().material.SetFloat("_SpecValue", (float)SpecValue.value);
    }
    private Texture2D imgfileload(String filename)
    {
        byte[] byte_texture = File.ReadAllBytes(Application.persistentDataPath+filename);
        Texture2D texture = new Texture2D(0, 0);
        if (byte_texture.Length > 0)
        {
            texture.LoadImage(byte_texture);
        }
        else
        {
            Debug.Log("not found imgfile, filename : " + filename);
        }
        return texture;
    }

    public void OnExit()
    {
        Application.Quit();
    }
}
