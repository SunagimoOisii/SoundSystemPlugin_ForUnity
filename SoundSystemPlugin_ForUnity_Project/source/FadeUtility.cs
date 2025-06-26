namespace SoundSystem
{
    using System;
    using System.Threading;
    using UnityEngine;
    using Cysharp.Threading.Tasks;

    /// <summary>
    /// AudioSource の音量フェードを共通処理として提供するヘルパー
    /// </summary>
    internal static class FadeUtility
    {
        public static async UniTask ExecuteVolumeTransition(
            CancellationTokenSource current,
            float duration,
            Action<float> onProgress,
            Action onComplete = null,
            Action<CancellationToken> onCanceled = null,
            Action<CancellationTokenSource> onCreated = null)
        {
            current?.Cancel();
            current?.Dispose();
            var cts = new CancellationTokenSource();
            onCreated?.Invoke(cts);
            var token = cts.Token;

            try
            {
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    if (token.IsCancellationRequested) return;

                    float t = elapsed / duration;
                    onProgress(t);

                    elapsed += Time.deltaTime;
                    await UniTask.Yield();
                }

                onProgress(1.0f);
                onComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                Log.Safe("ExecuteVolumeTransition中断:OperationCanceledException");
                onCanceled?.Invoke(token);
            }
        }
    }
}
