//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/Scripts/TouchInputActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @TouchInputActions: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @TouchInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""TouchInputActions"",
    ""maps"": [
        {
            ""name"": ""TouchControls"",
            ""id"": ""a6f096c3-9299-49f1-a715-947c946adf9b"",
            ""actions"": [
                {
                    ""name"": ""TouchPosition"",
                    ""type"": ""Value"",
                    ""id"": ""3ad82ca8-53f4-4a88-8f76-944e5a8c7d48"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""8069bea7-26d0-40d3-a910-f056e125e951"",
                    ""path"": ""<Touchscreen>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""TouchControls"",
                    ""action"": ""TouchPosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""TouchControls"",
            ""bindingGroup"": ""TouchControls"",
            ""devices"": [
                {
                    ""devicePath"": ""<Touchscreen>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // TouchControls
        m_TouchControls = asset.FindActionMap("TouchControls", throwIfNotFound: true);
        m_TouchControls_TouchPosition = m_TouchControls.FindAction("TouchPosition", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // TouchControls
    private readonly InputActionMap m_TouchControls;
    private List<ITouchControlsActions> m_TouchControlsActionsCallbackInterfaces = new List<ITouchControlsActions>();
    private readonly InputAction m_TouchControls_TouchPosition;
    public struct TouchControlsActions
    {
        private @TouchInputActions m_Wrapper;
        public TouchControlsActions(@TouchInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @TouchPosition => m_Wrapper.m_TouchControls_TouchPosition;
        public InputActionMap Get() { return m_Wrapper.m_TouchControls; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(TouchControlsActions set) { return set.Get(); }
        public void AddCallbacks(ITouchControlsActions instance)
        {
            if (instance == null || m_Wrapper.m_TouchControlsActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_TouchControlsActionsCallbackInterfaces.Add(instance);
            @TouchPosition.started += instance.OnTouchPosition;
            @TouchPosition.performed += instance.OnTouchPosition;
            @TouchPosition.canceled += instance.OnTouchPosition;
        }

        private void UnregisterCallbacks(ITouchControlsActions instance)
        {
            @TouchPosition.started -= instance.OnTouchPosition;
            @TouchPosition.performed -= instance.OnTouchPosition;
            @TouchPosition.canceled -= instance.OnTouchPosition;
        }

        public void RemoveCallbacks(ITouchControlsActions instance)
        {
            if (m_Wrapper.m_TouchControlsActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(ITouchControlsActions instance)
        {
            foreach (var item in m_Wrapper.m_TouchControlsActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_TouchControlsActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public TouchControlsActions @TouchControls => new TouchControlsActions(this);
    private int m_TouchControlsSchemeIndex = -1;
    public InputControlScheme TouchControlsScheme
    {
        get
        {
            if (m_TouchControlsSchemeIndex == -1) m_TouchControlsSchemeIndex = asset.FindControlSchemeIndex("TouchControls");
            return asset.controlSchemes[m_TouchControlsSchemeIndex];
        }
    }
    public interface ITouchControlsActions
    {
        void OnTouchPosition(InputAction.CallbackContext context);
    }
}