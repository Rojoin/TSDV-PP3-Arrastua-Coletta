using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameInputs
{
    public class InputController : MonoBehaviour
    {
        [Header("Inputs")]
        [SerializeField] private Vector2ChannelSO OnMoveChannel;
        [SerializeField] private Vector2ChannelSO OnGridMoveChannel;
        [SerializeField] private VoidChannelSO OnPauseChannel;
        [SerializeField] private VoidChannelSO OnInteractChannel;
        [SerializeField] private VoidChannelSO OnBackInteractChannel;
        [SerializeField] private VoidChannelSO OnCheatKillScreenEnemy;
        [SerializeField] private VoidChannelSO OnCheatWinGame;
        [SerializeField] private VoidChannelSO OnCheatLoseGame;
        [SerializeField] private VoidChannelSO OnCheatToggleConsole;
        [SerializeField] private IntChannelSO OnControlSchemeChange;
        [Header("Data")]
        [SerializeField] private PlayerStats _playerStats;
        [SerializeField] float offsetController = 0.70f;
        private Vector2 previousGridInput = Vector2.zero;
        private bool cheats;
        private const int keyboardSchemeValue = 0;
        private const int gamepadSchemeValue = 1;

        public void OnChangeInput(PlayerInput input)
        {
            string inputCurrentControlScheme = input.currentControlScheme;
            if (inputCurrentControlScheme.Equals("Gamepad"))
            {
                OnControlSchemeChange.RaiseEvent(gamepadSchemeValue);
                Debug.Log("Using Gamepad:" + inputCurrentControlScheme);
                _playerStats.ChangeControllerInput(gamepadSchemeValue);
            }
            else
            {
                OnControlSchemeChange.RaiseEvent(keyboardSchemeValue);
                _playerStats.ChangeControllerInput(keyboardSchemeValue);
                Debug.Log("Using Mouse & Keywoard");
            }
        }

        public void OnMove(InputAction.CallbackContext ctx)
        {
            OnMoveChannel.RaiseEvent(ctx.ReadValue<Vector2>());
        }

        public void OnGridMove(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                Vector2 ctxInput = ctx.ReadValue<Vector2>();

                OnGridMoveChannel.RaiseEvent(ctxInput);
            }

            if (ctx.canceled)
            {
                OnGridMoveChannel.RaiseEvent(Vector2.zero);
            }
        }

        public void OnStickGridMove(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                Vector2 ctxInput = ctx.ReadValue<Vector2>();

                if (ctxInput.x > offsetController)
                {
                    ctxInput.x = 1;
                }
                else if (ctxInput.x < -offsetController)
                {
                    ctxInput.x = -1;
                }
                else
                {
                    ctxInput.x = 0;
                }

                if (ctxInput.y > offsetController)
                {
                    ctxInput.y = 1;
                }
                else if (ctxInput.y < -offsetController)
                {
                    ctxInput.y = -1;
                }
                else
                {
                    ctxInput.y = 0;
                }

                var newValue = ctxInput;

                if (newValue != previousGridInput)
                {
                    OnGridMoveChannel.RaiseEvent(newValue);
                    previousGridInput = newValue;
                }
            }

            if (ctx.canceled)
            {
                OnGridMoveChannel.RaiseEvent(Vector2.zero);
            }
        }

        public void OnInteract(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                OnInteractChannel.RaiseEvent();
            }
        }

        public void OnBackInteract(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                OnBackInteractChannel.RaiseEvent();
            }
        }

        public void OnPauseMode(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                OnPauseChannel.RaiseEvent();
            }
        }

        public void OnCheatsSetState(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                cheats = !cheats;
                if (cheats)
                {
                    Debug.Log("Cheats Enable");
                }
                else
                {
                    Debug.Log("Cheats Disable");
                }
            }
        }

        public void OnCheatKillScreenEnemies(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                if (cheats)
                {
                    OnCheatKillScreenEnemy.RaiseEvent();
                }
            }
        }

        public void OnCheatWinScreen(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                if (cheats)
                {
                    OnCheatWinGame.RaiseEvent();
                }
            }
        }

        public void OnCheatLoseScreen(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                if (cheats)
                {
                    OnCheatLoseGame.RaiseEvent();
                }
            }
        }

        public void OnCheatTimeSpeed(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                if (cheats)
                {
                    if (Time.timeScale != 1)
                    {
                        Time.timeScale = 1;
                    }
                    else
                    {
                        Time.timeScale = 4;
                    }
                }
            }
        }

        public void OnShowConsole(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                if (cheats)
                {
                    OnCheatToggleConsole.RaiseEvent();
                }
            }
        }
    }
}