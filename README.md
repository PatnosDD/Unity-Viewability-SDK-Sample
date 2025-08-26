# Unity Viewability SDK Sample

This is my submission for a candidate assignment where I had to build a core piece of an ad SDK for Unity. I chose to tackle the challenge of **impression validation**: how to accurately and efficiently determine if an in-game ad is *actually* visible to a player before counting it as a valid view.

The system is designed with performance as a top priority. Each ad is monitored by an individual tracker, but a central manager staggers the intensive visibility checks across multiple frames. This ensures the game's frame rate remains smooth, even with many ads on screen.

To be both fast and accurate, the visibility check itself is a multi-stage funnel:

1.  **Initial Filtering:** First, it uses fast, broad checks to immediately ignore any ads that are obviously off-screen or behind the player.
2.  **Detailed Checks:** Next, it verifies that the ad is generally facing the camera and is large enough on screen to be readable.
3.  **Final Occlusion Check:** Finally, it performs a precise 3-point raycast to the ad's center and corners to ensure the line of sight is not blocked by any 3D objects.

### How to Run

The sample scene uses Unity's standard **Third-Person Character Controller** for demonstration.

1.  Open the project in **Unity 2021.3.45f1** or newer.
2.  Open the `PlayGround` scene.
3.  Hit **Play**.

Once it's running, switch to the **Scene View** (and make sure Gizmos are turned on!). You'll see the three debug raycasts shooting from the camera. They'll change color to show you what's happening:
* **Green:** The path is clear!
* **Red:** The path is blocked by something.