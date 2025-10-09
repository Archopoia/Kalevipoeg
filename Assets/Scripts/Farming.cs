using UnityEngine;
using System.Collections;

public class Farming : MonoBehaviour
{
    public Camera fpsCam;
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 15f;
    private float nextTimeToFire = 0f;

    public Animator anim;

    private PlayerInputHandler inputHandler;

    private void Start()
    {
        // Auto-find camera if not assigned
        if (fpsCam == null)
        {
            fpsCam = GetComponentInChildren<Camera>();
            if (fpsCam == null)
            {
                fpsCam = Camera.main;
            }
            if (fpsCam == null)
            {
                fpsCam = FindFirstObjectByType<Camera>();
            }

            if (fpsCam != null)
            {
                Debug.Log($"Farming: Auto-found camera: {fpsCam.name}");
            }
            else
            {
                Debug.LogError("Farming: No camera found! Please assign fpsCam in the inspector.");
            }
        }

        // First try to get it from the same GameObject
        inputHandler = GetComponent<PlayerInputHandler>();

        // If not found, search in parent
        if (inputHandler == null)
        {
            inputHandler = GetComponentInParent<PlayerInputHandler>();
        }

        // If still not found, search in children
        if (inputHandler == null)
        {
            inputHandler = GetComponentInChildren<PlayerInputHandler>();
        }

        // If still not found, search in the entire scene
        if (inputHandler == null)
        {
            inputHandler = FindFirstObjectByType<PlayerInputHandler>();
        }

        if (inputHandler == null)
        {
            Debug.LogError("PlayerInputHandler component not found anywhere in the scene. Please add PlayerInputHandler component to " + gameObject.name + " or another GameObject in the scene.");
        }
    }

    private void Update()
    {
        if (inputHandler != null && inputHandler.Fire1Pressed && Time.time >= nextTimeToFire)
        {
            Debug.Log("Farming: Fire1 detected, attempting to shoot");
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
        else if (inputHandler != null && inputHandler.Fire1Pressed)
        {
            Debug.Log("Farming: Fire1 pressed but fire rate cooldown active");
        }
        else if (inputHandler == null)
        {
            Debug.LogError("Farming: inputHandler is null!");
        }
    }

    private void Shoot()
    {
        // Check if camera is assigned
        if (fpsCam == null)
        {
            Debug.LogError("Farming: Cannot shoot - fpsCam is null!");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log($"Farming: Raycast hit {hit.transform.name} at distance {hit.distance}");

            Farmables farmables = hit.transform.GetComponent<Farmables>();
            if (farmables != null)
            {
                Debug.Log($"Farming: Found farmable resource on {hit.transform.name}, dealing {damage} damage");
                farmables.TakeDamage(damage);
                Debug.Log($"Farming: TakeDamage called successfully");

                // Play hit animation only if animator is assigned
                if (anim != null)
                {
                    StartCoroutine(PlayHitAnimation());
                }
                else
                {
                    Debug.LogWarning("Farming: anim (Animator) is not assigned, skipping hit animation");
                }

                // Play appropriate gathering sound based on what was hit
                if (AudioManager.Instance != null)
                {
                    // Check if it's a tree or stone (you may need to adjust this based on your Farmables implementation)
                    bool isTree = hit.transform.name.ToLower().Contains("tree");
                    AudioManager.Instance.PlayGatherSound(isTree);
                }
                else
                {
                    Debug.LogWarning("Farming: AudioManager.Instance is null, skipping gather sound");
                }
            }
            else
            {
                Debug.Log($"Farming: Hit {hit.transform.name} but it has no Farmables component");
                if (anim != null)
                {
                    anim.SetBool("isHitting", false);
                }
            }
        }
        else
        {
            Debug.Log("Farming: Raycast didn't hit anything");
        }
    }

    private IEnumerator PlayHitAnimation()
    {
        anim.SetBool("isHitting", true);
        yield return new WaitForSeconds(0.8f);
        anim.SetBool("isHitting", false);
    }
}








