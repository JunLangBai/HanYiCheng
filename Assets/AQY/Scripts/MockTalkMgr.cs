using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueEditor;
public class MockTalkMgr : MonoBehaviour
{
    public NPCConversation conversation;
    void Start()
    {
        ConversationManager.Instance.StartConversation(conversation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
