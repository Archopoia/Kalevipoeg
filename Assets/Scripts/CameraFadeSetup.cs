using UnityEngine;

/// <summary>
/// Utility script to help set up camera fading on existing sprites.
/// This can be used to automatically add camera fade functionality to sprites in the scene.
/// </summary>
public class CameraFadeSetup : MonoBehaviour
{
    [Header("Setup Settings")]
    [Tooltip("Automatically find and setup all sprites in the scene")]
    public bool setupAllSprites = false;

    [Tooltip("Only setup sprites with specific tags")]
    public string[] targetTags = { "Fadeable", "Sprite" };

    [Tooltip("Only setup sprites in specific layers")]
    public int[] targetLayers = { 0 };

    [Tooltip("Prefab to use for fade material")]
    public Material fadeMaterialPrefab;

    [Header("Fade Settings")]
    public float fadeStartDistance = 3f;
    public float fadeEndDistance = 1f;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    void Start()
    {
        if (setupAllSprites)
        {
            SetupAllSpritesInScene();
        }
    }

    [ContextMenu("Setup All Sprites in Scene")]
    public void SetupAllSpritesInScene()
    {
        if (enableDebugLogs)
        {
            Debug.Log("CameraFadeSetup: Starting to setup all sprites in scene...");
        }

        // Find all renderers in the scene
        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        int setupCount = 0;

        foreach (Renderer renderer in allRenderers)
        {
            // Check if this renderer should be setup
            if (ShouldSetupRenderer(renderer))
            {
                SetupRendererFade(renderer);
                setupCount++;
            }
        }

        if (enableDebugLogs)
        {
            Debug.Log($"CameraFadeSetup: Setup complete. Configured {setupCount} sprites for camera fading.");
        }
    }

    [ContextMenu("Setup Selected Objects")]
    public void SetupSelectedObjects()
    {
        // This would typically be used from the Unity Editor
        // For now, we'll setup all children of this object
        SetupChildren();
    }

    void SetupChildren()
    {
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
        int setupCount = 0;

        foreach (Renderer renderer in childRenderers)
        {
            if (ShouldSetupRenderer(renderer))
            {
                SetupRendererFade(renderer);
                setupCount++;
            }
        }

        if (enableDebugLogs)
        {
            Debug.Log($"CameraFadeSetup: Setup {setupCount} child objects for camera fading.");
        }
    }

    bool ShouldSetupRenderer(Renderer renderer)
    {
        // Check if already has camera fade controller
        if (renderer.GetComponent<CameraFadeController>() != null)
        {
            return false;
        }

        // Check tags
        if (targetTags.Length > 0)
        {
            bool hasValidTag = false;
            foreach (string tag in targetTags)
            {
                if (renderer.CompareTag(tag))
                {
                    hasValidTag = true;
                    break;
                }
            }
            if (!hasValidTag)
            {
                return false;
            }
        }

        // Check layers
        if (targetLayers.Length > 0)
        {
            bool hasValidLayer = false;
            foreach (int layer in targetLayers)
            {
                if (renderer.gameObject.layer == layer)
                {
                    hasValidLayer = true;
                    break;
                }
            }
            if (!hasValidLayer)
            {
                return false;
            }
        }

        // Only setup sprites (has SpriteRenderer or uses sprite materials)
        SpriteRenderer spriteRenderer = renderer as SpriteRenderer;
        if (spriteRenderer == null)
        {
            // Check if it's a regular renderer with sprite-like material
            if (renderer.material != null && !renderer.material.name.ToLower().Contains("sprite"))
            {
                return false;
            }
        }

        return true;
    }

    void SetupRendererFade(Renderer renderer)
    {
        // Add CameraFadeController component
        CameraFadeController fadeController = renderer.gameObject.AddComponent<CameraFadeController>();

        // Configure the fade controller
        fadeController.fadeStartDistance = fadeStartDistance;
        fadeController.fadeEndDistance = fadeEndDistance;
        fadeController.enableDebugLogs = enableDebugLogs;

        // Set the fade material if we have one
        if (fadeMaterialPrefab != null)
        {
            fadeController.fadeMaterial = fadeMaterialPrefab;
        }

        if (enableDebugLogs)
        {
            Debug.Log($"CameraFadeSetup: Added camera fade controller to {renderer.gameObject.name}");
        }
    }

    [ContextMenu("Remove All Fade Controllers")]
    public void RemoveAllFadeControllers()
    {
        CameraFadeController[] allControllers = FindObjectsByType<CameraFadeController>(FindObjectsSortMode.None);

        foreach (CameraFadeController controller in allControllers)
        {
            DestroyImmediate(controller);
        }

        if (enableDebugLogs)
        {
            Debug.Log($"CameraFadeSetup: Removed {allControllers.Length} camera fade controllers from scene.");
        }
    }

    [ContextMenu("Find Fade Material")]
    public void FindFadeMaterial()
    {
        Material fadeMaterial = Resources.Load<Material>("FadeNearCameraMaterial");

        if (fadeMaterial == null)
        {
            // Try to find it in the Materials folder
            fadeMaterial = Resources.Load<Material>("Sprites/Materials/FadeNearCameraMaterial");
        }

        if (fadeMaterial != null)
        {
            fadeMaterialPrefab = fadeMaterial;
            if (enableDebugLogs)
            {
                Debug.Log($"CameraFadeSetup: Found fade material: {fadeMaterial.name}");
            }
        }
        else
        {
            Debug.LogWarning("CameraFadeSetup: Could not find FadeNearCameraMaterial. Please assign it manually.");
        }
    }

    #region Public Methods

    /// <summary>
    /// Setup camera fading for a specific renderer
    /// </summary>
    public void SetupRenderer(Renderer renderer)
    {
        if (renderer != null && ShouldSetupRenderer(renderer))
        {
            SetupRendererFade(renderer);
        }
    }

    /// <summary>
    /// Setup camera fading for a specific GameObject
    /// </summary>
    public void SetupGameObject(GameObject gameObject)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            SetupRenderer(renderer);
        }
        else
        {
            Debug.LogWarning($"CameraFadeSetup: No renderer found on {gameObject.name}");
        }
    }

    #endregion
}
