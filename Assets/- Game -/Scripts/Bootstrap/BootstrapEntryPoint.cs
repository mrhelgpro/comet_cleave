using UnityEngine;

namespace Bootstrap
{
    public class BootstrapEntryPoint : MonoBehaviour
    {
        private void Awake()
        {
            if (CoreContext.IsReady)
            {
                CoreContext.Shutdown();
            }
        }

        private void Start()
        {
            SceneLoadService.LoadCoreScene();
        }
    }
}
