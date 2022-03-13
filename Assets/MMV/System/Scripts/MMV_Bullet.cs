using UnityEngine;
using System.Collections.Generic;

namespace MMV
{
    public class MMV_Bullet : MonoBehaviour
    {
        [SerializeField] private float particlesLifeTime;
        [SerializeField] private float audioLifeTime;

        [SerializeField] private ParticleSystem[] particles;
        [SerializeField] private AudioSource hitAudioPlayer;
        [SerializeField] private AudioClip hitSound;

        //--------------------------------------------------

        private float moveSpeed;
        private float explosionForce;
        private float explosionRange;

        private bool exploded;

        //--------------------------------------------------

        /// <summary>
        /// Movement velocity of bullet
        /// </summary>
        /// <value></value>
        public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

        public float ExplosionForce { get => explosionForce; set => explosionForce = value; }
        public float ExplosionRange { get => explosionRange; set => explosionRange = value; }

        /// <summary>
        /// waiting time to destroy particles from the collision of the shot
        /// </summary>
        public float ParticlesLifeTime => particlesLifeTime;

        /// <summary>
        /// waiting time to destroy the audioSource of the explosion sound
        /// </summary>
        /// <value></value>
        public float AudioLifeTime => audioLifeTime;

        /// <summary>
        /// Particles of effect of the collision of the shot
        /// </summary>
        public ParticleSystem[] Particles => particles;

        /// <summary>
        /// AudioSoucrce of the collision shot
        /// </summary>
        public AudioSource HitAudioPlayer => hitAudioPlayer;

        /// <summary>
        /// Colision shot sound effect
        /// </summary>
        public AudioClip HitSound => hitSound;

        void Start()
        {
            //---------------------------------------------------------

            foreach (var particle in particles)
            {
                ParticleSystem.MainModule _module = particle.main;
                _module.loop = false;

                if (particle.isPlaying)
                {
                    particle.Stop();
                }
            }

            //---------------------------------------------------------

            hitAudioPlayer.loop = false;

            if (hitAudioPlayer.isPlaying)
            {
                hitAudioPlayer.Stop();
            }
        }

        void Update()
        {
            MoveBullet();
        }

        private void OnTriggerStay(Collider other)
        {
            if (!exploded)
            {
                exploded = true;
                SpawnBulletParticles(other);
                PlayBulletHitSound();
                ApplyExplosionForce();
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// Move projectile
        /// </summary>
        private void MoveBullet()
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Spawn hit particles
        /// </summary>
        /// <param name="hit">
        /// The collision
        /// </param>
        private void SpawnBulletParticles(Collider hit)
        {
            foreach (var p in particles)
            {
                ParticleSystem _p = Instantiate(p, transform.position, Quaternion.LookRotation(hit.transform.position - transform.position));

                _p.Play();
                Destroy(_p.gameObject, particlesLifeTime);
            }
        }

        /// <summary>
        /// Play hit sound
        /// </summary>
        private void PlayBulletHitSound()
        {
            AudioSource audio = Instantiate(hitAudioPlayer, transform.position, Quaternion.identity);
            audio.enabled = true;
            audio.PlayOneShot(hitSound);
            Destroy(audio, audioLifeTime);
        }

        /// <summary>
        /// applies blast force to nearby objects
        /// </summary>
        private void ApplyExplosionForce()
        {
            // catch all nearby colliders
            var _nearbyObjects = new List<GameObject>();
            var _colliders = Physics.OverlapSphere(transform.position, explosionRange);

            foreach (var c in _colliders)
            {
                if (c.transform != transform) _nearbyObjects.Add(c.gameObject);
            }

            // applie explosion force
            foreach (var obj in _nearbyObjects)
            {
                var rb = obj.GetComponent<Rigidbody>();

                if (rb)
                {
                    var _forceDir = (obj.transform.position - transform.position).normalized;
                    var _distance = Mathf.Clamp(Vector3.Distance(transform.position, obj.transform.position), 0, explosionRange);
                    var _explosionForce = explosionRange - _distance;
                    rb.AddForce(_forceDir * explosionForce);
                }
            }
        }
    }
}
