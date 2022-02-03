using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostShaderRender : MonoBehaviour
{
    public RenderTexture texture;
    public Material effectMaterial;
    Camera camera;
    Rect cameraRect;

    void Start()
    {
        camera = GetComponent<Camera>();
        float viewWidth = camera.rect.xMin * Screen.width;
        float viewHeight = Screen.height - camera.rect.yMax * Screen.height;
        cameraRect = new Rect(viewWidth, viewHeight, camera.pixelWidth, camera.pixelHeight);
        texture.width = Screen.width;
        texture.height = Screen.height;

        //camera.targetTexture = texture;
        //effectMaterial.SetTexture("_MainTex", texture);
    }

    void OnGUI()
    {
        if (Event.current.type.Equals(EventType.Repaint))
        {
            Graphics.DrawTexture(new Rect(cameraRect), texture, effectMaterial);
        }
    }

    // works, if have ongui method included
    //IEnumerator OnPostRender()
    //{
    //    yield return new WaitForEndOfFrame();
    //    Graphics.DrawTexture(cameraRect, texture, effectMaterial);
    //
    //
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, effectMaterial);
    }
}
