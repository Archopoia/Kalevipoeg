using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Handles camera fading for 2D sprites using the Fade Near Camera shader.
/// This script should be attached to sprites that need to fade when the camera gets close to them.
/// </summary>
public class CameraFadeController : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("The camera that should trigger the fade effect")]
    public Camera targetCamera;

    [Tooltip("Distance at which the fade effect starts")]
    public float fadeStartDistance = 3f;

    [Tooltip("Distance at which the sprite becomes completely transparent")]
    public float fadeEndDistance = 1f;

    [Tooltip("Material that uses the Fade Near Camera shader")]
    public Material fadeMaterial;

    [Header("Debug")]
    public bool enableDebugLogs = false;

    private Renderer spriteRenderer;
    private Material originalMaterial;
    private bool isUsingFadeMaterial = false;
    private Vector3 cameraPosition;
    private float distanceToCamera;

    void Start()
    {
        // Get the sprite renderer
        spriteRenderer = GetComponent<Renderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"CameraFadeController: No Renderer found on {gameObject.name}!");
            return;
        }

        // Store the original material
        originalMaterial = spriteRenderer.material;

        // Find the main camera if not assigned
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindFirstObjectByType<Camera>();
            }
        }

        if (targetCamera == null)
        {
            Debug.LogError($"CameraFadeController: No camera found for {gameObject.name}!");
            return;
        }

        // Try to find the fade material if not assigned
        if (fadeMaterial == null)
        {
            fadeMaterial = FindFadeMaterial();
            if (fadeMaterial == null)
            {
                Debug.LogWarning($"CameraFadeController: No fade material found for {gameObject.name}. Creating a basic one...");
                CreateFadeMaterial();
            }
        }

        if (enableDebugLogs)
        {
            Debug.Log($"CameraFadeController initialized on {gameObject.name} with camera: {targetCamera.name}");
        }
    }

    void Update()
    {
        if (spriteRenderer == null || targetCamera == null || fadeMaterial == null)
            return;

        // Calculate distance to camera
        cameraPosition = targetCamera.transform.position;
        distanceToCamera = Vector3.Distance(transform.position, cameraPosition);

        // Determine if we should use fade material
        bool shouldUseFade = distanceToCamera <= fadeStartDistance;

        if (shouldUseFade != isUsingFadeMaterial)
        {
            if (shouldUseFade)
            {
                ApplyFadeMaterial();
            }
            else
            {
                RestoreOriginalMaterial();
            }
            isUsingFadeMaterial = shouldUseFade;
        }

        // Update fade parameters if using fade material
        if (isUsingFadeMaterial)
        {
            UpdateFadeParameters();
        }
    }

    void ApplyFadeMaterial()
    {
        if (fadeMaterial != null)
        {
            spriteRenderer.material = fadeMaterial;

            if (enableDebugLogs)
            {
                Debug.Log($"Applied fade material to {gameObject.name}");
            }
        }
    }

    void RestoreOriginalMaterial()
    {
        if (originalMaterial != null)
        {
            spriteRenderer.material = originalMaterial;

            if (enableDebugLogs)
            {
                Debug.Log($"Restored original material to {gameObject.name}");
            }
        }
    }

    void UpdateFadeParameters()
    {
        // Calculate fade amount based on distance
        float fadeAmount = Mathf.Clamp01((distanceToCamera - fadeEndDistance) / (fadeStartDistance - fadeEndDistance));

        // Update shader properties
        if (fadeMaterial.HasProperty("_SeeThroughDistance"))
        {
            fadeMaterial.SetFloat("_SeeThroughDistance", fadeStartDistance);
        }

        if (fadeMaterial.HasProperty("_Color"))
        {
            Color currentColor = fadeMaterial.GetColor("_Color");
            currentColor.a = fadeAmount;
            fadeMaterial.SetColor("_Color", currentColor);
        }

        if (enableDebugLogs && Time.frameCount % 60 == 0) // Log every 60 frames to avoid spam
        {
            Debug.Log($"Updated fade parameters for {gameObject.name}: Distance={distanceToCamera:F2}, FadeAmount={fadeAmount:F2}");
        }
    }

    Material FindFadeMaterial()
    {
        // Try to find a material that uses the Fade Near Camera shader
        Material[] allMaterials = Resources.FindObjectsOfTypeAll<Material>();

        foreach (Material mat in allMaterials)
        {
            if (mat.shader != null && mat.shader.name.Contains("Fade Near Camera"))
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"Found fade material: {mat.name}");
                }
                return mat;
            }
        }

        return null;
    }

    void CreateFadeMaterial()
    {
        // Create a basic fade material using the Fade Near Camera shader
        Shader fadeShader = Shader.Find("Shader Graphs/Fade Near Camera");

        if (fadeShader == null)
        {
            Debug.LogError("CameraFadeController: Fade Near Camera shader not found! Make sure the shader is compiled and available.");
            return;
        }

        fadeMaterial = new Material(fadeShader);
        fadeMaterial.name = "AutoGenerated_FadeMaterial";

        if (enableDebugLogs)
        {
            Debug.Log($"Created fade material: {fadeMaterial.name}");
        }
    }

    void OnDestroy()
    {
        // Clean up
        if (fadeMaterial != null && fadeMaterial.name.Contains("AutoGenerated"))
        {
            DestroyImmediate(fadeMaterial);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (targetCamera != null)
        {
            // Draw fade range gizmos
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, fadeStartDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, fadeEndDistance);

            // Draw line to camera
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, targetCamera.transform.position);
        }
    }

    #region Public Methods

    /// <summary>
    /// Manually set the fade distances
    /// </summary>
    public void SetFadeDistances(float startDistance, float endDistance)
    {
        fadeStartDistance = startDistance;
        fadeEndDistance = endDistance;
    }

    /// <summary>
    /// Set the target camera
    /// </summary>
    public void SetTargetCamera(Camera camera)
    {
        targetCamera = camera;
    }

    /// <summary>
    /// Set the fade material
    /// </summary>
    public void SetFadeMaterial(Material material)
    {
        fadeMaterial = material;
    }

    /// <summary>
    /// Get current distance to camera
    /// </summary>
    public float GetDistanceToCamera()
    {
        if (targetCamera != null)
        {
            return Vector3.Distance(transform.position, targetCamera.transform.position);
        }
        return float.MaxValue;
    }

    #endregion
}



