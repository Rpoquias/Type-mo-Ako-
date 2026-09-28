using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.IO;
using System.Collections;

public class UIElementToPNG : MonoBehaviour
{
    [Header("Assign References")]
    public RectTransform targetUI;
    public Camera uiCamera;
    public string fileName = "UI_Element_Safe.png";

    [Header("Capture Settings")]
    public bool autoAssignReferences = true;
    public int captureWidth = 512;
    public int captureHeight = 512;

    private void Start()
    {
        if (autoAssignReferences)
        {
            if (targetUI == null)
                targetUI = GetComponent<RectTransform>();

            if (uiCamera == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null && canvas.worldCamera != null)
                    uiCamera = canvas.worldCamera;
                else
                    uiCamera = Camera.main;
            }
        }
    }

    [ContextMenu("Capture Transparent UI Element")]
    public void CaptureTransparentUI()
    {
        if (Application.isPlaying)
            StartCoroutine(CaptureRoutine());
        else
            CaptureInEditMode();
    }

    private void CaptureInEditMode()
    {
        if (targetUI == null)
        {
            targetUI = transform.Find("Image")?.GetComponent<RectTransform>();
            if (targetUI == null)
            {
                Debug.LogError("❌ No target UI found. Please assign targetUI.");
                return;
            }
        }

        // Create temporary camera for URP
        GameObject tempCamGO = new GameObject("TempURPCaptureCamera");
        Camera tempCam = tempCamGO.AddComponent<Camera>();

        // Add URP camera data component
        UniversalAdditionalCameraData urpCamData = tempCam.GetUniversalAdditionalCameraData();

        // Configure URP camera settings
        tempCam.clearFlags = CameraClearFlags.SolidColor;
        tempCam.backgroundColor = Color.clear; // Transparent
        tempCam.orthographic = true;
        tempCam.cullingMask = 1 << targetUI.gameObject.layer;
        tempCam.depth = 100;

        // URP specific settings
        urpCamData.renderType = CameraRenderType.Base;
        urpCamData.requiresColorTexture = false;
        urpCamData.requiresDepthTexture = false;
        urpCamData.renderShadows = false;

        // Position camera to frame UI element
        Vector3[] corners = new Vector3[4];
        targetUI.GetWorldCorners(corners);

        Vector3 center = Vector3.zero;
        foreach (var corner in corners)
            center += corner;
        center /= 4;

        float width = Vector3.Distance(corners[0], corners[3]);
        float height = Vector3.Distance(corners[0], corners[1]);

        tempCam.transform.position = new Vector3(center.x, center.y, center.z - 10);
        tempCam.orthographicSize = Mathf.Max(width, height) / 2f;

        // Create render texture with alpha support
        RenderTexture rt = new RenderTexture(captureWidth, captureHeight, 0, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 1;
        rt.Create();

        // Render
        tempCam.targetTexture = rt;
        tempCam.Render();

        // Convert to Texture2D
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(captureWidth, captureHeight, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        // Save PNG
        byte[] png = tex.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, fileName);
        File.WriteAllBytes(path, png);

        Debug.Log($"✅ URP UI captured with transparency: {path}");

        // Cleanup
        DestroyImmediate(tempCamGO);
        rt.Release();
        DestroyImmediate(rt);
        DestroyImmediate(tex);
    }

    private IEnumerator CaptureRoutine()
    {
        yield return new WaitForEndOfFrame();

        if (targetUI == null || uiCamera == null)
        {
            Debug.LogError("❌ Please assign both targetUI and uiCamera.");
            yield break;
        }

        // Get URP camera data
        UniversalAdditionalCameraData urpCamData = uiCamera.GetUniversalAdditionalCameraData();

        // Store original settings
        RenderTexture originalRT = uiCamera.targetTexture;
        CameraClearFlags originalClearFlags = uiCamera.clearFlags;
        Color originalBgColor = uiCamera.backgroundColor;
        int originalCullingMask = uiCamera.cullingMask;

        // Store URP specific settings
        bool originalRequiresColorTexture = urpCamData.requiresColorTexture;
        bool originalRequiresDepthTexture = urpCamData.requiresDepthTexture;
        bool originalRenderShadows = urpCamData.renderShadows;

        // Create render texture
        RenderTexture rt = new RenderTexture(captureWidth, captureHeight, 0, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 1;
        rt.Create();

        // Configure camera for transparency
        uiCamera.targetTexture = rt;
        uiCamera.clearFlags = CameraClearFlags.SolidColor;
        uiCamera.backgroundColor = Color.clear;
        uiCamera.cullingMask = 1 << targetUI.gameObject.layer;

        // Configure URP settings for transparency
        urpCamData.requiresColorTexture = false;
        urpCamData.requiresDepthTexture = false;
        urpCamData.renderShadows = false;

        // Render
        uiCamera.Render();

        // Read pixels
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(captureWidth, captureHeight, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        tex.Apply();

        // Restore camera settings
        uiCamera.targetTexture = originalRT;
        uiCamera.clearFlags = originalClearFlags;
        uiCamera.backgroundColor = originalBgColor;
        uiCamera.cullingMask = originalCullingMask;

        // Restore URP settings
        urpCamData.requiresColorTexture = originalRequiresColorTexture;
        urpCamData.requiresDepthTexture = originalRequiresDepthTexture;
        urpCamData.renderShadows = originalRenderShadows;

        RenderTexture.active = null;

        // Save
        byte[] png = tex.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, fileName);
        File.WriteAllBytes(path, png);

        Debug.Log($"✅ URP UI captured: {path}");

        // Cleanup
        rt.Release();

        if (Application.isPlaying)
            Destroy(rt);
        else
            DestroyImmediate(rt);

        if (Application.isPlaying)
            Destroy(tex);
        else
            DestroyImmediate(tex);
    }
}
