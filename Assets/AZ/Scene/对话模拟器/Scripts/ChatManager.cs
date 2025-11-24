using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
   public GameObject rightBubble;
   public GameObject leftBubble;
   
   public RectTransform contentParent;
   
   public ScrollRect scrollRect;

   public void AddUserChat(string text)
   {
      GameObject newBubbleObj = Instantiate(leftBubble, contentParent);
      
      TextMeshProUGUI textComponent = newBubbleObj.GetComponentInChildren<TextMeshProUGUI>();
      
      if (textComponent != null)
      {
         textComponent.text = text;
      }

      // 2. 获取气泡上的动画脚本
      BubblePopEffect popEffect = newBubbleObj.GetComponent<BubblePopEffect>();

      // 3. 触发弹出动画
      if (popEffect != null)
      {
         popEffect.PlayPopAnimation();
      }
      else
      {
         Debug.LogError("新气泡预制体上没有找到 BubblePopEffect 脚本！");
      }

      // 4. (可选) 确保 ScrollView 自动滚动到最新添加的气泡
      // 启动协程进行滚动
      StartCoroutine(ScrollToBottomCoroutine());
   }
   
   public void AddLLMChat(string text)
   {
      GameObject newBubbleObj = Instantiate(rightBubble, contentParent);
      
      TextMeshProUGUI textComponent = newBubbleObj.GetComponentInChildren<TextMeshProUGUI>();
      
      if (textComponent != null)
      {
         textComponent.text = text;
      }

      // 2. 获取气泡上的动画脚本
      BubblePopEffect popEffect = newBubbleObj.GetComponent<BubblePopEffect>();

      // 3. 触发弹出动画
      if (popEffect != null)
      {
         popEffect.PlayPopAnimation();
      }
      else
      {
         Debug.LogError("新气泡预制体上没有找到 BubblePopEffect 脚本！");
      }
      
      // 4. (可选) 确保 ScrollView 自动滚动到最新添加的气泡
      // 启动协程进行滚动
      StartCoroutine(ScrollToBottomCoroutine());
   }
   
   // private void ScrollToBottom()
   // {
   //    if (scrollRect != null)
   //    {
   //       // 在下一帧执行滚动，确保Content的布局更新完毕
   //       LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
   //       scrollRect.verticalNormalizedPosition = 0f;
   //    }
   // }
   
   private IEnumerator ScrollToBottomCoroutine()
   {
      // 步骤 1: 等待一帧
      // 这一步让 Unity 有机会在正常帧更新中，完成所有 Content Size Fitter 的初次计算。
      yield return null; 

      // 步骤 2: 强制重建布局 1
      // 确保 Content Parent 的 Layout Group 更新。
      LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);

      // 步骤 3: 再次等待一帧
      // 在某些深度嵌套的 Layout 结构中，一次 ForceRebuild 不够。
      // 等待第二帧，让所有 Content Size Fitter 和 Layout Group 之间的依赖关系彻底结算。
      yield return null; 

      // 步骤 4: 强制重建布局 2 (确保最终尺寸确定)
      LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);

      // 步骤 5: 执行滚动
      if (scrollRect != null)
      {
         // 将滚动条位置设置为 0，表示滚动到底部。
         scrollRect.verticalNormalizedPosition = 0f;
            
         // 确保滚动立即生效，而不是在下一帧才开始移动
         Canvas.ForceUpdateCanvases();
      }
   }
}
