using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] RestartUI restartUI;
    const float HOLD_TIME = 3f;

    TacticsControls controls;

    void Awake()
    {
        controls = new TacticsControls();

        controls.Gameplay.Select.performed += OnSelect;
        controls.Gameplay.Cancel.performed += _ => player.Cancel();
        controls.Gameplay.Confirm.performed += _ => player.Confirm();

        controls.Gameplay.Restart.started += _ =>
        {
            if (restartUI) restartUI.Begin(HOLD_TIME);
        };
        controls.Gameplay.Restart.canceled += _ =>
        {
            if (restartUI) restartUI.Cancel();
        };
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
        {
            if (cell.Occupant)  
                player.HandleUnitClick(cell.Occupant);
            else                
                player.HandleCellClick(cell);
            return;
        }

        if (hit.collider.TryGetComponent(out Unit unit))
        {
            if (player.HasSelection && unit == player.SelectedUnit)
            {
                var dest = player.SingleMandatoryCell;
                player.HandleCellClick(dest ?? unit.Cell);
            }
            else
            {
                player.HandleUnitClick(unit);
            }
        }
    }
}
