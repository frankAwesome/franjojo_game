using UnityEngine;

namespace EzAPI
{
    namespace RunTime
    {
        /// <summary>
        /// A generic singleton class for MonoBehaviour that persists across scenes.
        /// </summary>
        /// <typeparam name="T">The type of MonoBehaviour that is a singleton.</typeparam>
        public class MonoBehaviourSingletonPersistent<T> : MonoBehaviour
            where T : MonoBehaviour
        {
            /// <summary>
            /// The static instance of the singleton.
            /// </summary>
            private static T _instance;

            /// <summary>
            /// Gets the instance of the singleton. If no instance exists, it will find or create one.
            /// </summary>
            public static T Instance
            {
                get
                {
                    // If the instance is null, try to find it or instantiate it
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();

                        if (_instance == null)
                        {
                            // Try to load from resources if no instance is found
                            T[] prefabs = Resources.LoadAll<T>("Singletons");
                            if (prefabs.Length > 0)
                            {
                                _instance = GameObject.Instantiate(prefabs[0]).GetComponent<T>();
                            }

                            // If still no instance, create a new GameObject and add the component
                            if (_instance == null)
                            {
                                GameObject obj = new GameObject(typeof(T).Name);
                                _instance = obj.AddComponent<T>();
                            }
                        }

                        // Ensure that the singleton persists across scenes
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                    return _instance;
                }
            }

            /// <summary>
            /// Called when the script instance is being loaded.
            /// Initializes the singleton instance and ensures that it persists across scenes.
            /// </summary>
            public virtual void Awake()
            {
                // If the instance is null, set it to this instance and make it persistent
                if (_instance == null)
                {
                    _instance = this as T;
                    DontDestroyOnLoad(this);
                }
                // If another instance already exists, destroy this one to maintain the singleton pattern
                else if (_instance != this)
                {
                    Destroy(gameObject);
                }
            }

            /// <summary>
            ///  Called when the script instance is being Destroyed
            /// </summary>
            public virtual void OnDestroy()
            {
                if (_instance == this)
                {
                    _instance = null;
                }
            }
        }
    }
}
