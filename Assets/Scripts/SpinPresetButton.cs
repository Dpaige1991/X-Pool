using UnityEngine;

public class SpinPresetButton : MonoBehaviour
{
    public CueStickController cue;

    int index = 0;

    // Call this from the Button OnClick
    public void CycleSpinPreset()
    {
        if (!cue) return;

        index = (index + 1) % 6;

        switch (index)
        {
            case 0: cue.SetSpin(Vector2.zero); break;                 // center
            case 1: cue.SetSpin(new Vector2(-1f, 0f)); break;         // left
            case 2: cue.SetSpin(new Vector2(1f, 0f)); break;         // right
            case 3: cue.SetSpin(new Vector2(0f, 1f)); break;         // top
            case 4: cue.SetSpin(new Vector2(0f, -1f)); break;         // back
            case 5: cue.SetSpin(new Vector2(0.7f, -0.7f)); break;     // side+draw (example)
        }
    }
}