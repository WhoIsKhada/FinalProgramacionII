using UnityEngine;
using System.Collections;

public class RaycastWeapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    public float damage = 25f;
    public float range = 100f;
    public float fireRate = 10f; // Rounds per second
    public float impactForce = 30f;

    [Header("Ammo")]
    public int maxAmmo = 12;
    public int currentAmmo;
    public float reloadTime = 1.5f;
    private bool isReloading = false;

    [Header("References")]
    public GameObject fpsCamera;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect; // Particle for hitting things
    public Animator animator; // Weapon/Arms animator

    // Private variables
    private float nextTimeToFire = 0f;

    void Start()
    {
        currentAmmo = maxAmmo;
        
        // Auto-assign camera if missing
        if (fpsCamera == null)
        {
            if (Camera.main != null)
            {
                fpsCamera = Camera.main.gameObject;
                Debug.Log("RaycastWeapon: Auto-assigned Main Camera.");
            }
            else
            {
                Debug.LogError("RaycastWeapon: No Main Camera found! Tag your camera as 'MainCamera'.");
            }
        }
    }

    void OnEnable()
    {
        isReloading = false;
        if(animator != null) animator.SetBool("IsReloading", false);
    }

    void Update()
    {
        if (isReloading)
            return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
        
        // Manual Reload
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        if (animator != null)
        {
            animator.SetTrigger("Reload");
        }

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
    }

    void Shoot()
    {
        if (muzzleFlash != null)
            muzzleFlash.Play();

        // Animation
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }

        currentAmmo--;

        RaycastHit hit;
        
        // Change: Use QueryTriggerInteraction.Ignore to avoid hitting invisible trigger zones (like aggro radius)
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.Log($"Hit: {hit.transform.name}");

            // Search in parent components too, in case we hit an arm or leg
            EnemyController enemy = hit.transform.GetComponentInParent<EnemyController>();
            
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                // Visual feedback: Blood effect could go here
            }

            // Optional: Apply force to hit object
            Rigidbody rb = hit.collider.attachedRigidbody; 
            if (rb != null)
            {
                rb.AddForce(-hit.normal * impactForce);
            }

            // Hit Effect
            if (impactEffect != null)
            {
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }
        }
    }
}
