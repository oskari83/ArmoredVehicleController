using System;
using UnityEngine;
using UnityEditor;

namespace MMV.Editor
{
    [CustomEditor(typeof(MMV_Shooter))]
    public class MMV_MBT_ShooterEditor : UnityEditor.Editor
    {
        // --- properties names

        private const string PROPERTY_NAME_SHOT_PARTICLES = "shot.particles";


        // ---

        private const float SHOT_MIN_RELOAD_TIME = 0.01f;
        private const float SHOT_MIN_PARTICLES_LIFETIME = 0.01f;
        private const float SHOT_MIN_AUDIO_LIFETIME = 0.01f;
        private const float SHOT_MIN_LIFETIME = 0.1f;
        private const int SHOT_MIN_SPEED = 1;
        private const int SHOT_DEFAULT_LAYER = 2; // ignore raycast
        private const int SHOT_MIN_RECOIL = 0;
        private const int BULLET_MIN_EXPLOSION_RANGE = 0;
        private const int BULLET_MIN_EXPLOSION_FORCE = 0;


        //----------------------------------------------

        private bool bulletCharacteristicsExpanded;
        private bool shotParticlesExpanded;
        private bool shoterExpanded;
        private bool shotSoundExpanded;


        //----------------------------------------------



        // --- properties 

        private SerializedProperty shotParticles;


        //----------------------------------------------

        private MMV_Shooter shooter;

        private void OnEnable()
        {
            shooter = (MMV_Shooter)target;

            // configure default shooter when adding this component
            {
                // configure bullet
                if (shooter.BulletSettings == null)
                {
                    var _bulletSettings = new MMV_Shooter.BulletCharacteristics();

                    _bulletSettings.MoveSpeed = 200;
                    _bulletSettings.DestroyTime = 10.0f;
                    _bulletSettings.IgnoreLayer = SHOT_DEFAULT_LAYER;

                    shooter.BulletSettings = _bulletSettings;
                }

                // configure shot control
                if (shooter.ShotControl == null)
                {
                    var _shot = new MMV_Shooter.Shot();

                    _shot.Particles = new ParticleSystem[] { };
                    _shot.ParticlesLifeTime = 10.0f;
                    _shot.RecoilForce = 50000;
                    _shot.ReloadTime = 3.0f;
                    _shot.ShotAudioLifeTime = 3.0f;

                    shooter.ShotControl = _shot;
                }
            }

            shotParticles = serializedObject.FindProperty(PROPERTY_NAME_SHOT_PARTICLES);

            Load();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Separator();

            ShowShotEditor();

            if (shooter.ShotControl.Spawner)
            {
                ShowEffectsEditor();
                ShowSoundEditor();
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(shooter);
            Save();
        }

        /// <summary>
        /// Configure the shot
        /// </summary>
        private void ShowShotEditor()
        {
            shoterExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(shoterExpanded, "shot");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (shoterExpanded)
            {
                EditorGUI.indentLevel++;

                var _spawner = (Transform)EditorGUILayout.ObjectField("spawner", shooter.ShotControl.Spawner, typeof(Transform), true);

                if (shooter.ShotControl.Spawner)
                {
                    var _bullet = (GameObject)EditorGUILayout.ObjectField("bullet", shooter.ShotControl.BulletPrefab, typeof(GameObject), true);

                    EditorGUILayout.Separator();

                    var _shotLayer = EditorGUILayout.LayerField("ignore layer", shooter.BulletSettings.IgnoreLayer);

                    EditorGUILayout.Separator();

                    var _bulletVelocity = EditorGUILayout.FloatField("bullet velocity", shooter.BulletSettings.MoveSpeed);
                    var _bulletLifeTime = EditorGUILayout.FloatField("bullet lifetime", shooter.BulletSettings.DestroyTime);
                    var _bulletExplosionForce = EditorGUILayout.IntField("bullet explosion force", shooter.BulletSettings.ExplosionForce);
                    var _bulletExplosionRange = EditorGUILayout.IntField("bullet explosion range", shooter.BulletSettings.ExplosionRange);

                    EditorGUILayout.Separator();

                    var _reloadTime = EditorGUILayout.FloatField("reload time", shooter.ShotControl.ReloadTime);
                    var _recoil = EditorGUILayout.IntField("recoil", shooter.ShotControl.RecoilForce);

                    EditorGUILayout.Separator();

                    EditorGUI.indentLevel--;


                    //----------------------------------------------

                    if (_reloadTime < SHOT_MIN_RELOAD_TIME) _reloadTime = SHOT_MIN_RELOAD_TIME;
                    if (_bulletVelocity < SHOT_MIN_SPEED) _bulletVelocity = SHOT_MIN_SPEED;
                    if (_bulletLifeTime < SHOT_MIN_LIFETIME) _bulletLifeTime = SHOT_MIN_LIFETIME;
                    if (_recoil < SHOT_MIN_RECOIL) _recoil = SHOT_MIN_RECOIL;
                    if (_bulletExplosionForce < BULLET_MIN_EXPLOSION_FORCE) _bulletExplosionForce = BULLET_MIN_EXPLOSION_FORCE;
                    if (_bulletExplosionRange < BULLET_MIN_EXPLOSION_RANGE) _bulletExplosionRange = BULLET_MIN_EXPLOSION_RANGE;


                    //----------------------------------------------

                    shooter.ShotControl.BulletPrefab = _bullet;
                    shooter.ShotControl.ReloadTime = _reloadTime;
                    shooter.ShotControl.RecoilForce = _recoil;
                    shooter.BulletSettings.MoveSpeed = _bulletVelocity;
                    shooter.BulletSettings.DestroyTime = _bulletLifeTime;
                    shooter.BulletSettings.IgnoreLayer = _shotLayer;
                    shooter.BulletSettings.ExplosionForce = _bulletExplosionForce;
                    shooter.BulletSettings.ExplosionRange = _bulletExplosionRange;
                }

                shooter.ShotControl.Spawner = _spawner;
            }
        }

        /// <summary>
        /// Show shot effects settings
        /// </summary>
        private void ShowEffectsEditor()
        {
            shotParticlesExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(shotParticlesExpanded, "Effects");

            EditorGUILayout.EndFoldoutHeaderGroup(); // < - used here for draw particles list bellow

            if (shotParticlesExpanded)
            {
                EditorGUI.indentLevel++;

                var _particles = EditorGUILayout.PropertyField(shotParticles);

                if (shooter.ShotControl.Particles.Length > 0)
                {
                    var _particlesLifeTime = EditorGUILayout.FloatField("particles life time", shooter.ShotControl.ParticlesLifeTime);


                    //----------------------------------------------

                    if (_particlesLifeTime < SHOT_MIN_PARTICLES_LIFETIME)
                    {
                        _particlesLifeTime = SHOT_MIN_PARTICLES_LIFETIME;
                    }

                    //----------------------------------------------
                    shooter.ShotControl.ParticlesLifeTime = _particlesLifeTime;
                }

                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Show shot sound settings
        /// </summary>
        private void ShowSoundEditor()
        {
            shotSoundExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(shotSoundExpanded, "Sound");

            if (shotSoundExpanded)
            {
                EditorGUI.indentLevel++;

                var _audioSource = (AudioSource)EditorGUILayout.ObjectField("audio source", shooter.ShotControl.Audio, typeof(AudioSource), true);

                if (shooter.ShotControl.Audio)
                {
                    EditorGUILayout.Separator();

                    var _audioLifeTime = EditorGUILayout.Slider("audio life time", shooter.ShotControl.ShotAudioLifeTime, SHOT_MIN_AUDIO_LIFETIME, shooter.ShotControl.ReloadTime);
                    var _audioClip = (AudioClip)EditorGUILayout.ObjectField("clip", shooter.ShotControl.ShootSound, typeof(AudioClip), true);


                    //----------------------------------------------

                    if (_audioLifeTime < SHOT_MIN_AUDIO_LIFETIME)
                    {
                        _audioLifeTime = SHOT_MIN_AUDIO_LIFETIME;
                    }


                    //----------------------------------------------

                    shooter.ShotControl.ShotAudioLifeTime = _audioLifeTime;
                    shooter.ShotControl.ShootSound = _audioClip;
                }

                EditorGUI.indentLevel--;

                shooter.ShotControl.Audio = _audioSource;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>
        /// Save editor data
        /// </summary>
        private void Save()
        {
            EditorPrefs.SetBool(nameof(shooter) + nameof(bulletCharacteristicsExpanded), bulletCharacteristicsExpanded);
            EditorPrefs.SetBool(nameof(shooter) + nameof(shotParticlesExpanded), shotParticlesExpanded);
            EditorPrefs.SetBool(nameof(shooter) + nameof(shoterExpanded), shoterExpanded);
            EditorPrefs.SetBool(nameof(shooter) + nameof(shotSoundExpanded), shotSoundExpanded);
        }

        /// <summary>
        /// Load editor data
        /// </summary>
        private void Load()
        {
            bulletCharacteristicsExpanded = EditorPrefs.GetBool(nameof(shooter) + nameof(bulletCharacteristicsExpanded));
            shotParticlesExpanded = EditorPrefs.GetBool(nameof(shooter) + nameof(shotParticlesExpanded));
            shoterExpanded = EditorPrefs.GetBool(nameof(shooter) + nameof(shoterExpanded));
            shotSoundExpanded = EditorPrefs.GetBool(nameof(shooter) + nameof(shotSoundExpanded));
        }
    }
}