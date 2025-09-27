# Azure Speech Services 話者分離デモ

このプロジェクトは、Azure Speech Servicesを使用して音声の話者分離（Speaker Diarization）を実行するC#コンソールアプリケーションです。

## 機能

- **多言語対応**: 日本語、英語（US/UK）、中国語、韓国語、スペイン語、フランス語、ドイツ語、イタリア語、ポルトガル語に対応
- **複数の処理モード**: 
  - リアルタイム話者分離（マイク入力）
  - ファイル処理（リアルタイム処理）
  - **高速バッチ処理**（推奨：2-10倍高速、60%安価）
- **コマンドライン対応**: 引数指定で即座に実行開始
- **言語切り替え**: 実行中に言語を変更可能
- **結果保存**: 話者分離結果をテキストファイルに保存
- **タイムスタンプ付き出力**: 各発話の開始・終了時刻を記録
- **話者統計**: 検出された話者ごとの発話回数と合計時間を表示

## 📊 処理方式の比較

| 項目 | リアルタイム処理 | バッチ処理 |
|------|----------------|-----------|
| **処理速度** | 1.0x-2.0x（音声と同じ～2倍） | **0.1x-0.5x**（2-10倍高速） |
| **料金** | $2.50/時間 | **$1.00/時間**（60%安い） |
| **精度** | 高精度 | **より高精度** |
| **推奨用途** | 短時間音声、プログレッシブ表示 | **長時間音声、本格運用** |
| **ファイルサイズ制限** | 実質無制限（4時間） | **1GB（10時間）** |
| **結果取得** | リアルタイム | 完了後一括 |

## 🔧 セットアップ

### 1. Azure Speech Servicesの設定

1. [Azure Portal](https://portal.azure.com)にアクセス
2. 「Cognitive Services」または「Speech Services」を検索して作成
3. リソースを作成後、「キーとエンドポイント」からキーとリージョンを確認

### 2. 環境変数の設定

#### 基本設定（通常はこれだけでOK）
```bash
# 必須
export SPEECH_KEY=your_speech_key_here
export SPEECH_REGION=japaneast
```

#### 高度な設定（オプション）
```bash
# カスタムエンドポイントを使用する場合（通常は不要）
export SPEECH_ENDPOINT=https://japaneast.api.cognitive.microsoft.com
```

#### Windows設定例
```cmd
# Command Prompt
set SPEECH_KEY=your_speech_key_here
set SPEECH_REGION=japaneast
set SPEECH_ENDPOINT=https://japaneast.api.cognitive.microsoft.com

# PowerShell
$env:SPEECH_KEY="your_speech_key_here"
$env:SPEECH_REGION="japaneast"
$env:SPEECH_ENDPOINT="https://japaneast.api.cognitive.microsoft.com"
```

## 🚀 クイックスタート

### 1. 最速実行（推奨）

```bash
# 環境変数設定
export SPEECH_KEY=your_speech_key_here
export SPEECH_REGION=eastus

# バッチ処理で即座に開始（英語）
dotnet run /path/to/your/audio.wav

# 日本語音声の場合
dotnet run --language ja-JP /path/to/your/audio.wav
```

### 2. 便利なスクリプト使用

```bash
# プロジェクトルートから
./quick_batch_diarization.sh /path/to/audio.wav
./quick_batch_diarization.sh audio.wav ja-JP
```

### 3. インタラクティブモード

```bash
# 従来の対話型インターフェース
dotnet run
```

## 📋 コマンドライン使用方法

### 基本構文
```bash
dotnet run [オプション] <WAVファイルパス>
```

### オプション一覧

| オプション | 短縮形 | デフォルト | 説明 |
|-----------|--------|-----------|------|
| `--file <path>` | `-f` | - | WAVファイルパス |
| `--language <lang>` | `-l` | `en-US` | 認識言語 |
| `--mode <mode>` | `-m` | `batch` | 処理モード |
| `--help` | `-h` | - | ヘルプ表示 |

### 言語指定

| 言語 | 正式コード | 短縮形 |
|------|------------|-------|
| 英語（米国） | `en-US` | `en`, `english` |
| 英語（英国） | `en-GB` | - |
| 日本語 | `ja-JP` | `ja`, `jp`, `japanese` |
| 中国語 | `zh-CN` | `zh`, `cn`, `chinese` |
| 韓国語 | `ko-KR` | `ko`, `kr`, `korean` |
| スペイン語 | `es-ES` | `es`, `spanish` |
| フランス語 | `fr-FR` | `fr`, `french` |
| ドイツ語 | `de-DE` | `de`, `german` |
| イタリア語 | `it-IT` | `it`, `italian` |
| ポルトガル語 | `pt-BR` | `pt`, `portuguese` |

### 処理モード

| モード | 説明 | 特徴 |
|-------|------|------|
| `batch` | 高速バッチ処理 | **推奨**: 2-10倍高速、60%安価、高精度 |
| `realtime` | リアルタイム処理 | プログレッシブ表示 |

### 使用例

```bash
# 基本的な使用（英語、バッチ処理）
dotnet run /Users/john/podcast.wav

# 日本語音声をリアルタイム処理
dotnet run --language ja-JP --mode realtime /Users/john/meeting.wav

# 短縮形を使用
dotnet run -l japanese -m batch audio.wav

# ヘルプ表示
dotnet run --help
```

### 3. プロジェクトのビルドと実行

```bash
# プロジェクトディレクトリに移動
cd AzureSpeechDiarization

# 依存関係の復元とビルド
dotnet restore
dotnet build

# インタラクティブモードで実行
dotnet run

# コマンドライン引数で直接実行
dotnet run /path/to/audio.wav
```

## 📱 使用方法

### インタラクティブモード

### インタラクティブモード

アプリケーションを引数なしで起動すると、まず言語選択画面が表示されます：

### 言語選択
```
=== 言語選択 ===
使用する言語を選択してください:
1. 日本語 (ja-JP)
2. English (US) - en-US
3. English (UK) - en-GB
4. 中文 (简体) - zh-CN
5. 한국어 - ko-KR
6. Español - es-ES
7. Français - fr-FR
8. Deutsch - de-DE
9. Italiano - it-IT
10. Português (Brasil) - pt-BR
```

言語を選択した後、以下のオプションが表示されます：

1. **マイクからリアルタイム話者分離**
   - マイクから音声を入力し、リアルタイムで話者を分離
   - Enterキーを押すと停止

2. **WAVファイルから話者分離（リアルタイム処理）**
   - 指定したWAVファイルからリアルタイム処理で話者分離
   - プログレッシブ表示

3. **WAVファイルから話者分離（高速バッチ処理）** 🚀
   - 高速・安価・高精度なバッチ処理
   - 長時間音声に最適

4. **言語を変更**
   - 認識言語を変更

5. **終了**
   - アプリケーションを終了

### コマンドラインモード

引数を指定して直接実行：

```bash
# 基本的な実行
dotnet run audio.wav

# 詳細指定
dotnet run --language ja-JP --mode batch /Users/john/meeting.wav
```

## 🌍 対応言語

| 言語 | 言語コード | 備考 |
|------|------------|------|
| 日本語 | ja-JP | 日本語音声認識 |
| English (US) | en-US | 米国英語音声認識 |
| English (UK) | en-GB | 英国英語音声認識 |
| 中文 (简体) | zh-CN | 中国語簡体字音声認識 |
| 한국어 | ko-KR | 韓国語音声認識 |
| Español | es-ES | スペイン語音声認識 |
| Français | fr-FR | フランス語音声認識 |
| Deutsch | de-DE | ドイツ語音声認識 |
| Italiano | it-IT | イタリア語音声認識 |
| Português (Brasil) | pt-BR | ブラジル系ポルトガル語音声認識 |

## 📤 出力結果

### コンソール表示例

### 🔧 標準設定（推奨）
通常は **SPEECH_KEY** と **SPEECH_REGION** のみ設定すれば十分です。
```bash
export SPEECH_KEY=your_key_here
export SPEECH_REGION=japaneast
```

### 🔧 カスタムエンドポイント（オプション）
以下の場合にのみ **SPEECH_ENDPOINT** を設定してください：
- **プライベートエンドポイント**: 企業環境での専用接続
- **特定地域指定**: パフォーマンス最適化のため
- **高度なセキュリティ**: 企業セキュリティポリシー要件

```bash
export SPEECH_ENDPOINT=https://japaneast.api.cognitive.microsoft.com
```

### エンドポイント形式例
| リージョン | エンドポイント |
|-----------|---------------|
| 東日本 | `https://japaneast.api.cognitive.microsoft.com` |
| 西日本 | `https://japanwest.api.cognitive.microsoft.com` |
| 米国東部 | `https://eastus.api.cognitive.microsoft.com` |
| 西ヨーロッパ | `https://westeurope.api.cognitive.microsoft.com` |

## 🎵 音声ファイルの準備

### MP3からWAVへの変換

プロジェクトルートに音声変換スクリプトを用意しています：

```bash
# 単一ファイル変換
./convert_mp3_to_wav.sh your_audio.mp3

# 複数ファイル一括変換
./batch_convert_mp3_to_wav.sh

# 変換例
./convert_mp3_to_wav.sh podcast.mp3
# → podcast.wav が作成される（Azure Speech最適化済み）
```

### 音声形式要件

| 項目 | 推奨設定 | 説明 |
|------|----------|------|
| **ファイル形式** | WAV | 必須 |
| **サンプルレート** | 16kHz | 自動変換 |
| **チャンネル** | モノラル | 自動変換 |
| **ビット深度** | 16bit | 自動変換 |
| **最大サイズ** | 1GB | バッチAPI制限 |
| **最大時間** | 10時間 | バッチAPI制限 |

## 💰 料金情報

### Azure Speech Services 料金（2024年12月時点）

| サービス | 料金/時間 | 特徴 |
|----------|-----------|------|
| **ConversationTranscriber** | $2.50 (~¥375) | リアルタイム処理 |
| **Batch API** | $1.00 (~¥150) | **60%安い**、高速 |
| **無料枠** | 月5時間 | 両サービス共通 |

### 実際の料金例

| 音声長 | リアルタイム | バッチ処理 | 無料枠での処理 |
|-------|-------------|-----------|--------------|
| 30分 | $1.25 (~¥188) | $0.50 (~¥75) | ✅ 無料 |
| 1時間 | $2.50 (~¥375) | $1.00 (~¥150) | ✅ 無料 |
| 2時間 | $5.00 (~¥750) | $2.00 (~¥300) | 部分無料 |
| 5時間 | $12.50 (~¥1,875) | $5.00 (~¥750) | 有料 |

## 🌍 対応言語

## 対応音声形式

- **ファイル処理**: WAV形式（16kHz、16bit、モノラルを推奨）
- **リアルタイム処理**: システムデフォルトのマイク入力

## 🚨 トラブルシューティング

### よくあるエラーと解決方法

#### 認証エラー（401）
```
❌ WebSocket upgrade failed: Authentication error (401)
```
**解決方法:**
- 環境変数 `SPEECH_KEY` と `SPEECH_REGION` が正しく設定されているか確認
- Azure Speech Servicesのサブスクリプションが有効か確認
- キーをコピー&ペーストする際の余分なスペースに注意

#### マイクが認識されない
**解決方法:**
- システムのマイク設定とプライバシー設定を確認
- 他のアプリケーションがマイクを使用していないか確認
- マイクの物理的な接続を確認

#### WAVファイルが読み込めない
**解決方法:**
- ファイルパスに日本語や特殊文字が含まれていないか確認
- WAVファイルの形式を確認（16kHz、16bit、モノラルを推奨）
- 変換スクリプトを使用: `./convert_mp3_to_wav.sh your_audio.mp3`

#### 話者分離の精度が低い
**改善方法:**
- 音質を向上させる（ノイズ除去、音声レベル調整）
- 話者間の発話間隔を適切に設ける
- モノラル音声を使用する
- バッチ処理を試す（より高精度なモデル使用）

#### 処理が遅い
**解決方法:**
- バッチ処理を使用する（2-10倍高速）
- より近いAzureリージョンを選択
- ネットワーク接続を確認

### パフォーマンス最適化

#### 音声ファイルの前処理
```bash
# 最適化された変換
ffmpeg -i input.mp3 -ar 16000 -ac 1 -ab 32k output.wav

# ノイズ低減
ffmpeg -i input.wav -af "anlmdn" output_denoised.wav

# 無音部分の除去
ffmpeg -i input.wav -af silenceremove=start_periods=1:start_duration=1:start_threshold=-60dB:detection=peak output_trimmed.wav
```

#### 長時間音声の処理
- **4時間未満**: リアルタイム処理またはバッチ処理
- **4時間以上**: バッチ処理必須
- **10時間以上**: ファイル分割を検討

## 🔗 関連リンク・参考資料

### 公式ドキュメント
- [Azure Speech Services ドキュメント](https://docs.microsoft.com/ja-jp/azure/cognitive-services/speech-service/)
- [Speaker Diarization の概要](https://docs.microsoft.com/ja-jp/azure/cognitive-services/speech-service/speaker-diarization)
- [.NET Speech SDK リファレンス](https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.cognitiveservices.speech)
- [Batch Transcription API](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/batch-transcription)

### 料金情報
- [Azure Speech Services 料金](https://azure.microsoft.com/ja-jp/pricing/details/cognitive-services/speech-services/)
- [無料試用版](https://azure.microsoft.com/ja-jp/free/cognitive-services/)

### 代替ソリューション
- [OpenAI Whisper](https://openai.com/research/whisper) - オープンソース音声認識
- [WhisperX](https://github.com/m-bain/whisperX) - Whisper + 話者分離
- [Deepgram](https://deepgram.com) - 高速音声認識API

## 📜 ライセンス

このプロジェクトはサンプルコードとして提供されており、自由に使用・改変できます。

## 🤝 貢献・フィードバック

- Issues: バグレポートや機能要求
- Pull Requests: コード改善の提案
- Discussions: 使用方法に関する質問

## 📝 更新履歴

- **v1.2.0**: コマンドライン引数対応、バッチ処理オプション追加
- **v1.1.0**: 多言語対応、エンドポイント設定オプション追加
- **v1.0.0**: 初回リリース、基本的な話者分離機能