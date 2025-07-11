# SoundSystem for Unity

## 概要
Unity 上での BGM・SE 管理を一本化するためのライブラリです。プリセットによる設定管理、AudioSource プール、複数方式のキャッシュ、ログ出力などを備え、ゲーム内のサウンド制御をシンプルにします。

## 主な機能
- BGM 再生：FadeIn / FadeOut / CrossFade に対応、フェード処理の中断が可能
- SE 再生：AudioSource プールで効率的に管理（FIFO または Strict）、FadeIn / 全体フェードアウト対応、フェード処理の中断
- SoundLoader：Addressables / Resources / Streaming から選択可能
- SoundCache：戦略クラスで LRU / TTL / Random / None の削除方式を切り替え
- SoundPresetProperty：BGM・SE のプリセット設定を ScriptableObject として管理（検索機能付き）
- ListenerEffector：AudioListener へのフィルター適用・無効化
- オートエビクト：一定間隔でキャッシュを自動削除
- 使用中のサウンドはキャッシュから削除しない参照カウント機能
- オートディスポーズ：シーン変更時の自動解放を選択可能
- ロギング：Safe / Warn / Error の 3 段階でログファイルを出力

## 必要環境
- Unity 2023 以降
- UniTask
- Addressables（`USE_ADDRESSABLES` 定義時）

## 導入方法
1. UniTask と（必要に応じて）Addressables をプロジェクトに導入します。
2. 本リポジトリをビルドして生成される `SoundSystem.dll` を `Assets/Plugins` に配置します。
3. Addressables を利用する場合は Player Settings の `Scripting Define Symbols` に `USE_ADDRESSABLES` を追加します。

## 初期化例
### 手動構成
```csharp
var cache  = SoundCacheFactory.CreateLRU(30f);
var loader = SoundLoaderFactory.Create(SoundLoaderFactory.Kind.Streaming, cache);
var pool   = AudioSourcePoolFactory.Create(
    AudioSourcePoolFactory.Kind.FIFO,
    mixerGroup,
    initSize: 8,
    maxSize: 32);
var soundSystem = new SoundSystem(
    loader,
    cache,
    pool,
    listener,
    mixer,
    mixerGroup,
    persistent: true);
soundSystem.StartAutoEvict(60f);
```
### プリセット利用
```csharp
var soundSystem = SoundSystem.CreateFromPreset(
    preset,
    listener,
    mixer,
    persistent: true);
```

## 使い方
### BGM
```csharp
await soundSystem.PlayBGM("bgm_title", 1.0f);
await soundSystem.FadeInBGM("bgm_intro", 2.0f, 1.0f);
await soundSystem.CrossFadeBGM("bgm_battle", 2.0f);
await soundSystem.PlayBGMWithPreset("bgm_battle", "BattlePreset");
soundSystem.InterruptBGMFade();
```
### SE
```csharp
await soundSystem.PlaySE("se_click", Vector3.zero, 1.0f, 1.0f, 1.0f);
await soundSystem.PlaySEWithPreset("se_explosion", "ExplosionPreset");
await soundSystem.FadeInSE("se_wind", 1.5f);
await soundSystem.FadeOutAllSE(1.0f);
await soundSystem.FadeInSEWithPreset("se_magic", "MagicPreset");
soundSystem.InterruptAllSEFade();
```
### Mixer パラメータ
```csharp
float? volume = soundSystem.RetrieveMixerParameter("MasterVolume");
soundSystem.SetMixerParameter("MasterVolume", -10.0f);
```
### エフェクト操作
```csharp
soundSystem.ApplyEffectFilter<AudioReverbFilter>(f => f.reverbLevel = 1000f);
soundSystem.DisableAllEffectFilter();
```

### キャッシュ状況の確認
```csharp
int count = cache.Count;
foreach (var key in cache.Keys)
{
    Debug.Log($"Cached:{key}");
}
```

### Listenerエフェクトプリセット設定
`SoundPresetProperty` の `listenerPresets` にフィルター設定を登録しておくと、
`SoundSystem.CreateFromPreset` 実行時に自動で適用されます。
フィルターの種類は `FilterKind` から選択し、
選択したフィルターに応じた項目がインスペクター上で表示されます。
プリセット一覧上で各フィルターの詳細を直接編集できるため、
別Assetを開かなくても設定が完結します。

## システム構成

### SoundSystem
```mermaid
graph TD
classDef highlight stroke-width:8px
SoundSystem:::highlight
BGMManager
SEManager
ListenerEffector
SoundLoader群
SoundCache群
SoundPresetProperty
SoundSystem -->|利用| BGMManager
SoundSystem -->|利用| SEManager
SoundSystem -->|利用| ListenerEffector
SoundSystem -->|依存| SoundLoader群
SoundSystem -->|依存| SoundCache群
SoundSystem -->|プリセット読込| SoundPresetProperty
```

### SoundLoader
```mermaid
graph TD
classDef highlight stroke-width:8px
SoundLoaderFactory:::highlight
ISoundLoader
SoundLoader_Addressables
SoundLoader_Resources
SoundLoader_Streaming
SoundCache群
SoundLoaderFactory -->|生成| SoundLoader_Addressables
SoundLoaderFactory -->|生成| SoundLoader_Resources
SoundLoaderFactory -->|生成| SoundLoader_Streaming
SoundLoader_Addressables -->|依存| SoundCache群
SoundLoader_Resources -->|依存| SoundCache群
SoundLoader_Streaming -->|依存| SoundCache群
```

### SoundCache
```mermaid
graph TD
classDef highlight stroke-width:8px
ISoundCache:::highlight
SoundCache
SoundCacheFactory
IEvictionStrategy
EvictionStrategy_LRU
EvictionStrategy_TTL
EvictionStrategy_Random
SoundCacheFactory -->|生成| SoundCache
SoundCacheFactory -->|生成| EvictionStrategy_LRU
SoundCacheFactory -->|生成| EvictionStrategy_TTL
SoundCacheFactory -->|生成| EvictionStrategy_Random
SoundCache -->|実装| ISoundCache
SoundCache -->|利用| IEvictionStrategy
EvictionStrategy_LRU -->|実装| IEvictionStrategy
EvictionStrategy_TTL -->|実装| IEvictionStrategy
EvictionStrategy_Random -->|実装| IEvictionStrategy
```

### SoundPreset
```mermaid
graph TD
classDef highlight stroke-width:8px
SoundPresetProperty:::highlight
SerializedBGMPresetDictionary
SerializedSEPresetDictionary
SerializedListenerPresetDictionary
SoundPresetProperty -->|BGMプリセット| SerializedBGMPresetDictionary
SoundPresetProperty -->|SEプリセット| SerializedSEPresetDictionary
SoundPresetProperty -->|Listenerエフェクトプリセット| SerializedListenerPresetDictionary
```

## 既存コードへの影響
`ISoundCache` に参照カウント用の `BeginUse` / `EndUse` が追加されました。
加えて、キャッシュ件数を取得する `Count` プロパティと、登録キーを列挙する `Keys` プロパティを実装しました。
既存の実装クラスを利用している場合は、これらのメンバーを適宜利用してください。

## ライセンス
本リポジトリは MIT ライセンスで公開されています。詳細は [LICENSE](LICENSE) を参照してください。
また、依存ライブラリの UniTask も MIT ライセンスで提供されています。第三者ライセンスの一覧は [THIRD-PARTY-LICENSE](THIRD-PARTY-LICENSE) を参照してください。
