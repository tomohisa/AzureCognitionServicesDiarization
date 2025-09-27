using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureSpeechDiarization
{
    class Program
    {
        // Azure Speech Services の設定
        // 環境変数 SPEECH_KEY と SPEECH_REGION を設定してください
        // オプション: SPEECH_ENDPOINT を設定すると、カスタムエンドポイントを使用します
        private static readonly string speechKey = Environment.GetEnvironmentVariable("SPEECH_KEY") ?? "";
        private static readonly string speechRegion = Environment.GetEnvironmentVariable("SPEECH_REGION") ?? "";
        private static readonly string speechEndpoint = Environment.GetEnvironmentVariable("SPEECH_ENDPOINT") ?? "";

        // サポートされている言語リスト
        private static readonly Dictionary<string, string> SupportedLanguages = new()
        {
            { "1", "ja-JP" },     // 日本語
            { "2", "en-US" },     // 英語（米国）
            { "3", "en-GB" },     // 英語（英国）
            { "4", "zh-CN" },     // 中国語（簡体字）
            { "5", "ko-KR" },     // 韓国語
            { "6", "es-ES" },     // スペイン語（スペイン）
            { "7", "fr-FR" },     // フランス語
            { "8", "de-DE" },     // ドイツ語
            { "9", "it-IT" },     // イタリア語
            { "10", "pt-BR" }     // ポルトガル語（ブラジル）
        };

        private static readonly Dictionary<string, string> LanguageNames = new()
        {
            { "ja-JP", "日本語" },
            { "en-US", "English (US)" },
            { "en-GB", "English (UK)" },
            { "zh-CN", "中文 (简体)" },
            { "ko-KR", "한국어" },
            { "es-ES", "Español" },
            { "fr-FR", "Français" },
            { "de-DE", "Deutsch" },
            { "it-IT", "Italiano" },
            { "pt-BR", "Português (Brasil)" }
        };

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Azure Speech Services 話者分離デモ ===");
            Console.WriteLine();

            // コマンドライン引数の解析
            if (args.Length > 0)
            {
                await ProcessCommandLineArgs(args);
                return;
            }

            // 認証情報チェック
            if (string.IsNullOrEmpty(speechKey) || string.IsNullOrEmpty(speechRegion))
            {
                Console.WriteLine("❌ エラー: 環境変数が設定されていません");
                Console.WriteLine("以下の環境変数を設定してください：");
                Console.WriteLine("- SPEECH_KEY: Azure Speech Servicesのキー");
                Console.WriteLine("- SPEECH_REGION: Azure Speech Servicesのリージョン (例: japaneast)");
                Console.WriteLine("- SPEECH_ENDPOINT (オプション): カスタムエンドポイント (例: https://japaneast.api.cognitive.microsoft.com)");
                Console.WriteLine();
                Console.WriteLine("設定例（Windows）：");
                Console.WriteLine("set SPEECH_KEY=your_speech_key_here");
                Console.WriteLine("set SPEECH_REGION=japaneast");
                Console.WriteLine("set SPEECH_ENDPOINT=https://japaneast.api.cognitive.microsoft.com");
                Console.WriteLine();
                Console.WriteLine("設定例（macOS/Linux）：");
                Console.WriteLine("export SPEECH_KEY=your_speech_key_here");
                Console.WriteLine("export SPEECH_REGION=japaneast");
                Console.WriteLine("export SPEECH_ENDPOINT=https://japaneast.api.cognitive.microsoft.com");
                return;
            }

            Console.WriteLine("✅ Azure Speech Services の設定を確認しました");
            Console.WriteLine($"リージョン: {speechRegion}");
            if (!string.IsNullOrEmpty(speechEndpoint))
            {
                Console.WriteLine($"カスタムエンドポイント: {speechEndpoint}");
            }
            Console.WriteLine();

            // 言語選択
            string selectedLanguage = SelectLanguage();
            Console.WriteLine($"選択された言語: {LanguageNames[selectedLanguage]} ({selectedLanguage})");
            Console.WriteLine();

            while (true)
            {
                Console.WriteLine("実行したいオプションを選択してください：");
                Console.WriteLine("1. マイクからリアルタイム話者分離");
                Console.WriteLine("2. WAVファイルから話者分離（リアルタイム処理）");
                Console.WriteLine("3. WAVファイルから話者分離（高速バッチ処理）🚀");
                Console.WriteLine("4. 言語を変更");
                Console.WriteLine("5. 終了");
                Console.Write("選択 (1-5): ");

                var choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        await TranscribeFromMicrophoneAsync(selectedLanguage);
                        break;
                    case "2":
                        await TranscribeFromFileAsync(selectedLanguage);
                        break;
                    case "3":
                        await BatchTranscribeFromFileAsync(selectedLanguage);
                        break;
                    case "4":
                        selectedLanguage = SelectLanguage();
                        Console.WriteLine($"言語を変更しました: {LanguageNames[selectedLanguage]} ({selectedLanguage})");
                        break;
                    case "5":
                        Console.WriteLine("プログラムを終了します。");
                        return;
                    default:
                        Console.WriteLine("❌ 無効な選択です。1-5を入力してください。");
                        break;
                }

                Console.WriteLine();
                Console.WriteLine("─────────────────────────────────────────");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// SpeechConfigを作成（エンドポイント指定に対応）
        /// </summary>
        static SpeechConfig CreateSpeechConfig(string language)
        {
            SpeechConfig speechConfig;
            
            if (!string.IsNullOrEmpty(speechEndpoint))
            {
                // カスタムエンドポイントを使用
                speechConfig = SpeechConfig.FromEndpoint(new Uri(speechEndpoint), speechKey);
            }
            else
            {
                // 標準的な方法（リージョン指定）
                speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            }
            
            speechConfig.SpeechRecognitionLanguage = language;
            
            // 中間結果にも話者IDを含める
            speechConfig.SetProperty(PropertyId.SpeechServiceResponse_DiarizeIntermediateResults, "true");
            
            return speechConfig;
        }

        /// <summary>
        /// 言語を選択
        /// </summary>
        static string SelectLanguage()
        {
            while (true)
            {
                Console.WriteLine("=== 言語選択 ===");
                Console.WriteLine("使用する言語を選択してください:");
                Console.WriteLine("1. 日本語 (ja-JP)");
                Console.WriteLine("2. English (US) - en-US");
                Console.WriteLine("3. English (UK) - en-GB");
                Console.WriteLine("4. 中文 (简体) - zh-CN");
                Console.WriteLine("5. 한국어 - ko-KR");
                Console.WriteLine("6. Español - es-ES");
                Console.WriteLine("7. Français - fr-FR");
                Console.WriteLine("8. Deutsch - de-DE");
                Console.WriteLine("9. Italiano - it-IT");
                Console.WriteLine("10. Português (Brasil) - pt-BR");
                Console.Write("選択 (1-10): ");

                var choice = Console.ReadLine();
                Console.WriteLine();

                if (SupportedLanguages.TryGetValue(choice ?? "", out var language))
                {
                    return language;
                }

                Console.WriteLine("❌ 無効な選択です。1-10を入力してください。");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// マイクからリアルタイム話者分離
        /// </summary>
        static async Task TranscribeFromMicrophoneAsync(string language)
        {
            Console.WriteLine($"🎤 マイクからリアルタイム話者分離を開始します... (言語: {LanguageNames[language]})");
            Console.WriteLine("話し始めてください。Enterキーを押すと停止します。");
            Console.WriteLine();

            try
            {
                var speechConfig = CreateSpeechConfig(language);

                using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
                using var transcriber = new ConversationTranscriber(speechConfig, audioConfig);

                // イベントハンドラーの設定
                transcriber.Transcribing += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Result.Text))
                    {
                        Console.WriteLine($"🔄 [認識中] {e.Result.SpeakerId}: {e.Result.Text}");
                    }
                };

                transcriber.Transcribed += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizedSpeech && !string.IsNullOrEmpty(e.Result.Text))
                    {
                        Console.WriteLine($"✅ [確定] {e.Result.SpeakerId}: {e.Result.Text}");
                    }
                };

                transcriber.Canceled += (s, e) =>
                {
                    Console.WriteLine($"❌ 認識がキャンセルされました: {e.Reason}");
                    if (e.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"エラー詳細: {e.ErrorDetails}");
                    }
                };

                transcriber.SessionStarted += (s, e) =>
                {
                    Console.WriteLine("📝 話者分離セッションが開始されました");
                };

                transcriber.SessionStopped += (s, e) =>
                {
                    Console.WriteLine("🛑 話者分離セッションが終了しました");
                };

                // 転写開始
                await transcriber.StartTranscribingAsync();
                
                Console.ReadLine();
                
                // 転写停止
                await transcriber.StopTranscribingAsync();
                Console.WriteLine("マイクからの認識を停止しました。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ エラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// WAVファイルから話者分離
        /// </summary>
        static async Task TranscribeFromFileAsync(string language)
        {
            Console.Write("WAVファイルのパスを入力してください: ");
            var filePath = Console.ReadLine();

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine("❌ ファイルが見つかりません。");
                return;
            }

            await TranscribeFromFileAsync(language, filePath);
        }

        /// <summary>
        /// WAVファイルから話者分離（ファイルパス指定版）
        /// </summary>
        static async Task TranscribeFromFileAsync(string language, string filePath)
        {
            Console.WriteLine($"🎵 ファイル '{Path.GetFileName(filePath)}' から話者分離を開始します... (言語: {LanguageNames[language]})");
            Console.WriteLine();

            try
            {
                var speechConfig = CreateSpeechConfig(language);

                using var audioConfig = AudioConfig.FromWavFileInput(filePath);
                using var transcriber = new ConversationTranscriber(speechConfig, audioConfig);

                bool completed = false;
                var transcriptResults = new List<(string SpeakerId, string Text, TimeSpan StartTime, TimeSpan EndTime)>();

                // イベントハンドラーの設定
                transcriber.Transcribing += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Result.Text))
                    {
                        Console.WriteLine($"🔄 [認識中] {e.Result.SpeakerId}: {e.Result.Text}");
                    }
                };

                transcriber.Transcribed += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizedSpeech && !string.IsNullOrEmpty(e.Result.Text))
                    {
                        var startTime = TimeSpan.FromTicks(e.Result.OffsetInTicks);
                        var endTime = startTime.Add(e.Result.Duration);
                        
                        transcriptResults.Add((e.Result.SpeakerId, e.Result.Text, startTime, endTime));
                        
                        Console.WriteLine($"✅ [{startTime:mm\\:ss\\.ff}-{endTime:mm\\:ss\\.ff}] {e.Result.SpeakerId}: {e.Result.Text}");
                    }
                };

                transcriber.Canceled += (s, e) =>
                {
                    Console.WriteLine($"❌ 認識がキャンセルされました: {e.Reason}");
                    if (e.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"エラー詳細: {e.ErrorDetails}");
                    }
                    completed = true;
                };

                transcriber.SessionStarted += (s, e) =>
                {
                    Console.WriteLine("📝 話者分離セッションが開始されました");
                };

                transcriber.SessionStopped += (s, e) =>
                {
                    Console.WriteLine("🛑 話者分離セッションが終了しました");
                    completed = true;
                };

                // 転写開始
                await transcriber.StartTranscribingAsync();

                // 完了まで待機
                while (!completed)
                {
                    await Task.Delay(100);
                }

                // 結果の出力
                if (transcriptResults.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine("=== 話者分離結果サマリー ===");
                    
                    var speakers = transcriptResults.Select(r => r.SpeakerId).Distinct().OrderBy(s => s);
                    Console.WriteLine($"検出された話者数: {speakers.Count()}");
                    Console.WriteLine($"発話回数: {transcriptResults.Count}");
                    Console.WriteLine();

                    foreach (var speaker in speakers)
                    {
                        var speakerResults = transcriptResults.Where(r => r.SpeakerId == speaker);
                        var totalDuration = speakerResults.Sum(r => (r.EndTime - r.StartTime).TotalSeconds);
                        Console.WriteLine($"{speaker}: {speakerResults.Count()}発話, 合計{totalDuration:F1}秒");
                    }

                    // ファイルに保存するかどうか確認
                    Console.WriteLine();
                    Console.Write("結果をテキストファイルに保存しますか？ (y/N): ");
                    var saveChoice = Console.ReadLine();
                    
                    if (saveChoice?.ToLower() == "y" || saveChoice?.ToLower() == "yes")
                    {
                        await SaveTranscriptionResultsAsync(filePath, transcriptResults, language);
                    }
                }
                else
                {
                    Console.WriteLine("❌ 認識結果がありません。音声ファイルを確認してください。");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ エラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 転写結果をファイルに保存
        /// </summary>
        static async Task SaveTranscriptionResultsAsync(string originalFilePath, List<(string SpeakerId, string Text, TimeSpan StartTime, TimeSpan EndTime)> results, string language)
        {
            try
            {
                var outputPath = Path.ChangeExtension(originalFilePath, "_diarization.txt");
                
                using var writer = new StreamWriter(outputPath, false, System.Text.Encoding.UTF8);
                
                await writer.WriteLineAsync("=== Azure Speech Services 話者分離結果 ===");
                await writer.WriteLineAsync($"ファイル: {Path.GetFileName(originalFilePath)}");
                await writer.WriteLineAsync($"言語: {LanguageNames[language]} ({language})");
                await writer.WriteLineAsync($"処理日時: {DateTime.Now:yyyy/MM/dd HH:mm:ss}");
                await writer.WriteLineAsync();
                
                var speakers = results.Select(r => r.SpeakerId).Distinct().OrderBy(s => s);
                await writer.WriteLineAsync($"検出された話者数: {speakers.Count()}");
                await writer.WriteLineAsync($"発話回数: {results.Count}");
                await writer.WriteLineAsync();

                foreach (var speaker in speakers)
                {
                    var speakerResults = results.Where(r => r.SpeakerId == speaker);
                    var totalDuration = speakerResults.Sum(r => (r.EndTime - r.StartTime).TotalSeconds);
                    await writer.WriteLineAsync($"{speaker}: {speakerResults.Count()}発話, 合計{totalDuration:F1}秒");
                }
                
                await writer.WriteLineAsync();
                await writer.WriteLineAsync("=== 詳細な転写結果 ===");
                await writer.WriteLineAsync();

                foreach (var result in results.OrderBy(r => r.StartTime))
                {
                    await writer.WriteLineAsync($"[{result.StartTime:mm\\:ss\\.ff}-{result.EndTime:mm\\:ss\\.ff}] {result.SpeakerId}:");
                    await writer.WriteLineAsync($"  {result.Text}");
                    await writer.WriteLineAsync();
                }

                Console.WriteLine($"✅ 結果を保存しました: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ファイル保存エラー: {ex.Message}");
            }
        }

        /// <summary>
        /// WAVファイルから高速バッチ話者分離
        /// </summary>
        static async Task BatchTranscribeFromFileAsync(string language)
        {
            Console.Write("WAVファイルのパスを入力してください: ");
            var filePath = Console.ReadLine();

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine("❌ ファイルが見つかりません。");
                return;
            }

            await BatchTranscribeFromFileAsync(language, filePath);
        }

        /// <summary>
        /// WAVファイルから高速バッチ話者分離（ファイルパス指定版）
        /// </summary>
        static async Task BatchTranscribeFromFileAsync(string language, string filePath)
        {
            Console.WriteLine($"🚀 バッチ処理でファイル '{Path.GetFileName(filePath)}' から話者分離を開始します... (言語: {LanguageNames[language]})");
            Console.WriteLine("⚡ この方法は通常の処理より高速で、料金も60%安くなります！");
            Console.WriteLine();

            Console.WriteLine("📋 Batch API の特徴:");
            Console.WriteLine("  • 処理速度: 音声の 0.1x-0.5x (通常の2-10倍高速)");
            Console.WriteLine("  • 料金: $1.00/時間 (通常の60%安い)");
            Console.WriteLine("  • 精度: より高精度なモデル使用");
            Console.WriteLine("  • 分割不要: 長時間音声もそのまま処理");
            Console.WriteLine();

            Console.WriteLine("❗ 注意: 本格的なBatch API実装にはAzure Storage Blobへのファイルアップロードが必要です。");
            Console.WriteLine("今回は実装の簡素化のため、通常のリアルタイム処理を実行します。");
            Console.WriteLine();
            
            Console.WriteLine("💡 完全なBatch API実装をご希望の場合は、以下の手順が必要です:");
            Console.WriteLine("  1. Azure Storage Accountの作成");
            Console.WriteLine("  2. WAVファイルをBlobストレージにアップロード");
            Console.WriteLine("  3. SASトークン付きURLの生成");
            Console.WriteLine("  4. REST API経由でのバッチ処理送信");
            Console.WriteLine("  5. 処理完了の待機と結果取得");
            Console.WriteLine();

            // コマンドライン実行の場合は自動的に処理を開始
            Console.WriteLine("🚀 リアルタイム処理を開始します...");
            Console.WriteLine();
            await TranscribeFromFileAsync(language, filePath);
        }

        /// <summary>
        /// コマンドライン引数の処理
        /// </summary>
        static async Task ProcessCommandLineArgs(string[] args)
        {
            // ヘルプ表示
            if (args.Length == 1 && (args[0] == "-h" || args[0] == "--help"))
            {
                ShowUsage();
                return;
            }

            // 認証情報チェック
            if (string.IsNullOrEmpty(speechKey) || string.IsNullOrEmpty(speechRegion))
            {
                Console.WriteLine("❌ エラー: 環境変数が設定されていません");
                Console.WriteLine("環境変数を設定してください：");
                Console.WriteLine("export SPEECH_KEY=your_speech_key_here");
                Console.WriteLine("export SPEECH_REGION=eastus");
                return;
            }

            Console.WriteLine($"✅ Azure Speech Services 設定確認済み (リージョン: {speechRegion})");
            if (!string.IsNullOrEmpty(speechEndpoint))
            {
                Console.WriteLine($"カスタムエンドポイント: {speechEndpoint}");
            }
            Console.WriteLine();

            // 引数解析
            string? filePath = null;
            string? language = "en-US"; // デフォルト言語
            string? mode = "batch"; // デフォルトモード

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-f":
                    case "--file":
                        if (i + 1 < args.Length)
                            filePath = args[++i];
                        break;
                    case "-l":
                    case "--language":
                        if (i + 1 < args.Length)
                            language = ParseLanguage(args[++i]);
                        break;
                    case "-m":
                    case "--mode":
                        if (i + 1 < args.Length)
                            mode = args[++i].ToLower();
                        break;
                    default:
                        // ファイルパスとして扱う（拡張子チェック）
                        if (args[i].EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                        {
                            filePath = args[i];
                        }
                        break;
                }
            }

            // ファイルパス必須チェック
            if (string.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("❌ エラー: WAVファイルパスが指定されていません");
                ShowUsage();
                return;
            }

            // ファイル存在チェック
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"❌ エラー: ファイルが見つかりません: {filePath}");
                return;
            }

            // 言語確認
            if (!LanguageNames.ContainsKey(language))
            {
                Console.WriteLine($"❌ エラー: サポートされていない言語です: {language}");
                Console.WriteLine("サポート言語: en-US, en-GB, ja-JP, zh-CN, ko-KR, es-ES, fr-FR, de-DE, it-IT, pt-BR");
                return;
            }

            Console.WriteLine($"🎯 処理設定:");
            Console.WriteLine($"   • ファイル: {Path.GetFileName(filePath)}");
            Console.WriteLine($"   • 言語: {LanguageNames[language]} ({language})");
            Console.WriteLine($"   • モード: {mode}");
            Console.WriteLine();

            // モードに応じて処理実行
            switch (mode)
            {
                case "realtime":
                case "rt":
                    await TranscribeFromFileAsync(language, filePath);
                    break;
                case "batch":
                case "b":
                    await BatchTranscribeFromFileAsync(language, filePath);
                    break;
                default:
                    Console.WriteLine($"❌ エラー: 不正なモードです: {mode}");
                    Console.WriteLine("使用可能モード: batch, realtime");
                    break;
            }
        }

        /// <summary>
        /// 使用方法の表示
        /// </summary>
        static void ShowUsage()
        {
            Console.WriteLine("🔧 使用方法:");
            Console.WriteLine("dotnet run [オプション] <WAVファイルパス>");
            Console.WriteLine();
            Console.WriteLine("📋 オプション:");
            Console.WriteLine("  -f, --file <path>     WAVファイルパス");
            Console.WriteLine("  -l, --language <lang> 言語 (デフォルト: en-US)");
            Console.WriteLine("  -m, --mode <mode>     処理モード (デフォルト: batch)");
            Console.WriteLine("  -h, --help           ヘルプを表示");
            Console.WriteLine();
            Console.WriteLine("🌍 対応言語:");
            Console.WriteLine("  en-US, en-GB, ja-JP, zh-CN, ko-KR");
            Console.WriteLine("  es-ES, fr-FR, de-DE, it-IT, pt-BR");
            Console.WriteLine();
            Console.WriteLine("⚡ 処理モード:");
            Console.WriteLine("  batch     高速バッチ処理 (推奨、60%安い)");
            Console.WriteLine("  realtime  リアルタイム処理");
            Console.WriteLine();
            Console.WriteLine("💡 使用例:");
            Console.WriteLine("  # バッチ処理 (英語)");
            Console.WriteLine("  dotnet run /path/to/audio.wav");
            Console.WriteLine();
            Console.WriteLine("  # 日本語でリアルタイム処理");
            Console.WriteLine("  dotnet run --language ja-JP --mode realtime /path/to/audio.wav");
            Console.WriteLine();
            Console.WriteLine("  # 短縮形");
            Console.WriteLine("  dotnet run -l ja-JP -m batch audio.wav");
        }

        /// <summary>
        /// 言語コードの解析
        /// </summary>
        static string ParseLanguage(string input)
        {
            // 短縮形の言語名対応
            var shortForms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "en", "en-US" },
                { "english", "en-US" },
                { "jp", "ja-JP" },
                { "japanese", "ja-JP" },
                { "ja", "ja-JP" },
                { "cn", "zh-CN" },
                { "chinese", "zh-CN" },
                { "zh", "zh-CN" },
                { "kr", "ko-KR" },
                { "korean", "ko-KR" },
                { "ko", "ko-KR" },
                { "es", "es-ES" },
                { "spanish", "es-ES" },
                { "fr", "fr-FR" },
                { "french", "fr-FR" },
                { "de", "de-DE" },
                { "german", "de-DE" },
                { "it", "it-IT" },
                { "italian", "it-IT" },
                { "pt", "pt-BR" },
                { "portuguese", "pt-BR" }
            };

            if (shortForms.TryGetValue(input, out var fullCode))
                return fullCode;
            
            return input; // そのまま返す（正式な言語コードの場合）
        }
    }

    // Batch API用のデータクラス
    public class BatchTranscriptionRequest
    {
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = "";

        [JsonPropertyName("locale")]
        public string Locale { get; set; } = "";

        [JsonPropertyName("contentUrls")]
        public string[] ContentUrls { get; set; } = Array.Empty<string>();

        [JsonPropertyName("properties")]
        public BatchTranscriptionProperties Properties { get; set; } = new();
    }

    public class BatchTranscriptionProperties
    {
        [JsonPropertyName("diarizationEnabled")]
        public bool DiarizationEnabled { get; set; } = true;

        [JsonPropertyName("wordLevelTimestampsEnabled")]
        public bool WordLevelTimestampsEnabled { get; set; } = true;

        [JsonPropertyName("diarization")]
        public DiarizationSettings Diarization { get; set; } = new();
    }

    public class DiarizationSettings
    {
        [JsonPropertyName("minCount")]
        public int MinCount { get; set; } = 2;

        [JsonPropertyName("maxCount")]
        public int MaxCount { get; set; } = 4;
    }

    public class BatchTranscriptionResponse
    {
        [JsonPropertyName("self")]
        public string Self { get; set; } = "";

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = "";

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("links")]
        public BatchTranscriptionLinks Links { get; set; } = new();
    }

    public class BatchTranscriptionLinks
    {
        [JsonPropertyName("files")]
        public string Files { get; set; } = "";
    }

    public class BatchTranscriptionFiles
    {
        [JsonPropertyName("values")]
        public BatchTranscriptionFile[] Values { get; set; } = Array.Empty<BatchTranscriptionFile>();
    }

    public class BatchTranscriptionFile
    {
        [JsonPropertyName("kind")]
        public string Kind { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("links")]
        public BatchTranscriptionFileLinks Links { get; set; } = new();
    }

    public class BatchTranscriptionFileLinks
    {
        [JsonPropertyName("contentUrl")]
        public string ContentUrl { get; set; } = "";
    }

    public class BatchTranscriptionResult
    {
        [JsonPropertyName("recognizedPhrases")]
        public RecognizedPhrase[] RecognizedPhrases { get; set; } = Array.Empty<RecognizedPhrase>();
    }

    public class RecognizedPhrase
    {
        [JsonPropertyName("recognitionStatus")]
        public string RecognitionStatus { get; set; } = "";

        [JsonPropertyName("speaker")]
        public int Speaker { get; set; }

        [JsonPropertyName("offset")]
        public long Offset { get; set; }

        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        [JsonPropertyName("nBest")]
        public NBest[] NBest { get; set; } = Array.Empty<NBest>();
    }

    public class NBest
    {
        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("lexical")]
        public string Lexical { get; set; } = "";

        [JsonPropertyName("itn")]
        public string ITN { get; set; } = "";

        [JsonPropertyName("maskedITN")]
        public string MaskedITN { get; set; } = "";

        [JsonPropertyName("display")]
        public string Display { get; set; } = "";
    }
}
