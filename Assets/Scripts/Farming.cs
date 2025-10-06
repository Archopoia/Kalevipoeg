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
            inputHandler = FindObjectOfType<PlayerInputHandler>();
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
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    private void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Farmables farmables = hit.transform.GetComponent<Farmables>();
            if (farmables != null)
            {
                farmables.TakeDamage(damage);
                {
                    StartCoroutine(PlayHitAnimation());
                }

                // Play appropriate gathering sound based on what was hit
                if (AudioManager.Instance != null)
                {
                    // Check if it's a tree or stone (you may need to adjust this based on your Farmables implementation)
                    bool isTree = hit.transform.name.ToLower().Contains("tree");
                    AudioManager.Instance.PlayGatherSound(isTree);
                }
            }
            else
            {
                anim.SetBool("isHitting", false);
            }
        }
    }

    private IEnumerator PlayHitAnimation()
    {
        anim.SetBool("isHitting", true);
        yield return new WaitForSeconds(0.8f);
        anim.SetBool("isHitting", false);
    }
}








