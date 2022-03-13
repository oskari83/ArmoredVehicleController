using System;
using UnityEngine;

namespace MMV
{
    public class MMV_Shooter : MonoBehaviour
    {
        [Serializable]
        public class BulletCharacteristics
        {
            [SerializeField] private float moveSpeed;
            [SerializeField] private float destroyTime;
            [SerializeField] private LayerMask ignoreLayer;
            [SerializeField] private int explosionRange;
            [SerializeField] private int explosionForce;

            /// <summary>
            /// Bullet speed
            /// </summary>
            /// <value></value>
            public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

            /// <summary>
            /// Life time of bullet
            /// </summary>
            /// <value></value>
            public float DestroyTime { get => destroyTime; set => destroyTime = value; }

            /// <summary>
            /// Detects a collision if another object has a set layer as "X"
            /// </summary>
            /// <value></value>
            public LayerMask IgnoreLayer { get => ignoreLayer; set => ignoreLayer = value; }

            /// <summary>
            /// Adds an explosion force to all nearby objects
            /// </summary>
            /// <value></value>
            public int ExplosionRange { get => explosionRange; set => explosionRange = value; }

            /// <summary>
            /// Explosion force applied to nearby objects
            /// </summary>
            /// <value></value>
            public int ExplosionForce { get => explosionForce; set => explosionForce = value; }
        }

        [Serializable]
        public class Shot
        {
            [SerializeField] private Transform spawner;
            [SerializeField] private GameObject bulletPrefab;

            [SerializeField] private float reloadTime;
            [SerializeField] private int recoilForce;

            [SerializeField] private float particlesLifeTime;
            [SerializeField] private float shotAudioLifeTime;

            [SerializeField] private AudioSource audioPlayer;
            [SerializeField] private AudioClip shootSound;

            [SerializeField] private ParticleSystem[] particles;

            /// <summary>
            /// Shoot interval
            /// </summary>
            /// <value></value>
            public float ReloadTime { get => reloadTime; set => reloadTime = value; }

            /// <summary>
            /// Where the shot is instantiated
            /// </summary>
            /// <value></value>
            public Transform Spawner { get => spawner; set => spawner = value; }

            /// <summary>
            /// Prefab of bullet
            /// </summary>
            /// <value></value>
            public GameObject BulletPrefab { get => bulletPrefab; set => bulletPrefab = value; }

            /// <summary>
            /// Shoot sound
            /// </summary>
            /// <value></value>
            public AudioSource Audio { get => audioPlayer; set => audioPlayer = value; }

            /// <summary>
            /// Shoot sound
            /// </summary>
            /// <value></value>
            public AudioClip ShootSound { get => shootSound; set => shootSound = value; }

            /// <summary>
            /// Shoot particles
            /// </summary>
            /// <value></value>
            public ParticleSystem[] Particles { get => particles; set => particles = value; }

            /// <summary>
            /// Stop shoot sound with time
            /// </summary>
            /// <value></value>
            public float ShotAudioLifeTime { get => shotAudioLifeTime; set => shotAudioLifeTime = value; }
            /// <summary>
            /// Time to destroy the particle
            /// </summary>
            /// <value></value>
            public float ParticlesLifeTime { get => particlesLifeTime; set => particlesLifeTime = value; }

            /// <summary>
            /// Apply force to backward when shot
            /// </summary>
            /// <value></value>
            public int RecoilForce { get => recoilForce; set => recoilForce = value; }
        }

        [SerializeField] private BulletCharacteristics bulletCharacteristics;
        [SerializeField] private Shot shot;

        //--------------------------------------------------------


        private float reloadProgress;
        private bool isReloading;
        private bool gunEnabled;

        private MMV_MBT_Vehicle vehicle;

        //--------------------------------------------------------


        /// <summary>
        /// Characteristics of bullet
        /// </summary>
        public BulletCharacteristics BulletSettings { get => bulletCharacteristics; set => bulletCharacteristics = value; }

        /// <summary>
        /// The current reload time
        /// </summary>
        /// <value></value>
        public float ReloadProgress { get => reloadProgress; }

        /// <summary>
        /// If is reloading
        /// </summary>
        /// <value></value>
        public bool IsReloading { get => isReloading; }

        /// <summary>
        /// Deactive or activate shooter
        /// </summary>
        /// <value></value>
        public bool GunEnabled { get => gunEnabled; set => gunEnabled = value; }

        /// <summary>
        /// Shoot control
        /// </summary>
        /// <value></value>
        public Shot ShotControl { get => shot; set => shot = value; }

        /// <summary>
        /// Owner of the gun
        /// </summary>
        /// <value></value>
        public MMV_MBT_Vehicle Vehicle { get => vehicle; set => vehicle = value; }

        void Start()
        {
            gunEnabled = true;
            reloadProgress = shot.ReloadTime; // start with shooter activated

            vehicle = GetComponentInChildren<MMV_MBT_Vehicle>();

            if (shot.Audio)
            {
                if (shot.Audio.isPlaying)
                {
                    shot.Audio.Stop();
                }

                shot.Audio.loop = false;
            }

            foreach (var particle in shot.Particles)
            {
                // prevent the particle from starting if "PlayOnAwake" is active
                if (particle.isPlaying)
                {
                    particle.Stop();
                }

                ParticleSystem.MainModule _mainModule = particle.main;
                _mainModule.loop = false;
            }
        }

        void Update()
        {
            if (!gunEnabled)
            {
                return;
            }

            // --- reload
            if (isReloading)
            {
                reloadProgress -= Time.deltaTime;

                if (reloadProgress <= 0)
                {
                    isReloading = false;
                }
            }
        }

        /// <summary>
        /// Shoot
        /// </summary>
        public void Shoot()
        {
            if (!shot.Spawner || !shot.BulletPrefab)
            {
                return;
            }

            if (!gunEnabled)
            {
                return;
            }

            if (!isReloading)
            {
                CreateBullet();
                isReloading = true;
                reloadProgress = shot.ReloadTime; // initialize chronometer

                if (shot.Audio && shot.ShootSound)
                {
                    shot.Audio.PlayOneShot(shot.ShootSound);
                }

                if (vehicle)
                {
                    // recoil
                    if (vehicle.Rb && vehicle.Turret.Gun)
                    {
                        var _recoil = -vehicle.Turret.Gun.forward * shot.RecoilForce;
                        vehicle.Rb.AddForceAtPosition(_recoil, vehicle.Turret.Gun.position);
                    }
                }

                foreach (var particle in shot.Particles)
                {
                    var _particle = Instantiate(particle, shot.Spawner.position, shot.Spawner.rotation);

                    _particle.Play();
                    Destroy(_particle, shot.ParticlesLifeTime);
                }
            }
        }

        /// <summary>
        /// Instantiate bullet 
        /// </summary>
        private void CreateBullet()
        {
            GameObject _bullet = MonoBehaviour.Instantiate(shot.BulletPrefab, shot.Spawner.position, shot.Spawner.rotation);
            MMV_Bullet _bulletComponent = _bullet.GetComponent<MMV_Bullet>();

            _bulletComponent.MoveSpeed = bulletCharacteristics.MoveSpeed;
            _bulletComponent.ExplosionForce = bulletCharacteristics.ExplosionForce;
            _bulletComponent.ExplosionRange = bulletCharacteristics.ExplosionRange;

            Destroy(_bullet, BulletSettings.DestroyTime);
        }
    }
}
