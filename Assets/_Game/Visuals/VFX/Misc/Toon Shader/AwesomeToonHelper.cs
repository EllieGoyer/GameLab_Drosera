using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AwesomeToon {

    [ExecuteInEditMode]
    public class AwesomeToonHelper : MonoBehaviour
    {
        [SerializeField] Material[] materials = null;

        private void Awake()
        {
            if (GetComponent<Renderer>() && materials != null)
            {
                foreach(Material m in materials)
                {
                    if (m == null || m.shader.Equals(Shader.Find("HDRP/Lit"))) continue;
                    m.shader = Shader.Find("Shader Graphs/SG_Multi_Toon");
                    Color mainColor = m.GetColor("Color_84686C61");
                    m.shader = Shader.Find("HDRP/Lit");
                    m.SetColor("_BaseColor", mainColor);
                }
                GetComponent<Renderer>().sharedMaterials = materials;
            }
                
        }
    }

    /* // Old code
     * struct LightSet {
        public int id;
        public Light light;
        public Vector3 dir;
        public Color color;
        public float atten;
        public float inView;

        public LightSet(Light newLight) {
            light = newLight;
            id = newLight.GetInstanceID();
            dir = Vector3.zero;
            color = Color.black;
            color.a = 0.01f;
            atten = 0f;
            inView = 1.1f; // Range -0.1 to 1.1 which is clamped 0-1 for faster consistent fade
        }
    }

    [ExecuteInEditMode]
    public class AwesomeToonHelper : MonoBehaviour {

        // Params
        [SerializeField] Material[] materials = null;
        [SerializeField] bool instanceMaterial = true;
        [SerializeField] bool showRaycasts = true;
        [SerializeField] Vector3 meshCenter = Vector3.zero;
        [Range(0,4)]
        [SerializeField] int maxLights = 4;

        [Header("Recieve Shadow Check")]
        [SerializeField] bool raycast = true;
        [SerializeField] LayerMask raycastMask = new LayerMask();
        [SerializeField] float raycastFadeSpeed = 10f;

        // State
        Vector3 posAbs;
        Dictionary<int, LightSet> lightSets;

        // Refs
        List<Material> materialInstances = new List<Material>();
        SkinnedMeshRenderer skinRenderer;
        MeshRenderer meshRenderer;

        // Toggle
        public static bool toggleRaycasts = false;

        void Start() {
            Init();
            GetLights();
            UpdateMaterial();
        }
        

        void OnValidate() {
            Init();
            UpdateMaterial();
        }

        void Init() {
            // Ensure the list is empty so we can add all new elements
            materialInstances.Clear();

            if (instanceMaterial) { //Add instances of the specified materials
                for (int i = 0; i < materials.Length; i++) {
                    if (materials[i] != null) {
                        materialInstances.Add(new Material(materials[i]));
                        materialInstances[i].name = "Instance of " + materials[i].name;
                    }
                }
            } else { //Add references to the specified materials
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null)
                    {
                        materialInstances.Add(materials[i]);
                    }
                }
            }

            //Convert the list into an identical array
            Material[] materialInstancesArr = new Material[materialInstances.Count];
            for (int i = 0; i < materialInstances.Count; i++)
            {
                if (materialInstances[i] != null)
                {
                    materialInstancesArr[i] = materialInstances[i];
                }
            }

            //Assign desired materials to object renderer
            skinRenderer = GetComponent<SkinnedMeshRenderer>();
            meshRenderer = GetComponent<MeshRenderer>();
            if (skinRenderer) skinRenderer.sharedMaterials = materialInstancesArr;
            if (meshRenderer) meshRenderer.sharedMaterials = materialInstancesArr;
        }

        // NOTE: If your game loads lights dynamically, this should be called to init new lights
        public void GetLights() {
            if (lightSets == null) {
                lightSets = new Dictionary<int, LightSet>();
            }

            Light[] lights = FindObjectsOfType<Light>();
            List<int> newIds = new List<int>();

            // Initialise new lights
            foreach (Light light in lights) {
                int id = light.GetInstanceID();
                newIds.Add(id);
                if (!lightSets.ContainsKey(id)) {
                    lightSets.Add(id, new LightSet(light));
                }
            }

            // Remove old lights
            List<int> oldIds = new List<int>(lightSets.Keys);
            foreach (int id in oldIds) {
                if (!newIds.Contains(id)) {
                    lightSets.Remove(id);
                }
            }
        }

        void Update() {
            posAbs = transform.position + meshCenter;

            // Always update lighting while in editor
            if (Application.isEditor && !Application.isPlaying) {
                GetLights();
            }

            if(!gameObject.isStatic)
                UpdateMaterial();
        }

        public static void UpdateAllLighting()
        {
            foreach(AwesomeToonHelper s in FindObjectsOfType<AwesomeToonHelper>())
            {
                s.UpdateMaterial();
            }
        }

        void UpdateMaterial() {
            if(materialInstances == null || !gameObject.activeSelf) return;

            // Refresh light data
            List<LightSet> sortedLights = new List<LightSet>();
            if (lightSets != null) {
                foreach (LightSet lightSet in lightSets.Values) {
                    sortedLights.Add(CalcLight(lightSet));
                }
            }

            // Sort lights by brightness
            sortedLights.Sort((x, y) => {
                float yBrightness = y.color.grayscale * y.atten;
                float xBrightness = x.color.grayscale * x.atten;
                return yBrightness.CompareTo(xBrightness);
            });

            // Apply lighting
            int i = 1;
            foreach (LightSet lightSet in sortedLights) {
                if (i > maxLights) break;
                if (lightSet.atten <= Mathf.Epsilon) break;

                // Use color Alpha to pass attenuation data
                Color color = lightSet.color;
                color.a = Mathf.Clamp(lightSet.atten, 0.01f, 0.99f); // UV might wrap around if attenuation is >1 or 0<

                for (int j = 0; j < materialInstances.Count; j++) {
                    if (materialInstances[j] != null) {
                        materialInstances[j].SetVector($"_L{i}_dir", lightSet.dir.normalized);
                        materialInstances[j].SetColor($"_L{i}_color", color);
                    }
                }
                i++;
            }

            // Turn off the remaining light slots
            while (i <= 4) {
                for (int j = 0; j < materialInstances.Count; j++) {
                    if (materialInstances[j] != null) {
                        materialInstances[j].SetVector($"_L{i}_dir", Vector3.up);
                        materialInstances[j].SetColor($"_L{i}_color", Color.black);
                    }
                }
                i++;
            }

            // Store updated light data
            foreach (LightSet lightSet in sortedLights) {
                lightSets[lightSet.id] = lightSet;
            }
        }

        LightSet CalcLight(LightSet lightSet) {
            Light light = lightSet.light;
            float inView = 1.1f;
            float dist;

            if (!light.isActiveAndEnabled) {
                lightSet.atten = 0f;
                return lightSet;
            }

            switch (light.type) {
                case LightType.Directional:
                    lightSet.dir = light.transform.forward * -1f;
                    inView = TestInView(lightSet.dir, 100f);
                    lightSet.color = light.color * light.intensity;
                    lightSet.atten = 1f;
                    break;

                case LightType.Point:
                    lightSet.dir = light.transform.position - posAbs;
                    dist = Mathf.Clamp01(lightSet.dir.magnitude / light.range);
                    inView = TestInView(lightSet.dir, lightSet.dir.magnitude);
                    lightSet.atten = CalcAttenuation(dist);
                    lightSet.color = light.color * lightSet.atten * light.intensity * 0.1f;
                    break;

                case LightType.Spot:
                    lightSet.dir = light.transform.position - posAbs;
                    dist = Mathf.Clamp01(lightSet.dir.magnitude / light.range);
                    float angle = Vector3.Angle(light.transform.forward * -1f, lightSet.dir.normalized);
                    float inFront = Mathf.Lerp(0f, 1f, (light.spotAngle - angle * 2f) / lightSet.dir.magnitude); // More edge fade when far away from light source
                    inView = inFront * TestInView(lightSet.dir, lightSet.dir.magnitude);
                    lightSet.atten = CalcAttenuation(dist);
                    lightSet.color = light.color * lightSet.atten * light.intensity * 0.05f;
                    break;

                default:
                    Debug.Log("Lighting type '" + light.type + "' not supported by Awesome Toon Helper (" + light.name + ").");
                    lightSet.atten = 0f;
                    break;
            }

            // Slowly fade lights on and off
            float fadeSpeed = (Application.isEditor && !Application.isPlaying)
                ? raycastFadeSpeed / 60f
                : raycastFadeSpeed * Time.deltaTime;

            lightSet.inView = Mathf.Lerp(lightSet.inView, inView, fadeSpeed);
            lightSet.color *= Mathf.Clamp01(lightSet.inView);

            return lightSet;
        }

        float TestInView(Vector3 dir, float dist) {
            if (!raycast) return 1.1f;
            RaycastHit hit;
            if (Physics.Raycast(posAbs, dir, out hit, dist, raycastMask)) {
                if(showRaycasts && toggleRaycasts)
                {
                    Debug.DrawRay(posAbs, dir.normalized * hit.distance, Color.red);
                }
                return -0.1f;
            } else {
                if(showRaycasts && toggleRaycasts)
                {
                    Debug.DrawRay(posAbs, dir.normalized * dist, Color.green);
                }
                return 1.1f;
            }
        }

        // Ref - Light Attenuation calc: https://forum.unity.com/threads/light-attentuation-equation.16006/#post-3354254
        float CalcAttenuation(float dist) {
            return Mathf.Clamp01(1.0f / (1.0f + 25f * dist * dist) * Mathf.Clamp01((1f - dist) * 5f));
        }

        private void OnDrawGizmosSelected() {
            Gizmos.DrawWireSphere(posAbs, 0.1f);
        }
    }*/
}