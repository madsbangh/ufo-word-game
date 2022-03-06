using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class ScreenshotUtility
    {
        [MenuItem("Screenshot Utility/Capture Screenshot")]
        public static void CaptureScreenshot()
        {
            ScreenCapture.CaptureScreenshot($"Screenshot{DateTime.Now:hh-mm-ss}.png");
        }
    }
}