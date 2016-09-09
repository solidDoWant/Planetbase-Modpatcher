using Planetbase;
using Redirection;
using UnityEngine;

namespace FreeFurnishing
{
    public class FreeFurnishing : IMod
    {
        public void Init()
        {
            Redirector.PerformRedirections();
            Debug.Log("[MOD] FreeFurnishing activated");

            GameObject go = new GameObject();
            go.AddComponent<xxx>();
            GameObject.DontDestroyOnLoad(go);
        }

        public void Update()
        {
        }
    }

    public class xxx : MonoBehaviour
    {
        public void OnGUI()
        {
            DebugManager.getInstance().mEnabled = true;
            DebugManager.getInstance().onGui();
        }
    }

    public abstract class CustomModule : Module
    {
        [RedirectFrom(typeof(Module))]
        public new bool canPlaceComponent(ConstructionComponent component)
        {
            if (Input.GetKeyUp(KeyCode.X))
            {
                // rotate
                component.getTransform().Rotate(Vector3.up * 15f);
            }

            // step
            Vector3 fromCenter = component.getPosition() - getPosition();
            fromCenter.x = Mathf.Round(fromCenter.x * 2f) * 0.5f;
            fromCenter.z = Mathf.Round(fromCenter.z * 2f) * 0.5f;
            component.setPosition(getPosition() + fromCenter);

            clampComponentPosition(component);
            return !this.intersectsAnyComponents(component) /*&& this.isValidLayoutPosition(component)*/;

            //bool flag = true;
            //if (this.mComponentLocations != null)
            //{
            //    this.snapToComponentLocation(component);
            //}
            //else
            //{
            //    this.clampComponentPosition(component);
            //    flag = this.isValidLayoutPosition(component);
            //}
            //return !this.intersectsAnyComponents(component) && flag;
        }
    }
}
