using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class MorseStackerMono : MonoBehaviour
{
    [SerializeField] MorseStackValidatedEvent m_onStackValidated = new MorseStackValidatedEvent();
    [System.Serializable]
    [SerializeField] class MorseStackValidatedEvent : UnityEvent<MorseStack> { }
    [SerializeField] MorseStack m_lastValidation = new MorseStack("");
    [SerializeField] MorseStack m_lastChange = new MorseStack("");

    private void OnEnable()
    {
        MorseStacker.AddStackValidatedListener(NotifyValidation);
        MorseStacker.AddStackChangeListener(NotifyChange);
    }
    private void OnDisable()
    {
        MorseStacker.RemoveStackValidatedListener(NotifyValidation);
        MorseStacker.RemoveStackChangeListener(NotifyChange);
    }

    private void NotifyChange(MorseStack stackCopyInfo)
    {
        m_lastChange = stackCopyInfo;
    }

    private void NotifyValidation(MorseStack stackCopyInfo)
    {
        m_lastValidation = stackCopyInfo;
        m_onStackValidated.Invoke(stackCopyInfo);
    }

    public void Update()
    {
        MorseStacker.CheckAutoValidationWithTimepass(Time.deltaTime);
    }

}

public class MorseStacker
{

    public delegate void MorseStackEvent(MorseStack stackCopyInfo);
    private static MorseStackEvent m_onStackChange;
    private static MorseStackEvent m_onStackValidated;

    public static void AddStackChangeListener(MorseStackEvent listener) { m_onStackChange += listener; }
    public static void RemoveStackChangeListener(MorseStackEvent listener) { m_onStackChange -= listener; }
    public static void AddStackValidatedListener(MorseStackEvent listener) { m_onStackValidated += listener; }
    public static void RemoveStackValidatedListener(MorseStackEvent listener) { m_onStackValidated -= listener; }
    public static Dictionary<string, MorseStack> m_stacks = new Dictionary<string, MorseStack>();
    public static Dictionary<string, float> m_stacksAutoValidationDelay = new Dictionary<string, float>();

    public static void StackOn(string stackName, MorseKey key)
    {
        CheckIfExisting(stackName);
        MorseStack ms = m_stacks[stackName];
        ms.Add(key);
        if(m_onStackChange!=null)   
            m_onStackChange(ms);
    }
    public static void StackOn(string stackName, MorseKey key, float validationDelay = 0.5f)
    {
        StackOn(stackName, key);
        SetValidationDelay(stackName, validationDelay);
    }

    private static MorseStackerMono m_instanceInScene;
    private static void CheckForInstanceInScene()
    {
        if (m_instanceInScene != null)
            return;
        m_instanceInScene = GameObject.FindObjectOfType<MorseStackerMono>();
        if (m_instanceInScene != null)
            return;
        GameObject inst = new GameObject("Morse Stacker Mono (Runtime instance)");
        m_instanceInScene=inst.AddComponent<MorseStackerMono>();
    }

    private static void CheckIfExisting(string stackName)
    {
        if (!m_stacks.ContainsKey(stackName))
            m_stacks.Add(stackName, new MorseStack(stackName));
        if (!m_stacksAutoValidationDelay.ContainsKey(stackName))
            m_stacksAutoValidationDelay.Add(stackName, 0f);
    }

   

    private static void SetValidationDelay(string stackName, float validationDelay)
    {
        CheckForInstanceInScene();
        CheckIfExisting(stackName);
        m_stacksAutoValidationDelay[stackName] = validationDelay;
    }

    public static void Validate(string stackName)
    {
        MorseStack info;
        Validate(stackName, out info);

    }
    public static void Cancel(string stackName)
    {
        CheckIfExisting(stackName);
        m_stacks[stackName].Clear();

    }
    public static void Validate(string stackName, out MorseStack info) {
        info = GetCopyAndReset(stackName);
        if (m_onStackValidated != null)
            m_onStackValidated(info);

    }
    private static MorseStack GetCopyAndReset(string stackName) {
        CheckIfExisting(stackName);
        MorseStack copy = m_stacks[stackName].GetCopy();
        m_stacks[stackName].Clear();
        return copy;
    }

    public static List<string> GetStackNameList()
    {
        return m_stacks.Keys.ToList();
    }
    public static void CheckAutoValidationWithTimepass(float timepass)
    {
        List<string> names = GetStackNameList();
        float timeLeft;
        for (int i = 0; i < names.Count; i++)
        {
            CheckIfExisting(names[i]);
            timeLeft = m_stacksAutoValidationDelay[names[i]];
            if (timeLeft > 0f) {
                timeLeft -= timepass;
                if (timeLeft <= 0f) {
                    timeLeft = 0f;
                    Validate(names[i]);
                }
                m_stacksAutoValidationDelay[names[i]] = timeLeft;
            }
        }
    }

    internal static bool AreEquals(MorseStack a, MorseStack b)
    {
        if (a.GetIdName() != b.GetIdName()) 
            return false;
        return a.GetMorseValue() == b.GetMorseValue();
        
    }
}
[System.Serializable]
public class MorseStack
{
    public MorseStack(string name)
    {
        m_nameId = name;
    }
    public string m_nameId;
    public List<MorseKey> m_currentMorse = new List<MorseKey>();

    public string GetIdName() { return m_nameId;}
    public void Add(MorseKey key)
    {
        m_currentMorse.Add(key);
    }
    public void Clear() { m_currentMorse.Clear(); }
    public MorseValue GetMorseValue() { return new MorseValue(m_currentMorse.ToArray()); }

    public MorseStack GetCopy()
    {
        MorseStack cpy = new MorseStack(GetIdName());
        for (int i = 0; i < m_currentMorse.Count; i++)
        {
            cpy.Add(m_currentMorse[i]);
        }
        return cpy;
    }

   
}
