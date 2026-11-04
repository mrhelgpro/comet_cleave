using UnityEngine;

namespace Gameplay
{
    public class GameplayInstaller : BaseInstaller
    {
        [SerializeField] private HoldInputController _holdInputController;

        public override void Install(GameContainer container)
        {
            container.BindInstance(_holdInputController);
        }
    }
}
