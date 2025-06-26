namespace SoundSystem
{
    using UnityEngine;
    using Cysharp.Threading.Tasks;
    using System;
    using System.Threading;

    /// <summary>
    /// AudioSource の音量フェードを制御するユーティリティ
    /// </summary>
    internal sealed class AudioSourceFader : IDisposable
    {
        private readonly AudioSource source;
        private CancellationTokenSource cts;

        public AudioSourceFader(AudioSource source)
        {
            this.source = source;
        }

        public void Cancel()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }

        public async UniTask Fade(float from, float to, float duration)
        {
            Cancel();
            cts = new();
            var token = cts.Token;
            try
            {
                float startTime = Time.time;
                while (true)
                {
                    if (token.IsCancellationRequested) return;
                    float t = (Time.time - startTime) / duration;
                    source.volume = Mathf.Lerp(from, to, t);
                    if (t >= 1f) break;
                    await UniTask.NextFrame(token);
                }
                source.volume = to;
            }
            catch (OperationCanceledException)
            {
                // フェード中断
            }
        }

        public void Dispose()
        {
            Cancel();
        }
    }
}
