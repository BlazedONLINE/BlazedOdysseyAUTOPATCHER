using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlazedOdyssey.UI.Common
{
    public class InputShortcuts : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                SceneManager.LoadScene(SceneNames.CharacterSelectionScene);
            }
        }
    }
}


