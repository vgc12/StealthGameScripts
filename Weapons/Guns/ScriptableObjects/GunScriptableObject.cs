using System.Collections;
using System.Collections.Generic;
using Damageables;
using UnityEngine;
using UnityEngine.Pool;
using Weapons.Guns.Ammo;

namespace Weapons.Guns.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
    public class GunScriptableObject : ScriptableObject
    {
        public GunType GunType;
        public string Name;
        public GameObject ModelPrefab;
        public Vector3 SpawnPoint;
        public Vector3 SpawnRotation;

   

        public List<AmmoScriptableObject> AmmoObjects;
        private AmmoScriptableObject _currentlyEquippedAmmo;

        public ShootConfigurationScriptableObject ShootingConfiguration;
        public TrailConfigScriptableObject TrailConfiguration;

        public ParticleSystem Impact;

        public bool spread = false;

        private MonoBehaviour _activeMonoBehavior;
        private GameObject _model;
        private float _lastShootTime;
        private ParticleSystem _muzzleFlash;
        private ObjectPool<TrailRenderer> _trailRendererPool;
        private ObjectPool<ParticleSystem> _impactPool;



        public void Spawn(Transform parent, MonoBehaviour activeMonoBehaviour)
        {

            GunScriptableObject o = new();

            _currentlyEquippedAmmo = AmmoObjects.Find(ammo => ammo.BulletType == BulletType.Normal);
            _currentlyEquippedAmmo.Spawn();

            _activeMonoBehavior = activeMonoBehaviour;
            _lastShootTime = 0;
            _trailRendererPool = new ObjectPool<TrailRenderer>(CreateTrailRenderer);
            _impactPool = new ObjectPool<ParticleSystem>(() =>
                {
                    return Instantiate(Impact);
                }
            );
            _model = Instantiate(ModelPrefab);
            _model.layer = LayerMask.NameToLayer("Gun");
            _model.transform.SetParent(parent, false);
            _model.transform.SetLocalPositionAndRotation(SpawnPoint, Quaternion.Euler(SpawnRotation));
            _muzzleFlash = _model.GetComponentInChildren<ParticleSystem>();
        }



        private TrailRenderer CreateTrailRenderer()
        {
            GameObject instance = new("Bullet Trail");
            TrailRenderer trail = instance.AddComponent<TrailRenderer>();

            trail.colorGradient = TrailConfiguration.Color;
            trail.widthCurve = TrailConfiguration.WidthCurve;
            trail.material = TrailConfiguration.TrailMaterial;
            trail.time = TrailConfiguration.Duration;
            trail.minVertexDistance = TrailConfiguration.MinVertexDistance;

            trail.emitting = false;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            return trail;
        }

        public void Shoot()
        {
            if (Time.time > ShootingConfiguration.FireRate + _lastShootTime && !_currentlyEquippedAmmo.IsEmpty)
            {
            
                _lastShootTime = Time.time;
                _muzzleFlash.Play();
                if (Physics.Raycast(_muzzleFlash.transform.position, GetDirection(), out RaycastHit hit, float.MaxValue, ShootingConfiguration.hitMask))
                {

                    FindHealthHandlerAndDamage(hit);
                    _activeMonoBehavior.StartCoroutine(PlayTrail(_muzzleFlash.transform.position, hit.point, hit));
                
                }
                else
                {
                    _activeMonoBehavior.StartCoroutine(PlayTrail(_muzzleFlash.transform.position, _muzzleFlash.transform.position + GetDirection() * TrailConfiguration.MissDistance, new RaycastHit()));

                }
                _currentlyEquippedAmmo.SubtractAmmo();
            }
        }

        private void FindHealthHandlerAndDamage(RaycastHit hit)
        {
            if (hit.collider.gameObject.TryGetComponent(out HealthHandler healthHandler))
            {
                healthHandler.healthScriptableObject.TakeDamage(_currentlyEquippedAmmo.Damage);   
            }
        }

        private IEnumerator PlayTrail(Vector3 start, Vector3 end, RaycastHit hit)
        {
            TrailRenderer trailInstance = _trailRendererPool.Get();
            trailInstance.gameObject.SetActive(true);
            trailInstance.transform.position = start;
            yield return null;
            trailInstance.emitting = true;

            float distance = Vector3.Distance(start, end);
            float remainingDistance = distance;
            while (remainingDistance > 0)
            {
                trailInstance.transform.position = Vector3.Lerp(start, end, Mathf.Clamp01(1 - (remainingDistance / distance)));
                remainingDistance -= TrailConfiguration.TrailSpeed * Time.deltaTime;
                yield return null;
            }

            trailInstance.transform.position = end;

            if (hit.collider)
            {
                ParticleSystem impact = _impactPool.Get();
                impact.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal));

            }

            yield return new WaitForSeconds(TrailConfiguration.Duration);
            yield return null;
            trailInstance.emitting = false;
            trailInstance.gameObject.SetActive(false);
            _trailRendererPool.Release(trailInstance);

        }



        private Vector3 GetDirection()
        {
            Vector3 direction = _muzzleFlash.transform.forward;
            if (spread)
            {
                direction += new Vector3(UnityEngine.Random.Range(-ShootingConfiguration.Spread.x, ShootingConfiguration.Spread.x),
                    UnityEngine.Random.Range(-ShootingConfiguration.Spread.y, ShootingConfiguration.Spread.y),
                    UnityEngine.Random.Range(-ShootingConfiguration.Spread.z, ShootingConfiguration.Spread.z));
                direction.Normalize();
            }
            return direction;
        }
    }
}
