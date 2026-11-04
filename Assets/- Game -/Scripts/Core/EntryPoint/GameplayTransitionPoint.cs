public class GameplayTransitionPoint : IInitializable
{
    void IInitializable.Initialize()
    {
        if (SceneLoadService.ShouldRestoreScene)
        {
            SceneLoadService.RestoreScene();
            return;
        }

        SceneLoadService.LoadGameplayScene();
    }
}
