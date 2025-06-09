using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    [SerializeField] PlayerController player;
    TacticsControls controls;

    void Awake()
    {
        controls = new TacticsControls();

        controls.Gameplay.Select.performed += OnSelect;
        controls.Gameplay.Cancel.performed += _ => player.Cancel();
        controls.Gameplay.Confirm.performed += _ => player.Confirm();

        controls.Gameplay.Restart.performed += _ =>
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void OnSelect(InputAction.CallbackContext ctx)
    {
        var pos = Mouse.current.position.ReadValue();
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(pos), out var hit)) return;

        if (hit.collider.TryGetComponent(out Cell cell))
            player.HandleCellClick(cell);
        else if (hit.collider.TryGetComponent(out Unit unit))
            player.HandleUnitClick(unit);
    }
}
