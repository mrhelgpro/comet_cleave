# Сomet Сleave

Unity prototype. A ball player in the bottom-left shoots by spending its own size: the longer the hold, the larger the shot and the obstacle infection radius. The ball hops along the cleared path to the goal; the door opens at 5 meters. The run is lost if the ball is fully spent on a shot, or if there is not enough size left to clear the path. The starting size includes a 20% margin.

## Run

Requires **Unity 6000.3.15f1**. In Unity Hub: **Add** → this folder → **Open**.

Scenes are in `Assets/- Scenes -/`:

| Scene | Role |
|-------|------|
| `BootstrapScene` | First scene of a build (build index 0). Loads Core, then gameplay. |
| `CoreScene` | Runtime root: DI and save. |
| `GameplayScene` | The level. |

For a **build**, the game starts from `Assets/- Scenes -/BootstrapScene.unity` (it is already first in Build Settings).

For **testing and development**, press Play from any open scene.
