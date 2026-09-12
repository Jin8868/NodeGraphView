using Game.NodeGraphs;
using UnityEngine;
using UnityEngine.UI;

namespace Game.NodeGraphs.View
{
    public interface ITaskPanelCallbacks
    {
        void OnComplete();
        void OnFail();
    }

    public class TaskPanel
    {
        private readonly GameObject panelView;
        private readonly Text titleText;
        private readonly Text descText;
        private readonly Button completeButton;
        private readonly Button failButton;
        private ITaskPanelCallbacks callbacks;

        public TaskPanel(Transform root)
        {
            if (root == null)
            {
                panelView = null;
                titleText = null;
                descText = null;
                completeButton = null;
                failButton = null;
                return;
            }

            Transform bg = root.Find("bg");
            panelView = bg == null ? root.gameObject : bg.gameObject;
            titleText = FindChildComponent<Text>(root, "bg/title");
            descText = FindChildComponent<Text>(root, "bg/desc");
            completeButton = FindChildComponent<Button>(root, "bg/CompleteBtn");
            failButton = FindChildComponent<Button>(root, "bg/FailBtn");

            BindButton(completeButton, true);
            BindButton(failButton, false);
        }

        public void SetCallbacks(ITaskPanelCallbacks panelCallbacks)
        {
            callbacks = panelCallbacks;
        }

        public void Refresh(string title, string description, bool canComplete, bool canFail)
        {
            if (titleText != null)
            {
                titleText.text = title ?? string.Empty;
            }

            if (descText != null)
            {
                descText.text = description ?? string.Empty;
            }

            SetButtonVisible(completeButton, canComplete);
            SetButtonVisible(failButton, canFail);
            Show();
        }

        public void Show()
        {
            if (panelView != null)
            {
                panelView.SetActive(true);
            }
        }

        public void Hide()
        {
            if (panelView != null)
            {
                panelView.SetActive(false);
            }
        }

        private void BindButton(Button button, bool complete)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            if (complete)
            {
                button.onClick.AddListener(() => callbacks?.OnComplete());
            }
            else
            {
                button.onClick.AddListener(() => callbacks?.OnFail());
            }
        }

        private static T FindChildComponent<T>(Transform root, string path) where T : Component
        {
            Transform child = root.Find(path);
            return child == null ? null : child.GetComponent<T>();
        }

        private static void SetButtonVisible(Button button, bool visible)
        {
            if (button != null)
            {
                button.gameObject.SetActive(visible);
            }
        }
    }
}
